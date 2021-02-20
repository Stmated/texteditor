using System;
using System.Collections.Generic;
using Eliason.TextEditor.TextStyles;
using Eliason.TextEditor.UndoRedo;

namespace Eliason.TextEditor.TextDocument.ByLines
{
    public partial class TextDocumentByLines : TextDocumentBase
    {
        private readonly List<TextLine> _lines = new List<TextLine>();

        public TextDocumentByLines()
        {
            this.TextSegmentStyledManager = new LineTextSegmentStyledManager(this);
        }

        private List<TextLine> Lines
        {
            get { return this._lines; }
        }

        public override ITextDocumentRenderer GetRenderer(ITextView textView)
        {
            return new TextDocumentLineRenderer(textView, this);
        }

        public override void Clear()
        {
            this.Lines.Clear();
        }

        public override ITextSegmentStyled CreateStyledTextSegment(TextStyleBase style)
        {
            return new TextAnchor(style);
        }

        public override char GetCharFromIndex(int index, int textColumnIndex)
        {
            var lineIndex = this.GetLineFromCharIndex(index, textColumnIndex);

            if (lineIndex == -1)
            {
                throw new ArgumentException("The line does not exist", "index");
            }

            var text = this.Lines[lineIndex].GetText(textColumnIndex);
            var firstCharIndex = this.GetFirstCharIndexFromLine(lineIndex);
            var actualIndex = index - firstCharIndex;
            if (text.Length == 0)
            {
                return '\0';
            }

            if (text.Length == actualIndex)
            {
                return '\n';
            }

            return text[actualIndex];
        }

        /// <summary>
        /// TODO: Implement so that if "textColumnIndex" is -1, all the columns are outputted.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="right"></param>
        /// <param name="textColumnIndex"></param>
        /// <returns></returns>
        public override IEnumerable<char> TextGetStream(int start, bool right, int textColumnIndex)
        {
            if (textColumnIndex == -1)
            {
                textColumnIndex = 0;
                // TODO: Need to get the text from all columns if textColumnIndex is negative
            }

            var lineIndex = this.GetLineFromCharIndex(start, textColumnIndex);

            if (lineIndex == -1)
            {
                yield break;
            }

            if (right)
            {
                var textLength = this.TextLength;

                for (var i = start; i < textLength; i++)
                {
                    var line = this.Lines[lineIndex];
                    var relativeIndex = i - line.Index;

                    if (relativeIndex < line.GetLength(textColumnIndex))
                    {
                        yield return line.GetLength(textColumnIndex) == 0
                            ? (char) 0
                            : line.GetText(textColumnIndex)[relativeIndex];
                    }
                    else
                    {
                        yield return '\n';
                        lineIndex++;
                    }
                }
            }
            else
            {
                for (var i = start; i >= 0; i--)
                {
                    var line = this.Lines[lineIndex];
                    var relativeIndex = i - line.Index;

                    if (relativeIndex == line.GetLength(textColumnIndex))
                    {
                        yield return '\n';
                        continue;
                    }

                    yield return line.GetLength(textColumnIndex) == 0
                        ? (char) 0
                        : line.GetText(textColumnIndex)[relativeIndex];

                    if (relativeIndex <= 0)
                    {
                        lineIndex--;
                    }
                }
            }
        }

