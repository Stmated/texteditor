using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using Eliason.TextEditor.Native;
using Eliason.Common;

namespace Eliason.TextEditor.TextView
{
    public class IMEComposition
    {
        private class CompositionInfo
        {
            public string Text { get; set; }
            public int SelectionStart { get; set; }
            public int SelectionEnd { get; set; }

            public int CaretStart { get; set; }
        }

        private CompositionInfo _imeComposition;

        private readonly ITextView _textView;

        public IMEComposition(ITextView textView)
        {
            this._textView = textView;
        }

        public bool IsCompositioning { get; private set; }

        public void StartComposition(IntPtr handle)
        {
            this._imeComposition = null;
            this.IsCompositioning = true;

            // Move the IME windows.
            var hIMC = SafeNativeMethods.ImmGetContext(handle);
            try
            {
                this.MoveImeWindow(handle, hIMC, this._textView.Caret.Location);
                SafeNativeMethods.ImmReleaseContext(handle, hIMC);
            }
            finally
            {
                SafeNativeMethods.ImmReleaseContext(handle, hIMC);
            }
        }

        public void EndComposition()
        {
            if (this._imeComposition != null)
            {
                if (this._textView.IsReadOnly == false)
                {
                    this._textView.SelectedText = this._imeComposition.Text;
                }
            }

            this._imeComposition = null;
            this.IsCompositioning = false;
        }

        private static bool IsTargetAttribute(byte attribute)
        {
            return (attribute == NativeConstants.ATTR_TARGET_CONVERTED || attribute == NativeConstants.ATTR_TARGET_NOTCONVERTED);
        }

        /// <summary>
        /// Updates the composition to a copy of what the Operating System has in its IME input.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="gcs"></param>
        /// <returns>Returns True if update was handled, False if not and hence default OS behavior should be fallbacked upon.</returns>
        public bool UpdateCurrentComposition(IntPtr handle, GCS gcs)
        {
            if (((gcs & GCS.GCS_RESULTSTR) == GCS.GCS_RESULTSTR))
            {
                if (this._imeComposition != null)
                {
                    //this.textView.SelectedText = this.imeComposition.Text;
                    //this.imeComposition = this.GetCurrentComposition(handle);
                }
                else
                {
                    return false;
                }
            }
            else if (((gcs & GCS.GCS_RESULTSTR) == GCS.GCS_RESULTSTR) == false)
            {
                this._imeComposition = this.GetCurrentComposition(handle);
            }

            return true;
        }

