using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;
using UnityProjectArchitect.AI.Prompts;

namespace UnityProjectArchitect.Services.Testing
{
    /// <summary>
    /// Functional test for Step 3B: Prompt Engineering System
    /// Tests core functionality without external dependencies
    /// </summary>
    public static class Step3BFunctionalityTest
    {
        public static async Task<bool> RunAllTestsAsync()
        {
            Console.WriteLine("=== Step 3B: Prompt Engineering System Tests ===");
            
            bool allTestsPassed = true;
            
            // Test 1: SectionSpecificPrompts
            Console.WriteLine("Testing SectionSpecificPrompts...");
            if (TestSectionSpecificPrompts())
            {
                Console.WriteLine("✅ SectionSpecificPrompts: PASSED");
            }
            else
            {
                Console.WriteLine("❌ SectionSpecificPrompts: FAILED");
                allTestsPassed = false;
            }
            
            // Test 2: ContextBuilder
            Console.WriteLine("Testing ContextBuilder...");
            if (TestContextBuilder())
            {
                Console.WriteLine("✅ ContextBuilder: PASSED");
            }
            else
            {
                Console.WriteLine("❌ ContextBuilder: FAILED");
                allTestsPassed = false;
            }
            
            // Test 3: PromptOptimizer
            Console.WriteLine("Testing PromptOptimizer...");
            if (TestPromptOptimizer())
            {
                Console.WriteLine("✅ PromptOptimizer: PASSED");
            }
            else
            {
                Console.WriteLine("❌ PromptOptimizer: FAILED");
                allTestsPassed = false;
            }
            
            // Test 4: PromptTemplateManager
            Console.WriteLine("Testing PromptTemplateManager...");
            if (await TestPromptTemplateManagerAsync())
            {
                Console.WriteLine("✅ PromptTemplateManager: PASSED");
            }
            else
            {
                Console.WriteLine("❌ PromptTemplateManager: FAILED");
                allTestsPassed = false;
            }
            
            Console.WriteLine($"\n=== Step 3B Test Results: {(allTestsPassed ? "ALL PASSED" : "SOME FAILED")} ===");
            return allTestsPassed;
        }
        
        private static bool TestSectionSpecificPrompts()
        {
            try
            {
                // Test each section prompt generation
                string generalPrompt = SectionSpecificPrompts.GetGeneralDescriptionPrompt();
                string archPrompt = SectionSpecificPrompts.GetSystemArchitecturePrompt();
                string dataPrompt = SectionSpecificPrompts.GetDataModelPrompt();
                string apiPrompt = SectionSpecificPrompts.GetAPISpecificationPrompt();
                string userPrompt = SectionSpecificPrompts.GetUserStoriesPrompt();
                string workPrompt = SectionSpecificPrompts.GetWorkTicketsPrompt();
                
                // Validate prompts are not empty and contain expected elements
                if (string.IsNullOrEmpty(generalPrompt) || !generalPrompt.Contains("{PROJECT_CONTEXT}"))
                    return false;
                if (string.IsNullOrEmpty(archPrompt) || !archPrompt.Contains("architecture"))
                    return false;
                if (string.IsNullOrEmpty(dataPrompt) || !dataPrompt.Contains("data"))
                    return false;
                if (string.IsNullOrEmpty(apiPrompt) || !apiPrompt.Contains("API"))
                    return false;
                if (string.IsNullOrEmpty(userPrompt) || !userPrompt.Contains("user"))
                    return false;
                if (string.IsNullOrEmpty(workPrompt) || !workPrompt.Contains("task"))
                    return false;
                
                // Test contextual prompt building
                ProjectData testProject = CreateTestProjectData();
                string contextualPrompt = SectionSpecificPrompts.BuildContextualPrompt(generalPrompt, testProject);
                
                if (string.IsNullOrEmpty(contextualPrompt) || contextualPrompt == generalPrompt)
                    return false;
                
                // Test word count estimation
                int wordCount = SectionSpecificPrompts.GetEstimatedTokenCount(DocumentationSectionType.GeneralProductDescription);
                if (wordCount <= 0)
                    return false;
                    
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SectionSpecificPrompts test error: {ex.Message}");
                return false;
            }
        }
        
