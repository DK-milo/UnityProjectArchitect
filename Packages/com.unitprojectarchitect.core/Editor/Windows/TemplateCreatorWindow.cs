using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityProjectArchitect.Unity;
using UnityProjectArchitect.Core;
using UnityProjectArchitect.Unity.Editor.Utilities;

namespace UnityProjectArchitect.Unity.Editor
{
    /// <summary>
    /// Unity Editor window for creating and managing custom project templates
    /// Provides UI for template configuration, folder structure design, and scene setup
    /// </summary>
    public class TemplateCreatorWindow : EditorWindow
    {
        private VisualElement _rootElement;
        private ScrollView _scrollView;
        private TextField _templateNameField;
        private TextField _templateDescriptionField;
        private DropdownField _projectTypeField;
        private VisualElement _folderStructureContainer;
        private VisualElement _sceneConfigContainer;
        private Button _saveButton;
        private Button _loadButton;
        private Button _previewButton;
        
        private TemplateConfigurationSO _currentTemplate;
        private List<string> _folderPaths = new List<string>();
        private List<SceneTemplateData> _sceneTemplates = new List<SceneTemplateData>();
        
        public static void ShowWindow()
        {
            TemplateCreatorWindow window = GetWindow<TemplateCreatorWindow>();
            window.titleContent = new GUIContent("Template Creator", "Create and manage custom project templates");
            window.minSize = new Vector2(600, 500);
            window.Show();
        }
        
        private void OnEnable()
        {
            _currentTemplate = ScriptableObject.CreateInstance<TemplateConfigurationSO>();
            _currentTemplate.Initialize("New Template", "Custom project template", ProjectType.General);
            
            CreateUI();
        }
        
        private void CreateUI()
        {
            _rootElement = rootVisualElement;
            _rootElement.Clear();
            
            _scrollView = new ScrollView();
            _rootElement.Add(_scrollView);
            
            CreateHeaderSection();
            CreateBasicInfoSection();
            CreateFolderStructureSection();
            CreateSceneConfigSection();
            CreateActionsSection();
            
            RefreshUI();
        }
        
        private void CreateHeaderSection()
        {
            VisualElement headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.justifyContent = Justify.SpaceBetween;
            headerContainer.style.alignItems = Align.Center;
            headerContainer.style.paddingBottom = 15;
            headerContainer.style.paddingTop = 10;
            headerContainer.style.paddingLeft = 10;
            headerContainer.style.paddingRight = 10;
            
            Label titleLabel = new Label("Template Creator");
            titleLabel.style.fontSize = 18;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            Label subtitleLabel = new Label("Design Custom Project Templates");
            subtitleLabel.style.fontSize = 12;
            subtitleLabel.style.color = Color.gray;
            
            VisualElement titleContainer = new VisualElement();
            titleContainer.Add(titleLabel);
            titleContainer.Add(subtitleLabel);
            
            headerContainer.Add(titleContainer);
            
            _scrollView.Add(headerContainer);
            
            HelpBox infoBox = new HelpBox("Create structure-only templates with folders, scenes, and documentation. Templates provide project organization without generating scripts.", HelpBoxMessageType.Info);
            infoBox.style.marginBottom = 15;
            _scrollView.Add(infoBox);
        }
        
        private void CreateBasicInfoSection()
        {
            Foldout basicInfoFoldout = new Foldout { text = "📝 Template Information", value = true };
            basicInfoFoldout.style.marginBottom = 15;
            
            _templateNameField = new TextField("Template Name");
            _templateNameField.value = _currentTemplate.TemplateName;
            _templateNameField.RegisterValueChangedCallback(evt => {
                _currentTemplate.UpdateName(evt.newValue);
                RefreshSaveButton();
            });
            
            _templateDescriptionField = new TextField("Description") { multiline = true };
            _templateDescriptionField.value = _currentTemplate.Description;
            _templateDescriptionField.style.height = 60;
            _templateDescriptionField.RegisterValueChangedCallback(evt => {
                _currentTemplate.UpdateDescription(evt.newValue);
                RefreshSaveButton();
            });
            
            List<string> projectTypeOptions = new List<string>();
            foreach (ProjectType type in Enum.GetValues(typeof(ProjectType)))
            {
                projectTypeOptions.Add(type.ToString());
            }
            
            _projectTypeField = new DropdownField("Project Type", projectTypeOptions, 0);
            _projectTypeField.value = _currentTemplate.Type.ToString();
            _projectTypeField.RegisterValueChangedCallback(evt => {
                if (Enum.TryParse<ProjectType>(evt.newValue, out ProjectType selectedType))
                {
                    _currentTemplate.UpdateType(selectedType);
                    RefreshSaveButton();
                }
            });
            
            basicInfoFoldout.Add(_templateNameField);
            basicInfoFoldout.Add(_templateDescriptionField);
            basicInfoFoldout.Add(_projectTypeField);
            
            _scrollView.Add(basicInfoFoldout);
        }
        
