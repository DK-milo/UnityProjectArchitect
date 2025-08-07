using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using UnityProjectArchitect.Core;
using UnityProjectArchitect.AI;

namespace UnityProjectArchitect.Services
{
    public class PDFExporter : IExportFormatter
    {
        public ExportFormat SupportedFormat => ExportFormat.PDF;
        public string DisplayName => "PDF";
        public string FileExtension => "pdf";
        public string MimeType => "application/pdf";

        private readonly MarkdownExporter _markdownExporter;
        private readonly Dictionary<string, PDFTemplate> _templates;
        private readonly UnityWebPDFGenerator _webPdfGenerator;

        public PDFExporter()
        {
            _markdownExporter = new MarkdownExporter();
            _templates = new Dictionary<string, PDFTemplate>();
            _webPdfGenerator = new UnityWebPDFGenerator();
            InitializeDefaultTemplates();
        }

        public async Task<ExportOperationResult> FormatAsync(ExportContent content, ExportOptions options)
        {
            ExportOperationResult result = new ExportOperationResult(ExportFormat.PDF, "");
            DateTime startTime = DateTime.Now;

            try
            {

                // Step 1: Generate Markdown content
                ExportOptions markdownOptions = CreateMarkdownOptions(options);
                ExportOperationResult markdownResult = await _markdownExporter.FormatAsync(content, markdownOptions);
                
                if (!markdownResult.Success)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Markdown generation failed: {markdownResult.ErrorMessage}";
                    return result;
                }

                string markdownContent = markdownResult.Metadata["content"] as string ?? "";
                
                // Step 2: Convert Markdown to HTML
                string htmlContent = await ConvertMarkdownToHtmlAsync(markdownContent, options);
                
                // Step 3: Apply PDF-specific formatting and styling
                string styledHtml = await ApplyPDFStylingAsync(htmlContent, content, options);
                
                // Step 4: Generate PDF metadata
                PDFMetadata pdfMetadata = GeneratePDFMetadata(content, options);
                
                // Step 5: Generate print-ready HTML using Unity web rendering
                string baseOutputPath = GetOption<string>(options, "OutputPath", null) ?? Path.GetTempPath();
                string filename = $"{content.Title ?? "Documentation"}_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                string outputPath = Path.Combine(baseOutputPath, filename);
                
                UnityWebPDFGenerator.PDFGenerationOptions pdfOptions = new UnityWebPDFGenerator.PDFGenerationOptions
                {
                    Title = pdfMetadata.Title,
                    Author = pdfMetadata.Author,
                    UseSystemBrowser = GetOption<bool>(options, "UseSystemBrowser", true),
                    OutputFormat = "html"
                };

                UnityWebPDFGenerator.PDFGenerationResult pdfResult = await _webPdfGenerator.GeneratePDFFromHtmlAsync(styledHtml, outputPath, pdfOptions);
                
                if (pdfResult.Success)
                {
                    result.Success = true;
                    result.GeneratedFiles.Add(outputPath);
                    result.TotalSizeBytes = pdfResult.FileSizeBytes;
                    result.ExportTime = DateTime.Now - startTime;
                    result.Metadata["pdf_path"] = outputPath;
                    result.Metadata["processing_time"] = pdfResult.ProcessingTime;
                    result.Metadata["html_instructions"] = pdfResult.Instructions;
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = $"PDF generation failed: {pdfResult.ErrorMessage}";
                    Debug.LogError($"PDFExporter HTML generation error: {pdfResult.ErrorMessage}");
                }

                result.Statistics = GenerateStatistics(content, markdownContent, styledHtml);
                result.Metadata["pdf_metadata"] = pdfMetadata;
                result.Metadata["markdown_content"] = markdownContent;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"PDF export failed: {ex.Message}";
                Debug.LogError($"PDFExporter error: {ex}");
            }

            return result;
        }

        public ValidationResult ValidateContent(ExportContent content)
        {
            ValidationResult validation = new ValidationResult { IsValid = true };

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

            // PDF-specific validations
            int totalWordCount = 0;
            if (content.Sections != null)
            {
                foreach (DocumentationSectionData section in content.Sections)
                {
                    totalWordCount += section.CurrentWordCount;
                }
            }

            if (totalWordCount > 50000)
            {
                validation.Warnings.Add("Large document detected - PDF generation may take longer");
            }

            return validation;
        }

