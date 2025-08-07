using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnityProjectArchitect.Unity;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Unity.Tests
{
    /// <summary>
    /// Integration tests for DLL services in Unity environment
    /// Tests that the hybrid DLL + Unity package architecture works correctly
    /// </summary>
    public class DLLIntegrationTest
    {
        [SetUp]
        public void Setup()
        {
            // Initialize Unity service bridge before each test
            UnityServiceBridge.Initialize();
        }

        [Test]
        public void TestServiceBridgeInitialization()
        {
            // Test that services can be retrieved without throwing exceptions
            Assert.DoesNotThrow(() =>
            {
                IProjectAnalyzer projectAnalyzer = UnityServiceBridge.GetProjectAnalyzer();
                Assert.IsNotNull(projectAnalyzer, "ProjectAnalyzer should not be null");
                
                IExportService exportService = UnityServiceBridge.GetExportService();
                Assert.IsNotNull(exportService, "ExportService should not be null");
                
                ITemplateManager templateManager = UnityServiceBridge.GetTemplateManager();
                Assert.IsNotNull(templateManager, "TemplateManager should not be null");
                
                IDocumentationGenerator documentationService = UnityServiceBridge.GetDocumentationService();
                Assert.IsNotNull(documentationService, "DocumentationService should not be null");
            });
        }

        [UnityTest]
        public System.Collections.IEnumerator TestProjectAnalyzerBasicFunctionality()
        {
            IProjectAnalyzer analyzer = UnityServiceBridge.GetProjectAnalyzer();
            Assert.IsNotNull(analyzer, "ProjectAnalyzer should be available");

            // Test basic validation (should not throw)
            Task<ValidationResult> validationTask = analyzer.ValidateProjectStructureAsync(Application.dataPath);
            
            yield return new WaitUntil(() => validationTask.IsCompleted);
            
            Assert.IsTrue(validationTask.IsCompletedSuccessfully, "Validation task should complete successfully");
            
            ValidationResult result = validationTask.Result;
            Assert.IsNotNull(result, "Validation result should not be null");
            
            Debug.Log($"✅ Project validation completed: {result.IsValid}");
        }

        [UnityTest]
        public System.Collections.IEnumerator TestExportServiceBasicFunctionality()
        {
            IExportService exportService = UnityServiceBridge.GetExportService();
            Assert.IsNotNull(exportService, "ExportService should be available");

            // Test getting supported formats
            List<ExportFormat> formats = exportService.GetSupportedFormats();
            Assert.IsNotNull(formats, "Supported formats should not be null");
            Assert.IsTrue(formats.Count > 0, "Should support at least one export format");
            
            Debug.Log($"✅ Export service supports {formats.Count} formats");
            
            // Test capabilities
            ExportCapabilities capabilities = exportService.GetCapabilities();
            Assert.IsNotNull(capabilities, "Capabilities should not be null");
            
            yield return null; // Complete immediately for this test
        }

        [Test]
        public void TestDLLTypesAreAccessible()
        {
            // Test that we can create instances of DLL types
            Assert.DoesNotThrow(() =>
            {
                ProjectData projectData = new ProjectData();
                Assert.IsNotNull(projectData, "ProjectData should be creatable");
                
                ValidationResult validationResult = new ValidationResult();
                Assert.IsNotNull(validationResult, "ValidationResult should be creatable");
                
                ExportRequest exportRequest = new ExportRequest(ExportFormat.Markdown, "test/path");
                Assert.IsNotNull(exportRequest, "ExportRequest should be creatable");
                Assert.AreEqual(ExportFormat.Markdown, exportRequest.Format);
                
                Debug.Log("✅ All DLL types are accessible from Unity");
            });
        }

        [Test]
        public void TestEnumValues()
        {
            // Test that enums from DLLs work correctly
            Assert.DoesNotThrow(() =>
            {
                ProjectType projectType = ProjectType.Game3D;
                UnityVersion unityVersion = UnityVersion.Unity2023_3;
                DocumentationStatus docStatus = DocumentationStatus.InProgress;
                ExportFormat exportFormat = ExportFormat.PDF;
                
                Debug.Log($"✅ Enum values accessible: {projectType}, {unityVersion}, {docStatus}, {exportFormat}");
            });
        }

        [UnityTest]
        public System.Collections.IEnumerator TestUserStoriesGeneratorConceptOnly()
        {
            UnityDocumentationService documentationService = UnityServiceBridge.GetDocumentationService();
            Assert.IsNotNull(documentationService, "DocumentationService should be available");

            // Create test project data that will be detected as concept project
            ProjectData conceptProjectData = new ProjectData();
            conceptProjectData.UpdateProjectName("AI Generated Test Concept");
            conceptProjectData.UpdateDescription("A 2D platformer game like Mario Bros where the player collects coins and jumps on enemies. The game features multiple levels with increasing difficulty and power-ups that give special abilities.");
            conceptProjectData.UpdateProjectType(ProjectType.Game2D);

            // Create User Stories section
            DocumentationSectionData userStoriesSection = new DocumentationSectionData
            {
                SectionType = DocumentationSectionType.UserStories,
                IsEnabled = true,
                Status = DocumentationStatus.NotStarted
            };

            // Test User Stories generation for concept project (should work)
            Task<string> generationTask = documentationService.GenerateDocumentationSectionAsync(userStoriesSection, conceptProjectData);
            
            yield return new WaitUntil(() => generationTask.IsCompleted);
            
            Assert.IsTrue(generationTask.IsCompletedSuccessfully, "User Stories generation for concept should complete successfully");
            
            string result = generationTask.Result;
            Assert.IsNotNull(result, "User Stories content should not be null");
            Assert.IsTrue(result.Length > 100, "User Stories content should have substantial length");
            Assert.IsTrue(result.Contains("User Stories"), "Content should contain User Stories header");
            
            Debug.Log($"✅ User Stories generated for concept project ({result.Length:N0} characters)");
            
            // Now test that regular project analysis throws exception
            ProjectData regularProjectData = new ProjectData();
            regularProjectData.UpdateProjectName("Regular Unity Project");
            regularProjectData.UpdateDescription(""); // No description = regular project
            regularProjectData.UpdateProjectType(ProjectType.Game3D);
            
            Task<string> regularGenerationTask = documentationService.GenerateDocumentationSectionAsync(userStoriesSection, regularProjectData);
            yield return new WaitUntil(() => regularGenerationTask.IsCompleted);
            
            string regularResult = regularGenerationTask.Result;
            Assert.IsTrue(regularResult.Contains("only available for game concepts"), "Should indicate UserStories is concept-only");
            
            Debug.Log($"✅ Confirmed UserStories blocked for regular project analysis");
        }

        [UnityTest]
        public System.Collections.IEnumerator TestWorkTicketsGeneratorConceptOnly()
        {
            UnityDocumentationService documentationService = UnityServiceBridge.GetDocumentationService();
            Assert.IsNotNull(documentationService, "DocumentationService should be available");

            // Create test project data that will be detected as concept project
            ProjectData conceptProjectData = new ProjectData();
            conceptProjectData.UpdateProjectName("AI Generated Test Concept");
            conceptProjectData.UpdateDescription("A 2D platformer game like Mario Bros where the player collects coins and jumps on enemies. The game features multiple levels with increasing difficulty and power-ups that give special abilities.");
            conceptProjectData.UpdateProjectType(ProjectType.Game2D);

            // Create Work Tickets section
            DocumentationSectionData workTicketsSection = new DocumentationSectionData
            {
                SectionType = DocumentationSectionType.WorkTickets,
                IsEnabled = true,
                Status = DocumentationStatus.NotStarted
            };

            // Test Work Tickets generation for concept project (should work)
            Task<string> generationTask = documentationService.GenerateDocumentationSectionAsync(workTicketsSection, conceptProjectData);
            
            yield return new WaitUntil(() => generationTask.IsCompleted);
            
            Assert.IsTrue(generationTask.IsCompletedSuccessfully, "Work Tickets generation for concept should complete successfully");
            
            string result = generationTask.Result;
            Assert.IsNotNull(result, "Work Tickets content should not be null");
            Assert.IsTrue(result.Length > 100, "Work Tickets content should have substantial length");
            Assert.IsTrue(result.Contains("Work Tickets"), "Content should contain Work Tickets header");
            
            Debug.Log($"✅ Work Tickets generated for concept project ({result.Length:N0} characters)");
            
            // Now test that regular project analysis throws exception
            ProjectData regularProjectData = new ProjectData();
            regularProjectData.UpdateProjectName("Regular Unity Project");
            regularProjectData.UpdateDescription(""); // No description = regular project
            regularProjectData.UpdateProjectType(ProjectType.Game3D);
            
            Task<string> regularGenerationTask = documentationService.GenerateDocumentationSectionAsync(workTicketsSection, regularProjectData);
            yield return new WaitUntil(() => regularGenerationTask.IsCompleted);
            
            string regularResult = regularGenerationTask.Result;
            Assert.IsTrue(regularResult.Contains("only available for game concepts"), "Should indicate WorkTickets is concept-only");
            
            Debug.Log($"✅ Confirmed WorkTickets blocked for regular project analysis");
        }

        [UnityTest]
        public System.Collections.IEnumerator TestProjectAnalysisGeneratorsIntegration()
        {
            UnityDocumentationService documentationService = UnityServiceBridge.GetDocumentationService();
            Assert.IsNotNull(documentationService, "DocumentationService should be available");

            // Create test project data (regular project, not concept)
            ProjectData projectData = new ProjectData();
            projectData.UpdateProjectName("Full Test Project");
            projectData.UpdateDescription(""); // No description = regular project analysis mode
            projectData.UpdateProjectType(ProjectType.Game3D);

            // Test project analysis documentation section types (NOT UserStories/WorkTickets)
            DocumentationSectionType[] projectAnalysisSectionTypes = new[]
            {
                DocumentationSectionType.GeneralProductDescription,
                DocumentationSectionType.SystemArchitecture,
                DocumentationSectionType.DataModel,
                DocumentationSectionType.APISpecification
            };

            int completedGenerators = 0;
            int totalGenerators = projectAnalysisSectionTypes.Length;

            foreach (DocumentationSectionType sectionType in projectAnalysisSectionTypes)
            {
                if (documentationService.CanGenerateSection(sectionType))
                {
                    DocumentationSectionData section = new DocumentationSectionData
                    {
                        SectionType = sectionType,
                        IsEnabled = true,
                        Status = DocumentationStatus.NotStarted
                    };

                    Debug.Log($"Testing project analysis {sectionType} generator...");

                    Task<string> generationTask = documentationService.GenerateDocumentationSectionAsync(section, projectData);
                    yield return new WaitUntil(() => generationTask.IsCompleted);

                    if (generationTask.IsCompletedSuccessfully)
                    {
                        string result = generationTask.Result;
                        Assert.IsNotNull(result, $"{sectionType} content should not be null");
                        Assert.IsTrue(result.Length > 50, $"{sectionType} content should have meaningful length");
                        
                        completedGenerators++;
                        Debug.Log($"✅ {sectionType} generator working ({result.Length:N0} characters)");
                    }
                    else
                    {
                        Debug.LogError($"❌ {sectionType} generator failed: {generationTask.Exception?.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"⚠️ {sectionType} generator not supported");
                }

                // Yield control to prevent Unity from freezing
                yield return new WaitForSeconds(0.1f);
            }

            Debug.Log($"✅ Project analysis integration test complete: {completedGenerators}/{totalGenerators} generators working");
            Assert.IsTrue(completedGenerators >= 3, $"At least 3 project analysis generators should work, got {completedGenerators}");
        }
    }
}