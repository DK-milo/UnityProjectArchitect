using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityProjectArchitect.Core;
using UnityProjectArchitect.API;

namespace UnityProjectArchitect.Services
{
    public class ExportService : IExportService
    {
        public event Action<ExportOperationResult> OnExportComplete;
        public event Action<string, float> OnExportProgress;
        public event Action<string> OnError;

        private readonly Dictionary<ExportFormat, IExportFormatter> _formatters;
        private readonly ExportServiceCapabilities _capabilities;

        public ExportService()
        {
            _formatters = new Dictionary<ExportFormat, IExportFormatter>();
            _capabilities = new ExportServiceCapabilities();
            
            InitializeFormatters();
            InitializeCapabilities();
        }

        public async Task<ExportOperationResult> ExportProjectDocumentationAsync(ProjectData projectData, ExportRequest exportRequest)
        {
            if (projectData == null)
            {
                ExportOperationResult errorResult = new ExportOperationResult(exportRequest.Format, exportRequest.OutputPath)
                {
                    Success = false,
                    ErrorMessage = "Project data cannot be null"
                };
                OnError?.Invoke(errorResult.ErrorMessage);
                return errorResult;
            }

            try
            {
                OnExportProgress?.Invoke("Preparing export...", 0.1f);

                // Validate export request
                ValidationResult validation = await ValidateExportRequestAsync(exportRequest);
                if (!validation.IsValid)
                {
                    ExportOperationResult errorResult = new ExportOperationResult(exportRequest.Format, exportRequest.OutputPath)
                    {
                        Success = false,
                        ErrorMessage = string.Join("; ", validation.Errors),
                        ValidationResult = validation
                    };
                    OnError?.Invoke(errorResult.ErrorMessage);
                    return errorResult;
                }

                OnExportProgress?.Invoke("Building export content...", 0.2f);

                // Build export content
                ExportContent exportContent = BuildExportContent(projectData, exportRequest);

                OnExportProgress?.Invoke($"Exporting to {exportRequest.Format}...", 0.4f);

                // Get formatter and export
                IExportFormatter formatter = GetFormatter(exportRequest.Format);
                if (formatter == null)
                {
                    ExportOperationResult errorResult = new ExportOperationResult(exportRequest.Format, exportRequest.OutputPath)
                    {
                        Success = false,
                        ErrorMessage = $"No formatter available for format: {exportRequest.Format}"
                    };
                    OnError?.Invoke(errorResult.ErrorMessage);
                    return errorResult;
                }

                // Perform the export
                ExportOperationResult result = await formatter.FormatAsync(exportContent, exportRequest.Options);
                result.Format = exportRequest.Format;
                result.OutputPath = exportRequest.OutputPath;

                if (result.Success)
                {
                    OnExportProgress?.Invoke("Writing files...", 0.8f);

                    // Write files to disk
                    await WriteExportFilesAsync(result, exportRequest);

                    OnExportProgress?.Invoke("Export completed!", 1.0f);
                    OnExportComplete?.Invoke(result);
                }
                else
                {
                    OnError?.Invoke(result.ErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                ExportOperationResult errorResult = new ExportOperationResult(exportRequest.Format, exportRequest.OutputPath)
                {
                    Success = false,
                    ErrorMessage = $"Export failed: {ex.Message}"
                };
                OnError?.Invoke(errorResult.ErrorMessage);
                Debug.LogError($"ExportService error: {ex}");
                return errorResult;
            }
        }

        public async Task<ExportOperationResult> ExportSectionAsync(DocumentationSectionData section, ExportRequest exportRequest)
        {
            if (section == null)
            {
                ExportOperationResult errorResult = new ExportOperationResult(exportRequest.Format, exportRequest.OutputPath)
                {
                    Success = false,
                    ErrorMessage = "Documentation section cannot be null"
                };
                OnError?.Invoke(errorResult.ErrorMessage);
                return errorResult;
            }

            try
            {
                OnExportProgress?.Invoke("Preparing section export...", 0.1f);

                // Create export content with single section
                ExportContent exportContent = new ExportContent
                {
                    Title = section.Title,
                    Sections = new List<DocumentationSectionData> { section }
                };

                IExportFormatter formatter = GetFormatter(exportRequest.Format);
                if (formatter == null)
                {
                    ExportOperationResult errorResult = new ExportOperationResult(exportRequest.Format, exportRequest.OutputPath)
                    {
                        Success = false,
                        ErrorMessage = $"No formatter available for format: {exportRequest.Format}"
                    };
                    OnError?.Invoke(errorResult.ErrorMessage);
                    return errorResult;
                }

                OnExportProgress?.Invoke("Exporting section...", 0.5f);

                ExportOperationResult result = await formatter.FormatAsync(exportContent, exportRequest.Options);
                result.Format = exportRequest.Format;
                result.OutputPath = exportRequest.OutputPath;

                if (result.Success)
                {
                    await WriteExportFilesAsync(result, exportRequest);
                    OnExportProgress?.Invoke("Section export completed!", 1.0f);
                    OnExportComplete?.Invoke(result);
                }
                else
                {
                    OnError?.Invoke(result.ErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                ExportOperationResult errorResult = new ExportOperationResult(exportRequest.Format, exportRequest.OutputPath)
                {
                    Success = false,
                    ErrorMessage = $"Section export failed: {ex.Message}"
                };
                OnError?.Invoke(errorResult.ErrorMessage);
                Debug.LogError($"ExportService section error: {ex}");
                return errorResult;
            }
        }

        public async Task<ExportOperationResult> ExportMultipleSectionsAsync(List<DocumentationSectionData> sections, ExportRequest exportRequest)
        {
            if (sections == null || sections.Count == 0)
            {
                ExportOperationResult errorResult = new ExportOperationResult(exportRequest.Format, exportRequest.OutputPath)
                {
                    Success = false,
                    ErrorMessage = "No sections provided for export"
                };
                OnError?.Invoke(errorResult.ErrorMessage);
                return errorResult;
            }

            try
            {
                OnExportProgress?.Invoke("Preparing multi-section export...", 0.1f);

                // Create export content with provided sections
                ExportContent exportContent = new ExportContent
                {
                    Title = "Multi-Section Documentation",
                    Sections = sections.Where(s => s.IsEnabled).ToList()
                };

                IExportFormatter formatter = GetFormatter(exportRequest.Format);
                if (formatter == null)
                {
                    ExportOperationResult errorResult = new ExportOperationResult(exportRequest.Format, exportRequest.OutputPath)
                    {
                        Success = false,
                        ErrorMessage = $"No formatter available for format: {exportRequest.Format}"
                    };
                    OnError?.Invoke(errorResult.ErrorMessage);
                    return errorResult;
                }

                OnExportProgress?.Invoke("Exporting sections...", 0.5f);

                ExportOperationResult result = await formatter.FormatAsync(exportContent, exportRequest.Options);
                result.Format = exportRequest.Format;
                result.OutputPath = exportRequest.OutputPath;

                if (result.Success)
                {
                    await WriteExportFilesAsync(result, exportRequest);
                    OnExportProgress?.Invoke("Multi-section export completed!", 1.0f);
                    OnExportComplete?.Invoke(result);
                }
                else
                {
                    OnError?.Invoke(result.ErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                ExportOperationResult errorResult = new ExportOperationResult(exportRequest.Format, exportRequest.OutputPath)
                {
                    Success = false,
                    ErrorMessage = $"Multi-section export failed: {ex.Message}"
                };
                OnError?.Invoke(errorResult.ErrorMessage);
                Debug.LogError($"ExportService multi-section error: {ex}");
                return errorResult;
            }
        }

        public async Task<ValidationResult> ValidateExportRequestAsync(ExportRequest exportRequest)
        {
            ValidationResult validation = new ValidationResult { IsValid = true };

            if (exportRequest == null)
            {
                validation.IsValid = false;
                validation.Errors.Add("Export request cannot be null");
                return validation;
            }

            // Validate format support
            if (!_formatters.ContainsKey(exportRequest.Format))
            {
                validation.IsValid = false;
                validation.Errors.Add($"Export format '{exportRequest.Format}' is not supported");
            }

            // Validate output path
            if (string.IsNullOrEmpty(exportRequest.OutputPath))
            {
                validation.Warnings.Add("Output path is empty - using current directory");
            }
            else if (!IsValidPath(exportRequest.OutputPath))
            {
                validation.IsValid = false;
                validation.Errors.Add($"Invalid output path: {exportRequest.OutputPath}");
            }

            // Validate filename
            if (string.IsNullOrEmpty(exportRequest.FileName))
            {
                validation.Warnings.Add("Filename is empty - using default name");
            }
            else if (!IsValidFileName(exportRequest.FileName))
            {
                validation.IsValid = false;
                validation.Errors.Add($"Invalid filename: {exportRequest.FileName}");
            }

            return validation;
        }

        public async Task<ExportPreview> GeneratePreviewAsync(ProjectData projectData, ExportFormat format)
        {
            try
            {
                ExportPreview preview = new ExportPreview
                {
                    Format = format
                };

                if (projectData == null)
                {
                    preview.PreviewText = "No project data available for preview";
                    return preview;
                }

                ExportContent exportContent = BuildExportContent(projectData, new ExportRequest(format, ""));
                IExportFormatter formatter = GetFormatter(format);

                if (formatter != null)
                {
                    // Generate a quick preview
                    ExportOptions options = new ExportOptions
                    {
                        IncludeTableOfContents = true,
                        IncludeMetadata = true
                    };

                    // Create preview text (first 500 characters)
                    ExportOperationResult result = await formatter.FormatAsync(exportContent, options);
                    if (result.Success && result.Metadata.ContainsKey("content"))
                    {
                        string content = result.Metadata["content"].ToString();
                        preview.PreviewText = content.Length > 500 ? content.Substring(0, 500) + "..." : content;
                    }

                    preview.EstimatedSize = result.TotalSizeBytes;
                    preview.EstimatedExportTime = TimeSpan.FromSeconds(2); // Rough estimate
                }

                // Build file structure preview
                preview.FileStructure = BuildFileStructurePreview(projectData, format);
                
                int enabledSections = projectData.DocumentationSections.Count(s => s.IsEnabled);
                preview.PageCount = Math.Max(1, enabledSections);

                return preview;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to generate preview: {ex.Message}");
                return new ExportPreview
                {
                    Format = format,
                    PreviewText = $"Preview generation failed: {ex.Message}"
                };
            }
        }

        public List<ExportFormat> GetSupportedFormats()
        {
            return _formatters.Keys.ToList();
        }

        public List<ExportTemplate> GetAvailableTemplates(ExportFormat format)
        {
            IExportFormatter formatter = GetFormatter(format);
            if (formatter != null)
            {
                return new List<ExportTemplate> { formatter.GetDefaultTemplate() };
            }
            return new List<ExportTemplate>();
        }

        public ExportServiceCapabilities GetCapabilities()
        {
            return _capabilities;
        }

        public async Task<string> GetOutputPathAsync(ExportRequest exportRequest)
        {
            string basePath = string.IsNullOrEmpty(exportRequest.OutputPath) ? Environment.CurrentDirectory : exportRequest.OutputPath;
            string fileName = string.IsNullOrEmpty(exportRequest.FileName) ? "documentation" : exportRequest.FileName;
            string extension = GetFileExtension(exportRequest.Format);
            
            return Path.Combine(basePath, $"{fileName}.{extension}");
        }

        public bool CanExport(ExportFormat format)
        {
            return _formatters.ContainsKey(format);
        }

        private void InitializeFormatters()
        {
            _formatters[ExportFormat.Markdown] = new MarkdownExporter();
            _formatters[ExportFormat.PDF] = new PDFExporter();
            _formatters[ExportFormat.JSON] = new UnityAssetExporter(); // Unity assets export as JSON-based
        }

        private void InitializeCapabilities()
        {
            _capabilities.SupportedFormats = GetSupportedFormats();
            _capabilities.SupportedScopes = new List<ExportScope>
            {
                ExportScope.AllSections,
                ExportScope.EnabledSections,
                ExportScope.CompletedSections,
                ExportScope.SelectedSections,
                ExportScope.SingleSection
            };
            
            _capabilities.SupportsCustomTemplates = true;
            _capabilities.SupportsPreview = true;
            _capabilities.SupportsProgressTracking = true;
            _capabilities.SupportsBatchExport = true;
            _capabilities.SupportsCompression = false;
            _capabilities.SupportsCustomStyling = true;

            _capabilities.Features["AsyncExport"] = true;
            _capabilities.Features["ValidationSupport"] = true;
            _capabilities.Features["MetadataSupport"] = true;
            _capabilities.Features["StatisticsGeneration"] = true;

            // Format-specific features
            _capabilities.FormatSpecificFeatures[ExportFormat.Markdown] = new List<string> { "Templates", "Emoji", "CodeBlocks" };
            _capabilities.FormatSpecificFeatures[ExportFormat.PDF] = new List<string> { "Styling", "PageNumbers", "TOC", "Bookmarks" };
            _capabilities.FormatSpecificFeatures[ExportFormat.JSON] = new List<string> { "ScriptableObjects", "UnityIntegration" };
        }

        private IExportFormatter GetFormatter(ExportFormat format)
        {
            return _formatters.ContainsKey(format) ? _formatters[format] : null;
        }

        private ExportContent BuildExportContent(ProjectData projectData, ExportRequest exportRequest)
        {
            ExportContent content = new ExportContent(projectData);

            // Filter sections based on scope
            switch (exportRequest.Scope)
            {
                case ExportScope.AllSections:
                    content.Sections = projectData.DocumentationSections.ToList();
                    break;
                case ExportScope.EnabledSections:
                    content.Sections = projectData.DocumentationSections.Where(s => s.IsEnabled).ToList();
                    break;
                case ExportScope.CompletedSections:
                    content.Sections = projectData.DocumentationSections.Where(s => s.Status == DocumentationStatus.Completed).ToList();
                    break;
                case ExportScope.SelectedSections:
                    content.Sections = projectData.DocumentationSections
                        .Where(s => exportRequest.IncludedSections.Contains(s.SectionType))
                        .ToList();
                    break;
                default:
                    content.Sections = projectData.DocumentationSections.Where(s => s.IsEnabled).ToList();
                    break;
            }

            // Add custom variables
            if (exportRequest.CustomVariables != null)
            {
                foreach (KeyValuePair<string, object> kvp in exportRequest.CustomVariables)
                {
                    content.Variables[kvp.Key] = kvp.Value;
                }
            }

            return content;
        }

        private async Task WriteExportFilesAsync(ExportOperationResult result, ExportRequest exportRequest)
        {
            try
            {
                // Ensure output directory exists
                string outputDir = Path.GetDirectoryName(exportRequest.OutputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                // Handle different export formats
                if (result.Format == ExportFormat.JSON) // Unity assets are handled differently
                {
                    // Unity assets are written by the UnityAssetExporter directly
                    return;
                }

                // Write content-based exports (Markdown, PDF, etc.)
                if (result.Metadata.ContainsKey("content"))
                {
                    string content = result.Metadata["content"].ToString();
                    string outputPath = await GetOutputPathAsync(exportRequest);
                    
                    await File.WriteAllTextAsync(outputPath, content);
                    result.GeneratedFiles.Clear();
                    result.GeneratedFiles.Add(outputPath);
                }

                // Handle HTML content for PDF (would need actual PDF generation library)
                if (result.Format == ExportFormat.PDF && result.Metadata.ContainsKey("html_content"))
                {
                    string htmlContent = result.Metadata["html_content"].ToString();
                    string htmlPath = Path.ChangeExtension(await GetOutputPathAsync(exportRequest), "html");
                    
                    await File.WriteAllTextAsync(htmlPath, htmlContent);
                    
                    // Note: Actual PDF generation would require a library like wkhtmltopdf or similar
                    // For now, we're just creating the HTML file
                    Debug.LogWarning("PDF generation requires additional libraries. HTML file created instead.");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Failed to write export files: {ex.Message}";
                Debug.LogError($"Write files error: {ex}");
            }
        }

        private List<string> BuildFileStructurePreview(ProjectData projectData, ExportFormat format)
        {
            List<string> structure = new List<string>();
            string extension = GetFileExtension(format);
            
            structure.Add($"{projectData.ProjectName}.{extension}");
            
            if (format == ExportFormat.JSON) // Unity assets
            {
                structure.Add($"  ├── {projectData.ProjectName}_ProjectData.asset");
                structure.Add("  ├── Sections/");
                
                foreach (DocumentationSectionData section in projectData.DocumentationSections.Where(s => s.IsEnabled))
                {
                    structure.Add($"  │   ├── {section.SectionType}_Section.asset");
                }
                
                structure.Add($"  └── {projectData.ProjectName}_Documentation.asset");
            }
            
            return structure;
        }

        private string GetFileExtension(ExportFormat format)
        {
            return format switch
            {
                ExportFormat.Markdown => "md",
                ExportFormat.PDF => "pdf",
                ExportFormat.HTML => "html",
                ExportFormat.JSON => "asset",
                ExportFormat.XML => "xml",
                ExportFormat.Word => "docx",
                _ => "txt"
            };
        }

        private bool IsValidPath(string path)
        {
            try
            {
                Path.GetFullPath(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
            
            char[] invalidChars = Path.GetInvalidFileNameChars();
            return !fileName.Any(c => invalidChars.Contains(c));
        }
    }
}