        public ExportTemplate GetDefaultTemplate()
        {
            return new ExportTemplate("default", "Default PDF", ExportFormat.PDF)
            {
                Description = "Standard PDF documentation template with professional styling using HTML-to-PDF conversion",
                Author = "Unity Project Architect",
                Version = "2.0",
                TemplateContent = GetDefaultTemplateContent()
            };
        }

        public List<ExportOption> GetAvailableOptions()
        {
            return new List<ExportOption>
            {
                new ExportOption { Name = "TemplateId", Description = "Template to use for PDF export", IsEnabled = true },
                new ExportOption { Name = "IncludeHeaderFooter", Description = "Add header and footer to pages", IsEnabled = true },
                new ExportOption { Name = "IncludePageNumbers", Description = "Add page numbers", IsEnabled = true },
                new ExportOption { Name = "IncludeTOC", Description = "Generate table of contents", IsEnabled = true },
                new ExportOption { Name = "FontFamily", Description = "Font family for body text", IsEnabled = true },
                new ExportOption { Name = "FontSize", Description = "Base font size in points", IsEnabled = true }
            };
        }

        private ExportOptions CreateMarkdownOptions(ExportOptions pdfOptions)
        {
            ExportOptions markdownOptions = new ExportOptions
            {
                IncludeTableOfContents = pdfOptions.IncludeTableOfContents,
                IncludeTimestamp = pdfOptions.IncludeTimestamp,
                IncludeMetadata = pdfOptions.IncludeMetadata,
                IncludeDiagrams = pdfOptions.IncludeDiagrams,
                IncludeCodeExamples = pdfOptions.IncludeCodeExamples,
                HeaderText = pdfOptions.HeaderText,
                FooterText = pdfOptions.FooterText,
                FormatSpecificOptions = new Dictionary<string, object>
                {
                    ["UseEmoji"] = GetOption<bool>(pdfOptions, "UseEmoji", false), // Disable emoji for PDF
                    ["IncludeCodeBlocks"] = true,
                    ["GenerateTOC"] = GetOption<bool>(pdfOptions, "IncludeTOC", true)
                }
            };

            return markdownOptions;
        }

        private async Task<string> ConvertMarkdownToHtmlAsync(string markdownContent, ExportOptions options)
        {
            if (string.IsNullOrEmpty(markdownContent))
                return "<html><body><p>No content to export</p></body></html>";

            // Basic Markdown to HTML conversion
            StringBuilder html = new StringBuilder();
            string[] lines = markdownContent.Split('\n');
            bool inCodeBlock = false;
            bool inList = false;

            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                
                if (string.IsNullOrEmpty(trimmed))
                {
                    if (!inCodeBlock)
                    {
                        if (inList)
                        {
                            html.AppendLine("</ul>");
                            inList = false;
                        }
                        html.AppendLine("<br>");
                    }
                    else
                    {
                        html.AppendLine();
                    }
                    continue;
                }

                // Code blocks
                if (trimmed.StartsWith("```"))
                {
                    if (inCodeBlock)
                    {
                        html.AppendLine("</code></pre>");
                        inCodeBlock = false;
                    }
                    else
                    {
                        string lang = trimmed.Length > 3 ? trimmed.Substring(3) : "";
                        html.AppendLine($"<pre class=\"code-block\"><code class=\"language-{lang}\">");
                        inCodeBlock = true;
                    }
                    continue;
                }

                if (inCodeBlock)
                {
                    html.AppendLine(EscapeHtml(line));
                    continue;
                }

                // Headers
                if (trimmed.StartsWith("# "))
                {
                    html.AppendLine($"<h1>{ProcessInlineMarkdown(trimmed.Substring(2))}</h1>");
                }
                else if (trimmed.StartsWith("## "))
                {
                    html.AppendLine($"<h2>{ProcessInlineMarkdown(trimmed.Substring(3))}</h2>");
                }
                else if (trimmed.StartsWith("### "))
                {
                    html.AppendLine($"<h3>{ProcessInlineMarkdown(trimmed.Substring(4))}</h3>");
                }
                else if (trimmed.StartsWith("#### "))
                {
                    html.AppendLine($"<h4>{ProcessInlineMarkdown(trimmed.Substring(5))}</h4>");
                }
                // Lists
                else if (trimmed.StartsWith("- ") || trimmed.StartsWith("* "))
                {
                    if (!inList)
                    {
                        html.AppendLine("<ul>");
                        inList = true;
                    }
                    html.AppendLine($"<li>{ProcessInlineMarkdown(trimmed.Substring(2))}</li>");
                }
                // Horizontal rule
                else if (trimmed == "---")
                {
                    if (inList)
                    {
                        html.AppendLine("</ul>");
                        inList = false;
                    }
                    html.AppendLine("<hr>");
                }
                // Regular paragraph
                else
                {
                    if (inList)
                    {
                        html.AppendLine("</ul>");
                        inList = false;
                    }
                    html.AppendLine($"<p>{ProcessInlineMarkdown(trimmed)}</p>");
                }
            }

