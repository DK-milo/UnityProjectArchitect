using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityProjectArchitect.Core;
using UnityProjectArchitect.Core.AI;
using UnityProjectArchitect.AI.Services;

namespace UnityProjectArchitect.AI.Services
{
    public static class AIIntegrationTest
    {
        public static async Task<ValidationResult> RunBasicIntegrationTest()
        {
            try
            {
                Console.WriteLine("🧪 Starting AI integration test...");

                ValidationResult keyManagerTest = TestAPIKeyManager();
                if (!keyManagerTest.IsValid)
                {
                    return ValidationResult.Failure($"API Key Manager test failed: {keyManagerTest.ErrorMessage}");
                }

                ValidationResult responseParserTest = TestResponseParser();
                if (!responseParserTest.IsValid)
                {
                    return ValidationResult.Failure($"Response Parser test failed: {responseParserTest.ErrorMessage}");
                }

                ValidationResult claudeClientTest = TestClaudeAPIClient();
                if (!claudeClientTest.IsValid)
                {
                    return ValidationResult.Failure($"Claude API Client test failed: {claudeClientTest.ErrorMessage}");
                }

                Console.WriteLine("✅ All AI integration tests passed successfully");
                return ValidationResult.Success("AI integration test completed successfully");
            }
            catch (Exception ex)
            {
                string errorMessage = $"AI integration test failed with exception: {ex.Message}";
                Console.WriteLine($"ERROR: {errorMessage}");
                return ValidationResult.Failure(errorMessage);
            }
        }

        private static ValidationResult TestAPIKeyManager()
        {
            try
            {
                Console.WriteLine("Testing API Key Manager...");

                APIKeyManager keyManager = APIKeyManager.Instance;
                
                if (keyManager == null)
                {
                    return ValidationResult.Failure("APIKeyManager instance is null");
                }

                ValidationResult mockKeyValidation = keyManager.ValidateAPIKey("sk-test123456789");
                if (!mockKeyValidation.IsValid)
                {
                    return ValidationResult.Failure("Mock API key validation failed");
                }

                ValidationResult invalidKeyValidation = keyManager.ValidateAPIKey("invalid-key");
                if (invalidKeyValidation.IsValid)
                {
                    return ValidationResult.Failure("Invalid key was incorrectly validated as valid");
                }

                APIKeyStatus status = keyManager.GetKeyStatus();
                if (status == null)
                {
                    return ValidationResult.Failure("GetKeyStatus returned null");
                }

                Console.WriteLine("✅ API Key Manager test passed");
                return ValidationResult.Success("API Key Manager working correctly");
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure($"API Key Manager test exception: {ex.Message}");
            }
        }

        private static ValidationResult TestResponseParser()
        {
            try
            {
                Console.WriteLine("Testing Response Parser...");

                string mockResponse = @"# Test Documentation

This is a test document with **bold text** and some content.

## Features
- Feature 1: Basic functionality
- Feature 2: Advanced options
- Feature 3: Integration support

## Code Example
```csharp
public class TestClass
{
    public string Name { get; set; }
    public void DoSomething() { }
}
```

## Summary
This document provides a comprehensive overview of the test system.";

                ParsedResponse parsed = ResponseParser.ParseContent(mockResponse, DocumentationSectionType.GeneralProductDescription);
                
                if (parsed == null)
                {
                    return ValidationResult.Failure("ParsedResponse is null");
                }

                if (!parsed.Success)
                {
                    return ValidationResult.Failure($"Parsing failed: {parsed.ErrorMessage}");
                }

                if (parsed.Headings.Count == 0)
                {
                    return ValidationResult.Failure("No headings were extracted");
                }

                if (parsed.Lists.Count == 0)
                {
                    return ValidationResult.Failure("No lists were extracted");
                }

                if (parsed.CodeBlocks.Count == 0)
                {
                    return ValidationResult.Failure("No code blocks were extracted");
                }

                if (parsed.WordCount <= 0)
                {
                    return ValidationResult.Failure("Word count is zero or negative");
                }

                if (parsed.QualityScore == null)
                {
                    return ValidationResult.Failure("Quality score is null");
                }

                string summary = parsed.GetSummary();
                if (string.IsNullOrEmpty(summary))
                {
                    return ValidationResult.Failure("Summary generation failed");
                }

                Console.WriteLine($"✅ Response Parser test passed - {parsed.WordCount} words, {parsed.Headings.Count} headings");
                return ValidationResult.Success("Response Parser working correctly");
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure($"Response Parser test exception: {ex.Message}");
            }
        }

