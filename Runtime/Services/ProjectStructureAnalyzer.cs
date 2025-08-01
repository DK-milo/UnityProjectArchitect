using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services
{
    public class ProjectStructureAnalyzer
    {
        private readonly Dictionary<string, ProjectType> projectTypePatterns = new Dictionary<string, ProjectType>
        {
            { "2D", ProjectType.Game2D },
            { "3D", ProjectType.Game3D },
            { "VR", ProjectType.VR },
            { "AR", ProjectType.AR },
            { "Mobile", ProjectType.Mobile },
            { "Tool", ProjectType.Tool },
            { "Template", ProjectType.Template }
        };

        private readonly List<string> standardFolders = new List<string>
        {
            "Scripts", "Prefabs", "Materials", "Textures", "Audio", "Animations", 
            "Scenes", "Fonts", "Resources", "StreamingAssets", "Editor", "Plugins"
        };

        public async Task<ProjectStructureAnalysis> AnalyzeAsync(string projectPath)
        {
            var analysis = new ProjectStructureAnalysis();

            await Task.Run(() =>
            {
                try
                {
                    var assetsPath = Path.Combine(projectPath, "Assets");
                    var projectSettingsPath = Path.Combine(projectPath, "ProjectSettings");

                    if (!Directory.Exists(assetsPath))
                    {
                        throw new DirectoryNotFoundException($"Assets folder not found at: {assetsPath}");
                    }

                    analysis.Folders = AnalyzeFolderStructure(assetsPath);
                    analysis.Files = AnalyzeFiles(assetsPath);
                    analysis.AssemblyDefinitions = AnalyzeAssemblyDefinitions(assetsPath);
                    analysis.Scenes = AnalyzeScenes(assetsPath);
                    analysis.DetectedProjectType = DetectProjectType(projectPath, analysis);
                    analysis.DetectedUnityVersion = DetectUnityVersion(projectSettingsPath);
                    analysis.FollowsStandardStructure = CheckStandardStructure(analysis.Folders);
                    analysis.Issues = DetectStructureIssues(analysis);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error analyzing project structure: {ex.Message}");
                    analysis.Issues.Add(new StructureIssue(StructureIssueType.MisplacedFile, 
                        $"Failed to analyze project structure: {ex.Message}", projectPath)
                    {
                        Severity = StructureIssueSeverity.Critical
                    });
                }
            });

            return analysis;
        }

        private List<FolderInfo> AnalyzeFolderStructure(string assetsPath)
        {
            var folders = new List<FolderInfo>();

            try
            {
                var directories = Directory.GetDirectories(assetsPath, "*", SearchOption.AllDirectories);
                
                foreach (var directory in directories)
                {
                    var dirInfo = new DirectoryInfo(directory);
                    var folderInfo = new FolderInfo(directory, dirInfo.Name)
                    {
                        FileCount = Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly).Length,
                        SubfolderCount = Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly).Length,
                        CreatedDate = dirInfo.CreationTime,
                        ModifiedDate = dirInfo.LastWriteTime,
                        Tags = DetermineFolderTags(dirInfo.Name, directory)
                    };

                    folderInfo.SizeBytes = CalculateFolderSize(directory);
                    folders.Add(folderInfo);
                }

                var rootFolder = new FolderInfo(assetsPath, "Assets")
                {
                    FileCount = Directory.GetFiles(assetsPath, "*", SearchOption.TopDirectoryOnly).Length,
                    SubfolderCount = Directory.GetDirectories(assetsPath, "*", SearchOption.TopDirectoryOnly).Length,
                    CreatedDate = new DirectoryInfo(assetsPath).CreationTime,
                    ModifiedDate = new DirectoryInfo(assetsPath).LastWriteTime,
                    Tags = new List<string> { "Root" }
                };
                rootFolder.SizeBytes = CalculateFolderSize(assetsPath);
                folders.Insert(0, rootFolder);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error analyzing folder structure: {ex.Message}");
            }

            return folders;
        }

        private List<Core.FileInfo> AnalyzeFiles(string assetsPath)
        {
            var files = new List<Core.FileInfo>();

            try
            {
                var allFiles = Directory.GetFiles(assetsPath, "*", SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith(".meta"))
                    .ToArray();

                foreach (var filePath in allFiles)
                {
                    var systemFileInfo = new System.IO.FileInfo(filePath);
                    var fileInfo = new Core.FileInfo(filePath)
                    {
                        SizeBytes = systemFileInfo.Length,
                        CreatedDate = systemFileInfo.CreationTime,
                        ModifiedDate = systemFileInfo.LastWriteTime,
                        Type = DetermineFileType(filePath)
                    };

                    if (fileInfo.Type == FileType.Script)
                    {
                        fileInfo.LineCount = CountLinesInFile(filePath);
                        fileInfo.Dependencies = ExtractScriptDependencies(filePath);
                    }

                    files.Add(fileInfo);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error analyzing files: {ex.Message}");
            }

            return files;
        }

        private List<AssemblyDefinitionInfo> AnalyzeAssemblyDefinitions(string assetsPath)
        {
            var assemblyDefinitions = new List<AssemblyDefinitionInfo>();

            try
            {
                var asmdefFiles = Directory.GetFiles(assetsPath, "*.asmdef", SearchOption.AllDirectories);

                foreach (var asmdefFile in asmdefFiles)
                {
                    try
                    {
                        var content = File.ReadAllText(asmdefFile);
                        var asmdefInfo = ParseAssemblyDefinition(asmdefFile, content);
                        assemblyDefinitions.Add(asmdefInfo);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to parse assembly definition {asmdefFile}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error analyzing assembly definitions: {ex.Message}");
            }

            return assemblyDefinitions;
        }

        private List<SceneInfo> AnalyzeScenes(string assetsPath)
        {
            var scenes = new List<SceneInfo>();

            try
            {
                var sceneFiles = Directory.GetFiles(assetsPath, "*.unity", SearchOption.AllDirectories);

                foreach (var sceneFile in sceneFiles)
                {
                    var sceneInfo = new SceneInfo(sceneFile)
                    {
                        FileSizeBytes = new System.IO.FileInfo(sceneFile).Length
                    };

                    try
                    {
                        var sceneData = AnalyzeSceneContent(sceneFile);
                        sceneInfo.GameObjectCount = sceneData.GameObjectCount;
                        sceneInfo.ComponentTypes = sceneData.ComponentTypes;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to analyze scene content for {sceneFile}: {ex.Message}");
                    }

                    scenes.Add(sceneInfo);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error analyzing scenes: {ex.Message}");
            }

            return scenes;
        }

        private ProjectType DetectProjectType(string projectPath, ProjectStructureAnalysis analysis)
        {
            try
            {
                var projectName = Path.GetFileName(projectPath).ToLower();
                
                foreach (var pattern in projectTypePatterns)
                {
                    if (projectName.Contains(pattern.Key.ToLower()))
                    {
                        return pattern.Value;
                    }
                }

                var hasVrFolders = analysis.Folders.Any(f => f.Name.ToLower().Contains("vr") || f.Name.ToLower().Contains("xr"));
                if (hasVrFolders)
                    return ProjectType.VR;

                var hasArFolders = analysis.Folders.Any(f => f.Name.ToLower().Contains("ar"));
                if (hasArFolders)
                    return ProjectType.AR;

                var hasMobileFolders = analysis.Folders.Any(f => f.Name.ToLower().Contains("mobile"));
                if (hasMobileFolders)
                    return ProjectType.Mobile;

                var has3DAssets = analysis.Files.Any(f => f.Extension == ".fbx" || f.Extension == ".obj");
                var has2DAssets = analysis.Files.Any(f => f.Extension == ".png" || f.Extension == ".jpg") && 
                                 !has3DAssets;
                
                if (has3DAssets)
                    return ProjectType.Game3D;
                else if (has2DAssets)
                    return ProjectType.Game2D;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error detecting project type: {ex.Message}");
            }

            return ProjectType.General;
        }

        private UnityVersion DetectUnityVersion(string projectSettingsPath)
        {
            try
            {
                var versionFilePath = Path.Combine(projectSettingsPath, "ProjectVersion.txt");
                
                if (!File.Exists(versionFilePath))
                    return UnityVersion.Unknown;

                var content = File.ReadAllText(versionFilePath);
                var versionMatch = Regex.Match(content, @"m_EditorVersion:\s*(\d+)\.(\d+)\.(\d+)");
                
                if (versionMatch.Success)
                {
                    var major = int.Parse(versionMatch.Groups[1].Value);
                    var minor = int.Parse(versionMatch.Groups[2].Value);
                    
                    return (major, minor) switch
                    {
                        (2023, 3) => UnityVersion.Unity2023_3,
                        (2023, 2) => UnityVersion.Unity2023_2,
                        (2023, 1) => UnityVersion.Unity2023_1,
                        (2022, 3) => UnityVersion.Unity2022_3,
                        (2022, 2) => UnityVersion.Unity2022_2,
                        (2022, 1) => UnityVersion.Unity2022_1,
                        (2021, 3) => UnityVersion.Unity2021_3,
                        _ => UnityVersion.Other
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error detecting Unity version: {ex.Message}");
            }

            return UnityVersion.Unknown;
        }

        private bool CheckStandardStructure(List<FolderInfo> folders)
        {
            var folderNames = folders.Select(f => f.Name).ToList();
            var standardFoldersFound = standardFolders.Count(sf => folderNames.Contains(sf));
            
            return standardFoldersFound >= (standardFolders.Count * 0.6f);
        }

        private List<StructureIssue> DetectStructureIssues(ProjectStructureAnalysis analysis)
        {
            var issues = new List<StructureIssue>();

            issues.AddRange(DetectMissingStandardFolders(analysis.Folders));
            issues.AddRange(DetectNamingIssues(analysis.Folders, analysis.Files));
            issues.AddRange(DetectDeepNesting(analysis.Folders));
            issues.AddRange(DetectLargeFiles(analysis.Files));
            issues.AddRange(DetectMisplacedFiles(analysis.Files));

            return issues;
        }

        private List<StructureIssue> DetectMissingStandardFolders(List<FolderInfo> folders)
        {
            var issues = new List<StructureIssue>();
            var folderNames = folders.Select(f => f.Name).ToHashSet();

            var criticalFolders = new[] { "Scripts", "Scenes" };
            var recommendedFolders = new[] { "Prefabs", "Materials", "Textures" };

            foreach (var criticalFolder in criticalFolders)
            {
                if (!folderNames.Contains(criticalFolder))
                {
                    issues.Add(new StructureIssue(StructureIssueType.MissingFolder,
                        $"Missing critical folder: {criticalFolder}",
                        $"Assets/{criticalFolder}")
                    {
                        Severity = StructureIssueSeverity.Warning,
                        Suggestion = $"Create a {criticalFolder} folder to organize your {criticalFolder.ToLower()}"
                    });
                }
            }

            foreach (var recommendedFolder in recommendedFolders)
            {
                if (!folderNames.Contains(recommendedFolder))
                {
                    issues.Add(new StructureIssue(StructureIssueType.MissingFolder,
                        $"Missing recommended folder: {recommendedFolder}",
                        $"Assets/{recommendedFolder}")
                    {
                        Severity = StructureIssueSeverity.Info,
                        Suggestion = $"Consider creating a {recommendedFolder} folder for better organization"
                    });
                }
            }

            return issues;
        }

        private List<StructureIssue> DetectNamingIssues(List<FolderInfo> folders, List<Core.FileInfo> files)
        {
            var issues = new List<StructureIssue>();

            foreach (var folder in folders)
            {
                if (HasNamingIssues(folder.Name))
                {
                    issues.Add(new StructureIssue(StructureIssueType.UnconventionalNaming,
                        $"Folder name '{folder.Name}' doesn't follow naming conventions",
                        folder.Path)
                    {
                        Severity = StructureIssueSeverity.Info,
                        Suggestion = "Use PascalCase for folder names without spaces or special characters"
                    });
                }
            }

            foreach (var file in files)
            {
                if (HasNamingIssues(file.Name))
                {
                    issues.Add(new StructureIssue(StructureIssueType.UnconventionalNaming,
                        $"File name '{file.Name}' doesn't follow naming conventions",
                        file.Path)
                    {
                        Severity = StructureIssueSeverity.Info,
                        Suggestion = "Use PascalCase for file names without spaces or special characters"
                    });
                }
            }

            return issues;
        }

        private List<StructureIssue> DetectDeepNesting(List<FolderInfo> folders)
        {
            var issues = new List<StructureIssue>();

            foreach (var folder in folders)
            {
                var depth = folder.Path.Split(Path.DirectorySeparatorChar).Length - 1;
                if (depth > 6)
                {
                    issues.Add(new StructureIssue(StructureIssueType.DeepNesting,
                        $"Folder is nested too deeply (depth: {depth})",
                        folder.Path)
                    {
                        Severity = StructureIssueSeverity.Warning,
                        Suggestion = "Consider flattening the folder structure for better maintainability"
                    });
                }
            }

            return issues;
        }

        private List<StructureIssue> DetectLargeFiles(List<Core.FileInfo> files)
        {
            var issues = new List<StructureIssue>();

            foreach (var file in files)
            {
                if (file.SizeBytes > 50 * 1024 * 1024) // 50 MB
                {
                    issues.Add(new StructureIssue(StructureIssueType.LargeFile,
                        $"File is very large ({FormatBytes(file.SizeBytes)})",
                        file.Path)
                    {
                        Severity = StructureIssueSeverity.Warning,
                        Suggestion = "Consider optimizing or splitting this large file"
                    });
                }
            }

            return issues;
        }

        private List<StructureIssue> DetectMisplacedFiles(List<Core.FileInfo> files)
        {
            var issues = new List<StructureIssue>();

            foreach (var file in files)
            {
                var expectedFolder = GetExpectedFolderForFileType(file.Type);
                var actualFolder = Path.GetDirectoryName(file.Path);
                
                if (!string.IsNullOrEmpty(expectedFolder) && 
                    !actualFolder.Contains(expectedFolder, StringComparison.OrdinalIgnoreCase))
                {
                    issues.Add(new StructureIssue(StructureIssueType.MisplacedFile,
                        $"{file.Type} file might be better placed in a {expectedFolder} folder",
                        file.Path)
                    {
                        Severity = StructureIssueSeverity.Info,
                        Suggestion = $"Consider moving this file to a {expectedFolder} folder for better organization"
                    });
                }
            }

            return issues;
        }

        private long CalculateFolderSize(string folderPath)
        {
            try
            {
                var files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
                return files.Sum(file => new System.IO.FileInfo(file).Length);
            }
            catch
            {
                return 0;
            }
        }

        private List<string> DetermineFolderTags(string folderName, string folderPath)
        {
            var tags = new List<string>();

            if (standardFolders.Contains(folderName))
                tags.Add("Standard");
            
            if (folderName.Contains("Editor"))
                tags.Add("Editor");
            
            if (folderName.Contains("Resources"))
                tags.Add("Resources");
            
            if (folderName.Contains("StreamingAssets"))
                tags.Add("StreamingAssets");
            
            if (folderPath.Contains("Plugins"))
                tags.Add("Plugin");

            return tags;
        }

        private FileType DetermineFileType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            
            return extension switch
            {
                ".cs" => FileType.Script,
                ".unity" => FileType.Scene,
                ".prefab" => FileType.Prefab,
                ".mat" => FileType.Material,
                ".png" or ".jpg" or ".jpeg" or ".tga" or ".bmp" => FileType.Texture,
                ".wav" or ".mp3" or ".ogg" or ".aiff" => FileType.Audio,
                ".fbx" or ".obj" or ".dae" or ".3ds" => FileType.Mesh,
                ".anim" => FileType.Animation,
                ".shader" or ".compute" => FileType.Shader,
                ".ttf" or ".otf" => FileType.Font,
                _ => FileType.Other
            };
        }

        private int CountLinesInFile(string filePath)
        {
            try
            {
                return File.ReadAllLines(filePath).Length;
            }
            catch
            {
                return 0;
            }
        }

        private List<string> ExtractScriptDependencies(string scriptPath)
        {
            var dependencies = new List<string>();

            try
            {
                var content = File.ReadAllText(scriptPath);
                var usingMatches = Regex.Matches(content, @"using\s+([A-Za-z_][A-Za-z0-9_.]*)\s*;");
                
                foreach (Match match in usingMatches)
                {
                    dependencies.Add(match.Groups[1].Value);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to extract dependencies from {scriptPath}: {ex.Message}");
            }

            return dependencies.Distinct().ToList();
        }

        private AssemblyDefinitionInfo ParseAssemblyDefinition(string filePath, string content)
        {
            var asmdefInfo = new AssemblyDefinitionInfo
            {
                Path = filePath,
                Name = Path.GetFileNameWithoutExtension(filePath)
            };

            try
            {
                var json = JsonUtility.FromJson<AssemblyDefinitionJson>(content);
                
                if (json != null)
                {
                    asmdefInfo.Name = json.name ?? asmdefInfo.Name;
                    asmdefInfo.References = json.references?.ToList() ?? new List<string>();
                    asmdefInfo.DefineConstraints = json.defineConstraints?.ToList() ?? new List<string>();
                    asmdefInfo.VersionDefines = json.versionDefines?.Select(vd => $"{vd.name}@{vd.expression}").ToList() ?? new List<string>();
                    asmdefInfo.AutoReferenced = json.autoReferenced;
                    asmdefInfo.NoEngineReferences = json.noEngineReferences;
                    
                    if (json.includePlatforms != null && json.includePlatforms.Length > 0)
                    {
                        asmdefInfo.Platforms.IncludePlatforms = json.includePlatforms.ToList();
                        asmdefInfo.Platforms.IncludesAnyPlatform = false;
                    }
                    else
                    {
                        asmdefInfo.Platforms.IncludesAnyPlatform = true;
                    }
                    
                    if (json.excludePlatforms != null)
                    {
                        asmdefInfo.Platforms.ExcludePlatforms = json.excludePlatforms.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to parse assembly definition JSON for {filePath}: {ex.Message}");
            }

            return asmdefInfo;
        }

        private (int GameObjectCount, List<string> ComponentTypes) AnalyzeSceneContent(string sceneFile)
        {
            var gameObjectCount = 0;
            var componentTypes = new HashSet<string>();

            try
            {
                var content = File.ReadAllText(sceneFile);
                
                var gameObjectMatches = Regex.Matches(content, @"GameObject:");
                gameObjectCount = gameObjectMatches.Count;
                
                var componentMatches = Regex.Matches(content, @"MonoBehaviour:\s*.*\n.*m_Script:.*guid:\s*([a-f0-9]+)");
                foreach (Match match in componentMatches)
                {
                    componentTypes.Add("MonoBehaviour");
                }
                
                var builtInComponents = new[] { "Transform", "Camera", "Light", "MeshRenderer", "Collider", "Rigidbody", "AudioSource" };
                foreach (var component in builtInComponents)
                {
                    if (content.Contains($"{component}:"))
                    {
                        componentTypes.Add(component);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to analyze scene content for {sceneFile}: {ex.Message}");
            }

            return (gameObjectCount, componentTypes.ToList());
        }

        private bool HasNamingIssues(string name)
        {
            return name.Contains(" ") || 
                   name.Contains("-") || 
                   Regex.IsMatch(name, @"[^a-zA-Z0-9_]") ||
                   char.IsLower(name[0]);
        }

        private string GetExpectedFolderForFileType(FileType fileType)
        {
            return fileType switch
            {
                FileType.Script => "Scripts",
                FileType.Scene => "Scenes",
                FileType.Prefab => "Prefabs",
                FileType.Material => "Materials",
                FileType.Texture => "Textures",
                FileType.Audio => "Audio",
                FileType.Mesh => "Models",
                FileType.Animation => "Animations",
                FileType.Shader => "Shaders",
                FileType.Font => "Fonts",
                _ => null
            };
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

        [Serializable]
        private class AssemblyDefinitionJson
        {
            public string name;
            public string[] references;
            public string[] defineConstraints;
            public VersionDefine[] versionDefines;
            public bool autoReferenced = true;
            public bool noEngineReferences = false;
            public string[] includePlatforms;
            public string[] excludePlatforms;
        }

        [Serializable]
        private class VersionDefine
        {
            public string name;
            public string expression;
            public string define;
        }
    }
}