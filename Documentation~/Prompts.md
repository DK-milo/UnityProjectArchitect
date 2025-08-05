# Unity Project Architect - Context Transfer for Claude Code

## Project Overview
I'm developing a Unity package called **"Unity Project Architect"** - an AI-powered project management and documentation tool for Unity developers. This is a 30-hour MVP project structured in 3 phases.

### Core Concept
A Unity Editor package that helps developers:
- Generate complete project documentation (6 standard sections)
- Create project templates and structures automatically
- Organize folder hierarchies and project setup (no script generation)
- Use AI assistance (Claude API) for planning and documentation
- Manage development workflows within Unity Editor

## Technical Stack
- **Unity 6 (6000.0.0f1+)** with UI Toolkit
- **C# .NET** for all logic
- **Unity Package Manager** distribution
- **Claude API integration** (user provides API key)
- **JetBrains Rider** as IDE
- **Git** for version control

## Project Timeline (30 hours total)

### Stage 1: Technical Documentation (8 hours)
**Status**: Starting now
**Deliverables**:
1. Product Requirements Document (PRD) - 2h
2. System Architecture - 2h  
3. Technical Specification - 1.5h
4. Data Model - 1h
5. Testing Plan - 1h
6. Development Roadmap - 0.5h

### Stage 2: Functional Code (18 hours)
**Sprint 2.1**: Core Framework (6h) - Package structure, data models, template system
**Sprint 2.2**: Documentation Generator (6h) - Auto-analysis, export system
**Sprint 2.3**: AI Integration (6h) - Claude API, prompt engineering

### Stage 3: Final Delivery (4 hours)
Testing, demo project, user documentation, package publishing

## Package Architecture

### Target Structure:
```
com.yourname.project-architect/
├── package.json
├── Runtime/
│   ├── Core/
│   │   ├── ProjectData.cs (ScriptableObject)
│   │   ├── DocumentationGenerator.cs
│   │   └── TemplateManager.cs
│   ├── UI/ (UI Toolkit components)
│   └── API/
│       ├── ClaudeIntegration.cs
│       └── AIAssistant.cs
├── Editor/
│   ├── Windows/
│   │   ├── ProjectArchitectWindow.cs (Main editor window)
│   │   └── TemplateCreatorWindow.cs
│   └── MenuItems/
├── Tests/
├── Documentation~/
└── Samples~/
```

## Key Features to Implement

### 1. Template System
- Project type templates (2D, 3D, VR, Mobile)
- Automatic folder structure generation
- Assembly Definition setup
- Scene templates

### 2. Documentation Generator (6 required sections)
- **General Product Description**
- **System Architecture** (with auto-generated diagrams)
- **Data Model** (from ScriptableObjects analysis)
- **API Specification** (internal package API)
- **User Stories** (AI-generated from project description)
- **Work Tickets** (breakdown of development tasks)

