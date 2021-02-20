#region File Header Text

// 
// - Scanlationshop
// -- Author: 	Stmated (stmated@gmail.com)
// -- License: 	MIT, unless falls under other respective "Resources\License (XYZ).txt"
// 

#endregion

#region Class Usage References

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Eliason.Common;
using Eliason.TextEditor.Native;
using Eliason.TextEditor.TextEditor;
using Eliason.TextEditor.TextStyles;
using Eliason.TextEditor.UndoRedo;

#endregion

namespace Eliason.TextEditor.TextView
{
    /// <summary>
    ///   This is a text control base that is decoupled from any other project code and can be used for other projects.
    ///   It inherits from <see cref = "TextEditor" /> which takes care of underlying fucntionality, while this class takes care
    ///   of anything relating to anything visual, such as rendering and keyboard/mouse support.
    /// </summary>
    /// <remarks>
    ///   Later:
    ///   * Change the tabbing to render and calculate as X number of spaces, instead of a 8*8 standard tabbing distance (better selection painting, etc)
    ///   * Do not rely on inheriting from "Control" at all. Make it a container which you ask to paint itself, and send keyboard/mouse input to
    /// </remarks>
    public partial class TextView : TextEditor.TextEditor, ITextView, IAutoSaver, IScrollAware
    {
        public static readonly List<TextColumnBase> ALL_TEXT_COLUMNS = new List<TextColumnBase>();

        static TextView()
        {
            ALL_TEXT_COLUMNS.Add(new TextColumnLineNumber());
        }

        public event EventHandler<SelectionChangedArgs> SelectionChanged;
        public event EventHandler<WordWrapChangeEventArgs> WordWrapChanging;

        private readonly List<TextSegmentVisualInfos> _textSegmentVisualInformations = new List<TextSegmentVisualInfos>();
        private int _visualLineCount;

        private Dictionary<ITextSegment, int> _layoutLinesOverflow = new Dictionary<ITextSegment, int>();

        private bool _overwriteMode;
        private ITextDocumentRenderer _renderer;
        private int _selectionColumnX = -1;
        private bool _selectionIsBackwards;
        private int[] _selectionLength = new int[0];
        private bool _selectionMode;
        private int _selectionModeStart;
        private int[] _selectionStart = new int[0];
        private ITextSegmentStyled[] _textSegmentSelection = new ITextSegmentStyled[0];
        private readonly List<TextColumnBase> _textColumns = new List<TextColumnBase>();

        private bool _wordWrap = true;
        private IScrollHost _scrollHost;
        private int _lineHeight;

        public bool RenderUnsavedMarker { get; set; }

        protected TextView(ITextDocument textDocument, ISettings settings, IScrollHost scrollHost = null)
            : base(settings)
        {
            SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            foreach (var column in ALL_TEXT_COLUMNS)
            {
                this._textColumns.Add(column);
            }

            SuspendLayout();

            // Set some useful default properties
            this.Cursor = Cursors.IBeam;
            this.Caret = new Caret(this);
            this.ForeColor = Color.Black;
            this.BackColor = Color.White;
            this._lineHeight = this.Font.Height;

            this.ScrollHost = scrollHost;

            this.Padding = new Padding(4, 0, 4, 0);

            this._imeComposition = new IMEComposition(this);

            this.RenderUnsavedMarker = true;

            this.AddStyle(
                new TextStyleManual(
                    "Selection",
                    "Selection",
                    "The style for the selection.",
                    SystemColors.HighlightText, SystemColors.Highlight,
                    null,
                    TextStyleDisplayMode.Always,
                    TextStylePaintMode.Inline) { RenderZIndex = 1 });

            this.AddStyle(
                new TextStyleManual(
                    "Default",
                    "Default",
                    "The default text style for the text",
                    this.ForeColor, Color.Transparent,
                    this.Font,
                    TextStyleDisplayMode.Always,
                    TextStylePaintMode.Inline));

            // Initialize the text buffer by sending in our document!
            this.InitializeTextBuffer(textDocument);

            ResumeLayout();
        }

        public IEnumerable<TextColumnBase> Columns
        {
            get { return this._textColumns; }
        }

        public IScrollHost ScrollHost
        {
            get { return this._scrollHost; }
            set
            {
                if (this._scrollHost != null)
                {
                    this._scrollHost.Detach();

                    this._scrollHost.VerticalScrollChanged -= this.VScroll_ValueChanged;
                    this._scrollHost.HorizontalScrollChanged -= this.HScroll_ValueChanged;
                }

                this._scrollHost = value;

                if (value != null)
                {
                    value.Attach();

                    value.VerticalScrollChanged += this.VScroll_ValueChanged;
                    value.HorizontalScrollChanged += this.HScroll_ValueChanged;
                }
            }
        }

