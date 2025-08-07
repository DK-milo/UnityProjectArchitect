using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services.ConceptAware
{
    /// <summary>
    /// Base class for concept-aware documentation generators that work with game descriptions
    /// instead of existing project analysis
    /// </summary>
    public abstract class BaseConceptAwareGenerator
    {
        protected readonly string gameDescription;
        protected readonly DocumentationSectionType sectionType;
        protected readonly GameConcept gameConcept;
        protected IAIAssistant _injectedAIAssistant;

        protected BaseConceptAwareGenerator(string gameDescription, DocumentationSectionType sectionType)
        {
            this.gameDescription = gameDescription ?? throw new ArgumentNullException(nameof(gameDescription));
            this.sectionType = sectionType;
            this.gameConcept = ParseGameConcept(gameDescription);
        }

        /// <summary>
        /// Inject a configured AI assistant to use instead of creating a new one
        /// </summary>
        public virtual void SetAIAssistant(IAIAssistant aiAssistant)
        {
            _injectedAIAssistant = aiAssistant;
        }

        public abstract Task<string> GenerateContentAsync();

        protected virtual GameConcept ParseGameConcept(string description)
        {
            GameConcept concept = new GameConcept();

            // Extract game title (look for **Title** format)
            Match titleMatch = Regex.Match(description, @"\*\*([^*]+)\*\*");
            if (titleMatch.Success)
            {
                concept.Title = titleMatch.Groups[1].Value.Trim();
            }
            else
            {
                concept.Title = "Untitled Game";
            }

            // Extract basic description (first paragraph)
            string[] paragraphs = description.Split(new string[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (paragraphs.Length > 1)
            {
                concept.CoreDescription = paragraphs[1].Trim();
            }
            else if (paragraphs.Length > 0)
            {
                concept.CoreDescription = paragraphs[0].Trim();
            }

            // Extract features (look for sections like **Core Gameplay:** or **Key Features:**)
            concept.CoreGameplayFeatures = ExtractSectionItems(description, "Core Gameplay");
            concept.KeyFeatures = ExtractSectionItems(description, "Key Features");

            // Extract metadata
            concept.TargetAudience = ExtractMetadata(description, "Target Audience");
            concept.Platform = ExtractMetadata(description, "Platform");
            concept.ArtStyle = ExtractMetadata(description, "Art Style");
            concept.DevelopmentTime = ExtractMetadata(description, "Development Time");

            // Analyze game type based on keywords
            concept.GameType = DetermineGameType(description);
            concept.GameGenre = DetermineGameGenre(description);
            concept.TechnicalFeatures = ExtractTechnicalFeatures(description);

            return concept;
        }

        protected virtual List<string> ExtractSectionItems(string description, string sectionName)
        {
            List<string> items = new List<string>();
            
            // Look for section header like **Core Gameplay:**
            string pattern = $@"\*\*{Regex.Escape(sectionName)}[:\s]*\*\*\s*\n((?:[-*]\s*[^\n]+\n?)+)";
            Match match = Regex.Match(description, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            
            if (match.Success)
            {
                string itemsText = match.Groups[1].Value;
                string[] lines = itemsText.Split('\n');
                
                foreach (string line in lines)
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("-") || trimmed.StartsWith("*"))
                    {
                        string item = trimmed.Substring(1).Trim();
                        if (!string.IsNullOrEmpty(item))
                        {
                            items.Add(item);
                        }
                    }
                }
            }
            
            return items;
        }

        protected virtual string ExtractMetadata(string description, string metadataKey)
        {
            string pattern = $@"\*\*{Regex.Escape(metadataKey)}[:\s]*\*\*\s*([^\n]+)";
            Match match = Regex.Match(description, pattern, RegexOptions.IgnoreCase);
            
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }

        protected virtual ProjectType DetermineGameType(string description)
        {
            string lowerDesc = description.ToLower();
            
            // VR detection
            if (lowerDesc.Contains("vr") || lowerDesc.Contains("virtual reality"))
                return ProjectType.VR;
            
            // AR detection - be more specific to avoid false positives like "adventure", "character", etc.
            if (lowerDesc.Contains("augmented reality") || 
                Regex.IsMatch(lowerDesc, @"\bar\b") || // whole word "ar" only
                lowerDesc.Contains("ar app") ||
                lowerDesc.Contains("ar game") ||
                lowerDesc.Contains("real-world integration") ||
                lowerDesc.Contains("camera overlay") ||
                lowerDesc.Contains("arkit") ||
                lowerDesc.Contains("arcore"))
                return ProjectType.AR;
            
            // Platform detection
            if (lowerDesc.Contains("mobile") || lowerDesc.Contains("android") || lowerDesc.Contains("ios"))
                return ProjectType.Mobile;
            
            // 2D/3D detection
            if (lowerDesc.Contains("2d") || lowerDesc.Contains("pixel") || lowerDesc.Contains("sprite"))
                return ProjectType.Game2D;
            if (lowerDesc.Contains("3d") || lowerDesc.Contains("three dimensional"))
                return ProjectType.Game3D;
            
            return ProjectType.Game3D; // Default
        }

        protected virtual GameGenre DetermineGameGenre(string description)
        {
            string lowerDesc = description.ToLower();
            
            if (lowerDesc.Contains("rpg") || lowerDesc.Contains("role-playing"))
                return GameGenre.RPG;
            if (lowerDesc.Contains("fps") || lowerDesc.Contains("first-person shooter"))
                return GameGenre.FPS;
            if (lowerDesc.Contains("platformer") || lowerDesc.Contains("jumping"))
                return GameGenre.Platformer;
            if (lowerDesc.Contains("puzzle") || lowerDesc.Contains("brain teaser"))
                return GameGenre.Puzzle;
            if (lowerDesc.Contains("strategy") || lowerDesc.Contains("tactical"))
                return GameGenre.Strategy;
            if (lowerDesc.Contains("racing") || lowerDesc.Contains("driving"))
                return GameGenre.Racing;
            if (lowerDesc.Contains("simulation") || lowerDesc.Contains("sim"))
                return GameGenre.Simulation;
            if (lowerDesc.Contains("action") || lowerDesc.Contains("combat"))
                return GameGenre.Action;
            if (lowerDesc.Contains("adventure") || lowerDesc.Contains("exploration"))
                return GameGenre.Adventure;
            
            return GameGenre.Action; // Default
        }

        protected virtual List<string> ExtractTechnicalFeatures(string description)
        {
            List<string> features = new List<string>();
            string lowerDesc = description.ToLower();
            
            if (lowerDesc.Contains("multiplayer") || lowerDesc.Contains("co-op") || lowerDesc.Contains("online"))
                features.Add("Multiplayer/Networking");
            if (lowerDesc.Contains("procedural") || lowerDesc.Contains("random generation"))
                features.Add("Procedural Generation");
            if (lowerDesc.Contains("ai") || lowerDesc.Contains("artificial intelligence"))
                features.Add("AI Systems");
            if (lowerDesc.Contains("physics") || lowerDesc.Contains("realistic movement"))
                features.Add("Advanced Physics");
            if (lowerDesc.Contains("shader") || lowerDesc.Contains("visual effects"))
                features.Add("Custom Shaders/VFX");
            if (lowerDesc.Contains("save") || lowerDesc.Contains("progress") || lowerDesc.Contains("persistent"))
                features.Add("Save System");
            if (lowerDesc.Contains("inventory") || lowerDesc.Contains("item management"))
                features.Add("Inventory System");
            if (lowerDesc.Contains("dialogue") || lowerDesc.Contains("conversation"))
                features.Add("Dialogue System");
            if (lowerDesc.Contains("skill tree") || lowerDesc.Contains("character progression"))
                features.Add("Character Progression");
            if (lowerDesc.Contains("crafting") || lowerDesc.Contains("building"))
                features.Add("Crafting/Building System");
            
            return features;
        }

        protected virtual string GetSectionHeader(string title, int level = 1)
        {
            string headerPrefix = new string('#', level);
            return $"{headerPrefix} {title}\n\n";
        }

        protected virtual string FormatList(IEnumerable<string> items, bool numbered = false)
        {
            if (items == null || !items.Any()) return "";

            StringBuilder sb = new StringBuilder();
            List<string> itemsList = items.ToList();

            for (int i = 0; i < itemsList.Count; i++)
            {
                string prefix = numbered ? $"{i + 1}. " : "- ";
                sb.AppendLine($"{prefix}{itemsList[i]}");
            }

            return sb.ToString() + "\n";
        }

        protected virtual string FormatTable(Dictionary<string, string> data, string keyHeader = "Property", string valueHeader = "Value")
        {
            if (data == null || !data.Any()) return "";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"| {keyHeader} | {valueHeader} |");
            sb.AppendLine($"|{new string('-', keyHeader.Length + 2)}|{new string('-', valueHeader.Length + 2)}|");

            foreach (KeyValuePair<string, string> kvp in data)
            {
                sb.AppendLine($"| {kvp.Key} | {kvp.Value} |");
            }

            return sb.ToString() + "\n";
        }

        protected virtual string AddTimestamp()
        {
            return $"*Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss} UTC*\n\n";
        }

        protected virtual string AddGenerationMetadata()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("---");
            sb.AppendLine("**Generation Metadata:**");
            sb.AppendLine($"- Generated by: {GetType().Name}");
            sb.AppendLine($"- Game Concept: {gameConcept.Title}");
            sb.AppendLine($"- Genre: {gameConcept.GameGenre}");
            sb.AppendLine($"- Type: {gameConcept.GameType}");
            sb.AppendLine($"- Generation Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("---\n");
            
            return sb.ToString();
        }
    }

    /// <summary>
    /// Represents parsed game concept information
    /// </summary>
    public class GameConcept
    {
        public string Title { get; set; } = "";
        public string CoreDescription { get; set; } = "";
        public List<string> CoreGameplayFeatures { get; set; } = new List<string>();
        public List<string> KeyFeatures { get; set; } = new List<string>();
        public List<string> TechnicalFeatures { get; set; } = new List<string>();
        public string TargetAudience { get; set; } = "";
        public string Platform { get; set; } = "";
        public string ArtStyle { get; set; } = "";
        public string DevelopmentTime { get; set; } = "";
        public ProjectType GameType { get; set; } = ProjectType.Game3D;
        public GameGenre GameGenre { get; set; } = GameGenre.Action;
    }

    /// <summary>
    /// Game genre enumeration
    /// </summary>
    public enum GameGenre
    {
        Action,
        Adventure,
        RPG,
        Strategy,
        Puzzle,
        Platformer,
        Racing,
        Simulation,
        FPS,
        Fighting,
        Sports,
        Horror,
        Survival
    }
}
