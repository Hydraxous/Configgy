using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Configgy.Configuration.AutoGeneration.BepinConfigTypes
{
    internal class BepinEnumDropdown : ConfigDropdown<int>
    {
        ConfigEntryBase entry;

        public BepinEnumDropdown(ConfigEntryBase entry, int value, int currentIndex, int[] values, string[] names = null, int defaultIndex=0) : base(values, names, defaultIndex)
        {
            this.value = value;
            this.entry = entry;
            this.currentIndex = currentIndex;
        }

        protected override void LoadValueCore()
        {
            firstLoadDone = true;
            SetValue(value);
            //Do nothing.
        }

        protected override int GetValueCore()
        {
            return (int) entry.BoxedValue;
        }

        protected override void SetValueCore(int value)
        {
            entry.BoxedValue = value;
            OnValueChanged?.Invoke(value);
        }

        protected override void SaveValueCore()
        {
            //do nothing.
            IsDirty = false;
        }
    }
}
