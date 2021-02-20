using System;

namespace Eliason.TextEditor
{
    public class AlterTextSegmentArgs : EventArgs
    {
        public ITextSegmentVisual TextSegment { get; private set; }
        public int CharacterCountDifference { get; private set; }

        public int LineIndex { get; private set; }
        public int TextColumnIndex { get; private set; }

        public AlterTextSegmentArgs(ITextSegmentVisual textSegment, int lineIndex, int charCountDiff, int textColumnIndex)
        {
            this.TextSegment = textSegment;
            this.LineIndex = lineIndex;
            this.CharacterCountDifference = charCountDiff;
            this.TextColumnIndex = textColumnIndex;
        }
    }
}