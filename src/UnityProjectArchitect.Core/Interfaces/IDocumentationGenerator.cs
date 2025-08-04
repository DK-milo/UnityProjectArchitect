using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnityProjectArchitect.Core
{
    public interface IDocumentationGenerator
    {
        event Action<DocumentationSectionType, float> OnSectionProgress;
        event Action<DocumentationGenerationResult> OnGenerationComplete;
        event Action<string> OnError;

        Task<DocumentationGenerationResult> GenerateAllSectionsAsync(ProjectData projectData);
        Task<DocumentationSectionResult> GenerateSectionAsync(ProjectData projectData, DocumentationSectionType sectionType);
        Task<ValidationResult> ValidateProjectForGeneration(ProjectData projectData);
        
        bool CanGenerateSection(DocumentationSectionType sectionType);
        string GetSectionTemplate(DocumentationSectionType sectionType);
        void SetCustomTemplate(DocumentationSectionType sectionType, string template);
        
        List<DocumentationSectionType> GetSupportedSections();
        GenerationCapabilities GetCapabilities();
    }

    public interface IDocumentationSectionGenerator
    {
        DocumentationSectionType SectionType { get; }
        string DisplayName { get; }
        string Description { get; }
        int Priority { get; }
        
        Task<DocumentationSectionResult> GenerateAsync(ProjectData projectData, DocumentationSectionData sectionData);
        ValidationResult ValidateRequirements(ProjectData projectData);
        string GetDefaultTemplate();
        List<string> GetRequiredData();
    }

    [Serializable]
    public class DocumentationGenerationResult
    {
        public bool Success { get; set; }
        public List<DocumentationSectionResult> SectionResults { get; set; }
        public ValidationResult ValidationResult { get; set; }
        public TimeSpan GenerationTime { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string ErrorMessage { get; set; }
        public GenerationStatistics Statistics { get; set; }

        public DocumentationGenerationResult()
        {
            SectionResults = new List<DocumentationSectionResult>();
            GeneratedAt = DateTime.Now;
            Statistics = new GenerationStatistics();
        }

        public DocumentationSectionResult GetSectionResult(DocumentationSectionType sectionType)
        {
            return SectionResults?.Find(r => r.SectionType == sectionType);
        }

        public bool HasErrors => !Success || SectionResults?.Exists(r => !r.Success) == true;
        public int TotalWordCount => SectionResults?.Sum(r => r.WordCount) ?? 0;
        public int CompletedSections => SectionResults?.Count(r => r.Success) ?? 0;
        public int FailedSections => SectionResults?.Count(r => !r.Success) ?? 0;
    }

    [Serializable]
    public class DocumentationSectionResult
    {
        public DocumentationSectionType SectionType { get; set; }
        public bool Success { get; set; }
        public string Content { get; set; }
        public string ErrorMessage { get; set; }
        public TimeSpan GenerationTime { get; set; }
        public DateTime GeneratedAt { get; set; }
        public int WordCount { get; set; }
        public ValidationResult ValidationResult { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public DocumentationSectionResult()
        {
            GeneratedAt = DateTime.Now;
            Metadata = new Dictionary<string, object>();
        }

        public DocumentationSectionResult(DocumentationSectionType type) : this()
        {
            SectionType = type;
        }

        public void SetContent(string content)
        {
            Content = content;
            WordCount = string.IsNullOrEmpty(content) ? 0 : 
                content.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }

    [Serializable]
    public class GenerationStatistics
    {
        public int TotalSections { get; set; }
        public int CompletedSections { get; set; }
        public int SkippedSections { get; set; }
        public int FailedSections { get; set; }
        public TimeSpan TotalTime { get; set; }
        public int TotalWordCount { get; set; }
        public Dictionary<DocumentationSectionType, TimeSpan> SectionTimes { get; set; }
        public Dictionary<string, int> ErrorCounts { get; set; }

        public GenerationStatistics()
        {
            SectionTimes = new Dictionary<DocumentationSectionType, TimeSpan>();
            ErrorCounts = new Dictionary<string, int>();
        }

        public float SuccessRate => TotalSections > 0 ? (float)CompletedSections / TotalSections : 0f;
        public TimeSpan AverageTimePerSection => CompletedSections > 0 ? 
            new TimeSpan(TotalTime.Ticks / CompletedSections) : TimeSpan.Zero;
    }

    [Serializable]
    public class GenerationCapabilities
    {
        public List<DocumentationSectionType> SupportedSections { get; set; }
        public List<ExportFormat> SupportedExportFormats { get; set; }
        public bool SupportsAI { get; set; }
        public bool SupportsCustomTemplates { get; set; }
        public bool SupportsProgressTracking { get; set; }
        public bool SupportsValidation { get; set; }
        public bool SupportsAsync { get; set; }
        public Dictionary<string, bool> Features { get; set; }

        public GenerationCapabilities()
        {
            SupportedSections = new List<DocumentationSectionType>();
            SupportedExportFormats = new List<ExportFormat>();
            Features = new Dictionary<string, bool>();
        }

        public bool HasFeature(string featureName)
        {
            return Features.ContainsKey(featureName) && Features[featureName];
        }
    }

    public static class DocumentationGeneratorExtensions
    {
        public static async Task<DocumentationSectionResult> RegenerateSection(
            this IDocumentationGenerator generator, 
            ProjectData projectData, 
            DocumentationSectionType sectionType)
        {
            DocumentationSectionData section = projectData.GetDocumentationSection(sectionType);
            if (section != null)
            {
                section.Status = DocumentationStatus.InProgress;
                section.Content = "";
            }

            return await generator.GenerateSectionAsync(projectData, sectionType);
        }

        public static bool IsGenerationComplete(this DocumentationGenerationResult result)
        {
            return result.Success && result.CompletedSections == result.Statistics.TotalSections;
        }

        public static string GetFormattedReport(this DocumentationGenerationResult result)
        {
            string report = $"📊 Documentation Generation Report\n";
            report += $"🎯 Success: {result.Success}\n";
            report += $"📄 Completed: {result.CompletedSections}/{result.Statistics.TotalSections} sections\n";
            report += $"⏱️ Time: {result.GenerationTime.TotalSeconds:F1}s\n";
            report += $"📝 Total Words: {result.TotalWordCount:N0}\n";

            if (result.FailedSections > 0)
            {
                report += $"❌ Failed: {result.FailedSections} sections\n";
                foreach (DocumentationSectionResult failed in result.SectionResults.Where(r => !r.Success))
                {
                    report += $"  • {failed.SectionType}: {failed.ErrorMessage}\n";
                }
            }

            return report;
        }
    }
}