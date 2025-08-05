using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.AI.Services
{
    /// <summary>
    /// Validates AI-generated content for quality, accuracy, format compliance, and Unity-specific requirements
    /// Provides comprehensive validation with detailed feedback and suggestions for improvement
    /// </summary>
    public class ContentValidator
    {
        private readonly Dictionary<DocumentationSectionType, ValidationRules> _sectionRules;
        private readonly ILogger _logger;
        private readonly ValidationConfiguration _config;

        public event Action<ContentValidationResult> OnValidationComplete;
        public event Action<string> OnValidationWarning;

        public ContentValidator() : this(new ConsoleLogger())
        {
        }

        public ContentValidator(ILogger logger, ValidationConfiguration config = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? new ValidationConfiguration();
            _sectionRules = new Dictionary<DocumentationSectionType, ValidationRules>();

            InitializeSectionRules();
        }

        /// <summary>
        /// Validates AI-generated content against section-specific rules and general quality standards
        /// </summary>
        public async Task<ContentValidationResult> ValidateContentAsync(string content, DocumentationSectionType sectionType)
        {
            if (string.IsNullOrEmpty(content))
            {
                return new ContentValidationResult
                {
                    IsValid = false,
                    Issues = new List<ValidationIssue> { new ValidationIssue("Content cannot be empty", ValidationSeverity.Critical) },
                    OverallScore = 0f
                };
            }

            DateTime startTime = DateTime.Now;
            _logger.Log($"Starting content validation for section: {sectionType}");

            try
            {
                ContentValidationResult result = new ContentValidationResult
                {
                    SectionType = sectionType,
                    ContentLength = content.Length,
                    WordCount = CountWords(content),
                    ValidationTimestamp = DateTime.Now,
                    Issues = new List<ValidationIssue>(),
                    Suggestions = new List<string>(),
                    Metrics = new ContentMetrics()
                };

                // Run all validation checks
                await ValidateGeneralQualityAsync(content, result);
                await ValidateSectionSpecificAsync(content, sectionType, result);
                await ValidateFormatComplianceAsync(content, result);
                await ValidateUnitySpecificContentAsync(content, sectionType, result);
                await ValidateLanguageQualityAsync(content, result);

                // Calculate overall scores
                CalculateValidationScores(result);

                // Determine if content is valid
                result.IsValid = result.Issues.Count == 0 || 
                                result.Issues.All(i => i.Severity != ValidationSeverity.Critical);

                result.ValidationDuration = DateTime.Now - startTime;

                _logger.Log($"Content validation completed in {result.ValidationDuration.TotalMilliseconds:F1}ms - " +
                           $"Valid: {result.IsValid}, Issues: {result.Issues.Count}, Score: {result.OverallScore:F2}");

                OnValidationComplete?.Invoke(result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Content validation failed: {ex.Message}");
                
                return new ContentValidationResult
                {
                    IsValid = false,
                    Issues = new List<ValidationIssue> 
                    { 
                        new ValidationIssue($"Validation error: {ex.Message}", ValidationSeverity.Critical) 
                    },
                    ValidationDuration = DateTime.Now - startTime
                };
            }
        }

        /// <summary>
        /// Validates content structure and formatting for specific documentation sections
        /// </summary>
        public async Task<StructureValidationResult> ValidateStructureAsync(string content, DocumentationSectionType sectionType)
        {
            StructureValidationResult result = new StructureValidationResult
            {
                SectionType = sectionType,
                StructureIssues = new List<string>(),
                MissingElements = new List<string>(),
                FormatIssues = new List<string>()
            };

            if (!_sectionRules.ContainsKey(sectionType))
            {
                result.StructureIssues.Add($"No validation rules defined for section: {sectionType}");
                return result;
            }

            ValidationRules rules = _sectionRules[sectionType];

            // Check required elements
            foreach (string requiredElement in rules.RequiredElements)
            {
                if (!ContentContainsElement(content, requiredElement))
                {
                    result.MissingElements.Add(requiredElement);
                }
            }

            // Check structure patterns
            foreach (StructurePattern pattern in rules.StructurePatterns)
            {
                if (!ValidateStructurePattern(content, pattern))
                {
                    result.StructureIssues.Add($"Missing or invalid structure: {pattern.Description}");
                }
            }

            // Check format requirements
            if (rules.RequiresHeaders && !HasProperHeaders(content))
            {
                result.FormatIssues.Add("Content should include proper section headers");
            }

            if (rules.RequiresBulletPoints && !HasBulletPoints(content))
            {
                result.FormatIssues.Add("Content should include bullet points or lists");
            }

            result.IsValid = result.MissingElements.Count == 0 && 
                           result.StructureIssues.Count == 0 && 
                           result.FormatIssues.Count == 0;

            return result;
        }

        /// <summary>
        /// Validates content for Unity-specific terminology, concepts, and best practices
        /// </summary>
        public async Task<UnityValidationResult> ValidateUnityContentAsync(string content)
        {
            UnityValidationResult result = new UnityValidationResult
            {
                UnityTermsFound = new List<string>(),
                ConceptsIdentified = new List<string>(),
                BestPracticesViolations = new List<string>(),
                Suggestions = new List<string>()
            };

            // Check for Unity-specific terms
            string[] unityTerms = 
            {
                "GameObject", "Component", "MonoBehaviour", "ScriptableObject", "Transform",
                "Unity Editor", "Prefab", "Scene", "Asset", "Inspector", "Hierarchy",
                "Project Window", "Console", "Build Settings", "Player Settings"
            };

            foreach (string term in unityTerms)
            {
                if (content.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    result.UnityTermsFound.Add(term);
                }
            }

            // Check for Unity concepts
            Dictionary<string, string> unityConcepts = new Dictionary<string, string>
            {
                { "component-based", "Component-based architecture" },
                { "entity component", "Entity Component System" },
                { "game loop", "Unity Game Loop" },
                { "lifecycle", "Unity Object Lifecycle" },
                { "serialization", "Unity Serialization" }
            };

            foreach (KeyValuePair<string, string> concept in unityConcepts)
            {
                if (content.Contains(concept.Key, StringComparison.OrdinalIgnoreCase))
                {
                    result.ConceptsIdentified.Add(concept.Value);
                }
            }

            // Check for potential best practices violations
            if (content.Contains("Update()", StringComparison.OrdinalIgnoreCase) && 
                !content.Contains("performance", StringComparison.OrdinalIgnoreCase))
            {
                result.BestPracticesViolations.Add("Update() usage mentioned without performance considerations");
            }

            if (content.Contains("singleton", StringComparison.OrdinalIgnoreCase) &&
                !content.Contains("careful", StringComparison.OrdinalIgnoreCase))
            {
                result.BestPracticesViolations.Add("Singleton pattern mentioned without cautions");
            }

            // Provide suggestions
            if (result.UnityTermsFound.Count < 3)
            {
                result.Suggestions.Add("Consider including more Unity-specific terminology");
            }

            if (!result.ConceptsIdentified.Any())
            {
                result.Suggestions.Add("Consider explaining relevant Unity concepts and architecture patterns");
            }

            result.UnityRelevanceScore = CalculateUnityRelevanceScore(result);
            result.IsUnityFocused = result.UnityRelevanceScore > 0.5f;

            return result;
        }

        #region Private Validation Methods

        private async Task ValidateGeneralQualityAsync(string content, ContentValidationResult result)
        {
            // Check minimum word count
            if (result.WordCount < _config.MinimumWordCount)
            {
                result.Issues.Add(new ValidationIssue(
                    $"Content too short: {result.WordCount} words (minimum: {_config.MinimumWordCount})",
                    ValidationSeverity.Warning));
            }

            // Check maximum word count
            if (result.WordCount > _config.MaximumWordCount)
            {
                result.Issues.Add(new ValidationIssue(
                    $"Content too long: {result.WordCount} words (maximum: {_config.MaximumWordCount})",
                    ValidationSeverity.Warning));
            }

            // Check for readability
            float readabilityScore = CalculateReadabilityScore(content);
            result.Metrics.ReadabilityScore = readabilityScore;
            
            if (readabilityScore < 0.3f)
            {
                result.Issues.Add(new ValidationIssue(
                    "Content may be difficult to read - consider simplifying language",
                    ValidationSeverity.Minor));
            }

            // Check for completeness indicators
            if (content.Contains("TODO", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("TBD", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("...", StringComparison.OrdinalIgnoreCase))
            {
                result.Issues.Add(new ValidationIssue(
                    "Content appears incomplete - contains TODO, TBD, or placeholder text",
                    ValidationSeverity.Major));
            }
        }

        private async Task ValidateSectionSpecificAsync(string content, DocumentationSectionType sectionType, ContentValidationResult result)
        {
            if (!_sectionRules.ContainsKey(sectionType))
                return;

            ValidationRules rules = _sectionRules[sectionType];

            // Check required elements
            foreach (string requiredElement in rules.RequiredElements)
            {
                if (!ContentContainsElement(content, requiredElement))
                {
                    result.Issues.Add(new ValidationIssue(
                        $"Missing required element: {requiredElement}",
                        ValidationSeverity.Major));
                }
            }

            // Check word count range for section
            if (rules.MinWordCount > 0 && result.WordCount < rules.MinWordCount)
            {
                result.Issues.Add(new ValidationIssue(
                    $"Section too short: {result.WordCount} words (minimum for {sectionType}: {rules.MinWordCount})",
                    ValidationSeverity.Warning));
            }

            if (rules.MaxWordCount > 0 && result.WordCount > rules.MaxWordCount)
            {
                result.Issues.Add(new ValidationIssue(
                    $"Section too long: {result.WordCount} words (maximum for {sectionType}: {rules.MaxWordCount})",
                    ValidationSeverity.Warning));
            }

            // Check section-specific patterns
            foreach (StructurePattern pattern in rules.StructurePatterns)
            {
                if (!ValidateStructurePattern(content, pattern))
                {
                    result.Issues.Add(new ValidationIssue(
                        $"Missing expected structure: {pattern.Description}",
                        ValidationSeverity.Minor));
                }
            }
        }

        private async Task ValidateFormatComplianceAsync(string content, ContentValidationResult result)
        {
            // Check for proper markdown formatting
            if (_config.RequireMarkdownFormatting)
            {
                if (!HasProperHeaders(content))
                {
                    result.Suggestions.Add("Consider adding section headers using markdown (# ## ###)");
                }

                if (!HasMarkdownLists(content) && result.WordCount > 100)
                {
                    result.Suggestions.Add("Consider using bullet points or numbered lists for better readability");
                }
            }

            // Check for code blocks if expected
            if (content.Contains("code", StringComparison.OrdinalIgnoreCase) &&
                !content.Contains("```") && !content.Contains("`"))
            {
                result.Suggestions.Add("Consider formatting code examples with markdown code blocks");
            }

            // Check paragraph structure
            string[] paragraphs = content.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (paragraphs.Length == 1 && result.WordCount > 200)
            {
                result.Issues.Add(new ValidationIssue(
                    "Content should be broken into multiple paragraphs for better readability",
                    ValidationSeverity.Minor));
            }
        }

        private async Task ValidateUnitySpecificContentAsync(string content, DocumentationSectionType sectionType, ContentValidationResult result)
        {
            UnityValidationResult unityValidation = await ValidateUnityContentAsync(content);
            result.Metrics.UnityRelevanceScore = unityValidation.UnityRelevanceScore;

            // For Unity-focused sections, ensure Unity relevance
            if (sectionType == DocumentationSectionType.SystemArchitecture ||
                sectionType == DocumentationSectionType.DataModel ||
                sectionType == DocumentationSectionType.APISpecification)
            {
                if (unityValidation.UnityRelevanceScore < 0.3f)
                {
                    result.Issues.Add(new ValidationIssue(
                        $"Content should be more Unity-specific for {sectionType} section",
                        ValidationSeverity.Minor));
                }

                if (unityValidation.UnityTermsFound.Count == 0)
                {
                    result.Suggestions.Add("Consider including Unity-specific terminology and concepts");
                }
            }

            // Add Unity-specific suggestions
            result.Suggestions.AddRange(unityValidation.Suggestions);
        }

        private async Task ValidateLanguageQualityAsync(string content, ContentValidationResult result)
        {
            // Check for grammar and clarity issues
            result.Metrics.GrammarScore = CalculateGrammarScore(content);
            result.Metrics.ClarityScore = CalculateClarityScore(content);

            if (result.Metrics.GrammarScore < 0.7f)
            {
                result.Issues.Add(new ValidationIssue(
                    "Content may contain grammar or language issues",
                    ValidationSeverity.Minor));
            }

            if (result.Metrics.ClarityScore < 0.6f)
            {
                result.Issues.Add(new ValidationIssue(
                    "Content clarity could be improved - consider simplifying complex sentences",
                    ValidationSeverity.Minor));
            }

            // Check for professional tone
            if (HasInformalLanguage(content))
            {
                result.Issues.Add(new ValidationIssue(
                    "Content contains informal language - consider using more professional tone",
                    ValidationSeverity.Minor));
            }
        }

        private void CalculateValidationScores(ContentValidationResult result)
        {
            // Calculate individual scores
            float structureScore = 1.0f - (result.Issues.Count(i => i.Message.Contains("structure") || i.Message.Contains("element")) * 0.2f);
            float qualityScore = (result.Metrics.ReadabilityScore + result.Metrics.GrammarScore + result.Metrics.ClarityScore) / 3f;
            float completenessScore = result.Issues.Any(i => i.Message.Contains("incomplete")) ? 0.5f : 1.0f;
            float formatScore = result.Issues.Any(i => i.Message.Contains("format")) ? 0.8f : 1.0f;

            // Weight scores based on importance
            result.OverallScore = (structureScore * 0.3f + qualityScore * 0.4f + completenessScore * 0.2f + formatScore * 0.1f);
            result.OverallScore = Math.Max(0f, Math.Min(1f, result.OverallScore));

            // Adjust score based on severity of issues
            int criticalIssues = result.Issues.Count(i => i.Severity == ValidationSeverity.Critical);
            int majorIssues = result.Issues.Count(i => i.Severity == ValidationSeverity.Major);
            
            result.OverallScore -= (criticalIssues * 0.3f + majorIssues * 0.1f);
            result.OverallScore = Math.Max(0f, result.OverallScore);
        }

        private void InitializeSectionRules()
        {
            // General Product Description rules
            _sectionRules[DocumentationSectionType.GeneralProductDescription] = new ValidationRules
            {
                RequiredElements = new List<string> { "purpose", "features", "target audience" },
                MinWordCount = 150,
                MaxWordCount = 500,
                RequiresHeaders = true,
                RequiresBulletPoints = true,
                StructurePatterns = new List<StructurePattern>
                {
                    new StructurePattern("Project overview", "Should include project name and description"),
                    new StructurePattern("Key features", "Should list main features or capabilities")
                }
            };

            // System Architecture rules
            _sectionRules[DocumentationSectionType.SystemArchitecture] = new ValidationRules
            {
                RequiredElements = new List<string> { "components", "architecture", "patterns" },
                MinWordCount = 200,
                MaxWordCount = 800,
                RequiresHeaders = true,
                RequiresBulletPoints = true,
                StructurePatterns = new List<StructurePattern>
                {
                    new StructurePattern("Architecture overview", "Should describe overall system design"),
                    new StructurePattern("Component breakdown", "Should list major components")
                }
            };

            // Data Model rules
            _sectionRules[DocumentationSectionType.DataModel] = new ValidationRules
            {
                RequiredElements = new List<string> { "data structures", "models", "relationships" },
                MinWordCount = 100,
                MaxWordCount = 600,
                RequiresHeaders = true,
                RequiresBulletPoints = false,
                StructurePatterns = new List<StructurePattern>
                {
                    new StructurePattern("Data models", "Should describe key data structures"),
                    new StructurePattern("Relationships", "Should explain how data models relate")
                }
            };

            // API Specification rules
            _sectionRules[DocumentationSectionType.APISpecification] = new ValidationRules
            {
                RequiredElements = new List<string> { "interfaces", "methods", "parameters" },
                MinWordCount = 150,
                MaxWordCount = 1000,
                RequiresHeaders = true,
                RequiresBulletPoints = true,
                StructurePatterns = new List<StructurePattern>
                {
                    new StructurePattern("API overview", "Should describe API purpose and scope"),
                    new StructurePattern("Interface definitions", "Should list key interfaces and methods")
                }
            };

            // User Stories rules
            _sectionRules[DocumentationSectionType.UserStories] = new ValidationRules
            {
                RequiredElements = new List<string> { "user", "goal", "benefit" },
                MinWordCount = 100,
                MaxWordCount = 400,
                RequiresHeaders = false,
                RequiresBulletPoints = true,
                StructurePatterns = new List<StructurePattern>
                {
                    new StructurePattern("User stories", "Should follow 'As a..., I want..., So that...' format")
                }
            };

            // Work Tickets rules
            _sectionRules[DocumentationSectionType.WorkTickets] = new ValidationRules
            {
                RequiredElements = new List<string> { "tasks", "implementation", "requirements" },
                MinWordCount = 200,
                MaxWordCount = 600,
                RequiresHeaders = true,
                RequiresBulletPoints = true,
                StructurePatterns = new List<StructurePattern>
                {
                    new StructurePattern("Task breakdown", "Should list specific implementation tasks"),
                    new StructurePattern("Acceptance criteria", "Should define completion criteria")
                }
            };
        }

        #endregion

        #region Helper Methods

        private int CountWords(string content)
        {
            if (string.IsNullOrEmpty(content))
                return 0;

            return content.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private bool ContentContainsElement(string content, string element)
        {
            return content.Contains(element, StringComparison.OrdinalIgnoreCase);
        }

        private bool ValidateStructurePattern(string content, StructurePattern pattern)
        {
            // Simple pattern matching - in a real implementation, this could be more sophisticated
            return content.Contains(pattern.Pattern, StringComparison.OrdinalIgnoreCase);
        }

        private bool HasProperHeaders(string content)
        {
            return Regex.IsMatch(content, @"^#+\s+.+", RegexOptions.Multiline);
        }

        private bool HasBulletPoints(string content)
        {
            return Regex.IsMatch(content, @"^\s*[-*+]\s+.+", RegexOptions.Multiline) ||
                   Regex.IsMatch(content, @"^\s*\d+\.\s+.+", RegexOptions.Multiline);
        }

        private bool HasMarkdownLists(string content)
        {
            return HasBulletPoints(content);
        }

        private float CalculateReadabilityScore(string content)
        {
            // Simple readability calculation based on sentence and word length
            string[] sentences = content.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            if (sentences.Length == 0) return 0f;

            float avgSentenceLength = (float)CountWords(content) / sentences.Length;
            float avgWordLength = (float)content.Length / CountWords(content);

            // Ideal ranges: 15-20 words per sentence, 4-6 characters per word
            float sentenceScore = Math.Max(0f, 1f - Math.Abs(avgSentenceLength - 17.5f) / 10f);
            float wordScore = Math.Max(0f, 1f - Math.Abs(avgWordLength - 5f) / 3f);

            return (sentenceScore + wordScore) / 2f;
        }

        private float CalculateGrammarScore(string content)
        {
            // Basic grammar checks
            float score = 1.0f;

            // Check for common grammar issues
            if (Regex.IsMatch(content, @"\b(there|their|they're)\s+(is|are)\b", RegexOptions.IgnoreCase))
                score -= 0.1f;

            if (Regex.IsMatch(content, @"\b(its|it's)\b", RegexOptions.IgnoreCase))
            {
                // Basic check for incorrect usage
                int itsCount = Regex.Matches(content, @"\bits\b", RegexOptions.IgnoreCase).Count;
                int itsApostropheCount = Regex.Matches(content, @"\bit's\b", RegexOptions.IgnoreCase).Count;
                if (itsApostropheCount > itsCount * 2) // Likely misused
                    score -= 0.1f;
            }

            return Math.Max(0f, score);
        }

        private float CalculateClarityScore(string content)
        {
            float score = 1.0f;

            // Check for passive voice (simple detection)
            int passiveCount = Regex.Matches(content, @"\b(was|were|been|being)\s+\w+ed\b", RegexOptions.IgnoreCase).Count;
            int totalSentences = content.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries).Length;
            
            if (totalSentences > 0)
            {
                float passiveRatio = (float)passiveCount / totalSentences;
                if (passiveRatio > 0.3f) // More than 30% passive voice
                    score -= 0.2f;
            }

            // Check for complex words (more than 3 syllables - simplified)
            string[] words = content.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int complexWords = words.Count(w => EstimateSyllables(w) > 3);
            float complexRatio = (float)complexWords / words.Length;
            
            if (complexRatio > 0.15f) // More than 15% complex words
                score -= 0.1f;

            return Math.Max(0f, score);
        }

        private bool HasInformalLanguage(string content)
        {
            string[] informalWords = { "gonna", "wanna", "kinda", "sorta", "yeah", "ok", "btw", "lol" };
            string lowerContent = content.ToLower();
            
            return informalWords.Any(word => lowerContent.Contains(word));
        }

        private int EstimateSyllables(string word)
        {
            // Simple syllable estimation
            word = word.ToLower().Trim();
            if (word.Length <= 3) return 1;

            int syllables = Regex.Matches(word, @"[aeiouy]+").Count;
            if (word.EndsWith("e")) syllables--;
            
            return Math.Max(1, syllables);
        }

        private float CalculateUnityRelevanceScore(UnityValidationResult result)
        {
            float score = 0f;
            
            // Weight different factors
            score += result.UnityTermsFound.Count * 0.1f;
            score += result.ConceptsIdentified.Count * 0.2f;
            score -= result.BestPracticesViolations.Count * 0.1f;

            return Math.Max(0f, Math.Min(1f, score));
        }

        #endregion
    }

    #region Supporting Data Models

    /// <summary>
    /// Configuration settings for content validation
    /// </summary>
    public class ValidationConfiguration
    {
        public int MinimumWordCount { get; set; } = 50;
        public int MaximumWordCount { get; set; } = 2000;
        public bool RequireMarkdownFormatting { get; set; } = true;
        public bool ValidateUnityContent { get; set; } = true;
        public bool StrictMode { get; set; } = false;
    }

    /// <summary>
    /// Validation rules for specific documentation sections
    /// </summary>
    public class ValidationRules
    {
        public List<string> RequiredElements { get; set; } = new List<string>();
        public int MinWordCount { get; set; }
        public int MaxWordCount { get; set; }
        public bool RequiresHeaders { get; set; }
        public bool RequiresBulletPoints { get; set; }
        public List<StructurePattern> StructurePatterns { get; set; } = new List<StructurePattern>();
    }

    /// <summary>
    /// Pattern for validating content structure
    /// </summary>
    public class StructurePattern
    {
        public string Pattern { get; set; }
        public string Description { get; set; }

        public StructurePattern(string pattern, string description)
        {
            Pattern = pattern;
            Description = description;
        }
    }

    /// <summary>
    /// Result of content validation with detailed feedback
    /// </summary>
    public class ContentValidationResult
    {
        public bool IsValid { get; set; }
        public DocumentationSectionType SectionType { get; set; }
        public int ContentLength { get; set; }
        public int WordCount { get; set; }
        public float OverallScore { get; set; }
        public DateTime ValidationTimestamp { get; set; }
        public TimeSpan ValidationDuration { get; set; }
        public List<ValidationIssue> Issues { get; set; } = new List<ValidationIssue>();
        public List<string> Suggestions { get; set; } = new List<string>();
        public ContentMetrics Metrics { get; set; } = new ContentMetrics();
    }

    /// <summary>
    /// Individual validation issue with severity level
    /// </summary>
    public class ValidationIssue
    {
        public string Message { get; set; }
        public ValidationSeverity Severity { get; set; }
        public string Context { get; set; }

        public ValidationIssue(string message, ValidationSeverity severity, string context = null)
        {
            Message = message;
            Severity = severity;
            Context = context;
        }
    }

    /// <summary>
    /// Severity levels for validation issues
    /// </summary>
    public enum ValidationSeverity
    {
        Minor,
        Warning,
        Major,
        Critical
    }

    /// <summary>
    /// Metrics about content quality and characteristics
    /// </summary>
    public class ContentMetrics
    {
        public float ReadabilityScore { get; set; }
        public float GrammarScore { get; set; }
        public float ClarityScore { get; set; }
        public float UnityRelevanceScore { get; set; }
        public float StructureScore { get; set; }
        public float CompletenessScore { get; set; }
    }

    /// <summary>
    /// Result of structure validation
    /// </summary>
    public class StructureValidationResult
    {
        public bool IsValid { get; set; }
        public DocumentationSectionType SectionType { get; set; }
        public List<string> StructureIssues { get; set; } = new List<string>();
        public List<string> MissingElements { get; set; } = new List<string>();
        public List<string> FormatIssues { get; set; } = new List<string>();
    }

    /// <summary>
    /// Result of Unity-specific content validation
    /// </summary>
    public class UnityValidationResult
    {
        public bool IsUnityFocused { get; set; }
        public float UnityRelevanceScore { get; set; }
        public List<string> UnityTermsFound { get; set; } = new List<string>();
        public List<string> ConceptsIdentified { get; set; } = new List<string>();
        public List<string> BestPracticesViolations { get; set; } = new List<string>();
        public List<string> Suggestions { get; set; } = new List<string>();
    }

    #endregion
}