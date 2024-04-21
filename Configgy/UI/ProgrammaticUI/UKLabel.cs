using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy.UI.ProgrammaticUI
{
    public class UKLabel
    {
        private Text _text;
        public Text TextComponent => _text;
        public UKLabel(RectTransform rect, string text)
        {
            DynUI.Label(rect, (l) =>
            {
                _text = l;
            });
        }

        public string Text
        {
            get => _text.text; set => _text.text = value;
        }
    }
}
