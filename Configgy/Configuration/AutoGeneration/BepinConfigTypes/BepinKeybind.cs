using BepInEx.Configuration;
using UnityEngine;

namespace Configgy.Configuration.AutoGeneration
{
    //Yes it just removes the serialization. It sucks. It works. I don't care.
    //This class is utterly disgusting. Too bad! I'm not going to fix it.
    internal class BepinKeybind : ConfigKeybind
    {
        ConfigEntry<KeyboardShortcut> keyboardShortcut;
        ConfigEntry<KeyCode> keyCode;

        public BepinKeybind(ConfigEntry<KeyboardShortcut> entry) : base(entry.GetDefault().MainKey)
        {
            this.keyboardShortcut = entry;
        }

        public BepinKeybind(ConfigEntry<KeyCode> entry) : base(entry.GetDefault())
        {
            this.keyCode = entry;
        }

        protected override void LoadValueCore()
        {
            firstLoadDone = true;
            //Do nothing.
        }

        protected override KeyCode GetValueCore()
        {
            if(keyboardShortcut != null)
                return keyboardShortcut.Value.MainKey;

            return keyCode.Value;
        }

        protected override void SetValueCore(KeyCode key)
        {
            if(keyboardShortcut != null)
                keyboardShortcut.Value = new KeyboardShortcut(key);
            
            OnValueChanged?.Invoke(value);
        }

        protected override void SaveValueCore()
        {
            //do nothing.
            IsDirty = false;
        }
    }
}
