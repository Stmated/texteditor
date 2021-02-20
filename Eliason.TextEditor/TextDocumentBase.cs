using System;
using System.Collections.Generic;
using System.Text;
using Eliason.TextEditor.UndoRedo;

namespace Eliason.TextEditor
{
    public abstract class TextDocumentBase : ITextDocument
    {
        public event EventHandler<AlterTextSegmentArgs> TextSegmentAlter;

        public event EventHandler<AlterTextSegmentArgs> TextSegmentRemoved;

        public event EventHandler<AlterTextSegmentArgs> TextSegmentAdded;

        /// <summary>
        ///   Occurs when the text has been modified in any way.
        /// </summary>
        public event EventHandler Modified;

        private readonly List<ITextView> textViews = new List<ITextView>();

        private bool isModified;

        public TextDocumentBase()
        {
            this.UndoRedoManager = new UndoRedoManager();
        }

        public void Dispose()
        {
            this.Clear();

            if (this.TextSegmentStyledManager != null)
            {
                this.TextSegmentStyledManager.Dispose();
                this.TextSegmentStyledManager = null;
            }

            if (this.UndoRedoManager != null)
            {
                this.UndoRedoManager.Dispose();
                this.UndoRedoManager = null;
            }
        }

        public int ReferenceCount
        {
            get { return this.textViews.Count; }
        }

        public void RegisterTextView(ITextView textView)
        {
            this.textViews.Add(textView);
            textView.Disposed += this.textView_Disposed;
        }

        private void textView_Disposed(object sender, EventArgs e)
        {
            this.textViews.Remove(sender as ITextView);

            if (this.ReferenceCount == 0)
            {
                this.Dispose();
            }
        }

        public IEnumerable<ITextView> GetTextViews()
        {
            return this.textViews;
        }

        public ITextSegmentStyledManager TextSegmentStyledManager { get; protected set; }

        public UndoRedoManager UndoRedoManager { get; private set; }

        public bool IsModified
        {
            get { return this.isModified; }

            set
            {
                this.isModified = value;

                if (this.Modified != null)
                {
                    this.Modified(this, EventArgs.Empty);
                }
            }
        }

        public abstract void InitializeTextColumn(int textColumnIndex);

        protected void DispatchTextSegmentAlter(AlterTextSegmentArgs e)
        {
            if (this.TextSegmentAlter != null)
            {
                this.TextSegmentAlter(this, e);
            }
        }

        protected void DispatchTextSegmentRemoved(AlterTextSegmentArgs e)
        {
            if (this.TextSegmentRemoved != null)
            {
                this.TextSegmentRemoved(this, e);
            }
        }

        protected void DispatchTextSegmentAdded(AlterTextSegmentArgs e)
        {
            if (this.TextSegmentAdded != null)
            {
                this.TextSegmentAdded(this, e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length">If a negative integer, it will get all the text.</param>
        /// <param name="textColumnIndex">If a negative integer, it will get the text of all text columns.</param>
        /// <returns></returns>
        public string TextGet(int start, int length, int textColumnIndex)
        {
            var sb = new StringBuilder();

            foreach (var c in this.TextGetStream(start, true, textColumnIndex))
            {
                sb.Append(c);

                // If length is -1 it will never equal 0, and will hence read everything it gets its hands on.
                if (--length == 0)
                {
                    break;
                }
            }

            return sb.ToString();
        }

        public abstract ITextDocumentRenderer GetRenderer(ITextView textView);

        public abstract void FakeFinalizingKey(int index, int textColumnIndex);

        public abstract int TextLength { get; }

        public abstract int LineCount { get; }

        public abstract UndoRedoCommandBase TextInsert(int index, string text, int textColumnIndex);

        public abstract UndoRedoCommandBase TextRemove(int index, int length, int textColumnIndex);

        public abstract void Clear();

        public abstract void TextAppendLine(string text, int textColumnIndex);

        public abstract IEnumerable<char> TextGetStream(int start, bool right, int textColumnIndex);

        public abstract char GetCharFromIndex(int index, int textColumnIndex);

        public abstract WordSegment GetWord(int globalIndex, bool strict, int textColumnIndex);

        public abstract WordSegment GetWord(int globalIndex, ITextSegment insideSegment, bool strict, int textColumnIndex);

        public abstract string GetLineText(int lineIndex, int textColumnIndex);

        public abstract int GetFirstCharIndexFromLine(int lineIndex);

        public abstract int GetLineLength(int lineIndex, int textColumnIndex);

        public abstract int GetLineFromCharIndex(int index, int textColumnIndex);

        public abstract ITextSegmentVisual GetVisualTextSegment(int lineIndex);

        public abstract ITextSegmentStyled CreateStyledTextSegment(TextStyles.TextStyleBase style);
    }
}