        private void CreateFolderStructureSection()
        {
            Foldout folderFoldout = new Foldout { text = "📁 Folder Structure", value = true };
            folderFoldout.style.marginBottom = 15;
            
            VisualElement headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.justifyContent = Justify.SpaceBetween;
            headerContainer.style.alignItems = Align.Center;
            headerContainer.style.marginBottom = 10;
            
            Label folderLabel = new Label("Folder Paths");
            folderLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            Button addFolderButton = new Button(() => AddFolderPath()) { text = "➕ Add Folder" };
            addFolderButton.style.minWidth = 100;
            
            headerContainer.Add(folderLabel);
            headerContainer.Add(addFolderButton);
            
            _folderStructureContainer = new VisualElement();
            
            folderFoldout.Add(headerContainer);
            folderFoldout.Add(_folderStructureContainer);
            
            // Add some default folders
            if (_folderPaths.Count == 0)
            {
                _folderPaths.Add("Scripts/Managers");
                _folderPaths.Add("Scripts/UI");
                _folderPaths.Add("Art/Textures");
                _folderPaths.Add("Art/Materials");
                _folderPaths.Add("Scenes");
                _folderPaths.Add("Documentation");
            }
            
            _scrollView.Add(folderFoldout);
        }
        
        private void CreateSceneConfigSection()
        {
            Foldout sceneFoldout = new Foldout { text = "🎬 Scene Configuration", value = true };
            sceneFoldout.style.marginBottom = 15;
            
            VisualElement headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.justifyContent = Justify.SpaceBetween;
            headerContainer.style.alignItems = Align.Center;
            headerContainer.style.marginBottom = 10;
            
            Label sceneLabel = new Label("Scene Templates");
            sceneLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            Button addSceneButton = new Button(() => AddSceneTemplate()) { text = "➕ Add Scene" };
            addSceneButton.style.minWidth = 100;
            
            headerContainer.Add(sceneLabel);
            headerContainer.Add(addSceneButton);
            
            _sceneConfigContainer = new VisualElement();
            
            sceneFoldout.Add(headerContainer);
            sceneFoldout.Add(_sceneConfigContainer);
            
            // Add default scene if none exist
            if (_sceneTemplates.Count == 0)
            {
                SceneTemplateData defaultScene = new SceneTemplateData();
                defaultScene.SceneName = "MainScene";
                defaultScene.ScenePath = "Scenes/MainScene.unity";
                defaultScene.IsMainScene = true;
                _sceneTemplates.Add(defaultScene);
            }
            
            _scrollView.Add(sceneFoldout);
        }
        
        private void CreateActionsSection()
        {
            VisualElement actionsContainer = new VisualElement();
            actionsContainer.style.marginBottom = 15;
            actionsContainer.style.paddingTop = 10;
            
            Label actionsLabel = new Label("🔧 Actions");
            actionsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            actionsLabel.style.marginBottom = 10;
            
            VisualElement buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.justifyContent = Justify.SpaceBetween;
            
            _loadButton = new Button(LoadTemplate) { text = "📂 Load Template" };
            _loadButton.style.flexGrow = 1;
            _loadButton.style.marginRight = 5;
            
            _previewButton = new Button(PreviewTemplate) { text = "👁️ Preview" };
            _previewButton.style.flexGrow = 1;
            _previewButton.style.marginRight = 5;
            
            _saveButton = new Button(SaveTemplate) { text = "💾 Save Template" };
            _saveButton.style.flexGrow = 1;
            _saveButton.style.marginRight = 5;
            
            Button applyButton = new Button(ApplyTemplate) { text = "🚀 Apply Template" };
            applyButton.style.flexGrow = 1;
            applyButton.style.backgroundColor = new Color(0.2f, 0.6f, 0.2f, 0.8f);
            
            buttonContainer.Add(_loadButton);
            buttonContainer.Add(_previewButton);
            buttonContainer.Add(_saveButton);
            buttonContainer.Add(applyButton);
            
            actionsContainer.Add(actionsLabel);
            actionsContainer.Add(buttonContainer);
            
            _scrollView.Add(actionsContainer);
        }
        
