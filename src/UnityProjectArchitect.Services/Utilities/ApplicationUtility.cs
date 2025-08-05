using System;
using System.IO;

namespace UnityProjectArchitect.Services
{
    public static class Application
    {
        public static string dataPath => Path.Combine(Environment.CurrentDirectory, "Assets");
        public static string streamingAssetsPath => Path.Combine(dataPath, "StreamingAssets");
    }
}