using Configgy.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy
{
    public class ConfigColor : ConfigValueElement<Color>
    {
        public ConfigColor(Color defaultValue) : base(defaultValue)
        {
            OnValueChanged += (_) => RefreshElementValue();
            OnValueChanged += UpdateColorDisplayColor;
            serializedColor = new SerializedColor(defaultValue);
            sliders = new Slider[4];
        }

        private SerializedColor serializedColor;

        private Slider[] sliders;
        private InputField hexInput;
        private Image colorDisplay;

        protected override void BuildElementCore(RectTransform rect)
        {
            DynUI.ConfigUI.CreateElementSlot(rect, this, (r) =>
            {
                DynUI.InputField(r, SetInputField);

                for(int i = 0; i < sliders.Length; i++)
                {
                    DynUI.Slider(r, (s) =>
                    {
                        SetSlider(s, i);
                    });
                }

                DynUI.ImageButton(r, (b, i) =>
                {
                    UnityEngine.Object.Destroy(b);
                    SetColorDisplay(i);
                });
            });
        }

        private void SetColorDisplay(Image image)
        {
            image.sprite = null;
            colorDisplay = image;
        }

        private void UpdateColorDisplayColor(Color color)
        {
            if (colorDisplay == null)
                return;

            colorDisplay.color = color;
        }


        protected override void LoadValueCore()
        {
            firstLoadDone = true; //nullable values apparently can just randomly have values so this annoying bool is needed

            if (config.TryGetValueAtAddress<SerializedColor>(descriptor.SerializationAddress, out SerializedColor color))
            {
                try
                {
                    SetValue(color.ToColor());
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            ResetValue();
            //SaveValue(); TODO idk check this.
        }

        private string ToHex(Color color)
        {
            return $"{ColorUtility.ToHtmlStringRGBA(color)}";
        }

        private bool TryParseHex(string hex, out Color color)
        {
            return ColorUtility.TryParseHtmlString(hex, out color);
        }


        protected override void SaveValueCore()
        {
            object obj = serializedColor;
            config.SetValueAtAddress(descriptor.SerializationAddress, obj);
            config.SaveDeferred();
            IsDirty = false;
        }

        private void SetInputField(InputField input)
        {
            if (input == null)
                return;

            hexInput = input;
            input.onEndEdit.AddListener((s) =>
            {
                //UX thing :3
                if(!s.StartsWith("#"))
                    s = $"#{s}";

                if (TryParseHex(s, out Color color))
                {
                    serializedColor = new SerializedColor(color);
                    SetValue(color);
                }
                else
                {
                    input.SetTextWithoutNotify(ToHex(serializedColor.ToColor()));
                }
            });
        }

        private void SetSlider(Slider slider, int index)
        {
            sliders[index] = slider;
            slider.onValueChanged.AddListener((v) => SetValueFromSlider(index, v));
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.wholeNumbers = false;
        }

        private void SetValueFromSlider(int index, float value)
        {
            serializedColor[index] = value;
            SetValue(serializedColor.ToColor());
        }

        protected override void SetValueCore(Color value)
        {
            this.value = value;
            serializedColor = new SerializedColor(value);
            OnValueChanged?.Invoke(value);
        }

        protected override void RefreshElementValueCore()
        {
            if (sliders == null)
                return;

            for(int i=0;i<sliders.Length;i++)
            {
                if (sliders[i] == null)
                    return;

                sliders[i].SetValueWithoutNotify(serializedColor[i]);
            }

            hexInput.SetTextWithoutNotify(ToHex(serializedColor.ToColor()));

            UpdateColorDisplayColor(serializedColor.ToColor());
        }
    }

    [Serializable]
    public struct SerializedColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public SerializedColor(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public Color ToColor()
        {
            return new Color(r, g, b, a);
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return r;
                    case 1:
                        return g;
                    case 2:
                        return b;
                    case 3:
                        return a;
                    default:
                        throw new IndexOutOfRangeException("Index is out of range of SerializedColor's values.");
                }
            }

            set
            {
                switch (index)
                {
                    case 0:
                        r = value;
                        break;
                    case 1:
                        g = value;
                        break;
                    case 2:
                        b = value;
                        break;
                    case 3:
                        a = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Index is out of range of SerializedColor's values.");
                }
            }
        }
    }
}
