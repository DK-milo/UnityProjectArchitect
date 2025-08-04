using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityProjectArchitect.Core
{
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }

    public enum ValidationType
    {
        ProjectStructure,
        Documentation,
        Templates,
        AIConfiguration,
        ExportSettings,
        Performance,
        Compatibility
    }

    public class ValidationIssue
    {
        public ValidationSeverity Severity { get; set; } = ValidationSeverity.Info;
        public ValidationType Type { get; set; } = ValidationType.ProjectStructure;
        public string Message { get; set; } = "";
        public string Details { get; set; } = "";
        public string SuggestedFix { get; set; } = "";
        public string Context { get; set; } = "";
        public DateTime Timestamp { get; set; }

        public ValidationIssue()
        {
            Timestamp = DateTime.Now;
        }

        public ValidationIssue(ValidationSeverity severity, ValidationType type, string message) : this()
        {
            Severity = severity;
            Type = type;
            Message = message;
        }

        public ValidationIssue(ValidationSeverity severity, ValidationType type, string message, string details, string suggestedFix = "") : this(severity, type, message)
        {
            Details = details;
            SuggestedFix = suggestedFix;
        }

        public bool IsBlocker => Severity >= ValidationSeverity.Error;
        public bool RequiresAttention => Severity >= ValidationSeverity.Warning;

        public override string ToString()
        {
            return $"[{Severity}] {Type}: {Message}";
        }
    }

    public class ValidationResult
    {
        private bool _isValid = true;
        private List<ValidationIssue> _issues = new List<ValidationIssue>();
        private float _validationScore = 100f;

        public bool IsValid 
        { 
            get => _isValid && !HasBlockers; 
            private set => _isValid = value; 
        }

        public List<ValidationIssue> Issues => _issues;
        public DateTime ValidationTime { get; set; }
        public string ValidationContext { get; set; } = "";
        public string ErrorMessage { get; set; } = "";

        public float ValidationScore 
        { 
            get => _validationScore; 
            private set => _validationScore = Math.Max(0f, Math.Min(value, 100f)); 
        }

        public bool HasIssues => _issues.Count > 0;
        public bool HasBlockers => _issues.Any(issue => issue.IsBlocker);
        public bool HasWarnings => _issues.Any(issue => issue.Severity == ValidationSeverity.Warning);
        public int IssueCount => _issues.Count;
        public int BlockerCount => _issues.Count(issue => issue.IsBlocker);
        public int WarningCount => _issues.Count(issue => issue.Severity == ValidationSeverity.Warning);
        public int InfoCount => _issues.Count(issue => issue.Severity == ValidationSeverity.Info);

        public ValidationResult()
        {
            ValidationTime = DateTime.Now;
            _issues = new List<ValidationIssue>();
        }

        public ValidationResult(string context) : this()
        {
            ValidationContext = context;
        }

        public void AddIssue(ValidationIssue issue)
        {
            if (issue != null)
            {
                _issues.Add(issue);
                RecalculateValidation();
            }
        }

        public void AddIssue(ValidationSeverity severity, ValidationType type, string message, string details = "", string suggestedFix = "", string context = "")
        {
            ValidationIssue issue = new ValidationIssue(severity, type, message, details, suggestedFix)
            {
                Context = context
            };
            AddIssue(issue);
        }

        public void AddInfo(ValidationType type, string message, string details = "")
        {
            AddIssue(ValidationSeverity.Info, type, message, details);
        }

        public void AddWarning(ValidationType type, string message, string details = "", string suggestedFix = "")
        {
            AddIssue(ValidationSeverity.Warning, type, message, details, suggestedFix);
        }

        public void AddError(ValidationType type, string message, string details = "", string suggestedFix = "")
        {
            AddIssue(ValidationSeverity.Error, type, message, details, suggestedFix);
        }

        public void AddCritical(ValidationType type, string message, string details = "", string suggestedFix = "")
        {
            AddIssue(ValidationSeverity.Critical, type, message, details, suggestedFix);
        }

        public List<ValidationIssue> GetIssuesBySeverity(ValidationSeverity severity)
        {
            return _issues.Where(issue => issue.Severity == severity).ToList();
        }

        public List<ValidationIssue> GetIssuesByType(ValidationType type)
        {
            return _issues.Where(issue => issue.Type == type).ToList();
        }

        public List<ValidationIssue> GetBlockers()
        {
            return _issues.Where(issue => issue.IsBlocker).ToList();
        }

        public List<ValidationIssue> GetWarningsAndErrors()
        {
            return _issues.Where(issue => issue.RequiresAttention).ToList();
        }

        public void RemoveIssue(ValidationIssue issue)
        {
            if (_issues.Remove(issue))
            {
                RecalculateValidation();
            }
        }

        public void ClearIssues()
        {
            _issues.Clear();
            RecalculateValidation();
        }

        public void ClearIssuesByType(ValidationType type)
        {
            _issues.RemoveAll(issue => issue.Type == type);
            RecalculateValidation();
        }

        public void ClearIssuesBySeverity(ValidationSeverity severity)
        {
            _issues.RemoveAll(issue => issue.Severity == severity);
            RecalculateValidation();
        }

        private void RecalculateValidation()
        {
            _isValid = !HasBlockers;
            CalculateScore();
            ValidationTime = DateTime.Now;
        }

        private void CalculateScore()
        {
            if (_issues.Count == 0)
            {
                _validationScore = 100f;
                return;
            }

            float penalty = 0f;
            foreach (ValidationIssue issue in _issues)
            {
                penalty += issue.Severity switch
                {
                    ValidationSeverity.Info => 0.5f,
                    ValidationSeverity.Warning => 2f,
                    ValidationSeverity.Error => 10f,
                    ValidationSeverity.Critical => 25f,
                    _ => 0f
                };
            }

            _validationScore = Math.Max(0f, 100f - penalty);
        }

        public ValidationSummary GetSummary()
        {
            return new ValidationSummary
            {
                IsValid = IsValid,
                TotalIssues = IssueCount,
                Blockers = BlockerCount,
                Warnings = WarningCount,
                InfoMessages = InfoCount,
                ValidationScore = ValidationScore,
                ValidationTime = ValidationTime,
                Context = ValidationContext
            };
        }

        public override string ToString()
        {
            if (IsValid)
                return $"Validation passed with {IssueCount} issues (Score: {ValidationScore:F1}/100)";
            else
                return $"Validation failed with {BlockerCount} blockers, {WarningCount} warnings (Score: {ValidationScore:F1}/100)";
        }

        public static ValidationResult Success(string message)
        {
            return new ValidationResult
            {
                _isValid = true,
                ValidationContext = message,
                ValidationTime = DateTime.Now
            };
        }

        public static ValidationResult Failure(string errorMessage)
        {
            var result = new ValidationResult
            {
                _isValid = false,
                ErrorMessage = errorMessage,
                ValidationTime = DateTime.Now
            };
            result.AddError(ValidationType.ProjectStructure, errorMessage);
            return result;
        }

        public static ValidationResult Combine(params ValidationResult[] results)
        {
            ValidationResult combined = new ValidationResult("Combined Validation");
            
            foreach (ValidationResult result in results)
            {
                if (result != null)
                {
                    combined._issues.AddRange(result.Issues);
                }
            }
            
            combined.RecalculateValidation();
            return combined;
        }
    }

    public class ValidationSummary
    {
        public bool IsValid { get; set; }
        public int TotalIssues { get; set; }
        public int Blockers { get; set; }
        public int Warnings { get; set; }
        public int InfoMessages { get; set; }
        public float ValidationScore { get; set; }
        public DateTime ValidationTime { get; set; }
        public string Context { get; set; } = "";

        public override string ToString()
        {
            return $"Valid: {IsValid}, Issues: {TotalIssues}, Blockers: {Blockers}, Score: {ValidationScore:F1}";
        }
    }

    public static class ValidationExtensions
    {
        public static bool HasIssuesOfType(this ValidationResult result, ValidationType type)
        {
            return result.Issues.Any(issue => issue.Type == type);
        }

        public static bool HasSeverityLevel(this ValidationResult result, ValidationSeverity severity)
        {
            return result.Issues.Any(issue => issue.Severity == severity);
        }

        public static string GetFormattedReport(this ValidationResult result)
        {
            if (!result.HasIssues)
                return "✅ Validation passed with no issues.";

            string report = $"📊 Validation Report (Score: {result.ValidationScore:F1}/100)\n";
            report += $"📅 Time: {result.ValidationTime:yyyy-MM-dd HH:mm:ss}\n\n";

            if (result.HasBlockers)
            {
                report += "🚫 **BLOCKERS:**\n";
                foreach (var blocker in result.GetBlockers())
                {
                    report += $"  • {blocker.Message}\n";
                    if (!string.IsNullOrEmpty(blocker.SuggestedFix))
                        report += $"    💡 Fix: {blocker.SuggestedFix}\n";
                }
                report += "\n";
            }

            if (result.HasWarnings)
            {
                report += "⚠️  **WARNINGS:**\n";
                foreach (var warning in result.GetIssuesBySeverity(ValidationSeverity.Warning))
                {
                    report += $"  • {warning.Message}\n";
                }
                report += "\n";
            }

            var infoIssues = result.GetIssuesBySeverity(ValidationSeverity.Info);
            if (infoIssues.Count > 0)
            {
                report += "ℹ️  **INFO:**\n";
                foreach (var info in infoIssues)
                {
                    report += $"  • {info.Message}\n";
                }
            }

            return report;
        }
    }
}