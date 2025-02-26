using Configgy.UI;
using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy
{
    public class ConfigInputField<T> : ConfigValueElement<T>
    {
        protected Func<T, bool> inputValidator;
        protected Func<string, ValueTuple<bool, T>> valueConverter;
        public Func<T, string> toStringOverride;

        public ConfigInputField(T defaultValue, Func<T, bool> inputValidator = null, Func<string, ValueTuple<bool, T>> typeConverter = null)  : base (defaultValue)
        {
            this.valueConverter = typeConverter ?? ValidateInputSyntax;
            this.inputValidator = inputValidator ?? ((v) => { return true; });
            this.toStringOverride = null;

            OnValueChanged += (_) => RefreshElementValue();
            RefreshElementValue();
        }

        protected InputField instancedField;

        private ValueTuple<bool, T> ValidateInputSyntax(string inputValue)
        {
            ValueTuple<bool, T> result = new ValueTuple<bool, T>();

            result.Item1 = false;
            result.Item2 = default(T);

            try
            {
                TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(T));
                object convertedValue = typeConverter.ConvertFromString(inputValue);
                result.Item2 = (T) convertedValue;
                result.Item1 = result.Item2 != null;
            }
            catch (Exception ex)
            {
                ConfiggyPlugin.Log.LogException(ex);
            }

            return result;
        }

        private void SetValueFromString(InputField source, string input)
        {
            if (source != instancedField) //prevent old non-null instance from calling this method.
                return;

            ValueTuple<bool, T> conversionResult;

            try
            {
                conversionResult = valueConverter.Invoke(input);
                if (!conversionResult.Item1)
                {
                    ConfiggyPlugin.Log.LogError("Syntax for field invalid! Conversion failed!");
                    RefreshElementValue();
                    return;
                }
            }
            catch (System.Exception ex)
            {
                ConfiggyPlugin.Log.LogException(ex);
                RefreshElementValue();
                return;
            }

            if(!inputValidator.Invoke(conversionResult.Item2))
            {
                ConfiggyPlugin.Log.LogError("Value validation failure. Rejected.");
                RefreshElementValue();
                return;
            }

            base.SetValue(conversionResult.Item2);
        }

        protected void SetInputField(InputField inputField)
        {
            inputField.onEndEdit.AddListener((s) => SetValueFromString(inputField, s));
            instancedField = inputField;
            RefreshElementValue();
        }

        protected override void RefreshElementValueCore()
        {
            if (instancedField == null)
                return;

            T value = GetValue();

            string valueString = null;
            if (toStringOverride != null)
                valueString = toStringOverride.Invoke(value);
            else
                valueString = value.ToString();

            instancedField.SetTextWithoutNotify(valueString);
        }

        protected override void BuildElementCore(RectTransform rect)
        {
            DynUI.ConfigUI.CreateElementSlot(rect, this, (r) =>
            {
                DynUI.InputField(r, SetInputField);
            },
            null);
        }
    }
}
