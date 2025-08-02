using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityProjectArchitect.API;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services
{
    public class RecommendationEngine
    {
        public async Task<List<ProjectRecommendation>> GenerateRecommendationsAsync(ProjectAnalysisResult analysisResult)
        {
            List<ProjectRecommendation> recommendations = new List<ProjectRecommendation>();

            await Task.Run(() =>
            {
                try
                {
                    recommendations.AddRange(GenerateStructureRecommendations(analysisResult));
                    recommendations.AddRange(GeneratePerformanceRecommendations(analysisResult));
                    recommendations.AddRange(GenerateArchitectureRecommendations(analysisResult));
                    recommendations.AddRange(GenerateCodeQualityRecommendations(analysisResult));
                    recommendations.AddRange(GenerateDependencyRecommendations(analysisResult));
                    recommendations.AddRange(GenerateSecurityRecommendations(analysisResult));
                    recommendations.AddRange(GenerateDocumentationRecommendations(analysisResult));
                    recommendations.AddRange(GenerateTestingRecommendations(analysisResult));
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error generating recommendations: {ex.Message}");
                    
                    recommendations.Add(new ProjectRecommendation(RecommendationType.Structure, 
                        "Analysis Error")
                    {
                        Priority = RecommendationPriority.High,
                        Description = $"Failed to generate complete recommendations: {ex.Message}",
                        Rationale = "Recommendation generation encountered an error"
                    });
                }
            });

            return recommendations.OrderByDescending(r => r.Priority).ToList();
        }

        private List<ProjectRecommendation> GenerateStructureRecommendations(ProjectAnalysisResult analysisResult)
        {
            List<ProjectRecommendation> recommendations = new List<ProjectRecommendation>();

            if (analysisResult.Structure == null) return recommendations;

            if (!analysisResult.Structure.FollowsStandardStructure)
            {
                recommendations.Add(new ProjectRecommendation(RecommendationType.Structure,
                    "Implement Standard Unity Folder Structure")
                {
                    Priority = RecommendationPriority.Medium,
                    Description = "Reorganize your project to follow Unity's recommended folder structure",
                    Rationale = "Standard folder structure improves navigation, collaboration, and asset management",
                    ActionSteps = new List<ActionStep>
                    {
                        new ActionStep("Create standard folders: Scripts, Prefabs, Materials, Textures", TimeSpan.FromMinutes(15)),
                        new ActionStep("Move existing assets to appropriate folders", TimeSpan.FromHours(1)),
                        new ActionStep("Update any hardcoded paths in scripts", TimeSpan.FromMinutes(30)),
                        new ActionStep("Verify all references are maintained after reorganization", TimeSpan.FromMinutes(20))
                    },
                    Benefits = { "Better organization", "Easier asset discovery", "Improved team collaboration", "Faster onboarding" },
                    Risks = { "Temporary broken references during migration", "Time investment required" },
                    Effort = new EstimatedEffort
                    {
                        MinTime = TimeSpan.FromHours(1),
                        MaxTime = TimeSpan.FromHours(4),
                        MostLikelyTime = TimeSpan.FromHours(2),
                        Complexity = 3,
                        RequiredSkills = new[] { "Unity Editor knowledge", "Asset management" }
                    }
                });
            }

            List<StructureIssue> missingFolders = analysisResult.Structure.Issues
                .Where(i => i.Type == StructureIssueType.MissingFolder && i.Severity >= StructureIssueSeverity.Warning)
                .ToList();

            if (missingFolders.Count > 0)
            {
                recommendations.Add(new ProjectRecommendation(RecommendationType.Structure,
                    "Create Missing Essential Folders")
                {
                    Priority = RecommendationPriority.Medium,
                    Description = $"Create {missingFolders.Count} missing essential folders for better organization",
                    Rationale = "Missing essential folders can lead to disorganized assets and difficult maintenance",
                    ActionSteps = missingFolders.Select(folder => 
                        new ActionStep($"Create {folder.Path} folder", TimeSpan.FromMinutes(2))
                    ).ToList(),
                    Benefits = { "Better asset organization", "Clearer project structure", "Easier asset location" },
                    Effort = new EstimatedEffort
                    {
                        MinTime = TimeSpan.FromMinutes(10),
                        MaxTime = TimeSpan.FromMinutes(30),
                        MostLikelyTime = TimeSpan.FromMinutes(15),
                        Complexity = 1
                    }
                });
            }

            List<StructureIssue> largeFiles = analysisResult.Structure.Issues
                .Where(i => i.Type == StructureIssueType.LargeFile)
                .ToList();

            if (largeFiles.Count > 0)
            {
                recommendations.Add(new ProjectRecommendation(RecommendationType.Performance,
                    "Optimize Large Files")
                {
                    Priority = RecommendationPriority.High,
                    Description = $"Optimize or split {largeFiles.Count} large files to improve performance",
                    Rationale = "Large files can slow down Unity Editor and build times, and may indicate oversized assets",
                    ActionSteps = new List<ActionStep>
                    {
                        new ActionStep("Review each large file to determine optimization strategy", TimeSpan.FromMinutes(30)),
                        new ActionStep("Compress textures and audio files where appropriate", TimeSpan.FromHours(1)),
                        new ActionStep("Split large scripts into smaller, focused classes", TimeSpan.FromHours(2)),
                        new ActionStep("Consider streaming for very large assets", TimeSpan.FromHours(1))
                    },
                    Benefits = { "Faster Editor performance", "Reduced build times", "Better memory usage", "Improved loading times" },
                    Risks = { "Potential quality loss from compression", "Refactoring effort for large scripts" }
                });
            }

            return recommendations;
        }

        private List<ProjectRecommendation> GeneratePerformanceRecommendations(ProjectAnalysisResult analysisResult)
        {
            List<ProjectRecommendation> recommendations = new List<ProjectRecommendation>();

            if (analysisResult.Performance == null) return recommendations;

            List<PerformanceIssue> criticalIssues = analysisResult.Performance.Issues
                .Where(i => i.Impact == PerformanceImpact.Critical)
                .ToList();

            if (criticalIssues.Count > 0)
            {
                recommendations.Add(new ProjectRecommendation(RecommendationType.Performance,
                    "Address Critical Performance Issues")
                {
                    Priority = RecommendationPriority.Critical,
                    Description = $"Immediately address {criticalIssues.Count} critical performance bottlenecks",
                    Rationale = "Critical performance issues can make your game unplayable or cause frequent crashes",
                    ActionSteps = criticalIssues.Select(issue => 
                        new ActionStep($"Fix: {issue.Description}", TimeSpan.FromHours(2))
                    ).Take(5).ToList(),
                    Benefits = { "Stable frame rate", "Better user experience", "Reduced crashes", "Improved responsiveness" },
                    Effort = new EstimatedEffort
                    {
                        MinTime = TimeSpan.FromHours(4),
                        MaxTime = TimeSpan.FromHours(16),
                        MostLikelyTime = TimeSpan.FromHours(8),
                        Complexity = 4,
                        RequiredSkills = new[] { "Unity optimization", "Performance profiling", "Rendering knowledge" }
                    }
                });
            }

            PerformanceMetrics metrics = analysisResult.Performance.Metrics;
            if (metrics != null)
            {
                if (metrics.TextureMemoryMB > 500)
                {
                    recommendations.Add(new ProjectRecommendation(RecommendationType.Performance,
                        "Optimize Texture Memory Usage")
                    {
                        Priority = RecommendationPriority.High,
                        Description = $"Reduce texture memory usage from {metrics.TextureMemoryMB}MB to improve performance",
                        Rationale = "High texture memory usage can cause performance issues, especially on mobile devices",
                        ActionSteps = new List<ActionStep>
                        {
                            new ActionStep("Audit all textures for appropriate resolution", TimeSpan.FromHours(2)),
                            new ActionStep("Apply texture compression where suitable", TimeSpan.FromHours(1)),
                            new ActionStep("Remove or downscale unused high-resolution textures", TimeSpan.FromMinutes(30)),
                            new ActionStep("Implement texture streaming for large environments", TimeSpan.FromHours(4))
                        },
                        Benefits = { "Reduced memory usage", "Better performance on mobile", "Faster loading times", "Smaller build size" },
                        Risks = { "Potential visual quality reduction", "Additional implementation complexity for streaming" }
                    });
                }

                if (metrics.DrawCalls > 1000)
                {
                    recommendations.Add(new ProjectRecommendation(RecommendationType.Performance,
                        "Reduce Draw Calls")
                    {
                        Priority = RecommendationPriority.High,
                        Description = $"Optimize rendering to reduce draw calls from {metrics.DrawCalls} to improve frame rate",
                        Rationale = "High draw call counts can severely impact rendering performance",
                        ActionSteps = new List<ActionStep>
                        {
                            new ActionStep("Implement texture atlasing for UI and sprites", TimeSpan.FromHours(3)),
                            new ActionStep("Use GPU instancing for repeated objects", TimeSpan.FromHours(2)),
                            new ActionStep("Combine meshes where appropriate", TimeSpan.FromHours(2)),
                            new ActionStep("Optimize material usage and sharing", TimeSpan.FromHours(1))
                        },
                        Benefits = { "Higher frame rates", "Better GPU performance", "Smoother gameplay", "Extended battery life on mobile" }
                    });
                }
            }

            return recommendations;
        }

        private List<ProjectRecommendation> GenerateArchitectureRecommendations(ProjectAnalysisResult analysisResult)
        {
            List<ProjectRecommendation> recommendations = new List<ProjectRecommendation>();

            if (analysisResult.Architecture == null) return recommendations;

            List<ArchitectureIssue> godClassIssues = analysisResult.Architecture.Issues
                .Where(i => i.Type == ArchitectureIssueType.GodClass)
                .ToList();

            if (godClassIssues.Count > 0)
            {
                recommendations.Add(new ProjectRecommendation(RecommendationType.Architecture,
                    "Refactor God Classes")
                {
                    Priority = RecommendationPriority.High,
                    Description = $"Break down {godClassIssues.Count} oversized classes into smaller, focused components",
                    Rationale = "God classes violate single responsibility principle and are difficult to maintain and test",
                    ActionSteps = new List<ActionStep>
                    {
                        new ActionStep("Identify distinct responsibilities in each god class", TimeSpan.FromHours(2)),
                        new ActionStep("Extract related methods into new focused classes", TimeSpan.FromHours(4)),
                        new ActionStep("Update references and dependencies", TimeSpan.FromHours(1)),
                        new ActionStep("Add unit tests for new smaller classes", TimeSpan.FromHours(2))
                    },
                    Benefits = { "Better code maintainability", "Easier testing", "Improved code reusability", "Clearer responsibilities" },
                    Risks = { "Temporary increase in complexity during refactoring", "Potential introduction of bugs" },
                    Effort = new EstimatedEffort
                    {
                        MinTime = TimeSpan.FromHours(6),
                        MaxTime = TimeSpan.FromHours(20),
                        MostLikelyTime = TimeSpan.FromHours(12),
                        Complexity = 4,
                        RequiredSkills = new[] { "Refactoring", "Object-oriented design", "Unit testing" }
                    }
                });
            }

            List<ArchitectureIssue> tightCouplingIssues = analysisResult.Architecture.Issues
                .Where(i => i.Type == ArchitectureIssueType.TightCoupling)
                .ToList();

            if (tightCouplingIssues.Count > 0)
            {
                recommendations.Add(new ProjectRecommendation(RecommendationType.Architecture,
                    "Reduce Component Coupling")
                {
                    Priority = RecommendationPriority.Medium,
                    Description = $"Reduce tight coupling between {tightCouplingIssues.Count} component pairs",
                    Rationale = "Tight coupling makes code harder to modify, test, and reuse",
                    ActionSteps = new List<ActionStep>
                    {
                        new ActionStep("Introduce interfaces to decouple dependencies", TimeSpan.FromHours(3)),
                        new ActionStep("Implement dependency injection or service locator pattern", TimeSpan.FromHours(4)),
                        new ActionStep("Use events or observers for loose communication", TimeSpan.FromHours(2)),
                        new ActionStep("Refactor direct class references to use abstractions", TimeSpan.FromHours(3))
                    },
                    Benefits = { "More flexible architecture", "Easier unit testing", "Better code reusability", "Simplified modifications" }
                });
            }

            if (analysisResult.Architecture.DetectedPattern == ArchitecturePattern.None)
            {
                recommendations.Add(new ProjectRecommendation(RecommendationType.Architecture,
                    "Implement Architectural Pattern")
                {
                    Priority = RecommendationPriority.Medium,
                    Description = "Consider implementing a clear architectural pattern for better code organization",
                    Rationale = "Well-defined architecture patterns improve code organization and team understanding",
                    ActionSteps = new List<ActionStep>
                    {
                        new ActionStep("Choose appropriate pattern (MVC, MVP, or Service-oriented)", TimeSpan.FromHours(1)),
                        new ActionStep("Create architectural guidelines document", TimeSpan.FromHours(2)),
                        new ActionStep("Gradually refactor existing code to follow pattern", TimeSpan.FromHours(8)),
                        new ActionStep("Train team members on the chosen pattern", TimeSpan.FromHours(2))
                    },
                    Benefits = { "Clearer code organization", "Better team understanding", "Easier onboarding", "Consistent development approach" }
                });
            }

            return recommendations;
        }

        private List<ProjectRecommendation> GenerateCodeQualityRecommendations(ProjectAnalysisResult analysisResult)
        {
            List<ProjectRecommendation> recommendations = new List<ProjectRecommendation>();

            if (analysisResult.Scripts == null) return recommendations;

            List<CodeIssue> criticalIssues = analysisResult.Scripts.Issues
                .Where(i => i.Severity == CodeIssueSeverity.Critical)
                .ToList();

            if (criticalIssues.Count > 0)
            {
                recommendations.Add(new ProjectRecommendation(RecommendationType.CodeQuality,
                    "Fix Critical Code Issues")
                {
                    Priority = RecommendationPriority.Critical,
                    Description = $"Immediately address {criticalIssues.Count} critical code issues",
                    Rationale = "Critical code issues can cause runtime errors, security vulnerabilities, or data loss",
                    ActionSteps = criticalIssues.Select(issue => 
                        new ActionStep($"Fix: {issue.Description}", TimeSpan.FromMinutes(30))
                    ).Take(10).ToList(),
                    Benefits = { "Prevent runtime errors", "Improve code stability", "Reduce security risks", "Better user experience" },
                    Effort = new EstimatedEffort
                    {
                        MinTime = TimeSpan.FromHours(2),
                        MaxTime = TimeSpan.FromHours(8),
                        MostLikelyTime = TimeSpan.FromHours(4),
                        Complexity = 3
                    }
                });
            }

            if (analysisResult.Scripts.Metrics?.AverageCyclomaticComplexity > 10)
            {
                recommendations.Add(new ProjectRecommendation(RecommendationType.CodeQuality,
                    "Reduce Code Complexity")
                {
                    Priority = RecommendationPriority.High,
                    Description = $"Simplify overly complex methods (current average: {analysisResult.Scripts.Metrics.AverageCyclomaticComplexity:F1})",
                    Rationale = "High complexity makes code harder to understand, test, and maintain",
                    ActionSteps = new List<ActionStep>
                    {
                        new ActionStep("Identify methods with complexity > 10", TimeSpan.FromMinutes(30)),
                        new ActionStep("Break complex methods into smaller functions", TimeSpan.FromHours(4)),
                        new ActionStep("Extract complex conditionals into meaningful method names", TimeSpan.FromHours(2)),
                        new ActionStep("Add unit tests for refactored methods", TimeSpan.FromHours(2))
                    },
                    Benefits = { "Easier code understanding", "Better testability", "Reduced bug risk", "Improved maintainability" }
                });
            }

            if (analysisResult.Scripts.Metrics?.CommentRatio < 0.1f)
            {
                recommendations.Add(new ProjectRecommendation(RecommendationType.Documentation,
                    "Improve Code Documentation")
                {
                    Priority = RecommendationPriority.Low,
                    Description = $"Increase code documentation from {analysisResult.Scripts.Metrics.CommentRatio * 100:F0}% to at least 15%",
                    Rationale = "Good documentation helps with code understanding and team collaboration",
                    ActionSteps = new List<ActionStep>
                    {
                        new ActionStep("Add XML documentation to public methods and classes", TimeSpan.FromHours(3)),
                        new ActionStep("Document complex algorithms and business logic", TimeSpan.FromHours(2)),
                        new ActionStep("Create architectural decision records", TimeSpan.FromHours(1)),
                        new ActionStep("Set up documentation standards for the team", TimeSpan.FromMinutes(30))
                    },
                    Benefits = { "Better code understanding", "Easier onboarding", "Improved maintenance", "Better IDE support" }
                });
            }

            return recommendations;
        }

        private List<ProjectRecommendation> GenerateDependencyRecommendations(ProjectAnalysisResult analysisResult)
        {
            List<ProjectRecommendation> recommendations = new List<ProjectRecommendation>();

            if (analysisResult.Scripts?.Dependencies == null) return recommendations;

            List<string> circularDependencies = analysisResult.Scripts.Dependencies.GetCircularDependencies();
            if (circularDependencies.Count > 0)
            {
                recommendations.Add(new ProjectRecommendation(RecommendationType.Dependencies,
                    "Resolve Circular Dependencies")
                {
                    Priority = RecommendationPriority.Critical,
                    Description = $"Break {circularDependencies.Count} circular dependency chains",
                    Rationale = "Circular dependencies can cause compilation issues and make code harder to understand",
                    ActionSteps = new List<ActionStep>
                    {
                        new ActionStep("Map out all circular dependency chains", TimeSpan.FromHours(1)),
                        new ActionStep("Introduce interfaces to break direct dependencies", TimeSpan.FromHours(3)),
                        new ActionStep("Extract shared functionality into separate modules", TimeSpan.FromHours(2)),
                        new ActionStep("Verify no new circular dependencies were introduced", TimeSpan.FromMinutes(30))
                    },
                    Benefits = { "Cleaner architecture", "Easier compilation", "Better testability", "Improved code organization" },
                    Risks = { "Temporary complexity during refactoring", "Potential for new dependencies" },
                    Effort = new EstimatedEffort
                    {
                        MinTime = TimeSpan.FromHours(4),
                        MaxTime = TimeSpan.FromHours(12),
                        MostLikelyTime = TimeSpan.FromHours(7),
                        Complexity = 4,
                        RequiredSkills = new[] { "Dependency analysis", "Refactoring", "Interface design" }
                    }
                });
            }

            return recommendations;
        }

        private List<ProjectRecommendation> GenerateSecurityRecommendations(ProjectAnalysisResult analysisResult)
        {
            List<ProjectRecommendation> recommendations = new List<ProjectRecommendation>();

            if (analysisResult.Scripts == null) return recommendations;

            List<CodeIssue> securityIssues = analysisResult.Scripts.Issues
                .Where(i => i.Type == CodeIssueType.SecurityIssue)
                .ToList();

            if (securityIssues.Count > 0)
            {
                recommendations.Add(new ProjectRecommendation(RecommendationType.Security,
                    "Address Security Vulnerabilities")
                {
                    Priority = RecommendationPriority.Critical,
                    Description = $"Fix {securityIssues.Count} identified security issues",
                    Rationale = "Security vulnerabilities can expose user data or allow malicious attacks",
                    ActionSteps = securityIssues.Select(issue => 
                        new ActionStep($"Secure: {issue.Description}", TimeSpan.FromHours(1))
                    ).Take(5).ToList(),
                    Benefits = { "Protected user data", "Reduced attack surface", "Compliance with security standards", "User trust" },
                    Effort = new EstimatedEffort
                    {
                        MinTime = TimeSpan.FromHours(2),
                        MaxTime = TimeSpan.FromHours(10),
                        MostLikelyTime = TimeSpan.FromHours(5),
                        Complexity = 4,
                        RequiredSkills = new[] { "Security knowledge", "Encryption", "Secure coding practices" }
                    }
                });
            }

            recommendations.Add(new ProjectRecommendation(RecommendationType.Security,
                "Implement Security Best Practices")
            {
                Priority = RecommendationPriority.Low,
                Description = "Proactively implement security best practices for your Unity project",
                Rationale = "Prevention is better than cure when it comes to security vulnerabilities",
                ActionSteps = new List<ActionStep>
                {
                    new ActionStep("Validate all user inputs and external data", TimeSpan.FromHours(2)),
                    new ActionStep("Implement proper error handling without exposing sensitive information", TimeSpan.FromHours(1)),
                    new ActionStep("Use secure communication protocols for networking", TimeSpan.FromHours(1)),
                    new ActionStep("Regular security code reviews", TimeSpan.FromHours(1))
                },
                Benefits = { "Proactive security", "User data protection", "Regulatory compliance", "Reduced security incidents" }
            });

            return recommendations;
        }

        private List<ProjectRecommendation> GenerateDocumentationRecommendations(ProjectAnalysisResult analysisResult)
        {
            List<ProjectRecommendation> recommendations = new List<ProjectRecommendation>();

            recommendations.Add(new ProjectRecommendation(RecommendationType.Documentation,
                "Create Comprehensive Project Documentation")
            {
                Priority = RecommendationPriority.Medium,
                Description = "Establish comprehensive documentation for better project maintenance and team collaboration",
                Rationale = "Good documentation reduces onboarding time and improves code maintainability",
                ActionSteps = new List<ActionStep>
                {
                    new ActionStep("Create README with project overview and setup instructions", TimeSpan.FromHours(1)),
                    new ActionStep("Document architecture decisions and patterns used", TimeSpan.FromHours(2)),
                    new ActionStep("Create coding standards and style guide", TimeSpan.FromHours(1)),
                    new ActionStep("Document build and deployment processes", TimeSpan.FromMinutes(30)),
                    new ActionStep("Set up automated documentation generation", TimeSpan.FromHours(1))
                },
                Benefits = { "Faster onboarding", "Better team collaboration", "Reduced knowledge silos", "Easier maintenance" },
                Effort = new EstimatedEffort
                {
                    MinTime = TimeSpan.FromHours(3),
                    MaxTime = TimeSpan.FromHours(8),
                    MostLikelyTime = TimeSpan.FromHours(5),
                    Complexity = 2,
                    RequiredSkills = new[] { "Technical writing", "Documentation tools" }
                }
            });

            return recommendations;
        }

        private List<ProjectRecommendation> GenerateTestingRecommendations(ProjectAnalysisResult analysisResult)
        {
            List<ProjectRecommendation> recommendations = new List<ProjectRecommendation>();

            if (analysisResult.Structure == null) return recommendations;

            bool hasTestInfrastructure = analysisResult.Structure.Folders
                .Any(f => f.Name.Contains("Test", StringComparison.OrdinalIgnoreCase)) ||
                analysisResult.Structure.Files
                .Any(f => f.Name.Contains("Test", StringComparison.OrdinalIgnoreCase));

            if (!hasTestInfrastructure)
            {
                recommendations.Add(new ProjectRecommendation(RecommendationType.Testing,
                    "Establish Testing Infrastructure")
                {
                    Priority = RecommendationPriority.Medium,
                    Description = "Set up unit testing framework and create initial test suite",
                    Rationale = "Automated testing catches bugs early and ensures code quality during development",
                    ActionSteps = new List<ActionStep>
                    {
                        new ActionStep("Install Unity Test Framework package", TimeSpan.FromMinutes(10)),
                        new ActionStep("Create Tests folder structure", TimeSpan.FromMinutes(5)),
                        new ActionStep("Write tests for critical business logic", TimeSpan.FromHours(4)),
                        new ActionStep("Set up continuous integration to run tests", TimeSpan.FromHours(2)),
                        new ActionStep("Establish testing guidelines for the team", TimeSpan.FromMinutes(30))
                    },
                    Benefits = { "Early bug detection", "Regression prevention", "Code quality assurance", "Confident refactoring" },
                    Effort = new EstimatedEffort
                    {
                        MinTime = TimeSpan.FromHours(4),
                        MaxTime = TimeSpan.FromHours(12),
                        MostLikelyTime = TimeSpan.FromHours(7),
                        Complexity = 3,
                        RequiredSkills = new[] { "Unit testing", "Unity Test Framework", "CI/CD" }
                    }
                });
            }
            else
            {
                int testFiles = analysisResult.Structure.Files
                    .Count(f => f.Name.Contains("Test", StringComparison.OrdinalIgnoreCase) && f.Extension == ".cs");
                
                int scriptFiles = analysisResult.Structure.Files
                    .Count(f => f.Extension == ".cs" && !f.Name.Contains("Test", StringComparison.OrdinalIgnoreCase));

                if (scriptFiles > 0)
                {
                    float testCoverage = (float)testFiles / scriptFiles;
                    
                    if (testCoverage < 0.3f)
                    {
                        recommendations.Add(new ProjectRecommendation(RecommendationType.Testing,
                            "Improve Test Coverage")
                        {
                            Priority = RecommendationPriority.Medium,
                            Description = $"Increase test coverage from {testCoverage * 100:F0}% to at least 30%",
                            Rationale = "Higher test coverage provides better confidence in code changes and refactoring",
                            ActionSteps = new List<ActionStep>
                            {
                                new ActionStep("Identify critical components lacking tests", TimeSpan.FromHours(1)),
                                new ActionStep("Write unit tests for core business logic", TimeSpan.FromHours(6)),
                                new ActionStep("Add integration tests for key workflows", TimeSpan.FromHours(3)),
                                new ActionStep("Set up code coverage reporting", TimeSpan.FromHours(1))
                            },
                            Benefits = { "Higher confidence in changes", "Better regression prevention", "Easier refactoring", "Quality assurance" }
                        });
                    }
                }
            }

            return recommendations;
        }
    }
}