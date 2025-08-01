using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityProjectArchitect.API;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services
{
    public abstract class BaseDocumentationGenerator
    {
        protected readonly ProjectAnalysisResult analysisResult;
        protected readonly DocumentationSectionType sectionType;

        protected BaseDocumentationGenerator(ProjectAnalysisResult analysisResult, DocumentationSectionType sectionType)
        {
            this.analysisResult = analysisResult ?? throw new ArgumentNullException(nameof(analysisResult));
            this.sectionType = sectionType;
        }

        public abstract Task<string> GenerateContentAsync();

        protected virtual string GetSectionHeader(string title, int level = 1)
        {
            var headerPrefix = new string('#', level);
            return $"{headerPrefix} {title}\n\n";
        }

        protected virtual string FormatList(IEnumerable<string> items, bool numbered = false)
        {
            if (items == null || !items.Any()) return "";

            var sb = new StringBuilder();
            var itemsList = items.ToList();

            for (int i = 0; i < itemsList.Count; i++)
            {
                var prefix = numbered ? $"{i + 1}. " : "- ";
                sb.AppendLine($"{prefix}{itemsList[i]}");
            }

            return sb.ToString() + "\n";
        }

        protected virtual string FormatTable(Dictionary<string, string> data, string keyHeader = "Property", string valueHeader = "Value")
        {
            if (data == null || !data.Any()) return "";

            var sb = new StringBuilder();
            sb.AppendLine($"| {keyHeader} | {valueHeader} |");
            sb.AppendLine($"|{new string('-', keyHeader.Length + 2)}|{new string('-', valueHeader.Length + 2)}|");

            foreach (var kvp in data)
            {
                sb.AppendLine($"| {kvp.Key} | {kvp.Value} |");
            }

            return sb.ToString() + "\n";
        }

        protected virtual string FormatCodeBlock(string code, string language = "csharp")
        {
            if (string.IsNullOrEmpty(code)) return "";

            return $"```{language}\n{code}\n```\n\n";
        }

        protected virtual string FormatInsightsList(List<ProjectInsight> insights, string title = "Key Insights")
        {
            if (insights == null || !insights.Any()) return "";

            var sb = new StringBuilder();
            sb.AppendLine(GetSectionHeader(title, 3));

            var groupedInsights = insights.GroupBy(i => i.Severity).OrderByDescending(g => g.Key);
            
            foreach (var group in groupedInsights)
            {
                var severityIcon = group.Key switch
                {
                    InsightSeverity.Critical => "🔴",
                    InsightSeverity.High => "🟠",
                    InsightSeverity.Medium => "🟡",
                    InsightSeverity.Low => "🔵",
                    _ => "ℹ️"
                };

                foreach (var insight in group.Take(5))
                {
                    sb.AppendLine($"{severityIcon} **{insight.Title}**: {insight.Description}");
                }
            }

            return sb.ToString() + "\n";
        }

        protected virtual string FormatRecommendationsList(List<ProjectRecommendation> recommendations, string title = "Recommendations")
        {
            if (recommendations == null || !recommendations.Any()) return "";

            var sb = new StringBuilder();
            sb.AppendLine(GetSectionHeader(title, 3));

            var highPriorityRecs = recommendations.Where(r => r.Priority == RecommendationPriority.High || r.Priority == RecommendationPriority.Critical)
                                                 .Take(5).ToList();

            foreach (var rec in highPriorityRecs)
            {
                var priorityIcon = rec.Priority switch
                {
                    RecommendationPriority.Critical => "🚨",
                    RecommendationPriority.High => "⚠️",
                    RecommendationPriority.Medium => "💡",
                    _ => "📝"
                };

                sb.AppendLine($"{priorityIcon} **{rec.Title}**");
                sb.AppendLine($"   - {rec.Description}");
                
                if (rec.ActionSteps.Any())
                {
                    sb.AppendLine($"   - Action: {rec.ActionSteps.First().Description}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        protected virtual string FormatMetrics(ProjectMetrics metrics)
        {
            if (metrics == null) return "";

            var metricsData = new Dictionary<string, string>
            {
                ["Total Files"] = metrics.TotalFiles.ToString("N0"),
                ["Total Size"] = metrics.FormattedSize,
                ["Script Files"] = metrics.ScriptFiles.ToString("N0"),
                ["Lines of Code"] = metrics.TotalLinesOfCode.ToString("N0"),
                ["Asset Files"] = metrics.AssetFiles.ToString("N0"),
                ["Scene Files"] = metrics.SceneFiles.ToString("N0")
            };

            if (metrics.CodeComplexity > 0)
            {
                metricsData["Average Complexity"] = metrics.CodeComplexity.ToString("F1");
            }

            if (metrics.Maintainability > 0)
            {
                metricsData["Maintainability Score"] = $"{metrics.Maintainability * 100:F0}%";
            }

            return FormatTable(metricsData, "Metric", "Value");
        }

        protected virtual string GetProjectTypeDescription(ProjectType projectType)
        {
            return projectType switch
            {
                ProjectType.Game2D => "2D Game Project - Focused on 2D gameplay mechanics and sprites",
                ProjectType.Game3D => "3D Game Project - Full 3D environment with complex models and lighting",
                ProjectType.VR => "Virtual Reality Project - Immersive VR experience with specialized interaction",
                ProjectType.AR => "Augmented Reality Project - AR application with real-world integration",
                ProjectType.Mobile => "Mobile Game Project - Optimized for mobile platforms and touch controls",
                ProjectType.Tool => "Unity Tool/Editor Extension - Development productivity tool",
                ProjectType.Template => "Project Template - Reusable project structure for specific use cases",
                _ => "General Unity Project - Flexible project structure for various applications"
            };
        }

        protected virtual string FormatArchitecturePattern(ArchitecturePattern pattern)
        {
            return pattern switch
            {
                ArchitecturePattern.MVC => "Model-View-Controller (MVC) - Clear separation of data, presentation, and control logic",
                ArchitecturePattern.MVP => "Model-View-Presenter (MVP) - Enhanced testability with presenter handling UI logic",
                ArchitecturePattern.MVVM => "Model-View-ViewModel (MVVM) - Data binding with reactive view models",
                ArchitecturePattern.Layered => "Layered Architecture - Organized in distinct functional layers",
                ArchitecturePattern.EventDriven => "Event-Driven Architecture - Loose coupling through event communication",
                ArchitecturePattern.ServiceOriented => "Service-Oriented Architecture - Modular services with defined interfaces",
                ArchitecturePattern.ComponentBased => "Component-Based Architecture - Composition over inheritance approach",
                ArchitecturePattern.EntityComponentSystem => "Entity-Component-System (ECS) - Data-oriented design pattern",
                _ => "No specific architecture pattern detected - Mixed or custom approach"
            };
        }

        protected virtual async Task<string> WrapInProgressIndicator(string content, string operation)
        {
            return await Task.FromResult($"<!-- Generated by {GetType().Name} -->\n{content}<!-- End {GetType().Name} -->\n");
        }

        protected virtual string AddTimestamp()
        {
            return $"*Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss} UTC*\n\n";
        }

        protected virtual string AddGenerationMetadata()
        {
            var sb = new StringBuilder();
            sb.AppendLine("---");
            sb.AppendLine("**Generation Metadata:**");
            sb.AppendLine($"- Generated by: {GetType().Name}");
            sb.AppendLine($"- Analysis Date: {analysisResult.AnalyzedAt:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"- Analysis Duration: {analysisResult.AnalysisTime.TotalSeconds:F1}s");
            sb.AppendLine($"- Project Path: {analysisResult.ProjectPath}");
            sb.AppendLine("---\n");
            
            return sb.ToString();
        }
    }
}