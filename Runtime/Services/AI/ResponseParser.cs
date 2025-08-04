using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityProjectArchitect.Core;
using UnityProjectArchitect.Core.AI;

namespace UnityProjectArchitect.Services.AI
{
    public class ResponseParser
    {
        private static readonly Dictionary<string, string> SectionDelimiters = new Dictionary<string, string>
        {
            { "markdown", "```" },
            { "code", "```" },
            { "json", "```json" },
            { "yaml", "```yaml" },
            { "xml", "```xml" }
        };

        private static readonly Regex MarkdownHeadingRegex = new Regex(@"^#{1,6}\s+(.+)$", RegexOptions.Multiline);
        private static readonly Regex ListItemRegex = new Regex(@"^[\*\-\+]\s+(.+)$", RegexOptions.Multiline);
        private static readonly Regex NumberedListRegex = new Regex(@"^\d+\.\s+(.+)$", RegexOptions.Multiline);
        private static readonly Regex CodeBlockRegex = new Regex(@"```(\w+)?\n(.*?)\n```", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        public static ParsedResponse ParseClaudeResponse(ClaudeAPIResponse response, DocumentationSectionType sectionType)
        {
            ParsedResponse result = new ParsedResponse();

            try
            {
                if (response == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "Response is null";
                    return result;
                }

                if (!response.IsSuccess)
                {
                    result.Success = false;
                    result.ErrorMessage = response.error?.message ?? "Unknown API error";
                    return result;
                }

                if (!response.HasContent)
                {
                    result.Success = false;
                    result.ErrorMessage = "Response contains no content";
                    return result;
                }

                string rawContent = response.GetTextContent();
                if (string.IsNullOrEmpty(rawContent))
                {
                    result.Success = false;
                    result.ErrorMessage = "Response content is empty";
                    return result;
                }

                result = ParseContent(rawContent, sectionType);
                result.TokensUsed = response.usage?.TotalTokens ?? 0;
                result.ProcessingTime = DateTime.Now;
                result.Success = true;

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Error parsing response: {ex.Message}";
                Debug.LogError($"ResponseParser error: {ex}");
                return result;
            }
        }

        public static ParsedResponse ParseContent(string rawContent, DocumentationSectionType sectionType)
        {
            ParsedResponse result = new ParsedResponse
            {
                RawContent = rawContent,
                SectionType = sectionType
            };

            try
            {
                result.CleanedContent = CleanContent(rawContent);
                result.Sections = ExtractSections(result.CleanedContent);
                result.Headings = ExtractHeadings(result.CleanedContent);
                result.Lists = ExtractLists(result.CleanedContent);
                result.CodeBlocks = ExtractCodeBlocks(result.CleanedContent);
                result.Tables = ExtractTables(result.CleanedContent);
                result.Links = ExtractLinks(result.CleanedContent);
                
                result.WordCount = CountWords(result.CleanedContent);
                result.EstimatedReadingTime = CalculateReadingTime(result.WordCount);
                result.QualityScore = CalculateQualityScore(result);
                
                ValidateContent(result, sectionType);
                
                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Error parsing content: {ex.Message}";
                Debug.LogError($"Content parsing error: {ex}");
                return result;
            }
        }

        private static string CleanContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            content = content.Trim();
            
            content = Regex.Replace(content, @"\r\n", "\n");
            content = Regex.Replace(content, @"\r", "\n");
            
            content = Regex.Replace(content, @"\n{3,}", "\n\n");
            
            content = Regex.Replace(content, @"[ \t]+", " ");
            
            content = Regex.Replace(content, @"[ \t]*\n", "\n");

            return content;
        }

        private static List<ContentSection> ExtractSections(string content)
        {
            List<ContentSection> sections = new List<ContentSection>();
            
            MatchCollection headingMatches = MarkdownHeadingRegex.Matches(content);
            
            for (int i = 0; i < headingMatches.Count; i++)
            {
                Match currentMatch = headingMatches[i];
                Match nextMatch = i < headingMatches.Count - 1 ? headingMatches[i + 1] : null;
                
                int startIndex = currentMatch.Index;
                int endIndex = nextMatch?.Index ?? content.Length;
                
                string sectionContent = content.Substring(startIndex, endIndex - startIndex).Trim();
                
                ContentSection section = new ContentSection
                {
                    Title = currentMatch.Groups[1].Value.Trim(),
                    Content = sectionContent,
                    Level = currentMatch.Value.TakeWhile(c => c == '#').Count(),
                    StartIndex = startIndex,
                    Length = sectionContent.Length
                };
                
                sections.Add(section);
            }

            if (sections.Count == 0)
            {
                sections.Add(new ContentSection
                {
                    Title = "Main Content",
                    Content = content,
                    Level = 1,
                    StartIndex = 0,
                    Length = content.Length
                });
            }

            return sections;
        }

