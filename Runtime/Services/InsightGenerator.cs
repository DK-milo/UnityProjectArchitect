using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityProjectArchitect.API;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services
{
    public class InsightGenerator
    {
        public async Task<List<ProjectInsight>> GenerateInsightsAsync(ProjectAnalysisResult analysisResult)
        {
            List<string> insights = new List<ProjectInsight>();

            await Task.Run(() =>
            {
                try
                {
                    insights.AddRange(GenerateStructureInsights(analysisResult));
                    insights.AddRange(GenerateCodeQualityInsights(analysisResult));
                    insights.AddRange(GeneratePerformanceInsights(analysisResult));
                    insights.AddRange(GenerateArchitectureInsights(analysisResult));
                    insights.AddRange(GenerateDependencyInsights(analysisResult));
                    insights.AddRange(GenerateMaintainabilityInsights(analysisResult));
                    insights.AddRange(GenerateTestingInsights(analysisResult));
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error generating insights: {ex.Message}");
                    
                    insights.Add(new ProjectInsight(InsightType.ProjectStructure, 
                        "Analysis Error", 
                        $"Failed to generate complete insights: {ex.Message}")
                    {
                        Severity = InsightSeverity.Critical,
                        Confidence = 1.0f
                    });
                }
            });

            return insights.OrderByDescending(i => i.Severity).ThenByDescending(i => i.Confidence).ToList();
        }

        private List<ProjectInsight> GenerateStructureInsights(ProjectAnalysisResult analysisResult)
        {
            List<string> insights = new List<ProjectInsight>();

            if (analysisResult.Structure == null) return insights;

            if (!analysisResult.Structure.FollowsStandardStructure)
            {
                insights.Add(new ProjectInsight(InsightType.ProjectStructure,
                    "Non-standard Project Structure",
                    "Your project doesn't follow Unity's recommended folder structure")
                {
                    Severity = InsightSeverity.Medium,
                    Confidence = 0.9f,
                    Context = "Project Organization",
                    Evidence = { $"Missing standard folders like Scripts, Prefabs, or Materials" }
                });
            }

            var deepNestingIssues = analysisResult.Structure.Issues
                .Where(i => i.Type == StructureIssueType.DeepNesting).ToList();
            
            if (deepNestingIssues.Count > 0)
            {
                insights.Add(new ProjectInsight(InsightType.ProjectStructure,
                    "Deep Folder Nesting Detected",
                    $"Found {deepNestingIssues.Count} folders with excessive nesting depth")
                {
                    Severity = InsightSeverity.Low,
                    Confidence = 0.8f,
                    Context = "Folder Structure",
                    Evidence = deepNestingIssues.Select(i => i.Path).Take(3).ToList()
                });
            }

            var largeFileIssues = analysisResult.Structure.Issues
                .Where(i => i.Type == StructureIssueType.LargeFile).ToList();
            
            if (largeFileIssues.Count > 0)
            {
                insights.Add(new ProjectInsight(InsightType.ProjectStructure,
                    "Large Files Detected",
                    $"Found {largeFileIssues.Count} unusually large files that may impact performance")
                {
                    Severity = InsightSeverity.Medium,
                    Confidence = 0.7f,
                    Context = "File Management",
                    Evidence = largeFileIssues.Select(i => $"{i.Path} - {i.Description}").Take(3).ToList()
                });
            }

            if (analysisResult.Structure.DetectedProjectType != ProjectType.General)
            {
                insights.Add(new ProjectInsight(InsightType.ProjectStructure,
                    $"Project Type Identified: {analysisResult.Structure.DetectedProjectType}",
                    $"Your project appears to be a {analysisResult.Structure.DetectedProjectType} project")
                {
                    Severity = InsightSeverity.Info,
                    Confidence = 0.8f,
                    Context = "Project Classification",
                    Data = { ["ProjectType"] = analysisResult.Structure.DetectedProjectType.ToString() }
                });
            }

            return insights;
        }

        private List<ProjectInsight> GenerateCodeQualityInsights(ProjectAnalysisResult analysisResult)
        {
            List<string> insights = new List<ProjectInsight>();

            if (analysisResult.Scripts == null) return insights;

            var criticalIssues = analysisResult.Scripts.Issues
                .Where(i => i.Severity == CodeIssueSeverity.Critical).ToList();
            
            if (criticalIssues.Count > 0)
            {
                insights.Add(new ProjectInsight(InsightType.CodeQuality,
                    "Critical Code Issues Found",
                    $"Detected {criticalIssues.Count} critical code issues that need immediate attention")
                {
                    Severity = InsightSeverity.Critical,
                    Confidence = 0.95f,
                    Context = "Code Quality",
                    Evidence = criticalIssues.Select(i => $"{i.Description} in {i.FilePath}").Take(5).ToList()
                });
            }

            var majorIssues = analysisResult.Scripts.Issues
                .Where(i => i.Severity == CodeIssueSeverity.Major).ToList();
            
            if (majorIssues.Count > 10)
            {
                insights.Add(new ProjectInsight(InsightType.CodeQuality,
                    "High Number of Code Issues",
                    $"Found {majorIssues.Count} major code issues that should be addressed")
                {
                    Severity = InsightSeverity.High,
                    Confidence = 0.9f,
                    Context = "Code Quality",
                    Evidence = majorIssues.GroupBy(i => i.Type)
                                        .Select(g => $"{g.Key}: {g.Count()} issues")
                                        .Take(5).ToList()
                });
            }

            if (analysisResult.Scripts.Metrics != null)
            {
                var avgComplexity = analysisResult.Scripts.Metrics.AverageCyclomaticComplexity;
                if (avgComplexity > 10)
                {
                    insights.Add(new ProjectInsight(InsightType.CodeQuality,
                        "High Code Complexity",
                        $"Average cyclomatic complexity is {avgComplexity:F1}, which is above recommended levels")
                    {
                        Severity = InsightSeverity.Medium,
                        Confidence = 0.85f,
                        Context = "Code Complexity",
                        Data = { ["AverageComplexity"] = avgComplexity },
                        Evidence = { "Recommended complexity is below 10 per method" }
                    });
                }

                var commentRatio = analysisResult.Scripts.Metrics.CommentRatio;
                if (commentRatio < 0.1f)
                {
                    insights.Add(new ProjectInsight(InsightType.CodeQuality,
                        "Low Documentation Coverage",
                        $"Only {commentRatio * 100:F1}% of your code contains comments")
                    {
                        Severity = InsightSeverity.Low,
                        Confidence = 0.7f,
                        Context = "Documentation",
                        Data = { ["CommentRatio"] = commentRatio },
                        Evidence = { "Recommended comment ratio is at least 15-20%" }
                    });
                }
            }

            var patterns = analysisResult.Scripts.DetectedPatterns;
            if (patterns.Count > 0)
            {
                List<string> strongPatterns = patterns.Where(p => p.Confidence > 0.8f).ToList();
                if (strongPatterns.Count > 0)
                {
                    insights.Add(new ProjectInsight(InsightType.CodeQuality,
                        "Design Patterns Detected",
                        $"Found {strongPatterns.Count} well-implemented design patterns in your code")
                    {
                        Severity = InsightSeverity.Info,
                        Confidence = 0.8f,
                        Context = "Code Architecture",
                        Evidence = strongPatterns.Select(p => $"{p.Name} pattern in {string.Join(", ", p.InvolvedClasses)}").ToList()
                    });
                }
            }

            return insights;
        }

        private List<ProjectInsight> GeneratePerformanceInsights(ProjectAnalysisResult analysisResult)
        {
            List<string> insights = new List<ProjectInsight>();

            if (analysisResult.Performance == null) return insights;

            var criticalPerformanceIssues = analysisResult.Performance.Issues
                .Where(i => i.Impact == PerformanceImpact.Critical).ToList();
            
            if (criticalPerformanceIssues.Count > 0)
            {
                insights.Add(new ProjectInsight(InsightType.Performance,
                    "Critical Performance Issues",
                    $"Detected {criticalPerformanceIssues.Count} critical performance bottlenecks")
                {
                    Severity = InsightSeverity.Critical,
                    Confidence = 0.9f,
                    Context = "Performance Optimization",
                    Evidence = criticalPerformanceIssues.Select(i => $"{i.Description} at {i.Location}").Take(3).ToList()
                });
            }

            var metrics = analysisResult.Performance.Metrics;
            if (metrics != null)
            {
                if (metrics.TextureMemoryMB > 500)
                {
                    insights.Add(new ProjectInsight(InsightType.Performance,
                        "High Texture Memory Usage",
                        $"Textures are using {metrics.TextureMemoryMB}MB of memory")
                    {
                        Severity = InsightSeverity.Medium,
                        Confidence = 0.8f,
                        Context = "Memory Usage",
                        Data = { ["TextureMemoryMB"] = metrics.TextureMemoryMB },
                        Evidence = { "Consider texture compression and resolution optimization" }
                    });
                }

                if (metrics.AudioMemoryMB > 100)
                {
                    insights.Add(new ProjectInsight(InsightType.Performance,
                        "High Audio Memory Usage",
                        $"Audio clips are using {metrics.AudioMemoryMB}MB of memory")
                    {
                        Severity = InsightSeverity.Low,
                        Confidence = 0.7f,
                        Context = "Memory Usage",
                        Data = { ["AudioMemoryMB"] = metrics.AudioMemoryMB },
                        Evidence = { "Consider audio compression and streaming for large files" }
                    });
                }
            }

            return insights;
        }

        private List<ProjectInsight> GenerateArchitectureInsights(ProjectAnalysisResult analysisResult)
        {
            List<string> insights = new List<ProjectInsight>();

            if (analysisResult.Architecture == null) return insights;

            if (analysisResult.Architecture.DetectedPattern != ArchitecturePattern.None)
            {
                insights.Add(new ProjectInsight(InsightType.Architecture,
                    $"Architecture Pattern: {analysisResult.Architecture.DetectedPattern}",
                    $"Your project follows the {analysisResult.Architecture.DetectedPattern} architectural pattern")
                {
                    Severity = InsightSeverity.Info,
                    Confidence = 0.8f,
                    Context = "Project Architecture",
                    Data = { ["ArchitecturePattern"] = analysisResult.Architecture.DetectedPattern.ToString() }
                });
            }

            var architectureIssues = analysisResult.Architecture.Issues;
            List<string> godClassIssues = architectureIssues.Where(i => i.Type == ArchitectureIssueType.GodClass).ToList();
            
            if (godClassIssues.Count > 0)
            {
                insights.Add(new ProjectInsight(InsightType.Architecture,
                    "God Classes Detected",
                    $"Found {godClassIssues.Count} classes with too many responsibilities")
                {
                    Severity = InsightSeverity.High,
                    Confidence = 0.85f,
                    Context = "Code Architecture",
                    Evidence = godClassIssues.Select(i => i.Description).Take(3).ToList()
                });
            }

            List<string> tightCouplingIssues = architectureIssues.Where(i => i.Type == ArchitectureIssueType.TightCoupling).ToList();
            if (tightCouplingIssues.Count > 0)
            {
                insights.Add(new ProjectInsight(InsightType.Architecture,
                    "Tight Coupling Detected",
                    $"Found {tightCouplingIssues.Count} instances of tight coupling between components")
                {
                    Severity = InsightSeverity.Medium,
                    Confidence = 0.8f,
                    Context = "Component Coupling",
                    Evidence = tightCouplingIssues.Select(i => i.Description).Take(3).ToList()
                });
            }

            if (analysisResult.Architecture.Metrics != null)
            {
                var coupling = analysisResult.Architecture.Metrics.AverageCoupling;
                if (coupling > 5)
                {
                    insights.Add(new ProjectInsight(InsightType.Architecture,
                        "High Component Coupling",
                        $"Average coupling is {coupling:F1}, which may indicate overly dependent components")
                    {
                        Severity = InsightSeverity.Medium,
                        Confidence = 0.7f,
                        Context = "Architecture Metrics",
                        Data = { ["AverageCoupling"] = coupling },
                        Evidence = { "Lower coupling leads to more maintainable code" }
                    });
                }

                var cohesion = analysisResult.Architecture.Metrics.AverageCohesion;
                if (cohesion < 0.6f)
                {
                    insights.Add(new ProjectInsight(InsightType.Architecture,
                        "Low Component Cohesion",
                        $"Average cohesion is {cohesion:F1}, suggesting components may lack focus")
                    {
                        Severity = InsightSeverity.Medium,
                        Confidence = 0.7f,
                        Context = "Architecture Metrics",
                        Data = { ["AverageCohesion"] = cohesion },
                        Evidence = { "Higher cohesion indicates well-focused components" }
                    });
                }
            }

            return insights;
        }

        private List<ProjectInsight> GenerateDependencyInsights(ProjectAnalysisResult analysisResult)
        {
            List<string> insights = new List<ProjectInsight>();

            if (analysisResult.Scripts?.Dependencies == null) return insights;

            var circularDependencies = analysisResult.Scripts.Dependencies.GetCircularDependencies();
            if (circularDependencies.Count > 0)
            {
                insights.Add(new ProjectInsight(InsightType.Dependencies,
                    "Circular Dependencies Found",
                    $"Detected {circularDependencies.Count} circular dependency chains")
                {
                    Severity = InsightSeverity.High,
                    Confidence = 0.95f,
                    Context = "Dependency Management",
                    Evidence = circularDependencies.Take(3).ToList()
                });
            }

            var totalDependencies = analysisResult.Scripts.Dependencies.Edges.Count;
            var totalNodes = analysisResult.Scripts.Dependencies.Nodes.Count;
            
            if (totalNodes > 0)
            {
                var avgDependenciesPerClass = (float)totalDependencies / totalNodes;
                if (avgDependenciesPerClass > 8)
                {
                    insights.Add(new ProjectInsight(InsightType.Dependencies,
                        "High Dependency Density",
                        $"Classes have an average of {avgDependenciesPerClass:F1} dependencies each")
                    {
                        Severity = InsightSeverity.Medium,
                        Confidence = 0.8f,
                        Context = "Dependency Analysis",
                        Data = { ["AverageDependencies"] = avgDependenciesPerClass },
                        Evidence = { "High dependency counts can make code harder to maintain and test" }
                    });
                }
            }

            return insights;
        }

        private List<ProjectInsight> GenerateMaintainabilityInsights(ProjectAnalysisResult analysisResult)
        {
            List<string> insights = new List<ProjectInsight>();

            if (analysisResult.Metrics == null) return insights;

            var maintainability = analysisResult.Metrics.Maintainability;
            if (maintainability < 0.6f)
            {
                insights.Add(new ProjectInsight(InsightType.Maintainability,
                    "Low Maintainability Score",
                    $"Project maintainability score is {maintainability * 100:F0}%, indicating potential maintenance challenges")
                {
                    Severity = InsightSeverity.High,
                    Confidence = 0.8f,
                    Context = "Code Maintainability",
                    Data = { ["MaintainabilityScore"] = maintainability },
                    Evidence = { "Score is calculated from complexity, technical debt, and code quality metrics" }
                });
            }

            var technicalDebt = analysisResult.Metrics.TechnicalDebt;
            if (technicalDebt > 0.7f)
            {
                insights.Add(new ProjectInsight(InsightType.Maintainability,
                    "High Technical Debt",
                    $"Technical debt level is {technicalDebt * 100:F0}%, suggesting accumulated code issues")
                {
                    Severity = InsightSeverity.High,
                    Confidence = 0.85f,
                    Context = "Technical Debt",
                    Data = { ["TechnicalDebt"] = technicalDebt },
                    Evidence = { "High technical debt increases development time and bug risk" }
                });
            }

            if (analysisResult.Scripts != null)
            {
                var totalClasses = analysisResult.Scripts.TotalClasses;
                var totalMethods = analysisResult.Scripts.TotalMethods;
                
                if (totalClasses > 0)
                {
                    var methodsPerClass = (float)totalMethods / totalClasses;
                    if (methodsPerClass > 15)
                    {
                        insights.Add(new ProjectInsight(InsightType.Maintainability,
                            "Large Class Sizes",
                            $"Classes average {methodsPerClass:F1} methods each, which may indicate oversized classes")
                        {
                            Severity = InsightSeverity.Medium,
                            Confidence = 0.7f,
                            Context = "Class Size Analysis",
                            Data = { ["MethodsPerClass"] = methodsPerClass },
                            Evidence = { "Smaller, focused classes are generally easier to maintain" }
                        });
                    }
                }
            }

            return insights;
        }

        private List<ProjectInsight> GenerateTestingInsights(ProjectAnalysisResult analysisResult)
        {
            List<string> insights = new List<ProjectInsight>();

            if (analysisResult.Structure == null) return insights;

            var hasTestFolders = analysisResult.Structure.Folders
                .Any(f => f.Name.Contains("Test", StringComparison.OrdinalIgnoreCase));
            
            var hasTestFiles = analysisResult.Structure.Files
                .Any(f => f.Name.Contains("Test", StringComparison.OrdinalIgnoreCase) && f.Extension == ".cs");

            if (!hasTestFolders && !hasTestFiles)
            {
                insights.Add(new ProjectInsight(InsightType.Testing,
                    "No Test Infrastructure Found",
                    "Your project doesn't appear to have any unit tests or testing infrastructure")
                {
                    Severity = InsightSeverity.Medium,
                    Confidence = 0.8f,
                    Context = "Quality Assurance",
                    Evidence = { "No test folders or test files detected in the project structure" }
                });
            }
            else if (hasTestFolders || hasTestFiles)
            {
                var testFileCount = analysisResult.Structure.Files
                    .Count(f => f.Name.Contains("Test", StringComparison.OrdinalIgnoreCase) && f.Extension == ".cs");
                
                var scriptFileCount = analysisResult.Structure.Files
                    .Count(f => f.Extension == ".cs" && !f.Name.Contains("Test", StringComparison.OrdinalIgnoreCase));

                if (scriptFileCount > 0)
                {
                    var testCoverageRatio = (float)testFileCount / scriptFileCount;
                    
                    if (testCoverageRatio < 0.1f)
                    {
                        insights.Add(new ProjectInsight(InsightType.Testing,
                            "Low Test Coverage",
                            $"Only {testCoverageRatio * 100:F0}% of your scripts have corresponding tests")
                        {
                            Severity = InsightSeverity.Medium,
                            Confidence = 0.7f,
                            Context = "Test Coverage",
                            Data = { ["TestCoverageRatio"] = testCoverageRatio },
                            Evidence = { $"{testFileCount} test files for {scriptFileCount} script files" }
                        });
                    }
                    else if (testCoverageRatio > 0.3f)
                    {
                        insights.Add(new ProjectInsight(InsightType.Testing,
                            "Good Test Coverage",
                            $"Your project has {testCoverageRatio * 100:F0}% test coverage, which is excellent")
                        {
                            Severity = InsightSeverity.Info,
                            Confidence = 0.8f,
                            Context = "Test Coverage",
                            Data = { ["TestCoverageRatio"] = testCoverageRatio },
                            Evidence = { $"{testFileCount} test files for {scriptFileCount} script files" }
                        });
                    }
                }
            }

            return insights;
        }
    }
}