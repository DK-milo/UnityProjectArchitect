using System;
using System.Collections.Generic;

namespace UnityProjectArchitect.AI.Services
{
    /// <summary>
    /// Interface for providing encrypted settings storage
    /// Unity implementation will use EditorPrefs, standalone can use registry/file
    /// </summary>
    public interface ISettingsProvider
    {
        string GetEncryptedValue(string key, string defaultValue = "");
        void SetEncryptedValue(string key, string value);
        void ClearEncryptedValue(string key);
        bool HasKey(string key);
    }

    /// <summary>
    /// Simple in-memory settings provider for testing and standalone scenarios
    /// </summary>
    public class InMemorySettingsProvider : ISettingsProvider
    {
        private readonly Dictionary<string, string> _settings = new Dictionary<string, string>();

        public string GetEncryptedValue(string key, string defaultValue = "")
        {
            return _settings.TryGetValue(key, out string value) ? value : defaultValue;
        }

        public void SetEncryptedValue(string key, string value)
        {
            _settings[key] = value;
        }

        public void ClearEncryptedValue(string key)
        {
            _settings.Remove(key);
        }

        public bool HasKey(string key)
        {
            return _settings.ContainsKey(key);
        }
    }
}