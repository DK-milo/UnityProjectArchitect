using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.API
{
    public interface IProjectAnalyzer
    {
        event Action<ProjectAnalysisResult> OnAnalysisComplete;
        event Action<string, float> OnAnalysisProgress;
        event Action<string> OnError;

        Task<ProjectAnalysisResult> AnalyzeProjectAsync(string projectPath);
        Task<ProjectAnalysisResult> AnalyzeProjectDataAsync(ProjectData projectData);
        Task<ScriptAnalysisResult> AnalyzeScriptsAsync(string scriptsPath);
        Task<AssetAnalysisResult> AnalyzeAssetsAsync(string assetsPath);
        Task<ArchitectureAnalysisResult> AnalyzeArchitectureAsync(ProjectData projectData);
        
        Task<List<ProjectInsight>> GetInsightsAsync(ProjectAnalysisResult analysisResult);
        Task<List<ProjectRecommendation>> GetRecommendationsAsync(ProjectAnalysisResult analysisResult);
        
        ProjectAnalyzerCapabilities GetCapabilities();
        bool CanAnalyze(string projectPath);
    }

    public interface IScriptAnalyzer
    {
        Task<ScriptAnalysisResult> AnalyzeScriptAsync(string scriptPath);
        Task<List<ClassDefinition>> ExtractClassDefinitionsAsync(string scriptPath);
        Task<List<MethodDefinition>> ExtractMethodDefinitionsAsync(string scriptPath);
        Task<DependencyGraph> BuildDependencyGraphAsync(string scriptsPath);
        
        List<string> GetSupportedLanguages();
        bool CanAnalyzeScript(string scriptPath);
    }

    public interface IAssetAnalyzer
    {
        Task<AssetAnalysisResult> AnalyzeAssetAsync(string assetPath);
        Task<List<AssetDependency>> GetAssetDependenciesAsync(string assetPath);
        Task<AssetUsageReport> GetAssetUsageReportAsync(string assetsPath);
        
        List<string> GetSupportedAssetTypes();
        bool CanAnalyzeAsset(string assetPath);
    }

    [Serializable]
    public class ProjectAnalysisResult
    {
        public bool Success { get; set; }
        public string ProjectPath { get; set; }
        public ProjectStructureAnalysis Structure { get; set; }
        public ScriptAnalysisResult Scripts { get; set; }
        public AssetAnalysisResult Assets { get; set; }
        public ArchitectureAnalysisResult Architecture { get; set; }
        public PerformanceAnalysis Performance { get; set; }
        public List<ProjectInsight> Insights { get; set; }
        public List<ProjectRecommendation> Recommendations { get; set; }
        public TimeSpan AnalysisTime { get; set; }
        public DateTime AnalyzedAt { get; set; }
        public string ErrorMessage { get; set; }
        public ProjectMetrics Metrics { get; set; }

        public ProjectAnalysisResult()
        {
            Insights = new List<ProjectInsight>();
            Recommendations = new List<ProjectRecommendation>();
            AnalyzedAt = DateTime.Now;
            Metrics = new ProjectMetrics();
        }

        public ProjectAnalysisResult(string projectPath) : this()
        {
            ProjectPath = projectPath;
        }
    }

    [Serializable]
    public class ProjectStructureAnalysis
    {
        public List<FolderInfo> Folders { get; set; }
        public List<FileInfo> Files { get; set; }
        public List<AssemblyDefinitionInfo> AssemblyDefinitions { get; set; }
        public List<SceneInfo> Scenes { get; set; }
        public ProjectType DetectedProjectType { get; set; }
        public UnityVersion DetectedUnityVersion { get; set; }
        public bool FollowsStandardStructure { get; set; }
        public List<StructureIssue> Issues { get; set; }

        public ProjectStructureAnalysis()
        {
            Folders = new List<FolderInfo>();
            Files = new List<FileInfo>();
            AssemblyDefinitions = new List<AssemblyDefinitionInfo>();
            Scenes = new List<SceneInfo>();
            Issues = new List<StructureIssue>();
        }

        public int TotalFiles => Files.Count;
        public long TotalSizeBytes => Files.Sum(f => f.SizeBytes);
        public string FormattedTotalSize => FormatBytes(TotalSizeBytes);

        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }

    [Serializable]
    public class ScriptAnalysisResult
    {
        public List<ClassDefinition> Classes { get; set; }
        public List<InterfaceDefinition> Interfaces { get; set; }
        public List<MethodDefinition> Methods { get; set; }
        public DependencyGraph Dependencies { get; set; }
        public List<CodeIssue> Issues { get; set; }
        public CodeMetrics Metrics { get; set; }
        public List<DesignPattern> DetectedPatterns { get; set; }

        public ScriptAnalysisResult()
        {
            Classes = new List<ClassDefinition>();
            Interfaces = new List<InterfaceDefinition>();
            Methods = new List<MethodDefinition>();
            Issues = new List<CodeIssue>();
            DetectedPatterns = new List<DesignPattern>();
            Metrics = new CodeMetrics();
        }

        public int TotalClasses => Classes.Count;
        public int TotalInterfaces => Interfaces.Count;
        public int TotalMethods => Methods.Count;
        public int TotalLinesOfCode => Classes.Sum(c => c.LinesOfCode) + Interfaces.Sum(i => i.LinesOfCode);
    }

    [Serializable]
    public class AssetAnalysisResult
    {
        public List<AssetInfo> Assets { get; set; }
        public List<AssetDependency> Dependencies { get; set; }
        public AssetUsageReport UsageReport { get; set; }
        public List<AssetIssue> Issues { get; set; }
        public AssetMetrics Metrics { get; set; }

        public AssetAnalysisResult()
        {
            Assets = new List<AssetInfo>();
            Dependencies = new List<AssetDependency>();
            Issues = new List<AssetIssue>();
            Metrics = new AssetMetrics();
        }

        public int TotalAssets => Assets.Count;
        public long TotalAssetSize => Assets.Sum(a => a.SizeBytes);
        public Dictionary<string, int> AssetsByType => Assets.GroupBy(a => a.AssetType)
                                                            .ToDictionary(g => g.Key, g => g.Count());
    }

    [Serializable]
    public class ArchitectureAnalysisResult
    {
        public List<ComponentInfo> Components { get; set; }
        public List<SystemConnection> Connections { get; set; }
        public List<LayerInfo> Layers { get; set; }
        public ArchitecturePattern DetectedPattern { get; set; }
        public List<ArchitectureIssue> Issues { get; set; }
        public ArchitectureMetrics Metrics { get; set; }

        public ArchitectureAnalysisResult()
        {
            Components = new List<ComponentInfo>();
            Connections = new List<SystemConnection>();
            Layers = new List<LayerInfo>();
            Issues = new List<ArchitectureIssue>();
            Metrics = new ArchitectureMetrics();
        }
    }

    [Serializable]
    public class ProjectInsight
    {
        public InsightType Type { get; set; }
        public InsightSeverity Severity { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Context { get; set; }
        public float Confidence { get; set; }
        public List<string> Evidence { get; set; }
        public Dictionary<string, object> Data { get; set; }

        public ProjectInsight()
        {
            Evidence = new List<string>();
            Data = new Dictionary<string, object>();
        }

        public ProjectInsight(InsightType type, string title, string description) : this()
        {
            Type = type;
            Title = title;
            Description = description;
        }
    }

    [Serializable]
    public class ProjectRecommendation
    {
        public RecommendationType Type { get; set; }
        public RecommendationPriority Priority { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Rationale { get; set; }
        public List<ActionStep> ActionSteps { get; set; }
        public EstimatedEffort Effort { get; set; }
        public List<string> Benefits { get; set; }
        public List<string> Risks { get; set; }

        public ProjectRecommendation()
        {
            ActionSteps = new List<ActionStep>();
            Benefits = new List<string>();
            Risks = new List<string>();
            Effort = new EstimatedEffort();
        }

        public ProjectRecommendation(RecommendationType type, string title) : this()
        {
            Type = type;
            Title = title;
        }
    }

    public enum InsightType
    {
        ProjectStructure,
        CodeQuality,
        Performance,
        Architecture,
        Dependencies,
        Security,
        Maintainability,
        Testing
    }

    public enum InsightSeverity
    {
        Info,
        Low,
        Medium,
        High,
        Critical
    }

    public enum RecommendationType
    {
        Structure,
        Performance,
        Architecture,
        CodeQuality,
        Dependencies,
        Security,
        Documentation,
        Testing
    }

    public enum RecommendationPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    [Serializable]
    public class ActionStep
    {
        public string Description { get; set; }
        public TimeSpan EstimatedTime { get; set; }
        public string[] Prerequisites { get; set; }
        public bool IsOptional { get; set; }

        public ActionStep()
        {
            Prerequisites = new string[0];
        }

        public ActionStep(string description, TimeSpan time = default) : this()
        {
            Description = description;
            EstimatedTime = time;
        }
    }

    [Serializable]
    public class EstimatedEffort
    {
        public TimeSpan MinTime { get; set; }
        public TimeSpan MaxTime { get; set; }
        public TimeSpan MostLikelyTime { get; set; }
        public int Complexity { get; set; }
        public string[] RequiredSkills { get; set; }

        public EstimatedEffort()
        {
            RequiredSkills = new string[0];
            Complexity = 1;
        }

        public TimeSpan EstimatedTime => 
            TimeSpan.FromTicks((MinTime.Ticks + 4 * MostLikelyTime.Ticks + MaxTime.Ticks) / 6);
    }

    [Serializable]
    public class ProjectAnalyzerCapabilities
    {
        public List<string> SupportedFileTypes { get; set; }
        public List<string> SupportedLanguages { get; set; }
        public bool SupportsScriptAnalysis { get; set; }
        public bool SupportsAssetAnalysis { get; set; }
        public bool SupportsArchitectureAnalysis { get; set; }
        public bool SupportsPerformanceAnalysis { get; set; }
        public bool SupportsInsightGeneration { get; set; }
        public bool SupportsRecommendations { get; set; }
        public Dictionary<string, bool> Features { get; set; }

        public ProjectAnalyzerCapabilities()
        {
            SupportedFileTypes = new List<string>();
            SupportedLanguages = new List<string>();
            Features = new Dictionary<string, bool>();
        }

        public bool HasFeature(string featureName)
        {
            return Features.ContainsKey(featureName) && Features[featureName];
        }
    }

    [Serializable]
    public class ProjectMetrics
    {
        public int TotalFiles { get; set; }
        public int TotalFolders { get; set; }
        public long TotalSizeBytes { get; set; }
        public int ScriptFiles { get; set; }
        public int AssetFiles { get; set; }
        public int SceneFiles { get; set; }
        public int TotalLinesOfCode { get; set; }
        public float CodeComplexity { get; set; }
        public float TechnicalDebt { get; set; }
        public float Maintainability { get; set; }

        public string FormattedSize => FormatBytes(TotalSizeBytes);

        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }

    public static class ProjectAnalyzerExtensions
    {
        public static List<ProjectInsight> GetInsightsByType(
            this ProjectAnalysisResult result, 
            InsightType insightType)
        {
            return result.Insights.Where(i => i.Type == insightType).ToList();
        }

        public static List<ProjectInsight> GetInsightsBySeverity(
            this ProjectAnalysisResult result, 
            InsightSeverity severity)
        {
            return result.Insights.Where(i => i.Severity == severity).ToList();
        }

        public static List<ProjectRecommendation> GetRecommendationsByPriority(
            this ProjectAnalysisResult result, 
            RecommendationPriority priority)
        {
            return result.Recommendations.Where(r => r.Priority == priority).ToList();
        }

        public static string GetFormattedReport(this ProjectAnalysisResult result)
        {
            string report = $"📊 Project Analysis Report\n";
            report += $"📂 Project: {System.IO.Path.GetFileName(result.ProjectPath)}\n";
            report += $"⏱️ Analysis Time: {result.AnalysisTime.TotalSeconds:F1}s\n";
            report += $"📁 Total Files: {result.Metrics.TotalFiles:N0}\n";
            report += $"💾 Total Size: {result.Metrics.FormattedSize}\n";
            report += $"📝 Lines of Code: {result.Metrics.TotalLinesOfCode:N0}\n";

            if (result.Insights.Count > 0)
            {
                report += $"\n🔍 Insights ({result.Insights.Count}):\n";
                foreach (string insight in result.Insights.Take(5))
                {
                    string severityIcon = insight.Severity switch
                    {
                        InsightSeverity.Critical => "🔴",
                        InsightSeverity.High => "🟠",
                        InsightSeverity.Medium => "🟡",
                        InsightSeverity.Low => "🔵",
                        _ => "ℹ️"
                    };
                    report += $"  {severityIcon} {insight.Title}\n";
                }
            }

            if (result.Recommendations.Count > 0)
            {
                List<ProjectRecommendation> highPriorityRecs = result.GetRecommendationsByPriority(RecommendationPriority.High);
                if (highPriorityRecs.Count > 0)
                {
                    report += $"\n💡 High Priority Recommendations:\n";
                    foreach (string rec in highPriorityRecs.Take(3))
                    {
                        report += $"  • {rec.Title}\n";
                    }
                }
            }

            return report;
        }
    }
}