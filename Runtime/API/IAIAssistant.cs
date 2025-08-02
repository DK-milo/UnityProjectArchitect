using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.API
{
    public interface IAIAssistant
    {
        event Action<AIOperationResult> OnOperationComplete;
        event Action<string, float> OnProgress;
        event Action<string> OnError;

        Task<AIOperationResult> GenerateContentAsync(AIRequest request);
        Task<AIOperationResult> EnhanceContentAsync(string content, AIEnhancementRequest enhancementRequest);
        Task<AIOperationResult> AnalyzeProjectAsync(ProjectData projectData);
        Task<AIOperationResult> GenerateSuggestionsAsync(ProjectData projectData, SuggestionType suggestionType);
        
        Task<ValidationResult> ValidateAPIKeyAsync(string apiKey, AIProvider provider);
        Task<bool> TestConnectionAsync(AIConfiguration configuration);
        
        List<AIProvider> GetSupportedProviders();
        AIConfiguration GetDefaultConfiguration(AIProvider provider);
        AICapabilities GetCapabilities(AIProvider provider);
        
        bool IsConfigured { get; }
        AIProvider CurrentProvider { get; }
    }

    public interface IAIPromptManager
    {
        Task<string> GetPromptAsync(DocumentationSectionType sectionType);
        Task<string> GetCustomPromptAsync(string promptName);
        
        void SetPrompt(DocumentationSectionType sectionType, string prompt);
        void SetCustomPrompt(string promptName, string prompt);
        
        List<string> GetAvailablePrompts();
        string GetDefaultPrompt(DocumentationSectionType sectionType);
        
        Task<AIPromptValidationResult> ValidatePromptAsync(string prompt);
        Task<string> OptimizePromptAsync(string prompt, PromptOptimizationRequest request);
    }

    [Serializable]
    public class AIRequest
    {
        public string Prompt { get; set; }
        public ProjectData ProjectContext { get; set; }
        public DocumentationSectionType? SectionType { get; set; }
        public AIConfiguration Configuration { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public List<string> RequiredElements { get; set; }
        public int TargetWordCount { get; set; }
        public string Style { get; set; }
        public AIRequestType RequestType { get; set; }

        public AIRequest()
        {
            Parameters = new Dictionary<string, object>();
            RequiredElements = new List<string>();
            TargetWordCount = 500;
            Style = "Professional";
            RequestType = AIRequestType.Generation;
        }

        public AIRequest(string prompt, ProjectData context) : this()
        {
            Prompt = prompt;
            ProjectContext = context;
        }
    }

    [Serializable]
    public class AIEnhancementRequest
    {
        public string OriginalContent { get; set; }
        public EnhancementType EnhancementType { get; set; }
        public string Instructions { get; set; }
        public int TargetWordCount { get; set; }
        public string Style { get; set; }
        public List<string> FocusAreas { get; set; }

        public AIEnhancementRequest()
        {
            FocusAreas = new List<string>();
            Style = "Professional";
        }

        public AIEnhancementRequest(string content, EnhancementType type) : this()
        {
            OriginalContent = content;
            EnhancementType = type;
        }
    }

    public enum EnhancementType
    {
        Improve,
        Expand,
        Summarize,
        Restructure,
        ProofRead,
        AddExamples,
        AddDiagrams,
        Translate
    }

    public enum SuggestionType
    {
        ProjectStructure,
        BestPractices,
        Architecture,
        Performance,
        Security,
        Testing,
        Documentation,
        Templates
    }

    public enum AIRequestType
    {
        Generation,
        Enhancement,
        Analysis,
        Suggestion,
        Validation,
        Translation
    }

    [Serializable]
    public class AIOperationResult
    {
        public bool Success { get; set; }
        public string Content { get; set; }
        public string ErrorMessage { get; set; }
        public AIProvider Provider { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public DateTime ProcessedAt { get; set; }
        public int TokensUsed { get; set; }
        public float ConfidenceScore { get; set; }
        public AIUsageStatistics Usage { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public AIOperationResult()
        {
            ProcessedAt = DateTime.Now;
            Metadata = new Dictionary<string, object>();
            Usage = new AIUsageStatistics();
        }

        public AIOperationResult(bool success, string content) : this()
        {
            Success = success;
            Content = content;
        }

        public int WordCount => string.IsNullOrEmpty(Content) ? 0 : 
            Content.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    [Serializable]
    public class AIPromptValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Issues { get; set; }
        public List<string> Suggestions { get; set; }
        public int EstimatedTokens { get; set; }
        public PromptQualityScore QualityScore { get; set; }

        public AIPromptValidationResult()
        {
            Issues = new List<string>();
            Suggestions = new List<string>();
            QualityScore = new PromptQualityScore();
        }
    }

    [Serializable]
    public class PromptQualityScore
    {
        public float Clarity { get; set; }
        public float Specificity { get; set; }
        public float Completeness { get; set; }
        public float OverallScore { get; set; }

        public PromptQualityScore()
        {
            OverallScore = (Clarity + Specificity + Completeness) / 3f;
        }
    }

    [Serializable]
    public class PromptOptimizationRequest
    {
        public string OriginalPrompt { get; set; }
        public OptimizationGoal Goal { get; set; }
        public int TargetTokenLimit { get; set; }
        public string Context { get; set; }
        public List<string> Keywords { get; set; }

        public PromptOptimizationRequest()
        {
            Keywords = new List<string>();
            Goal = OptimizationGoal.Clarity;
        }
    }

    public enum OptimizationGoal
    {
        Clarity,
        Conciseness,
        Specificity,
        TokenReduction,
        QualityImprovement
    }

    [Serializable]
    public class AIUsageStatistics
    {
        public int RequestCount { get; set; }
        public int TotalTokensUsed { get; set; }
        public TimeSpan TotalProcessingTime { get; set; }
        public float AverageConfidenceScore { get; set; }
        public Dictionary<AIProvider, int> ProviderUsage { get; set; }
        public Dictionary<DocumentationSectionType, int> SectionUsage { get; set; }

        public AIUsageStatistics()
        {
            ProviderUsage = new Dictionary<AIProvider, int>();
            SectionUsage = new Dictionary<DocumentationSectionType, int>();
        }

        public float AverageTokensPerRequest => RequestCount > 0 ? (float)TotalTokensUsed / RequestCount : 0f;
        public TimeSpan AverageProcessingTime => RequestCount > 0 ? 
            new TimeSpan(TotalProcessingTime.Ticks / RequestCount) : TimeSpan.Zero;
    }

    [Serializable]
    public class AICapabilities
    {
        public AIProvider Provider { get; set; }
        public List<string> SupportedModels { get; set; }
        public int MaxTokens { get; set; }
        public bool SupportsStreaming { get; set; }
        public bool SupportsImages { get; set; }
        public bool SupportsCodeGeneration { get; set; }
        public bool SupportsMultipleLanguages { get; set; }
        public List<EnhancementType> SupportedEnhancements { get; set; }
        public Dictionary<string, bool> Features { get; set; }

        public AICapabilities()
        {
            SupportedModels = new List<string>();
            SupportedEnhancements = new List<EnhancementType>();
            Features = new Dictionary<string, bool>();
        }

        public bool HasFeature(string featureName)
        {
            return Features.ContainsKey(featureName) && Features[featureName];
        }

        public bool SupportsEnhancement(EnhancementType enhancementType)
        {
            return SupportedEnhancements.Contains(enhancementType);
        }
    }

    public static class AIAssistantExtensions
    {
        public static async Task<AIOperationResult> GenerateDocumentationSection(
            this IAIAssistant aiAssistant,
            ProjectData projectData,
            DocumentationSectionData sectionData)
        {
            AIRequest request = new AIRequest(sectionData.CustomPrompt, projectData)
            {
                SectionType = sectionData.SectionType,
                TargetWordCount = sectionData.WordCountTarget,
                RequiredElements = sectionData.RequiredElements.ToList()
            };

            return await aiAssistant.GenerateContentAsync(request);
        }

        public static async Task<bool> IsProviderAvailable(
            this IAIAssistant aiAssistant,
            AIProvider provider)
        {
            List<AIProvider> supportedProviders = aiAssistant.GetSupportedProviders();
            if (!supportedProviders.Contains(provider))
                return false;

            AIConfiguration defaultConfig = aiAssistant.GetDefaultConfiguration(provider);
            return await aiAssistant.TestConnectionAsync(defaultConfig);
        }
        
        public static string GetFormattedReport(this AIOperationResult result)
        {
            string report = $"🤖 AI Operation Report\n";
            report += $"✅ Success: {result.Success}\n";
            report += $"🔮 Provider: {result.Provider}\n";
            report += $"⏱️ Time: {result.ProcessingTime.TotalSeconds:F1}s\n";
            report += $"🎯 Tokens Used: {result.TokensUsed:N0}\n";
            report += $"📊 Confidence: {result.ConfidenceScore:P1}\n";
            report += $"📝 Word Count: {result.WordCount:N0}\n";

            if (!result.Success)
            {
                report += $"❌ Error: {result.ErrorMessage}\n";
            }

            return report;
        }

        public static async Task<List<AIOperationResult>> GenerateAllSections(
            this IAIAssistant aiAssistant,
            ProjectData projectData)
        {
            List<AIOperationResult> results = new List<AIOperationResult>();
            
            foreach (string section in projectData.DocumentationSections)
            {
                if (section.IsEnabled && section.AIMode != AIGenerationMode.Disabled)
                {
                    AIOperationResult result = await aiAssistant.GenerateDocumentationSection(projectData, section);
                    results.Add(result);
                    
                    if (result.Success && section.AIMode == AIGenerationMode.FullGeneration)
                    {
                        section.Content = result.Content;
                        section.MarkAsGenerated();
                    }
                }
            }
            
            return results;
        }
    }
}