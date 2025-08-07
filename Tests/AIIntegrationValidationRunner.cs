using System;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;
using UnityProjectArchitect.Services.Testing;

namespace UnityProjectArchitect.Tests
{
    /// <summary>
    /// Test runner for AI integration validation
    /// </summary>
    public class AIIntegrationValidationRunner
    {
        /// <summary>
        /// Run complete AI integration validation test suite
        /// </summary>
        public static async Task<bool> RunAllTestsAsync()
        {
            Console.WriteLine("🧪 Running AI Integration Validation Tests...");
            Console.WriteLine(new string('=', 50));
            
            bool allTestsPassed = true;
            
            try
            {
                // Test 1: Basic AI Configuration
                Console.WriteLine("\n📋 Test 1: AI Configuration");
                ValidationResult configTest = await AIIntegrationValidationTest.TestAIConfigurationAsync();
                Console.WriteLine($"Result: {(configTest.IsValid ? "✅ PASSED" : "❌ FAILED")}");
                if (!configTest.IsValid)
                {
                    Console.WriteLine($"Error: {configTest.ErrorMessage}");
                    allTestsPassed = false;
                }
                
                // Test 2: Generator AI Integration
                Console.WriteLine("\n📋 Test 2: Generator AI Integration");
                ValidationResult generatorTest = await AIIntegrationValidationTest.TestGeneratorAIIntegrationAsync();
                Console.WriteLine($"Result: {(generatorTest.IsValid ? "✅ PASSED" : "❌ FAILED")}");
                if (!generatorTest.IsValid)
                {
                    Console.WriteLine($"Error: {generatorTest.ErrorMessage}");
                    allTestsPassed = false;
                }
                
                // Summary
                Console.WriteLine("\n" + new string('=', 50));
                Console.WriteLine($"🏁 Test Suite Complete: {(allTestsPassed ? "✅ ALL TESTS PASSED" : "❌ SOME TESTS FAILED")}");
                
                return allTestsPassed;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Test suite failed with exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }
        
        /// <summary>
        /// Main entry point for running tests
        /// </summary>
        public static async Task Main(string[] args)
        {
            try
            {
                bool success = await RunAllTestsAsync();
                Environment.Exit(success ? 0 : 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}
