using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Eliason.TextEditor.UndoRedo
{
    /// <summary>
    ///   UndoRedoManager is a concrete class that maintains the undo list and redo stack data structures. It also provides methods that
    ///   tell you whether there is something to undo or redo. The class is designed to be used directly in undo/redo menu item handlers,
    ///   and undo/redo menu item state update functions.
    /// 
    ///   This base code has been fetched from: http://www.codeproject.com/KB/cs/undo_support.aspx
    ///   But has been heavily rewritten since its code was not really that pretty.
    /// </summary>
    public class UndoRedoManager : IDisposable
    {
        private readonly Stack<UndoRedoInfo> _redoStack;
        private readonly List<UndoRedoInfo> _undoList;
        private int _maxUndoLevel = 10; // 10 as a low default.

        /// <summary>
        ///   Constructor which initializes the manager with up to 8 levels of undo/redo.
        /// </summary>
        public UndoRedoManager()
        {
            this._undoList = new List<UndoRedoInfo>();
            this._redoStack = new Stack<UndoRedoInfo>();

            this.AcceptsChanges = true;
        }

        public bool AcceptsChanges { get; set; }

        /// <summary>
        ///   Property for the maximum undo level.
        /// </summary>
        public int MaxUndoLevel
        {
            get { return this._maxUndoLevel; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "The max undo level cannot be a negative number.");
                }

                // To keep things simple, if you change the undo level, we clear all outstanding undo/redo commands.
                if (value != this._maxUndoLevel)
                {
                    this.ClearUndoRedo();
                    this._maxUndoLevel = value;
                }
            }
        }

        /// <summary>
        ///   Check if there is something to undo. Use this method to decide
        ///   whether your application's "Undo" menu item should be enabled or disabled.
        /// </summary>
        /// <returns>Returns true if there is something to undo, false otherwise.</returns>
        private bool CanUndo
        {
            get { return this._undoList.Count > 0; }
        }

        /// <summary>
        ///   Check if there is something to redo. Use this method to decide
        ///   whether your application's "Redo" menu item should be enabled or disabled.
        /// </summary>
        /// <returns>Returns true if there is something to redo, false otherwise.</returns>
        private bool CanRedo
        {
            get { return this._redoStack.Count > 0; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.ClearUndoRedo();
        }

        #endregion

        /// <summary>
        ///   Register a new undo command. Use this method after your
        ///   application has performed an operation/command that is undoable.
        /// </summary>
        /// <param name = "cmd">New command to add to the manager.</param>
        public void AddUndoCommand(IUndoRedo cmd)
        {
            this.AddUndoCommand(cmd, null);
        }

        /// <summary>
        ///   Register a new undo command along with an undo handler. 
        ///   The undo handler is used to perform the actual undo or redo operation later when requested.
        /// </summary>
        /// <param name = "cmd">New command to add to the manager.</param>
        /// <param name = "undoHandler">Undo handler to perform the actual undo/redo operation.</param>
        private void AddUndoCommand(IUndoRedo cmd, IUndoRedoHandler undoHandler)
        {
            if (cmd == null)
            {
                throw new NullReferenceException("The cmd sent to AddUndoCommand should not be null.");
            }

            if (this._maxUndoLevel == 0)
            {
                return;
            }

            if (this._undoList.Count == this._maxUndoLevel)
            {
                this._undoList.RemoveAt(0);
            }

            // Insert the new undoable command into the undo list.
            this._undoList.Add(new UndoRedoInfo(cmd, undoHandler));

            // Clear the redo stack.
            this.ClearRedo();
        }

        /// <summary>
        ///   Clear the internal undo/redo data structures. 
        ///   Use this method when your application performs an operation that cannot be undone.
        ///   For example, when the user "saves" or "commits" all the changes in the application.
        /// </summary>
        public void ClearUndoRedo()
        {
            this.ClearUndo();
            this.ClearRedo();
        }

        /// <summary>
        ///   Perform the undo operation. If an undo handler is specified, it
        ///   will be used to perform the actual operation. Otherwise, the command instance is asked to perform the undo.
        /// </summary>
        public void Undo()
        {
            if (this.CanUndo == false)
            {
                return;
            }

            // Remove newest entry from the undo list.
            var info = this._undoList[this._undoList.Count - 1];
            this._undoList.RemoveAt(this._undoList.Count - 1);

            // Perform the undo.
            info.Undo();

            // Now the command is available for redo. Push it onto the redo stack.
            this._redoStack.Push(info);

            if (info.UndoPair)
            {
                this.Undo();
            }
        }

        /// <summary>
        ///   Perform the redo operation. If an undo handler is specified, it will be used to perform the actual operation. 
        ///   Otherwise, the command instance is asked to perform the redo.
        /// </summary>
        public void Redo()
        {
            if (this.CanRedo == false)
            {
                return;
            }

            // Remove newest entry from the redo stack.
            var info = this._redoStack.Pop();

            // Perform the redo.
            info.Redo();

            // Now the command is available for undo again. Put it back
            // into the undo list.
            this._undoList.Add(info);

            if (info.RedoPair)
            {
                this.Redo();
            }
        }

        /// <summary>
        ///   Get the text value of the next undo command. Use this method
        ///   to update the Text property of your "Undo" menu item if
        ///   desired. For example, the text value for a command might be
        ///   "Draw Circle". This allows you to change your menu item Text
        ///   property to "&Undo Draw Circle".
        /// </summary>
        /// <returns>Text value of the next undo command.</returns>
        public string GetUndoText()
        {
            var cmd = this.PeekUndoCommand();

            if (cmd == null)
            {
                return String.Empty;
            }

            return cmd.Text;
        }

        /// <summary>
        ///   Get the text value of the next redo command. Use this method
        ///   to update the Text property of your "Redo" menu item if desired.
        ///   For example, the text value for a command might be "Draw Line".
        ///   This allows you to change your menu item text to "&Redo Draw Line".
        /// </summary>
        /// <returns>Text value of the next redo command.</returns>
        public string GetRedoText()
        {
            var cmd = this.PeekRedoCommand();

            if (cmd == null)
            {
                return String.Empty;
            }

            return cmd.Text;
        }

        /// <summary>
        ///   Get the next (or newest) undo command. It does not remove the command from the undo list.
        /// </summary>
        /// <returns>The next undo command.</returns>
        public IUndoRedo PeekUndoCommand()
        {
            if (this._undoList.Count == 0)
            {
                return null;
            }

            return this._undoList[this._undoList.Count - 1].UndoRedoObject;
        }

        /// <summary>
        ///   Get the next redo command. It does not remove the command from the redo stack.
        /// </summary>
        /// <returns>The next redo command.</returns>
        public IUndoRedo PeekRedoCommand()
        {
            if (this._redoStack.Count == 0)
            {
                return null;
            }

            return this._redoStack.Peek().UndoRedoObject;
        }

        /// <summary>
        ///   Retrieve all of the undo commands. Useful for debugging,
        ///   to analyze the contents of the undo list.
        /// </summary>
        /// <returns>Array of commands for undo.</returns>
        public IEnumerable<IUndoRedo> GetUndoCommands()
        {
            foreach (var info in this._undoList)
            {
                yield return info.UndoRedoObject;
            }
        }

        /// <summary>
        ///   Retrieve all of the redo commands. Useful for debugging,
        ///   to analyze the contents of the redo stack.
        /// </summary>
        /// <returns>Array of commands for redo.</returns>
        public IEnumerable<IUndoRedo> GetRedoCommands()
        {
            foreach (var info in this._redoStack)
            {
                yield return info.UndoRedoObject;
            }
        }

        /// <summary>
        ///   Clear the contents of the undo list.
        /// </summary>
        private void ClearUndo()
        {
            for (var i = 0; i < this._undoList.Count; i++)
            {
                this._undoList[i].Clear();
            }

            this._undoList.Clear();
        }

        /// <summary>
        ///   Clear the contents of the redo stack.
        /// </summary>
        private void ClearRedo()
        {
            while (this._redoStack.Count > 0)
            {
                this._redoStack.Pop().Clear();
            }

            this._redoStack.Clear();
        }

        /// <summary>
        ///   UndoRedoInfo is a nested, private class that is used as the
        ///   data type in the undo list and redo stack. It just stores
        ///   a reference to a command, and optionally, an undo handler.
        /// </summary>
        private class UndoRedoInfo : IUndoRedo
        {
            public UndoRedoInfo(IUndoRedo undoable, IUndoRedoHandler doHandler)
            {
                this.UndoRedoObject = undoable;
                this.UndoRedoHandler = doHandler;
            }

            public IUndoRedo UndoRedoObject { get; private set; }
            private IUndoRedoHandler UndoRedoHandler { get; set; }

            #region IUndoRedo Members

            public bool UndoPair
            {
                get { return this.UndoRedoObject.UndoPair; }
            }

            public bool RedoPair
            {
                get { return this.UndoRedoObject.RedoPair; }
            }

            [Localizable(true)]
            public string Text
            {
                get { return this.UndoRedoObject.Text; }
            }

            public void Undo()
            {
                if (this.UndoRedoHandler != null)
                {
                    this.UndoRedoHandler.Undo(this.UndoRedoObject);
                }
                else
                {
                    this.UndoRedoObject.Undo();
                }
            }

            public void Redo()
            {
                if (this.UndoRedoHandler != null)
                {
                    this.UndoRedoHandler.Redo(this.UndoRedoObject);
                }
                else
                {
                    this.UndoRedoObject.Redo();
                }
            }

            #endregion

            public void Clear()
            {
                this.UndoRedoHandler = null;
                this.UndoRedoObject = null;
            }
        }
    }
}