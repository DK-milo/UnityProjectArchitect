using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityProjectArchitect.Core;
using UnityProjectArchitect.Services;

namespace UnityProjectArchitect.Unity
{
    /// <summary>
    /// Bridge between Unity and Core DLL services
    /// Handles Unity-specific implementations of DLL interfaces
    /// </summary>
    public static class UnityServiceBridge
    {
        private static IProjectAnalyzer _projectAnalyzer;
        private static IExportService _exportService;
        private static ITemplateManager _templateManager;
        
        /// <summary>
        /// Initialize Unity service implementations
        /// This will be called from Unity Editor windows
        /// </summary>
        public static void Initialize()
        {
            try
            {
                // Initialize real DLL services
                _projectAnalyzer = new ProjectAnalyzer();
                _exportService = new ExportService();
                _templateManager = new TemplateManager();
                UnityEngine.Debug.Log("Unity Project Architect Service Bridge initialized with DLL services");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to initialize Unity Project Architect services: {ex.Message}");
                UnityEngine.Debug.LogError($"Stack trace: {ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// Get or create project analyzer instance
        /// </summary>
        public static IProjectAnalyzer GetProjectAnalyzer()
        {
            if (_projectAnalyzer == null)
            {
                Initialize();
            }
            return _projectAnalyzer ?? new MockProjectAnalyzer();
        }
        
        /// <summary>
        /// Get or create export service instance
        /// </summary>
        public static IExportService GetExportService()
        {
            if (_exportService == null)
            {
                Initialize();
            }
            return _exportService ?? new MockExportService();
        }
        
        /// <summary>
        /// Get or create template manager instance
        /// </summary>
        public static ITemplateManager GetTemplateManager()
        {
            if (_templateManager == null)
            {
                Initialize();
            }
            return _templateManager;
        }
        
        /// <summary>
        /// Create a validation result for Unity display
        /// </summary>
        public static void ShowValidationResult(ValidationResult result)
        {
            if (result.IsValid)
            {
                if (result.Issues != null && result.Issues.Count > 0)
                {
                    UnityEngine.Debug.Log($"✅ Validation completed with {result.Issues.Count} informational items");
                    foreach (var issue in result.Issues)
                    {
                        UnityEngine.Debug.Log($"- {issue.Type}: {issue.Message}");
                    }
                }
                else
                {
                    UnityEngine.Debug.Log("✅ Validation completed successfully - no issues found");
                }
            }
            else
            {
                int issueCount = result.Issues?.Count ?? 0;
                UnityEngine.Debug.LogWarning($"⚠️ Validation issues found: {issueCount} issues");
                if (result.Issues != null)
                {
                    foreach (var issue in result.Issues)
                    {
                        UnityEngine.Debug.LogWarning($"- {issue.Type}: {issue.Message}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Display progress in Unity
        /// </summary>
        public static void ShowProgress(string operation, float progress)
        {
            if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(
                "Unity Project Architect", 
                operation, 
                progress))
            {
                UnityEngine.Debug.Log("Operation cancelled by user");
            }
        }
        
        /// <summary>
        /// Clear Unity progress bar
        /// </summary>
        public static void ClearProgress()
        {
            UnityEditor.EditorUtility.ClearProgressBar();
        }
    }
    
    #region Mock Implementations (Fallback Only)
    
    /// <summary>
    /// Minimal mock implementation - only used as fallback if real services fail to initialize
    /// </summary>
    internal class MockProjectAnalyzer : IProjectAnalyzer
    {
        public event System.Action<ProjectAnalysisResult> OnAnalysisComplete;
        public event System.Action<string, float> OnAnalysisProgress;
        public event System.Action<string> OnError;

        public async Task<ProjectAnalysisResult> AnalyzeProjectAsync(string projectPath)
        {
            UnityEngine.Debug.LogWarning("Using mock ProjectAnalyzer - real services failed to initialize");
            await Task.Delay(100);
            return new ProjectAnalysisResult { Success = false, ErrorMessage = "Mock implementation" };
        }
        
        public Task<ProjectAnalysisResult> AnalyzeProjectDataAsync(ProjectData projectData)
        {
            return Task.FromResult(new ProjectAnalysisResult { Success = false, ErrorMessage = "Mock implementation" });
        }
        
        public Task<ScriptAnalysisResult> AnalyzeScriptsAsync(string projectPath)
        {
            return Task.FromResult(new ScriptAnalysisResult());
        }
        
        public Task<AssetAnalysisResult> AnalyzeAssetsAsync(string projectPath)
        {
            return Task.FromResult(new AssetAnalysisResult());
        }
        
        public Task<ArchitectureAnalysisResult> AnalyzeArchitectureAsync(ProjectData projectData)
        {
            return Task.FromResult(new ArchitectureAnalysisResult());
        }
        
        public Task<ValidationResult> ValidateProjectStructureAsync(string projectPath)
        {
            return Task.FromResult(new ValidationResult { IsValid = false });
        }
        
        public Task<System.Collections.Generic.List<ProjectInsight>> GetInsightsAsync(ProjectAnalysisResult analysisResult)
        {
            return Task.FromResult(new System.Collections.Generic.List<ProjectInsight>());
        }
        
        public Task<System.Collections.Generic.List<ProjectRecommendation>> GetRecommendationsAsync(ProjectAnalysisResult analysisResult)
        {
            return Task.FromResult(new System.Collections.Generic.List<ProjectRecommendation>());
        }
        
        public ProjectAnalyzerCapabilities GetCapabilities()
        {
            return new ProjectAnalyzerCapabilities();
        }
        
        public bool CanAnalyze(string projectPath)
        {
            return false;
        }
    }
    
    /// <summary>
    /// Minimal mock implementation - only used as fallback if real services fail to initialize
    /// </summary>
    internal class MockExportService : IExportService
    {
        public event System.Action<ExportOperationResult> OnExportComplete;
        public event System.Action<string, float> OnExportProgress;
        public event System.Action<string> OnError;
        
        public Task<ExportOperationResult> ExportProjectDocumentationAsync(ProjectData projectData, ExportRequest exportRequest)
        {
            UnityEngine.Debug.LogWarning("Using mock ExportService - real services failed to initialize");
            var result = new ExportOperationResult(exportRequest.Format, exportRequest.OutputPath)
            {
                Success = false,
                ErrorMessage = "Mock implementation"
            };
            return Task.FromResult(result);
        }
        
        public Task<ExportOperationResult> ExportSectionAsync(DocumentationSectionData section, ExportRequest exportRequest)
        {
            var result = new ExportOperationResult(exportRequest.Format, exportRequest.OutputPath)
            {
                Success = false,
                ErrorMessage = "Mock implementation"
            };
            return Task.FromResult(result);
        }
        
        public Task<ExportOperationResult> ExportMultipleSectionsAsync(System.Collections.Generic.List<DocumentationSectionData> sections, ExportRequest exportRequest)
        {
            var result = new ExportOperationResult(exportRequest.Format, exportRequest.OutputPath)
            {
                Success = false,
                ErrorMessage = "Mock implementation"
            };
            return Task.FromResult(result);
        }
        
        public Task<ValidationResult> ValidateExportRequestAsync(ExportRequest exportRequest)
        {
            return Task.FromResult(new ValidationResult { IsValid = false });
        }
        
        public Task<ExportPreview> GeneratePreviewAsync(ProjectData projectData, ExportFormat format)
        {
            return Task.FromResult(new ExportPreview { Format = format, PreviewText = "Mock preview" });
        }
        
        public System.Collections.Generic.List<ExportFormat> GetSupportedFormats()
        {
            return new System.Collections.Generic.List<ExportFormat>();
        }
        
        public System.Collections.Generic.List<ExportTemplate> GetAvailableTemplates(ExportFormat format)
        {
            return new System.Collections.Generic.List<ExportTemplate>();
        }
        
        public ExportServiceCapabilities GetCapabilities()
        {
            return new ExportServiceCapabilities();
        }
        
        public Task<string> GetOutputPathAsync(ExportRequest exportRequest)
        {
            return Task.FromResult(exportRequest.OutputPath);
        }
        
        public bool CanExport(ExportFormat format)
        {
            return false;
        }
    }
    
    #endregion
}