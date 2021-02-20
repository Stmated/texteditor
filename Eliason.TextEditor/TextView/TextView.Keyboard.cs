#region

using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

#endregion

namespace Eliason.TextEditor.TextView
{
    partial class TextView
    {
        protected virtual bool UseHotkeys
        {
            get { return true; }
        }

        protected override bool IsInputChar(char charCode)
        {
            return true;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return true;
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (e.Handled || this.IsReadOnly)
            {
                return;
            }

            if ((ModifierKeys & Keys.Alt) != Keys.Alt && (ModifierKeys & Keys.Control) == Keys.Control)
            {
                // We do not send in a keypress is Control is currently held down,
                // since it would create non-sensical characters.
                return;
            }

            if (e.KeyChar == '\r')
            {
                // This can happen if there is a popup above the text control  and the user cancels it with the Return key.
                // Then we get this character sent to us, a character that we do not listen to internally.
                // We never use \r in the internal text buffer, only \n. 
                // However, we save \r\n to file if "Windows Newlines" are enabled in preferences
                e.Handled = true;
                return;
            }

            if (this._imeComposition.IsCompositioning)
            {
                // This character comes from the IME as WM_IME_CHAR, which will be sent again as WM_CHAR anyway,
                // so if we did not abort here we'd add the same text twice.
                return;
            }

            if (this.SelectionLength > 0)
            {
                this.TextRemove(this.SelectionStart, this.SelectionLength);
                this.SelectionLength = 0;
            }

            var keyCharString = e.KeyChar.ToString();
            this.TextInsert(this.SelectionStart, keyCharString);

            this._selectionColumnX = -1;

            this.SetSelectionStart(this.SelectionStart + 1, ByInterface.ByKeyboard, this.CurrentTextColumnIndex);

            this.ScrollToCaret();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Handled || e.SuppressKeyPress || this._imeComposition.IsCompositioning)
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;

            switch (e.KeyCode)
            {
                case Keys.Up:
                    this.OnKeyDownVertical(true, -1, e.Shift);
                    break;
                case Keys.Down:
                    this.OnKeyDownVertical(false, this.LineHeight + (this.LineHeight/2), e.Shift);
                    break;
                case Keys.Left:
                    this.OnKeyDownHorizontal(false, e.Control, e.Shift);
                    break;
                case Keys.Right:
                    this.OnKeyDownHorizontal(true, e.Control, e.Shift);
                    break;
                case Keys.Delete:
                case Keys.Back:
                    this.OnKeyDownRemove(e);
                    break;
                case Keys.Enter:
                    this.OnKeyDownEnter();
                    break;
                case Keys.PageUp:
                    this.OnKeyDownVertical(true, -ClientRectangle.Height, e.Shift);
                    break;
                case Keys.PageDown:
                    this.OnKeyDownVertical(false, ClientRectangle.Height, e.Shift);
                    break;
                case Keys.Insert:
                    this._overwriteMode = !this._overwriteMode;
                    Invalidate();
                    break;
                case Keys.Home:
                    this.OnKeyDownHome(e);
                    break;
                case Keys.End:
                    this.OnKeyDownEnd(e);
                    break;
                default:
                    if (this.UseHotkeys)
                    {
                        this.OnKeyDownDefaultKeys(e);
                    }
                    else if (e.KeyCode != Keys.Escape)
                    {
                        // Do not do anything if Control is held down, since it only results in weird characters.
                        if (e.Control && e.Alt == false)
                        {
                            return;
                        }

                        e.Handled = false;
                        e.SuppressKeyPress = false;
                    }
                    break;
            }
        }

