using UnityEngine;
using UnityEngine.UI;

namespace Configgy
{
    public class FloatSlider : ConfigSlider<float>
    {
        public FloatSlider(float defaultValue, float min, float max) : base(defaultValue, min, max) {}

        protected override void BuildElementCore(RectTransform rect)
        {
            base.BuildElementCore(rect);
            instancedSlider.wholeNumbers = false;
            OnValueChanged += (v) => RefreshElementValue();
            RefreshElementValue();
        }

        protected override void ConfigureSliderRange(Slider slider)
        {
            slider.minValue = Min;
            slider.maxValue = Max;
        }

        protected override void LoadValueCore()
        {
            base.LoadValueCore();
            RefreshElementValue();
        }

        protected override void SetValueFromSlider(float value)
        {
            SetValue(value);
        }

        protected override void RefreshElementValueCore()
        {
            base.RefreshElementValueCore();
            if (instancedSlider == null)
                return;

            instancedSlider.SetValueWithoutNotify(GetValue());
        }
    }
}
