# Unity Project Architect - User Manual

**Version 0.3.0** - Complete Guide for Project Documentation & Organization

## 📋 Table of Contents

1. [Getting Started](#getting-started)
2. [Main Features](#main-features)
3. [Unity Editor Integration](#unity-editor-integration)
4. [Documentation Generation](#documentation-generation)
5. [Project Analysis](#project-analysis)
6. [Template System](#template-system)
7. [Export & Sharing](#export--sharing)
8. [Configuration](#configuration)
9. [Troubleshooting](#troubleshooting)
10. [Advanced Usage](#advanced-usage)

---

## Getting Started

### Installation

**Method 1: Package Manager (Recommended)**
1. Open Unity Package Manager (`Window > Package Manager`)
2. Click the `+` button → `Add package from git URL`
3. Enter the package URL or browse to the `.unitypackage` file
4. Wait for Unity to import and compile the package

**Method 2: Manual Import**
1. Download the `.unitypackage` file
2. In Unity: `Assets > Import Package > Custom Package`
3. Select the downloaded file and import all contents

### First Launch

1. After installation, go to `Window > Unity Project Architect > Main Window`
2. The package will automatically detect your project
3. You'll see a welcome screen with setup options

### Initial Setup

1. **Create Project Data Asset**:
   - Click "Create New Project Data Asset"
   - Choose a location in your Assets folder
   - The asset stores all your project documentation and settings

2. **Configure Project Type**:
   - Select your project type (2D Game, 3D Game, VR, Mobile, etc.)
   - This optimizes templates and analysis for your specific needs

3. **Optional AI Setup**:
   - For enhanced features, add your Claude API key in Settings
   - AI features work alongside manual documentation creation

---

## Main Features

### 📖 Documentation Generation

**6 Standard Documentation Sections:**

1. **General Description**: Project overview, goals, and key features
2. **System Architecture**: Technical structure and design patterns
3. **Data Model**: ScriptableObjects, data flow, and relationships
4. **API Specification**: Interface documentation and usage examples
5. **User Stories**: Feature requirements and acceptance criteria
6. **Work Tickets**: Implementation tasks and development roadmap

**Generation Methods:**
- **AI-Powered**: Intelligent content generation based on your project
- **Template-Based**: Pre-structured content you can customize
- **Manual**: Full control over content creation and editing

### 🎯 Project Analysis

**Comprehensive Analysis Features:**
- **Structure Analysis**: Folder organization and file structure
- **Script Analysis**: Code patterns and dependencies
- **Asset Analysis**: Resource usage and optimization opportunities
- **Architecture Detection**: Identify design patterns and architectural decisions

**Real-time Insights:**
- Performance recommendations
- Organization suggestions
- Best practice compliance
- Technical debt identification

### 🏗️ Template System

**Pre-built Templates:**
- General Unity Project
- 2D Mobile Game
- 3D PC Game
- VR Experience
- Educational Project
- Prototype Project

**Custom Template Creation:**
- Design your own folder structures
- Configure scene templates
- Save for team sharing
- Import/export configurations

---

## Unity Editor Integration

### Main Window (`Ctrl+Shift+P`)

**Access**: `Window > Unity Project Architect > Main Window`

**Sections:**
1. **Header**: Project overview and version info
2. **Project Configuration**: Project Data Asset management
3. **Documentation Sections**: Enable/disable sections, generate content
4. **Export Options**: Choose output formats and locations
5. **Actions**: Analyze project, refresh data, open Template Creator

**Key Controls:**
- **Generate**: Create content for individual sections
- **Generate All Enabled**: Batch generation for selected sections
- **Export Markdown/PDF**: Export documentation in chosen format
- **Analyze Project**: Run comprehensive project analysis
- **Template Creator**: Open template design window

### Template Creator (`Ctrl+Shift+T`)

**Access**: `Window > Unity Project Architect > Template Creator`

**Features:**
1. **Template Information**: Name, description, and project type
2. **Folder Structure**: Design directory hierarchies
3. **Scene Configuration**: Set up default scenes
4. **Actions**: Save, load, and preview templates

**Workflow:**
1. Enter template name and description
2. Add folder paths (e.g., "Scripts/Managers", "Art/Textures")
3. Configure scene templates with names and paths
4. Save template for reuse
5. Preview before applying to projects

### Menu Integration

**Tools Menu**:
- Main Window
- Template Creator  
- Quick Analysis
- Export Documentation (Markdown/PDF)
- Settings
- About

**Window Menu**:
- Direct access to main windows
- Organized under "Unity Project Architect"

**Assets Context Menu** (Right-click on folders):
- Analyze Selected Folder
- Apply Template to Folder
- Generate Documentation for Asset

**Help Menu**:
- Documentation
- Report Issue
- Feature Request

---

## Documentation Generation

### Setting Up Documentation

1. **Enable Sections**:
   - In the Main Window, check the sections you want to generate
   - Each section has a toggle, word count, and status indicator
   - Green checkmarks indicate completed sections

2. **Configure Content**:
   - Click "Generate" for individual sections
   - Use "Generate All Enabled" for batch processing
   - Content is generated based on your project analysis

3. **Review and Edit**:
   - Generated content appears in the Project Data Asset
   - Edit content directly in the Inspector
   - Content updates automatically in export formats

### Documentation Sections Explained

**General Description**:
- Project overview and elevator pitch
- Key features and unique selling points
- Target audience and platform information
- Development timeline and team information

**System Architecture**:
- Technical architecture overview
- Design patterns used in the project
- Component relationships and dependencies
- Performance considerations and scalability

**Data Model**:
- ScriptableObject definitions and usage
- Data flow and state management
- Serialization and persistence strategies
- Database or save system architecture

**API Specification**:
- Public interfaces and their usage
- Method signatures and parameters
- Usage examples and best practices
- Integration guidelines for team members

**User Stories**:
- Feature requirements in user story format
- Acceptance criteria and definition of done
- Priority levels and implementation order
- User experience considerations

**Work Tickets**:
- Implementation tasks and technical requirements
- Bug fixes and technical debt items
- Testing and quality assurance tasks
- Deployment and maintenance activities

---

## Project Analysis

### Running Analysis

**Automatic Analysis**:
- Runs when you open the Main Window
- Updates when project files change
- Provides real-time insights

**Manual Analysis**:
- Click "Analyze Project" in the Main Window
- Use "Quick Analysis" from the Tools menu
- Right-click folders for targeted analysis

### Understanding Analysis Results

**Metrics Section**:
- Analysis time and completion status
- Project size and complexity metrics
- File count and organization statistics

**Insights Section**:
- Color-coded insights (Info: Blue, Warning: Yellow, Critical: Red)
- Architecture observations
- Performance recommendations
- Organization suggestions

**Recommendations Section**:
- Priority-based suggestions (High, Medium, Low)
- Effort estimates for implementation
- Actionable next steps
- Best practice compliance

### Acting on Recommendations

1. **Review Recommendations**: Prioritize based on impact and effort
2. **Apply Actions**: Click "Apply" buttons for automated fixes
3. **Track Progress**: Monitor implementation through analysis updates
4. **Validate Changes**: Re-run analysis to see improvements

---

## Template System

### Using Pre-built Templates

1. **Access Templates**:
   - Open Template Creator window
   - Browse available templates in the dropdown
   - Preview template structure

2. **Apply Templates**:
   - Select template matching your project type
   - Click "Apply Template"
   - Choose target folder or apply to entire project

3. **Customize Application**:
   - Select which folders to create
   - Choose whether to merge with existing structure
   - Configure scene generation options

### Creating Custom Templates

**Template Design Process**:

1. **Define Information**:
   - Template name (e.g., "Mobile 2D Game")
   - Description explaining use case
   - Project type classification

2. **Design Folder Structure**:
   - Add folder paths one by one
   - Use forward slashes for hierarchy (e.g., "Scripts/Managers")
   - Consider standard Unity conventions

3. **Configure Scenes**:
   - Add scene templates with names and paths
   - Mark one scene as default
   - Configure scene hierarchy if needed

4. **Save and Test**:
   - Save template to Assets/Templates folder
   - Test application on new projects
   - Share with team members

**Template Best Practices**:
- Follow Unity naming conventions
- Create logical groupings (Scripts, Art, Audio, etc.)
- Include standard folders (Prefabs, Materials, Scenes)
- Consider project scalability
- Document template purpose and usage

### Sharing Templates

**Export Templates**:
- Save template assets to version control
- Export template configurations as .asset files
- Share templates through Unity packages

**Import Templates**:
- Load template .asset files into your project
- Templates appear automatically in Template Creator
- Team members can access shared templates

---

## Export & Sharing

### Export Formats

**Markdown Export**:
- GitHub-ready documentation with table of contents
- Emoji icons for visual appeal
- Code blocks and formatting preserved
- Suitable for README files and wikis

**PDF Export**:
- Professional document formatting
- Styled headings and sections
- Suitable for stakeholders and documentation packages
- Print-ready format

**Unity Assets**:
- ScriptableObject integration
- In-editor reference and access
- Linked to project data
- Version controlled with project

### Export Configuration

**Output Settings**:
1. Choose export format (Markdown/PDF)
2. Select output location
3. Configure filename and metadata
4. Choose sections to include

**Batch Export**:
- Export all enabled sections at once
- Maintain consistent formatting
- Automatic table of contents generation
- Progress tracking during export

### Sharing Documentation

**Team Collaboration**:
- Include Project Data Assets in version control
- Export documentation to shared locations
- Use consistent naming conventions
- Regular updates and synchronization

**External Sharing**:
- PDF exports for stakeholders
- Markdown for technical documentation
- Online hosting of exported content
- Integration with project management tools

---

## Configuration

### Project Settings

**Access**: `Edit > Project Settings > Unity Project Architect`

**AI Configuration**:
- Claude API key setup
- Content generation preferences
- Language and style settings
- Rate limiting and usage controls

**Export Settings**:
- Default output locations
- File naming conventions
- Format preferences
- Template customization

**Analysis Settings**:
- Analysis frequency and triggers
- Included file types and folders
- Performance thresholds
- Custom analysis rules

### Project Data Asset

**Inspector Configuration**:
- Project metadata (name, description, version)
- Team information and contacts
- Documentation section settings
- Template preferences

**Advanced Settings**:
- Custom documentation templates
- Analysis exclusion rules
- Export customization
- Integration preferences

---

## Troubleshooting

### Common Issues

**Package Won't Load**:
- Verify Unity version compatibility (2023.3+)
- Check console for compilation errors
- Reimport package if necessary
- Restart Unity Editor

**Documentation Generation Fails**:
- Check Project Data Asset is properly configured
- Verify enabled sections have required project information
- Review console for specific error messages
- Try generating individual sections first

**Export Problems**:
- Ensure output directory exists and is writable
- Check file name doesn't contain invalid characters
- Verify sufficient disk space
- Try different export formats

**Analysis Issues**:
- Large projects may take longer to analyze
- Exclude unnecessary folders from analysis
- Check for corrupt or inaccessible files
- Restart analysis if it appears stuck

### Performance Optimization

**Large Projects**:
- Use selective analysis on specific folders
- Exclude build outputs and temporary files
- Configure analysis settings for your project size
- Consider breaking large projects into modules

**Memory Usage**:
- Close unused documentation windows
- Clear analysis cache periodically
- Monitor Unity's memory usage during analysis
- Restart Unity if memory usage becomes excessive

### Getting Help

**Built-in Help**:
- Hover tooltips on UI elements
- Status messages in the Main Window
- Progress indicators during operations
- Error messages with specific guidance

**External Resources**:
- Package documentation and guides
- GitHub Issues for bug reports
- Community forums and discussions
- Feature request submissions

---

## Advanced Usage

### Scripting Integration

**Access Project Data in Code**:
```csharp
// Find Project Data Asset
var projectData = FindObjectOfType<UnityProjectDataAsset>();

// Access documentation sections
foreach (var section in projectData.ProjectData.DocumentationSections)
{
    Debug.Log($"Section: {section.SectionType}, Status: {section.Status}");
}
```

**Custom Analysis**:
- Extend analysis capabilities with custom scripts
- Add project-specific metrics and insights
- Integrate with existing development tools
- Automate documentation updates

### Automation

**CI/CD Integration**:
- Generate documentation as part of build process
- Validate documentation completeness
- Export documentation to deployment locations
- Trigger analysis on code changes

**Custom Workflows**:
- Create automated documentation pipelines
- Schedule regular analysis updates
- Integrate with project management systems
- Custom notification and reporting

### Team Workflows

**Standardization**:
- Establish team documentation standards
- Create shared template libraries
- Define analysis and review processes
- Maintain consistent project structures

**Collaboration**:
- Regular documentation reviews
- Shared responsibility for content updates
- Version control integration
- Cross-team template sharing

---

## Tips & Best Practices

### Documentation

1. **Start Early**: Begin documentation when project structure is defined
2. **Update Regularly**: Keep documentation current with development
3. **Be Specific**: Include concrete details and examples
4. **Review Often**: Regular team reviews maintain quality
5. **Export Regularly**: Keep external documentation synchronized

### Project Organization

1. **Use Templates**: Start with appropriate project templates
2. **Follow Conventions**: Stick to Unity and team naming standards  
3. **Regular Analysis**: Run analysis after major changes
4. **Act on Insights**: Address recommendations promptly
5. **Share Standards**: Maintain team consistency

### Team Collaboration

1. **Shared Templates**: Create and maintain team template library
2. **Documentation Standards**: Establish content and format guidelines
3. **Regular Reviews**: Schedule documentation review sessions
4. **Version Control**: Include all project data in source control
5. **Training**: Ensure team members understand the tool

---

**Need more help?** Check the package README, GitHub issues, or community forums for additional support and examples.

**Happy documenting!** 🚀