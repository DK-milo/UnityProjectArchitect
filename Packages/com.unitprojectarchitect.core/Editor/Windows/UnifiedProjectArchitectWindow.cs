using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityProjectArchitect.Unity;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Unity.Editor
{
    /// <summary>
    /// Unified Unity Project Architect Studio window with three main tabs:
    /// 1. Game Concept Studio - Transform ideas into documentation and project structure
    /// 2. Project Analyzer - Analyze existing projects and generate documentation  
    /// 3. Smart Template Creator - Create intelligent project templates
    /// </summary>
    public class UnifiedProjectArchitectWindow : EditorWindow
    {
        private VisualElement _rootElement;
        private VisualElement _tabButtonsContainer;
        private VisualElement _contentContainer;
        
        // Tab content containers
        private VisualElement _overviewTab;
        private VisualElement _gameConceptTab;
        private VisualElement _projectAnalyzerTab;
        private VisualElement _templateCreatorTab;
        
        // Current active tab
        private int _activeTab = 0;
        private readonly string[] _tabNames = { "Overview & Tutorial", "Game Concept Studio", "Project Analyzer", "Smart Template Creator" };
        private readonly string[] _tabIcons = { "📘", "🎮", "🔍", "📁" };
        
        // Game Concept Studio components
        private TextField _gameDescriptionField;
        private TextField _apiKeyField;
        private Button _generateDocsButton;
        private Button _exportDocsButton;
        private Button _createStructureButton;
        // Removed sticky action bar
        private Label _statusLabel;
        private VisualElement _documentationResults;
        private UnityProjectDataAsset _conceptProject;
        private Toggle _forceOfflineToggle;
        private Label _modeDescriptionLabel;
        
        // Project Analyzer components
        private UnityProjectDataAsset _currentProjectAsset;
        private VisualElement _projectAnalysisContainer;
        private ObjectField _projectField;
        private VisualElement _documentationContainer;
        private ProgressBar _analysisProgress;
        private Label _projectStatusLabel;
        
        // Template Creator components
        private TextField _templateNameField;
        private TextField _templateDescriptionField;
        private EnumField _projectTypeField;
        private VisualElement _folderStructureContainer;
        private VisualElement _suggestedFoldersContainer;
        private Button _saveTemplateButton;
        private Button _previewTemplateButton;
        private Label _templateStatusLabel;
        private List<string> _customFolders;
        private TemplateConfigurationSO _currentTemplate;
        private bool _compactMode;
        private int _baseFontSize;
        private string _textSizeSetting;
        private string _currentSearchTerm = "";
        private TwoPaneSplitView _splitView;
        private bool _isStyleSheetAttached;
        
        public static void ShowWindow()
        {
            UnifiedProjectArchitectWindow window = GetWindow<UnifiedProjectArchitectWindow>();
            window.titleContent = new GUIContent("Unity Project Architect Studio", "Unified tool for game documentation and project management");
            window.minSize = new Vector2(900, 600);
            window.Show();
        }
        
        private void OnEnable()
        {
            // Initialize Unity service bridge
            UnityServiceBridge.Initialize();
            _compactMode = EditorPrefs.GetBool("UnityProjectArchitect.CompactMode", false);
            _textSizeSetting = EditorPrefs.GetString("UnityProjectArchitect.TextSize", "Normal");
            _baseFontSize = GetBaseFontSize(_textSizeSetting);
            TryAttachStyleSheet();
            CreateUI();
        }
        
        private void CreateUI()
        {
            _rootElement = rootVisualElement;
            _rootElement.Clear();
            _baseFontSize = GetBaseFontSize(_textSizeSetting);
            TryAttachStyleSheet();
            
            // Add top header bar for modern look and quick status
            VisualElement topHeader = CreateTopHeaderBar();
            if (topHeader != null) topHeader.AddToClassList("upa-topbar");
            _rootElement.Add(topHeader);

            // Toolbar (search and quick actions)
            VisualElement toolbar = CreateToolbar();
            if (toolbar != null) toolbar.AddToClassList("upa-toolbar");
            _rootElement.Add(toolbar);
            
            // Create left side tab buttons
            CreateTabNavigation();
            
            // Create right side content area
            CreateContentArea();
            
            // Create resizable split view (left nav / content)
            _splitView = new TwoPaneSplitView(0, 220, TwoPaneSplitViewOrientation.Horizontal)
            {
                style = { flexGrow = 1, marginTop = 6 }
            };
            _splitView.Add(_tabButtonsContainer);
            _splitView.Add(_contentContainer);

            _rootElement.Add(_splitView);
            
            // Initialize tabs
            CreateAllTabs();
            SwitchToTab(0);
        }

        private void TryAttachStyleSheet()
        {
            if (_rootElement == null) return;
            string ussPath = "Packages/com.unitprojectarchitect.core/Editor/Windows/UnifiedProjectArchitectWindow.uss";
            StyleSheet ss = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
            if (ss != null && !_isStyleSheetAttached)
            {
                _rootElement.styleSheets.Add(ss);
                _isStyleSheetAttached = true;
            }
            // Always update the text size class on the root
            _rootElement.RemoveFromClassList("text-small");
            _rootElement.RemoveFromClassList("text-normal");
            _rootElement.RemoveFromClassList("text-large");
            _rootElement.RemoveFromClassList("text-xlarge");
            string cls = _textSizeSetting switch
            {
                "Small" => "text-small",
                "Large" => "text-large",
                "ExtraLarge" => "text-xlarge",
                _ => "text-normal"
            };
            _rootElement.AddToClassList(cls);
        }

        private enum TextSizeOption
        {
            Small,
            Normal,
            Large,
            ExtraLarge
        }

        private int GetBaseFontSize(string setting)
        {
            switch (setting)
            {
                case "Small":
                    return 12;
                case "Large":
                    return 16;
                case "ExtraLarge":
                    return 18;
                case "Normal":
                default:
                    return 14;
            }
        }

        private TextSizeOption TextSizeFromString(string setting)
        {
            return setting switch
            {
                "Small" => TextSizeOption.Small,
                "Large" => TextSizeOption.Large,
                "ExtraLarge" => TextSizeOption.ExtraLarge,
                _ => TextSizeOption.Normal
            };
        }
        
        private void CreateTabNavigation()
        {
            _tabButtonsContainer = new VisualElement();
            _tabButtonsContainer.AddToClassList("upa-sidebar");
            _tabButtonsContainer.style.fontSize = _baseFontSize;
            
            // Header
            Label headerLabel = new Label("Unity Project Architect Studio");
            headerLabel.AddToClassList("upa-fs-base");
            headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            headerLabel.style.color = Color.white;
            headerLabel.style.marginBottom = 20;
            headerLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _tabButtonsContainer.Add(headerLabel);
            
            // Create tab buttons
            for (int i = 0; i < _tabNames.Length; i++)
            {
                Button tabButton = CreateTabButton(i, _tabIcons[i], _tabNames[i]);
                _tabButtonsContainer.Add(tabButton);
            }
        }
        
        private Button CreateTabButton(int tabIndex, string icon, string tabName)
        {
            Button tabButton = new Button(() => SwitchToTab(tabIndex));
            tabButton.AddToClassList("upa-tabbtn");
            tabButton.style.height = _compactMode ? 44 : 56;
            tabButton.style.fontSize = _baseFontSize - 2;
            
            // Create button content with icon and text
            VisualElement buttonContent = new VisualElement();
            buttonContent.AddToClassList("upa-row");
            
            Label iconLabel = new Label(icon);
            iconLabel.style.fontSize = 16;
            iconLabel.style.marginRight = 8;
            
            Label textLabel = new Label(tabName);
            textLabel.style.fontSize = _baseFontSize;
            textLabel.style.whiteSpace = WhiteSpace.Normal;
            
            buttonContent.Add(iconLabel);
            buttonContent.Add(textLabel);
            tabButton.Add(buttonContent);
            
            // Subtle hover interactions on non-active tabs
            tabButton.RegisterCallback<MouseEnterEvent>(_ => {
                if (tabIndex != _activeTab)
                {
                    tabButton.style.backgroundColor = new Color(0.24f, 0.24f, 0.24f, 1f);
                }
            });
            tabButton.RegisterCallback<MouseLeaveEvent>(_ => {
                if (tabIndex != _activeTab)
                {
                    tabButton.style.backgroundColor = new Color(0.20f, 0.20f, 0.20f, 1f);
                }
            });
            
            return tabButton;
        }
        
        private void CreateContentArea()
        {
            _contentContainer = new VisualElement();
            _contentContainer.style.flexGrow = 1;
            _contentContainer.AddToClassList("upa-content");
            _contentContainer.style.fontSize = _baseFontSize;

            // Make header sticky by using a separate container (already added in each tab before the ScrollView)
        }

        // Sticky bar helper removed
        
        private void CreateAllTabs()
        {
            CreateOverviewTab();
            CreateGameConceptStudioTab();
            CreateProjectAnalyzerTab();
            CreateSmartTemplateCreatorTab();
        }

        private void CreateOverviewTab()
        {
            _overviewTab = new VisualElement();

            ScrollView scrollView = new ScrollView();
            _overviewTab.Add(scrollView);

            CreateTabHeader(scrollView, "📘 Overview & Tutorial", "Welcome! This guide explains how to get the most out of Unity Project Architect Studio.");
            AddHeaderDivider(scrollView);

            // Quick intro card
            VisualElement introCard = new VisualElement();
            introCard.AddToClassList("upa-card");

            Label introTitle = new Label("What is this studio?");
            introTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            introTitle.style.fontSize = _baseFontSize + 2;
            introTitle.style.marginBottom = 4;

            Label introText = new Label("It’s a unified workspace to design game concepts, analyze projects, and generate/export professional documentation with optional AI.");
            introText.style.whiteSpace = WhiteSpace.Normal;
            introText.style.fontSize = _baseFontSize - 1;

            introCard.Add(introTitle);
            introCard.Add(introText);
            scrollView.Add(introCard);

            // Sections for each tab
            scrollView.Add(CreateOverviewSection(
                "🎮 Game Concept Studio",
                "Describe your game idea, then generate documentation (General Description, User Stories, Work Tickets, System Architecture). You can export and scaffold a project structure.",
                "Go to Game Concept Studio",
                () => SwitchToTab(1)
            ));

            scrollView.Add(CreateOverviewSection(
                "🔍 Project Analyzer",
                "Load an existing Unity project’s data asset, analyze its structure, and generate documentation such as System Architecture. Export as Markdown/PDF.",
                "Go to Project Analyzer",
                () => SwitchToTab(2)
            ));

            scrollView.Add(CreateOverviewSection(
                "📁 Smart Template Creator",
                "Create reusable templates with smart folder suggestions based on project type. Customize folders and quickly scaffold them in your project.",
                "Go to Template Creator",
                () => SwitchToTab(3)
            ));
        }

        private VisualElement CreateOverviewSection(string title, string body, string ctaText, System.Action onClick)
        {
            VisualElement card = new VisualElement();
            card.AddToClassList("upa-card");

            Label titleLabel = new Label(title);
            titleLabel.AddToClassList("upa-fs-title");
            titleLabel.style.marginBottom = 2;

            Label bodyLabel = new Label(body);
            bodyLabel.AddToClassList("upa-fs-base");
            bodyLabel.style.whiteSpace = WhiteSpace.Normal;
            bodyLabel.style.marginBottom = 6;

            Button cta = new Button(onClick) { text = ctaText };
            cta.style.height = 28;
            cta.style.fontSize = _baseFontSize - 1;

            card.Add(titleLabel);
            card.Add(bodyLabel);
            card.Add(cta);
            return card;
        }

        private VisualElement CreateToolbar()
        {
            Toolbar toolbar = new Toolbar();
            // Styling via USS

            Label spacer = new Label("");
            spacer.style.flexGrow = 1;

            ToolbarSearchField searchField = new ToolbarSearchField();
            searchField.value = _currentSearchTerm;
            searchField.style.minWidth = 220;
            searchField.RegisterValueChangedCallback(evt => {
                _currentSearchTerm = evt.newValue ?? string.Empty;
                ApplySearchFilter(_currentSearchTerm);
            });

            toolbar.Add(spacer);
            toolbar.Add(searchField);
            return toolbar;
        }

        private void ApplySearchFilter(string term)
        {
            term = (term ?? string.Empty).Trim();

            VisualElement activeRoot = _activeTab switch
            {
                0 => _overviewTab,
                1 => _gameConceptTab,
                2 => _projectAnalyzerTab,
                3 => _templateCreatorTab,
                _ => null
            };

            if (activeRoot == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(term))
            {
                foreach (VisualElement child in activeRoot.Children())
                {
                    child.style.display = DisplayStyle.Flex;
                }
                return;
            }

            string lower = term.ToLowerInvariant();
            foreach (VisualElement child in activeRoot.Children())
            {
                bool matches = ElementMatchesSearch(child, lower);
                child.style.display = matches ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private bool ElementMatchesSearch(VisualElement element, string lower)
        {
            if (element == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(element.name) && element.name.IndexOf(lower, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (element is Label lbl && !string.IsNullOrEmpty(lbl.text) && lbl.text.IndexOf(lower, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (element is TextField tf && !string.IsNullOrEmpty(tf.value) && tf.value.IndexOf(lower, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (element is Foldout fo && !string.IsNullOrEmpty(fo.text) && fo.text.IndexOf(lower, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            foreach (VisualElement child in element.Children())
            {
                if (ElementMatchesSearch(child, lower))
                {
                    return true;
                }
            }
            return false;
        }
        
        private void CreateGameConceptStudioTab()
        {
            _gameConceptTab = new VisualElement();
            
            // Sticky action bar removed
            
            ScrollView scrollView = new ScrollView();
            _gameConceptTab.Add(scrollView);
            
            // Header
            CreateTabHeader(scrollView, "🎮 Game Concept Studio", 
                "Transform your game ideas into professional documentation and project structure");
            AddHeaderDivider(scrollView);
            
            // AI Configuration
            CreateAIConfigurationSection(scrollView);

            // Step 1: Game Description
            CreateGameDescriptionSection(scrollView);
            
            // Step 2: Generate Documentation
            CreateDocumentationGenerationSection(scrollView);
            
            // Step 3: Export & Structure Creation
            CreateExportAndStructureSection(scrollView);
            
            // Results
            CreateDocumentationResultsSection(scrollView);
        }
        
        private void CreateProjectAnalyzerTab()
        {
            _projectAnalyzerTab = new VisualElement();
            
            // Sticky action bar removed
            
            ScrollView scrollView = new ScrollView();
            _projectAnalyzerTab.Add(scrollView);
            
            // Header
            CreateTabHeader(scrollView, "🔍 Project Analyzer", 
                "Analyze your existing Unity project and generate comprehensive documentation");
            AddHeaderDivider(scrollView);
            
            // Project Configuration Section
            CreateProjectConfigurationSection(scrollView);
            
            // Documentation Generation Section
            CreateProjectDocumentationSection(scrollView);
            
            // Export Options Section  
            CreateProjectExportSection(scrollView);
            
            // Analysis Actions Section
            CreateAnalysisActionsSection(scrollView);
            
            // Initialize with existing project data if available
            InitializeProjectAnalyzer();
        }
        
        private void CreateSmartTemplateCreatorTab()
        {
            _templateCreatorTab = new VisualElement();
            
            // Sticky action bar removed
            
            ScrollView scrollView = new ScrollView();
            _templateCreatorTab.Add(scrollView);
            
            // Header
            CreateTabHeader(scrollView, "📁 Smart Template Creator", 
                "Create intelligent project templates with automatic folder structure suggestions");
            AddHeaderDivider(scrollView);
            
            // Initialize custom folders list
            _customFolders = new List<string>();
            
            // Template Basic Information
            CreateTemplateInfoSection(scrollView);
            
            // Project Type Selection with Smart Suggestions
            CreateProjectTypeSection(scrollView);
            
            // Smart Folder Suggestions
            CreateSmartSuggestionsSection(scrollView);
            
            // Custom Folder Structure
            CreateCustomFolderSection(scrollView);
            
            // Template Actions
            CreateTemplateActionsSection(scrollView);
        }
        
        private void CreateTabHeader(VisualElement parent, string title, string description)
        {
            VisualElement headerContainer = new VisualElement();
            headerContainer.AddToClassList("upa-card");
            
            Label titleLabel = new Label(title);
            titleLabel.AddToClassList("upa-fs-title");
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 5;
            
            Label descLabel = new Label(description);
            descLabel.AddToClassList("upa-fs-subtitle");
            descLabel.style.color = Color.gray;
            descLabel.style.whiteSpace = WhiteSpace.Normal;
            
            headerContainer.Add(titleLabel);
            headerContainer.Add(descLabel);
            parent.Add(headerContainer);
        }

        private void AddHeaderDivider(VisualElement parent)
        {
            VisualElement divider = new VisualElement();
            divider.AddToClassList("upa-divider");
            parent.Add(divider);
        }
        
        private void CreateGameDescriptionSection(ScrollView parent)
        {
            Foldout descriptionFoldout = new Foldout { text = "Step 1: Describe Your Game Concept", value = true };
            StyleFoldout(descriptionFoldout);
            
            Label instructionLabel = new Label("Describe your game idea in detail (use the template for best results):");
            instructionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            instructionLabel.style.marginBottom = 5;
            
            // Add helpful hint label
            Label hintLabel = new Label("💡 Tip: Click 'Use Template' for the optimal format, or 'Use Example' to see a complete sample");
            hintLabel.style.fontSize = _baseFontSize - 3;
            hintLabel.style.color = Color.cyan;
            hintLabel.style.marginBottom = 8;
            hintLabel.style.whiteSpace = WhiteSpace.Normal;
            
            // Create a ScrollView for the text field with proper scroll functionality
            ScrollView textFieldScrollView = new ScrollView();
            textFieldScrollView.style.height = 200;
            textFieldScrollView.style.marginBottom = 10;
            textFieldScrollView.style.borderTopLeftRadius = 3;
            textFieldScrollView.style.borderTopRightRadius = 3;
            textFieldScrollView.style.borderBottomLeftRadius = 3;
            textFieldScrollView.style.borderBottomRightRadius = 3;
            textFieldScrollView.style.borderLeftWidth = 1;
            textFieldScrollView.style.borderRightWidth = 1;
            textFieldScrollView.style.borderTopWidth = 1;
            textFieldScrollView.style.borderBottomWidth = 1;
            textFieldScrollView.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            textFieldScrollView.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            textFieldScrollView.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            textFieldScrollView.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            
            _gameDescriptionField = new TextField()
            {
                multiline = true,
                value = GetExampleGameDescription()
            };
            _gameDescriptionField.style.flexGrow = 1;
            _gameDescriptionField.style.flexShrink = 1;
            _gameDescriptionField.style.whiteSpace = WhiteSpace.Normal;
            _gameDescriptionField.style.unityTextAlign = TextAnchor.UpperLeft;
            _gameDescriptionField.style.minHeight = 180; // Ensure minimum height for scrolling
            _gameDescriptionField.RegisterValueChangedCallback(_ => {
                UpdateGenerateButtonState();
            });
            
            textFieldScrollView.Add(_gameDescriptionField);
            
            VisualElement buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.justifyContent = Justify.FlexEnd;
            
            Button templateButton = new Button(() => {
                _gameDescriptionField.value = GetGameDescriptionTemplate();
            }) { text = "📋 Use Template" };
            
            Button exampleButton = new Button(() => {
                _gameDescriptionField.value = GetExampleGameDescription();
            }) { text = "📝 Use Example" };
            exampleButton.style.marginLeft = 5;
            
            Button clearButton = new Button(() => {
                _gameDescriptionField.value = "";
            }) { text = "🗑️ Clear" };
            clearButton.style.marginLeft = 5;
            
            buttonContainer.Add(templateButton);
            buttonContainer.Add(exampleButton);
            buttonContainer.Add(clearButton);
            
            descriptionFoldout.Add(instructionLabel);
            descriptionFoldout.Add(hintLabel);
            descriptionFoldout.Add(textFieldScrollView);
            descriptionFoldout.Add(buttonContainer);
            
            parent.Add(descriptionFoldout);
        }
        
        private void CreateAIConfigurationSection(ScrollView parent)
        {
            Foldout configFoldout = new Foldout { text = "AI Configuration (Optional)", value = false };
            StyleFoldout(configFoldout);
            
            Label keyLabel = new Label("Claude API Key (uses intelligent fallbacks if not provided):");
            keyLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            keyLabel.style.marginBottom = 5;
            
            // Create a container for the API key field with better styling
            VisualElement apiKeyContainer = new VisualElement();
            apiKeyContainer.style.marginBottom = 10;
            apiKeyContainer.style.borderTopLeftRadius = 3;
            apiKeyContainer.style.borderTopRightRadius = 3;
            apiKeyContainer.style.borderBottomLeftRadius = 3;
            apiKeyContainer.style.borderBottomRightRadius = 3;
            apiKeyContainer.style.borderLeftWidth = 1;
            apiKeyContainer.style.borderRightWidth = 1;
            apiKeyContainer.style.borderTopWidth = 1;
            apiKeyContainer.style.borderBottomWidth = 1;
            apiKeyContainer.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            apiKeyContainer.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            apiKeyContainer.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            apiKeyContainer.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            
            _apiKeyField = new TextField()
            {
                isPasswordField = true,
                value = EditorPrefs.GetString("UnityProjectArchitect.ClaudeAPIKey", "")
            };
            _apiKeyField.style.flexGrow = 1;
            _apiKeyField.style.fontSize = _baseFontSize - 1;
            
            _apiKeyField.RegisterValueChangedCallback(evt => {
                EditorPrefs.SetString("UnityProjectArchitect.ClaudeAPIKey", evt.newValue);
                UpdateGenerationModeDisplay();
            });
            
            apiKeyContainer.Add(_apiKeyField);
            
            // Add status indicator for API key
            Label statusLabel = new Label("🔑 API Key Status: " + (string.IsNullOrEmpty(_apiKeyField.value) ? "Not configured (will use fallback)" : "Configured"));
            statusLabel.style.fontSize = _baseFontSize - 3;
            statusLabel.style.color = string.IsNullOrEmpty(_apiKeyField.value) ? new Color(1f, 0.5f, 0f, 1f) : Color.green;
            statusLabel.style.marginTop = 5;
            
            // Force Offline Mode toggle
            _forceOfflineToggle = new Toggle("Force Offline Mode")
            {
                value = EditorPrefs.GetBool("UnityProjectArchitect.ForceOffline", false)
            };
            _forceOfflineToggle.style.marginTop = 10;
            _forceOfflineToggle.style.fontSize = _baseFontSize - 1;
            
            _forceOfflineToggle.RegisterValueChangedCallback(evt => {
                EditorPrefs.SetBool("UnityProjectArchitect.ForceOffline", evt.newValue);
                UpdateGenerationModeDisplay();
            });
            
            // Mode description label
            _modeDescriptionLabel = new Label();
            _modeDescriptionLabel.style.fontSize = _baseFontSize - 3;
            _modeDescriptionLabel.style.marginTop = 5;
            _modeDescriptionLabel.style.whiteSpace = WhiteSpace.Normal;
            
            configFoldout.Add(keyLabel);
            configFoldout.Add(apiKeyContainer);
            configFoldout.Add(statusLabel);
            configFoldout.Add(_forceOfflineToggle);
            configFoldout.Add(_modeDescriptionLabel);
            
            // Initialize mode description
            UpdateGenerationModeDisplay();
            
            parent.Add(configFoldout);
        }
        
        /// <summary>
        /// Updates the generation mode display based on API key and force offline toggle
        /// </summary>
        private void UpdateGenerationModeDisplay()
        {
            if (_modeDescriptionLabel == null) return;
            
            bool hasApiKey = !string.IsNullOrEmpty(EditorPrefs.GetString("UnityProjectArchitect.ClaudeAPIKey", ""));
            bool forceOffline = EditorPrefs.GetBool("UnityProjectArchitect.ForceOffline", false);
            
            if (forceOffline)
            {
                _modeDescriptionLabel.text = "🔸 Mode: Offline - Using built-in templates and structured generation";
                _modeDescriptionLabel.style.color = new Color(0.60f, 0.35f, 0.05f, 1f);
            }
            else if (hasApiKey)
            {
                _modeDescriptionLabel.text = "🔸 Mode: AI-Enhanced - Using Claude AI for advanced content generation";
                _modeDescriptionLabel.style.color = new Color(0.15f, 0.40f, 0.15f, 1f);
            }
            else
            {
                _modeDescriptionLabel.text = "🔸 Mode: Smart Fallback - Will use AI if available, otherwise offline templates";
                _modeDescriptionLabel.style.color = new Color(0.40f, 0.40f, 0.40f, 1f);
            }
        }
        
        private void CreateDocumentationGenerationSection(ScrollView parent)
        {
            Foldout generationFoldout = new Foldout { text = "Step 2: Generate Documentation", value = true };
            StyleFoldout(generationFoldout);
            
            _generateDocsButton = new Button(GenerateDocumentationFromConcept) { text = "✨ Generate Professional Documentation" };
            _generateDocsButton.AddToClassList("upa-btn");
            _generateDocsButton.AddToClassList("upa-btn-lg");
            _generateDocsButton.AddToClassList("upa-btn-primary");
            
            _statusLabel = new Label("Ready to generate documentation from your game concept");
            _statusLabel.AddToClassList("upa-fs-base");
            _statusLabel.AddToClassList("upa-mt-10");
            
            generationFoldout.Add(_generateDocsButton);
            generationFoldout.Add(_statusLabel);
            
            parent.Add(generationFoldout);
            
            // Ensure correct initial state based on current description content
            UpdateGenerateButtonState();
        }

        private void UpdateGenerateButtonState()
        {
            if (_generateDocsButton == null) return;
            string text = _gameDescriptionField != null ? _gameDescriptionField.value : string.Empty;
            bool hasContent = !string.IsNullOrWhiteSpace(text) && text.Trim().Length >= 10;
            _generateDocsButton.SetEnabled(hasContent);
        }
        
        private void CreateExportAndStructureSection(ScrollView parent)
        {
            Foldout exportFoldout = new Foldout { text = "Step 3: Export Documentation & Create Project Structure", value = true };
            StyleFoldout(exportFoldout);
            
            Label instructionLabel = new Label("Choose what to do with your generated documentation:");
            instructionLabel.AddToClassList("upa-fs-base");
            instructionLabel.AddToClassList("upa-mb-10");
            
            VisualElement buttonContainer = new VisualElement();
            buttonContainer.AddToClassList("upa-row");
            // Normalize vertical padding
            buttonContainer.style.marginTop = 0;
            buttonContainer.style.marginBottom = 0;
            
            _exportDocsButton = new Button(ExportDocumentation) { text = "📄 Export Documentation" };
            _exportDocsButton.AddToClassList("upa-btn");
            _exportDocsButton.AddToClassList("upa-btn-md");
            _exportDocsButton.AddToClassList("upa-btn-normalize");
            _exportDocsButton.AddToClassList("upa-mr-5");
            _exportDocsButton.style.flexGrow = 1;
            _exportDocsButton.SetEnabled(false);
            
            _createStructureButton = new Button(CreateProjectStructure) { text = "📁 Create Project Structure" };
            _createStructureButton.AddToClassList("upa-btn");
            _createStructureButton.AddToClassList("upa-btn-md");
            _createStructureButton.AddToClassList("upa-btn-normalize");
            _createStructureButton.style.flexGrow = 1;
            _createStructureButton.SetEnabled(false);
            
            buttonContainer.Add(_exportDocsButton);
            buttonContainer.Add(_createStructureButton);
            
            exportFoldout.Add(instructionLabel);
            exportFoldout.Add(buttonContainer);
            
            parent.Add(exportFoldout);
        }
        
        private void CreateDocumentationResultsSection(ScrollView parent)
        {
            Foldout resultsFoldout = new Foldout { text = "Generated Documentation", value = true };
            StyleFoldout(resultsFoldout);
            
            _documentationResults = new VisualElement();
            _documentationResults.style.marginTop = 10;
            
            resultsFoldout.Add(_documentationResults);
            parent.Add(resultsFoldout);
        }
        
        private void SwitchToTab(int tabIndex)
        {
            _activeTab = tabIndex;
            
            // Clear content container
            _contentContainer.Clear();
            
            // Add appropriate tab content
            switch (tabIndex)
            {
                case 0:
                    _contentContainer.Add(_overviewTab);
                    break;
                case 1:
                    _contentContainer.Add(_gameConceptTab);
                    break;
                case 2:
                    _contentContainer.Add(_projectAnalyzerTab);
                    break;
                case 3:
                    _contentContainer.Add(_templateCreatorTab);
                    break;
            }
            
            // Update tab button styles (visual feedback for active tab)
            UpdateTabButtonStyles();
        }
        
        private void UpdateTabButtonStyles()
        {
            for (int i = 0; i < _tabButtonsContainer.childCount - 1; i++) // -1 for header
            {
                if (i == 0) continue; // Skip header
                
                Button tabButton = _tabButtonsContainer[i] as Button;
                if (tabButton != null)
                {
                    int tabIndex = i - 1; // Adjust for header
                    if (tabIndex == _activeTab)
                    {
                        tabButton.style.backgroundColor = new Color(0.28f, 0.42f, 0.58f, 1f);
                        tabButton.style.borderLeftColor = new Color(0.42f, 0.68f, 0.98f, 1f);
                    }
                    else
                    {
                        tabButton.style.backgroundColor = new Color(0.20f, 0.20f, 0.20f, 1f);
                        tabButton.style.borderLeftColor = new Color(0, 0, 0, 0);
                    }
                }
            }
        }

        private void StyleFoldout(Foldout foldout)
        {
            foldout.AddToClassList("upa-foldout");
            foldout.style.fontSize = _compactMode ? _baseFontSize - 1 : _baseFontSize;
        }

        private VisualElement CreateTopHeaderBar()
        {
            VisualElement topBar = new VisualElement();
            topBar.style.height = 54;
            topBar.style.flexDirection = FlexDirection.Row;
            topBar.style.justifyContent = Justify.SpaceBetween;
            topBar.style.alignItems = Align.Center;
            topBar.style.paddingLeft = 14;
            topBar.style.paddingRight = 14;
            topBar.style.backgroundColor = new Color(0.10f, 0.10f, 0.10f, 1f);
            topBar.style.borderBottomWidth = 1;
            topBar.style.borderBottomColor = new Color(0.22f, 0.22f, 0.22f, 1f);

            // Left: Title + subtitle
            VisualElement left = new VisualElement();
            left.style.flexDirection = FlexDirection.Column;

            Label title = new Label("🧠 Unity Project Architect Studio");
            title.style.fontSize = _baseFontSize + 1;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = Color.white;

            Label subtitle = new Label("Design, analyze and document your Unity projects");
            subtitle.style.fontSize = _baseFontSize - 2;
            subtitle.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);

            left.Add(title);
            left.Add(subtitle);

            // Right: AI status pill + compact toggle + text size selector
            VisualElement right = new VisualElement();
            right.style.flexDirection = FlexDirection.Row;
            right.style.alignItems = Align.Center;
            right.style.marginLeft = 8;

            bool aiConfigured = !string.IsNullOrEmpty(EditorPrefs.GetString("UnityProjectArchitect.ClaudeAPIKey", ""));
            Label aiStatus = new Label(aiConfigured ? "AI: Configured" : "AI: Offline Fallback");
            aiStatus.style.fontSize = _baseFontSize - 2;
            aiStatus.style.color = aiConfigured ? new Color(0.15f, 0.40f, 0.15f, 1f) : new Color(0.60f, 0.35f, 0.05f, 1f);
            aiStatus.style.backgroundColor = aiConfigured ? new Color(0.50f, 0.80f, 0.50f, 0.2f) : new Color(0.95f, 0.70f, 0.35f, 0.15f);
            aiStatus.style.borderTopLeftRadius = 10;
            aiStatus.style.borderTopRightRadius = 10;
            aiStatus.style.borderBottomLeftRadius = 10;
            aiStatus.style.borderBottomRightRadius = 10;
            aiStatus.style.paddingLeft = 8;
            aiStatus.style.paddingRight = 8;
            aiStatus.style.paddingTop = 4;
            aiStatus.style.paddingBottom = 4;

            Toggle compactToggle = new Toggle("Compact");
            compactToggle.value = _compactMode;
            compactToggle.style.fontSize = _baseFontSize - 2;
            compactToggle.style.marginLeft = 8;
            compactToggle.RegisterValueChangedCallback(evt => {
                _compactMode = evt.newValue;
                EditorPrefs.SetBool("UnityProjectArchitect.CompactMode", _compactMode);
                CreateUI();
            });

            // Text size dropdown
            EnumField textSizeDropdown = new EnumField("Text Size", TextSizeFromString(_textSizeSetting));
            textSizeDropdown.style.marginLeft = 10;
            textSizeDropdown.style.minWidth = 150;
            textSizeDropdown.RegisterValueChangedCallback(evt => {
                _textSizeSetting = evt.newValue.ToString();
                EditorPrefs.SetString("UnityProjectArchitect.TextSize", _textSizeSetting);
                _baseFontSize = GetBaseFontSize(_textSizeSetting);
                CreateUI();
            });

            right.Add(aiStatus);
            right.Add(compactToggle);
            right.Add(textSizeDropdown);

            topBar.Add(left);
            topBar.Add(right);
            return topBar;
        }
        
        private string GetGameDescriptionTemplate()
        {
            return @"**Your Game Title**

[Brief description of your game concept]

**Core Gameplay:**
- [Feature 1]
- [Feature 2]
- [Feature 3]

**Key Features:**
- [Feature 1]
- [Feature 2]
- [Feature 3]

**Target Audience:** [Your target players]
**Platform:** [PC/Console/Mobile/etc.]
**Art Style:** [Visual style description]
**Development Time:** [Timeline estimate]";
        }
        
        private string GetExampleGameDescription()
        {
            return @"**Mystic Forest Adventure**

A 3D action-adventure RPG set in an enchanted forest where players take on the role of a young mage discovering their magical abilities.

**Core Gameplay:**
- Explore a vast mystical forest with multiple biomes (enchanted groves, dark marshlands, crystal caves)
- Cast spells using gesture-based combat system
- Solve environmental puzzles using magical abilities
- Tame and befriend magical creatures as companions
- Craft potions and magical items from forest ingredients

**Key Features:**
- Dynamic weather system that affects gameplay
- Day/night cycle with different creatures and challenges
- Skill tree with 4 magic schools: Nature, Elemental, Light, and Shadow
- Base building system to create magical sanctuaries
- Multiplayer co-op for up to 4 players

**Target Audience:** Ages 13+ who enjoy fantasy RPGs like Skyrim and Zelda
**Platform:** PC and Console
**Art Style:** Stylized 3D with vibrant colors and magical effects
**Development Time:** 18 months with a team of 8 developers";
        }
        
        private async void GenerateDocumentationFromConcept()
        {
            if (string.IsNullOrWhiteSpace(_gameDescriptionField.value))
            {
                EditorUtility.DisplayDialog("Missing Description", "Please enter a game description first.", "OK");
                return;
            }

            _generateDocsButton.SetEnabled(false);
            _exportDocsButton.SetEnabled(false);
            _createStructureButton.SetEnabled(false);
            _statusLabel.text = "🔄 Creating project from game concept...";
            _statusLabel.style.color = Color.yellow;
            _documentationResults.Clear();
            
            try
            {
                // Create a concept project based on the game description
                _conceptProject = CreateConceptProjectFromDescription(_gameDescriptionField.value);
                
                _statusLabel.text = "✨ Generating AI-powered documentation...";
                
                // Generate documentation for different sections
                DocumentationSectionType[] sectionTypes = new[]
                {
                    DocumentationSectionType.GeneralProductDescription,
                    DocumentationSectionType.UserStories,
                    DocumentationSectionType.WorkTickets,
                    DocumentationSectionType.SystemArchitecture
                };

                UnityDocumentationService docService = UnityServiceBridge.GetDocumentationService();
                
                foreach (DocumentationSectionType sectionType in sectionTypes)
                {
                    _statusLabel.text = $"🔄 Generating {sectionType}...";
                    await Task.Delay(100); // Allow UI update
                    
                    DocumentationSectionData section = new DocumentationSectionData
                    {
                        SectionType = sectionType,
                        IsEnabled = true,
                        Status = DocumentationStatus.NotStarted,
                        AIMode = AIGenerationMode.FullGeneration,
                        CustomPrompt = $"Based on this game description: {_gameDescriptionField.value}"
                    };
                    
                    string content = await docService.GenerateDocumentationSectionAsync(section, _conceptProject.ProjectData);
                    
                    // Store the content in the section for export
                    section.Content = content;
                    section.Status = DocumentationStatus.Generated;
                    section.LastUpdated = System.DateTime.Now;
                    
                    // Add section to project data if not already present
                    DocumentationSectionData existingSection = _conceptProject.ProjectData.DocumentationSections
                        .FirstOrDefault(s => s.SectionType == sectionType);
                    if (existingSection != null)
                    {
                        existingSection.Content = content;
                        existingSection.Status = DocumentationStatus.Generated;
                        existingSection.LastUpdated = System.DateTime.Now;
                    }
                    else
                    {
                        _conceptProject.ProjectData.DocumentationSections.Add(section);
                    }
                    
                    CreateDocumentationCard(sectionType.ToString(), content);
                }
                
                _statusLabel.text = "✅ Documentation generation complete!";
                _statusLabel.style.color = Color.green;
                
                // Enable export and structure creation buttons
                _exportDocsButton.SetEnabled(true);
                _createStructureButton.SetEnabled(true);
                
                EditorUtility.DisplayDialog("Success!", 
                    "AI-powered documentation has been generated successfully! You can now export the documentation or create the project structure.", 
                    "Awesome!");
            }
            catch (Exception ex)
            {
                _statusLabel.text = $"❌ Generation failed: {ex.Message}";
                _statusLabel.style.color = Color.red;
                Debug.LogError($"Documentation generation failed: {ex}");
                
                EditorUtility.DisplayDialog("Generation Failed", 
                    $"Documentation generation failed: {ex.Message}\n\nCheck the Console for more details.", 
                    "OK");
            }
            
            _generateDocsButton.SetEnabled(true);
        }
        
        private async void ExportDocumentation()
        {
            if (_conceptProject == null)
            {
                EditorUtility.DisplayDialog("No Documentation", "Please generate documentation first.", "OK");
                return;
            }
            
            // Use the same export dialog as Project Analyzer tab
            ExportFormat format = ShowExportFormatDialog();
            if (format == ExportFormat.None) return; // User cancelled
            
            _exportDocsButton.SetEnabled(false);
            _statusLabel.text = "📄 Exporting documentation...";
            _statusLabel.style.color = Color.yellow;
            
            try
            {
                // Use the same export service as Project Analyzer
                IExportService exportService = UnityServiceBridge.GetExportService();
                
                ExportRequest request = new ExportRequest(format, "Assets/Documentation/")
                {
                    FileName = $"{_conceptProject.ProjectData.ProjectName}_Documentation"
                };
                
                ExportOperationResult result = await exportService.ExportProjectDocumentationAsync(_conceptProject.ProjectData, request);
                
                if (result.Success)
                {
                _statusLabel.text = "✅ Documentation exported successfully!";
                _statusLabel.style.color = Color.green;
                
                // Ask if user wants to open the export folder
                bool openFolder = EditorUtility.DisplayDialog("Export Complete", 
                        $"Documentation exported successfully to:\n{result.OutputPath}\n\nOpen folder?", "Open Folder", "Close");
                    
                if (openFolder)
                {
                        string folderPath = System.IO.Path.GetDirectoryName(result.OutputPath);
                        System.Diagnostics.Process.Start("explorer.exe", folderPath.Replace('/', '\\'));
                    }
                }
                else
                {
                    _statusLabel.text = $"❌ Export failed: {result.ErrorMessage}";
                    _statusLabel.style.color = Color.red;
                    
                    EditorUtility.DisplayDialog("Export Failed", 
                        $"Export failed: {result.ErrorMessage}", "OK");
                }
            }
            catch (System.Exception ex)
            {
                _statusLabel.text = $"❌ Export failed: {ex.Message}";
                _statusLabel.style.color = Color.red;
                Debug.LogError($"Documentation export failed: {ex}");
                
                EditorUtility.DisplayDialog("Export Failed", 
                    $"Documentation export failed: {ex.Message}", "OK");
            }
            
            _exportDocsButton.SetEnabled(true);
        }
        
        private ExportFormat ShowExportFormatDialog()
        {
            int choice = EditorUtility.DisplayDialogComplex("Export Documentation", 
                "Choose export format:", "📄 Markdown (.md)", "📑 PDF", "Cancel");
                
            return choice switch
            {
                0 => ExportFormat.Markdown,
                1 => ExportFormat.PDF,
                _ => ExportFormat.None // Cancel or close
            };
        }
        

        
        private void CreateProjectStructure()
        {
            if (_conceptProject == null)
            {
                EditorUtility.DisplayDialog("No Project Data", "Please generate documentation first.", "OK");
                return;
            }
            
            // Confirm with user before creating folders
            bool confirmed = EditorUtility.DisplayDialog("Create Project Structure", 
                $"Create folder structure for '{_conceptProject.ProjectData.ProjectName}'?\n\n" +
                $"This will create folders in your Unity project based on the project type: {_conceptProject.ProjectData.ProjectType}\n\n" +
                "Folders will be created in the Assets directory.", 
                "Create Structure", "Cancel");
                
            if (!confirmed) return;
            
            _createStructureButton.SetEnabled(false);
            _statusLabel.text = "📁 Creating project structure...";
            _statusLabel.style.color = Color.yellow;
            
            try
            {
                List<string> createdFolders = CreateFolderStructureBasedOnProjectType(_conceptProject.ProjectData.ProjectType);
                
                // Refresh the Asset Database to show new folders
                AssetDatabase.Refresh();
                
                _statusLabel.text = "✅ Project structure created successfully!";
                _statusLabel.style.color = Color.green;
                
                string folderList = string.Join("\n• ", createdFolders);
                EditorUtility.DisplayDialog("Structure Created!", 
                    $"Successfully created {createdFolders.Count} folders:\n\n• {folderList}\n\nCheck your Assets folder in the Project window.", 
                    "Great!");
                    
                Debug.Log($"✅ Created project structure with {createdFolders.Count} folders for {_conceptProject.ProjectData.ProjectType} project");
            }
            catch (System.Exception ex)
            {
                _statusLabel.text = $"❌ Structure creation failed: {ex.Message}";
                _statusLabel.style.color = Color.red;
                Debug.LogError($"Project structure creation failed: {ex}");
                
                EditorUtility.DisplayDialog("Creation Failed", 
                    $"Project structure creation failed: {ex.Message}", "OK");
            }
            
            _createStructureButton.SetEnabled(true);
        }
        
        private List<string> CreateFolderStructureBasedOnProjectType(ProjectType projectType)
        {
            List<string> createdFolders = new List<string>();
            List<string> folderStructure = GetSmartFolderStructure(projectType);
            
            foreach (string folder in folderStructure)
            {
                CreateFolderIfNotExists(folder);
                createdFolders.Add(folder);
            }
            
            return createdFolders;
        }
        
        private List<string> GetSmartFolderStructure(ProjectType projectType)
        {
            List<string> folders = new List<string>();
            
            // Common folders for all project types
            folders.AddRange(new[]
            {
                "Scripts",
                "Scenes", 
                "Audio",
                "Documentation"
            });
            
            // Project type specific folders
            switch (projectType)
            {
                case ProjectType.Game2D:
                case ProjectType.Mobile2D:
                case ProjectType.PC2D:
                    folders.AddRange(new[]
                    {
                        "Sprites",
                        "Sprites/Characters",
                        "Sprites/Environment",
                        "Sprites/UI", 
                        "Animations",
                        "Tilemaps",
                        "UI/Prefabs",
                        "Effects/Particles2D"
                    });
                    break;
                    
                case ProjectType.Game3D:
                case ProjectType.Mobile3D:
                case ProjectType.PC3D:
                case ProjectType.GameDevelopment:
                    folders.AddRange(new[]
                    {
                        "Models",
                        "Models/Characters", 
                        "Models/Environment",
                        "Models/Props",
                        "Textures",
                        "Materials",
                        "Animations",
                        "Effects/Particles",
                        "Shaders",
                        "Prefabs/Characters",
                        "Prefabs/Environment"
                    });
                    break;
                    
                case ProjectType.VR:
                case ProjectType.VRApplication:
                    folders.AddRange(new[]
                    {
                        "Models",
                        "Models/Interactions",
                        "Models/Environment", 
                        "Textures",
                        "Materials",
                        "Interactions",
                        "Spatial",
                        "XR",
                        "Prefabs/VR"
                    });
                    break;
                    
                case ProjectType.AR:
                    folders.AddRange(new[]
                    {
                        "Models",
                        "Textures", 
                        "Materials",
                        "AR/Tracking",
                        "AR/Anchors",
                        "Effects/AR"
                    });
                    break;
                    
                case ProjectType.Mobile:
                case ProjectType.MobileGame:
                    folders.AddRange(new[]
                    {
                        "UI/Mobile",
                        "Optimization",
                        "Platform/Android",
                        "Platform/iOS",
                        "Audio/Compressed"
                    });
                    break;
                    
                case ProjectType.Multiplayer:
                    folders.AddRange(new[]
                    {
                        "Networking",
                        "Networking/Client", 
                        "Networking/Server",
                        "Networking/Shared"
                    });
                    break;
                    
                case ProjectType.EditorTool:
                case ProjectType.Tool:
                    folders.AddRange(new[]
                    {
                        "Editor",
                        "Editor/Windows",
                        "Editor/Tools", 
                        "Editor/Inspectors",
                        "Runtime/Utilities"
                    });
                    break;
                    
                case ProjectType.Educational:
                    folders.AddRange(new[]
                    {
                        "Learning",
                        "Learning/Lessons",
                        "Learning/Exercises",
                        "UI/Educational"
                    });
                    break;
                    
                case ProjectType.Simulation:
                    folders.AddRange(new[]
                    {
                        "Simulation",
                        "Simulation/Physics",
                        "Simulation/Data", 
                        "Analytics"
                    });
                    break;
                    
                case ProjectType.Prototype:
                    folders.AddRange(new[]
                    {
                        "Prototypes",
                        "Experiments",
                        "TestAssets"
                    });
                    break;
                    
                default: // General or Unknown
                    folders.AddRange(new[]
                    {
                        "Art",
                        "Prefabs", 
                        "Resources"
                    });
                    break;
            }
            
            // Add project-specific folders based on game description analysis
            folders.AddRange(GetContextualFolders());
            
            return folders.Distinct().OrderBy(f => f).ToList();
        }
        
        private List<string> GetContextualFolders()
        {
            List<string> contextFolders = new List<string>();
            
            if (_conceptProject?.ProjectData?.ProjectDescription != null)
            {
                string description = _conceptProject.ProjectData.ProjectDescription.ToLower();
                
                // Analyze description for specific features
                if (description.Contains("inventory") || description.Contains("items") || description.Contains("equipment"))
                {
                    contextFolders.Add("UI/Inventory");
                    contextFolders.Add("Items");
                }
                
                if (description.Contains("multiplayer") || description.Contains("co-op") || description.Contains("online"))
                {
                    contextFolders.Add("Networking");
                }
                
                if (description.Contains("level") || description.Contains("stage") || description.Contains("world"))
                {
                    contextFolders.Add("Levels");
                }
                
                if (description.Contains("boss") || description.Contains("enemy") || description.Contains("combat"))
                {
                    contextFolders.Add("Combat");
                    contextFolders.Add("Enemies");
                }
                
                if (description.Contains("ui") || description.Contains("menu") || description.Contains("hud"))
                {
                    contextFolders.Add("UI/Menus");
                    contextFolders.Add("UI/HUD");
                }
                
                if (description.Contains("puzzle") || description.Contains("mini-game"))
                {
                    contextFolders.Add("Puzzles");
                }
                
                if (description.Contains("crafting") || description.Contains("building"))
                {
                    contextFolders.Add("Crafting");
                }
            }
            
            return contextFolders;
        }
        
        private void CreateFolderIfNotExists(string folderPath)
        {
            string assetsPath = "Assets/" + folderPath;
            
            if (!AssetDatabase.IsValidFolder(assetsPath))
            {
                // Split path and create parent directories if needed
                string[] pathParts = folderPath.Split('/');
                string currentPath = "Assets";
                
                for (int i = 0; i < pathParts.Length; i++)
                {
                    string nextPath = currentPath + "/" + pathParts[i];
                    
                    if (!AssetDatabase.IsValidFolder(nextPath))
                    {
                        string guid = AssetDatabase.CreateFolder(currentPath, pathParts[i]);
                        if (string.IsNullOrEmpty(guid))
                        {
                            Debug.LogError($"Failed to create folder: {nextPath}");
                        }
                    }
                    
                    currentPath = nextPath;
                }
            }
        }
        
        private UnityProjectDataAsset CreateConceptProjectFromDescription(string description)
        {
            // Create a temporary concept project
            UnityProjectDataAsset conceptProject = CreateInstance<UnityProjectDataAsset>();
            conceptProject.Initialize();
            
            // Extract game name from description (first line or default)
            string[] lines = description.Split('\n');
            string gameName = "AI Generated Game";
            
            foreach (string line in lines)
            {
                if (line.StartsWith("**") && line.EndsWith("**") && line.Length > 4)
                {
                    gameName = line.Substring(2, line.Length - 4);
                    break;
                }
            }
            
            conceptProject.UpdateProjectName(gameName);
            conceptProject.ProjectData.ProjectDescription = description;
            conceptProject.ProjectData.ProjectType = ProjectType.Game3D; // Default, could be smarter
            
            return conceptProject;
        }
        
        private void CreateDocumentationCard(string title, string content)
        {
            VisualElement card = new VisualElement();
            card.AddToClassList("upa-card");
            
            // Use a foldout so the content area is collapsible per section
            Foldout sectionFoldout = new Foldout { text = $"📄 {title}", value = false };
            sectionFoldout.AddToClassList("upa-foldout");
            
            // Create a ScrollView for the content with proper scroll functionality
            ScrollView contentScrollView = new ScrollView();
            contentScrollView.style.height = 250;
            
            TextField contentField = new TextField()
            {
                multiline = true,
                value = content,
                isReadOnly = true
            };
            contentField.style.flexGrow = 1;
            contentField.style.flexShrink = 1;
            contentField.style.whiteSpace = WhiteSpace.Normal;
            contentField.style.unityTextAlign = TextAnchor.UpperLeft;
            contentField.style.fontSize = _baseFontSize - 1;
            contentField.style.minHeight = 230; // Ensure minimum height for scrolling
            
            contentScrollView.Add(contentField);
            
            int wordCount = string.IsNullOrEmpty(content) ? 0 : content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            Label statsLabel = new Label($"Generated: {content.Length:N0} characters, {wordCount:N0} words");
            statsLabel.style.fontSize = _baseFontSize - 3;
            statsLabel.style.color = Color.gray;
            statsLabel.style.marginTop = 5;
            
            sectionFoldout.Add(contentScrollView);
            sectionFoldout.Add(statsLabel);

            card.Add(sectionFoldout);
            
            _documentationResults.Add(card);
        }
        
        #region Project Analyzer Methods
        
        private void CreateProjectConfigurationSection(ScrollView parent)
        {
            Foldout projectFoldout = new Foldout { text = "📁 Project Configuration", value = true };
            StyleFoldout(projectFoldout);
            
            _projectField = new ObjectField("Project Data Asset")
            {
                objectType = typeof(UnityProjectDataAsset),
                allowSceneObjects = false,
                value = _currentProjectAsset
            };
            _projectField.RegisterValueChangedCallback(OnProjectAssetChanged);
            
            VisualElement buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.marginTop = 10;
            
            Button createButton = new Button(CreateNewProjectAsset) { text = "Create New Project Data Asset" };
            createButton.style.flexGrow = 1;
            createButton.style.marginRight = 5;
            
            Button editButton = new Button(() => {
                if (_currentProjectAsset != null)
                {
                    Selection.activeObject = _currentProjectAsset;
                    EditorGUIUtility.PingObject(_currentProjectAsset);
                }
            }) { text = "Edit Project Settings" };
            editButton.style.flexGrow = 1;
            editButton.style.marginLeft = 5;
            
            buttonContainer.Add(createButton);
            buttonContainer.Add(editButton);
            
            projectFoldout.Add(_projectField);
            projectFoldout.Add(buttonContainer);
            
            parent.Add(projectFoldout);
        }
        
        private void CreateProjectDocumentationSection(ScrollView parent)
        {
            Foldout documentationFoldout = new Foldout { text = "📖 Documentation Sections", value = true };
            StyleFoldout(documentationFoldout);
            
            _documentationContainer = new VisualElement();
            documentationFoldout.Add(_documentationContainer);
            
            parent.Add(documentationFoldout);
        }
        
        private void CreateProjectExportSection(ScrollView parent)
        {
            Foldout exportFoldout = new Foldout { text = "📤 Export Options", value = false };
            StyleFoldout(exportFoldout);
            
            Label exportLabel = new Label("Export Formats");
            exportLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            exportLabel.style.marginBottom = 5;
            
            VisualElement buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.justifyContent = Justify.SpaceBetween;
            
            Button markdownButton = new Button(() => ExportProjectDocumentation(ExportFormat.Markdown)) 
            { 
                text = "📄 Export Markdown" 
            };
            markdownButton.style.flexGrow = 1;
            markdownButton.style.marginRight = 5;
            
            Button pdfButton = new Button(() => ExportProjectDocumentation(ExportFormat.PDF)) 
            { 
                text = "📑 Export PDF" 
            };
            pdfButton.style.flexGrow = 1;
            
            buttonContainer.Add(markdownButton);
            buttonContainer.Add(pdfButton);
            
            exportFoldout.Add(exportLabel);
            exportFoldout.Add(buttonContainer);
            
            parent.Add(exportFoldout);
        }
        
        private void CreateAnalysisActionsSection(ScrollView parent)
        {
            Foldout actionsContainer = new Foldout { text = "🔧 Analysis Actions", value = true };
            StyleFoldout(actionsContainer);
            
            _projectStatusLabel = new Label("Ready to analyze project and generate documentation");
            _projectStatusLabel.AddToClassList("upa-fs-base");
            _projectStatusLabel.AddToClassList("upa-mb-10");
            
            VisualElement buttonContainer = new VisualElement();
            buttonContainer.AddToClassList("upa-row");
            
            Button analyzeButton = new Button(AnalyzeCurrentProject) { text = "🔍 Analyze Project" };
            analyzeButton.AddToClassList("upa-btn");
            analyzeButton.AddToClassList("upa-btn-md");
            analyzeButton.AddToClassList("upa-btn-normalize");
            analyzeButton.AddToClassList("upa-mr-5");
            analyzeButton.style.flexGrow = 1;
            
            Button refreshButton = new Button(() => {
                if (_currentProjectAsset != null)
                {
                    _currentProjectAsset.SaveToJson();
                }
                RefreshProjectUI();
            }) { text = "🔄 Refresh Data" };
            refreshButton.AddToClassList("upa-btn");
            refreshButton.AddToClassList("upa-btn-md");
            refreshButton.AddToClassList("upa-btn-normalize");
            refreshButton.style.flexGrow = 1;
            
            buttonContainer.Add(analyzeButton);
            buttonContainer.Add(refreshButton);
            
            // Progress bar for analysis
            _analysisProgress = new ProgressBar();
            _analysisProgress.style.marginTop = 5;
            _analysisProgress.style.display = DisplayStyle.None;
            
            actionsContainer.Add(_projectStatusLabel);
            actionsContainer.Add(buttonContainer);
            actionsContainer.Add(_analysisProgress);
            
            parent.Add(actionsContainer);
        }
        
        private void InitializeProjectAnalyzer()
        {
            // Try to find existing project data
            string[] guids = AssetDatabase.FindAssets("t:UnityProjectDataAsset");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _currentProjectAsset = AssetDatabase.LoadAssetAtPath<UnityProjectDataAsset>(path);
                if (_projectField != null)
                {
                    _projectField.value = _currentProjectAsset;
                }
            }
            
            RefreshProjectUI();
        }
        
        private void OnProjectAssetChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            _currentProjectAsset = evt.newValue as UnityProjectDataAsset;
            RefreshProjectUI();
        }
        
        private void RefreshProjectUI()
        {
            RefreshProjectDocumentationSections();
        }
        
        private void RefreshProjectDocumentationSections()
        {
            if (_documentationContainer == null) return;
            
            _documentationContainer.Clear();
            
            if (_currentProjectAsset == null)
            {
                HelpBox warningBox = new HelpBox("No project data asset selected. Create or select a project data asset first.", HelpBoxMessageType.Warning);
                _documentationContainer.Add(warningBox);
                return;
            }
            
            ProjectData projectData = _currentProjectAsset.ProjectData;
            
            // Clean up any concept-only sections that may exist from before the separation
            projectData.RemoveConceptOnlySections();
            
            if (projectData.DocumentationSections.Count == 0)
            {
                HelpBox warningBox = new HelpBox("No documentation sections found. Initialize project data first.", HelpBoxMessageType.Warning);
                _documentationContainer.Add(warningBox);
                return;
            }
            
            // Filter out duplicate sections and concept-only sections (UserStories/WorkTickets) to prevent UI clutter
            Dictionary<DocumentationSectionType, DocumentationSectionData> uniqueSections = new Dictionary<DocumentationSectionType, DocumentationSectionData>();
            foreach (DocumentationSectionData section in projectData.DocumentationSections)
            {
                // Skip UserStories and WorkTickets - they're only for Game Concept Studio, not Project Analysis
                if (section.SectionType == DocumentationSectionType.UserStories || 
                    section.SectionType == DocumentationSectionType.WorkTickets)
                {
                    continue;
                }
                
                if (!uniqueSections.ContainsKey(section.SectionType))
                {
                    uniqueSections[section.SectionType] = section;
                }
            }
            
            foreach (DocumentationSectionData section in uniqueSections.Values)
            {
                CreateProjectDocumentationSectionItem(section);
            }
        }
        
        private void CreateProjectDocumentationSectionItem(DocumentationSectionData section)
        {
            VisualElement sectionContainer = new VisualElement();
            sectionContainer.style.flexDirection = FlexDirection.Row;
            sectionContainer.style.alignItems = Align.Center;
            sectionContainer.style.paddingBottom = 5;
            sectionContainer.style.paddingTop = 5;
            sectionContainer.style.borderBottomWidth = 1;
            sectionContainer.style.borderBottomColor = Color.gray;
            
            // Enabled toggle
            Toggle enabledToggle = new Toggle();
            enabledToggle.value = section.IsEnabled;
            enabledToggle.style.marginRight = 10;
            enabledToggle.RegisterValueChangedCallback(evt => {
                section.IsEnabled = evt.newValue;
                if (_currentProjectAsset != null)
                {
                    _currentProjectAsset.SaveToJson();
                }
            });
            
            // Section name and status
            string statusIcon = GetStatusIcon(section.Status);
            Label sectionLabel = new Label($"{statusIcon} {section.SectionType}");
            sectionLabel.style.flexGrow = 1;
            sectionLabel.style.marginRight = 10;
            
            // Word count
            int wordCount = string.IsNullOrEmpty(section.Content) ? 0 : section.Content.Split(' ').Length;
            Label wordCountLabel = new Label($"{wordCount} words");
            wordCountLabel.style.fontSize = _baseFontSize - 3;
            wordCountLabel.style.color = Color.gray;
            wordCountLabel.style.marginRight = 10;
            wordCountLabel.style.minWidth = 60;
            
            // Generate button
            Button generateButton = new Button(() => GenerateProjectDocumentationSection(section)) 
            { 
                text = "Generate" 
            };
            generateButton.style.minWidth = 70;
            generateButton.SetEnabled(section.IsEnabled);
            
            sectionContainer.Add(enabledToggle);
            sectionContainer.Add(sectionLabel);
            sectionContainer.Add(wordCountLabel);
            sectionContainer.Add(generateButton);
            
            _documentationContainer.Add(sectionContainer);
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
                
                _currentProjectAsset = asset;
                if (_projectField != null)
                {
                    _projectField.value = asset;
                }
                Selection.activeObject = asset;
            }
        }
        
        private async void GenerateProjectDocumentationSection(DocumentationSectionData section)
        {
            Debug.Log($"Generating documentation for section: {section.SectionType}");
            
            try
            {
                // Show progress indicator
                section.Status = DocumentationStatus.InProgress;
                section.LastUpdated = System.DateTime.Now;
                RefreshProjectDocumentationSections();
                
                // Get documentation service from Unity bridge
                UnityDocumentationService documentationService = UnityServiceBridge.GetDocumentationService();
                
                // Generate content using actual DLL services
                string generatedContent = await documentationService.GenerateDocumentationSectionAsync(section, _currentProjectAsset.ProjectData);
                
                // Update section with generated content
                section.Content = generatedContent;
                section.Status = DocumentationStatus.Generated;
                section.LastUpdated = System.DateTime.Now;
                
                _currentProjectAsset.SaveToJson();
                RefreshProjectDocumentationSections();
                
                Debug.Log($"✅ Successfully generated {section.SectionType} documentation ({generatedContent.Length:N0} characters)");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to generate {section.SectionType} documentation: {ex.Message}");
                
                // Set error status and fallback content
                section.Status = DocumentationStatus.NotStarted;
                section.Content = $"# {section.SectionType}\n\n**Generation Error:** {ex.Message}\n\n*Generated on {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}*";
                section.LastUpdated = System.DateTime.Now;
                
                _currentProjectAsset.SaveToJson();
                RefreshProjectDocumentationSections();
            }
        }
        
        private async void ExportProjectDocumentation(ExportFormat format)
        {
            Debug.Log($"Exporting project documentation as {format}");
            
            if (_currentProjectAsset == null)
            {
                EditorUtility.DisplayDialog("No Project", "Please select or create a project data asset first.", "OK");
                return;
            }
            
            IExportService exportService = UnityServiceBridge.GetExportService();
            
            ExportRequest request = new ExportRequest(format, "Assets/Documentation/")
            {
                FileName = $"{_currentProjectAsset.ProjectData.ProjectName}_Documentation"
            };
            
            try
            {
                ExportOperationResult result = await exportService.ExportProjectDocumentationAsync(_currentProjectAsset.ProjectData, request);
                
                if (result.GeneratedFiles != null && result.GeneratedFiles.Count > 0)
                {
                    Debug.LogError($"  - Generated files: {string.Join(", ", result.GeneratedFiles)}");
                }
                Debug.LogError($"  - Metadata keys: {(result.Metadata != null ? string.Join(", ", result.Metadata.Keys) : "none")}");
                if (result.Metadata != null && result.Metadata.ContainsKey("fallback_mode"))
                {
                    Debug.LogError($"  - FALLBACK MODE: {result.Metadata["fallback_mode"]}");
                    string fallbackReason = result.Metadata.ContainsKey("fallback_reason") ? result.Metadata["fallback_reason"].ToString() : "unknown";
                    Debug.LogError($"  - FALLBACK REASON: {fallbackReason}");
                }
                
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
        
        private async void AnalyzeCurrentProject()
        {
            Debug.Log("Analyzing project structure...");
            
            _projectStatusLabel.text = "🔄 Analyzing project structure...";
            _projectStatusLabel.style.color = Color.yellow;
            
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
                    _projectStatusLabel.text = "✅ Project analysis completed successfully.";
                    _projectStatusLabel.style.color = Color.green;
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
                        _projectStatusLabel.text = "ℹ️ Project analysis completed. Empty project detected - no issues found.";
                        _projectStatusLabel.style.color = Color.cyan;
                        Debug.Log("ℹ️ Project analysis completed. Empty project detected - no issues found.");
                    }
                    else
                    {
                        // Actual analysis failure
                        validationResult.IsValid = false;
                        _projectStatusLabel.text = $"⚠️ Project analysis completed with issues: {result.ErrorMessage}";
                        _projectStatusLabel.style.color = Color.yellow;
                        Debug.LogWarning($"⚠️ Project analysis completed with issues: {result.ErrorMessage}");
                    }
                }
                
                UnityServiceBridge.ShowValidationResult(validationResult);
            }
            catch (System.Exception ex)
            {
                _projectStatusLabel.text = $"❌ Project analysis failed: {ex.Message}";
                _projectStatusLabel.style.color = Color.red;
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
        
        #endregion
        
        #region Smart Template Creator Methods
        
        private void CreateTemplateInfoSection(ScrollView parent)
        {
            Foldout infoFoldout = new Foldout { text = "Template Information", value = true };
            StyleFoldout(infoFoldout);
            
            Label nameLabel = new Label("Template Name:");
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.marginBottom = 5;
            
            _templateNameField = new TextField()
            {
                value = "New Smart Template"
            };
            _templateNameField.style.marginBottom = 10;
            
            Label descLabel = new Label("Description:");
            descLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            descLabel.style.marginBottom = 5;
            
            // Create a ScrollView for the description field with proper scroll functionality
            ScrollView descFieldScrollView = new ScrollView();
            descFieldScrollView.style.height = 80;
            descFieldScrollView.style.marginBottom = 10;
            descFieldScrollView.style.borderTopLeftRadius = 3;
            descFieldScrollView.style.borderTopRightRadius = 3;
            descFieldScrollView.style.borderBottomLeftRadius = 3;
            descFieldScrollView.style.borderBottomRightRadius = 3;
            descFieldScrollView.style.borderLeftWidth = 1;
            descFieldScrollView.style.borderRightWidth = 1;
            descFieldScrollView.style.borderTopWidth = 1;
            descFieldScrollView.style.borderBottomWidth = 1;
            descFieldScrollView.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            descFieldScrollView.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            descFieldScrollView.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            descFieldScrollView.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            
            _templateDescriptionField = new TextField()
            {
                multiline = true,
                value = "A smart template with intelligent folder suggestions"
            };
            _templateDescriptionField.style.flexGrow = 1;
            _templateDescriptionField.style.flexShrink = 1;
            _templateDescriptionField.style.whiteSpace = WhiteSpace.Normal;
            _templateDescriptionField.style.unityTextAlign = TextAnchor.UpperLeft;
            _templateDescriptionField.style.fontSize = _baseFontSize - 1;
            _templateDescriptionField.style.minHeight = 60; // Ensure minimum height for scrolling
            
            descFieldScrollView.Add(_templateDescriptionField);
            
            infoFoldout.Add(nameLabel);
            infoFoldout.Add(_templateNameField);
            infoFoldout.Add(descLabel);
            infoFoldout.Add(descFieldScrollView);
            
            parent.Add(infoFoldout);
        }
        
        private void CreateProjectTypeSection(ScrollView parent)
        {
            Foldout typeFoldout = new Foldout { text = "Project Type & Smart Suggestions", value = true };
            StyleFoldout(typeFoldout);
            
            Label typeLabel = new Label("Project Type:");
            typeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            typeLabel.style.marginBottom = 5;
            
            _projectTypeField = new EnumField("Project Type", ProjectType.Game3D);
            _projectTypeField.RegisterValueChangedCallback(evt => {
                OnProjectTypeChanged((ProjectType)evt.newValue);
            });
            
            typeFoldout.Add(typeLabel);
            typeFoldout.Add(_projectTypeField);
            
            parent.Add(typeFoldout);
        }
        
        private void CreateSmartSuggestionsSection(ScrollView parent)
        {
            Foldout suggestionsFoldout = new Foldout { text = "Smart Folder Suggestions", value = true };
            StyleFoldout(suggestionsFoldout);
            
            Label instructionLabel = new Label("Based on your project type, here are intelligent folder suggestions:");
            instructionLabel.style.fontSize = _baseFontSize - 1;
            instructionLabel.style.marginBottom = 10;
            instructionLabel.style.whiteSpace = WhiteSpace.Normal;
            
            _suggestedFoldersContainer = new VisualElement();
            _suggestedFoldersContainer.style.marginTop = 10;
            
            suggestionsFoldout.Add(instructionLabel);
            suggestionsFoldout.Add(_suggestedFoldersContainer);
            
            parent.Add(suggestionsFoldout);
            
            // Generate initial suggestions
            GenerateSmartSuggestions();
        }
        
        private void CreateCustomFolderSection(ScrollView parent)
        {
            Foldout customFoldout = new Foldout { text = "Custom Folder Structure", value = true };
            StyleFoldout(customFoldout);
            
            Label instructionLabel = new Label("Add custom folders specific to your project needs:");
            instructionLabel.style.fontSize = _baseFontSize - 1;
            instructionLabel.style.marginBottom = 10;
            
            VisualElement addFolderContainer = new VisualElement();
            addFolderContainer.style.flexDirection = FlexDirection.Row;
            addFolderContainer.style.marginBottom = 10;
            
            TextField newFolderField = new TextField()
            {
                value = "",
                style = { flexGrow = 1, marginRight = 5 }
            };
            newFolderField.RegisterCallback<KeyDownEvent>(evt => {
                if (evt.keyCode == KeyCode.Return)
                {
                    AddCustomFolder(newFolderField.value);
                    newFolderField.value = "";
                }
            });
            
            Button addFolderButton = new Button(() => {
                AddCustomFolder(newFolderField.value);
                newFolderField.value = "";
            }) { text = "➕ Add Folder" };
            
            addFolderContainer.Add(newFolderField);
            addFolderContainer.Add(addFolderButton);
            
            _folderStructureContainer = new VisualElement();
            _folderStructureContainer.style.marginTop = 10;
            
            customFoldout.Add(instructionLabel);
            customFoldout.Add(addFolderContainer);
            customFoldout.Add(_folderStructureContainer);
            
            parent.Add(customFoldout);
        }
        
        private void CreateTemplateActionsSection(ScrollView parent)
        {
            Foldout actionsFoldout = new Foldout { text = "Template Actions", value = true };
            StyleFoldout(actionsFoldout);
            
            _templateStatusLabel = new Label("Ready to create template and folders");
            _templateStatusLabel.style.fontSize = _baseFontSize - 1;
            _templateStatusLabel.style.color = Color.green;
            _templateStatusLabel.style.marginBottom = 10;
            
            // First row: Preview and Save Template
            VisualElement firstButtonContainer = new VisualElement();
            firstButtonContainer.style.flexDirection = FlexDirection.Row;
            firstButtonContainer.style.marginBottom = 10;
            
            _previewTemplateButton = new Button(PreviewTemplate)
            {
                text = "👁️ Preview Template"
            };
            _previewTemplateButton.style.height = 35;
            _previewTemplateButton.style.flexGrow = 1;
            _previewTemplateButton.style.marginRight = 5;
            
            _saveTemplateButton = new Button(SaveTemplate)
            {
                text = "💾 Save Template"
            };
            _saveTemplateButton.style.height = 35;
            _saveTemplateButton.style.flexGrow = 1;
            _saveTemplateButton.style.marginLeft = 5;
            _saveTemplateButton.style.backgroundColor = new Color(0.2f, 0.6f, 0.2f, 1f);
            
            firstButtonContainer.Add(_previewTemplateButton);
            firstButtonContainer.Add(_saveTemplateButton);
            
            // Second row: Create Folders Now (highlighted button)
            Button createFoldersButton = new Button(CreateFoldersNow)
            {
                text = "🚀 Create Folders Now"
            };
            createFoldersButton.style.height = 40;
            createFoldersButton.style.fontSize = _baseFontSize + 0;
            createFoldersButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            createFoldersButton.style.backgroundColor = new Color(0.1f, 0.4f, 0.8f, 1f);
            createFoldersButton.style.marginTop = 5;
            
            actionsFoldout.Add(_templateStatusLabel);
            actionsFoldout.Add(firstButtonContainer);
            actionsFoldout.Add(createFoldersButton);
            
            parent.Add(actionsFoldout);
        }
        
        private void OnProjectTypeChanged(ProjectType newProjectType)
        {
            GenerateSmartSuggestions();
        }
        
        private void GenerateSmartSuggestions()
        {
            _suggestedFoldersContainer.Clear();
            
            ProjectType selectedType = (ProjectType)_projectTypeField.value;
            List<string> suggestedFolders = GetSmartFolderStructure(selectedType);
            
            if (suggestedFolders.Count == 0)
            {
                Label noSuggestionsLabel = new Label("No specific suggestions available for this project type.");
                noSuggestionsLabel.style.fontSize = _baseFontSize - 3;
                noSuggestionsLabel.style.color = Color.gray;
                noSuggestionsLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                _suggestedFoldersContainer.Add(noSuggestionsLabel);
                return;
            }
            
            // Create expandable section for all suggestions
            Foldout suggestionsFoldout = new Foldout 
            { 
                text = $"📁 Smart Folder Suggestions ({suggestedFolders.Count} folders)", 
                value = true 
            };
            suggestionsFoldout.style.marginBottom = 10;
            suggestionsFoldout.style.fontSize = _baseFontSize - 1;
            suggestionsFoldout.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            // Add all suggestions to the expandable section
            for (int i = 0; i < suggestedFolders.Count; i++)
            {
                CreateSuggestionCard(suggestedFolders[i], suggestionsFoldout);
            }
            
            _suggestedFoldersContainer.Add(suggestionsFoldout);
            
            // Add summary label
            Label summaryLabel = new Label($"💡 {suggestedFolders.Count} intelligent folder suggestions for {selectedType} projects");
            summaryLabel.style.fontSize = _baseFontSize - 3;
            summaryLabel.style.color = Color.cyan;
            summaryLabel.style.marginTop = 10;
            summaryLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            _suggestedFoldersContainer.Add(summaryLabel);
        }
        
        private void CreateSuggestionCard(string folderName, VisualElement parentContainer = null)
        {
            VisualElement card = new VisualElement();
            card.AddToClassList("upa-card");
            card.style.flexDirection = FlexDirection.Row;
            card.style.alignItems = Align.Center;
            
            Label folderLabel = new Label($"📁 {folderName}");
            folderLabel.style.fontSize = _baseFontSize - 1;
            folderLabel.style.flexGrow = 1;
            folderLabel.style.unityFontStyleAndWeight = FontStyle.Normal;
            
            Button addButton = new Button(() => AddSuggestedFolder(folderName))
            {
                text = "➕ Add"
            };
            addButton.style.height = 24;
            addButton.style.fontSize = _baseFontSize - 3;
            addButton.style.width = 50;
            addButton.style.backgroundColor = new Color(0.2f, 0.6f, 0.2f, 1f);
            addButton.style.borderTopLeftRadius = 3;
            addButton.style.borderTopRightRadius = 3;
            addButton.style.borderBottomLeftRadius = 3;
            addButton.style.borderBottomRightRadius = 3;
            
            card.Add(folderLabel);
            card.Add(addButton);
            
            if (parentContainer != null)
            {
                parentContainer.Add(card);
            }
            else
            {
                _suggestedFoldersContainer.Add(card);
            }
        }
        
        private void AddSuggestedFolder(string folderName)
        {
            if (!_customFolders.Contains(folderName))
            {
                _customFolders.Add(folderName);
                RefreshCustomFoldersDisplay();
            }
        }
        
        private void AddCustomFolder(string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName)) return;
            
            folderName = folderName.Trim();
            if (!_customFolders.Contains(folderName))
            {
                _customFolders.Add(folderName);
                RefreshCustomFoldersDisplay();
            }
        }
        
        private void RefreshCustomFoldersDisplay()
        {
            _folderStructureContainer.Clear();
            
            if (_customFolders.Count == 0)
            {
                Label emptyLabel = new Label("No custom folders added yet. Use suggestions above or add your own.");
                emptyLabel.style.fontSize = _baseFontSize - 3;
                emptyLabel.style.color = Color.gray;
                emptyLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                _folderStructureContainer.Add(emptyLabel);
                return;
            }
            
            foreach (string folder in _customFolders)
            {
                CreateCustomFolderCard(folder);
            }
            
            Label countLabel = new Label($"Total folders: {_customFolders.Count}");
            countLabel.style.fontSize = _baseFontSize - 3;
            countLabel.style.color = Color.gray;
            countLabel.style.marginTop = 10;
            _folderStructureContainer.Add(countLabel);
        }
        
        private void CreateCustomFolderCard(string folderName)
        {
            VisualElement card = new VisualElement();
            card.AddToClassList("upa-card");
            card.style.flexDirection = FlexDirection.Row;
            card.style.alignItems = Align.Center;
            
            Label folderLabel = new Label($"📁 {folderName}");
            folderLabel.style.fontSize = _baseFontSize - 1;
            folderLabel.style.flexGrow = 1;
            
            Button removeButton = new Button(() => RemoveCustomFolder(folderName))
            {
                text = "✕"
            };
            removeButton.style.height = 18;
            removeButton.style.fontSize = _baseFontSize - 3;
            removeButton.style.width = 25;
            removeButton.style.backgroundColor = new Color(0.7f, 0.2f, 0.2f, 0.8f);
            
            card.Add(folderLabel);
            card.Add(removeButton);
            
            _folderStructureContainer.Add(card);
        }
        
        private void RemoveCustomFolder(string folderName)
        {
            _customFolders.Remove(folderName);
            RefreshCustomFoldersDisplay();
        }
        
        private void PreviewTemplate()
        {
            if (string.IsNullOrWhiteSpace(_templateNameField.value))
            {
                EditorUtility.DisplayDialog("Missing Template Name", "Please enter a template name first.", "OK");
                return;
            }
            
            string preview = CreateTemplatePreview();
            EditorUtility.DisplayDialog("Template Preview", preview, "OK");
        }
        
        private string CreateTemplatePreview()
        {
            System.Text.StringBuilder preview = new System.Text.StringBuilder();
            preview.AppendLine($"Template: {_templateNameField.value}");
            preview.AppendLine($"Description: {_templateDescriptionField.value}");
            preview.AppendLine($"Project Type: {_projectTypeField.value}");
            preview.AppendLine($"Total Folders: {_customFolders.Count}");
            preview.AppendLine();
            preview.AppendLine("Folder Structure:");
            
            List<string> sortedFolders = _customFolders.OrderBy(f => f).ToList();
            foreach (string folder in sortedFolders)
            {
                preview.AppendLine($"  📁 {folder}");
            }
            
            if (sortedFolders.Count == 0)
            {
                preview.AppendLine("  (No folders added yet)");
            }
            
            return preview.ToString();
        }
        
        private void SaveTemplate()
        {
            if (string.IsNullOrWhiteSpace(_templateNameField.value))
            {
                EditorUtility.DisplayDialog("Missing Template Name", "Please enter a template name first.", "OK");
                return;
            }
            
            if (_customFolders.Count == 0)
            {
                bool proceed = EditorUtility.DisplayDialog("No Folders", 
                    "No folders have been added to this template. Do you want to save an empty template?", 
                    "Save Anyway", "Cancel");
                if (!proceed) return;
            }
            
            _saveTemplateButton.SetEnabled(false);
            _templateStatusLabel.text = "💾 Saving template...";
            _templateStatusLabel.style.color = Color.yellow;
            
            try
            {
                // Create template configuration
                _currentTemplate = ScriptableObject.CreateInstance<TemplateConfigurationSO>();
                _currentTemplate.Initialize(
                    _templateNameField.value, 
                    _templateDescriptionField.value, 
                    (ProjectType)_projectTypeField.value
                );
                
                // Add folders to template
                foreach (string folderPath in _customFolders)
                {
                    _currentTemplate.FolderPaths.Add(folderPath);
                }
                
                // Save template as asset
                string templatePath = $"Assets/Templates/{SanitizeFileName(_templateNameField.value)}.asset";
                
                // Ensure Templates directory exists
                string directory = "Assets/Templates";
                if (!AssetDatabase.IsValidFolder(directory))
                {
                    AssetDatabase.CreateFolder("Assets", "Templates");
                }
                
                string uniquePath = AssetDatabase.GenerateUniqueAssetPath(templatePath);
                AssetDatabase.CreateAsset(_currentTemplate, uniquePath);
                AssetDatabase.SaveAssets();
                
                // Select the created template
                Selection.activeObject = _currentTemplate;
                EditorGUIUtility.PingObject(_currentTemplate);
                
                _templateStatusLabel.text = "✅ Template saved successfully!";
                _templateStatusLabel.style.color = Color.green;
                
                EditorUtility.DisplayDialog("Template Saved!", 
                    $"Smart template '{_templateNameField.value}' saved successfully!\n\n" +
                    $"Location: {uniquePath}\n" +
                    $"Folders: {_customFolders.Count}\n\n" +
                    "You can now use this template in the Game Concept Studio or apply it directly from the Assets menu.", 
                    "Great!");
                    
                Debug.Log($"✅ Created smart template '{_templateNameField.value}' with {_customFolders.Count} folders at {uniquePath}");
            }
            catch (System.Exception ex)
            {
                _templateStatusLabel.text = $"❌ Save failed: {ex.Message}";
                _templateStatusLabel.style.color = Color.red;
                Debug.LogError($"Template save failed: {ex}");
                
                EditorUtility.DisplayDialog("Save Failed", 
                    $"Template save failed: {ex.Message}", "OK");
            }
            
            _saveTemplateButton.SetEnabled(true);
        }
        
        private void CreateFoldersNow()
        {
            if (_customFolders.Count == 0)
            {
                EditorUtility.DisplayDialog("No Folders to Create", 
                    "No folders have been added yet. Please add some custom folders or use the smart suggestions before creating folders.", 
                    "OK");
                return;
            }
            
            // Create detailed preview with confirmation dialog
            string folderPreview = CreateFolderPreview();
            bool confirmed = EditorUtility.DisplayDialog("🚀 Create Folders - Preview & Confirmation", 
                folderPreview, 
                "✅ Create Folders Now", "❌ Cancel");
                
            if (!confirmed) return;
            
            _templateStatusLabel.text = "🚀 Creating folder structure...";
            _templateStatusLabel.style.color = Color.yellow;
            
            try
            {
                List<string> createdFolders = new List<string>();
                List<string> skippedFolders = new List<string>();
                
                foreach (string folderPath in _customFolders)
                {
                    if (CreateFolderIfNotExistsWithCheck(folderPath))
                    {
                        createdFolders.Add(folderPath);
                    }
                    else
                    {
                        skippedFolders.Add(folderPath);
                    }
                }
                
                // Refresh the Asset Database to show new folders
                AssetDatabase.Refresh();
                
                _templateStatusLabel.text = "✅ Folders created successfully!";
                _templateStatusLabel.style.color = Color.green;
                
                // Create detailed success message
                System.Text.StringBuilder resultMessage = new System.Text.StringBuilder();
                resultMessage.AppendLine($"🎉 Folder creation completed successfully!\n");
                
                if (createdFolders.Count > 0)
                {
                    resultMessage.AppendLine($"✅ Created {createdFolders.Count} new folders:");
                    foreach (string folder in createdFolders)
                    {
                        resultMessage.AppendLine($"  📁 Assets/{folder}");
                    }
                    resultMessage.AppendLine();
                }
                
                if (skippedFolders.Count > 0)
                {
                    resultMessage.AppendLine($"⏭️ Skipped {skippedFolders.Count} existing folders:");
                    foreach (string folder in skippedFolders)
                    {
                        resultMessage.AppendLine($"  📁 Assets/{folder}");
                    }
                    resultMessage.AppendLine();
                }
                
                resultMessage.AppendLine("📂 Check your Assets folder in the Project window to see the new folder structure.");
                resultMessage.AppendLine("💡 You can now start organizing your project files into these folders!");
                
                EditorUtility.DisplayDialog("🎉 Folders Created Successfully!", resultMessage.ToString(), "Great!");
                    
                Debug.Log($"✅ Created {createdFolders.Count} folders from Smart Template Creator (skipped {skippedFolders.Count} existing)");
            }
            catch (System.Exception ex)
            {
                _templateStatusLabel.text = $"❌ Folder creation failed: {ex.Message}";
                _templateStatusLabel.style.color = Color.red;
                Debug.LogError($"Folder creation failed: {ex}");
                
                EditorUtility.DisplayDialog("❌ Creation Failed", 
                    $"Folder creation failed: {ex.Message}\n\nCheck the Console for more details.", "OK");
            }
        }
        
        private string CreateFolderPreview()
        {
            System.Text.StringBuilder preview = new System.Text.StringBuilder();
            preview.AppendLine($"🚀 You are about to create {_customFolders.Count} folders in your Unity project:\n");
            
            ProjectType selectedType = (ProjectType)_projectTypeField.value;
            preview.AppendLine($"📋 Template: {_templateNameField.value}");
            preview.AppendLine($"🎯 Project Type: {selectedType}");
            preview.AppendLine($"📁 Total Folders: {_customFolders.Count}");
            preview.AppendLine();
            preview.AppendLine("📂 Folder Structure to Create:");
            
            List<string> sortedFolders = _customFolders.OrderBy(f => f).ToList();
            foreach (string folder in sortedFolders)
            {
                string assetsPath = $"Assets/{folder}";
                bool exists = AssetDatabase.IsValidFolder(assetsPath);
                string status = exists ? " (already exists - will skip)" : " (new)";
                string icon = exists ? "⏭️" : "📁";
                preview.AppendLine($"  {icon} {folder}{status}");
            }
            
            preview.AppendLine();
            preview.AppendLine("ℹ️  Information:");
            preview.AppendLine("  • All folders will be created in the Assets directory");
            preview.AppendLine("  • Existing folders will be skipped without modification");
            preview.AppendLine("  • Folder structure will be created with proper hierarchy");
            preview.AppendLine("  • Asset Database will be refreshed automatically");
            
            return preview.ToString();
        }
        
        private bool CreateFolderIfNotExistsWithCheck(string folderPath)
        {
            string assetsPath = "Assets/" + folderPath;
            
            if (AssetDatabase.IsValidFolder(assetsPath))
            {
                return false; // Folder already exists, skipped
            }
            
            // Split path and create parent directories if needed
            string[] pathParts = folderPath.Split('/');
            string currentPath = "Assets";
            
            for (int i = 0; i < pathParts.Length; i++)
            {
                string nextPath = currentPath + "/" + pathParts[i];
                
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    string guid = AssetDatabase.CreateFolder(currentPath, pathParts[i]);
                    if (string.IsNullOrEmpty(guid))
                    {
                        Debug.LogError($"Failed to create folder: {nextPath}");
                        return false;
                    }
                }
                
                currentPath = nextPath;
            }
            
            return true; // Folder was created successfully
        }
        
        private string SanitizeFileName(string fileName)
        {
            // Remove invalid characters for file names
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return System.Text.RegularExpressions.Regex.Replace(fileName, invalidRegStr, "_");
        }
        
        #endregion
    }
}