using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;
using Newtonsoft.Json;
using UnityProjectArchitect.AI;

namespace UnityProjectArchitect.Services
{
    public class UnityAssetExporter : IExportFormatter
    {
        public ExportFormat SupportedFormat => ExportFormat.JSON; // Using JSON as Unity asset export format
        public string DisplayName => "Unity Asset";
        public string FileExtension => "json";
        public string MimeType => "application/json";

        private readonly Dictionary<string, AssetTemplate> _templates;

        public UnityAssetExporter()
        {
            _templates = new Dictionary<string, AssetTemplate>();
            InitializeDefaultTemplates();
        }

        public async Task<ExportOperationResult> FormatAsync(ExportContent content, ExportOptions options)
        {
            ExportOperationResult result = new ExportOperationResult(ExportFormat.JSON, "");
            DateTime startTime = DateTime.Now;

            try
            {
                string templateId = GetOption<string>(options, "TemplateId", "default");
                AssetTemplate template = GetTemplate(templateId);
                
                List<string> exportedAssets = new List<string>();
                long totalSizeBytes = 0L;

                // Export ProjectData as ScriptableObject
                if (content.ProjectData != null)
                {
                    ExportedAsset projectAsset = await ExportProjectDataAsync(content.ProjectData, options);
                    if (projectAsset != null)
                    {
                        exportedAssets.Add(projectAsset.AssetPath);
                        totalSizeBytes += projectAsset.SizeBytes;
                    }
                }

                // Export each documentation section as individual ScriptableObjects
                if (content.Sections != null && content.Sections.Count > 0)
                {
                    foreach (DocumentationSectionData section in content.Sections.Where(s => s.IsEnabled))
                    {
                        ExportedAsset sectionAsset = await ExportDocumentationSectionAsync(section, options);
                        if (sectionAsset != null)
                        {
                            exportedAssets.Add(sectionAsset.AssetPath);
                            totalSizeBytes += sectionAsset.SizeBytes;
                        }
                    }
                }

                // Export documentation collection as master asset
                ExportedAsset collectionAsset = await ExportDocumentationCollectionAsync(content, options);
                if (collectionAsset != null)
                {
                    exportedAssets.Add(collectionAsset.AssetPath);
                    totalSizeBytes += collectionAsset.SizeBytes;
                }

                result.Success = true;
                result.GeneratedFiles = exportedAssets;
                result.TotalSizeBytes = totalSizeBytes;
                result.ExportTime = DateTime.Now - startTime;
                result.Statistics = GenerateStatistics(content, exportedAssets.Count);

                // Store export information in metadata
                result.Metadata["exported_assets"] = exportedAssets;
                result.Metadata["template_used"] = templateId;

                // Log completion (Unity-independent logging would be injected via ILogger)
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Unity Asset export failed: {ex.Message}";
                // Log error (Unity-independent logging would be injected via ILogger)
            }

            return result;
        }

        public ValidationResult ValidateContent(ExportContent content)
        {
            ValidationResult validation = new ValidationResult { IsValid = true };

            if (content == null)
            {
                validation.IsValid = false;
                validation.Errors.Add("Export content cannot be null");
                return validation;
            }

            // Note: This Unity-independent version exports to JSON format

            if (content.ProjectData == null)
            {
                validation.Warnings.Add("No project data to export");
            }

            if (content.Sections == null || content.Sections.Count == 0)
            {
                validation.Warnings.Add("No documentation sections to export");
            }

            // Validate asset paths
            string basePath = GetOption<string>(null, "OutputPath", "Documentation/Generated/");
            if (!IsValidAssetPath(basePath))
            {
                validation.Errors.Add($"Invalid asset output path: {basePath}");
            }

            return validation;
        }

        public ExportTemplate GetDefaultTemplate()
        {
            return new ExportTemplate("default", "Default Unity Asset", ExportFormat.JSON)
            {
                Description = "Standard Unity ScriptableObject export template",
                Author = "Unity Project Architect",
                Version = "1.0",
                TemplateContent = GetDefaultTemplateContent()
            };
        }

        public List<ExportOption> GetAvailableOptions()
        {
            return new List<ExportOption>
            {
                new ExportOption { Name = "TemplateId", Description = "Template to use for asset export", IsEnabled = true },
                new ExportOption { Name = "OutputPath", Description = "Path to save exported assets", IsEnabled = true },
                new ExportOption { Name = "CreateMenuItems", Description = "Generate Unity menu items for assets", IsEnabled = false },
                new ExportOption { Name = "IncludeMetadata", Description = "Include export metadata in assets", IsEnabled = true },
                new ExportOption { Name = "OverwriteExisting", Description = "Overwrite existing assets", IsEnabled = false },
                new ExportOption { Name = "RefreshAssetDatabase", Description = "Refresh Unity asset database after export", IsEnabled = true },
                new ExportOption { Name = "CreateFolders", Description = "Create folder structure for organization", IsEnabled = true },
                new ExportOption { Name = "AssetNamingConvention", Description = "Naming convention: kebab-case, PascalCase, camelCase", IsEnabled = true }
            };
        }

