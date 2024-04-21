using System.IO;
using System.Reflection;
using UnityEngine;

namespace Configgy
{
    internal static class Paths
    {
        public static string ExecutionPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);    
        public static string GameFolder => Path.GetDirectoryName(Application.dataPath);
        public static string BepInExFolder => Path.Combine(GameFolder, "BepInEx");
        public static string BepInExConfigFolder => Path.Combine(BepInExFolder, "config");
        public static string DataFolder => Path.Combine(BepInExConfigFolder, "Configgy");
        public static string LegacyDataFolder => Path.Combine(ExecutionPath, "Configs");

        public static void CheckFolders()
        {
            if (!Directory.Exists(DataFolder))
                Directory.CreateDirectory(DataFolder);
        }
    }
}