        public unsafe void Paint(IntPtr hdc, int x, int y, ICaret caret)
        {
            if (this._imeComposition == null)
            {
                return;
            }

            var textRect = this._textView.GetTextRectangle(false);

            var startX = x;
            var startY = y;

            var newLines = new List<int>();

            fixed (char* c = this._imeComposition.Text)
            {
                var clientSize = this._textView.ClientSize;
                var caretY = -1;
                var caretX = -1;
                for (var i = 0; i <= this._imeComposition.Text.Length; i++)
                {
                    var s = Size.Empty;

                    var charOffset = newLines.Count == 0
                        ? 0
                        : newLines[newLines.Count - 1];
                    SafeNativeMethods.GetTextExtentPoint32(hdc, c + charOffset, i - charOffset, ref s);

                    if (caretY == -1)
                    {
                        if (i == this._imeComposition.CaretStart)
                        {
                            caretY = newLines.Count * this._textView.LineHeight;
                            caretX = s.Width;
                        }
                    }

                    if (x + s.Width >= clientSize.Width)
                    {
                        newLines.Add(i - 1);
                    }
                }

                var previousBkMode = SafeNativeMethods.SetBkMode(hdc, NativeConstants.OPAQUE);
                var previousBkColor = SafeNativeMethods.SetBkColor(hdc, ColorTranslator.ToWin32(Color.WhiteSmoke));
                var previousTextColor = SafeNativeMethods.SetTextColor(hdc, ColorTranslator.ToWin32(Color.Black));

                for (var i = 0; i <= newLines.Count; i++)
                {
                    var start = i > 0 ? newLines[i - 1] : 0;
                    var end = i < newLines.Count ? newLines[i] : this._imeComposition.Text.Length;

                    SafeNativeMethods.TextOut(hdc, x, y, c + start, end - start);

                    var selStart = Math.Max(this._imeComposition.SelectionStart, start);
                    var selEnd = Math.Min(this._imeComposition.SelectionEnd, end);

                    if (selStart >= start && selEnd <= end)
                    {
                        var preSize = Size.Empty;

                        if (selStart - start > 0)
                        {
                            SafeNativeMethods.GetTextExtentPoint32(hdc, c, selStart - start, ref preSize);
                        }

                        var selectionSize = Size.Empty;
                        SafeNativeMethods.GetTextExtentPoint32(hdc, c + selStart, selEnd - selStart, ref selectionSize);

                        if (selectionSize.Width > 0)
                        {
                            SafeNativeMethods.MoveToEx(hdc, x + preSize.Width, y + selectionSize.Height, IntPtr.Zero);
                            SafeNativeMethods.LineTo(hdc, x + preSize.Width + selectionSize.Width, y + selectionSize.Height);
                        }
                    }

                    x = textRect.Left;
                    y += this._textView.LineHeight + 1;
                }

                SafeNativeMethods.SetBkMode(hdc, previousBkMode);
                SafeNativeMethods.SetBkColor(hdc, previousBkColor);
                SafeNativeMethods.SetTextColor(hdc, previousTextColor);

                caret.Render(hdc, startX + caretX, startY + caretY);
            }
        }

        private CompositionInfo GetCurrentComposition(IntPtr handle)
        {
            var hIMC = IntPtr.Zero;

            try
            {
                hIMC = SafeNativeMethods.ImmGetContext(handle);

                var start = 0;
                var end = 0;
                this.GetSelection(hIMC, ref start, ref end);

                var info = new CompositionInfo
                {
                    Text = GetString(hIMC),
                    SelectionStart = start,
                    SelectionEnd = end,
                    CaretStart = SafeNativeMethods.ImmGetCompositionStringW(hIMC, (int)GCS.GCS_CURSORPOS, null, 0)
                };

                return info;
            }
            finally
            {
                if (hIMC != IntPtr.Zero)
                {
                    SafeNativeMethods.ImmReleaseContext(handle, hIMC);
                }
            }
        }

        /// <summary>
        /// IntPtr handle is the handle to the textbox
        /// </summary>
        /// <param name="hIMC"></param>
        /// <returns></returns>
        private static string GetString(IntPtr hIMC)
        {
            var strLen = SafeNativeMethods.ImmGetCompositionStringW(hIMC, (int)GCS.GCS_COMPSTR, null, 0);

            if (strLen > 0)
            {
                var buffer = new byte[strLen];
                SafeNativeMethods.ImmGetCompositionStringW(hIMC, (int)GCS.GCS_COMPSTR, buffer, strLen);
                return Encoding.Unicode.GetString(buffer);
            }

            return string.Empty;
        }

        /// <summary>
        /// Helper function for ImeInput::GetCompositionInfo() method, to get the target
        /// range that's selected by the user in the current composition string.
        /// </summary>
        private void GetSelection(IntPtr immContext, ref int targetStart, ref int targetEnd)
        {
            var attribute_size = SafeNativeMethods.ImmGetCompositionStringW(immContext, (int)GCS.GCS_COMPATTR, null, 0);
            if (attribute_size > 0)
            {
                var attribute_data = new byte[attribute_size];

                if (attribute_data.Length > 0)
                {
                    SafeNativeMethods.ImmGetCompositionStringW(immContext, (int)GCS.GCS_COMPATTR, attribute_data, attribute_size);

                    for (targetStart = 0; targetStart < attribute_size; ++targetStart)
                    {
                        if (IsTargetAttribute(attribute_data[targetStart]))
                        {
                            break;
                        }
                    }

                    for (targetEnd = targetStart; targetEnd < attribute_size; ++targetEnd)
                    {
                        if (!IsTargetAttribute(attribute_data[targetEnd]))
                        {
                            break;
                        }
                    }

                    if (targetStart == attribute_size)
                    {
                        // This composition clause does not contain any target clauses,
                        // i.e. this clauses is an input clause.
                        // We treat the whole composition as a target clause.
                        targetStart = 0;
                        targetEnd = attribute_size;
                    }
                }
            }
        }