        private void OnKeyDownVertical(bool up, int yChange, bool select)
        {
            if (select && this.SelectionLength <= 0)
            {
                this._selectionIsBackwards = up;
            }

            var index = this._selectionIsBackwards ? this.SelectionStart : this.SelectionStart + this.SelectionLength;
            var p2 = this.GetVirtualPositionFromCharIndex(index);
            Point p;

            if (this._selectionColumnX != -1)
            {
                p = new Point(this._selectionColumnX, p2.Y + yChange);
            }
            else
            {
                p = new Point(p2.X, p2.Y + yChange);
                this._selectionColumnX = p2.X;
            }

            var newSelectionStart = this.GetCharIndexFromVirtualPosition(p, this.CurrentTextColumnIndex);

            var length = up ? ((index) - newSelectionStart) : ((newSelectionStart - (index))*-1);

            if (select)
            {
                if (this._selectionIsBackwards)
                {
                    // We send as "Manually" since in logical terms the arrows keys are not really keystrokes,
                    // just a start of an action. And by saying that it is manual, we can do actions that are
                    // too heavy to do for each keystroke but can be done if the indexes changes.
                    this.SetSelectionStart(this.SelectionStart - length, ByInterface.Manually, this.CurrentTextColumnIndex);
                    this.SelectionLength += length;
                }
                else
                {
                    this.SelectionLength -= length;
                }
            }
            else
            {
                if (this._selectionIsBackwards)
                {
                    this.SelectionLength = 0;
                    this.SetSelectionStart(this.SelectionStart - length, ByInterface.Manually, this.CurrentTextColumnIndex);
                }
                else
                {
                    this.SetSelectionStart((this.SelectionStart + this.SelectionLength) - length, ByInterface.Manually, this.CurrentTextColumnIndex);
                    this.SelectionLength = 0;
                }
            }

            this.ScrollToCaret();
        }

        private void OnKeyDownHorizontal(bool right, bool largeStep, bool select)
        {
            if (select && this.SelectionLength <= 0)
            {
                this._selectionIsBackwards = right == false;
            }

            var length = largeStep
                ? this.GetLengthByLargeStep(
                    this._selectionIsBackwards
                        ? this.SelectionStart
                        : this.SelectionStart + this.SelectionLength, right)
                : 1;

            if (right)
            {
                length = length*-1;
            }

            if (select)
            {
                if (this._selectionIsBackwards)
                {
                    this.SetSelectionStart(this.SelectionStart - length, ByInterface.Manually, this.CurrentTextColumnIndex);
                    this.SelectionLength += length;
                }
                else
                {
                    this.SelectionLength -= length;
                }
            }
            else
            {
                if (this._selectionIsBackwards)
                {
                    this.SelectionLength = 0;
                    this.SetSelectionStart(this.SelectionStart - length, ByInterface.Manually, this.CurrentTextColumnIndex);
                }
                else
                {
                    this.SetSelectionStart((this.SelectionStart + this.SelectionLength) - length, ByInterface.Manually, this.CurrentTextColumnIndex);
                    this.SelectionLength = 0;
                }
            }

            this._selectionColumnX = this.GetVirtualPositionFromCharIndex(this.SelectionStart).X;

            this.ScrollToCaret();
        }