        private static ValidationResult TestClaudeAPIClient()
        {
            try
            {
                Console.WriteLine("Testing Claude API Client...");

                ClaudeAPIClient client = ClaudeAPIClient.Instance;
                
                if (client == null)
                {
                    return ValidationResult.Failure("ClaudeAPIClient instance is null");
                }

                ClaudeConfiguration config = client.Configuration;
                if (config == null)
                {
                    return ValidationResult.Failure("Configuration is null");
                }

                ClaudeRateLimitInfo rateLimitInfo = client.RateLimitInfo;
                if (rateLimitInfo == null)
                {
                    return ValidationResult.Failure("RateLimitInfo is null");
                }

                ClaudeAPIRequest testRequest = new ClaudeAPIRequest("Test message", "Test system prompt");
                
                if (string.IsNullOrEmpty(testRequest.model))
                {
                    return ValidationResult.Failure("Default model not set in request");
                }

                if (testRequest.messages == null || testRequest.messages.Count == 0)
                {
                    return ValidationResult.Failure("Request messages not properly initialized");
                }

                ClaudeAPIResponse mockResponse = new ClaudeAPIResponse();
                mockResponse.content.Add(new ClaudeContent("text", "This is a mock response for testing."));
                mockResponse.usage = new ClaudeUsage { input_tokens = 10, output_tokens = 15 };
                mockResponse.id = "test-response-id";

                if (!mockResponse.IsSuccess)
                {
                    return ValidationResult.Failure("Mock response should be marked as successful");
                }

                string textContent = mockResponse.GetTextContent();
                if (string.IsNullOrEmpty(textContent))
                {
                    return ValidationResult.Failure("Failed to extract text content from mock response");
                }

                Dictionary<string, object> status = client.GetStatus();
                if (status == null || status.Count == 0)
                {
                    return ValidationResult.Failure("Status dictionary is null or empty");
                }

                Console.WriteLine("✅ Claude API Client test passed");
                return ValidationResult.Success("Claude API Client working correctly");
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure($"Claude API Client test exception: {ex.Message}");
            }
        }

        public static async Task<ValidationResult> TestWithMockAPI()
        {
            try
            {
                Console.WriteLine("🧪 Testing with mock API data...");

                ClaudeAPIResponse mockResponse = CreateMockAPIResponse();
                ParsedResponse parsed = ResponseParser.ParseClaudeResponse(mockResponse, DocumentationSectionType.GeneralProductDescription);

                if (!parsed.Success)
                {
                    return ValidationResult.Failure($"Mock API response parsing failed: {parsed.ErrorMessage}");
                }

                if (parsed.WordCount <= 0)
                {
                    return ValidationResult.Failure("Mock response has no word count");
                }

                if (parsed.QualityScore.OverallScore <= 0)
                {
                    return ValidationResult.Failure("Mock response has zero quality score");
                }

                string report = parsed.GetFormattedReport();
                Console.WriteLine($"Mock API Test Report:\n{report}");

                Console.WriteLine("✅ Mock API test completed successfully");
                return ValidationResult.Success("Mock API integration test passed");
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure($"Mock API test exception: {ex.Message}");
            }
        }

        private static ClaudeAPIResponse CreateMockAPIResponse()
        {
            ClaudeAPIResponse response = new ClaudeAPIResponse
            {
                id = "mock-response-123",
                type = "message",
                role = "assistant",
                model = "claude-3-sonnet-20240229",
                stop_reason = "end_turn",
                usage = new ClaudeUsage
                {
                    input_tokens = 50,
                    output_tokens = 200
                }
            };

            string mockContent = @"# Unity Project Documentation

## Overview
This Unity project is a comprehensive game development framework designed to streamline the creation of interactive experiences.

## Key Features
- **Modular Architecture**: Clean separation of concerns with well-defined interfaces
- **Performance Optimized**: Efficient memory management and optimized rendering pipeline
- **Cross-Platform**: Support for multiple deployment targets including mobile and desktop

## Technical Implementation
```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private GameState currentState;
    [SerializeField] private PlayerController player;
    
    public void Initialize()
    {
        currentState = GameState.Loading;
        SetupGameSystems();
    }
}
```

## Architecture Components
| Component | Purpose | Dependencies |
|-----------|---------|--------------|
| GameManager | Core game flow control | SceneManager, UIManager |
| PlayerController | Player input and movement | InputSystem, Physics |
| UIManager | User interface management | Canvas, EventSystem |

## Development Guidelines
The project follows Unity best practices and maintains a clean, testable codebase suitable for team development.

For more information, visit our [documentation site](https://docs.example.com).";

            response.content.Add(new ClaudeContent("text", mockContent));

            return response;
        }

        public static void LogTestResults(ValidationResult result)
        {
            if (result.IsValid)
            {
                Console.WriteLine($"✅ Test Result: {result.ValidationContext}");
            }
            else
            {
                Console.WriteLine($"ERROR: ❌ Test Failed: {result.ErrorMessage}");
            }
        }
    }
}