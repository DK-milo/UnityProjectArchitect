using System;
using UnityEngine;

namespace UnityProjectArchitect.Core
{
    public enum DocumentationSectionType
    {
        GeneralProductDescription,
        SystemArchitecture,
        DataModel,
        APISpecification,
        UserStories,
        WorkTickets
    }

    public enum DocumentationStatus
    {
        NotStarted,
        InProgress,
        Generated,
        ReviewRequired,
        Completed
    }

    public enum AIGenerationMode
    {
        Disabled,
        AssistanceOnly,
        FullGeneration,
        ReviewAndEnhance
    }

    [Serializable]
    public class DocumentationSectionData
    {
        [SerializeField] private DocumentationSectionType _sectionType;
        [SerializeField] private string _title = "";
        [SerializeField] private string _content = "";
        [SerializeField] private bool _isEnabled = true;
        [SerializeField] private DocumentationStatus _status = DocumentationStatus.NotStarted;
        [SerializeField] private AIGenerationMode _aiMode = AIGenerationMode.AssistanceOnly;
        [SerializeField] private DateTime _lastUpdated;
        [SerializeField] private string _customPrompt = "";
        [SerializeField] private int _wordCountTarget = 500;
        [SerializeField] private string[] _requiredElements = new string[0];

        public DocumentationSectionType SectionType 
        { 
            get => _sectionType; 
            set => _sectionType = value; 
        }

        public string Title 
        { 
            get => string.IsNullOrEmpty(_title) ? GetDefaultTitle() : _title; 
            set => _title = value; 
        }

        public string Content 
        { 
            get => _content; 
            set 
            { 
                _content = value; 
                _lastUpdated = DateTime.Now;
            } 
        }

        public bool IsEnabled 
        { 
            get => _isEnabled; 
            set => _isEnabled = value; 
        }

        public DocumentationStatus Status 
        { 
            get => _status; 
            set => _status = value; 
        }

        public AIGenerationMode AIMode 
        { 
            get => _aiMode; 
            set => _aiMode = value; 
        }

        public DateTime LastUpdated 
        { 
            get => _lastUpdated; 
            set => _lastUpdated = value; 
        }

        public string CustomPrompt 
        { 
            get => string.IsNullOrEmpty(_customPrompt) ? GetDefaultPrompt() : _customPrompt; 
            set => _customPrompt = value; 
        }

        public int WordCountTarget 
        { 
            get => _wordCountTarget; 
            set => _wordCountTarget = Math.Max(100, value); 
        }

        public string[] RequiredElements 
        { 
            get => _requiredElements ?? new string[0]; 
            set => _requiredElements = value ?? new string[0]; 
        }

        public bool HasContent => !string.IsNullOrWhiteSpace(_content);
        public int CurrentWordCount => HasContent ? _content.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length : 0;
        public bool MeetsWordTarget => CurrentWordCount >= _wordCountTarget * 0.8f; // 80% of target is acceptable

        public DocumentationSectionData()
        {
            _lastUpdated = DateTime.Now;
            _requiredElements = GetDefaultRequiredElements();
        }

        public DocumentationSectionData(DocumentationSectionType type) : this()
        {
            _sectionType = type;
            _wordCountTarget = GetDefaultWordCountTarget();
            _requiredElements = GetDefaultRequiredElements();
        }

        private string GetDefaultTitle()
        {
            return _sectionType switch
            {
                DocumentationSectionType.GeneralProductDescription => "General Product Description",
                DocumentationSectionType.SystemArchitecture => "System Architecture",
                DocumentationSectionType.DataModel => "Data Model",
                DocumentationSectionType.APISpecification => "API Specification",
                DocumentationSectionType.UserStories => "User Stories",
                DocumentationSectionType.WorkTickets => "Work Tickets",
                _ => _sectionType.ToString()
            };
        }

