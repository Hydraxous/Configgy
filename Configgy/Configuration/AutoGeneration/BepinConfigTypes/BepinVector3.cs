using BepInEx.Configuration;
using System;
using UnityEngine;

namespace Configgy.Configuration.AutoGeneration
{
    //Yes it just removes the serialization. It sucks. It works. I don't care.
    internal class BepinVector3 : ConfigVector3
    {
        private ConfigEntry<Vector3> entry;
        public BepinVector3(ConfigEntry<Vector3> entry, Func<Vector3, bool> inputValidator = null) : base(entry.GetDefault(), inputValidator)
        {
            this.entry = entry;
        }

        protected override void LoadValueCore()
        {
            firstLoadDone = true;
            //Do nothing.
        }

        protected override Vector3 GetValueCore()
        {
            return entry.Value;
        }

        protected override void SetValueCore(Vector3 value)
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