        public override UndoRedoCommandBase TextRemove(int removalStart, int removalLength, int textColumnIndex)
        {
            var lineIndex = this.GetLineFromCharIndex(removalStart, textColumnIndex);

            if (lineIndex == -1)
            {
                // The user is trying to remove text that does not exist.
                // The only time I know that this could happen is if the text control has no content whatsoever
                // and the user tries to backspace or delete away more stuff.
                return null;
            }

            // The value could be -1 if the user is removing from the start of the line to make the current line's content mend with the line above,
            // or remove the current line if it has no content whatsoever.
            var relativeStart = Math.Max(0, removalStart - this.Lines[lineIndex].Index);

            if (lineIndex == this.Lines.Count - 1)
            {
                // This should only happen if we are at the end of the text and we're using delete to remove the characters
                // before this. When doing this we'd try to remove something that does not exist. So let's just skip, then.
                if (relativeStart + removalLength > this.Lines[lineIndex].GetLength(textColumnIndex))
                {
                    return null;
                }
            }

            string textUndoRemoved = null;
            if (this.UndoRedoManager.AcceptsChanges)
            {
                // TODO: This can be made faster by merging it into the loop below, instead of using the slow TextGet(int, int)
                //      (by using a StringBuilder and concating the string inside the loop)
                textUndoRemoved = this.TextGet(removalStart, removalLength, textColumnIndex);
            }

            var modifyingLineContent = true;

            var decreasingLength = removalLength;
            var currentRelativeStart = relativeStart;

            for (var i = lineIndex; i < this.LineCount; i++)
            {
                if (modifyingLineContent)
                {
                    var mendThisAndNextLine = currentRelativeStart + decreasingLength > this.Lines[i].GetLength(textColumnIndex);

                    if (mendThisAndNextLine)
                    {
                        // The removal is longer than this line, which means that the line after this one should be merged with the current.

                        if (i + 1 != this.LineCount)
                        {
                            // Move all the text anchors to the line to be mended to. Setting the TextLine will remove the text segment from
                            // the line and move it to the one to be merged into.
                            while (this.Lines[i + 1].StyledTextSegments.Count > 0)
                            {
                                var seg = this.Lines[i + 1].StyledTextSegments[0];
                                seg.TextLine = this.Lines[i];

                                // Make this update to the final removal length.
                                seg.Index += this.Lines[i].GetLength(textColumnIndex);
                            }

                            this.Lines[i].SetText(textColumnIndex, this.Lines[i].GetText(textColumnIndex) + this.Lines[i + 1].GetText(textColumnIndex));

                            // We call the "alter" so that both inserting/removing can have a common place to listen to any modification to the document.
                            this.DispatchTextSegmentAlter(new AlterTextSegmentArgs(this.Lines[i + 1], i + 1, 0, textColumnIndex));

                            // -1 in character count difference since the newline was removed.
                            this.DispatchTextSegmentRemoved(new AlterTextSegmentArgs(this.Lines[i + 1], i + 1, -1, textColumnIndex));

                            this.Lines.RemoveAt(i + 1); // Remove the line. Sayonara, adios.

                            decreasingLength--; // -1 length since one newline has been removed.
                        }
                        else
                        {
#if DEBUG
                            foreach (var textView in this.GetTextViews())
                            {
                                textView.Settings.Notifier.Error("Warning", "TextRemove out of index! Ohnoes!");
                            }
#endif
                            // This should never happen since the selection should be limited. 
                            // But I think for some very small chance, an error could appear here.
                            // TODO: Try and find the small chance for when it happens!
                        }
                    }

                    // Same check is done twice since this might be the last line, and if we had an "else" it would
                    // mean that we exit the line loop before we get to this point where we remove the text.
                    if ((currentRelativeStart + decreasingLength > this.Lines[i].GetLength(textColumnIndex)) == false)
                    {
                        // The modifying of text ends at this line, the subsequent lines only need to have their starting indexes updated.
                        modifyingLineContent = false;

                        // The decreasingLength will be 0 (zero) here if we are removing from the start of the line to mend with the above live.
                        if (decreasingLength > 0)
                        {
                            this.Lines[i].SetText(textColumnIndex, this.Lines[i].GetText(textColumnIndex).Remove(currentRelativeStart, decreasingLength));

                            // [GFX] Update line count and horizontal scroll overflow for line
                            this.DispatchTextSegmentAlter(new AlterTextSegmentArgs(this.Lines[i], i, -decreasingLength, textColumnIndex));
                        }

                        // Remove any text style that is inside the span of the removed text.
                        for (var segIdx = 0; segIdx < this.Lines[i].StyledTextSegments.Count; segIdx++)
                        {
                            var seg = this.Lines[i].StyledTextSegments[segIdx];

                            if (seg.Index >= currentRelativeStart && seg.Index + seg.GetLength(textColumnIndex) <= currentRelativeStart + decreasingLength)
                            {
                                if (seg.Style.Type != TextStyleType.Pinned)
                                {
                                    // The styled segment is completely inside the removal span and should hence be removed.
                                    seg.TextLine = null;
                                    segIdx--;
                                }
                            }
                            else if (seg.Index + seg.GetLength(textColumnIndex) > currentRelativeStart + decreasingLength)
                            {
                                // The styled segment is completely outside, and after, the removal span and should hence be moved.
                                // This is handled by the text style modifier method below.
                                //seg.Index -= decreasingLength;
                            }
                            else if (seg.Index > currentRelativeStart)
                            {
                                // The styled segment is partially removed, and nothing should have to be done.
                                // But to be safe and follow the visual representation this styled segment should be moved to the removal start.
                                seg.Index -= (seg.Index - currentRelativeStart);
                            }
                        }

                        foreach (var textView in this.GetTextViews())
                        {
                            // Modify styles for each of the attached text views
                            this.ModifyStyledTextSegments(textView, this.Lines[i], currentRelativeStart, char.MinValue, true, -decreasingLength, textColumnIndex);
                        }
                    }
                    else if (mendThisAndNextLine)
                    {
                        i--; // -1 since it allows us to stay on the same line.
                    }
                }
                else
                {
                    // This line is not one to be modified, it is a subsequent line. We only need to change its starting index to be correct.
                    this.Lines[i].Index -= removalLength;
                }
            }

            TextRemovedUndoRedoCommand undoRedoCommand = null;

            if (this.UndoRedoManager.AcceptsChanges)
            {
                undoRedoCommand = new TextRemovedUndoRedoCommand(this, textUndoRemoved, removalStart, textColumnIndex);
                this.UndoRedoManager.AddUndoCommand(undoRedoCommand);
            }

            this.IsModified = true;

            return undoRedoCommand;
        }

