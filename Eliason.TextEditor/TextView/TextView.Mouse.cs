#region

using System;
using System.Windows.Forms;

#endregion

namespace Eliason.TextEditor.TextView
{
    partial class TextView
    {
        private int _clickCount;
        private int _clickLastIndex;
        private long _clickLastMs = -1;

        /// <summary>
        ///   Event fired whent he user clicks the mouse.
        ///   Keeps track of the number of consequtive clicks, and if the clicks are on the same character index, the behaviors will be:
        ///   1. Set the caret pos
        ///   2. Select the word until different types of characters found on sides
        ///   3. Select word until whitespace found on sides
        ///   4. Select whole line
        ///   5+. The click count is set to 1 and it cycles back
        /// </summary>
        /// <param name = "e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (this.Focused == false)
            {
                this.Focus();
            }

            if (ClientRectangle.Contains(e.Location) == false)
            {
                return;
            }

            var index = this.GetCharIndexFromPhysicalPosition(e.Location);

            var columnLeftX = 0;
            var columnRightX = 0;
            var clientSize = this.ClientSize;
            foreach (var column in this.Columns)
            {
                var x1 = (column.FloatLeft ? columnLeftX : clientSize.Width - columnRightX - column.Width);
                var x2 = (x1 + column.Width);

                if (column.IsEnabled(this.Settings))
                {
                    if (e.X >= x1 && e.X <= x2 && e.Y < this.GetTextRectangle(true).Height)
                    {
                        var lineIndex = this.GetLineFromCharIndex(index);
                        column.PerformMouseDown(this, lineIndex, e.Location, this.CurrentTextColumnIndex);

                        // We only do this, and do not let any other action do its job.
                        return;
                    }
                }

                if (column.FloatLeft)
                {
                    columnLeftX += column.Width;
                }
                else
                {
                    columnRightX += column.Width;
                }
            }

            if (e.Button == MouseButtons.Right)
            {
                this._clickCount = 0;

                if (this.SelectionLength == 0)
                {
                    this.SetSelectionStart(index, ByInterface.ByMouseRight, this.CurrentTextColumnIndex);
                    Invalidate();
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (this._clickLastMs == -1 || this._clickLastIndex != index || (Environment.TickCount - this._clickLastMs) > SystemInformation.DoubleClickTime)
                {
                    this._clickCount = 0;
                }

                this._clickCount++;

                if (this._clickCount > 4)
                {
                    this._clickCount = 1;
                }

                if (this._clickCount == 4)
                {
                    var lineIndex = GetLineFromCharIndex(this.SelectionStart);

                    if (lineIndex != -1)
                    {
                        this.SetSelectionStart(GetFirstCharIndexFromLine(lineIndex), ByInterface.ByMouse, this.CurrentTextColumnIndex);
                        this.SelectionLength = GetLineLength(lineIndex);
                        Invalidate();
                    }
                }
                else if (this._clickCount >= 2)
                {
                    var word = GetWord(this.SelectionStart, this._clickCount == 2);

                    if (word != null)
                    {
                        this.SetSelectionStart(word.Start, ByInterface.ByMouse, this.CurrentTextColumnIndex);
                        this.SelectionLength = word.End - word.Start;
                        Invalidate();
                    }
                }
                else
                {
                    if (this._selectionMode == false)
                    {
                        var loc = e.Location;

                        this._selectionModeStart = index;
                        this.SetSelectionStart(this._selectionModeStart, ByInterface.ByMouse, this.CurrentTextColumnIndex);
                        this.SelectionLength = 0;

                        loc.Offset(this.ScrollHost.ScrollPosH, this.ScrollHost.ScrollPosVIntegral);

                        var closestIndex = this.GetCharIndexFromVirtualPosition(loc, this.CurrentTextColumnIndex);

                        if (closestIndex != -1)
                        {
                            var indexPoint = this.GetPositionFromCharIndex(closestIndex);

                            this._selectionColumnX = indexPoint.X;

                            if (this._renderer.FocusedStyledSegment != null)
                            {
                                this._renderer.FocusedStyledSegment.Execute();
                            }

                            this._selectionMode = true;

                            Invalidate();
                        }
                    }
                }

                this.ScrollToCaret();

                this._clickLastMs = Environment.TickCount;
                this._clickLastIndex = index;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            this._selectionMode = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var index = this.GetCharIndexFromPhysicalPosition(e.Location);

            if (ClientRectangle.Contains(e.Location))
            {
                this.SetFocusedStyleSegment(index, true, this.CurrentTextColumnIndex);
            }

            var foundFocused = false;
            var columnLeftX = 0;
            var columnRightX = 0;
            var textRectangle = this.GetTextRectangle(true);
            var clientSize = this.ClientSize;
            foreach (var column in this.Columns)
            {
                var x1 = (column.FloatLeft ? columnLeftX : clientSize.Width - columnRightX - column.Width);
                var x2 = (x1 + column.Width);

                if (column.IsEnabled(this.Settings))
                {
                    var focused = e.X >= x1 && e.X <= x2 && e.Y < textRectangle.Height;
                    if (column.SetFocused(focused, this, this.CurrentTextColumnIndex))
                    {
                        this.Invalidate();
                    }

                    foundFocused = foundFocused | column.Focused;
                }

                if (column.FloatLeft)
                {
                    columnLeftX += column.Width;
                }
                else
                {
                    columnRightX += column.Width;
                }
            }

            //this.currentCursor = foundFocused ? this.Cursor : Cursors.IBeam;

            // If we are in selection mode, it means that the mouse has been pressed down,
            // and we're moving the cursor over the text.
            if (this._selectionMode)
            {
                if (index < this._selectionModeStart)
                {
                    var length = this._selectionModeStart - index;

                    this.SetSelectionStart(index, ByInterface.ByMouse, this.CurrentTextColumnIndex);
                    this.SelectionLength = length;
                    this._selectionIsBackwards = true;
                }
                else
                {
                    if (this.SelectionStart != this._selectionModeStart)
                    {
                        this.SetSelectionStart(this._selectionModeStart, ByInterface.ByMouse, this.CurrentTextColumnIndex);
                    }

                    this.SelectionLength = index - this.SelectionStart;
                    this._selectionIsBackwards = false;
                }

                this.ScrollToCaret();
            }
        }
    }
}