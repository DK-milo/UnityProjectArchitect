# Coding Conventions & Unity Package Guidelines
## Unity Project Architect

**Version:** 1.0  
**Date:** July 21, 2025  

---

## 1. Unity Package Development Best Practices

### 1.1 Package Structure
```
com.unitprojectarchitect.core/
├── package.json                 # Package metadata
├── CHANGELOG.md                 # Version history
├── README.md                    # Package overview
├── LICENSE.md                   # License information
├── Runtime/                     # Runtime code
│   ├── *.asmdef                 # Assembly definition
│   ├── Core/                    # Core data models
│   ├── UI/                      # UI Toolkit components
│   └── API/                     # Public interfaces
├── Editor/                      # Editor-only code
│   ├── *.asmdef                 # Editor assembly definition
│   ├── Windows/                 # Editor windows
│   ├── MenuItems/               # Menu integrations
│   └── Inspectors/              # Custom inspectors
├── Tests/                       # Unit and integration tests
│   ├── Runtime/                 # Runtime tests
│   └── Editor/                  # Editor tests
├── Documentation~/              # Package documentation
├── Samples~/                    # Sample projects
└── Third Party Notices.md       # Third-party licenses
```

### 1.2 Assembly Definition Guidelines
- **Runtime Assembly**: `UnityProjectArchitect.Runtime`
  - Contains all runtime functionality
  - References only Unity core packages
  - Auto-referenced for easy user consumption
- **Editor Assembly**: `UnityProjectArchitect.Editor`
  - Contains all editor-only functionality
  - References Runtime assembly
  - Platform-restricted to Editor only
- **Test Assemblies**: Separate assemblies for Runtime and Editor tests
  - Include test runner references
  - Use `UNITY_INCLUDE_TESTS` define constraint

---

## 2. C# Coding Standards

### 2.1 Naming Conventions
```csharp
// Namespaces: PascalCase with company/package prefix
namespace UnityProjectArchitect.Core
namespace UnityProjectArchitect.Editor.Windows

// Classes: PascalCase
public class DocumentationGenerator
public class ProjectArchitectWindow

// Interfaces: PascalCase with 'I' prefix
public interface IDocumentationGenerator
public interface IAIAssistant

// Methods: PascalCase
public void GenerateDocumentation()
private bool ValidateProject()

// Properties: PascalCase
public string ProjectName { get; set; }
public bool IsValid { get; private set; }

// Fields: camelCase with underscore prefix for private
private string _projectPath;
public readonly string DefaultTemplatePath;

// Constants: PascalCase
public const string PackageVersion = "1.0.0";
private const int MaxRetryAttempts = 3;

// Events: PascalCase
public event System.Action<DocumentationResult> DocumentationGenerated;

// Enums: PascalCase for type and values
public enum DocumentationType
{
    GeneralDescription,
    SystemArchitecture,
    DataModel
}
```

### 2.2 Variable Declarations
**ALWAYS use explicit types instead of var keyword:**
```csharp
// ✅ CORRECT - Use explicit types
StringBuilder sb = new StringBuilder();
List<ClassDefinition> classes = new List<ClassDefinition>();
Dictionary<string, string> properties = new Dictionary<string, string>();
IEnumerable<UserStory> stories = GenerateStories().Where(s => s.Priority == "High");

// ❌ INCORRECT - Do not use var
var sb = new StringBuilder();
var classes = new List<ClassDefinition>();
var properties = new Dictionary<string, string>();
var stories = GenerateStories().Where(s => s.Priority == "High");

// Exception: var is acceptable only for anonymous types and complex generic types where the type is immediately obvious
var anonymousObject = new { Name = "Test", Value = 42 };
var query = from item in collection
            where item.IsValid
            select new { item.Name, item.Count };
```

**Rationale:**
- Explicit types improve code readability and maintainability
- Makes the code self-documenting
- Helps catch type-related bugs at compile time
- Aligns with Unity's coding standards for packages
- Easier for other developers to understand the codebase

