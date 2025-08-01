using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityProjectArchitect.API;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services
{
    public class UserStoriesGenerator : BaseDocumentationGenerator
    {
        public UserStoriesGenerator(ProjectAnalysisResult analysisResult) 
            : base(analysisResult, DocumentationSectionType.UserStories)
        {
        }

        public override async Task<string> GenerateContentAsync()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetSectionHeader("User Stories"));
            sb.AppendLine(AddTimestamp());

            sb.AppendLine(await GenerateUserStoriesOverviewAsync());
            sb.AppendLine(await GenerateEpicsAsync());
            sb.AppendLine(await GenerateFeatureStoriesAsync());
            sb.AppendLine(await GenerateTechnicalStoriesAsync());
            sb.AppendLine(await GenerateAcceptanceCriteriaAsync());
            sb.AppendLine(await GenerateStoryMappingAsync());

            sb.AppendLine(AddGenerationMetadata());

            return await WrapInProgressIndicator(sb.ToString(), "User Stories Generation");
        }

        private async Task<string> GenerateUserStoriesOverviewAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("User Stories Overview", 2));

                sb.AppendLine("User stories define the functionality from the end-user perspective, focusing on the value delivered by each feature.");
                sb.AppendLine();

                if (analysisResult.Structure != null)
                {
                    ProjectType projectType = analysisResult.Structure.DetectedProjectType;
                    string userPersona = GetPrimaryUserPersona(projectType);
                    
                    sb.AppendLine($"**Primary User Persona:** {userPersona}");
                    sb.AppendLine($"**Project Context:** {GetProjectTypeDescription(projectType)}");
                    sb.AppendLine();

                    int estimatedStoryCount = EstimateStoryCount();
                    sb.AppendLine($"**Estimated Story Scope:** {estimatedStoryCount} user stories based on project analysis");
                    sb.AppendLine();
                }

                sb.AppendLine("**Story Format:**");
                sb.AppendLine("- **As a** [user persona]");
                sb.AppendLine("- **I want** [functionality/goal]");
                sb.AppendLine("- **So that** [business value/benefit]");
                sb.AppendLine();

                return sb.ToString();
            });
        }

        private async Task<string> GenerateEpicsAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Epics", 2));

                List<Epic> epics = GenerateProjectEpics();
                
                if (epics.Any())
                {
                    sb.AppendLine("High-level feature groupings that encompass multiple user stories:");
                    sb.AppendLine();

                    foreach (Epic epic in epics)
                    {
                        sb.AppendLine($"### Epic: {epic.Title}");
                        sb.AppendLine($"**Description:** {epic.Description}");
                        sb.AppendLine($"**Value:** {epic.BusinessValue}");
                        sb.AppendLine($"**Estimated Stories:** {epic.EstimatedStories}");
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("Epics will be defined based on the specific feature requirements and user journey mapping.");
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateFeatureStoriesAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Feature Stories", 2));

                List<UserStory> stories = GenerateFeatureBasedStories();
                
                if (stories.Any())
                {
                    sb.AppendLine("User stories organized by core features and functionality:");
                    sb.AppendLine();

                    IOrderedEnumerable<IGrouping<string, UserStory>> storiesByCategory = stories.GroupBy(s => s.Category).OrderBy(g => g.Key);

                    foreach (var categoryGroup in storiesByCategory)
                    {
                        sb.AppendLine($"### {categoryGroup.Key} Features");
                        sb.AppendLine();

                        foreach (UserStory story in categoryGroup.Take(8))
                        {
                            sb.AppendLine($"**Story {story.Id}:** {story.Title}");
                            sb.AppendLine($"- **As a** {story.UserPersona}");
                            sb.AppendLine($"- **I want** {story.Functionality}");
                            sb.AppendLine($"- **So that** {story.BusinessValue}");
                            sb.AppendLine($"- **Priority:** {story.Priority}");
                            sb.AppendLine($"- **Estimated Effort:** {story.EstimatedPoints} points");
                            sb.AppendLine();
                        }

                        if (categoryGroup.Count() > 8)
                        {
                            sb.AppendLine($"*... and {categoryGroup.Count() - 8} more {categoryGroup.Key.ToLower()} stories*");
                            sb.AppendLine();
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Feature stories will be generated based on:");
                    sb.AppendLine("- Core gameplay mechanics");
                    sb.AppendLine("- User interface interactions");
                    sb.AppendLine("- Data management features");
                    sb.AppendLine("- System integration points");
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateTechnicalStoriesAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Technical Stories", 2));

                List<TechnicalStory> technicalStories = GenerateTechnicalBasedStories();
                
                if (technicalStories.Any())
                {
                    sb.AppendLine("Technical user stories focusing on infrastructure, performance, and development enablement:");
                    sb.AppendLine();

                    foreach (TechnicalStory story in technicalStories.Take(10))
                    {
                        sb.AppendLine($"**Technical Story:** {story.Title}");
                        sb.AppendLine($"- **As a** {story.UserPersona}");
                        sb.AppendLine($"- **I want** {story.Functionality}");
                        sb.AppendLine($"- **So that** {story.BusinessValue}");
                        sb.AppendLine($"- **Technical Focus:** {story.TechnicalArea}");
                        sb.AppendLine($"- **Complexity:** {story.TechnicalComplexity}");
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("Technical stories will address:");
                    sb.AppendLine("- Performance optimization requirements");
                    sb.AppendLine("- Platform compatibility needs");
                    sb.AppendLine("- Security and data protection");
                    sb.AppendLine("- Development tooling and workflows");
                    sb.AppendLine("- Testing and quality assurance");
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateAcceptanceCriteriaAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Acceptance Criteria Examples", 2));

                List<UserStory> sampleStories = GenerateFeatureBasedStories().Take(3).ToList();
                
                if (sampleStories.Any())
                {
                    sb.AppendLine("Detailed acceptance criteria for key user stories:");
                    sb.AppendLine();

                    foreach (UserStory story in sampleStories)
                    {
                        sb.AppendLine($"### {story.Title}");
                        sb.AppendLine("**Acceptance Criteria:**");
                        
                        List<AcceptanceCriterion> criteria = GenerateAcceptanceCriteria(story);
                        foreach (AcceptanceCriterion criterion in criteria)
                        {
                            sb.AppendLine($"- **Given** {criterion.Given}");
                            sb.AppendLine($"  **When** {criterion.When}");
                            sb.AppendLine($"  **Then** {criterion.Then}");
                            sb.AppendLine();
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Acceptance criteria will follow the Given-When-Then format:");
                    sb.AppendLine("- **Given** [initial context or state]");
                    sb.AppendLine("- **When** [action or event occurs]");
                    sb.AppendLine("- **Then** [expected outcome or result]");
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }

        private async Task<string> GenerateStoryMappingAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetSectionHeader("Story Mapping & Prioritization", 2));

                if (analysisResult.Scripts?.Classes != null)
                {
                    sb.AppendLine("**Story Prioritization Matrix:**");
                    sb.AppendLine();

                    sb.AppendLine("```mermaid");
                    sb.AppendLine("quadrantChart");
                    sb.AppendLine("    title Story Priority Matrix");
                    sb.AppendLine("    x-axis Low Effort --> High Effort");
                    sb.AppendLine("    y-axis Low Value --> High Value");
                    sb.AppendLine("    quadrant-1 Quick Wins");
                    sb.AppendLine("    quadrant-2 Major Projects");
                    sb.AppendLine("    quadrant-3 Fill-ins");
                    sb.AppendLine("    quadrant-4 Questionable");

                    IEnumerable<UserStory> stories = GenerateFeatureBasedStories().Take(8);
                    foreach (UserStory story in stories)
                    {
                        float effort = story.EstimatedPoints / 10.0f;
                        float value = GetStoryValue(story) / 10.0f;
                        sb.AppendLine($"    {story.Title}: [{effort:F1}, {value:F1}]");
                    }

                    sb.AppendLine("```");
                    sb.AppendLine();
                }

                sb.AppendLine("**Release Planning:**");
                sb.AppendLine("- **MVP (Minimum Viable Product):** Core functionality stories");
                sb.AppendLine("- **Release 1:** Essential features for user engagement");
                sb.AppendLine("- **Release 2:** Enhanced features and polish");
                sb.AppendLine("- **Future Releases:** Advanced features and optimizations");
                sb.AppendLine();

                return sb.ToString();
            });
        }

        private string GetPrimaryUserPersona(ProjectType projectType)
        {
            return projectType switch
            {
                ProjectType.Game2D or ProjectType.Game3D => "Game Player",
                ProjectType.Mobile => "Mobile User",
                ProjectType.VR => "VR Experience User",
                ProjectType.AR => "AR Application User",
                ProjectType.Tool => "Unity Developer",
                ProjectType.Template => "Project Developer",
                _ => "End User"
            };
        }

        private int EstimateStoryCount()
        {
            int baseStories = 10;
            
            if (analysisResult.Scripts?.Classes != null)
            {
                int monoBehaviours = analysisResult.Scripts.Classes.Count(c => c.IsMonoBehaviour);
                int scriptableObjects = analysisResult.Scripts.Classes.Count(c => c.IsScriptableObject);
                baseStories += monoBehaviours * 2 + scriptableObjects;
            }

            if (analysisResult.Structure?.Scenes != null)
            {
                baseStories += analysisResult.Structure.Scenes.Count * 3;
            }

            return Math.Min(baseStories, 50); // Cap at reasonable number
        }

        private List<Epic> GenerateProjectEpics()
        {
            List<Epic> epics = new List<Epic>();

            if (analysisResult.Structure?.DetectedProjectType == ProjectType.Game2D || 
                analysisResult.Structure?.DetectedProjectType == ProjectType.Game3D)
            {
                epics.Add(new Epic
                {
                    Title = "Core Gameplay",
                    Description = "Essential game mechanics and player interactions",
                    BusinessValue = "Provides the fundamental gaming experience",
                    EstimatedStories = 8
                });

                epics.Add(new Epic
                {
                    Title = "User Interface",
                    Description = "Game menus, HUD, and user interaction systems",
                    BusinessValue = "Ensures intuitive and accessible user experience",
                    EstimatedStories = 6
                });
            }

            if (analysisResult.Scripts?.Classes?.Any(c => c.IsScriptableObject) == true)
            {
                epics.Add(new Epic
                {
                    Title = "Data Management",
                    Description = "Configuration, settings, and persistent data handling",
                    BusinessValue = "Enables customization and data persistence",
                    EstimatedStories = 4
                });
            }

            return epics;
        }

        private List<UserStory> GenerateFeatureBasedStories()
        {
            List<UserStory> stories = new List<UserStory>();
            int storyId = 1;

            if (analysisResult.Scripts?.Classes?.Any(c => c.IsMonoBehaviour) == true)
            {
                string userPersona = GetPrimaryUserPersona(analysisResult.Structure?.DetectedProjectType ?? ProjectType.Unknown);
                
                stories.Add(new UserStory
                {
                    Id = $"US-{storyId++:D3}",
                    Title = "Interactive Object Interaction",
                    Category = "Gameplay",
                    UserPersona = userPersona,
                    Functionality = "interact with objects in the game world",
                    BusinessValue = "I can engage with the environment and progress through the experience",
                    Priority = "High",
                    EstimatedPoints = 5
                });

                stories.Add(new UserStory
                {
                    Id = $"US-{storyId++:D3}",
                    Title = "Game State Management",
                    Category = "Core System",
                    UserPersona = userPersona,
                    Functionality = "experience consistent game state across sessions",
                    BusinessValue = "my progress is maintained and I can continue where I left off",
                    Priority = "High",
                    EstimatedPoints = 8
                });
            }

            if (analysisResult.Scripts?.Classes?.Any(c => c.IsScriptableObject) == true)
            {
                stories.Add(new UserStory
                {
                    Id = $"US-{storyId++:D3}",
                    Title = "Configuration Management",
                    Category = "Data",
                    UserPersona = "System Administrator",
                    Functionality = "customize game settings through configuration files",
                    BusinessValue = "the experience can be tailored to different requirements",
                    Priority = "Medium",
                    EstimatedPoints = 3
                });
            }

            if (analysisResult.Structure?.Scenes?.Count > 1)
            {
                stories.Add(new UserStory
                {
                    Id = $"US-{storyId++:D3}",
                    Title = "Scene Navigation",
                    Category = "Navigation",
                    UserPersona = GetPrimaryUserPersona(analysisResult.Structure?.DetectedProjectType ?? ProjectType.Unknown),
                    Functionality = "navigate between different areas or levels",
                    BusinessValue = "I can explore the full content and experience variety",
                    Priority = "High",
                    EstimatedPoints = 5
                });
            }

            return stories;
        }

        private List<TechnicalStory> GenerateTechnicalBasedStories()
        {
            List<TechnicalStory> stories = new List<TechnicalStory>();

            if (analysisResult.Performance?.Issues?.Any() == true)
            {
                stories.Add(new TechnicalStory
                {
                    Title = "Performance Optimization",
                    UserPersona = "End User",
                    Functionality = "experience smooth performance across different devices",
                    BusinessValue = "the application runs efficiently without lag or stuttering",
                    TechnicalArea = "Performance",
                    TechnicalComplexity = "High"
                });
            }

            if (analysisResult.Structure?.AssemblyDefinitions?.Any() == true)
            {
                stories.Add(new TechnicalStory
                {
                    Title = "Modular Architecture",
                    UserPersona = "Developer",
                    Functionality = "work with well-organized, modular code",
                    BusinessValue = "development is more efficient and maintainable",
                    TechnicalArea = "Architecture",
                    TechnicalComplexity = "Medium"
                });
            }

            return stories;
        }

        private List<AcceptanceCriterion> GenerateAcceptanceCriteria(UserStory story)
        {
            List<AcceptanceCriterion> criteria = new List<AcceptanceCriterion>();

            if (story.Category == "Gameplay")
            {
                criteria.Add(new AcceptanceCriterion
                {
                    Given = "the user is in the game world",
                    When = "they interact with an interactive object",
                    Then = "the object responds with appropriate feedback and state change"
                });

                criteria.Add(new AcceptanceCriterion
                {
                    Given = "an object has been interacted with",
                    When = "the user tries to interact again",
                    Then = "the system handles the repeat interaction appropriately"
                });
            }

            return criteria;
        }

        private float GetStoryValue(UserStory story)
        {
            return story.Priority switch
            {
                "High" => 8.0f,
                "Medium" => 5.0f,
                "Low" => 2.0f,
                _ => 1.0f
            };
        }

        private class Epic
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string BusinessValue { get; set; }
            public int EstimatedStories { get; set; }
        }

        private class UserStory
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Category { get; set; }
            public string UserPersona { get; set; }
            public string Functionality { get; set; }
            public string BusinessValue { get; set; }
            public string Priority { get; set; }
            public int EstimatedPoints { get; set; }
        }

        private class TechnicalStory : UserStory
        {
            public string TechnicalArea { get; set; }
            public string TechnicalComplexity { get; set; }
        }

        private class AcceptanceCriterion
        {
            public string Given { get; set; }
            public string When { get; set; }
            public string Then { get; set; }
        }
    }
}