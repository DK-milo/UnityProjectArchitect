using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityProjectArchitect.AI;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services
{
    public class GeneralProductDescriptionGenerator : BaseDocumentationGenerator
    {
        public GeneralProductDescriptionGenerator(ProjectAnalysisResult analysisResult) 
            : base(analysisResult, DocumentationSectionType.GeneralProductDescription)
        {
        }

        public override async Task<string> GenerateContentAsync()
        {
            // Try AI generation first if available
            try
            {
                System.Diagnostics.Debug.WriteLine("GeneralProductDescriptionGenerator: Starting AI generation attempt...");
                string aiContent = await TryGenerateWithAIAsync();
                if (!string.IsNullOrEmpty(aiContent))
                {
                    System.Diagnostics.Debug.WriteLine($"GeneralProductDescriptionGenerator: AI generation successful! Content length: {aiContent.Length}");
                    return aiContent;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("GeneralProductDescriptionGenerator: AI generation returned empty content, falling back to offline mode");
                }
            }
            catch (Exception ex)
            {
                // Log the error but continue with fallback generation
                System.Diagnostics.Debug.WriteLine($"GeneralProductDescriptionGenerator: AI generation failed, using offline fallback: {ex.Message}");
            }

            // Fallback to standard generation
            System.Diagnostics.Debug.WriteLine("GeneralProductDescriptionGenerator: Starting fallback generation...");
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetSectionHeader("General Product Description"));
            sb.AppendLine(AddTimestamp());

            sb.AppendLine(await GenerateProjectOverviewAsync());
            sb.AppendLine(await GenerateProjectTypeAndScopeAsync());
            sb.AppendLine(await GenerateKeyFeaturesAsync());
            sb.AppendLine(await GenerateTechnicalHighlightsAsync());
            sb.AppendLine(await GenerateProjectMetricsAsync());
            sb.AppendLine(await GenerateProjectInsightsAsync());

            sb.AppendLine(AddGenerationMetadata());

            string result = await WrapInProgressIndicator(sb.ToString(), "General Product Description Generation");
            System.Diagnostics.Debug.WriteLine($"GeneralProductDescriptionGenerator: Fallback generation complete! Content length: {result.Length}");
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
                System.Diagnostics.Debug.WriteLine("GeneralProductDescriptionGenerator: Getting AI assistant from Unity bridge...");
                aiAssistant = UnityProjectArchitect.Unity.UnityServiceBridge.GetAIAssistant();
                System.Diagnostics.Debug.WriteLine($"GeneralProductDescriptionGenerator: AI assistant retrieved. Is null? {aiAssistant == null}");
                if (aiAssistant != null)
                {
                    System.Diagnostics.Debug.WriteLine($"GeneralProductDescriptionGenerator: AI assistant IsConfigured: {aiAssistant.IsConfigured}");
                }
                #else
                // For non-Unity builds, create a default AI assistant
                System.Diagnostics.Debug.WriteLine("GeneralProductDescriptionGenerator: Creating default AI assistant...");
                aiAssistant = new UnityProjectArchitect.AI.Services.AIAssistant();
                System.Diagnostics.Debug.WriteLine($"GeneralProductDescriptionGenerator: Default AI assistant IsConfigured: {aiAssistant?.IsConfigured}");
                #endif

                if (aiAssistant?.IsConfigured == true)
                {
                    // Create AI request for general product description
                    AIRequest request = new AIRequest
                    {
                        RequestType = AIRequestType.Generation,
                        Prompt = BuildProductDescriptionPrompt(),
                        ProjectContext = BuildProjectData(),
                        SectionType = DocumentationSectionType.GeneralProductDescription,
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
                        aiContent.AppendLine("<!-- Generated by GeneralProductDescriptionGenerator using AI -->");
                        aiContent.AppendLine($"*Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss} UTC using {result.Provider} AI*");
                        aiContent.AppendLine();
                        aiContent.AppendLine(result.Content);
                        aiContent.AppendLine();
                        aiContent.AppendLine("---");
                        aiContent.AppendLine("**AI Generation Metadata:**");
                        aiContent.AppendLine("- Generated by: GeneralProductDescriptionGenerator with AI");
                        aiContent.AppendLine($"- AI Provider: {result.Provider}");
                        aiContent.AppendLine($"- Processing Time: {result.ProcessingTime.TotalSeconds:F2} seconds");
                        aiContent.AppendLine($"- Confidence Score: {result.ConfidenceScore:P0}");
                        aiContent.AppendLine($"- Generation Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        aiContent.AppendLine("---");
                        aiContent.AppendLine("<!-- End GeneralProductDescriptionGenerator -->");
                        
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

        private string BuildProductDescriptionPrompt()
        {
            string projectName = GetProjectName();
            
            return $@"Generate a comprehensive General Product Description for a Unity project called '{projectName}'. 

Based on the project analysis data provided, create a professional product description that includes:

1. **Project Overview**: Name, type, Unity version, and analysis date
2. **Project Type & Scope**: Detected project type, architecture pattern, folder structure, file counts
3. **Key Features & Components**: Identified features, design patterns, and components
4. **Technical Highlights**: Code quality metrics, performance characteristics, assembly organization
5. **Project Metrics**: Asset distribution, code metrics, and project statistics
6. **Project Insights**: Important insights and improvement opportunities

Use the actual project data provided in the context. Format the output as professional markdown with clear headers and sections. Include specific numbers and metrics where available. Make it sound professional and technical but accessible.

Focus on factual information derived from the project analysis rather than speculative content.";
        }

        private ProjectData BuildProjectData()
        {
            return new ProjectData
            {
                ProjectName = GetProjectName(),
                ProjectDescription = $"Unity project analyzed on {analysisResult.AnalyzedAt:yyyy-MM-dd}"
            };
        }

        private async Task<string> GenerateProjectOverviewAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Project Overview", 2));

                string projectName = GetProjectName();
                sb.AppendLine($"**Project Name:** {projectName}");

                if (analysisResult.Structure != null)
                {
                    string projectTypeDesc = GetProjectTypeDescription(analysisResult.Structure.DetectedProjectType);
                    sb.AppendLine($"**Project Type:** {projectTypeDesc}");

                    if (analysisResult.Structure.DetectedUnityVersion != UnityVersion.Unknown)
                    {
                        sb.AppendLine($"**Unity Version:** {analysisResult.Structure.DetectedUnityVersion}");
                    }
                }

                sb.AppendLine($"**Analysis Date:** {analysisResult.AnalyzedAt:yyyy-MM-dd}");
                sb.AppendLine();

                if (analysisResult.Structure?.FollowsStandardStructure == true)
                {
                    sb.AppendLine("This project follows Unity's recommended project structure conventions, indicating a well-organized codebase that should be easy to navigate and maintain.");
                }
                else
                {
                    sb.AppendLine("This project uses a custom organization structure. Consider reviewing the project organization for potential improvements.");
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateProjectTypeAndScopeAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Project Type & Scope", 2));

                if (analysisResult.Structure != null)
                {
                    ProjectType projectType = analysisResult.Structure.DetectedProjectType;
                    sb.AppendLine($"**Detected Project Type:** {projectType}");
                    sb.AppendLine($"{GetProjectTypeDescription(projectType)}");
                    sb.AppendLine();

                    if (analysisResult.Architecture?.DetectedPattern != ArchitecturePattern.None)
                    {
                        sb.AppendLine($"**Architecture Pattern:** {analysisResult.Architecture.DetectedPattern}");
                        sb.AppendLine($"{FormatArchitecturePattern(analysisResult.Architecture.DetectedPattern)}");
                        sb.AppendLine();
                    }

                    int folders = analysisResult.Structure.Folders?.Count ?? 0;
                    int files = analysisResult.Structure.Files?.Count ?? 0;
                    int scenes = analysisResult.Structure.Scenes?.Count ?? 0;

                    sb.AppendLine("**Project Scope:**");
                    sb.AppendLine($"- **Folders:** {folders} directories organizing project assets");
                    sb.AppendLine($"- **Files:** {files} total files across all project directories");
                    sb.AppendLine($"- **Scenes:** {scenes} Unity scenes defining game levels/states");

                    if (analysisResult.Scripts != null)
                    {
                        sb.AppendLine($"- **Scripts:** {analysisResult.Scripts.TotalClasses} C# classes with {analysisResult.Scripts.TotalMethods} methods");
                        sb.AppendLine($"- **Code Volume:** {analysisResult.Scripts.TotalLinesOfCode:N0} lines of code");
                    }
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateKeyFeaturesAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Key Features & Components", 2));

                List<string> features = ExtractKeyFeatures();
                if (features.Any())
                {
                    sb.AppendLine("Based on project analysis, the following key features and components were identified:");
                    sb.AppendLine();
                    sb.Append(FormatList(features));
                }
                else
                {
                    sb.AppendLine("Key features will be identified as the project structure evolves and more components are implemented.");
                    sb.AppendLine();
                }

                if (analysisResult.Scripts?.DetectedPatterns?.Any() == true)
                {
                    sb.AppendLine("**Implemented Design Patterns:**");
                    List<string> patterns = analysisResult.Scripts.DetectedPatterns
                        .Where(p => p.Confidence > 0.7f)
                        .Select(p => $"{p.Name} - {p.Evidence}")
                        .ToList();
                    
                    if (patterns.Any())
                    {
                        sb.Append(FormatList(patterns));
                    }
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateTechnicalHighlightsAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Technical Highlights", 2));

                if (analysisResult.Scripts?.Metrics != null)
                {
                    CodeMetrics metrics = analysisResult.Scripts.Metrics;
                    
                    sb.AppendLine("**Code Quality Metrics:**");
                    sb.AppendLine($"- **Complexity:** Average cyclomatic complexity of {metrics.AverageCyclomaticComplexity:F1}");
                    sb.AppendLine($"- **Methods per Class:** {metrics.MethodsPerClass:F1} average methods per class");
                    
                    if (metrics.CommentRatio > 0)
                    {
                        sb.AppendLine($"- **Documentation:** {metrics.CommentRatio * 100:F1}% comment-to-code ratio");
                    }
                    sb.AppendLine();
                }

                if (analysisResult.Performance?.Metrics != null)
                {
                    PerformanceMetrics perfMetrics = analysisResult.Performance.Metrics;
                    sb.AppendLine("**Performance Characteristics:**");
                    
                    if (perfMetrics.TextureMemoryMB > 0)
                    {
                        sb.AppendLine($"- **Texture Memory:** {perfMetrics.TextureMemoryMB}MB of texture assets");
                    }
                    
                    if (perfMetrics.AudioMemoryMB > 0)
                    {
                        sb.AppendLine($"- **Audio Memory:** {perfMetrics.AudioMemoryMB}MB of audio assets");
                    }
                    
                    if (perfMetrics.DrawCalls > 0)
                    {
                        sb.AppendLine($"- **Rendering:** Estimated {perfMetrics.DrawCalls} draw calls");
                    }
                    sb.AppendLine();
                }

                if (analysisResult.Structure?.AssemblyDefinitions?.Any() == true)
                {
                    sb.AppendLine($"**Assembly Organization:** {analysisResult.Structure.AssemblyDefinitions.Count} assembly definitions providing modular code organization");
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateProjectMetricsAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Project Metrics", 2));

                if (analysisResult.Metrics != null)
                {
                    sb.Append(FormatMetrics(analysisResult.Metrics));
                }

                if (analysisResult.Assets?.Metrics != null)
                {
                    AssetMetrics assetMetrics = analysisResult.Assets.Metrics;
                    sb.AppendLine("**Asset Distribution:**");
                    
                    if (assetMetrics.AssetCountByType.Any())
                    {
                        foreach (KeyValuePair<string, int> assetType in assetMetrics.AssetCountByType.OrderByDescending(kvp => kvp.Value).Take(5))
                        {
                            sb.AppendLine($"- **{assetType.Key}:** {assetType.Value} files");
                        }
                        sb.AppendLine();
                    }
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateProjectInsightsAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();

                if (analysisResult.Insights?.Any() == true)
                {
                    List<ProjectInsight> importantInsights = analysisResult.Insights
                        .Where(i => i.Severity >= InsightSeverity.Medium)
                        .OrderByDescending(i => i.Severity)
                        .ThenByDescending(i => i.Confidence)
                        .Take(5)
                        .ToList();

                    if (importantInsights.Any())
                    {
                        sb.Append(FormatInsightsList(importantInsights, "Key Project Insights"));
                    }
                }

                if (analysisResult.Recommendations?.Any() == true)
                {
                    List<ProjectRecommendation> topRecommendations = analysisResult.Recommendations
                        .Where(r => r.Priority >= RecommendationPriority.Medium)
                        .OrderByDescending(r => r.Priority)
                        .Take(3)
                        .ToList();

                    if (topRecommendations.Any())
                    {
                        sb.Append(FormatRecommendationsList(topRecommendations, "Improvement Opportunities"));
                    }
                }

                return sb.ToString();
            });
        }

        private string GetProjectName()
        {
            if (!string.IsNullOrEmpty(analysisResult.ProjectPath))
            {
                return System.IO.Path.GetFileName(analysisResult.ProjectPath.TrimEnd(System.IO.Path.DirectorySeparatorChar));
            }
            
            return "Unity Project";
        }

        private List<string> ExtractKeyFeatures()
        {
            List<string> features = new List<string>();

            if (analysisResult.Structure?.Scenes?.Any() == true)
            {
                int sceneCount = analysisResult.Structure.Scenes.Count;
                features.Add($"Multi-scene architecture with {sceneCount} game scenes");
            }

            if (analysisResult.Scripts?.Classes?.Any(c => c.IsMonoBehaviour) == true)
            {
                int gameplayScripts = analysisResult.Scripts.Classes.Count(c => c.IsMonoBehaviour);
                features.Add($"Interactive gameplay with {gameplayScripts} MonoBehaviour components");
            }

            if (analysisResult.Scripts?.Classes?.Any(c => c.IsScriptableObject) == true)
            {
                int dataObjects = analysisResult.Scripts.Classes.Count(c => c.IsScriptableObject);
                features.Add($"Data-driven design with {dataObjects} ScriptableObject configurations");
            }

            if (analysisResult.Assets?.Assets?.Any(a => a.AssetType == "Prefab") == true)
            {
                int prefabCount = analysisResult.Assets.Assets.Count(a => a.AssetType == "Prefab");
                features.Add($"Modular design with {prefabCount} reusable prefab components");
            }

            if (analysisResult.Assets?.Assets?.Any(a => a.AssetType == "Material") == true)
            {
                int materialCount = analysisResult.Assets.Assets.Count(a => a.AssetType == "Material");
                features.Add($"Custom visual styling with {materialCount} material definitions");
            }

            if (analysisResult.Architecture?.DetectedPattern != ArchitecturePattern.None)
            {
                features.Add($"Structured architecture following {analysisResult.Architecture.DetectedPattern} pattern");
            }

            return features;
        }
    }
}