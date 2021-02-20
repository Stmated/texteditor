#region

#endregion

namespace Eliason.TextEditor.UndoRedo
{
    public class TextBaseUndoRedoCommand : UndoRedoCommandBase
    {
        public TextBaseUndoRedoCommand(ITextDocument textDocument, int startIndex, int textColumnIndex)
        {
            this.StartIndex = startIndex;
            this.TextDocument = textDocument;
            this.TextColumnIndex = textColumnIndex;
        }

        public ITextDocument TextDocument { get; private set; }
        public int StartIndex { get; private set; }
        public int TextColumnIndex { get; private set; }
    }
}