        /// <summary>
        ///   This should not be used, unless it is for saving purposes or such.
        ///   This is because it is really, really slow; since this text control's system is not built for in-text operations, but in-line operations.
        /// </summary>
        public new string Text
        {
            get
            {
                var sb = new StringBuilder();

                foreach (var c in TextDocument.TextGetStream(0, true, -1))
                {
                    sb.Append(c);
                }

                return sb.ToString();
            }
        }

        public int LongestLineWidth
        {
            get
            {
                var highestWidth = 0;

                foreach (var layoutLineOverflow in this._layoutLinesOverflow)
                {
                    if (layoutLineOverflow.Value > highestWidth)
                    {
                        highestWidth = layoutLineOverflow.Value;
                    }
                }

                return highestWidth;
            }
        }

        public virtual bool CanScrollOutside
        {
            get { return false; }
        }

        #region ITextView Members

        public bool IsReadOnly { get; set; }

        /// <summary>
        ///   Gets the caret
        /// </summary>
        public ICaret Caret { get; private set; }

        public int LineHeight
        {
            get { return this._lineHeight; }
            set { this._lineHeight = value; }
        }

        public Rectangle GetTextRectangle(bool physical)
        {
            return this.GetTextRectangle(physical, true);
        }

        public Rectangle GetTextRectangle(bool physical, bool textOutput)
        {
            var paddingLeft = textOutput ? Padding.Left : 0;
            var paddingRight = textOutput ? Padding.Right : 0;
            var paddingTop = textOutput ? Padding.Top : 0;

            foreach (var textColumn in this.Columns)
            {
                if (textColumn.IsEnabled(this.Settings))
                {
                    if (textColumn.FloatLeft)
                    {
                        paddingLeft += textColumn.Width;
                    }
                    else
                    {
                        paddingRight += textColumn.Width;
                    }
                }
            }

            var clientSize = ClientSize;
            return new Rectangle(
                new Point(paddingLeft, paddingTop),
                new Size(
                    this._wordWrap || physical ? (clientSize.Width - paddingLeft - paddingRight) : int.MaxValue,
                    clientSize.Height));
        }

        public int SelectionStart
        {
            get
            {
                // TODO: IndexOutOfBounds här, måste täcka fler områden där den här kan kallas på före "set"
                //       (är för tungt att kolla på varje get om är korrekt, måste vara optimiserat vid kritiska punkter i koden istället)

                return this._selectionStart[this.CurrentTextColumnIndex];
            }
            set
            {
                this.EnsureSelectionArrays(this.CurrentTextColumnIndex);

                this.SetSelectionStart(value, ByInterface.Unknown, this.CurrentTextColumnIndex);
            }
        }

        public int SelectionLength
        {
            get { return this._selectionLength[this.CurrentTextColumnIndex]; }
            set
            {
                this.EnsureSelectionArrays(this.CurrentTextColumnIndex);

                if (value == this.SelectionLength)
                {
                    return;
                }

                var newLength = Math.Max(0, Math.Min(TextLength - this.SelectionStart, value));
                this._selectionLength[this.CurrentTextColumnIndex] = newLength;

                this._textSegmentSelection[this.CurrentTextColumnIndex].SetLength(this.CurrentTextColumnIndex, newLength);

                this.UpdateCaretLocation();
                this.Caret.ResetBlink();

                if (this.SelectionChanged != null)
                {
                    this.SelectionChanged(this, new SelectionChangedArgs(ByInterface.Unknown, this.CurrentTextColumnIndex));
                }
            }
        }

        public string TooltipText { get; protected set; }

        public void Select(int start, int length)
        {
            this.SelectionStart = start;
            this.SelectionLength = length;
        }

        public void ScrollToCaret()
        {
            var index = this._selectionIsBackwards ? this.SelectionStart : this.SelectionStart + this.SelectionLength;
            var selectionLengthPoint = this.GetVirtualPositionFromCharIndex(index);

            this.ScrollHost.ScrollToPoint(new Point(selectionLengthPoint.X - this.GetTextRectangle(false).Left, selectionLengthPoint.Y), false, this.WordWrap);
        }

        public override sealed Font Font
        {
            get { return base.Font; }
            set
            {
                base.Font = value;
                this._lineHeight = this.Font.Height;
            }
        }

