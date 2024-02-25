using BepInEx.Configuration;

using System;
using System.Linq;

namespace Configgy.Configuration.AutoGeneration
{
    internal class BepinDropdown<T> : ConfigDropdown<T> where T : IEquatable<T>
    {
        protected ConfigEntry<T> entry;

        public BepinDropdown(ConfigEntry<T> entry, AcceptableValueList<T> values) : base(values.AcceptableValues, GetNames(entry, values), GetDefaultIndex(entry, values))
        {
            this.entry = entry;
            this.value = entry.Value;
        }

        private static string[] GetNames(ConfigEntry<T> entry, AcceptableValueList<T> values)
        {
            Func<T, string> getName = entry.GetTag<Func<T, string>>() ?? (val => val.ToString());
            return values.AcceptableValues.Select(getName).ToArray();
        }

        private static int GetDefaultIndex(ConfigEntry<T> entry, AcceptableValueList<T> values)
        {
            T defaultValue = entry.GetDefault();
            int i = Array.FindIndex(values.AcceptableValues, defaultValue.Equals);
            return i >= 0 ? i : 0;
        }

        protected override void LoadValueCore()
        {
            firstLoadDone = true;
            //Do nothing.
        }

        protected override T GetValueCore()
        {
            return entry.Value;
        }

        protected override void SetValueCore(T value)
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
