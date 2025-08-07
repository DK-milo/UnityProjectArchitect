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
                _projectAnalyzer = new ProjectAnalyzer();
                _exportService = new ExportService();
                _templateManager = new TemplateManager();
                _aiAssistant = new AIAssistant();
                _documentationService = new UnityDocumentationService();
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
            
            if (_exportService == null)
            {
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
                // Check if this is a concept project (has description but minimal actual project content)
                bool isConceptProject = IsConceptProject(projectData);
                
                if (isConceptProject && !string.IsNullOrEmpty(projectData.ProjectDescription))
                {
                    UnityEngine.Debug.Log($"🎯 Detected concept project - using concept-aware generator for {section.SectionType}");
                    return await GenerateFromConceptAsync(section.SectionType, projectData.ProjectDescription);
                }
                else
                {
                    UnityEngine.Debug.Log($"🔍 Detected existing project - using project analysis for {section.SectionType}");
                    return await GenerateFromProjectAnalysisAsync(section, projectData);
                }
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
        /// Determine if this is a concept project vs existing project
        /// </summary>
        private bool IsConceptProject(ProjectData projectData)
        {
            // Check if we have any meaningful description (lowered threshold for short concepts)
            bool hasDescription = !string.IsNullOrEmpty(projectData.ProjectDescription) && 
                                 projectData.ProjectDescription.Trim().Length > 10;
            
            // Check if project name suggests it's a concept (contains "AI Generated", etc.)
            bool hasConceptualName = projectData.ProjectName.Contains("AI Generated") || 
                                   projectData.ProjectName.Contains("Concept") ||
                                   projectData.ProjectName.Contains("Test") ||
                                   projectData.ProjectName == "My project";
            
            // If we have any description and conceptual indicators, treat as concept
            if (hasDescription && hasConceptualName)
            {
                return true;
            }
            
            // Also check if the description contains typical game concept markers or game-related keywords
            if (hasDescription)
            {
                string description = projectData.ProjectDescription.ToLower();
                bool hasConceptMarkers = description.Contains("**") || // Markdown formatting
                                       description.Contains("core gameplay") ||
                                       description.Contains("key features") ||
                                       description.Contains("target audience") ||
                                       description.Contains("art style") ||
                                       description.Contains("development time");
                
                // Also check for game-related keywords that suggest a concept
                bool hasGameKeywords = description.Contains("game") ||
                                     description.Contains("clone") ||
                                     description.Contains("platformer") ||
                                     description.Contains("rpg") ||
                                     description.Contains("adventure") ||
                                     description.Contains("puzzle") ||
                                     description.Contains("shooter") ||
                                     description.Contains("simulation") ||
                                     description.Contains("strategy") ||
                                     description.Contains("want to create") ||
                                     description.Contains("idea for") ||
                                     description.Contains("mario") ||
                                     description.Contains("zelda") ||
                                     description.Contains("player") ||
                                     description.Contains("multiplayer") ||
                                     description.Contains("2d") ||
                                     description.Contains("3d");
                
                if (hasConceptMarkers || hasGameKeywords)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Generate documentation from game concept using concept-aware generators
        /// </summary>
        private async Task<string> GenerateFromConceptAsync(DocumentationSectionType sectionType, string gameDescription)
        {
            try
            {
                // Use concept-aware generators that work with game descriptions
                return sectionType switch
                {
                    DocumentationSectionType.GeneralProductDescription => 
                        await new UnityProjectArchitect.Services.ConceptAware.ConceptualProductDescriptionGenerator(gameDescription).GenerateContentAsync(),
                    DocumentationSectionType.UserStories => 
                        await new UnityProjectArchitect.Services.ConceptAware.ConceptualUserStoriesGenerator(gameDescription).GenerateContentAsync(),
                    DocumentationSectionType.WorkTickets => 
                        await new UnityProjectArchitect.Services.ConceptAware.ConceptualWorkTicketsGenerator(gameDescription).GenerateContentAsync(),
                    DocumentationSectionType.SystemArchitecture => 
                        await GenerateConceptualArchitecture(gameDescription),
                    DocumentationSectionType.DataModel => 
                        await GenerateConceptualDataModel(gameDescription),
                    DocumentationSectionType.APISpecification => 
                        await GenerateConceptualAPISpec(gameDescription),
                    _ => $"# {sectionType}\n\n**Concept-aware generation not yet implemented for this section type.**\n\n*Generated on {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}*"
                };
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Concept-aware generation failed for {sectionType}: {ex.Message}");
                
                // Fallback to a basic template
                return $"# {sectionType}\n\n" +
                       $"**Based on Game Concept:** {gameDescription.Split('\n')[0]}\n\n" +
                       $"*This section would contain {sectionType.ToString().ToLower()} information based on the game concept.*\n\n" +
                       $"*Generated on {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}*";
            }
        }
        
        /// <summary>
        /// Generate documentation from existing project analysis
        /// </summary>
        private async Task<string> GenerateFromProjectAnalysisAsync(DocumentationSectionData section, ProjectData projectData)
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
        
        /// <summary>
        /// Generate conceptual system architecture based on game description
        /// </summary>
        private async Task<string> GenerateConceptualArchitecture(string gameDescription)
        {
            // Simple conceptual architecture generator
            return await Task.FromResult($@"<!-- Generated by ConceptualArchitectureGenerator -->
# System Architecture

*Generated on {System.DateTime.Now:yyyy-MM-dd HH:mm:ss} UTC*

## Architecture Overview

Based on the game concept, the following system architecture is recommended:

### Core Systems

- **Game Manager**: Central controller for game state and flow
- **Player Controller**: Handles player input and character movement
- **Scene Management**: Manages level loading and transitions
- **UI System**: User interface components and navigation
- **Audio Manager**: Sound effects and music management
- **Save/Load System**: Persistent data management

### Game-Specific Systems

{GenerateGameSpecificSystems(gameDescription)}

### Recommended Architecture Pattern

**Component-Based Architecture** - Unity's entity-component system approach
- Modular components for different game behaviors
- Easy to extend and maintain
- Good performance characteristics
- Well-suited for Unity development

### Data Flow

1. **Input Layer**: Player input collection and processing
2. **Game Logic Layer**: Core gameplay mechanics and rules
3. **Presentation Layer**: Visual and audio feedback
4. **Persistence Layer**: Save data and settings management

---
**Generation Metadata:**
- Generated by: ConceptualArchitectureGenerator
- Based on game concept analysis
- Generation Date: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}
---

<!-- End ConceptualArchitectureGenerator -->");
        }
        
        /// <summary>
        /// Generate conceptual data model based on game description
        /// </summary>
        private async Task<string> GenerateConceptualDataModel(string gameDescription)
        {
            return await Task.FromResult($@"<!-- Generated by ConceptualDataModelGenerator -->
# Data Model

*Generated on {System.DateTime.Now:yyyy-MM-dd HH:mm:ss} UTC*

## Data Architecture Overview

Based on the game concept, the following data model is recommended:

### Core Data Classes

```csharp
// Player data
[Serializable]
public class PlayerData
{{
    public string playerName;
    public Vector3 position;
    public float health;
    public int level;
}}

// Game settings
[Serializable]
public class GameSettings
{{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public bool fullscreen;
}}
```

### Game-Specific Data

{GenerateGameSpecificDataModel(gameDescription)}

### ScriptableObject Templates

- **GameConfig**: Global game configuration and balancing values
- **LevelData**: Information about game levels or stages
- **ItemDatabase**: Collectible items and their properties
- **AudioBank**: Sound effects and music references

### Data Persistence Strategy

- **PlayerPrefs**: Simple settings and preferences
- **JSON Files**: Complex game state and progress
- **ScriptableObjects**: Design-time data and configuration
- **Binary Serialization**: Performance-critical save data

---
**Generation Metadata:**
- Generated by: ConceptualDataModelGenerator
- Based on game concept analysis
- Generation Date: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}
---

<!-- End ConceptualDataModelGenerator -->");
        }
        
        /// <summary>
        /// Generate conceptual API specification based on game description
        /// </summary>
        private async Task<string> GenerateConceptualAPISpec(string gameDescription)
        {
            return await Task.FromResult($@"<!-- Generated by ConceptualAPISpecGenerator -->
# API Specification

*Generated on {System.DateTime.Now:yyyy-MM-dd HH:mm:ss} UTC*

## Public API Overview

Based on the game concept, the following public APIs are recommended:

### Core Game APIs

```csharp
// Game management
public interface IGameManager
{{
    void StartGame();
    void PauseGame();
    void EndGame();
    GameState CurrentState {{ get; }}
}}

// Player management
public interface IPlayerController
{{
    void MovePlayer(Vector3 direction);
    void HandleInput(InputAction action);
    PlayerData GetPlayerData();
}}
```

### Game-Specific APIs

{GenerateGameSpecificAPIs(gameDescription)}

### Event System

```csharp
// Game events
public static class GameEvents
{{
    public static event System.Action<GameState> OnGameStateChanged;
    public static event System.Action<PlayerData> OnPlayerDataChanged;
    public static event System.Action<float> OnScoreChanged;
}}
```

### Integration Points

- **Unity Input System**: Modern input handling
- **Unity Audio**: Sound and music integration
- **Unity UI Toolkit**: Modern UI development
- **Unity Analytics**: Player behavior tracking

---
**Generation Metadata:**
- Generated by: ConceptualAPISpecGenerator
- Based on game concept analysis
- Generation Date: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}
---

<!-- End ConceptualAPISpecGenerator -->");
        }
        
        private string GenerateGameSpecificSystems(string gameDescription)
        {
            string description = gameDescription.ToLower();
            System.Text.StringBuilder systems = new System.Text.StringBuilder();
            
            if (description.Contains("inventory") || description.Contains("item"))
                systems.AppendLine("- **Inventory System**: Item collection and management");
            if (description.Contains("combat") || description.Contains("battle"))
                systems.AppendLine("- **Combat System**: Battle mechanics and damage calculation");
            if (description.Contains("dialogue") || description.Contains("conversation"))
                systems.AppendLine("- **Dialogue System**: NPC conversations and story progression");
            if (description.Contains("quest") || description.Contains("mission"))
                systems.AppendLine("- **Quest System**: Mission tracking and completion");
            if (description.Contains("multiplayer") || description.Contains("online"))
                systems.AppendLine("- **Network System**: Multiplayer functionality and synchronization");
            if (description.Contains("procedural") || description.Contains("random"))
                systems.AppendLine("- **Procedural Generation**: Dynamic content creation");
            
            return systems.Length > 0 ? systems.ToString() : "- **Core Gameplay System**: Main game mechanics implementation";
        }
        
        private string GenerateGameSpecificDataModel(string gameDescription)
        {
            string description = gameDescription.ToLower();
            System.Text.StringBuilder dataModel = new System.Text.StringBuilder();
            
            dataModel.AppendLine("```csharp");
            
            if (description.Contains("inventory") || description.Contains("item"))
            {
                dataModel.AppendLine(@"// Inventory system
[Serializable]
public class InventoryData
{
    public List<ItemData> items;
    public int maxCapacity;
}

[Serializable]
public class ItemData
{
    public string itemId;
    public string itemName;
    public int quantity;
}");
            }
            
            if (description.Contains("level") || description.Contains("stage"))
            {
                dataModel.AppendLine(@"
// Level progression
[Serializable]
public class LevelData
{
    public int levelNumber;
    public string levelName;
    public bool isUnlocked;
    public float bestTime;
}");
            }
            
            dataModel.AppendLine("```");
            return dataModel.ToString();
        }
        
        private string GenerateGameSpecificAPIs(string gameDescription)
        {
            string description = gameDescription.ToLower();
            System.Text.StringBuilder apis = new System.Text.StringBuilder();
            
            apis.AppendLine("```csharp");
            
            if (description.Contains("inventory"))
            {
                apis.AppendLine(@"// Inventory management
public interface IInventorySystem
{
    bool AddItem(ItemData item);
    bool RemoveItem(string itemId);
    List<ItemData> GetAllItems();
}");
            }
            
            if (description.Contains("combat"))
            {
                apis.AppendLine(@"
// Combat system
public interface ICombatSystem
{
    void AttackTarget(GameObject target);
    void TakeDamage(float damage);
    bool IsAlive();
}");
            }
            
            apis.AppendLine("```");
            return apis.ToString();
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