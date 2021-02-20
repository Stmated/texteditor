using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Eliason.TextEditor.Native;
using Eliason.Common;

namespace Eliason.TextEditor.TextView
{
    public class Caret : ICaret
    {
        private static readonly int staticCaretBlinkTime = SystemInformation.CaretBlinkTime;
        private SafeHandleGDI _caretBrush;
        private readonly System.Threading.Timer _caretTimer;
        private readonly ITextView _textView;

        public bool IsShown { get; set; }

        public bool IsInView { get; set; }

        public int Index { get; set; }

        public Point Location { get; set; }

        public Caret(ITextView textView)
        {
            this._textView = textView;

            var c = Color.FromArgb(
                (byte) (255 - this._textView.BackColor.R),
                (byte) (255 - this._textView.BackColor.G),
                (byte) (255 - this._textView.BackColor.B));

            this._caretBrush = new SafeHandleGDI(SafeNativeMethods.CreateSolidBrush(ColorTranslator.ToWin32(c)));

            this._caretTimer = new System.Threading.Timer(this.caretTimer_Tick, null, staticCaretBlinkTime, Timeout.Infinite);
        }

        public void Render(IntPtr hdc)
        {
            this.Render(hdc, this.Location.X, this.Location.Y);
        }

        public void Render(IntPtr hdc, int x, int y)
        {
            if (this.IsInView == false || this.IsShown == false)
            {
                return;
            }

            var rect = new RECT
            {
                left = x,
                top = y,
                right = x + SystemInformation.CaretWidth,
                bottom = y + this._textView.LineHeight
            };

            SafeNativeMethods.FillRect(hdc, ref rect, this._caretBrush.DangerousGetHandle());
        }

        private void caretTimer_Tick(object sender)
        {
            this.IsShown = !this.IsShown;
            this._textView.Invalidate();

            this._caretTimer.Change(staticCaretBlinkTime, Timeout.Infinite);
        }

        public void ResetBlink()
        {
            this.IsShown = this._textView.Focused;
            this._caretTimer.Change(staticCaretBlinkTime, Timeout.Infinite);
            this._textView.Invalidate();
        }

        public void Dispose()
        {
            if (this._caretTimer != null)
            {
                this._caretTimer.Dispose();
            }

            if (this._caretBrush != null)
            {
                this._caretBrush.Dispose();
                this._caretBrush = null;
            }
        }
    }
}