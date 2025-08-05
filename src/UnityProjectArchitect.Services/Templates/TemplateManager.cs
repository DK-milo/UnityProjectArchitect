using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityProjectArchitect.AI.Services;
using UnityProjectArchitect.Core;
using UnityProjectArchitect.Services.Utilities;

namespace UnityProjectArchitect.Services
{
    public class TemplateManager : ITemplateManager
    {
        public event Action<TemplateOperationResult> OnTemplateApplied;
        public event Action<string, float> OnTemplateProgress;
        public event Action<string> OnError;

        private readonly List<ProjectTemplate> _availableTemplates;
        private readonly FolderStructureManager _folderManager;
        private readonly TemplateValidator _validator;
        private readonly ConflictResolver _conflictResolver;
        
        public TemplateManager()
        {
            _availableTemplates = new List<ProjectTemplate>();
            _folderManager = new FolderStructureManager();
            _validator = new TemplateValidator();
            _conflictResolver = new ConflictResolver();
            LoadBuiltInTemplates();
        }

        public async Task<List<ProjectTemplate>> GetAvailableTemplatesAsync()
        {
            await Task.Delay(1); // Simulate async operation
            return new List<ProjectTemplate>(_availableTemplates);
        }

        public async Task<List<ProjectTemplate>> GetTemplatesForProjectType(ProjectType projectType)
        {
            await Task.Delay(1);
            return _availableTemplates
                .Where(t => t.TargetProjectType == projectType || t.TargetProjectType == ProjectType.General)
                .OrderBy(t => t.TargetProjectType == projectType ? 0 : 1)
                .ThenBy(t => t.TemplateName)
                .ToList();
        }

        public async Task<ProjectTemplate> GetTemplateByIdAsync(string templateId)
        {
            await Task.Delay(1);
            return _availableTemplates.FirstOrDefault(t => t.TemplateId == templateId);
        }

        public async Task<TemplateOperationResult> ApplyTemplateAsync(ProjectData projectData, ProjectTemplate template)
        {
            TemplateOperationResult result = new TemplateOperationResult(template.TemplateId, template.TemplateName);
            DateTime startTime = DateTime.Now;
            
            try
            {
                OnTemplateProgress?.Invoke("Validating template compatibility...", 0.1f);
                
                ValidationResult validationResult = await ValidateTemplateCompatibilityAsync(template, projectData);
                if (!validationResult.IsValid)
                {
                    result.Success = false;
                    result.ErrorMessage = "Template validation failed: " + validationResult.GetFormattedReport();
                    result.ValidationResult = validationResult;
                    return result;
                }

                OnTemplateProgress?.Invoke("Detecting conflicts...", 0.2f);
                
                List<TemplateConflict> conflicts = DetectConflicts(projectData, template);
                if (conflicts.Count > 0)
                {
                    OnTemplateProgress?.Invoke("Resolving conflicts...", 0.3f);
                    
                    TemplateOperationResult conflictResult = await ResolveConflictsAsync(conflicts, ConflictResolution.Skip);
                    result.ResolvedConflicts = conflicts;
                    
                    if (!conflictResult.Success)
                    {
                        result.Success = false;
                        result.ErrorMessage = "Conflict resolution failed: " + conflictResult.ErrorMessage;
                        return result;
                    }
                }

                OnTemplateProgress?.Invoke("Creating folder structure...", 0.5f);
                
                FolderOperationResult folderResult = await _folderManager.CreateFolderStructureAsync(
                    template.FolderStructure, 
                    Application.dataPath
                );
                
                if (folderResult.Success)
                {
                    result.CreatedFolders.AddRange(folderResult.AffectedPaths);
                }

                OnTemplateProgress?.Invoke("Creating scenes...", 0.7f);
                
                await CreateScenesFromTemplate(template, result);

                OnTemplateProgress?.Invoke("Creating assembly definitions...", 0.8f);
                
                await CreateAssemblyDefinitions(template, result);

                OnTemplateProgress?.Invoke("Updating project data...", 0.9f);
                
                UpdateProjectDataFromTemplate(projectData, template);

                OnTemplateProgress?.Invoke("Finalizing...", 1.0f);
                
                result.Success = true;
                result.OperationTime = DateTime.Now - startTime;

#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                OnError?.Invoke($"Template application failed: {ex.Message}");
            }
            finally
            {
                result.OperationTime = DateTime.Now - startTime;
                OnTemplateApplied?.Invoke(result);
            }

            return result;
        }

