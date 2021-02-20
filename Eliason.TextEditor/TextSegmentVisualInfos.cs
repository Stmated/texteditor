using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace Eliason.TextEditor
{
    public class TextSegmentVisualInfo
    {
        public int LineCountVisual { get; set; }

        public int[] LineSplitIndexes { get; set; }

        public int[] TabSplitIndexes { get; set; }

        public Size Size { get; set; }

        public int TextColumnIndex { get; set; }
    }

    public class TextSegmentVisualInfos : IEnumerable<TextSegmentVisualInfo>
    {
        private readonly TextSegmentVisualInfo[] _columns;

        public TextSegmentVisualInfos(TextSegmentVisualInfo[] columns)
        {
            this._columns = columns;
        }

        public TextSegmentVisualInfos()
        {
        }

        public int GetLineCountVisual(int textColumnIndex)
        {
            if (this._columns == null || textColumnIndex >= this._columns.Length)
            {
                return 0;
            }

            return this._columns[textColumnIndex].LineCountVisual;
        }

        public int[] GetLineSplitIndexes(int textColumnIndex)
        {
            if (textColumnIndex > 0)
            {
                return null;
            }

            return this._columns[textColumnIndex].LineSplitIndexes;
        }

        public int[] GetTabSplitIndexes(int textColumnIndex)
        {
            return this._columns[textColumnIndex].TabSplitIndexes;
        }

        public Size GetSize(int textColumnIndex)
        {
            return this._columns[textColumnIndex].Size;
        }

        public TextSegmentVisualInfo GetVisualInfo(int textColumnIndex)
        {
            return this._columns[textColumnIndex];
        }

        /// <summary>
        /// What is actually needed here? Will it ever be needed.
        /// </summary>
        /// <returns></returns>
        public int GetTextColumnIndex()
        {
            return -1;
        }

        public IEnumerator<TextSegmentVisualInfo> GetEnumerator()
        {
            return ((IEnumerable<TextSegmentVisualInfo>) this._columns).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}