using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.AI.Services
{
    /// <summary>
    /// Manages multi-turn AI conversations with context preservation, memory management,
    /// and conversation history persistence for enhanced user interaction experiences
    /// </summary>
    public class ConversationManager
    {
        private readonly Dictionary<string, Conversation> _activeConversations;
        private readonly Dictionary<string, DateTime> _conversationLastAccess;
        private readonly object _conversationLock = new object();
        private readonly ILogger _logger;
        private readonly int _maxConversations;
        private readonly TimeSpan _conversationTimeout;
        private readonly int _maxMessagesPerConversation;

        public event Action<string> OnConversationStarted;
        public event Action<string> OnConversationEnded;
        public event Action<string, ConversationMessage> OnMessageAdded;
        public event Action<string> OnConversationCleanup;

        public int ActiveConversationCount
        {
            get
            {
                lock (_conversationLock)
                {
                    return _activeConversations.Count;
                }
            }
        }

        public ConversationManager() : this(new ConsoleLogger())
        {
        }

        public ConversationManager(ILogger logger, int maxConversations = 50, int maxMessagesPerConversation = 100)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _maxConversations = maxConversations;
            _maxMessagesPerConversation = maxMessagesPerConversation;
            _conversationTimeout = TimeSpan.FromHours(2); // Conversations timeout after 2 hours of inactivity

            _activeConversations = new Dictionary<string, Conversation>();
            _conversationLastAccess = new Dictionary<string, DateTime>();

            // Start cleanup task
            _ = Task.Run(PeriodicCleanupAsync);
        }

        /// <summary>
        /// Starts a new conversation with optional initial context and configuration
        /// </summary>
        public async Task<string> StartConversationAsync(string userId = null, string initialContext = null, ConversationConfiguration configuration = null)
        {
            string conversationId = GenerateConversationId();
            
            try
            {
                lock (_conversationLock)
                {
                    // Clean up if we're at capacity
                    if (_activeConversations.Count >= _maxConversations)
                    {
                        CleanupOldestConversations(5);
                    }

                    Conversation conversation = new Conversation
                    {
                        Id = conversationId,
                        UserId = userId ?? "anonymous",
                        StartedAt = DateTime.Now,
                        LastAccessedAt = DateTime.Now,
                        Configuration = configuration ?? new ConversationConfiguration(),
                        Messages = new List<ConversationMessage>(),
                        Context = new ConversationContext()
                    };

                    // Add initial context if provided
                    if (!string.IsNullOrEmpty(initialContext))
                    {
                        conversation.Context.InitialContext = initialContext;
                    }

                    _activeConversations[conversationId] = conversation;
                    _conversationLastAccess[conversationId] = DateTime.Now;
                }

                _logger.Log($"Started new conversation: {conversationId} for user: {userId ?? "anonymous"}");
                OnConversationStarted?.Invoke(conversationId);

                return conversationId;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to start conversation: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Adds a message to an existing conversation with role-based validation and context management
        /// </summary>
        public async Task<bool> AddMessageAsync(string conversationId, string role, string content, bool preAddToDict = false, Conversation conversation = null)
        {
            if (string.IsNullOrEmpty(conversationId))
                throw new ArgumentException("Conversation ID cannot be null or empty", nameof(conversationId));
            if (string.IsNullOrEmpty(role))
                throw new ArgumentException("Role cannot be null or empty", nameof(role));
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Content cannot be null or empty", nameof(content));

            try
            {
                Conversation targetConversation = conversation;

                if (!preAddToDict)
                {
                    lock (_conversationLock)
                    {
                        if (!_activeConversations.ContainsKey(conversationId))
                        {
                            _logger.LogWarning($"Attempted to add message to non-existent conversation: {conversationId}");
                            return false;
                        }

                        targetConversation = _activeConversations[conversationId];
                        _conversationLastAccess[conversationId] = DateTime.Now;
                        targetConversation.LastAccessedAt = DateTime.Now;
                    }
                }

                // Validate role
                if (!IsValidRole(role))
                {
                    _logger.LogError($"Invalid message role: {role}");
                    return false;
                }

                ConversationMessage message = new ConversationMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    Role = role,
                    Content = content,
                    Timestamp = DateTime.Now,
                    TokenCount = EstimateTokenCount(content),
                    Metadata = new Dictionary<string, object>()
                };

                lock (_conversationLock)
                {
                    // Check message limit
                    if (targetConversation.Messages.Count >= _maxMessagesPerConversation)
                    {
                        // Remove oldest non-system messages to make room
                        TrimConversationMessages(targetConversation);
                    }

                    targetConversation.Messages.Add(message);
                    
                    // Update conversation statistics
                    targetConversation.TotalMessages++;
                    targetConversation.TotalTokens += message.TokenCount;
                }

                _logger.Log($"Added {role} message to conversation {conversationId}: {content.Length} characters");
                OnMessageAdded?.Invoke(conversationId, message);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to add message to conversation {conversationId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Retrieves conversation history with optional filtering and pagination
        /// </summary>
        public async Task<ConversationHistory> GetConversationHistoryAsync(string conversationId, int maxMessages = 50, string roleFilter = null)
        {
            if (string.IsNullOrEmpty(conversationId))
                throw new ArgumentException("Conversation ID cannot be null or empty", nameof(conversationId));

            try
            {
                lock (_conversationLock)
                {
                    if (!_activeConversations.ContainsKey(conversationId))
                    {
                        return null;
                    }

                    Conversation conversation = _activeConversations[conversationId];
                    _conversationLastAccess[conversationId] = DateTime.Now;
                    conversation.LastAccessedAt = DateTime.Now;

                    List<ConversationMessage> messages = conversation.Messages;

                    // Apply role filter if specified
                    if (!string.IsNullOrEmpty(roleFilter))
                    {
                        messages = messages.Where(m => m.Role.Equals(roleFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                    }

                    // Apply pagination
                    if (maxMessages > 0 && messages.Count > maxMessages)
                    {
                        messages = messages.TakeLast(maxMessages).ToList();
                    }

                    return new ConversationHistory
                    {
                        ConversationId = conversationId,
                        UserId = conversation.UserId,
                        StartedAt = conversation.StartedAt,
                        LastAccessedAt = conversation.LastAccessedAt,
                        TotalMessages = conversation.TotalMessages,
                        TotalTokens = conversation.TotalTokens,
                        Messages = messages,
                        Context = conversation.Context,
                        Configuration = conversation.Configuration
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get conversation history for {conversationId}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Ends a conversation and optionally preserves it for future reference
        /// </summary>
        public async Task<bool> EndConversationAsync(string conversationId, bool preserve = false)
        {
            if (string.IsNullOrEmpty(conversationId))
                return false;

            try
            {
                lock (_conversationLock)
                {
                    if (!_activeConversations.ContainsKey(conversationId))
                    {
                        return false;
                    }

                    Conversation conversation = _activeConversations[conversationId];
                    conversation.EndedAt = DateTime.Now;

                    if (preserve)
                    {
                        // In a real implementation, this would save to persistent storage
                        _logger.Log($"Conversation {conversationId} ended and preserved with {conversation.TotalMessages} messages");
                    }
                    else
                    {
                        _logger.Log($"Conversation {conversationId} ended and removed with {conversation.TotalMessages} messages");
                    }

                    _activeConversations.Remove(conversationId);
                    _conversationLastAccess.Remove(conversationId);
                }

                OnConversationEnded?.Invoke(conversationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to end conversation {conversationId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets conversation context for AI prompt building
        /// </summary>
        public async Task<string> GetConversationContextAsync(string conversationId, int contextMessageCount = 10)
        {
            ConversationHistory history = await GetConversationHistoryAsync(conversationId, contextMessageCount);
            if (history == null)
                return string.Empty;

            List<string> contextParts = new List<string>();

            // Add initial context if available
            if (!string.IsNullOrEmpty(history.Context.InitialContext))
            {
                contextParts.Add($"Initial Context: {history.Context.InitialContext}");
            }

            // Add recent messages
            foreach (ConversationMessage message in history.Messages.TakeLast(contextMessageCount))
            {
                if (message.Role != "system") // Exclude system messages from context
                {
                    contextParts.Add($"{message.Role}: {message.Content}");
                }
            }

            return string.Join("\n\n", contextParts);
        }

        /// <summary>
        /// Clears all conversations (use with caution)
        /// </summary>
        public void ClearAllConversations()
        {
            lock (_conversationLock)
            {
                int count = _activeConversations.Count;
                _activeConversations.Clear();
                _conversationLastAccess.Clear();
                _logger.Log($"Cleared all {count} active conversations");
            }
        }

        /// <summary>
        /// Gets statistics about conversation manager performance
        /// </summary>
        public ConversationManagerStatistics GetStatistics()
        {
            lock (_conversationLock)
            {
                int totalMessages = _activeConversations.Values.Sum(c => c.TotalMessages);
                int totalTokens = _activeConversations.Values.Sum(c => c.TotalTokens);
                TimeSpan averageConversationAge = _activeConversations.Count > 0
                    ? TimeSpan.FromTicks(_activeConversations.Values.Sum(c => (DateTime.Now - c.StartedAt).Ticks) / _activeConversations.Count)
                    : TimeSpan.Zero;

                return new ConversationManagerStatistics
                {
                    ActiveConversations = _activeConversations.Count,
                    TotalMessages = totalMessages,
                    TotalTokens = totalTokens,
                    AverageMessagesPerConversation = _activeConversations.Count > 0 ? (float)totalMessages / _activeConversations.Count : 0f,
                    AverageConversationAge = averageConversationAge,
                    MemoryUsageEstimate = EstimateMemoryUsage()
                };
            }
        }

        #region Private Helper Methods

        private string GenerateConversationId()
        {
            return $"conv_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString()[..8]}";
        }

        private bool IsValidRole(string role)
        {
            string[] validRoles = { "user", "assistant", "system" };
            return validRoles.Contains(role.ToLower());
        }

        private int EstimateTokenCount(string content)
        {
            // Simple token estimation: ~4 characters per token
            return Math.Max(1, content.Length / 4);
        }

        private void TrimConversationMessages(Conversation conversation)
        {
            // Keep system messages and recent messages
            List<ConversationMessage> systemMessages = conversation.Messages.Where(m => m.Role == "system").ToList();
            List<ConversationMessage> otherMessages = conversation.Messages.Where(m => m.Role != "system").ToList();

            // Keep last 80% of non-system messages
            int keepCount = Math.Max(10, (int)(otherMessages.Count * 0.8));
            List<ConversationMessage> messagesToKeep = otherMessages.TakeLast(keepCount).ToList();

            conversation.Messages = systemMessages.Concat(messagesToKeep).OrderBy(m => m.Timestamp).ToList();
            
            _logger.Log($"Trimmed conversation {conversation.Id} from {systemMessages.Count + otherMessages.Count} to {conversation.Messages.Count} messages");
        }

        private async Task UpdateConversationContextAsync(Conversation conversation, ConversationMessage message)
        {
            // Update context based on message content and role
            if (message.Role == "user")
            {
                conversation.Context.LastUserMessage = message.Content;
                conversation.Context.UserMessageCount++;
            }
            else if (message.Role == "assistant")
            {
                conversation.Context.LastAssistantMessage = message.Content;
                conversation.Context.AssistantMessageCount++;
            }

            // Extract and update topics/keywords from message content
            List<string> keywords = ExtractKeywords(message.Content);
            foreach (string keyword in keywords)
            {
                if (!conversation.Context.Topics.Contains(keyword))
                {
                    conversation.Context.Topics.Add(keyword);
                }
            }

            // Limit topic list size
            if (conversation.Context.Topics.Count > 20)
            {
                conversation.Context.Topics = conversation.Context.Topics.TakeLast(20).ToList();
            }
        }

        private List<string> ExtractKeywords(string content)
        {
            // Simple keyword extraction based on Unity/development terms
            string[] keywords = 
            {
                "unity", "scriptableobject", "monobehaviour", "gameobject", "component",
                "architecture", "pattern", "api", "interface", "class", "method", "property",
                "documentation", "project", "template", "generator", "analysis", "export"
            };

            List<string> found = new List<string>();
            string lowerContent = content.ToLower();

            foreach (string keyword in keywords)
            {
                if (lowerContent.Contains(keyword))
                {
                    found.Add(keyword);
                }
            }

            return found.Distinct().ToList();
        }

        private void CleanupOldestConversations(int count)
        {
            List<string> oldestConversations = _conversationLastAccess
                .OrderBy(kvp => kvp.Value)
                .Take(count)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (string conversationId in oldestConversations)
            {
                _activeConversations.Remove(conversationId);
                _conversationLastAccess.Remove(conversationId);
                OnConversationCleanup?.Invoke(conversationId);
            }

            _logger.Log($"Cleaned up {count} oldest conversations");
        }

        private async Task PeriodicCleanupAsync()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(30)); // Run every 30 minutes

                    lock (_conversationLock)
                    {
                        DateTime cutoffTime = DateTime.Now - _conversationTimeout;
                        List<string> expiredConversations = _conversationLastAccess
                            .Where(kvp => kvp.Value < cutoffTime)
                            .Select(kvp => kvp.Key)
                            .ToList();

                        foreach (string conversationId in expiredConversations)
                        {
                            _activeConversations.Remove(conversationId);
                            _conversationLastAccess.Remove(conversationId);
                            OnConversationCleanup?.Invoke(conversationId);
                        }

                        if (expiredConversations.Any())
                        {
                            _logger.Log($"Cleaned up {expiredConversations.Count} expired conversations");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error during periodic cleanup: {ex.Message}");
                }
            }
        }

        private long EstimateMemoryUsage()
        {
            // Rough memory usage estimate
            long totalBytes = 0;
            
            foreach (Conversation conversation in _activeConversations.Values)
            {
                totalBytes += conversation.Messages.Sum(m => m.Content.Length * 2); // Unicode characters = 2 bytes
                totalBytes += conversation.Messages.Count * 200; // Overhead per message
            }

            return totalBytes;
        }

        #endregion
    }

    #region Supporting Data Models

    /// <summary>
    /// Represents a complete conversation with metadata and configuration
    /// </summary>
    [Serializable]
    public class Conversation
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime LastAccessedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int TotalMessages { get; set; }
        public int TotalTokens { get; set; }
        public List<ConversationMessage> Messages { get; set; }
        public ConversationContext Context { get; set; }
        public ConversationConfiguration Configuration { get; set; }

        public Conversation()
        {
            Messages = new List<ConversationMessage>();
            Context = new ConversationContext();
            Configuration = new ConversationConfiguration();
        }

        public TimeSpan Duration => (EndedAt ?? DateTime.Now) - StartedAt;
        public bool IsActive => !EndedAt.HasValue;
    }

    /// <summary>
    /// Individual message within a conversation
    /// </summary>
    [Serializable]
    public class ConversationMessage
    {
        public string Id { get; set; }
        public string Role { get; set; } // "user", "assistant", "system"
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public int TokenCount { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public ConversationMessage()
        {
            Metadata = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Context information maintained throughout a conversation
    /// </summary>
    [Serializable]
    public class ConversationContext
    {
        public string InitialContext { get; set; }
        public string LastUserMessage { get; set; }
        public string LastAssistantMessage { get; set; }
        public int UserMessageCount { get; set; }
        public int AssistantMessageCount { get; set; }
        public List<string> Topics { get; set; }
        public Dictionary<string, object> CustomData { get; set; }

        public ConversationContext()
        {
            Topics = new List<string>();
            CustomData = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Configuration settings for conversation behavior
    /// </summary>
    [Serializable]
    public class ConversationConfiguration
    {
        public int MaxMessages { get; set; } = 100;
        public int MaxTokensPerMessage { get; set; } = 4000;
        public bool PreserveSystemMessages { get; set; } = true;
        public bool EnableContextTracking { get; set; } = true;
        public bool EnableTopicExtraction { get; set; } = true;
        public TimeSpan InactivityTimeout { get; set; } = TimeSpan.FromHours(2);
    }

    /// <summary>
    /// Conversation history with filtered messages and metadata
    /// </summary>
    public class ConversationHistory
    {
        public string ConversationId { get; set; }
        public string UserId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime LastAccessedAt { get; set; }
        public int TotalMessages { get; set; }
        public int TotalTokens { get; set; }
        public List<ConversationMessage> Messages { get; set; }
        public ConversationContext Context { get; set; }
        public ConversationConfiguration Configuration { get; set; }

        public ConversationHistory()
        {
            Messages = new List<ConversationMessage>();
        }
    }

    /// <summary>
    /// Statistics about conversation manager performance and usage
    /// </summary>
    public class ConversationManagerStatistics
    {
        public int ActiveConversations { get; set; }
        public int TotalMessages { get; set; }
        public int TotalTokens { get; set; }
        public float AverageMessagesPerConversation { get; set; }
        public TimeSpan AverageConversationAge { get; set; }
        public long MemoryUsageEstimate { get; set; }
    }

    #endregion
}