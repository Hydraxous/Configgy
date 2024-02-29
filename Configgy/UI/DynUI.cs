using Configgy.Assets;
using Configgy.UI.Template;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy.UI
{
    //Shoutout to xzxADIxzx for inspiring this class from the Jaket mod's UI class.
    public static class DynUI
    {

        public static void Frame(RectTransform rect, Action<Frame> onInstance)
        {
            Instantiate<Frame>(rect, PluginAssets.FramePrefab, onInstance);
        }

        public static void Button(RectTransform rect, Action<Button> onInstance)
        {
            Instantiate<Button>(rect, PluginAssets.ButtonPrefab, onInstance);
        }

        public static void ImageButton(RectTransform rect, Action<Button, Image> onInstance)
        {
            Action<Button> onSubInstance = new Action<Button>((b) =>
            {
                Image icon = b.GetComponentsInChildren<Image>().Where(x=>x.name == "Icon").FirstOrDefault();
                onInstance?.Invoke(b,icon);
            });

            Instantiate<Button>(rect, PluginAssets.ImageButtonPrefab, onSubInstance);
        }

        public static void ScrollRect(RectTransform rect, Action<ScrollRect> onInstance)
        {
            Instantiate<ScrollRect>(rect, PluginAssets.ScrollRectPrefab, onInstance);
        }

        public static void Label(RectTransform rect, Action<Text> onInstance)
        {
            Instantiate<Text>(rect, PluginAssets.TextPrefab, onInstance);
        }

        public static void InputField(RectTransform rect, Action<InputField> onInstance)
        {
            Instantiate<InputField>(rect, PluginAssets.InputFieldPrefab, onInstance);
        }

        public static void Slider(RectTransform rect, Action<Slider> onInstance)
        {
            Instantiate<Slider>(rect, PluginAssets.SliderPrefab, onInstance);
        }

        public static void Toggle(RectTransform rect, Action<Toggle> onInstance)
        {
            Instantiate<Toggle>(rect, PluginAssets.TogglePrefab, onInstance);
        }

        public static void Dropdown(RectTransform rect, Action<Dropdown> onInstance)
        {
            Instantiate<Dropdown>(rect, PluginAssets.DropdownPrefab, onInstance);
        }

        public static void Div(RectTransform rect, Action<RectTransform> onInstance)
        {
            GameObject div = new GameObject("div");
            div.transform.parent = rect.transform;
            onInstance?.Invoke(div.AddComponent<RectTransform>());
        }

        public static void Description(RectTransform rect, Action<RectTransform, Text> onInstance)
        {
            GameObject description = GameObject.Instantiate(PluginAssets.DescriptionPrefab, rect);
            RectTransform rectTransform = description.GetComponent<RectTransform>();
            Text text = description.GetComponentInChildren<Text>();
            onInstance?.Invoke(rectTransform, text);
        }

        public static void Component<T>(Transform transform, Action<T> onInstance) where T : Component
        {
            onInstance?.Invoke(transform.gameObject.AddComponent<T>());
        }

        private static void Instantiate<T>(RectTransform rect, GameObject prefab, Action<T> onInstance) where T : Component
        {
            onInstance?.Invoke(GameObject.Instantiate(prefab, rect).GetComponent<T>());
        }

        public static void SetAnchors(this RectTransform rect, float minX, float minY, float maxX, float maxY)
        {
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.anchorMin = new Vector2(minX, minY);
        }

        public static ColorBlock SetFirstColor(this ColorBlock colorBlock, Color color)
        {
            colorBlock.normalColor = color;
            return colorBlock;
        }

        public static class ConfigUI
        {
            public static void CreateElementSlot<T>(RectTransform rect, ConfigValueElement<T> valueElement, Action<RectTransform> onInstance, Action<RectTransform> onButtonSlots = null)
            {
                ConfiggableAttribute configgable = (valueElement as IConfigElement).GetDescriptor();

                DynUI.Frame(rect, (f) =>
                {

                    f.RectTransform.sizeDelta = new Vector2(f.RectTransform.sizeDelta.x, 55f);

                    DynUI.Div(f.Content, (operatorsDiv) =>
                    {
                        operatorsDiv.SetAnchors(0, 0, 0.2f, 1f);
                        DynUI.Layout.Fill(operatorsDiv);

                        DynUI.Component<HorizontalLayoutGroup>(operatorsDiv, (hlg) =>
                        {
                            hlg.childForceExpandHeight = true;
                            hlg.childForceExpandWidth = true;

                            hlg.childControlHeight = false;
                            hlg.childControlWidth = false;
                            hlg.childAlignment = TextAnchor.MiddleLeft;
                        });

                        DynUI.ImageButton(operatorsDiv, (button, icon) =>
                        {
                            RectTransform rt = button.GetComponent<RectTransform>();
                            rt.sizeDelta = new Vector2(40f, 40f);
                            icon.sprite = PluginAssets.Icon_Reset;
                            button.onClick.AddListener(valueElement.ResetValue);
                        });

                        if (!string.IsNullOrEmpty(configgable.Description))
                        {
                            GameObject newDescriptionBox = null;

                            DynUI.Description(rect, (r, desc) =>
                            {
                                newDescriptionBox = r.gameObject;
                                desc.text = configgable.Description;
                                r.gameObject.SetActive(false);
                            });

                            DynUI.ImageButton(operatorsDiv, (button, icon) =>
                            {
                                RectTransform rt = button.GetComponent<RectTransform>();
                                rt.sizeDelta = new Vector2(40f, 40f);
                                icon.sprite = PluginAssets.Icon_Info;
                                button.onClick.AddListener(() => newDescriptionBox.SetActive(!newDescriptionBox.activeInHierarchy));
                            });
                        }

                        //Debug
                        /*
                        DynUI.ImageButton(operatorsDiv, (button, icon) =>
                        {
                            RectTransform rt = button.GetComponent<RectTransform>();
                            rt.sizeDelta = new Vector2(55f, 55f);
                            button.onClick.AddListener(() => { Debug.Log(valueElement.Value); });
                        });

                        DynUI.ImageButton(operatorsDiv, (button, icon) =>
                        {
                            RectTransform rt = button.GetComponent<RectTransform>();
                            rt.sizeDelta = new Vector2(55f, 55f);
                            button.onClick.AddListener(() => { Debug.Log(valueElement.DefaultValue); });
                        });
                        */
                        onButtonSlots?.Invoke(operatorsDiv);
                    });


                    DynUI.Div(f.Content, (elementsDiv) =>
                    {
                        elementsDiv.SetAnchors(0.2f, 0, 1f, 1f);
                        DynUI.Layout.Fill(elementsDiv);

                        DynUI.Component<HorizontalLayoutGroup>(elementsDiv, (hlg) =>
                        {
                            hlg.childForceExpandHeight = true;
                            hlg.childForceExpandWidth = true;

                            hlg.childControlHeight = true;
                            hlg.childControlWidth = true;

                            hlg.childAlignment = TextAnchor.MiddleRight;
                        });

                        DynUI.Label(elementsDiv, (t) =>
                        {
                            t.text = configgable.DisplayName;
                            t.fontSize = 4;
                            t.resizeTextMaxSize = 25;
                        });

                        onInstance?.Invoke(elementsDiv);
                    });
                });
            }
        }

        public static class Layout
        {
            public static void HalfTop(RectTransform rt)
            {
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(1, 1);
            }

            public static void HalfBottom(RectTransform rt)
            {
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 0.5f);
            }

            public static void HalfLeft(RectTransform rt)
            {
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(0.5f, 1);
            }

            public static void HalfRight(RectTransform rt)
            {
                rt.anchorMin = new Vector2(0,0);
                rt.anchorMax = new Vector2(0.5f,1);
            }

            public static void FillParent(RectTransform rt)
            {
                rt.anchorMin = new Vector2(0,0);
                rt.anchorMax = new Vector2(1, 1);
                Fill(rt);
            }

            public static void Fill(RectTransform rt)
            {
                rt.offsetMax= new Vector2(0,0);
                rt.offsetMin= new Vector2(0,0);
                rt.sizeDelta = new Vector2(0f,0f);
            }

            public static void Padding(RectTransform rt, float padding)
            {
                rt.anchoredPosition = new Vector2(-(padding*2f),-(padding*2f));
            }

            public static void CenterAnchor(RectTransform rt)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
            }
        }

    }
}
