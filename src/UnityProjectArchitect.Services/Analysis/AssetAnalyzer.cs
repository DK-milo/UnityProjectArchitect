using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using UnityProjectArchitect.AI;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services
{
    public class AssetAnalyzer : IAssetAnalyzer
    {
        private readonly List<string> supportedAssetTypes = new List<string>
        {
            "Texture2D", "Material", "Mesh", "AudioClip", "AnimationClip", "Shader", "Font", 
            "Prefab", "Scene", "ScriptableObject", "Sprite", "Video", "Cubemap", "RenderTexture"
        };

        private readonly Dictionary<string, string> extensionToAssetType = new Dictionary<string, string>
        {
            { ".png", "Texture2D" }, { ".jpg", "Texture2D" }, { ".jpeg", "Texture2D" }, { ".tga", "Texture2D" }, { ".psd", "Texture2D" },
            { ".mat", "Material" },
            { ".fbx", "Mesh" }, { ".obj", "Mesh" }, { ".dae", "Mesh" }, { ".3ds", "Mesh" },
            { ".wav", "AudioClip" }, { ".mp3", "AudioClip" }, { ".ogg", "AudioClip" }, { ".aiff", "AudioClip" },
            { ".anim", "AnimationClip" },
            { ".shader", "Shader" }, { ".compute", "ComputeShader" },
            { ".ttf", "Font" }, { ".otf", "Font" },
            { ".prefab", "Prefab" },
            { ".unity", "Scene" },
            { ".asset", "ScriptableObject" },
            { ".mp4", "Video" }, { ".mov", "Video" }, { ".avi", "Video" }
        };

        public async Task<AssetAnalysisResult> AnalyzeAssetAsync(string assetPath)
        {
            AssetAnalysisResult result = new AssetAnalysisResult();

            if (File.Exists(assetPath))
            {
                AssetInfo assetInfo = await AnalyzeSingleAssetAsync(assetPath);
                result.Assets.Add(assetInfo);
            }
            else if (Directory.Exists(assetPath))
            {
                await AnalyzeDirectoryAsync(assetPath, result);
            }
            else
            {
                throw new ArgumentException($"Asset path does not exist: {assetPath}");
            }

            result.Dependencies = await GetAssetDependenciesAsync(assetPath);
            result.UsageReport = await GetAssetUsageReportAsync(assetPath);
            result.Issues = DetectAssetIssues(result);
            result.Metrics = CalculateAssetMetrics(result);

            return result;
        }

        public async Task<List<AssetDependency>> GetAssetDependenciesAsync(string assetPath)
        {
            List<AssetDependency> dependencies = new List<AssetDependency>();

            if (File.Exists(assetPath))
            {
                dependencies.AddRange(await AnalyzeAssetDependencies(assetPath));
            }
            else if (Directory.Exists(assetPath))
            {
                string[] assetFiles = GetAllAssetFiles(assetPath);
                
                foreach (string file in assetFiles)
                {
                    List<AssetDependency> fileDependencies = await AnalyzeAssetDependencies(file);
                    dependencies.AddRange(fileDependencies);
                }
            }

            return dependencies.Distinct().ToList();
        }

        public async Task<AssetUsageReport> GetAssetUsageReportAsync(string assetsPath)
        {
            AssetUsageReport report = new AssetUsageReport();
            string[] allAssets = GetAllAssetFiles(assetsPath);

            Dictionary<string, AssetUsageInfo> assetDependencies = new Dictionary<string, AssetUsageInfo>();

            foreach (string asset in allAssets)
            {
                assetDependencies[asset] = new AssetUsageInfo(asset);
            }

            foreach (string asset in allAssets)
            {
                List<AssetDependency> dependencies = await AnalyzeAssetDependencies(asset);
                
                foreach (AssetDependency dependency in dependencies)
                {
                    if (assetDependencies.ContainsKey(dependency.DependencyPath))
                    {
                        assetDependencies[dependency.DependencyPath].UsageCount++;
                        assetDependencies[dependency.DependencyPath].UsedByAssets.Add(asset);
                    }
                }
            }

            report.UsageInfos = assetDependencies.Values.ToList();
            report.UnusedAssets = assetDependencies.Where(kvp => kvp.Value.UsageCount == 0).Select(kvp => kvp.Key).ToList();
            
            List<string> sceneFiles = allAssets.Where(a => Path.GetExtension(a) == ".unity").ToList();
            foreach (string scene in sceneFiles)
            {
                List<string> sceneDependencies = await AnalyzeSceneDependencies(scene);
                foreach (string dependency in sceneDependencies)
                {
                    if (assetDependencies.ContainsKey(dependency))
                    {
                        assetDependencies[dependency].UsedByScenes.Add(scene);
                    }
                }
            }

            report.UsageByType = report.UsageInfos
                .GroupBy(ui => GetAssetTypeFromPath(ui.AssetPath))
                .ToDictionary(g => g.Key, g => g.Count());

            return report;
        }

        public List<string> GetSupportedAssetTypes()
        {
            return new List<string>(supportedAssetTypes);
        }

        public bool CanAnalyzeAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return false;

            if (File.Exists(assetPath))
            {
                string extension = Path.GetExtension(assetPath).ToLower();
                return extensionToAssetType.ContainsKey(extension);
            }

            return Directory.Exists(assetPath);
        }

        private async Task AnalyzeDirectoryAsync(string directoryPath, AssetAnalysisResult result)
        {
            string[] assetFiles = GetAllAssetFiles(directoryPath);

            foreach (string file in assetFiles)
            {
                try
                {
                    AssetInfo assetInfo = await AnalyzeSingleAssetAsync(file);
                    result.Assets.Add(assetInfo);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to analyze asset {file}: {ex.Message}");
                }
            }
        }

        private async Task<AssetInfo> AnalyzeSingleAssetAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                FileInfo fileInfo = new FileInfo(filePath);
                AssetInfo assetInfo = new AssetInfo(filePath)
                {
                    AssetType = GetAssetTypeFromPath(filePath),
                    SizeBytes = fileInfo.Length,
                    ModifiedDate = fileInfo.LastWriteTime,
                    ImportedDate = fileInfo.CreationTime
                };

                assetInfo.Metadata = ExtractAssetMetadata(filePath, assetInfo.AssetType);
                assetInfo.Labels = ExtractAssetLabels(filePath);

                return assetInfo;
            });
        }

        private string[] GetAllAssetFiles(string directoryPath)
        {
            List<string> assetFiles = new List<string>();
            string[] supportedExtensions = extensionToAssetType.Keys.ToArray();

            foreach (string extension in supportedExtensions)
            {
                string[] files = Directory.GetFiles(directoryPath, $"*{extension}", SearchOption.AllDirectories);
                assetFiles.AddRange(files);
            }

            return assetFiles.ToArray();
        }

        private string GetAssetTypeFromPath(string assetPath)
        {
            string extension = Path.GetExtension(assetPath).ToLower();
            return extensionToAssetType.ContainsKey(extension) ? extensionToAssetType[extension] : "Unknown";
        }

        private async Task<List<AssetDependency>> AnalyzeAssetDependencies(string assetPath)
        {
            return await Task.Run(() =>
            {
                List<AssetDependency> dependencies = new List<AssetDependency>();
                string assetType = GetAssetTypeFromPath(assetPath);

                switch (assetType)
                {
                    case "Material":
                        dependencies.AddRange(AnalyzeMaterialDependencies(assetPath));
                        break;
                    case "Prefab":
                        dependencies.AddRange(AnalyzePrefabDependencies(assetPath));
                        break;
                    case "Scene":
                        dependencies.AddRange(AnalyzeSceneDependenciesFromFile(assetPath));
                        break;
                    case "ScriptableObject":
                        dependencies.AddRange(AnalyzeScriptableObjectDependencies(assetPath));
                        break;
                }

                return dependencies;
            });
        }

        private List<AssetDependency> AnalyzeMaterialDependencies(string materialPath)
        {
            List<AssetDependency> dependencies = new List<AssetDependency>();

            try
            {
                string content = File.ReadAllText(materialPath);
                
                List<string> textureReferences = ExtractYamlReferences(content, "Texture2D");
                foreach (string reference in textureReferences)
                {
                    dependencies.Add(new AssetDependency(materialPath, reference, AssetDependencyType.Direct)
                    {
                        IsDirectDependency = true
                    });
                }

                List<string> shaderReferences = ExtractYamlReferences(content, "Shader");
                foreach (string reference in shaderReferences)
                {
                    dependencies.Add(new AssetDependency(materialPath, reference, AssetDependencyType.Direct)
                    {
                        IsDirectDependency = true
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to analyze material dependencies for {materialPath}: {ex.Message}");
            }

            return dependencies;
        }

        private List<AssetDependency> AnalyzePrefabDependencies(string prefabPath)
        {
            List<AssetDependency> dependencies = new List<AssetDependency>();

            try
            {
                string content = File.ReadAllText(prefabPath);
                
                List<string> meshReferences = ExtractYamlReferences(content, "Mesh");
                foreach (string reference in meshReferences)
                {
                    dependencies.Add(new AssetDependency(prefabPath, reference, AssetDependencyType.Direct)
                    {
                        IsDirectDependency = true
                    });
                }

                List<string> materialReferences = ExtractYamlReferences(content, "Material");
                foreach (string reference in materialReferences)
                {
                    dependencies.Add(new AssetDependency(prefabPath, reference, AssetDependencyType.Direct)
                    {
                        IsDirectDependency = true
                    });
                }

                List<string> scriptReferences = ExtractYamlReferences(content, "MonoScript");
                foreach (string reference in scriptReferences)
                {
                    dependencies.Add(new AssetDependency(prefabPath, reference, AssetDependencyType.Direct)
                    {
                        IsDirectDependency = true
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to analyze prefab dependencies for {prefabPath}: {ex.Message}");
            }

            return dependencies;
        }

        private List<AssetDependency> AnalyzeSceneDependenciesFromFile(string scenePath)
        {
            List<string> dependencies = new List<AssetDependency>();

            try
            {
                string content = File.ReadAllText(scenePath);
                
                List<string> prefabReferences = ExtractYamlReferences(content, "Prefab");
                foreach (string reference in prefabReferences)
                {
                    dependencies.Add(new AssetDependency(scenePath, reference, AssetDependencyType.Direct)
                    {
                        IsDirectDependency = true
                    });
                }

                List<string> materialReferences = ExtractYamlReferences(content, "Material");
                foreach (string reference in materialReferences)
                {
                    dependencies.Add(new AssetDependency(scenePath, reference, AssetDependencyType.Direct)
                    {
                        IsDirectDependency = true
                    });
                }

                List<string> textureReferences = ExtractYamlReferences(content, "Texture2D");
                foreach (string reference in textureReferences)
                {
                    dependencies.Add(new AssetDependency(scenePath, reference, AssetDependencyType.Direct)
                    {
                        IsDirectDependency = true
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to analyze scene dependencies for {scenePath}: {ex.Message}");
            }

            return dependencies;
        }

        private List<AssetDependency> AnalyzeScriptableObjectDependencies(string assetPath)
        {
            List<string> dependencies = new List<AssetDependency>();

            try
            {
                string content = File.ReadAllText(assetPath);
                
                List<string> scriptReferences = ExtractYamlReferences(content, "MonoScript");
                foreach (string reference in scriptReferences)
                {
                    dependencies.Add(new AssetDependency(assetPath, reference, AssetDependencyType.Direct)
                    {
                        IsDirectDependency = true
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to analyze ScriptableObject dependencies for {assetPath}: {ex.Message}");
            }

            return dependencies;
        }

        private async Task<List<string>> AnalyzeSceneDependencies(string scenePath)
        {
            return await Task.Run(() =>
            {
                List<string> dependencies = new List<string>();

                try
                {
                    string content = File.ReadAllText(scenePath);
                    
                    dependencies.AddRange(ExtractYamlReferences(content, "Prefab"));
                    dependencies.AddRange(ExtractYamlReferences(content, "Material"));
                    dependencies.AddRange(ExtractYamlReferences(content, "Texture2D"));
                    dependencies.AddRange(ExtractYamlReferences(content, "Mesh"));
                    dependencies.AddRange(ExtractYamlReferences(content, "AudioClip"));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to analyze scene dependencies for {scenePath}: {ex.Message}");
                }

                return dependencies.Distinct().ToList();
            });
        }

        private List<string> ExtractYamlReferences(string yamlContent, string assetType)
        {
            List<string> references = new List<string>();
            
            string[] lines = yamlContent.Split('\n');
            
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                
                if (line.Contains("fileID:") && line.Contains("guid:"))
                {
                    int guidStart = line.IndexOf("guid: ") + 6;
                    if (guidStart > 5)
                    {
                        int guidEnd = line.IndexOf(',', guidStart);
                        if (guidEnd == -1) guidEnd = line.IndexOf('}', guidStart);
                        
                        if (guidEnd > guidStart)
                        {
                            string guid = line.Substring(guidStart, guidEnd - guidStart).Trim();
                            if (!string.IsNullOrEmpty(guid) && guid != "0")
                            {
                                references.Add($"guid:{guid}");
                            }
                        }
                    }
                }
            }

            return references.Distinct().ToList();
        }

        private Dictionary<string, object> ExtractAssetMetadata(string assetPath, string assetType)
        {
            Dictionary<string, object> metadata = new Dictionary<string, object>();

            switch (assetType)
            {
                case "Texture2D":
                    metadata = ExtractTextureMetadata(assetPath);
                    break;
                case "AudioClip":
                    metadata = ExtractAudioMetadata(assetPath);
                    break;
                case "Mesh":
                    metadata = ExtractMeshMetadata(assetPath);
                    break;
            }

            return metadata;
        }

        private Dictionary<string, object> ExtractTextureMetadata(string texturePath)
        {
            Dictionary<string, object> metadata = new Dictionary<string, object>();

            try
            {
                string metaPath = texturePath + ".meta";
                if (File.Exists(metaPath))
                {
                    string metaContent = File.ReadAllText(metaPath);
                    
                    if (metaContent.Contains("maxTextureSize:"))
                    {
                        System.Text.RegularExpressions.Match sizeMatch = System.Text.RegularExpressions.Regex.Match(metaContent, @"maxTextureSize:\s*(\d+)");
                        if (sizeMatch.Success)
                        {
                            metadata["MaxTextureSize"] = int.Parse(sizeMatch.Groups[1].Value);
                        }
                    }

                    if (metaContent.Contains("textureFormat:"))
                    {
                        System.Text.RegularExpressions.Match formatMatch = System.Text.RegularExpressions.Regex.Match(metaContent, @"textureFormat:\s*(-?\d+)");
                        if (formatMatch.Success)
                        {
                            metadata["TextureFormat"] = int.Parse(formatMatch.Groups[1].Value);
                        }
                    }

                    metadata["HasMipMaps"] = metaContent.Contains("generateMipMaps: 1");
                    metadata["IsReadable"] = metaContent.Contains("isReadable: 1");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to extract texture metadata for {texturePath}: {ex.Message}");
            }

            return metadata;
        }

        private Dictionary<string, object> ExtractAudioMetadata(string audioPath)
        {
            Dictionary<string, object> metadata = new Dictionary<string, object>();

            try
            {
                string metaPath = audioPath + ".meta";
                if (File.Exists(metaPath))
                {
                    string metaContent = File.ReadAllText(metaPath);
                    
                    if (metaContent.Contains("loadType:"))
                    {
                        System.Text.RegularExpressions.Match loadTypeMatch = System.Text.RegularExpressions.Regex.Match(metaContent, @"loadType:\s*(\d+)");
                        if (loadTypeMatch.Success)
                        {
                            metadata["LoadType"] = int.Parse(loadTypeMatch.Groups[1].Value);
                        }
                    }

                    metadata["Is3D"] = metaContent.Contains("3D: 1");
                    metadata["IsLooping"] = metaContent.Contains("loopTime: 1");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to extract audio metadata for {audioPath}: {ex.Message}");
            }

            return metadata;
        }

        private Dictionary<string, object> ExtractMeshMetadata(string meshPath)
        {
            Dictionary<string, object> metadata = new Dictionary<string, object>();

            try
            {
                string metaPath = meshPath + ".meta";
                if (File.Exists(metaPath))
                {
                    string metaContent = File.ReadAllText(metaPath);
                    
                    metadata["HasAnimations"] = metaContent.Contains("importAnimation: 1");
                    metadata["OptimizeMesh"] = metaContent.Contains("optimizeMesh: 1");
                    metadata["GenerateColliders"] = metaContent.Contains("addCollider: 1");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to extract mesh metadata for {meshPath}: {ex.Message}");
            }

            return metadata;
        }

        private List<string> ExtractAssetLabels(string assetPath)
        {
            List<string> labels = new List<string>();

            try
            {
                string metaPath = assetPath + ".meta";
                if (File.Exists(metaPath))
                {
                    string metaContent = File.ReadAllText(metaPath);
                    
                    System.Text.RegularExpressions.Match labelsMatch = System.Text.RegularExpressions.Regex.Match(metaContent, @"labels:\s*\[(.*?)\]", System.Text.RegularExpressions.RegexOptions.Singleline);
                    if (labelsMatch.Success)
                    {
                        string labelsText = labelsMatch.Groups[1].Value;
                        System.Text.RegularExpressions.MatchCollection labelMatches = System.Text.RegularExpressions.Regex.Matches(labelsText, @"'([^']*)'");
                        
                        foreach (System.Text.RegularExpressions.Match match in labelMatches)
                        {
                            labels.Add(match.Groups[1].Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to extract asset labels for {assetPath}: {ex.Message}");
            }

            return labels;
        }

        private List<AssetIssue> DetectAssetIssues(AssetAnalysisResult result)
        {
            List<AssetIssue> issues = new List<AssetIssue>();

            foreach (AssetInfo asset in result.Assets)
            {
                if (asset.SizeBytes > 10 * 1024 * 1024)
                {
                    issues.Add(new AssetIssue(AssetIssueType.LargeAsset, 
                        $"Asset {asset.Name} is very large ({FormatBytes(asset.SizeBytes)})", 
                        asset.Path)
                    {
                        Severity = AssetIssueSeverity.Warning,
                        Suggestion = "Consider optimizing this asset to reduce file size"
                    });
                }

                if (asset.AssetType == "Texture2D" && asset.Metadata.ContainsKey("MaxTextureSize"))
                {
                    int maxSize = (int)asset.Metadata["MaxTextureSize"];
                    if (maxSize > 2048)
                    {
                        issues.Add(new AssetIssue(AssetIssueType.UnoptimizedAsset, 
                            $"Texture {asset.Name} has very high resolution ({maxSize}x{maxSize})", 
                            asset.Path)
                        {
                            Severity = AssetIssueSeverity.Warning,
                            Suggestion = "Consider reducing texture resolution if not needed"
                        });
                    }
                }
            }

            if (result.UsageReport != null)
            {
                foreach (string unusedAsset in result.UsageReport.UnusedAssets)
                {
                    issues.Add(new AssetIssue(AssetIssueType.UnusedAsset, 
                        $"Asset is not used anywhere in the project", 
                        unusedAsset)
                    {
                        Severity = AssetIssueSeverity.Info,
                        Suggestion = "Consider removing this asset if it's not needed"
                    });
                }
            }

            return issues;
        }

        private AssetMetrics CalculateAssetMetrics(AssetAnalysisResult result)
        {
            AssetMetrics metrics = new AssetMetrics
            {
                TotalAssets = result.Assets.Count,
                TotalSizeBytes = result.Assets.Sum(a => a.SizeBytes)
            };

            metrics.AssetCountByType = result.Assets
                .GroupBy(a => a.AssetType)
                .ToDictionary(g => g.Key, g => g.Count());

            metrics.AssetSizeByType = result.Assets
                .GroupBy(a => a.AssetType)
                .ToDictionary(g => g.Key, g => g.Sum(a => a.SizeBytes));

            if (result.UsageReport != null)
            {
                metrics.UnusedAssets = result.UsageReport.UnusedAssets.Count;
            }

            if (metrics.TotalAssets > 0)
            {
                metrics.AverageAssetSize = (float)metrics.TotalSizeBytes / metrics.TotalAssets;
            }

            return metrics;
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