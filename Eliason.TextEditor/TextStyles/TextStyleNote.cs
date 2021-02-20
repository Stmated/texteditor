#region

using System;
using System.Drawing;
using Eliason.TextEditor.Native;
using Eliason.Common;

#endregion

namespace Eliason.TextEditor.TextStyles
{
    public class TextStyleNote : TextStyleBase
    {
        private static SafeHandleGDI staticBrushRed;

        public TextStyleNote()
        {
        }

        public override string Name
        {
            get { return Strings.TextControl_Style_Note_Name; }
        }

        public override string NameKey
        {
            get { return Strings.TextControl_Style_Note_Name; }
        }

        public override string Description
        {
            get { return Strings.TextControl_Style_Note_Description; }
        }

        public override TextStylePaintMode PaintMode
        {
            get { return TextStylePaintMode.Custom; }
        }

        public override TextStyleType Type
        {
            get { return TextStyleType.Pinned; }
        }

        public override TextStyleBase Clone()
        {
            var anchor = new TextStyleNote();
            return anchor;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override RenderStateItem GetNaturalRenderColors(ITextEditor textEditor)
        {
            var rsi = new RenderStateItem();
            this.FillRenderStateItem(textEditor, rsi);

            rsi.BackColor = ColorTranslator.ToWin32(Color.Red);

            return rsi;
        }

        public override void Paint(IntPtr hdc, ITextSegmentStyled textSegment, ITextView textView, TextSegmentVisualInfo info, int x, int y, int lineHeight, StyleRenderInfo sri)
        {
            if (staticBrushRed == null)
            {
                staticBrushRed = new SafeHandleGDI(SafeNativeMethods.CreateSolidBrush(ColorTranslator.ToWin32(Color.Red)));
            }

            var spaceSize = (lineHeight/4d);

            var rectTop = new RECT
            {
                left = x,
                top = y,
                right = x + (lineHeight/8),
                bottom = (int) (y + (lineHeight - (spaceSize)))
            };

            var rectBottom = new RECT
            {
                left = rectTop.left,
                top = (int) (rectTop.bottom + (spaceSize/2)),
                right = rectTop.right
            };
            rectBottom.bottom = (int) (rectBottom.top + (spaceSize/2));

            SafeNativeMethods.FillRect(hdc, ref rectTop, staticBrushRed.DangerousGetHandle());
            SafeNativeMethods.FillRect(hdc, ref rectBottom, staticBrushRed.DangerousGetHandle());
        }

        public override void FillRenderStateItem(ITextEditor textEditor, RenderStateItem rsi)
        {
        }
    }
}