        private static List<string> ExtractHeadings(string content)
        {
            List<string> headings = new List<string>();
            
            MatchCollection matches = MarkdownHeadingRegex.Matches(content);
            foreach (Match match in matches)
            {
                headings.Add(match.Groups[1].Value.Trim());
            }
            
            return headings;
        }

        private static List<ListItem> ExtractLists(string content)
        {
            List<ListItem> lists = new List<ListItem>();
            
            MatchCollection bulletMatches = ListItemRegex.Matches(content);
            foreach (Match match in bulletMatches)
            {
                lists.Add(new ListItem
                {
                    Content = match.Groups[1].Value.Trim(),
                    Type = ListType.Bullet,
                    Index = match.Index
                });
            }
            
            MatchCollection numberedMatches = NumberedListRegex.Matches(content);
            foreach (Match match in numberedMatches)
            {
                lists.Add(new ListItem
                {
                    Content = match.Groups[1].Value.Trim(),
                    Type = ListType.Numbered,
                    Index = match.Index
                });
            }
            
            return lists.OrderBy(l => l.Index).ToList();
        }

        private static List<CodeBlock> ExtractCodeBlocks(string content)
        {
            List<CodeBlock> codeBlocks = new List<CodeBlock>();
            
            MatchCollection matches = CodeBlockRegex.Matches(content);
            foreach (Match match in matches)
            {
                codeBlocks.Add(new CodeBlock
                {
                    Language = match.Groups[1].Success ? match.Groups[1].Value : "text",
                    Code = match.Groups[2].Value.Trim(),
                    StartIndex = match.Index,
                    Length = match.Length
                });
            }
            
            return codeBlocks;
        }

        private static List<TableData> ExtractTables(string content)
        {
            List<TableData> tables = new List<TableData>();
            
            string[] lines = content.Split('\n');
            List<int> tableStarts = new List<int>();
            
            for (int i = 0; i < lines.Length - 1; i++)
            {
                if (lines[i].Contains('|') && lines[i + 1].Contains('|') && 
                    lines[i + 1].Contains('-'))
                {
                    tableStarts.Add(i);
                }
            }
            
            foreach (int start in tableStarts)
            {
                TableData table = ParseMarkdownTable(lines.Skip(start).ToArray());
                if (table.Rows.Count > 0)
                {
                    tables.Add(table);
                }
            }
            
            return tables;
        }

        private static TableData ParseMarkdownTable(string[] lines)
        {
            TableData table = new TableData();
            
            if (lines.Length < 2)
                return table;
            
            table.Headers = lines[0].Split('|')
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .Select(h => h.Trim())
                .ToList();
            
            for (int i = 2; i < lines.Length; i++)
            {
                if (!lines[i].Contains('|'))
                    break;
                
                List<string> row = lines[i].Split('|')
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Select(c => c.Trim())
                    .ToList();
                
                if (row.Count > 0)
                {
                    table.Rows.Add(row);
                }
            }
            
            return table;
        }

        private static List<LinkData> ExtractLinks(string content)
        {
            List<LinkData> links = new List<LinkData>();
            
            Regex linkRegex = new Regex(@"\[([^\]]+)\]\(([^\)]+)\)", RegexOptions.IgnoreCase);
            MatchCollection matches = linkRegex.Matches(content);
            
            foreach (Match match in matches)
            {
                links.Add(new LinkData
                {
                    Text = match.Groups[1].Value,
                    Url = match.Groups[2].Value,
                    StartIndex = match.Index,
                    Length = match.Length
                });
            }
            
            return links;
        }

        private static int CountWords(string content)
        {
            if (string.IsNullOrEmpty(content))
                return 0;
            
            string cleanContent = Regex.Replace(content, @"[^\w\s]", " ");
            string[] words = cleanContent.Split(new char[] { ' ', '\n', '\r', '\t' }, 
                StringSplitOptions.RemoveEmptyEntries);
            
            return words.Length;
        }

        private static TimeSpan CalculateReadingTime(int wordCount)
        {
            const int wordsPerMinute = 200;
            double minutes = (double)wordCount / wordsPerMinute;
            return TimeSpan.FromMinutes(Math.Max(1, Math.Ceiling(minutes)));
        }

