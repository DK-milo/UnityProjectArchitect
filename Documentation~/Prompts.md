# Unity Project Architect - Context Transfer for Claude Code

## Project Overview
I'm developing a Unity package called **"Unity Project Architect"** - an AI-powered project management and documentation tool for Unity developers. This is a 30-hour MVP project structured in 3 phases.

### Core Concept
A Unity Editor package that helps developers:
- Generate complete project documentation (6 standard sections)
- Create project templates and structures automatically
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