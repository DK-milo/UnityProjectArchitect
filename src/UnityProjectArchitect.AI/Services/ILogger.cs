using System;

namespace UnityProjectArchitect.AI.Services
{
    /// <summary>
    /// Interface for logging operations
    /// Unity implementation will use Debug.Log, standalone uses Console or other logger
    /// </summary>
    public interface ILogger
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogError(Exception exception);
    }

    /// <summary>
    /// Console-based logger for standalone scenarios
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"[INFO] {DateTime.Now:HH:mm:ss} {message}");
        }

        public void LogWarning(string message)
        {
            Console.WriteLine($"[WARN] {DateTime.Now:HH:mm:ss} {message}");
        }

        public void LogError(string message)
        {
            Console.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss} {message}");
        }

        public void LogError(Exception exception)
        {
            Console.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss} {exception}");
        }
    }

    /// <summary>
    /// No-op logger for scenarios where logging is not needed
    /// </summary>
    public class NullLogger : ILogger
    {
        public void Log(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message) { }
        public void LogError(Exception exception) { }
    }
}