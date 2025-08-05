using System;
using System.Collections.Generic;

namespace UnityProjectArchitect.Core
{
    // Template-related models that don't conflict with existing classes

    public class SceneTemplateData
    {
        public string Name { get; set; } = "";
        public string SceneName { get; set; } = "";  // Services expects this property
        public string Description { get; set; } = "";
        public string ScenePath { get; set; } = "";
        public bool IsMainScene { get; set; } = false;
        public bool IncludeInBuild { get; set; } = true;
        public bool CreateOnApply { get; set; } = true;  // Services expects this property
        public int BuildIndex { get; set; } = -1;
        public List<string> RequiredComponents { get; set; } = new List<string>();
        public Dictionary<string, object> SceneSettings { get; set; } = new Dictionary<string, object>();
    }

    public class AssemblyDefinitionTemplate
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public List<string> References { get; set; } = new List<string>();
        public List<string> IncludePlatforms { get; set; } = new List<string>();
        public List<string> ExcludePlatforms { get; set; } = new List<string>();
        public bool AllowUnsafeCode { get; set; } = false;
        public bool OverrideReferences { get; set; } = false;
        public List<string> PrecompiledReferences { get; set; } = new List<string>();
        public bool AutoReferenced { get; set; } = true;
        public bool NoEngineReferences { get; set; } = false;
    }

    public class DocumentationSection
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Title { get; set; } = "";
        public DocumentationSectionType Type { get; set; }
        public string Content { get; set; } = "";
        public string Template { get; set; } = "";
        public int Order { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsRequired { get; set; } = false;
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        public List<string> Tags { get; set; } = new List<string>();
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        
        // Properties required by Services
        public string CustomPrompt { get; set; } = "";
        public AIGenerationMode AIMode { get; set; } = AIGenerationMode.Disabled;
        public int CurrentWordCount => Content.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;

        public DocumentationSection()
        {
            CreatedDate = DateTime.Now;
            LastModifiedDate = CreatedDate;
        }
    }

    public class TemplateApplyResult
    {
        public bool Success { get; set; } = true;
        public string ErrorMessage { get; set; } = "";
        public List<string> CreatedFiles { get; set; } = new List<string>();
        public List<string> CreatedFolders { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public TimeSpan ApplyTime { get; set; }
        public DateTime AppliedAt { get; set; }

        public TemplateApplyResult()
        {
            AppliedAt = DateTime.Now;
        }
    }

    public class TemplateValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> MissingDependencies { get; set; } = new List<string>();
        public List<string> IncompatibleVersions { get; set; } = new List<string>();
    }

    public enum TemplateCategory
    {
        GameDevelopment,
        Tool,
        Library,
        Sample,
        Tutorial,
        Prototype,
        Custom
    }

    public enum TemplateComplexity
    {
        Beginner,
        Intermediate,
        Advanced,
        Expert
    }
}