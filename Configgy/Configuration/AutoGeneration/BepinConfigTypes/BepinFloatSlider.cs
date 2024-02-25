using BepInEx.Configuration;

namespace Configgy.Configuration.AutoGeneration
{
    internal class BepinFloatSlider : FloatSlider
    {
        private ConfigEntry<float> entry;
        public BepinFloatSlider(ConfigEntry<float> entry, AcceptableValueRange<float> range) : base(entry.GetDefault(), range.MinValue, range.MaxValue)
        {
            this.entry = entry;
        }

        protected override void LoadValueCore()
        {
            firstLoadDone = true;
            //Do nothing.
        }

        protected override float GetValueCore()
        {
            return entry.Value;
        }

        protected override void SetValueCore(float value)
        {
            entry.Value = value;
            OnValueChanged?.Invoke(value);
        }

        protected override void SaveValueCore()
        {
            //do nothing.
            IsDirty = false;
        }
    }
}
