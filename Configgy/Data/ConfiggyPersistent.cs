using UnityEngine;

namespace Configgy
{
    /// <summary>
    /// Serializes values without a UI element
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConfiggyPersistent<T> : ConfigValueElement<T>
    {
        public ConfiggyPersistent(T defaultValue) : base(defaultValue) {}
        protected override void BuildElementCore(RectTransform rect) {}
        protected override void RefreshElementValueCore() {}
    }
}