            if (inCodeBlock)
            {
                html.AppendLine("</code></pre>");
            }
            if (inList)
            {
                html.AppendLine("</ul>");
            }

            return html.ToString();
        }

        private string ProcessInlineMarkdown(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            // Bold
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\*\*(.*?)\*\*", "<strong>$1</strong>");
            
            // Italic
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\*(.*?)\*", "<em>$1</em>");
            
            // Code
            text = System.Text.RegularExpressions.Regex.Replace(text, @"`(.*?)`", "<code>$1</code>");
            
            // Links
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\[(.*?)\]\((.*?)\)", "<a href=\"$2\">$1</a>");

            return EscapeHtml(text);
        }

        private string EscapeHtml(string text)
        {
            return text?.Replace("&", "&amp;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;") ?? "";
        }

        private async Task<string> ApplyPDFStylingAsync(string htmlContent, ExportContent content, ExportOptions options)
        {
            string templateId = GetOption<string>(options, "TemplateId", "default");
            PDFTemplate template = GetTemplate(templateId);
            
            StringBuilder styledHtml = new StringBuilder();
            
            // HTML document structure
            styledHtml.AppendLine("<!DOCTYPE html>");
            styledHtml.AppendLine("<html>");
            styledHtml.AppendLine("<head>");
            styledHtml.AppendLine("<meta charset=\"UTF-8\">");
            styledHtml.AppendLine($"<title>{content.Title ?? "Documentation"}</title>");
            styledHtml.AppendLine("<style>");
            styledHtml.AppendLine(await GenerateCSSAsync(options, template));
            styledHtml.AppendLine("</style>");
            styledHtml.AppendLine("</head>");
            styledHtml.AppendLine("<body>");
            
            // Header
            if (GetOption<bool>(options, "IncludeHeaderFooter", true))
            {
                styledHtml.AppendLine("<div class=\"header\">");
                styledHtml.AppendLine($"<h1>{content.Title ?? "Documentation"}</h1>");
                if (content.ProjectData != null)
                {
                    styledHtml.AppendLine($"<p>Version: {content.ProjectData.ProjectVersion}</p>");
                }
                styledHtml.AppendLine("</div>");
            }

            // Main content
            styledHtml.AppendLine("<div class=\"content\">");
            styledHtml.AppendLine(htmlContent);
            styledHtml.AppendLine("</div>");

            // Footer
            if (GetOption<bool>(options, "IncludeHeaderFooter", true))
            {
                styledHtml.AppendLine("<div class=\"footer\">");
                styledHtml.AppendLine($"<p>Generated by Unity Project Architect on {DateTime.Now:yyyy-MM-dd}</p>");
                styledHtml.AppendLine("</div>");
            }

            styledHtml.AppendLine("</body>");
            styledHtml.AppendLine("</html>");

            return styledHtml.ToString();
        }

        private async Task<string> GenerateCSSAsync(ExportOptions options, PDFTemplate template)
        {
            StringBuilder css = new StringBuilder();
            
            string fontFamily = GetOption<string>(options, "FontFamily", "Arial");
            string fontSize = GetOption<string>(options, "FontSize", "11");

            css.AppendLine("body {");
            css.AppendLine($"  font-family: {fontFamily}, sans-serif;");
            css.AppendLine($"  font-size: {fontSize}pt;");
            css.AppendLine("  line-height: 1.6;");
            css.AppendLine("  color: #333;");
            css.AppendLine("}");

            css.AppendLine("h1, h2, h3, h4, h5, h6 { color: #2c3e50; margin-top: 1.5em; margin-bottom: 0.5em; }");
            css.AppendLine("h1 { font-size: 24pt; border-bottom: 2px solid #3498db; padding-bottom: 0.3em; }");
            css.AppendLine("h2 { font-size: 20pt; }");
            css.AppendLine("h3 { font-size: 16pt; }");
            css.AppendLine("h4 { font-size: 14pt; }");

            css.AppendLine("p { margin: 1em 0; text-align: justify; }");
            css.AppendLine("ul, ol { margin: 1em 0; padding-left: 2em; }");
            css.AppendLine("li { margin: 0.5em 0; }");

            css.AppendLine(".code-block {");
            css.AppendLine("  background-color: #f8f9fa;");
            css.AppendLine("  border: 1px solid #e9ecef;");
            css.AppendLine("  border-radius: 4px;");
            css.AppendLine("  padding: 1em;");
            css.AppendLine("  margin: 1em 0;");
            css.AppendLine("  font-family: 'Courier New', monospace;");
            css.AppendLine("  font-size: 9pt;");
            css.AppendLine("  overflow-x: auto;");
            css.AppendLine("}");

            css.AppendLine("code { background-color: #f8f9fa; padding: 0.2em 0.4em; border-radius: 3px; font-family: 'Courier New', monospace; }");
            css.AppendLine("strong { font-weight: bold; }");
            css.AppendLine("em { font-style: italic; }");
            css.AppendLine("hr { border: none; border-top: 1px solid #ccc; margin: 2em 0; }");

            css.AppendLine(".header { text-align: center; margin-bottom: 2em; }");
            css.AppendLine(".footer { text-align: center; margin-top: 2em; font-size: 9pt; color: #666; }");

            // Add template-specific styles
            if (!string.IsNullOrEmpty(template.CSS))
            {
                css.AppendLine();
                css.AppendLine("/* Template-specific styles */");
                css.AppendLine(template.CSS);
            }

            return css.ToString();
        }

        private PDFMetadata GeneratePDFMetadata(ExportContent content, ExportOptions options)
        {
            return new PDFMetadata
            {
                Title = content.Title ?? "Documentation",
                Author = content.ProjectData?.TeamName ?? "Unity Project Architect",
                Subject = content.ProjectData?.ProjectDescription ?? "Project Documentation",
                Keywords = $"Unity, Documentation, {content.ProjectData?.ProjectType}",
                Creator = "Unity Project Architect",
                Producer = "Unity Project Architect PDF Exporter",
                CreationDate = DateTime.Now,
                ModificationDate = DateTime.Now
            };
        }

        private ExportStatistics GenerateStatistics(ExportContent content, string markdownContent, string htmlContent)
        {
            ExportStatistics stats = new ExportStatistics();
            
            if (content.Sections != null)
            {
                stats.TotalSections = content.Sections.Count;
                stats.ExportedSections = content.Sections.Where(s => s.IsEnabled && s.HasContent).Count();
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

        private void InitializeDefaultTemplates()
        {
            _templates["default"] = new PDFTemplate
            {
                Id = "default",
                Name = "Default",
                CSS = ""
            };

            _templates["professional"] = new PDFTemplate
            {
                Id = "professional",
                Name = "Professional",
                CSS = @"
                    h1 { color: #1a365d; }
                    h2 { color: #2d3748; }
                    .header { border-bottom: 3px solid #3182ce; }
                    .code-block { background-color: #edf2f7; }
                "
            };

            _templates["minimal"] = new PDFTemplate
            {
                Id = "minimal",
                Name = "Minimal",
                CSS = @"
                    h1, h2, h3, h4, h5, h6 { color: #000; }
                    h1 { border-bottom: 1px solid #ccc; }
                    .header { border: none; }
                "
            };
        }

        private PDFTemplate GetTemplate(string templateId)
        {
            return _templates.ContainsKey(templateId) ? _templates[templateId] : _templates["default"];
        }

        private string GetDefaultTemplateContent()
        {
            return "Default PDF template with professional styling and layout using HTML-to-PDF conversion via browser printing.";
        }

        private class PDFTemplate
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string CSS { get; set; } = "";
        }

        private class PDFMetadata
        {
            public string Title { get; set; } = "";
            public string Author { get; set; } = "";
            public string Subject { get; set; } = "";
            public string Keywords { get; set; } = "";
            public string Creator { get; set; } = "";
            public string Producer { get; set; } = "";
            public DateTime CreationDate { get; set; }
            public DateTime ModificationDate { get; set; }
        }
    }
}