        private void MoveImeWindow(IntPtr windowHandle, IntPtr immContext, Point p)
        {
            var x = p.X;
            var y = p.Y;

            // As written in a comment in ImeInput::CreateImeWindow(),
            // Chinese IMEs ignore function calls to ::ImmSetCandidateWindow()
            // when a user disables TSF (Text Service Framework) and CUAS (Cicero
            // Unaware Application Support).
            // On the other hand, when a user enables TSF and CUAS, Chinese IMEs
            // ignore the position of the current system caret and uses the
            // parameters given to ::ImmSetCandidateWindow() with its 'dwStyle'
            // parameter CFS_CANDIDATEPOS.
            // Therefore, we do not only call ::ImmSetCandidateWindow() but also
            // set the positions of the temporary system caret if it exists.
            var candidatePosition = new CANDIDATEFORM
            {
                dwIndex = 0,
                rcArea = new RECT { bottom = 0, left = 0, right = 0, top = 0 },
                dwStyle = NativeConstants.CFS_CANDIDATEPOS,
                ptCurrentPos = new POINT(x, y + 10)
            };

            var size = Marshal.SizeOf(candidatePosition);
            var cfPtr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(candidatePosition, cfPtr, true);

            var result = SafeNativeMethods.ImmSetCandidateWindow(immContext, cfPtr);

            Marshal.DestroyStructure(cfPtr, typeof(CANDIDATEFORM));

            #region Chinese/Korean

            /*if (system_caret_)
            {
                switch (PRIMARYLANGID(input_language_id_))
                {
                    case LANG_JAPANESE:
                        SetCaretPos(x, y + caret_rect_.height());
                        break;
                    default:
                        SetCaretPos(x, y);
                        break;
                }
            }
            if (PRIMARYLANGID(input_language_id_) == LANG_KOREAN)
            {
                // Chinese IMEs and Japanese IMEs require the upper-left corner of
                // the caret to move the position of their candidate windows.
                // On the other hand, Korean IMEs require the lower-left corner of the
                // caret to move their candidate windows.
                y += kCaretMargin;
            }*/

            #endregion

            // Japanese IMEs and Korean IMEs also use the rectangle given to
            // ::ImmSetCandidateWindow() with its 'dwStyle' parameter CFS_EXCLUDE
            // to move their candidate windows when a user disables TSF and CUAS.
            // Therefore, we also set this parameter here.
            var excludeRectangle = new CANDIDATEFORM
            {
                dwIndex = 0,
                dwStyle = NativeConstants.CFS_EXCLUDE,
                ptCurrentPos = new POINT(x, y + 10),
                rcArea = new RECT { left = x, top = y + 10, right = x + 10, bottom = y + this._textView.LineHeight + 10 }
            };

            var size2 = Marshal.SizeOf(excludeRectangle);
            var cfPtr2 = Marshal.AllocHGlobal(size2);
            Marshal.StructureToPtr(excludeRectangle, cfPtr2, true);

            var result2 = SafeNativeMethods.ImmSetCandidateWindow(immContext, cfPtr2);

            Marshal.DestroyStructure(cfPtr2, typeof(CANDIDATEFORM));

            //ImmSetCandidateWindow(imm_context, exclude_rectangle);
        }
    }
}