### 2.3 Code Organization
```csharp
// File header template
/*
 * Unity Project Architect
 * Copyright (c) 2025 Unity Project Architect Team
 * Licensed under MIT License
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; // Only in Editor assemblies

namespace UnityProjectArchitect.Core
{
    /// <summary>
    /// Handles automatic generation of project documentation.
    /// Supports multiple output formats and AI-enhanced content creation.
    /// </summary>
    public class DocumentationGenerator : IDocumentationGenerator
    {
        #region Fields
        private readonly ProjectData _projectData;
        private readonly List<IDocumentationSection> _sections;
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; }
        public int SectionCount => _sections?.Count ?? 0;
        #endregion

        #region Events
        public event Action<DocumentationResult> DocumentationGenerated;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            Initialize();
        }
        #endregion

        #region Public Methods
        public DocumentationResult GenerateDocumentation(DocumentationSettings settings)
        {
            // Implementation
        }
        #endregion

        #region Private Methods
        private void Initialize()
        {
            // Implementation
        }
        #endregion

        #region Event Handlers
        private void OnDocumentationCompleted(DocumentationResult result)
        {
            DocumentationGenerated?.Invoke(result);
        }
        #endregion
    }
}
```

### 2.3 Unity-Specific Patterns

#### ScriptableObject Data Containers
```csharp
[CreateAssetMenu(fileName = "ProjectData", menuName = "Unity Project Architect/Project Data")]
public class ProjectData : ScriptableObject
{
    [Header("Project Information")]
    [SerializeField] private string _projectName;
    [SerializeField] private string _projectDescription;
    
    [Header("Documentation Settings")]
    [SerializeField] private DocumentationType[] _enabledSections;
    
    public string ProjectName => _projectName;
    public string ProjectDescription => _projectDescription;
    public IReadOnlyList<DocumentationType> EnabledSections => _enabledSections;
    
    public void Initialize(string name, string description)
    {
        _projectName = name;
        _projectDescription = description;
    }
}
```

#### Editor Window Implementation
```csharp
public class ProjectArchitectWindow : EditorWindow
{
    private VisualElement _rootElement;
    private ProjectData _currentProject;
    
    [MenuItem("Tools/Project Architect/Main Window")]
    public static void ShowWindow()
    {
        var window = GetWindow<ProjectArchitectWindow>();
        window.titleContent = new GUIContent("Project Architect");
        window.Show();
    }
    
    public void CreateGUI()
    {
        _rootElement = rootVisualElement;
        LoadUIDocument();
        BindUIElements();
    }
    
    private void LoadUIDocument()
    {
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Packages/com.unitprojectarchitect.core/Editor/UI/ProjectArchitectWindow.uxml");
        visualTree.CloneTree(_rootElement);
    }
}
```

---

## 3. Unity UI Toolkit Conventions

### 3.1 UXML Structure
```xml
<!-- ProjectArchitectWindow.uxml -->
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:uie="UnityEditor.UIElements">
    <ui:VisualElement name="root-container" class="root-container">
        <ui:VisualElement name="header" class="header">
            <ui:Label text="Unity Project Architect" class="title" />
        </ui:VisualElement>
        
        <ui:VisualElement name="content" class="content">
            <ui:TwoPaneSplitView fixed-pane-initial-dimension="200">
                <ui:VisualElement name="sidebar" class="sidebar">
                    <!-- Navigation elements -->
                </ui:VisualElement>
                <ui:VisualElement name="main-content" class="main-content">
                    <!-- Main content area -->
                </ui:VisualElement>
            </ui:TwoPaneSplitView>
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

### 3.2 USS Styling
```css
/* ProjectArchitectWindow.uss */
.root-container {
    flex: 1;
    background-color: var(--unity-colors-window-background);
}

.header {
    height: 30px;
    background-color: var(--unity-colors-toolbar-background);
    border-bottom-width: 1px;
    border-color: var(--unity-colors-toolbar-border);
    padding: 4px 8px;
}

.title {
    font-size: 14px;
    font-style: bold;
    color: var(--unity-colors-label-text);
    -unity-text-align: middle-left;
}

