#region

using System;
using System.Drawing;

#endregion

namespace Eliason.TextEditor
{
    public interface ITextDocumentRenderer
    {
        ITextView TextView { get; }

        /// <summary>
        /// TODO: Remove this and make it a callback that the text renderer gets its value from instead. This is not decoupled enough.
        /// </summary>
        ITextSegmentStyled FocusedStyledSegment { get; set; }

        void Dispose();

        void Render(IntPtr hdc, int viewportX, int viewportY, Size size);
    }
}