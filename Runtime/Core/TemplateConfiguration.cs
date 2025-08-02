using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityProjectArchitect.Core
{
    public enum ProjectType
    {
        General,
        Mobile2D,
        Mobile3D,
        PC2D,
        PC3D,
        VR,
        AR,
        WebGL,
        Console,
        Multiplayer,
        Educational,
        Prototype
    }

    public enum UnityVersion
    {
        Unity2022_3,
        Unity2023_3,
        Unity6
    }

    public enum AIProvider
    {
        None,
        Claude,
        GPT,
        Local
    }

    public enum FolderType
    {
        Scripts,
        Prefabs,
        Materials,
        Textures,
        Audio,
        Scenes,
        Animation,
        Resources,
        StreamingAssets,
        Editor,
        Plugins,
        Documentation,
        Art,
        Custom
    }

    [Serializable]
    public class TemplateReference
    {
        [SerializeField] private string _templateId = "";
        [SerializeField] private string _templateName = "";
        [SerializeField] private string _templateVersion = "1.0.0";
        [SerializeField] private DateTime _appliedDate;
        [SerializeField] private Dictionary<string, string> _customizations = new Dictionary<string, string>();

        public string TemplateId 
        { 
            get => _templateId; 
            set => _templateId = value; 
        }

        public string TemplateName 
        { 
            get => _templateName; 
            set => _templateName = value; 
        }

        public string TemplateVersion 
        { 
            get => _templateVersion; 
            set => _templateVersion = value; 
        }

        public DateTime AppliedDate 
        { 
            get => _appliedDate; 
            set => _appliedDate = value; 
        }

        public Dictionary<string, string> Customizations => _customizations;

        public TemplateReference()
        {
            _appliedDate = DateTime.Now;
            _customizations = new Dictionary<string, string>();
        }

        public TemplateReference(string id, string name, string version = "1.0.0") : this()
        {
            _templateId = id;
            _templateName = name;
            _templateVersion = version;
        }

        public override bool Equals(object obj)
        {
            return obj is TemplateReference other && _templateId == other._templateId;
        }

        public override int GetHashCode()
        {
            return _templateId?.GetHashCode() ?? 0;
        }
    }

    [Serializable]
    public class FolderStructureData
    {
        [SerializeField] private List<FolderDefinition> _folders = new List<FolderDefinition>();
        [SerializeField] private string _rootPath = "Assets";
        [SerializeField] private bool _useStandardUnityFolders = true;

        public List<FolderDefinition> Folders => _folders;
        public string RootPath 
        { 
            get => _rootPath; 
            set => _rootPath = value; 
        }
        public bool UseStandardUnityFolders 
        { 
            get => _useStandardUnityFolders; 
            set => _useStandardUnityFolders = value; 
        }

        public FolderStructureData()
        {
            InitializeDefaultFolders();
        }

        private void InitializeDefaultFolders()
        {
            if (_useStandardUnityFolders)
            {
                _folders.AddRange(new[]
                {
                    new FolderDefinition("Scripts", FolderType.Scripts, "Game logic and components"),
                    new FolderDefinition("Prefabs", FolderType.Prefabs, "Reusable game objects"),
                    new FolderDefinition("Materials", FolderType.Materials, "Material assets"),
                    new FolderDefinition("Textures", FolderType.Textures, "Texture and image assets"),
                    new FolderDefinition("Audio", FolderType.Audio, "Sound effects and music"),
                    new FolderDefinition("Scenes", FolderType.Scenes, "Unity scenes"),
                    new FolderDefinition("Animation", FolderType.Animation, "Animation controllers and clips")
                });
            }
        }

        public void AddFolder(FolderDefinition folder)
        {
            if (!_folders.Contains(folder))
            {
                _folders.Add(folder);
            }
        }

        public void RemoveFolder(FolderDefinition folder)
        {
            _folders.Remove(folder);
        }

        public FolderDefinition GetFolder(string name)
        {
            return _folders.Find(f => f.Name == name);
        }
    }

    [Serializable]
    public class FolderDefinition
    {
        [SerializeField] private string _name = "";
        [SerializeField] private FolderType _folderType = FolderType.Custom;
        [SerializeField] private string _description = "";
        [SerializeField] private string _relativePath = "";
        [SerializeField] private bool _createOnApply = true;
        [SerializeField] private List<FolderDefinition> _subFolders = new List<FolderDefinition>();

        public string Name 
        { 
            get => _name; 
            set => _name = value; 
        }

        public FolderType FolderType 
        { 
            get => _folderType; 
            set => _folderType = value; 
        }

        public string Description 
        { 
            get => _description; 
            set => _description = value; 
        }

        public string RelativePath 
        { 
            get => string.IsNullOrEmpty(_relativePath) ? _name : _relativePath; 
            set => _relativePath = value; 
        }

        public bool CreateOnApply 
        { 
            get => _createOnApply; 
            set => _createOnApply = value; 
        }

        public List<FolderDefinition> SubFolders => _subFolders;

        public FolderDefinition()
        {
            _subFolders = new List<FolderDefinition>();
        }

        public FolderDefinition(string folderName, FolderType type, string desc = "") : this()
        {
            _name = folderName;
            _folderType = type;
            _description = desc;
        }

        public void AddSubFolder(FolderDefinition subFolder)
        {
            if (!_subFolders.Contains(subFolder))
            {
                _subFolders.Add(subFolder);
            }
        }


        public override bool Equals(object obj)
        {
            return obj is FolderDefinition other && _name == other._name && _folderType == other._folderType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_name, _folderType);
        }
    }

    [CreateAssetMenu(fileName = "ProjectTemplate", menuName = "Unity Project Architect/Project Template", order = 2)]
    public class ProjectTemplate : ScriptableObject
    {
        [Header("Template Information")]
        [SerializeField] private string _templateId = "";
        [SerializeField] private string _templateName = "";
        [SerializeField] private string _templateDescription = "";
        [SerializeField] private string _templateVersion = "1.0.0";
        [SerializeField] private ProjectType _targetProjectType = ProjectType.General;
        [SerializeField] private UnityVersion _minUnityVersion = UnityVersion.Unity2023_3;

        [Header("Template Configuration")]
        [SerializeField] private FolderStructureData _folderStructure = new FolderStructureData();
        [SerializeField] private List<string> _requiredPackages = new List<string>();
        [SerializeField] private List<SceneTemplate> _sceneTemplates = new List<SceneTemplate>();
        [SerializeField] private List<string> _assemblyDefinitions = new List<string>();

        [Header("Documentation")]
        [SerializeField] private List<DocumentationSectionData> _defaultDocumentationSections = new List<DocumentationSectionData>();
        [SerializeField] private string[] _recommendedTags = new string[0];

        [Header("Metadata")]
        [SerializeField] private string _author = "";
        [SerializeField] private DateTime _createdDate;
        [SerializeField] private string[] _keywords = new string[0];

        public string TemplateId 
        { 
            get => string.IsNullOrEmpty(_templateId) ? name : _templateId; 
            set => _templateId = value; 
        }

        public string TemplateName 
        { 
            get => string.IsNullOrEmpty(_templateName) ? name : _templateName; 
            set => _templateName = value; 
        }

        public string TemplateDescription 
        { 
            get => _templateDescription; 
            set => _templateDescription = value; 
        }

        public string TemplateVersion 
        { 
            get => _templateVersion; 
            set => _templateVersion = value; 
        }

        public ProjectType TargetProjectType 
        { 
            get => _targetProjectType; 
            set => _targetProjectType = value; 
        }

        public UnityVersion MinUnityVersion 
        { 
            get => _minUnityVersion; 
            set => _minUnityVersion = value; 
        }

        public FolderStructureData FolderStructure => _folderStructure;
        public List<string> RequiredPackages => _requiredPackages;
        public List<SceneTemplate> SceneTemplates => _sceneTemplates;
        public List<string> AssemblyDefinitions => _assemblyDefinitions;
        public List<DocumentationSectionData> DefaultDocumentationSections => _defaultDocumentationSections;
        public string[] RecommendedTags => _recommendedTags ?? new string[0];
        public string Author 
        { 
            get => _author; 
            set => _author = value; 
        }
        public DateTime CreatedDate => _createdDate;
        public string[] Keywords => _keywords ?? new string[0];

        private void OnEnable()
        {
            if (_createdDate == default)
            {
                _createdDate = DateTime.Now;
            }

            if (string.IsNullOrEmpty(_templateId))
            {
                _templateId = name;
            }
        }

        public bool IsCompatibleWith(UnityVersion version)
        {
            return version >= minUnityVersion;
        }

        public TemplateReference CreateReference()
        {
            return new TemplateReference(TemplateId, TemplateName, TemplateVersion);
        }

        [ContextMenu("Generate Default Documentation Sections")]
        public void GenerateDefaultDocumentationSections()
        {
            _defaultDocumentationSections.Clear();
            foreach (DocumentationSectionType sectionType in Enum.GetValues<DocumentationSectionType>())
            {
                _defaultDocumentationSections.Add(new DocumentationSectionData(sectionType));
            }
        }
    }

    [Serializable]
    public class SceneTemplate
    {
        [SerializeField] private string _sceneName = "";
        [SerializeField] private string _sceneDescription = "";
        [SerializeField] private bool _createOnApply = true;
        [SerializeField] private List<string> _requiredGameObjects = new List<string>();

        public string SceneName 
        { 
            get => _sceneName; 
            set => _sceneName = value; 
        }

        public string SceneDescription 
        { 
            get => _sceneDescription; 
            set => _sceneDescription = value; 
        }

        public bool CreateOnApply 
        { 
            get => _createOnApply; 
            set => _createOnApply = value; 
        }

        public List<string> RequiredGameObjects => _requiredGameObjects;

        public SceneTemplate()
        {
            _requiredGameObjects = new List<string>();
        }

        public SceneTemplate(string name, string description = "") : this()
        {
            _sceneName = name;
            _sceneDescription = description;
        }
    }
}