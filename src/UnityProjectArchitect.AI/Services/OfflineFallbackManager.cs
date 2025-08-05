using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.AI.Services
{
    /// <summary>
    /// Manages offline fallback mechanisms when AI services are unavailable,
    /// providing template-based content generation and cached responses for continuity
    /// </summary>
    public class OfflineFallbackManager
    {
        private readonly Dictionary<DocumentationSectionType, string> _templateCache;
        private readonly Dictionary<string, string> _responseCache;
        private readonly Dictionary<string, OfflineFallbackTemplate> _fallbackTemplates;
        private readonly ILogger _logger;
        private readonly string _templatesPath;
        private readonly string _cachePath;
        private readonly OfflineFallbackConfiguration _configuration;

        public event Action OnFallbackActivated;
        public event Action OnFallbackDeactivated;
        public event Action<string> OnTemplateUsed;

        public bool IsFallbackMode { get; private set; }
        public DateTime? LastOnlineTime { get; private set; }
        public int CachedResponsesCount => _responseCache.Count;
        public int AvailableTemplatesCount => _fallbackTemplates.Count;

        public OfflineFallbackManager() : this(new ConsoleLogger())
        {
        }

        public OfflineFallbackManager(ILogger logger, OfflineFallbackConfiguration configuration = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? new OfflineFallbackConfiguration();
            
            _templateCache = new Dictionary<DocumentationSectionType, string>();
            _responseCache = new Dictionary<string, string>();
            _fallbackTemplates = new Dictionary<string, OfflineFallbackTemplate>();
            
            _templatesPath = GetDefaultTemplatesPath();
            _cachePath = GetDefaultCachePath();

            InitializeFallbackTemplates();
            LoadCachedResponses();
        }

        /// <summary>
        /// Activates offline fallback mode when AI services are unavailable
        /// </summary>
        public async Task<bool> ActivateFallbackModeAsync(string reason = "AI service unavailable")
        {
            if (IsFallbackMode)
                return true;

            try
            {
                IsFallbackMode = true;
                _logger.LogWarning($"Activating offline fallback mode: {reason}");

                // Ensure all necessary templates are loaded
                await LoadAllTemplatesAsync();

                // Clear any pending AI requests
                await ClearPendingRequestsAsync();

                OnFallbackActivated?.Invoke();
                
                _logger.Log($"Offline fallback mode activated with {_fallbackTemplates.Count} templates available");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to activate fallback mode: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Deactivates offline fallback mode when AI services become available
        /// </summary>
        public async Task<bool> DeactivateFallbackModeAsync()
        {
            if (!IsFallbackMode)
                return true;

            try
            {
                IsFallbackMode = false;
                LastOnlineTime = DateTime.Now;
                
                _logger.Log("Deactivating offline fallback mode - AI services restored");

                // Save any generated content to cache for future offline use
                await SaveGeneratedContentToCacheAsync();

                OnFallbackDeactivated?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to deactivate fallback mode: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generates content using offline templates when AI is unavailable
        /// </summary>
        public async Task<AIOperationResult> GenerateOfflineContentAsync(AIRequest request)
        {
            if (!IsFallbackMode)
            {
                return new AIOperationResult(false, string.Empty)
                {
                    ErrorMessage = "Offline fallback not activated"
                };
            }

            DateTime startTime = DateTime.Now;
            _logger.Log($"Generating offline content for section: {request.SectionType}");

            try
            {
                // Try to get cached response first
                string cacheKey = GenerateCacheKey(request);
                if (_responseCache.ContainsKey(cacheKey) && _configuration.UseCachedResponses)
                {
                    _logger.Log($"Using cached response for request: {cacheKey}");
                    return new AIOperationResult(true, _responseCache[cacheKey])
                    {
                        Provider = AIProvider.Offline,
                        ProcessingTime = DateTime.Now - startTime,
                        ConfidenceScore = 0.6f, // Lower confidence for cached content
                        Metadata = new Dictionary<string, object> { { "Source", "Cache" } }
                    };
                }

                // Generate content using templates
                string content = await GenerateTemplateBasedContentAsync(request);
                
                if (string.IsNullOrEmpty(content))
                {
                    return new AIOperationResult(false, string.Empty)
                    {
                        ErrorMessage = "No suitable offline template found",
                        Provider = AIProvider.Offline,
                        ProcessingTime = DateTime.Now - startTime
                    };
                }

                // Cache the generated content
                if (_configuration.CacheGeneratedContent)
                {
                    _responseCache[cacheKey] = content;
                }

                AIOperationResult result = new AIOperationResult(true, content)
                {
                    Provider = AIProvider.Offline,
                    ProcessingTime = DateTime.Now - startTime,
                    ConfidenceScore = 0.7f, // Moderate confidence for template-based content
                    TokensUsed = EstimateTokenCount(content),
                    Metadata = new Dictionary<string, object>
                    {
                        { "Source", "Template" },
                        { "TemplateId", GetBestTemplateId(request) },
                        { "FallbackMode", true }
                    }
                };

                OnTemplateUsed?.Invoke($"Template used for {request.SectionType}");
                _logger.Log($"Generated offline content: {content.Length} characters in {result.ProcessingTime.TotalMilliseconds:F1}ms");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Offline content generation failed: {ex.Message}");
                
                return new AIOperationResult(false, string.Empty)
                {
                    ErrorMessage = $"Offline generation failed: {ex.Message}",
                    Provider = AIProvider.Offline,
                    ProcessingTime = DateTime.Now - startTime
                };
            }
        }

        /// <summary>
        /// Provides fallback project analysis using predefined patterns and templates
        /// </summary>
        public async Task<AIOperationResult> AnalyzeProjectOfflineAsync(ProjectData projectData)
        {
            if (!IsFallbackMode)
                return new AIOperationResult(false, string.Empty) { ErrorMessage = "Offline mode not active" };

            DateTime startTime = DateTime.Now;
            
            try
            {
                OfflineProjectAnalysisResult analysis = await PerformOfflineAnalysisAsync(projectData);
                string analysisContent = GenerateAnalysisReport(analysis);

                return new AIOperationResult(true, analysisContent)
                {
                    Provider = AIProvider.Offline,
                    ProcessingTime = DateTime.Now - startTime,
                    ConfidenceScore = 0.5f, // Lower confidence for rule-based analysis
                    TokensUsed = EstimateTokenCount(analysisContent),
                    Metadata = new Dictionary<string, object>
                    {
                        { "AnalysisType", "RuleBased" },
                        { "ComponentsAnalyzed", analysis.ComponentsFound.Count },
                        { "IssuesFound", analysis.PotentialIssues.Count }
                    }
                };
            }
            catch (Exception ex)
            {
                return new AIOperationResult(false, string.Empty)
                {
                    ErrorMessage = $"Offline analysis failed: {ex.Message}",
                    Provider = AIProvider.Offline,
                    ProcessingTime = DateTime.Now - startTime
                };
            }
        }

        /// <summary>
        /// Gets offline suggestions based on predefined patterns and best practices
        /// </summary>
        public async Task<List<string>> GetOfflineSuggestionsAsync(ProjectData projectData, SuggestionType suggestionType)
        {
            List<string> suggestions = new List<string>();

            try
            {
                switch (suggestionType)
                {
                    case SuggestionType.ProjectStructure:
                        suggestions.AddRange(GetStructureSuggestions(projectData));
                        break;
                    case SuggestionType.BestPractices:
                        suggestions.AddRange(GetBestPracticeSuggestions(projectData));
                        break;
                    case SuggestionType.Architecture:
                        suggestions.AddRange(GetArchitectureSuggestions(projectData));
                        break;
                    case SuggestionType.Performance:
                        suggestions.AddRange(GetPerformanceSuggestions(projectData));
                        break;
                    case SuggestionType.Documentation:
                        suggestions.AddRange(GetDocumentationSuggestions(projectData));
                        break;
                    default:
                        suggestions.AddRange(GetGeneralSuggestions());
                        break;
                }

                _logger.Log($"Generated {suggestions.Count} offline suggestions for {suggestionType}");
                return suggestions;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to generate offline suggestions: {ex.Message}");
                return new List<string> { "Unable to generate suggestions in offline mode" };
            }
        }

        /// <summary>
        /// Caches successful AI responses for future offline use
        /// </summary>
        public async Task<bool> CacheResponseAsync(AIRequest request, AIOperationResult result)
        {
            if (!result.Success || string.IsNullOrEmpty(result.Content))
                return false;

            try
            {
                string cacheKey = GenerateCacheKey(request);
                _responseCache[cacheKey] = result.Content;

                if (_configuration.PersistCache)
                {
                    await SaveCacheToFileAsync();
                }

                _logger.Log($"Cached AI response: {cacheKey}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to cache response: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets status information about offline fallback capabilities
        /// </summary>
        public OfflineFallbackStatus GetStatus()
        {
            return new OfflineFallbackStatus
            {
                IsFallbackMode = IsFallbackMode,
                LastOnlineTime = LastOnlineTime,
                AvailableTemplates = _fallbackTemplates.Count,
                CachedResponses = _responseCache.Count,
                TemplateTypes = _fallbackTemplates.Keys.ToList(),
                Configuration = _configuration,
                MemoryUsage = EstimateMemoryUsage()
            };
        }

        #region Private Implementation Methods

        private void InitializeFallbackTemplates()
        {
            // Initialize templates for each documentation section
            _fallbackTemplates["general_description"] = new OfflineFallbackTemplate
            {
                Id = "general_description",
                SectionType = DocumentationSectionType.GeneralProductDescription,
                Template = GetGeneralDescriptionTemplate(),
                Variables = new List<string> { "project_name", "project_description", "key_features", "target_audience" }
            };

            _fallbackTemplates["system_architecture"] = new OfflineFallbackTemplate
            {
                Id = "system_architecture",
                SectionType = DocumentationSectionType.SystemArchitecture,
                Template = GetSystemArchitectureTemplate(),
                Variables = new List<string> { "project_name", "main_components", "architecture_pattern", "dependencies" }
            };

            _fallbackTemplates["data_model"] = new OfflineFallbackTemplate
            {
                Id = "data_model",
                SectionType = DocumentationSectionType.DataModel,
                Template = GetDataModelTemplate(),
                Variables = new List<string> { "project_name", "data_structures", "scriptable_objects", "relationships" }
            };

            _fallbackTemplates["api_specification"] = new OfflineFallbackTemplate
            {
                Id = "api_specification",
                SectionType = DocumentationSectionType.APISpecification,
                Template = GetAPISpecificationTemplate(),
                Variables = new List<string> { "project_name", "main_interfaces", "key_methods", "usage_examples" }
            };

            _fallbackTemplates["user_stories"] = new OfflineFallbackTemplate
            {
                Id = "user_stories",
                SectionType = DocumentationSectionType.UserStories,
                Template = GetUserStoriesTemplate(),
                Variables = new List<string> { "project_name", "user_types", "main_goals", "user_benefits" }
            };

            _fallbackTemplates["work_tickets"] = new OfflineFallbackTemplate
            {
                Id = "work_tickets",
                SectionType = DocumentationSectionType.WorkTickets,
                Template = GetWorkTicketsTemplate(),
                Variables = new List<string> { "project_name", "development_tasks", "implementation_steps", "acceptance_criteria" }
            };
        }

        private async Task<string> GenerateTemplateBasedContentAsync(AIRequest request)
        {
            if (!request.SectionType.HasValue)
                return string.Empty;

            string templateId = GetBestTemplateId(request);
            if (!_fallbackTemplates.ContainsKey(templateId))
                return string.Empty;

            OfflineFallbackTemplate template = _fallbackTemplates[templateId];
            string content = template.Template;

            // Replace template variables with project-specific content
            Dictionary<string, string> variables = ExtractVariables(request);
            foreach (KeyValuePair<string, string> variable in variables)
            {
                content = content.Replace($"{{{variable.Key}}}", variable.Value);
            }

            // Clean up any remaining template variables
            content = System.Text.RegularExpressions.Regex.Replace(content, @"\{[^}]*\}", "[Content to be defined]");

            return content;
        }

        private string GetBestTemplateId(AIRequest request)
        {
            if (!request.SectionType.HasValue)
                return "general_description";

            return request.SectionType.Value switch
            {
                DocumentationSectionType.GeneralProductDescription => "general_description",
                DocumentationSectionType.SystemArchitecture => "system_architecture",
                DocumentationSectionType.DataModel => "data_model",
                DocumentationSectionType.APISpecification => "api_specification",
                DocumentationSectionType.UserStories => "user_stories",
                DocumentationSectionType.WorkTickets => "work_tickets",
                _ => "general_description"
            };
        }

        private Dictionary<string, string> ExtractVariables(AIRequest request)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>();

            if (request.ProjectContext != null)
            {
                variables["project_name"] = request.ProjectContext.ProjectName ?? "Unity Project";
                variables["project_description"] = request.ProjectContext.ProjectDescription ?? "A Unity project";
                variables["target_audience"] = "Unity developers";
                variables["unity_version"] = request.ProjectContext.TargetUnityVersion.ToString() ?? "2023.3+";
            }

            // Add default values for common variables
            variables["key_features"] = "Project management, Documentation generation, Template system";
            variables["main_components"] = "Core system, UI components, Data models";
            variables["architecture_pattern"] = "Component-based architecture";
            variables["dependencies"] = "Unity Engine, UI Toolkit";
            variables["data_structures"] = "ScriptableObjects, Configuration data";
            variables["scriptable_objects"] = "ProjectData, DocumentationSection, TemplateConfiguration";
            variables["relationships"] = "Hierarchical data organization with validation";
            variables["main_interfaces"] = "IProjectAnalyzer, IDocumentationGenerator, ITemplateManager";
            variables["key_methods"] = "AnalyzeProject(), GenerateDocumentation(), CreateTemplate()";
            variables["usage_examples"] = "Basic project setup and documentation generation";
            variables["user_types"] = "Solo developers, Team leads, Project managers";
            variables["main_goals"] = "Streamline project documentation, Standardize project structure";
            variables["user_benefits"] = "Reduced documentation time, Consistent project organization";
            variables["development_tasks"] = "Implementation, Testing, Documentation";
            variables["implementation_steps"] = "Design, Code, Test, Review";
            variables["acceptance_criteria"] = "Feature complete, Tests passing, Documentation updated";

            return variables;
        }

        private async Task<OfflineProjectAnalysisResult> PerformOfflineAnalysisAsync(ProjectData projectData)
        {
            OfflineProjectAnalysisResult result = new OfflineProjectAnalysisResult
            {
                ProjectName = projectData.ProjectName,
                AnalysisDate = DateTime.Now,
                ComponentsFound = new List<string>(),
                PotentialIssues = new List<string>(),
                Recommendations = new List<string>()
            };

            // Basic analysis based on project data
            if (projectData.DocumentationSections != null)
            {
                result.ComponentsFound.Add($"Documentation sections: {projectData.DocumentationSections.Count}");
                
                int emptyDocumentation = projectData.DocumentationSections.Count(s => string.IsNullOrEmpty(s.Content));
                if (emptyDocumentation > 0)
                {
                    result.PotentialIssues.Add($"{emptyDocumentation} documentation sections are empty");
                    result.Recommendations.Add("Complete missing documentation sections");
                }
            }

            if (projectData.FolderStructure != null)
            {
                result.ComponentsFound.Add("Folder structure defined");
                result.Recommendations.Add("Review folder organization for optimal structure");
            }

            // Add generic recommendations
            result.Recommendations.Add("Consider implementing automated testing");
            result.Recommendations.Add("Establish coding standards and conventions");
            result.Recommendations.Add("Plan for scalable architecture patterns");

            return result;
        }

        private string GenerateAnalysisReport(OfflineProjectAnalysisResult analysis)
        {
            System.Text.StringBuilder report = new System.Text.StringBuilder();
            
            report.AppendLine($"# Project Analysis Report: {analysis.ProjectName}");
            report.AppendLine($"**Generated:** {analysis.AnalysisDate:yyyy-MM-dd HH:mm:ss} (Offline Mode)");
            report.AppendLine();

            report.AppendLine("## Components Found");
            foreach (string component in analysis.ComponentsFound)
            {
                report.AppendLine($"- {component}");
            }
            report.AppendLine();

            if (analysis.PotentialIssues.Any())
            {
                report.AppendLine("## Potential Issues");
                foreach (string issue in analysis.PotentialIssues)
                {
                    report.AppendLine($"- ⚠️ {issue}");
                }
                report.AppendLine();
            }

            report.AppendLine("## Recommendations");
            foreach (string recommendation in analysis.Recommendations)
            {
                report.AppendLine($"- 💡 {recommendation}");
            }

            report.AppendLine();
            report.AppendLine("*Note: This analysis was generated in offline mode using rule-based patterns. " +
                             "For more detailed insights, reconnect to AI services.*");

            return report.ToString();
        }

        private List<string> GetStructureSuggestions(ProjectData projectData)
        {
            return new List<string>
            {
                "Organize scripts into logical folders (Scripts/Core, Scripts/UI, Scripts/Data)",
                "Separate runtime and editor-only code",
                "Use assembly definitions to control compilation dependencies",
                "Create dedicated folders for assets by type (Textures, Models, Audio)",
                "Implement a consistent naming convention for files and folders"
            };
        }

        private List<string> GetBestPracticeSuggestions(ProjectData projectData)
        {
            return new List<string>
            {
                "Use ScriptableObjects for configuration data",
                "Implement proper error handling and logging",
                "Follow SOLID principles in code architecture",
                "Use Unity's component-based design patterns",
                "Implement object pooling for frequently instantiated objects",
                "Use prefabs for reusable GameObjects"
            };
        }

        private List<string> GetArchitectureSuggestions(ProjectData projectData)
        {
            return new List<string>
            {
                "Consider implementing a service locator pattern",
                "Use dependency injection for better testability",
                "Implement event-driven architecture for loose coupling",
                "Separate business logic from Unity-specific code",
                "Use interfaces to define contracts between systems",
                "Consider implementing a state machine for complex behaviors"
            };
        }

        private List<string> GetPerformanceSuggestions(ProjectData projectData)
        {
            return new List<string>
            {
                "Profile regularly using Unity's Profiler",
                "Optimize Update() calls - consider caching and pooling",
                "Use Unity's Job System for multi-threaded operations",
                "Implement LOD (Level of Detail) systems for complex models",
                "Optimize texture sizes and compression settings",
                "Use Unity's addressable asset system for large projects"
            };
        }

        private List<string> GetDocumentationSuggestions(ProjectData projectData)
        {
            return new List<string>
            {
                "Document public APIs with XML documentation comments",
                "Create architectural decision records (ADRs)",
                "Maintain a project glossary for domain-specific terms",
                "Document build and deployment processes",
                "Create onboarding documentation for new team members",
                "Keep documentation up-to-date with code changes"
            };
        }

        private List<string> GetGeneralSuggestions()
        {
            return new List<string>
            {
                "Establish regular code review processes",
                "Implement continuous integration and testing",
                "Use version control best practices",
                "Plan for cross-platform compatibility",
                "Consider accessibility requirements",
                "Implement proper error reporting and analytics"
            };
        }

        private string GenerateCacheKey(AIRequest request)
        {
            List<string> keyParts = new List<string>();
            
            if (request.SectionType.HasValue)
                keyParts.Add(request.SectionType.Value.ToString());
            
            if (request.ProjectContext != null)
                keyParts.Add(request.ProjectContext.ProjectName ?? "unknown");
            
            if (!string.IsNullOrEmpty(request.Prompt))
                keyParts.Add(request.Prompt.GetHashCode().ToString());

            return string.Join("_", keyParts);
        }

        private int EstimateTokenCount(string content)
        {
            return Math.Max(1, content.Length / 4);
        }

        private long EstimateMemoryUsage()
        {
            long totalBytes = 0;
            
            foreach (string response in _responseCache.Values)
            {
                totalBytes += response.Length * 2; // Unicode characters
            }
            
            foreach (OfflineFallbackTemplate template in _fallbackTemplates.Values)
            {
                totalBytes += template.Template.Length * 2;
            }

            return totalBytes;
        }

        private async Task LoadAllTemplatesAsync()
        {
            // Templates are initialized in constructor, this could load from files if needed
            await Task.CompletedTask;
        }

        private async Task ClearPendingRequestsAsync()
        {
            // Clear any pending AI requests - implementation depends on architecture
            await Task.CompletedTask;
        }

        private async Task SaveGeneratedContentToCacheAsync()
        {
            // Save cache to persistent storage
            if (_configuration.PersistCache)
            {
                await SaveCacheToFileAsync();
            }
        }

        private async Task SaveCacheToFileAsync()
        {
            try
            {
                if (!Directory.Exists(_cachePath))
                {
                    Directory.CreateDirectory(_cachePath);
                }

                string cacheFile = Path.Combine(_cachePath, "offline_cache.json");
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(_responseCache, Newtonsoft.Json.Formatting.Indented);
                await File.WriteAllTextAsync(cacheFile, json);
                
                _logger.Log($"Saved {_responseCache.Count} cached responses to {cacheFile}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save cache to file: {ex.Message}");
            }
        }

        private void LoadCachedResponses()
        {
            try
            {
                string cacheFile = Path.Combine(_cachePath, "offline_cache.json");
                if (File.Exists(cacheFile))
                {
                    string json = File.ReadAllText(cacheFile);
                    Dictionary<string, string> cached = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    
                    if (cached != null)
                    {
                        foreach (KeyValuePair<string, string> item in cached)
                        {
                            _responseCache[item.Key] = item.Value;
                        }
                        
                        _logger.Log($"Loaded {_responseCache.Count} cached responses from {cacheFile}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load cached responses: {ex.Message}");
            }
        }

        private static string GetDefaultTemplatesPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Offline");
        }

        private static string GetDefaultCachePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache", "Offline");
        }

        #endregion

        #region Template Definitions

        private string GetGeneralDescriptionTemplate()
        {
            return @"# {project_name}

## Overview
{project_description}

This Unity project is designed to provide {key_features} for {target_audience}.

## Key Features
- Comprehensive project management tools
- Automated documentation generation
- Template-based project structure
- Unity Editor integration
- Extensible architecture

## Target Audience
This tool is intended for {target_audience} who need to:
- Streamline project documentation processes  
- Maintain consistent project structures
- Improve development workflow efficiency

## Unity Version
Compatible with {unity_version} and later versions.

## Getting Started
1. Import the package into your Unity project
2. Configure your project settings
3. Generate documentation using the built-in templates
4. Customize templates as needed for your workflow";
        }

        private string GetSystemArchitectureTemplate()
        {
            return @"# System Architecture - {project_name}

## Architecture Overview
The {project_name} follows a {architecture_pattern} with clear separation of concerns and modular design.

## Main Components
{main_components}

### Core System
- **ProjectData**: Central data model for project information
- **DocumentationGenerator**: Handles automated content generation
- **TemplateManager**: Manages project templates and structures

### Dependencies
{dependencies}

## Design Patterns
- **Component Pattern**: Used throughout Unity integration
- **Template Method**: Applied to documentation generation
- **Observer Pattern**: For event-driven updates
- **Strategy Pattern**: For different generation algorithms

## Data Flow
1. Project analysis and data collection
2. Template selection and customization
3. Content generation and validation
4. Output formatting and export

## Extensibility
The system is designed to be easily extended through:
- Custom template implementations
- Plugin-based architecture
- Event-driven extension points";
        }

        private string GetDataModelTemplate()
        {
            return @"# Data Model - {project_name}

## Overview
The data model is built around Unity's ScriptableObject system for serializable, inspector-friendly data management.

## Core Data Structures
{data_structures}

### Primary ScriptableObjects
{scriptable_objects}

**ProjectData**
- Project metadata and configuration
- Documentation section references
- Template associations

**documentationSection**
- Section type and content
- Generation settings and validation rules
- Custom prompt definitions

**TemplateConfiguration**
- Folder structure definitions
- Scene template specifications
- Asset organization rules

## Data Relationships
{relationships}

## Serialization
All data models implement Unity's serialization system:
- JSON export/import capabilities
- Inspector-based editing
- Runtime data validation
- Version compatibility handling

## Validation Framework
- Required field validation
- Data integrity checks
- Cross-reference validation
- Custom validation rules";
        }

        private string GetAPISpecificationTemplate()
        {
            return @"# API Specification - {project_name}

## Overview
This document describes the public API interfaces and their usage patterns.

## Core Interfaces
{main_interfaces}

### IProjectAnalyzer
Provides project analysis and insights functionality.

**Key Methods:**
{key_methods}

```csharp
public interface IProjectAnalyzer
{
    Task<ProjectAnalysisResult> AnalyzeProjectAsync(string projectPath);
    Task<List<ProjectInsight>> GenerateInsightsAsync(ProjectData data);
    ValidationResult ValidateProject(ProjectData data);
}
```

### IDocumentationGenerator
Handles automated documentation generation.

```csharp
public interface IDocumentationGenerator  
{
    Task<DocumentationResult> GenerateAsync(ProjectData data, DocumentationSectionType type);
    Task<ExportResult> ExportAsync(DocumentationData data, ExportFormat format);
    ValidationResult ValidateContent(string content, DocumentationSectionType type);
}
```

## Usage Examples
{usage_examples}

### Basic Project Analysis
```csharp
IProjectAnalyzer analyzer = new ProjectAnalyzer();
ProjectAnalysisResult result = await analyzer.AnalyzeProjectAsync(projectPath);
```

### Documentation Generation
```csharp
IDocumentationGenerator generator = new DocumentationGenerator();
DocumentationResult result = await generator.GenerateAsync(projectData, DocumentationSectionType.SystemArchitecture);
```

## Error Handling
All methods return result objects with success/failure status and detailed error information when operations fail.";
        }

        private string GetUserStoriesTemplate()
        {
            return @"# User Stories - {project_name}

## Overview
This section defines the user stories that drive the development of {project_name}.

## Primary User Types
{user_types}

## Core User Stories

### For Solo Developers
**Story 1: Quick Project Setup**
- **As a** solo indie developer
- **I want** to quickly set up a new Unity project with standard structure  
- **So that** I can focus on game development instead of project organization

**Story 2: Automated Documentation**
- **As a** solo developer with limited time
- **I want** to automatically generate project documentation
- **So that** I can maintain professional documentation without manual effort

### For Team Leads
**Story 3: Team Standardization**
- **As a** team lead
- **I want** to enforce consistent project structures across team members
- **So that** new developers can quickly understand and contribute to projects

**Story 4: Project Analysis**
- **As a** technical lead
- **I want** to analyze project architecture and get improvement suggestions
- **So that** I can maintain code quality and identify potential issues

### For Project Managers
**Story 5: Progress Tracking**
- **As a** project manager
- **I want** to generate comprehensive project reports
- **So that** I can track development progress and communicate status to stakeholders

## User Goals
{main_goals}

## User Benefits
{user_benefits}

## Acceptance Criteria
- All user stories must be implementable within the Unity Editor
- Documentation generation must be completed in under 60 seconds
- Template system must support customization and extension
- All features must work offline when possible";
        }

        private string GetWorkTicketsTemplate()
        {
            return @"# Work Tickets - {project_name}

## Overview
This section breaks down the development work into manageable tickets and tasks.

## Development Tasks
{development_tasks}

## Implementation Tickets

### Ticket 1: Core Data Models
**Priority:** High
**Estimated Effort:** 4 hours

**Description:**
Implement the foundational data models using ScriptableObjects.

**Implementation Steps:**
{implementation_steps}
1. Create ProjectData ScriptableObject
2. Implement DocumentationSection model
3. Design TemplateConfiguration structure
4. Add validation framework
5. Create serialization support

**Acceptance Criteria:**
{acceptance_criteria}
- All models serialize correctly
- Validation rules are enforced
- Inspector integration works properly
- Unit tests cover core functionality

### Ticket 2: Documentation Generation System
**Priority:** High  
**Estimated Effort:** 6 hours

**Description:**
Build the automated documentation generation pipeline.

**Implementation Steps:**
1. Design IDocumentationGenerator interface
2. Create section-specific generators
3. Implement template system
4. Add export functionality
5. Integrate with Unity Editor UI

**Acceptance Criteria:**
- Generates all 6 documentation sections
- Multiple export formats supported
- Template customization available
- Performance meets requirements (<60s generation time)

### Ticket 3: Project Analysis Engine  
**Priority:** Medium
**Estimated Effort:** 5 hours

**Description:**
Implement intelligent project analysis and insights.

**Implementation Steps:**
1. Create IProjectAnalyzer interface
2. Build file system analysis
3. Implement pattern recognition
4. Generate actionable insights
5. Add recommendation engine

**Acceptance Criteria:**
- Analyzes project structure accurately
- Identifies common issues and patterns
- Provides actionable recommendations
- Integrates with documentation generation

### Ticket 4: Unity Editor Integration
**Priority:** Medium
**Estimated Effort:** 4 hours

**Description:**
Create Unity Editor windows and menu integration.

**Implementation Steps:**
1. Design main editor window UI
2. Create template management interface
3. Add project analysis dashboard
4. Implement export functionality
5. Add help and documentation

**Acceptance Criteria:**
- User-friendly Editor interface
- All features accessible through UI
- Responsive and intuitive design
- Comprehensive help system

## Testing Strategy
- Unit tests for all core functionality
- Integration tests for Unity Editor features
- Performance testing for large projects
- User acceptance testing with target personas

## Definition of Done
- Code reviewed and approved
- Tests passing with >80% coverage
- Documentation updated
- Performance requirements met
- User acceptance criteria validated";
        }

        #endregion
    }

    #region Supporting Data Models

    /// <summary>
    /// Configuration for offline fallback behavior
    /// </summary>
    public class OfflineFallbackConfiguration
    {
        public bool UseCachedResponses { get; set; } = true;
        public bool CacheGeneratedContent { get; set; } = true;
        public bool PersistCache { get; set; } = true;
        public int MaxCacheSize { get; set; } = 1000;
        public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromDays(7);
        public bool EnableTemplateGeneration { get; set; } = true;
    }

    /// <summary>
    /// Template for offline content generation
    /// </summary>
    public class OfflineFallbackTemplate
    {
        public string Id { get; set; }
        public DocumentationSectionType SectionType { get; set; }
        public string Template { get; set; }
        public List<string> Variables { get; set; } = new List<string>();
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Status information about offline fallback system
    /// </summary>
    public class OfflineFallbackStatus
    {
        public bool IsFallbackMode { get; set; }
        public DateTime? LastOnlineTime { get; set; }
        public int AvailableTemplates { get; set; }
        public int CachedResponses { get; set; }
        public List<string> TemplateTypes { get; set; } = new List<string>();
        public OfflineFallbackConfiguration Configuration { get; set; }
        public long MemoryUsage { get; set; }
    }

    /// <summary>
    /// Result of offline project analysis
    /// </summary>
    public class OfflineProjectAnalysisResult
    {
        public string ProjectName { get; set; }
        public DateTime AnalysisDate { get; set; }
        public List<string> ComponentsFound { get; set; } = new List<string>();
        public List<string> PotentialIssues { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
    }

    #endregion
}