        public override void TextAppendLine(string text, int textColumnIndex)
        {
            // Special consideration is taken if it is the first line being appended. Then it begins on index 0 (obviously).
            var newLine = new TextLine(this.TextLength > 0 ? this.TextLength + 1 : 0, text);

            this.Lines.Add(newLine);

            this.DispatchTextSegmentAlter(new AlterTextSegmentArgs(newLine, this.Lines.Count - 1, newLine.GetLength(textColumnIndex) + 1, textColumnIndex));

            foreach (var textView in this.GetTextViews())
            {
                this.TextSegmentStyledManager.SearchAndApplyTo(textView, newLine, 0, newLine.GetLength(textColumnIndex), textColumnIndex);
            }
        }

        public override void FakeFinalizingKey(int index, int textColumnIndex)
        {
            var lineIndex = this.GetLineFromCharIndex(index, textColumnIndex);

            if (lineIndex == -1)
            {
                return;
            }

            var textLine = this.Lines[lineIndex];

            foreach (var textView in this.GetTextViews())
            {
                this.ModifyStyledTextSegments(textView, textLine, index - textLine.Index, '\0', false, 0, textColumnIndex);
            }
        }

        public override WordSegment GetWord(int globalIndex, ITextSegment insideSegment, bool strict, int textColumnIndex)
        {
            return insideSegment.GetText(textColumnIndex).GetWord(globalIndex, insideSegment.Index, strict);
        }

