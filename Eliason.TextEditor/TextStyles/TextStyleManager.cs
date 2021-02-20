using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eliason.TextEditor.TextStyles
{
    public class TextStyleManager
    {
        public Dictionary<String, TextStyleBase> Styles { get; private set; }

        public TextStyleManager()
        {
            this.Styles = new Dictionary<string, TextStyleBase>();
        }
    }
}