        public async Task<TemplateOperationResult> ApplyMultipleTemplatesAsync(ProjectData projectData, List<ProjectTemplate> templates)
        {
            TemplateOperationResult combinedResult = new TemplateOperationResult("multiple", "Multiple Templates");
            DateTime startTime = DateTime.Now;

            try
            {
                for (int i = 0; i < templates.Count; i++)
                {
                    ProjectTemplate template = templates[i];
                    float progress = (float)i / templates.Count;
                    
                    OnTemplateProgress?.Invoke($"Applying template {i + 1}/{templates.Count}: {template.TemplateName}", progress);
                    
                    TemplateOperationResult result = await ApplyTemplateAsync(projectData, template);
                    
                    if (result.Success)
                    {
                        combinedResult.CreatedFolders.AddRange(result.CreatedFolders);
                        combinedResult.CreatedFiles.AddRange(result.CreatedFiles);
                        combinedResult.ModifiedFiles.AddRange(result.ModifiedFiles);
                        combinedResult.ResolvedConflicts.AddRange(result.ResolvedConflicts);
                    }
                    else
                    {
                        combinedResult.Success = false;
                        combinedResult.ErrorMessage += $"{template.TemplateName}: {result.ErrorMessage}; ";
                    }
                }

                combinedResult.Success = combinedResult.Success && combinedResult.TotalChanges > 0;
            }
            catch (Exception ex)
            {
                combinedResult.Success = false;
                combinedResult.ErrorMessage = ex.Message;
            }
            finally
            {
                combinedResult.OperationTime = DateTime.Now - startTime;
            }

            return combinedResult;
        }

        public async Task<ValidationResult> ValidateTemplateAsync(ProjectTemplate template)
        {
            return await _validator.ValidateAsync(template);
        }

        public async Task<ValidationResult> ValidateTemplateCompatibilityAsync(ProjectTemplate template, ProjectData projectData)
        {
            return await _validator.ValidateCompatibilityAsync(template, projectData);
        }

        public async Task<ProjectTemplate> CreateTemplateFromProjectAsync(ProjectData projectData, string templateName)
        {
            ProjectTemplate template = ScriptableObject.CreateInstance<ProjectTemplate>();
            template.name = templateName;
            template.TemplateName = templateName;
            template.TemplateId = Guid.NewGuid().ToString();
            template.TargetProjectType = projectData.ProjectType;
            template.Author = projectData.TeamName;
            template.TemplateDescription = $"Template created from {projectData.ProjectName}";

            // Analyze current project structure
            FolderStructureData currentStructure = await _folderManager.AnalyzeExistingStructureAsync(Application.dataPath);
            template.FolderStructure.Folders = currentStructure.Folders;

            // Copy documentation sections as defaults
            template.DefaultDocumentationSections.Clear();
            foreach (DocumentationSectionData section in projectData.DocumentationSections)
            {
                DocumentationSection templateSection = new DocumentationSection
                {
                    Type = section.SectionType,
                    IsEnabled = section.IsEnabled,
                    AIMode = section.AIMode,
                    Content = section.Content,
                    Title = section.Title,
                    CustomPrompt = section.CustomPrompt
                };
                template.DefaultDocumentationSections.Add(templateSection);
            }

            await Task.Delay(1);
            return template;
        }