        public override sealed Color ForeColor
        {
            get { return base.ForeColor; }
            set { base.ForeColor = value; }
        }

        public override sealed Color BackColor
        {
            get
            {
                if (Enabled == false)
                {
                    return SystemColors.Control;
                }

                return base.BackColor;
            }
            set { base.BackColor = value; }
        }

        public virtual bool WordWrap
        {
            get { return this._wordWrap; }

            set
            {
                var args = new WordWrapChangeEventArgs(value);

                if (this.WordWrapChanging != null)
                {
                    this.WordWrapChanging(this, args);
                }

                if (args.Cancel)
                {
                    return;
                }

                this._wordWrap = args.Enable;
            }
        }

        public bool WordWrapGlyphs { get; set; }

        public string SelectedText
        {
            get
            {
                var sb = new StringBuilder();
                var count = this.SelectionLength + 1;

                foreach (var c in TextGetStream(this.SelectionStart, true))
                {
                    count--;

                    if (count == 0)
                    {
                        break;
                    }

                    sb.Append(c);
                }

                return sb.ToString();
            }
            set
            {
                UndoRedoCommandBase removeUndoRedo = null;
                var selStart = this.SelectionStart;
                var selLen = this.SelectionLength;

                if (this.SelectionLength > 0)
                {
                    removeUndoRedo = this.TextRemove(selStart, selLen);
                    this.SelectionLength = 0;

                    if (removeUndoRedo == null)
                    {
                        return;
                    }
                }

                var insertUndoRedo = this.TextInsert(selStart, value);

                this.SetSelectionStart(selStart + value.Length, ByInterface.Manually, this.CurrentTextColumnIndex);

                if (insertUndoRedo != null && removeUndoRedo != null)
                {
                    insertUndoRedo.UndoPair = true;
                    removeUndoRedo.RedoPair = true;
                }

                this.ScrollToCaret();

                Invalidate();
            }
        }

        public override void Open()
        {
            if (String.IsNullOrEmpty(CurrentFilePath) == false && IsModified)
            {
                switch (this.Settings.Notifier.AskYesNoCancel(Strings.TextControl_Title, String.Format(Strings.TextControl_Open_ChangesPending_Save_Ask, CurrentFilePath)))
                {
                    case AskResult.Yes:
                        Save();
                        break;
                    case AskResult.Cancel:
                        return;
                }
            }

            using (var ofd = this.GetOpenFileDialog())
            {
                switch (ofd.ShowDialog())
                {
                    case DialogResult.OK:
                        if (String.IsNullOrEmpty(ofd.FileName) == false)
                        {
                            this.Open(ofd.FileName);
                        }
                        break;
                }
            }
        }

        public override void Open(string filePath)
        {
            base.Open(filePath);

            if (TextDocument == null)
            {
                Dispose();
            }
            else
            {

                var anchorTextFile = GetAnchorFileName(this.CurrentFilePath);
                if (File.Exists(anchorTextFile))
                {
                    using (var s = this.GetFileStream(anchorTextFile))
                    {
                        using (var sr = new StreamReader(s, Encoding.UTF8, true))
                        {
                            while (sr.EndOfStream == false)
                            {
                                var anchorString = sr.ReadLine();
                                var anchorStringParts = anchorString.Split('|');

                                var anchorStyleName = anchorStringParts[0];
                                var anchorIndex = int.Parse(anchorStringParts[1]);
                                var anchorLength = int.Parse(anchorStringParts[2]);
                                var content = anchorStringParts[3];

                                var anchorStyle = this.GetTextStyle(anchorStyleName);
                                var anchor = this.TextDocument.CreateStyledTextSegment(anchorStyle);

                                var lineStartIndex = this.GetFirstCharIndexFromLine(this.GetLineFromCharIndex(anchorIndex));

                                anchor.Index = anchorIndex - lineStartIndex;
                                anchor.SetLength(this.CurrentTextColumnIndex, anchorLength);
                                anchor.Object = content;

                                this.TextDocument.TextSegmentStyledManager.AddManualTextSegment(anchor, anchorIndex, this.CurrentTextColumnIndex);
                            }
                        }
                    }
                }

                // Update
                PerformLayout();
                //PerformLayout();

                this.SelectionStart = 0;
                this.SelectionLength = 0;
                Invalidate();
            }
        }

        public override void Clear()
        {
            base.Clear();

            this.SelectionStart = 0;
            this.SelectionLength = 0;
        }

