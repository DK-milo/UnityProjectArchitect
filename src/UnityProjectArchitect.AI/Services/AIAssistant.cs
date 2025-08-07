using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;
using UnityProjectArchitect.Core.AI;
using UnityProjectArchitect.AI.Prompts;
using AIProvider = UnityProjectArchitect.Core.AIProvider;
using AIConfiguration = UnityProjectArchitect.Core.AIConfiguration;

namespace UnityProjectArchitect.AI.Services
{
    /// <summary>
    /// Main AI Assistant service that orchestrates all AI operations including content generation,
    /// enhancement, project analysis, and multi-provider support with comprehensive error handling
    /// </summary>
    public class AIAssistant : IAIAssistant
    {
        public event Action<AIOperationResult> OnOperationComplete;
        public event Action<string, float> OnProgress;
        public event Action<string> OnError;

        private readonly ClaudeAPIClient _claudeClient;
        private readonly PromptTemplateManager _promptManager;
        private readonly PromptOptimizer _promptOptimizer;
        private readonly ConversationManager _conversationManager;
        private readonly ContentValidator _contentValidator;
        private readonly AIUsageStatistics _usageStatistics;
        private readonly Dictionary<AIProvider, AICapabilities> _providerCapabilities;
        private readonly AIConfiguration _currentConfiguration;
        private readonly ILogger _logger;
        private readonly APIKeyManager _keyManager;

        private bool _isConfigured = false;
        private AIProvider _currentProvider = AIProvider.Claude;

        public bool IsConfigured => _isConfigured && _claudeClient.IsConfigured;
        public AIProvider CurrentProvider => _currentProvider;

        public AIAssistant() : this(new ConsoleLogger())
        {
        }

        public AIAssistant(ILogger logger) : this(logger, null)
        {
        }

        public AIAssistant(ILogger logger, APIKeyManager keyManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _keyManager = keyManager ?? new APIKeyManager(new InMemorySettingsProvider(), _logger);
            
            // Use provided key manager or create default one
            APIKeyManager apiKeyManager = keyManager ?? APIKeyManager.Instance;
            
            _claudeClient = new ClaudeAPIClient(new StandardHttpClient(), _logger, apiKeyManager);
            _promptManager = new PromptTemplateManager(_logger, GetDefaultTemplatesPath());
            _promptOptimizer = new PromptOptimizer();
            _conversationManager = new ConversationManager(_logger);
            _contentValidator = new ContentValidator(_logger);
            _usageStatistics = new AIUsageStatistics();
            _providerCapabilities = new Dictionary<AIProvider, AICapabilities>();
            _currentConfiguration = new AIConfiguration();

            InitializeProviderCapabilities();
            LoadConfigurationFromKeyManager();
            ValidateConfiguration();
        }

