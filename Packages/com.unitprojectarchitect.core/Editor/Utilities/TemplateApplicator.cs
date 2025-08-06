using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityProjectArchitect.Unity;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Unity.Editor.Utilities
{
    /// <summary>
    /// Utility for applying template configurations to Unity projects
    /// Creates folder structures and scenes based on template definitions
    /// </summary>
    public static class TemplateApplicator
    {
        /// <summary>
        /// Apply a template configuration to the current Unity project
        /// </summary>
        /// <param name="template">The template configuration to apply</param>
        /// <param name="basePath">Base path for creating folders (default: "Assets")</param>
        /// <returns>Result of template application</returns>
        public static TemplateApplyResult ApplyTemplate(TemplateConfigurationSO template, string basePath = "Assets")
        {
            TemplateApplyResult result = new TemplateApplyResult();
            DateTime startTime = DateTime.Now;
            
            try
            {
                Debug.Log($"Applying template: {template.TemplateName}");
                
                // Create folder structure
                CreateFolderStructure(template, basePath, result);
                
                // Create scenes
                CreateScenes(template, result);
                
                // Refresh the Asset Database to show new folders and files
                AssetDatabase.Refresh();
                
                result.ApplyTime = DateTime.Now - startTime;
                result.Success = true;
                
                Debug.Log($"Template '{template.TemplateName}' applied successfully in {result.ApplyTime.TotalSeconds:F2} seconds");
                Debug.Log($"Created {result.CreatedFolders.Count} folders and {result.CreatedFiles.Count} files");
                
                // Show completion dialog
                EditorUtility.DisplayDialog("Template Applied", 
                    $"Template '{template.TemplateName}' has been applied successfully!\n\n" +
                    $"Created:\n" +
                    $"• {result.CreatedFolders.Count} folders\n" +
                    $"• {result.CreatedFiles.Count} scenes\n\n" +
                    $"Time: {result.ApplyTime.TotalSeconds:F2} seconds", "OK");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.ApplyTime = DateTime.Now - startTime;
                
                Debug.LogError($"Failed to apply template '{template.TemplateName}': {ex.Message}");
                EditorUtility.DisplayDialog("Template Application Failed", 
                    $"Failed to apply template '{template.TemplateName}':\n\n{ex.Message}", "OK");
            }
            
            return result;
        }
        
        /// <summary>
        /// Create folder structure from template
        /// </summary>
        private static void CreateFolderStructure(TemplateConfigurationSO template, string basePath, TemplateApplyResult result)
        {
            if (template.FolderPaths == null || template.FolderPaths.Count == 0)
            {
                result.Warnings.Add("No folder paths defined in template");
                return;
            }
            
            foreach (string folderPath in template.FolderPaths)
            {
                if (string.IsNullOrWhiteSpace(folderPath))
                    continue;
                    
                string fullPath = Path.Combine(basePath, folderPath).Replace('\\', '/');
                CreateFolderRecursive(fullPath, result);
            }
        }
        
        /// <summary>
        /// Recursively create folder structure
        /// </summary>
        private static void CreateFolderRecursive(string path, TemplateApplyResult result)
        {
            // Normalize path separators
            path = path.Replace('\\', '/');
            
            // Check if folder already exists
            if (AssetDatabase.IsValidFolder(path))
            {
                result.Warnings.Add($"Folder already exists: {path}");
                return;
            }
            
            // Get parent directory
            string parentPath = Path.GetDirectoryName(path).Replace('\\', '/');
            string folderName = Path.GetFileName(path);
            
            // Ensure parent exists
            if (!string.IsNullOrEmpty(parentPath) && parentPath != "Assets" && !AssetDatabase.IsValidFolder(parentPath))
            {
                CreateFolderRecursive(parentPath, result);
            }
            
            // Create the folder
            try
            {
                string guid = AssetDatabase.CreateFolder(parentPath, folderName);
                if (!string.IsNullOrEmpty(guid))
                {
                    result.CreatedFolders.Add(path);
                    Debug.Log($"Created folder: {path}");
                }
                else
                {
                    result.Warnings.Add($"Failed to create folder: {path}");
                }
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Error creating folder {path}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Create scenes from template
        /// </summary>
        private static void CreateScenes(TemplateConfigurationSO template, TemplateApplyResult result)
        {
            if (template.SceneTemplates == null || template.SceneTemplates.Count == 0)
            {
                result.Warnings.Add("No scene templates defined");
                return;
            }
            
            Scene mainSceneToLoad = default;
            
            foreach (SceneTemplateData sceneTemplate in template.SceneTemplates)
            {
                if (string.IsNullOrWhiteSpace(sceneTemplate.SceneName) || string.IsNullOrWhiteSpace(sceneTemplate.ScenePath))
                    continue;
                    
                CreateScene(sceneTemplate, result, ref mainSceneToLoad);
            }
            
            // Load the main scene if one was created
            if (mainSceneToLoad.IsValid())
            {
                EditorSceneManager.OpenScene(mainSceneToLoad.path);
                Debug.Log($"Opened main scene: {mainSceneToLoad.path}");
            }
        }
        
        /// <summary>
        /// Create a single scene from template data
        /// </summary>
        private static void CreateScene(SceneTemplateData sceneTemplate, TemplateApplyResult result, ref Scene mainSceneToLoad)
        {
            string scenePath = sceneTemplate.ScenePath;
            if (!scenePath.StartsWith("Assets/"))
                scenePath = "Assets/" + scenePath;
            if (!scenePath.EndsWith(".unity"))
                scenePath += ".unity";
                
            // Ensure the scene directory exists
            string sceneDirectory = Path.GetDirectoryName(scenePath).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(sceneDirectory))
            {
                CreateFolderRecursive(sceneDirectory, result);
            }
            
            // Check if scene already exists
            if (File.Exists(scenePath))
            {
                result.Warnings.Add($"Scene already exists: {scenePath}");
                return;
            }
            
            try
            {
                // Create new scene
                Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                
                // Add scene name as a comment GameObject for identification
                GameObject sceneInfo = new GameObject($"_SceneInfo_{sceneTemplate.SceneName}");
                sceneInfo.AddComponent<SceneInfoComponent>().Initialize(sceneTemplate);
                
                // Save the scene
                bool saved = EditorSceneManager.SaveScene(newScene, scenePath);
                if (saved)
                {
                    result.CreatedFiles.Add(scenePath);
                    Debug.Log($"Created scene: {scenePath}");
                    
                    // Remember main scene
                    if (sceneTemplate.IsMainScene)
                    {
                        mainSceneToLoad = newScene;
                    }
                }
                else
                {
                    result.Warnings.Add($"Failed to save scene: {scenePath}");
                }
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Error creating scene {scenePath}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Validate that a template can be applied
        /// </summary>
        public static TemplateValidationResult ValidateTemplate(TemplateConfigurationSO template)
        {
            TemplateValidationResult result = new TemplateValidationResult();
            
            if (template == null)
            {
                result.IsValid = false;
                result.Errors.Add("Template is null");
                return result;
            }
            
            if (string.IsNullOrWhiteSpace(template.TemplateName))
            {
                result.Errors.Add("Template name is required");
            }
            
            if (template.FolderPaths == null || template.FolderPaths.Count == 0)
            {
                result.Warnings.Add("No folder structure defined");
            }
            
            if (template.SceneTemplates == null || template.SceneTemplates.Count == 0)
            {
                result.Warnings.Add("No scene templates defined");
            }
            else
            {
                // Check for multiple main scenes
                int mainSceneCount = 0;
                foreach (SceneTemplateData scene in template.SceneTemplates)
                {
                    if (scene.IsMainScene)
                        mainSceneCount++;
                }
                
                if (mainSceneCount > 1)
                {
                    result.Warnings.Add("Multiple scenes marked as main scene");
                }
            }
            
            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }
    
    /// <summary>
    /// Component to store scene template information in created scenes
    /// </summary>
    public class SceneInfoComponent : MonoBehaviour
    {
        [SerializeField] private string sceneName;
        [SerializeField] private bool isMainScene;
        [SerializeField] private string templateName;
        
        public void Initialize(SceneTemplateData sceneTemplate)
        {
            sceneName = sceneTemplate.SceneName;
            isMainScene = sceneTemplate.IsMainScene;
            // Store additional info if needed
        }
    }
}