using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.API
{
    public interface ITemplateManager
    {
        event Action<TemplateOperationResult> OnTemplateApplied;
        event Action<string, float> OnTemplateProgress;
        event Action<string> OnError;

        Task<List<ProjectTemplate>> GetAvailableTemplatesAsync();
        Task<List<ProjectTemplate>> GetTemplatesForProjectType(ProjectType projectType);
        Task<ProjectTemplate> GetTemplateByIdAsync(string templateId);
        
        Task<TemplateOperationResult> ApplyTemplateAsync(ProjectData projectData, ProjectTemplate template);
        Task<TemplateOperationResult> ApplyMultipleTemplatesAsync(ProjectData projectData, List<ProjectTemplate> templates);
        
        Task<ValidationResult> ValidateTemplateAsync(ProjectTemplate template);
        Task<ValidationResult> ValidateTemplateCompatibilityAsync(ProjectTemplate template, ProjectData projectData);
        
        Task<ProjectTemplate> CreateTemplateFromProjectAsync(ProjectData projectData, string templateName);
        Task<TemplateOperationResult> SaveTemplateAsync(ProjectTemplate template);
        Task<TemplateOperationResult> DeleteTemplateAsync(string templateId);
        
        List<TemplateConflict> DetectConflicts(ProjectData projectData, ProjectTemplate template);
        Task<TemplateOperationResult> ResolveConflictsAsync(List<TemplateConflict> conflicts, ConflictResolution resolution);
        
        TemplateManagerCapabilities GetCapabilities();
    }

    public interface IFolderStructureManager
    {
        Task<FolderOperationResult> CreateFolderStructureAsync(FolderStructureData folderStructure, string basePath);
        Task<ValidationResult> ValidateFolderStructureAsync(FolderStructureData folderStructure);
        Task<FolderStructureData> AnalyzeExistingStructureAsync(string projectPath);
        
        List<string> GetStandardUnityFolders();
        FolderStructureData CreateStandardStructure(ProjectType projectType);
        
        bool FolderExists(string path);
        Task<FolderOperationResult> CreateFolderAsync(string path);
        Task<FolderOperationResult> MoveFolderAsync(string sourcePath, string destinationPath);
        Task<FolderOperationResult> DeleteFolderAsync(string path);
    }

    [Serializable]
    public class TemplateOperationResult
    {
        public bool Success { get; set; }
        public string TemplateId { get; set; }
        public string TemplateName { get; set; }
        public List<string> CreatedFolders { get; set; }
        public List<string> CreatedFiles { get; set; }
        public List<string> ModifiedFiles { get; set; }
        public List<TemplateConflict> ResolvedConflicts { get; set; }
        public TimeSpan OperationTime { get; set; }
        public DateTime AppliedAt { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationResult ValidationResult { get; set; }

        public TemplateOperationResult()
        {
            CreatedFolders = new List<string>();
            CreatedFiles = new List<string>();
            ModifiedFiles = new List<string>();
            ResolvedConflicts = new List<TemplateConflict>();
            AppliedAt = DateTime.Now;
        }

        public TemplateOperationResult(string templateId, string templateName) : this()
        {
            TemplateId = templateId;
            TemplateName = templateName;
        }

        public int TotalChanges => CreatedFolders.Count + CreatedFiles.Count + ModifiedFiles.Count;
        public bool HasConflicts => ResolvedConflicts.Count > 0;
    }

    [Serializable]
    public class FolderOperationResult
    {
        public bool Success { get; set; }
        public string Path { get; set; }
        public FolderOperationType OperationType { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime OperationTime { get; set; }
        public List<string> AffectedPaths { get; set; }

        public FolderOperationResult()
        {
            OperationTime = DateTime.Now;
            AffectedPaths = new List<string>();
        }

        public FolderOperationResult(FolderOperationType type, string path) : this()
        {
            OperationType = type;
            Path = path;
        }
    }

    public enum FolderOperationType
    {
        Create,
        Move,
        Delete,
        Modify,
        Validate
    }

    [Serializable]
    public class TemplateConflict
    {
        public TemplateConflictType ConflictType { get; set; }
        public string ResourcePath { get; set; }
        public string ExistingContent { get; set; }
        public string TemplateContent { get; set; }
        public ConflictResolution RecommendedResolution { get; set; }
        public string Description { get; set; }
        public TemplateSeverity Severity { get; set; }

        public TemplateConflict()
        {
            Severity = TemplateSeverity.Warning;
            RecommendedResolution = ConflictResolution.Skip;
        }

        public TemplateConflict(TemplateConflictType type, string path, string description) : this()
        {
            ConflictType = type;
            ResourcePath = path;
            Description = description;
        }
    }

    public enum TemplateConflictType
    {
        FileExists,
        FolderExists,
        PackageConflict,
        AssemblyDefinitionConflict,
        SceneConflict,
        SettingsConflict
    }

    public enum ConflictResolution
    {
        Skip,
        Overwrite,
        Merge,
        Rename,
        Backup
    }

    public enum TemplateSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }

    [Serializable]
    public class TemplateManagerCapabilities
    {
        public List<ProjectType> SupportedProjectTypes { get; set; }
        public bool SupportsCustomTemplates { get; set; }
        public bool SupportsTemplateValidation { get; set; }
        public bool SupportsConflictResolution { get; set; }
        public bool SupportsProgressTracking { get; set; }
        public bool SupportsUndo { get; set; }
        public bool SupportsBackup { get; set; }
        public List<string> SupportedFileTypes { get; set; }
        public Dictionary<string, bool> Features { get; set; }

        public TemplateManagerCapabilities()
        {
            SupportedProjectTypes = new List<ProjectType>();
            SupportedFileTypes = new List<string>();
            Features = new Dictionary<string, bool>();
        }

        public bool HasFeature(string featureName)
        {
            return Features.ContainsKey(featureName) && Features[featureName];
        }
    }

    public static class TemplateManagerExtensions
    {
        public static async Task<bool> IsTemplateCompatible(
            this ITemplateManager templateManager, 
            ProjectTemplate template, 
            ProjectData projectData)
        {
            ValidationResult validationResult = await templateManager.ValidateTemplateCompatibilityAsync(template, projectData);
            return validationResult.IsValid;
        }

        public static bool HasProjectType(this ProjectTemplate template, ProjectType projectType)
        {
            return template.TargetProjectType == projectType || template.TargetProjectType == ProjectType.General;
        }

        public static List<TemplateConflict> GetCriticalConflicts(this List<TemplateConflict> conflicts)
        {
            return conflicts.Where(c => c.Severity >= TemplateSeverity.Error).ToList();
        }

        public static string GetFormattedReport(this TemplateOperationResult result)
        {
            var report = $"📋 Template Operation Report: {result.TemplateName}\n";
            report += $"✅ Success: {result.Success}\n";
            report += $"⏱️ Time: {result.OperationTime.TotalSeconds:F1}s\n";
            report += $"📁 Created Folders: {result.CreatedFolders.Count}\n";
            report += $"📄 Created Files: {result.CreatedFiles.Count}\n";
            report += $"📝 Modified Files: {result.ModifiedFiles.Count}\n";

            if (result.HasConflicts)
            {
                report += $"⚠️ Resolved Conflicts: {result.ResolvedConflicts.Count}\n";
                foreach (string conflict in result.ResolvedConflicts)
                {
                    report += $"  • {conflict.ConflictType}: {conflict.ResourcePath}\n";
                }
            }

            if (!result.Success)
            {
                report += $"❌ Error: {result.ErrorMessage}\n";
            }

            return report;
        }

        public static async Task<List<ProjectTemplate>> GetRecommendedTemplates(
            this ITemplateManager templateManager,
            ProjectData projectData)
        {
            List<ProjectTemplate> allTemplates = await templateManager.GetAvailableTemplatesAsync();
            List<ProjectTemplate> recommended = new List<ProjectTemplate>();

            foreach (string template in allTemplates)
            {
                if (template.HasProjectType(projectData.ProjectType))
                {
                    ValidationResult validationResult = await templateManager.ValidateTemplateCompatibilityAsync(template, projectData);
                    if (validationResult.IsValid || validationResult.WarningCount <= 2)
                    {
                        recommended.Add(template);
                    }
                }
            }

            return recommended.OrderBy(t => t.TargetProjectType == projectData.ProjectType ? 0 : 1)
                             .ThenBy(t => t.TemplateName)
                             .ToList();
        }
    }
}