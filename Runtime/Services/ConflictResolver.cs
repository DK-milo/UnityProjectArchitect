using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityProjectArchitect.API;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services
{
    public class ConflictResolver
    {
        public List<TemplateConflict> DetectConflicts(ProjectData projectData, ProjectTemplate template)
        {
            var conflicts = new List<TemplateConflict>();

            // Detect folder conflicts
            DetectFolderConflicts(template, conflicts);

            // Detect scene conflicts
            DetectSceneConflicts(template, conflicts);

            // Detect assembly definition conflicts
            DetectAssemblyDefinitionConflicts(template, conflicts);

            // Detect package conflicts
            DetectPackageConflicts(template, conflicts);

            // Detect settings conflicts
            DetectSettingsConflicts(template, projectData, conflicts);

            return conflicts;
        }

        public async Task<TemplateOperationResult> ResolveConflictsAsync(List<TemplateConflict> conflicts, ConflictResolution defaultResolution)
        {
            var result = new TemplateOperationResult("conflict-resolution", "Conflict Resolution");
            var resolvedConflicts = new List<TemplateConflict>();

            try
            {
                foreach (var conflict in conflicts)
                {
                    var resolution = conflict.RecommendedResolution != ConflictResolution.Skip 
                        ? conflict.RecommendedResolution 
                        : defaultResolution;

                    var resolved = await ResolveIndividualConflictAsync(conflict, resolution);
                    if (resolved)
                    {
                        resolvedConflicts.Add(conflict);
                    }
                }

                result.Success = true;
                result.ResolvedConflicts = resolvedConflicts;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        private void DetectFolderConflicts(ProjectTemplate template, List<TemplateConflict> conflicts)
        {
            if (template.FolderStructure?.Folders == null)
                return;

            foreach (var folder in template.FolderStructure.Folders)
            {
                var folderPath = Path.Combine(Application.dataPath, folder.RelativePath);
                
                if (Directory.Exists(folderPath))
                {
                    var conflict = new TemplateConflict(
                        TemplateConflictType.FolderExists,
                        folderPath,
                        $"Folder '{folder.Name}' already exists at {folderPath}"
                    )
                    {
                        Severity = TemplateSeverity.Warning,
                        RecommendedResolution = ConflictResolution.Skip
                    };

                    conflicts.Add(conflict);
                }

                // Check subfolders recursively
                DetectSubFolderConflicts(folder, folderPath, conflicts);
            }
        }

        private void DetectSubFolderConflicts(FolderDefinition folder, string basePath, List<TemplateConflict> conflicts)
        {
            foreach (var subFolder in folder.SubFolders)
            {
                var subFolderPath = Path.Combine(basePath, subFolder.RelativePath);
                
                if (Directory.Exists(subFolderPath))
                {
                    var conflict = new TemplateConflict(
                        TemplateConflictType.FolderExists,
                        subFolderPath,
                        $"Subfolder '{subFolder.Name}' already exists at {subFolderPath}"
                    )
                    {
                        Severity = TemplateSeverity.Info,
                        RecommendedResolution = ConflictResolution.Skip
                    };

                    conflicts.Add(conflict);
                }

                // Recursive check
                DetectSubFolderConflicts(subFolder, subFolderPath, conflicts);
            }
        }

        private void DetectSceneConflicts(ProjectTemplate template, List<TemplateConflict> conflicts)
        {
            if (template.SceneTemplates?.Count == 0)
                return;

            foreach (var sceneTemplate in template.SceneTemplates)
            {
                if (!sceneTemplate.CreateOnApply)
                    continue;

                var scenePath = Path.Combine(Application.dataPath, "Scenes", $"{sceneTemplate.SceneName}.unity");
                
                if (File.Exists(scenePath))
                {
                    var conflict = new TemplateConflict(
                        TemplateConflictType.SceneConflict,
                        scenePath,
                        $"Scene '{sceneTemplate.SceneName}' already exists"
                    )
                    {
                        Severity = TemplateSeverity.Warning,
                        RecommendedResolution = ConflictResolution.Rename
                    };

                    conflicts.Add(conflict);
                }
            }
        }

        private void DetectAssemblyDefinitionConflicts(ProjectTemplate template, List<TemplateConflict> conflicts)
        {
            if (template.AssemblyDefinitions?.Count == 0)
                return;

            foreach (var asmdef in template.AssemblyDefinitions)
            {
                var asmdefPath = Path.Combine(Application.dataPath, "Scripts", $"{asmdef}.asmdef");
                
                if (File.Exists(asmdefPath))
                {
                    var conflict = new TemplateConflict(
                        TemplateConflictType.AssemblyDefinitionConflict,
                        asmdefPath,
                        $"Assembly definition '{asmdef}' already exists"
                    )
                    {
                        Severity = TemplateSeverity.Error,
                        RecommendedResolution = ConflictResolution.Skip
                    };

                    conflicts.Add(conflict);
                }
            }
        }

        private void DetectPackageConflicts(ProjectTemplate template, List<TemplateConflict> conflicts)
        {
            if (template.RequiredPackages?.Count == 0)
                return;

            // Read existing packages from manifest.json
            var manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            var existingPackages = new HashSet<string>();

            try
            {
                if (File.Exists(manifestPath))
                {
                    var manifestContent = File.ReadAllText(manifestPath);
                    var manifest = JsonUtility.FromJson<PackageManifest>(manifestContent);
                    
                    if (manifest?.dependencies != null)
                    {
                        foreach (var package in manifest.dependencies.Keys)
                        {
                            existingPackages.Add(package);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to read package manifest: {ex.Message}");
            }

            foreach (var requiredPackage in template.RequiredPackages)
            {
                if (existingPackages.Contains(requiredPackage))
                {
                    var conflict = new TemplateConflict(
                        TemplateConflictType.PackageConflict,
                        requiredPackage,
                        $"Package '{requiredPackage}' is already installed"
                    )
                    {
                        Severity = TemplateSeverity.Info,
                        RecommendedResolution = ConflictResolution.Skip
                    };

                    conflicts.Add(conflict);
                }
            }
        }

        private void DetectSettingsConflicts(ProjectTemplate template, ProjectData projectData, List<TemplateConflict> conflicts)
        {
            // Check project type conflicts
            if (template.TargetProjectType != ProjectType.General && 
                template.TargetProjectType != projectData.ProjectType)
            {
                var conflict = new TemplateConflict(
                    TemplateConflictType.SettingsConflict,
                    "ProjectType",
                    $"Template is for {template.TargetProjectType} but project is {projectData.ProjectType}"
                )
                {
                    Severity = TemplateSeverity.Warning,
                    RecommendedResolution = ConflictResolution.Skip
                };

                conflicts.Add(conflict);
            }

            // Check Unity version conflicts
            if (!template.IsCompatibleWith(projectData.TargetUnityVersion))
            {
                var conflict = new TemplateConflict(
                    TemplateConflictType.SettingsConflict,
                    "UnityVersion",
                    $"Template requires Unity {template.MinUnityVersion}+ but project uses {projectData.TargetUnityVersion}"
                )
                {
                    Severity = TemplateSeverity.Error,
                    RecommendedResolution = ConflictResolution.Skip
                };

                conflicts.Add(conflict);
            }

            // Check documentation section conflicts
            if (template.DefaultDocumentationSections?.Count > 0)
            {
                foreach (var templateSection in template.DefaultDocumentationSections)
                {
                    var existingSection = projectData.GetDocumentationSection(templateSection.SectionType);
                    if (existingSection != null && existingSection.HasContent)
                    {
                        var conflict = new TemplateConflict(
                            TemplateConflictType.SettingsConflict,
                            $"DocumentationSection.{templateSection.SectionType}",
                            $"Documentation section '{templateSection.SectionType}' already has content"
                        )
                        {
                            Severity = TemplateSeverity.Warning,
                            RecommendedResolution = ConflictResolution.Skip
                        };

                        conflicts.Add(conflict);
                    }
                }
            }
        }

        private async Task<bool> ResolveIndividualConflictAsync(TemplateConflict conflict, ConflictResolution resolution)
        {
            try
            {
                switch (resolution)
                {
                    case ConflictResolution.Skip:
                        return await SkipConflict(conflict);
                        
                    case ConflictResolution.Overwrite:
                        return await OverwriteConflict(conflict);
                        
                    case ConflictResolution.Merge:
                        return await MergeConflict(conflict);
                        
                    case ConflictResolution.Rename:
                        return await RenameConflict(conflict);
                        
                    case ConflictResolution.Backup:
                        return await BackupAndOverwrite(conflict);
                        
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to resolve conflict {conflict.ResourcePath}: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SkipConflict(TemplateConflict conflict)
        {
            // Simply mark as resolved by skipping
            Debug.Log($"Skipping conflict: {conflict.Description}");
            await Task.Delay(1);
            return true;
        }

        private async Task<bool> OverwriteConflict(TemplateConflict conflict)
        {
            switch (conflict.ConflictType)
            {
                case TemplateConflictType.FileExists:
                case TemplateConflictType.SceneConflict:
                    if (File.Exists(conflict.ResourcePath))
                    {
                        File.Delete(conflict.ResourcePath);
                        Debug.Log($"Overwritten file: {conflict.ResourcePath}");
                        return true;
                    }
                    break;

                case TemplateConflictType.FolderExists:
                    if (Directory.Exists(conflict.ResourcePath))
                    {
                        Directory.Delete(conflict.ResourcePath, true);
                        Debug.Log($"Overwritten folder: {conflict.ResourcePath}");
                        return true;
                    }
                    break;
            }

            await Task.Delay(1);
            return false;
        }

        private async Task<bool> MergeConflict(TemplateConflict conflict)
        {
            // Merge logic would be implemented based on conflict type
            // For now, we'll treat merge as skip for most cases
            Debug.Log($"Merging conflict: {conflict.Description}");
            
            switch (conflict.ConflictType)
            {
                case TemplateConflictType.PackageConflict:
                    // Package already exists, so merge is essentially skip
                    return true;
                    
                case TemplateConflictType.SettingsConflict:
                    // Settings conflicts can often be merged by taking both values
                    return true;
                    
                default:
                    return await SkipConflict(conflict);
            }
        }

        private async Task<bool> RenameConflict(TemplateConflict conflict)
        {
            switch (conflict.ConflictType)
            {
                case TemplateConflictType.FileExists:
                case TemplateConflictType.SceneConflict:
                    var directory = Path.GetDirectoryName(conflict.ResourcePath);
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(conflict.ResourcePath);
                    var extension = Path.GetExtension(conflict.ResourcePath);
                    
                    var counter = 1;
                    string newPath;
                    do
                    {
                        var newFileName = $"{fileNameWithoutExtension}_{counter}{extension}";
                        newPath = Path.Combine(directory, newFileName);
                        counter++;
                    } while (File.Exists(newPath) && counter < 100);

                    if (counter < 100)
                    {
                        Debug.Log($"Will create renamed file: {newPath}");
                        return true;
                    }
                    break;

                case TemplateConflictType.FolderExists:
                    var parentDir = Path.GetDirectoryName(conflict.ResourcePath);
                    var folderName = Path.GetFileName(conflict.ResourcePath);
                    
                    counter = 1;
                    do
                    {
                        var newFolderName = $"{folderName}_{counter}";
                        newPath = Path.Combine(parentDir, newFolderName);
                        counter++;
                    } while (Directory.Exists(newPath) && counter < 100);

                    if (counter < 100)
                    {
                        Debug.Log($"Will create renamed folder: {newPath}");
                        return true;
                    }
                    break;
            }

            await Task.Delay(1);
            return false;
        }

        private async Task<bool> BackupAndOverwrite(TemplateConflict conflict)
        {
            try
            {
                var backupPath = GetBackupPath(conflict.ResourcePath);
                
                switch (conflict.ConflictType)
                {
                    case TemplateConflictType.FileExists:
                    case TemplateConflictType.SceneConflict:
                        if (File.Exists(conflict.ResourcePath))
                        {
                            File.Copy(conflict.ResourcePath, backupPath, true);
                            File.Delete(conflict.ResourcePath);
                            Debug.Log($"Backed up and will overwrite: {conflict.ResourcePath} -> {backupPath}");
                            return true;
                        }
                        break;

                    case TemplateConflictType.FolderExists:
                        if (Directory.Exists(conflict.ResourcePath))
                        {
                            CopyDirectory(conflict.ResourcePath, backupPath);
                            Directory.Delete(conflict.ResourcePath, true);
                            Debug.Log($"Backed up and will overwrite: {conflict.ResourcePath} -> {backupPath}");
                            return true;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to backup: {ex.Message}");
                return false;
            }

            await Task.Delay(1);
            return false;
        }

        private string GetBackupPath(string originalPath)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var directory = Path.GetDirectoryName(originalPath);
            var fileName = Path.GetFileNameWithoutExtension(originalPath);
            var extension = Path.GetExtension(originalPath);
            
            return Path.Combine(directory, $"{fileName}_backup_{timestamp}{extension}");
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);
            
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(destDir, fileName);
                File.Copy(file, destFile, true);
            }
            
            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var dirName = Path.GetFileName(dir);
                var destSubDir = Path.Combine(destDir, dirName);
                CopyDirectory(dir, destSubDir);
            }
        }

        [Serializable]
        private class PackageManifest
        {
            public Dictionary<string, string> dependencies;
        }
    }
}