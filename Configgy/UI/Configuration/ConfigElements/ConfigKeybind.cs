using Configgy.UI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy
{
    public class ConfigKeybind : ConfigValueElement<KeyCode>
    {
        public ConfigKeybind(KeyCode keyCode) : base(keyCode)
        {
            OnValueChanged += (_) => RefreshElementValue();
            RefreshElementValue();
        }

        private Text keybindText;
        private Button keybindButton;
        private ConfigurationPage page;
        public bool IsBeingRebound { get; private set; }

        protected override void BuildElementCore(RectTransform rect)
        {
            IsBeingRebound = false;
            page = rect.GetComponentInParent<ConfigurationPage>();

            DynUI.ConfigUI.CreateElementSlot(rect, this, (r) =>
            {
                DynUI.Button(r, (b) =>
                {
                    SetKeybindButton(b);
                });
            });
        }

        public event Action OnRebindStart;
        public event Action OnRebindEnd;

        private void SetKeybindButton(Button button)
        {
            keybindButton = button;
            keybindText = button.GetComponentInChildren<Text>();
            keybindButton.onClick.AddListener(StartRebinding);
        }

        private void StartRebinding()
        {
            if(page == null)
            {
                page = keybindButton.GetComponentInParent<ConfigurationPage>();
            }

            if(page == null)
            {
                ConfiggyPlugin.Log.LogError("Page could not be found?");
                return;
            }

            keybindText.text = "<color=orange>???</color>";
            keybindButton.interactable = false;

            IsBeingRebound = true;

            page.preventClosing = true;
            page.StartCoroutine(RebindProcess());
            OnRebindStart?.Invoke();
        }

        public bool IsPressed()
        {
            if (IsBeingRebound)
                return false;

            if (Value == KeyCode.None)
                return false;

            return Input.GetKey(Value);
        }

        public bool WasPeformed()
        {
            if (IsBeingRebound)
                return false;

            if (Value == KeyCode.None)
                return false;

            return Input.GetKeyDown(Value);
        }

        public bool WasReleased()
        {
            if (IsBeingRebound)
                return false;

            if (Value == KeyCode.None)
                return false;

            return Input.GetKeyUp(Value);
        }

        protected override void RefreshElementValueCore()
        {
            if (keybindText == null)
                return;

            keybindText.text = GetValue().ToString();
        }

        private void FinishRebind(KeyCode keycode)
        {
            keybindButton.interactable = true;
            page.preventClosing = false;

            IsBeingRebound = false;

            SetValue(keycode);
            OnRebindEnd?.Invoke();
        }

        private IEnumerator RebindProcess()
        {
            float timer = 10f;
            IsBeingRebound = true;
            KeyCode currentKeyCode = Value;
            int counter = 0;
            yield return new WaitForSecondsRealtime(0.18f);
            while (IsBeingRebound && timer > 0f)
            {
                yield return null;
                timer -= Time.deltaTime;
                Event current = Event.current;
                if (current.type == EventType.KeyDown || current.type == EventType.KeyUp)
                {
                    switch (current.keyCode)
                    {
                        case KeyCode.Escape:
                            FinishRebind(KeyCode.None);
                            yield break;
                        default:
                            FinishRebind(current.keyCode);
                            yield break;
                        case KeyCode.None:
                            break;
                    }

                    counter++;
                }
            }

            FinishRebind(currentKeyCode);
        }
    }
}
