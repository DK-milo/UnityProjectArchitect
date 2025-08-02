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
        [SerializeField] private ValidationSeverity _severity = ValidationSeverity.Info;
        [SerializeField] private ValidationType _type = ValidationType.ProjectStructure;
        [SerializeField] private string _message = "";
        [SerializeField] private string _details = "";
        [SerializeField] private string _suggestedFix = "";
        [SerializeField] private string _context = "";
        [SerializeField] private DateTime _timestamp;

        public ValidationSeverity Severity 
        { 
            get => _severity; 
            set => _severity = value; 
        }

        public ValidationType Type 
        { 
            get => _type; 
            set => _type = value; 
        }

        public string Message 
        { 
            get => _message; 
            set => _message = value; 
        }

        public string Details 
        { 
            get => _details; 
            set => _details = value; 
        }

        public string SuggestedFix 
        { 
            get => _suggestedFix; 
            set => _suggestedFix = value; 
        }

        public string Context 
        { 
            get => _context; 
            set => _context = value; 
        }

        public DateTime Timestamp 
        { 
            get => _timestamp; 
            set => _timestamp = value; 
        }

        public ValidationIssue()
        {
            _timestamp = DateTime.Now;
        }

        public ValidationIssue(ValidationSeverity sev, ValidationType typ, string msg) : this()
        {
            _severity = sev;
            _type = typ;
            _message = msg;
        }

        public ValidationIssue(ValidationSeverity sev, ValidationType typ, string msg, string det, string fix = "") : this(sev, typ, msg)
        {
            _details = det;
            _suggestedFix = fix;
        }

        public bool IsBlocker => _severity >= ValidationSeverity.Error;
        public bool RequiresAttention => _severity >= ValidationSeverity.Warning;

        public override string ToString()
        {
            return $"[{_severity}] {_type}: {_message}";
        }
    }

    [Serializable]
    public class ValidationResult
    {
        [SerializeField] private bool _isValid = true;
        [SerializeField] private List<ValidationIssue> _issues = new List<ValidationIssue>();
        [SerializeField] private DateTime _validationTime;
        [SerializeField] private string _validationContext = "";
        [SerializeField] private float _validationScore = 100f;

        public bool IsValid 
        { 
            get => _isValid && !HasBlockers; 
            private set => _isValid = value; 
        }

        public List<ValidationIssue> Issues => _issues;

        public DateTime ValidationTime 
        { 
            get => _validationTime; 
            set => _validationTime = value; 
        }

        public string ValidationContext 
        { 
            get => _validationContext; 
            set => _validationContext = value; 
        }

        public float ValidationScore 
        { 
            get => _validationScore; 
            private set => _validationScore = Mathf.Clamp(value, 0f, 100f); 
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
            _validationTime = DateTime.Now;
            _issues = new List<ValidationIssue>();
        }

        public ValidationResult(string context) : this()
        {
            _validationContext = context;
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
            _validationTime = DateTime.Now;
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

            string report = $"📊 Validation Report (Score: {result.ValidationScore:F1}/100)\n";
            report += $"📅 Time: {result.ValidationTime:yyyy-MM-dd HH:mm:ss}\n\n";

            if (result.HasBlockers)
            {
                report += "🚫 **BLOCKERS:**\n";
                foreach (string blocker in result.GetBlockers())
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
                foreach (string warning in result.GetIssuesBySeverity(ValidationSeverity.Warning))
                {
                    report += $"  • {warning.Message}\n";
                }
                report += "\n";
            }

            List<ValidationIssue> infoIssues = result.GetIssuesBySeverity(ValidationSeverity.Info);
            if (infoIssues.Count > 0)
            {
                report += "ℹ️  **INFO:**\n";
                foreach (string info in infoIssues)
                {
                    report += $"  • {info.Message}\n";
                }
            }

            return report;
        }
    }
}