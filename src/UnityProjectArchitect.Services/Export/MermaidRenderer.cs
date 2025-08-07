using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services
{
    public class MermaidRenderer
    {
        private readonly Dictionary<string, string> _themeOptions;
        private readonly string _tempDirectory;
        private readonly Dictionary<string, string> _diagramCache;

        public MermaidRenderer()
        {
            _themeOptions = new Dictionary<string, string>
            {
                { "default", "default" },
                { "dark", "dark" },
                { "forest", "forest" },
                { "neutral", "neutral" }
            };
            
            _tempDirectory = Path.Combine(Path.GetTempPath(), "UnityProjectArchitect", "Mermaid");
            _diagramCache = new Dictionary<string, string>();
            
            EnsureTempDirectoryExists();
        }

        /// <summary>
        /// Renders all Mermaid diagrams in the provided content and returns the processed content with image references
        /// </summary>
        public async Task<string> RenderDiagramsInContentAsync(string content, MermaidRenderOptions options = null)
        {
            options ??= new MermaidRenderOptions();
            
            try
            {
                // Find all Mermaid code blocks
                List<MermaidDiagram> diagrams = ExtractMermaidDiagrams(content);
                if (diagrams.Count == 0)
                {
                    Debug.Log("No Mermaid diagrams found in content");
                    return content;
                }

                string processedContent = content;
                int renderedCount = 0;

                // Render each diagram
                foreach (MermaidDiagram diagram in diagrams)
                {
                    try
                    {
                        string imagePath = await RenderDiagramAsync(diagram, options);
                        if (!string.IsNullOrEmpty(imagePath))
                        {
                            // Replace the Mermaid code block with image reference
                            string imageReference = CreateImageReference(imagePath, diagram.Title, options.OutputFormat);
                            processedContent = processedContent.Replace(diagram.FullBlock, imageReference);
                            renderedCount++;
                        }
                        else if (options.FallbackToSyntax)
                        {
                            Debug.LogWarning($"Failed to render diagram '{diagram.Title}', keeping original syntax");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to render diagram '{diagram.Title}': {ex.Message}");
                        if (!options.FallbackToSyntax)
                        {
                            // Replace with error message
                            string errorMessage = $"<!-- Error rendering diagram '{diagram.Title}': {ex.Message} -->";
                            processedContent = processedContent.Replace(diagram.FullBlock, errorMessage);
                        }
                    }
                }

                Debug.Log($"Successfully rendered {renderedCount}/{diagrams.Count} Mermaid diagrams");
                return processedContent;
            }
            catch (Exception ex)
            {
                Debug.LogError($"MermaidRenderer error: {ex}");
                return options.FallbackToSyntax ? content : $"<!-- Mermaid rendering failed: {ex.Message} -->\n{content}";
            }
        }

        /// <summary>
        /// Renders a single Mermaid diagram and returns the image file path
        /// </summary>
        public async Task<string> RenderDiagramAsync(MermaidDiagram diagram, MermaidRenderOptions options = null)
        {
            options ??= new MermaidRenderOptions();
            
            // Check cache first
            string cacheKey = GenerateCacheKey(diagram.Content, options);
            if (_diagramCache.ContainsKey(cacheKey) && File.Exists(_diagramCache[cacheKey]))
            {
                Debug.Log($"Using cached diagram: {diagram.Title}");
                return _diagramCache[cacheKey];
            }

            // Check if Mermaid CLI is available
            if (!await IsMermaidCliAvailableAsync())
            {
                Debug.LogError("Mermaid CLI (mmdc) is not available. Please install with: npm install -g @mermaid-js/mermaid-cli");
                return null;
            }

            try
            {
                string inputFile = Path.Combine(_tempDirectory, $"{diagram.SafeTitle}.mmd");
                string outputFile = Path.Combine(_tempDirectory, $"{diagram.SafeTitle}.{options.OutputFormat.ToString().ToLower()}");

                // Write diagram content to temporary file
                await File.WriteAllTextAsync(inputFile, diagram.Content);

                // Build Mermaid CLI command
                string command = BuildMermaidCommand(inputFile, outputFile, options);
                
                // Execute Mermaid CLI
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "mmdc",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        string output = await process.StandardOutput.ReadToEndAsync();
                        string error = await process.StandardError.ReadToEndAsync();
                        
                        process.WaitForExit();

                        if (process.ExitCode == 0 && File.Exists(outputFile))
                        {
                            // Cache the result
                            _diagramCache[cacheKey] = outputFile;
                            Debug.Log($"Successfully rendered diagram: {diagram.Title} -> {outputFile}");
                            return outputFile;
                        }
                        else
                        {
                            Debug.LogError($"Mermaid CLI failed for '{diagram.Title}'. Exit code: {process.ExitCode}, Error: {error}");
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error rendering Mermaid diagram '{diagram.Title}': {ex}");
                return null;
            }

            return null;
        }

        /// <summary>
        /// Checks if Mermaid CLI is available on the system
        /// </summary>
        public async Task<bool> IsMermaidCliAvailableAsync()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "mmdc",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        return process.ExitCode == 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Cleans up temporary files and cache
        /// </summary>
        public void Cleanup()
        {
            try
            {
                if (Directory.Exists(_tempDirectory))
                {
                    Directory.Delete(_tempDirectory, true);
                    Debug.Log("Cleaned up Mermaid temporary files");
                }
                _diagramCache.Clear();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error cleaning up Mermaid temp files: {ex}");
            }
        }

        private List<MermaidDiagram> ExtractMermaidDiagrams(string content)
        {
            List<MermaidDiagram> diagrams = new List<MermaidDiagram>();
            
            // Regex to match Mermaid code blocks: ```mermaid ... ```
            string pattern = @"```mermaid\s*\n(.*?)\n```";
            MatchCollection matches = Regex.Matches(content, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            int diagramIndex = 0;
            foreach (Match match in matches)
            {
                string diagramContent = match.Groups[1].Value.Trim();
                string title = ExtractDiagramTitle(diagramContent) ?? $"diagram_{diagramIndex:D2}";
                
                diagrams.Add(new MermaidDiagram
                {
                    Title = title,
                    SafeTitle = MakeSafeFilename(title),
                    Content = diagramContent,
                    FullBlock = match.Value
                });
                
                diagramIndex++;
            }

            return diagrams;
        }

        private string ExtractDiagramTitle(string content)
        {
            // Try to extract title from common Mermaid patterns
            string[] lines = content.Split('\n');
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                
                // Look for title in various formats
                if (trimmed.StartsWith("title:"))
                {
                    return trimmed.Substring(6).Trim();
                }
                else if (trimmed.StartsWith("graph") || trimmed.StartsWith("flowchart"))
                {
                    // Extract from graph declaration
                    string[] parts = trimmed.Split(' ');
                    if (parts.Length > 2)
                    {
                        return string.Join(" ", parts.Skip(2));
                    }
                }
                else if (trimmed.Contains("-->") || trimmed.Contains("---"))
                {
                    // First node might be a good title
                    string firstNode = trimmed.Split(new[] { "-->" }, StringSplitOptions.None)[0]
                                             .Split(new[] { "---" }, StringSplitOptions.None)[0]
                                             .Trim();
                    if (!string.IsNullOrEmpty(firstNode) && !firstNode.Contains("[") && !firstNode.Contains("("))
                    {
                        return firstNode;
                    }
                }
            }

            return null;
        }

        private string MakeSafeFilename(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return "diagram";
            
            // Replace invalid characters
            string safe = Regex.Replace(filename, @"[<>:""/\\|?*]", "_");
            safe = Regex.Replace(safe, @"\s+", "_");
            safe = safe.Trim('_');
            
            return string.IsNullOrEmpty(safe) ? "diagram" : safe;
        }

        private string BuildMermaidCommand(string inputFile, string outputFile, MermaidRenderOptions options)
        {
            List<string> args = new List<string>
            {
                $"-i \"{inputFile}\"",
                $"-o \"{outputFile}\"",
                $"-t {options.Theme}",
                $"-b {options.BackgroundColor}"
            };

            if (options.Width > 0)
                args.Add($"-w {options.Width}");
            
            if (options.Height > 0)
                args.Add($"-H {options.Height}");

            if (options.Scale > 0)
                args.Add($"-s {options.Scale}");

            return string.Join(" ", args);
        }

        private string CreateImageReference(string imagePath, string title, MermaidOutputFormat format)
        {
            // Convert absolute path to relative for better portability
            string relativePath = Path.GetFileName(imagePath);
            
            return format switch
            {
                MermaidOutputFormat.SVG => $"![{title}]({relativePath})",
                MermaidOutputFormat.PNG => $"![{title}]({relativePath})",
                MermaidOutputFormat.PDF => $"![{title}]({relativePath})",
                _ => $"![{title}]({relativePath})"
            };
        }

        private string GenerateCacheKey(string content, MermaidRenderOptions options)
        {
            string combined = $"{content}_{options.Theme}_{options.OutputFormat}_{options.Width}_{options.Height}_{options.Scale}_{options.BackgroundColor}";
            return combined.GetHashCode().ToString();
        }

        private void EnsureTempDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(_tempDirectory))
                {
                    Directory.CreateDirectory(_tempDirectory);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create temp directory: {ex}");
            }
        }
    }

    public class MermaidDiagram
    {
        public string Title { get; set; } = "";
        public string SafeTitle { get; set; } = "";
        public string Content { get; set; } = "";
        public string FullBlock { get; set; } = "";
    }

    public class MermaidRenderOptions
    {
        public MermaidOutputFormat OutputFormat { get; set; } = MermaidOutputFormat.PNG;
        public string Theme { get; set; } = "default";
        public string BackgroundColor { get; set; } = "white";
        public int Width { get; set; } = 0; // 0 means auto
        public int Height { get; set; } = 0; // 0 means auto
        public double Scale { get; set; } = 1.0;
        public bool FallbackToSyntax { get; set; } = true;
    }

    public enum MermaidOutputFormat
    {
        PNG,
        SVG,
        PDF
    }
}