        public override WordSegment GetWord(int globalIndex, bool strict, int textColumnIndex)
        {
            var lineIndex = this.GetLineFromCharIndex(globalIndex, textColumnIndex);

            if (lineIndex == -1)
            {
                return null;
            }

            var text = this.GetLineText(lineIndex, textColumnIndex);

            if (text.Length == 0)
            {
                return null;
            }

            return text.GetWord(globalIndex, this.GetFirstCharIndexFromLine(lineIndex), strict);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textView"></param>
        /// <param name="textLine"></param>
        /// <param name="relativeIndex">Relative index is the index before the new character, so text[relativeIndex] is the <see cref="c"/>.</param>
        /// <param name="c"></param>
        /// <param name="alterTextStyleIndexes"></param>
        /// <param name="length"></param>
        /// <param name="textColumnIndex"></param>
        private void ModifyStyledTextSegments(ITextView textView, TextLine textLine, int relativeIndex, char c, bool alterTextStyleIndexes, int length, int textColumnIndex)
        {
            var isErase = c == '\0' && length <= 0;

            if (isErase)
            {
                length *= -1;
            }

            IEnumerable<ITextSegmentStyled> befores = null;
            IEnumerable<ITextSegmentStyled> afters = null;

            var startSearch = Math.Max(0, relativeIndex - 1);

            const bool foundMatch = false;

            if (alterTextStyleIndexes)
            {
                var offsetCount = 0;
                for (var n = 0; n < textLine.StyledTextSegments.Count - offsetCount; n++)
                {
                    if (textLine.StyledTextSegments[n].Index > textLine.GetLength(textColumnIndex))
                    {
                        // The style starts after the end of the line, which is silly and should be filtered out.
                        textLine.StyledTextSegments[n].TextLine = null;
                        n--;
                    }
                    else if (isErase && textLine.StyledTextSegments[n].Style.Type == TextStyleType.Automatic && textLine.StyledTextSegments[n].Index >= relativeIndex && textLine.StyledTextSegments[n].Index + textLine.StyledTextSegments[n].GetLength(textColumnIndex) <= relativeIndex + length)
                    {
                        // The whole styled text segment was inside the removed range of text.
                        // It does not exist anymore and is automatically eligible for removing.
                        textLine.StyledTextSegments[n].TextLine = null;
                        n--;
                    }
                    else if (textLine.StyledTextSegments[n].Style.Type == TextStyleType.Automatic && textLine.StyledTextSegments[n].Contains(relativeIndex, length, isErase, true))
                    {
                        if (befores == null)
                        {
                            // This will search from the character before the change, the character before a new addition,
                            // or the character before the character that was removed.
                            befores = this.FindStyles(textView, textLine, startSearch, textColumnIndex);

                            // If a new addition, we search from the character after the change (so "pol<new space>ice" will search on "i" and not <new space>).
                            // When removing we only need to search from where the removal took place.
                            var afterSearchIdx = isErase == false ? Math.Min(textLine.GetLength(textColumnIndex), startSearch + 2) : startSearch + 1;

                            afters = this.FindStyles(textView, textLine, afterSearchIdx, textColumnIndex);
                        }

                        var current = textLine.StyledTextSegments[n];

                        if (befores != null || afters != null)
                        {
                            ITextSegmentStyled before = null;
                            ITextSegmentStyled after = null;

                            if (befores != null)
                            {
                                foreach (var b in befores)
                                {
                                    if (b.Style.NameKey == current.Style.NameKey)
                                    {
                                        before = b;
                                        break;
                                    }
                                }
                            }

                            if (afters != null)
                            {
                                foreach (var a in afters)
                                {
                                    if (a.Style.NameKey == current.Style.NameKey)
                                    {
                                        after = a;
                                        break;
                                    }
                                }
                            }

                            if (before == null)
                            {
                                if (after == null)
                                {
                                    // The styled text segment is no more since it is no longer found.
                                    current.TextLine = null;
                                    n--;
                                    continue;
                                }

                                before = after;
                                after = null;
                            }
                            else if (after != null && before.Object != null && before.Object.Equals(after.Object))
                            {
                                after = null;
                            }

                            if (before.Index == current.Index && before.GetLength(textColumnIndex) == current.GetLength(textColumnIndex)
                                && ((before.Object != null && before.Object.Equals(current.Object)) || before.Object == null && current.Object == null))
                            {
                                continue; // No change was done. Just don't do any changes.
                            }

                            //foundMatch = true;

                            if (before.Contains(current.Index, current.GetLength(textColumnIndex), true, true))
                            {
                                current.TextLine = null;
                                n--;
                            }

                            ((TextAnchor) before).TextLine = textLine;
                            offsetCount++;

                            if (after != null)
                            {
                                ((TextAnchor) after).TextLine = textLine;
                                offsetCount++;
                            }
                        }
                        else
                        {
                            // The styled text segment is no more since it is no longer found.
                            current.TextLine = null;
                            n--;
                        }
                    }
                    else if (textLine.StyledTextSegments[n].Index > relativeIndex)
                    {
                        if (isErase)
                        {
                            textLine.StyledTextSegments[n].Index -= length;
                        }
                        else
                        {
                            textLine.StyledTextSegments[n].Index += length;
                        }
                    }
                }
            }

            if (foundMatch == false)
            {
#if DEBUG
                var foundOne = 
#endif
                    this.TextSegmentStyledManager.SearchAndApplyTo(textView, textLine, startSearch, 1, CharacterIsFinalizer(c), textColumnIndex);
            }
        }

        private static bool CharacterIsFinalizer(char c)
        {
            if (c == '\0')
            {
                return true;
            }

            if (Char.IsWhiteSpace(c))
            {
                return true;
            }

            if (Char.IsPunctuation(c))
            {
                return true;
            }

            if (Char.IsSeparator(c))
            {
                return true;
            }

            if (Char.IsControl(c))
            {
                return true;
            }

            if (Char.IsSymbol(c))
            {
                return true;
            }

            switch (c)
            {
                case '\n':
                    return true;
                default:
                    return false;
            }
        }

        private IEnumerable<ITextSegmentStyled> FindStyles(ITextView textView, ITextSegment textLine, int relativeIndex, int textColumnIndex)
        {
            foreach (var style in textView.GetTextStyles())
            {
                var foundStyle = style.FindStyledTextSegment(textView, textLine, this, relativeIndex, 0, textColumnIndex);

                if (foundStyle != null)
                {
                    yield return foundStyle;
                }
            }
        }

        public override UndoRedoCommandBase TextInsert(int start, string text, int textColumnIndex)
        {
            if (this.Lines.Count == 0)
            {
                this.TextAppendLine(String.Empty, textColumnIndex);
            }

            var lineIndex = this.GetLineFromCharIndex(start, textColumnIndex);

            var relativeIndex = Math.Min(this.Lines[lineIndex].GetLength(textColumnIndex), start - this.Lines[lineIndex].Index);
            var length = text.Length;
            var origLength = length;
            var characterAdditions = 0;

            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    #region Finalize line and insert new one after current

                    // Get the text that is after the newline character
                    var newText = this.Lines[lineIndex].GetText(textColumnIndex).Substring(relativeIndex, this.Lines[lineIndex].GetLength(textColumnIndex) - relativeIndex);

                    var diff1 = this.Lines[lineIndex].GetLength(textColumnIndex) - relativeIndex;

                    // Remove the text that is after the newline character from this current line, since it will be placed on the next line instead.
                    this.Lines[lineIndex].SetText(textColumnIndex, this.Lines[lineIndex].GetText(textColumnIndex).Remove(relativeIndex, diff1));

                    // [GFX] Update line count and horizontal scroll overflow for first line.
                    this.DispatchTextSegmentAlter(new AlterTextSegmentArgs(this.Lines[lineIndex], lineIndex, -diff1, textColumnIndex));

                    this.Lines.Insert(lineIndex + 1, new TextLine(start + i + 1 + (length - origLength), newText));

                    #region Move the text anchors to the new line if they are after or on the newline

                    for (var taIndex = 0; taIndex < this.Lines[lineIndex].StyledTextSegments.Count; taIndex++)
                    {
                        // Move if after the newline or if the newline is on the start of the line (ie. for certain want to move everything).
                        if (this.Lines[lineIndex].StyledTextSegments[taIndex].Index <= relativeIndex && relativeIndex != 0)
                        {
                            continue;
                        }

                        // Remove as much from the index as the offset from where the change started on the previous line.
                        this.Lines[lineIndex].StyledTextSegments[taIndex].Index -= relativeIndex;
                        this.Lines[lineIndex].StyledTextSegments[taIndex].TextLine = this.Lines[lineIndex + 1];

                        taIndex--;
                    }

                    #endregion

                    // [GFX] Update line count and horizontal scroll overflow for new line
                    // The number of changed characters will be the number of characters so far inserted
                    // before this newline, since that is the number of new characters before we split between the lines.
                    // And we add one character since the newline is not accounted for as an actual character.
                    this.DispatchTextSegmentAdded(new AlterTextSegmentArgs(this.Lines[lineIndex + 1], lineIndex + 1, characterAdditions + 1, textColumnIndex));

                    // And then we start from the beginning with this counter since it will be on a new line and the TextSegmentAlter call
                    // has already been handled for the already added number of characters.
                    characterAdditions = 0;

                    #endregion

                    foreach (var textView in this.GetTextViews())
                    {
                        // Modify text styles for the current line.
                        this.ModifyStyledTextSegments(textView, this.Lines[lineIndex], relativeIndex, text[i], true, 1, textColumnIndex);
                    }

                    relativeIndex = 0;

                    lineIndex++;

                    continue;
                }

                this.Lines[lineIndex].SetText(textColumnIndex, this.Lines[lineIndex].GetText(textColumnIndex).Insert(relativeIndex, text[i].ToString()));

                characterAdditions++;

                // Modify text styles.
                foreach (var textView in this.GetTextViews())
                {
                    this.ModifyStyledTextSegments(textView, this.Lines[lineIndex], relativeIndex, text[i], true, 1, textColumnIndex);
                }

                relativeIndex++;
            }

