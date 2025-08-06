using UnityEngine;
using UnityEditor;
using UnityProjectArchitect.Unity.Editor;
using UnityProjectArchitect.Unity;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Unity.Editor.MenuItems
{
    /// <summary>
    /// Unity Editor menu items for Unity Project Architect
    /// Provides quick access to main functionality through Unity's menu system
    /// </summary>
    public static class ProjectArchitectMenuItems
    {
        private const string MenuRoot = "Tools/Unity Project Architect/";
        private const int Priority = 1000;
        
        [MenuItem(MenuRoot + "Main Window %#p", priority = Priority)]
        public static void OpenMainWindow()
        {
            ProjectArchitectWindow.ShowWindow();
        }
        
        [MenuItem(MenuRoot + "Template Creator %#t", priority = Priority + 1)]
        public static void OpenTemplateCreator()
        {
            TemplateCreatorWindow.ShowWindow();
        }
        
        [MenuItem(MenuRoot + "Export Documentation/Markdown", priority = Priority + 10)]
        public static void ExportMarkdown()
        {
            ProjectArchitectWindow window = EditorWindow.GetWindow<ProjectArchitectWindow>();
            window.Show();
            
            EditorApplication.delayCall += () => {
                Debug.Log("Export Markdown requested from menu");
            };
        }
        
        [MenuItem(MenuRoot + "Export Documentation/PDF", priority = Priority + 11)]
        public static void ExportPDF()
        {
            ProjectArchitectWindow window = EditorWindow.GetWindow<ProjectArchitectWindow>();
            window.Show();
            
            EditorApplication.delayCall += () => {
                Debug.Log("Export PDF requested from menu");
            };
        }
        
        [MenuItem(MenuRoot + "Settings", priority = Priority + 20)]
        public static void OpenSettings()
        {
            // Open Unity Project Settings for Unity Project Architect
            SettingsService.OpenProjectSettings("Project/Unity Project Architect");
        }
        
        [MenuItem(MenuRoot + "Documentation", priority = Priority + 40)]
        public static void OpenDocumentation()
        {
            Application.OpenURL("https://github.com/UnityProjectArchitect/Documentation");
        }
        
        [MenuItem(MenuRoot + "About", priority = Priority + 50)]
        public static void ShowAbout()
        {
            EditorUtility.DisplayDialog("Unity Project Architect", 
                "Unity Project Architect v0.3.0\n\n" +
                "AI-powered project documentation and organization tool.\n\n" +
                "Features:\n" +
                "• Structure-only project templates\n" +
                "• Automated documentation generation\n" +
                "• Multi-format export (Markdown, PDF)\n" +
                "• Unity Editor integration\n\n" +
                "Copyright © 2025 Unity Project Architect Team", 
                "OK");
        }
        
        // Context menu items for project assets
        [MenuItem("Assets/Unity Project Architect/Analyze Selected Folder", priority = Priority + 100)]
        public static void AnalyzeSelectedFolder()
        {
            UnityEngine.Object[] selection = Selection.objects;
            if (selection.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select a folder to analyze.", "OK");
                return;
            }
            
            string selectedPath = AssetDatabase.GetAssetPath(selection[0]);
            if (System.IO.Directory.Exists(selectedPath))
            {
                Debug.Log($"Analyzing folder: {selectedPath}");
                ProjectArchitectWindow.ShowWindow();
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid Selection", "Please select a folder to analyze.", "OK");
            }
        }
        
        [MenuItem("Assets/Unity Project Architect/Analyze Selected Folder", validate = true)]
        public static bool ValidateAnalyzeSelectedFolder()
        {
            if (Selection.objects.Length != 1) return false;
            
            string selectedPath = AssetDatabase.GetAssetPath(Selection.objects[0]);
            return System.IO.Directory.Exists(selectedPath);
        }
        
        [MenuItem("Assets/Unity Project Architect/Create Project Data Asset", priority = Priority + 101)]
        public static void CreateProjectDataAsset()
        {
            UnityProjectDataAsset asset = ScriptableObject.CreateInstance<UnityProjectDataAsset>();
            asset.Initialize();
            asset.UpdateProjectName(Application.productName);
            
            string path = "Assets/ProjectData.asset";
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(path);
            
            AssetDatabase.CreateAsset(asset, uniquePath);
            AssetDatabase.SaveAssets();
            
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            
            Debug.Log($"Created Project Data Asset at: {uniquePath}");
        }
        
        [MenuItem("Assets/Unity Project Architect/Create Template Configuration", priority = Priority + 102)]
        public static void CreateTemplateConfiguration()
        {
            TemplateConfigurationSO template = ScriptableObject.CreateInstance<TemplateConfigurationSO>();
            template.Initialize("New Template", "Custom project template", ProjectType.General);
            
            string path = "Assets/Templates/NewTemplate.asset";
            
            // Ensure Templates directory exists
            string directory = "Assets/Templates";
            if (!AssetDatabase.IsValidFolder(directory))
            {
                AssetDatabase.CreateFolder("Assets", "Templates");
            }
            
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(path);
            
            AssetDatabase.CreateAsset(template, uniquePath);
            AssetDatabase.SaveAssets();
            
            Selection.activeObject = template;
            EditorGUIUtility.PingObject(template);
            
            Debug.Log($"Created Template Configuration at: {uniquePath}");
        }
        
        [MenuItem("Assets/Unity Project Architect/Apply Template", priority = Priority + 103)]
        public static void ApplyTemplate()
        {
            UnityEngine.Object[] selection = Selection.objects;
            if (selection.Length != 1)
            {
                EditorUtility.DisplayDialog("Invalid Selection", "Please select exactly one Template Configuration asset.", "OK");
                return;
            }
            
            TemplateConfigurationSO template = selection[0] as TemplateConfigurationSO;
            if (template == null)
            {
                EditorUtility.DisplayDialog("Invalid Selection", "Please select a Template Configuration asset.", "OK");
                return;
            }
            
            // Validate template before applying
            var validation = UnityProjectArchitect.Unity.Editor.Utilities.TemplateApplicator.ValidateTemplate(template);
            if (!validation.IsValid)
            {
                string errors = string.Join("\n", validation.Errors);
                EditorUtility.DisplayDialog("Template Validation Failed", $"Cannot apply template:\n\n{errors}", "OK");
                return;
            }
            
            // Show warnings if any
            if (validation.Warnings.Count > 0)
            {
                string warnings = string.Join("\n", validation.Warnings);
                bool proceed = EditorUtility.DisplayDialog("Template Warnings", 
                    $"Template has warnings:\n\n{warnings}\n\nDo you want to proceed?", "Apply", "Cancel");
                if (!proceed)
                    return;
            }
            
            // Confirm application
            bool confirmed = EditorUtility.DisplayDialog("Apply Template", 
                $"Apply template '{template.TemplateName}' to the current project?\n\n" +
                $"This will create folders and scenes as defined in the template.", "Apply", "Cancel");
            
            if (confirmed)
            {
                UnityProjectArchitect.Unity.Editor.Utilities.TemplateApplicator.ApplyTemplate(template);
            }
        }
        
        [MenuItem("Assets/Unity Project Architect/Apply Template", validate = true)]
        public static bool ValidateApplyTemplate()
        {
            if (Selection.objects.Length != 1) return false;
            return Selection.objects[0] is TemplateConfigurationSO;
        }
        
        // Help menu items
        [MenuItem("Help/Unity Project Architect/Documentation", priority = Priority)]
        public static void HelpDocumentation()
        {
            OpenDocumentation();
        }
        
        [MenuItem("Help/Unity Project Architect/Report Issue", priority = Priority + 1)]
        public static void ReportIssue()
        {
            Application.OpenURL("https://github.com/UnityProjectArchitect/Issues");
        }
        
        [MenuItem("Help/Unity Project Architect/Feature Request", priority = Priority + 2)]
        public static void FeatureRequest()
        {
            Application.OpenURL("https://github.com/UnityProjectArchitect/Feature-Requests");
        }
        
    }
}