using System;
using System.Threading.Tasks;
using UnityProjectArchitect.Tests;

namespace UnityProjectArchitect.TestRunner
{
    /// <summary>
    /// Test runner to validate the Unity Project Architect AI functionality
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== Unity Project Architect AI Test Suite ===");
            Console.WriteLine("Testing AI functionality and documentation generation...");
            Console.WriteLine();

            bool testsPassed = true;

            try
            {
                // Run quick integration test first
                Console.WriteLine("Running Quick Integration Test...");
                bool quickTestPassed = await QuickIntegrationTest.RunQuickTestAsync();
                testsPassed &= quickTestPassed;
                Console.WriteLine($"Quick Test Result: {(quickTestPassed ? "PASSED" : "FAILED")}");
                Console.WriteLine();

                // Run comprehensive integration tests
                Console.WriteLine("Running Comprehensive AI Integration Tests...");
                bool comprehensiveTestsPassed = await AIIntegrationTestRunner.RunIntegrationTestsAsync();
                testsPassed &= comprehensiveTestsPassed;
                Console.WriteLine($"Comprehensive Tests Result: {(comprehensiveTestsPassed ? "PASSED" : "FAILED")}");
                Console.WriteLine();

                // Summary
                Console.WriteLine("=== TEST SUMMARY ===");
                if (testsPassed)
                {
                    Console.WriteLine("✅ ALL TESTS PASSED! Unity Project Architect AI functionality is working correctly.");
                    Console.WriteLine();
                    Console.WriteLine("Your Unity Project Architect package is ready to use with:");
                    Console.WriteLine("• AI-powered documentation generation");
                    Console.WriteLine("• Smart project analysis");
                    Console.WriteLine("• Template creation and management");
                    Console.WriteLine("• Offline fallback capabilities");
                    Console.WriteLine();
                    Console.WriteLine("How to test AI functionality in Unity:");
                    Console.WriteLine("1. Open Unity and go to Tools > Unity Project Architect > Unity Project Architect Studio (Ctrl+Shift+P)");
                    Console.WriteLine("2. Use the 'Game Concept Studio' tab to describe your game");
                    Console.WriteLine("3. Click 'Generate All Documentation' to see AI-powered documentation");
                    Console.WriteLine("4. Export the results to Markdown or create project structure");
                }
                else
                {
                    Console.WriteLine("❌ Some tests failed. Please check the output above for details.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test suite failed with exception: {ex.Message}");
                testsPassed = false;
            }

            Environment.Exit(testsPassed ? 0 : 1);
        }
    }
}