        public override bool SaveAs()
        {
            using (var sfd = this.GetSaveFileDialog())
            {
                switch (sfd.ShowDialog())
                {
                    case DialogResult.OK:
                        {
                            return SaveAs(sfd.FileName, false);
                        }
                }
            }

            return false;
        }

        public int GetVisualLineCount()
        {
            return this._visualLineCount;
        }

        public TextSegmentVisualInfos GetVisualInformation(int lineIndex)
        {
            return this._textSegmentVisualInformations[lineIndex];
        }

        public Size GetLineSize(int lineIndex, int x, int start, int length, int textColumnIndex)
        {
            var hdc = this.GetHdcDangerous();

            var size = TextDocument.GetVisualTextSegment(lineIndex).GetSize(
                hdc, x, start, length,
                this.GetVisualInformation(lineIndex).GetVisualInfo(textColumnIndex));

            this.ReleaseHdc();

            return size;
        }

        /// <summary>
        ///   TODO: The performance of this method is too low, it should be able to be improved.
        /// </summary>
        /// <returns></returns>
        public WordSegment GetSelectedWord()
        {
            if (this.SelectionLength > 0)
            {
                return new WordSegment
                {
                    Word = this.SelectedText.Trim(),
                    Start = this.SelectionStart,
                    End = this.SelectionStart + this.SelectionLength
                };
            }

            return GetWord(this.SelectionStart, true);
        }

        public int GetCurrentLine()
        {
            return GetLineFromCharIndex(this.SelectionStart);
        }

        public override UndoRedoCommandBase TextInsert(int start, string text)
        {
            var result = base.TextInsert(start, text);

            return result;
        }

        public override UndoRedoCommandBase TextRemove(int start, int length)
        {
            var result = base.TextRemove(start, length);

            return result;
        }

        public virtual void SelectAll()
        {
            this.SetSelectionStart(0, ByInterface.Unknown, this.CurrentTextColumnIndex);
            this.SelectionLength = TextLength;
            Invalidate();
        }

        public override void Undo()
        {
            var cmd = TextDocument.UndoRedoManager.PeekUndoCommand();

            if (cmd == null)
            {
                return;
            }

            base.Undo();

            if (cmd is TextBaseUndoRedoCommand)
            {
                if (cmd is TextAddedUndoRedoCommand)
                {
                    this.SelectionLength = 0;
                    this.SelectionStart = ((TextAddedUndoRedoCommand)cmd).StartIndex;
                }
                else
                {
                    this.SelectionLength = 0;
                    this.SelectionStart = ((TextRemovedUndoRedoCommand)cmd).StartIndex + ((TextRemovedUndoRedoCommand)cmd).PreviousText.Length;
                }

                this.TextDocument.FakeFinalizingKey(this.SelectionStart, (cmd as TextBaseUndoRedoCommand).TextColumnIndex);

                this.ScrollToCaret();
            }
        }

        public override void Redo()
        {
            var cmd = TextDocument.UndoRedoManager.PeekRedoCommand();

            if (cmd == null)
            {
                return;
            }

            base.Redo();

            if (cmd is TextBaseUndoRedoCommand)
            {
                if (cmd is TextAddedUndoRedoCommand)
                {
                    this.SelectionLength = 0;
                    this.SelectionStart = ((TextAddedUndoRedoCommand)cmd).StartIndex + ((TextAddedUndoRedoCommand)cmd).InputtedText.Length;
                }
                else
                {
                    this.SelectionLength = 0;
                    this.SelectionStart = ((TextRemovedUndoRedoCommand)cmd).StartIndex;
                }

                this.TextDocument.FakeFinalizingKey(this.SelectionStart, (cmd as TextBaseUndoRedoCommand).TextColumnIndex);

                this.ScrollToCaret();
            }
        }

        #endregion

        private void InitializeTextBuffer(ITextDocument textBuffer)
        {
            this.SetTextBufferStrategy(textBuffer);

            InitializeComponent();

            this.EnsureSelectionArrays(this.CurrentTextColumnIndex);

            if (this.LineHeight == 0)
            {
                this.LineHeight = this.Font.Height;
            }

            if (IsVirtual == false)
            {
                this.Clear();
            }
            else
            {
                // Since this text view has not been around since the start of the text buffer, we are not synched
                // with the current width and height of all the text inside the control.
                // So we check the bounds and calculate wordwrapping and all that.
                var hdc = this.GetHdc().DangerousGetHandle();

                for (var i = 0; i < LineCount; i++)
                {
                    var textLine = TextDocument.GetVisualTextSegment(i);

                    var information = textLine.CalculateVisuals(this, hdc, this.GetTextRectangle(false, true).Width, this.LineHeight);

                    this._textSegmentVisualInformations.Add(information);

                    var bounds = information.GetSize(this.CurrentTextColumnIndex);
                    this.UpdateLineOverflow(textLine, bounds.Width);
                    this._visualLineCount += information.GetLineCountVisual(this.CurrentTextColumnIndex);
                }

                this.ReleaseHdc();
            }
        }

