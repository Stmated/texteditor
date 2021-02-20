using System;
using System.Drawing;
using System.Windows.Forms;
using Eliason.TextEditor.Extensions;
using Eliason.TextEditor.Native;
using Eliason.TextEditor.TextDocument.ByLines;
using Eliason.Common;

namespace Eliason.TextEditor.TextView
{
    public class TextColumnLineNumber : TextColumnBase
    {
        public override string Name
        {
            get { return "Line numbers"; }
        }

        public override string Key
        {
            get { return "LineNumber"; }
        }

        public override bool FloatLeft
        {
            get { return true; }
        }

        public override Image GetImage(ITextView textView)
        {
            return textView.Settings.BitmapLineNumbers;
        }

        private SafeHandleGDI _handleBkBrush;
        private SafeHandleGDI _handleBkActiveBrush;
        private SafeHandleGDI _handlePen;
        private SafeHandleGDI _handleFont;
        private int _previousLength;

        private Cursor _previousCursor;

        public override unsafe void PaintLine(IntPtr hdc, RendererState rs)
        {
            if (this._handleFont == null)
            {
                using (var f = new Font(rs.TextView.Font.FontFamily, rs.TextView.Font.Size - 2))
                {
                    this._handleFont = new SafeHandleGDI(f.ToHfont());
                }

                this._handlePen = new SafeHandleGDI(SafeNativeMethods.CreatePen(NativeConstants.PS_SOLID, -1, ColorTranslator.ToWin32(Color.Silver)));
                this._handleBkBrush = new SafeHandleGDI(SafeNativeMethods.CreateSolidBrush(ColorTranslator.ToWin32(Color.White)));
                this._handleBkActiveBrush = new SafeHandleGDI(SafeNativeMethods.CreateSolidBrush(ColorTranslator.ToWin32(Color.FromArgb(240, 240, 250))));
            }

            var str = (rs.LineIndexVirtual + 1).ToString();
            var textLength = rs.TextView.GetVisualLineCount().ToString().Length;

            if (textLength != this._previousLength)
            {
                // We check if the number of lines have changed to contain an extra character.
                // In that case we'll need to perform a layout calculation for the whole text control.
                // So, call on PerformLayout(). This *might* screw up the current painting, but it is very inlikely.
                this._previousLength = textLength;
                rs.TextView.PerformLayout();
            }

            fixed (char* c = str)
            {
                var isCurrentLine = rs.LineIndexVirtual == rs.LineIndexVirtualFocused;

                var previousFont = SafeNativeMethods.SelectObject(hdc, this._handleFont.DangerousGetHandle());
                var previousPen = SafeNativeMethods.SelectObject(hdc, this._handlePen.DangerousGetHandle());

                var color = isCurrentLine ? Color.DimGray : Color.LightSteelBlue;
                var previousForeColor = SafeNativeMethods.SetTextColor(hdc, ColorTranslator.ToWin32(color));
                var previousBkMode = SafeNativeMethods.SetBkMode(hdc, NativeConstants.TRANSPARENT);

                var r = new RECT
                {
                    top = rs.Y - rs.ViewportY,
                    right = rs.X + this.Width,
                    bottom = rs.Y + rs.LineHeight,
                    left = rs.X
                };

                SafeNativeMethods.FillRect(hdc, ref r, this._handleBkActiveBrush.DangerousGetHandle());
                SafeNativeMethods.MoveToEx(hdc, rs.X + this.Width, rs.Y - rs.ViewportY, IntPtr.Zero);
                SafeNativeMethods.LineTo(hdc, rs.X + this.Width, rs.Y + rs.LineHeight - rs.ViewportY);
                SafeNativeMethods.TextOut(hdc, 2 + rs.X, rs.Y - rs.ViewportY, c, str.Length);

                SafeNativeMethods.SelectObject(hdc, previousPen);
                SafeNativeMethods.SetTextColor(hdc, previousForeColor);
                SafeNativeMethods.SetBkMode(hdc, previousBkMode);
                SafeNativeMethods.SelectObject(hdc, previousFont);
            }
        }

        public override void Dispose()
        {
            if (this._handlePen != null)
            {
                this._handlePen.Dispose();
                this._handlePen = null;
            }

            if (this._handleFont != null)
            {
                this._handleFont.Dispose();
                this._handleFont = null;
            }

            if (this._handleBkBrush != null)
            {
                this._handleBkBrush.Dispose();
                this._handleBkBrush = null;
            }

            if (this._handleBkActiveBrush != null)
            {
                this._handleBkActiveBrush.Dispose();
                this._handleBkActiveBrush = null;
            }
        }

        public override void UpdateWidth(IntPtr hdc, ITextView textView)
        {
            var longestString = "".Prefix('0', textView.GetVisualLineCount().ToString().Length + 1);

            unsafe
            {
                fixed (char* c = longestString)
                {
                    var size = Size.Empty;
                    SafeNativeMethods.GetTextExtentPoint32(hdc, c, longestString.Length, ref size);
                    this.SetWidth(size.Width);
                }
            }
        }

        protected override void PerformGotFocus(ITextView textView, int textColumnIndex)
        {
            this._previousCursor = ((Control) textView).Cursor;
            ((Control) textView).Cursor = Cursors.Arrow;
            base.PerformGotFocus(textView, textColumnIndex);
        }

        protected override void PerformLostFocus(ITextView textView, int textColumnIndex)
        {
            ((Control) textView).Cursor = this._previousCursor;
            base.PerformLostFocus(textView, textColumnIndex);
        }

        public override void PerformMouseMove(ITextView textView, int lineIndex, Point p, int textColumnIndex)
        {
            base.PerformMouseMove(textView, lineIndex, p, textColumnIndex);
        }

        public override void PerformMouseDown(ITextView textView, int lineIndex, Point p, int textColumnIndex)
        {
            var firstIndex = textView.GetFirstCharIndexFromLine(lineIndex);
            var length = textView.GetLineLength(lineIndex);

            textView.Select(firstIndex, length);
            textView.Invalidate();

            base.PerformMouseDown(textView, lineIndex, p, textColumnIndex);
        }

        public override void PerformMouseUp(ITextView textView, int lineIndex, Point p, int textColumnIndex)
        {
            base.PerformMouseUp(textView, lineIndex, p, textColumnIndex);
        }
    }
}