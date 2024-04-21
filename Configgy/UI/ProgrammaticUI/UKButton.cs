using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy.UI.ProgrammaticUI
{
    public class UKButton : IUIElement
    {
        private Button _button;
        public Button ButtonComponent => _button;
        
        private Text _label;
        public Text TextComponet => _label;

        public event Action OnPressed;

        private void OnButtonPressCallback()
        {
            OnPressed?.Invoke();
        }

        public UKButton(RectTransform rt, string label, Action onPress = null)
        {
            DynUI.Button(rt, (b) =>
            {
                _button = b;
                _label = b.GetComponentInChildren<Text>();
                _label.text = label;
                _button.onClick.AddListener(OnButtonPressCallback);
            });

            OnPressed += onPress;
        }

        public void SetInteractable(bool interactable)
        {
            _button.interactable = interactable;
        }

        public void SetVisible(bool visible)
        {
            _button.gameObject.SetActive(visible);
        }
    }
}
