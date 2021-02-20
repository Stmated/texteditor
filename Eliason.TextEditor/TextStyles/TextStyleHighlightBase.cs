using System;
using System.Collections.Generic;
using System.Drawing;

namespace Eliason.TextEditor.TextStyles
{
    public abstract class TextStyleHighlightBase : TextStyleTextColorer
    {
        public override string Name
        {
            get { return Strings.TextControl_Style_Highlight_Name; }
        }

        public override string NameKey
        {
            get { return "Highlight"; }
        }

        public override string Description
        {
            get { return Strings.TextControl_Style_Highlight_Description; }
        }

        public override TextStylePaintMode PaintMode
        {
            get { return TextStylePaintMode.Inline; }
        }

        public override Color GetColorBack(ITextEditor textEditor)
        {
            return textEditor.Settings.ColorHighlightBack;
        }

        public override Color GetColorFore(ITextEditor textEditor)
        {
            return textEditor.Settings.ColorHighlightFore;
        }

        protected abstract IEnumerable<string> WordList { get; }

        public override ITextSegmentStyled FindStyledTextSegment(ITextEditor textEditor, ITextSegment textSegment, ITextDocument document, int startIndex, int length, int textColumnIndex)
        {
            var text = textSegment.GetText(textColumnIndex);
            if (startIndex >= text.Length)
            {
                return null;
            }

            foreach (var word in this.WordList)
            {
                for (var i = 0; i < word.Length; i++)
                {
                    if (word[i] != text[startIndex])
                    {
                        continue;
                    }

                    var found = true;

                    var left = i - 1;
                    for (; left >= 0; left--)
                    {
                        var relativeIndex = startIndex - (i - left);

                        if (relativeIndex < 0)
                        {
                            found = false;
                            break;
                        }

                        var c = text[relativeIndex];
                        if (word[left] != c)
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found == false)
                    {
                        break;
                    }

                    left = Math.Max(0, left);

                    var right = i + 1;
                    for (; right < word.Length; right++)
                    {
                        var relativeIndex = startIndex + (right - i);

                        if (relativeIndex >= text.Length)
                        {
                            found = false;
                            break;
                        }

                        var c = text[relativeIndex];
                        if (word[right] != c)
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                    {
                        var newTextSegment = document.CreateStyledTextSegment(this);

                        var relativeIndex = startIndex - (i - left);
                        newTextSegment.Index = relativeIndex;
                        newTextSegment.SetLength(textColumnIndex, right - left);

                        newTextSegment.Object = word;

                        return newTextSegment;
                    }
                }
            }

            return null;
        }

        public override RenderStateItem GetNaturalRenderColors(ITextEditor textEditor)
        {
            return new RenderStateItem
            {
                BackColor = ColorTranslator.ToWin32(this.GetColorBack(textEditor)),
                ForeColor = ColorTranslator.ToWin32(this.GetColorFore(textEditor))
            };
        }

        public override void Paint(IntPtr hdc, ITextSegmentStyled textSegment, ITextView textView, TextSegmentVisualInfo info, int x, int y, int lineHeight, StyleRenderInfo sri)
        {
        }
    }
}