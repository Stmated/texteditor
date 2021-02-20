using System;
using System.Drawing;

namespace Eliason.TextEditor
{
    public interface ICaret : IDisposable
    {
        Point Location { get; set; }

        bool IsShown { get; set; }

        bool IsInView { get; set; }

        int Index { get; set; }

        void Render(IntPtr hdc);

        void Render(IntPtr hdc, int x, int y);

        void ResetBlink();
    }
}