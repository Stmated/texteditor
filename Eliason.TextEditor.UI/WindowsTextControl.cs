using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eliason.TextEditor.UI
{
    /// <summary>
    /// Implementation of TextView that uses a regular Windows scrollbar instead of the custom-painted one.
    /// 
    /// TODO: Inherit from Control
    /// TODO: Add Windows scrollbars
    /// TODO: Implement IScrollHost on itself, and make it interface with the Windows scrolling
    /// </summary>
    public class WindowsTextControl : TextView.TextView
    {
        public WindowsTextControl(ITextDocument textDocument, ISettings settings, IScrollHost scrollHost)
            : base(textDocument, settings, scrollHost)
        {
        }
    }
}
