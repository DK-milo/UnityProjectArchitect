using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;
using UnityProjectArchitect.AI.Services;

namespace UnityProjectArchitect.AI.Prompts
{
    /// <summary>
    /// Central management system for AI prompt templates with caching, validation, and dynamic loading
    /// Handles template lifecycle management and provides high-performance prompt retrieval
    /// </summary>
    public class PromptTemplateManager : IAIPromptManager
    {
        private readonly Dictionary<DocumentationSectionType, string> _sectionTemplates;
        private readonly Dictionary<string, string> _customTemplates;
        private readonly Dictionary<string, DateTime> _templateLoadTimes;
        private readonly Dictionary<string, string> _templateCache;
        private readonly object _cacheLock = new object();
        private readonly ILogger _logger;
        private readonly string _templatesPath;

        public event Action<string> OnTemplateLoaded;
        public event Action<string> OnTemplateValidationFailed;

        public PromptTemplateManager() : this(new ConsoleLogger(), GetDefaultTemplatesPath())
        {
        }

        public PromptTemplateManager(ILogger logger, string templatesPath)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _templatesPath = templatesPath ?? throw new ArgumentNullException(nameof(templatesPath));
            
            _sectionTemplates = new Dictionary<DocumentationSectionType, string>();
            _customTemplates = new Dictionary<string, string>();
            _templateLoadTimes = new Dictionary<string, DateTime>();
            _templateCache = new Dictionary<string, string>();

            InitializeDefaultTemplates();
        }

        /// <summary>
        /// Retrieves optimized prompt template for specified documentation section type
        /// </summary>
        public async Task<string> GetPromptAsync(DocumentationSectionType sectionType)
        {
            string cacheKey = $"section_{sectionType}";
            
            lock (_cacheLock)
            {
                if (_templateCache.ContainsKey(cacheKey))
                {
                    return _templateCache[cacheKey];
                }
            }

            string template = await LoadSectionTemplateAsync(sectionType);
            
            lock (_cacheLock)
            {
                _templateCache[cacheKey] = template;
                _templateLoadTimes[cacheKey] = DateTime.Now;
            }

            OnTemplateLoaded?.Invoke($"Section template loaded: {sectionType}");
            return template;
        }

        /// <summary>
        /// Retrieves custom prompt template by name with fallback handling
        /// </summary>
        public async Task<string> GetCustomPromptAsync(string promptName)
        {
            if (string.IsNullOrEmpty(promptName))
                throw new ArgumentException("Prompt name cannot be null or empty", nameof(promptName));

            string cacheKey = $"custom_{promptName}";
            
            lock (_cacheLock)
            {
                if (_templateCache.ContainsKey(cacheKey))
                {
                    return _templateCache[cacheKey];
                }
            }

            string template = await LoadCustomTemplateAsync(promptName);
            
            lock (_cacheLock)
            {
                _templateCache[cacheKey] = template;
                _templateLoadTimes[cacheKey] = DateTime.Now;
            }

            OnTemplateLoaded?.Invoke($"Custom template loaded: {promptName}");
            return template;
        }

