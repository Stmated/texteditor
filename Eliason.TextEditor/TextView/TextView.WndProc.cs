#region

using System.Windows.Forms;
using Eliason.TextEditor.Native;
using Eliason.Common;

#endregion

namespace Eliason.TextEditor.TextView
{
    partial class TextView
    {
        private int _onResizePreviousFirstVisibleLine = -1;

        private readonly IMEComposition _imeComposition;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case NativeConstants.WM_IME_STARTCOMPOSITION:
                    {
                        this._imeComposition.StartComposition(Handle);

                        this.Caret.ResetBlink();
                        Invalidate();
                    }
                    break;

                case NativeConstants.WM_IME_ENDCOMPOSITION:
                    {
                        this._imeComposition.EndComposition();

                        base.WndProc(ref m);
                    }
                    break;

                case NativeConstants.WM_IME_COMPOSITION:
                    {
                        if (this._imeComposition.UpdateCurrentComposition(Handle, (GCS)m.LParam.ToInt32()) == false)
                        {
                            base.WndProc(ref m);
                        }

                        this.Caret.ResetBlink();
                        Invalidate();
                    }
                    break;

                case NativeConstants.WM_WINDOWPOSCHANGING:
                    {
                        // Save the current selection index...
                        this._onResizePreviousFirstVisibleLine = this.SelectionStart;

                        base.WndProc(ref m);
                    }
                    break;

                case NativeConstants.WM_WINDOWPOSCHANGED:
                    {
                        base.WndProc(ref m);

                        // ... and jump the viewport to that index when resize is done.
                        if (this._onResizePreviousFirstVisibleLine != -1)
                        {
                            this.ScrollHost.ScrollToPoint(this.GetVirtualPositionFromCharIndex(this._onResizePreviousFirstVisibleLine), true, this.WordWrap);
                        }
                    }
                    break;

                default:
                    {
                        base.WndProc(ref m);
                    }
                    break;
            }
        }
    }
}