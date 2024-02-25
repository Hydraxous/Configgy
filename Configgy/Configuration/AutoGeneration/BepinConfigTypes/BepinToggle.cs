using BepInEx.Configuration;

namespace Configgy.Configuration.AutoGeneration
{
    //Yes it just removes the serialization. It sucks. It works. I don't care.
    internal class BepinToggle : ConfigToggle
    {
        private ConfigEntry<bool> entry;
        public BepinToggle(ConfigEntry<bool> entry) : base(entry.GetDefault())
        {
            this.entry = entry;
        }

        protected override void LoadValueCore()
        {
            firstLoadDone = true;
            //Do nothing.
        }

        protected override bool GetValueCore()
        {
            return entry.Value;
        }

        protected override void SetValueCore(bool value)
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
