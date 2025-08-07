using System;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;
using UnityProjectArchitect.AI.Services;

namespace UnityProjectArchitect.Services.Testing
{
    /// <summary>
    /// Test utility to validate AI integration and API key configuration
    /// </summary>
    public static class AIIntegrationValidationTest
    {
        /// <summary>
        /// Test API key retrieval and AI assistant configuration
        /// </summary>
        public static async Task<ValidationResult> TestAIConfigurationAsync()
        {
            try
            {
                // Test Unity bridge initialization
                #if UNITY_EDITOR
                UnityProjectArchitect.Unity.UnityServiceBridge.Initialize();
                IAIAssistant aiAssistant = UnityProjectArchitect.Unity.UnityServiceBridge.GetAIAssistant();
                
                UnityEngine.Debug.Log("🧪 Starting AI integration validation test...");
                
                // Test 1: API Key Manager with Unity settings
                UnityEngine.Debug.Log("🔑 Testing API key retrieval from Unity EditorPrefs...");
                UnityConsoleLogger logger = new UnityConsoleLogger();
                UnityEditorSettingsProvider settingsProvider = new UnityEditorSettingsProvider(logger);
                APIKeyManager keyManager = new APIKeyManager(settingsProvider, logger);
                
                bool hasKey = keyManager.HasClaudeAPIKey();
                UnityEngine.Debug.Log($"API key detected: {(hasKey ? "✅ Yes" : "❌ No")}");
                
                if (hasKey)
                {
                    string apiKey = keyManager.GetClaudeAPIKey();
                    string keyPreview = !string.IsNullOrEmpty(apiKey) ? 
                        $"sk-...{apiKey.Substring(Math.Max(0, apiKey.Length - 6))}" : 
                        "Invalid";
                    UnityEngine.Debug.Log($"API key preview: {keyPreview}");
                }
                
                // Test 2: AI Assistant configuration
                UnityEngine.Debug.Log("🤖 Testing AI Assistant configuration...");
                bool isConfigured = aiAssistant?.IsConfigured == true;
                UnityEngine.Debug.Log($"AI Assistant configured: {(isConfigured ? "✅ Yes" : "❌ No")}");
                
                // Test 3: Simple AI request (if configured)
                if (isConfigured)
                {
                    UnityEngine.Debug.Log("📤 Testing simple AI request...");
                    
                    AIRequest testRequest = new AIRequest
                    {
                        RequestType = AIRequestType.Generation,
                        Prompt = "Respond with exactly 'Hello from Unity Project Architect!' to confirm the connection is working.",
                        Configuration = new AIConfiguration
                        {
                            Provider = AIProvider.Claude,
                            MaxTokens = 50,
                            Temperature = 0.1f
                        }
                    };
                    
                    AIOperationResult result = await aiAssistant.GenerateContentAsync(testRequest);
                    
                    if (result.Success)
                    {
                        UnityEngine.Debug.Log($"✅ AI request successful! Response: {result.Content}");
                        UnityEngine.Debug.Log($"Processing time: {result.ProcessingTime.TotalSeconds:F2}s");
                        
                        return ValidationResult.Success("AI integration is working correctly");
                    }
                    else
                    {
                        UnityEngine.Debug.LogError($"❌ AI request failed: {result.ErrorMessage}");
                        return ValidationResult.Failure($"AI request failed: {result.ErrorMessage}");
                    }
                }
                else
                {
                    string reason = !hasKey ? "No API key found" : "AI Assistant not configured";
                    UnityEngine.Debug.LogWarning($"⚠️ Cannot test AI request: {reason}");
                    return ValidationResult.Failure($"AI integration not ready: {reason}");
                }
                
                #else
                return ValidationResult.Failure("AI integration test only available in Unity Editor");
                #endif
            }
            catch (Exception ex)
            {
                string errorMessage = $"AI integration test failed: {ex.Message}";
                #if UNITY_EDITOR
                UnityEngine.Debug.LogError(errorMessage);
                if (ex.InnerException != null)
                {
                    UnityEngine.Debug.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                #else
                Console.WriteLine(errorMessage);
                #endif
                
                return ValidationResult.Failure(errorMessage);
            }
        }
        
        /// <summary>
        /// Test generator AI integration specifically
        /// </summary>
        public static async Task<ValidationResult> TestGeneratorAIIntegrationAsync()
        {
            try
            {
                #if UNITY_EDITOR
                UnityEngine.Debug.Log("🔧 Testing generator AI integration...");
                
                // Test concept-aware generator
                var userStoriesGenerator = new UnityProjectArchitect.Services.ConceptAware.ConceptualUserStoriesGenerator(
                    "**Mario-style Platformer**\n\nA classic 2D platformer game inspired by Super Mario Bros."
                );
                
                string content = await userStoriesGenerator.GenerateContentAsync();
                
                bool hasAIContent = content.Contains("Claude AI") || content.Contains("AI Provider");
                UnityEngine.Debug.Log($"Generator AI integration: {(hasAIContent ? "✅ Working" : "⚠️ Using fallback")}");
                
                // Test regular generator  
                ProjectAnalysisResult mockAnalysis = new ProjectAnalysisResult
                {
                    Success = true,
                    ProjectPath = UnityEngine.Application.dataPath,
                    AnalyzedAt = DateTime.Now
                };
                
                var productDescGenerator = new GeneralProductDescriptionGenerator(mockAnalysis);
                string productContent = await productDescGenerator.GenerateContentAsync();
                
                bool hasProductAI = productContent.Contains("AI Provider") || productContent.Contains("Claude AI");
                UnityEngine.Debug.Log($"Product Description AI: {(hasProductAI ? "✅ Working" : "⚠️ Using fallback")}");
                
                if (hasAIContent || hasProductAI)
                {
                    return ValidationResult.Success("Generator AI integration is working");
                }
                else
                {
                    return ValidationResult.Success("Generators are working but using fallback (no AI configuration)");
                }
                
                #else
                return ValidationResult.Failure("Generator test only available in Unity Editor");
                #endif
            }
            catch (Exception ex)
            {
                string errorMessage = $"Generator AI integration test failed: {ex.Message}";
                #if UNITY_EDITOR
                UnityEngine.Debug.LogError(errorMessage);
                #else
                Console.WriteLine(errorMessage);
                #endif
                
                return ValidationResult.Failure(errorMessage);
            }
        }
    }
}
