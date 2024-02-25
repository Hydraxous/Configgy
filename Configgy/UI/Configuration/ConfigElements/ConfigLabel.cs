using Configgy.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy
{
    public class ConfigLabel : IConfigElement
    {
        private ConfiggableAttribute descriptor;

        private Text label;
        private string labelText;
        private float height;

        public ConfigLabel(string label, float height = 55f)
        {
            this.labelText = label;
            this.height = 55f;
        }

        public void SetHeight(float height)
        {
            this.height = height;

            if (label)
                label.GetComponent<RectTransform>().sizeDelta = new Vector2(0, height);
        }

        public Text GetLabel()
        {
            return label;
        }

        public string GetCurrentText()
        {
            return labelText;
        }

        public void SetText(string label)
        {
            this.labelText = label;

            if (this.label)
                this.label.text = label;
        }

        void IConfigElement.BindConfig(ConfigBuilder configBuilder)
        {
        }

        void IConfigElement.BindDescriptor(ConfiggableAttribute configgable)
        {
            descriptor = configgable;
        }

        void IConfigElement.BuildElement(RectTransform rect)
        {
            DynUI.Label(rect, (t) =>
            {
                label = t;
                label.text = labelText;
                RectTransform rt = label.GetComponent<RectTransform>();
                DynUI.Layout.CenterAnchor(rt);
                rt.sizeDelta = new Vector2(0, height);
            });

        }

        ConfiggableAttribute IConfigElement.GetDescriptor()
        {
            return descriptor;
        }

        void IConfigElement.OnMenuClose()
        {
        }

        void IConfigElement.OnMenuOpen()
        {
        }
    }
}