.content {
    flex: 1;
    padding: 8px;
}
```

---

## 4. API Design Principles

### 4.1 Public Interface Design
```csharp
// Service interfaces - clean, focused responsibilities
public interface IDocumentationGenerator
{
    DocumentationResult GenerateDocumentation(DocumentationRequest request);
    Task<DocumentationResult> GenerateDocumentationAsync(DocumentationRequest request);
    bool CanGenerate(ProjectData projectData);
}

// Data contracts - immutable when possible
public readonly struct DocumentationRequest
{
    public ProjectData ProjectData { get; }
    public DocumentationType[] Sections { get; }
    public DocumentationSettings Settings { get; }
    
    public DocumentationRequest(ProjectData projectData, DocumentationType[] sections, DocumentationSettings settings)
    {
        ProjectData = projectData ?? throw new ArgumentNullException(nameof(projectData));
        Sections = sections ?? throw new ArgumentNullException(nameof(sections));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }
}

// Result types - clear success/failure states
public sealed class DocumentationResult
{
    public bool IsSuccess { get; }
    public string[] GeneratedSections { get; }
    public string ErrorMessage { get; }
    public Exception Exception { get; }
    
    private DocumentationResult(bool isSuccess, string[] sections, string errorMessage, Exception exception)
    {
        IsSuccess = isSuccess;
        GeneratedSections = sections ?? Array.Empty<string>();
        ErrorMessage = errorMessage;
        Exception = exception;
    }
    
    public static DocumentationResult Success(string[] sections) => 
        new DocumentationResult(true, sections, null, null);
        
    public static DocumentationResult Failure(string errorMessage, Exception exception = null) => 
        new DocumentationResult(false, null, errorMessage, exception);
}
```

### 4.2 Configuration and Settings
```csharp
// Settings classes should be ScriptableObjects for Unity integration
[CreateAssetMenu(fileName = "DocumentationSettings", menuName = "Unity Project Architect/Documentation Settings")]
public class DocumentationSettings : ScriptableObject
{
    [Header("Output Configuration")]
    [SerializeField] private DocumentationFormat _outputFormat = DocumentationFormat.Markdown;
    [SerializeField] private string _outputPath = "Documentation/Generated";
    
    [Header("AI Configuration")]
    [SerializeField] private bool _enableAI = true;
    [SerializeField] private string _customPromptTemplate;
    
    public DocumentationFormat OutputFormat => _outputFormat;
    public string OutputPath => _outputPath;
    public bool EnableAI => _enableAI;
    public string CustomPromptTemplate => _customPromptTemplate;
    
    // Validation
    public ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(_outputPath))
            return ValidationResult.Error("Output path cannot be empty");
            
        if (_enableAI && string.IsNullOrWhiteSpace(EditorPrefs.GetString("UnityProjectArchitect.ApiKey")))
            return ValidationResult.Warning("AI features enabled but no API key configured");
            
        return ValidationResult.Success();
    }
}
```

---

## 5. Error Handling & Logging

### 5.1 Exception Handling
```csharp
public class ProjectArchitectException : Exception
{
    public string Context { get; }
    
    public ProjectArchitectException(string message, string context = null) 
        : base(message)
    {
        Context = context;
    }
    
    public ProjectArchitectException(string message, Exception innerException, string context = null) 
        : base(message, innerException)
    {
        Context = context;
    }
}

// Usage in service methods
public async Task<DocumentationResult> GenerateDocumentationAsync(DocumentationRequest request)
{
    try
    {
        var result = await GenerateInternalAsync(request);
        return DocumentationResult.Success(result);
    }
    catch (ProjectArchitectException ex)
    {
        Debug.LogError($"[ProjectArchitect] Documentation generation failed: {ex.Message}");
        return DocumentationResult.Failure(ex.Message, ex);
    }
    catch (Exception ex)
    {
        Debug.LogException(ex);
        return DocumentationResult.Failure("Unexpected error during documentation generation", ex);
    }
}
```

### 5.2 Logging Standards
```csharp
public static class ProjectArchitectLogger
{
    private const string LogPrefix = "[ProjectArchitect]";
    
    public static void LogInfo(string message) => 
        Debug.Log($"{LogPrefix} {message}");
    