        private static bool TestContextBuilder()
        {
            try
            {
                ProjectData testProject = CreateTestProjectData();
                
                // Test basic project context building
                string projectContext = ContextBuilder.BuildProjectContext(testProject);
                if (string.IsNullOrEmpty(projectContext))
                    return false;
                
                // Verify context contains expected project information
                if (!projectContext.Contains("Test Project") || !projectContext.Contains("Unity2023_3"))
                    return false;
                
                // Test section context building
                DocumentationSectionData sectionData = new DocumentationSectionData
                {
                    SectionType = DocumentationSectionType.GeneralProductDescription,
                    WordCountTarget = 500,
                    Status = DocumentationStatus.InProgress
                };
                
                string sectionContext = ContextBuilder.BuildSectionContext(testProject, sectionData);
                if (string.IsNullOrEmpty(sectionContext))
                    return false;
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ContextBuilder test error: {ex.Message}");
                return false;
            }
        }
        
        private static bool TestPromptOptimizer()
        {
            try
            {
                PromptOptimizer optimizer = new PromptOptimizer();
                
                string testPrompt = "Generate a very comprehensive and detailed documentation for this Unity project that should include all the necessary information and details that developers need to understand the project structure and architecture.";
                
                // Test token estimation
                int tokenCount = optimizer.EstimateTokenCount(testPrompt);
                if (tokenCount <= 0)
                    return false;
                
                // Test prompt analysis
                PromptOptimizationAnalysis analysis = optimizer.AnalyzePrompt(testPrompt);
                if (analysis == null || analysis.EstimatedTokens != tokenCount)
                    return false;
                
                // Test prompt optimization
                PromptOptimizationRequest request = new PromptOptimizationRequest
                {
                    Goal = OptimizationGoal.TokenReduction,
                    TargetTokenLimit = 100
                };
                
                string optimizedPrompt = optimizer.OptimizePrompt(testPrompt, request);
                if (string.IsNullOrEmpty(optimizedPrompt) || optimizedPrompt.Length >= testPrompt.Length)
                    return false;
                
                // Test prompt validation
                PromptValidationResult validation = optimizer.ValidatePrompt(testPrompt);
                if (validation == null)
                    return false;
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PromptOptimizer test error: {ex.Message}");
                return false;
            }
        }
        
        private static async Task<bool> TestPromptTemplateManagerAsync()
        {
            try
            {
                PromptTemplateManager manager = new PromptTemplateManager();
                
                // Test getting default prompts
                string prompt = await manager.GetPromptAsync(DocumentationSectionType.GeneralProductDescription);
                if (string.IsNullOrEmpty(prompt))
                    return false;
                
                // Test prompt validation
                AIPromptValidationResult validation = await manager.ValidatePromptAsync(prompt);
                if (validation == null || !validation.IsValid)
                    return false;
                
                // Test custom prompt setting
                string customPrompt = "This is a test custom prompt with {PROJECT_CONTEXT} placeholder.";
                manager.SetPrompt(DocumentationSectionType.GeneralProductDescription, customPrompt);
                
                string retrievedPrompt = await manager.GetPromptAsync(DocumentationSectionType.GeneralProductDescription);
                if (retrievedPrompt != customPrompt)
                    return false;
                
                // Test available prompts listing
                List<string> availablePrompts = manager.GetAvailablePrompts();
                if (availablePrompts == null || availablePrompts.Count == 0)
                    return false;
                
                // Test cache functionality
                TemplateCacheStatistics stats = manager.GetCacheStatistics();
                if (stats == null)
                    return false;
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PromptTemplateManager test error: {ex.Message}");
                return false;
            }
        }
        
        private static ProjectData CreateTestProjectData()
        {
            return new ProjectData
            {
                ProjectName = "Test Project",
                ProjectDescription = "A test Unity project for validation",
                ProjectType = ProjectType.GameDevelopment,
                TargetUnityVersion = UnityVersion.Unity2023_3,
                TeamName = "Test Team",
                ContactEmail = "test@example.com",
                ProjectVersion = "1.0.0",
                UseAIAssistance = true,
                AIProvider = AIProvider.Claude
            };
        }
    }
}