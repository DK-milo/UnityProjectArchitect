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
    }
}