using System;
using System.Collections.Generic;
using System.Drawing;
using Eliason.TextEditor.TextStyles;
using Eliason.TextEditor.TextView;

namespace Eliason.TextEditor
{
    public interface ITextView : ITextEditor
    {
        event EventHandler<SelectionChangedArgs> SelectionChanged;
        event EventHandler<WordWrapChangeEventArgs> WordWrapChanging;
        event EventHandler Disposed;

        Size Size { get; }

        Size ClientSize { get; }

        Rectangle GetTextRectangle(bool physical);
        Rectangle GetTextRectangle(bool physical, bool textOutput);

        /// <summary>
        /// Gets the height of the line in ratio of the font height. 1.0 being font height, 2.0 being one line spacing.
        /// </summary>
        int LineHeight { get; set; }

        Font Font { get; }

        Color ForeColor { get; }

        Color BackColor { get; }

        Color LineHighlightColor { get; }

        Image BackgroundImage { get; }

        ICaret Caret { get; }

        void AddStyle(TextStyleBase anchorStyle);

        IEnumerable<TextStyleBase> GetTextStyles();

        TextStyleBase GetTextStyle(string typeKey);

        IEnumerable<TextColumnBase> Columns { get; }

        int SelectionStart { get; set; }
        int SelectionLength { get; set; }
        string SelectedText { get; set; }
        bool WordWrap { get; set; }

        string TooltipText { get; }

        bool IsDisposed { get; }
        bool IsReadOnly { get; set; }

        void Select(int start, int length);
        void SelectAll();
        WordSegment GetSelectedWord();

        IntPtr GetHdcDangerous();
        void ReleaseHdc();

        int GetCurrentLine();
        int GetVisualLineCount();
        Point GetVirtualPositionFromCharIndex(int index);
        Point GetPositionFromCharIndex(int index);
        TextSegmentVisualInfos GetVisualInformation(int lineIndex);

        /// <summary>
        ///   TODO: Replace lineIndex with an ITextSegment to be more decoupled based on its text buffer implementation (nodes are not per-line)
        /// </summary>
        Size GetLineSize(int lineIndex, int x, int start, int length, int textColumnIndex);

        IScrollHost ScrollHost { get; }

        void ScrollToCaret();
        bool Focus();

        bool Focused { get; }
        bool WordWrapGlyphs { get; }

        void Invalidate();
        void PerformLayout();
    }
}