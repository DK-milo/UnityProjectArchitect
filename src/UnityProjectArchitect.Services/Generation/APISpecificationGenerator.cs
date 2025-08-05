using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityProjectArchitect.AI;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services
{
    public class APISpecificationGenerator : BaseDocumentationGenerator
    {
        public APISpecificationGenerator(ProjectAnalysisResult analysisResult) 
            : base(analysisResult, DocumentationSectionType.APISpecification)
        {
        }

        public override async Task<string> GenerateContentAsync()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetSectionHeader("API Specification"));
            sb.AppendLine(AddTimestamp());

            sb.AppendLine(await GenerateAPIOverviewAsync());
            sb.AppendLine(await GeneratePublicInterfacesAsync());
            sb.AppendLine(await GeneratePublicClassesAsync());
            sb.AppendLine(await GenerateServiceAPIsAsync());
            sb.AppendLine(await GenerateExtensionPointsAsync());
            sb.AppendLine(await GenerateUsageExamplesAsync());

            sb.AppendLine(AddGenerationMetadata());

            return await WrapInProgressIndicator(sb.ToString(), "API Specification Generation");
        }

        private async Task<string> GenerateAPIOverviewAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("API Overview", 2));

                if (analysisResult.Scripts?.Classes != null)
                {
                    List<InterfaceDefinition> publicInterfaces = analysisResult.Scripts.Interfaces
                        .Where(i => i.AccessModifier == AccessModifier.Public)
                        .ToList();

                    List<ClassDefinition> publicClasses = analysisResult.Scripts.Classes
                        .Where(c => c.AccessModifier == AccessModifier.Public && !c.IsMonoBehaviour)
                        .ToList();

                    List<ClassDefinition> publicMonoBehaviours = analysisResult.Scripts.Classes
                        .Where(c => c.AccessModifier == AccessModifier.Public && c.IsMonoBehaviour)
                        .ToList();

                    sb.AppendLine("**Public API Surface:**");
                    sb.AppendLine($"- **Interfaces:** {publicInterfaces.Count} public contracts");
                    sb.AppendLine($"- **Classes:** {publicClasses.Count} public implementations");
                    sb.AppendLine($"- **Components:** {publicMonoBehaviours.Count} MonoBehaviour components");
                    sb.AppendLine();

                    int totalPublicMethods = publicInterfaces.Sum(i => i.Methods.Count) + 
                                           publicClasses.Sum(c => c.Methods.Count(m => m.AccessModifier == AccessModifier.Public));

                    sb.AppendLine($"**API Complexity:** {totalPublicMethods} public methods across all interfaces and classes");
                    sb.AppendLine();

                    if (publicInterfaces.Any())
                    {
                        sb.AppendLine("**Design Approach:** Interface-driven architecture promoting loose coupling and testability");
                    }
                    else
                    {
                        sb.AppendLine("**Design Approach:** Class-based architecture - consider adding interfaces for better testability");
                    }
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }

        private async Task<string> GeneratePublicInterfacesAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Public Interfaces", 2));

                if (analysisResult.Scripts?.Interfaces != null)
                {
                    List<InterfaceDefinition> publicInterfaces = analysisResult.Scripts.Interfaces
                        .Where(i => i.AccessModifier == AccessModifier.Public)
                        .OrderBy(i => i.Name)
                        .ToList();

                    if (publicInterfaces.Any())
                    {
                        foreach (InterfaceDefinition iface in publicInterfaces)
                        {
                            sb.AppendLine($"### {iface.Name}");
                            sb.AppendLine($"**Namespace:** `{iface.Namespace}`");
                            sb.AppendLine($"**File:** `{System.IO.Path.GetFileName(iface.FilePath)}`");
                            
                            if (iface.BaseInterfaces.Any())
                            {
                                sb.AppendLine($"**Inherits:** {string.Join(", ", iface.BaseInterfaces)}");
                            }
                            sb.AppendLine();

                            if (iface.Methods.Any())
                            {
                                sb.AppendLine("**Methods:**");
                                sb.AppendLine("```csharp");
                                foreach (MethodSignature method in iface.Methods)
                                {
                                    string parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));
                                    sb.AppendLine($"{method.ReturnType} {method.Name}({parameters});");
                                }
                                sb.AppendLine("```");
                                sb.AppendLine();
                            }

                            if (iface.Properties.Any())
                            {
                                sb.AppendLine("**Properties:**");
                                Dictionary<string, string> propertiesTable = new Dictionary<string, string>();
                                foreach (PropertySignature prop in iface.Properties)
                                {
                                    List<string> accessors = new List<string>();
                                    if (prop.HasGetter) accessors.Add("get");
                                    if (prop.HasSetter) accessors.Add("set");
                                    propertiesTable[prop.Name] = $"{prop.Type} {{ {string.Join("; ", accessors)}; }}";
                                }
                                sb.Append(FormatTable(propertiesTable, "Property", "Type & Accessors"));
                            }

                            sb.AppendLine($"**Purpose:** {GetInterfacePurpose(iface)}");
                            sb.AppendLine();
                        }
                    }
                    else
                    {
                        sb.AppendLine("No public interfaces found. Consider defining interfaces for:");
                        sb.AppendLine("- Service contracts");
                        sb.AppendLine("- Data access layers");
                        sb.AppendLine("- Plugin extension points");
                        sb.AppendLine("- Testable components");
                        sb.AppendLine();
                    }
                }

                return sb.ToString();
            });
        }

        private async Task<string> GeneratePublicClassesAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Public Classes", 2));

                if (analysisResult.Scripts?.Classes != null)
                {
                    List<ClassDefinition> publicClasses = analysisResult.Scripts.Classes
                        .Where(c => c.AccessModifier == AccessModifier.Public && !c.IsMonoBehaviour)
                        .OrderBy(c => c.Name)
                        .ToList();

                    if (publicClasses.Any())
                    {
                        foreach (ClassDefinition publicClass in publicClasses.Take(10))
                        {
                            sb.AppendLine($"### {publicClass.Name}");
                            sb.AppendLine($"**Type:** {publicClass.Type}");
                            sb.AppendLine($"**Namespace:** `{publicClass.Namespace}`");
                            
                            if (publicClass.BaseClasses.Any())
                            {
                                sb.AppendLine($"**Inherits:** {string.Join(", ", publicClass.BaseClasses)}");
                            }
                            
                            if (publicClass.Interfaces.Any())
                            {
                                sb.AppendLine($"**Implements:** {string.Join(", ", publicClass.Interfaces)}");
                            }
                            sb.AppendLine();

                            List<MethodDefinition> publicMethods = publicClass.Methods
                                .Where(m => m.AccessModifier == AccessModifier.Public)
                                .ToList();

                            if (publicMethods.Any())
                            {
                                sb.AppendLine("**Public Methods:**");
                                foreach (MethodDefinition method in publicMethods.Take(8))
                                {
                                    string modifiers = GetMethodModifiers(method);
                                    string parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));
                                    sb.AppendLine($"- `{modifiers}{method.ReturnType} {method.Name}({parameters})`");
                                }
                                
                                if (publicMethods.Count > 8)
                                {
                                    sb.AppendLine($"- *... and {publicMethods.Count - 8} more public methods*");
                                }
                                sb.AppendLine();
                            }

                            List<PropertyDefinition> publicProperties = publicClass.Properties
                                .Where(p => p.AccessModifier == AccessModifier.Public)
                                .ToList();

                            if (publicProperties.Any())
                            {
                                sb.AppendLine("**Public Properties:**");
                                foreach (PropertyDefinition prop in publicProperties.Take(5))
                                {
                                    string accessors = GetPropertyAccessors(prop);
                                    sb.AppendLine($"- `{prop.Type} {prop.Name} {accessors}`");
                                }
                                sb.AppendLine();
                            }

                            sb.AppendLine($"**Usage:** {GetClassUsageDescription(publicClass)}");
                            sb.AppendLine();
                        }

                        if (publicClasses.Count > 10)
                        {
                            sb.AppendLine($"*... and {publicClasses.Count - 10} more public classes*");
                            sb.AppendLine();
                        }
                    }
                    else
                    {
                        sb.AppendLine("No public classes found beyond MonoBehaviour components.");
                        sb.AppendLine();
                    }
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateServiceAPIsAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Service APIs", 2));

                if (analysisResult.Scripts?.Classes != null)
                {
                    List<ClassDefinition> serviceClasses = analysisResult.Scripts.Classes
                        .Where(c => IsServiceClass(c))
                        .OrderBy(c => c.Name)
                        .ToList();

                    if (serviceClasses.Any())
                    {
                        sb.AppendLine("Core service APIs providing application functionality:");
                        sb.AppendLine();

                        foreach (ClassDefinition service in serviceClasses)
                        {
                            sb.AppendLine($"### {service.Name}");
                            
                            InterfaceDefinition serviceInterface = analysisResult.Scripts.Interfaces
                                .FirstOrDefault(i => service.Interfaces.Contains(i.Name));

                            if (serviceInterface != null)
                            {
                                sb.AppendLine($"**Contract:** `{serviceInterface.Name}`");
                                sb.AppendLine($"**Implementation:** `{service.Name}`");
                                sb.AppendLine();

                                sb.AppendLine("**Service Methods:**");
                                foreach (MethodSignature method in serviceInterface.Methods.Take(6))
                                {
                                    bool isAsync = method.ReturnType.Contains("Task");
                                    string asyncIndicator = isAsync ? " (async)" : "";
                                    string parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));
                                    sb.AppendLine($"- `{method.ReturnType} {method.Name}({parameters})`{asyncIndicator}");
                                }
                                sb.AppendLine();
                            }
                            else
                            {
                                List<MethodDefinition> publicMethods = service.Methods
                                    .Where(m => m.AccessModifier == AccessModifier.Public)
                                    .Take(5)
                                    .ToList();

                                if (publicMethods.Any())
                                {
                                    sb.AppendLine("**Public API:**");
                                    foreach (MethodDefinition method in publicMethods)
                                    {
                                        string asyncIndicator = method.IsAsync ? " (async)" : "";
                                        string parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));
                                        sb.AppendLine($"- `{method.ReturnType} {method.Name}({parameters})`{asyncIndicator}");
                                    }
                                    sb.AppendLine();
                                }
                            }

                            sb.AppendLine($"**Responsibility:** {GetServiceDescription(service)}");
                            sb.AppendLine();
                        }
                    }
                    else
                    {
                        sb.AppendLine("No dedicated service classes identified. Consider implementing service layers for:");
                        sb.AppendLine("- Data management");
                        sb.AppendLine("- External API integration");
                        sb.AppendLine("- Game state management");
                        sb.AppendLine("- Audio/graphics services");
                        sb.AppendLine();
                    }
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateExtensionPointsAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Extension Points", 2));

                if (analysisResult.Scripts?.Classes != null)
                {
                    List<ClassDefinition> abstractClasses = analysisResult.Scripts.Classes
                        .Where(c => c.Methods.Any(m => m.Name.Contains("virtual") || m.Name.Contains("abstract")))
                        .ToList();

                    List<InterfaceDefinition> interfacesWithEvents = analysisResult.Scripts.Interfaces
                        .Where(i => i.Name.Contains("Event") || i.Name.Contains("Handler") || i.Name.Contains("Listener"))
                        .ToList();

                    if (abstractClasses.Any() || interfacesWithEvents.Any())
                    {
                        sb.AppendLine("**Extensibility Features:**");

                        if (abstractClasses.Any())
                        {
                            sb.AppendLine();
                            sb.AppendLine("**Abstract Base Classes:**");
                            foreach (ClassDefinition abstractClass in abstractClasses.Take(5))
                            {
                                sb.AppendLine($"- **{abstractClass.Name}** - Extend for custom {GetExtensionPurpose(abstractClass)} implementations");
                            }
                        }

                        if (interfacesWithEvents.Any())
                        {
                            sb.AppendLine();
                            sb.AppendLine("**Event Interfaces:**");
                            foreach (InterfaceDefinition eventInterface in interfacesWithEvents.Take(5))
                            {
                                sb.AppendLine($"- **{eventInterface.Name}** - Implement for custom event handling");
                            }
                        }
                        sb.AppendLine();
                    }

                    // Check for plugin-style architecture
                    List<InterfaceDefinition> pluginInterfaces = analysisResult.Scripts.Interfaces
                        .Where(i => i.Name.Contains("Plugin") || i.Name.Contains("Provider") || i.Name.Contains("Factory"))
                        .ToList();

                    if (pluginInterfaces.Any())
                    {
                        sb.AppendLine("**Plugin Architecture:**");
                        foreach (InterfaceDefinition plugin in pluginInterfaces)
                        {
                            sb.AppendLine($"- **{plugin.Name}** - Plugin interface for extending functionality");
                        }
                        sb.AppendLine();
                    }

                    if (!abstractClasses.Any() && !interfacesWithEvents.Any() && !pluginInterfaces.Any())
                    {
                        sb.AppendLine("No clear extension points identified. Consider adding:");
                        sb.AppendLine("- Abstract base classes for customizable behavior");
                        sb.AppendLine("- Event interfaces for loose coupling");
                        sb.AppendLine("- Plugin interfaces for modular architecture");
                        sb.AppendLine("- Factory patterns for object creation");
                        sb.AppendLine();
                    }
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateUsageExamplesAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Usage Examples", 2));

                if (analysisResult.Scripts?.Interfaces != null && analysisResult.Scripts.Interfaces.Any())
                {
                    InterfaceDefinition primaryInterface = analysisResult.Scripts.Interfaces
                        .Where(i => i.AccessModifier == AccessModifier.Public)
                        .OrderByDescending(i => i.Methods.Count)
                        .FirstOrDefault();

                    if (primaryInterface != null)
                    {
                        sb.AppendLine($"**Basic Usage - {primaryInterface.Name}:**");
                        sb.AppendLine("```csharp");
                        sb.AppendLine($"// Initialize the service");
                        sb.AppendLine($"var service = new {primaryInterface.Name.Substring(1)}(); // Assuming I-prefix removal");
                        sb.AppendLine();

                        foreach (MethodSignature method in primaryInterface.Methods.Take(3))
                        {
                            string sampleCall = GenerateSampleMethodCall(method);
                            sb.AppendLine($"// {GetMethodDescription(method)}");
                            sb.AppendLine(sampleCall);
                            sb.AppendLine();
                        }
                        sb.AppendLine("```");
                        sb.AppendLine();
                    }
                }

                if (analysisResult.Scripts?.Classes?.Any(c => c.IsScriptableObject) == true)
                {
                    ClassDefinition scriptableObject = analysisResult.Scripts.Classes
                        .First(c => c.IsScriptableObject);

                    sb.AppendLine($"**ScriptableObject Usage - {scriptableObject.Name}:**");
                    sb.AppendLine("```csharp");
                    sb.AppendLine($"// Create asset instance");
                    sb.AppendLine($"var data = ScriptableObject.CreateInstance<{scriptableObject.Name}>();");
                    sb.AppendLine();

                    IEnumerable<PropertyDefinition> publicProperties = scriptableObject.Properties
                        .Where(p => p.AccessModifier == AccessModifier.Public)
                        .Take(2);

                    foreach (PropertyDefinition prop in publicProperties)
                    {
                        string sampleValue = GetSampleValue(prop.Type);
                        sb.AppendLine($"// Configure {prop.Name}");
                        sb.AppendLine($"data.{prop.Name} = {sampleValue};");
                    }
                    sb.AppendLine("```");
                    sb.AppendLine();
                }

                sb.AppendLine("**Integration Notes:**");
                sb.AppendLine("- Follow dependency injection patterns where interfaces are available");
                sb.AppendLine("- Use async/await for methods returning Task types");
                sb.AppendLine("- Handle exceptions appropriately in production code");
                sb.AppendLine("- Consider using Unity's lifecycle methods for MonoBehaviour components");
                sb.AppendLine();

                return sb.ToString();
            });
        }

        private bool IsServiceClass(ClassDefinition classDefinition)
        {
            return classDefinition.Name.EndsWith("Service") ||
                   classDefinition.Name.EndsWith("Manager") ||
                   classDefinition.Name.EndsWith("Provider") ||
                   classDefinition.Name.Contains("Service") ||
                   classDefinition.Interfaces.Any(i => i.StartsWith("I") && (i.Contains("Service") || i.Contains("Manager")));
        }

        private string GetInterfacePurpose(InterfaceDefinition iface)
        {
            if (iface.Name.Contains("Service")) return "Service contract defining business logic operations";
            if (iface.Name.Contains("Manager")) return "Management interface for coordinating resources";
            if (iface.Name.Contains("Provider")) return "Provider contract for supplying data or services";
            if (iface.Name.Contains("Repository")) return "Data access contract for persistent storage";
            if (iface.Name.Contains("Factory")) return "Factory interface for object creation";
            
            return $"Contract defining {iface.Name} behavior and responsibilities";
        }

        private string GetClassUsageDescription(ClassDefinition classDefinition)
        {
            if (classDefinition.IsScriptableObject) return "Data container - create instances via ScriptableObject.CreateInstance<>()";
            if (classDefinition.Name.Contains("Service")) return "Service implementation - typically injected or accessed via singleton";
            if (classDefinition.Name.Contains("Manager")) return "Manager class - coordinate resources and system state";
            if (classDefinition.Name.Contains("Util")) return "Utility class - static methods for common operations";
            
            return $"Instantiate and use as needed in your application logic";
        }

        private string GetServiceDescription(ClassDefinition service)
        {
            if (service.Name.Contains("Data")) return "Manages data operations and persistence";
            if (service.Name.Contains("Audio")) return "Handles audio playback and sound management";
            if (service.Name.Contains("UI")) return "Manages user interface and interaction";
            if (service.Name.Contains("Game")) return "Core game logic and state management";
            if (service.Name.Contains("Network")) return "Network communication and connectivity";
            
            return $"Provides {service.Name.Replace("Service", "").Replace("Manager", "")} functionality";
        }

        private string GetExtensionPurpose(ClassDefinition abstractClass)
        {
            if (abstractClass.Name.Contains("Handler")) return "event handling";
            if (abstractClass.Name.Contains("Generator")) return "content generation";
            if (abstractClass.Name.Contains("Processor")) return "data processing";
            if (abstractClass.Name.Contains("Controller")) return "control logic";
            
            return "behavior customization";
        }

        private string GetMethodModifiers(MethodDefinition method)
        {
            List<string> modifiers = new List<string>();
            
            if (method.IsStatic) modifiers.Add("static");
            if (method.IsAsync) modifiers.Add("async");
            if (method.IsVirtual) modifiers.Add("virtual");
            if (method.IsOverride) modifiers.Add("override");
            
            return modifiers.Any() ? string.Join(" ", modifiers) + " " : "";
        }

        private string GetPropertyAccessors(PropertyDefinition property)
        {
            if (property.HasGetter && property.HasSetter) return "{ get; set; }";
            if (property.HasGetter) return "{ get; }";
            if (property.HasSetter) return "{ set; }";
            return "{ }";
        }

        private string GenerateSampleMethodCall(MethodSignature method)
        {
            string parameters = string.Join(", ", method.Parameters.Select(p => GetSampleValue(p.Type)));
            bool isAsync = method.ReturnType.Contains("Task");
            string awaitKeyword = isAsync ? "await " : "";
            string assignment = method.ReturnType != "void" && !method.ReturnType.Contains("Task") ? "var result = " : "";
            
            return $"{assignment}{awaitKeyword}service.{method.Name}({parameters});";
        }

        private string GetMethodDescription(MethodSignature method)
        {
            if (method.Name.Contains("Create")) return "Creates a new instance or resource";
            if (method.Name.Contains("Get")) return "Retrieves data or objects";
            if (method.Name.Contains("Save")) return "Persists data to storage";
            if (method.Name.Contains("Load")) return "Loads data from storage";
            if (method.Name.Contains("Update")) return "Updates existing data";
            if (method.Name.Contains("Delete")) return "Removes data or resources";
            
            return $"Executes {method.Name} operation";
        }

        private string GetSampleValue(string type)
        {
            return type.ToLower() switch
            {
                "string" => "\"example\"",
                "int" => "42",
                "float" => "1.0f",
                "bool" => "true",
                "double" => "1.0",
                "long" => "123L",
                _ when type.Contains("[]") => "new " + type + " { }",
                _ when type.Contains("List") => "new " + type + "()",
                _ => "null"
            };
        }
    }
}