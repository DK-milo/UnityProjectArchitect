using System;

namespace UnityProjectArchitect.Services
{
    public static class Debug
    {
        public static void Log(object message)
        {
            Console.WriteLine($"[LOG] {message}");
        }

        public static void LogWarning(object message)
        {
            Console.WriteLine($"[WARNING] {message}");
        }

        public static void LogError(object message)
        {
            Console.WriteLine($"[ERROR] {message}");
        }
    }
}