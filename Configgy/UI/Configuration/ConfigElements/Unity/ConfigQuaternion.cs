using System;
using System.Linq;
using UnityEngine;

namespace Configgy
{
    public class ConfigQuaternion : ConfigInputField<Quaternion>
    {
        private float[] serializedQuaternion;

        public ConfigQuaternion(Quaternion defaultValue, Func<Quaternion, bool> inputValidator = null) : base(defaultValue, inputValidator, null)
        {
            serializedQuaternion = new float[] { defaultValue.x, defaultValue.y, defaultValue.z, defaultValue.w };
            valueConverter = (v) =>
            {
                //Simple zero shortcut
                if (v == "0")
                    return (true, Quaternion.identity);

                if(v.ToLower() == "identity")
                    return (true, Quaternion.identity);

                //Parse the floats
                string[] values = v.Split(',');
                if (values.Length != 3)
                    return (false, Quaternion.identity);

                float[] floats = values.Select(x => float.Parse(x)).ToArray();
                return (true, Quaternion.Euler(new Vector3(floats[0], floats[1], floats[2])));
            };

            toStringOverride = (v) =>
            {
                return $"{v.eulerAngles.x},{v.eulerAngles.y},{v.eulerAngles.z}";
            };
        }

        //We need to load and save in this way because Unity made Vector3 values contain themselves and
        //it causes a stack overflow when serializing with NewtonSoft. Thanks Unity.
        protected override void LoadValueCore()
        {
            firstLoadDone = true;

            if (config.TryGetValueAtAddress<float[]>(descriptor.SerializationAddress, out float[] quaternion))
            {
                try
                {
                    SetValue(new Quaternion(quaternion[0], quaternion[1], quaternion[2], quaternion[3]));
                    return;
                }
                catch (Exception ex)
                {
                    ConfiggyPlugin.Log.LogException(ex);
                }
            }

            ResetValue();
        }

        protected override void SetValueCore(Quaternion value)
        {
            this.value = value;
            serializedQuaternion ??= new float[2];
            serializedQuaternion[0] = value.x;
            serializedQuaternion[1] = value.y;
            serializedQuaternion[2] = value.z;
            serializedQuaternion[3] = value.w;
            OnValueChanged?.Invoke(value);
        }

        protected override void SaveValueCore()
        {
            serializedQuaternion ??= new float[2];
            object obj = serializedQuaternion;
            config.SetValueAtAddress(descriptor.SerializationAddress, obj);
            config.SaveDeferred();
            IsDirty = false;
        }
    }
}
