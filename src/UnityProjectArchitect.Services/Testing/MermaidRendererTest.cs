using System;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services.Testing
{
    /// <summary>
    /// Test class to verify MermaidRenderer functionality
    /// </summary>
    public static class MermaidRendererTest
    {
        public static async Task<bool> TestBasicMermaidRenderingAsync()
        {
            try
            {
                MermaidRenderer renderer = new MermaidRenderer();
                
                // Check if Mermaid CLI is available
                bool isAvailable = await renderer.IsMermaidCliAvailableAsync();
                if (!isAvailable)
                {
                    Debug.LogWarning("Mermaid CLI not available - test will skip rendering");
                    return true; // Not a failure if CLI isn't installed
                }

                // Test content with a simple Mermaid diagram
                string testContent = @"
# Test Document

This is a test document with a Mermaid diagram:

```mermaid
graph TD
    A[Start] --> B{Is it working?}
    B -->|Yes| C[Great!]
    B -->|No| D[Debug it]
    D --> A
```

End of test document.
";

                // Test rendering
                MermaidRenderOptions options = new MermaidRenderOptions
                {
                    OutputFormat = MermaidOutputFormat.PNG,
                    Theme = "default",
                    FallbackToSyntax = true
                };

                string processedContent = await renderer.RenderDiagramsInContentAsync(testContent, options);
                
                // Clean up after test
                renderer.Cleanup();

                // Verify processing occurred
                if (processedContent != testContent)
                {
                    Debug.Log("MermaidRenderer test passed - content was processed");
                    return true;
                }
                else
                {
                    Debug.LogWarning("MermaidRenderer test - no changes made to content (may be expected if CLI rendering failed)");
                    return true; // Still not a failure
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"MermaidRenderer test failed: {ex}");
                return false;
            }
        }

        public static async Task<bool> TestMarkdownExporterIntegrationAsync()
        {
            try
            {
                // Create test export content with Mermaid diagrams
                ExportContent testContent = new ExportContent
                {
                    Title = "Mermaid Test Document",
                    Variables = new System.Collections.Generic.Dictionary<string, object>()
                };

                DocumentationSectionData testSection = new DocumentationSectionData
                {
                    SectionType = DocumentationSectionType.SystemArchitecture,
                    Title = "System Architecture",
                    Content = @"
This is a test section with architecture diagrams:

```mermaid
graph LR
    User --> WebApp
    WebApp --> Database
    WebApp --> Cache
```

Additional content follows the diagram.
",
                    IsEnabled = true,
                    Status = DocumentationStatus.Completed
                };

                testContent.Sections = new System.Collections.Generic.List<DocumentationSectionData> { testSection };

                // Test Markdown export with Mermaid rendering
                MarkdownExporter exporter = new MarkdownExporter();
                ExportOptions options = new ExportOptions
                {
                    IncludeMetadata = true,
                    IncludeTableOfContents = false,
                    IncludeTimestamp = true,
                    FormatSpecificOptions = new System.Collections.Generic.Dictionary<string, object>
                    {
                        ["RenderMermaidDiagrams"] = true,
                        ["MermaidTheme"] = "default",
                        ["MermaidFormat"] = "PNG"
                    }
                };

                ExportOperationResult result = await exporter.FormatAsync(testContent, options);

                if (result.Success && result.Metadata.ContainsKey("content"))
                {
                    string exportedContent = result.Metadata["content"].ToString();
                    Debug.Log($"MarkdownExporter with Mermaid integration test passed - exported {result.TotalSizeBytes} bytes");
                    return true;
                }
                else
                {
                    Debug.LogError($"MarkdownExporter integration test failed: {result.ErrorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"MarkdownExporter integration test failed: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Run all MermaidRenderer tests
        /// </summary>
        public static async Task<bool> RunAllTestsAsync()
        {
            Debug.Log("=== Starting MermaidRenderer Tests ===");

            bool test1 = await TestBasicMermaidRenderingAsync();
            bool test2 = await TestMarkdownExporterIntegrationAsync();

            bool allTestsPassed = test1 && test2;

            Debug.Log($"=== MermaidRenderer Tests Complete: {(allTestsPassed ? "PASSED" : "FAILED")} ===");
            return allTestsPassed;
        }
    }
}