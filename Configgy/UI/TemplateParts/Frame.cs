using UnityEngine;
using UnityEngine.UI;

namespace Configgy.UI.Template
{
    public class Frame : MonoBehaviour
    {
        [SerializeField] private Image border;
        [SerializeField] private Image background;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private RectTransform content;

        public virtual RectTransform Content => content;
        public virtual RectTransform RectTransform => rectTransform;

        public virtual void SetBorderColor(Color color)
        {
            border.color = color;
        }

        public virtual void SetBackgroundColor(Color color) 
        {
            background.color = color;
        }

        public virtual void SetInnerSpacing(Vector2 inset)
        {
            content.offsetMin = inset;
            content.offsetMax = -inset;
        }
    }
}
