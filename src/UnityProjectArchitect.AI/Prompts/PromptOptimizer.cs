using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.AI.Prompts
{
    /// <summary>
    /// Advanced prompt optimization for token efficiency and clarity
    /// Provides intelligent prompt refinement while preserving meaning and context
    /// </summary>
    public class PromptOptimizer
    {
        private const double AverageTokenToCharRatio = 0.25; // Approximate tokens per character
        private const int OptimalPromptLength = 1500; // Target prompt length for efficiency
        private const int MaxPromptLength = 3000; // Maximum allowed prompt length

        private readonly Dictionary<string, string> _commonOptimizations;
        private readonly HashSet<string> _preservedTerms;
        private readonly Dictionary<string, int> _tokenWeights;

        public PromptOptimizer()
        {
            _commonOptimizations = InitializeCommonOptimizations();
            _preservedTerms = InitializePreservedTerms();
            _tokenWeights = InitializeTokenWeights();
        }

        /// <summary>
        /// Optimizes prompt for token efficiency while preserving meaning and context
        /// </summary>
        public string OptimizePrompt(string prompt, PromptOptimizationRequest request)
        {
            if (string.IsNullOrEmpty(prompt))
                throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            string optimizedPrompt = prompt;

            // Apply optimization based on goal
            switch (request.Goal)
            {
                case OptimizationGoal.TokenReduction:
                    optimizedPrompt = OptimizeForTokenReduction(optimizedPrompt, request);
                    break;
                    
                case OptimizationGoal.Clarity:
                    optimizedPrompt = OptimizeForClarity(optimizedPrompt, request);
                    break;
                    
                case OptimizationGoal.Conciseness:
                    optimizedPrompt = OptimizeForConciseness(optimizedPrompt, request);
                    break;
                    
                case OptimizationGoal.Specificity:
                    optimizedPrompt = OptimizeForSpecificity(optimizedPrompt, request);
                    break;
                    
                case OptimizationGoal.QualityImprovement:
                    optimizedPrompt = OptimizeForQuality(optimizedPrompt, request);
                    break;
            }

            // Ensure we meet target token limit if specified
            if (request.TargetTokenLimit > 0)
            {
                optimizedPrompt = EnforceTokenLimit(optimizedPrompt, request.TargetTokenLimit);
            }

            return optimizedPrompt;
        }

        /// <summary>
        /// Estimates token count for a given prompt using character-based approximation
        /// </summary>
        public int EstimateTokenCount(string prompt)
        {
            if (string.IsNullOrEmpty(prompt))
                return 0;

            // Remove excessive whitespace and normalize
            string normalizedPrompt = Regex.Replace(prompt, @"\s+", " ").Trim();
            
            // Estimate using character count with adjustments for common patterns
            int baseTokenCount = (int)(normalizedPrompt.Length * AverageTokenToCharRatio);
            
            // Adjust for technical terms and special characters
            int technicalTermAdjustment = CountTechnicalTerms(normalizedPrompt) * 2;
            int specialCharAdjustment = CountSpecialCharacters(normalizedPrompt);
            
            return baseTokenCount + technicalTermAdjustment + specialCharAdjustment;
        }

        /// <summary>
        /// Analyzes prompt structure and provides optimization suggestions
        /// </summary>
        public PromptOptimizationAnalysis AnalyzePrompt(string prompt)
        {
            if (string.IsNullOrEmpty(prompt))
                throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));

            PromptOptimizationAnalysis analysis = new PromptOptimizationAnalysis
            {
                OriginalPrompt = prompt,
                EstimatedTokens = EstimateTokenCount(prompt),
                CharacterCount = prompt.Length,
                WordCount = CountWords(prompt),
                SentenceCount = CountSentences(prompt)
            };

            // Analyze structure and content
            analysis.HasClearInstructions = HasClearInstructions(prompt);
            analysis.HasSpecificRequirements = HasSpecificRequirements(prompt);
            analysis.HasContextPlaceholders = HasContextPlaceholders(prompt);
            analysis.RedundancyScore = CalculateRedundancyScore(prompt);
            analysis.ClarityScore = CalculateClarityScore(prompt);
            analysis.TechnicalTermDensity = CalculateTechnicalTermDensity(prompt);

            // Generate optimization suggestions
            analysis.OptimizationSuggestions = GenerateOptimizationSuggestions(analysis);

            return analysis;
        }

        /// <summary>
        /// Validates prompt against token limits and quality criteria
        /// </summary>
        public PromptValidationResult ValidatePrompt(string prompt, int maxTokens = 4000)
        {
            if (string.IsNullOrEmpty(prompt))
                throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));

            PromptValidationResult result = new PromptValidationResult
            {
                IsValid = true,
                EstimatedTokens = EstimateTokenCount(prompt),
                Issues = new List<string>(),
                Suggestions = new List<string>()
            };

            // Token limit validation
            if (result.EstimatedTokens > maxTokens)
            {
                result.IsValid = false;
                result.Issues.Add($"Prompt exceeds token limit: {result.EstimatedTokens} > {maxTokens}");
                result.Suggestions.Add("Consider using more concise language or removing less critical sections");
            }

            // Structure validation
            if (!HasClearInstructions(prompt))
            {
                result.Suggestions.Add("Consider adding clearer instruction verbs (generate, create, analyze, etc.)");
            }

            if (!HasContextPlaceholders(prompt))
            {
                result.Suggestions.Add("Consider adding context placeholders for dynamic content");
            }

            // Quality validation
            float redundancyScore = CalculateRedundancyScore(prompt);
            if (redundancyScore > 0.3f)
            {
                result.Suggestions.Add("Prompt contains repetitive content that could be condensed");
            }

            return result;
        }

        private string OptimizeForTokenReduction(string prompt, PromptOptimizationRequest request)
        {
            StringBuilder optimized = new StringBuilder(prompt);

            // Apply common token reduction optimizations
            foreach (KeyValuePair<string, string> optimization in _commonOptimizations)
            {
                optimized.Replace(optimization.Key, optimization.Value);
            }

            // Remove excessive formatting and redundant words
            string result = optimized.ToString();
            result = RemoveRedundantWords(result);
            result = SimplifyFormatting(result);
            result = ConsolidateInstructions(result);

            return result;
        }

        private string OptimizeForClarity(string prompt, PromptOptimizationRequest request)
        {
            StringBuilder optimized = new StringBuilder(prompt);

            // Improve sentence structure and word choice
            string result = optimized.ToString();
            result = ImproveInstructionClarity(result);
            result = AddTransitionalPhrases(result);
            result = StandardizeTerminology(result);

            return result;
        }

        private string OptimizeForConciseness(string prompt, PromptOptimizationRequest request)
        {
            string result = prompt;
            
            // Remove unnecessary adjectives and adverbs
            result = RemoveUnnecessaryModifiers(result);
            
            // Convert complex sentences to simpler forms
            result = SimplifySentenceStructure(result);
            
            // Remove redundant explanations
            result = RemoveRedundantExplanations(result);

            return result;
        }

        private string OptimizeForSpecificity(string prompt, PromptOptimizationRequest request)
        {
            StringBuilder optimized = new StringBuilder(prompt);

            // Add specific technical terms and requirements
            string result = optimized.ToString();
            result = AddTechnicalSpecificity(result, request.Keywords);
            result = EnhanceRequirements(result);
            result = AddContextualDetails(result, request.Context);

            return result;
        }

        private string OptimizeForQuality(string prompt, PromptOptimizationRequest request)
        {
            string result = prompt;

            // Apply multiple optimization strategies
            result = OptimizeForClarity(result, request);
            result = OptimizeForSpecificity(result, request);
            result = OptimizeForTokenReduction(result, request);

            // Final quality improvements
            result = EnsureLogicalFlow(result);
            result = BalanceDetailAndConciseness(result);

            return result;
        }

        private string EnforceTokenLimit(string prompt, int targetTokenLimit)
        {
            int currentTokens = EstimateTokenCount(prompt);
            if (currentTokens <= targetTokenLimit)
                return prompt;

            // Calculate reduction ratio needed
            double reductionRatio = (double)targetTokenLimit / currentTokens;
            
            // Intelligently trim content while preserving structure
            return IntelligentTrim(prompt, reductionRatio);
        }

        private string IntelligentTrim(string prompt, double reductionRatio)
        {
            string[] sections = prompt.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder trimmed = new StringBuilder();
            
            foreach (string section in sections)
            {
                string trimmedSection = TrimSection(section, reductionRatio);
                if (!string.IsNullOrEmpty(trimmedSection))
                {
                    if (trimmed.Length > 0)
                        trimmed.AppendLine();
                    trimmed.AppendLine(trimmedSection);
                }
            }

            return trimmed.ToString();
        }

        private string TrimSection(string section, double reductionRatio)
        {
            // Preserve headers and key instructions
            if (section.StartsWith("**") || section.Contains("Requirements:") || section.Contains("Format:"))
            {
                return section; // Keep structural elements
            }

            // Trim examples and explanations more aggressively
            if (section.Contains("example") || section.Contains("Example"))
            {
                return TruncateToRatio(section, reductionRatio * 0.7);
            }

            return TruncateToRatio(section, reductionRatio);
        }

        private string TruncateToRatio(string text, double ratio)
        {
            int targetLength = (int)(text.Length * ratio);
            if (targetLength >= text.Length)
                return text;

            // Find a good breaking point near the target
            int breakPoint = Math.Min(targetLength, text.Length);
            
            // Try to break at sentence or word boundaries
            int sentenceBreak = text.LastIndexOf('.', breakPoint);
            int wordBreak = text.LastIndexOf(' ', breakPoint);

            if (sentenceBreak > targetLength * 0.8)
                return text.Substring(0, sentenceBreak + 1);
            else if (wordBreak > targetLength * 0.8)
                return text.Substring(0, wordBreak) + "...";
            else
                return text.Substring(0, breakPoint) + "...";
        }

        private Dictionary<string, string> InitializeCommonOptimizations()
        {
            return new Dictionary<string, string>
            {
                { "in order to", "to" },
                { "it is important to", "" },
                { "please note that", "" },
                { "it should be noted that", "" },
                { "make sure to", "" },
                { "be sure to", "" },
                { "you should", "" },
                { "it is recommended to", "" },
                { "for the purpose of", "for" },
                { "in the event that", "if" },
                { "at this point in time", "now" },
                { "due to the fact that", "because" },
                { "in a manner that", "to" },
                { "comprehensive and detailed", "comprehensive" },
                { "clear and concise", "clear" },
                { "accurate and precise", "accurate" }
            };
        }

        private HashSet<string> InitializePreservedTerms()
        {
            return new HashSet<string>
            {
                "Unity", "UnityEngine", "ScriptableObject", "MonoBehaviour",
                "GameObject", "Transform", "Component", "Prefab",
                "API", "interface", "namespace", "class", "method",
                "architecture", "pattern", "model", "data", "analysis"
            };
        }

        private Dictionary<string, int> InitializeTokenWeights()
        {
            return new Dictionary<string, int>
            {
                { "technical", 2 },
                { "implementation", 2 },
                { "architecture", 2 },
                { "specification", 2 },
                { "requirements", 2 },
                { "analysis", 2 },
                { "Unity", 2 },
                { "ScriptableObject", 3 },
                { "MonoBehaviour", 3 }
            };
        }

        // Helper methods for various optimization strategies
        private string RemoveRedundantWords(string text)
        {
            // Remove common redundant word patterns
            string result = Regex.Replace(text, @"\b(very|really|quite|rather|somewhat)\s+", "");
            result = Regex.Replace(result, @"\b(in order to|so as to)\b", "to");
            result = Regex.Replace(result, @"\bthat\s+", "", RegexOptions.IgnoreCase);
            
            return result;
        }

        private string SimplifyFormatting(string text)
        {
            // Consolidate excessive line breaks and whitespace
            string result = Regex.Replace(text, @"\n{3,}", "\n\n");
            result = Regex.Replace(result, @"\s{2,}", " ");
            
            return result.Trim();
        }

        private string ConsolidateInstructions(string text)
        {
            // Merge similar instruction patterns
            StringBuilder consolidated = new StringBuilder();
            string[] lines = text.Split('\n');
            
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                {
                    consolidated.AppendLine();
                    continue;
                }
                
                // Check for consolidation opportunities with next lines
                bool consolidated_line = false;
                if (i < lines.Length - 1 && IsConsolidatable(line, lines[i + 1]))
                {
                    consolidated.AppendLine(ConsolidateLines(line, lines[i + 1]));
                    i++; // Skip next line as it's been consolidated
                    consolidated_line = true;
                }
                
                if (!consolidated_line)
                {
                    consolidated.AppendLine(line);
                }
            }
            
            return consolidated.ToString();
        }

        private bool IsConsolidatable(string line1, string line2)
        {
            // Check if two lines can be consolidated based on similar instruction patterns
            string[] instructionWords = { "include", "provide", "ensure", "describe", "explain", "create" };
            
            foreach (string word in instructionWords)
            {
                if (line1.ToLower().Contains(word) && line2.ToLower().Contains(word))
                {
                    return true;
                }
            }
            
            return false;
        }

        private string ConsolidateLines(string line1, string line2)
        {
            // Simple consolidation - combine with "and"
            return $"{line1.TrimEnd('.')} and {line2.ToLower()}";
        }

        private string ImproveInstructionClarity(string text)
        {
            // Enhance clarity by strengthening instruction verbs
            Dictionary<string, string> clarityImprovements = new Dictionary<string, string>
            {
                { "make", "create" },
                { "do", "generate" },
                { "write", "generate" },
                { "put", "include" },
                { "show", "display" },
                { "tell", "explain" }
            };

            string result = text;
            foreach (KeyValuePair<string, string> improvement in clarityImprovements)
            {
                result = Regex.Replace(result, $@"\b{improvement.Key}\b", improvement.Value, RegexOptions.IgnoreCase);
            }

            return result;
        }

        private string AddTransitionalPhrases(string text)
        {
            // Add transitional phrases to improve flow (sparingly to avoid token bloat)
            string result = text;
            
            // Only add where it significantly improves clarity
            result = Regex.Replace(result, @"(\*\*Requirements:\*\*)", "$1\nThe following requirements must be met:");
            result = Regex.Replace(result, @"(\*\*Format:\*\*)", "$1\nStructure the output as follows:");
            
            return result;
        }

        private string StandardizeTerminology(string text)
        {
            // Ensure consistent use of technical terminology
            Dictionary<string, string> standardizations = new Dictionary<string, string>
            {
                { "script", "Script" },
                { "unity", "Unity" },
                { "gameobject", "GameObject" },
                { "monobehaviour", "MonoBehaviour" },
                { "scriptableobject", "ScriptableObject" }
            };

            string result = text;
            foreach (KeyValuePair<string, string> standard in standardizations)
            {
                result = Regex.Replace(result, $@"\b{standard.Key}\b", standard.Value, RegexOptions.IgnoreCase);
            }

            return result;
        }

        private string RemoveUnnecessaryModifiers(string text)
        {
            // Remove adjectives and adverbs that don't add specific value
            string[] unnecessaryModifiers = { "very", "really", "quite", "rather", "pretty", "somewhat", "fairly" };
            
            string result = text;
            foreach (string modifier in unnecessaryModifiers)
            {
                result = Regex.Replace(result, $@"\b{modifier}\s+", "", RegexOptions.IgnoreCase);
            }
            
            return result;
        }

        private string SimplifySentenceStructure(string text)
        {
            // Convert complex sentences to simpler forms
            string result = text;
            
            // Convert "which" clauses to separate sentences where appropriate
            result = Regex.Replace(result, @",\s*which\s+", ". This ");
            
            // Simplify "that" clauses
            result = Regex.Replace(result, @"\s+that\s+", " ");
            
            return result;
        }

        private string RemoveRedundantExplanations(string text)
        {
            // Remove explanatory phrases that don't add value
            string[] redundantPhrases = 
            {
                "as mentioned before", "as stated earlier", "it is worth noting",
                "it should be emphasized", "importantly", "significantly"
            };

            string result = text;
            foreach (string phrase in redundantPhrases)
            {
                result = Regex.Replace(result, $@"\b{phrase}\b", "", RegexOptions.IgnoreCase);
            }
            
            return result;
        }

        private string AddTechnicalSpecificity(string text, List<string> keywords)
        {
            if (keywords == null || keywords.Count == 0)
                return text;

            // Enhance technical specificity by incorporating relevant keywords
            StringBuilder enhanced = new StringBuilder(text);
            
            // Add keywords to relevant sections
            foreach (string keyword in keywords.Take(5)) // Limit to avoid bloat
            {
                if (!text.ToLower().Contains(keyword.ToLower()))
                {
                    // Add keyword in context-appropriate location
                    string contextPhrase = $"including {keyword}";
                    enhanced.Replace("technical aspects", $"technical aspects {contextPhrase}");
                }
            }
            
            return enhanced.ToString();
        }

        private string EnhanceRequirements(string text)
        {
            // Make requirements more specific and actionable
            string result = text;
            
            // Strengthen requirement language
            result = Regex.Replace(result, @"should\s+", "must ");
            result = Regex.Replace(result, @"might\s+", "should ");
            result = Regex.Replace(result, @"could\s+", "should ");
            
            return result;
        }

        private string AddContextualDetails(string text, string context)
        {
            if (string.IsNullOrEmpty(context))
                return text;

            // Add contextual details where appropriate
            StringBuilder enhanced = new StringBuilder(text);
            
            // Insert context in appropriate locations
            if (text.Contains("project") && !text.Contains(context))
            {
                enhanced.Replace("project", $"project ({context})");
            }
            
            return enhanced.ToString();
        }

        private string EnsureLogicalFlow(string text)
        {
            // Ensure sections flow logically
            string[] sections = text.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder reordered = new StringBuilder();
            
            // Prioritize key sections
            List<string> prioritizedSections = sections
                .OrderBy(s => GetSectionPriority(s))
                .ToList();
            
            foreach (string section in prioritizedSections)
            {
                if (reordered.Length > 0)
                    reordered.AppendLine();
                reordered.AppendLine(section);
            }
            
            return reordered.ToString();
        }

        private int GetSectionPriority(string section)
        {
            if (section.Contains("Requirements:")) return 1;
            if (section.Contains("Format:")) return 2;
            if (section.Contains("Context:")) return 3;
            if (section.Contains("Output")) return 4;
            return 5;
        }

        private string BalanceDetailAndConciseness(string text)
        {
            // Ensure appropriate level of detail without unnecessary verbosity
            string[] sentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder balanced = new StringBuilder();
            
            foreach (string sentence in sentences)
            {
                string trimmedSentence = sentence.Trim();
                if (!string.IsNullOrEmpty(trimmedSentence) && IsValuableSentence(trimmedSentence))
                {
                    balanced.Append(trimmedSentence);
                    if (!trimmedSentence.EndsWith(".") && !trimmedSentence.EndsWith("!") && !trimmedSentence.EndsWith("?"))
                        balanced.Append(".");
                    balanced.Append(" ");
                }
            }
            
            return balanced.ToString().Trim();
        }

        private bool IsValuableSentence(string sentence)
        {
            // Determine if sentence adds value
            if (sentence.Length < 10) return false;
            
            string[] fillerPhrases = { "it is important", "please note", "it should be", "make sure" };
            foreach (string filler in fillerPhrases)
            {
                if (sentence.ToLower().Contains(filler))
                    return false;
            }
            
            return true;
        }

        // Analysis helper methods
        private bool HasClearInstructions(string prompt)
        {
            string[] instructionVerbs = { "generate", "create", "analyze", "describe", "explain", "provide", "include", "document" };
            string lowerPrompt = prompt.ToLower();
            return instructionVerbs.Any(verb => lowerPrompt.Contains(verb));
        }

        private bool HasSpecificRequirements(string prompt)
        {
            return prompt.Contains("Requirements:") || prompt.Contains("Format:") || 
                   prompt.Contains("must") || prompt.Contains("should");
        }

        private bool HasContextPlaceholders(string prompt)
        {
            return prompt.Contains("{") && prompt.Contains("}");
        }

        private float CalculateRedundancyScore(string prompt)
        {
            string[] words = prompt.ToLower().Split(new[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, int> wordCounts = words.GroupBy(w => w).ToDictionary(g => g.Key, g => g.Count());
            
            int duplicates = wordCounts.Values.Where(count => count > 1).Sum(count => count - 1);
            return duplicates / (float)words.Length;
        }

        private float CalculateClarityScore(string prompt)
        {
            float score = 0f;
            
            // Check for clear structure
            if (prompt.Contains("**")) score += 0.2f;
            if (HasClearInstructions(prompt)) score += 0.3f;
            if (HasSpecificRequirements(prompt)) score += 0.3f;
            if (prompt.Contains("Format:")) score += 0.2f;
            
            return Math.Min(score, 1f);
        }

        private float CalculateTechnicalTermDensity(string prompt)
        {
            string[] technicalTerms = { "Unity", "ScriptableObject", "API", "interface", "architecture", "pattern", "model", "analysis" };
            string lowerPrompt = prompt.ToLower();
            
            int technicalTermCount = technicalTerms.Count(term => lowerPrompt.Contains(term.ToLower()));
            int totalWords = CountWords(prompt);
            
            return totalWords > 0 ? technicalTermCount / (float)totalWords : 0f;
        }

        private int CountWords(string text)
        {
            return text.Split(new[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private int CountSentences(string text)
        {
            return text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private int CountTechnicalTerms(string prompt)
        {
            string[] technicalTerms = { "Unity", "ScriptableObject", "MonoBehaviour", "GameObject", "API", "interface", "namespace", "class" };
            string lowerPrompt = prompt.ToLower();
            return technicalTerms.Count(term => lowerPrompt.Contains(term.ToLower()));
        }

        private int CountSpecialCharacters(string prompt)
        {
            return prompt.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
        }

        private List<string> GenerateOptimizationSuggestions(PromptOptimizationAnalysis analysis)
        {
            List<string> suggestions = new List<string>();
            
            if (analysis.EstimatedTokens > OptimalPromptLength)
                suggestions.Add("Consider reducing prompt length for better token efficiency");
                
            if (analysis.RedundancyScore > 0.2f)
                suggestions.Add("Remove redundant words and phrases to improve conciseness");
                
            if (!analysis.HasClearInstructions)
                suggestions.Add("Add clear instruction verbs (generate, create, analyze) for better AI understanding");
                
            if (!analysis.HasContextPlaceholders)
                suggestions.Add("Add context placeholders for dynamic content insertion");
                
            if (analysis.TechnicalTermDensity < 0.05f)
                suggestions.Add("Consider adding more specific technical terminology");
                
            return suggestions;
        }
    }

    /// <summary>
    /// Comprehensive analysis of prompt structure and optimization opportunities
    /// </summary>
    [Serializable]
    public class PromptOptimizationAnalysis
    {
        public string OriginalPrompt { get; set; }
        public int EstimatedTokens { get; set; }
        public int CharacterCount { get; set; }
        public int WordCount { get; set; }
        public int SentenceCount { get; set; }
        public bool HasClearInstructions { get; set; }
        public bool HasSpecificRequirements { get; set; }
        public bool HasContextPlaceholders { get; set; }
        public float RedundancyScore { get; set; }
        public float ClarityScore { get; set; }
        public float TechnicalTermDensity { get; set; }
        public List<string> OptimizationSuggestions { get; set; }

        public PromptOptimizationAnalysis()
        {
            OptimizationSuggestions = new List<string>();
        }

        public bool IsOptimal => EstimatedTokens <= 1500 && ClarityScore >= 0.7f && RedundancyScore <= 0.2f;
        public bool NeedsOptimization => EstimatedTokens > 2000 || ClarityScore < 0.5f || RedundancyScore > 0.3f;
    }

    /// <summary>
    /// Validation result for prompt quality and compliance
    /// </summary>
    [Serializable]
    public class PromptValidationResult
    {
        public bool IsValid { get; set; }
        public int EstimatedTokens { get; set; }
        public List<string> Issues { get; set; }
        public List<string> Suggestions { get; set; }

        public PromptValidationResult()
        {
            Issues = new List<string>();
            Suggestions = new List<string>();
        }

        public bool HasIssues => Issues.Count > 0;
        public bool HasSuggestions => Suggestions.Count > 0;
    }
}