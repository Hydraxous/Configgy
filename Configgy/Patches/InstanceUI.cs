using Configgy.Assets;
using Configgy.UI;
using HarmonyLib;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy.Patches
{
    [HarmonyPatch(typeof(CanvasController))]
    public static class InstanceUI
    {
        public static RectTransform CanvasRect { get; private set; }

        private static ConfigurationMenu instancedMenu;

        [HarmonyPatch("Awake"), HarmonyPostfix]
        public static void OnStart(CanvasController __instance)
        {
            CanvasRect = __instance.GetComponent<RectTransform>();
            instancedMenu = GameObject.Instantiate(PluginAssets.ConfigurationMenu, CanvasRect).GetComponentInChildren<ConfigurationMenu>(true);
            InstanceOpenConfigButtonPauseMenu(CanvasRect);
            InstanceOpenConfigButtonMainMenu(CanvasRect);
            InstanceModalDialogueManager(CanvasRect);

        }

        private static void InstanceModalDialogueManager(RectTransform rect)
        {
            GameObject modalDialogueManagerObject = GameObject.Instantiate(PluginAssets.ModalDialogueManager, rect);
        }

        private static void InstanceOpenConfigButtonPauseMenu(RectTransform rect)
        {
            Transform pausemenu = rect.GetChildren().Where(x => x.name == "PauseMenu").FirstOrDefault();
            RectTransform pauseMenuRect = pausemenu.GetComponent<RectTransform>();

            Button optionMenuButton = pausemenu.GetComponentsInChildren<Button>().Where(x => x.name == "Options").FirstOrDefault();
            RectTransform optionButtonRect = optionMenuButton.GetComponent<RectTransform>();

            float buttonHeight = optionButtonRect.sizeDelta.y;
            Vector2 optionButtonPosition = optionButtonRect.anchoredPosition;

            DynUI.ImageButton(pauseMenuRect, (b,i) =>
            {
                i.sprite = PluginAssets.Icon_Configgy;

                RectTransform buttonRect = b.GetComponent<RectTransform>();
                buttonRect.SetAnchors(0.5f,0.5f,0.5f,0.5f);
                buttonRect.sizeDelta = new Vector2(buttonHeight, buttonHeight);
                optionButtonPosition.x += (optionButtonRect.sizeDelta.x / 2f) + (buttonHeight/2f) + 2f;
                buttonRect.anchoredPosition = optionButtonPosition;
                b.onClick.AddListener(() =>
                {
                    pauseMenuRect.gameObject.SetActive(false);
                    OptionsManager.Instance.UnPause();
                });
                b.onClick.AddListener(ConfigurationMenu.Open);
            });
        }

        private static void InstanceOpenConfigButtonMainMenu(RectTransform rect)
        {
            //Only on main menu.
            if (SceneHelper.CurrentScene != "Main Menu")
                return;

            Transform mainMenu = rect.GetChildren().Where(x => x.name == "Main Menu (1)").FirstOrDefault();
            RectTransform mainMenuRect = mainMenu.GetComponent<RectTransform>();

            RectTransform panel = mainMenu.GetChildren().Where(x => x.name == "Panel").FirstOrDefault().GetComponent<RectTransform>();
            float buttonHeight = panel.sizeDelta.y;
            Vector2 panelPos = panel.anchoredPosition;

            DynUI.ImageButton(mainMenuRect, (b, i) =>
            {
                i.sprite = PluginAssets.Icon_Configgy;

                RectTransform buttonRect = b.GetComponent<RectTransform>();
                buttonRect.SetAnchors(0.5f, 0.5f, 0.5f, 0.5f);
                buttonRect.sizeDelta = new Vector2(buttonHeight, buttonHeight);

                panelPos.x += (panel.sizeDelta.x / 2f) + (buttonHeight / 2f) + 2f;
                buttonRect.anchoredPosition = panelPos;
                b.onClick.AddListener(ConfigurationMenu.Open);
            });
        }
    }
}
