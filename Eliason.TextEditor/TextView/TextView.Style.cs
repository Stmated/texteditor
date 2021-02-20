using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eliason.TextEditor.TextStyles;

namespace Eliason.TextEditor.TextView
{
    public partial class TextView
    {
        private readonly List<TextStyleBase> _textStyles = new List<TextStyleBase>();

        public void AddStyle(TextStyleBase anchorStyle)
        {
            this._textStyles.Add(anchorStyle);
        }

        public IEnumerable<TextStyleBase> GetTextStyles()
        {
            foreach (var style in this.Settings.GetTextStyles())
            {
                yield return style;
            }

            foreach (var style in this._textStyles)
            {
                yield return style;
            }
        }

        public TextStyleBase GetTextStyle(string typeKey)
        {
            return this.Settings.GetTextStyle(typeKey) ?? this._textStyles.FirstOrDefault(anchorStyle => anchorStyle.NameKey == typeKey);
        }
    }
}
