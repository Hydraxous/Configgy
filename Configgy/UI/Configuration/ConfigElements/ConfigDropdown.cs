using Configgy.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy
{
    public class ConfigDropdown<T> : ConfigValueElement<T>
    {
        public string[] Names { get; private set; }
        public T[] Values { get; private set; }

        protected int defaultIndex;
        protected int? currentIndex;

        public ConfigDropdown(T[] values, string[] names = null, int defaultIndex = 0) : base(values[defaultIndex])
        {
            this.Values = values;
            this.Names = CreateNames(values, names);
            this.defaultIndex = defaultIndex;
            OnValueChanged += (_) => RefreshElementValue();
            RefreshElementValue();
        }

        private string[] CreateNames(T[] values, string[] providedNames)
        {
            //If the provided names are null, we just use the ToString method of the values.
            if (providedNames == null)
                return values.Select(x => x.ToString()).ToArray();

            string[] names = new string[values.Length];

            //If the provided names are less than the values, we use the provided names and then ToString for the rest.
            for (int i = 0; i < values.Length; i++)
                names[i] = (i < providedNames.Length) ? providedNames[i] : values[i].ToString();

            return names;
        }

        public void SetOptions(T[] values, string[] names = null, int newIndex = 0, int defaultIndex = 0)
        {
            int valueLen = values.Length;

            if (defaultIndex >= valueLen || defaultIndex < 0)
                throw new IndexOutOfRangeException("Default index is out of bounds with provided values.");

            if (newIndex >= valueLen || newIndex < 0)
                throw new IndexOutOfRangeException("Default index is out of bounds with provided values.");

            this.Values = values;
            this.Names = CreateNames(values, names);
            this.defaultIndex = defaultIndex;
            SetIndex(newIndex);
        }

        protected Dropdown instancedDropdown;

        protected override void LoadValueCore()
        {
            //Get value from data manager.
            firstLoadDone = true;

            if (config.TryGetValueAtAddress<int>(descriptor.SerializationAddress, out int value))
            {
                try
                {
                    SetIndexCore(value);
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            ResetValue();
            SaveValue();
        }

        protected override void SaveValueCore()
        {
            object obj = currentIndex; //We dont serialize the value, we serialize the index.
            config.SetValueAtAddress(descriptor.SerializationAddress, obj);
            config.SaveDeferred();
            IsDirty = false;
        }

        protected override void ResetValueCore()
        {
            SetIndexCore(defaultIndex);
        }

        protected override T GetValueCore()
        {
            return Values[GetCurrentIndex()];
        }

        public string GetSelectedIndexName()
        {
            return Names[GetCurrentIndex()];
        }

        public int GetCurrentIndex()
        {
            return GetCurrentIndexCore();
        }

        protected virtual int GetCurrentIndexCore()
        {
            if(currentIndex == null)
            {
                LoadValue();
            }

            return currentIndex.Value;
        }

        protected void SetDropdown(Dropdown dropdown)
        {
            dropdown.onValueChanged.AddListener((v) => SetValueFromDropdown(dropdown, v));
            instancedDropdown = dropdown;
            RefreshElementValue();
        }

        protected void SetValueFromDropdown(Dropdown source, int newValue)
        {
            if (source != instancedDropdown)
                return;

            SetIndex(newValue);
        }

        protected void RefreshValue()
        {
            SetValue(Values[GetCurrentIndex()]);
        }

        protected override void SetValueCore(T value)
        {
            if(!Values.Contains(value))
                throw new KeyNotFoundException("Unable to set Dropdown's value directly. It is not contained within the values. Use SetValuesAndNames and SetIndex.");

            base.SetValueCore(value);
        }

        public void SetIndex(int index)
        {
            if (index >= Values.Length || index < 0)
                throw new IndexOutOfRangeException("Index is out of range of Dropdown's values.");

            SetIndexCore(index);
        }

        protected virtual void SetIndexCore(int index)
        {
            currentIndex = index;
            RefreshValue();
        }

        protected override void BuildElementCore(ConfiggableAttribute configgable, RectTransform rect)
        {
            DynUI.ConfigUI.CreateElementSlot(rect, this, (r) =>
            {
                DynUI.Dropdown(r, SetDropdown);
            },
            null);
        }

        protected override void RefreshElementValueCore()
        {
            if (instancedDropdown == null)
                return;

            instancedDropdown.ClearOptions();
            instancedDropdown.options = Names.Select(x => new Dropdown.OptionData(x)).ToList();
            instancedDropdown.SetValueWithoutNotify(GetCurrentIndex());
            instancedDropdown.RefreshShownValue();
        }
    }
}
