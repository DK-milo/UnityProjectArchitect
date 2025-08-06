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
                var projectAnalyzer = UnityServiceBridge.GetProjectAnalyzer();
                Assert.IsNotNull(projectAnalyzer, "ProjectAnalyzer should not be null");
                
                var exportService = UnityServiceBridge.GetExportService();
                Assert.IsNotNull(exportService, "ExportService should not be null");
                
                var templateManager = UnityServiceBridge.GetTemplateManager();
                Assert.IsNotNull(templateManager, "TemplateManager should not be null");
                
                var documentationService = UnityServiceBridge.GetDocumentationService();
                Assert.IsNotNull(documentationService, "DocumentationService should not be null");
            });
        }

        [UnityTest]
        public System.Collections.IEnumerator TestProjectAnalyzerBasicFunctionality()
        {
            var analyzer = UnityServiceBridge.GetProjectAnalyzer();
            Assert.IsNotNull(analyzer, "ProjectAnalyzer should be available");

            // Test basic validation (should not throw)
            var validationTask = analyzer.ValidateProjectStructureAsync(Application.dataPath);
            
            yield return new WaitUntil(() => validationTask.IsCompleted);
            
            Assert.IsTrue(validationTask.IsCompletedSuccessfully, "Validation task should complete successfully");
            
            var result = validationTask.Result;
            Assert.IsNotNull(result, "Validation result should not be null");
            
            Debug.Log($"✅ Project validation completed: {result.IsValid}");
        }

        [UnityTest]
        public System.Collections.IEnumerator TestExportServiceBasicFunctionality()
        {
            var exportService = UnityServiceBridge.GetExportService();
            Assert.IsNotNull(exportService, "ExportService should be available");

            // Test getting supported formats
            var formats = exportService.GetSupportedFormats();
            Assert.IsNotNull(formats, "Supported formats should not be null");
            Assert.IsTrue(formats.Count > 0, "Should support at least one export format");
            
            Debug.Log($"✅ Export service supports {formats.Count} formats");
            
            // Test capabilities
            var capabilities = exportService.GetCapabilities();
            Assert.IsNotNull(capabilities, "Capabilities should not be null");
            
            yield return null; // Complete immediately for this test
        }

        [Test]
        public void TestDLLTypesAreAccessible()
        {
            // Test that we can create instances of DLL types
            Assert.DoesNotThrow(() =>
            {
                var projectData = new ProjectData();
                Assert.IsNotNull(projectData, "ProjectData should be creatable");
                
                var validationResult = new ValidationResult();
                Assert.IsNotNull(validationResult, "ValidationResult should be creatable");
                
                var exportRequest = new ExportRequest(ExportFormat.Markdown, "test/path");
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
                var projectType = ProjectType.Game3D;
                var unityVersion = UnityVersion.Unity2023_3;
                var docStatus = DocumentationStatus.InProgress;
                var exportFormat = ExportFormat.PDF;
                
                Debug.Log($"✅ Enum values accessible: {projectType}, {unityVersion}, {docStatus}, {exportFormat}");
            });
        }

        [UnityTest]
        public System.Collections.IEnumerator TestUserStoriesGeneratorIntegration()
        {
            var documentationService = UnityServiceBridge.GetDocumentationService();
            Assert.IsNotNull(documentationService, "DocumentationService should be available");

            // Create test project data
            var projectData = new ProjectData();
            projectData.UpdateProjectName("Test Unity Project");
            projectData.UpdateDescription("Test project for User Stories generation");
            projectData.UpdateProjectType(ProjectType.Game3D);

            // Create User Stories section
            var userStoriesSection = new DocumentationSectionData
            {
                SectionType = DocumentationSectionType.UserStories,
                IsEnabled = true,
                Status = DocumentationStatus.NotStarted
            };

            // Test User Stories generation
            var generationTask = documentationService.GenerateDocumentationSectionAsync(userStoriesSection, projectData);
            
            yield return new WaitUntil(() => generationTask.IsCompleted);
            
            Assert.IsTrue(generationTask.IsCompletedSuccessfully, "User Stories generation task should complete successfully");
            
            string result = generationTask.Result;
            Assert.IsNotNull(result, "User Stories content should not be null");
            Assert.IsTrue(result.Length > 100, "User Stories content should have substantial length");
            Assert.IsTrue(result.Contains("User Stories"), "Content should contain User Stories header");
            Assert.IsTrue(result.Contains("As a"), "Content should contain user story format");
            
            Debug.Log($"✅ User Stories generated successfully ({result.Length:N0} characters)");
            Debug.Log($"Content preview: {result.Substring(0, Math.Min(200, result.Length))}...");
        }

        [UnityTest]
        public System.Collections.IEnumerator TestWorkTicketsGeneratorIntegration()
        {
            var documentationService = UnityServiceBridge.GetDocumentationService();
            Assert.IsNotNull(documentationService, "DocumentationService should be available");

            // Create test project data
            var projectData = new ProjectData();
            projectData.UpdateProjectName("Test Unity Project");
            projectData.UpdateDescription("Test project for Work Tickets generation");
            projectData.UpdateProjectType(ProjectType.Game3D);

            // Create Work Tickets section
            var workTicketsSection = new DocumentationSectionData
            {
                SectionType = DocumentationSectionType.WorkTickets,
                IsEnabled = true,
                Status = DocumentationStatus.NotStarted
            };

            // Test Work Tickets generation
            var generationTask = documentationService.GenerateDocumentationSectionAsync(workTicketsSection, projectData);
            
            yield return new WaitUntil(() => generationTask.IsCompleted);
            
            Assert.IsTrue(generationTask.IsCompletedSuccessfully, "Work Tickets generation task should complete successfully");
            
            string result = generationTask.Result;
            Assert.IsNotNull(result, "Work Tickets content should not be null");
            Assert.IsTrue(result.Length > 100, "Work Tickets content should have substantial length");
            Assert.IsTrue(result.Contains("Work Tickets"), "Content should contain Work Tickets header");
            Assert.IsTrue(result.Contains("Implementation") || result.Contains("tickets"), "Content should contain ticket-related content");
            
            Debug.Log($"✅ Work Tickets generated successfully ({result.Length:N0} characters)");
            Debug.Log($"Content preview: {result.Substring(0, Math.Min(200, result.Length))}...");
        }

        [UnityTest]
        public System.Collections.IEnumerator TestAllDocumentationGeneratorsIntegration()
        {
            var documentationService = UnityServiceBridge.GetDocumentationService();
            Assert.IsNotNull(documentationService, "DocumentationService should be available");

            // Create test project data
            var projectData = new ProjectData();
            projectData.UpdateProjectName("Full Test Project");
            projectData.UpdateDescription("Complete test for all documentation generators");
            projectData.UpdateProjectType(ProjectType.Game3D);

            // Test all supported documentation section types
            var sectionTypes = new[]
            {
                DocumentationSectionType.GeneralProductDescription,
                DocumentationSectionType.SystemArchitecture,
                DocumentationSectionType.DataModel,
                DocumentationSectionType.APISpecification,
                DocumentationSectionType.UserStories,
                DocumentationSectionType.WorkTickets
            };

            int completedGenerators = 0;
            int totalGenerators = sectionTypes.Length;

            foreach (DocumentationSectionType sectionType in sectionTypes)
            {
                if (documentationService.CanGenerateSection(sectionType))
                {
                    var section = new DocumentationSectionData
                    {
                        SectionType = sectionType,
                        IsEnabled = true,
                        Status = DocumentationStatus.NotStarted
                    };

                    Debug.Log($"Testing {sectionType} generator...");

                    var generationTask = documentationService.GenerateDocumentationSectionAsync(section, projectData);
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

            Debug.Log($"✅ Integration test complete: {completedGenerators}/{totalGenerators} generators working");
            Assert.IsTrue(completedGenerators >= 4, $"At least 4 generators should work, got {completedGenerators}");
        }
    }
}