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
    public class FolderStructureManager : IFolderStructureManager
    {
        private readonly List<string> _standardUnityFolders = new()
        {
            "Scripts", "Prefabs", "Materials", "Textures", "Audio", "Scenes", 
            "Animation", "Resources", "StreamingAssets", "Editor", "Plugins"
        };

        public async Task<FolderOperationResult> CreateFolderStructureAsync(FolderStructureData folderStructure, string basePath)
        {
            FolderOperationResult result = new FolderOperationResult(FolderOperationType.Create, basePath);
            
            try
            {
                if (folderStructure?.Folders == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "Folder structure data is null or empty";
                    return result;
                }

                List<string> createdPaths = new List<string>();
                
                foreach (string folder in folderStructure.Folders)
                {
                    if (folder.CreateOnApply)
                    {
                        FolderOperationResult folderResult = await CreateFolderRecursiveAsync(folder, basePath);
                        if (folderResult.Success)
                        {
                            createdPaths.AddRange(folderResult.AffectedPaths);
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to create folder {folder.Name}: {folderResult.ErrorMessage}");
                        }
                    }
                }

                result.Success = true;
                result.AffectedPaths = createdPaths;
                
#if UNITY_EDITOR
                if (createdPaths.Count > 0)
                {
                    UnityEditor.AssetDatabase.Refresh();
                }
#endif
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                Debug.LogError($"Failed to create folder structure: {ex.Message}");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateFolderStructureAsync(FolderStructureData folderStructure)
        {
            ValidationResult validationResult = new ValidationResult("Folder Structure Validation");
            
            try
            {
                if (folderStructure?.Folders == null)
                {
                    validationResult.AddError(ValidationType.ProjectStructure, 
                        "Folder structure is null", 
                        "The provided folder structure data is null or contains no folders");
                    return validationResult;
                }

                // Check for duplicate folder names
                List<string> duplicates = folderStructure.Folders
                    .GroupBy(f => f.Name.ToLowerInvariant())
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicates.Count > 0)
                {
                    validationResult.AddWarning(ValidationType.ProjectStructure,
                        "Duplicate folder names detected",
                        $"Found duplicate folders: {string.Join(", ", duplicates)}",
                        "Ensure all folder names are unique to avoid conflicts");
                }

                // Check for invalid characters in folder names
                foreach (string folder in folderStructure.Folders)
                {
                    if (string.IsNullOrWhiteSpace(folder.Name))
                    {
                        validationResult.AddError(ValidationType.ProjectStructure,
                            "Empty folder name",
                            "Found a folder with empty or whitespace-only name",
                            "Provide a valid name for all folders");
                        continue;
                    }

                    char[] invalidChars = Path.GetInvalidFileNameChars();
                    if (folder.Name.IndexOfAny(invalidChars) >= 0)
                    {
                        validationResult.AddError(ValidationType.ProjectStructure,
                            $"Invalid characters in folder name: {folder.Name}",
                            $"Folder name contains invalid characters: {string.Join(", ", invalidChars.Where(c => folder.Name.Contains(c)))}",
                            "Remove invalid characters from folder names");
                    }

                    // Validate sub-folders recursively
                    await ValidateSubFolders(folder, validationResult);
                }

                // Check for recommended Unity folder structure
                bool hasScriptsFolder = folderStructure.Folders.Any(f => 
                    f.Name.Equals("Scripts", StringComparison.OrdinalIgnoreCase));
                
                if (!hasScriptsFolder)
                {
                    validationResult.AddInfo(ValidationType.ProjectStructure,
                        "No Scripts folder found",
                        "Consider adding a Scripts folder for better organization");
                }

                bool hasScenesFolder = folderStructure.Folders.Any(f => 
                    f.Name.Equals("Scenes", StringComparison.OrdinalIgnoreCase));
                
                if (!hasScenesFolder)
                {
                    validationResult.AddInfo(ValidationType.ProjectStructure,
                        "No Scenes folder found",
                        "Consider adding a Scenes folder to organize Unity scenes");
                }
            }
            catch (Exception ex)
            {
                validationResult.AddError(ValidationType.ProjectStructure,
                    "Validation failed",
                    ex.Message);
            }

            await Task.Delay(1); // Simulate async operation
            return validationResult;
        }

        public async Task<FolderStructureData> AnalyzeExistingStructureAsync(string projectPath)
        {
            FolderStructureData folderStructure = new FolderStructureData();
            
            try
            {
                if (!Directory.Exists(projectPath))
                {
                    Debug.LogError($"Project path does not exist: {projectPath}");
                    return folderStructure;
                }

                string[] directories = Directory.GetDirectories(projectPath, "*", SearchOption.TopDirectoryOnly);
                
                foreach (string directory in directories)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(directory);
                    FolderType folderType = DetermineFolderType(dirInfo.Name);
                    
                    FolderDefinition folderDefinition = new FolderDefinition(dirInfo.Name, folderType)
                    {
                        RelativePath = Path.GetRelativePath(projectPath, directory),
                        CreateOnApply = true
                    };

                    // Analyze subdirectories
                    await AnalyzeSubDirectories(directory, folderDefinition, projectPath);
                    
                    folderStructure.AddFolder(folderDefinition);
                }

                // Set the root path
                folderStructure.RootPath = projectPath;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to analyze existing structure: {ex.Message}");
            }

            return folderStructure;
        }

        public List<string> GetStandardUnityFolders()
        {
            return new List<string>(_standardUnityFolders);
        }

        public FolderStructureData CreateStandardStructure(ProjectType projectType)
        {
            FolderStructureData structure = new FolderStructureData();
            
            // Base folders for all project types
            FolderDefinition[] baseFolders = new[]
            {
                new FolderDefinition("Scripts", FolderType.Scripts, "Game logic and components"),
                new FolderDefinition("Prefabs", FolderType.Prefabs, "Reusable game objects"),
                new FolderDefinition("Materials", FolderType.Materials, "Material assets"),
                new FolderDefinition("Scenes", FolderType.Scenes, "Unity scenes"),
                new FolderDefinition("Audio", FolderType.Audio, "Sound effects and music")
            };

            foreach (string folder in baseFolders)
            {
                structure.AddFolder(folder);
            }

            // Project type specific folders
            switch (projectType)
            {
                case ProjectType.Mobile2D:
                case ProjectType.PC2D:
                    structure.AddFolder(new FolderDefinition("Sprites", FolderType.Textures, "2D sprites and textures"));
                    structure.AddFolder(new FolderDefinition("Animation", FolderType.Animation, "2D animations"));
                    structure.AddFolder(new FolderDefinition("UI", FolderType.Prefabs, "UI elements"));
                    break;

                case ProjectType.Mobile3D:
                case ProjectType.PC3D:
                case ProjectType.Console:
                    structure.AddFolder(new FolderDefinition("Models", FolderType.Art, "3D models and meshes"));
                    structure.AddFolder(new FolderDefinition("Textures", FolderType.Textures, "Texture assets"));
                    structure.AddFolder(new FolderDefinition("Animation", FolderType.Animation, "3D animations"));
                    structure.AddFolder(new FolderDefinition("Shaders", FolderType.Custom, "Custom shaders"));
                    break;

                case ProjectType.VR:
                case ProjectType.AR:
                    structure.AddFolder(new FolderDefinition("Models", FolderType.Art, "3D models"));
                    structure.AddFolder(new FolderDefinition("Textures", FolderType.Textures, "Texture assets"));
                    structure.AddFolder(new FolderDefinition("XR", FolderType.Custom, "XR specific assets"));
                    structure.AddFolder(new FolderDefinition("Interaction", FolderType.Scripts, "XR interaction scripts"));
                    break;

                case ProjectType.WebGL:
                    structure.AddFolder(new FolderDefinition("Textures", FolderType.Textures, "Optimized textures"));
                    structure.AddFolder(new FolderDefinition("WebGL", FolderType.Custom, "WebGL specific assets"));
                    break;

                case ProjectType.Multiplayer:
                    structure.AddFolder(new FolderDefinition("Textures", FolderType.Textures, "Texture assets"));
                    structure.AddFolder(new FolderDefinition("Networking", FolderType.Scripts, "Network scripts"));
                    structure.AddFolder(new FolderDefinition("Server", FolderType.Custom, "Server-side assets"));
                    break;

                default:
                    structure.AddFolder(new FolderDefinition("Textures", FolderType.Textures, "Texture assets"));
                    structure.AddFolder(new FolderDefinition("Animation", FolderType.Animation, "Animation assets"));
                    break;
            }

            // Common optional folders
            structure.AddFolder(new FolderDefinition("Resources", FolderType.Resources, "Runtime loaded resources"));
            structure.AddFolder(new FolderDefinition("Editor", FolderType.Editor, "Editor-only scripts"));
            structure.AddFolder(new FolderDefinition("Documentation", FolderType.Documentation, "Project documentation"));

            return structure;
        }

        public bool FolderExists(string path)
        {
            return Directory.Exists(path);
        }

        public async Task<FolderOperationResult> CreateFolderAsync(string path)
        {
            FolderOperationResult result = new FolderOperationResult(FolderOperationType.Create, path);
            
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    result.AffectedPaths.Add(path);
                    
#if UNITY_EDITOR
                    UnityEditor.AssetDatabase.Refresh();
#endif
                }
                
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            await Task.Delay(1);
            return result;
        }

        public async Task<FolderOperationResult> MoveFolderAsync(string sourcePath, string destinationPath)
        {
            FolderOperationResult result = new FolderOperationResult(FolderOperationType.Move, sourcePath);
            
            try
            {
                if (Directory.Exists(sourcePath))
                {
                    Directory.Move(sourcePath, destinationPath);
                    result.AffectedPaths.Add(sourcePath);
                    result.AffectedPaths.Add(destinationPath);
                    
#if UNITY_EDITOR
                    UnityEditor.AssetDatabase.Refresh();
#endif
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = "Source folder does not exist";
                }
                
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            await Task.Delay(1);
            return result;
        }

        public async Task<FolderOperationResult> DeleteFolderAsync(string path)
        {
            FolderOperationResult result = new FolderOperationResult(FolderOperationType.Delete, path);
            
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    result.AffectedPaths.Add(path);
                    
#if UNITY_EDITOR
                    UnityEditor.AssetDatabase.Refresh();
#endif
                }
                
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            await Task.Delay(1);
            return result;
        }

        private async Task<FolderOperationResult> CreateFolderRecursiveAsync(FolderDefinition folder, string basePath)
        {
            FolderOperationResult result = new FolderOperationResult(FolderOperationType.Create, folder.RelativePath);
            List<string> createdPaths = new List<string>();
            
            try
            {
                string fullPath = Path.Combine(basePath, folder.RelativePath);
                
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                    createdPaths.Add(fullPath);
                }

                // Create subfolders recursively
                foreach (string subFolder in folder.SubFolders)
                {
                    if (subFolder.CreateOnApply)
                    {
                        FolderOperationResult subFolderResult = await CreateFolderRecursiveAsync(subFolder, fullPath);
                        if (subFolderResult.Success)
                        {
                            createdPaths.AddRange(subFolderResult.AffectedPaths);
                        }
                    }
                }

                // Create template files if specified
                foreach (string template in folder.FileTemplates)
                {
                    await CreateTemplateFile(fullPath, template);
                }

                result.Success = true;
                result.AffectedPaths = createdPaths;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        private async Task CreateTemplateFile(string folderPath, string templateName)
        {
            try
            {
                string fileName = GetTemplateFileName(templateName);
                string filePath = Path.Combine(folderPath, fileName);
                
                if (!File.Exists(filePath))
                {
                    string content = GetTemplateContent(templateName);
                    await File.WriteAllTextAsync(filePath, content);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create template file {templateName}: {ex.Message}");
            }
        }

        private string GetTemplateFileName(string templateName)
        {
            return templateName switch
            {
                "ReadMe" => "README.md",
                "GitIgnore" => ".gitignore",
                "GameManager" => "GameManager.cs",
                "PlayerController" => "PlayerController.cs",
                _ => $"{templateName}.txt"
            };
        }

        private string GetTemplateContent(string templateName)
        {
            return templateName switch
            {
                "ReadMe" => "# Project Documentation\n\nThis folder contains project documentation and notes.",
                "GitIgnore" => GetUnityGitIgnoreContent(),
                "GameManager" => GetGameManagerTemplate(),
                "PlayerController" => GetPlayerControllerTemplate(),
                _ => $"// Template file: {templateName}\n// Generated by Unity Project Architect"
            };
        }

        private string GetUnityGitIgnoreContent()
        {
            return @"# Unity generated files
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/

# Asset store tools
[Aa]ssets/AssetStoreTools*

# Visual Studio cache directory
.vs/

# Gradle cache directory
.gradle/

# Autogenerated VS/MD/Consulo solution and project files
ExportedObj/
.consulo/
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db

# Unity3D generated meta files
*.pidb.meta
*.pdb.meta
*.mdb.meta

# Unity3D generated file on crash reports
sysinfo.txt

# Builds
*.apk
*.unitypackage
*.app

# Crashlytics generated file
crashlytics-build.properties";
        }

        private string GetGameManagerTemplate()
        {
            return @"using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        // Initialize game systems here
    }
}";
        }

        private string GetPlayerControllerTemplate()
        {
            return @"using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    
    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // Handle player input here
    }
}";
        }

        private FolderType DetermineFolderType(string folderName)
        {
            return folderName.ToLowerInvariant() switch
            {
                "scripts" => FolderType.Scripts,
                "prefabs" => FolderType.Prefabs,
                "materials" => FolderType.Materials,
                "textures" => FolderType.Textures,
                "sprites" => FolderType.Textures,
                "audio" => FolderType.Audio,
                "scenes" => FolderType.Scenes,
                "animation" => FolderType.Animation,
                "animations" => FolderType.Animation,
                "resources" => FolderType.Resources,
                "streamingassets" => FolderType.StreamingAssets,
                "editor" => FolderType.Editor,
                "plugins" => FolderType.Plugins,
                "documentation" => FolderType.Documentation,
                "models" => FolderType.Art,
                "art" => FolderType.Art,
                _ => FolderType.Custom
            };
        }

        private async Task AnalyzeSubDirectories(string parentPath, FolderDefinition parentFolder, string projectRootPath)
        {
            try
            {
                string[] subdirectories = Directory.GetDirectories(parentPath, "*", SearchOption.TopDirectoryOnly);
                
                foreach (string subdirectory in subdirectories)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(subdirectory);
                    FolderType folderType = DetermineFolderType(dirInfo.Name);
                    
                    FolderDefinition subFolderDefinition = new FolderDefinition(dirInfo.Name, folderType)
                    {
                        RelativePath = Path.GetRelativePath(projectRootPath, subdirectory),
                        CreateOnApply = true
                    };

                    parentFolder.AddSubFolder(subFolderDefinition);
                    
                    // Recursively analyze deeper levels (limit depth to avoid infinite loops)
                    if (parentFolder.SubFolders.Count < 10) // Arbitrary depth limit
                    {
                        await AnalyzeSubDirectories(subdirectory, subFolderDefinition, projectRootPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to analyze subdirectories of {parentPath}: {ex.Message}");
            }
        }

        private async Task ValidateSubFolders(FolderDefinition folder, ValidationResult validationResult)
        {
            foreach (string subFolder in folder.SubFolders)
            {
                if (string.IsNullOrWhiteSpace(subFolder.Name))
                {
                    validationResult.AddError(ValidationType.ProjectStructure,
                        $"Empty subfolder name in {folder.Name}",
                        "Found a subfolder with empty or whitespace-only name",
                        "Provide a valid name for all subfolders");
                    continue;
                }

                char[] invalidChars = Path.GetInvalidFileNameChars();
                if (subFolder.Name.IndexOfAny(invalidChars) >= 0)
                {
                    validationResult.AddError(ValidationType.ProjectStructure,
                        $"Invalid characters in subfolder name: {subFolder.Name}",
                        $"Subfolder name contains invalid characters",
                        "Remove invalid characters from subfolder names");
                }

                // Recursive validation
                await ValidateSubFolders(subFolder, validationResult);
            }
        }
    }
}