#region

using System;

#endregion

namespace Eliason.TextEditor.UndoRedo
{
    public sealed class TextRemovedUndoRedoCommand : TextBaseUndoRedoCommand
    {
        public TextRemovedUndoRedoCommand(ITextDocument textDocument, string previousText, int startIndex, int textColumnIndex)
            : base(textDocument, startIndex, textColumnIndex)
        {
            this.PreviousText = previousText;
        }

        public string PreviousText { get; private set; }

        public override string Text
        {
            get
            {
                //return "";

                if (this.PreviousText.Length > 15)
                {
                    return String.Format("remove '{0}...'", this.PreviousText.Substring(0, 15).Replace("\n", "\\n"));
                }

                return String.Format("remove '{0}'", this.PreviousText.Replace("\n", "\\n"));
            }
        }

        public override void Undo()
        {
            TextDocument.UndoRedoManager.AcceptsChanges = false;
            TextDocument.TextInsert(StartIndex, this.PreviousText, this.TextColumnIndex);
            TextDocument.UndoRedoManager.AcceptsChanges = true;
        }

        public override void Redo()
        {
            TextDocument.UndoRedoManager.AcceptsChanges = false;
            TextDocument.TextRemove(StartIndex, this.PreviousText.Length, this.TextColumnIndex);
            TextDocument.UndoRedoManager.AcceptsChanges = true;
        }
    }
}