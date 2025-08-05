# Unity Project Architect - Testing & Validation Guide

## Package Validation

### Installation Verification

**After Package Installation:**
1. Create a new Unity project (Unity 2023.3 or later)
2. Import the package through Package Manager
3. Verify package appears in `Window > Unity Project Architect`

**Expected Results:**
- No compilation errors in Console
- Menu items appear under `Window > Unity Project Architect`
- Package version displays correctly in About dialog

### Feature Testing Checklist

**Core Functionality:**
- [ ] ✅ Main Window opens without errors
- [ ] ✅ Template Creator window functions properly
- [ ] ✅ Project Data Asset creation works
- [ ] ✅ Project analysis completes successfully
- [ ] ✅ Documentation generation produces content
- [ ] ✅ Export functions (Markdown/PDF) work correctly

**User Interface:**
- [ ] ✅ All UI elements render correctly
- [ ] ✅ Progress bars and status indicators update
- [ ] ✅ Menu items respond appropriately
- [ ] ✅ Keyboard shortcuts function (Ctrl+Shift+P, Ctrl+Shift+T)

**Integration:**
- [ ] ✅ Asset context menus appear on folders
- [ ] ✅ Project Settings integration works
- [ ] ✅ ScriptableObject creation and editing functions

## Manual Testing Workflow

### Basic Workflow Test
1. **Open Main Window**: `Window > Unity Project Architect > Main Window`
2. **Create Project Data**: Click "Create New Project Data Asset"
3. **Configure Project**: Set project type and enable documentation sections
4. **Run Analysis**: Click "Analyze Project" and verify results display
5. **Generate Documentation**: Select sections and generate content
6. **Export Documentation**: Test both Markdown and PDF export

### Template System Test
1. **Open Template Creator**: `Window > Unity Project Architect > Template Creator`
2. **Create Custom Template**: Design folder structure and scene configuration
3. **Save Template**: Export template configuration
4. **Apply Template**: Use template in a new test project
5. **Verify Structure**: Confirm folders and scenes are created correctly

### AI Integration Test (Optional)
1. **Configure API Key**: Add Claude API key in Project Settings
2. **Enable AI Features**: Verify AI-powered generation options appear
3. **Generate AI Content**: Test AI-enhanced documentation generation
4. **Fallback Testing**: Verify system works without API key

## Automated Testing

### Unity Test Runner
1. Open `Window > General > Test Runner`
2. Switch to `PlayMode` tab
3. Look for `Unity.ProjectArchitect.Tests`
4. Run test suite to verify core functionality

### Expected Test Results
- **DLL Integration**: Services initialize correctly
- **Data Persistence**: ScriptableObjects save and load properly
- **UI Components**: Windows open and function without errors
- **Export System**: Files generate with correct formatting

## Console Messages

### Normal Operation
**Successful Initialization:**
```
Unity Project Architect Service Bridge initialized with DLL services
Project data loaded successfully
Analysis completed: X files processed
```

**Feature Usage:**
```
Template applied: [TemplateName] 
Documentation generated: [SectionName]
Export completed: [OutputPath]
```

### Error Indicators
**Common Issues:**
- Missing dependencies: Check Package Manager
- API key errors: Verify Claude API configuration
- File access errors: Check file permissions and paths
- Performance warnings: Consider project size and complexity

## Performance Validation

### Acceptable Performance Ranges
- **Package Import**: < 30 seconds
- **Window Opening**: < 2 seconds
- **Project Analysis**: < 10 seconds for typical projects
- **Documentation Generation**: < 5 seconds per section
- **Export Operations**: < 15 seconds for complete documentation

### Large Project Considerations
- Projects with 1000+ scripts may take longer to analyze
- Consider selective analysis for very large codebases
- Monitor Unity memory usage during operations
- Use incremental analysis when possible

## Troubleshooting Common Issues

### Installation Problems
**Package Won't Import:**
- Verify Unity version compatibility (2023.3+)
- Check available disk space
- Clear Package Manager cache
- Try manual .unitypackage import

**Compilation Errors:**
- Restart Unity Editor
- Reimport package completely
- Check for conflicting packages
- Review Console for specific errors

### Runtime Issues
**Windows Won't Open:**
- Check for Console errors
- Verify package assemblies loaded correctly
- Try resetting window layouts
- Restart Unity if necessary

**Features Not Working:**
- Verify Project Data Asset is created and configured
- Check project path accessibility
- Ensure required permissions for file operations
- Review API key configuration if using AI features

## Quality Assurance Checklist

### Pre-Release Validation
- [ ] ✅ Package installs cleanly in fresh Unity project
- [ ] ✅ All windows and UI elements function correctly
- [ ] ✅ Documentation generation produces expected output
- [ ] ✅ Export functions create properly formatted files
- [ ] ✅ Template system creates correct folder structures
- [ ] ✅ Performance meets acceptable thresholds
- [ ] ✅ Console shows no errors during normal operation
- [ ] ✅ Memory usage remains stable during extended use

### Cross-Platform Testing
- [ ] ✅ Windows Unity Editor
- [ ] ✅ macOS Unity Editor  
- [ ] ✅ Linux Unity Editor (if applicable)

### Version Compatibility
- [ ] ✅ Unity 2023.3 LTS
- [ ] ✅ Unity 6 (latest)
- [ ] ✅ Future Unity versions (as available)

---

**For Developers:** Additional technical testing procedures and development-specific validation steps are documented in the development workflow files.
- `✅ Project validation completed`
- `✅ Export service supports X formats`
- `✅ All DLL types are accessible from Unity`

## Troubleshooting

**"Service not initialized" errors:**
- Check console for initialization error messages
- Ensure all three DLLs are in `Runtime/Plugins/` folder

**Interface implementation errors:**
- Re-import the package if you see interface mismatch errors
- Check that all DLL files are the latest version

**DLL not found errors:**
- Verify DLLs are in `Runtime/Plugins/` folder: Core.dll, AI.dll, Services.dll
- Check assembly definition references include all three DLLs

**Type not found errors:**
- Ensure assembly definitions include DLL references
- Unity 6000.0.53+ required for .NET Standard 2.1 compatibility