            // [GFX] Update line count and horizontal scrolling overflow for current line
            this.DispatchTextSegmentAlter(new AlterTextSegmentArgs(this.Lines[lineIndex], lineIndex, characterAdditions, textColumnIndex));

            if (length != 0)
            {
                for (var i = lineIndex + 1; i < this.Lines.Count; i++)
                {
                    this.Lines[i].Index += length;
                }
            }

            TextAddedUndoRedoCommand undoRedoCommand = null;

            if (this.UndoRedoManager.AcceptsChanges && length > 0)
            {
                undoRedoCommand = new TextAddedUndoRedoCommand(this, text, start, textColumnIndex);
                this.UndoRedoManager.AddUndoCommand(undoRedoCommand);
            }

            this.IsModified = true;

            return undoRedoCommand;
        }

        public override int TextLength
        {
            get
            {
                if (this.Lines.Count == 0)
                {
                    return 0;
                }

                return this.Lines[this.Lines.Count - 1].Index + this.Lines[this.Lines.Count - 1].GetLength(0); // Text.Length;
            }
        }

        public override ITextSegmentVisual GetVisualTextSegment(int lineIndex)
        {
            return this.Lines[lineIndex];
        }

        public override void InitializeTextColumn(int textColumnIndex)
        {
            for (var i = 0; i < this.LineCount; i++)
            {
                if (String.IsNullOrEmpty(this.Lines[i].GetText(textColumnIndex)))
                {
                    this.Lines[i].SetText(textColumnIndex, String.Empty);
                }
            }
        }

