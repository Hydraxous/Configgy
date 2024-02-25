using BepInEx.Configuration;
using System;
using UnityEngine;

namespace Configgy.Configuration.AutoGeneration
{
    //Yes it just removes the serialization. It sucks. It works. I don't care.
    internal class BepinQuaternion : ConfigQuaternion
    {
        private ConfigEntry<Quaternion> entry;
        public BepinQuaternion(ConfigEntry<Quaternion> entry, Func<Quaternion, bool> inputValidator = null) : base(entry.GetDefault(), inputValidator)
        {
            this.entry = entry;
        }

        protected override void LoadValueCore()
        {
            firstLoadDone = true;
            //Do nothing.
        }

        protected override Quaternion GetValueCore()
        {
            return entry.Value;
        }

        protected override void SetValueCore(Quaternion value)
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