        private static ContentQualityScore CalculateQualityScore(ParsedResponse response)
        {
            ContentQualityScore score = new ContentQualityScore();
            
            score.HasStructure = response.Headings.Count > 0 ? 1.0f : 0.0f;
            score.HasLists = response.Lists.Count > 0 ? 1.0f : 0.0f;
            score.HasCodeExamples = response.CodeBlocks.Count > 0 ? 1.0f : 0.0f;
            score.HasTables = response.Tables.Count > 0 ? 0.5f : 0.0f;
            score.HasLinks = response.Links.Count > 0 ? 0.5f : 0.0f;
            
            score.WordCountScore = CalculateWordCountScore(response.WordCount);
            score.ReadabilityScore = CalculateReadabilityScore(response.CleanedContent);
            
            score.OverallScore = (score.HasStructure + score.HasLists + score.HasCodeExamples + 
                                 score.HasTables + score.HasLinks + score.WordCountScore + 
                                 score.ReadabilityScore) / 7.0f;
            
            return score;
        }

        private static float CalculateWordCountScore(int wordCount)
        {
            if (wordCount < 50) return 0.2f;
            if (wordCount < 150) return 0.5f;
            if (wordCount < 300) return 0.8f;
            if (wordCount < 1000) return 1.0f;
            if (wordCount < 2000) return 0.9f;
            return 0.7f;
        }

        private static float CalculateReadabilityScore(string content)
        {
            if (string.IsNullOrEmpty(content))
                return 0.0f;
            
            string[] sentences = content.Split(new char[] { '.', '!', '?' }, 
                StringSplitOptions.RemoveEmptyEntries);
            
            if (sentences.Length == 0)
                return 0.5f;
            
            int totalWords = CountWords(content);
            float avgWordsPerSentence = (float)totalWords / sentences.Length;
            
            if (avgWordsPerSentence < 10) return 0.6f;
            if (avgWordsPerSentence < 20) return 1.0f;
            if (avgWordsPerSentence < 30) return 0.8f;
            return 0.5f;
        }

        private static void ValidateContent(ParsedResponse response, DocumentationSectionType sectionType)
        {
            List<string> validationIssues = new List<string>();
            
            switch (sectionType)
            {
                case DocumentationSectionType.GeneralProductDescription:
                    ValidateGeneralDescription(response, validationIssues);
                    break;
                case DocumentationSectionType.SystemArchitecture:
                    ValidateSystemArchitecture(response, validationIssues);
                    break;
                case DocumentationSectionType.API:
                    ValidateAPIDocumentation(response, validationIssues);
                    break;
                case DocumentationSectionType.DataModel:
                    ValidateDataModel(response, validationIssues);
                    break;
                case DocumentationSectionType.UserStories:
                    ValidateUserStories(response, validationIssues);
                    break;
                case DocumentationSectionType.WorkTickets:
                    ValidateWorkTickets(response, validationIssues);
                    break;
            }
            
            response.ValidationIssues = validationIssues;
            response.IsValidated = true;
        }

        private static void ValidateGeneralDescription(ParsedResponse response, List<string> issues)
        {
            if (response.WordCount < 100)
                issues.Add("Content appears too brief for a comprehensive product description");
            
            if (response.Headings.Count == 0)
                issues.Add("No headings found - consider adding structure with headers");
            
            if (!response.CleanedContent.ToLower().Contains("purpose") && 
                !response.CleanedContent.ToLower().Contains("goal"))
                issues.Add("Content should clearly state the project's purpose or goals");
        }

        private static void ValidateSystemArchitecture(ParsedResponse response, List<string> issues)
        {
            if (response.CodeBlocks.Count == 0)
                issues.Add("Architecture documentation should include code examples or diagrams");
            
            if (!response.CleanedContent.ToLower().Contains("component") && 
                !response.CleanedContent.ToLower().Contains("service"))
                issues.Add("Architecture should describe system components or services");
        }

        private static void ValidateAPIDocumentation(ParsedResponse response, List<string> issues)
        {
            if (response.CodeBlocks.Count == 0)
                issues.Add("API documentation should include code examples");
            
            if (response.Lists.Count == 0)
                issues.Add("API documentation should include parameter lists or endpoint lists");
        }

        private static void ValidateDataModel(ParsedResponse response, List<string> issues)
        {
            if (response.CodeBlocks.Count == 0 && response.Tables.Count == 0)
                issues.Add("Data model documentation should include code examples or data tables");
        }

        private static void ValidateUserStories(ParsedResponse response, List<string> issues)
        {
            if (response.Lists.Count == 0)
                issues.Add("User stories should be presented in list format");
            
            if (!response.CleanedContent.ToLower().Contains("as a") && 
                !response.CleanedContent.ToLower().Contains("user"))
                issues.Add("User stories should follow standard format: 'As a [user], I want [goal]'");
        }

        private static void ValidateWorkTickets(ParsedResponse response, List<string> issues)
        {
            if (response.Lists.Count == 0)
                issues.Add("Work tickets should be presented in list format");
            
            if (response.Headings.Count == 0)
                issues.Add("Work tickets should be organized by categories or priorities");
        }
    }
}