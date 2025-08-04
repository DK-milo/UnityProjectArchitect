using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityProjectArchitect.AI;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services
{
    public class DataModelGenerator : BaseDocumentationGenerator
    {
        public DataModelGenerator(ProjectAnalysisResult analysisResult) 
            : base(analysisResult, DocumentationSectionType.DataModel)
        {
        }

        public override async Task<string> GenerateContentAsync()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetSectionHeader("Data Model"));
            sb.AppendLine(AddTimestamp());

            sb.AppendLine(await GenerateDataModelOverviewAsync());
            sb.AppendLine(await GenerateScriptableObjectsAsync());
            sb.AppendLine(await GenerateDataClassesAsync());
            sb.AppendLine(await GenerateEnumsAndStructsAsync());
            sb.AppendLine(await GenerateDataRelationshipsAsync());
            sb.AppendLine(await GenerateDataModelDiagramAsync());

            sb.AppendLine(AddGenerationMetadata());

            return await WrapInProgressIndicator(sb.ToString(), "Data Model Generation");
        }

        private async Task<string> GenerateDataModelOverviewAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Data Model Overview", 2));

                if (analysisResult.Scripts?.Classes != null)
                {
                    List<ClassDefinition> scriptableObjects = analysisResult.Scripts.Classes.Where(c => c.IsScriptableObject).ToList();
                    List<ClassDefinition> dataClasses = analysisResult.Scripts.Classes.Where(c => IsDataClass(c)).ToList();
                    List<ClassDefinition> enums = analysisResult.Scripts.Classes.Where(c => c.Type == ClassType.Enum).ToList();
                    List<ClassDefinition> structs = analysisResult.Scripts.Classes.Where(c => c.Type == ClassType.Struct).ToList();

                    sb.AppendLine("**Data Model Components:**");
                    sb.AppendLine($"- **ScriptableObjects:** {scriptableObjects.Count} data containers");
                    sb.AppendLine($"- **Data Classes:** {dataClasses.Count} data structures");
                    sb.AppendLine($"- **Enumerations:** {enums.Count} enum types");
                    sb.AppendLine($"- **Value Types:** {structs.Count} struct definitions");
                    sb.AppendLine();

                    if (scriptableObjects.Any() || dataClasses.Any())
                    {
                        sb.AppendLine("**Data Architecture Approach:**");
                        
                        if (scriptableObjects.Any())
                        {
                            sb.AppendLine("- **Asset-Based Data:** Using ScriptableObjects for configuration and persistent data");
                        }
                        
                        if (dataClasses.Any())
                        {
                            sb.AppendLine("- **Runtime Data Structures:** Custom classes for managing application state");
                        }
                        
                        if (structs.Any())
                        {
                            sb.AppendLine("- **Value Types:** Lightweight structs for performance-critical data");
                        }
                        sb.AppendLine();
                    }

                    int totalProperties = analysisResult.Scripts.Classes.Sum(c => c.Properties.Count);
                    int totalFields = analysisResult.Scripts.Classes.Sum(c => c.Fields.Count);
                    
                    sb.AppendLine($"**Data Complexity:** {totalProperties} properties and {totalFields} fields across all data structures");
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine("Data model analysis will be available once script analysis is completed.");
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateScriptableObjectsAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("ScriptableObject Data Containers", 2));

                if (analysisResult.Scripts?.Classes != null)
                {
                    List<ClassDefinition> scriptableObjects = analysisResult.Scripts.Classes
                        .Where(c => c.IsScriptableObject)
                        .OrderBy(c => c.Name)
                        .ToList();

                    if (scriptableObjects.Any())
                    {
                        sb.AppendLine("ScriptableObjects provide persistent, asset-based data storage for configuration and game data:");
                        sb.AppendLine();

                        foreach (ClassDefinition so in scriptableObjects)
                        {
                            sb.AppendLine($"### {so.Name}");
                            sb.AppendLine($"**Namespace:** `{so.Namespace}`");
                            sb.AppendLine($"**File:** `{System.IO.Path.GetFileName(so.FilePath)}`");
                            sb.AppendLine();

                            if (so.Properties.Any())
                            {
                                sb.AppendLine("**Properties:**");
                                foreach (PropertyDefinition prop in so.Properties.Take(10))
                                {
                                    string accessModifier = prop.AccessModifier == AccessModifier.Public ? "" : $"{prop.AccessModifier.ToString().ToLower()} ";
                                    sb.AppendLine($"- `{accessModifier}{prop.Type} {prop.Name}` - {GetPropertyDescription(prop)}");
                                }
                                
                                if (so.Properties.Count > 10)
                                {
                                    sb.AppendLine($"- ... and {so.Properties.Count - 10} more properties");
                                }
                                sb.AppendLine();
                            }

                            if (so.Fields.Any())
                            {
                                sb.AppendLine("**Fields:**");
                                foreach (FieldDefinition field in so.Fields.Where(f => f.AccessModifier == AccessModifier.Public).Take(5))
                                {
                                    string modifiers = GetFieldModifiers(field);
                                    sb.AppendLine($"- `{modifiers}{field.Type} {field.Name}` - {GetFieldDescription(field)}");
                                }
                                sb.AppendLine();
                            }

                            if (so.Attributes.Any())
                            {
                                List<string> unityAttributes = so.Attributes.Where(a => a.Contains("CreateAssetMenu") || a.Contains("System.Serializable")).ToList();
                                if (unityAttributes.Any())
                                {
                                    sb.AppendLine($"**Unity Attributes:** {string.Join(", ", unityAttributes)}");
                                    sb.AppendLine();
                                }
                            }
                        }
                    }
                    else
                    {
                        sb.AppendLine("No ScriptableObject data containers found. Consider using ScriptableObjects for:");
                        sb.AppendLine("- Game configuration data");
                        sb.AppendLine("- Character stats and progression");
                        sb.AppendLine("- Level/scene data");
                        sb.AppendLine("- Audio/visual settings");
                        sb.AppendLine();
                    }
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateDataClassesAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Data Classes", 2));

                if (analysisResult.Scripts?.Classes != null)
                {
                    List<ClassDefinition> dataClasses = analysisResult.Scripts.Classes
                        .Where(c => IsDataClass(c) && !c.IsScriptableObject)
                        .OrderBy(c => c.Name)
                        .ToList();

                    if (dataClasses.Any())
                    {
                        sb.AppendLine("Runtime data structures for managing application state and temporary data:");
                        sb.AppendLine();

                        foreach (ClassDefinition dataClass in dataClasses.Take(10))
                        {
                            sb.AppendLine($"### {dataClass.Name}");
                            sb.AppendLine($"**Type:** {dataClass.Type}");
                            sb.AppendLine($"**Namespace:** `{dataClass.Namespace}`");
                            
                            if (dataClass.BaseClasses.Any())
                            {
                                sb.AppendLine($"**Inherits:** {string.Join(", ", dataClass.BaseClasses)}");
                            }
                            
                            if (dataClass.Interfaces.Any())
                            {
                                sb.AppendLine($"**Implements:** {string.Join(", ", dataClass.Interfaces)}");
                            }
                            sb.AppendLine();

                            if (dataClass.Properties.Any())
                            {
                                sb.AppendLine("**Data Properties:**");
                                Dictionary<string, string> propertiesTable = new Dictionary<string, string>();
                                
                                foreach (PropertyDefinition prop in dataClass.Properties.Take(8))
                                {
                                    string accessInfo = GetPropertyAccessInfo(prop);
                                    propertiesTable[prop.Name] = $"{prop.Type} {accessInfo}";
                                }
                                
                                sb.Append(FormatTable(propertiesTable, "Property", "Type & Access"));
                            }

                            if (dataClass.Fields.Any())
                            {
                                List<FieldDefinition> publicFields = dataClass.Fields.Where(f => f.AccessModifier == AccessModifier.Public).ToList();
                                if (publicFields.Any())
                                {
                                    sb.AppendLine("**Public Fields:**");
                                    foreach (FieldDefinition field in publicFields.Take(5))
                                    {
                                        string modifiers = GetFieldModifiers(field);
                                        sb.AppendLine($"- `{modifiers}{field.Type} {field.Name}`");
                                    }
                                    sb.AppendLine();
                                }
                            }
                        }

                        if (dataClasses.Count > 10)
                        {
                            sb.AppendLine($"*... and {dataClasses.Count - 10} more data classes*");
                            sb.AppendLine();
                        }
                    }
                    else
                    {
                        sb.AppendLine("No dedicated data classes identified. Data classes are useful for:");
                        sb.AppendLine("- Player/character data structures");
                        sb.AppendLine("- Game state containers");
                        sb.AppendLine("- Configuration objects");
                        sb.AppendLine("- Event data structures");
                        sb.AppendLine();
                    }
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateEnumsAndStructsAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Enumerations & Value Types", 2));

                if (analysisResult.Scripts?.Classes != null)
                {
                    List<ClassDefinition> enums = analysisResult.Scripts.Classes.Where(c => c.Type == ClassType.Enum).ToList();
                    List<ClassDefinition> structs = analysisResult.Scripts.Classes.Where(c => c.Type == ClassType.Struct).ToList();

                    if (enums.Any())
                    {
                        sb.AppendLine("**Enumerations:**");
                        sb.AppendLine("Defining discrete states and categories within the application:");
                        sb.AppendLine();

                        foreach (ClassDefinition enumClass in enums.Take(8))
                        {
                            sb.AppendLine($"- **{enumClass.Name}** (`{enumClass.Namespace}`) - {GetEnumDescription(enumClass)}");
                        }
                        
                        if (enums.Count > 8)
                        {
                            sb.AppendLine($"- *... and {enums.Count - 8} more enumerations*");
                        }
                        sb.AppendLine();
                    }

                    if (structs.Any())
                    {
                        sb.AppendLine("**Value Types (Structs):**");
                        sb.AppendLine("Lightweight data structures for performance-critical scenarios:");
                        sb.AppendLine();

                        foreach (ClassDefinition structClass in structs.Take(5))
                        {
                            sb.AppendLine($"### {structClass.Name}");
                            sb.AppendLine($"**Namespace:** `{structClass.Namespace}`");
                            
                            if (structClass.Properties.Any())
                            {
                                sb.AppendLine($"**Properties:** {structClass.Properties.Count}");
                                IEnumerable<string> propNames = structClass.Properties.Take(3).Select(p => p.Name);
                                sb.AppendLine($"- {string.Join(", ", propNames)}");
                            }
                            
                            if (structClass.Fields.Any())
                            {
                                sb.AppendLine($"**Fields:** {structClass.Fields.Count}");
                                IEnumerable<string> fieldNames = structClass.Fields.Take(3).Select(f => f.Name);
                                sb.AppendLine($"- {string.Join(", ", fieldNames)}");
                            }
                            sb.AppendLine();
                        }
                    }

                    if (!enums.Any() && !structs.Any())
                    {
                        sb.AppendLine("No enumerations or value types found. Consider adding:");
                        sb.AppendLine("- **Enums** for game states, character types, difficulty levels");
                        sb.AppendLine("- **Structs** for coordinates, colors, small data containers");
                        sb.AppendLine();
                    }
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateDataRelationshipsAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Data Relationships", 2));

                if (analysisResult.Scripts?.Dependencies != null)
                {
                    DependencyGraph dependencies = analysisResult.Scripts.Dependencies;
                    List<ClassDefinition> dataClasses = analysisResult.Scripts.Classes.Where(c => IsDataClass(c) || c.IsScriptableObject).ToList();

                    if (dataClasses.Any())
                    {
                        sb.AppendLine("**Data Inheritance Hierarchy:**");
                        
                        List<string> inheritanceRelations = new List<string>();
                        foreach (ClassDefinition dataClass in dataClasses)
                        {
                            if (dataClass.BaseClasses.Any())
                            {
                                foreach (string baseClass in dataClass.BaseClasses)
                                {
                                    if (dataClasses.Any(dc => dc.Name == baseClass))
                                    {
                                        inheritanceRelations.Add($"{dataClass.Name} → {baseClass}");
                                    }
                                }
                            }
                        }

                        if (inheritanceRelations.Any())
                        {
                            sb.Append(FormatList(inheritanceRelations.Take(10)));
                        }
                        else
                        {
                            sb.AppendLine("- No inheritance relationships detected between data classes");
                            sb.AppendLine();
                        }

                        sb.AppendLine("**Data Composition:**");
                        List<string> compositionRelations = new List<string>();
                        
                        foreach (ClassDefinition dataClass in dataClasses)
                        {
                            List<PropertyDefinition> complexProperties = dataClass.Properties
                                .Where(p => dataClasses.Any(dc => dc.Name == p.Type || p.Type.Contains(dc.Name)))
                                .ToList();

                            foreach (string prop in complexProperties.Take(3))
                            {
                                compositionRelations.Add($"{dataClass.Name}.{prop.Name} : {prop.Type}");
                            }
                        }

                        if (compositionRelations.Any())
                        {
                            sb.Append(FormatList(compositionRelations.Take(10)));
                        }
                        else
                        {
                            sb.AppendLine("- No complex data composition relationships detected");
                            sb.AppendLine();
                        }
                    }
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateDataModelDiagramAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Data Model Diagram", 2));

                if (analysisResult.Scripts?.Classes != null)
                {
                    List<ClassDefinition> dataClasses = analysisResult.Scripts.Classes
                        .Where(c => IsDataClass(c) || c.IsScriptableObject)
                        .Take(15) // Limit for readability
                        .ToList();

                    if (dataClasses.Any())
                    {
                        sb.AppendLine("```mermaid");
                        sb.AppendLine("classDiagram");
                        sb.AppendLine("    %% Data Model Class Diagram");
                        sb.AppendLine();

                        foreach (ClassDefinition dataClass in dataClasses)
                        {
                            sb.AppendLine($"    class {dataClass.Name} {{");
                            
                            // Add key properties
                            foreach (string prop in dataClass.Properties.Take(5))
                            {
                                string visibility = GetVisibilitySymbol(prop.AccessModifier);
                                sb.AppendLine($"        {visibility}{prop.Type} {prop.Name}");
                            }
                            
                            // Add key fields
                            foreach (string field in dataClass.Fields.Where(f => f.AccessModifier == AccessModifier.Public).Take(3))
                            {
                                string visibility = GetVisibilitySymbol(field.AccessModifier);
                                sb.AppendLine($"        {visibility}{field.Type} {field.Name}");
                            }
                            
                            sb.AppendLine("    }");
                            sb.AppendLine();
                        }

                        // Add relationships
                        foreach (ClassDefinition dataClass in dataClasses)
                        {
                            foreach (string baseClass in dataClass.BaseClasses)
                            {
                                if (dataClasses.Any(dc => dc.Name == baseClass))
                                {
                                    sb.AppendLine($"    {baseClass} <|-- {dataClass.Name}");
                                }
                            }

                            // Show composition relationships
                            foreach (string prop in dataClass.Properties.Take(2))
                            {
                                ClassDefinition relatedClass = dataClasses.FirstOrDefault(dc => dc.Name == prop.Type || prop.Type.Contains(dc.Name));
                                if (relatedClass != null && relatedClass.Name != dataClass.Name)
                                {
                                    sb.AppendLine($"    {dataClass.Name} --> {relatedClass.Name} : {prop.Name}");
                                }
                            }
                        }

                        // Add styling
                        sb.AppendLine();
                        sb.AppendLine("    %% Styling");
                        foreach (string so in dataClasses.Where(c => c.IsScriptableObject))
                        {
                            sb.AppendLine($"    class {so.Name} {{");
                            sb.AppendLine("        <<ScriptableObject>>");
                            sb.AppendLine("    }");
                        }

                        sb.AppendLine("```");
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.AppendLine("Data model diagram will be generated once data structures are identified and analyzed.");
                        sb.AppendLine();
                    }
                }

                return sb.ToString();
            });
        }

        private bool IsDataClass(ClassDefinition classDefinition)
        {
            // Heuristics to identify data classes
            bool hasDataIndicators = classDefinition.Name.EndsWith("Data") ||
                                  classDefinition.Name.EndsWith("Config") ||
                                  classDefinition.Name.EndsWith("Settings") ||
                                  classDefinition.Name.EndsWith("Info") ||
                                  classDefinition.Name.Contains("Model");

            bool hasOnlyDataMembers = classDefinition.Methods.Count <= 2 && // Constructor + maybe one helper
                                   (classDefinition.Properties.Count > 0 || classDefinition.Fields.Count > 0);

            bool hasSerializationAttributes = classDefinition.Attributes.Any(a => 
                a.Contains("Serializable") || a.Contains("DataContract"));

            return hasDataIndicators || hasOnlyDataMembers || hasSerializationAttributes;
        }

        private string GetPropertyDescription(PropertyDefinition property)
        {
            if (property.IsAutoProperty)
                return "Auto-property";
            
            List<string> accessors = new List<string>();
            if (property.HasGetter) accessors.Add("get");
            if (property.HasSetter) accessors.Add("set");
            
            return $"Property with {string.Join(", ", accessors)} accessor(s)";
        }

        private string GetFieldDescription(FieldDefinition field)
        {
            List<string> descriptors = new List<string>();
            
            if (field.IsStatic) descriptors.Add("static");
            if (field.IsReadOnly) descriptors.Add("readonly");
            if (field.IsConst) descriptors.Add("const");
            
            if (!descriptors.Any()) descriptors.Add("instance field");
            
            return string.Join(" ", descriptors);
        }

        private string GetFieldModifiers(FieldDefinition field)
        {
            List<string> modifiers = new List<string>();
            
            if (field.AccessModifier != AccessModifier.Private)
                modifiers.Add(field.AccessModifier.ToString().ToLower());
            
            if (field.IsStatic) modifiers.Add("static");
            if (field.IsReadOnly) modifiers.Add("readonly");
            if (field.IsConst) modifiers.Add("const");
            
            return modifiers.Any() ? string.Join(" ", modifiers) + " " : "";
        }

        private string GetPropertyAccessInfo(PropertyDefinition property)
        {
            List<string> info = new List<string>();
            
            if (property.HasGetter && property.HasSetter)
                info.Add("{ get; set; }");
            else if (property.HasGetter)
                info.Add("{ get; }");
            else if (property.HasSetter)
                info.Add("{ set; }");
            
            return info.Any() ? string.Join(" ", info) : "";
        }

        private string GetEnumDescription(ClassDefinition enumClass)
        {
            if (enumClass.Name.Contains("State"))
                return "State enumeration";
            if (enumClass.Name.Contains("Type"))
                return "Type classification";
            if (enumClass.Name.Contains("Mode"))
                return "Mode selection";
            
            return "Enumeration";
        }

        private string GetVisibilitySymbol(AccessModifier accessModifier)
        {
            return accessModifier switch
            {
                AccessModifier.Public => "+",
                AccessModifier.Protected => "#",
                AccessModifier.Private => "-",
                _ => "~"
            };
        }
    }
}