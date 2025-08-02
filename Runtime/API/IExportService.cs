using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.API
{
    public interface IExportService
    {
        event Action<ExportOperationResult> OnExportComplete;
        event Action<string, float> OnExportProgress;
        event Action<string> OnError;

        Task<ExportOperationResult> ExportProjectDocumentationAsync(ProjectData projectData, ExportRequest exportRequest);
        Task<ExportOperationResult> ExportSectionAsync(DocumentationSectionData section, ExportRequest exportRequest);
        Task<ExportOperationResult> ExportMultipleSectionsAsync(List<DocumentationSectionData> sections, ExportRequest exportRequest);
        
        Task<ValidationResult> ValidateExportRequestAsync(ExportRequest exportRequest);
        Task<ExportPreview> GeneratePreviewAsync(ProjectData projectData, ExportFormat format);
        
        List<ExportFormat> GetSupportedFormats();
        List<ExportTemplate> GetAvailableTemplates(ExportFormat format);
        ExportServiceCapabilities GetCapabilities();
        
        Task<string> GetOutputPathAsync(ExportRequest exportRequest);
        bool CanExport(ExportFormat format);
    }

    public interface IExportFormatter
    {
        ExportFormat SupportedFormat { get; }
        string DisplayName { get; }
        string FileExtension { get; }
        string MimeType { get; }
        
        Task<ExportOperationResult> FormatAsync(ExportContent content, ExportOptions options);
        ValidationResult ValidateContent(ExportContent content);
        ExportTemplate GetDefaultTemplate();
        List<ExportOption> GetAvailableOptions();
    }

    public interface IExportTemplate
    {
        string TemplateId { get; }
        string TemplateName { get; }
        ExportFormat Format { get; }
        string Description { get; }
        
        Task<string> ApplyTemplateAsync(ExportContent content, Dictionary<string, object> variables);
        ValidationResult ValidateTemplate();
        List<TemplateVariable> GetRequiredVariables();
        ExportTemplate Clone();
    }

    [Serializable]
    public class ExportRequest
    {
        public ExportFormat Format { get; set; }
        public string OutputPath { get; set; }
        public string FileName { get; set; }
        public ExportScope Scope { get; set; }
        public List<DocumentationSectionType> IncludedSections { get; set; }
        public ExportOptions Options { get; set; }
        public string TemplateId { get; set; }
        public Dictionary<string, object> CustomVariables { get; set; }
        public ExportQuality Quality { get; set; }

        public ExportRequest()
        {
            IncludedSections = new List<DocumentationSectionType>();
            Options = new ExportOptions();
            CustomVariables = new Dictionary<string, object>();
            Quality = ExportQuality.Standard;
            Scope = ExportScope.AllSections;
        }

        public ExportRequest(ExportFormat format, string outputPath) : this()
        {
            Format = format;
            OutputPath = outputPath;
        }
    }

    [Serializable]
    public class ExportOptions
    {
        public bool IncludeTableOfContents { get; set; }
        public bool IncludeTimestamp { get; set; }
        public bool IncludeMetadata { get; set; }
        public bool IncludeDiagrams { get; set; }
        public bool IncludeCodeExamples { get; set; }
        public bool CompressOutput { get; set; }
        public bool CreateSubfolders { get; set; }
        public bool OverwriteExisting { get; set; }
        public string CustomCSS { get; set; }
        public string HeaderText { get; set; }
        public string FooterText { get; set; }
        public Dictionary<string, object> FormatSpecificOptions { get; set; }

        public ExportOptions()
        {
            IncludeTableOfContents = true;
            IncludeTimestamp = true;
            IncludeMetadata = true;
            IncludeDiagrams = true;
            IncludeCodeExamples = true;
            CreateSubfolders = true;
            OverwriteExisting = false;
            FormatSpecificOptions = new Dictionary<string, object>();
        }
    }

    public enum ExportScope
    {
        AllSections,
        EnabledSections,
        CompletedSections,
        SelectedSections,
        SingleSection
    }

    public enum ExportQuality
    {
        Draft,
        Standard,
        High,
        Print
    }

    [Serializable]
    public class ExportOperationResult
    {
        public bool Success { get; set; }
        public ExportFormat Format { get; set; }
        public string OutputPath { get; set; }
        public List<string> GeneratedFiles { get; set; }
        public long TotalSizeBytes { get; set; }
        public TimeSpan ExportTime { get; set; }
        public DateTime ExportedAt { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationResult ValidationResult { get; set; }
        public ExportStatistics Statistics { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public ExportOperationResult()
        {
            GeneratedFiles = new List<string>();
            ExportedAt = DateTime.Now;
            Statistics = new ExportStatistics();
            Metadata = new Dictionary<string, object>();
        }

        public ExportOperationResult(ExportFormat format, string outputPath) : this()
        {
            Format = format;
            OutputPath = outputPath;
        }

        public string FormattedSize => FormatBytes(TotalSizeBytes);
        public bool HasGeneratedFiles => GeneratedFiles.Count > 0;

        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }

    [Serializable]
    public class ExportContent
    {
        public string Title { get; set; }
        public ProjectData ProjectData { get; set; }
        public List<DocumentationSectionData> Sections { get; set; }
        public Dictionary<string, object> Variables { get; set; }
        public List<DiagramInfo> Diagrams { get; set; }
        public List<CodeExample> CodeExamples { get; set; }
        public ExportMetadata Metadata { get; set; }

        public ExportContent()
        {
            Sections = new List<DocumentationSectionData>();
            Variables = new Dictionary<string, object>();
            Diagrams = new List<DiagramInfo>();
            CodeExamples = new List<CodeExample>();
            Metadata = new ExportMetadata();
        }

        public ExportContent(ProjectData projectData) : this()
        {
            ProjectData = projectData;
            Title = projectData.ProjectName;
            Sections = projectData.DocumentationSections.Where(s => s.IsEnabled).ToList();
        }
    }

    [Serializable]
    public class ExportMetadata
    {
        public string Author { get; set; }
        public string Version { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModified { get; set; }
        public string Generator { get; set; }
        public Dictionary<string, string> CustomProperties { get; set; }

        public ExportMetadata()
        {
            CreatedDate = DateTime.Now;
            LastModified = DateTime.Now;
            Generator = "Unity Project Architect";
            CustomProperties = new Dictionary<string, string>();
        }
    }

    [Serializable]
    public class ExportPreview
    {
        public ExportFormat Format { get; set; }
        public string PreviewText { get; set; }
        public List<string> FileStructure { get; set; }
        public long EstimatedSize { get; set; }
        public int PageCount { get; set; }
        public TimeSpan EstimatedExportTime { get; set; }
        public List<string> RequiredResources { get; set; }

        public ExportPreview()
        {
            FileStructure = new List<string>();
            RequiredResources = new List<string>();
        }

        public string FormattedEstimatedSize => FormatBytes(EstimatedSize);

        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }

    [Serializable]
    public class ExportStatistics
    {
        public int TotalSections { get; set; }
        public int ExportedSections { get; set; }
        public int SkippedSections { get; set; }
        public int TotalPages { get; set; }
        public int TotalWords { get; set; }
        public int TotalImages { get; set; }
        public int TotalDiagrams { get; set; }
        public Dictionary<DocumentationSectionType, TimeSpan> SectionExportTimes { get; set; }

        public ExportStatistics()
        {
            SectionExportTimes = new Dictionary<DocumentationSectionType, TimeSpan>();
        }

        public float ExportSuccessRate => TotalSections > 0 ? (float)ExportedSections / TotalSections : 0f;
        public TimeSpan AverageTimePerSection => ExportedSections > 0 ? 
            TimeSpan.FromTicks(SectionExportTimes.Values.Sum(t => t.Ticks) / ExportedSections) : TimeSpan.Zero;
    }

    [Serializable]
    public class ExportServiceCapabilities
    {
        public List<ExportFormat> SupportedFormats { get; set; }
        public List<ExportScope> SupportedScopes { get; set; }
        public bool SupportsCustomTemplates { get; set; }
        public bool SupportsPreview { get; set; }
        public bool SupportsProgressTracking { get; set; }
        public bool SupportsBatchExport { get; set; }
        public bool SupportsCompression { get; set; }
        public bool SupportsCustomStyling { get; set; }
        public Dictionary<string, bool> Features { get; set; }
        public Dictionary<ExportFormat, List<string>> FormatSpecificFeatures { get; set; }

        public ExportServiceCapabilities()
        {
            SupportedFormats = new List<ExportFormat>();
            SupportedScopes = new List<ExportScope>();
            Features = new Dictionary<string, bool>();
            FormatSpecificFeatures = new Dictionary<ExportFormat, List<string>>();
        }

        public bool HasFeature(string featureName)
        {
            return Features.ContainsKey(featureName) && Features[featureName];
        }

        public bool SupportsFormat(ExportFormat format)
        {
            return SupportedFormats.Contains(format);
        }

        public List<string> GetFormatFeatures(ExportFormat format)
        {
            return FormatSpecificFeatures.ContainsKey(format) ? FormatSpecificFeatures[format] : new List<string>();
        }
    }

    [Serializable]
    public class ExportTemplate
    {
        public string TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string Description { get; set; }
        public ExportFormat Format { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string TemplateContent { get; set; }
        public List<TemplateVariable> Variables { get; set; }
        public Dictionary<string, object> Settings { get; set; }

        public ExportTemplate()
        {
            Variables = new List<TemplateVariable>();
            Settings = new Dictionary<string, object>();
        }

        public ExportTemplate(string id, string name, ExportFormat format) : this()
        {
            TemplateId = id;
            TemplateName = name;
            Format = format;
        }
    }

    [Serializable]
    public class TemplateVariable
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public VariableType Type { get; set; }
        public object DefaultValue { get; set; }
        public bool IsRequired { get; set; }
        public string ValidationPattern { get; set; }

        public TemplateVariable()
        {
            Type = VariableType.String;
            IsRequired = false;
        }

        public TemplateVariable(string name, VariableType type, object defaultValue = null) : this()
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
        }
    }

    public enum VariableType
    {
        String,
        Integer,
        Boolean,
        DateTime,
        List,
        Dictionary
    }

    public static class ExportServiceExtensions
    {
        public static async Task<ExportOperationResult> ExportToMultipleFormats(
            this IExportService exportService,
            ProjectData projectData,
            List<ExportFormat> formats,
            string basePath)
        {
            var combinedResult = new ExportOperationResult();
            var allFiles = new List<string>();
            long totalSize = 0;
            var totalTime = TimeSpan.Zero;

            foreach (var format in formats)
            {
                var request = new ExportRequest(format, basePath)
                {
                    FileName = $"{projectData.ProjectName}.{GetFileExtension(format)}"
                };

                var result = await exportService.ExportProjectDocumentationAsync(projectData, request);
                
                if (result.Success)
                {
                    allFiles.AddRange(result.GeneratedFiles);
                    totalSize += result.TotalSizeBytes;
                    totalTime = totalTime.Add(result.ExportTime);
                }
                else
                {
                    combinedResult.Success = false;
                    combinedResult.ErrorMessage += $"{format}: {result.ErrorMessage}; ";
                }
            }

            combinedResult.Success = combinedResult.Success && allFiles.Count > 0;
            combinedResult.GeneratedFiles = allFiles;
            combinedResult.TotalSizeBytes = totalSize;
            combinedResult.ExportTime = totalTime;

            return combinedResult;
        }

        public static string GetFileExtension(ExportFormat format)
        {
            return format switch
            {
                ExportFormat.Markdown => "md",
                ExportFormat.PDF => "pdf",
                ExportFormat.HTML => "html",
                ExportFormat.JSON => "json",
                ExportFormat.XML => "xml",
                ExportFormat.Word => "docx",
                _ => "txt"
            };
        }

        public static string GetFormattedReport(this ExportOperationResult result)
        {
            var report = $"📤 Export Operation Report\n";
            report += $"✅ Success: {result.Success}\n";
            report += $"📄 Format: {result.Format}\n";
            report += $"📁 Output: {result.OutputPath}\n";
            report += $"📊 Files Generated: {result.GeneratedFiles.Count}\n";
            report += $"💾 Total Size: {result.FormattedSize}\n";
            report += $"⏱️ Export Time: {result.ExportTime.TotalSeconds:F1}s\n";

            if (result.Statistics != null)
            {
                report += $"📑 Sections: {result.Statistics.ExportedSections}/{result.Statistics.TotalSections}\n";
                report += $"📝 Total Words: {result.Statistics.TotalWords:N0}\n";
            }

            if (!result.Success)
            {
                report += $"❌ Error: {result.ErrorMessage}\n";
            }

            return report;
        }

        public static bool IsReadyForExport(this ProjectData projectData, ExportScope scope)
        {
            return scope switch
            {
                ExportScope.AllSections => projectData.DocumentationSections.Count > 0,
                ExportScope.EnabledSections => projectData.DocumentationSections.Any(s => s.IsEnabled),
                ExportScope.CompletedSections => projectData.DocumentationSections.Any(s => s.Status == DocumentationStatus.Completed),
                ExportScope.SelectedSections => true,
                ExportScope.SingleSection => true,
                _ => false
            };
        }
    }

    [Serializable]
    public class DiagramInfo
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DiagramType Type { get; set; }
        public string ImagePath { get; set; }
        public string ImageData { get; set; }
        public Dictionary<string, object> Properties { get; set; }

        public DiagramInfo()
        {
            Properties = new Dictionary<string, object>();
        }

        public DiagramInfo(string id, string title, DiagramType type) : this()
        {
            Id = id;
            Title = title;
            Type = type;
        }
    }

    [Serializable]
    public class CodeExample
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public string Code { get; set; }
        public string FilePath { get; set; }
        public List<string> Tags { get; set; }

        public CodeExample()
        {
            Tags = new List<string>();
            Language = "csharp";
        }

        public CodeExample(string id, string title, string code) : this()
        {
            Id = id;
            Title = title;
            Code = code;
        }
    }

    public enum DiagramType
    {
        Architecture,
        Sequence,
        Flow,
        Class,
        Entity,
        Network,
        Component,
        Custom
    }
}