        /// <summary>
        /// Generates AI content based on the provided request with comprehensive error handling and progress tracking
        /// </summary>
        public async Task<AIOperationResult> GenerateContentAsync(AIRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            DateTime startTime = DateTime.Now;
            _logger.Log($"Starting content generation for request type: {request.RequestType}");

            try
            {
                OnProgress?.Invoke("Initializing content generation...", 0.1f);

                // Validate request
                ValidationResult validation = await ValidateAIRequestAsync(request);
                if (!validation.IsValid)
                {
                    AIOperationResult validationError = new AIOperationResult(false, string.Empty)
                    {
                        ErrorMessage = validation.ErrorMessage,
                        Provider = _currentProvider,
                        ProcessingTime = DateTime.Now - startTime
                    };
                    OnError?.Invoke(validation.ErrorMessage);
                    OnOperationComplete?.Invoke(validationError);
                    return validationError;
                }

                OnProgress?.Invoke("Building context and optimizing prompt...", 0.3f);

                // Build context and optimize prompt
                string optimizedPrompt = await BuildOptimizedPromptAsync(request);
                string projectContext = await ContextBuilder.BuildContextAsync(request.ProjectContext);

                OnProgress?.Invoke("Sending request to AI provider...", 0.5f);

                // Generate content using current provider
                AIOperationResult result = await GenerateWithProviderAsync(optimizedPrompt, projectContext, request);

                OnProgress?.Invoke("Validating generated content...", 0.8f);

                // Validate generated content
                if (result.Success && !string.IsNullOrEmpty(result.Content))
                {
                    ContentValidationResult contentValidation = await _contentValidator.ValidateContentAsync(
                        result.Content, request.SectionType ?? DocumentationSectionType.GeneralProductDescription);
                    
                    if (!contentValidation.IsValid)
                    {
                        result.Metadata["ContentValidationIssues"] = contentValidation.Issues;
                        result.ConfidenceScore *= 0.7f; // Reduce confidence for validation issues
                    }
                }

                OnProgress?.Invoke("Finalizing result...", 1.0f);

                // Update statistics
                UpdateUsageStatistics(result, request);

                // Store conversation history if multi-turn
                if (request.Parameters.ContainsKey("ConversationId"))
                {
                    string conversationId = request.Parameters["ConversationId"].ToString();
                    await _conversationManager.AddMessageAsync(conversationId, "user", request.Prompt);
                    if (result.Success)
                    {
                        await _conversationManager.AddMessageAsync(conversationId, "assistant", result.Content);
                    }
                }

                _logger.Log($"Content generation completed in {result.ProcessingTime.TotalSeconds:F2}s");
                OnOperationComplete?.Invoke(result);
                return result;
            }
            catch (Exception ex)
            {
                string errorMessage = $"Content generation failed: {ex.Message}";
                _logger.LogError(errorMessage);
                
                AIOperationResult errorResult = new AIOperationResult(false, string.Empty)
                {
                    ErrorMessage = errorMessage,
                    Provider = _currentProvider,
                    ProcessingTime = DateTime.Now - startTime
                };
                
                OnError?.Invoke(errorMessage);
                OnOperationComplete?.Invoke(errorResult);
                return errorResult;
            }
        }

        /// <summary>
        /// Enhances existing content with AI-powered improvements based on enhancement type
        /// </summary>
        public async Task<AIOperationResult> EnhanceContentAsync(string content, AIEnhancementRequest enhancementRequest)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Content cannot be null or empty", nameof(content));
            if (enhancementRequest == null)
                throw new ArgumentNullException(nameof(enhancementRequest));

            DateTime startTime = DateTime.Now;
            _logger.Log($"Starting content enhancement with type: {enhancementRequest.EnhancementType}");

            try
            {
                OnProgress?.Invoke("Preparing content enhancement...", 0.2f);

                // Build enhancement prompt
                string enhancementPrompt = BuildEnhancementPrompt(content, enhancementRequest);
                
                // Create AI request for enhancement
                AIRequest request = new AIRequest(enhancementPrompt, null)
                {
                    RequestType = AIRequestType.Enhancement,
                    TargetWordCount = enhancementRequest.TargetWordCount,
                    Style = enhancementRequest.Style,
                    Configuration = _currentConfiguration
                };

                request.Parameters["OriginalContent"] = content;
                request.Parameters["EnhancementType"] = enhancementRequest.EnhancementType.ToString();

                OnProgress?.Invoke("Processing enhancement...", 0.5f);

                // Generate enhanced content
                AIOperationResult result = await GenerateWithProviderAsync(enhancementPrompt, content, request);

                OnProgress?.Invoke("Validating enhancement...", 0.8f);

                // Additional validation for enhancements
                if (result.Success)
                {
                    result = await ValidateEnhancementResultAsync(result, content, enhancementRequest);
                }

                UpdateUsageStatistics(result, request);

                OnProgress?.Invoke("Enhancement complete", 1.0f);
                _logger.Log($"Content enhancement completed in {result.ProcessingTime.TotalSeconds:F2}s");
                
                OnOperationComplete?.Invoke(result);
                return result;
            }
            catch (Exception ex)
            {
                string errorMessage = $"Content enhancement failed: {ex.Message}";
                _logger.LogError(errorMessage);
                
                AIOperationResult errorResult = new AIOperationResult(false, string.Empty)
                {
                    ErrorMessage = errorMessage,
                    Provider = _currentProvider,
                    ProcessingTime = DateTime.Now - startTime
                };
                
                OnError?.Invoke(errorMessage);
                OnOperationComplete?.Invoke(errorResult);
                return errorResult;
            }
        }

