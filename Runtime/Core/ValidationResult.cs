using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    [Serializable]
    public class ValidationIssue
    {
        [SerializeField] private ValidationSeverity severity = ValidationSeverity.Info;
        [SerializeField] private ValidationType type = ValidationType.ProjectStructure;
        [SerializeField] private string message = "";
        [SerializeField] private string details = "";
        [SerializeField] private string suggestedFix = "";
        [SerializeField] private string context = "";
        [SerializeField] private DateTime timestamp;

        public ValidationSeverity Severity 
        { 
            get => severity; 
            set => severity = value; 
        }

        public ValidationType Type 
        { 
            get => type; 
            set => type = value; 
        }

        public string Message 
        { 
            get => message; 
            set => message = value; 
        }

        public string Details 
        { 
            get => details; 
            set => details = value; 
        }

        public string SuggestedFix 
        { 
            get => suggestedFix; 
            set => suggestedFix = value; 
        }

        public string Context 
        { 
            get => context; 
            set => context = value; 
        }

        public DateTime Timestamp 
        { 
            get => timestamp; 
            set => timestamp = value; 
        }

        public ValidationIssue()
        {
            timestamp = DateTime.Now;
        }

        public ValidationIssue(ValidationSeverity sev, ValidationType typ, string msg) : this()
        {
            severity = sev;
            type = typ;
            message = msg;
        }

        public ValidationIssue(ValidationSeverity sev, ValidationType typ, string msg, string det, string fix = "") : this(sev, typ, msg)
        {
            details = det;
            suggestedFix = fix;
        }

        public bool IsBlocker => severity >= ValidationSeverity.Error;
        public bool RequiresAttention => severity >= ValidationSeverity.Warning;

        public override string ToString()
        {
            return $"[{severity}] {type}: {message}";
        }
    }

    [Serializable]
    public class ValidationResult
    {
        [SerializeField] private bool isValid = true;
        [SerializeField] private List<ValidationIssue> issues = new List<ValidationIssue>();
        [SerializeField] private DateTime validationTime;
        [SerializeField] private string validationContext = "";
        [SerializeField] private float validationScore = 100f;

        public bool IsValid 
        { 
            get => isValid && !HasBlockers; 
            private set => isValid = value; 
        }

        public List<ValidationIssue> Issues => issues;

        public DateTime ValidationTime 
        { 
            get => validationTime; 
            set => validationTime = value; 
        }

        public string ValidationContext 
        { 
            get => validationContext; 
            set => validationContext = value; 
        }

        public float ValidationScore 
        { 
            get => validationScore; 
            private set => validationScore = Mathf.Clamp(value, 0f, 100f); 
        }

        public bool HasIssues => issues.Count > 0;
        public bool HasBlockers => issues.Any(issue => issue.IsBlocker);
        public bool HasWarnings => issues.Any(issue => issue.Severity == ValidationSeverity.Warning);
        public int IssueCount => issues.Count;
        public int BlockerCount => issues.Count(issue => issue.IsBlocker);
        public int WarningCount => issues.Count(issue => issue.Severity == ValidationSeverity.Warning);
        public int InfoCount => issues.Count(issue => issue.Severity == ValidationSeverity.Info);

        public ValidationResult()
        {
            validationTime = DateTime.Now;
            issues = new List<ValidationIssue>();
        }

        public ValidationResult(string context) : this()
        {
            validationContext = context;
        }

        public void AddIssue(ValidationIssue issue)
        {
            if (issue != null)
            {
                issues.Add(issue);
                RecalculateValidation();
            }
        }

        public void AddIssue(ValidationSeverity severity, ValidationType type, string message, string details = "", string suggestedFix = "", string context = "")
        {
            var issue = new ValidationIssue(severity, type, message, details, suggestedFix)
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
            return issues.Where(issue => issue.Severity == severity).ToList();
        }

        public List<ValidationIssue> GetIssuesByType(ValidationType type)
        {
            return issues.Where(issue => issue.Type == type).ToList();
        }

        public List<ValidationIssue> GetBlockers()
        {
            return issues.Where(issue => issue.IsBlocker).ToList();
        }

        public List<ValidationIssue> GetWarningsAndErrors()
        {
            return issues.Where(issue => issue.RequiresAttention).ToList();
        }

        public void RemoveIssue(ValidationIssue issue)
        {
            if (issues.Remove(issue))
            {
                RecalculateValidation();
            }
        }

        public void ClearIssues()
        {
            issues.Clear();
            RecalculateValidation();
        }

        public void ClearIssuesByType(ValidationType type)
        {
            issues.RemoveAll(issue => issue.Type == type);
            RecalculateValidation();
        }

        public void ClearIssuesBySeverity(ValidationSeverity severity)
        {
            issues.RemoveAll(issue => issue.Severity == severity);
            RecalculateValidation();
        }

        private void RecalculateValidation()
        {
            isValid = !HasBlockers;
            CalculateScore();
            validationTime = DateTime.Now;
        }

        private void CalculateScore()
        {
            if (issues.Count == 0)
            {
                validationScore = 100f;
                return;
            }

            float penalty = 0f;
            foreach (var issue in issues)
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

            validationScore = Math.Max(0f, 100f - penalty);
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

        public static ValidationResult Combine(params ValidationResult[] results)
        {
            var combined = new ValidationResult("Combined Validation");
            
            foreach (var result in results)
            {
                if (result != null)
                {
                    combined.issues.AddRange(result.Issues);
                }
            }
            
            combined.RecalculateValidation();
            return combined;
        }
    }

    [Serializable]
    public class ValidationSummary
    {
        public bool IsValid;
        public int TotalIssues;
        public int Blockers;
        public int Warnings;
        public int InfoMessages;
        public float ValidationScore;
        public DateTime ValidationTime;
        public string Context;

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

            var report = $"📊 Validation Report (Score: {result.ValidationScore:F1}/100)\n";
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