namespace Eliason.TextEditor.UndoRedo
{
    /// <summary>
    ///   Optional interface that your application classes can implement
    ///   in order to perform the actual undo/redo functionality.
    ///   It's optional because you can choose to implement undo/redo
    ///   functionality solely within the derived command classes.
    /// </summary>
    public interface IUndoRedoHandler
    {
        void Undo(IUndoRedo cmd);
        void Redo(IUndoRedo cmd);
    }
}