# Unity Project Architect - User Manual

## Overview
Unity Project Architect is an AI-powered Unity Editor package that helps you create professional project documentation and organize your Unity projects with intelligent templates.

## What You Can Do

### 📁 **Project Organization**
- **Apply Project Templates**: Choose from built-in templates (General, Mobile 2D, PC 3D) to instantly create organized folder structures
- **Create Custom Templates**: Save your current project structure as a reusable template for future projects
- **Smart Folder Management**: Automatically organize assets into standard Unity folders (Scripts, Prefabs, Materials, etc.)

### 📖 **Documentation Generation**
- **6 Standard Documentation Sections**: Automatically generate complete project documentation including:
  - General Product Description
  - System Architecture
  - Data Model
  - API Specification
  - User Stories
  - Work Tickets
- **AI-Powered Content**: Use Claude API integration for intelligent documentation generation
- **Export Options**: Export documentation to Markdown or PDF formats

### 🎯 **Project Management**
- **Project Analysis**: Analyze existing project structure and generate insights
- **Progress Tracking**: Monitor documentation completion and project organization status
- **Team Collaboration**: Standardize project structures across team members

## How to Use

### 1. **Installation**
1. Open Unity Package Manager
2. Add package from git URL or import the `.unitypackage`
3. Package will appear in `Window > Unity Project Architect`

### 2. **First Time Setup**
1. Go to `Window > Unity Project Architect`
2. Create or load a `ProjectData` asset for your project
3. Configure your project type (2D, 3D, VR, Mobile, etc.)
4. (Optional) Add your Claude API key for AI features

### 3. **Apply a Project Template**
1. Open the Project Architect window
2. Click "Templates" tab
3. Select a template that matches your project type:
   - **General Unity Project**: Standard folders for any Unity project
   - **Mobile 2D Game**: Optimized for 2D mobile games with sprites and UI folders
   - **PC 3D Game**: Complete 3D setup with models, shaders, and lighting folders
4. Click "Apply Template"
5. The plugin will create the folder structure and organize your project

### 4. **Generate Documentation**
1. In the Project Architect window, go to "Documentation" tab
2. Configure which sections you want to generate
3. For AI-powered generation:
   - Ensure Claude API key is configured
   - Customize prompts for each section
4. Click "Generate Documentation"
5. Review and edit the generated content
6. Export to Markdown or PDF when ready

### 5. **Create Custom Templates**
1. Organize your project structure as desired
2. Go to "Templates" tab
3. Click "Create Template from Current Project"
4. Name your template and add description
5. Save for reuse in future projects

## Key Features

### ✅ **What the Plugin Creates**
- Organized folder structures
- Unity scenes with basic setup
- Assembly definition files
- Complete project documentation
- Template assets for reuse

### ❌ **What the Plugin Does NOT Create**
- C# scripts or gameplay code
- MonoBehaviour components
- Game mechanics or logic
- Art assets or content

## Requirements

### **Minimum Requirements**
- Unity 6 (6000.0.0f1+)
- No external dependencies

### **Optional for Enhanced Features**
- Claude API key for AI-powered documentation
- Internet connection for AI features

## Tips for Best Results

1. **Start Early**: Apply templates when creating new projects for best organization
2. **Customize Templates**: Create project-specific templates for your team's workflow
3. **Regular Documentation**: Update documentation as your project evolves
4. **AI Prompts**: Customize AI prompts to match your project's specific needs
5. **Team Standards**: Use consistent templates across your team for better collaboration

## Support

For issues, feature requests, or contributions, visit the project repository or contact the development team.

---

*Unity Project Architect - Simplifying Unity project management and documentation since 2025*