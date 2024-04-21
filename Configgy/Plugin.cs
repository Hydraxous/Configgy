using BepInEx;
using BepInEx.Configuration;
using Configgy.Assets;
using Configgy.Configuration.AutoGeneration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Configgy
{
    [BepInPlugin(ConstInfo.GUID, ConstInfo.NAME, ConstInfo.VERSION)]
    [BepInProcess("ULTRAKILL.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony;

        private ConfigBuilder configgyConfig;

        public static bool UsingLatest = true;
        public static string LatestVersion { get; private set; } = ConstInfo.VERSION;

        private void Awake()
        {
            PluginAssets.Initialize();
            harmony = new Harmony(ConstInfo.GUID+".harmony");
            harmony.PatchAll();

            Paths.CheckFolders();

            configgyConfig = new ConfigBuilder(ConstInfo.GUID, "Configgy");
            configgyConfig.BuildAll();

            VersionCheck.CheckVersion(ConstInfo.GITHUB_VERSION_URL, ConstInfo.VERSION, (r, latest) =>
            {
                UsingLatest = r;
                if (!UsingLatest)
                {
                    LatestVersion = latest;
                    Debug.LogWarning($"New version of {ConstInfo.NAME} available. Current:({ConstInfo.VERSION}) Latest: ({LatestVersion})");
                }
            });

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            Logger.LogInfo($"Plugin {ConstInfo.NAME} is loaded!");
        }

        private void SceneManager_sceneLoaded(Scene _, LoadSceneMode __)
        {
            if (SceneHelper.CurrentScene != "Main Menu")
                return;

            BepinAutoGenerator.Generate();
        }

        //Save data on exit
        private void OnApplicationQuit()
        {
            foreach (var config in ConfigurationManager.GetMenus())
            {
                config.SaveData();
            }
        }
    }
}
