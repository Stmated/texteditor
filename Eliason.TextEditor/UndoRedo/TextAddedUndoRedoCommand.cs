#region

using System;

#endregion

namespace Eliason.TextEditor.UndoRedo
{
    public sealed class TextAddedUndoRedoCommand : TextBaseUndoRedoCommand
    {
        public TextAddedUndoRedoCommand(ITextDocument textDocument, string inputtedText, int startIndex, int textColumnIndex)
            : base(textDocument, startIndex, textColumnIndex)
        {
            this.InputtedText = inputtedText;
        }

        public string InputtedText { get; private set; }

        public override string Text
        {
            get
            {
                //return "";

                if (this.InputtedText.Length > 15)
                {
                    return String.Format("add '{0}...'", this.InputtedText.Substring(0, 15).Replace("\n", "\\n"));
                }

                return String.Format("add '{0}'", this.InputtedText.Replace("\n", "\\n"));
            }
        }

        public override void Undo()
        {
            TextDocument.UndoRedoManager.AcceptsChanges = false;
            TextDocument.TextRemove(StartIndex, this.InputtedText.Length, this.TextColumnIndex);
            TextDocument.UndoRedoManager.AcceptsChanges = true;
        }

        public override void Redo()
        {
            TextDocument.UndoRedoManager.AcceptsChanges = false;
            TextDocument.TextInsert(StartIndex, this.InputtedText, this.TextColumnIndex);
            TextDocument.UndoRedoManager.AcceptsChanges = true;
        }
    }
}