        public override string GetLineText(int lineIndex, int textColumnIndex)
        {
            try
            {
                return this.Lines[lineIndex].GetText(textColumnIndex);
            }
            catch
            {
                // If the line does not exist, then we should get a null back instead.
                return null;
            }
        }

        public override int GetFirstCharIndexFromLine(int lineIndex)
        {
            if (lineIndex >= this.Lines.Count)
            {
                return -1;
            }

            return this.Lines[lineIndex].Index;
        }

        public override int GetLineLength(int lineIndex, int textColumnIndex)
        {
            if (lineIndex >= this.Lines.Count)
            {
                return -1;
            }

            return this.Lines[lineIndex].GetLength(textColumnIndex);
        }

        public override int GetLineFromCharIndex(int index, int textColumnIndex)
        {
            if (index == -1 || this.Lines.Count == 0)
            {
                return -1;
            }

            // Set the low number of the array
            var lowNum = 0;

            // Set the high number of the array
            var highNum = this.Lines.Count - 1;

            // Loop while the low number is less or equal to the high number
            while (lowNum <= highNum)
            {
                // Get the middle point in the array
                var midNum = (lowNum + highNum)/2;
                var midLine = this.Lines[midNum];

                // Now start checking the values
                if (index < midLine.Index)
                {
                    // Search value is lower than this index of our array, so set the high number equal to the middle number - 1.
                    highNum = midNum - 1;
                }
                else if (index > midLine.Index)
                {
                    if (index <= midLine.Index + midLine.GetLength(textColumnIndex))
                    {
                        return midNum;
                    }

                    // Search value is higher than this index of our array, so set the low number to the middle number + 1
                    lowNum = midNum + 1;
                }
                else if (index == midLine.Index)
                {
                    return midNum;
                }
            }

            // We allow too high values to be sent in here. Even int.MaxValue should get the last line.
            return this.Lines.Count - 1;
        }

        public override int LineCount
        {
            get { return this.Lines.Count; }
        }

        ~TextDocumentByLines()
        {
            this.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}