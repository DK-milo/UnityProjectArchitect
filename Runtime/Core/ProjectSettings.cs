using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityProjectArchitect.Core
{
    [CreateAssetMenu(fileName = "ProjectSettings", menuName = "Unity Project Architect/Project Settings", order = 3)]
    public class ProjectSettings : ScriptableObject
    {
        [Header("AI Configuration")]
        [SerializeField] private AIProvider defaultAIProvider = AIProvider.Claude;
        [SerializeField] private string aiApiEndpoint = "";
        [SerializeField] private int aiRequestTimeout = 30;
        [SerializeField] private int maxRetryAttempts = 3;

        [Header("Documentation Generation")]
        [SerializeField] private bool enableAutoGeneration = true;
        [SerializeField] private string defaultOutputFormat = "markdown";
        [SerializeField] private bool includeTimestamps = true;
        [SerializeField] private bool enableVersioning = true;
        [SerializeField] private int maxVersionHistory = 10;

        [Header("Template Management")]
        [SerializeField] private List<ProjectTemplate> availableTemplates = new List<ProjectTemplate>();
        [SerializeField] private bool autoApplyRecommendedTemplates = false;
        [SerializeField] private string customTemplatesPath = "Templates/";

        [Header("Export Settings")]
        [SerializeField] private List<ExportFormat> enabledExportFormats = new List<ExportFormat>();
        [SerializeField] private string defaultExportPath = "Documentation/Export/";
        [SerializeField] private bool createSubfoldersForExports = true;

        [Header("Performance")]
        [SerializeField] private bool enableProgressIndicators = true;
        [SerializeField] private bool enableBackgroundProcessing = true;
        [SerializeField] private int maxConcurrentOperations = 3;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogging = false;
        [SerializeField] private LogLevel logLevel = LogLevel.Info;

        public AIProvider DefaultAIProvider 
        { 
            get => defaultAIProvider; 
            set => defaultAIProvider = value; 
        }

        public string AIApiEndpoint 
        { 
            get => aiApiEndpoint; 
            set => aiApiEndpoint = value; 
        }

        public int AIRequestTimeout 
        { 
            get => aiRequestTimeout; 
            set => aiRequestTimeout = Math.Max(5, value); 
        }

        public int MaxRetryAttempts 
        { 
            get => maxRetryAttempts; 
            set => maxRetryAttempts = Math.Max(1, Math.Min(10, value)); 
        }

        public bool EnableAutoGeneration 
        { 
            get => enableAutoGeneration; 
            set => enableAutoGeneration = value; 
        }

        public string DefaultOutputFormat 
        { 
            get => defaultOutputFormat; 
            set => defaultOutputFormat = value; 
        }

        public bool IncludeTimestamps 
        { 
            get => includeTimestamps; 
            set => includeTimestamps = value; 
        }

        public bool EnableVersioning 
        { 
            get => enableVersioning; 
            set => enableVersioning = value; 
        }

        public int MaxVersionHistory 
        { 
            get => maxVersionHistory; 
            set => maxVersionHistory = Math.Max(1, Math.Min(100, value)); 
        }

        public List<ProjectTemplate> AvailableTemplates => availableTemplates;

        public bool AutoApplyRecommendedTemplates 
        { 
            get => autoApplyRecommendedTemplates; 
            set => autoApplyRecommendedTemplates = value; 
        }

        public string CustomTemplatesPath 
        { 
            get => customTemplatesPath; 
            set => customTemplatesPath = value; 
        }

        public List<ExportFormat> EnabledExportFormats => enabledExportFormats;

        public string DefaultExportPath 
        { 
            get => defaultExportPath; 
            set => defaultExportPath = value; 
        }

        public bool CreateSubfoldersForExports 
        { 
            get => createSubfoldersForExports; 
            set => createSubfoldersForExports = value; 
        }

        public bool EnableProgressIndicators 
        { 
            get => enableProgressIndicators; 
            set => enableProgressIndicators = value; 
        }

        public bool EnableBackgroundProcessing 
        { 
            get => enableBackgroundProcessing; 
            set => enableBackgroundProcessing = value; 
        }

        public int MaxConcurrentOperations 
        { 
            get => maxConcurrentOperations; 
            set => maxConcurrentOperations = Math.Max(1, Math.Min(10, value)); 
        }

        public bool EnableDebugLogging 
        { 
            get => enableDebugLogging; 
            set => enableDebugLogging = value; 
        }

        public LogLevel LogLevel 
        { 
            get => logLevel; 
            set => logLevel = value; 
        }

        private void OnEnable()
        {
            InitializeDefaultSettings();
        }

        private void InitializeDefaultSettings()
        {
            if (enabledExportFormats.Count == 0)
            {
                enabledExportFormats.AddRange(new[]
                {
                    ExportFormat.Markdown,
                    ExportFormat.PDF,
                    ExportFormat.HTML
                });
            }
        }

        public void AddTemplate(ProjectTemplate template)
        {
            if (template != null && !availableTemplates.Contains(template))
            {
                availableTemplates.Add(template);
            }
        }

        public void RemoveTemplate(ProjectTemplate template)
        {
            availableTemplates.Remove(template);
        }

        public ProjectTemplate GetTemplateById(string templateId)
        {
            return availableTemplates.Find(t => t.TemplateId == templateId);
        }

        public List<ProjectTemplate> GetTemplatesForProjectType(ProjectType projectType)
        {
            return availableTemplates.FindAll(t => t.TargetProjectType == projectType || t.TargetProjectType == ProjectType.General);
        }

        public bool IsExportFormatEnabled(ExportFormat format)
        {
            return enabledExportFormats.Contains(format);
        }

        [ContextMenu("Reset to Defaults")]
        public void ResetToDefaults()
        {
            defaultAIProvider = AIProvider.Claude;
            aiApiEndpoint = "";
            aiRequestTimeout = 30;
            maxRetryAttempts = 3;
            enableAutoGeneration = true;
            defaultOutputFormat = "markdown";
            includeTimestamps = true;
            enableVersioning = true;
            maxVersionHistory = 10;
            availableTemplates.Clear();
            autoApplyRecommendedTemplates = false;
            customTemplatesPath = "Templates/";
            enabledExportFormats.Clear();
            InitializeDefaultSettings();
            defaultExportPath = "Documentation/Export/";
            createSubfoldersForExports = true;
            enableProgressIndicators = true;
            enableBackgroundProcessing = true;
            maxConcurrentOperations = 3;
            enableDebugLogging = false;
            logLevel = LogLevel.Info;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    public enum ExportFormat
    {
        Markdown,
        PDF,
        HTML,
        JSON,
        XML,
        Word
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    [Serializable]
    public class AIConfiguration
    {
        [SerializeField] private AIProvider provider = AIProvider.Claude;
        [SerializeField] private string apiKey = "";
        [SerializeField] private string modelName = "";
        [SerializeField] private float temperature = 0.7f;
        [SerializeField] private int maxTokens = 2000;
        [SerializeField] private bool streamResponse = false;

        public AIProvider Provider 
        { 
            get => provider; 
            set => provider = value; 
        }

        public string ApiKey 
        { 
            get => apiKey; 
            set => apiKey = value; 
        }

        public string ModelName 
        { 
            get => modelName; 
            set => modelName = value; 
        }

        public float Temperature 
        { 
            get => temperature; 
            set => temperature = Mathf.Clamp01(value); 
        }

        public int MaxTokens 
        { 
            get => maxTokens; 
            set => maxTokens = Math.Max(100, Math.Min(8192, value)); 
        }

        public bool StreamResponse 
        { 
            get => streamResponse; 
            set => streamResponse = value; 
        }

        public bool IsValid()
        {
            return provider != AIProvider.None && !string.IsNullOrEmpty(apiKey);
        }
    }

    [Serializable]
    public class GenerationPreferences
    {
        [SerializeField] private bool useCustomPrompts = false;
        [SerializeField] private bool includeCodeExamples = true;
        [SerializeField] private bool generateDiagrams = true;
        [SerializeField] private bool includeTOC = true;
        [SerializeField] private string documentationStyle = "Professional";

        public bool UseCustomPrompts 
        { 
            get => useCustomPrompts; 
            set => useCustomPrompts = value; 
        }

        public bool IncludeCodeExamples 
        { 
            get => includeCodeExamples; 
            set => includeCodeExamples = value; 
        }

        public bool GenerateDiagrams 
        { 
            get => generateDiagrams; 
            set => generateDiagrams = value; 
        }

        public bool IncludeTOC 
        { 
            get => includeTOC; 
            set => includeTOC = value; 
        }

        public string DocumentationStyle 
        { 
            get => documentationStyle; 
            set => documentationStyle = value; 
        }
    }
}