using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Configgy
{
    public static class VersionCheck
    {
        public static void CheckVersion(string githubURL, string runningVersionName, Action<bool, string> onCheckComplete = null)
        {
            behaviour.StartCoroutine(CheckLatestVersion(githubURL, runningVersionName, onCheckComplete));
        }

        private static MonoBehaviour _behaviour;
        private static MonoBehaviour behaviour
        {
            get
            {
                if(_behaviour == null)
                {
                    _behaviour = new GameObject("VersionChecker").AddComponent<BehaviourRelay>();
                }
                return _behaviour;
            }
        }

        //matches current mod version with latest release on github
        private static IEnumerator CheckLatestVersion(string githubURL, string runningVersionName, Action<bool, string> onCheckComplete = null)
        {
            bool usingLatest = true;
            string latestVersionName = "UNKNOWN";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(githubURL))
            {
                yield return webRequest.SendWebRequest();

                if (!webRequest.isNetworkError)
                {
                    string page = webRequest.downloadHandler.text;
                    try
                    {
                        latestVersionName = JArray.Parse(page)[0].Value<string>("name");
                        usingLatest = (latestVersionName == runningVersionName);
                    }
                    catch (System.Exception e)
                    {
                        usingLatest = true;
                        latestVersionName = runningVersionName;
                        ConfiggyPlugin.Log.LogError($"Error getting version info for {runningVersionName}. {e}");
                    }

                }
            }

            onCheckComplete.Invoke(usingLatest, latestVersionName);
        }
    }
}
