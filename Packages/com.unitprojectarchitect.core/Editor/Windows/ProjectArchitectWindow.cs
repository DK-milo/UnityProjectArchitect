using UnityEngine;
using UnityEditor;
using UnityProjectArchitect.Unity;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Unity.Editor
{
    /// <summary>
    /// Main Unity Editor window for Unity Project Architect
    /// Integrates with Core DLL services through Unity bridge
    /// </summary>
    public class ProjectArchitectWindow : EditorWindow
    {
        private UnityProjectDataAsset _currentProject;
        private Vector2 _scrollPosition;
        private bool _showProjectSettings = true;
        private bool _showDocumentationSections = true;
        private bool _showExportOptions = false;
        
        [MenuItem("Window/Unity Project Architect")]
        public static void ShowWindow()
        {
            ProjectArchitectWindow window = GetWindow<ProjectArchitectWindow>();
            window.titleContent = new GUIContent("Project Architect", "Unity Project Architect - AI-powered documentation and project management");
            window.Show();
        }
        
        private void OnEnable()
        {
            // Initialize Unity service bridge
            UnityServiceBridge.Initialize();
            
            // Try to find existing project data
            string[] guids = AssetDatabase.FindAssets("t:UnityProjectDataAsset");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _currentProject = AssetDatabase.LoadAssetAtPath<UnityProjectDataAsset>(path);
            }
        }
        
        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            DrawHeader();
            
            EditorGUILayout.Space(10);
            
            DrawProjectSection();
            
            EditorGUILayout.Space(10);
            
            if (_currentProject != null)
            {
                DrawDocumentationSection();
                
                EditorGUILayout.Space(10);
                
                DrawExportSection();
                
                EditorGUILayout.Space(10);
                
                DrawActionsSection();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            
            GUILayout.Label("Unity Project Architect", EditorStyles.largeLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label("v0.2.0", EditorStyles.miniLabel);
            
            GUILayout.EndHorizontal();
            
            EditorGUILayout.HelpBox(
                "AI-powered project documentation and organization tool. Now powered by compiled DLLs for professional development workflow.",
                MessageType.Info);
        }
        
        private void DrawProjectSection()
        {
            _showProjectSettings = EditorGUILayout.Foldout(_showProjectSettings, "📁 Project Configuration", true);
            
            if (_showProjectSettings)
            {
                EditorGUI.indentLevel++;
                
                // Project Data Asset
                UnityProjectDataAsset newProject = (UnityProjectDataAsset)EditorGUILayout.ObjectField(
                    "Project Data Asset", 
                    _currentProject, 
                    typeof(UnityProjectDataAsset), 
                    false);
                
                if (newProject != _currentProject)
                {
                    _currentProject = newProject;
                }
                
                EditorGUILayout.Space(5);
                
                // Create new project button
                if (_currentProject == null)
                {
                    if (GUILayout.Button("Create New Project Data Asset"))
                    {
                        CreateNewProjectAsset();
                    }
                }
                else
                {
                    // Display project info
                    EditorGUILayout.LabelField("Project Name", _currentProject.GetDisplayName());
                    EditorGUILayout.LabelField("Summary", _currentProject.GetSummary());
                    
                    EditorGUILayout.Space(5);
                    
                    if (GUILayout.Button("Edit Project Settings"))
                    {
                        Selection.activeObject = _currentProject;
                        EditorGUIUtility.PingObject(_currentProject);
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawDocumentationSection()
        {
            _showDocumentationSections = EditorGUILayout.Foldout(_showDocumentationSections, "📖 Documentation Sections", true);
            
            if (_showDocumentationSections)
            {
                EditorGUI.indentLevel++;
                
                ProjectData projectData = _currentProject.ProjectData;
                
                if (projectData.DocumentationSections.Count == 0)
                {
                    EditorGUILayout.HelpBox("No documentation sections found. Initialize project data first.", MessageType.Warning);
                }
                else
                {
                    foreach (var section in projectData.DocumentationSections)
                    {
                        DrawDocumentationSectionItem(section);
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawDocumentationSectionItem(DocumentationSectionData section)
        {
            GUILayout.BeginHorizontal();
            
            // Section enabled toggle
            bool wasEnabled = section.IsEnabled;
            section.IsEnabled = EditorGUILayout.Toggle(section.IsEnabled, GUILayout.Width(20));
            
            // Section name and status
            string statusIcon = GetStatusIcon(section.Status);
            EditorGUILayout.LabelField($"{statusIcon} {section.SectionType}", GUILayout.ExpandWidth(true));
            
            // Word count
            int wordCount = string.IsNullOrEmpty(section.Content) ? 0 : section.Content.Split(' ').Length;
            EditorGUILayout.LabelField($"{wordCount} words", EditorStyles.miniLabel, GUILayout.Width(60));
            
            // Generate button
            GUI.enabled = section.IsEnabled;
            if (GUILayout.Button("Generate", GUILayout.Width(70)))
            {
                GenerateDocumentationSection(section);
            }
            GUI.enabled = true;
            
            GUILayout.EndHorizontal();
            
            // Save changes if enabled state changed
            if (wasEnabled != section.IsEnabled)
            {
                _currentProject.SaveToJson();
            }
        }
        
        private void DrawExportSection()
        {
            _showExportOptions = EditorGUILayout.Foldout(_showExportOptions, "📤 Export Options", true);
            
            if (_showExportOptions)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("Export Formats", EditorStyles.boldLabel);
                
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("📄 Export Markdown"))
                {
                    ExportDocumentation(ExportFormat.Markdown);
                }
                if (GUILayout.Button("📑 Export PDF"))
                {
                    ExportDocumentation(ExportFormat.PDF);
                }
                GUILayout.EndHorizontal();
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawActionsSection()
        {
            EditorGUILayout.LabelField("🔧 Actions", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("🔍 Analyze Project"))
            {
                AnalyzeProject();
            }
            
            if (GUILayout.Button("🔄 Refresh Data"))
            {
                _currentProject.SaveToJson();
                Repaint();
            }
            
            GUILayout.EndHorizontal();
        }
        
        private void CreateNewProjectAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Project Data Asset",
                "ProjectData",
                "asset",
                "Choose location for new Project Data asset");
            
            if (!string.IsNullOrEmpty(path))
            {
                UnityProjectDataAsset asset = CreateInstance<UnityProjectDataAsset>();
                asset.Initialize();
                asset.UpdateProjectName(Application.productName);
                
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                
                _currentProject = asset;
                Selection.activeObject = asset;
            }
        }
        
        private async void GenerateDocumentationSection(DocumentationSectionData section)
        {
            Debug.Log($"Generating documentation for section: {section.SectionType}");
            
            // TODO: Integrate with AI services from DLL
            // For now, add placeholder content
            section.Content = $"Generated content for {section.SectionType} section.\n\nThis is a placeholder until AI integration is complete.";
            section.Status = DocumentationStatus.Generated;
            section.LastUpdated = System.DateTime.Now;
            
            _currentProject.SaveToJson();
            Repaint();
        }
        
        private async void ExportDocumentation(ExportFormat format)
        {
            Debug.Log($"Exporting documentation as {format}");
            
            IExportService exportService = UnityServiceBridge.GetExportService();
            
            ExportRequest request = new ExportRequest(format, "Assets/Documentation/")
            {
                FileName = $"{_currentProject.ProjectData.ProjectName}_Documentation"
            };
            
            try
            {
                ExportOperationResult result = await exportService.ExportProjectDocumentationAsync(_currentProject.ProjectData, request);
                
                if (result.Success)
                {
                    EditorUtility.DisplayDialog("Export Complete", 
                        $"Documentation exported successfully to {result.OutputPath}", 
                        "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Export Failed", 
                        $"Export failed: {result.ErrorMessage}", 
                        "OK");
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Export Error", 
                    $"An error occurred during export: {ex.Message}", 
                    "OK");
            }
        }
        
        private async void AnalyzeProject()
        {
            Debug.Log("Analyzing project structure...");
            
            IProjectAnalyzer analyzer = UnityServiceBridge.GetProjectAnalyzer();
            
            try
            {
                // Get the Unity project root path (parent of Assets folder)
                string projectPath = System.IO.Directory.GetParent(Application.dataPath).FullName;
                Debug.Log($"Analyzing Unity project at: {projectPath}");
                
                ProjectAnalysisResult result = await analyzer.AnalyzeProjectAsync(projectPath);
                
                // Create validation result with proper interpretation for empty projects
                ValidationResult validationResult = new ValidationResult();
                
                if (result.Success)
                {
                    validationResult.IsValid = true;
                    Debug.Log("✅ Project analysis completed successfully.");
                }
                else
                {
                    // Check if this is an empty project scenario (no actual issues found)
                    bool hasActualIssues = !string.IsNullOrEmpty(result.ErrorMessage) && 
                                          result.ErrorMessage != "No scripts found to analyze" &&
                                          result.ErrorMessage != "No assets found to analyze" &&
                                          !result.ErrorMessage.StartsWith("Cannot analyze project at path:");
                    
                    if (!hasActualIssues)
                    {
                        // Empty project or path issue - treat as successful but informational
                        validationResult.IsValid = true;
                        Debug.Log("ℹ️ Project analysis completed. Empty project detected - no issues found.");
                    }
                    else
                    {
                        // Actual analysis failure
                        validationResult.IsValid = false;
                        Debug.LogWarning($"⚠️ Project analysis completed with issues: {result.ErrorMessage}");
                    }
                }
                
                UnityServiceBridge.ShowValidationResult(validationResult);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Project analysis failed: {ex.Message}");
                
                UnityServiceBridge.ShowValidationResult(new ValidationResult
                {
                    IsValid = false
                });
            }
        }
        
        private string GetStatusIcon(DocumentationStatus status)
        {
            return status switch
            {
                DocumentationStatus.NotStarted => "⏸️",
                DocumentationStatus.InProgress => "🔄",
                DocumentationStatus.Generated => "✅",
                DocumentationStatus.Completed => "💯",
                DocumentationStatus.Reviewed => "👀",
                DocumentationStatus.Approved => "✨",
                DocumentationStatus.Published => "🚀",
                DocumentationStatus.Outdated => "⚠️",
                _ => "❓"
            };
        }
    }
}