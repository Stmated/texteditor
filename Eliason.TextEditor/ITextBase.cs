using System;
using System.Collections.Generic;
using Eliason.TextEditor.TextStyles;
using Eliason.TextEditor.UndoRedo;

namespace Eliason.TextEditor
{
    public interface ITextBaseCommon : IDisposable
    {
        int TextLength { get; }

        int LineCount { get; }

        bool IsModified { get; set; }

        event EventHandler Modified;

        void Clear();

        int GetFirstCharIndexFromLine(int lineIndex);

        // TODO: This is too coupled with a line-based design and does not really suit other designs, such as node-based ones.
        ITextSegmentVisual GetVisualTextSegment(int lineIndex);

        ITextSegmentStyled CreateStyledTextSegment(TextStyleBase style);
    }

    public interface ITextBase : ITextBaseCommon
    {
        UndoRedoCommandBase TextInsert(int index, string text, int textColumnIndex);

        UndoRedoCommandBase TextRemove(int index, int length, int textColumnIndex);

        void TextAppendLine(string text, int textColumnIndex);

        string TextGet(int start, int length, int textColumnIndex);

        IEnumerable<char> TextGetStream(int start, bool right, int textColumnIndex);

        char GetCharFromIndex(int index, int textColumnIndex);

        WordSegment GetWord(int globalIndex, bool strict, int textColumnIndex);

        WordSegment GetWord(int globalIndex, ITextSegment insideSegment, bool strict, int textColumnIndex);

        string GetLineText(int lineIndex, int textColumnIndex);

        int GetLineLength(int lineIndex, int textColumnIndex);

        int GetLineFromCharIndex(int index, int textColumnIndex);
    }
}