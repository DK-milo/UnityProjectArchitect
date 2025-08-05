using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.AI.Prompts
{
    /// <summary>
    /// Intelligent project context extraction for AI prompts
    /// Analyzes project data to build comprehensive, contextually-aware prompt context
    /// </summary>
    public static class ContextBuilder
    {
        private const int MaxContextLength = 2000; // Maximum context length to avoid token limits
        private const int MaxItemsPerCategory = 10; // Maximum items to include per analysis category

        /// <summary>
        /// Builds comprehensive project context for AI prompts from project data and analysis results
        /// </summary>
        public static string BuildProjectContext(ProjectData projectData)
        {
            if (projectData == null)
                throw new ArgumentNullException(nameof(projectData));

            StringBuilder context = new StringBuilder();
            
            // Project Overview
            AppendProjectOverview(context, projectData);
            
            // Technical Context
            AppendTechnicalContext(context, projectData);
            
            // Content Analysis
            AppendContentAnalysis(context, projectData);
            
            // Documentation Context
            AppendDocumentationContext(context, projectData);

            string fullContext = context.ToString();
            
            // Truncate if too long while preserving structure
            if (fullContext.Length > MaxContextLength)
            {
                return TruncateContextIntelligently(fullContext, MaxContextLength);
            }

            return fullContext;
        }

        /// <summary>
        /// Builds specialized context for specific analysis types
        /// </summary>
        public static string BuildAnalysisContext(ProjectAnalysisResult analysisResult, AnalysisContextType contextType)
        {
            if (analysisResult == null)
                throw new ArgumentNullException(nameof(analysisResult));

            StringBuilder context = new StringBuilder();

            switch (contextType)
            {
                case AnalysisContextType.Architecture:
                    AppendArchitectureContext(context, analysisResult);
                    break;
                    
                case AnalysisContextType.Scripts:
                    AppendScriptContext(context, analysisResult);
                    break;
                    
                case AnalysisContextType.Assets:
                    AppendAssetContext(context, analysisResult);
                    break;
                    
                case AnalysisContextType.Performance:
                    AppendPerformanceContext(context, analysisResult);
                    break;
                    
                case AnalysisContextType.Issues:
                    AppendIssueContext(context, analysisResult);
                    break;
                    
                default:
                    AppendGeneralAnalysisContext(context, analysisResult);
                    break;
            }

            return context.ToString();
        }

        /// <summary>
        /// Builds context specific to documentation section requirements
        /// </summary>
        public static string BuildSectionContext(ProjectData projectData, DocumentationSectionData sectionData)
        {
            if (projectData == null)
                throw new ArgumentNullException(nameof(projectData));
            if (sectionData == null)
                throw new ArgumentNullException(nameof(sectionData));

            StringBuilder context = new StringBuilder();
            
            // Base project context
            string baseContext = BuildProjectContext(projectData);
            context.AppendLine(baseContext);
            context.AppendLine();
            
            // Section-specific context
            AppendSectionSpecificContext(context, sectionData, projectData);

            return context.ToString();
        }

        /// <summary>
        /// Extracts key insights and recommendations for context building
        /// </summary>
        public static string BuildInsightsContext(List<ProjectInsight> insights, List<ProjectRecommendation> recommendations)
        {
            StringBuilder context = new StringBuilder();
            
            if (insights != null && insights.Count > 0)
            {
                context.AppendLine("**Project Insights:**");
                foreach (ProjectInsight insight in insights.Take(MaxItemsPerCategory))
                {
                    context.AppendLine($"- {insight.Type}: {insight.Description}");
                    context.AppendLine($"  Severity: {insight.Severity}, Confidence: {insight.Confidence:P0}");
                }
                context.AppendLine();
            }

            if (recommendations != null && recommendations.Count > 0)
            {
                context.AppendLine("**Key Recommendations:**");
                foreach (ProjectRecommendation recommendation in recommendations.Take(MaxItemsPerCategory))
                {
                    context.AppendLine($"- {recommendation.Type}: {recommendation.Title}");
                    context.AppendLine($"  Priority: {recommendation.Priority}, Effort: {recommendation.Effort.EstimatedTime}");
                }
            }

            return context.ToString();
        }

        /// <summary>
        /// Async wrapper for building context from project data
        /// </summary>
        public static async Task<string> BuildContextAsync(ProjectData projectData)
        {
            return await Task.FromResult(BuildProjectContext(projectData));
        }

        /// <summary>
        /// Builds comprehensive context for project analysis
        /// </summary>
        public static async Task<string> BuildProjectAnalysisContextAsync(ProjectData projectData)
        {
            if (projectData == null)
                throw new ArgumentNullException(nameof(projectData));

            StringBuilder context = new StringBuilder();
            
            // Project Overview
            AppendProjectOverview(context, projectData);
            
            // Technical Context  
            AppendTechnicalContext(context, projectData);
            
            // Content Analysis
            AppendContentAnalysis(context, projectData);
            
            // Documentation Context
            AppendDocumentationContext(context, projectData);

            // Additional analysis-specific context
            context.AppendLine("**Analysis Focus:**");
            context.AppendLine("- Project structure and organization");
            context.AppendLine("- Code quality and architecture patterns");
            context.AppendLine("- Documentation completeness");
            context.AppendLine("- Best practice adherence");
            context.AppendLine();

            string fullContext = context.ToString();
            
            // Truncate if too long while preserving structure
            if (fullContext.Length > MaxContextLength)
            {
                fullContext = TruncateContextIntelligently(fullContext, MaxContextLength);
            }

            return await Task.FromResult(fullContext);
        }

        private static void AppendProjectOverview(StringBuilder context, ProjectData projectData)
        {
            context.AppendLine("**Project Overview:**");
            context.AppendLine($"- Name: {projectData.ProjectName ?? "Unnamed Project"}");
            context.AppendLine($"- Type: {projectData.ProjectType}");
            context.AppendLine($"- Unity Version: {projectData.TargetUnityVersion}");
            
            if (!string.IsNullOrEmpty(projectData.ProjectDescription))
            {
                context.AppendLine($"- Description: {TruncateText(projectData.ProjectDescription, 200)}");
            }
            
            context.AppendLine($"- Documentation Sections: {projectData.DocumentationSections.Count}");
            context.AppendLine();
        }

        private static void AppendTechnicalContext(StringBuilder context, ProjectData projectData)
        {
            context.AppendLine("**Technical Context:**");
            
            // Unity version info
            context.AppendLine($"- Unity Version: {projectData.TargetUnityVersion}");
            
            // Project metadata
            if (!string.IsNullOrEmpty(projectData.TeamName))
                context.AppendLine($"- Team: {projectData.TeamName}");
            if (!string.IsNullOrEmpty(projectData.ContactEmail))
                context.AppendLine($"- Contact: {projectData.ContactEmail}");
            if (!string.IsNullOrEmpty(projectData.ProjectVersion))
                context.AppendLine($"- Version: {projectData.ProjectVersion}");
            if (!string.IsNullOrEmpty(projectData.RepositoryUrl))
                context.AppendLine($"- Repository: {projectData.RepositoryUrl}");
            
            // AI settings
            if (projectData.UseAIAssistance)
            {
                context.AppendLine($"- AI Provider: {projectData.AIProvider}");
            }
            
            context.AppendLine();
        }

        private static void AppendContentAnalysis(StringBuilder context, ProjectData projectData)
        {
            context.AppendLine("**Content Analysis:**");
            
            // Folder structure insights
            if (projectData.FolderStructure != null && projectData.FolderStructure.Folders != null && projectData.FolderStructure.Folders.Count > 0)
            {
                List<string> topLevelFolders = projectData.FolderStructure.Folders
                    .Take(10)
                    .Select(f => f.Name)
                    .ToList();
                
                if (topLevelFolders.Count > 0)
                {
                    context.AppendLine($"- Folder Structure: {string.Join(", ", topLevelFolders)}");
                }
            }
            
            // File information
            if (projectData.FolderStructure != null && projectData.FolderStructure.Files != null && projectData.FolderStructure.Files.Count > 0)
            {
                var fileExtensions = projectData.FolderStructure.Files
                    .GroupBy(f => f.Extension)
                    .Take(8)
                    .Select(g => $"{g.Key} ({g.Count()})")
                    .ToList();
                    
                if (fileExtensions.Count > 0)
                {
                    context.AppendLine($"- File Types: {string.Join(", ", fileExtensions)}");
                }
            }
            
            context.AppendLine();
        }

        private static void AppendDocumentationContext(StringBuilder context, ProjectData projectData)
        {
            if (projectData.DocumentationSections == null || projectData.DocumentationSections.Count == 0)
                return;

            context.AppendLine("**Documentation Status:**");
            
            int enabledSections = projectData.DocumentationSections.Count(s => s.IsEnabled);
            int completedSections = projectData.DocumentationSections.Count(s => s.Status == DocumentationStatus.Completed);
            
            context.AppendLine($"- Enabled Sections: {enabledSections}/{projectData.DocumentationSections.Count}");
            context.AppendLine($"- Completed Sections: {completedSections}/{enabledSections}");
            
            // Document pending sections
            List<DocumentationSectionData> pendingSections = projectData.DocumentationSections
                .Where(s => s.IsEnabled && s.Status != DocumentationStatus.Completed)
                .Take(5)
                .ToList();
                
            if (pendingSections.Count > 0)
            {
                string pending = string.Join(", ", pendingSections.Select(s => s.SectionType.ToString()));
                context.AppendLine($"- Pending Sections: {pending}");
            }

            context.AppendLine();
        }

        private static void AppendArchitectureContext(StringBuilder context, ProjectAnalysisResult analysisResult)
        {
            context.AppendLine("**Architecture Analysis:**");
            
            if (analysisResult.Architecture != null)
            {
                ArchitectureAnalysisResult architecture = analysisResult.Architecture;
                
                if (architecture.Components != null && architecture.Components.Count > 0)
                {
                    context.AppendLine($"- Components: {architecture.Components.Count} identified");
                }
                
                if (architecture.Layers != null && architecture.Layers.Count > 0)
                {
                    string layers = string.Join(", ", architecture.Layers.Take(5).Select(l => l.Name ?? "Unknown"));
                    context.AppendLine($"- Architecture Layers: {layers}");
                }
                
                if (architecture.Connections != null && architecture.Connections.Count > 0)
                {
                    context.AppendLine($"- System Connections: {architecture.Connections.Count} identified");
                }
                
                if (architecture.DetectedPattern != null)
                {
                    context.AppendLine($"- Architecture Pattern: {architecture.DetectedPattern}");
                }
            }
            
            context.AppendLine();
        }

        private static void AppendScriptContext(StringBuilder context, ProjectAnalysisResult analysisResult)
        {
            context.AppendLine("**Script Analysis:**");
            
            if (analysisResult.Scripts != null)
            {
                ScriptAnalysisResult scripts = analysisResult.Scripts;
                
                context.AppendLine($"- Total Classes: {scripts.TotalClasses}");
                context.AppendLine($"- Total Interfaces: {scripts.TotalInterfaces}");
                context.AppendLine($"- Total Methods: {scripts.TotalMethods}");
                context.AppendLine($"- Lines of Code: {scripts.TotalLinesOfCode:N0}");
                
                if (scripts.DetectedPatterns != null && scripts.DetectedPatterns.Count > 0)
                {
                    string patterns = string.Join(", ", scripts.DetectedPatterns.Take(6).Select(p => p.Name));
                    context.AppendLine($"- Design Patterns: {patterns}");
                }
                
                if (scripts.Issues != null && scripts.Issues.Count > 0)
                {
                    context.AppendLine($"- Code Issues: {scripts.Issues.Count}");
                }
            }
            
            context.AppendLine();
        }

        private static void AppendAssetContext(StringBuilder context, ProjectAnalysisResult analysisResult)
        {
            context.AppendLine("**Asset Analysis:**");
            
            if (analysisResult.Assets != null)
            {
                AssetAnalysisResult assets = analysisResult.Assets;
                
                context.AppendLine($"- Total Assets: {assets.TotalAssets}");
                context.AppendLine($"- Total Size: {assets.TotalAssetSize / 1024 / 1024:F1} MB");
                
                if (assets.AssetsByType != null && assets.AssetsByType.Count > 0)
                {
                    List<string> topAssetTypes = assets.AssetsByType
                        .OrderByDescending(a => a.Value)
                        .Take(5)
                        .Select(a => $"{a.Key} ({a.Value})")
                        .ToList();
                    context.AppendLine($"- Asset Distribution: {string.Join(", ", topAssetTypes)}");
                }
                
                if (assets.Dependencies != null && assets.Dependencies.Count > 0)
                {
                    context.AppendLine($"- Asset Dependencies: {assets.Dependencies.Count} relationships");
                }
                
                if (assets.Issues != null && assets.Issues.Count > 0)
                {
                    context.AppendLine($"- Asset Issues: {assets.Issues.Count} identified");
                }
            }
            
            context.AppendLine();
        }

        private static void AppendPerformanceContext(StringBuilder context, ProjectAnalysisResult analysisResult)
        {
            context.AppendLine("**Performance Analysis:**");
            
            if (analysisResult.Performance != null)
            {
                if (analysisResult.Performance.Issues != null && analysisResult.Performance.Issues.Count > 0)
                {
                    var criticalIssues = analysisResult.Performance.Issues
                        .Where(i => i.Impact == PerformanceImpact.Critical || i.Impact == PerformanceImpact.High)
                        .Take(5)
                        .ToList();
                        
                    if (criticalIssues.Count > 0)
                    {
                        context.AppendLine($"- Critical Performance Issues: {criticalIssues.Count}");
                        foreach (var issue in criticalIssues.Take(3))
                        {
                            context.AppendLine($"  • {issue.Type}: {TruncateText(issue.Description, 80)}");
                        }
                    }
                    else
                    {
                        context.AppendLine($"- Performance Issues: {analysisResult.Performance.Issues.Count} identified");
                    }
                }
                else
                {
                    context.AppendLine("- No performance issues detected");
                }
            }
            
            context.AppendLine();
        }

        private static void AppendIssueContext(StringBuilder context, ProjectAnalysisResult analysisResult)
        {
            context.AppendLine("**Issue Analysis:**");
            
            if (analysisResult.Issues != null && analysisResult.Issues.Count > 0)
            {
                context.AppendLine($"- Total Issues: {analysisResult.Issues.Count} identified");
                
                // Group issues by type if possible
                var issueTypes = analysisResult.Issues
                    .GroupBy(i => i.GetType().Name)
                    .Select(g => $"{g.Key}: {g.Count()}")
                    .ToList();
                    
                if (issueTypes.Count > 0)
                {
                    context.AppendLine($"- Issue Types: {string.Join(", ", issueTypes.Take(5))}");
                }
            }
            else
            {
                context.AppendLine("- No issues detected");
            }
            
            context.AppendLine();
        }

        private static void AppendGeneralAnalysisContext(StringBuilder context, ProjectAnalysisResult analysisResult)
        {
            context.AppendLine("**Analysis Summary:**");
            context.AppendLine($"- Analysis Completed: {analysisResult.AnalyzedAt:yyyy-MM-dd HH:mm}");
            context.AppendLine($"- Processing Time: {analysisResult.AnalysisTime.TotalSeconds:F1} seconds");
            context.AppendLine($"- Success: {analysisResult.Success}");
            
            if (!string.IsNullOrEmpty(analysisResult.ErrorMessage))
            {
                context.AppendLine($"- Issues: {TruncateText(analysisResult.ErrorMessage, 100)}");
            }
            
            if (analysisResult.Metrics != null)
            {
                context.AppendLine($"- Total Files: {analysisResult.Metrics.TotalFiles:N0}");
                context.AppendLine($"- Total Size: {analysisResult.Metrics.FormattedSize}");
            }
            
            context.AppendLine();
        }


        private static void AppendSectionSpecificContext(StringBuilder context, DocumentationSectionData sectionData, ProjectData projectData)
        {
            context.AppendLine($"**Section Context: {sectionData.SectionType}**");
            context.AppendLine($"- Target Word Count: {sectionData.WordCountTarget}");
            context.AppendLine($"- Current Status: {sectionData.Status}");
            context.AppendLine($"- AI Mode: {sectionData.AIMode}");
            
            if (sectionData.RequiredElements != null && sectionData.RequiredElements.Count > 0)
            {
                context.AppendLine($"- Required Elements: {string.Join(", ", sectionData.RequiredElements.Take(8))}");
            }
            
            if (!string.IsNullOrEmpty(sectionData.CustomPrompt))
            {
                context.AppendLine($"- Custom Instructions: {TruncateText(sectionData.CustomPrompt, 150)}");
            }
            
            context.AppendLine();
        }

        private static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;
                
            return text.Substring(0, maxLength - 3) + "...";
        }

        private static string TruncateContextIntelligently(string context, int maxLength)
        {
            if (context.Length <= maxLength)
                return context;

            // Find section boundaries and preserve structure
            string[] sections = context.Split(new[] { "**" }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder truncated = new StringBuilder();
            
            foreach (string section in sections)
            {
                if (truncated.Length + section.Length + 4 <= maxLength) // +4 for "**" markers
                {
                    if (truncated.Length > 0)
                        truncated.Append("**");
                    truncated.Append(section);
                    if (truncated.Length > 0)
                        truncated.Append("**");
                }
                else
                {
                    break;
                }
            }
            
            if (truncated.Length < context.Length)
            {
                truncated.AppendLine();
                truncated.AppendLine("...[Context truncated for token efficiency]");
            }
            
            return truncated.ToString();
        }
    }

    /// <summary>
    /// Specifies type of analysis context to build for specialized prompts
    /// </summary>
    public enum AnalysisContextType
    {
        General,
        Architecture,
        Scripts,
        Assets,
        Performance,
        Issues,
        Insights,
        Recommendations
    }
}