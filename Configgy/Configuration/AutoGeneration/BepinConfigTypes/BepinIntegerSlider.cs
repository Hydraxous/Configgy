using BepInEx.Configuration;

namespace Configgy.Configuration.AutoGeneration
{
    internal class BepinIntegerSlider : IntegerSlider
    {
        private ConfigEntry<int> entry;
        public BepinIntegerSlider(ConfigEntry<int> entry, AcceptableValueRange<int> range) : base(entry.GetDefault(), range.MinValue, range.MaxValue)
        {
            this.entry = entry;
        }

        protected override void LoadValueCore()
        {
            firstLoadDone = true;
            //Do nothing.
        }

        protected override int GetValueCore()
        {
            return entry.Value;
        }

        protected override void SetValueCore(int value)
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
