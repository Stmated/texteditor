namespace Eliason.TextEditor.TextStyles
{
    /*public sealed class TextStyleURL : TextStyleTextColorer
    {
        private static SafeHandleGDI staticPenUnderline;

        public override string Name
        {
            get { return Strings.TextControl_Style_URL_Name; }
        }

        public override string NameKey
        {
            get { return Strings.TextControl_Style_URL_Name; }
        }

        public override string Description
        {
            get { return Strings.TextControl_Style_URL_Description; }
        }

        public override TextStylePaintMode PaintMode
        {
            get { return TextStylePaintMode.Custom; }
        }

        public override bool CanExecute
        {
            get
            {
                return true;
            }
        }

        public override Color ColorBack
        {
            get { return Color.Transparent; }
        }

        public override Color ColorFore
        {
            get { return Color.Blue; }
        }

        public override RenderStateItem GetNaturalRenderColors()
        {
            var rsi = new RenderStateItem();
            this.FillRenderStateItem(rsi);

            rsi.ForeColor = ColorTranslator.ToWin32(Color.Blue);

            return rsi;
        }

        public override void Paint(IntPtr hdc, ITextSegmentStyled textSegment, ITextView textView, TextSegmentVisualInfos info, int x, int y, int lineHeight, bool isHot)
        {
            // Buggy
            if (isHot)
            {
                if (staticPenUnderline == null)
                {
                    staticPenUnderline = new SafeHandleGDI(SafeNativeMethods.CreatePen(NativeConstants.PS_SOLID, -1, ColorTranslator.ToWin32(Color.Blue)));
                }

                var wordSize = textSegment.Parent.GetSize(hdc, x, textSegment.Index, textSegment.Length, info);

                var previousPen = SafeNativeMethods.SelectObject(hdc, staticPenUnderline.DangerousGetHandle());
                var previousBkMode = SafeNativeMethods.SetBkMode(hdc, NativeConstants.TRANSPARENT);

                SafeNativeMethods.MoveToEx(hdc, x - wordSize.Width, y + lineHeight - 2, IntPtr.Zero);
                SafeNativeMethods.LineTo(hdc, x, y + lineHeight - 2);

                SafeNativeMethods.SelectObject(hdc, previousPen);
                SafeNativeMethods.SetBkMode(hdc, previousBkMode);
            }
        }

        public override ITextSegmentStyled FindStyledTextSegment(ITextSegment textSegment, ITextDocument handler, int startIndex, int length)
        {
            if (startIndex >= textSegment.Text.Length)
            {
                return null;
            }

            if (length < 0)
            {
                length = 0;
            }

            for (int i = startIndex; i <= startIndex + length; i++)
            {
                int indexStart = i;
                int indexEnd = i;

                for (; indexStart > 0; indexStart--)
                {
                    if (Char.IsWhiteSpace(textSegment.Text[indexStart]))
                    {
                        indexStart++;
                        break;
                    }
                }

                for (; indexEnd < textSegment.Length; indexEnd++)
                {
                    if (Char.IsWhiteSpace(textSegment.Text[indexEnd]))
                    {
                        break;
                    }
                }

                if (indexEnd > indexStart)
                {
                    var potentialUrlString = textSegment.Text.Substring(indexStart, indexEnd - indexStart);

                    if (potentialUrlString.StartsWith("http:") || potentialUrlString.StartsWith("www."))
                    {
                        var newTextSegment = handler.CreateStyledTextSegment(this);

                        newTextSegment.Index = indexStart;
                        newTextSegment.Length = indexEnd - indexStart;

                        newTextSegment.Object = potentialUrlString;

                        return newTextSegment;
                    }
                }
            }

            return null;
        }

        public override bool Execute(ITextSegmentStyled styledTextSegment)
        {
            Process.Start(styledTextSegment.Object.ToString());

            return true;
        }

        public override TextStyleBase Clone()
        {
            var anchorStyle = new TextStyleURL();

            return anchorStyle;
        }
    }*/
}