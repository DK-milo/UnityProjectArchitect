using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityProjectArchitect.AI;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services
{
    public class ScriptAnalyzer : IScriptAnalyzer
    {
        private readonly List<string> supportedLanguages = new List<string> { "C#" };
        private readonly List<string> supportedExtensions = new List<string> { ".cs" };

        public async Task<ScriptAnalysisResult> AnalyzeScriptAsync(string scriptPath)
        {
            ScriptAnalysisResult result = new ScriptAnalysisResult();
            
            if (File.Exists(scriptPath))
            {
                await AnalyzeSingleScriptAsync(scriptPath, result);
            }
            else if (Directory.Exists(scriptPath))
            {
                await AnalyzeDirectoryAsync(scriptPath, result);
            }
            else
            {
                throw new ArgumentException($"Script path does not exist: {scriptPath}");
            }

            result.Dependencies = await BuildDependencyGraphAsync(scriptPath);
            result.DetectedPatterns = DetectDesignPatterns(result.Classes);
            result.Issues = DetectCodeIssues(result);
            result.Metrics = CalculateCodeMetrics(result);

            return result;
        }

        public async Task<List<ClassDefinition>> ExtractClassDefinitionsAsync(string scriptPath)
        {
            List<ClassDefinition> classes = new List<ClassDefinition>();
            
            if (File.Exists(scriptPath))
            {
                string content = await File.ReadAllTextAsync(scriptPath);
                classes.AddRange(ExtractClassesFromContent(content, scriptPath));
            }
            else if (Directory.Exists(scriptPath))
            {
                string[] scriptFiles = Directory.GetFiles(scriptPath, "*.cs", SearchOption.AllDirectories);
                
                foreach (string file in scriptFiles)
                {
                    string content = await File.ReadAllTextAsync(file);
                    classes.AddRange(ExtractClassesFromContent(content, file));
                }
            }

            return classes;
        }

        public async Task<List<MethodDefinition>> ExtractMethodDefinitionsAsync(string scriptPath)
        {
            List<MethodDefinition> methods = new List<MethodDefinition>();
            List<ClassDefinition> classes = await ExtractClassDefinitionsAsync(scriptPath);
            
            foreach (ClassDefinition classDefinition in classes)
            {
                methods.AddRange(classDefinition.Methods);
            }

            return methods;
        }

        public async Task<DependencyGraph> BuildDependencyGraphAsync(string scriptsPath)
        {
            DependencyGraph dependencyGraph = new DependencyGraph();
            List<ClassDefinition> classes = await ExtractClassDefinitionsAsync(scriptsPath);

            foreach (ClassDefinition classDefinition in classes)
            {
                DependencyNode node = new DependencyNode(classDefinition.FullName, classDefinition.Name, "Class")
                {
                    FilePath = classDefinition.FilePath
                };
                dependencyGraph.Nodes.Add(node);

                List<string> dependencies = new List<string>();
                
                dependencies.AddRange(classDefinition.BaseClasses);
                dependencies.AddRange(classDefinition.Interfaces);
                
                foreach (MethodDefinition method in classDefinition.Methods)
                {
                    dependencies.AddRange(ExtractTypeDependenciesFromMethod(method));
                }

                dependencyGraph.DirectDependencies[classDefinition.FullName] = dependencies.Distinct().ToList();

                foreach (string dependency in dependencies.Distinct())
                {
                    DependencyEdge edge = new DependencyEdge(classDefinition.FullName, dependency, DetermineDependencyType(dependency, classDefinition));
                    dependencyGraph.Edges.Add(edge);

                    if (!dependencyGraph.ReverseDependencies.ContainsKey(dependency))
                    {
                        dependencyGraph.ReverseDependencies[dependency] = new List<string>();
                    }
                    dependencyGraph.ReverseDependencies[dependency].Add(classDefinition.FullName);
                }
            }

            return dependencyGraph;
        }

        public List<string> GetSupportedLanguages()
        {
            return new List<string>(supportedLanguages);
        }

        public bool CanAnalyzeScript(string scriptPath)
        {
            if (string.IsNullOrEmpty(scriptPath))
                return false;

            if (File.Exists(scriptPath))
            {
                return supportedExtensions.Contains(Path.GetExtension(scriptPath));
            }

            return Directory.Exists(scriptPath);
        }

        private async Task AnalyzeSingleScriptAsync(string filePath, ScriptAnalysisResult result)
        {
            string content = await File.ReadAllTextAsync(filePath);
            List<ClassDefinition> classes = ExtractClassesFromContent(content, filePath);
            result.Classes.AddRange(classes);

            List<InterfaceDefinition> interfaces = ExtractInterfacesFromContent(content, filePath);
            result.Interfaces.AddRange(interfaces);

            foreach (string classDefinition in classes)
            {
                result.Methods.AddRange(classDefinition.Methods);
            }
        }

        private async Task AnalyzeDirectoryAsync(string directoryPath, ScriptAnalysisResult result)
        {
            string[] scriptFiles = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories);
            
            foreach (string file in scriptFiles)
            {
                await AnalyzeSingleScriptAsync(file, result);
            }
        }

        private List<ClassDefinition> ExtractClassesFromContent(string content, string filePath)
        {
            List<string> classes = new List<ClassDefinition>();
            string[] lines = content.Split('\n');
            
            string namespacePattern = @"namespace\s+([A-Za-z_][A-Za-z0-9_.]*)\s*{";
            string classPattern = @"(public|private|protected|internal)?\s*(abstract|sealed|static)?\s*(class|struct)\s+([A-Za-z_][A-Za-z0-9_]*)\s*(?::\s*([^{]+))?\s*{";

            string currentNamespace = "";
            Match namespaceMatch = Regex.Match(content, namespacePattern);
            if (namespaceMatch.Success)
            {
                currentNamespace = namespaceMatch.Groups[1].Value;
            }

            MatchCollection classMatches = Regex.Matches(content, classPattern);
            
            foreach (Match match in classMatches)
            {
                ClassDefinition classDefinition = new ClassDefinition
                {
                    Name = match.Groups[4].Value,
                    FullName = string.IsNullOrEmpty(currentNamespace) ? match.Groups[4].Value : $"{currentNamespace}.{match.Groups[4].Value}",
                    Namespace = currentNamespace,
                    FilePath = filePath,
                    AccessModifier = ParseAccessModifier(match.Groups[1].Value),
                    Type = match.Groups[3].Value == "struct" ? ClassType.Struct : ClassType.Class
                };

                if (!string.IsNullOrEmpty(match.Groups[5].Value))
                {
                    string[] inheritance = match.Groups[5].Value.Split(',').Select(s => s.Trim()).ToArray();
                    foreach (string item in inheritance)
                    {
                        if (item.StartsWith("I") && char.IsUpper(item[1]))
                        {
                            classDefinition.Interfaces.Add(item);
                        }
                        else
                        {
                            classDefinition.BaseClasses.Add(item);
                        }
                    }
                }

                classDefinition.Methods = ExtractMethodsFromClass(content, classDefinition.Name);
                classDefinition.Properties = ExtractPropertiesFromClass(content, classDefinition.Name);
                classDefinition.Fields = ExtractFieldsFromClass(content, classDefinition.Name);
                classDefinition.Attributes = ExtractAttributesFromClass(content, classDefinition.Name);
                
                string classContent = ExtractClassContent(content, classDefinition.Name);
                classDefinition.LinesOfCode = classContent.Split('\n').Length;
                classDefinition.Complexity = CalculateClassComplexity(classContent);

                classes.Add(classDefinition);
            }

            return classes;
        }

        private List<InterfaceDefinition> ExtractInterfacesFromContent(string content, string filePath)
        {
            List<InterfaceDefinition> interfaces = new List<InterfaceDefinition>();
            
            string namespacePattern = @"namespace\s+([A-Za-z_][A-Za-z0-9_.]*)\s*{";
            string interfacePattern = @"(public|private|protected|internal)?\s*interface\s+([A-Za-z_][A-Za-z0-9_]*)\s*(?::\s*([^{]+))?\s*{";

            string currentNamespace = "";
            Match namespaceMatch = Regex.Match(content, namespacePattern);
            if (namespaceMatch.Success)
            {
                currentNamespace = namespaceMatch.Groups[1].Value;
            }

            MatchCollection interfaceMatches = Regex.Matches(content, interfacePattern);

            foreach (Match match in interfaceMatches)
            {
                InterfaceDefinition interfaceDefinition = new InterfaceDefinition
                {
                    Name = match.Groups[2].Value,
                    FullName = string.IsNullOrEmpty(currentNamespace) ? match.Groups[2].Value : $"{currentNamespace}.{match.Groups[2].Value}",
                    Namespace = currentNamespace,
                    FilePath = filePath,
                    AccessModifier = ParseAccessModifier(match.Groups[1].Value)
                };

                if (!string.IsNullOrEmpty(match.Groups[3].Value))
                {
                    interfaceDefinition.BaseInterfaces = match.Groups[3].Value.Split(',').Select(s => s.Trim()).ToList();
                }

                string interfaceContent = ExtractInterfaceContent(content, interfaceDefinition.Name);
                interfaceDefinition.Methods = ExtractMethodSignaturesFromInterface(interfaceContent);
                interfaceDefinition.Properties = ExtractPropertySignaturesFromInterface(interfaceContent);
                interfaceDefinition.LinesOfCode = interfaceContent.Split('\n').Length;

                interfaces.Add(interfaceDefinition);
            }

            return interfaces;
        }

        private List<MethodDefinition> ExtractMethodsFromClass(string content, string className)
        {
            List<MethodDefinition> methods = new List<MethodDefinition>();
            
            string methodPattern = @"(public|private|protected|internal)?\s*(static|virtual|override|abstract)?\s*(async)?\s*([A-Za-z_][A-Za-z0-9_<>,\[\]]*)\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(([^)]*)\)\s*{";
            MatchCollection matches = Regex.Matches(content, methodPattern);

            foreach (Match match in matches)
            {
                MethodDefinition method = new MethodDefinition
                {
                    Name = match.Groups[5].Value,
                    ReturnType = match.Groups[4].Value,
                    AccessModifier = ParseAccessModifier(match.Groups[1].Value),
                    IsStatic = match.Groups[2].Value.Contains("static"),
                    IsVirtual = match.Groups[2].Value.Contains("virtual"),
                    IsOverride = match.Groups[2].Value.Contains("override"),
                    IsAsync = match.Groups[3].Value == "async"
                };

                if (!string.IsNullOrEmpty(match.Groups[6].Value))
                {
                    method.Parameters = ParseParameters(match.Groups[6].Value);
                }

                string methodContent = ExtractMethodContent(content, method.Name);
                method.LinesOfCode = methodContent.Split('\n').Length;
                method.CyclomaticComplexity = CalculateMethodComplexity(methodContent);

                methods.Add(method);
            }

            return methods;
        }

        private List<PropertyDefinition> ExtractPropertiesFromClass(string content, string className)
        {
            List<PropertyDefinition> properties = new List<PropertyDefinition>();
            
            string propertyPattern = @"(public|private|protected|internal)?\s*([A-Za-z_][A-Za-z0-9_<>,\[\]]*)\s+([A-Za-z_][A-Za-z0-9_]*)\s*{\s*(get[^}]*})?[^}]*(set[^}]*})?";
            MatchCollection matches = Regex.Matches(content, propertyPattern);

            foreach (Match match in matches)
            {
                PropertyDefinition property = new PropertyDefinition
                {
                    Name = match.Groups[3].Value,
                    Type = match.Groups[2].Value,
                    AccessModifier = ParseAccessModifier(match.Groups[1].Value),
                    HasGetter = !string.IsNullOrEmpty(match.Groups[4].Value),
                    HasSetter = !string.IsNullOrEmpty(match.Groups[5].Value),
                    IsAutoProperty = match.Groups[4].Value.Contains("get;") || match.Groups[5].Value.Contains("set;")
                };

                properties.Add(property);
            }

            return properties;
        }

        private List<FieldDefinition> ExtractFieldsFromClass(string content, string className)
        {
            List<FieldDefinition> fields = new List<FieldDefinition>();
            
            string fieldPattern = @"(public|private|protected|internal)?\s*(static|readonly|const)?\s*([A-Za-z_][A-Za-z0-9_<>,\[\]]*)\s+([A-Za-z_][A-Za-z0-9_]*)\s*(?:=\s*([^;]+))?\s*;";
            MatchCollection matches = Regex.Matches(content, fieldPattern);

            foreach (Match match in matches)
            {
                FieldDefinition field = new FieldDefinition
                {
                    Name = match.Groups[4].Value,
                    Type = match.Groups[3].Value,
                    AccessModifier = ParseAccessModifier(match.Groups[1].Value),
                    IsStatic = match.Groups[2].Value.Contains("static"),
                    IsReadOnly = match.Groups[2].Value.Contains("readonly"),
                    IsConst = match.Groups[2].Value.Contains("const"),
                    DefaultValue = match.Groups[5].Value
                };

                fields.Add(field);
            }

            return fields;
        }

        private List<string> ExtractAttributesFromClass(string content, string className)
        {
            List<string> attributes = new List<string>();
            
            string attributePattern = @"\[([^\]]+)\]";
            MatchCollection matches = Regex.Matches(content, attributePattern);

            foreach (Match match in matches)
            {
                attributes.Add(match.Groups[1].Value);
            }

            return attributes;
        }

        private List<MethodSignature> ExtractMethodSignaturesFromInterface(string content)
        {
            List<MethodSignature> methods = new List<MethodSignature>();
            
            string methodPattern = @"([A-Za-z_][A-Za-z0-9_<>,\[\]]*)\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(([^)]*)\)\s*;";
            MatchCollection matches = Regex.Matches(content, methodPattern);

            foreach (Match match in matches)
            {
                MethodSignature method = new MethodSignature
                {
                    ReturnType = match.Groups[1].Value,
                    Name = match.Groups[2].Value
                };

                if (!string.IsNullOrEmpty(match.Groups[3].Value))
                {
                    method.Parameters = ParseParameters(match.Groups[3].Value);
                }

                methods.Add(method);
            }

            return methods;
        }

        private List<PropertySignature> ExtractPropertySignaturesFromInterface(string content)
        {
            List<PropertySignature> properties = new List<PropertySignature>();
            
            string propertyPattern = @"([A-Za-z_][A-Za-z0-9_<>,\[\]]*)\s+([A-Za-z_][A-Za-z0-9_]*)\s*{\s*(get[^}]*;)?[^}]*(set[^}]*;)?";
            MatchCollection matches = Regex.Matches(content, propertyPattern);

            foreach (Match match in matches)
            {
                PropertySignature property = new PropertySignature
                {
                    Type = match.Groups[1].Value,
                    Name = match.Groups[2].Value,
                    HasGetter = !string.IsNullOrEmpty(match.Groups[3].Value),
                    HasSetter = !string.IsNullOrEmpty(match.Groups[4].Value)
                };

                properties.Add(property);
            }

            return properties;
        }

        private List<ParameterDefinition> ParseParameters(string parametersString)
        {
            List<ParameterDefinition> parameters = new List<ParameterDefinition>();
            
            if (string.IsNullOrWhiteSpace(parametersString))
                return parameters;

            string[] paramParts = parametersString.Split(',');
            
            foreach (string part in paramParts)
            {
                string trimmed = part.Trim();
                string[] words = trimmed.Split(' ');
                
                if (words.Length >= 2)
                {
                    ParameterDefinition parameter = new ParameterDefinition
                    {
                        Type = words[words.Length - 2],
                        Name = words[words.Length - 1]
                    };

                    if (trimmed.Contains("out "))
                        parameter.IsOut = true;
                    if (trimmed.Contains("ref "))
                        parameter.IsRef = true;
                    if (trimmed.Contains("params "))
                        parameter.IsParams = true;

                    if (trimmed.Contains("="))
                    {
                        int equalIndex = trimmed.IndexOf('=');
                        parameter.DefaultValue = trimmed.Substring(equalIndex + 1).Trim();
                    }

                    parameters.Add(parameter);
                }
            }

            return parameters;
        }

        private AccessModifier ParseAccessModifier(string modifier)
        {
            return modifier?.ToLower() switch
            {
                "public" => AccessModifier.Public,
                "private" => AccessModifier.Private,
                "protected" => AccessModifier.Protected,
                "internal" => AccessModifier.Internal,
                _ => AccessModifier.Private
            };
        }

        private string ExtractClassContent(string content, string className)
        {
            string pattern = $@"class\s+{className}\s*[^{{]*{{([^{{}}]*({{[^{{}}]*}}[^{{}}]*)*[^{{}}]*)}}";
            Match match = Regex.Match(content, pattern, RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value : "";
        }

        private string ExtractInterfaceContent(string content, string interfaceName)
        {
            string pattern = $@"interface\s+{interfaceName}\s*[^{{]*{{([^{{}}]*({{[^{{}}]*}}[^{{}}]*)*[^{{}}]*)}}";
            Match match = Regex.Match(content, pattern, RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value : "";
        }

        private string ExtractMethodContent(string content, string methodName)
        {
            string pattern = $@"{methodName}\s*\([^)]*\)\s*{{([^{{}}]*({{[^{{}}]*}}[^{{}}]*)*[^{{}}]*)}}";
            Match match = Regex.Match(content, pattern, RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value : "";
        }

        private float CalculateClassComplexity(string classContent)
        {
            string[] complexityKeywords = new[] { "if", "else", "while", "for", "foreach", "switch", "case", "catch", "&&", "||" };
            float complexity = 1f;

            foreach (string keyword in complexityKeywords)
            {
                complexity += Regex.Matches(classContent, $@"\b{keyword}\b").Count;
            }

            return complexity;
        }

        private float CalculateMethodComplexity(string methodContent)
        {
            string[] complexityKeywords = new[] { "if", "else", "while", "for", "foreach", "switch", "case", "catch", "&&", "||" };
            float complexity = 1f;

            foreach (string keyword in complexityKeywords)
            {
                complexity += Regex.Matches(methodContent, $@"\b{keyword}\b").Count;
            }

            return complexity;
        }

        private List<string> ExtractTypeDependenciesFromMethod(MethodDefinition method)
        {
            List<string> dependencies = new List<string>();
            
            dependencies.Add(method.ReturnType);
            dependencies.AddRange(method.Parameters.Select(p => p.Type));

            return dependencies.Where(d => !string.IsNullOrEmpty(d) && !IsPrimitiveType(d)).ToList();
        }

        private bool IsPrimitiveType(string type)
        {
            string[] primitiveTypes = new[] { "int", "float", "double", "bool", "string", "char", "byte", "short", "long", "decimal", "void" };
            return primitiveTypes.Contains(type.ToLower());
        }

        private DependencyType DetermineDependencyType(string dependency, ClassDefinition classDefinition)
        {
            if (classDefinition.BaseClasses.Contains(dependency))
                return DependencyType.Inheritance;
            
            if (classDefinition.Interfaces.Contains(dependency))
                return DependencyType.Inheritance;
            
            return DependencyType.Usage;
        }

        private List<DesignPattern> DetectDesignPatterns(List<ClassDefinition> classes)
        {
            List<DesignPattern> patterns = new List<DesignPattern>();

            patterns.AddRange(DetectSingletonPattern(classes));
            patterns.AddRange(DetectFactoryPattern(classes));
            patterns.AddRange(DetectObserverPattern(classes));

            return patterns;
        }

        private List<DesignPattern> DetectSingletonPattern(List<ClassDefinition> classes)
        {
            List<DesignPattern> patterns = new List<DesignPattern>();

            foreach (ClassDefinition classDefinition in classes)
            {
                bool hasStaticInstance = classDefinition.Fields.Any(f => f.IsStatic && f.Type == classDefinition.Name);
                bool hasPrivateConstructor = classDefinition.Methods.Any(m => m.Name == classDefinition.Name && m.AccessModifier == AccessModifier.Private);
                
                if (hasStaticInstance && hasPrivateConstructor)
                {
                    patterns.Add(new DesignPattern(DesignPatternType.Singleton, "Singleton")
                    {
                        Confidence = 0.9f,
                        InvolvedClasses = { classDefinition.Name },
                        Evidence = "Has static instance field and private constructor"
                    });
                }
            }

            return patterns;
        }

        private List<DesignPattern> DetectFactoryPattern(List<ClassDefinition> classes)
        {
            List<DesignPattern> patterns = new List<DesignPattern>();

            foreach (ClassDefinition classDefinition in classes)
            {
                if (classDefinition.Name.Contains("Factory"))
                {
                    bool hasCreateMethod = classDefinition.Methods.Any(m => m.Name.Contains("Create"));
                    
                    if (hasCreateMethod)
                    {
                        patterns.Add(new DesignPattern(DesignPatternType.Factory, "Factory")
                        {
                            Confidence = 0.8f,
                            InvolvedClasses = { classDefinition.Name },
                            Evidence = "Class name contains 'Factory' and has Create method"
                        });
                    }
                }
            }

            return patterns;
        }

        private List<DesignPattern> DetectObserverPattern(List<ClassDefinition> classes)
        {
            List<DesignPattern> patterns = new List<DesignPattern>();

            foreach (ClassDefinition classDefinition in classes)
            {
                bool hasEventFields = classDefinition.Fields.Any(f => f.Type.Contains("Action") || f.Type.Contains("Event"));
                bool hasNotifyMethod = classDefinition.Methods.Any(m => m.Name.Contains("Notify") || m.Name.Contains("Update"));
                
                if (hasEventFields && hasNotifyMethod)
                {
                    patterns.Add(new DesignPattern(DesignPatternType.Observer, "Observer")
                    {
                        Confidence = 0.7f,
                        InvolvedClasses = { classDefinition.Name },
                        Evidence = "Has event fields and notify/update methods"
                    });
                }
            }

            return patterns;
        }

        private List<CodeIssue> DetectCodeIssues(ScriptAnalysisResult result)
        {
            List<CodeIssue> issues = new List<CodeIssue>();

            foreach (ClassDefinition classDefinition in result.Classes)
            {
                if (classDefinition.Methods.Count > 20)
                {
                    issues.Add(new CodeIssue(CodeIssueType.CodeSmell, 
                        $"Class {classDefinition.Name} has too many methods ({classDefinition.Methods.Count})", 
                        classDefinition.FilePath)
                    {
                        Severity = CodeIssueSeverity.Major,
                        LineNumber = classDefinition.StartLine,
                        Suggestion = "Consider breaking this class into smaller, more focused classes"
                    });
                }

                if (classDefinition.LinesOfCode > 500)
                {
                    issues.Add(new CodeIssue(CodeIssueType.CodeSmell, 
                        $"Class {classDefinition.Name} is very large ({classDefinition.LinesOfCode} lines)", 
                        classDefinition.FilePath)
                    {
                        Severity = CodeIssueSeverity.Major,
                        LineNumber = classDefinition.StartLine,
                        Suggestion = "Consider refactoring this class to reduce its size"
                    });
                }

                foreach (MethodDefinition method in classDefinition.Methods)
                {
                    if (method.CyclomaticComplexity > 10)
                    {
                        issues.Add(new CodeIssue(CodeIssueType.CodeSmell, 
                            $"Method {method.Name} has high cyclomatic complexity ({method.CyclomaticComplexity})", 
                            classDefinition.FilePath)
                        {
                            Severity = CodeIssueSeverity.Major,
                            LineNumber = method.StartLine,
                            Suggestion = "Consider breaking this method into smaller, simpler methods"
                        });
                    }
                }
            }

            return issues;
        }

        private CodeMetrics CalculateCodeMetrics(ScriptAnalysisResult result)
        {
            CodeMetrics metrics = new CodeMetrics
            {
                TotalClasses = result.Classes.Count,
                TotalMethods = result.Methods.Count,
                TotalLinesOfCode = result.Classes.Sum(c => c.LinesOfCode)
            };

            if (result.Methods.Count > 0)
            {
                metrics.AverageCyclomaticComplexity = result.Methods.Average(m => m.CyclomaticComplexity);
                metrics.MaxCyclomaticComplexity = result.Methods.Max(m => m.CyclomaticComplexity);
            }

            metrics.MetricsByType["MonoBehaviour"] = result.Classes.Count(c => c.IsMonoBehaviour);
            metrics.MetricsByType["ScriptableObject"] = result.Classes.Count(c => c.IsScriptableObject);
            metrics.MetricsByType["Interface"] = result.Interfaces.Count;

            return metrics;
        }
    }
}