using BepInEx.Configuration;

using System.Collections.Generic;

using UnityEngine;

namespace Configgy.Configuration.AutoGeneration
{
    internal static class DirtyConfigFiles
    {
        private static readonly HashSet<ConfigFile> files = [];

        public static void Mark(ConfigFile file)
        {
            if (!file.SaveOnConfigSet)
            {
                files.Add(file);
            }
        }

        public static void Save(ConfigFile file)
        {
            if (files.Remove(file))
            {
                file.Save();
            }
        }
    }
}