        private string GetDefaultPrompt()
        {
            return _sectionType switch
            {
                DocumentationSectionType.GeneralProductDescription => 
                    "Create a comprehensive product description that covers the project's purpose, target audience, key features, and value proposition. Include market context and competitive advantages.",
                
                DocumentationSectionType.SystemArchitecture => 
                    "Generate a detailed system architecture document including component diagrams, data flow, integration points, and technical stack. Focus on scalability and maintainability.",
                
                DocumentationSectionType.DataModel => 
                    "Document the data model including entities, relationships, data flow, and persistence strategy. Include database schema and data validation rules.",
                
                DocumentationSectionType.APISpecification => 
                    "Create comprehensive API documentation including endpoints, request/response schemas, authentication, error handling, and usage examples.",
                
                DocumentationSectionType.UserStories => 
                    "Generate user stories following the 'As a... I want... So that...' format. Include acceptance criteria, priority levels, and estimated effort.",
                
                DocumentationSectionType.WorkTickets => 
                    "Break down the project into actionable work tickets with clear descriptions, acceptance criteria, dependencies, and time estimates.",
                
                _ => "Generate comprehensive documentation for this section based on the project context."
            };
        }

        private int GetDefaultWordCountTarget()
        {
            return _sectionType switch
            {
                DocumentationSectionType.GeneralProductDescription => 800,
                DocumentationSectionType.SystemArchitecture => 1200,
                DocumentationSectionType.DataModel => 600,
                DocumentationSectionType.APISpecification => 1000,
                DocumentationSectionType.UserStories => 1500,
                DocumentationSectionType.WorkTickets => 2000,
                _ => 500
            };
        }

        private string[] GetDefaultRequiredElements()
        {
            return _sectionType switch
            {
                DocumentationSectionType.GeneralProductDescription => new[]
                {
                    "Project Purpose",
                    "Target Audience",
                    "Key Features",
                    "Value Proposition",
                    "Success Metrics"
                },
                
                DocumentationSectionType.SystemArchitecture => new[]
                {
                    "Component Diagram",
                    "Data Flow",
                    "Technology Stack",
                    "Integration Points",
                    "Security Considerations"
                },
                
                DocumentationSectionType.DataModel => new[]
                {
                    "Entity Definitions",
                    "Relationships",
                    "Data Validation",
                    "Persistence Strategy"
                },
                
                DocumentationSectionType.APISpecification => new[]
                {
                    "Endpoint List",
                    "Request/Response Schemas",
                    "Authentication",
                    "Error Handling",
                    "Usage Examples"
                },
                
                DocumentationSectionType.UserStories => new[]
                {
                    "User Roles",
                    "Story Format",
                    "Acceptance Criteria",
                    "Priority Levels"
                },
                
                DocumentationSectionType.WorkTickets => new[]
                {
                    "Task Breakdown",
                    "Dependencies",
                    "Time Estimates",
                    "Acceptance Criteria"
                },
                
                _ => new string[0]
            };
        }

        public void MarkAsGenerated()
        {
            _status = DocumentationStatus.Generated;
            _lastUpdated = DateTime.Now;
        }

        public void MarkAsCompleted()
        {
            _status = DocumentationStatus.Completed;
            _lastUpdated = DateTime.Now;
        }

        public bool ValidateContent()
        {
            if (!HasContent) return false;
            if (!MeetsWordTarget) return false;
            
            // Check if required elements are mentioned in content
            foreach (string element in RequiredElements)
            {
                if (!_content.Contains(element, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            
            return true;
        }

        public float GetCompletionPercentage()
        {
            if (!HasContent) return 0f;
            
            float wordProgress = Math.Min(1f, (float)CurrentWordCount / _wordCountTarget);
            float elementProgress = RequiredElements.Length > 0 ? 
                RequiredElements.Count(element => _content.Contains(element, StringComparison.OrdinalIgnoreCase)) / (float)RequiredElements.Length : 1f;
            
            return (wordProgress * 0.6f) + (elementProgress * 0.4f);
        }
    }
}