#region

using System;
using Eliason.TextEditor.TextStyles;

#endregion

namespace Eliason.TextEditor
{
    public interface ITextSegmentStyled : ITextSegment
    {
        /// <summary>
        /// Gets the index this segment has in offset to the document, and not the line.
        /// </summary>
        int IndexGlobal { get; }

        TextStyleBase Style { get; }

        IComparable Object { get; set; }

        ITextSegmentVisual Parent { get; }

        void ShowInfo();

        bool CanExecute { get; }

        bool Execute();
    }
}