    public static void LogWarning(string message) => 
        Debug.LogWarning($"{LogPrefix} {message}");
    
    public static void LogError(string message) => 
        Debug.LogError($"{LogPrefix} {message}");
    
    public static void LogException(Exception exception, string context = null) =>
        Debug.LogException(new ProjectArchitectException($"Exception in {context}", exception, context));
}
```

---

## 6. Testing Conventions

### 6.1 Unit Test Structure
```csharp
[TestFixture]
public class DocumentationGeneratorTests
{
    private DocumentationGenerator _generator;
    private ProjectData _testProjectData;
    
    [SetUp]
    public void Setup()
    {
        _generator = new DocumentationGenerator();
        _testProjectData = ScriptableObject.CreateInstance<ProjectData>();
        _testProjectData.Initialize("Test Project", "Test Description");
    }
    
    [TearDown]
    public void TearDown()
    {
        if (_testProjectData != null)
            Object.DestroyImmediate(_testProjectData);
    }
    
    [Test]
    public void GenerateDocumentation_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var request = new DocumentationRequest(
            _testProjectData, 
            new[] { DocumentationType.GeneralDescription }, 
            CreateDefaultSettings());
        
        // Act
        var result = _generator.GenerateDocumentation(request);
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotEmpty(result.GeneratedSections);
    }
    
    [Test]
    public void GenerateDocumentation_WithNullProjectData_ThrowsException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new DocumentationRequest(null, new DocumentationType[0], CreateDefaultSettings()));
    }
    
    private DocumentationSettings CreateDefaultSettings()
    {
        var settings = ScriptableObject.CreateInstance<DocumentationSettings>();
        // Configure test settings
        return settings;
    }
}
```

### 6.2 Editor Test Structure
```csharp
[TestFixture]
public class ProjectArchitectWindowTests
{
    private ProjectArchitectWindow _window;
    
    [SetUp]
    public void Setup()
    {
        _window = EditorWindow.GetWindow<ProjectArchitectWindow>();
    }
    
    [TearDown]
    public void TearDown()
    {
        if (_window != null)
            _window.Close();
    }
    
    [UnityTest]
    public IEnumerator Window_Opens_Successfully()
    {
        // Arrange & Act
        ProjectArchitectWindow.ShowWindow();
        
        yield return null; // Wait one frame
        
        // Assert
        var openWindow = EditorWindow.GetWindow<ProjectArchitectWindow>();
        Assert.IsNotNull(openWindow);
        Assert.IsTrue(openWindow.hasFocus);
    }
}
```

---

## 7. Performance Guidelines

### 7.1 Memory Management
- Use object pooling for frequently created/destroyed objects
- Dispose of temporary assets properly
- Cache expensive operations (file I/O, API calls)
- Use weak references for event subscriptions to prevent memory leaks

### 7.2 Async Operations
```csharp
public async Task<DocumentationResult> GenerateDocumentationAsync(DocumentationRequest request, CancellationToken cancellationToken = default)
{
    using (var progressReporter = new ProgressReporter("Generating documentation..."))
    {
        var tasks = request.Sections.Select(async (section, index) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            progressReporter.Report((float)index / request.Sections.Length, $"Processing {section}...");
            return await GenerateSectionAsync(section, request.ProjectData, cancellationToken);
        });
        
        var results = await Task.WhenAll(tasks);
        return DocumentationResult.Success(results);
    }
}
```

---

## 8. Version Control & Package Maintenance

### 8.1 Version Numbering
- Follow semantic versioning (MAJOR.MINOR.PATCH)
- Update version in package.json for each release
- Maintain CHANGELOG.md with release notes

### 8.2 Backward Compatibility
- Mark obsolete APIs with `[System.Obsolete]` attribute
- Provide migration path for breaking changes
- Support at least 2 major Unity versions simultaneously

### 8.3 Package Publishing
- Validate package with Unity Package Validation Suite
- Test on multiple Unity versions before release
- Include comprehensive samples and documentation

---

**Document Status**: Draft v1.0  
**Next Review**: After initial code implementation  
**Approval Required**: Senior Unity Developer