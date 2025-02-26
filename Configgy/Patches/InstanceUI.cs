using Configgy.Assets;
using Configgy.UI;
using HarmonyLib;
using System.Linq;
using TMPro;
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

            Transform optionsPage = rect.GetChildren().Where(x => x.name == "OptionsMenu").FirstOrDefault();
            if (!CheckObject(optionsPage, "Options Page is null."))
                return;

            RectTransform optionsMenuRect = optionsPage.GetComponent<RectTransform>();

            RectTransform navRail = optionsPage.GetChildren().Where(x => x.name == "Navigation Rail").FirstOrDefault().GetComponent<RectTransform>();
            if (!CheckObject(navRail, "Navigation Rail is null."))
                return;

            //Find a button and replicate it.
            GameObject buttonSourceObj = navRail.GetChildren().Where(x => x.GetComponent<Button>()).Select(x=>x.gameObject).FirstOrDefault();
            if (!CheckObject(buttonSourceObj, "Navigation Rail Button is null."))
                return;

            int childIndex = navRail.childCount - 3; //2 offset for "Back" button and empty spacing.

            //Check if saves button is present and change our button index to be just after it.
            GameObject savesButton = navRail.GetChildren().Where(x => x.name == "Saves").Select(x=>x.gameObject).FirstOrDefault();
            if (savesButton)
            {
                childIndex = savesButton.transform.GetSiblingIndex()+1;
            }

            GameObject configgyButtonObj = GameObject.Instantiate(buttonSourceObj, navRail);
            configgyButtonObj.transform.SetSiblingIndex(childIndex);
            TextMeshProUGUI buttonText = configgyButtonObj.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = ConstInfo.NAME.ToUpper();

            Button configgyButton = configgyButtonObj.GetComponent<Button>();
            configgyButton.m_OnClick = new Button.ButtonClickedEvent(); //Clear existing events and add ours. Double check this doesnt break anything.
            configgyButton.m_OnClick.AddListener(ConfigurationMenu.Open);
        }

        private static bool CheckObject(object o, string error)
        {
            if (o == null)
            {
                ConfiggyPlugin.Log.LogError(error);
                return false;
            }

            return true;
        }

        //Old, but kept in-case I need to change it back.
        private static void GenerateConfigurationMenuButton(RectTransform rect, Vector2 size, Vector2 position)
        {
            DynUI.ImageButton(rect, (b, i) =>
            {
                i.sprite = PluginAssets.Icon_Configgy;

                RectTransform buttonRect = b.GetComponent<RectTransform>();
                buttonRect.SetAnchors(0.5f, 0.5f, 0.5f, 0.5f);
                buttonRect.sizeDelta = size;

                buttonRect.anchoredPosition = position;
                b.onClick.AddListener(ConfigurationMenu.Open);
            });
        }
    }
}
