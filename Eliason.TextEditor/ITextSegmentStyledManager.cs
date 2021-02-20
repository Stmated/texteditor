#region

using System;
using System.Collections.Generic;
using Eliason.TextEditor.TextStyles;

#endregion

namespace Eliason.TextEditor
{
    public interface ITextSegmentStyledManager : IDisposable
    {
        //IEnumerable<TextStyleBase> GetStyles(ITextEditor textEditor);
        IEnumerable<ITextSegmentStyled> GetStyledTextSegments();
        IEnumerable<ITextSegmentStyled> GetStyledTextSegments(string typeKey);
        //TextStyleBase GetTextStyle(ITextEditor textEditor, string typeKey);

        void Clear(bool clearPinned);
        bool SearchAndApplyTo(ITextView textView, int lineIndex, int textColumnIndex);
        bool SearchAndApplyTo(ITextView textView, ITextSegment textSegment, int index, int length, int textColumnIndex);
        bool SearchAndApplyTo(ITextView textView, ITextSegment textSegment, int index, int length, bool changeWasFinalizer, int textColumnIndex);

        IEnumerable<ITextSegmentStyled> Get(int index, int textColumnIndex);
        IEnumerable<ITextSegmentStyled> Get(int index, string ofType, int textColumnIndex);
        ITextSegmentStyled GetClosest(int index, string ofType);

        void AddManualTextSegment(ITextSegmentStyled textSegment, int index, int textColumnIndex);
        //void AddStyle(TextStyleBase anchorStyle);

        void RemoveTextSegment(ITextSegmentStyled textSegment);
    }
}