using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityProjectArchitect.AI;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services
{
    public class SystemArchitectureGenerator : BaseDocumentationGenerator
    {
        public SystemArchitectureGenerator(ProjectAnalysisResult analysisResult) 
            : base(analysisResult, DocumentationSectionType.SystemArchitecture)
        {
        }

        public override async Task<string> GenerateContentAsync()
        {
            // Try AI generation first if available
            try
            {
                System.Diagnostics.Debug.WriteLine("SystemArchitectureGenerator: Starting AI generation attempt...");
                string aiContent = await TryGenerateWithAIAsync();
                if (!string.IsNullOrEmpty(aiContent))
                {
                    System.Diagnostics.Debug.WriteLine($"SystemArchitectureGenerator: AI generation successful! Content length: {aiContent.Length}");
                    return aiContent;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("SystemArchitectureGenerator: AI generation returned empty content, falling back to offline mode");
                }
            }
            catch (Exception ex)
            {
                // Log the error but continue with fallback generation
                System.Diagnostics.Debug.WriteLine($"SystemArchitectureGenerator: AI generation failed, using offline fallback: {ex.Message}");
            }

            // Fallback to standard generation
            System.Diagnostics.Debug.WriteLine("SystemArchitectureGenerator: Starting fallback generation...");
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetSectionHeader("System Architecture"));
            sb.AppendLine(AddTimestamp());

            sb.AppendLine(await GenerateArchitectureOverviewAsync());
            sb.AppendLine(await GenerateComponentDiagramAsync());
            sb.AppendLine(await GenerateLayerArchitectureAsync());
            sb.AppendLine(await GenerateDependencyAnalysisAsync());
            sb.AppendLine(await GenerateArchitecturalPatternsAsync());
            sb.AppendLine(await GenerateArchitecturalInsightsAsync());

            sb.AppendLine(AddGenerationMetadata());

            string result = await WrapInProgressIndicator(sb.ToString(), "System Architecture Generation");
            System.Diagnostics.Debug.WriteLine($"SystemArchitectureGenerator: Fallback generation complete! Content length: {result.Length}");
            return result;
        }

        private async Task<string> TryGenerateWithAIAsync()
        {
            try
            {
                // Get properly configured AI assistant
                IAIAssistant aiAssistant = null;
                #if UNITY_EDITOR
                // Use Unity bridge's configured AI assistant
                System.Diagnostics.Debug.WriteLine("SystemArchitectureGenerator: Getting AI assistant from Unity bridge...");
                aiAssistant = UnityProjectArchitect.Unity.UnityServiceBridge.GetAIAssistant();
                System.Diagnostics.Debug.WriteLine($"SystemArchitectureGenerator: AI assistant retrieved. Is null? {aiAssistant == null}");
                if (aiAssistant != null)
                {
                    System.Diagnostics.Debug.WriteLine($"SystemArchitectureGenerator: AI assistant IsConfigured: {aiAssistant.IsConfigured}");
                }
                #else
                // For non-Unity builds, create a default AI assistant
                System.Diagnostics.Debug.WriteLine("SystemArchitectureGenerator: Creating default AI assistant...");
                aiAssistant = new UnityProjectArchitect.AI.Services.AIAssistant();
                System.Diagnostics.Debug.WriteLine($"SystemArchitectureGenerator: Default AI assistant IsConfigured: {aiAssistant?.IsConfigured}");
                #endif

                if (aiAssistant?.IsConfigured == true)
                {
                    // Create AI request for system architecture
                    AIRequest request = new AIRequest
                    {
                        RequestType = AIRequestType.Generation,
                        Prompt = BuildSystemArchitecturePrompt(),
                        ProjectContext = BuildProjectData(),
                        SectionType = DocumentationSectionType.SystemArchitecture,
                        Configuration = new AIConfiguration
                        {
                            Provider = AIProvider.Claude,
                            MaxTokens = 4000,
                            Temperature = 0.7f
                        }
                    };

                    AIOperationResult result = await aiAssistant.GenerateContentAsync(request);
                    if (result.Success && !string.IsNullOrEmpty(result.Content))
                    {
                        // Add AI generation metadata
                        StringBuilder aiContent = new StringBuilder();
                        aiContent.AppendLine("<!-- Generated by SystemArchitectureGenerator using AI -->");
                        aiContent.AppendLine($"*Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss} UTC using {result.Provider} AI*");
                        aiContent.AppendLine();
                        aiContent.AppendLine(result.Content);
                        aiContent.AppendLine();
                        aiContent.AppendLine("---");
                        aiContent.AppendLine("**AI Generation Metadata:**");
                        aiContent.AppendLine("- Generated by: SystemArchitectureGenerator with AI");
                        aiContent.AppendLine($"- AI Provider: {result.Provider}");
                        aiContent.AppendLine($"- Processing Time: {result.ProcessingTime.TotalSeconds:F2} seconds");
                        aiContent.AppendLine($"- Confidence Score: {result.ConfidenceScore:P0}");
                        aiContent.AppendLine($"- Generation Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        aiContent.AppendLine("---");
                        aiContent.AppendLine("<!-- End SystemArchitectureGenerator -->");
                        
                        return aiContent.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log detailed error information
                System.Diagnostics.Debug.WriteLine($"AI generation error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }

            return null; // Indicates fallback should be used
        }

        private string BuildSystemArchitecturePrompt()
        {
            string projectName = analysisResult.ProjectPath != null ? 
                System.IO.Path.GetFileName(analysisResult.ProjectPath.TrimEnd(System.IO.Path.DirectorySeparatorChar)) : 
                "Unity Project";
            
            return $@"Generate a comprehensive System Architecture document for a Unity project called '{projectName}'. 

Based on the project analysis data provided, create a detailed system architecture that includes:

1. **Architecture Overview**: Primary architecture pattern, metrics (components, connections, layers), coupling and cohesion analysis
2. **Component Architecture**: Component diagram with categories, relationships, and integration points
3. **Layer Architecture**: Layered structure with dependencies and level organization  
4. **Dependency Analysis**: Code dependencies, circular dependency detection, asset dependencies
5. **Architectural Patterns**: Detected design patterns, Unity-specific patterns, modular organization
6. **Architectural Insights**: Issues, recommendations, and improvement opportunities

Use the actual project analysis data provided in the context. Include specific metrics, component counts, and architectural details where available. Create Mermaid diagrams for visualizing architecture components and relationships. Focus on Unity-specific architectural patterns and best practices.

Format the output as professional markdown with clear headers, code blocks, and Mermaid diagrams where appropriate.";
        }

        private ProjectData BuildProjectData()
        {
            return new ProjectData
            {
                ProjectName = analysisResult.ProjectPath != null ? 
                    System.IO.Path.GetFileName(analysisResult.ProjectPath.TrimEnd(System.IO.Path.DirectorySeparatorChar)) : 
                    "Unity Project",
                ProjectDescription = $"Unity project analyzed on {analysisResult.AnalyzedAt:yyyy-MM-dd}"
            };
        }

        private async Task<string> GenerateArchitectureOverviewAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Architecture Overview", 2));

                if (analysisResult.Architecture?.DetectedPattern != ArchitecturePattern.None)
                {
                    sb.AppendLine($"**Primary Architecture Pattern:** {analysisResult.Architecture.DetectedPattern}");
                    sb.AppendLine($"{FormatArchitecturePattern(analysisResult.Architecture.DetectedPattern)}");
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine("**Architecture Pattern:** Custom/Mixed approach");
                    sb.AppendLine("The project uses a flexible architecture that may combine multiple patterns or follow custom conventions.");
                    sb.AppendLine();
                }

                if (analysisResult.Architecture != null)
                {
                    int totalComponents = analysisResult.Architecture.Components?.Count ?? 0;
                    int totalConnections = analysisResult.Architecture.Connections?.Count ?? 0;
                    int totalLayers = analysisResult.Architecture.Layers?.Count ?? 0;

                    sb.AppendLine("**Architecture Metrics:**");
                    sb.AppendLine($"- **Components:** {totalComponents} architectural components");
                    sb.AppendLine($"- **Connections:** {totalConnections} inter-component relationships");
                    sb.AppendLine($"- **Layers:** {totalLayers} distinct architectural layers");
                    
                    if (analysisResult.Architecture.Metrics != null)
                    {
                        ArchitectureMetrics metrics = analysisResult.Architecture.Metrics;
                        sb.AppendLine($"- **Average Coupling:** {metrics.AverageCoupling:F2} (lower is better)");
                        sb.AppendLine($"- **Average Cohesion:** {metrics.AverageCohesion:F2} (higher is better)");
                        sb.AppendLine($"- **Distance from Main Sequence:** {metrics.DistanceFromMainSequence:F2}");
                    }
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateComponentDiagramAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Component Architecture", 2));

                if (analysisResult.Architecture?.Components?.Any() == true)
                {
                    sb.AppendLine("```mermaid");
                    sb.AppendLine("graph TD");
                    sb.AppendLine("    %% Component Architecture Diagram");
                    sb.AppendLine();

                    List<IGrouping<ComponentCategory, ComponentInfo>> componentsByCategory = analysisResult.Architecture.Components
                        .GroupBy(c => c.Category)
                        .OrderBy(g => g.Key)
                        .ToList();

                    foreach (IGrouping<ComponentCategory, ComponentInfo> categoryGroup in componentsByCategory)
                    {
                        sb.AppendLine($"    %% {categoryGroup.Key} Components");
                        
                        foreach (ComponentInfo component in categoryGroup.Take(10)) // Limit for readability
                        {
                            string nodeId = GetSafeNodeId(component.Name);
                            string nodeStyle = GetComponentNodeStyle(component.Category);
                            sb.AppendLine($"    {nodeId}[\"{component.Name}\"]");
                            sb.AppendLine($"    {nodeId} --> {nodeId}_type{{\"Type: {component.Type}\"}}");
                            sb.AppendLine($"    class {nodeId} {nodeStyle}");
                        }
                        sb.AppendLine();
                    }

                    // Add connections between components
                    if (analysisResult.Architecture.Connections?.Any() == true)
                    {
                        sb.AppendLine("    %% Component Connections");
                        foreach (SystemConnection connection in analysisResult.Architecture.Connections.Take(20))
                        {
                            string fromId = GetSafeNodeId(connection.FromComponent);
                            string toId = GetSafeNodeId(connection.ToComponent);
                            string connectionStyle = GetConnectionStyle(connection.Type);
                            sb.AppendLine($"    {fromId} {connectionStyle} {toId}");
                        }
                    }

                    // Add styling
                    sb.AppendLine();
                    sb.AppendLine("    %% Styling");
                    sb.AppendLine("    classDef coreComponent fill:#e1f5fe,stroke:#01579b,stroke-width:2px");
                    sb.AppendLine("    classDef uiComponent fill:#f3e5f5,stroke:#4a148c,stroke-width:2px");
                    sb.AppendLine("    classDef gameplayComponent fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px");
                    sb.AppendLine("    classDef utilityComponent fill:#fff3e0,stroke:#e65100,stroke-width:2px");
                    sb.AppendLine("```");
                    sb.AppendLine();

                    sb.AppendLine("**Component Categories:**");
                    foreach (IGrouping<ComponentCategory, ComponentInfo> categoryGroup in componentsByCategory)
                    {
                        sb.AppendLine($"- **{categoryGroup.Key}:** {categoryGroup.Count()} components");
                        IEnumerable<string> topComponents = categoryGroup.Take(5).Select(c => c.Name);
                        sb.AppendLine($"  - {string.Join(", ", topComponents)}");
                    }
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine("Component architecture will be visualized as the project structure develops and more components are identified.");
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateLayerArchitectureAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Layer Architecture", 2));

                if (analysisResult.Architecture?.Layers?.Any() == true)
                {
                    List<LayerInfo> layersByLevel = analysisResult.Architecture.Layers
                        .OrderBy(l => l.Level)
                        .ToList();

                    sb.AppendLine("```mermaid");
                    sb.AppendLine("graph TB");
                    sb.AppendLine("    %% Layered Architecture");
                    sb.AppendLine();

                    foreach (LayerInfo layer in layersByLevel)
                    {
                        string layerId = GetSafeNodeId(layer.Name);
                        sb.AppendLine($"    {layerId}[\"{layer.Name} Layer\"]");
                        sb.AppendLine($"    {layerId}_desc[\"{layer.Description ?? $"Level {layer.Level}"}\"]");
                        sb.AppendLine($"    {layerId} --> {layerId}_desc");
                        
                        if (layer.Components.Any())
                        {
                            int componentCount = layer.Components.Count;
                            sb.AppendLine($"    {layerId}_count[\"{componentCount} Components\"]");
                            sb.AppendLine($"    {layerId} --> {layerId}_count");
                        }
                    }

                    // Add layer dependencies
                    for (int i = 0; i < layersByLevel.Count - 1; i++)
                    {
                        string currentLayer = GetSafeNodeId(layersByLevel[i].Name);
                        string nextLayer = GetSafeNodeId(layersByLevel[i + 1].Name);
                        sb.AppendLine($"    {currentLayer} --> {nextLayer}");
                    }

                    sb.AppendLine("```");
                    sb.AppendLine();

                    sb.AppendLine("**Layer Details:**");
                    foreach (LayerInfo layer in layersByLevel)
                    {
                        sb.AppendLine($"- **{layer.Name} (Level {layer.Level}):** {layer.Components.Count} components");
                        if (!string.IsNullOrEmpty(layer.Description))
                        {
                            sb.AppendLine($"  - {layer.Description}");
                        }
                        if (layer.Components.Any())
                        {
                            IEnumerable<string> topComponents = layer.Components.Take(5);
                            sb.AppendLine($"  - Components: {string.Join(", ", topComponents)}");
                        }
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("Layer architecture will be defined as the project grows and architectural patterns become more established.");
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateDependencyAnalysisAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Dependency Analysis", 2));

                if (analysisResult.Scripts?.Dependencies != null)
                {
                    DependencyGraph dependencies = analysisResult.Scripts.Dependencies;
                    
                    sb.AppendLine("**Dependency Overview:**");
                    sb.AppendLine($"- **Total Nodes:** {dependencies.Nodes.Count} code elements");
                    sb.AppendLine($"- **Total Dependencies:** {dependencies.Edges.Count} relationships");
                    sb.AppendLine($"- **Average Dependencies per Node:** {(dependencies.Nodes.Count > 0 ? (float)dependencies.Edges.Count / dependencies.Nodes.Count : 0):F1}");
                    sb.AppendLine();

                    // Check for circular dependencies
                    IEnumerable<string> circularDeps = dependencies.GetCircularDependencies();
                    if (circularDeps.Any())
                    {
                        sb.AppendLine("⚠️ **Circular Dependencies Detected:**");
                        foreach (string cycle in circularDeps.Take(5))
                        {
                            sb.AppendLine($"- {cycle}");
                        }
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.AppendLine("✅ **No circular dependencies detected** - Clean dependency structure");
                        sb.AppendLine();
                    }

                    // Dependency graph visualization
                    if (dependencies.Nodes.Count <= 20) // Only show for smaller projects
                    {
                        sb.AppendLine("**Dependency Graph:**");
                        sb.AppendLine("```mermaid");
                        sb.AppendLine("graph LR");
                        sb.AppendLine("    %% Dependency Relationships");
                        
                        foreach (DependencyEdge edge in dependencies.Edges.Take(15))
                        {
                            string fromId = GetSafeNodeId(edge.FromId);
                            string toId = GetSafeNodeId(edge.ToId);
                            string arrow = edge.DependencyType == DependencyType.Inheritance ? "==> |inherits|" : "--> |uses|";
                            sb.AppendLine($"    {fromId} {arrow} {toId}");
                        }
                        
                        sb.AppendLine("```");
                        sb.AppendLine();
                    }
                }

                if (analysisResult.Assets?.Dependencies?.Any() == true)
                {
                    List<AssetDependency> assetDeps = analysisResult.Assets.Dependencies;
                    int unusedAssets = assetDeps.Where(d => d.Type == AssetDependencyType.Unused).Count();
                    int missingAssets = assetDeps.Where(d => d.Type == AssetDependencyType.Missing).Count();

                    sb.AppendLine("**Asset Dependencies:**");
                    sb.AppendLine($"- **Total Asset Dependencies:** {assetDeps.Count}");
                    
                    if (unusedAssets > 0)
                    {
                        sb.AppendLine($"- **Unused Assets:** {unusedAssets} assets not referenced");
                    }
                    
                    if (missingAssets > 0)
                    {
                        sb.AppendLine($"- **Missing Assets:** {missingAssets} missing references");
                    }
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateArchitecturalPatternsAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Architectural Patterns", 2));

                if (analysisResult.Scripts?.DetectedPatterns?.Any() == true)
                {
                    List<DesignPattern> patterns = analysisResult.Scripts.DetectedPatterns
                        .Where(p => p.Confidence > 0.6f)
                        .OrderByDescending(p => p.Confidence)
                        .ToList();

                    if (patterns.Any())
                    {
                        sb.AppendLine("**Detected Design Patterns:**");
                        foreach (DesignPattern pattern in patterns)
                        {
                            string confidenceBar = new string('█', (int)(pattern.Confidence * 10));
                            sb.AppendLine($"- **{pattern.Name}** (Confidence: {pattern.Confidence:P0}) `{confidenceBar}`");
                            sb.AppendLine($"  - Evidence: {pattern.Evidence}");
                            sb.AppendLine($"  - Classes: {string.Join(", ", pattern.InvolvedClasses)}");
                            sb.AppendLine();
                        }
                    }
                }

                if (analysisResult.Structure?.AssemblyDefinitions?.Any() == true)
                {
                    sb.AppendLine("**Modular Architecture:**");
                    sb.AppendLine($"The project uses {analysisResult.Structure.AssemblyDefinitions.Count} assembly definitions for modular organization:");
                    
                    foreach (AssemblyDefinitionInfo assembly in analysisResult.Structure.AssemblyDefinitions.Take(10))
                    {
                        sb.AppendLine($"- **{assembly.Name}**");
                        if (assembly.References.Any())
                        {
                            sb.AppendLine($"  - Dependencies: {string.Join(", ", assembly.References.Take(3))}");
                        }
                    }
                    sb.AppendLine();
                }

                if (analysisResult.Scripts?.Classes?.Any(c => c.IsMonoBehaviour) == true)
                {
                    int monoBehaviours = analysisResult.Scripts.Classes.Count(c => c.IsMonoBehaviour);
                    int scriptableObjects = analysisResult.Scripts.Classes.Count(c => c.IsScriptableObject);
                    
                    sb.AppendLine("**Unity-Specific Patterns:**");
                    sb.AppendLine($"- **Component Pattern:** {monoBehaviours} MonoBehaviour components");
                    sb.AppendLine($"- **Data-Oriented Design:** {scriptableObjects} ScriptableObject data containers");
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateArchitecturalInsightsAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();

                if (analysisResult.Architecture?.Issues?.Any() == true)
                {
                    List<ArchitectureIssue> criticalIssues = analysisResult.Architecture.Issues
                        .Where(i => i.Severity == ArchitectureIssueSeverity.Critical || i.Severity == ArchitectureIssueSeverity.Major)
                        .OrderByDescending(i => i.Severity)
                        .Take(5)
                        .ToList();

                    if (criticalIssues.Any())
                    {
                        sb.AppendLine(GetSectionHeader("Architectural Issues", 3));
                        foreach (ArchitectureIssue issue in criticalIssues)
                        {
                            string severityIcon = issue.Severity switch
                            {
                                ArchitectureIssueSeverity.Critical => "🔴",
                                ArchitectureIssueSeverity.Major => "🟠",
                                ArchitectureIssueSeverity.Minor => "🟡",
                                _ => "ℹ️"
                            };

                            sb.AppendLine($"{severityIcon} **{issue.Type}**: {issue.Description}");
                            if (!string.IsNullOrEmpty(issue.Suggestion))
                            {
                                sb.AppendLine($"   - Suggestion: {issue.Suggestion}");
                            }
                        }
                        sb.AppendLine();
                    }
                }

                List<ProjectInsight> architectureInsights = analysisResult.Insights?
                    .Where(i => i.Type == InsightType.Architecture)
                    .OrderByDescending(i => i.Severity)
                    .Take(3)
                    .ToList();

                if (architectureInsights?.Any() == true)
                {
                    sb.Append(FormatInsightsList(architectureInsights, "Architecture Insights"));
                }

                List<ProjectRecommendation> architectureRecommendations = analysisResult.Recommendations?
                    .Where(r => r.Type == RecommendationType.Architecture)
                    .OrderByDescending(r => r.Priority)
                    .Take(3)
                    .ToList();

                if (architectureRecommendations?.Any() == true)
                {
                    sb.Append(FormatRecommendationsList(architectureRecommendations, "Architecture Improvements"));
                }

                return sb.ToString();
            });
        }

        private string GetSafeNodeId(string name)
        {
            return System.Text.RegularExpressions.Regex.Replace(name, @"[^a-zA-Z0-9_]", "_");
        }

        private string GetComponentNodeStyle(ComponentCategory category)
        {
            return category switch
            {
                ComponentCategory.Core => "coreComponent",
                ComponentCategory.UI => "uiComponent",
                ComponentCategory.Gameplay => "gameplayComponent",
                ComponentCategory.Utility => "utilityComponent",
                _ => "coreComponent"
            };
        }

        private string GetConnectionStyle(ConnectionType connectionType)
        {
            return connectionType switch
            {
                ConnectionType.Inheritance => "==>",
                ConnectionType.Composition => "-->|contains|",
                ConnectionType.Dependency => "-.->",
                ConnectionType.DataFlow => "-->|data|",
                _ => "-->"
            };
        }
    }
}