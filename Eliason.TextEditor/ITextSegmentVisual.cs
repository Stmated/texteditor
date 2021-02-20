using System;
using System.Collections.Generic;
using System.Drawing;

namespace Eliason.TextEditor
{
    public interface ITextSegmentVisual : ITextSegment
    {
        TextSegmentVisualInfos CalculateVisuals(ITextView textView, IntPtr hdc, int width, int lineHeight);

        Size GetSize(IntPtr hdc, int x, int start, int length, TextSegmentVisualInfo info);

        IDictionary<string, string> Metadata { get; }
    }
}