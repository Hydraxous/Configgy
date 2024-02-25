using BepInEx.Configuration;
using System;
using UnityEngine;

namespace Configgy.Configuration.AutoGeneration
{
    //Yes it just removes the serialization. It sucks. It works. I don't care.
    internal class BepinVector2 : ConfigVector2
    {
        private ConfigEntry<Vector2> entry;
        public BepinVector2(ConfigEntry<Vector2> entry, Func<Vector2, bool> inputValidator = null) : base(entry.GetDefault(), inputValidator)
        {
            this.entry = entry;
        }

        protected override void LoadValueCore()
        {
            firstLoadDone = true;
            //Do nothing.
        }

        protected override Vector2 GetValueCore()
        {
            return entry.Value;
        }

        protected override void SetValueCore(Vector2 value)
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
