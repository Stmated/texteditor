using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Eliason.TextEditor.UndoRedo;

namespace Eliason.TextEditor
{
    public interface ITextEditor : ITextBaseCommon
    {
        int CurrentTextColumnIndex { get; }

        ITextDocument TextDocument { get; }

        /// <summary>
        /// TODO: Remove this, since the text editor should not be locked to a file, it should be able to work against any source.
        /// </summary>
        string CurrentFilePath { get; set; }

        bool IsVirtual { get; }

        CultureInfo Language { get; set; }

        [Localizable(true)]
        string Description { get; set; }

        string FilterStringLine(string text);

        string TextGet();

        void Open();

        void Open(string filePath);

        bool Save();

        bool SaveAs();

        bool SaveAs(string filePath);

        void Undo();

        void Redo();

        UndoRedoCommandBase TextInsert(int index, string tex);

        UndoRedoCommandBase TextRemove(int index, int length);

        void TextAppendLine(string text);

        string TextGet(int start, int length);

        IEnumerable<char> TextGetStream(int start, bool right);

        char GetCharFromIndex(int index);

        WordSegment GetWord(int globalIndex, bool strict);

        WordSegment GetWord(int globalIndex, ITextSegment insideSegment, bool strict);

        string GetLineText(int lineIndex);

        int GetLineLength(int lineIndex);

        int GetLineFromCharIndex(int index);

        ISettings Settings { get; }
    }
}