        public async Task<TemplateOperationResult> SaveTemplateAsync(ProjectTemplate template)
        {
            TemplateOperationResult result = new TemplateOperationResult(template.TemplateId, template.TemplateName);
            
            try
            {
#if UNITY_EDITOR
                string path = $"Assets/ProjectTemplates/{template.TemplateName}.asset";
                string directory = Path.GetDirectoryName(path);
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                UnityEditor.AssetDatabase.CreateAsset(template, path);
                UnityEditor.AssetDatabase.SaveAssets();
                
                result.CreatedFiles.Add(path);
#endif

                if (!_availableTemplates.Contains(template))
                {
                    _availableTemplates.Add(template);
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

        public async Task<TemplateOperationResult> DeleteTemplateAsync(string templateId)
        {
            TemplateOperationResult result = new TemplateOperationResult(templateId, "");
            
            try
            {
                ProjectTemplate template = _availableTemplates.FirstOrDefault(t => t.TemplateId == templateId);
                if (template != null)
                {
                    _availableTemplates.Remove(template);
                    
#if UNITY_EDITOR
                    string assetPath = UnityEditor.AssetDatabase.GetAssetPath(template);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        UnityEditor.AssetDatabase.DeleteAsset(assetPath);
                        result.ModifiedFiles.Add(assetPath);
                    }
#endif
                    result.Success = true;
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = "Template not found";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            await Task.Delay(1);
            return result;
        }

        public List<TemplateConflict> DetectConflicts(ProjectData projectData, ProjectTemplate template)
        {
            return _conflictResolver.DetectConflicts(projectData, template);
        }

        public async Task<TemplateOperationResult> ResolveConflictsAsync(List<TemplateConflict> conflicts, ConflictResolution resolution)
        {
            return await _conflictResolver.ResolveConflictsAsync(conflicts, resolution);
        }

        public TemplateManagerCapabilities GetCapabilities()
        {
            return new TemplateManagerCapabilities
            {
                SupportedProjectTypes = Enum.GetValues(typeof(ProjectType)).Cast<ProjectType>().ToList(),
                SupportsCustomTemplates = true,
                SupportsTemplateValidation = true,
                SupportsConflictResolution = true,
                SupportsProgressTracking = true,
                SupportsUndo = false,
                SupportsBackup = true,
                SupportedFileTypes = new List<string> { ".unity", ".asmdef", ".json", ".md" },
                Features = new Dictionary<string, bool>
                {
                    { "AsyncOperations", true },
                    { "BatchProcessing", true },
                    { "ValidationReporting", true },
                    { "ConflictDetection", true },
                    { "FolderStructureManagement", true },
                    { "SceneTemplates", true },
                    { "AssemblyDefinitions", true }
                }
            };
        }

        private void LoadBuiltInTemplates()
        {
            // Load built-in templates from Resources or create them programmatically
            _availableTemplates.AddRange(CreateBuiltInTemplates());
        }

        private List<ProjectTemplate> CreateBuiltInTemplates()
        {
            List<ProjectTemplate> templates = new List<ProjectTemplate>();

            // General Template
            ProjectTemplate generalTemplate = CreateGeneralTemplate();
            templates.Add(generalTemplate);

            // Mobile 2D Template
            ProjectTemplate mobile2DTemplate = CreateMobile2DTemplate();
            templates.Add(mobile2DTemplate);

            // PC 3D Template
            ProjectTemplate pc3DTemplate = CreatePC3DTemplate();
            templates.Add(pc3DTemplate);

            return templates;
        }

        private ProjectTemplate CreateGeneralTemplate()
        {
            ProjectTemplate template = ScriptableObject.CreateInstance<ProjectTemplate>();
            template.name = "GeneralTemplate";
            template.TemplateId = "general-unity-template";
            template.TemplateName = "General Unity Project";
            template.TemplateDescription = "A general-purpose Unity project template with standard folder structure";
            template.TargetProjectType = ProjectType.General;
            template.Author = "Unity Project Architect";
            
            // Create standard folder structure
            FolderStructureData folderStructure = new FolderStructureData();
            AddFoldersToStructure(folderStructure, new[]
            {
                new FolderDefinition("Scripts", FolderType.Scripts, "Game logic and components"),
                new FolderDefinition("Prefabs", FolderType.Prefabs, "Reusable game objects"),
                new FolderDefinition("Materials", FolderType.Materials, "Material assets"),
                new FolderDefinition("Textures", FolderType.Textures, "Texture and image assets"),
                new FolderDefinition("Audio", FolderType.Audio, "Sound effects and music"),
                new FolderDefinition("Scenes", FolderType.Scenes, "Unity scenes"),
                new FolderDefinition("Animation", FolderType.Animation, "Animation controllers and clips")
            });
            template.FolderStructure = folderStructure;

            // Default documentation sections
            template.InitializeDefaultDocumentationSections();

            return template;
        }

        private ProjectTemplate CreateMobile2DTemplate()
        {
            ProjectTemplate template = ScriptableObject.CreateInstance<ProjectTemplate>();
            template.name = "Mobile2DTemplate";
            template.TemplateId = "mobile-2d-template";
            template.TemplateName = "Mobile 2D Game";
            template.TemplateDescription = "Template for mobile 2D games with touch input and performance optimization";
            template.TargetProjectType = ProjectType.Mobile2D;
            template.Author = "Unity Project Architect";

            // Mobile-specific folder structure
            FolderStructureData folderStructure = new FolderStructureData();
            AddFoldersToStructure(folderStructure, new[]
            {
                new FolderDefinition("Scripts", FolderType.Scripts),
                new FolderDefinition("Sprites", FolderType.Textures, "2D sprites and textures"),
                new FolderDefinition("UI", FolderType.Prefabs, "UI prefabs and elements"),
                new FolderDefinition("Audio", FolderType.Audio),
                new FolderDefinition("Scenes", FolderType.Scenes),
                new FolderDefinition("Animation", FolderType.Animation),
                new FolderDefinition("Mobile", FolderType.Custom, "Mobile-specific assets and configs")
            });
            template.FolderStructure = folderStructure;

            // Mobile-specific packages
            template.RequiredPackages.AddRange(new[]
            {
                "com.unity.2d.sprite",
                "com.unity.2d.tilemap",
                "com.unity.mobile.notifications"
            });

            template.InitializeDefaultDocumentationSections();
            return template;
        }

        private ProjectTemplate CreatePC3DTemplate()
        {
            ProjectTemplate template = ScriptableObject.CreateInstance<ProjectTemplate>();
            template.name = "PC3DTemplate";
            template.TemplateId = "pc-3d-template";
            template.TemplateName = "PC 3D Game";
            template.TemplateDescription = "Template for PC 3D games with advanced graphics and input systems";
            template.TargetProjectType = ProjectType.PC3D;
            template.Author = "Unity Project Architect";

            // 3D-specific folder structure
            FolderStructureData folderStructure = new FolderStructureData();
            AddFoldersToStructure(folderStructure, new[]
            {
                new FolderDefinition("Scripts", FolderType.Scripts),
                new FolderDefinition("Models", FolderType.Art, "3D models and meshes"),
                new FolderDefinition("Materials", FolderType.Materials),
                new FolderDefinition("Textures", FolderType.Textures),
                new FolderDefinition("Shaders", FolderType.Custom, "Custom shaders"),
                new FolderDefinition("Audio", FolderType.Audio),
                new FolderDefinition("Scenes", FolderType.Scenes),
                new FolderDefinition("Animation", FolderType.Animation),
                new FolderDefinition("Lighting", FolderType.Custom, "Lighting settings and profiles")
            });
            template.FolderStructure = folderStructure;

            // 3D-specific packages
            template.RequiredPackages.AddRange(new[]
            {
                "com.unity.render-pipelines.universal",
                "com.unity.postprocessing",
                "com.unity.cinemachine"
            });

            template.InitializeDefaultDocumentationSections();
            return template;
        }

        private async Task CreateScenesFromTemplate(ProjectTemplate template, TemplateOperationResult result)
        {
            foreach (SceneTemplateData sceneTemplate in template.SceneTemplates)
            {
                if (sceneTemplate.CreateOnApply)
                {
                    try
                    {
#if UNITY_EDITOR
                        string scenePath = $"Assets/Scenes/{sceneTemplate.SceneName}.unity";
                        string sceneDirectory = Path.GetDirectoryName(scenePath);
                        
                        if (!Directory.Exists(sceneDirectory))
                        {
                            Directory.CreateDirectory(sceneDirectory);
                            result.CreatedFolders.Add(sceneDirectory);
                        }

                        UnityEngine.SceneManagement.Scene newScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                            UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
                            UnityEditor.SceneManagement.NewSceneMode.Single
                        );

                        // Add required GameObjects
                        foreach (string gameObjectName in sceneTemplate.RequiredGameObjects)
                        {
                            GameObject go = new GameObject(gameObjectName);
                        }

                        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(newScene, scenePath);
                        result.CreatedFiles.Add(scenePath);
#endif
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to create scene {sceneTemplate.SceneName}: {ex.Message}");
                    }
                }
            }

            await Task.Delay(1);
        }

        private async Task CreateAssemblyDefinitions(ProjectTemplate template, TemplateOperationResult result)
        {
            foreach (AssemblyDefinitionTemplate asmdef in template.AssemblyDefinitions)
            {
                try
                {
#if UNITY_EDITOR
                    string asmdefPath = $"Assets/Scripts/{asmdef.Name}.asmdef";
                    string asmdefDirectory = Path.GetDirectoryName(asmdefPath);
                    
                    if (!Directory.Exists(asmdefDirectory))
                    {
                        Directory.CreateDirectory(asmdefDirectory);
                        result.CreatedFolders.Add(asmdefDirectory);
                    }

                    object asmdefContent = new
                    {
                        name = asmdef.Name,
                        references = new string[0],
                        includePlatforms = new string[0],
                        excludePlatforms = new string[0],
                        allowUnsafeCode = false,
                        overrideReferences = false,
                        precompiledReferences = new string[0],
                        autoReferenced = true,
                        defineConstraints = new string[0],
                        versionDefines = new string[0],
                        noEngineReferences = false
                    };

                    string json = JsonUtility.ToJson(asmdefContent, true);
                    File.WriteAllText(asmdefPath, json);
                    result.CreatedFiles.Add(asmdefPath);
#endif
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to create assembly definition {asmdef.Name}: {ex.Message}");
                }
            }

            await Task.Delay(1);
        }

        private void AddFoldersToStructure(FolderStructureData folderStructure, FolderDefinition[] folders)
        {
            foreach (FolderDefinition folder in folders)
            {
                var folderInfo = new FolderStructureData.FolderInfo
                {
                    Name = folder.Name,
                    Path = folder.Path,
                    CreatedDate = folder.CreatedDate
                };
                folderStructure.Folders.Add(folderInfo);
            }
        }

        private void UpdateProjectDataFromTemplate(ProjectData projectData, ProjectTemplate template)
        {
            // Add template reference to project data
            TemplateReference templateRef = template.CreateReference();
            projectData.AddTemplate(templateRef);

            // Update folder structure
            if (template.FolderStructure.Folders.Count > 0)
            {
                foreach (var folder in template.FolderStructure.Folders)
                {
                    var folderDefinition = new FolderDefinition(folder.Name, FolderType.Custom)
                    {
                        Path = folder.Path,
                        CreatedDate = folder.CreatedDate
                    };
                    projectData.FolderStructure.AddFolder(folderDefinition);
                }
            }

            // Update documentation sections if they're not already configured
            foreach (DocumentationSection defaultSection in template.DefaultDocumentationSections)
            {
                DocumentationSectionData existingSection = projectData.GetDocumentationSection(defaultSection.Type);
                if (existingSection != null && string.IsNullOrEmpty(existingSection.CustomPrompt))
                {
                    existingSection.CustomPrompt = defaultSection.CustomPrompt;
                    existingSection.AIMode = defaultSection.AIMode;
                }
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(projectData);
#endif
        }
    }
}