using UnityEngine;

namespace Configgy
{
    /// <summary>
    /// Serializes values without a UI element
    /// </summary>
    /// <typeparam name="T">type to serialize</typeparam>
    public class ConfiggyPersistent<T> : ConfigValueElement<T>
    {
        public ConfiggyPersistent(T defaultValue) : base(defaultValue) {}
        protected override void BuildElementCore(RectTransform rect) {}
        protected override void RefreshElementValueCore() { }

        /// <summary>
        /// This will force the config to save immediately, instead of waiting for auto-save. Please use sparingly.
        /// </summary>
        public void ForceSave()
        {
            config.SaveData();
        }
    }
}
