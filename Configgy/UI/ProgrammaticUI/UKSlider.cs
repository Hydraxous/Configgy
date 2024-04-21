using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy.UI.ProgrammaticUI
{
    public class UKSlider : IUIElement
    {
        private Slider _slider;
        public Slider SliderComponent => _slider;

        public UKSlider(RectTransform rt, float value, float min, float max, bool isInteger)
        {
            DynUI.Slider(rt, (s) =>
            {
                _slider = s;
                _slider.value = value;
                _slider.minValue = min;
                _slider.maxValue = max;
                _slider.wholeNumbers = isInteger;
                _slider.onValueChanged.AddListener(OnValueChangedCallback);
            });
        }

        protected virtual void OnValueChangedCallback(float value) {}

        public void SetVisible(bool visible) => throw new NotImplementedException();
    }

    public class UKFloatSlider : UKSlider
    {
        public UKFloatSlider(RectTransform rt, float value, int min, int max, Action<int> onValueChanged = null) : base(rt, value, min, max, false) { }

        protected override void OnValueChangedCallback(float value)
        {
            OnValueChanged?.Invoke(value);
        }

        public event Action<float> OnValueChanged;
    }
}
