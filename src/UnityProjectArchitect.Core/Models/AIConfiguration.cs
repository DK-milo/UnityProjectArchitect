using System;
using System.Collections.Generic;

namespace UnityProjectArchitect.Core
{
    public class AIConfiguration
    {
        public string ApiKey { get; set; } = "";
        public AIProvider Provider { get; set; } = AIProvider.Claude;
        public string Model { get; set; } = "claude-3-sonnet-20240229";
        public int MaxTokens { get; set; } = 4000;
        public float Temperature { get; set; } = 0.7f;
        public int TimeoutSeconds { get; set; } = 60;
        public int MaxRetries { get; set; } = 3;
        public bool EnableLogging { get; set; } = true;
        public Dictionary<string, object> CustomSettings { get; set; } = new Dictionary<string, object>();

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(ApiKey) && 
                   MaxTokens > 0 && 
                   TimeoutSeconds > 0 && 
                   Temperature >= 0 && Temperature <= 1;
        }
    }

    public class AIResponse
    {
        public bool Success { get; set; }
        public string Content { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
        public int TokensUsed { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public DateTime ProcessedAt { get; set; }
        public float ConfidenceScore { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        public AIResponse()
        {
            ProcessedAt = DateTime.Now;
        }

        public static AIResponse CreateSuccess(string content)
        {
            return new AIResponse
            {
                Success = true,
                Content = content,
                ProcessedAt = DateTime.Now
            };
        }

        public static AIResponse CreateError(string errorMessage)
        {
            return new AIResponse
            {
                Success = false,
                ErrorMessage = errorMessage,
                ProcessedAt = DateTime.Now
            };
        }
    }

    // Template Models - only non-conflicting ones
    public class TemplateConfiguration
    {
        public string Name { get; set; } = "";
        public string Version { get; set; } = "1.0.0";
        public string Description { get; set; } = "";
        public List<string> RequiredFeatures { get; set; } = new List<string>();
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public TemplateConfiguration()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = CreatedAt;
        }
    }

    public class ProjectTemplate
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string TemplateName { get; set; } = "";
        public string Description { get; set; } = "";
        public string Version { get; set; } = "1.0.0";
        public string Category { get; set; } = "";
        public List<string> Tags { get; set; } = new List<string>();
        public ProjectType TargetProjectType { get; set; } = ProjectType.General;
        public TemplateConfiguration Configuration { get; set; } = new TemplateConfiguration();
        public List<TemplateFile> Files { get; set; } = new List<TemplateFile>();
        public List<TemplateFolderStructure> FolderStructure { get; set; } = new List<TemplateFolderStructure>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ProjectTemplate()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = CreatedAt;
        }
    }

    public class TemplateFile
    {
        public string RelativePath { get; set; } = "";
        public string Content { get; set; } = "";
        public string ContentType { get; set; } = "text";
        public bool IsTemplate { get; set; } = true;
        public List<string> Variables { get; set; } = new List<string>();
    }

    public class TemplateFolderStructure
    {
        public string RelativePath { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsRequired { get; set; } = true;
        public string Description { get; set; } = "";
    }
}