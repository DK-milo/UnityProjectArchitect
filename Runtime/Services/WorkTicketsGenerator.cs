using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityProjectArchitect.API;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services
{
    public class WorkTicketsGenerator : BaseDocumentationGenerator
    {
        public WorkTicketsGenerator(ProjectAnalysisResult analysisResult) 
            : base(analysisResult, DocumentationSectionType.WorkTickets)
        {
        }

        public override async Task<string> GenerateContentAsync()
        {
            var sb = new StringBuilder();

            sb.AppendLine(GetSectionHeader("Work Tickets"));
            sb.AppendLine(AddTimestamp());

            sb.AppendLine(await GenerateWorkTicketsOverviewAsync());
            sb.AppendLine(await GenerateImplementationTicketsAsync());
            sb.AppendLine(await GenerateBugFixTicketsAsync());
            sb.AppendLine(await GenerateRefactoringTicketsAsync());
            sb.AppendLine(await GenerateTestingTicketsAsync());
            sb.AppendLine(await GenerateDocumentationTicketsAsync());
            sb.AppendLine(await GenerateTicketPrioritizationAsync());

            sb.AppendLine(AddGenerationMetadata());

            return await WrapInProgressIndicator(sb.ToString(), "Work Tickets Generation");
        }

        private async Task<string> GenerateWorkTicketsOverviewAsync()
        {
            return await Task.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Work Tickets Overview", 2));

                sb.AppendLine("Development work tickets derived from project analysis, user stories, and technical requirements.");
                sb.AppendLine();

                var totalTickets = EstimateTotalTickets();
                sb.AppendLine($"**Estimated Ticket Count:** {totalTickets} work items across all categories");
                sb.AppendLine();

                sb.AppendLine("**Ticket Categories:**");
                sb.AppendLine("- **Implementation:** New feature development");
                sb.AppendLine("- **Bug Fixes:** Issue resolution and corrections");
                sb.AppendLine("- **Refactoring:** Code quality and architecture improvements");
                sb.AppendLine("- **Testing:** Quality assurance and validation");
                sb.AppendLine("- **Documentation:** Knowledge sharing and maintenance");
                sb.AppendLine();

                sb.AppendLine("**Ticket Workflow:**");
                sb.AppendLine("1. **Backlog** → Ready for planning");
                sb.AppendLine("2. **Ready** → Available for development");
                sb.AppendLine("3. **In Progress** → Currently being worked on");
                sb.AppendLine("4. **Review** → Code review and testing");
                sb.AppendLine("5. **Done** → Completed and deployed");
                sb.AppendLine();

                return sb.ToString();
            });
        }

        private async Task<string> GenerateImplementationTicketsAsync()
        {
            return await Task.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Implementation Tickets", 2));

                var implementationTickets = GenerateImplementationTickets();
                
                if (implementationTickets.Any())
                {
                    sb.AppendLine("Feature development tickets based on user stories and technical requirements:");
                    sb.AppendLine();

                    var ticketsByPriority = implementationTickets
                        .GroupBy(t => t.Priority)
                        .OrderByDescending(g => GetPriorityOrder(g.Key));

                    foreach (var priorityGroup in ticketsByPriority)
                    {
                        sb.AppendLine($"### {priorityGroup.Key} Priority");
                        sb.AppendLine();

                        foreach (var ticket in priorityGroup.Take(6))
                        {
                            sb.AppendLine($"**{ticket.Id}: {ticket.Title}**");
                            sb.AppendLine($"- **Type:** {ticket.Type}");
                            sb.AppendLine($"- **Component:** {ticket.Component}");
                            sb.AppendLine($"- **Estimate:** {ticket.EstimatedHours} hours");
                            sb.AppendLine($"- **Description:** {ticket.Description}");
                            
                            if (ticket.AcceptanceCriteria.Any())
                            {
                                sb.AppendLine("- **Acceptance Criteria:**");
                                foreach (var criterion in ticket.AcceptanceCriteria.Take(3))
                                {
                                    sb.AppendLine($"  - {criterion}");
                                }
                            }
                            
                            if (ticket.Dependencies.Any())
                            {
                                sb.AppendLine($"- **Dependencies:** {string.Join(", ", ticket.Dependencies)}");
                            }
                            sb.AppendLine();
                        }

                        if (priorityGroup.Count() > 6)
                        {
                            sb.AppendLine($"*... and {priorityGroup.Count() - 6} more {priorityGroup.Key.ToLower()} priority tickets*");
                            sb.AppendLine();
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Implementation tickets will be generated based on:");
                    sb.AppendLine("- User story breakdown into technical tasks");
                    sb.AppendLine("- Feature requirements and specifications");
                    sb.AppendLine("- Technical architecture decisions");
                    sb.AppendLine("- Integration and API development needs");
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateBugFixTicketsAsync()
        {
            return await Task.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Bug Fix Tickets", 2));

                var bugTickets = GenerateBugFixTickets();
                
                if (bugTickets.Any())
                {
                    sb.AppendLine("Issue resolution tickets based on project analysis and potential problem areas:");
                    sb.AppendLine();

                    var ticketsBySeverity = bugTickets
                        .GroupBy(t => t.Severity)
                        .OrderByDescending(g => GetSeverityOrder(g.Key));

                    foreach (var severityGroup in ticketsBySeverity)
                    {
                        sb.AppendLine($"### {severityGroup.Key} Severity");
                        sb.AppendLine();

                        foreach (var ticket in severityGroup.Take(4))
                        {
                            sb.AppendLine($"**{ticket.Id}: {ticket.Title}**");
                            sb.AppendLine($"- **Severity:** {ticket.Severity}");
                            sb.AppendLine($"- **Component:** {ticket.Component}");
                            sb.AppendLine($"- **Issue:** {ticket.Description}");
                            sb.AppendLine($"- **Impact:** {ticket.Impact}");
                            sb.AppendLine($"- **Estimated Fix Time:** {ticket.EstimatedHours} hours");
                            sb.AppendLine();
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Bug fix tickets will address:");
                    sb.AppendLine("- Performance bottlenecks identified in analysis");
                    sb.AppendLine("- Memory leaks and resource management issues");
                    sb.AppendLine("- UI/UX inconsistencies and accessibility problems");
                    sb.AppendLine("- Logic errors and edge case handling");
                    sb.AppendLine("- Platform-specific compatibility issues");
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateRefactoringTicketsAsync()
        {
            return await Task.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Refactoring Tickets", 2));

                var refactoringTickets = GenerateRefactoringTickets();
                
                if (refactoringTickets.Any())
                {
                    sb.AppendLine("Code quality improvement tickets based on architecture analysis:");
                    sb.AppendLine();

                    foreach (var ticket in refactoringTickets.Take(8))
                    {
                        sb.AppendLine($"**{ticket.Id}: {ticket.Title}**");
                        sb.AppendLine($"- **Focus Area:** {ticket.RefactoringType}");
                        sb.AppendLine($"- **Component:** {ticket.Component}");
                        sb.AppendLine($"- **Current Issue:** {ticket.CurrentProblem}");
                        sb.AppendLine($"- **Improvement Goal:** {ticket.ImprovementGoal}");
                        sb.AppendLine($"- **Complexity:** {ticket.Complexity}");
                        sb.AppendLine($"- **Estimated Effort:** {ticket.EstimatedHours} hours");
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("Refactoring opportunities will focus on:");
                    sb.AppendLine("- Reducing code complexity and cyclomatic complexity");
                    sb.AppendLine("- Improving class cohesion and reducing coupling");
                    sb.AppendLine("- Eliminating code duplicates and technical debt");
                    sb.AppendLine("- Enhancing naming conventions and code readability");
                    sb.AppendLine("- Optimizing performance-critical code paths");
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateTestingTicketsAsync()
        {
            return await Task.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Testing Tickets", 2));

                var testingTickets = GenerateTestingTickets();
                
                if (testingTickets.Any())
                {
                    sb.AppendLine("Quality assurance tickets for comprehensive testing coverage:");
                    sb.AppendLine();

                    var ticketsByType = testingTickets.GroupBy(t => t.TestType);

                    foreach (var typeGroup in ticketsByType)
                    {
                        sb.AppendLine($"### {typeGroup.Key} Testing");
                        sb.AppendLine();

                        foreach (var ticket in typeGroup.Take(4))
                        {
                            sb.AppendLine($"**{ticket.Id}: {ticket.Title}**");
                            sb.AppendLine($"- **Test Type:** {ticket.TestType}");
                            sb.AppendLine($"- **Component:** {ticket.Component}");
                            sb.AppendLine($"- **Scope:** {ticket.TestScope}");
                            sb.AppendLine($"- **Coverage Goal:** {ticket.CoverageTarget}%");
                            sb.AppendLine($"- **Estimated Time:** {ticket.EstimatedHours} hours");
                            sb.AppendLine();
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Testing strategy will include:");
                    sb.AppendLine("- **Unit Tests:** Individual component validation");
                    sb.AppendLine("- **Integration Tests:** Component interaction verification");
                    sb.AppendLine("- **Performance Tests:** Load and stress testing");
                    sb.AppendLine("- **UI Tests:** User interface and interaction testing");
                    sb.AppendLine("- **Platform Tests:** Cross-platform compatibility validation");
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateDocumentationTicketsAsync()
        {
            return await Task.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Documentation Tickets", 2));

                var docTickets = GenerateDocumentationTickets();
                
                if (docTickets.Any())
                {
                    sb.AppendLine("Documentation and knowledge sharing tickets:");
                    sb.AppendLine();

                    foreach (var ticket in docTickets.Take(6))
                    {
                        sb.AppendLine($"**{ticket.Id}: {ticket.Title}**");
                        sb.AppendLine($"- **Doc Type:** {ticket.DocumentationType}");
                        sb.AppendLine($"- **Target Audience:** {ticket.TargetAudience}");
                        sb.AppendLine($"- **Content:** {ticket.ContentDescription}");
                        sb.AppendLine($"- **Format:** {ticket.Format}");
                        sb.AppendLine($"- **Estimated Effort:** {ticket.EstimatedHours} hours");
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("Documentation needs will cover:");
                    sb.AppendLine("- API documentation and code comments");
                    sb.AppendLine("- User guides and tutorials");
                    sb.AppendLine("- Technical architecture documentation"); 
                    sb.AppendLine("- Deployment and configuration guides");
                    sb.AppendLine("- Troubleshooting and FAQ sections");
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateTicketPrioritizationAsync()
        {
            return await Task.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Ticket Prioritization & Planning", 2));

                var allTickets = new List<WorkTicket>();
                allTickets.AddRange(GenerateImplementationTickets());
                allTickets.AddRange(GenerateBugFixTickets().Cast<WorkTicket>());
                allTickets.AddRange(GenerateRefactoringTickets().Cast<WorkTicket>());

                if (allTickets.Any())
                {
                    sb.AppendLine("**Sprint Planning Matrix:**");
                    sb.AppendLine();

                    sb.AppendLine("```mermaid");
                    sb.AppendLine("gantt");
                    sb.AppendLine("    title Development Timeline");
                    sb.AppendLine("    dateFormat  YYYY-MM-DD");
                    sb.AppendLine("    section Sprint 1");
                    
                    var highPriorityTickets = allTickets.Where(t => t.Priority == "High").Take(4);
                    foreach (var ticket in highPriorityTickets)
                    {
                        int duration = Math.Max(ticket.EstimatedHours / 8, 1); // Convert hours to days
                        sb.AppendLine($"    {ticket.Title.Replace(" ", "_")} :active, {duration}d");
                    }

                    sb.AppendLine("    section Sprint 2");
                    var mediumPriorityTickets = allTickets.Where(t => t.Priority == "Medium").Take(3);
                    foreach (var ticket in mediumPriorityTickets)
                    {
                        int duration = Math.Max(ticket.EstimatedHours / 8, 1);
                        sb.AppendLine($"    {ticket.Title.Replace(" ", "_")} :{duration}d");
                    }

                    sb.AppendLine("```");
                    sb.AppendLine();
                }

                sb.AppendLine("**Prioritization Guidelines:**");
                sb.AppendLine();
                sb.AppendLine("1. **Critical/High Priority:** Blocking issues, core functionality");
                sb.AppendLine("2. **Medium Priority:** Important features, performance improvements");
                sb.AppendLine("3. **Low Priority:** Nice-to-have features, minor enhancements");
                sb.AppendLine();

                sb.AppendLine("**Estimation Guidelines:**");
                sb.AppendLine("- **1-2 hours:** Simple bug fixes, minor updates");
                sb.AppendLine("- **4-8 hours:** Feature implementation, moderate refactoring");
                sb.AppendLine("- **16+ hours:** Complex features, major architectural changes");
                sb.AppendLine();

                return sb.ToString();
            });
        }

        private int EstimateTotalTickets()
        {
            var baseTickets = 15;
            
            if (analysisResult.Scripts?.Classes != null)
            {
                baseTickets += analysisResult.Scripts.Classes.Count / 3; // ~1 ticket per 3 classes
            }

            if (analysisResult.Issues?.Any() == true)
            {
                baseTickets += analysisResult.Issues.Count;
            }

            if (analysisResult.Recommendations?.Any() == true)
            {
                baseTickets += analysisResult.Recommendations.Count;
            }

            return Math.Min(baseTickets, 75); // Cap at reasonable number
        }

        private List<WorkTicket> GenerateImplementationTickets()
        {
            var tickets = new List<WorkTicket>();
            var ticketId = 1;

            if (analysisResult.Scripts?.Classes?.Any(c => c.IsMonoBehaviour) == true)
            {
                tickets.Add(new WorkTicket
                {
                    Id = $"IMPL-{ticketId++:D3}",
                    Title = "Implement Player Controller",
                    Type = "Feature",
                    Priority = "High",
                    Component = "Gameplay",
                    EstimatedHours = 16,
                    Description = "Create responsive player movement and interaction system",
                    AcceptanceCriteria = new[]
                    {
                        "Player responds to input with smooth movement",
                        "Collision detection works correctly",
                        "Animation system integrates properly"
                    },
                    Dependencies = new[] { "Input System Setup", "Animation Framework" }
                });
            }

            if (analysisResult.Scripts?.Classes?.Any(c => c.IsScriptableObject) == true)
            {
                tickets.Add(new WorkTicket
                {
                    Id = $"IMPL-{ticketId++:D3}",
                    Title = "Configuration Management System",
                    Type = "Infrastructure",
                    Priority = "Medium",
                    Component = "Data",
                    EstimatedHours = 12,
                    Description = "Implement ScriptableObject-based configuration management",
                    AcceptanceCriteria = new[]
                    {
                        "Settings can be modified through inspector",
                        "Configuration persists across sessions",
                        "Validation prevents invalid settings"
                    },
                    Dependencies = new[] { "Data Architecture" }
                });
            }

            if (analysisResult.Structure?.Scenes?.Count > 1)
            {
                tickets.Add(new WorkTicket
                {
                    Id = $"IMPL-{ticketId++:D3}",
                    Title = "Scene Transition System",
                    Type = "Feature",
                    Priority = "High",
                    Component = "Navigation",
                    EstimatedHours = 8,
                    Description = "Create smooth transitions between game scenes",
                    AcceptanceCriteria = new[]
                    {
                        "Loading screens display during transitions",
                        "Game state persists across scenes",
                        "Memory is properly cleaned up"
                    },
                    Dependencies = new[] { "Game State Manager" }
                });
            }

            return tickets;
        }

        private List<BugTicket> GenerateBugFixTickets()
        {
            var tickets = new List<BugTicket>();
            var ticketId = 1;

            if (analysisResult.Performance?.Issues?.Any() == true)
            {
                tickets.Add(new BugTicket
                {
                    Id = $"BUG-{ticketId++:D3}",
                    Title = "Performance Optimization",
                    Severity = "High",
                    Component = "Performance",
                    EstimatedHours = 6,
                    Description = "Address performance bottlenecks identified in analysis",
                    Impact = "Slow performance affects user experience",
                    Priority = "High"
                });
            }

            if (analysisResult.Scripts?.Dependencies?.GetCircularDependencies()?.Any() == true)
            {
                tickets.Add(new BugTicket
                {
                    Id = $"BUG-{ticketId++:D3}",
                    Title = "Resolve Circular Dependencies",
                    Severity = "Medium",
                    Component = "Architecture",
                    EstimatedHours = 4,
                    Description = "Break circular dependencies in code structure",
                    Impact = "Circular dependencies make code harder to maintain and test",
                    Priority = "Medium"
                });
            }

            return tickets;
        }

        private List<RefactoringTicket> GenerateRefactoringTickets()
        {
            var tickets = new List<RefactoringTicket>();
            var ticketId = 1;

            if (analysisResult.Scripts?.Metrics?.AverageCyclomaticComplexity > 10)
            {
                tickets.Add(new RefactoringTicket
                {
                    Id = $"REF-{ticketId++:D3}",
                    Title = "Reduce Cyclomatic Complexity",
                    RefactoringType = "Complexity Reduction",
                    Component = "Core Classes",
                    EstimatedHours = 8,
                    CurrentProblem = "High cyclomatic complexity makes code difficult to test and maintain",
                    ImprovementGoal = "Reduce average complexity to below 10 through method extraction",
                    Complexity = "Medium",
                    Priority = "Medium"
                });
            }

            if (analysisResult.Scripts?.Classes?.Any(c => c.Methods.Count > 20) == true)
            {
                tickets.Add(new RefactoringTicket
                {
                    Id = $"REF-{ticketId++:D3}",
                    Title = "Break Down Large Classes",
                    RefactoringType = "Class Decomposition",
                    Component = "Large Classes",
                    EstimatedHours = 12,
                    CurrentProblem = "Some classes have too many responsibilities",
                    ImprovementGoal = "Apply Single Responsibility Principle to improve maintainability",
                    Complexity = "High",
                    Priority = "Low"
                });
            }

            return tickets;
        }

        private List<TestingTicket> GenerateTestingTickets()
        {
            var tickets = new List<TestingTicket>();
            var ticketId = 1;

            if (analysisResult.Scripts?.Classes?.Any() == true)
            {
                tickets.Add(new TestingTicket
                {
                    Id = $"TEST-{ticketId++:D3}",
                    Title = "Unit Test Coverage",
                    TestType = "Unit Tests",
                    Component = "Core Logic",
                    EstimatedHours = 20,
                    TestScope = "All public methods in core classes",
                    CoverageTarget = 80
                });

                tickets.Add(new TestingTicket
                {
                    Id = $"TEST-{ticketId++:D3}",
                    Title = "Integration Testing",
                    TestType = "Integration Tests",
                    Component = "System Integration",
                    EstimatedHours = 16,
                    TestScope = "Component interactions and data flow",
                    CoverageTarget = 70
                });
            }

            return tickets;
        }

        private List<DocumentationTicket> GenerateDocumentationTickets()
        {
            var tickets = new List<DocumentationTicket>();
            var ticketId = 1;

            if (analysisResult.Scripts?.Interfaces?.Any() == true)
            {
                tickets.Add(new DocumentationTicket
                {
                    Id = $"DOC-{ticketId++:D3}",
                    Title = "API Documentation",
                    DocumentationType = "Technical Documentation",
                    TargetAudience = "Developers",
                    ContentDescription = "Document all public interfaces and their usage",
                    Format = "XML Comments + Generated HTML",
                    EstimatedHours = 6
                });
            }

            tickets.Add(new DocumentationTicket
            {
                Id = $"DOC-{ticketId++:D3}",
                Title = "Setup Guide",
                DocumentationType = "User Guide",
                TargetAudience = "End Users",
                ContentDescription = "Step-by-step installation and configuration guide",
                Format = "Markdown",
                EstimatedHours = 4
            });

            return tickets;
        }

        private int GetPriorityOrder(string priority)
        {
            return priority switch
            {
                "Critical" => 4,
                "High" => 3,
                "Medium" => 2,
                "Low" => 1,
                _ => 0
            };
        }

        private int GetSeverityOrder(string severity)
        {
            return severity switch
            {
                "Critical" => 4,
                "High" => 3,
                "Medium" => 2,
                "Low" => 1,
                _ => 0
            };
        }

        private class WorkTicket
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Type { get; set; }
            public string Priority { get; set; }
            public string Component { get; set; }
            public int EstimatedHours { get; set; }
            public string Description { get; set; }
            public string[] AcceptanceCriteria { get; set; } = Array.Empty<string>();
            public string[] Dependencies { get; set; } = Array.Empty<string>();
        }

        private class BugTicket : WorkTicket
        {
            public string Severity { get; set; }
            public string Impact { get; set; }
        }

        private class RefactoringTicket : WorkTicket
        {
            public string RefactoringType { get; set; }
            public string CurrentProblem { get; set; }
            public string ImprovementGoal { get; set; }
            public string Complexity { get; set; }
        }

        private class TestingTicket : WorkTicket
        {
            public string TestType { get; set; }
            public string TestScope { get; set; }
            public int CoverageTarget { get; set; }
        }

        private class DocumentationTicket : WorkTicket
        {
            public string DocumentationType { get; set; }
            public string TargetAudience { get; set; }
            public string ContentDescription { get; set; }
            public string Format { get; set; }
        }
    }
}