using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services
{
    public class PandocIntegration
    {
        private readonly Dictionary<string, string> _supportedEngines;
        private readonly int _defaultTimeoutMs = 120000; // 2 minutes
        private string _pandocPath;
        private string _pandocVersion;
        private bool _isInitialized;

        public bool IsAvailable => _isInitialized && !string.IsNullOrEmpty(_pandocPath);
        public string Version => _pandocVersion ?? "Unknown";
        public string PandocPath => _pandocPath;

        public PandocIntegration()
        {
            _supportedEngines = new Dictionary<string, string>
            {
                ["wkhtmltopdf"] = "wkhtmltopdf (recommended)",
                ["weasyprint"] = "WeasyPrint",
                ["chrome"] = "Chrome/Chromium headless",
                ["prince"] = "Prince XML"
            };
        }

        public async Task<bool> InitializeAsync()
        {
            if (_isInitialized) return IsAvailable;

            try
            {
                _pandocPath = await FindPandocExecutableAsync();
                if (!string.IsNullOrEmpty(_pandocPath))
                {
                    _pandocVersion = await GetPandocVersionAsync();
                    _isInitialized = true;
                    return ValidateVersion();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to initialize Pandoc: {ex.Message}");
            }

            _isInitialized = true;
            return false;
        }

        public async Task<PandocResult> ConvertHtmlToPdfAsync(
            string htmlContent, 
            string outputPath, 
            PandocOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (!IsAvailable)
            {
                return PandocResult.Failure("Pandoc is not available. Please install Pandoc 2.11+ and ensure it's in your PATH.");
            }

            options = options ?? new PandocOptions();
            PandocResult result = new PandocResult();
            string tempHtmlPath = null;
            string tempCssPath = null;

            try
            {
                // Create temporary HTML file
                tempHtmlPath = Path.GetTempFileName() + ".html";
                await File.WriteAllTextAsync(tempHtmlPath, htmlContent, Encoding.UTF8, cancellationToken);

                // Create temporary CSS file if provided
                if (!string.IsNullOrEmpty(options.CustomCSS))
                {
                    tempCssPath = Path.GetTempFileName() + ".css";
                    await File.WriteAllTextAsync(tempCssPath, options.CustomCSS, Encoding.UTF8, cancellationToken);
                }

                // Build Pandoc command arguments
                List<string> arguments = BuildPandocArguments(tempHtmlPath, outputPath, tempCssPath, options);

                // Execute Pandoc
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = _pandocPath,
                    Arguments = string.Join(" ", arguments),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(outputPath)
                };

                DateTime startTime = DateTime.Now;
                using Process process = new Process { StartInfo = startInfo };

                StringBuilder outputBuilder = new StringBuilder();
                StringBuilder errorBuilder = new StringBuilder();

                process.OutputDataReceived += (sender, args) => {
                    if (args.Data != null) outputBuilder.AppendLine(args.Data);
                };

                process.ErrorDataReceived += (sender, args) => {
                    if (args.Data != null) errorBuilder.AppendLine(args.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Wait for completion with timeout
                int timeoutMs = options.TimeoutMs > 0 ? options.TimeoutMs : _defaultTimeoutMs;
                bool completed = await Task.Run(() => process.WaitForExit(timeoutMs), cancellationToken);

                TimeSpan processingTime = DateTime.Now - startTime;

                if (!completed)
                {
                    try { process.Kill(); } catch { }
                    return PandocResult.Failure($"Pandoc conversion timed out after {timeoutMs / 1000} seconds");
                }

                if (process.ExitCode != 0)
                {
                    string errorOutput = errorBuilder.ToString();
                    return PandocResult.Failure($"Pandoc conversion failed (exit code {process.ExitCode}): {errorOutput}");
                }

                // Validate output file was created
                if (!File.Exists(outputPath))
                {
                    return PandocResult.Failure("Pandoc completed but output PDF file was not created");
                }

                System.IO.FileInfo pdfInfo = new System.IO.FileInfo(outputPath);
                result.Success = true;
                result.OutputPath = outputPath;
                result.FileSizeBytes = pdfInfo.Length;
                result.ProcessingTime = processingTime;
                result.PandocOutput = outputBuilder.ToString();

                return result;
            }
            catch (OperationCanceledException)
            {
                return PandocResult.Failure("PDF conversion was cancelled");
            }
            catch (Exception ex)
            {
                return PandocResult.Failure($"PDF conversion failed: {ex.Message}");
            }
            finally
            {
                // Clean up temporary files
                try
                {
                    if (tempHtmlPath != null && File.Exists(tempHtmlPath))
                        File.Delete(tempHtmlPath);
                    if (tempCssPath != null && File.Exists(tempCssPath))
                        File.Delete(tempCssPath);
                }
                catch { }
            }
        }

        public ValidationResult ValidateInstallation()
        {
            ValidationResult validation = new ValidationResult { IsValid = true };

            if (!_isInitialized)
            {
                validation.IsValid = false;
                validation.Errors.Add("Pandoc integration not initialized. Call InitializeAsync() first.");
                return validation;
            }

            if (string.IsNullOrEmpty(_pandocPath))
            {
                validation.IsValid = false;
                validation.Errors.Add("Pandoc executable not found. Please install Pandoc and ensure it's in your system PATH.");
                validation.Errors.Add("Download Pandoc from: https://pandoc.org/installing.html");
                return validation;
            }

            if (!File.Exists(_pandocPath))
            {
                validation.IsValid = false;
                validation.Errors.Add($"Pandoc executable not found at: {_pandocPath}");
                return validation;
            }

            if (!ValidateVersion())
            {
                validation.Warnings.Add($"Pandoc version {_pandocVersion} detected. Version 2.11+ recommended for best PDF generation.");
            }

            return validation;
        }

        public Dictionary<string, string> GetSupportedEngines()
        {
            return new Dictionary<string, string>(_supportedEngines);
        }

        public async Task<bool> IsEngineAvailableAsync(string engineName)
        {
            if (!IsAvailable || !_supportedEngines.ContainsKey(engineName))
                return false;

            try
            {
                ProcessStartInfo testInfo = new ProcessStartInfo
                {
                    FileName = engineName,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process process = Process.Start(testInfo);
                await Task.Run(() => process.WaitForExit(5000));
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetRecommendedEngineAsync()
        {
            // Test engines in order of preference
            string[] preferredEngines = { "wkhtmltopdf", "weasyprint", "chrome" };

            foreach (string engine in preferredEngines)
            {
                if (await IsEngineAvailableAsync(engine))
                    return engine;
            }

            return "wkhtmltopdf"; // Default fallback
        }

        private async Task<string> FindPandocExecutableAsync()
        {
            string[] possibleNames = { "pandoc", "pandoc.exe" };
            
            // First, try to find pandoc in PATH
            foreach (string name in possibleNames)
            {
                try
                {
                    ProcessStartInfo pathTest = new ProcessStartInfo
                    {
                        FileName = name,
                        Arguments = "--version",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using Process process = Process.Start(pathTest);
                    await Task.Run(() => process.WaitForExit(5000));
                    
                    if (process.ExitCode == 0)
                    {
                        return name; // Found in PATH
                    }
                }
                catch
                {
                    continue;
                }
            }

            // If not in PATH, try common installation locations
            string[] commonPaths = GetCommonPandocPaths();
            
            foreach (string path in commonPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        private string[] GetCommonPandocPaths()
        {
            List<string> paths = new List<string>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                paths.AddRange(new[]
                {
                    @"C:\Program Files\Pandoc\pandoc.exe",
                    @"C:\Program Files (x86)\Pandoc\pandoc.exe",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pandoc", "pandoc.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "cabal", "bin", "pandoc.exe")
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                paths.AddRange(new[]
                {
                    "/usr/local/bin/pandoc",
                    "/opt/homebrew/bin/pandoc",
                    "/usr/bin/pandoc"
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                paths.AddRange(new[]
                {
                    "/usr/bin/pandoc",
                    "/usr/local/bin/pandoc",
                    "/snap/bin/pandoc"
                });
            }

            return paths.ToArray();
        }

        private async Task<string> GetPandocVersionAsync()
        {
            try
            {
                ProcessStartInfo versionInfo = new ProcessStartInfo
                {
                    FileName = _pandocPath,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process process = Process.Start(versionInfo);
                string output = await process.StandardOutput.ReadToEndAsync();
                await Task.Run(() => process.WaitForExit(5000));

                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    // Extract version from first line (e.g., "pandoc 2.14.2")
                    string[] lines = output.Split('\n');
                    if (lines.Length > 0)
                    {
                        string[] parts = lines[0].Trim().Split(' ');
                        if (parts.Length >= 2)
                        {
                            return parts[1];
                        }
                    }
                }
            }
            catch { }

            return "Unknown";
        }

        private bool ValidateVersion()
        {
            if (string.IsNullOrEmpty(_pandocVersion) || _pandocVersion == "Unknown")
                return false;

            try
            {
                System.Version version = System.Version.Parse(_pandocVersion);
                System.Version minimumVersion = new System.Version(2, 11);
                return version >= minimumVersion;
            }
            catch
            {
                return false;
            }
        }

        private List<string> BuildPandocArguments(string inputPath, string outputPath, string cssPath, PandocOptions options)
        {
            List<string> args = new List<string>
            {
                $"\"{inputPath}\"",
                "-o", $"\"{outputPath}\"",
                "--pdf-engine=" + options.PdfEngine
            };

            // Add CSS file if provided
            if (!string.IsNullOrEmpty(cssPath))
            {
                args.Add($"--css=\"{cssPath}\"");
            }

            // Page layout options
            if (!string.IsNullOrEmpty(options.PageSize))
            {
                args.Add($"-V papersize={options.PageSize.ToLower()}");
            }

            if (!string.IsNullOrEmpty(options.MarginTop))
                args.Add($"-V margin-top={options.MarginTop}");
            if (!string.IsNullOrEmpty(options.MarginBottom))
                args.Add($"-V margin-bottom={options.MarginBottom}");
            if (!string.IsNullOrEmpty(options.MarginLeft))
                args.Add($"-V margin-left={options.MarginLeft}");
            if (!string.IsNullOrEmpty(options.MarginRight))
                args.Add($"-V margin-right={options.MarginRight}");

            // Font settings
            if (!string.IsNullOrEmpty(options.FontFamily))
                args.Add($"-V mainfont=\"{options.FontFamily}\"");
            if (options.FontSize > 0)
                args.Add($"-V fontsize={options.FontSize}pt");

            // Table of contents
            if (options.IncludeTableOfContents)
            {
                args.Add("--toc");
                if (options.TocDepth > 0)
                    args.Add($"--toc-depth={options.TocDepth}");
            }

            // Additional options
            if (options.IncludeBookmarks)
                args.Add("-V bookmarks=true");

            if (options.NumberSections)
                args.Add("--number-sections");

            // Custom variables
            if (options.Variables?.Count > 0)
            {
                foreach (KeyValuePair<string, string> variable in options.Variables)
                {
                    args.Add($"-V {variable.Key}=\"{variable.Value}\"");
                }
            }

            return args;
        }
    }

    public class PandocOptions
    {
        public string PdfEngine { get; set; } = "wkhtmltopdf";
        public string PageSize { get; set; } = "A4";
        public string MarginTop { get; set; } = "1in";
        public string MarginBottom { get; set; } = "1in";
        public string MarginLeft { get; set; } = "1in";
        public string MarginRight { get; set; } = "1in";
        public string FontFamily { get; set; } = "Arial";
        public int FontSize { get; set; } = 11;
        public bool IncludeTableOfContents { get; set; } = true;
        public int TocDepth { get; set; } = 3;
        public bool IncludeBookmarks { get; set; } = true;
        public bool NumberSections { get; set; } = false;
        public string CustomCSS { get; set; }
        public int TimeoutMs { get; set; } = 120000; // 2 minutes
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
    }

    public class PandocResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string OutputPath { get; set; }
        public long FileSizeBytes { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public string PandocOutput { get; set; }

        public string FormattedFileSize
        {
            get
            {
                if (FileSizeBytes < 1024) return $"{FileSizeBytes} B";
                if (FileSizeBytes < 1024 * 1024) return $"{FileSizeBytes / 1024:F1} KB";
                return $"{FileSizeBytes / (1024 * 1024):F1} MB";
            }
        }

        public static PandocResult CreateSuccess(string outputPath, long fileSize, TimeSpan processingTime)
        {
            return new PandocResult
            {
                Success = true,
                OutputPath = outputPath,
                FileSizeBytes = fileSize,
                ProcessingTime = processingTime
            };
        }

        public static PandocResult Failure(string errorMessage)
        {
            return new PandocResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}