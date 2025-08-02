using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityProjectArchitect.Core;
using UnityProjectArchitect.API;

namespace UnityProjectArchitect.Services
{
    public class ExportTemplateManager
    {
        private readonly Dictionary<string, ExportTemplate> _templates;
        private readonly Dictionary<ExportFormat, List<ExportTemplate>> _formatTemplates;
        private readonly string _templateBasePath;

        public ExportTemplateManager()
        {
            _templates = new Dictionary<string, ExportTemplate>();
            _formatTemplates = new Dictionary<ExportFormat, List<ExportTemplate>>();
            _templateBasePath = Path.Combine(Application.streamingAssetsPath, "UnityProjectArchitect", "Templates");
            
            InitializeDefaultTemplates();
        }

        public List<ExportTemplate> GetTemplatesForFormat(ExportFormat format)
        {
            return _formatTemplates.ContainsKey(format) ? _formatTemplates[format] : new List<ExportTemplate>();
        }

        public ExportTemplate GetTemplate(string templateId)
        {
            return _templates.ContainsKey(templateId) ? _templates[templateId] : null;
        }

        public ExportTemplate GetDefaultTemplate(ExportFormat format)
        {
            List<ExportTemplate> templates = GetTemplatesForFormat(format);
            return templates.FirstOrDefault(t => t.TemplateId.EndsWith("_default")) ?? templates.FirstOrDefault();
        }

        public void RegisterTemplate(ExportTemplate template)
        {
            if (template == null || string.IsNullOrEmpty(template.TemplateId))
            {
                Debug.LogWarning("Cannot register null template or template with empty ID");
                return;
            }

            _templates[template.TemplateId] = template;

            if (!_formatTemplates.ContainsKey(template.Format))
            {
                _formatTemplates[template.Format] = new List<ExportTemplate>();
            }

            ExportTemplate existingTemplate = _formatTemplates[template.Format].FirstOrDefault(t => t.TemplateId == template.TemplateId);
            if (existingTemplate != null)
            {
                _formatTemplates[template.Format].Remove(existingTemplate);
            }

            _formatTemplates[template.Format].Add(template);
        }

        public void UnregisterTemplate(string templateId)
        {
            if (!_templates.ContainsKey(templateId)) return;

            ExportTemplate template = _templates[templateId];
            _templates.Remove(templateId);

            if (_formatTemplates.ContainsKey(template.Format))
            {
                _formatTemplates[template.Format].RemoveAll(t => t.TemplateId == templateId);
            }
        }

        public async Task<string> ProcessTemplateAsync(ExportTemplate template, ExportContent content, Dictionary<string, object> variables = null)
        {
            if (template == null || string.IsNullOrEmpty(template.TemplateContent))
            {
                return "";
            }

            Dictionary<string, object> allVariables = CreateDefaultVariables(content);
            
            // Merge custom variables
            if (variables != null)
            {
                foreach (KeyValuePair<string, object> kvp in variables)
                {
                    allVariables[kvp.Key] = kvp.Value;
                }
            }

            // Merge content variables
            if (content.Variables != null)
            {
                foreach (KeyValuePair<string, object> kvp in content.Variables)
                {
                    allVariables[kvp.Key] = kvp.Value;
                }
            }

            return await ProcessTemplateVariables(template.TemplateContent, allVariables);
        }

        public ValidationResult ValidateTemplate(ExportTemplate template)
        {
            ValidationResult validation = new ValidationResult { IsValid = true };

            if (template == null)
            {
                validation.IsValid = false;
                validation.Errors.Add("Template cannot be null");
                return validation;
            }

            if (string.IsNullOrEmpty(template.TemplateId))
            {
                validation.IsValid = false;
                validation.Errors.Add("Template ID cannot be empty");
            }

            if (string.IsNullOrEmpty(template.TemplateName))
            {
                validation.Warnings.Add("Template name is empty");
            }

            if (string.IsNullOrEmpty(template.TemplateContent))
            {
                validation.IsValid = false;
                validation.Errors.Add("Template content cannot be empty");
            }

            // Validate template variables
            if (template.Variables != null)
            {
                foreach (TemplateVariable variable in template.Variables)
                {
                    if (string.IsNullOrEmpty(variable.Name))
                    {
                        validation.Errors.Add("Template variable name cannot be empty");
                    }
                }
            }

            // Check for template syntax errors
            ValidationResult syntaxValidation = ValidateTemplateSyntax(template.TemplateContent);
            validation.Errors.AddRange(syntaxValidation.Errors);
            validation.Warnings.AddRange(syntaxValidation.Warnings);

            return validation;
        }