        private void RefreshUI()
        {
            RefreshFolderStructure();
            RefreshSceneConfiguration();
            RefreshSaveButton();
        }
        
        private void RefreshFolderStructure()
        {
            _folderStructureContainer.Clear();
            
            if (_folderPaths.Count == 0)
            {
                Label emptyLabel = new Label("No folders configured. Click 'Add Folder' to start.");
                emptyLabel.style.color = Color.gray;
                emptyLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                _folderStructureContainer.Add(emptyLabel);
                return;
            }
            
            for (int i = 0; i < _folderPaths.Count; i++)
            {
                CreateFolderPathItem(i);
            }
        }
        
        private void CreateFolderPathItem(int index)
        {
            VisualElement folderItem = new VisualElement();
            folderItem.style.flexDirection = FlexDirection.Row;
            folderItem.style.alignItems = Align.Center;
            folderItem.style.marginBottom = 5;
            
            TextField pathField = new TextField();
            pathField.value = _folderPaths[index];
            pathField.style.flexGrow = 1;
            pathField.style.marginRight = 5;
            pathField.RegisterValueChangedCallback(evt => {
                _folderPaths[index] = evt.newValue;
                UpdateFolderStructure();
                RefreshSaveButton();
            });
            
            Button removeButton = new Button(() => RemoveFolderPath(index)) { text = "🗑️" };
            removeButton.style.minWidth = 30;
            removeButton.style.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.3f);
            
            folderItem.Add(pathField);
            folderItem.Add(removeButton);
            
            _folderStructureContainer.Add(folderItem);
        }
        
        private void RefreshSceneConfiguration()
        {
            _sceneConfigContainer.Clear();
            
            if (_sceneTemplates.Count == 0)
            {
                Label emptyLabel = new Label("No scenes configured. Click 'Add Scene' to start.");
                emptyLabel.style.color = Color.gray;
                emptyLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                _sceneConfigContainer.Add(emptyLabel);
                return;
            }
            
            for (int i = 0; i < _sceneTemplates.Count; i++)
            {
                CreateSceneTemplateItem(i);
            }
        }
        
