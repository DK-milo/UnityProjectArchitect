using System;
using UnityEngine;
using UnityProjectArchitect.Core;
using Newtonsoft.Json;

namespace UnityProjectArchitect.Unity
{
    /// <summary>
    /// Unity ScriptableObject wrapper for ProjectData from Core DLL
    /// Provides Unity Editor integration while using DLL business logic
    /// </summary>
    [CreateAssetMenu(fileName = "ProjectData", menuName = "Unity Project Architect/Project Data", order = 1)]
    public class UnityProjectDataAsset : ScriptableObject
    {
        [Header("Project Configuration")]
        [SerializeField] private string projectDataJson = "";
        
        private ProjectData _projectData;
        
        /// <summary>
        /// Access to the Core DLL ProjectData instance
        /// </summary>
        public ProjectData ProjectData 
        { 
            get 
            { 
                if (_projectData == null)
                {
                    LoadFromJson();
                }
                return _projectData; 
            } 
        }
        
        /// <summary>
        /// Initialize with a new ProjectData instance
        /// </summary>
        public void Initialize()
        {
            _projectData = new ProjectData();
            SaveToJson();
        }
        
        /// <summary>
        /// Save current ProjectData to JSON for Unity serialization
        /// </summary>
        public void SaveToJson()
        {
            if (_projectData != null)
            {
                projectDataJson = JsonConvert.SerializeObject(_projectData, Formatting.Indented);
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
        
        /// <summary>
        /// Force refresh project analysis by clearing cached analysis data
        /// </summary>
        public void ClearAnalysisCache()
        {
            if (_projectData != null)
            {
                // Clear any cached analysis data that might contain outdated project type
                _projectData.ClearAnalysisData();
                SaveToJson();
                Debug.Log("Project analysis cache cleared. Next analysis will be fresh.");
            }
        }
        
        /// <summary>
        /// Load ProjectData from JSON
        /// </summary>
        private void LoadFromJson()
        {
            if (!string.IsNullOrEmpty(projectDataJson))
            {
                try
                {
                    _projectData = JsonConvert.DeserializeObject<ProjectData>(projectDataJson);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to deserialize ProjectData: {ex.Message}");
                    _projectData = new ProjectData();
                }
            }
            else
            {
                _projectData = new ProjectData();
            }
        }
        
        /// <summary>
        /// Unity Editor validation
        /// </summary>
        private void OnValidate()
        {
            if (_projectData != null)
            {
                SaveToJson();
            }
        }
        
        /// <summary>
        /// Ensure we have valid data when awakening
        /// </summary>
        private void Awake()
        {
            if (_projectData == null)
            {
                LoadFromJson();
            }
        }
        
        /// <summary>
        /// Update project name to match asset name
        /// </summary>
        public void UpdateProjectName(string newName)
        {
            if (_projectData != null)
            {
                _projectData.ProjectName = newName;
                SaveToJson();
            }
        }
        
        /// <summary>
        /// Get formatted display name for Unity Inspector
        /// </summary>
        public string GetDisplayName()
        {
            return _projectData?.ProjectName ?? "Unnamed Project";
        }
        
        /// <summary>
        /// Get project summary for Unity Inspector
        /// </summary>
        public string GetSummary()
        {
            if (_projectData == null) return "No project data";
            
            int enabledSections = 0;
            foreach (DocumentationSectionData section in _projectData.DocumentationSections)
            {
                if (section.IsEnabled) enabledSections++;
            }
            
            return $"{_projectData.ProjectType} project with {enabledSections} documentation sections enabled";
        }
    }
}