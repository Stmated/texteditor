using System;
using System.Drawing;
using System.Windows.Forms;
using Eliason.TextEditor.Native;
using Eliason.TextEditor.TextDocument.ByLines;
using Eliason.Common;

namespace Eliason.TextEditor.TextView
{
    public class TextColumnNotes : TextColumnBase
    {
        private SafeHandleGDI _handlePen;
        private SafeHandleGDI _hbmpInfo;
        private SafeHandleGDI _hbmpInfoFaded;
        private SafeHandleGDI _brushBackground;

        public override string Name
        {
            get { return "Notes"; }
        }

        public override string Key
        {
            get { return "Notes"; }
        }

        public override bool FloatLeft
        {
            get { return false; }
        }

        public override Image GetImage(ITextView textView)
        {
            return textView.Settings.BitmapInfo;
        }

        public override unsafe void PaintLine(IntPtr hdc, RendererState rs)
        {
            if (this._hbmpInfo == null)
            {
                this._hbmpInfo = new SafeHandleGDI(((Bitmap)this.GetImage(rs.TextView)).GetHbitmap(Color.White));
            }

            if (this._handlePen == null)
            {
                this._handlePen = new SafeHandleGDI(SafeNativeMethods.CreatePen(NativeConstants.PS_SOLID, -1, ColorTranslator.ToWin32(Color.Silver)));
                this._brushBackground = new SafeHandleGDI(SafeNativeMethods.CreateSolidBrush(ColorTranslator.ToWin32(Color.White)));

                using (var faded = new Bitmap(16, 16))
                {
                    using (var g = Graphics.FromImage(faded))
                    {
                        g.DrawImage(this.GetImage(rs.TextView), 0, 0);

                        using (var b = new SolidBrush(Color.FromArgb(240, Color.White)))
                        {
                            g.FillRectangle(b, 0, 0, 16, 16);
                        }
                    }

                    this._hbmpInfoFaded = new SafeHandleGDI(faded.GetHbitmap(Color.White));
                }
            }

            var clientSize = rs.TextView.ClientSize;

            var previousPen = SafeNativeMethods.SelectObject(hdc, this._handlePen.DangerousGetHandle());
            SafeNativeMethods.MoveToEx(hdc, clientSize.Width - this.Width, rs.Y - rs.ViewportY, IntPtr.Zero);
            SafeNativeMethods.LineTo(hdc, clientSize.Width - this.Width, rs.Y - rs.ViewportY + rs.LineHeight);

            var hasNote = rs.Line.Metadata.ContainsKey("Note");

            if (hasNote)
            {
                SafeHandleGDI hbmp = this._hbmpInfo;

                // create a mirror device context associated with the target device context
                var hdcCreated = SafeNativeMethods.CreateCompatibleDC(hdc);

                // connect the bitmap with the mirror device context
                var hBmOld = SafeNativeMethods.SelectObject(hdcCreated, hbmp.DangerousGetHandle());

                // transfer the bitmap data from the mirror device context to the target DC
                SafeNativeMethods.BitBlt(hdc, clientSize.Width - 16, rs.Y - rs.ViewportY, 16, 16, hdcCreated, 0, 0, NativeConstants.SRCCOPY);

                SafeNativeMethods.SelectObject(hdc, previousPen);
                SafeNativeMethods.DeleteObject(hBmOld);
                SafeNativeMethods.DeleteDC(hdcCreated);
            }
            else
            {
                var r = new RECT
                {
                    top = rs.Y - rs.ViewportY,
                    right = rs.X + this.Width,
                    bottom = rs.Y + rs.LineHeight,
                    left = rs.X + 1
                };

                SafeNativeMethods.FillRect(hdc, ref r, this._brushBackground.DangerousGetHandle());
            }

            if (rs.LineIndexVirtual == rs.LineIndexVirtualFocused && hasNote)
            {
                // We are currently on the same line
                var note = rs.Line.Metadata["Note"];
                var size = TextRenderer.MeasureText(note, rs.TextView.Font, rs.TextRectangle.Size);

                var r = new RECT
                {
                    top = rs.Y - rs.ViewportY,
                    right = rs.X - 5,
                    bottom = rs.Y + size.Height,
                    left = rs.X - size.Width - 5
                };

                SafeNativeMethods.FillRect(hdc, ref r, this._brushBackground.DangerousGetHandle());

                fixed (char* c = note)
                {
                    SafeNativeMethods.TextOut(hdc, r.left, r.top, c, note.Length);
                }
            }
        }