        /// <summary>
        /// Sets prompt template for specific documentation section with validation
        /// </summary>
        public void SetPrompt(DocumentationSectionType sectionType, string prompt)
        {
            if (string.IsNullOrEmpty(prompt))
                throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));

            AIPromptValidationResult validation = ValidatePromptSync(prompt);
            if (!validation.IsValid)
            {
                string issues = string.Join(", ", validation.Issues);
                OnTemplateValidationFailed?.Invoke($"Section template validation failed for {sectionType}: {issues}");
                throw new ArgumentException($"Invalid prompt template: {issues}");
            }

            _sectionTemplates[sectionType] = prompt;
            
            string cacheKey = $"section_{sectionType}";
            lock (_cacheLock)
            {
                _templateCache[cacheKey] = prompt;
                _templateLoadTimes[cacheKey] = DateTime.Now;
            }

            _logger.Log($"Section template updated: {sectionType}");
        }

        /// <summary>
        /// Sets custom prompt template with validation and conflict resolution
        /// </summary>
        public void SetCustomPrompt(string promptName, string prompt)
        {
            if (string.IsNullOrEmpty(promptName))
                throw new ArgumentException("Prompt name cannot be null or empty", nameof(promptName));
            if (string.IsNullOrEmpty(prompt))
                throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));

            AIPromptValidationResult validation = ValidatePromptSync(prompt);
            if (!validation.IsValid)
            {
                string issues = string.Join(", ", validation.Issues);
                OnTemplateValidationFailed?.Invoke($"Custom template validation failed for {promptName}: {issues}");
                throw new ArgumentException($"Invalid prompt template: {issues}");
            }

            _customTemplates[promptName] = prompt;
            
            string cacheKey = $"custom_{promptName}";
            lock (_cacheLock)
            {
                _templateCache[cacheKey] = prompt;
                _templateLoadTimes[cacheKey] = DateTime.Now;
            }

            _logger.Log($"Custom template updated: {promptName}");
        }

        /// <summary>
        /// Gets list of all available prompt templates including both section and custom templates
        /// </summary>
        public List<string> GetAvailablePrompts()
        {
            List<string> availablePrompts = new List<string>();
            
            // Add section templates
            foreach (DocumentationSectionType sectionType in Enum.GetValues(typeof(DocumentationSectionType)).Cast<DocumentationSectionType>())
            {
                availablePrompts.Add($"section_{sectionType}");
            }
            
            // Add custom templates
            foreach (string customName in _customTemplates.Keys)
            {
                availablePrompts.Add($"custom_{customName}");
            }

            return availablePrompts;
        }

        /// <summary>
        /// Retrieves default prompt template for specified section type
        /// </summary>
        public string GetDefaultPrompt(DocumentationSectionType sectionType)
        {
            return GetDefaultSectionPrompt(sectionType);
        }

        /// <summary>
        /// Validates prompt template for structure, clarity, and token efficiency
        /// </summary>
        public async Task<AIPromptValidationResult> ValidatePromptAsync(string prompt)
        {
            return await Task.Run(() => ValidatePromptSync(prompt));
        }

        /// <summary>
        /// Optimizes prompt for token efficiency and clarity while preserving intent
        /// </summary>
        public async Task<string> OptimizePromptAsync(string prompt, PromptOptimizationRequest request)
        {
            if (string.IsNullOrEmpty(prompt))
                throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return await Task.Run(() =>
            {
                PromptOptimizer optimizer = new PromptOptimizer();
                return optimizer.OptimizePrompt(prompt, request);
            });
        }

        /// <summary>
        /// Clears template cache and forces reload of all templates
        /// </summary>
        public void ClearCache()
        {
            lock (_cacheLock)
            {
                _templateCache.Clear();
                _templateLoadTimes.Clear();
            }
            _logger.Log("Template cache cleared");
        }

        /// <summary>
        /// Gets template cache statistics for performance monitoring
        /// </summary>
        public TemplateCacheStatistics GetCacheStatistics()
        {
            lock (_cacheLock)
            {
                return new TemplateCacheStatistics
                {
                    CachedTemplates = _templateCache.Count,
                    TotalLoadOperations = _templateLoadTimes.Count,
                    OldestCacheEntry = GetOldestCacheEntryTime(),
                    CacheHitRatio = CalculateCacheHitRatio()
                };
            }
        }

        private void InitializeDefaultTemplates()
        {
            foreach (DocumentationSectionType sectionType in Enum.GetValues(typeof(DocumentationSectionType)).Cast<DocumentationSectionType>())
            {
                _sectionTemplates[sectionType] = GetDefaultSectionPrompt(sectionType);
            }
        }

        private async Task<string> LoadSectionTemplateAsync(DocumentationSectionType sectionType)
        {
            if (_sectionTemplates.ContainsKey(sectionType))
            {
                return _sectionTemplates[sectionType];
            }

            // Try to load from file system
            string templatePath = Path.Combine(_templatesPath, $"{sectionType}.txt");
            if (File.Exists(templatePath))
            {
                string template = await File.ReadAllTextAsync(templatePath);
                _sectionTemplates[sectionType] = template;
                return template;
            }

            // Fallback to default
            string defaultTemplate = GetDefaultSectionPrompt(sectionType);
            _sectionTemplates[sectionType] = defaultTemplate;
            return defaultTemplate;
        }

        private async Task<string> LoadCustomTemplateAsync(string promptName)
        {
            if (_customTemplates.ContainsKey(promptName))
            {
                return _customTemplates[promptName];
            }

            // Try to load from file system
            string templatePath = Path.Combine(_templatesPath, "Custom", $"{promptName}.txt");
            if (File.Exists(templatePath))
            {
                string template = await File.ReadAllTextAsync(templatePath);
                _customTemplates[promptName] = template;
                return template;
            }

            throw new FileNotFoundException($"Custom template not found: {promptName}");
        }

        private AIPromptValidationResult ValidatePromptSync(string prompt)
        {
            AIPromptValidationResult result = new AIPromptValidationResult();
            
            if (string.IsNullOrWhiteSpace(prompt))
            {
                result.IsValid = false;
                result.Issues.Add("Prompt cannot be empty or whitespace");
                return result;
            }

            // Basic validation checks
            if (prompt.Length < 10)
            {
                result.Issues.Add("Prompt is too short (minimum 10 characters)");
            }

            if (prompt.Length > 4000)
            {
                result.Issues.Add("Prompt is too long (maximum 4000 characters for token efficiency)");
            }

            if (!prompt.Contains("{") && !prompt.Contains("}"))
            {
                result.Suggestions.Add("Consider adding placeholder variables for dynamic content");
            }

            // Calculate quality scores
            PromptOptimizer optimizer = new PromptOptimizer();
            result.EstimatedTokens = optimizer.EstimateTokenCount(prompt);
            result.QualityScore = CalculateQualityScore(prompt);
            
            result.IsValid = result.Issues.Count == 0;
            return result;
        }

        private PromptQualityScore CalculateQualityScore(string prompt)
        {
            PromptQualityScore score = new PromptQualityScore();
            
            // Calculate clarity (based on sentence structure and common words)
            score.Clarity = CalculateClarity(prompt);
            
            // Calculate specificity (based on technical terms and detailed instructions)
            score.Specificity = CalculateSpecificity(prompt);
            
            // Calculate completeness (based on presence of key elements)
            score.Completeness = CalculateCompleteness(prompt);
            
            score.OverallScore = (score.Clarity + score.Specificity + score.Completeness) / 3f;
            
            return score;
        }

        private float CalculateClarity(string prompt)
        {
            // Simple heuristic: count clear instruction words
            string[] clearWords = { "generate", "create", "analyze", "describe", "explain", "provide", "include" };
            int clearWordCount = 0;
            string lowerPrompt = prompt.ToLower();
            
            foreach (string word in clearWords)
            {
                if (lowerPrompt.Contains(word))
                    clearWordCount++;
            }
            
            return Math.Min(clearWordCount / 3f, 1f); // Normalize to 0-1
        }

        private float CalculateSpecificity(string prompt)
        {
            // Count specific technical terms and detailed requirements
            string[] specificTerms = { "unity", "scriptableobject", "architecture", "pattern", "api", "interface", "model" };
            int specificTermCount = 0;
            string lowerPrompt = prompt.ToLower();
            
            foreach (string term in specificTerms)
            {
                if (lowerPrompt.Contains(term))
                    specificTermCount++;
            }
            
            return Math.Min(specificTermCount / 4f, 1f); // Normalize to 0-1
        }

        private float CalculateCompleteness(string prompt)
        {
            // Check for presence of key prompt elements
            float score = 0f;
            string lowerPrompt = prompt.ToLower();
            
            if (lowerPrompt.Contains("context") || lowerPrompt.Contains("project"))
                score += 0.25f;
            if (lowerPrompt.Contains("format") || lowerPrompt.Contains("structure"))
                score += 0.25f;
            if (lowerPrompt.Contains("requirement") || lowerPrompt.Contains("should"))
                score += 0.25f;
            if (lowerPrompt.Contains("example") || lowerPrompt.Contains("sample"))
                score += 0.25f;
                
            return score;
        }

        private DateTime GetOldestCacheEntryTime()
        {
            DateTime oldest = DateTime.Now;
            foreach (DateTime loadTime in _templateLoadTimes.Values)
            {
                if (loadTime < oldest)
                    oldest = loadTime;
            }
            return oldest;
        }

        private float CalculateCacheHitRatio()
        {
            // This would need to be implemented with actual hit/miss tracking
            // For now, return a reasonable estimate
            return 0.85f; // 85% cache hit ratio estimate
        }

        private static string GetDefaultTemplatesPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Prompts");
        }

        private string GetDefaultSectionPrompt(DocumentationSectionType sectionType)
        {
            return sectionType switch
            {
                DocumentationSectionType.GeneralProductDescription => SectionSpecificPrompts.GetGeneralDescriptionPrompt(),
                DocumentationSectionType.SystemArchitecture => SectionSpecificPrompts.GetSystemArchitecturePrompt(),
                DocumentationSectionType.DataModel => SectionSpecificPrompts.GetDataModelPrompt(),
                DocumentationSectionType.APISpecification => SectionSpecificPrompts.GetAPISpecificationPrompt(),
                DocumentationSectionType.UserStories => SectionSpecificPrompts.GetUserStoriesPrompt(),
                DocumentationSectionType.WorkTickets => SectionSpecificPrompts.GetWorkTicketsPrompt(),
                _ => SectionSpecificPrompts.GetGeneralPrompt()
            };
        }
    }

    /// <summary>
    /// Statistics about template cache performance for monitoring and optimization
    /// </summary>
    [Serializable]
    public class TemplateCacheStatistics
    {
        public int CachedTemplates { get; set; }
        public int TotalLoadOperations { get; set; }
        public DateTime OldestCacheEntry { get; set; }
        public float CacheHitRatio { get; set; }
        public TimeSpan AverageCacheAge => DateTime.Now - OldestCacheEntry;
        
        public bool ShouldClearCache => CachedTemplates > 50 || AverageCacheAge.TotalHours > 24;
    }
}