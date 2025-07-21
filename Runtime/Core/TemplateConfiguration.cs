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
        [SerializeField] private string templateId = "";
        [SerializeField] private string templateName = "";
        [SerializeField] private string templateVersion = "1.0.0";
        [SerializeField] private DateTime appliedDate;
        [SerializeField] private Dictionary<string, string> customizations = new Dictionary<string, string>();

        public string TemplateId 
        { 
            get => templateId; 
            set => templateId = value; 
        }

        public string TemplateName 
        { 
            get => templateName; 
            set => templateName = value; 
        }

        public string TemplateVersion 
        { 
            get => templateVersion; 
            set => templateVersion = value; 
        }

        public DateTime AppliedDate 
        { 
            get => appliedDate; 
            set => appliedDate = value; 
        }

        public Dictionary<string, string> Customizations => customizations;

        public TemplateReference()
        {
            appliedDate = DateTime.Now;
            customizations = new Dictionary<string, string>();
        }

        public TemplateReference(string id, string name, string version = "1.0.0") : this()
        {
            templateId = id;
            templateName = name;
            templateVersion = version;
        }

        public override bool Equals(object obj)
        {
            return obj is TemplateReference other && templateId == other.templateId;
        }

        public override int GetHashCode()
        {
            return templateId?.GetHashCode() ?? 0;
        }
    }

    [Serializable]
    public class FolderStructureData
    {
        [SerializeField] private List<FolderDefinition> folders = new List<FolderDefinition>();
        [SerializeField] private string rootPath = "Assets";
        [SerializeField] private bool useStandardUnityFolders = true;

        public List<FolderDefinition> Folders => folders;
        public string RootPath 
        { 
            get => rootPath; 
            set => rootPath = value; 
        }
        public bool UseStandardUnityFolders 
        { 
            get => useStandardUnityFolders; 
            set => useStandardUnityFolders = value; 
        }

        public FolderStructureData()
        {
            InitializeDefaultFolders();
        }

        private void InitializeDefaultFolders()
        {
            if (useStandardUnityFolders)
            {
                folders.AddRange(new[]
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
            if (!folders.Contains(folder))
            {
                folders.Add(folder);
            }
        }

        public void RemoveFolder(FolderDefinition folder)
        {
            folders.Remove(folder);
        }

        public FolderDefinition GetFolder(string name)
        {
            return folders.Find(f => f.Name == name);
        }
    }

    [Serializable]
    public class FolderDefinition
    {
        [SerializeField] private string name = "";
        [SerializeField] private FolderType folderType = FolderType.Custom;
        [SerializeField] private string description = "";
        [SerializeField] private string relativePath = "";
        [SerializeField] private bool createOnApply = true;
        [SerializeField] private List<FolderDefinition> subFolders = new List<FolderDefinition>();
        [SerializeField] private List<string> fileTemplates = new List<string>();

        public string Name 
        { 
            get => name; 
            set => name = value; 
        }

        public FolderType FolderType 
        { 
            get => folderType; 
            set => folderType = value; 
        }

        public string Description 
        { 
            get => description; 
            set => description = value; 
        }

        public string RelativePath 
        { 
            get => string.IsNullOrEmpty(relativePath) ? name : relativePath; 
            set => relativePath = value; 
        }

        public bool CreateOnApply 
        { 
            get => createOnApply; 
            set => createOnApply = value; 
        }

        public List<FolderDefinition> SubFolders => subFolders;
        public List<string> FileTemplates => fileTemplates;

        public FolderDefinition()
        {
            subFolders = new List<FolderDefinition>();
            fileTemplates = new List<string>();
        }

        public FolderDefinition(string folderName, FolderType type, string desc = "") : this()
        {
            name = folderName;
            folderType = type;
            description = desc;
        }

        public void AddSubFolder(FolderDefinition subFolder)
        {
            if (!subFolders.Contains(subFolder))
            {
                subFolders.Add(subFolder);
            }
        }

        public void AddFileTemplate(string templateName)
        {
            if (!fileTemplates.Contains(templateName))
            {
                fileTemplates.Add(templateName);
            }
        }

        public override bool Equals(object obj)
        {
            return obj is FolderDefinition other && name == other.name && folderType == other.folderType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name, folderType);
        }
    }

    [CreateAssetMenu(fileName = "ProjectTemplate", menuName = "Unity Project Architect/Project Template", order = 2)]
    public class ProjectTemplate : ScriptableObject
    {
        [Header("Template Information")]
        [SerializeField] private string templateId = "";
        [SerializeField] private string templateName = "";
        [SerializeField] private string templateDescription = "";
        [SerializeField] private string templateVersion = "1.0.0";
        [SerializeField] private ProjectType targetProjectType = ProjectType.General;
        [SerializeField] private UnityVersion minUnityVersion = UnityVersion.Unity2023_3;

        [Header("Template Configuration")]
        [SerializeField] private FolderStructureData folderStructure = new FolderStructureData();
        [SerializeField] private List<string> requiredPackages = new List<string>();
        [SerializeField] private List<SceneTemplate> sceneTemplates = new List<SceneTemplate>();
        [SerializeField] private List<string> assemblyDefinitions = new List<string>();

        [Header("Documentation")]
        [SerializeField] private List<DocumentationSectionData> defaultDocumentationSections = new List<DocumentationSectionData>();
        [SerializeField] private string[] recommendedTags = new string[0];

        [Header("Metadata")]
        [SerializeField] private string author = "";
        [SerializeField] private DateTime createdDate;
        [SerializeField] private string[] keywords = new string[0];

        public string TemplateId 
        { 
            get => string.IsNullOrEmpty(templateId) ? name : templateId; 
            set => templateId = value; 
        }

        public string TemplateName 
        { 
            get => string.IsNullOrEmpty(templateName) ? name : templateName; 
            set => templateName = value; 
        }

        public string TemplateDescription 
        { 
            get => templateDescription; 
            set => templateDescription = value; 
        }

        public string TemplateVersion 
        { 
            get => templateVersion; 
            set => templateVersion = value; 
        }

        public ProjectType TargetProjectType 
        { 
            get => targetProjectType; 
            set => targetProjectType = value; 
        }

        public UnityVersion MinUnityVersion 
        { 
            get => minUnityVersion; 
            set => minUnityVersion = value; 
        }

        public FolderStructureData FolderStructure => folderStructure;
        public List<string> RequiredPackages => requiredPackages;
        public List<SceneTemplate> SceneTemplates => sceneTemplates;
        public List<string> AssemblyDefinitions => assemblyDefinitions;
        public List<DocumentationSectionData> DefaultDocumentationSections => defaultDocumentationSections;
        public string[] RecommendedTags => recommendedTags ?? new string[0];
        public string Author 
        { 
            get => author; 
            set => author = value; 
        }
        public DateTime CreatedDate => createdDate;
        public string[] Keywords => keywords ?? new string[0];

        private void OnEnable()
        {
            if (createdDate == default)
            {
                createdDate = DateTime.Now;
            }

            if (string.IsNullOrEmpty(templateId))
            {
                templateId = name;
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
            defaultDocumentationSections.Clear();
            foreach (DocumentationSectionType sectionType in Enum.GetValues<DocumentationSectionType>())
            {
                defaultDocumentationSections.Add(new DocumentationSectionData(sectionType));
            }
        }
    }

    [Serializable]
    public class SceneTemplate
    {
        [SerializeField] private string sceneName = "";
        [SerializeField] private string sceneDescription = "";
        [SerializeField] private bool createOnApply = true;
        [SerializeField] private List<string> requiredGameObjects = new List<string>();

        public string SceneName 
        { 
            get => sceneName; 
            set => sceneName = value; 
        }

        public string SceneDescription 
        { 
            get => sceneDescription; 
            set => sceneDescription = value; 
        }

        public bool CreateOnApply 
        { 
            get => createOnApply; 
            set => createOnApply = value; 
        }

        public List<string> RequiredGameObjects => requiredGameObjects;

        public SceneTemplate()
        {
            requiredGameObjects = new List<string>();
        }

        public SceneTemplate(string name, string description = "") : this()
        {
            sceneName = name;
            sceneDescription = description;
        }
    }
}