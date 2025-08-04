using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityProjectArchitect.Core
{
    // Analysis Results - ProjectAnalysisResult is defined in IProjectAnalyzer.cs

    public class StructureAnalysis
    {
        public List<FolderInfo> Folders { get; set; } = new List<FolderInfo>();
        public List<FileInfo> Files { get; set; } = new List<FileInfo>();
        public List<StructureIssue> Issues { get; set; } = new List<StructureIssue>();
        public bool FollowsStandardStructure { get; set; }
        public StructureMetrics Metrics { get; set; } = new StructureMetrics();
    }

    public class ScriptAnalysis
    {
        public List<ClassDefinition> Classes { get; set; } = new List<ClassDefinition>();
        public List<InterfaceDefinition> Interfaces { get; set; } = new List<InterfaceDefinition>();
        public List<CodeIssue> Issues { get; set; } = new List<CodeIssue>();
        public CodeMetrics Metrics { get; set; } = new CodeMetrics();
        public DependencyGraph Dependencies { get; set; } = new DependencyGraph();
        public List<DesignPattern> DetectedPatterns { get; set; } = new List<DesignPattern>();
    }

    public class ArchitectureAnalysis
    {
        public ArchitecturePattern DetectedPattern { get; set; } = ArchitecturePattern.None;
        public List<ComponentInfo> Components { get; set; } = new List<ComponentInfo>();
        public List<SystemConnection> Connections { get; set; } = new List<SystemConnection>();
        public List<LayerInfo> Layers { get; set; } = new List<LayerInfo>();
        public List<ArchitectureIssue> Issues { get; set; } = new List<ArchitectureIssue>();
        public ArchitectureMetrics Metrics { get; set; } = new ArchitectureMetrics();
    }

    public class AssetAnalysis
    {
        public List<AssetInfo> Assets { get; set; } = new List<AssetInfo>();
        public List<AssetIssue> Issues { get; set; } = new List<AssetIssue>();
        public AssetMetrics Metrics { get; set; } = new AssetMetrics();
        public AssetUsageReport UsageReport { get; set; } = new AssetUsageReport();
    }

    // Core Data Models
    public class FolderInfo
    {
        public string Path { get; set; } = "";
        public string Name { get; set; } = "";
        public int FileCount { get; set; }
        public int SubfolderCount { get; set; }
        public long SizeBytes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class FileInfo
    {
        public string Path { get; set; } = "";
        public string Name { get; set; } = "";
        public string Extension { get; set; } = "";
        public long SizeBytes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public FileType Type { get; set; }
        public int LineCount { get; set; }
        public List<string> Dependencies { get; set; } = new List<string>();
    }

    public class ClassDefinition
    {
        public string Name { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Namespace { get; set; } = "";
        public string FilePath { get; set; } = "";
        public ClassType Type { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public List<string> BaseClasses { get; set; } = new List<string>();
        public List<string> Interfaces { get; set; } = new List<string>();
        public List<MethodDefinition> Methods { get; set; } = new List<MethodDefinition>();
        public List<PropertyDefinition> Properties { get; set; } = new List<PropertyDefinition>();
        public List<FieldDefinition> Fields { get; set; } = new List<FieldDefinition>();
        public List<string> Attributes { get; set; } = new List<string>();
        public int LinesOfCode { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public float Complexity { get; set; }

        public bool IsMonoBehaviour => BaseClasses.Contains("MonoBehaviour") || BaseClasses.Contains("UnityEngine.MonoBehaviour");
        public bool IsScriptableObject => BaseClasses.Contains("ScriptableObject") || BaseClasses.Contains("UnityEngine.ScriptableObject");
    }

    public class InterfaceDefinition
    {
        public string Name { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Namespace { get; set; } = "";
        public string FilePath { get; set; } = "";
        public AccessModifier AccessModifier { get; set; }
        public List<string> BaseInterfaces { get; set; } = new List<string>();
        public List<MethodSignature> Methods { get; set; } = new List<MethodSignature>();
        public List<PropertySignature> Properties { get; set; } = new List<PropertySignature>();
        public int LinesOfCode { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
    }

    public class MethodDefinition
    {
        public string Name { get; set; } = "";
        public string ReturnType { get; set; } = "";
        public AccessModifier AccessModifier { get; set; }
        public List<ParameterDefinition> Parameters { get; set; } = new List<ParameterDefinition>();
        public List<string> Attributes { get; set; } = new List<string>();
        public int LinesOfCode { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public float CyclomaticComplexity { get; set; }
        public bool IsAsync { get; set; }
        public bool IsStatic { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsOverride { get; set; }
    }

    public class PropertyDefinition
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public AccessModifier AccessModifier { get; set; }
        public bool HasGetter { get; set; }
        public bool HasSetter { get; set; }
        public bool IsAutoProperty { get; set; }
        public List<string> Attributes { get; set; } = new List<string>();
    }

    public class FieldDefinition
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public AccessModifier AccessModifier { get; set; }
        public bool IsStatic { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsConst { get; set; }
        public string DefaultValue { get; set; } = "";
        public List<string> Attributes { get; set; } = new List<string>();
    }

    public class ParameterDefinition
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string DefaultValue { get; set; } = "";
        public bool IsOut { get; set; }
        public bool IsRef { get; set; }
        public bool IsParams { get; set; }
    }

    public class MethodSignature
    {
        public string Name { get; set; } = "";
        public string ReturnType { get; set; } = "";
        public List<ParameterDefinition> Parameters { get; set; } = new List<ParameterDefinition>();
    }

    public class PropertySignature
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public bool HasGetter { get; set; }
        public bool HasSetter { get; set; }
    }

    // Analysis Issues
    public class StructureIssue
    {
        public StructureIssueType Type { get; set; }
        public string Description { get; set; } = "";
        public string Path { get; set; } = "";
        public StructureIssueSeverity Severity { get; set; }
        public string Suggestion { get; set; } = "";
    }

    public class CodeIssue
    {
        public CodeIssueType Type { get; set; }
        public CodeIssueSeverity Severity { get; set; }
        public string Description { get; set; } = "";
        public string FilePath { get; set; } = "";
        public int LineNumber { get; set; }
        public string CodeSnippet { get; set; } = "";
        public string Suggestion { get; set; } = "";
        public string RuleId { get; set; } = "";
    }

    public class ArchitectureIssue
    {
        public ArchitectureIssueType Type { get; set; }
        public ArchitectureIssueSeverity Severity { get; set; }
        public string Description { get; set; } = "";
        public List<string> AffectedComponents { get; set; } = new List<string>();
        public string Suggestion { get; set; } = "";
    }

    public class AssetIssue
    {
        public AssetIssueType Type { get; set; }
        public AssetIssueSeverity Severity { get; set; }
        public string Description { get; set; } = "";
        public string AssetPath { get; set; } = "";
        public string Suggestion { get; set; } = "";
    }

    public class PerformanceIssue
    {
        public PerformanceIssueType Type { get; set; }
        public string Description { get; set; } = "";
        public string Location { get; set; } = "";
        public PerformanceImpact Impact { get; set; }
        public string Suggestion { get; set; } = "";
    }

    // Metrics
    public class StructureMetrics
    {
        public int TotalFolders { get; set; }
        public int TotalFiles { get; set; }
        public int MaxDepth { get; set; }
        public float AverageFilesPerFolder { get; set; }
        public long TotalSizeBytes { get; set; }
    }

    public class CodeMetrics
    {
        public int TotalLinesOfCode { get; set; }
        public int LinesOfCodeWithoutComments { get; set; }
        public int CommentLines { get; set; }
        public int BlankLines { get; set; }
        public float AverageCyclomaticComplexity { get; set; }
        public float MaxCyclomaticComplexity { get; set; }
        public int TotalMethods { get; set; }
        public int TotalClasses { get; set; }
        public float CodeDuplication { get; set; }
        public float TestCoverage { get; set; }
        public Dictionary<string, int> MetricsByType { get; set; } = new Dictionary<string, int>();

        public float CommentRatio => TotalLinesOfCode > 0 ? (float)CommentLines / TotalLinesOfCode : 0f;
        public float MethodsPerClass => TotalClasses > 0 ? (float)TotalMethods / TotalClasses : 0f;
    }

    public class ArchitectureMetrics
    {
        public int TotalComponents { get; set; }
        public int TotalConnections { get; set; }
        public float AverageCoupling { get; set; }
        public float AverageCohesion { get; set; }
        public int CyclomaticComplexity { get; set; }
        public float Instability { get; set; }
        public float Abstractness { get; set; }

        public float DistanceFromMainSequence => Math.Abs(Abstractness + Instability - 1);
    }

    public class PerformanceMetrics
    {
        public int DrawCalls { get; set; }
        public int Vertices { get; set; }
        public int Triangles { get; set; }
        public long TextureMemoryMB { get; set; }
        public long MeshMemoryMB { get; set; }
        public int AudioClips { get; set; }
        public long AudioMemoryMB { get; set; }
        public Dictionary<string, float> CustomMetrics { get; set; } = new Dictionary<string, float>();
    }

    public class AssetMetrics
    {
        public int TotalAssets { get; set; }
        public long TotalSizeBytes { get; set; }
        public Dictionary<string, int> AssetCountByType { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, long> AssetSizeByType { get; set; } = new Dictionary<string, long>();
        public int UnusedAssets { get; set; }
        public int MissingAssets { get; set; }
        public float AverageAssetSize { get; set; }

        public string FormattedTotalSize => FormatBytes(TotalSizeBytes);

        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }

    // Additional Models
    public class PerformanceAnalysis
    {
        public List<PerformanceIssue> Issues { get; set; } = new List<PerformanceIssue>();
        public PerformanceMetrics Metrics { get; set; } = new PerformanceMetrics();
        public List<PerformanceRecommendation> Recommendations { get; set; } = new List<PerformanceRecommendation>();
    }

    public class PerformanceRecommendation
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public PerformanceImpact ExpectedImpact { get; set; }
        public int ImplementationEffort { get; set; }
    }

    public class DependencyGraph
    {
        public List<DependencyNode> Nodes { get; set; } = new List<DependencyNode>();
        public List<DependencyEdge> Edges { get; set; } = new List<DependencyEdge>();
        public Dictionary<string, List<string>> DirectDependencies { get; set; } = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> ReverseDependencies { get; set; } = new Dictionary<string, List<string>>();

        public List<string> GetCircularDependencies()
        {
            List<string> cycles = new List<string>();
            HashSet<string> visited = new HashSet<string>();
            HashSet<string> recursionStack = new HashSet<string>();

            foreach (var node in Nodes)
            {
                if (!visited.Contains(node.Id))
                {
                    FindCycles(node.Id, visited, recursionStack, cycles, new List<string>());
                }
            }

            return cycles;
        }

        private void FindCycles(string nodeId, HashSet<string> visited, HashSet<string> recursionStack, 
                              List<string> cycles, List<string> currentPath)
        {
            visited.Add(nodeId);
            recursionStack.Add(nodeId);
            currentPath.Add(nodeId);

            if (DirectDependencies.ContainsKey(nodeId))
            {
                foreach (string dependency in DirectDependencies[nodeId])
                {
                    if (!visited.Contains(dependency))
                    {
                        FindCycles(dependency, visited, recursionStack, cycles, new List<string>(currentPath));
                    }
                    else if (recursionStack.Contains(dependency))
                    {
                        int cycleStart = currentPath.IndexOf(dependency);
                        string cycle = string.Join(" -> ", currentPath.Skip(cycleStart).Concat(new[] { dependency }));
                        cycles.Add(cycle);
                    }
                }
            }

            recursionStack.Remove(nodeId);
        }
    }

    public class DependencyNode
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string FilePath { get; set; } = "";
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class DependencyEdge
    {
        public string FromId { get; set; } = "";
        public string ToId { get; set; } = "";
        public DependencyType DependencyType { get; set; }
        public string Description { get; set; } = "";
    }

    public class DesignPattern
    {
        public DesignPatternType Type { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> InvolvedClasses { get; set; } = new List<string>();
        public float Confidence { get; set; }
        public string Evidence { get; set; } = "";
    }

    public class AssetInfo
    {
        public string Path { get; set; } = "";
        public string Name { get; set; } = "";
        public string AssetType { get; set; } = "";
        public long SizeBytes { get; set; }
        public DateTime ImportedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ImporterType { get; set; } = "";
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public List<string> Labels { get; set; } = new List<string>();
    }

    public class AssetDependency
    {
        public string AssetPath { get; set; } = "";
        public string DependencyPath { get; set; } = "";
        public AssetDependencyType Type { get; set; }
        public bool IsDirectDependency { get; set; }
    }

    public class AssemblyDefinitionInfo
    {
        public string Path { get; set; } = "";
        public string Name { get; set; } = "";
        public List<string> References { get; set; } = new List<string>();
        public List<string> DefineConstraints { get; set; } = new List<string>();
        public List<string> VersionDefines { get; set; } = new List<string>();
        public bool AutoReferenced { get; set; }
        public bool NoEngineReferences { get; set; }
        public AssemblyDefinitionPlatforms Platforms { get; set; } = new AssemblyDefinitionPlatforms();
    }

    public class AssemblyDefinitionPlatforms
    {
        public bool IncludesAnyPlatform { get; set; }
        public List<string> IncludePlatforms { get; set; } = new List<string>();
        public List<string> ExcludePlatforms { get; set; } = new List<string>();
    }

    public class SceneInfo
    {
        public string Path { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsInBuildSettings { get; set; }
        public int BuildIndex { get; set; } = -1;
        public bool IsEnabled { get; set; }
        public int GameObjectCount { get; set; }
        public List<string> ComponentTypes { get; set; } = new List<string>();
        public long FileSizeBytes { get; set; }
    }

    public class ExportOption
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsEnabled { get; set; } = true;
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
    }

    public class AssetUsageReport
    {
        public List<AssetUsageInfo> UsageInfos { get; set; } = new List<AssetUsageInfo>();
        public List<string> UnusedAssets { get; set; } = new List<string>();
        public List<string> MissingAssets { get; set; } = new List<string>();
        public Dictionary<string, int> UsageByType { get; set; } = new Dictionary<string, int>();
    }

    public class AssetUsageInfo
    {
        public string AssetPath { get; set; } = "";
        public int UsageCount { get; set; }
        public List<string> UsedByAssets { get; set; } = new List<string>();
        public List<string> UsedByScenes { get; set; } = new List<string>();
    }

    public class ComponentInfo
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public ComponentCategory Category { get; set; }
        public List<string> Dependencies { get; set; } = new List<string>();
        public List<string> Dependents { get; set; } = new List<string>();
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }

    public class SystemConnection
    {
        public string FromComponent { get; set; } = "";
        public string ToComponent { get; set; } = "";
        public ConnectionType Type { get; set; }
        public string Description { get; set; } = "";
        public float Strength { get; set; }
    }

    public class LayerInfo
    {
        public string Name { get; set; } = "";
        public int Level { get; set; }
        public List<string> Components { get; set; } = new List<string>();
        public string Description { get; set; } = "";
    }

    // Enums (continuation from previous files)
    public enum FileType
    {
        Script, Scene, Prefab, Material, Texture, Audio, Mesh, Animation, Shader, Font, Other
    }

    public enum ClassType
    {
        Class, Struct, Interface, Enum, Delegate
    }

    public enum AccessModifier
    {
        Public, Private, Protected, Internal, ProtectedInternal, PrivateProtected
    }

    public enum StructureIssueType
    {
        MissingFolder, UnconventionalNaming, DeepNesting, LargeFile, CircularDependency, MisplacedFile
    }

    public enum StructureIssueSeverity
    {
        Info, Warning, Error, Critical
    }

    public enum DependencyType
    {
        Inheritance, Composition, Usage, Reference, Import
    }

    public enum CodeIssueType
    {
        CodeSmell, BestPracticeViolation, PotentialBug, PerformanceIssue, SecurityIssue, StyleViolation
    }

    public enum CodeIssueSeverity
    {
        Info, Minor, Major, Critical
    }

    public enum DesignPatternType
    {
        Singleton, Factory, Observer, Command, Strategy, Decorator, Adapter, Facade, Proxy, Builder, 
        Prototype, TemplateMethod, State, Bridge, Composite, Flyweight, ChainOfResponsibility, 
        Mediator, Memento, Visitor, Interpreter
    }

    public enum AssetIssueType
    {
        MissingAsset, UnusedAsset, LargeAsset, UnoptimizedAsset, DuplicateAsset, BrokenReference
    }

    public enum AssetIssueSeverity
    {
        Info, Warning, Error, Critical
    }

    public enum AssetDependencyType
    {
        Direct, Indirect, Cyclic, Unused, Missing
    }

    public enum ComponentCategory
    {
        Core, UI, Gameplay, Utility, External, ThirdParty
    }

    public enum ConnectionType
    {
        DataFlow, ControlFlow, Dependency, Inheritance, Composition, Usage
    }

    public enum ArchitecturePattern
    {
        None, MVC, MVP, MVVM, Layered, EventDriven, Microkernel, ServiceOriented, 
        ComponentBased, EntityComponentSystem
    }

    public enum ArchitectureIssueType
    {
        LayerViolation, CircularDependency, TightCoupling, LowCohesion, GodClass, DeadCode, UnusedInterface
    }

    public enum ArchitectureIssueSeverity
    {
        Info, Minor, Major, Critical
    }

    public enum PerformanceIssueType
    {
        HighDrawCalls, LargeTextureSize, UnoptimizedMesh, ExcessiveMemoryUsage, SlowScript, 
        UnbatchedRendering, LargeAudioFile
    }

    public enum PerformanceImpact
    {
        Low, Medium, High, Critical
    }
}