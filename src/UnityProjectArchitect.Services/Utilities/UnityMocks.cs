using System;

namespace UnityProjectArchitect.Services.Utilities
{
    // Mock ScriptableObject for Services DLL compatibility
    public class ScriptableObject
    {
        public static T CreateInstance<T>() where T : new()
        {
            return new T();
        }
    }

    // Mock JsonUtility for Services DLL compatibility - simple implementation
    public static class JsonUtility
    {
        public static string ToJson(object obj, bool prettyPrint = false)
        {
            // Simple mock implementation - just return a basic JSON-like string
            if (obj == null) return "null";
            
            // For basic objects, return a simple representation
            return $"{{ \"type\": \"{obj.GetType().Name}\", \"toString\": \"{obj}\" }}";
        }

        public static T FromJson<T>(string json)
        {
            // Simple mock implementation - just return default instance
            return default(T);
        }
    }

    // Mock Application for Services DLL compatibility
    public static class Application
    {
        public static string dataPath = "Assets";
    }

    // Mock Debug for Services DLL compatibility
    public static class Debug
    {
        public static void Log(string message)
        {
            Console.WriteLine($"[LOG] {message}");
        }

        public static void LogWarning(string message)
        {
            Console.WriteLine($"[WARNING] {message}");
        }

        public static void LogError(string message)
        {
            Console.WriteLine($"[ERROR] {message}");
        }
    }

    // Mock GameObject for Services DLL compatibility
    public class GameObject
    {
        public string name;

        public GameObject(string name)
        {
            this.name = name;
        }
    }
}