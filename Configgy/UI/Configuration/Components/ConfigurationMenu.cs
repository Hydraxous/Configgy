using Configgy.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Configgy.UI
{
    public class ConfigurationMenu : MonoBehaviour
    {
        [SerializeField] private RectTransform contentbody;

        private ConfigurationPage rootPage;
        private Dictionary<string,ConfigurationPage> pageManifest = new Dictionary<string,ConfigurationPage>();

        private bool menuBuilt = false;

        [Configgable(displayName:"Open Config Menu")]
        private static ConfigKeybind cfgKey = new ConfigKeybind(KeyCode.Backslash);

        [Configgable(displayName: "Notify When Update Available")]
        private static ConfigToggle notifyOnUpdateAvailable = new ConfigToggle(true);

        private GameObject[] menus;

        ConfigurationPage lastOpenPage;

        private static event Action onGlobalOpen;
        private static event Action<string> onGlobalOpenAtPath;
        private static event Action onGlobalClose;
        private bool menuOpen = false;

        internal static bool openedOnce;

        private void Awake()
        {
            if (!menuBuilt)
                BuildMenus(ConfigurationManager.GetMenus());

            onGlobalOpen += OpenMenu;
            onGlobalClose += CloseMenu;
            onGlobalOpenAtPath += OpenMenuAtPath;
            ConfigurationManager.OnMenusChanged += BuildMenus;
        }

        private void Update()
        {
            if (menuOpen)
            {
                CheckMenuOpen();
                return;
            }

            if (!cfgKey.WasPeformed())
                return;

            OpenMenu();
        }

        private void CheckMenuOpen()
        {
            if (menus == null)
                return;

            for(int i=0;i< menus.Length;i++)
            {
                if (menus[i] == null)
                    continue;

                if (menus[i].activeInHierarchy)
                    return;
            }

            //No menus are open so unpause.
            Unpause();
        }

        internal void Rebuild() 
        {
            BuildMenus(ConfigurationManager.GetMenus());
        }

        private void DestroyPages()
        {
            ConfigurationPage[] pages = pageManifest.Values.ToArray();

            for (int i=0;i<pages.Length;i++)
            {
                ConfigurationPage page = pages[i];
                Destroy(page.gameObject);
            }

            pageManifest.Clear();
            menuBuilt = false;
        }

        private void BuildMenus(ConfigBuilder[] menus)
        {
            DestroyPages();

            Debug.Log($"Building Configgy Menus {menus.Length}");

            NewPage((mainPage) =>
            {
                rootPage = mainPage;
                mainPage.SetHeader("Configuration");
                mainPage.Close();

                pageManifest.Add("", rootPage);
            });

            foreach(var menu in menus.OrderBy(x => x.DisplayName))
            {
                IConfigElement[] elements = menu.GetConfigElements();
                if(elements == null)
                {
                    Debug.LogError($"ConfigBuilder: {menu.GUID} has null elements. It cannot be built.");
                    continue;
                }

                BuildMenu(elements);
            }
        }

        private void BuildMenu(IConfigElement[] configElements)
        {
            foreach (IConfigElement configElement in configElements.OrderBy(x => x.GetDescriptor().Path))
            {
                BuildElement(configElement);
            }

            foreach(KeyValuePair<string, ConfigurationPage> page in pageManifest)
            {
                page.Value.RebuildPage();
            }

            menuBuilt = true;
        }

        private void BuildMenuTreeFromPath(string fullPath)
        {
            string[] path = fullPath.Split('/');

            string currentPathAddress = "";

            ConfigurationPage previousPage = null;

            for (int i = 0; i < path.Length; i++)
            {
                currentPathAddress += path[i];

                if (!pageManifest.ContainsKey(currentPathAddress))
                {
                    if (previousPage == null)
                        previousPage = rootPage;

                    NewPage((page) =>
                    {
                        //Have to create a new reference because previousPage changes and it causes issues when the button is pressed.
                        ConfigurationPage closablePage = previousPage;

                        //Add button to parent page to access subpage
                        ConfigButton openSubPageButton = new ConfigButton(() =>
                        {
                            closablePage.Close();
                            page.Open();
                        }, path[i]);

                        closablePage.AddElement(openSubPageButton);

                        //Configure page
                        page.SetHeader(path[i]);
                        page.SetParent(closablePage);
                        page.gameObject.name = currentPathAddress;
                        page.SetFooter(currentPathAddress);
                        page.Close();

                        previousPage = page;
                        pageManifest.Add(currentPathAddress, page);
                    });
                }else
                {
                    previousPage = pageManifest[currentPathAddress];
                }
                
                currentPathAddress += "/";
            }
        }
        
        private void BuildElement(IConfigElement element)
        {
            ConfiggableAttribute descriptor = element.GetDescriptor();
            
            string path = "";

            if (descriptor != null && descriptor.Owner != null)
            {
                path = $"{descriptor.Owner.DisplayName}";
                if(!string.IsNullOrEmpty(descriptor.Path))
                    path += $"/{descriptor.Path}";
            }else
            {
                path = "/Other";
            }

            BuildMenuTreeFromPath(path);

            pageManifest[path].AddElement(element);
        }

        private void NewPage(Action<ConfigurationPage> onInstance)
        {
            GameObject newPage = GameObject.Instantiate(PluginAssets.ConfigurationPage, contentbody);
            ConfigurationPage page = newPage.GetComponent<ConfigurationPage>();
            newPage.AddComponent<BehaviourRelay>().OnOnEnable += (g) => lastOpenPage = page;
            onInstance?.Invoke(page);
        }

        public void OpenMenu()
        {
            if (menuOpen)
                return;

            menus = transform.GetChildren().Select(x=>x.gameObject).ToArray();
            Pauser.Pause(menus);

            if (lastOpenPage != null)
                lastOpenPage.Open();
            else
                rootPage.Open();
            
            menuOpen = true;

            if (!openedOnce)
            {
                openedOnce = true;
                if (!Plugin.UsingLatest && notifyOnUpdateAvailable.Value)
                {
                    ShowUpdatePrompt();
                }
            }

        }

        public void OpenMenuAtPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                OpenMenu();
                return;
            }

            menus = transform.GetChildren().Select(x => x.gameObject).ToArray();
            Pauser.Pause(menus);

            string copyPath = path;
            //ensure path exists, if not go up a directory until it does.
            while (!pageManifest.ContainsKey(copyPath) && !string.IsNullOrEmpty(copyPath) && copyPath != "/")
            {
                copyPath = Path.GetDirectoryName(copyPath);
            }

            if (string.IsNullOrEmpty(copyPath) || !pageManifest.ContainsKey(copyPath))
            {
                OpenMenu();
                return;
            }

            pageManifest[copyPath].Open();
            menuOpen = true;

            if (!openedOnce)
            {
                openedOnce = true;
                if (!Plugin.UsingLatest && notifyOnUpdateAvailable.Value)
                {
                    ShowUpdatePrompt();
                }
            }
        }

        private void ShowUpdatePrompt()
        {
            ModalDialogue.ShowDialogue(new ModalDialogueEvent()
            {
                Title = "Outdated",
                Message = $"You are using an outdated version of {ConstInfo.NAME}: (<color=red>{ConstInfo.VERSION}</color>). Please update to the latest version from Github or Thunderstore: (<color=green>{Plugin.LatestVersion}</color>)",
                Options = new DialogueBoxOption[]
                        {
                            new DialogueBoxOption()
                            {
                                Name = "Open Browser",
                                Color = Color.white,
                                OnClick = () => Application.OpenURL(ConstInfo.GITHUB_URL+"/releases/latest")
                            },
                            new DialogueBoxOption()
                            {
                                Name = "Later",
                                Color = Color.white,
                                OnClick = () => { }
                            },
                            new DialogueBoxOption()
                            {
                                Name = "Don't Ask Again.",
                                Color = Color.red,
                                OnClick = () =>
                                {
                                    notifyOnUpdateAvailable.SetValue(false);
                                }
                            }
                        }
            });
        }

        public void CloseMenu()
        {
            if (!menuOpen)
                return;

            menus = transform.GetChildren().Select(x => x.gameObject).ToArray();
            for(int i = 0; i < menus.Length; i++)
            {
                menus[i].SetActive(false);
            }
        }

        public static void Open()
        {
            onGlobalOpen?.Invoke();
        }

        public static void OpenAtPath(string path)
        {
            onGlobalOpenAtPath?.Invoke(path);
        }

        public static void Close()
        {
            onGlobalClose?.Invoke();
        }

        private void Unpause()
        {
            Pauser.Unpause();
            menuOpen = false;
        }

        private void OnDestroy()
        {
            ConfigurationManager.OnMenusChanged -= BuildMenus;
            onGlobalOpen -= OpenMenu;
            onGlobalClose -= CloseMenu;
            onGlobalOpenAtPath -= OpenMenuAtPath;
        }
    }
}
