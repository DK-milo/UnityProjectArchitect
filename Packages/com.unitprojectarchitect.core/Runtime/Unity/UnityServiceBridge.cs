using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityProjectArchitect.Core;
using UnityProjectArchitect.Services;
using UnityProjectArchitect.AI.Services;

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
        private static IAIAssistant _aiAssistant;
        private static UnityDocumentationService _documentationService;
        
        /// <summary>
        /// Initialize Unity service implementations
        /// This will be called from Unity Editor windows
        /// </summary>
        public static void Initialize()
        {
            try
            {
                // Initialize real DLL services
                UnityEngine.Debug.Log("UnityServiceBridge: Creating ProjectAnalyzer...");
                _projectAnalyzer = new ProjectAnalyzer();
                UnityEngine.Debug.Log("UnityServiceBridge: ✅ ProjectAnalyzer created");
                
                UnityEngine.Debug.Log("UnityServiceBridge: Creating ExportService...");
                _exportService = new ExportService();
                UnityEngine.Debug.Log("UnityServiceBridge: ✅ ExportService created");
                
                UnityEngine.Debug.Log("UnityServiceBridge: Creating TemplateManager...");
                _templateManager = new TemplateManager();
                UnityEngine.Debug.Log("UnityServiceBridge: ✅ TemplateManager created");
                
                UnityEngine.Debug.Log("UnityServiceBridge: Creating AIAssistant...");
                _aiAssistant = new AIAssistant();
                UnityEngine.Debug.Log("UnityServiceBridge: ✅ AIAssistant created");
                
                UnityEngine.Debug.Log("UnityServiceBridge: Creating UnityDocumentationService...");
                _documentationService = new UnityDocumentationService();
                UnityEngine.Debug.Log("UnityServiceBridge: ✅ UnityDocumentationService created");
                
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
            UnityEngine.Debug.LogError("🔍 DEBUG: GetExportService called! _exportService is " + (_exportService == null ? "NULL" : "NOT NULL"));
            
            if (_exportService == null)
            {
                UnityEngine.Debug.LogError("🔍 DEBUG: _exportService is null, calling Initialize()...");
                Initialize();
                UnityEngine.Debug.LogError("🔍 DEBUG: After Initialize(), _exportService is " + (_exportService == null ? "STILL NULL" : "NOW NOT NULL"));
            }
            
            if (_exportService == null)
            {
                UnityEngine.Debug.LogError("🔍 DEBUG: Returning MockExportService because _exportService is null!");
                return new MockExportService();
            }
            else
            {
                // Let's test if our debug ExportService is working by calling a method
                System.Collections.Generic.List<ExportFormat> formats = _exportService.GetSupportedFormats();
                return _exportService;
            }
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
        /// Get or create AI assistant instance
        /// </summary>
        public static IAIAssistant GetAIAssistant()
        {
            if (_aiAssistant == null)
            {
                Initialize();
            }
            return _aiAssistant;
        }
        
        /// <summary>
        /// Get or create documentation service instance
        /// </summary>
        public static UnityDocumentationService GetDocumentationService()
        {
            if (_documentationService == null)
            {
                Initialize();
            }
            return _documentationService;
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
                    foreach (UnityProjectArchitect.Core.ValidationIssue issue in result.Issues)
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
                    foreach (UnityProjectArchitect.Core.ValidationIssue issue in result.Issues)
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
            ExportOperationResult result = new ExportOperationResult(exportRequest.Format, exportRequest.OutputPath)
            {
                Success = false,
                ErrorMessage = "Mock implementation"
            };
            return Task.FromResult(result);
        }
        
        public Task<ExportOperationResult> ExportSectionAsync(DocumentationSectionData section, ExportRequest exportRequest)
        {
            ExportOperationResult result = new ExportOperationResult(exportRequest.Format, exportRequest.OutputPath)
            {
                Success = false,
                ErrorMessage = "Mock implementation"
            };
            return Task.FromResult(result);
        }
        
        public Task<ExportOperationResult> ExportMultipleSectionsAsync(System.Collections.Generic.List<DocumentationSectionData> sections, ExportRequest exportRequest)
        {
            ExportOperationResult result = new ExportOperationResult(exportRequest.Format, exportRequest.OutputPath)
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
    
    /// <summary>
    /// Unity-specific documentation service that integrates the DLL documentation generators
    /// with Unity Editor requirements and UI
    /// </summary>
    public class UnityDocumentationService
    {
        /// <summary>
        /// Generate content for a specific documentation section using the appropriate DLL generator
        /// </summary>
        public async Task<string> GenerateDocumentationSectionAsync(DocumentationSectionData section, ProjectData projectData)
        {
            try
            {
                // First, perform project analysis to provide data to generators
                IProjectAnalyzer analyzer = UnityServiceBridge.GetProjectAnalyzer();
                string projectPath = System.IO.Directory.GetParent(UnityEngine.Application.dataPath).FullName;
                ProjectAnalysisResult analysisResult = await analyzer.AnalyzeProjectAsync(projectPath);
                
                // If analysis fails, create a basic analysis result
                if (!analysisResult.Success)
                {
                    analysisResult = new ProjectAnalysisResult
                    {
                        Success = true,
                        ProjectPath = projectPath,
                        AnalyzedAt = System.DateTime.Now,
                        AnalysisTime = System.TimeSpan.FromSeconds(1)
                    };
                }
                
                // Create the appropriate documentation generator based on section type
                BaseDocumentationGenerator generator = section.SectionType switch
                {
                    DocumentationSectionType.GeneralProductDescription => new GeneralProductDescriptionGenerator(analysisResult),
                    DocumentationSectionType.SystemArchitecture => new SystemArchitectureGenerator(analysisResult),
                    DocumentationSectionType.DataModel => new DataModelGenerator(analysisResult),
                    DocumentationSectionType.APISpecification => new APISpecificationGenerator(analysisResult),
                    DocumentationSectionType.UserStories => new UserStoriesGenerator(analysisResult),
                    DocumentationSectionType.WorkTickets => new WorkTicketsGenerator(analysisResult),
                    _ => throw new System.NotSupportedException($"Documentation section type {section.SectionType} is not supported")
                };
                
                // Generate the content using the DLL generator
                string generatedContent = await generator.GenerateContentAsync();
                
                UnityEngine.Debug.Log($"✅ Generated {section.SectionType} documentation ({generatedContent.Length:N0} characters)");
                
                return generatedContent;
            }
            catch (System.Exception ex)
            {
                string errorMessage = $"Failed to generate {section.SectionType} documentation: {ex.Message}";
                UnityEngine.Debug.LogError(errorMessage);
                
                // Return fallback content with error information
                return $"# {section.SectionType}\n\n" +
                       $"**Generation Error:** {errorMessage}\n\n" +
                       $"*Generated on {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}*\n\n" +
                       $"Please check the Unity console for more details about this error.";
            }
        }
        
        /// <summary>
        /// Check if a documentation section type is supported
        /// </summary>
        public bool CanGenerateSection(DocumentationSectionType sectionType)
        {
            return sectionType switch
            {
                DocumentationSectionType.GeneralProductDescription => true,
                DocumentationSectionType.SystemArchitecture => true,
                DocumentationSectionType.DataModel => true,
                DocumentationSectionType.APISpecification => true,
                DocumentationSectionType.UserStories => true,
                DocumentationSectionType.WorkTickets => true,
                _ => false
            };
        }
    }
}