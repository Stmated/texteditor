using System;
using System.Collections.Generic;
using Eliason.TextEditor.UndoRedo;

namespace Eliason.TextEditor
{
    public interface ITextDocument : ITextBase
    {
        event EventHandler<AlterTextSegmentArgs> TextSegmentAlter;
        event EventHandler<AlterTextSegmentArgs> TextSegmentRemoved;
        event EventHandler<AlterTextSegmentArgs> TextSegmentAdded;

        int ReferenceCount { get; }
        void RegisterTextView(ITextView textView);

        ITextDocumentRenderer GetRenderer(ITextView textView);

        IEnumerable<ITextView> GetTextViews();

        ITextSegmentStyledManager TextSegmentStyledManager { get; }

        /// <summary>
        /// TODO: Move this into the ITextView, since each different text view should have a separate undo/redo stack? Or maybe that's stupid?...
        /// </summary>
        UndoRedoManager UndoRedoManager { get; }

        void FakeFinalizingKey(int index, int textColumnIndex);

        void InitializeTextColumn(int textColumnIndex);
    }
}