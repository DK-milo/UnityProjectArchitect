using System;
using System.Collections.Generic;
using UnityEngine;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Unity
{
    /// <summary>
    /// Unity ScriptableObject wrapper for TemplateConfiguration
    /// Provides Unity-compatible template configuration for editor integration
    /// </summary>
    [CreateAssetMenu(fileName = "TemplateConfiguration", menuName = "Unity Project Architect/Template Configuration")]
    public class TemplateConfigurationSO : ScriptableObject
    {
        [SerializeField] private string _templateName = "";
        [SerializeField] private string _description = "";
        [SerializeField] private ProjectType _projectType = ProjectType.General;
        [SerializeField] private List<string> _folderPaths = new List<string>();
        [SerializeField] private List<SceneTemplateData> _sceneTemplates = new List<SceneTemplateData>();
        
        public string TemplateName 
        { 
            get => _templateName; 
            private set => _templateName = value; 
        }
        
        public string Description 
        { 
            get => _description; 
            private set => _description = value; 
        }
        
        public ProjectType Type 
        { 
            get => _projectType; 
            private set => _projectType = value; 
        }
        
        public List<string> FolderPaths => _folderPaths;
        public List<SceneTemplateData> SceneTemplates => _sceneTemplates;
        
        public FolderStructureData FolderStructure 
        { 
            get 
            {
                FolderStructureData folderStructure = new FolderStructureData();
                folderStructure.Folders = new List<FolderStructureData.FolderInfo>();
                foreach (string path in _folderPaths)
                {
                    folderStructure.Folders.Add(new FolderStructureData.FolderInfo { Path = path });
                }
                folderStructure.Files = new List<FolderStructureData.FileInfo>();
                return folderStructure;
            }
        }
        
        public void Initialize(string templateName, string description, ProjectType projectType)
        {
            _templateName = templateName;
            _description = description;
            _projectType = projectType;
        }
        
        public void UpdateName(string name)
        {
            _templateName = name;
        }
        
        public void UpdateDescription(string description)
        {
            _description = description;
        }
        
        public void UpdateType(ProjectType type)
        {
            _projectType = type;
        }
        
        public void UpdateFolderStructure(FolderStructureData folderStructure)
        {
            _folderPaths.Clear();
            if (folderStructure?.Folders != null)
            {
                foreach (FolderStructureData.FolderInfo folder in folderStructure.Folders)
                {
                    _folderPaths.Add(folder.Path);
                }
            }
        }
        
        public void UpdateSceneTemplates(List<SceneTemplateData> sceneTemplates)
        {
            _sceneTemplates.Clear();
            if (sceneTemplates != null)
            {
                _sceneTemplates.AddRange(sceneTemplates);
            }
        }
    }
}