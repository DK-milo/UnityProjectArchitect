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
    public class MarkdownExporter : IExportFormatter
    {
        public ExportFormat SupportedFormat => ExportFormat.Markdown;
        public string DisplayName => "Markdown";
        public string FileExtension => "md";
        public string MimeType => "text/markdown";

        private readonly Dictionary<string, MarkdownTemplate> _templates;
        private readonly StringBuilder _builder;

        public MarkdownExporter()
        {
            _templates = new Dictionary<string, MarkdownTemplate>();
            _builder = new StringBuilder();
            InitializeDefaultTemplates();
        }

        public async Task<ExportOperationResult> FormatAsync(ExportContent content, ExportOptions options)
        {
            var result = new ExportOperationResult(ExportFormat.Markdown, "");
            var startTime = DateTime.Now;

            try
            {
                _builder.Clear();
                
                // Apply template or use default
                var templateId = options.FormatSpecificOptions?.ContainsKey("TemplateId") == true 
                    ? options.FormatSpecificOptions["TemplateId"].ToString() 
                    : "default";
                
                var template = GetTemplate(templateId);
                var markdownContent = await ApplyTemplateAsync(content, template, options);
                
                result.Success = true;
                result.GeneratedFiles.Add($"{content.Title}.md");
                result.TotalSizeBytes = Encoding.UTF8.GetByteCount(markdownContent);
                result.ExportTime = DateTime.Now - startTime;
                
                // Store content in metadata for file writing
                result.Metadata["content"] = markdownContent;
                result.Statistics = GenerateStatistics(content, markdownContent);

                Debug.Log($"Markdown export completed: {result.FormattedSize} in {result.ExportTime.TotalSeconds:F1}s");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Markdown export failed: {ex.Message}";
                Debug.LogError($"MarkdownExporter error: {ex}");
            }

            return result;
        }

        public ValidationResult ValidateContent(ExportContent content)
        {
            var validation = new ValidationResult { IsValid = true };

            if (content == null)
            {
                validation.IsValid = false;
                validation.Errors.Add("Export content cannot be null");
                return validation;
            }

            if (string.IsNullOrEmpty(content.Title))
            {
                validation.Warnings.Add("Content title is empty - using default");
            }

            if (content.ProjectData == null)
            {
                validation.Warnings.Add("Project data is null - limited formatting available");
            }

            if (content.Sections == null || content.Sections.Count == 0)
            {
                validation.Warnings.Add("No documentation sections to export");
            }

            return validation;
        }

        public ExportTemplate GetDefaultTemplate()
        {
            return new ExportTemplate("default", "Default Markdown", ExportFormat.Markdown)
            {
                Description = "Standard markdown documentation template",
                Author = "Unity Project Architect",
                Version = "1.0",
                TemplateContent = GetDefaultTemplateContent()
            };
        }

        public List<ExportOption> GetAvailableOptions()
        {
            return new List<ExportOption>
            {
                new ExportOption("TemplateId", "Template", "default", "Template to use for export"),
                new ExportOption("IncludeCodeBlocks", "Include Code Blocks", true, "Include code examples in fenced blocks"),
                new ExportOption("GenerateTOC", "Generate Table of Contents", true, "Auto-generate table of contents"),
                new ExportOption("IncludeMetadata", "Include Metadata", true, "Add project metadata header"),
                new ExportOption("UseEmoji", "Use Emoji Icons", true, "Include emoji in section headers"),
                new ExportOption("LinkStyle", "Link Style", "inline", "Link style: inline, reference, or auto")
            };
        }

        private async Task<string> ApplyTemplateAsync(ExportContent content, MarkdownTemplate template, ExportOptions options)
        {
            _builder.Clear();

            var variables = CreateTemplateVariables(content, options);
            var processedTemplate = await ProcessTemplateVariables(template.Content, variables);
            
            // Process sections
            if (content.Sections?.Count > 0)
            {
                foreach (var section in content.Sections.Where(s => s.IsEnabled))
                {
                    await AppendSectionAsync(section, options);
                }
            }

            return processedTemplate + _builder.ToString();
        }

        private Dictionary<string, object> CreateTemplateVariables(ExportContent content, ExportOptions options)
        {
            var variables = new Dictionary<string, object>();
            
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
                variables["project_name"] = content.Title ?? "Untitled Project";
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

            // Options
            variables["include_toc"] = options.IncludeTableOfContents;
            variables["include_timestamp"] = options.IncludeTimestamp;
            variables["use_emoji"] = GetOption<bool>(options, "UseEmoji", true);

            // Add custom variables
            if (content.Variables != null)
            {
                foreach (var kvp in content.Variables)
                {
                    variables[kvp.Key] = kvp.Value;
                }
            }

            return variables;
        }

        private async Task<string> ProcessTemplateVariables(string template, Dictionary<string, object> variables)
        {
            var processed = template;
            
            foreach (var variable in variables)
            {
                var placeholder = $"{{{{{variable.Key}}}}}";
                processed = processed.Replace(placeholder, variable.Value?.ToString() ?? "");
            }

            return processed;
        }

        private async Task AppendSectionAsync(DocumentationSectionData section, ExportOptions options)
        {
            if (!section.HasContent) return;

            var useEmoji = GetOption<bool>(options, "UseEmoji", true);
            var emoji = useEmoji ? GetSectionEmoji(section.SectionType) : "";
            
            _builder.AppendLine();
            _builder.AppendLine($"## {emoji}{section.Title}");
            _builder.AppendLine();

            // Add section metadata if enabled
            if (options.IncludeMetadata)
            {
                _builder.AppendLine($"**Status:** {section.Status}  ");
                _builder.AppendLine($"**Last Updated:** {section.LastUpdated:yyyy-MM-dd}  ");
                _builder.AppendLine($"**Word Count:** {section.CurrentWordCount}  ");
                _builder.AppendLine();
            }

            // Process content with markdown formatting
            var processedContent = ProcessMarkdownContent(section.Content, options);
            _builder.AppendLine(processedContent);
        }

        private string ProcessMarkdownContent(string content, ExportOptions options)
        {
            if (string.IsNullOrEmpty(content)) return "";

            var processed = content;
            var includeCodeBlocks = GetOption<bool>(options, "IncludeCodeBlocks", true);

            // Ensure proper line endings
            processed = processed.Replace("\r\n", "\n").Replace("\r", "\n");

            // Add code block formatting if needed
            if (includeCodeBlocks)
            {
                processed = FormatCodeBlocks(processed);
            }

            return processed;
        }

        private string FormatCodeBlocks(string content)
        {
            // Simple code block detection and formatting
            var lines = content.Split('\n');
            var result = new StringBuilder();
            bool inCodeBlock = false;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                
                // Detect code patterns
                if (IsCodeLine(trimmed) && !inCodeBlock)
                {
                    result.AppendLine("```csharp");
                    inCodeBlock = true;
                }
                else if (inCodeBlock && !IsCodeLine(trimmed) && !string.IsNullOrWhiteSpace(trimmed))
                {
                    result.AppendLine("```");
                    result.AppendLine();
                    inCodeBlock = false;
                }

                result.AppendLine(line);
            }

            if (inCodeBlock)
            {
                result.AppendLine("```");
            }

            return result.ToString();
        }

        private bool IsCodeLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            
            // Basic code detection patterns
            return line.Contains("class ") || line.Contains("public ") || line.Contains("private ") ||
                   line.Contains("void ") || line.Contains("return ") || line.EndsWith(";") ||
                   line.EndsWith("{") || line.EndsWith("}") || line.StartsWith("using ");
        }

        private string GetSectionEmoji(DocumentationSectionType sectionType)
        {
            return sectionType switch
            {
                DocumentationSectionType.GeneralProductDescription => "📋 ",
                DocumentationSectionType.SystemArchitecture => "🏗️ ",
                DocumentationSectionType.DataModel => "🗃️ ",
                DocumentationSectionType.APISpecification => "🔌 ",
                DocumentationSectionType.UserStories => "👤 ",
                DocumentationSectionType.WorkTickets => "🎫 ",
                _ => "📄 "
            };
        }

        private T GetOption<T>(ExportOptions options, string key, T defaultValue)
        {
            if (options.FormatSpecificOptions?.ContainsKey(key) == true)
            {
                try
                {
                    return (T)Convert.ChangeType(options.FormatSpecificOptions[key], typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        private ExportStatistics GenerateStatistics(ExportContent content, string markdownContent)
        {
            var stats = new ExportStatistics();
            
            if (content.Sections != null)
            {
                stats.TotalSections = content.Sections.Count;
                stats.ExportedSections = content.Sections.Count(s => s.IsEnabled && s.HasContent);
                stats.SkippedSections = stats.TotalSections - stats.ExportedSections;
            }

            if (!string.IsNullOrEmpty(markdownContent))
            {
                stats.TotalWords = markdownContent.Split(new char[] { ' ', '\n', '\r', '\t' }, 
                    StringSplitOptions.RemoveEmptyEntries).Length;
                stats.TotalPages = Math.Max(1, stats.TotalWords / 250); // Estimate 250 words per page
            }

            return stats;
        }

        private void InitializeDefaultTemplates()
        {
            _templates["default"] = new MarkdownTemplate
            {
                Id = "default",
                Name = "Default",
                Content = GetDefaultTemplateContent()
            };

            _templates["detailed"] = new MarkdownTemplate
            {
                Id = "detailed",
                Name = "Detailed",
                Content = GetDetailedTemplateContent()
            };

            _templates["minimal"] = new MarkdownTemplate
            {
                Id = "minimal", 
                Name = "Minimal",
                Content = GetMinimalTemplateContent()
            };
        }

        private MarkdownTemplate GetTemplate(string templateId)
        {
            return _templates.ContainsKey(templateId) ? _templates[templateId] : _templates["default"];
        }

        private string GetDefaultTemplateContent()
        {
            return @"# {{project_name}}

{{project_description}}

**Version:** {{project_version}}  
**Generated:** {{export_date}} {{export_time}}  
**Generator:** {{generator}}  

---

";
        }

        private string GetDetailedTemplateContent()
        {
            return @"# 📋 {{project_name}}

## Project Overview

{{project_description}}

## Project Information

| Property | Value |
|----------|-------|
| **Version** | {{project_version}} |
| **Team** | {{team_name}} |
| **Contact** | {{contact_email}} |
| **Repository** | {{repository_url}} |
| **Generated** | {{export_date}} {{export_time}} |
| **Generator** | {{generator}} |
| **Sections** | {{section_count}} |

---

";
        }

        private string GetMinimalTemplateContent()
        {
            return @"# {{project_name}}

{{project_description}}

---

";
        }

        private class MarkdownTemplate
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Content { get; set; }
        }

        private class ExportOption
        {
            public string Key { get; set; }
            public string DisplayName { get; set; }
            public object DefaultValue { get; set; }
            public string Description { get; set; }

            public ExportOption(string key, string displayName, object defaultValue, string description)
            {
                Key = key;
                DisplayName = displayName;
                DefaultValue = defaultValue;
                Description = description;
            }
        }
    }
}