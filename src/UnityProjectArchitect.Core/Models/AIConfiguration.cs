using System;
using System.Collections.Generic;

namespace UnityProjectArchitect.Core
{
    public class AIConfiguration
    {
        public string ApiKey { get; set; } = "";
        public AIProvider Provider { get; set; } = AIProvider.Claude;
        public string Model { get; set; } = "claude-3-5-sonnet-20241022";
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
        public string TemplateId { get; set; } = "";  // Services expects this property
        public string Name { get; set; } = "";
        public string name { get; set; } = "";  // Services expects lowercase name property
        public string TemplateName { get; set; } = "";
        public string Description { get; set; } = "";
        public string TemplateDescription { get; set; } = "";  // Services expects this property
        public string Version { get; set; } = "1.0.0";
        public string TemplateVersion { get; set; } = "1.0.0";  // Services expects this property
        public string Category { get; set; } = "";
        public string Author { get; set; } = "";
        public List<string> Tags { get; set; } = new List<string>();
        public ProjectType TargetProjectType { get; set; } = ProjectType.General;
        public UnityVersion MinUnityVersion { get; set; } = UnityVersion.Unity2022_3;
        public TemplateConfiguration Configuration { get; set; } = new TemplateConfiguration();
        public List<TemplateFile> Files { get; set; } = new List<TemplateFile>();
        public FolderStructureData FolderStructure { get; set; } = new FolderStructureData();
        
        // Additional properties required by Services
        public List<SceneTemplateData> SceneTemplates { get; set; } = new List<SceneTemplateData>();
        public List<AssemblyDefinitionTemplate> AssemblyDefinitions { get; set; } = new List<AssemblyDefinitionTemplate>();
        public List<string> RequiredPackages { get; set; } = new List<string>();
        public bool GenerateDefaultDocumentationSections { get; set; } = true;
        public List<DocumentationSection> DefaultDocumentationSections { get; set; } = new List<DocumentationSection>();
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
        public List<string> PreRequisites { get; set; } = new List<string>();
        public bool IsBuiltIn { get; set; } = false;
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        public ProjectTemplate()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = CreatedAt;
            CreatedDate = CreatedAt;
            LastModifiedDate = CreatedAt;
        }

        public TemplateReference CreateReference()
        {
            return new TemplateReference
            {
                TemplateId = TemplateId,
                TemplateName = TemplateName,
                AppliedDate = DateTime.Now,
                Version = TemplateVersion
            };
        }

        public bool IsCompatibleWith(ProjectData projectData)
        {
            // Basic compatibility check
            if (TargetProjectType != ProjectType.General && TargetProjectType != projectData.ProjectType)
                return false;
            
            return true;
        }

        public void InitializeDefaultDocumentationSections()
        {
            // Method version for Services compatibility
            if (DefaultDocumentationSections.Count == 0)
            {
                DefaultDocumentationSections.AddRange(new[]
                {
                    new DocumentationSection { Type = DocumentationSectionType.GeneralProductDescription, IsEnabled = true },
                    new DocumentationSection { Type = DocumentationSectionType.SystemArchitecture, IsEnabled = true },
                    new DocumentationSection { Type = DocumentationSectionType.DataModel, IsEnabled = true },
                    new DocumentationSection { Type = DocumentationSectionType.APISpecification, IsEnabled = true },
                    new DocumentationSection { Type = DocumentationSectionType.UserStories, IsEnabled = true },
                    new DocumentationSection { Type = DocumentationSectionType.WorkTickets, IsEnabled = true }
                });
            }
        }

        // Property version for Services compatibility - different name to avoid conflict
        public List<DocumentationSection> DefaultSections 
        { 
            get 
            { 
                if (DefaultDocumentationSections.Count == 0)
                {
                    InitializeDefaultDocumentationSections();
                }
                return DefaultDocumentationSections; 
            } 
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