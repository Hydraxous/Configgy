using Configgy.UI.Template;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Configgy.UI.ProgrammaticUI
{
    public class UKFrame : Frame, IUIElement
    {
        private Frame _frame;
        public override RectTransform RectTransform => _frame.RectTransform;
        public override RectTransform Content => _frame.Content;

        public UKFrame(RectTransform rect)
        {
            DynUI.Frame(rect, (f) =>
            {
                _frame = f;
            });
        }

        public override void SetBorderColor(Color color) => _frame.SetBorderColor(color);
        public override void SetBackgroundColor(Color color) => _frame.SetBackgroundColor(color);
        public void SetVisible(bool visible) => _frame.gameObject.SetActive(visible);
    }
}