        private void CreateSceneTemplateItem(int index)
        {
            VisualElement sceneItem = new VisualElement();
            sceneItem.style.marginBottom = 10;
            sceneItem.style.paddingTop = 5;
            sceneItem.style.paddingBottom = 5;
            sceneItem.style.paddingLeft = 5;
            sceneItem.style.paddingRight = 5;
            sceneItem.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
            sceneItem.style.borderTopLeftRadius = 5;
            sceneItem.style.borderTopRightRadius = 5;
            sceneItem.style.borderBottomLeftRadius = 5;
            sceneItem.style.borderBottomRightRadius = 5;
            
            VisualElement headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.marginBottom = 5;
            
            TextField sceneNameField = new TextField("Scene Name");
            sceneNameField.value = _sceneTemplates[index].SceneName;
            sceneNameField.style.flexGrow = 1;
            sceneNameField.style.marginRight = 5;
            sceneNameField.RegisterValueChangedCallback(evt => {
                _sceneTemplates[index].SceneName = evt.newValue;
                RefreshSaveButton();
            });
            
            Toggle defaultToggle = new Toggle("Default");
            defaultToggle.value = _sceneTemplates[index].IsMainScene;
            defaultToggle.style.marginRight = 5;
            defaultToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue)
                {
                    // Only one scene can be default
                    for (int j = 0; j < _sceneTemplates.Count; j++)
                    {
                        _sceneTemplates[j].IsMainScene = (j == index);
                    }
                    RefreshSceneConfiguration();
                }
                else
                {
                    _sceneTemplates[index].IsMainScene = false;
                }
                RefreshSaveButton();
            });
            
            Button removeSceneButton = new Button(() => RemoveSceneTemplate(index)) { text = "🗑️" };
            removeSceneButton.style.minWidth = 30;
            removeSceneButton.style.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.3f);
            
            TextField scenePathField = new TextField("Scene Path");
            scenePathField.value = _sceneTemplates[index].ScenePath;
            scenePathField.RegisterValueChangedCallback(evt => {
                _sceneTemplates[index].ScenePath = evt.newValue;
                RefreshSaveButton();
            });
            
            headerRow.Add(sceneNameField);
            headerRow.Add(defaultToggle);
            headerRow.Add(removeSceneButton);
            
            sceneItem.Add(headerRow);
            sceneItem.Add(scenePathField);
            
            _sceneConfigContainer.Add(sceneItem);
        }
        
        private void AddFolderPath()
        {
            _folderPaths.Add("New Folder Path");
            RefreshFolderStructure();
            RefreshSaveButton();
        }
        
        private void RemoveFolderPath(int index)
        {
            if (index >= 0 && index < _folderPaths.Count)
            {
                _folderPaths.RemoveAt(index);
                RefreshFolderStructure();
                UpdateFolderStructure();
                RefreshSaveButton();
            }
        }
        
        private void AddSceneTemplate()
        {
            SceneTemplateData newScene = new SceneTemplateData();
            newScene.SceneName = "NewScene";
            newScene.ScenePath = "Scenes/NewScene.unity";
            newScene.IsMainScene = false;
            
            _sceneTemplates.Add(newScene);
            RefreshSceneConfiguration();
            RefreshSaveButton();
        }
        
        private void RemoveSceneTemplate(int index)
        {
            if (index >= 0 && index < _sceneTemplates.Count)
            {
                _sceneTemplates.RemoveAt(index);
                RefreshSceneConfiguration();
                RefreshSaveButton();
            }
        }
        
        private void UpdateFolderStructure()
        {
            FolderStructureData folderStructure = new FolderStructureData();
            folderStructure.Folders = _folderPaths.ConvertAll(path => new FolderStructureData.FolderInfo { Path = path });
            folderStructure.Files = new List<FolderStructureData.FileInfo>(); // Structure-only templates don't create files
            
            _currentTemplate.UpdateFolderStructure(folderStructure);
        }
        
        private void RefreshSaveButton()
        {
            bool isValid = !string.IsNullOrEmpty(_currentTemplate.TemplateName) && 
                          !string.IsNullOrEmpty(_currentTemplate.Description);
            
            _saveButton.SetEnabled(isValid);
            
            if (!isValid)
            {
                _saveButton.tooltip = "Template name and description are required";
            }
            else
            {
                _saveButton.tooltip = "Save template configuration";
            }
        }
        
        private void LoadTemplate()
        {
            string path = EditorUtility.OpenFilePanel(
                "Load Template Configuration",
                "Assets/Templates",
                "asset");
            
            if (!string.IsNullOrEmpty(path))
            {
                string relativePath = FileUtil.GetProjectRelativePath(path);
                if (!string.IsNullOrEmpty(relativePath))
                {
                    TemplateConfigurationSO loadedTemplate = AssetDatabase.LoadAssetAtPath<TemplateConfigurationSO>(relativePath);
                    if (loadedTemplate != null)
                    {
                        _currentTemplate = loadedTemplate;
                        
                        // Update UI fields
                        _templateNameField.value = _currentTemplate.TemplateName;
                        _templateDescriptionField.value = _currentTemplate.Description;
                        _projectTypeField.value = _currentTemplate.Type.ToString();
                        
                        // Update folder paths
                        _folderPaths.Clear();
                        if (_currentTemplate.FolderStructure != null && _currentTemplate.FolderStructure.Folders != null)
                        {
                            foreach (var folder in _currentTemplate.FolderStructure.Folders)
                            {
                                _folderPaths.Add(folder.Path);
                            }
                        }
                        
                        // Update scene templates
                        _sceneTemplates.Clear();
                        if (_currentTemplate.SceneTemplates != null)
                        {
                            _sceneTemplates.AddRange(_currentTemplate.SceneTemplates);
                        }
                        
                        RefreshUI();
                        
                        Debug.Log($"Loaded template: {_currentTemplate.TemplateName}");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Load Error", "Failed to load template configuration.", "OK");
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Load Error", "Please select a file within the project.", "OK");
                }
            }
        }
        
        private void PreviewTemplate()
        {
            string preview = GenerateTemplatePreview();
            
            PreviewWindow.ShowPreview("Template Preview", preview);
        }
        
        private string GenerateTemplatePreview()
        {
            System.Text.StringBuilder preview = new System.Text.StringBuilder();
            
            preview.AppendLine($"Template: {_currentTemplate.TemplateName}");
            preview.AppendLine($"Description: {_currentTemplate.Description}");
            preview.AppendLine($"Type: {_currentTemplate.Type}");
            preview.AppendLine();
            
            preview.AppendLine("Folder Structure:");
            foreach (string folder in _folderPaths)
            {
                preview.AppendLine($"  📁 {folder}");
            }
            preview.AppendLine();
            
            preview.AppendLine("Scene Configuration:");
            foreach (SceneTemplateData scene in _sceneTemplates)
            {
                string defaultMarker = scene.IsMainScene ? " (Default)" : "";
                preview.AppendLine($"  🎬 {scene.SceneName}{defaultMarker}");
                preview.AppendLine($"     Path: {scene.ScenePath}");
            }
            
            return preview.ToString();
        }
        
        private void SaveTemplate()
        {
            if (string.IsNullOrEmpty(_currentTemplate.TemplateName))
            {
                EditorUtility.DisplayDialog("Save Error", "Template name is required.", "OK");
                return;
            }
            
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Template Configuration",
                _currentTemplate.TemplateName,
                "asset",
                "Choose location for template configuration");
            
            if (!string.IsNullOrEmpty(path))
            {
                // Create a new ScriptableObject instance for saving to avoid conflicts
                TemplateConfigurationSO templateToSave = ScriptableObject.CreateInstance<TemplateConfigurationSO>();
                templateToSave.Initialize(_currentTemplate.TemplateName, _currentTemplate.Description, _currentTemplate.Type);
                
                // Update template with current data
                UpdateFolderStructure();
                templateToSave.UpdateFolderStructure(_currentTemplate.FolderStructure);
                templateToSave.UpdateSceneTemplates(_sceneTemplates);
                
                AssetDatabase.CreateAsset(templateToSave, path);
                AssetDatabase.SaveAssets();
                
                EditorUtility.DisplayDialog("Template Saved", 
                    $"Template '{templateToSave.TemplateName}' saved successfully to {path}", "OK");
                
                Debug.Log($"Template saved: {templateToSave.TemplateName} at {path}");
                
                // Reset current template to a new instance for the next template creation
                ResetTemplate();
            }
        }
        
        private void ResetTemplate()
        {
            // Create a new template instance for the next template
            _currentTemplate = ScriptableObject.CreateInstance<TemplateConfigurationSO>();
            _currentTemplate.Initialize("New Template", "Custom project template", ProjectType.General);
            
            // Reset UI fields
            _templateNameField.value = _currentTemplate.TemplateName;
            _templateDescriptionField.value = _currentTemplate.Description;
            _projectTypeField.value = _currentTemplate.Type.ToString();
            
            // Reset folder paths to defaults
            _folderPaths.Clear();
            _folderPaths.Add("Scripts/Managers");
            _folderPaths.Add("Scripts/UI");
            _folderPaths.Add("Art/Textures");
            _folderPaths.Add("Art/Materials");
            _folderPaths.Add("Scenes");
            _folderPaths.Add("Documentation");
            
            // Reset scene templates to defaults
            _sceneTemplates.Clear();
            SceneTemplateData defaultScene = new SceneTemplateData();
            defaultScene.SceneName = "MainScene";
            defaultScene.ScenePath = "Scenes/MainScene.unity";
            defaultScene.IsMainScene = true;
            _sceneTemplates.Add(defaultScene);
            
            // Refresh the UI
            RefreshUI();
        }
        
        private void ApplyTemplate()
        {
            // Validate template before applying
            TemplateValidationResult validation = TemplateApplicator.ValidateTemplate(_currentTemplate);
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
            
            // Update template with current data before applying
            UpdateFolderStructure();
            _currentTemplate.UpdateSceneTemplates(_sceneTemplates);
            
            // Confirm application
            bool confirmed = EditorUtility.DisplayDialog("Apply Template", 
                $"Apply template '{_currentTemplate.TemplateName}' to the current project?\n\n" +
                $"This will create {_folderPaths.Count} folders and {_sceneTemplates.Count} scenes as defined in the template.", 
                "Apply", "Cancel");
            
            if (confirmed)
            {
                TemplateApplicator.ApplyTemplate(_currentTemplate);
            }
        }
    }
    
    /// <summary>
    /// Simple preview window for template configuration
    /// </summary>
    public class PreviewWindow : EditorWindow
    {
        private string _content;
        private Vector2 _scrollPosition;
        
        public static void ShowPreview(string title, string content)
        {
            PreviewWindow window = CreateInstance<PreviewWindow>();
            window.titleContent = new GUIContent(title);
            window._content = content;
            window.minSize = new Vector2(400, 300);
            window.ShowUtility();
        }
        
        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorGUILayout.TextArea(_content, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
    }
}