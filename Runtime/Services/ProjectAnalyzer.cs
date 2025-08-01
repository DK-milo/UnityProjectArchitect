using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityProjectArchitect.API;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services
{
    public class ProjectAnalyzer : IProjectAnalyzer
    {
        public event Action<ProjectAnalysisResult> OnAnalysisComplete;
        public event Action<string, float> OnAnalysisProgress;
        public event Action<string> OnError;

        private readonly IScriptAnalyzer scriptAnalyzer;
        private readonly IAssetAnalyzer assetAnalyzer;
        private readonly ProjectStructureAnalyzer structureAnalyzer;
        private readonly InsightGenerator insightGenerator;
        private readonly RecommendationEngine recommendationEngine;

        public ProjectAnalyzer()
        {
            scriptAnalyzer = new ScriptAnalyzer();
            assetAnalyzer = new AssetAnalyzer();
            structureAnalyzer = new ProjectStructureAnalyzer();
            insightGenerator = new InsightGenerator();
            recommendationEngine = new RecommendationEngine();
        }

        public ProjectAnalyzer(IScriptAnalyzer scriptAnalyzer, IAssetAnalyzer assetAnalyzer)
        {
            this.scriptAnalyzer = scriptAnalyzer ?? new ScriptAnalyzer();
            this.assetAnalyzer = assetAnalyzer ?? new AssetAnalyzer();
            structureAnalyzer = new ProjectStructureAnalyzer();
            insightGenerator = new InsightGenerator();
            recommendationEngine = new RecommendationEngine();
        }

        public async Task<ProjectAnalysisResult> AnalyzeProjectAsync(string projectPath)
        {
            var result = new ProjectAnalysisResult(projectPath);
            var startTime = DateTime.Now;

            try
            {
                if (!CanAnalyze(projectPath))
                {
                    result.Success = false;
                    result.ErrorMessage = $"Cannot analyze project at path: {projectPath}";
                    OnError?.Invoke(result.ErrorMessage);
                    return result;
                }

                OnAnalysisProgress?.Invoke("Starting project analysis...", 0f);

                OnAnalysisProgress?.Invoke("Analyzing project structure...", 0.1f);
                result.Structure = await AnalyzeProjectStructureAsync(projectPath);

                OnAnalysisProgress?.Invoke("Analyzing scripts...", 0.3f);
                var scriptsPath = Path.Combine(projectPath, "Assets", "Scripts");
                if (Directory.Exists(scriptsPath))
                {
                    result.Scripts = await AnalyzeScriptsAsync(scriptsPath);
                }
                else
                {
                    result.Scripts = await AnalyzeScriptsAsync(Path.Combine(projectPath, "Assets"));
                }

                OnAnalysisProgress?.Invoke("Analyzing assets...", 0.5f);
                result.Assets = await AnalyzeAssetsAsync(Path.Combine(projectPath, "Assets"));

                OnAnalysisProgress?.Invoke("Analyzing architecture...", 0.7f);
                result.Architecture = await AnalyzeArchitectureFromResultsAsync(result);

                OnAnalysisProgress?.Invoke("Analyzing performance...", 0.8f);
                result.Performance = await AnalyzePerformanceAsync(result);

                OnAnalysisProgress?.Invoke("Generating insights...", 0.9f);
                result.Insights = await GetInsightsAsync(result);

                OnAnalysisProgress?.Invoke("Generating recommendations...", 0.95f);
                result.Recommendations = await GetRecommendationsAsync(result);

                result.Metrics = CalculateProjectMetrics(result);
                result.Success = true;
                result.AnalysisTime = DateTime.Now - startTime;

                OnAnalysisProgress?.Invoke("Analysis complete!", 1f);
                OnAnalysisComplete?.Invoke(result);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.AnalysisTime = DateTime.Now - startTime;
                OnError?.Invoke($"Analysis failed: {ex.Message}");
            }

            return result;
        }

        public async Task<ProjectAnalysisResult> AnalyzeProjectDataAsync(ProjectData projectData)
        {
            if (projectData == null)
            {
                throw new ArgumentNullException(nameof(projectData));
            }

            var projectPath = Application.dataPath;
            var result = await AnalyzeProjectAsync(Path.GetDirectoryName(projectPath));
            
            result.Metrics.TechnicalDebt = CalculateTechnicalDebt(result);
            result.Metrics.Maintainability = CalculateMaintainability(result);

            return result;
        }

        public async Task<ScriptAnalysisResult> AnalyzeScriptsAsync(string scriptsPath)
        {
            if (scriptAnalyzer == null)
            {
                throw new InvalidOperationException("Script analyzer not initialized");
            }

            return await scriptAnalyzer.AnalyzeScriptAsync(scriptsPath);
        }

        public async Task<AssetAnalysisResult> AnalyzeAssetsAsync(string assetsPath)
        {
            if (assetAnalyzer == null)
            {
                throw new InvalidOperationException("Asset analyzer not initialized");
            }

            return await assetAnalyzer.AnalyzeAssetAsync(assetsPath);
        }

        public async Task<ArchitectureAnalysisResult> AnalyzeArchitectureAsync(ProjectData projectData)
        {
            var result = new ArchitectureAnalysisResult();
            
            var projectPath = Application.dataPath;
            var analysisResult = await AnalyzeProjectAsync(Path.GetDirectoryName(projectPath));
            
            return await AnalyzeArchitectureFromResultsAsync(analysisResult);
        }

        public async Task<List<ProjectInsight>> GetInsightsAsync(ProjectAnalysisResult analysisResult)
        {
            return await insightGenerator.GenerateInsightsAsync(analysisResult);
        }

        public async Task<List<ProjectRecommendation>> GetRecommendationsAsync(ProjectAnalysisResult analysisResult)
        {
            return await recommendationEngine.GenerateRecommendationsAsync(analysisResult);
        }

        public ProjectAnalyzerCapabilities GetCapabilities()
        {
            return new ProjectAnalyzerCapabilities
            {
                SupportedFileTypes = new List<string> { ".cs", ".unity", ".prefab", ".asset", ".mat", ".shader" },
                SupportedLanguages = new List<string> { "C#", "ShaderLab", "HLSL" },
                SupportsScriptAnalysis = true,
                SupportsAssetAnalysis = true,
                SupportsArchitectureAnalysis = true,
                SupportsPerformanceAnalysis = true,
                SupportsInsightGeneration = true,
                SupportsRecommendations = true,
                Features = new Dictionary<string, bool>
                {
                    ["DependencyAnalysis"] = true,
                    ["CodeMetrics"] = true,
                    ["DesignPatternDetection"] = true,
                    ["PerformanceAnalysis"] = true,
                    ["AssetOptimization"] = true,
                    ["ArchitectureValidation"] = true
                }
            };
        }

        public bool CanAnalyze(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath) || !Directory.Exists(projectPath))
                return false;

            var assetsPath = Path.Combine(projectPath, "Assets");
            var projectSettingsPath = Path.Combine(projectPath, "ProjectSettings");
            
            return Directory.Exists(assetsPath) && Directory.Exists(projectSettingsPath);
        }

        private async Task<ProjectStructureAnalysis> AnalyzeProjectStructureAsync(string projectPath)
        {
            return await structureAnalyzer.AnalyzeAsync(projectPath);
        }

        private async Task<ArchitectureAnalysisResult> AnalyzeArchitectureFromResultsAsync(ProjectAnalysisResult analysisResult)
        {
            return await Task.Run(() =>
            {
                var result = new ArchitectureAnalysisResult();

                if (analysisResult.Scripts?.Classes != null)
                {
                    foreach (var classDefinition in analysisResult.Scripts.Classes)
                    {
                        var component = new ComponentInfo(classDefinition.Name, "Class")
                        {
                            Category = DetermineComponentCategory(classDefinition),
                            Dependencies = classDefinition.BaseClasses.Concat(classDefinition.Interfaces).ToList()
                        };
                        result.Components.Add(component);
                    }

                    result.DetectedPattern = DetectArchitecturePattern(analysisResult.Scripts.Classes);
                    result.Connections = BuildSystemConnections(analysisResult.Scripts.Classes);
                    result.Layers = IdentifyArchitectureLayers(result.Components);
                    result.Issues = DetectArchitectureIssues(result);
                    result.Metrics = CalculateArchitectureMetrics(result);
                }

                return result;
            });
        }

        private async Task<PerformanceAnalysis> AnalyzePerformanceAsync(ProjectAnalysisResult analysisResult)
        {
            return await Task.Run(() =>
            {
                var performance = new PerformanceAnalysis();

                if (analysisResult.Assets != null)
                {
                    performance.Issues.AddRange(DetectPerformanceIssues(analysisResult.Assets));
                    performance.Metrics = CalculatePerformanceMetrics(analysisResult.Assets);
                    performance.Recommendations = GeneratePerformanceRecommendations(performance.Issues, performance.Metrics);
                }

                return performance;
            });
        }

        private ComponentCategory DetermineComponentCategory(ClassDefinition classDefinition)
        {
            if (classDefinition.IsMonoBehaviour)
                return ComponentCategory.Gameplay;
            
            if (classDefinition.Name.Contains("UI") || classDefinition.Name.Contains("Canvas"))
                return ComponentCategory.UI;
            
            if (classDefinition.Name.Contains("Manager") || classDefinition.Name.Contains("Service"))
                return ComponentCategory.Core;
            
            if (classDefinition.Name.Contains("Util") || classDefinition.Name.Contains("Helper"))
                return ComponentCategory.Utility;
            
            return ComponentCategory.Core;
        }

        private ArchitecturePattern DetectArchitecturePattern(List<ClassDefinition> classes)
        {
            var hasControllers = classes.Any(c => c.Name.Contains("Controller"));
            var hasViews = classes.Any(c => c.Name.Contains("View") || c.IsMonoBehaviour);
            var hasModels = classes.Any(c => c.Name.Contains("Model") || c.IsScriptableObject);

            if (hasControllers && hasViews && hasModels)
                return ArchitecturePattern.MVC;
            
            var hasManagers = classes.Any(c => c.Name.Contains("Manager"));
            var hasServices = classes.Any(c => c.Name.Contains("Service"));
            
            if (hasManagers && hasServices)
                return ArchitecturePattern.ServiceOriented;
            
            var hasComponents = classes.Count(c => c.IsMonoBehaviour) > classes.Count * 0.6f;
            if (hasComponents)
                return ArchitecturePattern.ComponentBased;

            return ArchitecturePattern.None;
        }

        private List<SystemConnection> BuildSystemConnections(List<ClassDefinition> classes)
        {
            var connections = new List<SystemConnection>();

            foreach (var classDefinition in classes)
            {
                foreach (var baseClass in classDefinition.BaseClasses)
                {
                    var targetClass = classes.FirstOrDefault(c => c.Name == baseClass);
                    if (targetClass != null)
                    {
                        connections.Add(new SystemConnection(classDefinition.Name, targetClass.Name, ConnectionType.Inheritance));
                    }
                }

                foreach (var interfaceName in classDefinition.Interfaces)
                {
                    var targetInterface = classes.FirstOrDefault(c => c.Name == interfaceName);
                    if (targetInterface != null)
                    {
                        connections.Add(new SystemConnection(classDefinition.Name, targetInterface.Name, ConnectionType.Inheritance));
                    }
                }
            }

            return connections;
        }

        private List<LayerInfo> IdentifyArchitectureLayers(List<ComponentInfo> components)
        {
            var layers = new List<LayerInfo>();

            var presentationComponents = components.Where(c => c.Category == ComponentCategory.UI).Select(c => c.Name).ToList();
            if (presentationComponents.Any())
            {
                layers.Add(new LayerInfo("Presentation", 1) { Components = presentationComponents });
            }

            var gameplayComponents = components.Where(c => c.Category == ComponentCategory.Gameplay).Select(c => c.Name).ToList();
            if (gameplayComponents.Any())
            {
                layers.Add(new LayerInfo("Gameplay", 2) { Components = gameplayComponents });
            }

            var coreComponents = components.Where(c => c.Category == ComponentCategory.Core).Select(c => c.Name).ToList();
            if (coreComponents.Any())
            {
                layers.Add(new LayerInfo("Core", 3) { Components = coreComponents });
            }

            var utilityComponents = components.Where(c => c.Category == ComponentCategory.Utility).Select(c => c.Name).ToList();
            if (utilityComponents.Any())
            {
                layers.Add(new LayerInfo("Utility", 4) { Components = utilityComponents });
            }

            return layers;
        }

        private List<ArchitectureIssue> DetectArchitectureIssues(ArchitectureAnalysisResult architecture)
        {
            var issues = new List<ArchitectureIssue>();

            var godClasses = architecture.Components.Where(c => 
                c.Properties.ContainsKey("MethodCount") && 
                (int)c.Properties["MethodCount"] > 20).ToList();
            
            foreach (var godClass in godClasses)
            {
                issues.Add(new ArchitectureIssue(ArchitectureIssueType.GodClass, 
                    $"Class {godClass.Name} has too many methods and responsibilities")
                {
                    Severity = ArchitectureIssueSeverity.Major,
                    AffectedComponents = { godClass.Name }
                });
            }

            return issues;
        }

        private ArchitectureMetrics CalculateArchitectureMetrics(ArchitectureAnalysisResult architecture)
        {
            return new ArchitectureMetrics
            {
                TotalComponents = architecture.Components.Count,
                TotalConnections = architecture.Connections.Count,
                AverageCoupling = architecture.Components.Count > 0 ? 
                    (float)architecture.Connections.Count / architecture.Components.Count : 0f,
                AverageCohesion = 0.8f,
                Instability = 0.3f,
                Abstractness = 0.5f
            };
        }

        private List<PerformanceIssue> DetectPerformanceIssues(AssetAnalysisResult assets)
        {
            var issues = new List<PerformanceIssue>();

            var largeTextures = assets.Assets.Where(a => 
                a.AssetType == "Texture2D" && a.SizeBytes > 4 * 1024 * 1024).ToList();
            
            foreach (var texture in largeTextures)
            {
                issues.Add(new PerformanceIssue(PerformanceIssueType.LargeTextureSize,
                    $"Texture {texture.Name} is very large ({FormatBytes(texture.SizeBytes)})")
                {
                    Location = texture.Path,
                    Impact = PerformanceImpact.Medium
                });
            }

            return issues;
        }

        private PerformanceMetrics CalculatePerformanceMetrics(AssetAnalysisResult assets)
        {
            var textureAssets = assets.Assets.Where(a => a.AssetType == "Texture2D").ToList();
            var meshAssets = assets.Assets.Where(a => a.AssetType == "Mesh").ToList();
            var audioAssets = assets.Assets.Where(a => a.AssetType == "AudioClip").ToList();

            return new PerformanceMetrics
            {
                TextureMemoryMB = textureAssets.Sum(a => a.SizeBytes) / (1024 * 1024),
                MeshMemoryMB = meshAssets.Sum(a => a.SizeBytes) / (1024 * 1024),
                AudioClips = audioAssets.Count,
                AudioMemoryMB = audioAssets.Sum(a => a.SizeBytes) / (1024 * 1024)
            };
        }

        private List<PerformanceRecommendation> GeneratePerformanceRecommendations(List<PerformanceIssue> issues, PerformanceMetrics metrics)
        {
            var recommendations = new List<PerformanceRecommendation>();

            if (metrics.TextureMemoryMB > 100)
            {
                recommendations.Add(new PerformanceRecommendation(
                    "Optimize Texture Compression",
                    "Consider using compressed texture formats to reduce memory usage")
                {
                    ExpectedImpact = PerformanceImpact.High,
                    ImplementationEffort = 3
                });
            }

            return recommendations;
        }

        private ProjectMetrics CalculateProjectMetrics(ProjectAnalysisResult result)
        {
            var metrics = new ProjectMetrics();

            if (result.Structure != null)
            {
                metrics.TotalFiles = result.Structure.TotalFiles;
                metrics.TotalFolders = result.Structure.Folders.Count;
                metrics.TotalSizeBytes = result.Structure.TotalSizeBytes;
                metrics.SceneFiles = result.Structure.Scenes.Count;
            }

            if (result.Scripts != null)
            {
                metrics.ScriptFiles = result.Scripts.Classes.Count;
                metrics.TotalLinesOfCode = result.Scripts.TotalLinesOfCode;
                metrics.CodeComplexity = result.Scripts.Metrics?.AverageCyclomaticComplexity ?? 0f;
            }

            if (result.Assets != null)
            {
                metrics.AssetFiles = result.Assets.TotalAssets;
            }

            return metrics;
        }

        private float CalculateTechnicalDebt(ProjectAnalysisResult result)
        {
            float debt = 0f;
            int factors = 0;

            if (result.Scripts?.Issues != null)
            {
                debt += result.Scripts.Issues.Count(i => i.Severity == CodeIssueSeverity.Major) * 0.3f;
                debt += result.Scripts.Issues.Count(i => i.Severity == CodeIssueSeverity.Critical) * 0.5f;
                factors++;
            }

            if (result.Architecture?.Issues != null)
            {
                debt += result.Architecture.Issues.Count(i => i.Severity == ArchitectureIssueSeverity.Major) * 0.4f;
                debt += result.Architecture.Issues.Count(i => i.Severity == ArchitectureIssueSeverity.Critical) * 0.6f;
                factors++;
            }

            return factors > 0 ? Math.Min(debt / factors, 1f) : 0f;
        }

        private float CalculateMaintainability(ProjectAnalysisResult result)
        {
            float maintainability = 1f;

            if (result.Scripts?.Metrics != null)
            {
                var complexity = result.Scripts.Metrics.AverageCyclomaticComplexity;
                maintainability -= Math.Min(complexity / 20f, 0.4f);
            }

            if (result.Metrics != null)
            {
                maintainability -= Math.Min(result.Metrics.TechnicalDebt, 0.3f);
            }

            return Math.Max(maintainability, 0f);
        }

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
}