        public class UndoRedoColumnNoteEdit : UndoRedo.UndoRedoCommandBase
        {
            public ITextView TextView { get; set; }
            public int LineIndex { get; set; }

            public string PreviousText { get; set; }
            public string NewText { get; set; }

            public override string Text
            {
                get { return "note @" + (this.LineIndex + 1); }
            }

            public override void Undo()
            {
                this.Do(this.PreviousText);
            }

            public override void Redo()
            {
                this.Do(this.NewText);
            }

            private void Do(string text)
            {
                this.TextView.TextDocument.UndoRedoManager.AcceptsChanges = false;
                var line = this.TextView.GetVisualTextSegment(this.LineIndex);
                if (String.IsNullOrEmpty(text))
                {
                    line.Metadata.Remove("Note");
                }
                else
                {
                    if (line.Metadata.ContainsKey("Note"))
                    {
                        line.Metadata["Note"] = text;
                    }
                    else
                    {
                        line.Metadata.Add("Note", text);
                    }
                }
                this.TextView.TextDocument.UndoRedoManager.AcceptsChanges = true;
                this.TextView.Invalidate();
            }
        }

        public override void PerformMouseDown(ITextView textView, int lineIndex, Point p, int textColumnIndex)
        {
            this.PerformLostFocus(textView, textColumnIndex);
            var line = textView.GetVisualTextSegment(lineIndex);

            if (String.IsNullOrEmpty(line.GetText(textColumnIndex)))
            {
                // We do not allow adding notes to lines with no content.
                return;
            }

            var noteExists = line.Metadata.ContainsKey("Note");
            var defaultValue = noteExists ? line.Metadata["Note"] : String.Empty;

            var title = String.Format(noteExists ? "Update note for '{0}...'" : "Set note for '{0}...'", line.GetText(textColumnIndex).Substring(0, Math.Min(line.GetText(textColumnIndex).Length, 15)));
            var result = textView.Settings.Notifier.AskInput(new NotifierInputRequest<String>()
            {
                Title = title,
                DefaultValue = defaultValue,
                CanCancel = true
            });

            if (result.Cancelled == false)
            {
                var undoRedo = new UndoRedoColumnNoteEdit
                {
                    TextView = textView,
                    LineIndex = lineIndex,
                    PreviousText = defaultValue,
                    NewText = "" + result.Result
                };
                textView.TextDocument.UndoRedoManager.AddUndoCommand(undoRedo);

                // Execute it to set to the NewText
                undoRedo.Redo();
            }

            textView.Invalidate();

            base.PerformMouseDown(textView, lineIndex, p, textColumnIndex);
        }

        public override void PerformMouseUp(ITextView textView, int lineIndex, Point p, int textColumnIndex)
        {
            base.PerformMouseUp(textView, lineIndex, p, textColumnIndex);
        }

        public override void PerformMouseMove(ITextView textView, int lineIndex, Point p, int textColumnIndex)
        {
            base.PerformMouseMove(textView, lineIndex, p, textColumnIndex);
        }

        private Cursor _previousCursor;

        protected override void PerformGotFocus(ITextView textView, int textColumnIndex)
        {
            this._previousCursor = ((Control) textView).Cursor;
            ((Control) textView).Cursor = Cursors.Hand;
            base.PerformGotFocus(textView, textColumnIndex);
        }

        protected override void PerformLostFocus(ITextView textView, int textColumnIndex)
        {
            ((Control) textView).Cursor = this._previousCursor;
            base.PerformLostFocus(textView, textColumnIndex);
        }

        public override void UpdateWidth(IntPtr hdc, ITextView textView)
        {
            this.SetWidth(17);
        }

        public override void Dispose()
        {
            this._hbmpInfo.Dispose();
            this._hbmpInfo = null;

            this._handlePen.Dispose();
            this._handlePen = null;
        }
    }
}