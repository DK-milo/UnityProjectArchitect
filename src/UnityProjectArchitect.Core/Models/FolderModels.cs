using System;
using System.Collections.Generic;

namespace UnityProjectArchitect.Core
{
    public enum FolderType
    {
        Scripts,
        Prefabs,
        Materials,
        Scenes,
        Audio,
        Textures,
        Animation,
        Art,
        Custom,
        Resources,
        Editor,
        Documentation
    }

    public class FolderDefinition
    {
        public string Name { get; set; } = "";
        public FolderType Type { get; set; }
        public string Description { get; set; } = "";
        public string Path { get; set; } = "";
        public bool IsRequired { get; set; } = true;
        public bool IsCreated { get; set; } = false;
        public List<FolderDefinition> SubFolders { get; set; } = new List<FolderDefinition>();
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        public DateTime CreatedDate { get; set; }

        public FolderDefinition()
        {
            CreatedDate = DateTime.Now;
        }

        public FolderDefinition(string name, FolderType type) : this()
        {
            Name = name;
            Type = type;
        }

        public FolderDefinition(string name, FolderType type, string description) : this(name, type)
        {
            Description = description;
        }

        public void AddSubFolder(FolderDefinition subFolder)
        {
            if (!SubFolders.Contains(subFolder))
            {
                SubFolders.Add(subFolder);
            }
        }

        public bool RemoveSubFolder(FolderDefinition subFolder)
        {
            return SubFolders.Remove(subFolder);
        }

        public FolderDefinition FindSubFolder(string name)
        {
            return SubFolders.Find(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public override bool Equals(object obj)
        {
            return obj is FolderDefinition other && Name == other.Name && Type == other.Type;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Type);
        }
    }

}