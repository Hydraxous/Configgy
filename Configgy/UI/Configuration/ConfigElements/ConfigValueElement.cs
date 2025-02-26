using System;
using System.Reflection;

using UnityEngine;
using UnityEngine.Assertions;


namespace Configgy
{
    public abstract class ConfigValueElement : IConfigElement
    {
        protected ConfiggableAttribute descriptor;
        protected ConfigBuilder config;

        private bool initialized => descriptor != null;
        protected bool firstLoadDone = false;
        public bool IsDirty { get; protected set; }

        public abstract Type ConfigValueType { get; }

        protected abstract void BuildElementCore(RectTransform rect);

        internal abstract void BindField(FieldInfo field);

        protected abstract void LoadValueCore();
        protected abstract void SaveValueCore();
        protected abstract void ResetValueCore();
        protected abstract void RefreshElementValueCore();

        public void LoadValue()
        {
            if (!initialized)
                return;

            LoadValueCore();
            firstLoadDone = true; //just to be safe.
        }

        public void SaveValue() => SaveValueCore();
        public void ResetValue() => ResetValueCore();
        public void RefreshElementValue() => RefreshElementValueCore();

        #region Implement IConfigElement

        public void BuildElement(RectTransform rect)
        {
            if (!initialized)
                return;

            BuildElementCore(rect);
        }

        public void OnMenuOpen()
        {
            RefreshElementValue();
        }

        public void OnMenuClose()
        {
            if (IsDirty)
                SaveValue();
        }

        void IConfigElement.BindDescriptor(ConfiggableAttribute configgable) => this.descriptor = configgable;
        ConfiggableAttribute IConfigElement.GetDescriptor() => descriptor;
        void IConfigElement.BindConfig(ConfigBuilder config) => this.config = config;

        #endregion

        public static ConfigValueElement<T> Create<T>(T defaultValue)
        {
            ConfigValueElement element = defaultValue switch
            {
                bool v => new ConfigToggle(v),
                Vector2 v => new ConfigVector2(v),
                Vector3 v => new ConfigVector3(v),
                Quaternion v => new ConfigQuaternion(v),
                Color v => new ConfigColor(v),
                KeyCode v => new ConfigKeybind(v),

                Enum => ConfigDropdown<T>.ForEnum(defaultValue),

                sbyte or short or int or long
                or byte or ushort or uint or ulong
                or float or double or char or string
                => new ConfigInputField<T>(defaultValue),

                _ => null,
            };

            if (element is null)
                return null;

            return (ConfigValueElement<T>)element;
        }
    }

    public abstract class ConfigValueElement<T> : ConfigValueElement
    {
        public sealed override Type ConfigValueType => typeof(T);

        protected readonly T defaultValue;
        protected internal T? value;

        public Action<T> OnValueChanged;

        public ConfigValueElement(T defaultValue)
        {
            this.defaultValue = defaultValue;
        }

        protected override void LoadValueCore()
        {
            //Get value from data manager.
            //This should probably be changed to something more reliable and not static.

            firstLoadDone = true; //nullable values apparently can just randomly have values so this annoying bool is needed
            
            //For dynamically created elements, the descriptor and config may not be set yet.
            if (descriptor == null || config == null)
                return;

            if (config.TryGetValueAtAddress<T>(descriptor.SerializationAddress, out T value))
            {
                try
                {
                    SetValue(value);
                    return;
                }
                catch (Exception ex)
                {
                    ConfiggyPlugin.Log.LogException(ex);
                }
            }

            ResetValue();
            //SaveValue(); TODO idk check this.
        }

        protected override void SaveValueCore()
        {
            object obj = GetValue();
            config.SetValueAtAddress(descriptor.SerializationAddress, obj);
            config.SaveDeferred();
            IsDirty = false;
        }

        public virtual T DefaultValue => defaultValue;

        public T Value => GetValue();

        public T GetValue() => GetValueCore();

        protected virtual T GetValueCore()
        {
            if (value == null || !firstLoadDone)
            {
                LoadValue();
            }

            return value;
        }

        public void SetValue(T value)
        {
            SetValueCore(value);
            IsDirty = true;
        }

        protected virtual void SetValueCore(T value)
        {
            this.value = value;
            OnValueChanged?.Invoke(value);
        }

        protected override void ResetValueCore()
        {
            SetValue(DefaultValue);
        }

        public override string ToString() => GetValue().ToString();

        internal sealed override void BindField(FieldInfo field)
        {
            Assert.AreEqual(expected: typeof(T), actual: field.FieldType);
            OnValueChanged += v => field.SetValue(null, v);
        }
    }
}
