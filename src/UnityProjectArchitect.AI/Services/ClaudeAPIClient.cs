using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;
using UnityProjectArchitect.Core.AI;
using UnityProjectArchitect.AI.Services;

namespace UnityProjectArchitect.AI.Services
{
    public class ClaudeAPIClient
    {
        public event Action<ClaudeAPIResponse> OnResponseReceived;
        public event Action<string> OnError;
        public event Action<float> OnProgress;
        public event Action<string> OnStatusUpdate;

        private readonly APIKeyManager _keyManager;
        private readonly ClaudeConfiguration _configuration;
        private readonly ClaudeRateLimitInfo _rateLimitInfo;
        private readonly Dictionary<string, DateTime> _lastRequestTimes;
        private readonly object _requestLock = new object();
        private readonly IHttpClientWrapper _httpClient;
        private readonly ILogger _logger;

        private static ClaudeAPIClient _instance;
        private static readonly object _instanceLock = new object();

        public static ClaudeAPIClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ClaudeAPIClient();
                        }
                    }
                }
                return _instance;
            }
        }

        private ClaudeAPIClient() : this(new StandardHttpClient(), new ConsoleLogger())
        {
        }

        public ClaudeAPIClient(IHttpClientWrapper httpClient, ILogger logger) : this(httpClient, logger, null)
        {
        }

        public ClaudeAPIClient(IHttpClientWrapper httpClient, ILogger logger, APIKeyManager keyManager)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _keyManager = keyManager ?? new APIKeyManager(new InMemorySettingsProvider(), logger);
            _configuration = LoadConfiguration();
            _rateLimitInfo = new ClaudeRateLimitInfo();
            _lastRequestTimes = new Dictionary<string, DateTime>();
        }

        public bool IsConfigured => _keyManager.HasClaudeAPIKey() && _configuration.IsValid();

        public ClaudeConfiguration Configuration => _configuration;

        public ClaudeRateLimitInfo RateLimitInfo => _rateLimitInfo;

        public async Task<ClaudeAPIResponse> SendRequestAsync(ClaudeAPIRequest request)
        {
            _logger.Log($"[AI DEBUG] SendRequestAsync: IsConfigured={IsConfigured}, Request model={request.model}, MaxTokens={request.max_tokens}, Temp={request.temperature}");
            if (!IsConfigured)
            {
                _logger.LogError("[AI DEBUG] Claude API client is not properly configured");
                ClaudeAPIResponse errorResponse = new ClaudeAPIResponse
                {
                    error = new ClaudeError("configuration_error", "Claude API client is not properly configured")
                };
                return errorResponse;
            }
            try
            {
                OnStatusUpdate?.Invoke("Preparing API request...");
                OnProgress?.Invoke(0.1f);
                ValidationResult validation = ValidateRequest(request);
                if (!validation.IsValid)
                {
                    _logger.LogError($"[AI DEBUG] Request validation failed: {validation.ErrorMessage}");
                    return new ClaudeAPIResponse
                    {
                        error = new ClaudeError("validation_error", validation.ErrorMessage)
                    };
                }
                OnStatusUpdate?.Invoke("Checking rate limits...");
                OnProgress?.Invoke(0.2f);
                if (!await WaitForRateLimit(request))
                {
                    _logger.LogError("[AI DEBUG] Rate limit exceeded, please try again later");
                    return new ClaudeAPIResponse
                    {
                        error = new ClaudeError("rate_limit_error", "Rate limit exceeded, please try again later")
                    };
                }
                OnStatusUpdate?.Invoke("Sending request to Claude API...");
                OnProgress?.Invoke(0.3f);
                _logger.Log($"[AI DEBUG] Sending HTTP request to endpoint: {_configuration.baseUrl}");
                ClaudeAPIResponse response = await SendRequestWithRetry(request);
                _logger.Log($"[AI DEBUG] HTTP request completed. Success={response.IsSuccess}, Error={response.error?.message}");
                OnProgress?.Invoke(1.0f);
                OnStatusUpdate?.Invoke(response.IsSuccess ? "Request completed successfully" : "Request failed");
                OnResponseReceived?.Invoke(response);
                return response;
            }
            catch (Exception ex)
            {
                string errorMessage = $"[AI DEBUG] Unexpected error in SendRequestAsync: {ex.Message}\n{ex.StackTrace}";
                _logger.LogError(errorMessage);
                OnError?.Invoke(errorMessage);
                return new ClaudeAPIResponse
                {
                    error = new ClaudeError("client_error", errorMessage)
                };
            }
        }

        private async Task<ClaudeAPIResponse> SendRequestWithRetry(ClaudeAPIRequest request)
        {
            int maxRetries = _configuration.maxRetries;
            float baseDelay = _configuration.retryDelaySeconds;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    if (attempt > 0)
                    {
                        float delay = baseDelay * (float)Math.Pow(2, attempt - 1);
                        OnStatusUpdate?.Invoke($"Retrying request (attempt {attempt + 1}/{maxRetries + 1}) after {delay:F1}s...");
                        await Task.Delay(TimeSpan.FromSeconds(delay));
                    }

                    ClaudeAPIResponse response = await SendSingleRequest(request);

                    if (response.IsSuccess)
                    {
                        return response;
                    }

                    if (response.error != null && !IsRetryableError(response.error))
                    {
                        _logger.LogWarning($"Non-retryable error: {response.error.message}");
                        return response;
                    }

                    if (attempt == maxRetries)
                    {
                        _logger.LogError($"Max retries ({maxRetries}) exceeded");
                        return response;
                    }

                    _logger.LogWarning($"Request failed (attempt {attempt + 1}), retrying: {response.error?.message}");
                }
                catch (Exception ex)
                {
                    if (attempt == maxRetries)
                    {
                        string errorMessage = $"Request failed after {maxRetries} retries: {ex.Message}";
                        _logger.LogError(errorMessage);
                        return new ClaudeAPIResponse
                        {
                            error = new ClaudeError("max_retries_exceeded", errorMessage)
                        };
                    }

                    _logger.LogWarning($"Request attempt {attempt + 1} failed with exception: {ex.Message}");
                }
            }

            return new ClaudeAPIResponse
            {
                error = new ClaudeError("unknown_error", "Request failed for unknown reasons")
            };
        }

        private async Task<ClaudeAPIResponse> SendSingleRequest(ClaudeAPIRequest request)
        {
            string apiKey = _keyManager.GetClaudeAPIKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("[AI DEBUG] API key not found or invalid");
                return new ClaudeAPIResponse
                {
                    error = new ClaudeError("authentication_error", "API key not found or invalid")
                };
            }
            try
            {
                string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                Dictionary<string, string> headers = _configuration.GetHeaders();
                _logger.Log($"[AI DEBUG] SendSingleRequest: Endpoint={_configuration.baseUrl}, PayloadLength={jsonPayload.Length}, Headers={string.Join(", ", headers.Keys)}");
                DateTime startTime = DateTime.Now;
                HttpResponse httpResponse = await _httpClient.PostWithHeadersAsync(
                    _configuration.baseUrl, 
                    jsonPayload, 
                    headers, 
                    _configuration.timeoutSeconds);
                TimeSpan requestTime = DateTime.Now - startTime;
                _logger.Log($"[AI DEBUG] HTTP response: StatusCode={httpResponse.StatusCode}, Success={httpResponse.IsSuccess}, Time={requestTime.TotalSeconds:F2}s, ContentLength={httpResponse.Content?.Length}");
                if (_configuration.enableLogging)
                {
                    _logger.Log($"[AI DEBUG] HTTP response content: {httpResponse.Content?.Substring(0, Math.Min(500, httpResponse.Content.Length))}");
                }
                UpdateRateLimitInfo(httpResponse.Headers);
                if (httpResponse.IsSuccess)
                {
                    try
                    {
                        ClaudeAPIResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<ClaudeAPIResponse>(httpResponse.Content);
                        if (response == null)
                        {
                            _logger.LogError("[AI DEBUG] Failed to parse API response");
                            return new ClaudeAPIResponse
                            {
                                error = new ClaudeError("parse_error", "Failed to parse API response")
                            };
                        }
                        if (_configuration.enableLogging && response.IsSuccess)
                        {
                            _logger.Log($"[AI DEBUG] Claude API success: {response.usage?.TotalTokens ?? 0} tokens used");
                        }
                        return response;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[AI DEBUG] Failed to parse response JSON: {ex.Message}\n{ex.StackTrace}");
                        return new ClaudeAPIResponse
                        {
                            error = new ClaudeError("parse_error", $"Failed to parse response JSON: {ex.Message}")
                        };
                    }
                }
                else
                {
                    string errorMessage = $"HTTP {httpResponse.StatusCode}: {httpResponse.ErrorMessage}";
                    _logger.LogError($"[AI DEBUG] HTTP error: {errorMessage}");
                    if (!string.IsNullOrEmpty(httpResponse.Content))
                    {
                        try
                        {
                            ClaudeAPIResponse errorResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ClaudeAPIResponse>(httpResponse.Content);
                            if (errorResponse?.error != null)
                            {
                                return errorResponse;
                            }
                        }
                        catch
                        {
                            errorMessage += $" - Response: {httpResponse.Content}";
                        }
                    }
                    return new ClaudeAPIResponse
                    {
                        error = new ClaudeError("http_error", errorMessage)
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[AI DEBUG] Request failed: {ex.Message}\n{ex.StackTrace}");
                return new ClaudeAPIResponse
                {
                    error = new ClaudeError("request_error", $"Request failed: {ex.Message}")
                };
            }
        }

        private ValidationResult ValidateRequest(ClaudeAPIRequest request)
        {
            if (request == null)
            {
                return ValidationResult.Failure("Request cannot be null");
            }

            if (string.IsNullOrEmpty(request.model))
            {
                return ValidationResult.Failure("Model must be specified");
            }

            if (request.messages == null || request.messages.Count == 0)
            {
                return ValidationResult.Failure("At least one message is required");
            }

            if (request.max_tokens <= 0 || request.max_tokens > 4096)
            {
                return ValidationResult.Failure("Max tokens must be between 1 and 4096");
            }

            if (request.temperature < 0.0f || request.temperature > 1.0f)
            {
                return ValidationResult.Failure("Temperature must be between 0.0 and 1.0");
            }

            foreach (ClaudeMessage message in request.messages)
            {
                if (string.IsNullOrEmpty(message.role))
                {
                    return ValidationResult.Failure("Message role cannot be empty");
                }

                if (string.IsNullOrEmpty(message.content))
                {
                    return ValidationResult.Failure("Message content cannot be empty");
                }

                if (message.role != "user" && message.role != "assistant")
                {
                    return ValidationResult.Failure($"Invalid message role: {message.role}");
                }
            }

            return ValidationResult.Success("Request validation passed");
        }

        private async Task<bool> WaitForRateLimit(ClaudeAPIRequest request)
        {
            lock (_requestLock)
            {
                int estimatedTokens = EstimateTokens(request);
                
                if (!_rateLimitInfo.CanMakeRequest(estimatedTokens))
                {
                    TimeSpan waitTime = _rateLimitInfo.resetTime - DateTime.Now;
                    if (waitTime.TotalMinutes > 1)
                    {
                        return false;
                    }
                }

                string requestKey = $"{request.model}_{DateTime.Now:yyyyMMddHHmm}";
                if (_lastRequestTimes.ContainsKey(requestKey))
                {
                    TimeSpan timeSinceLastRequest = DateTime.Now - _lastRequestTimes[requestKey];
                    if (timeSinceLastRequest.TotalSeconds < 1)
                    {
                        return false;
                    }
                }

                _lastRequestTimes[requestKey] = DateTime.Now;
                return true;
            }
        }

        private int EstimateTokens(ClaudeAPIRequest request)
        {
            int estimatedTokens = 0;

            if (!string.IsNullOrEmpty(request.system))
            {
                estimatedTokens += request.system.Length / 4;
            }

            foreach (ClaudeMessage message in request.messages)
            {
                if (!string.IsNullOrEmpty(message.content))
                {
                    estimatedTokens += message.content.Length / 4;
                }
            }

            estimatedTokens += request.max_tokens;

            return Math.Max(100, estimatedTokens);
        }

        private bool IsRetryableError(ClaudeError error)
        {
            if (error == null)
                return false;

            string errorType = error.type?.ToLower() ?? string.Empty;
            string errorMessage = error.message?.ToLower() ?? string.Empty;

            string[] retryableTypes = { "server_error", "timeout", "rate_limit_error", "overloaded_error" };
            string[] retryableMessages = { "timeout", "server error", "rate limit", "overloaded", "busy" };

            foreach (string type in retryableTypes)
            {
                if (errorType.Contains(type))
                    return true;
            }

            foreach (string message in retryableMessages)
            {
                if (errorMessage.Contains(message))
                    return true;
            }

            return false;
        }

        private void UpdateRateLimitInfo(Dictionary<string, string> headers)
        {
            if (headers != null)
            {
                _rateLimitInfo.UpdateFromHeaders(headers);
            }
        }

        private ClaudeConfiguration LoadConfiguration()
        {
            string apiKey = _keyManager.GetClaudeAPIKey();
            ClaudeConfiguration config = new ClaudeConfiguration(apiKey);
            
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("[AI DEBUG] No API key found in ClaudeAPIClient configuration");
            }
            
            return config;
        }

        public async Task<ValidationResult> TestConnectionAsync()
        {
            try
            {
                if (!IsConfigured)
                {
                    return ValidationResult.Failure("API client is not configured");
                }

                ClaudeAPIRequest testRequest = new ClaudeAPIRequest("Hello", "Respond with just 'Hello' to test the connection.");
                testRequest.max_tokens = 10;
                testRequest.temperature = 0.0f;

                OnStatusUpdate?.Invoke("Testing API connection...");

                ClaudeAPIResponse response = await SendSingleRequest(testRequest);

                if (response.IsSuccess)
                {
                    return ValidationResult.Success("Connection test successful");
                }
                else
                {
                    string errorMessage = response.error?.message ?? "Unknown connection error";
                    return ValidationResult.Failure($"Connection test failed: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure($"Connection test exception: {ex.Message}");
            }
        }

        public void UpdateConfiguration(ClaudeConfiguration newConfiguration)
        {
            if (newConfiguration == null)
            {
                _logger.LogError("Cannot update with null configuration");
                return;
            }

            if (!newConfiguration.IsValid())
            {
                _logger.LogError("Cannot update with invalid configuration");
                return;
            }

            _configuration.baseUrl = newConfiguration.baseUrl;
            _configuration.defaultModel = newConfiguration.defaultModel;
            _configuration.maxTokens = newConfiguration.maxTokens;
            _configuration.temperature = newConfiguration.temperature;
            _configuration.timeoutSeconds = newConfiguration.timeoutSeconds;
            _configuration.maxRetries = newConfiguration.maxRetries;
            _configuration.retryDelaySeconds = newConfiguration.retryDelaySeconds;
            _configuration.enableLogging = newConfiguration.enableLogging;

            _logger.Log("Claude API configuration updated successfully");
        }

        public ClaudeAPIClient CreateQuickRequest(string userMessage, string systemPrompt = null)
        {
            if (string.IsNullOrEmpty(userMessage))
            {
                throw new ArgumentException("User message cannot be empty", nameof(userMessage));
            }

            return this;
        }

        public async Task<string> GetQuickResponseAsync(string userMessage, string systemPrompt = null)
        {
            try
            {
                ClaudeAPIRequest request = new ClaudeAPIRequest(userMessage, systemPrompt);
                ClaudeAPIResponse response = await SendRequestAsync(request);

                if (response.IsSuccess)
                {
                    return response.GetTextContent();
                }
                else
                {
                    string errorMessage = response.error?.message ?? "Unknown error";
                    _logger.LogError($"Quick response failed: {errorMessage}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Quick response exception: {ex.Message}");
                return null;
            }
        }

        public void ClearCache()
        {
            lock (_requestLock)
            {
                _lastRequestTimes.Clear();
                _logger.Log("Claude API client cache cleared");
            }
        }

        public Dictionary<string, object> GetStatus()
        {
            Dictionary<string, object> status = new Dictionary<string, object>
            {
                { "IsConfigured", IsConfigured },
                { "HasAPIKey", _keyManager.HasClaudeAPIKey() },
                { "RemainingRequests", _rateLimitInfo.remainingRequests },
                { "RemainingTokens", _rateLimitInfo.remainingTokens },
                { "RateLimitReset", _rateLimitInfo.resetTime.ToString("yyyy-MM-dd HH:mm:ss") },
                { "CachedRequests", _lastRequestTimes.Count },
                { "Configuration", new Dictionary<string, object>
                    {
                        { "BaseUrl", _configuration.baseUrl },
                        { "DefaultModel", _configuration.defaultModel },
                        { "MaxTokens", _configuration.maxTokens },
                        { "Temperature", _configuration.temperature },
                        { "TimeoutSeconds", _configuration.timeoutSeconds },
                        { "MaxRetries", _configuration.maxRetries },
                        { "EnableLogging", _configuration.enableLogging }
                    }
                }
            };

            return status;
        }
    }
}