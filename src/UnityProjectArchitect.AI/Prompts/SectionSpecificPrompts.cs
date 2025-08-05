using System;
using System.Collections.Generic;
using System.Text;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.AI.Prompts
{
    /// <summary>
    /// Specialized prompt generators tailored for each documentation section type
    /// Provides contextually-aware, high-quality prompts optimized for AI generation
    /// </summary>
    public static class SectionSpecificPrompts
    {
        /// <summary>
        /// Generates comprehensive prompt for General Product Description section
        /// Focuses on project overview, purpose, and key features
        /// </summary>
        public static string GetGeneralDescriptionPrompt()
        {
            return @"Generate a comprehensive General Product Description for this Unity project based on the provided project context.

**Requirements:**
- Create a professional project overview that explains the purpose and scope
- Include key features and capabilities based on the project analysis
- Describe the target audience and use cases
- Highlight unique selling points and technical strengths
- Use clear, engaging language suitable for both technical and non-technical readers

**Context to analyze:**
- Project structure and organization patterns
- Script architectures and design patterns used
- Asset types and content scope
- Unity version and platform targets
- Custom tools and editor extensions

**Format:**
Use markdown formatting with clear sections:
1. **Project Overview** - Brief introduction and purpose
2. **Key Features** - Bullet points of main capabilities
3. **Target Audience** - Who would use this project
4. **Technical Highlights** - Notable technical aspects
5. **Use Cases** - Practical applications and scenarios

**Tone:** Professional yet accessible, focusing on value proposition and technical excellence.

**Output length:** 300-500 words

Project Context: {PROJECT_CONTEXT}
Project Analysis: {PROJECT_ANALYSIS}";
        }

        /// <summary>
        /// Generates detailed prompt for System Architecture section
        /// Emphasizes technical structure, patterns, and design decisions
        /// </summary>
        public static string GetSystemArchitecturePrompt()
        {
            return @"Generate a detailed System Architecture documentation for this Unity project based on the comprehensive project analysis.

**Requirements:**
- Document the overall system architecture and design patterns
- Explain component relationships and data flow
- Identify architectural patterns (MVC, Observer, Factory, etc.)
- Describe Unity-specific architecture choices (ScriptableObjects, MonoBehaviours, etc.)
- Include performance and scalability considerations
- Highlight separation of concerns and modularity

**Analysis Focus:**
- Script organization and namespace structure
- Interface usage and abstraction patterns
- Data management and persistence strategies
- Event systems and communication patterns
- Asset organization and resource management
- Editor tool integration and extensibility

**Format:**
Structure as markdown with technical diagrams described in text:
1. **Architecture Overview** - High-level system design
2. **Core Components** - Main system components and responsibilities
3. **Design Patterns** - Patterns used and their implementation
4. **Data Flow** - How data moves through the system
5. **Unity Integration** - Unity-specific architectural decisions
6. **Scalability** - How the architecture supports growth
7. **Extension Points** - Areas designed for future expansion

**Tone:** Technical and precise, suitable for developers and architects.

**Output length:** 500-800 words

Project Context: {PROJECT_CONTEXT}
Architecture Analysis: {ARCHITECTURE_ANALYSIS}
Script Analysis: {SCRIPT_ANALYSIS}";
        }

        /// <summary>
        /// Generates comprehensive prompt for Data Model section
        /// Focuses on data structures, relationships, and serialization
        /// </summary>
        public static string GetDataModelPrompt()
        {
            return @"Generate comprehensive Data Model documentation for this Unity project, focusing on data structures, relationships, and serialization patterns.

**Requirements:**
- Document all major data classes and their purposes
- Explain data relationships and dependencies
- Describe serialization strategies (ScriptableObjects, JSON, Binary)
- Cover data validation and integrity patterns
- Include data persistence and storage approaches
- Highlight Unity-specific data patterns

**Analysis Areas:**
- ScriptableObject data containers and their usage
- Serializable classes and data transfer objects
- Configuration and settings data structures
- Runtime data management and caching
- Asset referencing and data binding patterns
- Editor data persistence and project settings

**Format:**
Organize as markdown with clear data structure documentation:
1. **Data Architecture Overview** - High-level data organization
2. **Core Data Models** - Primary data classes and their roles
3. **Data Relationships** - How data entities connect and depend on each other
4. **Serialization Strategy** - Persistence and storage approaches
5. **Configuration Management** - Settings and configuration data handling
6. **Runtime Data Flow** - How data is created, modified, and consumed
7. **Data Validation** - Integrity checks and validation patterns

**Tone:** Technical documentation style with code examples described in text.

**Output length:** 400-600 words

Project Context: {PROJECT_CONTEXT}
Data Model Analysis: {DATA_MODEL_ANALYSIS}
Asset Analysis: {ASSET_ANALYSIS}";
        }

        /// <summary>
        /// Generates detailed prompt for API Specification section
        /// Covers public interfaces, methods, and usage patterns
        /// </summary>
        public static string GetAPISpecificationPrompt()
        {
            return @"Generate detailed API Specification documentation for this Unity project, covering all public interfaces, methods, and usage patterns.

**Requirements:**
- Document all public APIs and their intended usage
- Provide method signatures and parameter descriptions
- Include usage examples and best practices
- Cover error handling and edge cases
- Describe API versioning and compatibility
- Include Unity Editor API extensions

**API Documentation Focus:**
- Public interfaces and their implementations
- Static utility classes and helper methods
- Extension methods and additional functionality
- Event systems and callback patterns
- Configuration APIs and customization points
- Unity Editor menu items and custom inspectors

**Format:**
Structure as comprehensive API reference:
1. **API Overview** - High-level API organization and design philosophy
2. **Core Interfaces** - Primary public interfaces and their contracts
3. **Utility Classes** - Helper classes and static methods
4. **Extension APIs** - Extensibility points and customization
5. **Event System** - Events, callbacks, and notification patterns
6. **Configuration API** - Settings and configuration management
7. **Unity Integration** - Editor extensions and Unity-specific APIs
8. **Usage Examples** - Common usage patterns and code samples (described)
9. **Error Handling** - Exception types and error recovery patterns

**Tone:** Technical reference style, suitable for developers integrating with the API.

**Output length:** 600-900 words

Project Context: {PROJECT_CONTEXT}
Interface Analysis: {INTERFACE_ANALYSIS}
Public API Analysis: {PUBLIC_API_ANALYSIS}";
        }

        /// <summary>
        /// Generates comprehensive prompt for User Stories section
        /// Focuses on feature requirements from user perspective
        /// </summary>
        public static string GetUserStoriesPrompt()
        {
            return @"Generate comprehensive User Stories documentation for this Unity project based on feature analysis and intended functionality.

**Requirements:**
- Create user stories following Agile methodology (As a... I want... So that...)
- Organize stories into logical epics and themes
- Include acceptance criteria for each story
- Prioritize stories by importance and implementation complexity
- Cover both end-user and developer user stories
- Include edge cases and error scenarios

**Story Categories to Cover:**
- Core functionality and primary features
- Unity Editor integration and workflow
- Configuration and customization capabilities
- Import/export and data management features
- Performance and optimization requirements
- Error handling and user feedback scenarios

**Format:**
Structure as organized user story collection:
1. **Epic Overview** - High-level feature themes and user goals
2. **Core User Stories** - Primary functionality from user perspective
3. **Developer Stories** - Technical requirements and integration needs
4. **Configuration Stories** - Customization and settings management
5. **Data Management Stories** - Import, export, and persistence features
6. **Quality Stories** - Performance, reliability, and usability requirements
7. **Edge Case Stories** - Error handling and boundary conditions

**Story Format for each:**
- **As a** [user type] **I want** [functionality] **so that** [benefit/value]
- **Acceptance Criteria:** Specific, testable requirements
- **Priority:** High/Medium/Low based on feature analysis

**Tone:** User-focused and business-oriented, emphasizing value and outcomes.

**Output length:** 500-700 words

Project Context: {PROJECT_CONTEXT}
Feature Analysis: {FEATURE_ANALYSIS}
User Workflow Analysis: {USER_WORKFLOW_ANALYSIS}";
        }

        /// <summary>
        /// Generates detailed prompt for Work Tickets section
        /// Creates actionable development tasks and implementation tickets
        /// </summary>
        public static string GetWorkTicketsPrompt()
        {
            return @"Generate detailed Work Tickets (implementation tasks) for this Unity project based on the technical analysis and identified development needs.

**Requirements:**
- Create specific, actionable development tasks
- Include technical implementation details and requirements
- Estimate complexity and time requirements
- Organize tickets by feature area and priority
- Include testing and validation requirements
- Cover both development and maintenance tasks

**Ticket Categories:**
- Core feature implementation tasks
- Bug fixes and technical debt resolution
- Performance optimization and refactoring
- Testing and quality assurance tasks
- Documentation and knowledge transfer work
- Unity version upgrades and compatibility tasks

**Format:**
Structure as comprehensive task breakdown:
1. **Development Tickets** - New feature implementation tasks
2. **Bug Fix Tickets** - Issues and defects to resolve
3. **Refactoring Tickets** - Code quality and architectural improvements
4. **Performance Tickets** - Optimization and efficiency improvements
5. **Testing Tickets** - Test coverage and quality assurance tasks
6. **Documentation Tickets** - Knowledge transfer and documentation updates
7. **Maintenance Tickets** - Ongoing support and update requirements

**Ticket Format for each:**
- **Title:** Clear, action-oriented task description
- **Description:** Detailed technical requirements and context
- **Acceptance Criteria:** Specific, testable completion requirements
- **Complexity:** Story points or time estimate
- **Dependencies:** Prerequisites and related tickets
- **Technical Notes:** Implementation guidance and considerations

**Tone:** Technical and task-oriented, suitable for development team planning.

**Output length:** 600-800 words

Project Context: {PROJECT_CONTEXT}
Technical Analysis: {TECHNICAL_ANALYSIS}
Issue Analysis: {ISSUE_ANALYSIS}";
        }

        /// <summary>
        /// Generates general-purpose prompt for custom or undefined section types
        /// Provides flexible template for any documentation needs
        /// </summary>
        public static string GetGeneralPrompt()
        {
            return @"Generate comprehensive documentation content for this Unity project section based on the provided context and requirements.

**Requirements:**
- Analyze the project context to understand scope and purpose
- Create content appropriate for the intended audience
- Use clear, professional language with proper technical terminology
- Structure content with logical organization and clear sections
- Include relevant technical details and implementation insights
- Ensure content is actionable and valuable for readers

**Analysis Focus:**
- Project structure and organization
- Technical implementation patterns
- Feature capabilities and functionality
- Integration points and dependencies
- Best practices and recommendations
- Potential improvements and optimizations

**Format:**
Structure content with clear organization:
1. **Overview** - Introduction and purpose
2. **Key Concepts** - Important ideas and terminology
3. **Implementation Details** - Technical specifics and patterns
4. **Usage Guidelines** - How to work with or use the system
5. **Best Practices** - Recommended approaches and patterns
6. **Considerations** - Important factors and trade-offs

**Tone:** Professional and informative, balancing technical accuracy with accessibility.

**Output length:** 400-600 words

Project Context: {PROJECT_CONTEXT}
Section Requirements: {SECTION_REQUIREMENTS}
Additional Context: {ADDITIONAL_CONTEXT}";
        }

        /// <summary>
        /// Builds context-aware prompt by replacing placeholders with project-specific information
        /// </summary>
        public static string BuildContextualPrompt(string basePrompt, ProjectData projectData, Dictionary<string, string> additionalContext = null)
        {
            if (string.IsNullOrEmpty(basePrompt))
                throw new ArgumentException("Base prompt cannot be null or empty", nameof(basePrompt));
            if (projectData == null)
                throw new ArgumentNullException(nameof(projectData));

            StringBuilder contextualPrompt = new StringBuilder(basePrompt);
            
            // Replace standard project context placeholders
            contextualPrompt.Replace("{PROJECT_CONTEXT}", ContextBuilder.BuildProjectContext(projectData));
            contextualPrompt.Replace("{PROJECT_NAME}", projectData.ProjectName ?? "Unity Project");
            contextualPrompt.Replace("{PROJECT_TYPE}", projectData.ProjectType.ToString());
            contextualPrompt.Replace("{UNITY_VERSION}", projectData.TargetUnityVersion.ToString());

            // Replace additional context placeholders if provided
            if (additionalContext != null)
            {
                foreach (KeyValuePair<string, string> context in additionalContext)
                {
                    contextualPrompt.Replace($"{{{context.Key}}}", context.Value ?? string.Empty);
                }
            }

            return contextualPrompt.ToString();
        }

        /// <summary>
        /// Gets estimated token count for a specific section type prompt
        /// </summary>
        public static int GetEstimatedTokenCount(DocumentationSectionType sectionType)
        {
            string prompt = GetPromptForSectionType(sectionType);
            PromptOptimizer optimizer = new PromptOptimizer();
            return optimizer.EstimateTokenCount(prompt);
        }

        /// <summary>
        /// Gets prompt template for specified section type
        /// </summary>
        public static string GetPromptForSectionType(DocumentationSectionType sectionType)
        {
            return sectionType switch
            {
                DocumentationSectionType.GeneralProductDescription => GetGeneralDescriptionPrompt(),
                DocumentationSectionType.SystemArchitecture => GetSystemArchitecturePrompt(),
                DocumentationSectionType.DataModel => GetDataModelPrompt(),
                DocumentationSectionType.APISpecification => GetAPISpecificationPrompt(),
                DocumentationSectionType.UserStories => GetUserStoriesPrompt(),
                DocumentationSectionType.WorkTickets => GetWorkTicketsPrompt(),
                _ => GetGeneralPrompt()
            };
        }

        /// <summary>
        /// Validates that a prompt template contains required placeholders for section type
        /// </summary>
        public static List<string> ValidatePromptTemplate(string prompt, DocumentationSectionType sectionType)
        {
            List<string> issues = new List<string>();
            
            if (string.IsNullOrWhiteSpace(prompt))
            {
                issues.Add("Prompt template cannot be empty");
                return issues;
            }

            // Check for required placeholders
            if (!prompt.Contains("{PROJECT_CONTEXT}"))
            {
                issues.Add("Prompt template must contain {PROJECT_CONTEXT} placeholder");
            }

            // Section-specific validation
            switch (sectionType)
            {
                case DocumentationSectionType.SystemArchitecture:
                    if (!prompt.Contains("architecture") && !prompt.Contains("Architecture"))
                    {
                        issues.Add("System Architecture prompt should reference architecture concepts");
                    }
                    break;
                    
                case DocumentationSectionType.DataModel:
                    if (!prompt.Contains("data") && !prompt.Contains("Data"))
                    {
                        issues.Add("Data Model prompt should reference data concepts");
                    }
                    break;
                    
                case DocumentationSectionType.APISpecification:
                    if (!prompt.Contains("API") && !prompt.Contains("api") && !prompt.Contains("interface"))
                    {
                        issues.Add("API Specification prompt should reference API or interface concepts");
                    }
                    break;
                    
                case DocumentationSectionType.UserStories:
                    if (!prompt.Contains("user") && !prompt.Contains("User"))
                    {
                        issues.Add("User Stories prompt should reference user concepts");
                    }
                    break;
                    
                case DocumentationSectionType.WorkTickets:
                    if (!prompt.Contains("task") && !prompt.Contains("ticket") && !prompt.Contains("implementation"))
                    {
                        issues.Add("Work Tickets prompt should reference task or implementation concepts");
                    }
                    break;
            }

            return issues;
        }

        /// <summary>
        /// Gets recommended word count range for section type
        /// </summary>
        public static (int min, int max) GetRecommendedWordCount(DocumentationSectionType sectionType)
        {
            return sectionType switch
            {
                DocumentationSectionType.GeneralProductDescription => (300, 500),
                DocumentationSectionType.SystemArchitecture => (500, 800),
                DocumentationSectionType.DataModel => (400, 600),
                DocumentationSectionType.APISpecification => (600, 900),
                DocumentationSectionType.UserStories => (500, 700),
                DocumentationSectionType.WorkTickets => (600, 800),
                _ => (400, 600)
            };
        }
    }
}