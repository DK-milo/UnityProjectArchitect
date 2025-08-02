using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.API
{
    public interface IValidationService
    {
        event Action<ValidationOperationResult> OnValidationComplete;
        event Action<string, float> OnValidationProgress;
        event Action<string> OnError;

        Task<ValidationResult> ValidateProjectAsync(ProjectData projectData);
        Task<ValidationResult> ValidateDocumentationAsync(List<DocumentationSectionData> sections);
        Task<ValidationResult> ValidateTemplateAsync(ProjectTemplate template);
        Task<ValidationResult> ValidateAIConfigurationAsync(AIConfiguration configuration);
        Task<ValidationResult> ValidateExportRequestAsync(ExportRequest exportRequest);
        
        Task<ValidationResult> ValidateProjectStructureAsync(string projectPath);
        Task<ValidationResult> ValidateScriptsAsync(string scriptsPath);
        Task<ValidationResult> ValidateAssetsAsync(string assetsPath);
        
        ValidationRule[] GetAvailableRules();
        ValidationRule[] GetActiveRules();
        void SetActiveRules(ValidationRule[] rules);
        
        ValidationServiceCapabilities GetCapabilities();
        bool CanValidate(ValidationType validationType);
    }

    public interface IValidationRule
    {
        string RuleId { get; }
        string RuleName { get; }
        string Description { get; }
        ValidationType ValidationType { get; }
        ValidationSeverity DefaultSeverity { get; }
        bool IsEnabled { get; set; }
        
        Task<ValidationResult> ValidateAsync(object target, ValidationContext context);
        bool CanValidate(object target);
        RuleConfiguration GetConfiguration();
        void SetConfiguration(RuleConfiguration configuration);
    }

    public interface IValidationRuleProvider
    {
        List<IValidationRule> GetRules();
        List<IValidationRule> GetRulesForType(ValidationType validationType);
        IValidationRule GetRule(string ruleId);
        
        void RegisterRule(IValidationRule rule);
        void UnregisterRule(string ruleId);
        
        bool HasRule(string ruleId);
        int RuleCount { get; }
    }

    [Serializable]
    public class ValidationOperationResult
    {
        public bool Success { get; set; }
        public List<ValidationResult> Results { get; set; }
        public ValidationSummary OverallSummary { get; set; }
        public TimeSpan ValidationTime { get; set; }
        public DateTime ValidatedAt { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<ValidationType, ValidationResult> ResultsByType { get; set; }

        public ValidationOperationResult()
        {
            Results = new List<ValidationResult>();
            ValidatedAt = DateTime.Now;
            ResultsByType = new Dictionary<ValidationType, ValidationResult>();
        }

        public ValidationResult GetResult(ValidationType validationType)
        {
            return ResultsByType.ContainsKey(validationType) ? ResultsByType[validationType] : null;
        }

        public bool HasResultForType(ValidationType validationType)
        {
            return ResultsByType.ContainsKey(validationType);
        }

        public int TotalIssues => Results.Sum(r => r.IssueCount);
        public int TotalBlockers => Results.Sum(r => r.BlockerCount);
        public int TotalWarnings => Results.Sum(r => r.WarningCount);
        public float AverageScore => Results.Count > 0 ? Results.Average(r => r.ValidationScore) : 0f;
    }

    [Serializable]
    public class ValidationContext
    {
        public string ProjectPath { get; set; }
        public ProjectData ProjectData { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public List<string> ExcludedPaths { get; set; }
        public ValidationSettings Settings { get; set; }
        public ValidationScope Scope { get; set; }

        public ValidationContext()
        {
            Parameters = new Dictionary<string, object>();
            ExcludedPaths = new List<string>();
            Settings = new ValidationSettings();
            Scope = ValidationScope.Full;
        }

        public ValidationContext(ProjectData projectData) : this()
        {
            ProjectData = projectData;
        }

        public T GetParameter<T>(string key, T defaultValue = default)
        {
            return Parameters.ContainsKey(key) && Parameters[key] is T value ? value : defaultValue;
        }

        public void SetParameter(string key, object value)
        {
            Parameters[key] = value;
        }
    }

    [Serializable]
    public class ValidationSettings
    {
        public ValidationSeverity MinimumSeverity { get; set; }
        public bool StopOnFirstError { get; set; }
        public bool ValidateInParallel { get; set; }
        public int MaxConcurrentValidations { get; set; }
        public TimeSpan Timeout { get; set; }
        public bool IncludePerformanceMetrics { get; set; }
        public List<string> CustomRulePaths { get; set; }
        public Dictionary<string, object> RuleSettings { get; set; }

        public ValidationSettings()
        {
            MinimumSeverity = ValidationSeverity.Info;
            StopOnFirstError = false;
            ValidateInParallel = true;
            MaxConcurrentValidations = 4;
            Timeout = TimeSpan.FromMinutes(5);
            IncludePerformanceMetrics = false;
            CustomRulePaths = new List<string>();
            RuleSettings = new Dictionary<string, object>();
        }
    }

    public enum ValidationScope
    {
        Quick,
        Standard,
        Full,
        Custom
    }

    [Serializable]
    public class ValidationRule
    {
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string Description { get; set; }
        public ValidationType ValidationType { get; set; }
        public ValidationSeverity DefaultSeverity { get; set; }
        public bool IsEnabled { get; set; }
        public RuleConfiguration Configuration { get; set; }
        public List<string> Tags { get; set; }
        public string Category { get; set; }

        public ValidationRule()
        {
            IsEnabled = true;
            Configuration = new RuleConfiguration();
            Tags = new List<string>();
        }

        public ValidationRule(string id, string name, ValidationType type) : this()
        {
            RuleId = id;
            RuleName = name;
            ValidationType = type;
        }
    }

    [Serializable]
    public class RuleConfiguration
    {
        public ValidationSeverity Severity { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public List<string> ExcludePatterns { get; set; }
        public List<string> IncludePatterns { get; set; }
        public bool IsCustom { get; set; }

        public RuleConfiguration()
        {
            Parameters = new Dictionary<string, object>();
            ExcludePatterns = new List<string>();
            IncludePatterns = new List<string>();
        }

        public T GetParameter<T>(string key, T defaultValue = default)
        {
            return Parameters.ContainsKey(key) && Parameters[key] is T value ? value : defaultValue;
        }

        public void SetParameter(string key, object value)
        {
            Parameters[key] = value;
        }
    }

    [Serializable]
    public class ValidationServiceCapabilities
    {
        public List<ValidationType> SupportedValidationTypes { get; set; }
        public List<ValidationScope> SupportedScopes { get; set; }
        public bool SupportsCustomRules { get; set; }
        public bool SupportsParallelValidation { get; set; }
        public bool SupportsProgressTracking { get; set; }
        public bool SupportsRuleConfiguration { get; set; }
        public bool SupportsValidationProfiles { get; set; }
        public int MaxConcurrentValidations { get; set; }
        public Dictionary<string, bool> Features { get; set; }

        public ValidationServiceCapabilities()
        {
            SupportedValidationTypes = new List<ValidationType>();
            SupportedScopes = new List<ValidationScope>();
            Features = new Dictionary<string, bool>();
            MaxConcurrentValidations = 4;
        }

        public bool HasFeature(string featureName)
        {
            return Features.ContainsKey(featureName) && Features[featureName];
        }

        public bool SupportsValidationType(ValidationType validationType)
        {
            return SupportedValidationTypes.Contains(validationType);
        }
    }

    [Serializable]
    public class ValidationProfile
    {
        public string ProfileId { get; set; }
        public string ProfileName { get; set; }
        public string Description { get; set; }
        public List<ValidationRule> Rules { get; set; }
        public ValidationSettings Settings { get; set; }
        public ProjectType[] ApplicableProjectTypes { get; set; }
        public bool IsDefault { get; set; }

        public ValidationProfile()
        {
            Rules = new List<ValidationRule>();
            Settings = new ValidationSettings();
            ApplicableProjectTypes = new ProjectType[0];
        }

        public ValidationProfile(string id, string name) : this()
        {
            ProfileId = id;
            ProfileName = name;
        }

        public bool IsApplicableToProject(ProjectType projectType)
        {
            return ApplicableProjectTypes.Length == 0 || ApplicableProjectTypes.Contains(projectType);
        }
    }

    public static class ValidationServiceExtensions
    {
        public static async Task<ValidationOperationResult> ValidateAllAspects(
            this IValidationService validationService,
            ProjectData projectData,
            string projectPath)
        {
            ValidationOperationResult operationResult = new ValidationOperationResult();
            List<string> results = new List<ValidationResult>();

            try
            {
                ValidationResult projectResult = await validationService.ValidateProjectAsync(projectData);
                results.Add(projectResult);
                operationResult.ResultsByType[ValidationType.ProjectStructure] = projectResult;

                ValidationResult documentationResult = await validationService.ValidateDocumentationAsync(projectData.DocumentationSections);
                results.Add(documentationResult);
                operationResult.ResultsByType[ValidationType.Documentation] = documentationResult;

                if (!string.IsNullOrEmpty(projectPath))
                {
                    ValidationResult structureResult = await validationService.ValidateProjectStructureAsync(projectPath);
                    results.Add(structureResult);
                    operationResult.ResultsByType[ValidationType.ProjectStructure] = structureResult;
                }

                operationResult.Results = results;
                operationResult.Success = results.All(r => r.IsValid);
                operationResult.OverallSummary = CreateOverallSummary(results);
            }
            catch (Exception ex)
            {
                operationResult.Success = false;
                operationResult.ErrorMessage = ex.Message;
            }

            return operationResult;
        }

        private static ValidationSummary CreateOverallSummary(List<ValidationResult> results)
        {
            return new ValidationSummary
            {
                IsValid = results.All(r => r.IsValid),
                TotalIssues = results.Sum(r => r.IssueCount),
                Blockers = results.Sum(r => r.BlockerCount),
                Warnings = results.Sum(r => r.WarningCount),
                InfoMessages = results.Sum(r => r.InfoCount),
                ValidationScore = results.Count > 0 ? results.Average(r => r.ValidationScore) : 0f,
                ValidationTime = DateTime.Now,
                Context = "Overall Project Validation"
            };
        }

        public static List<ValidationIssue> GetAllIssues(this ValidationOperationResult result)
        {
            List<string> allIssues = new List<ValidationIssue>();
            foreach (string validationResult in result.Results)
            {
                allIssues.AddRange(validationResult.Issues);
            }
            return allIssues;
        }

        public static List<ValidationIssue> GetBlockingIssues(this ValidationOperationResult result)
        {
            return result.GetAllIssues().Where(issue => issue.IsBlocker).ToList();
        }

        public static string GetFormattedReport(this ValidationOperationResult result)
        {
            string report = $"🔍 Validation Operation Report\n";
            report += $"✅ Overall Success: {result.Success}\n";
            report += $"⏱️ Total Time: {result.ValidationTime.TotalSeconds:F1}s\n";
            report += $"📊 Average Score: {result.AverageScore:F1}/100\n";
            report += $"🚨 Total Issues: {result.TotalIssues}\n";
            report += $"🚫 Blockers: {result.TotalBlockers}\n";
            report += $"⚠️ Warnings: {result.TotalWarnings}\n\n";

            foreach (string kvp in result.ResultsByType)
            {
                ValidationType validationType = kvp.Key;
                ValidationResult validationResult = kvp.Value;
                
                report += $"📋 {validationType}:\n";
                report += $"  Score: {validationResult.ValidationScore:F1}/100\n";
                report += $"  Issues: {validationResult.IssueCount} (Blockers: {validationResult.BlockerCount})\n";
                
                if (validationResult.BlockerCount > 0)
                {
                    IEnumerable<ValidationIssue> blockers = validationResult.GetBlockers().Take(3);
                    foreach (string blocker in blockers)
                    {
                        report += $"    🚫 {blocker.Message}\n";
                    }
                }
                report += "\n";
            }

            if (!result.Success)
            {
                report += $"❌ Operation Error: {result.ErrorMessage}\n";
            }

            return report;
        }

        public static async Task<ValidationResult> QuickValidate(
            this IValidationService validationService,
            ProjectData projectData)
        {
            return await validationService.ValidateProjectAsync(projectData);
        }

        public static bool IsReadyForProduction(this ValidationOperationResult result)
        {
            return result.Success && 
                   result.TotalBlockers == 0 && 
                   result.AverageScore >= 80f;
        }

        public static bool HasCriticalIssues(this ValidationOperationResult result)
        {
            return result.GetAllIssues().Any(issue => issue.Severity == ValidationSeverity.Critical);
        }

        public static Dictionary<ValidationType, float> GetScoresByType(this ValidationOperationResult result)
        {
            return result.ResultsByType.ToDictionary(
                kvp => kvp.Key, 
                kvp => kvp.Value.ValidationScore
            );
        }
    }

    public static class ValidationRuleExtensions
    {
        public static bool MatchesTag(this ValidationRule rule, string tag)
        {
            return rule.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase);
        }

        public static bool MatchesCategory(this ValidationRule rule, string category)
        {
            return string.Equals(rule.Category, category, StringComparison.OrdinalIgnoreCase);
        }

        public static ValidationRule WithSeverity(this ValidationRule rule, ValidationSeverity severity)
        {
            rule.Configuration.Severity = severity;
            return rule;
        }

        public static ValidationRule WithTag(this ValidationRule rule, string tag)
        {
            if (!rule.Tags.Contains(tag))
            {
                rule.Tags.Add(tag);
            }
            return rule;
        }

        public static ValidationRule WithCategory(this ValidationRule rule, string category)
        {
            rule.Category = category;
            return rule;
        }
    }
}