using BepInEx.Configuration;
using UnityEngine;

namespace Configgy.Configuration.AutoGeneration
{
    //Yes it just removes the serialization. It sucks. It works. I don't care.
    internal class BepinColor : ConfigColor
    {
        private ConfigEntry<Color> entry;
        public BepinColor(ConfigEntry<Color> entry) : base(entry.GetDefault())
        {
            this.entry = entry;
        }

        protected override void LoadValueCore()
        {
            firstLoadDone = true;
            //Do nothing.
        }

        protected override Color GetValueCore()
        {
            return entry.Value;
        }

        protected override void SetValueCore(Color value)
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