        /// <summary>
        /// Analyzes Unity project structure and provides intelligent insights and recommendations
        /// </summary>
        public async Task<AIOperationResult> AnalyzeProjectAsync(ProjectData projectData)
        {
            if (projectData == null)
                throw new ArgumentNullException(nameof(projectData));

            DateTime startTime = DateTime.Now;
            _logger.Log($"Starting project analysis for: {projectData.ProjectName}");

            try
            {
                OnProgress?.Invoke("Analyzing project structure...", 0.3f);

                // Build comprehensive project context
                string projectContext = await ContextBuilder.BuildProjectAnalysisContextAsync(projectData);
                
                // Get analysis prompt
                string analysisPrompt = await _promptManager.GetCustomPromptAsync("project_analysis");
                
                AIRequest request = new AIRequest(analysisPrompt, projectData)
                {
                    RequestType = AIRequestType.Analysis,
                    Configuration = _currentConfiguration
                };

                OnProgress?.Invoke("Generating analysis insights...", 0.6f);

                AIOperationResult result = await GenerateWithProviderAsync(analysisPrompt, projectContext, request);

                OnProgress?.Invoke("Processing analysis results...", 0.9f);

                if (result.Success)
                {
                    // Parse and enrich analysis results
                    result = await EnrichAnalysisResultAsync(result, projectData);
                }

                UpdateUsageStatistics(result, request);

                OnProgress?.Invoke("Analysis complete", 1.0f);
                _logger.Log($"Project analysis completed in {result.ProcessingTime.TotalSeconds:F2}s");
                
                OnOperationComplete?.Invoke(result);
                return result;
            }
            catch (Exception ex)
            {
                string errorMessage = $"Project analysis failed: {ex.Message}";
                _logger.LogError(errorMessage);
                
                AIOperationResult errorResult = new AIOperationResult(false, string.Empty)
                {
                    ErrorMessage = errorMessage,
                    Provider = _currentProvider,
                    ProcessingTime = DateTime.Now - startTime
                };
                
                OnError?.Invoke(errorMessage);
                OnOperationComplete?.Invoke(errorResult);
                return errorResult;
            }
        }

        /// <summary>
        /// Generates contextual suggestions based on project data and suggestion type
        /// </summary>
        public async Task<AIOperationResult> GenerateSuggestionsAsync(ProjectData projectData, SuggestionType suggestionType)
        {
            if (projectData == null)
                throw new ArgumentNullException(nameof(projectData));

            DateTime startTime = DateTime.Now;
            _logger.Log($"Generating suggestions for type: {suggestionType}");

            try
            {
                OnProgress?.Invoke("Preparing suggestion generation...", 0.2f);

                string suggestionPrompt = BuildSuggestionPrompt(projectData, suggestionType);
                string projectContext = await ContextBuilder.BuildContextAsync(projectData);

                AIRequest request = new AIRequest(suggestionPrompt, projectData)
                {
                    RequestType = AIRequestType.Suggestion,
                    Configuration = _currentConfiguration
                };

                request.Parameters["SuggestionType"] = suggestionType.ToString();

                OnProgress?.Invoke("Generating suggestions...", 0.6f);

                AIOperationResult result = await GenerateWithProviderAsync(suggestionPrompt, projectContext, request);

                OnProgress?.Invoke("Processing suggestions...", 0.9f);

                if (result.Success)
                {
                    result = await ProcessSuggestionsAsync(result, suggestionType);
                }

                UpdateUsageStatistics(result, request);

                OnProgress?.Invoke("Suggestions ready", 1.0f);
                _logger.Log($"Suggestion generation completed in {result.ProcessingTime.TotalSeconds:F2}s");
                
                OnOperationComplete?.Invoke(result);
                return result;
            }
            catch (Exception ex)
            {
                string errorMessage = $"Suggestion generation failed: {ex.Message}";
                _logger.LogError(errorMessage);
                
                AIOperationResult errorResult = new AIOperationResult(false, string.Empty)
                {
                    ErrorMessage = errorMessage,
                    Provider = _currentProvider,
                    ProcessingTime = DateTime.Now - startTime
                };
                
                OnError?.Invoke(errorMessage);
                OnOperationComplete?.Invoke(errorResult);
                return errorResult;
            }
        }