        private void EnsureSelectionArrays(int textColumnIndex)
        {
            if (textColumnIndex < this._selectionStart.Length)
            {
                // There exists a text column for the given column index,
                // so there's nothing to do here except happily return.
                return;
            }

            // Add a new column index if needed.
            var currentSelectionStarts = new List<int>(this._selectionStart);
            var currentSelectionLengths = new List<int>(this._selectionStart);
            var currentSelectionStyles = new List<ITextSegmentStyled>(this._textSegmentSelection);
            for (var i = this._selectionStart.Length; i <= textColumnIndex; i++)
            {
                currentSelectionStarts.Add(0);
                currentSelectionLengths.Add(0);

                currentSelectionStyles.Add(TextDocument.CreateStyledTextSegment(this.GetTextStyle("Selection")));
            }

            this._selectionStart = currentSelectionStarts.ToArray();
            this._selectionLength = currentSelectionLengths.ToArray();
            this._textSegmentSelection = currentSelectionStyles.ToArray();
        }

        private void SetSelectionStart(int index, ByInterface method, int textColumnIndex)
        {
            this.EnsureSelectionArrays(textColumnIndex);

            if (index == this.SelectionStart)
            {
                return;
            }

            if (method == ByInterface.Manually || method == ByInterface.ByMouse)
            {
                // We are moving to another location in the text, and hence we should make sure that text styles are checked for
                // in a delayed manner and not on each keystroke (like spellchecking).
                // They are now found and/or updated before moving away.
                TextDocument.FakeFinalizingKey(this._selectionStart[textColumnIndex], textColumnIndex);
            }

            this._selectionStart[textColumnIndex] = Math.Max(0, Math.Min(TextLength, index));

            this._textSegmentSelection[textColumnIndex].Index = this._selectionStart[textColumnIndex];

            this.UpdateCaretLocation();
            this.Caret.ResetBlink();

            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this, new SelectionChangedArgs(method, textColumnIndex));
            }
        }

        private void SetFocusedStyleSegment(int globalIndex, bool fromMouse, int textColumIndex)
        {
            var previous = this._renderer.FocusedStyledSegment;

            ITextSegmentStyled focusedStyledSegment = null;

            foreach (var styledSegment in TextDocument.TextSegmentStyledManager.Get(globalIndex, textColumIndex))
            {
                if (styledSegment.CanExecute == false)
                {
                    continue;
                }

                if (focusedStyledSegment == null)
                {
                    focusedStyledSegment = styledSegment;
                }

                if (styledSegment.CanExecute)
                {
                    focusedStyledSegment = styledSegment;
                    break;
                }
            }

            this._renderer.FocusedStyledSegment = focusedStyledSegment;

            if (this._renderer.FocusedStyledSegment != previous)
            {
                if (fromMouse && this.ScrollHost.IsScrollingHorizontally == false && this.ScrollHost.IsScrollingVertically == false)
                {
                    Cursor = focusedStyledSegment == null ? Cursors.IBeam : Cursors.Hand;
                }

                Invalidate();
            }
        }

        private void HScroll_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.UpdateCaretLocation();
            Invalidate();
        }

        private void VScroll_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.UpdateCaretLocation();

            if (e.By == ValueChangedBy.MouseClick || e.By == ValueChangedBy.MouseMove)
            {
                var firstIndex = this.GetCharIndexFromPhysicalPosition(Point.Empty);

                var closestMatch = TextDocument.TextSegmentStyledManager.GetClosest(firstIndex, "IndexState");

                if (closestMatch != null)
                {
                    this.TooltipText = closestMatch.GetText(this.CurrentTextColumnIndex);
                }
            }

            Invalidate();
        }

        /// <summary>
        /// TODO: DO NOT CALL THIS CODE WHEN WE SCROLL VERTICALLY? WHY WOULD WE EVER? IT FEELS STUPID AS SHIT!
        /// </summary>
        private void UpdateCaretLocation()
        {
            var textRectangle = this.GetTextRectangle(true, true);
            var followWithViewport = this.Caret.IsInView;

            var caretIndex = this._selectionIsBackwards
                ? this.SelectionStart
                : this.SelectionStart + this.SelectionLength;

            var caretPoint = this.GetVirtualPositionFromCharIndex(caretIndex);

            if (this.ScrollHost != null)
            {
                caretPoint.Offset(-this.ScrollHost.ScrollPosH, -this.ScrollHost.ScrollPosVIntegral);
            }

            this.Caret.Location = caretPoint;
            this.Caret.Index = caretIndex;

            this.Caret.IsInView = LineCount == 0 ||
                                  (
                                      caretPoint.X < textRectangle.X
                                      || caretPoint.X > textRectangle.Right
                                      || caretPoint.Y < textRectangle.Y
                                      || caretPoint.Y > textRectangle.Bottom
                                      ) == false;

            if (Focused == false)
            {
                if (followWithViewport)
                {
                    this.ScrollToCaret();
                }
            }
        }

        protected virtual DialogResult PerformCloseInternal()
        {
            var result = DialogResult.Yes;

            if (IsModified && IsVirtual == false)
            {
                var question = String.IsNullOrEmpty(CurrentFilePath)
                    ? Strings.TextControl_Close_SaveDiscardCancel_New
                    : String.Format(Strings.TextControl_Close_SaveDiscardCancel, CurrentFilePath);

                switch (this.Settings.Notifier.AskYesNoCancel(Strings.TextControl_Title, question))
                {
                    case AskResult.Yes:
                        {
                            result = Save() ? DialogResult.Yes : DialogResult.No;
                            break;
                        }
                    case AskResult.No:
                        {
                            result = DialogResult.OK;
                        }
                        break;
                    case AskResult.Cancel:
                        return DialogResult.No;
                }

                var textViews = new List<ITextView>(TextDocument.GetTextViews());

                foreach (var textView in textViews)
                {
                    if (textView != this)
                    {
                        textView.Dispose();
                    }
                }
            }

            return result;
        }

        protected override void SaveToFile(string filePath, Encoding encoding)
        {
            using (new WaitUIHandler(this))
            {
                base.SaveToFile(filePath, encoding);
            }
        }

        protected override void SetTextBufferStrategy(ITextDocument textDocument)
        {
            if (TextDocument != null)
            {
                // In case the text buffer strategy already exists, we need to unset this first.
                TextDocument.Modified -= this.TextDocument_Modified;
                TextDocument.TextSegmentAlter -= this.TextDocument_TextSegmentAlter;
                TextDocument.TextSegmentRemoved -= this.TextDocument_TextSegmentRemoved;
                TextDocument.TextSegmentAdded -= this.TextDocument_TextSegmentAdded;
            }

            base.SetTextBufferStrategy(textDocument);

            textDocument = TextDocument;

            textDocument.RegisterTextView(this);

            this._renderer = textDocument.GetRenderer(this);

            textDocument.TextSegmentAlter += this.TextDocument_TextSegmentAlter;
            textDocument.TextSegmentRemoved += this.TextDocument_TextSegmentRemoved;
            textDocument.TextSegmentAdded += this.TextDocument_TextSegmentAdded;
            textDocument.Modified += this.TextDocument_Modified;
        }

        private void TextDocument_TextSegmentRemoved(object sender, AlterTextSegmentArgs e)
        {
            var textLine = e.TextSegment;

            // Remove the line count.
            this._visualLineCount -= this.GetVisualInformation(e.LineIndex).GetLineCountVisual(e.TextColumnIndex);
            this._layoutLinesOverflow.Remove(textLine); // Remove any potential width overflow the line had.
            this._textSegmentVisualInformations.RemoveAt(e.LineIndex);

            if (Focused == false)
            {
                // If it is not focused, it means that this is probably a virtual copy of the text control,
                // and we need to make it keep the same selection start so it does not jump around.
                if (e.TextSegment.Index <= this.SelectionStart)
                {
                    this.SelectionStart += e.CharacterCountDifference;
                }
            }
        }

        private void TextDocument_TextSegmentAdded(object sender, AlterTextSegmentArgs e)
        {
            var textLine = e.TextSegment;

            var hdc = this.GetHdc().DangerousGetHandle();
            var newInformation = textLine.CalculateVisuals(this, hdc, this.GetTextRectangle(false, true).Width, this.LineHeight);
            this.ReleaseHdc();

            // Insert an empty one here that is later overwritten in "UpdateVisualInformationLine".
            // This insertion is just a placeholder so that the code later knows that all values have changed from the default.
            this._textSegmentVisualInformations.Insert(e.LineIndex, new TextSegmentVisualInfos());

            this.UpdateVisualInformationLine(e.LineIndex, e.TextSegment, e.CharacterCountDifference, newInformation);
        }

        private void TextDocument_TextSegmentAlter(object sender, AlterTextSegmentArgs e)
        {
            var textLine = e.TextSegment;

            if (this._textSegmentVisualInformations.Count - 1 < e.LineIndex)
            {
                // Add an empty one so that the check below returns that the previous line count was 0.
                this._textSegmentVisualInformations.Add(new TextSegmentVisualInfos());
            }

            var hdc = this.GetHdc().DangerousGetHandle();
            var information = textLine.CalculateVisuals(this, hdc, this.GetTextRectangle(false, true).Width, this.LineHeight);
            this.ReleaseHdc();

            this.UpdateVisualInformationLine(e.LineIndex, e.TextSegment, e.CharacterCountDifference, information);
        }

        private void TextDocument_Modified(object sender, EventArgs e)
        {
            if (Focused == false)
            {
                this.UpdateCaretLocation();
            }

            this.UpdateScrollbars();
            //this.ScrollHost.LayoutScrollBars();
            //this.HorizontalScroll.Layout();
            //this.VerticalScroll.Layout();
        }

        private void UpdateVisualInformationLine(int lineIndex, ITextSegmentVisual textSegment, int charDiff, TextSegmentVisualInfos infos)
        {
            var bounds = infos.GetSize(0);
            this.UpdateLineOverflow(textSegment, bounds.Width);

            var a = infos.GetLineCountVisual(0);
            var b = this.GetVisualInformation(lineIndex);
            var c = b.GetLineCountVisual(0);

            this._visualLineCount += a - c;

            this._textSegmentVisualInformations[lineIndex] = infos;

            if (Focused == false)
            {
                if (textSegment.Index <= this.SelectionStart)
                {
                    this.SelectionStart += charDiff;
                }
            }
        }

        private void UpdateLineOverflow(ITextSegment line, int width)
        {
            if (this._layoutLinesOverflow.ContainsKey(line))
            {
                if (width < ClientSize.Width)
                {
                    this._layoutLinesOverflow.Remove(line);
                }
                else
                {
                    this._layoutLinesOverflow[line] = width;
                }
            }
            else
            {
                if (width > ClientSize.Width)
                {
                    this._layoutLinesOverflow.Add(line, width);
                }
            }
        }

        protected virtual void ToggleWordwrap()
        {
            this.WordWrap = !this.WordWrap;

            PerformLayout();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (TextDocument != null)
            {
                TextDocument.Modified -= this.TextDocument_Modified;
                TextDocument.TextSegmentAlter -= this.TextDocument_TextSegmentAlter;
                TextDocument.TextSegmentRemoved -= this.TextDocument_TextSegmentRemoved;
                TextDocument.TextSegmentAdded -= this.TextDocument_TextSegmentAdded;
            }

            this.ScrollHost.VerticalScrollChanged -= this.VScroll_ValueChanged;
            this.ScrollHost.HorizontalScrollChanged -= this.HScroll_ValueChanged;
            this.ScrollHost.Detach();

            Events.Dispose();
            this.DisposeBuffer();

            foreach (var textStyle in this._textStyles)
            {
                textStyle.Dispose();
            }

            this._textStyles.Clear();

            if (this.Caret != null)
            {
                this.Caret.Dispose();
                this.Caret = null;
            }

            if (this._renderer != null)
            {
                this._renderer.Dispose();
                this._renderer = null;
            }

            if (this._layoutLinesOverflow != null)
            {
                this._layoutLinesOverflow.Clear();
                this._layoutLinesOverflow = null;
            }

            this._textSegmentSelection = null;

            if (this.backBrush != null)
            {
                this.backBrush.Dispose();
                this.backBrush = null;
            }

            if (this.caretBrushSelection != null)
            {
                this.caretBrushSelection.Dispose();
                this.caretBrushSelection = null;
            }

            foreach (var column in this.Columns)
            {
                column.Dispose();
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            // Force the caret to show up right away, so the reponse feels snappier.
            this.Caret.ResetBlink();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            // Invalidate so that we are sure the caret is not left behind when the control is not focused.
            this.Caret.ResetBlink();
            Invalidate();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            this.OnLayout();

            base.OnLayout(levent);
        }

        private void OnLayout()
        {
            if (IsDisposed)
            {
                return;
            }

            this.DisposeBuffer();

            if (this.GetTextRectangle(false).Width <= 0)
            {
                return;
            }

            var hdc = this.GetHdc().DangerousGetHandle();

            this._visualLineCount = 0;
            var lineCount = LineCount;

            for (var i = 0; i < lineCount; i++)
            {
                var visualSegment = GetVisualTextSegment(i);

                var visualInfos = visualSegment.CalculateVisuals(this, hdc, this.GetTextRectangle(false, true).Width, this.LineHeight);
                this._textSegmentVisualInformations[i] = visualInfos;

                this.UpdateLineOverflow(visualSegment, visualInfos.GetSize(0).Width);

                this._visualLineCount += visualInfos.GetLineCountVisual(0);
            }

            foreach (var column in this.Columns)
            {
                column.UpdateWidth(hdc, this);
            }

            this.ReleaseHdc();

            foreach (var anchorStyle in this.GetTextStyles())
            {
                anchorStyle.PaintResetBuffer();
            }

            this.UpdateScrollbars();
        }

        private void UpdateScrollbars()
        {
            var highestWidth = this.LongestLineWidth;
            var totalHeight = this.GetVisualLineCount() * this.LineHeight;

            if (this.ScrollHost != null)
            {
                this.ScrollHost.OnContentSizeChanged(highestWidth, totalHeight);
            }
        }

        protected virtual OpenFileDialog GetOpenFileDialog()
        {
            var ofd = new OpenFileDialog
            {
                AddExtension = true,
                CheckPathExists = true,
                DefaultExt = "txt",
                Filter = @"Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Multiselect = false
            };

            return ofd;
        }

        protected virtual SaveFileDialog GetSaveFileDialog()
        {
            var sfd = new SaveFileDialog
            {
                CheckPathExists = true,
                DefaultExt = "txt",
                Filter = @"Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            return sfd;
        }

        protected virtual bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public virtual void Paste()
        {
            if (this.IsReadOnly)
            {
                return;
            }

            var text = Clipboard.GetText();

            if (String.IsNullOrEmpty(text) == false)
            {
                text = text.Replace("\r\n", "\n");
                // Replace any Windows newline to the newline type we actually care about.

                if (this.SelectionLength > 0)
                {
                    this.TextRemove(this.SelectionStart, this.SelectionLength);
                    this.SelectionLength = 0;
                }

                this.TextInsert(this.SelectionStart, text);

                this.SetSelectionStart(this.SelectionStart + text.Length, ByInterface.Unknown, this.CurrentTextColumnIndex);
                this.ScrollToCaret();
            }
        }

        public virtual void Cut()
        {
            this.Copy();
            this.TextRemove(this.SelectionStart, this.SelectionLength);
            this.SelectionLength = 0;

            this.ScrollToCaret();
        }

        public virtual void Copy()
        {
            if (this.SelectionLength <= 0)
            {
                return;
            }

            var sb = new StringBuilder();

            var index = 0;
            foreach (var c in TextGetStream(this.SelectionStart, true))
            {
                sb.Append(c);

                if (++index >= this.SelectionLength)
                {
                    break;
                }
            }

            var text = sb.ToString();

            if (String.IsNullOrEmpty(text))
            {
                return;
            }

            text = GetFormattedSaveString(text);

            if (String.IsNullOrEmpty(text))
            {
                return;
            }

            try
            {
                Clipboard.SetText(text);
            }
            catch (Exception ex)
            {
                var hwnd = SafeNativeMethods.GetOpenClipboardWindow();

                if (hwnd != IntPtr.Zero)
                {
                    var windowText = Strings.NotAvailable;

                    try
                    {
                        var buff = new StringBuilder(256);
                        var textLength = SafeNativeMethods.GetWindowText(hwnd, buff, 256);

                        if (textLength > 0)
                        {
                            windowText = buff.ToString();
                        }
                    }
                    catch
                    {
                        // Just ignore any exception here.
                    }

                    this.Settings.Notifier.Error(Strings.TextControl_Title, String.Format(Strings.TextControl_CouldNotCopyToClipboardBecauseOf, ex.Message, windowText), ex);
                }
                else
                {
                    this.Settings.Notifier.Error(Strings.TextControl_Title, String.Format(Strings.TextControl_CouldNotCopyToClipboardBecause, ex.Message), ex);
                }
            }

            this.ScrollToCaret();
        }

        public override string ToString()
        {
            return CurrentFilePath;
        }
    }
}