using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityProjectArchitect.Core
{
    [CreateAssetMenu(fileName = "ProjectSettings", menuName = "Unity Project Architect/Project Settings", order = 3)]
    public class ProjectSettings : ScriptableObject
    {
        [Header("AI Configuration")]
        [SerializeField] private AIProvider _defaultAIProvider = AIProvider.Claude;
        [SerializeField] private string _aiApiEndpoint = "";
        [SerializeField] private int _aiRequestTimeout = 30;
        [SerializeField] private int _maxRetryAttempts = 3;

        [Header("Documentation Generation")]
        [SerializeField] private bool _enableAutoGeneration = true;
        [SerializeField] private string _defaultOutputFormat = "markdown";
        [SerializeField] private bool _includeTimestamps = true;
        [SerializeField] private bool _enableVersioning = true;
        [SerializeField] private int _maxVersionHistory = 10;

        [Header("Template Management")]
        [SerializeField] private List<ProjectTemplate> _availableTemplates = new List<ProjectTemplate>();
        [SerializeField] private bool _autoApplyRecommendedTemplates = false;
        [SerializeField] private string _customTemplatesPath = "Templates/";

        [Header("Export Settings")]
        [SerializeField] private List<ExportFormat> _enabledExportFormats = new List<ExportFormat>();
        [SerializeField] private string _defaultExportPath = "Documentation/Export/";
        [SerializeField] private bool _createSubfoldersForExports = true;

        [Header("Performance")]
        [SerializeField] private bool _enableProgressIndicators = true;
        [SerializeField] private bool _enableBackgroundProcessing = true;
        [SerializeField] private int _maxConcurrentOperations = 3;

        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogging = false;
        [SerializeField] private LogLevel _logLevel = LogLevel.Info;

        public AIProvider DefaultAIProvider 
        { 
            get => _defaultAIProvider; 
            set => _defaultAIProvider = value; 
        }

        public string AIApiEndpoint 
        { 
            get => _aiApiEndpoint; 
            set => _aiApiEndpoint = value; 
        }

        public int AIRequestTimeout 
        { 
            get => _aiRequestTimeout; 
            set => _aiRequestTimeout = Math.Max(5, value); 
        }

        public int MaxRetryAttempts 
        { 
            get => _maxRetryAttempts; 
            set => _maxRetryAttempts = Math.Max(1, Math.Min(10, value)); 
        }

        public bool EnableAutoGeneration 
        { 
            get => _enableAutoGeneration; 
            set => _enableAutoGeneration = value; 
        }

        public string DefaultOutputFormat 
        { 
            get => _defaultOutputFormat; 
            set => _defaultOutputFormat = value; 
        }

        public bool IncludeTimestamps 
        { 
            get => _includeTimestamps; 
            set => _includeTimestamps = value; 
        }

        public bool EnableVersioning 
        { 
            get => _enableVersioning; 
            set => _enableVersioning = value; 
        }

        public int MaxVersionHistory 
        { 
            get => _maxVersionHistory; 
            set => _maxVersionHistory = Math.Max(1, Math.Min(100, value)); 
        }

        public List<ProjectTemplate> AvailableTemplates => _availableTemplates;

        public bool AutoApplyRecommendedTemplates 
        { 
            get => _autoApplyRecommendedTemplates; 
            set => _autoApplyRecommendedTemplates = value; 
        }

        public string CustomTemplatesPath 
        { 
            get => _customTemplatesPath; 
            set => _customTemplatesPath = value; 
        }

        public List<ExportFormat> EnabledExportFormats => _enabledExportFormats;

        public string DefaultExportPath 
        { 
            get => _defaultExportPath; 
            set => _defaultExportPath = value; 
        }

        public bool CreateSubfoldersForExports 
        { 
            get => _createSubfoldersForExports; 
            set => _createSubfoldersForExports = value; 
        }

        public bool EnableProgressIndicators 
        { 
            get => _enableProgressIndicators; 
            set => _enableProgressIndicators = value; 
        }

        public bool EnableBackgroundProcessing 
        { 
            get => _enableBackgroundProcessing; 
            set => _enableBackgroundProcessing = value; 
        }

        public int MaxConcurrentOperations 
        { 
            get => _maxConcurrentOperations; 
            set => _maxConcurrentOperations = Math.Max(1, Math.Min(10, value)); 
        }

        public bool EnableDebugLogging 
        { 
            get => _enableDebugLogging; 
            set => _enableDebugLogging = value; 
        }

        public LogLevel LogLevel 
        { 
            get => _logLevel; 
            set => _logLevel = value; 
        }

        private void OnEnable()
        {
            InitializeDefaultSettings();
        }

        private void InitializeDefaultSettings()
        {
            if (_enabledExportFormats.Count == 0)
            {
                _enabledExportFormats.AddRange(new[]
                {
                    ExportFormat.Markdown,
                    ExportFormat.PDF,
                    ExportFormat.HTML
                });
            }
        }

        public void AddTemplate(ProjectTemplate template)
        {
            if (template != null && !_availableTemplates.Contains(template))
            {
                _availableTemplates.Add(template);
            }
        }

        public void RemoveTemplate(ProjectTemplate template)
        {
            _availableTemplates.Remove(template);
        }

        public ProjectTemplate GetTemplateById(string templateId)
        {
            return _availableTemplates.Find(t => t.TemplateId == templateId);
        }

        public List<ProjectTemplate> GetTemplatesForProjectType(ProjectType projectType)
        {
            return _availableTemplates.FindAll(t => t.TargetProjectType == projectType || t.TargetProjectType == ProjectType.General);
        }

        public bool IsExportFormatEnabled(ExportFormat format)
        {
            return _enabledExportFormats.Contains(format);
        }

        [ContextMenu("Reset to Defaults")]
        public void ResetToDefaults()
        {
            _defaultAIProvider = AIProvider.Claude;
            _aiApiEndpoint = "";
            _aiRequestTimeout = 30;
            _maxRetryAttempts = 3;
            _enableAutoGeneration = true;
            _defaultOutputFormat = "markdown";
            _includeTimestamps = true;
            _enableVersioning = true;
            _maxVersionHistory = 10;
            _availableTemplates.Clear();
            _autoApplyRecommendedTemplates = false;
            _customTemplatesPath = "Templates/";
            _enabledExportFormats.Clear();
            InitializeDefaultSettings();
            _defaultExportPath = "Documentation/Export/";
            _createSubfoldersForExports = true;
            _enableProgressIndicators = true;
            _enableBackgroundProcessing = true;
            _maxConcurrentOperations = 3;
            _enableDebugLogging = false;
            _logLevel = LogLevel.Info;

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
        [SerializeField] private AIProvider _provider = AIProvider.Claude;
        [SerializeField] private string _apiKey = "";
        [SerializeField] private string _modelName = "";
        [SerializeField] private float _temperature = 0.7f;
        [SerializeField] private int _maxTokens = 2000;
        [SerializeField] private bool _streamResponse = false;

        public AIProvider Provider 
        { 
            get => _provider; 
            set => _provider = value; 
        }

        public string ApiKey 
        { 
            get => _apiKey; 
            set => _apiKey = value; 
        }

        public string ModelName 
        { 
            get => _modelName; 
            set => _modelName = value; 
        }

        public float Temperature 
        { 
            get => _temperature; 
            set => _temperature = Mathf.Clamp01(value); 
        }

        public int MaxTokens 
        { 
            get => _maxTokens; 
            set => _maxTokens = Math.Max(100, Math.Min(8192, value)); 
        }

        public bool StreamResponse 
        { 
            get => _streamResponse; 
            set => _streamResponse = value; 
        }

        public bool IsValid()
        {
            return _provider != AIProvider.None && !string.IsNullOrEmpty(_apiKey);
        }
    }

    [Serializable]
    public class GenerationPreferences
    {
        [SerializeField] private bool _useCustomPrompts = false;
        [SerializeField] private bool _includeCodeExamples = true;
        [SerializeField] private bool _generateDiagrams = true;
        [SerializeField] private bool _includeTOC = true;
        [SerializeField] private string _documentationStyle = "Professional";

        public bool UseCustomPrompts 
        { 
            get => _useCustomPrompts; 
            set => _useCustomPrompts = value; 
        }

        public bool IncludeCodeExamples 
        { 
            get => _includeCodeExamples; 
            set => _includeCodeExamples = value; 
        }

        public bool GenerateDiagrams 
        { 
            get => _generateDiagrams; 
            set => _generateDiagrams = value; 
        }

        public bool IncludeTOC 
        { 
            get => _includeTOC; 
            set => _includeTOC = value; 
        }

        public string DocumentationStyle 
        { 
            get => _documentationStyle; 
            set => _documentationStyle = value; 
        }
    }
}