        /// <summary>
        /// Validates API key for specified provider with comprehensive testing
        /// </summary>
        public async Task<ValidationResult> ValidateAPIKeyAsync(string apiKey, AIProvider provider)
        {
            if (string.IsNullOrEmpty(apiKey))
                return ValidationResult.Failure("API key cannot be null or empty");

            try
            {
                switch (provider)
                {
                    case AIProvider.Claude:
                        // Store current key temporarily
                        APIKeyManager keyManager = new APIKeyManager(new InMemorySettingsProvider(), _logger);
                        keyManager.SetClaudeAPIKey(apiKey);
                        
                        // Test connection with new key
                        ClaudeAPIClient testClient = new ClaudeAPIClient(new StandardHttpClient(), _logger);
                        return await testClient.TestConnectionAsync();

                    default:
                        return ValidationResult.Failure($"Provider {provider} not supported yet");
                }
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure($"API key validation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests connection to AI provider with current configuration
        /// </summary>
        public async Task<bool> TestConnectionAsync(AIConfiguration configuration)
        {
            if (configuration == null)
                return false;

            try
            {
                switch (configuration.Provider)
                {
                    case AIProvider.Claude:
                        return await _claudeClient.TestConnectionAsync() != null;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Connection test failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets list of supported AI providers
        /// </summary>
        public List<AIProvider> GalSupportedProviders()
        {
            return new List<AIProvider> { AIProvider.Claude };
        }

        /// <summary>
        /// Gets default configuration for specified provider
        /// </summary>
        public AIConfiguration GetDefaultConfiguration(AIProvider provider)
        {
            return provider switch
            {
                AIProvider.Claude => new AIConfiguration
                {
                    Provider = AIProvider.Claude,
                    Model = "claude-3-sonnet-20240229",
                    MaxTokens = 2048,
                    Temperature = 0.7f,
                    TimeoutSeconds = 30,
                    MaxRetries = 3,
                    EnableLogging = true
                },
                _ => new AIConfiguration { Provider = provider }
            };
        }

        /// <summary>
        /// Gets capabilities for specified AI provider
        /// </summary>
        public AICapabilities GetCapabilities(AIProvider provider)
        {
            return _providerCapabilities.ContainsKey(provider) 
                ? _providerCapabilities[provider] 
                : new AICapabilities { Provider = provider };
        }

        public List<AIProvider> GetSupportedProviders()
        {
            return GetSupportedProviders();
        }

        #region Private Implementation Methods

        private void InitializeProviderCapabilities()
        {
            // Claude capabilities
            _providerCapabilities[AIProvider.Claude] = new AICapabilities
            {
                Provider = AIProvider.Claude,
                SupportedModels = new List<string>
                {
                    "claude-3-sonnet-20240229",
                    "claude-3-haiku-20240307",
                    "claude-3-opus-20240229"
                },
                MaxTokens = 4096,
                SupportsStreaming = false,
                SupportsImages = true,
                SupportsCodeGeneration = true,
                SupportsMultipleLanguages = true,
                SupportedEnhancements = Enum.GetValues(typeof(EnhancementType)).Cast<EnhancementType>().ToList(),
                Features = new Dictionary<string, bool>
                {
                    { "ConversationHistory", true },
                    { "ContextWindow", true },
                    { "CustomPrompts", true },
                    { "BatchProcessing", false },
                    { "FinetuningSupport", false }
                }
            };
        }

        private void LoadConfigurationFromKeyManager()
        {
            try
            {
                // Load API key from the key manager
                string apiKey = _keyManager.GetClaudeAPIKey();
                if (!string.IsNullOrEmpty(apiKey))
                {
                    _currentConfiguration.ApiKey = apiKey;
                    _logger.Log("API key loaded successfully into AI configuration");
                }
                else
                {
                    _logger.LogWarning("No API key available for AI configuration");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load configuration from key manager: {ex.Message}");
            }
        }

        private void ValidateConfiguration()
        {
            _isConfigured = _claudeClient.IsConfigured && _currentConfiguration.IsValid();
            
            if (_isConfigured)
            {
                _logger.Log("AI Assistant successfully configured");
            }
            else
            {
                _logger.LogWarning("AI Assistant configuration incomplete - some features may not be available");
            }
        }

        private async Task<ValidationResult> ValidateAIRequestAsync(AIRequest request)
        {
            if (request == null)
                return ValidationResult.Failure("Request cannot be null");
            
            if (string.IsNullOrEmpty(request.Prompt))
                return ValidationResult.Failure("Prompt cannot be empty");
            
            if (request.Configuration != null && !request.Configuration.IsValid())
                return ValidationResult.Failure("Invalid configuration provided");

            // Validate prompt quality
            AIPromptValidationResult promptValidation = await _promptManager.ValidatePromptAsync(request.Prompt);
            if (!promptValidation.IsValid)
            {
                string issues = string.Join(", ", promptValidation.Issues);
                return ValidationResult.Failure($"Prompt validation failed: {issues}");
            }

            return ValidationResult.Success("Request validation passed");
        }

        private async Task<string> BuildOptimizedPromptAsync(AIRequest request)
        {
            string basePrompt = request.Prompt;

            // Get section-specific prompt if available
            if (request.SectionType.HasValue)
            {
                string sectionPrompt = await _promptManager.GetPromptAsync(request.SectionType.Value);
                basePrompt = CombinePrompts(sectionPrompt, basePrompt);
            }

            // Optimize prompt for token efficiency
            PromptOptimizationRequest optimizationRequest = new PromptOptimizationRequest
            {
                OriginalPrompt = basePrompt,
                Goal = OptimizationGoal.TokenReduction,
                TargetTokenLimit = request.Configuration?.MaxTokens ?? 2048
            };

            return await _promptManager.OptimizePromptAsync(basePrompt, optimizationRequest);
        }

        private string CombinePrompts(string sectionPrompt, string userPrompt)
        {
            if (string.IsNullOrEmpty(sectionPrompt))
                return userPrompt;
            
            if (string.IsNullOrEmpty(userPrompt))
                return sectionPrompt;

            return $"{sectionPrompt}\n\nAdditional Instructions:\n{userPrompt}";
        }

        private async Task<AIOperationResult> GenerateWithProviderAsync(string prompt, string context, AIRequest request)
        {
            DateTime startTime = DateTime.Now;

            try
            {
                switch (_currentProvider)
                {
                    case AIProvider.Claude:
                        ClaudeAPIRequest claudeRequest = new ClaudeAPIRequest(prompt, context);
                        claudeRequest.max_tokens = request.Configuration?.MaxTokens ?? 2048;
                        claudeRequest.temperature = request.Configuration?.Temperature ?? 0.7f;

                        ClaudeAPIResponse response = await _claudeClient.SendRequestAsync(claudeRequest);
                        
                        return new AIOperationResult(response.IsSuccess, response.GetTextContent())
                        {
                            Provider = AIProvider.Claude,
                            ProcessingTime = DateTime.Now - startTime,
                            TokensUsed = response.usage?.TotalTokens ?? 0,
                            ConfidenceScore = CalculateConfidenceScore(response),
                            ErrorMessage = response.error?.message,
                            Metadata = new Dictionary<string, object>
                            {
                                { "Model", claudeRequest.model },
                                { "Temperature", claudeRequest.temperature },
                                { "MaxTokens", claudeRequest.max_tokens }
                            }
                        };

                    default:
                        throw new NotSupportedException($"Provider {_currentProvider} is not supported");
                }
            }
            catch (Exception ex)
            {
                return new AIOperationResult(false, string.Empty)
                {
                    ErrorMessage = $"Provider operation failed: {ex.Message}",
                    Provider = _currentProvider,
                    ProcessingTime = DateTime.Now - startTime
                };
            }
        }

        private float CalculateConfidenceScore(ClaudeAPIResponse response)
        {
            if (!response.IsSuccess || string.IsNullOrEmpty(response.GetTextContent()))
                return 0f;

            float score = 0.8f; // Base confidence for successful response

            // Adjust based on response length (too short or too long reduces confidence)
            int contentLength = response.GetTextContent().Length;
            if (contentLength < 100)
                score *= 0.7f;
            else if (contentLength > 5000)
                score *= 0.9f;

            // Adjust based on token usage efficiency
            if (response.usage != null)
            {
                float tokenEfficiency = (float)response.usage.output_tokens / response.usage.input_tokens;
                if (tokenEfficiency > 0.1f && tokenEfficiency < 2.0f)
                    score *= 1.1f; // Good token efficiency
            }

            return Math.Min(score, 1.0f);
        }

        private string BuildEnhancementPrompt(string content, AIEnhancementRequest enhancementRequest)
        {
            string enhancementInstructions = enhancementRequest.EnhancementType switch
            {
                EnhancementType.Improve => "Improve the quality, clarity, and readability of the following content while preserving the original meaning and structure.",
                EnhancementType.Expand => "Expand the following content with additional relevant details, examples, and explanations while maintaining coherence.",
                EnhancementType.Summarize => "Create a concise summary of the following content, highlighting the key points and main ideas.",
                EnhancementType.Restructure => "Restructure the following content for better organization, flow, and logical presentation.",
                EnhancementType.ProofRead => "Proofread the following content for grammar, spelling, punctuation, and style improvements.",
                EnhancementType.AddExamples => "Add relevant examples, code samples, and practical illustrations to the following content.",
                EnhancementType.AddDiagrams => "Suggest and describe diagrams, charts, or visual elements that would enhance the following content.",
                EnhancementType.Translate => "Translate the following content while preserving technical accuracy and context.",
                _ => "Enhance the following content according to best practices."
            };

            string prompt = $"{enhancementInstructions}\n\n";
            
            if (!string.IsNullOrEmpty(enhancementRequest.Instructions))
            {
                prompt += $"Additional Instructions: {enhancementRequest.Instructions}\n\n";
            }

            if (enhancementRequest.FocusAreas?.Any() == true)
            {
                prompt += $"Focus Areas: {string.Join(", ", enhancementRequest.FocusAreas)}\n\n";
            }

            if (enhancementRequest.TargetWordCount > 0)
            {
                prompt += $"Target Word Count: Approximately {enhancementRequest.TargetWordCount} words\n\n";
            }

            prompt += $"Style: {enhancementRequest.Style}\n\n";
            prompt += $"Original Content:\n{content}";

            return prompt;
        }

        private async Task<AIOperationResult> ValidateEnhancementResultAsync(AIOperationResult result, string originalContent, AIEnhancementRequest enhancementRequest)
        {
            // Validate that enhancement meets requirements
            if (enhancementRequest.TargetWordCount > 0)
            {
                int actualWordCount = result.WordCount;
                int targetWordCount = enhancementRequest.TargetWordCount;
                float deviation = Math.Abs(actualWordCount - targetWordCount) / (float)targetWordCount;
                
                if (deviation > 0.5f) // More than 50% deviation
                {
                    result.ConfidenceScore *= 0.8f;
                    result.Metadata["WordCountDeviation"] = $"{deviation:P1}";
                }
            }

            // Validate content based on enhancement type
            switch (enhancementRequest.EnhancementType)
            {
                case EnhancementType.Summarize:
                    if (result.WordCount >= originalContent.Split(' ').Length)
                    {
                        result.ConfidenceScore *= 0.7f; // Summary should be shorter
                    }
                    break;
                
                case EnhancementType.Expand:
                    if (result.WordCount <= originalContent.Split(' ').Length)
                    {
                        result.ConfidenceScore *= 0.7f; // Expansion should be longer
                    }
                    break;
            }

            return result;
        }

        private string BuildSuggestionPrompt(ProjectData projectData, SuggestionType suggestionType)
        {
            string basePrompt = suggestionType switch
            {
                SuggestionType.ProjectStructure => "Analyze the project structure and provide suggestions for better organization, folder hierarchy, and architectural improvements.",
                SuggestionType.BestPractices => "Review the project and suggest Unity best practices, coding standards, and development workflow improvements.",
                SuggestionType.Architecture => "Analyze the system architecture and recommend design patterns, architectural improvements, and scalability enhancements.",
                SuggestionType.Performance => "Identify potential performance bottlenecks and suggest optimization strategies for better runtime performance.",
                SuggestionType.Security => "Review the project for security considerations and recommend security best practices and vulnerability mitigation.",
                SuggestionType.Testing => "Suggest testing strategies, unit test coverage improvements, and quality assurance practices.",
                SuggestionType.Documentation => "Recommend documentation improvements, missing documentation areas, and documentation best practices.",
                SuggestionType.Templates => "Suggest project templates, code generation opportunities, and automation possibilities.",
                _ => "Provide general suggestions for project improvement."
            };

            return $"{basePrompt}\n\nProject Context:\n- Name: {projectData.ProjectName}\n- Description: {projectData.ProjectDescription}\n- Unity Version: {projectData.TargetUnityVersion}";
        }

        private async Task<AIOperationResult> EnrichAnalysisResultAsync(AIOperationResult result, ProjectData projectData)
        {
            // Add metadata about the analysis
            result.Metadata["ProjectName"] = projectData.ProjectName;
            result.Metadata["AnalysisType"] = "ProjectAnalysis";
            result.Metadata["UnityVersion"] = projectData.TargetUnityVersion;
            result.Metadata["DocumentationSections"] = projectData.DocumentationSections.Count;

            return result;
        }

        private async Task<AIOperationResult> ProcessSuggestionsAsync(AIOperationResult result, SuggestionType suggestionType)
        {
            // Add metadata about the suggestions
            result.Metadata["SuggestionType"] = suggestionType.ToString();
            result.Metadata["SuggestionCategory"] = GetSuggestionCategory(suggestionType);

            return result;
        }

        private string GetSuggestionCategory(SuggestionType suggestionType)
        {
            return suggestionType switch
            {
                SuggestionType.ProjectStructure or SuggestionType.Architecture => "Architecture",
                SuggestionType.Performance or SuggestionType.Security => "Optimization",
                SuggestionType.Testing or SuggestionType.Documentation => "Quality",
                SuggestionType.BestPractices or SuggestionType.Templates => "Development",
                _ => "General"
            };
        }

        private void UpdateUsageStatistics(AIOperationResult result, AIRequest request)
        {
            _usageStatistics.RequestCount++;
            _usageStatistics.TotalTokensUsed += result.TokensUsed;
            _usageStatistics.TotalProcessingTime = _usageStatistics.TotalProcessingTime.Add(result.ProcessingTime);
            
            if (result.ConfidenceScore > 0)
            {
                _usageStatistics.AverageConfidenceScore = 
                    (_usageStatistics.AverageConfidenceScore * (_usageStatistics.RequestCount - 1) + result.ConfidenceScore) / _usageStatistics.RequestCount;
            }

            if (!_usageStatistics.ProviderUsage.ContainsKey(result.Provider))
                _usageStatistics.ProviderUsage[result.Provider] = 0;
            _usageStatistics.ProviderUsage[result.Provider]++;

            if (request.SectionType.HasValue)
            {
                if (!_usageStatistics.SectionUsage.ContainsKey(request.SectionType.Value))
                    _usageStatistics.SectionUsage[request.SectionType.Value] = 0;
                _usageStatistics.SectionUsage[request.SectionType.Value]++;
            }
        }

        private static string GetDefaultTemplatesPath()
        {
            return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "AI");
        }

        #endregion
    }
}