        private async Task<ExportedAsset> ExportProjectDataAsync(ProjectData projectData, ExportOptions options)
        {
            try
            {
                string outputPath = GetOption<string>(options, "OutputPath", "Documentation/Generated/");
                string fileName = SanitizeFileName($"{projectData.ProjectName}_ProjectData");
                string assetPath = Path.Combine(outputPath, $"{fileName}.json");

                // Ensure directory exists
                string directory = Path.GetDirectoryName(assetPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Create a copy of the project data for export
                ProjectData exportedData = JsonConvert.DeserializeObject<ProjectData>(JsonConvert.SerializeObject(projectData));

                // Add export metadata
                if (GetOption<bool>(options, "IncludeMetadata", true))
                {
                    // Note: This would require extending ProjectData with export metadata fields
                    // For now, we'll add a comment or use the existing metadata fields
                }

                // Save as JSON file instead of Unity asset
                string jsonContent = JsonConvert.SerializeObject(exportedData, Formatting.Indented);
                await File.WriteAllTextAsync(assetPath.Replace(".asset", ".json"), jsonContent);

                FileInfo fileInfo = new FileInfo(assetPath);
                return new ExportedAsset
                {
                    AssetPath = assetPath,
                    AssetType = "ProjectData",
                    SizeBytes = fileInfo.Exists ? fileInfo.Length : 0
                };
            }
            catch (Exception ex)
            {
                // Log error (Unity-independent logging would be injected via ILogger)
                return null;
            }
        }

        private async Task<ExportedAsset> ExportDocumentationSectionAsync(DocumentationSectionData section, ExportOptions options)
        {
            try
            {
                string outputPath = GetOption<string>(options, "OutputPath", "Documentation/Generated/");
                string fileName = SanitizeFileName($"{section.SectionType}_Section");
                string assetPath = Path.Combine(outputPath, "Sections", $"{fileName}.json");

                // Ensure directory exists
                string directory = Path.GetDirectoryName(assetPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Create a DocumentationSection wrapper object
                DocumentationSectionAsset sectionAsset = new DocumentationSectionAsset();
                sectionAsset.Initialize(section);

                // Save as JSON file instead of Unity asset
                string jsonContent = JsonConvert.SerializeObject(sectionAsset, Formatting.Indented);
                await File.WriteAllTextAsync(assetPath.Replace(".asset", ".json"), jsonContent);

                FileInfo fileInfo = new FileInfo(assetPath);
                return new ExportedAsset
                {
                    AssetPath = assetPath,
                    AssetType = "DocumentationSection",
                    SizeBytes = fileInfo.Exists ? fileInfo.Length : 0
                };
            }
            catch (Exception ex)
            {
                // Log error (Unity-independent logging would be injected via ILogger)
                return null;
            }
        }

        private async Task<ExportedAsset> ExportDocumentationCollectionAsync(ExportContent content, ExportOptions options)
        {
            try
            {
                string outputPath = GetOption<string>(options, "OutputPath", "Documentation/Generated/");
                string fileName = SanitizeFileName($"{content.Title}_Documentation");
                string assetPath = Path.Combine(outputPath, $"{fileName}.json");

                // Ensure directory exists
                string directory = Path.GetDirectoryName(assetPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Create a DocumentationCollection object
                DocumentationCollectionAsset collectionAsset = new DocumentationCollectionAsset();
                collectionAsset.Initialize(content);

                // Save as JSON file instead of Unity asset
                string jsonContent = JsonConvert.SerializeObject(collectionAsset, Formatting.Indented);
                await File.WriteAllTextAsync(assetPath.Replace(".asset", ".json"), jsonContent);

                FileInfo fileInfo = new FileInfo(assetPath);
                return new ExportedAsset
                {
                    AssetPath = assetPath,
                    AssetType = "DocumentationCollection",
                    SizeBytes = fileInfo.Exists ? fileInfo.Length : 0
                };
            }
            catch (Exception ex)
            {
                // Log error (Unity-independent logging would be injected via ILogger)
                return null;
            }
        }

        private string SanitizeFileName(string fileName)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            string sanitized = string.Concat(fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
            return sanitized.Replace(" ", "_");
        }

        private bool IsValidAssetPath(string path)
        {
            return !string.IsNullOrEmpty(path);
        }

        private T GetOption<T>(ExportOptions options, string key, T defaultValue)
        {
            if (options?.FormatSpecificOptions?.ContainsKey(key) == true)
            {
                try
                {
                    return (T)Convert.ChangeType(options.FormatSpecificOptions[key], typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        private ExportStatistics GenerateStatistics(ExportContent content, int exportedAssetCount)
        {
            ExportStatistics stats = new ExportStatistics();
            
            if (content.Sections != null)
            {
                stats.TotalSections = content.Sections.Count;
                stats.ExportedSections = content.Sections.Count(s => s.IsEnabled && !string.IsNullOrEmpty(s.Content));
                stats.SkippedSections = stats.TotalSections - stats.ExportedSections;
            }

            // For Unity assets, pages represent the number of assets created
            stats.TotalPages = exportedAssetCount;

            return stats;
        }

        private void InitializeDefaultTemplates()
        {
            _templates["default"] = new AssetTemplate
            {
                Id = "default",
                Name = "Default",
                Description = "Standard ScriptableObject export with all data"
            };

            _templates["minimal"] = new AssetTemplate
            {
                Id = "minimal",
                Name = "Minimal",
                Description = "Minimal ScriptableObject export with essential data only"
            };
        }

        private AssetTemplate GetTemplate(string templateId)
        {
            return _templates.ContainsKey(templateId) ? _templates[templateId] : _templates["default"];
        }

        private string GetDefaultTemplateContent()
        {
            return "Standard Unity ScriptableObject export template for project documentation.";
        }

        private class AssetTemplate
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        private class ExportedAsset
        {
            public string AssetPath { get; set; }
            public string AssetType { get; set; }
            public long SizeBytes { get; set; }
        }

    }

    // Helper classes for Unity Asset export (Unity-independent)
    public class DocumentationSectionAsset
    {
        // Section Information
        private DocumentationSectionType _sectionType;
        private string _sectionTitle;
        private string _sectionContent;
        private DocumentationStatus _status;
        private DateTime _lastUpdated;
        private int _wordCount;
        
        // Export Metadata
        private DateTime _exportedAt;
        private string _exportedBy;

        public DocumentationSectionType SectionType => _sectionType;
        public string SectionTitle => _sectionTitle;
        public string SectionContent => _sectionContent;
        public DocumentationStatus Status => _status;
        public DateTime LastUpdated => _lastUpdated;
        public int WordCount => _wordCount;
        public DateTime ExportedAt => _exportedAt;
        public string ExportedBy => _exportedBy;

        public void Initialize(DocumentationSectionData sectionData)
        {
            _sectionType = sectionData.SectionType;
            _sectionTitle = sectionData.Title;
            _sectionContent = sectionData.Content;
            _status = sectionData.Status;
            _lastUpdated = sectionData.LastUpdated;
            _wordCount = CalculateWordCount(sectionData.Content);
            _exportedAt = DateTime.Now;
            _exportedBy = "Unity Project Architect";
        }

        private int CalculateWordCount(string content)
        {
            if (string.IsNullOrEmpty(content))
                return 0;
            return content.Split(new char[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }

    public class DocumentationCollectionAsset
    {
        // Collection Information
        private string _collectionTitle;
        private string _projectName;
        private string _projectVersion;
        private int _totalSections;
        private int _completedSections;
        
        // Export Metadata
        private DateTime _exportedAt;
        private string _exportedBy;
        private List<string> _includedSectionTypes;

        public string CollectionTitle => _collectionTitle;
        public string ProjectName => _projectName;
        public string ProjectVersion => _projectVersion;
        public int TotalSections => _totalSections;
        public int CompletedSections => _completedSections;
        public DateTime ExportedAt => _exportedAt;
        public string ExportedBy => _exportedBy;
        public List<string> IncludedSectionTypes => _includedSectionTypes;

        private int CalculateWordCount(string content)
        {
            if (string.IsNullOrEmpty(content))
                return 0;
            return content.Split(new char[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public void Initialize(ExportContent content)
        {
            _collectionTitle = content.Title ?? "Documentation";
            _projectName = content.ProjectData?.ProjectName ?? "Unknown Project";
            _projectVersion = content.ProjectData?.ProjectVersion ?? "1.0.0";
            _totalSections = content.Sections?.Count ?? 0;
            _completedSections = content.Sections?.Count(s => s.Status == DocumentationStatus.Completed) ?? 0;
            _exportedAt = DateTime.Now;
            _exportedBy = "Unity Project Architect";
            _includedSectionTypes = content.Sections?.Where(s => s.IsEnabled)
                                                   .Select(s => s.SectionType.ToString())
                                                   .ToList() ?? new List<string>();
        }
    }
}