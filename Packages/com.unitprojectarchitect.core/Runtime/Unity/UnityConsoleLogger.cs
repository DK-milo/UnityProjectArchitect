using System;
using UnityProjectArchitect.AI.Services;

namespace UnityProjectArchitect.Unity
{
    /// <summary>
    /// Unity-specific logger that outputs to Unity console
    /// </summary>
    public class UnityConsoleLogger : ILogger
    {
        public void Log(string message)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            UnityEngine.Debug.Log($"[UnityProjectArchitect] {message}");
#else
            Console.WriteLine($"[UnityProjectArchitect] {message}");
#endif
        }

        public void LogWarning(string message)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            UnityEngine.Debug.LogWarning($"[UnityProjectArchitect] {message}");
#else
            Console.WriteLine($"[UnityProjectArchitect WARNING] {message}");
#endif
        }

        public void LogError(string message)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            UnityEngine.Debug.LogError($"[UnityProjectArchitect] {message}");
#else
            Console.WriteLine($"[UnityProjectArchitect ERROR] {message}");
#endif
        }

        public void LogError(Exception exception)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            UnityEngine.Debug.LogException(exception);
#else
            Console.WriteLine($"[UnityProjectArchitect EXCEPTION] {exception}");
#endif
        }
    }
}