### 3. AI Integration
- Claude API integration (user's API key)
- Specialized prompts for each documentation section
- Project analysis and suggestions
- Automatic content generation

### 4. Unity Editor Integration
- Custom Editor Window with UI Toolkit
- Project analysis tools
- Export system (Markdown, PDF, Unity assets)
- Real-time documentation updates

## Development Environment Setup

### Required Tools (for development only):
- Unity 6 with UI Toolkit package
- JetBrains Rider
- Git repository
- Mermaid CLI (for diagram templates)
- Pandoc (for document conversion)

### User Requirements (for final package):
- Unity 6 only
- Optional: Claude API key for AI features

## Current Status & Immediate Tasks

**STARTING WITH STAGE 1 - DOCUMENTATION**

**Next immediate task**: Create the Product Requirements Document (PRD) defining:
- Exact feature scope
- User personas and use cases  
- Technical constraints and requirements
- Success metrics
- Package API design

## Key Design Principles

1. **Zero external dependencies** for end users
2. **Plug-and-play** installation via Unity Package Manager
3. **AI-optional** - works without API key, enhanced with it
4. **Template-based** - pre-generated assets, not runtime generation
5. **Unity-native** - follows Unity conventions and patterns
6. **Structure-only templates** - creates folders, scenes, and documentation; users write their own scripts

## Development Guidelines

- Use Unity 2023.3+ compatible code
- Follow Unity package development best practices
- Implement proper Assembly Definition structure  
- Create comprehensive test suite
- Document all public APIs
- Use UI Toolkit for all editor interfaces

## Success Criteria

**MVP Complete when**:
- Package installs cleanly in any Unity 6 project
- All 6 documentation sections generate automatically  
- Claude API integration works with user-provided keys
- Template system creates functional project structures
- Editor UI is intuitive and bug-free
- Complete test coverage

## Request for Claude Code

Please help me start with **Stage 1: Technical Documentation**, beginning with the **Product Requirements Document (PRD)**. 

I need you to:
1. Set up the proper package structure in my Rider project
2. Create the initial package.json and assembly definitions
3. Help me write a comprehensive PRD that defines this package's exact scope and requirements
4. Guide me through the system architecture design
5. Establish coding patterns and conventions for the Unity package

I want to follow Unity package development best practices and create professional-grade documentation that will guide the entire development process.

Can you help me start by setting up the project structure and beginning the PRD creation?

---

# Development Props

## Stage 1: Technical Documentation (COMPLETED ✅)

### Prompt 1A: Product Requirements Document Creation
```
Create a comprehensive PRD for Unity Project Architect following these technical specifications:

REQUIREMENTS:
- 30-hour MVP timeline with 3 development stages (Stage 1: 8h, Stage 2: 18h, Stage 3: 4h)
- Unity 6+ compatibility with UI Toolkit integration
- AI-powered documentation generation using Claude API
- Structure-only templates (no script generation)
- 6 standard documentation sections: General Product Description, System Architecture, Data Model, API Specification, User Stories, Work Tickets

DELIVERABLES:
1. Executive Summary with key value propositions
2. Target user personas (Solo Developer, Team Lead, Freelance Developer) with specific use cases
3. Complete functional requirements breakdown (Core MVP + Post-MVP features)
4. Technical requirements including platform compatibility, performance benchmarks, security considerations
5. Detailed development roadmap with substage breakdown and hour estimates
6. Package API design with extension points
7. Acceptance criteria and definition of done

FORMAT: Professional technical document with clear section hierarchy, measurable success metrics, and detailed technical constraints.
```

### Prompt 1B: System Architecture Design
```
Design the complete system architecture for Unity Project Architect package:

TECHNICAL REQUIREMENTS:
- Modular design with clear separation of concerns
- ScriptableObject-based data persistence
- Interface-driven service architecture
- Unity Package Manager distribution structure
- Assembly Definition organization for Runtime/Editor separation

ARCHITECTURE COMPONENTS:
1. Core Data Models: ProjectData, DocumentationSection, TemplateConfiguration, ProjectSettings, ValidationResult
2. Service Interfaces: IDocumentationGenerator, ITemplateManager, IAIAssistant, IProjectAnalyzer, IExportService, IValidationService
3. Template Management System: FolderStructureManager, TemplateValidator, ConflictResolver
4. Analysis Engine: ProjectAnalyzer, ScriptAnalyzer, AssetAnalyzer, InsightGenerator, RecommendationEngine
5. AI Integration: ClaudeAPIClient, PromptTemplateManager, AIAssistant
6. Unity Editor Integration: Custom EditorWindows with UI Toolkit, MenuItems

IMPLEMENTATION APPROACH:
- Use dependency injection pattern for service management
- Implement async/await for long-running operations with progress reporting
- Apply SOLID principles throughout the codebase
- Include comprehensive error handling and logging
- Design for testability with 80%+ code coverage target
```

## Stage 2.1: Core Framework (COMPLETED ✅)

### Prompt 2.1A: Core Data Models Implementation
```
Implement the complete core data model architecture for Unity Project Architect:

STEP-BY-STEP IMPLEMENTATION:
1. CREATE ProjectData.cs ScriptableObject:
   - Project metadata (name, description, version, type, Unity version)
   - Team and contact information
   - Documentation settings with 6 standard sections
   - AI integration preferences
   - Template configuration references
   - Automatic change tracking with MarkModified()

2. CREATE DocumentationSection.cs:
   - Enum for 6 section types (GeneralProductDescription, SystemArchitecture, DataModel, APISpecification, UserStories, WorkTickets)
   - Section data structure with content, status, last updated
   - Validation rules and content requirements
   - Export formatting options

3. CREATE TemplateConfiguration.cs:
   - Project type definitions (General, Game2D, Game3D, VR, AR, Mobile, Tool, Template)
   - Folder structure templates with hierarchical organization
   - Scene template configurations
   - Assembly definition templates
   - Validation and conflict resolution support

4. CREATE ProjectSettings.cs:
   - Global AI provider settings (Claude API configuration)
   - Export preferences (formats, paths, naming conventions)
   - Performance settings (analysis depth, caching options)
   - User interface preferences

5. CREATE ValidationResult.cs:
   - Comprehensive validation framework
   - Issue tracking with severity levels (Info, Warning, Error, Critical)
   - Validation rules engine
   - Detailed error reporting with suggestions

TECHNICAL REQUIREMENTS:
- All classes must inherit from ScriptableObject where appropriate
- Include proper serialization attributes
- Implement IValidatable interface where needed
- Add comprehensive XML documentation
- Include Unity-specific attributes for editor integration
```

### Prompt 2.1B: Service Interfaces Design
```
Create the complete service interface architecture following SOLID principles:

INTERFACE IMPLEMENTATIONS REQUIRED:

1. IDocumentationGenerator.cs:
   - Async documentation generation methods for all 6 sections
   - Project analysis integration
   - Export format support (Markdown, PDF, Unity assets)
   - Progress reporting with cancellation support
   - Template-based generation with AI enhancement options

2. ITemplateManager.cs:
   - Template creation, validation, and application
   - Folder structure management
   - Assembly definition generation
   - Scene template application
   - Conflict resolution and rollback mechanisms

3. IAIAssistant.cs:
   - Claude API integration with secure key management
   - Section-specific content generation
   - Project analysis and recommendations
   - Conversation management for multi-turn interactions
   - Fallback mechanisms for offline operation

4. IProjectAnalyzer.cs:
   - Comprehensive project structure analysis
   - Script dependency analysis with circular detection
   - Asset usage analysis and optimization recommendations
   - Architecture pattern detection
   - Performance analysis with actionable insights

5. IExportService.cs:
   - Multi-format export pipeline (Markdown, PDF, Unity assets)
   - Template-based formatting
   - Batch export operations
   - Custom export format support

6. IValidationService.cs:
   - Project validation with detailed reporting
   - Template validation and compatibility checking
   - Content validation for AI-generated sections
   - Real-time validation with immediate feedback

DESIGN PATTERNS TO IMPLEMENT:
- Repository pattern for data access
- Factory pattern for service creation
- Observer pattern for progress reporting
- Strategy pattern for different export formats
- Command pattern for undoable operations
```

### Prompt 2.1C: Template System Foundation
```
Implement the complete template management system:

CORE COMPONENTS TO BUILD:

1. TemplateManager.cs Service Implementation:
   - Template discovery and loading from package resources
   - Dynamic template application with progress reporting
   - Template customization and parameter injection
   - Version management and compatibility checking
   - Template sharing and import/export functionality

2. FolderStructureManager.cs:
   - Hierarchical folder creation with proper Unity asset database integration
   - Standard Unity folder patterns (Scripts, Prefabs, Materials, Textures, Audio, etc.)
   - Custom folder structure support with validation
   - Folder template inheritance and composition
   - Automatic .meta file generation and GUID management

3. TemplateValidator.cs:
   - Template integrity checking
   - Dependency validation
   - Unity version compatibility verification
   - Asset reference validation
   - Custom validation rule support with extensible rule engine

4. ConflictResolver.cs:
   - File and folder conflict detection
   - Multiple resolution strategies (Overwrite, Skip, Rename, Merge)
   - User interaction for conflict resolution decisions
   - Rollback mechanisms for failed applications
   - Detailed conflict reporting and logging

TECHNICAL IMPLEMENTATION DETAILS:
- Use Unity's AssetDatabase API for all file operations
- Implement proper async/await patterns for file I/O
- Include comprehensive error handling with specific exception types
- Support undo/redo operations through Unity's Undo system
- Integrate with Unity's progress bar system for long operations
- Include unit tests with mock file system for testing

FILE STRUCTURE REQUIREMENTS:
- Support nested folder hierarchies with unlimited depth
- Handle special Unity folders (Editor, Resources, StreamingAssets, Plugins)
- Assembly definition file generation with proper dependencies
- Scene template creation with default GameObjects and components
- Package dependency management and validation
```

## Sprint 2.2A: Project Analysis Engine (COMPLETED ✅)

### Prompt 2.2A: Project Analysis Engine Implementation
```
Build a comprehensive project analysis engine with intelligent insights and recommendations:

CORE ANALYSIS COMPONENTS:

1. AnalysisDataModels.cs - Complete Data Model Set:
   - ClassDefinition, MethodDefinition, PropertyDefinition, FieldDefinition with full C# metadata
   - AssetInfo, AssetDependency, AssetUsageReport with Unity-specific analysis
   - ProjectInsight with severity levels and confidence scoring
   - ProjectRecommendation with detailed action steps and effort estimation
   - Performance, Architecture, and Dependency analysis models
   - 15+ supporting enums and data structures

2. ProjectAnalyzer.cs - Main Analysis Service:
   - Async project analysis with progress reporting (10 stages)
   - Integration of script, asset, and structure analyzers
   - Comprehensive error handling and logging
   - Project metrics calculation (lines of code, complexity, technical debt)
   - Insight and recommendation generation pipeline
   - Event-driven architecture for real-time updates

3. ScriptAnalyzer.cs - C# Code Analysis Engine:
   - Regex-based class and interface extraction from source files
   - Method, property, and field analysis with access modifiers
   - Dependency graph building with circular dependency detection
   - Design pattern detection (Singleton, Factory, Observer, etc.)
   - Cyclomatic complexity calculation for methods and classes
   - Code issue detection (god classes, high complexity, naming conventions)

4. AssetAnalyzer.cs - Unity Asset Analysis:
   - Support for 15+ Unity asset types (textures, materials, meshes, audio, etc.)
   - YAML parsing for asset dependency analysis
   - Asset usage reporting and unused asset detection
   - Metadata extraction from .meta files (import settings, compression, etc.)
   - Performance issue detection (large files, unoptimized settings)
   - Asset optimization recommendations

5. ProjectStructureAnalyzer.cs - Organization Analysis:
   - Unity project structure validation against standard patterns
   - File type classification and organization analysis
   - Assembly definition parsing and dependency mapping
   - Scene content analysis (GameObject counts, component types)
   - Project type detection (2D/3D/VR/AR/Mobile) using heuristics
   - Unity version detection from ProjectSettings

6. InsightGenerator.cs - Intelligent Analysis:
   - 7 categories of insights: Structure, CodeQuality, Performance, Architecture, Dependencies, Maintainability, Testing
   - Severity-based prioritization (Info, Low, Medium, High, Critical)
   - Confidence scoring based on evidence strength
   - Context-aware analysis with detailed evidence collection
   - Actionable insights with specific improvement suggestions

7. RecommendationEngine.cs - Improvement Recommendations:
   - Priority-based recommendation ranking (Low, Medium, High, Critical)
   - Detailed action steps with time estimates and prerequisites
   - Risk/benefit analysis for each recommendation
   - Effort estimation with complexity scoring (1-5 scale)
   - Required skills identification for implementation
   - 8 recommendation categories covering all aspects of project health

TECHNICAL IMPLEMENTATION REQUIREMENTS:
- Async/await throughout with CancellationToken support
- Comprehensive error handling with graceful degradation
- Memory-efficient processing for large projects
- Extensible architecture for adding new analysis types
- Unit testable with dependency injection
- Progress reporting with meaningful status messages
- Caching system for expensive analysis operations

ANALYSIS DEPTH FEATURES:
- Circular dependency detection with path visualization
- Code complexity metrics with industry standard thresholds
- Asset dependency chain analysis with unused asset identification
- Architecture pattern recognition with confidence scoring
- Performance bottleneck identification with specific recommendations
- Technical debt calculation based on code issues and complexity
- Maintainability scoring with actionable improvement paths
```
### Prompt 2.2B: Documentation Section Generators Implementation (COMPLETED ✅)
```
Implement the complete documentation section generator system for Unity Project Architect:

STEP-BY-STEP IMPLEMENTATION:
1. CREATE BaseDocumentationGenerator.cs abstract base class:
   - Common markdown generation utilities (headers, lists, tables, code blocks)
   - Project analysis result integration
   - Async task patterns with progress reporting
   - Metadata generation (timestamps, generation info)
   - Formatting helpers for insights, recommendations, and metrics

2. CREATE GeneralProductDescriptionGenerator.cs:
   - Project overview with type detection and Unity version
   - Key features extraction from project analysis
   - Technical highlights including code quality metrics
   - Project metrics and asset distribution
   - Important insights and improvement opportunities

3. CREATE SystemArchitectureGenerator.cs:
   - Architecture pattern detection and visualization
   - Component diagrams using Mermaid syntax
   - Layer architecture with dependency analysis
   - Circular dependency detection and reporting
   - Architectural patterns and Unity-specific approaches

4. CREATE DataModelGenerator.cs:
   - ScriptableObject documentation with properties and fields
   - Data class analysis and relationship mapping
   - Enumerations and value types documentation
   - Data model diagrams with inheritance and composition
   - Data architecture approach recommendations

5. CREATE APISpecificationGenerator.cs:
   - Public interface documentation with method signatures
   - Public class analysis with usage descriptions
   - Service API identification and documentation
   - Extension points and plugin architecture
   - Usage examples with sample code generation

6. CREATE UserStoriesGenerator.cs:
   - Epic generation based on project type and features
   - Feature-based user stories with acceptance criteria
   - Technical stories for infrastructure and performance
   - Story mapping and prioritization matrix
   - Release planning with story categorization

7. CREATE WorkTicketsGenerator.cs:
   - Implementation tickets from user stories breakdown
   - Bug fix tickets from analysis issues
   - Refactoring tickets from code quality analysis
   - Testing tickets with coverage targets
   - Documentation tickets and prioritization guidelines

TECHNICAL REQUIREMENTS:
- All generators inherit from BaseDocumentationGenerator
- Use ProjectAnalysisResult for contextual content generation
- Implement async patterns with Task.Run for CPU-bound operations
- Generate markdown with Mermaid diagrams for visualizations
- Include comprehensive error handling and null checks
- Support dynamic content based on project characteristics
```
## Next Stage Prompts (PENDING)

### Prompt 2.2B: Documentation Section Generators (PENDING ⏳)
```
NEXT IMPLEMENTATION: Create specialized generators for each of the 6 documentation sections, integrating with the completed analysis engine to produce contextual, project-specific documentation content.
```

### Prompt 3A: Claude API Integration (✅ COMPLETED)
```
IMPLEMENTED: Step 3A - Claude API Integration (2h)

Description: Complete Claude API integration with secure authentication, comprehensive error handling, and robust response parsing for AI-powered documentation generation.

Deliverables Completed:
- ClaudeAPIClient.cs: Full Claude API client with retry mechanisms, rate limiting, and secure authentication
- APIKeyManager.cs: Encrypted API key storage using AES encryption with integrity validation
- ResponseParser.cs: Comprehensive AI response parsing with content analysis and quality scoring
- ClaudeAPIModels.cs & ResponseModels.cs: Complete data models with validation framework
- AIIntegrationTest.cs: Mock testing suite for validation and integration testing

Technical Implementation:
- Secure API key management with AES-256 encryption
- Retry logic with exponential backoff for network resilience
- Comprehensive error handling and validation
- Thread-safe operations with async/await patterns
- Mock framework for offline testing and development
- Quality scoring and confidence metrics for AI responses

Integration Points:
- IProjectAnalyzer integration for context-aware prompts
- IDocumentationGenerator integration for AI-enhanced content
- Extensible prompt system for different documentation sections
- Fallback mechanisms for offline operation
```

### Prompt 3B: AI Prompt Engineering System (✅ COMPLETED)
```
IMPLEMENTED: Step 3B - Prompt Engineering System (2h)

Description: Comprehensive AI prompt engineering system with specialized templates, context building, and optimization for high-quality documentation generation.

Deliverables Completed:
- PromptTemplateManager.cs: Template management with caching, validation, and dynamic loading
- SectionSpecificPrompts.cs: 6 specialized prompt generators for each documentation section
- ContextBuilder.cs: Project-aware context building with intelligent truncation and analysis integration
- PromptOptimizer.cs: Token efficiency optimization and prompt quality analysis
- Step3BFunctionalityTest.cs: Comprehensive functional test suite for all components

Technical Implementation:
- Template caching system with TTL management and performance monitoring
- Context-aware prompt building with 2000-character intelligent truncation
- Quality scoring system (Clarity, Specificity, Completeness) with configurable thresholds
- Token count estimation and optimization for cost efficiency
- Extensible template system supporting filesystem-based custom templates
- Comprehensive validation framework with section-specific rules

Section-Specific Templates:
- General Product Description (300-500 words): Project overview with value proposition
- System Architecture (500-800 words): Technical structure with design patterns
- Data Model (400-600 words): ScriptableObject and data relationship documentation
- API Specification (600-900 words): Interface documentation with usage examples
- User Stories (500-700 words): Agile stories with acceptance criteria
- Work Tickets (600-800 words): Implementation tasks with complexity estimation

Context Building Features:
- Project overview extraction (name, type, Unity version, description)
- Technical context (team info, version, repository, AI settings)
- Content analysis (folder structure, file types, asset distribution)
- Architecture analysis (components, layers, connections, patterns)
- Script analysis (classes, interfaces, methods, design patterns)
- Asset analysis (count, size, type distribution, dependencies)
- Performance analysis with critical issue identification

Quality Assurance:
- Template validation with required placeholder verification
- Quality metrics calculation with weighted scoring
- Context validation and optimization
- Comprehensive test coverage with mock data
- Build verification with zero compilation errors

Integration Points:
- IProjectAnalyzer for dynamic context building
- IDocumentationGenerator for template-based content generation
- Claude API services for optimized prompt delivery
- Unity Editor for real-time template customization

Next Steps: Step 3C - AI Assistant service implementation with conversation management
```

### Prompt 4A: Unity Editor Integration (PENDING ⏳)
```
NEXT IMPLEMENTATION: Create comprehensive Unity Editor window using UI Toolkit, integrating all completed services into an intuitive user interface following Unity design patterns.
```