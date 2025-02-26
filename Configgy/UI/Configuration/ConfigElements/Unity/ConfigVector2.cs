using System;
using System.Linq;
using UnityEngine;

namespace Configgy
{
    public class ConfigVector2 : ConfigInputField<Vector2>
    {
        private float[] serializedVector2;

        public ConfigVector2(Vector2 defaultValue, Func<Vector2, bool> inputValidator = null) : base(defaultValue, inputValidator, null)
        {
            serializedVector2 = new float[] { defaultValue.x, defaultValue.y };
            valueConverter = (v) =>
            {
                //Simple zero shortcut
                if (v == "0")
                    return (true, Vector2.zero);

                //Parse the float
                string[] values = v.Split(',');
                if (values.Length != 2)
                    return (false, Vector2.zero);

                float[] floats = values.Select(x => float.Parse(x)).ToArray();
                return (true, new Vector2(floats[0], floats[1]));
            };

            toStringOverride = (v) =>
            {
                return $"{v.x},{v.y}";
            };
        }

        //We need to load and save in this way because Unity made Vector3 values contain themselves and
        //it causes a stack overflow when serializing with NewtonSoft. Thanks Unity.
        protected override void LoadValueCore()
        {
            firstLoadDone = true;

            if (config.TryGetValueAtAddress<float[]>(descriptor.SerializationAddress, out float[] vec2))
            {
                try
                {
                    SetValue(new Vector2(vec2[0], vec2[1]));
                    return;
                }
                catch (Exception ex)
                {
                    ConfiggyPlugin.Log.LogException(ex);
                }
            }

            ResetValue();
        }

        protected override void SetValueCore(Vector2 value)
        {
            this.value = value;
            serializedVector2 ??= new float[2];
            serializedVector2[0] = value.x;
            serializedVector2[1] = value.y;
            OnValueChanged?.Invoke(value);
        }

        protected override void SaveValueCore()
        {
            serializedVector2 ??= new float[2];
            object obj = serializedVector2;
            config.SetValueAtAddress(descriptor.SerializationAddress, obj);
            config.SaveDeferred();
            IsDirty = false;
        }
    }
}
