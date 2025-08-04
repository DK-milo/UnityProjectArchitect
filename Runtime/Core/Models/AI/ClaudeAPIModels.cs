using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityProjectArchitect.Core.AI
{
    [Serializable]
    public class ClaudeAPIRequest
    {
        public string model { get; set; } = "claude-3-sonnet-20240229";
        public int max_tokens { get; set; } = 4000;
        public List<ClaudeMessage> messages { get; set; }
        public ClaudeSystemMessage system { get; set; }
        public float temperature { get; set; } = 0.7f;
        public List<string> stop_sequences { get; set; }
        public string stream { get; set; } = "false";

        public ClaudeAPIRequest()
        {
            messages = new List<ClaudeMessage>();
            stop_sequences = new List<string>();
        }

        public ClaudeAPIRequest(string userMessage, string systemPrompt = null) : this()
        {
            messages.Add(new ClaudeMessage("user", userMessage));
            if (!string.IsNullOrEmpty(systemPrompt))
            {
                system = new ClaudeSystemMessage(systemPrompt);
            }
        }
    }

    [Serializable]
    public class ClaudeMessage
    {
        public string role { get; set; }
        public string content { get; set; }

        public ClaudeMessage()
        {
        }

        public ClaudeMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }

    [Serializable]
    public class ClaudeSystemMessage
    {
        public string content { get; set; }

        public ClaudeSystemMessage()
        {
        }

        public ClaudeSystemMessage(string content)
        {
            this.content = content;
        }
    }

    [Serializable]
    public class ClaudeAPIResponse
    {
        public string id { get; set; }
        public string type { get; set; }
        public string role { get; set; }
        public List<ClaudeContent> content { get; set; }
        public string model { get; set; }
        public string stop_reason { get; set; }
        public string stop_sequence { get; set; }
        public ClaudeUsage usage { get; set; }
        public ClaudeError error { get; set; }

        public ClaudeAPIResponse()
        {
            content = new List<ClaudeContent>();
        }

        public string GetTextContent()
        {
            if (content == null || content.Count == 0)
                return string.Empty;

            List<string> textParts = new List<string>();
            foreach (ClaudeContent contentItem in content)
            {
                if (contentItem.type == "text" && !string.IsNullOrEmpty(contentItem.text))
                {
                    textParts.Add(contentItem.text);
                }
            }

            return string.Join("\n", textParts);
        }

        public bool IsSuccess => error == null && !string.IsNullOrEmpty(id);
        public bool HasContent => content != null && content.Count > 0;
    }

    [Serializable]
    public class ClaudeContent
    {
        public string type { get; set; }
        public string text { get; set; }

        public ClaudeContent()
        {
        }

        public ClaudeContent(string type, string text)
        {
            this.type = type;
            this.text = text;
        }
    }

    [Serializable]
    public class ClaudeUsage
    {
        public int input_tokens { get; set; }
        public int output_tokens { get; set; }

        public int TotalTokens => input_tokens + output_tokens;
    }

    [Serializable]
    public class ClaudeError
    {
        public string type { get; set; }
        public string message { get; set; }

        public ClaudeError()
        {
        }

        public ClaudeError(string type, string message)
        {
            this.type = type;
            this.message = message;
        }

        public override string ToString()
        {
            return $"Claude API Error ({type}): {message}";
        }
    }

    [Serializable]
    public class ClaudeConfiguration
    {
        public string apiKey { get; set; }
        public string baseUrl { get; set; } = "https://api.anthropic.com/v1/messages";
        public string defaultModel { get; set; } = "claude-3-sonnet-20240229";
        public int maxTokens { get; set; } = 4000;
        public float temperature { get; set; } = 0.7f;
        public int timeoutSeconds { get; set; } = 60;
        public int maxRetries { get; set; } = 3;
        public float retryDelaySeconds { get; set; } = 1.0f;
        public bool enableLogging { get; set; } = true;

        public ClaudeConfiguration()
        {
        }

        public ClaudeConfiguration(string apiKey) : this()
        {
            this.apiKey = apiKey;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(apiKey) && 
                   !string.IsNullOrEmpty(baseUrl) && 
                   !string.IsNullOrEmpty(defaultModel) &&
                   maxTokens > 0 && 
                   timeoutSeconds > 0;
        }

        public Dictionary<string, string> GetHeaders()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "x-api-key", apiKey },
                { "anthropic-version", "2023-06-01" }
            };

            return headers;
        }
    }

    [Serializable]
    public class ClaudeRateLimitInfo
    {
        public int requestsPerMinute { get; set; } = 50;
        public int tokensPerMinute { get; set; } = 40000;
        public int remainingRequests { get; set; }
        public int remainingTokens { get; set; }
        public DateTime resetTime { get; set; }

        public ClaudeRateLimitInfo()
        {
            remainingRequests = requestsPerMinute;
            remainingTokens = tokensPerMinute;
            resetTime = DateTime.Now.AddMinutes(1);
        }

        public bool CanMakeRequest(int estimatedTokens = 1000)
        {
            return remainingRequests > 0 && remainingTokens >= estimatedTokens;
        }

        public void UpdateFromHeaders(Dictionary<string, string> headers)
        {
            if (headers.ContainsKey("x-ratelimit-requests-remaining"))
            {
                int.TryParse(headers["x-ratelimit-requests-remaining"], out remainingRequests);
            }

            if (headers.ContainsKey("x-ratelimit-tokens-remaining"))
            {
                int.TryParse(headers["x-ratelimit-tokens-remaining"], out remainingTokens);
            }

            if (headers.ContainsKey("x-ratelimit-requests-reset"))
            {
                if (DateTime.TryParse(headers["x-ratelimit-requests-reset"], out DateTime reset))
                {
                    resetTime = reset;
                }
            }
        }
    }

    public enum ClaudeModel
    {
        Claude3Haiku,
        Claude3Sonnet,
        Claude3Opus,
        Claude35Sonnet
    }

    public static class ClaudeModelExtensions
    {
        private static readonly Dictionary<ClaudeModel, string> ModelNames = new Dictionary<ClaudeModel, string>
        {
            { ClaudeModel.Claude3Haiku, "claude-3-haiku-20240307" },
            { ClaudeModel.Claude3Sonnet, "claude-3-sonnet-20240229" },
            { ClaudeModel.Claude3Opus, "claude-3-opus-20240229" },
            { ClaudeModel.Claude35Sonnet, "claude-3-5-sonnet-20240620" }
        };

        private static readonly Dictionary<ClaudeModel, int> ModelTokenLimits = new Dictionary<ClaudeModel, int>
        {
            { ClaudeModel.Claude3Haiku, 200000 },
            { ClaudeModel.Claude3Sonnet, 200000 },
            { ClaudeModel.Claude3Opus, 200000 },
            { ClaudeModel.Claude35Sonnet, 200000 }
        };

        public static string GetModelName(this ClaudeModel model)
        {
            return ModelNames.ContainsKey(model) ? ModelNames[model] : ModelNames[ClaudeModel.Claude3Sonnet];
        }

        public static int GetTokenLimit(this ClaudeModel model)
        {
            return ModelTokenLimits.ContainsKey(model) ? ModelTokenLimits[model] : 200000;
        }

        public static ClaudeModel ParseModel(string modelName)
        {
            foreach (KeyValuePair<ClaudeModel, string> kvp in ModelNames)
            {
                if (kvp.Value.Equals(modelName, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Key;
                }
            }
            return ClaudeModel.Claude3Sonnet;
        }

        public static List<ClaudeModel> GetAllModels()
        {
            return new List<ClaudeModel>
            {
                ClaudeModel.Claude3Haiku,
                ClaudeModel.Claude3Sonnet,
                ClaudeModel.Claude3Opus,
                ClaudeModel.Claude35Sonnet
            };
        }

        public static bool IsValidModel(string modelName)
        {
            return ModelNames.ContainsValue(modelName);
        }
    }
}