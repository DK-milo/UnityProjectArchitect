using System;
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
    /// Unity Editor window for testing AI functionality with game descriptions
    /// Allows users to input game descriptions and generate documentation using AI
    /// </summary>
    public class AITestWindow : EditorWindow
    {
        private VisualElement _rootElement;
        private ScrollView _scrollView;
        private TextField _gameDescriptionField;
        private TextField _apiKeyField;
        private Button _testAIButton;
        private Button _generateDocsButton;
        private Label _statusLabel;
        private VisualElement _resultsContainer;
        private UnityProjectDataAsset _testProject;
        
        public static void ShowWindow()
        {
            AITestWindow window = GetWindow<AITestWindow>();
            window.titleContent = new GUIContent("AI Test Window", "Test AI functionality with game descriptions");
            window.minSize = new Vector2(600, 700);
            window.Show();
        }
        
        private void OnEnable()
        {
            // Initialize Unity service bridge
            UnityServiceBridge.Initialize();
            CreateUI();
        }

        private void CreateUI()
        {
            _rootElement = rootVisualElement;
            _rootElement.Clear();
            
            _scrollView = new ScrollView();
            _rootElement.Add(_scrollView);
            
            CreateHeaderSection();
            CreateGameDescriptionSection();
            CreateAIConfigSection();
            CreateActionsSection();
            CreateResultsSection();
        }

        private void CreateHeaderSection()
        {
            VisualElement headerContainer = new VisualElement();
            headerContainer.style.paddingBottom = 15;
            headerContainer.style.paddingTop = 10;
            headerContainer.style.paddingLeft = 10;
            headerContainer.style.paddingRight = 10;
            
            Label titleLabel = new Label("🤖 AI Test Window");
            titleLabel.style.fontSize = 18;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            Label descLabel = new Label("Test AI-powered documentation generation from game descriptions");
            descLabel.style.fontSize = 12;
            descLabel.style.color = Color.gray;
            descLabel.style.marginTop = 5;
            
            headerContainer.Add(titleLabel);
            headerContainer.Add(descLabel);
            
            _scrollView.Add(headerContainer);
            
            HelpBox infoBox = new HelpBox(
                "Describe your Unity game idea below, and the AI will help generate comprehensive documentation including user stories, work tickets, and technical specifications.",
                HelpBoxMessageType.Info);
            infoBox.style.marginBottom = 15;
            _scrollView.Add(infoBox);
        }

        private void CreateGameDescriptionSection()
        {
            Foldout descriptionFoldout = new Foldout { text = "🎮 Game Description", value = true };
            descriptionFoldout.style.marginBottom = 15;
            
            Label instructionLabel = new Label("Describe your game concept:");
            instructionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            instructionLabel.style.marginBottom = 5;
            
            _gameDescriptionField = new TextField()
            {
                multiline = true,
                value = GetExampleGameDescription()
            };
            _gameDescriptionField.style.height = 150;
            _gameDescriptionField.style.marginBottom = 10;
            
            Button exampleButton = new Button(() => {
                _gameDescriptionField.value = GetExampleGameDescription();
            }) { text = "📝 Use Example Game Description" };
            
            Button clearButton = new Button(() => {
                _gameDescriptionField.value = "";
            }) { text = "🗑️ Clear" };
            clearButton.style.marginLeft = 5;
            
            VisualElement buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.Add(exampleButton);
            buttonContainer.Add(clearButton);
            
            descriptionFoldout.Add(instructionLabel);
            descriptionFoldout.Add(_gameDescriptionField);
            descriptionFoldout.Add(buttonContainer);
            
            _scrollView.Add(descriptionFoldout);
        }

        private void CreateAIConfigSection()
        {
            Foldout configFoldout = new Foldout { text = "⚙️ AI Configuration", value = false };
            configFoldout.style.marginBottom = 15;
            
            Label keyLabel = new Label("Claude API Key (Optional - uses fallback generators if not provided):");
            keyLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            keyLabel.style.marginBottom = 5;
            
            _apiKeyField = new TextField()
            {
                isPasswordField = true,
                value = EditorPrefs.GetString("UnityProjectArchitect.ClaudeAPIKey", "")
            };
            _apiKeyField.RegisterValueChangedCallback(evt => {
                EditorPrefs.SetString("UnityProjectArchitect.ClaudeAPIKey", evt.newValue);
            });
            
            Label statusLabel = new Label("AI Status: Checking...");
            statusLabel.style.marginTop = 10;
            statusLabel.style.fontSize = 12;
            
            _testAIButton = new Button(TestAIConnection) { text = "🔍 Test AI Connection" };
            _testAIButton.style.marginTop = 10;
            
            configFoldout.Add(keyLabel);
            configFoldout.Add(_apiKeyField);
            configFoldout.Add(statusLabel);
            configFoldout.Add(_testAIButton);
            
            _scrollView.Add(configFoldout);
        }

        private void CreateActionsSection()
        {
            VisualElement actionsContainer = new VisualElement();
            actionsContainer.style.marginBottom = 15;
            
            Label actionsLabel = new Label("🚀 Actions");
            actionsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            actionsLabel.style.marginBottom = 10;
            
            _generateDocsButton = new Button(GenerateDocumentationFromDescription) 
            { 
                text = "✨ Generate Documentation with AI" 
            };
            _generateDocsButton.style.height = 40;
            _generateDocsButton.style.fontSize = 14;
            _generateDocsButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            _statusLabel = new Label("Ready to generate documentation");
            _statusLabel.style.marginTop = 10;
            _statusLabel.style.fontSize = 12;
            _statusLabel.style.color = Color.green;
            
            actionsContainer.Add(actionsLabel);
            actionsContainer.Add(_generateDocsButton);
            actionsContainer.Add(_statusLabel);
            
            _scrollView.Add(actionsContainer);
        }

        private void CreateResultsSection()
        {
            Foldout resultsFoldout = new Foldout { text = "📋 Generated Results", value = true };
            resultsFoldout.style.marginBottom = 15;
            
            _resultsContainer = new VisualElement();
            _resultsContainer.style.marginTop = 10;
            
            resultsFoldout.Add(_resultsContainer);
            _scrollView.Add(resultsFoldout);
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

        private void TestAIConnection()
        {
            _statusLabel.text = "Testing AI connection...";
            _statusLabel.style.color = Color.yellow;
            _testAIButton.SetEnabled(false);
            
            try
            {
                IAIAssistant aiAssistant = UnityServiceBridge.GetAIAssistant();
                
                // Test if AI is configured
                if (aiAssistant.IsConfigured)
                {
                    _statusLabel.text = "✅ AI Connection successful! Claude API is ready.";
                    _statusLabel.style.color = Color.green;
                }
                else
                {
                    _statusLabel.text = "⚠️ AI not configured. Will use fallback generators (still functional).";
                    _statusLabel.style.color = Color.yellow;
                }
            }
            catch (Exception ex)
            {
                _statusLabel.text = $"❌ AI test failed: {ex.Message}";
                _statusLabel.style.color = Color.red;
                Debug.LogError($"AI connection test failed: {ex}");
            }
            
            _testAIButton.SetEnabled(true);
        }

        private async void GenerateDocumentationFromDescription()
        {
            if (string.IsNullOrWhiteSpace(_gameDescriptionField.value))
            {
                EditorUtility.DisplayDialog("Missing Description", "Please enter a game description first.", "OK");
                return;
            }

            _generateDocsButton.SetEnabled(false);
            _statusLabel.text = "🔄 Creating test project from game description...";
            _statusLabel.style.color = Color.yellow;
            _resultsContainer.Clear();
            
            try
            {
                // Create a test project based on the game description
                _testProject = CreateTestProjectFromDescription(_gameDescriptionField.value);
                
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
                    
                    string content = await docService.GenerateDocumentationSectionAsync(section, _testProject.ProjectData);
                    
                    CreateResultCard(sectionType.ToString(), content);
                }
                
                _statusLabel.text = "✅ Documentation generation complete!";
                _statusLabel.style.color = Color.green;
                
                // Show success message
                EditorUtility.DisplayDialog("Success!", 
                    "AI-powered documentation has been generated successfully! Check the results below.", 
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

        private UnityProjectDataAsset CreateTestProjectFromDescription(string description)
        {
            // Create a temporary test project
            UnityProjectDataAsset testProject = CreateInstance<UnityProjectDataAsset>();
            testProject.Initialize();
            
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
            
            testProject.UpdateProjectName(gameName);
            testProject.ProjectData.ProjectDescription = description;
            testProject.ProjectData.ProjectType = ProjectType.Game3D;
            
            return testProject;
        }

        private void CreateResultCard(string title, string content)
        {
            VisualElement card = new VisualElement();
            card.style.marginBottom = 15;
            card.style.paddingTop = 10;
            card.style.paddingBottom = 10;
            card.style.paddingLeft = 10;
            card.style.paddingRight = 10;
            card.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
            card.style.borderTopLeftRadius = 5;
            card.style.borderTopRightRadius = 5;
            card.style.borderBottomLeftRadius = 5;
            card.style.borderBottomRightRadius = 5;
            
            // Collapsible section for each generated result
            Foldout sectionFoldout = new Foldout { text = $"📄 {title}", value = false };
            sectionFoldout.style.marginBottom = 5;
            
            TextField contentField = new TextField()
            {
                multiline = true,
                value = content,
                isReadOnly = true
            };
            contentField.style.height = 200;
            
            int wordCount = string.IsNullOrEmpty(content) ? 0 : content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            Label statsLabel = new Label($"Generated: {content.Length:N0} characters, {wordCount:N0} words");
            statsLabel.style.fontSize = 10;
            statsLabel.style.color = Color.gray;
            statsLabel.style.marginTop = 5;
            
            sectionFoldout.Add(contentField);
            sectionFoldout.Add(statsLabel);

            card.Add(sectionFoldout);
            
            _resultsContainer.Add(card);
        }
    }
}