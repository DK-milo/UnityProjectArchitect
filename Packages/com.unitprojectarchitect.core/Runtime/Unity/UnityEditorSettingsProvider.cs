using System;
using UnityProjectArchitect.AI.Services;

namespace UnityProjectArchitect.Unity
{
    /// <summary>
    /// Unity Editor specific settings provider that bridges EditorPrefs with AIAssistant
    /// </summary>
    public class UnityEditorSettingsProvider : ISettingsProvider
    {
        private readonly ILogger _logger;

        public UnityEditorSettingsProvider(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string GetEncryptedValue(string key, string defaultValue = "")
        {
            try
            {
#if UNITY_EDITOR
                // Map our key to Unity EditorPrefs for the API key specifically
                if (key == "UnityProjectArchitect_ClaudeAPIKey")
                {
                    return UnityEditor.EditorPrefs.GetString("UnityProjectArchitect.ClaudeAPIKey", defaultValue);
                }
                
                // For other keys, use standard EditorPrefs
                return UnityEditor.EditorPrefs.GetString(key, defaultValue);
#else
                return defaultValue;
#endif
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to get encrypted value for key '{key}': {ex.Message}");
                return defaultValue;
            }
        }

        public void SetEncryptedValue(string key, string value)
        {
            try
            {
#if UNITY_EDITOR
                // Map our key to Unity EditorPrefs for the API key specifically
                if (key == "UnityProjectArchitect_ClaudeAPIKey")
                {
                    UnityEditor.EditorPrefs.SetString("UnityProjectArchitect.ClaudeAPIKey", value);
                }
                else
                {
                    // For other keys, use standard EditorPrefs
                    UnityEditor.EditorPrefs.SetString(key, value);
                }
#endif
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to set encrypted value for key '{key}': {ex.Message}");
            }
        }

        public void ClearEncryptedValue(string key)
        {
            try
            {
#if UNITY_EDITOR
                // Map our key to Unity EditorPrefs for the API key specifically
                if (key == "UnityProjectArchitect_ClaudeAPIKey")
                {
                    UnityEditor.EditorPrefs.DeleteKey("UnityProjectArchitect.ClaudeAPIKey");
                }
                else
                {
                    // For other keys, use standard EditorPrefs
                    UnityEditor.EditorPrefs.DeleteKey(key);
                }
#endif
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to clear encrypted value for key '{key}': {ex.Message}");
            }
        }

        public bool HasKey(string key)
        {
            try
            {
#if UNITY_EDITOR
                // Map our key to Unity EditorPrefs for the API key specifically
                if (key == "UnityProjectArchitect_ClaudeAPIKey")
                {
                    return UnityEditor.EditorPrefs.HasKey("UnityProjectArchitect.ClaudeAPIKey");
                }
                
                // For other keys, use standard EditorPrefs
                return UnityEditor.EditorPrefs.HasKey(key);
#else
                return false;
#endif
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to check key '{key}': {ex.Message}");
                return false;
            }
        }
    }
}
