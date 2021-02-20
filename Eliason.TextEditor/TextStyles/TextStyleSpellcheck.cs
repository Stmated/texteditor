using System;
using System.Drawing;
using System.Globalization;
using Eliason.TextEditor.Native;
using Eliason.Common;

namespace Eliason.TextEditor.TextStyles
{
    public class TextStyleSpellcheck : TextStyleTextColorer
    {
        private static SafeHandleGDI staticPenUnderline;
        private CultureInfo _cultureInfo;

        public override string Name
        {
            get { return Strings.Spellchecking_Title; }
        }

        public override string NameKey
        {
            get { return "Spellcheck"; }
        }

        public override string Description
        {
            get { return Strings.TextControl_Style_Spellcheck_Description; }
        }

        public override bool UpdateOnlyOnFinalizingChange
        {
            get { return true; }
        }

        public override TextStylePaintMode PaintMode
        {
            get { return TextStylePaintMode.Custom; }
        }

        public override Color GetColorBack(ITextEditor textEditor)
        {
            return textEditor.Settings.ColorSpellcheckBack;
        }

        public override Color GetColorFore(ITextEditor textEditor)
        {
            return textEditor.Settings.ColorSpellcheckFore;
        }

        public override TextStyleType Type
        {
            get { return TextStyleType.Automatic; }
        }

        public override ITextSegmentStyled FindStyledTextSegment(ITextEditor textEditor, ITextSegment textSegment, ITextDocument document, int index, int length, int textColumnIndex)
        {
            // TODO: This is way too slow. Should be able to speed up significantly.
            foreach (var textView in document.GetTextViews())
            {
                this._cultureInfo = textView.Language;

                if (this._cultureInfo != null)
                {
                    break;
                }
            }

            if (textEditor.Settings.SpellcheckEnabled(this._cultureInfo) == false || textEditor.Settings.InlineEnabled == false)
            {
                return null;
            }

            var searchStartIndex = textSegment.Index + index;
            var selectedWord = document.GetWord(searchStartIndex, textSegment, true, textColumnIndex);

            if (selectedWord == null)
            {
                // If the current index has no word, then we exit.
                return null;
            }

            if (selectedWord.End - selectedWord.Start < 3)
            {
                // If the word is shorter than 3 characters, then we exit.
                return null;
            }

            if (textEditor.Settings.CheckIfWordIsValid(selectedWord.Word, textSegment.GetText(textColumnIndex)) == false)
            {
                // If the word is not a valid word according to out filter settings, then we exit.
                return null;
            }

            try
            {
                //var spellcheckValid = ;
                //foreach (var check in LanguageFeature.FeatureFetchMultiple<bool>(this._cultureInfo, "Spellcheck.Check", new LFSString(selectedWord.Word)))
                //{
                //    spellcheckValid = check;
                //    if (spellcheckValid)
                //    {
                //        break;
                //    }
                //}

                if (textEditor.Settings.IsSpelledCorrectly(selectedWord.Word))
                {
                    // If the word is a correctly spelled word, then we exit.
                    return null;
                }
            }
            catch (Exception ex)
            {
            }

            // The word is not spelled correctly, so we create a style and return it.
            var incorrectlySpelledTextSegment = document.CreateStyledTextSegment(this);

            incorrectlySpelledTextSegment.Index = selectedWord.Start - textSegment.Index;
            incorrectlySpelledTextSegment.SetLength(textColumnIndex, selectedWord.End - selectedWord.Start);
            incorrectlySpelledTextSegment.Object = selectedWord.Word;

            return incorrectlySpelledTextSegment;
        }

        private static Color staticUnderlineColor;

        public override RenderStateItem GetNaturalRenderColors(ITextEditor textEditor)
        {
            var rsi = new RenderStateItem();
            this.FillRenderStateItem(textEditor, rsi);

            rsi.ForeColor = ColorTranslator.ToWin32(staticUnderlineColor);

            return rsi;
        }

        public override void Paint(IntPtr hdc, ITextSegmentStyled styledSegment, ITextView textView, TextSegmentVisualInfo info, int x, int y, int lineHeight, StyleRenderInfo sri)
        {
            //if (this._paintUnderline == null)
            //{
            //    this._paintUnderline = this.Settings.SpellcheckUnderlineEnabled; // Bridge.Get().GetSafe("Text.Style.Spellcheck.Underline.Enabled", true);
            //}

            if (textView.Settings.SpellcheckUnderlineEnabled == false)
            {
                return;
            }

            if (staticPenUnderline == null)
            {
                staticUnderlineColor = textView.Settings.ColorSpellcheckUnderline; // Bridge.Get().GetSafe("Text.Style.Spellcheck.Underline.Color", Color.Red);

                //int underlineType = Bridge.Get().GetSafe("Text.Style.Spellcheck.Underline.Type", (int) PenType.Dot);
                staticPenUnderline = new SafeHandleGDI(SafeNativeMethods.CreatePen((int)textView.Settings.SpellcheckUnderlineType, -1, ColorTranslator.ToWin32(staticUnderlineColor)));
            }

            var wordSize = styledSegment.Parent.GetSize(hdc, x, styledSegment.Index, styledSegment.GetLength(info.TextColumnIndex), info);

            var previousPen = SafeNativeMethods.SelectObject(hdc, staticPenUnderline.DangerousGetHandle());
            var previousBkMode = SafeNativeMethods.SetBkMode(hdc, NativeConstants.TRANSPARENT);

            SafeNativeMethods.MoveToEx(hdc, x - wordSize.Width, y + lineHeight - 2, IntPtr.Zero);
            SafeNativeMethods.LineTo(hdc, x, y + lineHeight - 2);

            SafeNativeMethods.SelectObject(hdc, previousPen);
            SafeNativeMethods.SetBkMode(hdc, previousBkMode);
        }

        public override TextStyleBase Clone()
        {
            var anchor = new TextStyleSpellcheck();

            return anchor;
        }
    }
}