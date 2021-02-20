using System.ComponentModel;

namespace Eliason.TextEditor.UndoRedo
{
    /// <summary>
    ///   IUndoRedo is an interface for undo/redo support that is
    ///   implemented by the UndoRedoCommand class.
    /// </summary>
    public interface IUndoRedo
    {
        bool UndoPair { get; }

        bool RedoPair { get; }

        [Localizable(true)]
        string Text { get; }

        void Undo();

        void Redo();
    }
}