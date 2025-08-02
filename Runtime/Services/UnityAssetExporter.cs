using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityProjectArchitect.Core;
using UnityProjectArchitect.API;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityProjectArchitect.Services
{
    public class UnityAssetExporter : IExportFormatter
    {
        public ExportFormat SupportedFormat => ExportFormat.JSON; // Using JSON as Unity asset export format
        public string DisplayName => "Unity Asset";
        public string FileExtension => "asset";
        public string MimeType => "application/unity";

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

                Debug.Log($"Unity Asset export completed: {exportedAssets.Count} assets, {result.FormattedSize} in {result.ExportTime.TotalSeconds:F1}s");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Unity Asset export failed: {ex.Message}";
                Debug.LogError($"UnityAssetExporter error: {ex}");
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

#if !UNITY_EDITOR
            validation.IsValid = false;
            validation.Errors.Add("Unity Asset export is only available in Unity Editor");
            return validation;
#endif

            if (content.ProjectData == null)
            {
                validation.Warnings.Add("No project data to export");
            }

            if (content.Sections == null || content.Sections.Count == 0)
            {
                validation.Warnings.Add("No documentation sections to export");
            }

            // Validate asset paths
            string basePath = GetOption<string>(null, "OutputPath", "Assets/Documentation/Generated/");
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
                new ExportOption("TemplateId", "Template", "default", "Template to use for asset export"),
                new ExportOption("OutputPath", "Output Path", "Assets/Documentation/Generated/", "Path to save exported assets"),
                new ExportOption("CreateMenuItems", "Create Menu Items", false, "Generate Unity menu items for assets"),
                new ExportOption("IncludeMetadata", "Include Metadata", true, "Include export metadata in assets"),
                new ExportOption("OverwriteExisting", "Overwrite Existing", false, "Overwrite existing assets"),
                new ExportOption("RefreshAssetDatabase", "Refresh Asset Database", true, "Refresh Unity asset database after export"),
                new ExportOption("CreateFolders", "Create Folders", true, "Create folder structure for organization"),
                new ExportOption("AssetNamingConvention", "Asset Naming", "kebab-case", "Naming convention: kebab-case, PascalCase, camelCase")
            };
        }

        private async Task<ExportedAsset> ExportProjectDataAsync(ProjectData projectData, ExportOptions options)
        {
#if UNITY_EDITOR
            try
            {
                string outputPath = GetOption<string>(options, "OutputPath", "Assets/Documentation/Generated/");
                string fileName = SanitizeFileName($"{projectData.ProjectName}_ProjectData");
                string assetPath = Path.Combine(outputPath, $"{fileName}.asset");

                // Ensure directory exists
                string directory = Path.GetDirectoryName(assetPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Create a copy of the project data for export
                ProjectData exportedData = ScriptableObject.CreateInstance<ProjectData>();
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(projectData), exportedData);

                // Add export metadata
                if (GetOption<bool>(options, "IncludeMetadata", true))
                {
                    // Note: This would require extending ProjectData with export metadata fields
                    // For now, we'll add a comment or use the existing metadata fields
                }

                AssetDatabase.CreateAsset(exportedData, assetPath);
                
                if (GetOption<bool>(options, "RefreshAssetDatabase", true))
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

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
                Debug.LogError($"Failed to export ProjectData: {ex.Message}");
                return null;
            }
#else
            await Task.CompletedTask;
            return null;
#endif
        }

        private async Task<ExportedAsset> ExportDocumentationSectionAsync(DocumentationSectionData section, ExportOptions options)
        {
#if UNITY_EDITOR
            try
            {
                string outputPath = GetOption<string>(options, "OutputPath", "Assets/Documentation/Generated/");
                string fileName = SanitizeFileName($"{section.SectionType}_Section");
                string assetPath = Path.Combine(outputPath, "Sections", $"{fileName}.asset");

                // Ensure directory exists
                string directory = Path.GetDirectoryName(assetPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Create a DocumentationSection ScriptableObject wrapper
                DocumentationSectionAsset sectionAsset = ScriptableObject.CreateInstance<DocumentationSectionAsset>();
                sectionAsset.Initialize(section);

                AssetDatabase.CreateAsset(sectionAsset, assetPath);
                
                if (GetOption<bool>(options, "RefreshAssetDatabase", true))
                {
                    AssetDatabase.SaveAssets();
                }

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
                Debug.LogError($"Failed to export DocumentationSection {section.SectionType}: {ex.Message}");
                return null;
            }
#else
            await Task.CompletedTask;
            return null;
#endif
        }

        private async Task<ExportedAsset> ExportDocumentationCollectionAsync(ExportContent content, ExportOptions options)
        {
#if UNITY_EDITOR
            try
            {
                string outputPath = GetOption<string>(options, "OutputPath", "Assets/Documentation/Generated/");
                string fileName = SanitizeFileName($"{content.Title}_Documentation");
                string assetPath = Path.Combine(outputPath, $"{fileName}.asset");

                // Ensure directory exists
                string directory = Path.GetDirectoryName(assetPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Create a DocumentationCollection ScriptableObject
                DocumentationCollectionAsset collectionAsset = ScriptableObject.CreateInstance<DocumentationCollectionAsset>();
                collectionAsset.Initialize(content);

                AssetDatabase.CreateAsset(collectionAsset, assetPath);
                
                if (GetOption<bool>(options, "RefreshAssetDatabase", true))
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

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
                Debug.LogError($"Failed to export DocumentationCollection: {ex.Message}");
                return null;
            }
#else
            await Task.CompletedTask;
            return null;
#endif
        }

        private string SanitizeFileName(string fileName)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            string sanitized = string.Concat(fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
            return sanitized.Replace(" ", "_");
        }

        private bool IsValidAssetPath(string path)
        {
            return !string.IsNullOrEmpty(path) && path.StartsWith("Assets/");
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
                stats.ExportedSections = content.Sections.Count(s => s.IsEnabled && s.HasContent);
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

        private class ExportOption
        {
            public string Key { get; set; }
            public string DisplayName { get; set; }
            public object DefaultValue { get; set; }
            public string Description { get; set; }

            public ExportOption(string key, string displayName, object defaultValue, string description)
            {
                Key = key;
                DisplayName = displayName;
                DefaultValue = defaultValue;
                Description = description;
            }
        }
    }

    // Helper ScriptableObjects for Unity Asset export
    [CreateAssetMenu(fileName = "DocumentationSection", menuName = "Unity Project Architect/Documentation Section", order = 10)]
    public class DocumentationSectionAsset : ScriptableObject
    {
        [Header("Section Information")]
        [SerializeField] private DocumentationSectionType _sectionType;
        [SerializeField] private string _sectionTitle;
        [SerializeField] private string _sectionContent;
        [SerializeField] private DocumentationStatus _status;
        [SerializeField] private DateTime _lastUpdated;
        [SerializeField] private int _wordCount;
        
        [Header("Export Metadata")]
        [SerializeField] private DateTime _exportedAt;
        [SerializeField] private string _exportedBy;

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
            _wordCount = sectionData.CurrentWordCount;
            _exportedAt = DateTime.Now;
            _exportedBy = "Unity Project Architect";
        }
    }

    [CreateAssetMenu(fileName = "DocumentationCollection", menuName = "Unity Project Architect/Documentation Collection", order = 11)]
    public class DocumentationCollectionAsset : ScriptableObject
    {
        [Header("Collection Information")]
        [SerializeField] private string _collectionTitle;
        [SerializeField] private string _projectName;
        [SerializeField] private string _projectVersion;
        [SerializeField] private int _totalSections;
        [SerializeField] private int _completedSections;
        
        [Header("Export Metadata")]
        [SerializeField] private DateTime _exportedAt;
        [SerializeField] private string _exportedBy;
        [SerializeField] private List<string> _includedSectionTypes;

        public string CollectionTitle => _collectionTitle;
        public string ProjectName => _projectName;
        public string ProjectVersion => _projectVersion;
        public int TotalSections => _totalSections;
        public int CompletedSections => _completedSections;
        public DateTime ExportedAt => _exportedAt;
        public string ExportedBy => _exportedBy;
        public List<string> IncludedSectionTypes => _includedSectionTypes;

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