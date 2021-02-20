using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Eliason.Common;

namespace Eliason.TextEditor
{
    /// <summary>
    /// TODO: Get rid of as much as possible from here! It should be lightweight!
    /// </summary>
    public interface IScrollHost
    {
        void Attach();
        void Detach();

        void OnContentSizeChanged(int width, int height);

        int ScrollPosH { get; }

        int ScrollPosVIntegral { get; }

        int HorizontalMax { get; }
        int VerticalMax { get; }

        bool IsScrollingHorizontally { get; }
        bool IsScrollingVertically { get; }

        event EventHandler<ValueChangedEventArgs> VerticalScrollChanged;
        event EventHandler<ValueChangedEventArgs> HorizontalScrollChanged;

        void ScrollToPoint(Point point, bool force = false, bool ignoreHorizontalMovement = false, ValueChangedBy cause = ValueChangedBy.Unspecified);
    }
}
