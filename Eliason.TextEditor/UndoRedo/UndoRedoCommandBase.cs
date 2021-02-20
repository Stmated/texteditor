using System.ComponentModel;

namespace Eliason.TextEditor.UndoRedo
{
    /// <summary>
    ///   UndoRedoCommand is an abstract class that represents an undoable
    ///   or redoable operation or command. It provides virtual Undo()
    ///   and Redo() methods which your derived command classes can
    ///   override in order to implement actual undo/redo functionality.
    ///   In a derived command class, you also have the option of
    ///   not overriding the Undo() and Redo() methods. Instead, you
    ///   can treat the derived command class like a data class and
    ///   simply provide extra fields, properties, or methods that an
    ///   external class (one that implements IUndoRedoHandler) can use to
    ///   perform the actual undo/redo functionality.
    /// </summary>
    public abstract class UndoRedoCommandBase : IUndoRedo
    {
        #region IUndoRedo Members

        public bool UndoPair { get; set; }
        public bool RedoPair { get; set; }

        /// <summary>
        ///   GetText() should return a short description of the
        ///   user operation associated with this command. For example,
        ///   a graphics line drawing operation might have the
        ///   text, "Draw Line". This method can be used to update
        ///   the Text property of an undo menu item. For example,
        ///   instead of just displaying "&Undo", the Text property
        ///   of an undo menu item can be augmented to "&Undo Draw Line".
        /// </summary>
        /// <returns>Short description of the command.</returns>'
        [Localizable(true)]
        public virtual string Text
        {
            get { return @""; }
        }

        /// <summary>
        ///   Perform undo of this command.
        /// </summary>
        public virtual void Undo()
        {
            // Empty implementation.
        }

        /// <summary>
        ///   Perform redo of this command.
        /// </summary>
        public virtual void Redo()
        {
            // Empty implementation.
        }

        #endregion
    }
}