using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services
{
    public class TemplateValidator
    {
        public async Task<ValidationResult> ValidateAsync(ProjectTemplate template)
        {
            ValidationResult result = new ValidationResult($"Template Validation: {template?.TemplateName ?? "Unknown"}");
            
            if (template == null)
            {
                result.AddCritical(ValidationType.Templates, 
                    "Template is null", 
                    "Cannot validate a null template reference");
                return result;
            }

            await ValidateBasicProperties(template, result);
            await ValidateFolderStructure(template, result);
            await ValidateDocumentationSections(template, result);
            await ValidateRequiredPackages(template, result);
            await ValidateSceneTemplates(template, result);
            await ValidateAssemblyDefinitions(template, result);

            return result;
        }

        public async Task<ValidationResult> ValidateCompatibilityAsync(ProjectTemplate template, ProjectData projectData)
        {
            ValidationResult result = new ValidationResult($"Compatibility Validation: {template?.TemplateName ?? "Unknown"}");
            
            if (template == null)
            {
                result.AddCritical(ValidationType.Templates, 
                    "Template is null", 
                    "Cannot validate compatibility with a null template");
                return result;
            }

            if (projectData == null)
            {
                result.AddCritical(ValidationType.Templates, 
                    "Project data is null", 
                    "Cannot validate compatibility with null project data");
                return result;
            }

            await ValidateUnityVersionCompatibility(template, projectData, result);
            await ValidateProjectTypeCompatibility(template, projectData, result);
            await ValidateExistingStructureCompatibility(template, projectData, result);
            await ValidatePackageCompatibility(template, projectData, result);

            return result;
        }

        private async Task ValidateBasicProperties(ProjectTemplate template, ValidationResult result)
        {
            // Validate Template ID
            if (string.IsNullOrWhiteSpace(template.TemplateId))
            {
                result.AddError(ValidationType.Templates,
                    "Missing template ID",
                    "Template must have a unique identifier",
                    "Set a unique TemplateId for the template");
            }
            else if (template.TemplateId.Length < 3)
            {
                result.AddWarning(ValidationType.Templates,
                    "Template ID too short",
                    "Template ID should be at least 3 characters long for better identification");
            }

            // Validate Template Name
            if (string.IsNullOrWhiteSpace(template.TemplateName))
            {
                result.AddError(ValidationType.Templates,
                    "Missing template name",
                    "Template must have a display name",
                    "Set a descriptive TemplateName for the template");
            }

            // Validate Description
            if (string.IsNullOrWhiteSpace(template.TemplateDescription))
            {
                result.AddWarning(ValidationType.Templates,
                    "Missing template description",
                    "Template should have a description for better usability",
                    "Add a TemplateDescription explaining the template's purpose");
            }

            // Validate Author
            if (string.IsNullOrWhiteSpace(template.Author))
            {
                result.AddInfo(ValidationType.Templates,
                    "Missing author information",
                    "Consider adding author information for better template attribution");
            }

            // Validate Version
            if (string.IsNullOrWhiteSpace(template.TemplateVersion))
            {
                result.AddWarning(ValidationType.Templates,
                    "Missing template version",
                    "Template should have version information",
                    "Set a TemplateVersion following semantic versioning (e.g., 1.0.0)");
            }
            else if (!IsValidSemanticVersion(template.TemplateVersion))
            {
                result.AddWarning(ValidationType.Templates,
                    "Invalid version format",
                    $"Template version '{template.TemplateVersion}' doesn't follow semantic versioning",
                    "Use semantic versioning format (e.g., 1.0.0, 2.1.3)");
            }

            await Task.Delay(1); // Simulate async operation
        }

        private async Task ValidateFolderStructure(ProjectTemplate template, ValidationResult result)
        {
            if (template.FolderStructure?.Folders == null || template.FolderStructure.Folders.Count == 0)
            {
                result.AddWarning(ValidationType.Templates,
                    "No folder structure defined",
                    "Template doesn't define any folder structure",
                    "Consider adding a basic folder structure for better project organization");
                return;
            }

            // Check for duplicate folder names
            List<string> duplicateNames = template.FolderStructure.Folders
                .GroupBy(f => f.Name.ToLowerInvariant())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateNames.Count > 0)
            {
                result.AddError(ValidationType.Templates,
                    "Duplicate folder names",
                    $"Template contains duplicate folder names: {string.Join(", ", duplicateNames)}",
                    "Ensure all folder names are unique");
            }

            // Check for invalid folder names
            foreach (FolderStructureData.FolderInfo folder in template.FolderStructure.Folders)
            {
                if (string.IsNullOrWhiteSpace(folder.Name))
                {
                    result.AddError(ValidationType.Templates,
                        "Empty folder name",
                        "Template contains folders with empty names",
                        "Provide valid names for all folders");
                    continue;
                }

                char[] invalidChars = Path.GetInvalidFileNameChars();
                if (folder.Name.IndexOfAny(invalidChars) >= 0)
                {
                    result.AddError(ValidationType.Templates,
                        $"Invalid folder name: {folder.Name}",
                        $"Folder name contains invalid characters",
                        "Remove invalid characters from folder names");
                }

                // Note: FolderInfo doesn't have SubFolders property, so no recursive validation needed
            }

            // Check for recommended Unity folders
            bool hasScriptsFolder = template.FolderStructure.Folders
                .Any(f => f.Name.Equals("Scripts", StringComparison.OrdinalIgnoreCase));
            
            if (!hasScriptsFolder)
            {
                result.AddInfo(ValidationType.Templates,
                    "No Scripts folder",
                    "Template doesn't include a Scripts folder",
                    "Consider adding a Scripts folder for code organization");
            }

            await Task.Delay(1);
        }

        private async Task ValidateDocumentationSections(ProjectTemplate template, ValidationResult result)
        {
            if (template.DefaultDocumentationSections?.Count == 0)
            {
                result.AddInfo(ValidationType.Templates,
                    "No default documentation sections",
                    "Template doesn't define default documentation sections");
                return;
            }

            // Check for duplicate section types
            List<DocumentationSectionType> duplicateSections = template.DefaultDocumentationSections
                .GroupBy(s => s.SectionType)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateSections.Count > 0)
            {
                result.AddError(ValidationType.Templates,
                    "Duplicate documentation sections",
                    $"Template contains duplicate section types: {string.Join(", ", duplicateSections)}",
                    "Ensure each documentation section type appears only once");
            }

            // Validate each section
            foreach (DocumentationSection section in template.DefaultDocumentationSections)
            {
                if (section.WordCountTarget <= 0)
                {
                    result.AddWarning(ValidationType.Templates,
                        $"Invalid word count target for {section.SectionType}",
                        "Word count target should be greater than 0",
                        "Set a reasonable word count target (e.g., 200-1000)");
                }

                if (section.AIMode == AIGenerationMode.FullGeneration && 
                    string.IsNullOrWhiteSpace(section.CustomPrompt))
                {
                    result.AddWarning(ValidationType.Templates,
                        $"Missing AI prompt for {section.SectionType}",
                        "Section is set for full AI generation but has no custom prompt",
                        "Provide a custom prompt or change the AI generation mode");
                }
            }

            await Task.Delay(1);
        }

        private async Task ValidateRequiredPackages(ProjectTemplate template, ValidationResult result)
        {
            if (template.RequiredPackages?.Count == 0)
            {
                return; // No packages to validate
            }

            foreach (string package in template.RequiredPackages)
            {
                if (string.IsNullOrWhiteSpace(package))
                {
                    result.AddError(ValidationType.Templates,
                        "Empty package name",
                        "Template contains empty package names in RequiredPackages",
                        "Remove empty entries or provide valid package names");
                    continue;
                }

                // Check package name format
                if (!package.Contains('.'))
                {
                    result.AddWarning(ValidationType.Templates,
                        $"Invalid package format: {package}",
                        "Package name should follow Unity package naming convention (e.g., com.unity.render-pipelines.universal)",
                        "Use proper Unity package naming format");
                }

                // Check for common Unity packages
                if (IsKnownUnityPackage(package))
                {
                    result.AddInfo(ValidationType.Templates,
                        $"Unity package detected: {package}",
                        "Template requires a known Unity package");
                }
            }

            await Task.Delay(1);
        }

        private async Task ValidateSceneTemplates(ProjectTemplate template, ValidationResult result)
        {
            if (template.SceneTemplates?.Count == 0)
            {
                return; // No scene templates to validate
            }

            foreach (SceneTemplateData sceneTemplate in template.SceneTemplates)
            {
                if (string.IsNullOrWhiteSpace(sceneTemplate.SceneName))
                {
                    result.AddError(ValidationType.Templates,
                        "Empty scene name",
                        "Template contains scene templates with empty names",
                        "Provide valid names for all scene templates");
                    continue;
                }

                // Check for invalid characters in scene name
                char[] invalidChars = Path.GetInvalidFileNameChars();
                if (sceneTemplate.SceneName.IndexOfAny(invalidChars) >= 0)
                {
                    result.AddError(ValidationType.Templates,
                        $"Invalid scene name: {sceneTemplate.SceneName}",
                        "Scene name contains invalid characters",
                        "Remove invalid characters from scene names");
                }

                // Check for reasonable scene naming
                if (sceneTemplate.SceneName.Length > 50)
                {
                    result.AddWarning(ValidationType.Templates,
                        $"Long scene name: {sceneTemplate.SceneName}",
                        "Scene name is unusually long",
                        "Consider using shorter, more descriptive scene names");
                }
            }

            await Task.Delay(1);
        }

        private async Task ValidateAssemblyDefinitions(ProjectTemplate template, ValidationResult result)
        {
            if (template.AssemblyDefinitions?.Count == 0)
            {
                return; // No assembly definitions to validate
            }

            foreach (AssemblyDefinitionTemplate asmdef in template.AssemblyDefinitions)
            {
                if (string.IsNullOrWhiteSpace(asmdef.Name))
                {
                    result.AddError(ValidationType.Templates,
                        "Empty assembly definition name",
                        "Template contains empty assembly definition names",
                        "Remove empty entries or provide valid assembly definition names");
                    continue;
                }

                // Check for invalid characters
                char[] invalidChars = Path.GetInvalidFileNameChars();
                if (asmdef.Name.IndexOfAny(invalidChars) >= 0)
                {
                    result.AddError(ValidationType.Templates,
                        $"Invalid assembly definition name: {asmdef.Name}",
                        "Assembly definition name contains invalid characters",
                        "Remove invalid characters from assembly definition names");
                }

                // Check naming convention
                if (!asmdef.Name.Contains('.') && !asmdef.Name.EndsWith("Assembly"))
                {
                    result.AddInfo(ValidationType.Templates,
                        $"Assembly naming suggestion: {asmdef.Name}",
                        "Consider following Unity assembly naming conventions (e.g., ProjectName.Core, ProjectName.Scripts)");
                }
            }

            await Task.Delay(1);
        }

        private async Task ValidateUnityVersionCompatibility(ProjectTemplate template, ProjectData projectData, ValidationResult result)
        {
            if (!template.IsCompatibleWith(projectData))
            {
                result.AddError(ValidationType.Compatibility,
                    "Unity version incompatibility",
                    $"Template requires Unity {template.MinUnityVersion} or higher, but project targets {projectData.TargetUnityVersion}",
                    "Update the project's target Unity version or use a different template");
            }
            else if (template.MinUnityVersion > projectData.TargetUnityVersion)
            {
                result.AddWarning(ValidationType.Compatibility,
                    "Unity version mismatch",
                    $"Template is designed for Unity {template.MinUnityVersion}, project uses {projectData.TargetUnityVersion}");
            }

            await Task.Delay(1);
        }

        private async Task ValidateProjectTypeCompatibility(ProjectTemplate template, ProjectData projectData, ValidationResult result)
        {
            if (template.TargetProjectType != ProjectType.General && 
                template.TargetProjectType != projectData.ProjectType)
            {
                result.AddWarning(ValidationType.Compatibility,
                    "Project type mismatch",
                    $"Template is designed for {template.TargetProjectType} projects, but current project is {projectData.ProjectType}",
                    "Template may still work but might not be optimized for your project type");
            }

            await Task.Delay(1);
        }

        private async Task ValidateExistingStructureCompatibility(ProjectTemplate template, ProjectData projectData, ValidationResult result)
        {
            // This would check if the template's folder structure conflicts with existing folders
            // For now, we'll add a basic check
            if (template.FolderStructure?.Folders?.Count > 0)
            {
                result.AddInfo(ValidationType.Compatibility,
                    "Folder structure will be applied",
                    $"Template will create {template.FolderStructure.Folders.Count} folders");
            }

            await Task.Delay(1);
        }

        private async Task ValidatePackageCompatibility(ProjectTemplate template, ProjectData projectData, ValidationResult result)
        {
            if (template.RequiredPackages?.Count > 0)
            {
                result.AddInfo(ValidationType.Compatibility,
                    "Package requirements",
                    $"Template requires {template.RequiredPackages.Count} Unity packages to be installed");

                // Check for potentially conflicting packages
                List<string> heavyPackages = template.RequiredPackages
                    .Where(p => IsHeavyPackage(p))
                    .ToList();

                if (heavyPackages.Count > 0)
                {
                    result.AddWarning(ValidationType.Performance,
                        "Heavy packages detected",
                        $"Template includes resource-intensive packages: {string.Join(", ", heavyPackages)}",
                        "Consider the performance impact of these packages on your target platform");
                }
            }

            await Task.Delay(1);
        }

        private void ValidateSubFolders(FolderDefinition folder, ValidationResult result)
        {
            foreach (FolderDefinition subFolder in folder.SubFolders)
            {
                if (string.IsNullOrWhiteSpace(subFolder.Name))
                {
                    result.AddError(ValidationType.Templates,
                        $"Empty subfolder name in {folder.Name}",
                        "Template contains subfolders with empty names",
                        "Provide valid names for all subfolders");
                    continue;
                }

                char[] invalidChars = Path.GetInvalidFileNameChars();
                if (subFolder.Name.IndexOfAny(invalidChars) >= 0)
                {
                    result.AddError(ValidationType.Templates,
                        $"Invalid subfolder name: {subFolder.Name} in {folder.Name}",
                        "Subfolder name contains invalid characters",
                        "Remove invalid characters from subfolder names");
                }

                // Recursive validation
                ValidateSubFolders(subFolder, result);
            }
        }

        private bool IsValidSemanticVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return false;

            string[] parts = version.Split('.');
            if (parts.Length != 3)
                return false;

            return parts.All(part => int.TryParse(part, out _));
        }

        private bool IsKnownUnityPackage(string packageName)
        {
            string[] knownPackages = new[]
            {
                "com.unity.render-pipelines",
                "com.unity.ui",
                "com.unity.ugui",
                "com.unity.2d",
                "com.unity.cinemachine",
                "com.unity.postprocessing",
                "com.unity.timeline",
                "com.unity.animation",
                "com.unity.mobile",
                "com.unity.ads",
                "com.unity.analytics",
                "com.unity.purchasing",
                "com.unity.textmeshpro"
            };

            return knownPackages.Any(known => packageName.StartsWith(known, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsHeavyPackage(string packageName)
        {
            string[] heavyPackages = new[]
            {
                "com.unity.render-pipelines.high-definition",
                "com.unity.render-pipelines.universal",
                "com.unity.postprocessing",
                "com.unity.cinemachine",
                "com.unity.addressables",
                "com.unity.netcode"
            };

            return heavyPackages.Any(heavy => packageName.StartsWith(heavy, StringComparison.OrdinalIgnoreCase));
        }
    }
}