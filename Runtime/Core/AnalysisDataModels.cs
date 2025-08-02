using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityProjectArchitect.Core
{
    [Serializable]
    public class FolderInfo
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public int FileCount { get; set; }
        public int SubfolderCount { get; set; }
        public long SizeBytes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public List<string> Tags { get; set; }

        public FolderInfo()
        {
            Tags = new List<string>();
        }

        public FolderInfo(string path, string name) : this()
        {
            Path = path;
            Name = name;
        }
    }

    [Serializable]
    public class FileInfo
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public long SizeBytes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public FileType Type { get; set; }
        public int LineCount { get; set; }
        public List<string> Dependencies { get; set; }

        public FileInfo()
        {
            Dependencies = new List<string>();
        }

        public FileInfo(string path) : this()
        {
            Path = path;
            Name = System.IO.Path.GetFileName(path);
            Extension = System.IO.Path.GetExtension(path);
        }
    }

    [Serializable]
    public class AssemblyDefinitionInfo
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public List<string> References { get; set; }
        public List<string> DefineConstraints { get; set; }
        public List<string> VersionDefines { get; set; }
        public bool AutoReferenced { get; set; }
        public bool NoEngineReferences { get; set; }
        public AssemblyDefinitionPlatforms Platforms { get; set; }

        public AssemblyDefinitionInfo()
        {
            References = new List<string>();
            DefineConstraints = new List<string>();
            VersionDefines = new List<string>();
            Platforms = new AssemblyDefinitionPlatforms();
        }
    }

    [Serializable]
    public class AssemblyDefinitionPlatforms
    {
        public bool IncludesAnyPlatform { get; set; }
        public List<string> IncludePlatforms { get; set; }
        public List<string> ExcludePlatforms { get; set; }

        public AssemblyDefinitionPlatforms()
        {
            IncludePlatforms = new List<string>();
            ExcludePlatforms = new List<string>();
        }
    }

    [Serializable]
    public class SceneInfo
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public bool IsInBuildSettings { get; set; }
        public int BuildIndex { get; set; }
        public bool IsEnabled { get; set; }
        public int GameObjectCount { get; set; }
        public List<string> ComponentTypes { get; set; }
        public long FileSizeBytes { get; set; }

        public SceneInfo()
        {
            ComponentTypes = new List<string>();
            BuildIndex = -1;
        }

        public SceneInfo(string path) : this()
        {
            Path = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
        }
    }

    [Serializable]
    public class StructureIssue
    {
        public StructureIssueType Type { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public StructureIssueSeverity Severity { get; set; }
        public string Suggestion { get; set; }

        public StructureIssue() { }

        public StructureIssue(StructureIssueType type, string description, string path)
        {
            Type = type;
            Description = description;
            Path = path;
        }
    }

    [Serializable]
    public class ClassDefinition
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Namespace { get; set; }
        public string FilePath { get; set; }
        public ClassType Type { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public List<string> BaseClasses { get; set; }
        public List<string> Interfaces { get; set; }
        public List<MethodDefinition> Methods { get; set; }
        public List<PropertyDefinition> Properties { get; set; }
        public List<FieldDefinition> Fields { get; set; }
        public List<string> Attributes { get; set; }
        public int LinesOfCode { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public float Complexity { get; set; }

        public ClassDefinition()
        {
            BaseClasses = new List<string>();
            Interfaces = new List<string>();
            Methods = new List<MethodDefinition>();
            Properties = new List<PropertyDefinition>();
            Fields = new List<FieldDefinition>();
            Attributes = new List<string>();
        }

        public bool IsMonoBehaviour => BaseClasses.Contains("MonoBehaviour") || BaseClasses.Contains("UnityEngine.MonoBehaviour");
        public bool IsScriptableObject => BaseClasses.Contains("ScriptableObject") || BaseClasses.Contains("UnityEngine.ScriptableObject");
    }

    [Serializable]
    public class InterfaceDefinition
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Namespace { get; set; }
        public string FilePath { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public List<string> BaseInterfaces { get; set; }
        public List<MethodSignature> Methods { get; set; }
        public List<PropertySignature> Properties { get; set; }
        public int LinesOfCode { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }

        public InterfaceDefinition()
        {
            BaseInterfaces = new List<string>();
            Methods = new List<MethodSignature>();
            Properties = new List<PropertySignature>();
        }
    }

    [Serializable]
    public class MethodDefinition
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public List<ParameterDefinition> Parameters { get; set; }
        public List<string> Attributes { get; set; }
        public int LinesOfCode { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public float CyclomaticComplexity { get; set; }
        public bool IsAsync { get; set; }
        public bool IsStatic { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsOverride { get; set; }

        public MethodDefinition()
        {
            Parameters = new List<ParameterDefinition>();
            Attributes = new List<string>();
        }
    }

    [Serializable]
    public class PropertyDefinition
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public bool HasGetter { get; set; }
        public bool HasSetter { get; set; }
        public bool IsAutoProperty { get; set; }
        public List<string> Attributes { get; set; }

        public PropertyDefinition()
        {
            Attributes = new List<string>();
        }
    }

    [Serializable]
    public class FieldDefinition
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public bool IsStatic { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsConst { get; set; }
        public string DefaultValue { get; set; }
        public List<string> Attributes { get; set; }

        public FieldDefinition()
        {
            Attributes = new List<string>();
        }
    }

    [Serializable]
    public class ParameterDefinition
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string DefaultValue { get; set; }
        public bool IsOut { get; set; }
        public bool IsRef { get; set; }
        public bool IsParams { get; set; }

        public ParameterDefinition() { }

        public ParameterDefinition(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }

    [Serializable]
    public class MethodSignature
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public List<ParameterDefinition> Parameters { get; set; }

        public MethodSignature()
        {
            Parameters = new List<ParameterDefinition>();
        }
    }

    [Serializable]
    public class PropertySignature
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool HasGetter { get; set; }
        public bool HasSetter { get; set; }
    }

    [Serializable]
    public class DependencyGraph
    {
        public List<DependencyNode> Nodes { get; set; }
        public List<DependencyEdge> Edges { get; set; }
        public Dictionary<string, List<string>> DirectDependencies { get; set; }
        public Dictionary<string, List<string>> ReverseDependencies { get; set; }

        public DependencyGraph()
        {
            Nodes = new List<DependencyNode>();
            Edges = new List<DependencyEdge>();
            DirectDependencies = new Dictionary<string, List<string>>();
            ReverseDependencies = new Dictionary<string, List<string>>();
        }

        public List<string> GetCircularDependencies()
        {
            List<string> cycles = new List<string>();
            HashSet visited = new HashSet<string>();
            HashSet recursionStack = new HashSet<string>();

            foreach (string node in Nodes)
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
                        var cycleStart = currentPath.IndexOf(dependency);
                        var cycle = string.Join(" -> ", currentPath.Skip(cycleStart).Concat(new[] { dependency }));
                        cycles.Add(cycle);
                    }
                }
            }

            recursionStack.Remove(nodeId);
        }
    }

    [Serializable]
    public class DependencyNode
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string FilePath { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public DependencyNode()
        {
            Metadata = new Dictionary<string, object>();
        }

        public DependencyNode(string id, string name, string type) : this()
        {
            Id = id;
            Name = name;
            Type = type;
        }
    }

    [Serializable]
    public class DependencyEdge
    {
        public string FromId { get; set; }
        public string ToId { get; set; }
        public DependencyType DependencyType { get; set; }
        public string Description { get; set; }

        public DependencyEdge() { }

        public DependencyEdge(string fromId, string toId, DependencyType dependencyType)
        {
            FromId = fromId;
            ToId = toId;
            DependencyType = dependencyType;
        }
    }

    [Serializable]
    public class CodeIssue
    {
        public CodeIssueType Type { get; set; }
        public CodeIssueSeverity Severity { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string CodeSnippet { get; set; }
        public string Suggestion { get; set; }
        public string RuleId { get; set; }

        public CodeIssue() { }

        public CodeIssue(CodeIssueType type, string description, string filePath)
        {
            Type = type;
            Description = description;
            FilePath = filePath;
        }
    }

    [Serializable]
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
        public Dictionary<string, int> MetricsByType { get; set; }

        public CodeMetrics()
        {
            MetricsByType = new Dictionary<string, int>();
        }

        public float CommentRatio => TotalLinesOfCode > 0 ? (float)CommentLines / TotalLinesOfCode : 0f;
        public float MethodsPerClass => TotalClasses > 0 ? (float)TotalMethods / TotalClasses : 0f;
    }

    [Serializable]
    public class DesignPattern
    {
        public DesignPatternType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> InvolvedClasses { get; set; }
        public float Confidence { get; set; }
        public string Evidence { get; set; }

        public DesignPattern()
        {
            InvolvedClasses = new List<string>();
        }

        public DesignPattern(DesignPatternType type, string name) : this()
        {
            Type = type;
            Name = name;
        }
    }

    [Serializable]
    public class AssetInfo
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string AssetType { get; set; }
        public long SizeBytes { get; set; }
        public DateTime ImportedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ImporterType { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        public List<string> Labels { get; set; }

        public AssetInfo()
        {
            Metadata = new Dictionary<string, object>();
            Labels = new List<string>();
        }

        public AssetInfo(string path) : this()
        {
            Path = path;
            Name = System.IO.Path.GetFileName(path);
        }
    }

    [Serializable]
    public class AssetDependency
    {
        public string AssetPath { get; set; }
        public string DependencyPath { get; set; }
        public AssetDependencyType Type { get; set; }
        public bool IsDirectDependency { get; set; }

        public AssetDependency() { }

        public AssetDependency(string assetPath, string dependencyPath, AssetDependencyType type)
        {
            AssetPath = assetPath;
            DependencyPath = dependencyPath;
            Type = type;
        }
    }

    [Serializable]
    public class AssetUsageReport
    {
        public List<AssetUsageInfo> UsageInfos { get; set; }
        public List<string> UnusedAssets { get; set; }
        public List<string> MissingAssets { get; set; }
        public Dictionary<string, int> UsageByType { get; set; }

        public AssetUsageReport()
        {
            UsageInfos = new List<AssetUsageInfo>();
            UnusedAssets = new List<string>();
            MissingAssets = new List<string>();
            UsageByType = new Dictionary<string, int>();
        }
    }

    [Serializable]
    public class AssetUsageInfo
    {
        public string AssetPath { get; set; }
        public int UsageCount { get; set; }
        public List<string> UsedByAssets { get; set; }
        public List<string> UsedByScenes { get; set; }

        public AssetUsageInfo()
        {
            UsedByAssets = new List<string>();
            UsedByScenes = new List<string>();
        }

        public AssetUsageInfo(string assetPath) : this()
        {
            AssetPath = assetPath;
        }
    }

    [Serializable]
    public class AssetIssue
    {
        public AssetIssueType Type { get; set; }
        public AssetIssueSeverity Severity { get; set; }
        public string Description { get; set; }
        public string AssetPath { get; set; }
        public string Suggestion { get; set; }

        public AssetIssue() { }

        public AssetIssue(AssetIssueType type, string description, string assetPath)
        {
            Type = type;
            Description = description;
            AssetPath = assetPath;
        }
    }

    [Serializable]
    public class AssetMetrics
    {
        public int TotalAssets { get; set; }
        public long TotalSizeBytes { get; set; }
        public Dictionary<string, int> AssetCountByType { get; set; }
        public Dictionary<string, long> AssetSizeByType { get; set; }
        public int UnusedAssets { get; set; }
        public int MissingAssets { get; set; }
        public float AverageAssetSize { get; set; }

        public AssetMetrics()
        {
            AssetCountByType = new Dictionary<string, int>();
            AssetSizeByType = new Dictionary<string, long>();
        }

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

    [Serializable]
    public class ComponentInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public ComponentCategory Category { get; set; }
        public List<string> Dependencies { get; set; }
        public List<string> Dependents { get; set; }
        public Dictionary<string, object> Properties { get; set; }

        public ComponentInfo()
        {
            Dependencies = new List<string>();
            Dependents = new List<string>();
            Properties = new Dictionary<string, object>();
        }

        public ComponentInfo(string name, string type) : this()
        {
            Name = name;
            Type = type;
        }
    }

    [Serializable]
    public class SystemConnection
    {
        public string FromComponent { get; set; }
        public string ToComponent { get; set; }
        public ConnectionType Type { get; set; }
        public string Description { get; set; }
        public float Strength { get; set; }

        public SystemConnection() { }

        public SystemConnection(string from, string to, ConnectionType type)
        {
            FromComponent = from;
            ToComponent = to;
            Type = type;
        }
    }

    [Serializable]
    public class LayerInfo
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public List<string> Components { get; set; }
        public string Description { get; set; }

        public LayerInfo()
        {
            Components = new List<string>();
        }

        public LayerInfo(string name, int level) : this()
        {
            Name = name;
            Level = level;
        }
    }

    [Serializable]
    public class ArchitectureIssue
    {
        public ArchitectureIssueType Type { get; set; }
        public ArchitectureIssueSeverity Severity { get; set; }
        public string Description { get; set; }
        public List<string> AffectedComponents { get; set; }
        public string Suggestion { get; set; }

        public ArchitectureIssue()
        {
            AffectedComponents = new List<string>();
        }

        public ArchitectureIssue(ArchitectureIssueType type, string description) : this()
        {
            Type = type;
            Description = description;
        }
    }

    [Serializable]
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

    [Serializable]
    public class PerformanceAnalysis
    {
        public List<PerformanceIssue> Issues { get; set; }
        public PerformanceMetrics Metrics { get; set; }
        public List<PerformanceRecommendation> Recommendations { get; set; }

        public PerformanceAnalysis()
        {
            Issues = new List<PerformanceIssue>();
            Metrics = new PerformanceMetrics();
            Recommendations = new List<PerformanceRecommendation>();
        }
    }

    [Serializable]
    public class PerformanceIssue
    {
        public PerformanceIssueType Type { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public PerformanceImpact Impact { get; set; }
        public string Suggestion { get; set; }

        public PerformanceIssue() { }

        public PerformanceIssue(PerformanceIssueType type, string description)
        {
            Type = type;
            Description = description;
        }
    }

    [Serializable]
    public class PerformanceMetrics
    {
        public int DrawCalls { get; set; }
        public int Vertices { get; set; }
        public int Triangles { get; set; }
        public long TextureMemoryMB { get; set; }
        public long MeshMemoryMB { get; set; }
        public int AudioClips { get; set; }
        public long AudioMemoryMB { get; set; }
        public Dictionary<string, float> CustomMetrics { get; set; }

        public PerformanceMetrics()
        {
            CustomMetrics = new Dictionary<string, float>();
        }
    }

    [Serializable]
    public class PerformanceRecommendation
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public PerformanceImpact ExpectedImpact { get; set; }
        public int ImplementationEffort { get; set; }

        public PerformanceRecommendation() { }

        public PerformanceRecommendation(string title, string description)
        {
            Title = title;
            Description = description;
        }
    }

    public enum FileType
    {
        Script,
        Scene,
        Prefab,
        Material,
        Texture,
        Audio,
        Mesh,
        Animation,
        Shader,
        Font,
        Other
    }

    public enum ClassType
    {
        Class,
        Struct,
        Interface,
        Enum,
        Delegate
    }

    public enum AccessModifier
    {
        Public,
        Private,
        Protected,
        Internal,
        ProtectedInternal,
        PrivateProtected
    }

    public enum StructureIssueType
    {
        MissingFolder,
        UnconventionalNaming,
        DeepNesting,
        LargeFile,
        CircularDependency,
        MisplacedFile
    }

    public enum StructureIssueSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }

    public enum DependencyType
    {
        Inheritance,
        Composition,
        Usage,
        Reference,
        Import
    }

    public enum CodeIssueType
    {
        CodeSmell,
        BestPracticeViolation,
        PotentialBug,
        PerformanceIssue,
        SecurityIssue,
        StyleViolation
    }

    public enum CodeIssueSeverity
    {
        Info,
        Minor,
        Major,
        Critical
    }

    public enum DesignPatternType
    {
        Singleton,
        Factory,
        Observer,
        Command,
        Strategy,
        Decorator,
        Adapter,
        Facade,
        Proxy,
        Builder,
        Prototype,
        TemplateMethod,
        State,
        Bridge,
        Composite,
        Flyweight,
        ChainOfResponsibility,
        Mediator,
        Memento,
        Visitor,
        Interpreter
    }

    public enum AssetDependencyType
    {
        Direct,
        Indirect,
        Cyclic,
        Unused,
        Missing
    }

    public enum AssetIssueType
    {
        MissingAsset,
        UnusedAsset,
        LargeAsset,
        UnoptimizedAsset,
        DuplicateAsset,
        BrokenReference
    }

    public enum AssetIssueSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }

    public enum ComponentCategory
    {
        Core,
        UI,
        Gameplay,
        Utility,
        External,
        ThirdParty
    }

    public enum ConnectionType
    {
        DataFlow,
        ControlFlow,
        Dependency,
        Inheritance,
        Composition,
        Usage
    }

    public enum ArchitecturePattern
    {
        None,
        MVC,
        MVP,
        MVVM,
        Layered,
        EventDriven,
        Microkernel,
        ServiceOriented,
        ComponentBased,
        EntityComponentSystem
    }

    public enum ArchitectureIssueType
    {
        LayerViolation,
        CircularDependency,
        TightCoupling,
        LowCohesion,
        GodClass,
        DeadCode,
        UnusedInterface
    }

    public enum ArchitectureIssueSeverity
    {
        Info,
        Minor,
        Major,
        Critical
    }

    public enum PerformanceIssueType
    {
        HighDrawCalls,
        LargeTextureSize,
        UnoptimizedMesh,
        ExcessiveMemoryUsage,
        SlowScript,
        UnbatchedRendering,
        LargeAudioFile
    }

    public enum PerformanceImpact
    {
        Low,
        Medium,
        High,
        Critical
    }
}