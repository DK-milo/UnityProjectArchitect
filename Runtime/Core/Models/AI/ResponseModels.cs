using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Core.AI
{
    [Serializable]
    public class ParsedResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string RawContent { get; set; }
        public string CleanedContent { get; set; }
        public DocumentationSectionType SectionType { get; set; }
        public List<ContentSection> Sections { get; set; }
        public List<string> Headings { get; set; }
        public List<ListItem> Lists { get; set; }
        public List<CodeBlock> CodeBlocks { get; set; }
        public List<TableData> Tables { get; set; }
        public List<LinkData> Links { get; set; }
        public int WordCount { get; set; }
        public TimeSpan EstimatedReadingTime { get; set; }
        public ContentQualityScore QualityScore { get; set; }
        public List<string> ValidationIssues { get; set; }
        public bool IsValidated { get; set; }
        public int TokensUsed { get; set; }
        public DateTime ProcessingTime { get; set; }

        public ParsedResponse()
        {
            Sections = new List<ContentSection>();
            Headings = new List<string>();
            Lists = new List<ListItem>();
            CodeBlocks = new List<CodeBlock>();
            Tables = new List<TableData>();
            Links = new List<LinkData>();
            ValidationIssues = new List<string>();
            QualityScore = new ContentQualityScore();
            ProcessingTime = DateTime.Now;
        }

        public bool HasValidationIssues => ValidationIssues != null && ValidationIssues.Count > 0;
        public bool IsHighQuality => QualityScore.OverallScore >= 0.7f;
        public bool IsWellStructured => Headings.Count > 0 && Sections.Count > 1;
        public bool HasRichContent => CodeBlocks.Count > 0 || Tables.Count > 0 || Lists.Count > 2;
    }

    [Serializable]
    public class ContentSection
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int Level { get; set; }
        public int StartIndex { get; set; }
        public int Length { get; set; }
        public List<string> Keywords { get; set; }
        public SectionType Type { get; set; }

        public ContentSection()
        {
            Keywords = new List<string>();
            Type = SectionType.General;
        }

        public int WordCount => string.IsNullOrEmpty(Content) ? 0 : 
            Content.Split(new char[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;

        public bool IsMainSection => Level == 1;
        public bool IsSubSection => Level > 1;
        public bool HasContent => !string.IsNullOrEmpty(Content) && WordCount > 10;
    }

    [Serializable]
    public class ListItem
    {
        public string Content { get; set; }
        public ListType Type { get; set; }
        public int Index { get; set; }
        public int Level { get; set; }
        public List<ListItem> SubItems { get; set; }

        public ListItem()
        {
            SubItems = new List<ListItem>();
            Level = 0;
        }

        public bool HasSubItems => SubItems != null && SubItems.Count > 0;
        public bool IsNested => Level > 0;
    }

    [Serializable]
    public class CodeBlock
    {
        public string Language { get; set; }
        public string Code { get; set; }
        public int StartIndex { get; set; }
        public int Length { get; set; }
        public List<string> Comments { get; set; }
        public CodeBlockType Type { get; set; }

        public CodeBlock()
        {
            Comments = new List<string>();
            Type = CodeBlockType.Code;
        }

        public int LineCount => string.IsNullOrEmpty(Code) ? 0 : Code.Split('\n').Length;
        public bool HasLanguage => !string.IsNullOrEmpty(Language) && Language != "text";
        public bool IsEmpty => string.IsNullOrEmpty(Code) || Code.Trim().Length == 0;
    }

    [Serializable]
    public class TableData
    {
        public List<string> Headers { get; set; }
        public List<List<string>> Rows { get; set; }
        public string Caption { get; set; }
        public TableType Type { get; set; }

        public TableData()
        {
            Headers = new List<string>();
            Rows = new List<List<string>>();
            Type = TableType.Data;
        }

        public int ColumnCount => Headers.Count;
        public int RowCount => Rows.Count;
        public bool HasHeaders => Headers.Count > 0;
        public bool HasData => Rows.Count > 0;
        public bool IsValid => HasHeaders && HasData && Rows.All(row => row.Count == ColumnCount);
    }

    [Serializable]
    public class LinkData
    {
        public string Text { get; set; }
        public string Url { get; set; }
        public int StartIndex { get; set; }
        public int Length { get; set; }
        public LinkType Type { get; set; }
        public bool IsExternal { get; set; }

        public LinkData()
        {
            Type = LinkType.Unknown;
        }

        public bool IsValid => !string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(Url);
        public bool IsInternal => !IsExternal;
    }

    [Serializable]
    public class ContentQualityScore
    {
        public float HasStructure { get; set; }
        public float HasLists { get; set; }
        public float HasCodeExamples { get; set; }
        public float HasTables { get; set; }
        public float HasLinks { get; set; }
        public float WordCountScore { get; set; }
        public float ReadabilityScore { get; set; }
        public float OverallScore { get; set; }
        public List<string> StrengthAreas { get; set; }
        public List<string> ImprovementAreas { get; set; }

        public ContentQualityScore()
        {
            StrengthAreas = new List<string>();
            ImprovementAreas = new List<string>();
        }

        public string GetQualityRating()
        {
            if (OverallScore >= 0.9f) return "Excellent";
            if (OverallScore >= 0.7f) return "Good";
            if (OverallScore >= 0.5f) return "Fair";
            if (OverallScore >= 0.3f) return "Poor";
            return "Very Poor";
        }

        public bool IsAcceptable => OverallScore >= 0.5f;
        public bool IsHighQuality => OverallScore >= 0.7f;
    }

    public enum ListType
    {
        Bullet,
        Numbered,
        Checklist,
        Definition
    }

    public enum CodeBlockType
    {
        Code,
        Configuration,
        Data,
        Script,
        Markup,
        Query
    }

    public enum TableType
    {
        Data,
        Comparison,
        Configuration,
        Results,
        Specification
    }

    public enum LinkType
    {
        Unknown,
        Documentation,
        Reference,
        Download,
        External,
        Internal,
        Image,
        Video
    }

    public enum SectionType
    {
        General,
        Introduction,
        Implementation,
        Configuration,
        Examples,
        References,
        Appendix
    }

    [Serializable]
    public class AIProvider
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string ApiUrl { get; set; }
        public bool IsSupported { get; set; }
        public AIProviderType Type { get; set; }

        public static readonly AIProvider Claude = new AIProvider
        {
            Name = "claude",
            DisplayName = "Claude (Anthropic)",
            ApiUrl = "https://api.anthropic.com/v1/messages",
            IsSupported = true,
            Type = AIProviderType.Claude
        };

        public static readonly AIProvider OpenAI = new AIProvider
        {
            Name = "openai",
            DisplayName = "OpenAI GPT",
            ApiUrl = "https://api.openai.com/v1/chat/completions",
            IsSupported = false,
            Type = AIProviderType.OpenAI
        };

        public static List<AIProvider> GetAllProviders()
        {
            return new List<AIProvider> { Claude, OpenAI };
        }

        public static List<AIProvider> GetSupportedProviders()
        {
            return GetAllProviders().Where(p => p.IsSupported).ToList();
        }
    }

    public enum AIProviderType
    {
        Claude,
        OpenAI,
        Custom
    }

    [Serializable]
    public class AIConfiguration
    {
        public AIProvider Provider { get; set; }
        public string ApiKey { get; set; }
        public string Model { get; set; }
        public int MaxTokens { get; set; } = 4000;
        public float Temperature { get; set; } = 0.7f;
        public int TimeoutSeconds { get; set; } = 60;
        public int MaxRetries { get; set; } = 3;
        public bool EnableLogging { get; set; } = true;
        public Dictionary<string, object> CustomSettings { get; set; }

        public AIConfiguration()
        {
            Provider = AIProvider.Claude;
            CustomSettings = new Dictionary<string, object>();
        }

        public AIConfiguration(AIProvider provider, string apiKey) : this()
        {
            Provider = provider;
            ApiKey = apiKey;
        }

        public bool IsValid()
        {
            return Provider != null && 
                   !string.IsNullOrEmpty(ApiKey) && 
                   !string.IsNullOrEmpty(Model) &&
                   MaxTokens > 0 && 
                   TimeoutSeconds > 0;
        }

        public Dictionary<string, string> GetHeaders()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            };

            if (Provider.Type == AIProviderType.Claude)
            {
                headers.Add("x-api-key", ApiKey);
                headers.Add("anthropic-version", "2023-06-01");
            }
            else if (Provider.Type == AIProviderType.OpenAI)
            {
                headers.Add("Authorization", $"Bearer {ApiKey}");
            }

            return headers;
        }
    }

    public static class ParsedResponseExtensions
    {
        public static string GetSummary(this ParsedResponse response)
        {
            if (response == null || !response.Success)
                return "Failed to parse response";

            List<string> summary = new List<string>();
            summary.Add($"📄 {response.WordCount:N0} words");
            summary.Add($"⏱️ {response.EstimatedReadingTime.TotalMinutes:F0} min read");
            summary.Add($"📊 Quality: {response.QualityScore.GetQualityRating()}");

            if (response.Headings.Count > 0)
                summary.Add($"📋 {response.Headings.Count} sections");

            if (response.CodeBlocks.Count > 0)
                summary.Add($"💻 {response.CodeBlocks.Count} code blocks");

            if (response.Tables.Count > 0)
                summary.Add($"📊 {response.Tables.Count} tables");

            if (response.Links.Count > 0)
                summary.Add($"🔗 {response.Links.Count} links");

            return string.Join(" • ", summary);
        }

        public static List<string> GetKeyTopics(this ParsedResponse response, int maxTopics = 5)
        {
            if (response == null || !response.Success)
                return new List<string>();

            List<string> topics = new List<string>();
            topics.AddRange(response.Headings.Take(maxTopics));

            if (topics.Count < maxTopics)
            {
                List<string> listItems = response.Lists
                    .Where(l => !string.IsNullOrEmpty(l.Content))
                    .Select(l => l.Content.Split(' ').FirstOrDefault())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Take(maxTopics - topics.Count)
                    .ToList();
                topics.AddRange(listItems);
            }

            return topics.Take(maxTopics).ToList();
        }

        public static bool IsComprehensive(this ParsedResponse response)
        {
            if (response == null || !response.Success)
                return false;

            return response.WordCount >= 200 &&
                   response.Headings.Count >= 2 &&
                   response.QualityScore.OverallScore >= 0.6f &&
                   !response.HasValidationIssues;
        }

        public static string GetFormattedReport(this ParsedResponse response)
        {
            if (response == null)
                return "No response data available";

            if (!response.Success)
                return $"❌ Parsing failed: {response.ErrorMessage}";

            string report = "🤖 AI Content Analysis Report\n";
            report += "═══════════════════════════════\n\n";

            report += $"📊 **Overview**\n";
            report += $"   • Content Length: {response.WordCount:N0} words\n";
            report += $"   • Reading Time: ~{response.EstimatedReadingTime.TotalMinutes:F0} minutes\n";
            report += $"   • Quality Score: {response.QualityScore.OverallScore:P1} ({response.QualityScore.GetQualityRating()})\n";
            report += $"   • Structure: {response.Headings.Count} headings, {response.Sections.Count} sections\n\n";

            report += $"📋 **Content Elements**\n";
            report += $"   • Lists: {response.Lists.Count}\n";
            report += $"   • Code Blocks: {response.CodeBlocks.Count}\n";
            report += $"   • Tables: {response.Tables.Count}\n";
            report += $"   • Links: {response.Links.Count}\n\n";

            if (response.HasValidationIssues)
            {
                report += $"⚠️ **Validation Issues** ({response.ValidationIssues.Count})\n";
                foreach (string issue in response.ValidationIssues.Take(5))
                {
                    report += $"   • {issue}\n";
                }
                if (response.ValidationIssues.Count > 5)
                {
                    report += $"   • ... and {response.ValidationIssues.Count - 5} more issues\n";
                }
                report += "\n";
            }

            List<string> keyTopics = response.GetKeyTopics();
            if (keyTopics.Count > 0)
            {
                report += $"🔑 **Key Topics**\n";
                foreach (string topic in keyTopics)
                {
                    report += $"   • {topic}\n";
                }
                report += "\n";
            }

            report += $"✅ **Status**: {(response.IsComprehensive() ? "Comprehensive" : "Basic")} content generated";

            return report;
        }
    }
}