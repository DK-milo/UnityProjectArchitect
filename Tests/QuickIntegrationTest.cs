using System;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;
using UnityProjectArchitect.AI.Services;
using UnityProjectArchitect.AI.Prompts;

namespace UnityProjectArchitect.Tests
{
    /// <summary>
    /// Quick integration test to verify Step 3C: AI Assistant Interface components work together
    /// </summary>
    public class QuickIntegrationTest
    {
        public static async Task<bool> RunQuickTestAsync()
        {
            Console.WriteLine("=== Quick AI Assistant Integration Test ===");
            
            try
            {
                // Test 1: Initialize components
                Console.WriteLine("1. Initializing AI components...");
                ILogger logger = new ConsoleLogger();
                AIAssistant aiAssistant = new AIAssistant(logger);
                PromptTemplateManager promptManager = new PromptTemplateManager(logger, GetTestTemplatesPath());
                ConversationManager conversationManager = new ConversationManager(logger);
                ContentValidator contentValidator = new ContentValidator(logger);
                OfflineFallbackManager offlineFallbackManager = new OfflineFallbackManager(logger);

                // Test 2: Check basic functionality
                Console.WriteLine("2. Testing basic component functionality...");
                
                // Test AI Assistant
                List<AIProvider> supportedProviders = aiAssistant.GetSupportedProviders();
                if (supportedProviders.Count == 0)
                {
                    Console.WriteLine("❌ No supported AI providers found");
                    return false;
                }
                Console.WriteLine($"✅ AI Assistant supports {supportedProviders.Count} provider(s)");

                // Test Prompt Manager
                List<string> availablePrompts = promptManager.GetAvailablePrompts();
                if (availablePrompts.Count == 0)
                {
                    Console.WriteLine("❌ No prompt templates available");
                    return false;
                }
                Console.WriteLine($"✅ Prompt Manager has {availablePrompts.Count} template(s)");

                // Test Conversation Manager
                string conversationId = await conversationManager.StartConversationAsync("test_user", "Test context");
                if (string.IsNullOrEmpty(conversationId))
                {
                    Console.WriteLine("❌ Failed to start conversation");
                    return false;
                }
                Console.WriteLine($"✅ Conversation Manager started conversation: {conversationId}");

                // Test Content Validator
                string testContent = "# Test Unity Project\n\nThis is a test Unity project for validation.";
                ContentValidationResult validationResult = await contentValidator.ValidateContentAsync(testContent, DocumentationSectionType.GeneralProductDescription);
                if (validationResult == null)
                {
                    Console.WriteLine("❌ Content validation failed");
                    return false;
                }
                Console.WriteLine($"✅ Content Validator working - Content valid: {validationResult.IsValid}");

                // Test Offline Fallback Manager
                bool fallbackActivated = await offlineFallbackManager.ActivateFallbackModeAsync("Testing");
                if (!fallbackActivated)
                {
                    Console.WriteLine("❌ Failed to activate offline fallback");
                    return false;
                }
                Console.WriteLine("✅ Offline Fallback Manager activated successfully");

                // Test 3: Integration workflow
                Console.WriteLine("3. Testing integrated workflow...");
                
                ProjectData testProjectData = new ProjectData
                {
                    ProjectName = "Test Integration Project",
                    ProjectDescription = "A test project for AI integration validation",
                    TargetUnityVersion = UnityVersion.Unity2023_3,
                    UseAIAssistance = true,
                    AIProvider = AIProvider.Claude
                };

                // Test offline content generation
                AIRequest testRequest = new AIRequest("Generate test documentation", testProjectData)
                {
                    SectionType = DocumentationSectionType.GeneralProductDescription,
                    TargetWordCount = 200
                };

                AIOperationResult offlineResult = await offlineFallbackManager.GenerateOfflineContentAsync(testRequest);
                if (!offlineResult.Success)
                {
                    Console.WriteLine($"❌ Offline content generation failed: {offlineResult.ErrorMessage}");
                    return false;
                }
                Console.WriteLine($"✅ Offline content generated: {offlineResult.Content.Length} characters");

                // Test conversation functionality
                bool messageAdded = await conversationManager.AddMessageAsync(conversationId, "user", "Generate documentation for my Unity project");
                if (!messageAdded)
                {
                    Console.WriteLine("❌ Failed to add message to conversation");
                    return false;
                }
                Console.WriteLine("✅ Message added to conversation successfully");

                ConversationHistory conversationHistory = await conversationManager.GetConversationHistoryAsync(conversationId);
                if (conversationHistory == null || conversationHistory.Messages.Count == 0)
                {
                    Console.WriteLine("❌ Failed to retrieve conversation history");
                    return false;
                }
                Console.WriteLine($"✅ Retrieved conversation history with {conversationHistory.Messages.Count} message(s)");

                // Clean up
                await conversationManager.EndConversationAsync(conversationId);
                await offlineFallbackManager.DeactivateFallbackModeAsync();

                Console.WriteLine("=== Integration Test PASSED ===");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Integration test failed with exception: {ex.Message}");
                return false;
            }
        }

        private static string GetTestTemplatesPath()
        {
            return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "Templates");
        }
    }

}