        public async Task<ExportTemplate> LoadTemplateFromFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"Template file not found: {filePath}");
                    return null;
                }

                string content = await File.ReadAllTextAsync(filePath);
                ExportTemplate template = ParseTemplateFromContent(content, Path.GetFileNameWithoutExtension(filePath));
                
                return template;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load template from {filePath}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> SaveTemplateToFileAsync(ExportTemplate template, string filePath)
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string templateData = SerializeTemplate(template);
                await File.WriteAllTextAsync(filePath, templateData);
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save template to {filePath}: {ex.Message}");
                return false;
            }
        }

        public List<string> GetAvailableVariables()
        {
            return new List<string>
            {
                "project_name", "project_description", "project_version",
                "team_name", "contact_email", "repository_url",
                "export_date", "export_time", "generator",
                "section_count", "total_words", "completion_percentage"
            };
        }

        private void InitializeDefaultTemplates()
        {
            // Markdown templates
            RegisterTemplate(CreateMarkdownDefaultTemplate());
            RegisterTemplate(CreateMarkdownDetailedTemplate());
            RegisterTemplate(CreateMarkdownMinimalTemplate());

            // PDF templates
            RegisterTemplate(CreatePDFDefaultTemplate());
            RegisterTemplate(CreatePDFProfessionalTemplate());
            RegisterTemplate(CreatePDFMinimalTemplate());

            // Unity Asset templates
            RegisterTemplate(CreateUnityAssetDefaultTemplate());
            RegisterTemplate(CreateUnityAssetMinimalTemplate());
        }

        private ExportTemplate CreateMarkdownDefaultTemplate()
        {
            return new ExportTemplate("markdown_default", "Default Markdown", ExportFormat.Markdown)
            {
                Description = "Standard markdown template with project header and table of contents",
                Author = "Unity Project Architect",
                Version = "1.0",
                TemplateContent = @"# {{project_name}}

{{project_description}}

**Version:** {{project_version}}  
**Generated:** {{export_date}} {{export_time}}  
**Generator:** {{generator}}  

---

## Project Information

| Property | Value |
|----------|-------|
| **Team** | {{team_name}} |
| **Contact** | {{contact_email}} |
| **Repository** | {{repository_url}} |
| **Sections** | {{section_count}} |
| **Total Words** | {{total_words}} |

---

",
                Variables = new List<TemplateVariable>
                {
                    new TemplateVariable("project_name", VariableType.String, "Untitled Project"),
                    new TemplateVariable("project_description", VariableType.String, ""),
                    new TemplateVariable("include_toc", VariableType.Boolean, true)
                }
            };
        }

        private ExportTemplate CreateMarkdownDetailedTemplate()
        {
            return new ExportTemplate("markdown_detailed", "Detailed Markdown", ExportFormat.Markdown)
            {
                Description = "Comprehensive markdown template with extensive metadata and styling",
                Author = "Unity Project Architect",
                Version = "1.0",
                TemplateContent = @"# 📋 {{project_name}}

> {{project_description}}

## 📊 Project Overview

| Property | Value |
|----------|-------|
| **Version** | {{project_version}} |
| **Team** | {{team_name}} |
| **Contact** | {{contact_email}} |
| **Repository** | {{repository_url}} |
| **Generated** | {{export_date}} at {{export_time}} |
| **Generator** | {{generator}} |
| **Documentation Sections** | {{section_count}} |
| **Total Words** | {{total_words:N0}} |
| **Completion** | {{completion_percentage:P1}} |

---

## 📑 Table of Contents

[Auto-generated based on sections]

---

",
                Variables = new List<TemplateVariable>
                {
                    new TemplateVariable("use_emoji", VariableType.Boolean, true),
                    new TemplateVariable("include_stats", VariableType.Boolean, true)
                }
            };
        }

        private ExportTemplate CreateMarkdownMinimalTemplate()
        {
            return new ExportTemplate("markdown_minimal", "Minimal Markdown", ExportFormat.Markdown)
            {
                Description = "Clean, minimal markdown template",
                Author = "Unity Project Architect",
                Version = "1.0",
                TemplateContent = @"# {{project_name}}

{{project_description}}

---

",
                Variables = new List<TemplateVariable>
                {
                    new TemplateVariable("project_name", VariableType.String, "Documentation"),
                    new TemplateVariable("project_description", VariableType.String, "")
                }
            };
        }

        private ExportTemplate CreatePDFDefaultTemplate()
        {
            return new ExportTemplate("pdf_default", "Default PDF", ExportFormat.PDF)
            {
                Description = "Standard PDF template with professional layout",
                Author = "Unity Project Architect",
                Version = "1.0",
                TemplateContent = @"<!DOCTYPE html>
<html>
<head>
    <title>{{project_name}} - Documentation</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; }
        .header { text-align: center; margin-bottom: 2em; }
        .project-info { margin: 2em 0; }
        h1 { color: #2c3e50; border-bottom: 2px solid #3498db; }
        h2 { color: #34495e; }
    </style>
</head>
<body>
    <div class=""header"">
        <h1>{{project_name}}</h1>
        <p>{{project_description}}</p>
        <p><strong>Version {{project_version}}</strong></p>
    </div>
    
    <div class=""project-info"">
        <p><strong>Generated:</strong> {{export_date}} {{export_time}}</p>
        <p><strong>Team:</strong> {{team_name}}</p>
        <p><strong>Contact:</strong> {{contact_email}}</p>
    </div>
</body>
</html>",
                Variables = new List<TemplateVariable>
                {
                    new TemplateVariable("include_header", VariableType.Boolean, true),
                    new TemplateVariable("include_footer", VariableType.Boolean, true)
                }
            };
        }

        private ExportTemplate CreatePDFProfessionalTemplate()
        {
            return new ExportTemplate("pdf_professional", "Professional PDF", ExportFormat.PDF)
            {
                Description = "Professional PDF template with corporate styling",
                Author = "Unity Project Architect",
                Version = "1.0",
                TemplateContent = @"<!DOCTYPE html>
<html>
<head>
    <title>{{project_name}} - Professional Documentation</title>
    <style>
        body { font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.8; color: #2c3e50; }
        .cover-page { text-align: center; padding: 4em 0; border-bottom: 3px solid #3498db; }
        .logo-area { margin-bottom: 2em; }
        .project-title { font-size: 2.5em; font-weight: 300; margin: 1em 0; }
        .project-subtitle { font-size: 1.2em; color: #7f8c8d; }
        .metadata-table { margin: 2em auto; max-width: 600px; }
        .metadata-table td { padding: 0.5em; border-bottom: 1px solid #ecf0f1; }
    </style>
</head>
<body>
    <div class=""cover-page"">
        <div class=""logo-area"">
            <!-- Logo placeholder -->
        </div>
        
        <h1 class=""project-title"">{{project_name}}</h1>
        <p class=""project-subtitle"">{{project_description}}</p>
        
        <table class=""metadata-table"">
            <tr><td><strong>Version:</strong></td><td>{{project_version}}</td></tr>
            <tr><td><strong>Team:</strong></td><td>{{team_name}}</td></tr>
            <tr><td><strong>Contact:</strong></td><td>{{contact_email}}</td></tr>
            <tr><td><strong>Generated:</strong></td><td>{{export_date}} {{export_time}}</td></tr>
        </table>
    </div>
</body>
</html>",
                Variables = new List<TemplateVariable>
                {
                    new TemplateVariable("company_logo", VariableType.String, ""),
                    new TemplateVariable("company_name", VariableType.String, "")
                }
            };
        }

        private ExportTemplate CreatePDFMinimalTemplate()
        {
            return new ExportTemplate("pdf_minimal", "Minimal PDF", ExportFormat.PDF)
            {
                Description = "Clean, minimal PDF template",
                Author = "Unity Project Architect",
                Version = "1.0",
                TemplateContent = @"<!DOCTYPE html>
<html>
<head>
    <title>{{project_name}}</title>
    <style>
        body { font-family: serif; line-height: 1.5; max-width: 800px; margin: 0 auto; }
        h1 { font-weight: normal; }
        .subtitle { color: #666; font-style: italic; }
    </style>
</head>
<body>
    <h1>{{project_name}}</h1>
    <p class=""subtitle"">{{project_description}}</p>
    <hr>
</body>
</html>",
                Variables = new List<TemplateVariable>()
            };
        }

        private ExportTemplate CreateUnityAssetDefaultTemplate()
        {
            return new ExportTemplate("unity_default", "Default Unity Asset", ExportFormat.JSON)
            {
                Description = "Standard Unity ScriptableObject export template",
                Author = "Unity Project Architect",
                Version = "1.0",
                TemplateContent = "Standard Unity asset export with full project data and metadata",
                Variables = new List<TemplateVariable>
                {
                    new TemplateVariable("create_menu_items", VariableType.Boolean, false),
                    new TemplateVariable("include_metadata", VariableType.Boolean, true)
                }
            };
        }

        private ExportTemplate CreateUnityAssetMinimalTemplate()
        {
            return new ExportTemplate("unity_minimal", "Minimal Unity Asset", ExportFormat.JSON)
            {
                Description = "Minimal Unity ScriptableObject export with essential data only",
                Author = "Unity Project Architect",
                Version = "1.0",
                TemplateContent = "Minimal Unity asset export with essential project data",
                Variables = new List<TemplateVariable>
                {
                    new TemplateVariable("include_metadata", VariableType.Boolean, false)
                }
            };
        }

        private Dictionary<string, object> CreateDefaultVariables(ExportContent content)
        {
            Dictionary<string, object> variables = new Dictionary<string, object>();

            // Project variables
            if (content.ProjectData != null)
            {
                variables["project_name"] = content.ProjectData.ProjectName ?? "Untitled Project";
                variables["project_description"] = content.ProjectData.ProjectDescription ?? "";
                variables["project_version"] = content.ProjectData.ProjectVersion ?? "1.0.0";
                variables["team_name"] = content.ProjectData.TeamName ?? "";
                variables["contact_email"] = content.ProjectData.ContactEmail ?? "";
                variables["repository_url"] = content.ProjectData.RepositoryUrl ?? "";
            }
            else
            {
                variables["project_name"] = content.Title ?? "Documentation";
                variables["project_description"] = "";
                variables["project_version"] = "1.0.0";
                variables["team_name"] = "";
                variables["contact_email"] = "";
                variables["repository_url"] = "";
            }

            // Export metadata
            variables["export_date"] = DateTime.Now.ToString("yyyy-MM-dd");
            variables["export_time"] = DateTime.Now.ToString("HH:mm:ss");
            variables["generator"] = "Unity Project Architect";
            variables["section_count"] = content.Sections?.Count ?? 0;

            // Statistics
            int totalWords = content.Sections?.Sum(s => s.CurrentWordCount) ?? 0;
            variables["total_words"] = totalWords;

            int completedSections = content.Sections?.Count(s => s.Status == DocumentationStatus.Completed) ?? 0;
            int totalSections = content.Sections?.Count ?? 1;
            variables["completion_percentage"] = (float)completedSections / totalSections;

            return variables;
        }

        private async Task<string> ProcessTemplateVariables(string template, Dictionary<string, object> variables)
        {
            string processed = template;

            foreach (KeyValuePair<string, object> variable in variables)
            {
                string placeholder = $"{{{{{variable.Key}}}}}";
                string value = variable.Value?.ToString() ?? "";
                
                // Handle formatting for specific types
                if (variable.Value is float floatValue && variable.Key.Contains("percentage"))
                {
                    value = (floatValue * 100).ToString("F1") + "%";
                }
                else if (variable.Value is int intValue && variable.Key.Contains("words"))
                {
                    value = intValue.ToString("N0");
                }

                processed = processed.Replace(placeholder, value);
            }

            return processed;
        }

        private ValidationResult ValidateTemplateSyntax(string templateContent)
        {
            ValidationResult validation = new ValidationResult { IsValid = true };

            if (string.IsNullOrEmpty(templateContent))
            {
                return validation;
            }

            // Check for unmatched variable brackets
            int openBrackets = templateContent.Count(c => c == '{');
            int closeBrackets = templateContent.Count(c => c == '}');

            if (openBrackets != closeBrackets)
            {
                validation.Warnings.Add("Unmatched variable brackets found in template");
            }

            // Check for valid variable syntax
            System.Text.RegularExpressions.Regex variablePattern = new System.Text.RegularExpressions.Regex(@"\{\{(\w+)\}\}");
            System.Text.RegularExpressions.MatchCollection matches = variablePattern.Matches(templateContent);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                string variableName = match.Groups[1].Value;
                if (string.IsNullOrEmpty(variableName))
                {
                    validation.Warnings.Add("Empty variable name found in template");
                }
            }

            return validation;
        }

        private ExportTemplate ParseTemplateFromContent(string content, string templateId)
        {
            // Simple template parsing - in a real implementation, this would be more sophisticated
            ExportTemplate template = new ExportTemplate(templateId, templateId, ExportFormat.Markdown)
            {
                TemplateContent = content,
                Author = "Custom",
                Version = "1.0",
                Description = "Custom template"
            };

            return template;
        }

        private string SerializeTemplate(ExportTemplate template)
        {
            // Simple serialization - in a real implementation, this would use JSON or XML
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"# Template: {template.TemplateName}");
            sb.AppendLine($"# ID: {template.TemplateId}");
            sb.AppendLine($"# Format: {template.Format}");
            sb.AppendLine($"# Description: {template.Description}");
            sb.AppendLine($"# Author: {template.Author}");
            sb.AppendLine($"# Version: {template.Version}");
            sb.AppendLine("# Content:");
            sb.AppendLine(template.TemplateContent);

            return sb.ToString();
        }
    }
}