        private void OnKeyDownDefaultKeys(KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.W && e.Control))
            {
                this.ToggleWordwrap();
            }
            else if ((e.KeyCode == Keys.X && e.Control) || (e.KeyCode == Keys.Delete && e.Shift))
            {
                this.Cut();
            }
            else if ((e.KeyCode == Keys.C && e.Control) || (e.KeyCode == Keys.Insert && e.Control))
            {
                this.Copy();
            }
            else if ((e.KeyCode == Keys.V && e.Control) || (e.KeyCode == Keys.Insert && e.Shift))
            {
                this.Paste();
            }
            else if ((e.KeyCode == Keys.Z && e.Control))
            {
                this.Undo();
            }
            else if ((e.KeyCode == Keys.Y && e.Control))
            {
                this.Redo();
            }
            else if ((e.KeyCode == Keys.A && e.Control))
            {
                this.SelectAll();
            }
            else
            {
                e.Handled = false;
                e.SuppressKeyPress = false;
            }
        }

        private void OnKeyDownEnd(KeyEventArgs e)
        {
            if (this.SelectionLength == 0)
            {
                this._selectionIsBackwards = false;
            }
            if (e.Control)
            {
                if (e.Shift)
                {
                    if (this._selectionIsBackwards)
                    {
                        this._selectionIsBackwards = false;

                        this.SetSelectionStart(this.SelectionStart + this.SelectionLength, ByInterface.ByKeyboard, this.CurrentTextColumnIndex);
                        this.SelectionLength = TextLength - this.SelectionStart;
                    }
                    else
                    {
                        this.SelectionLength = TextLength - this.SelectionStart;
                    }
                }
                else
                {
                    this.SetSelectionStart(TextLength, ByInterface.ByKeyboard, this.CurrentTextColumnIndex);
                    this.SelectionLength = 0;
                }
            }
            else
            {
                var index = this._selectionIsBackwards
                    ? this.SelectionStart
                    : this.SelectionStart + this.SelectionLength;

                var lineIndex = GetLineFromCharIndex(index);

                if (e.Shift)
                {
                    var lineEndIndex = GetFirstCharIndexFromLine(lineIndex) + this.GetIndexOfNextVisualLinebreak(lineIndex, index, true, this.CurrentTextColumnIndex); // GetLineLength(lineIndex);
                    var difference = lineEndIndex - index;

                    if (this._selectionIsBackwards)
                    {
                        if (this.SelectionLength - difference < 0)
                        {
                            this._selectionIsBackwards = false;

                            this.SetSelectionStart(this.SelectionStart + this.SelectionLength, ByInterface.ByKeyboard, this.CurrentTextColumnIndex);
                            this.SelectionLength = lineEndIndex - this.SelectionStart;
                        }
                        else
                        {
                            this.SetSelectionStart(this.SelectionStart + difference, ByInterface.ByKeyboard, this.CurrentTextColumnIndex);
                            this.SelectionLength -= difference;
                        }
                    }
                    else
                    {
                        this.SelectionLength += difference;
                    }
                }
                else
                {
                    var newIndex = GetFirstCharIndexFromLine(lineIndex) + this.GetIndexOfNextVisualLinebreak(lineIndex, index, true, this.CurrentTextColumnIndex);

                    this.SetSelectionStart(newIndex, ByInterface.ByKeyboard, this.CurrentTextColumnIndex);
                    this.SelectionLength = 0;
                }
            }
            this.ScrollToCaret();
        }

        private void OnKeyDownHome(KeyEventArgs e)
        {
            if (this.SelectionLength == 0)
            {
                this._selectionIsBackwards = true;
            }
            if (e.Control)
            {
                if (e.Shift)
                {
                    if (this._selectionIsBackwards)
                    {
                        this.SelectionLength = this.SelectionStart + this.SelectionLength;
                    }
                    else
                    {
                        this.SelectionLength = this.SelectionStart;
                        this._selectionIsBackwards = true;
                    }

                    this.SetSelectionStart(0, ByInterface.ByKeyboard, this.CurrentTextColumnIndex);
                }
                else
                {
                    this.SetSelectionStart(0, ByInterface.ByKeyboard, this.CurrentTextColumnIndex);
                    this.SelectionLength = 0;
                }
            }
            else
            {
                var index = this._selectionIsBackwards
                    ? this.SelectionStart
                    : this.SelectionStart + this.SelectionLength;

                var lineIndex = GetLineFromCharIndex(index);

                if (e.Shift)
                {
                    var difference = index - this.GetIndexOfNextVisualLinebreak(lineIndex, index, false, this.CurrentTextColumnIndex);

                    if (this._selectionIsBackwards)
                    {
                        this.SetSelectionStart(this.SelectionStart - difference, ByInterface.ByKeyboard, this.CurrentTextColumnIndex);
                        this.SelectionLength += difference;
                    }
                    else
                    {
                        var negative = 0;

                        if (this.SelectionLength - difference < 0)
                        {
                            this._selectionIsBackwards = true;

                            negative = difference - this.SelectionLength;
                            this.SelectionLength = negative;
                        }
                        else
                        {
                            this.SelectionLength -= difference;
                        }

                        this.SetSelectionStart(this.SelectionStart - negative, ByInterface.ByKeyboard, this.CurrentTextColumnIndex);
                    }
                }
                else
                {
                    this.SetSelectionStart(this.GetIndexOfNextVisualLinebreak(lineIndex, index, false, this.CurrentTextColumnIndex), ByInterface.ByKeyboard, this.CurrentTextColumnIndex);
                    this.SelectionLength = 0;
                }
            }
            this.ScrollToCaret();
        }

        private void OnKeyDownEnter()
        {
            var undoRedoLinebreak = this.TextInsert(this.SelectionStart, "\n");

            var lineIndex = GetLineFromCharIndex(this.SelectionStart);

            // Now let's perform the text filters on the current line, since an accepted (Newlined) line is when we should apply it.
            var originalText = GetLineText(lineIndex);
            var filteredText = FilterStringLine(originalText);

            if (originalText != filteredText)
            {
                var lineFirstIndex = GetFirstCharIndexFromLine(lineIndex);
                var lineLength = originalText.Length;

                var undoRedoRemove = this.TextRemove(lineFirstIndex, lineLength);
                var undoRedoInsert = this.TextInsert(lineFirstIndex, filteredText);

                undoRedoLinebreak.RedoPair = true;
                undoRedoRemove.RedoPair = true;
                undoRedoRemove.UndoPair = true;
                undoRedoInsert.UndoPair = true;

                var diff = filteredText.Length - originalText.Length;

                var newSelectionStart = (this.SelectionStart + diff) + 1; // +1 since the newline \n was added.

                if (newSelectionStart == this.SelectionStart)
                {
                    // Even if the index is the same, the caret should still update since the location might
                    // have changed even though the lengths are all the same.
                    this.UpdateCaretLocation();
                    this.Caret.ResetBlink();
                }
                else
                {
                    this.SetSelectionStart(newSelectionStart, ByInterface.ByKeyboard, this.CurrentTextColumnIndex);
                }
            }
            else
            {
                //var t1 = GetLineText(lineIndex);

                this.SetSelectionStart(this.SelectionStart + 1, ByInterface.ByKeyboard, this.CurrentTextColumnIndex);
            }

            this.ScrollToCaret();
        }

        private void OnKeyDownRemove(KeyEventArgs e)
        {
            var right = e.KeyCode == Keys.Delete;

            if (this.SelectionLength > 0)
            {
                this.TextRemove(this.SelectionStart, this.SelectionLength);
            }
            else
            {
                var length = e.Control ? this.GetLengthByLargeStep(this.SelectionStart, right) : 1;

                if (right == false)
                {
                    this.TextRemove(this.SelectionStart - length, length);
                    this.SetSelectionStart(this.SelectionStart - length, ByInterface.ByKeyboard, this.CurrentTextColumnIndex);
                }
                else
                {
                    this.TextRemove(this.SelectionStart, length);
                    this.Invalidate();
                }
            }

            this.SelectionLength = 0;
            this.ScrollToCaret();
        }

        /// <summary>
        ///   TODO:   If start is whitespace, it should skip ahead until it is not, since the wanted behavior of large steps through text
        ///   should be to skip one word per click.
        /// </summary>
        /// <param name = "start"></param>
        /// <param name = "right"></param>
        /// <returns></returns>
        private int GetLengthByLargeStep(int start, bool right)
        {
            var increment = right ? 1 : -1;
            start = right ? start : start - 1;
            var variableStart = start;
            var foundButSkipWhitespace = false;

            var category = UnicodeCategory.Format;

            foreach (var c in TextGetStream(start, right))
            {
                if (foundButSkipWhitespace)
                {
                    if (char.IsWhiteSpace(c) == false)
                    {
                        break;
                    }
                }

                if (category == UnicodeCategory.Format)
                {
                    var newCategory = c.GetSimplifiedUnicodeCategory();
                    if (right == false && newCategory == UnicodeCategory.SpaceSeparator)
                    {
                        variableStart += increment;
                        continue;
                    }

                    category = newCategory;
                }

                if (c.GetSimplifiedUnicodeCategory() != category)
                {
                    foundButSkipWhitespace = true;

                    if (right == false || char.IsWhiteSpace(c) == false)
                    {
                        break;
                    }
                }

                variableStart += increment;
            }

            return right ? (variableStart - start) : (start - variableStart);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineIndex"></param>
        /// <param name="globalStartIndex"></param>
        /// <param name="searchForward">If true, it returns a relative number from line start index, if false it returns the global index.</param>
        /// <returns></returns>
        private int GetIndexOfNextVisualLinebreak(int lineIndex, int globalStartIndex, bool searchForward, int textColumnIndex)
        {
            var lineStartIndex = GetFirstCharIndexFromLine(lineIndex);
            var relativeStartIndex = globalStartIndex - lineStartIndex;

            var lineBreaks = this.GetVisualInformation(lineIndex).GetLineSplitIndexes(textColumnIndex);

            if (lineBreaks == null || lineBreaks.Length == 0)
            {
                return searchForward ? GetLineLength(lineIndex) : lineStartIndex;
            }

            if (searchForward)
            {
                for (var i = 0; i < lineBreaks.Length; i++)
                {
                    if (relativeStartIndex <= lineBreaks[i])
                    {
                        return lineBreaks[i];
                    }
                }

                return GetLineLength(lineIndex);
            }

            for (var i = lineBreaks.Length - 1; i >= 0; i--)
            {
                if (relativeStartIndex >= lineBreaks[i])
                {
                    return lineStartIndex + lineBreaks[i];
                }
            }

            return lineStartIndex;
        }
    }
}