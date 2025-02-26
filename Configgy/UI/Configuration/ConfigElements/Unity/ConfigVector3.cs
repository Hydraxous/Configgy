using System;
using System.Linq;
using UnityEngine;

namespace Configgy
{
    public class ConfigVector3 : ConfigInputField<Vector3>
    {
        private float[] serializedVector3;

        public ConfigVector3(Vector3 defaultValue, Func<Vector3, bool> inputValidator = null) : base(defaultValue, inputValidator, null)
        {
            serializedVector3 = new float[] { defaultValue.x, defaultValue.y, defaultValue.z };
            valueConverter = (v) =>
            {
                //Simple zero shortcut
                if (v == "0")
                    return (true, Vector3.zero);

                //Parse the floats
                string[] values = v.Split(',');
                if (values.Length != 3)
                    return (false, Vector3.zero);

                float[] floats = values.Select(x => float.Parse(x)).ToArray();
                return (true, new Vector3(floats[0], floats[1], floats[2]));
            };

            toStringOverride = (v) =>
            {
                return $"{v.x},{v.y},{v.z}";
            };
        }

        //We need to load and save in this way because Unity made Vector3 values contain themselves and
        //it causes a stack overflow when serializing with NewtonSoft. Thanks Unity.
        protected override void LoadValueCore()
        {
            firstLoadDone = true;

            if (config.TryGetValueAtAddress<float[]>(descriptor.SerializationAddress, out float[] vec3))
            {
                try
                {
                    SetValue(new Vector3(vec3[0], vec3[1], vec3[1]));
                    return;
                }
                catch (Exception ex)
                {
                    ConfiggyPlugin.Log.LogException(ex);
                }
            }

            ResetValue();
        }

        protected override void SetValueCore(Vector3 value)
        {
            this.value = value;
            serializedVector3 ??= new float[3];
            serializedVector3[0] = value.x;
            serializedVector3[1] = value.y;
            serializedVector3[2] = value.z;
            OnValueChanged?.Invoke(value);
        }

        protected override void SaveValueCore()
        {
            serializedVector3 ??= new float[3];
            object obj = serializedVector3;
            config.SetValueAtAddress(descriptor.SerializationAddress, obj);
            config.SaveDeferred();
            IsDirty = false;
        }
    }
}
