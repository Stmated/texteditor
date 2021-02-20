#region

using System;
using System.Drawing;

#endregion

namespace Eliason.TextEditor.TextStyles
{
    public class TextStyleManual : TextStyleBase
    {
        private readonly string _description;
        private readonly TextStyleDisplayMode _displayMode;
        private readonly string _name;
        private readonly string _nameKey;
        private readonly TextStylePaintMode _paintMode;
        private readonly Color _colorBackground;
        private readonly Color _colorFont;
        private readonly Font _font;

        private int _win32ColorFore = -1;
        private int _win32ColorBack = -1;

        public int RenderZIndex { get; set; }

        public TextStyleManual(
            string name, string nameKey, string description, Color colorFont,
            Color colorBackground, Font font, TextStyleDisplayMode displayMode,
            TextStylePaintMode paintMode)
        {
            this._name = name;
            this._nameKey = nameKey;
            this._description = description;
            this._colorBackground = colorBackground;
            this._displayMode = displayMode;
            this._colorFont = colorFont;
            this._font = font;
            this._paintMode = paintMode;
        }

        public override TextStyleType Type
        {
            get { return TextStyleType.Manual; }
        }

        public override string Name
        {
            get { return this._name; }
        }

        public override string NameKey
        {
            get { return this._nameKey; }
        }

        public override string Description
        {
            get { return this._description; }
        }

        public Color ColorBackground
        {
            get { return this._colorBackground; }
        }

        public Color ColorFont
        {
            get { return this._colorFont; }
        }

        public Font Font
        {
            get { return this._font; }
        }

        public override TextStyleDisplayMode GetDisplayMode(ITextEditor textEditor)
        {
            return this._displayMode;
        }

        public override TextStylePaintMode PaintMode
        {
            get { return this._paintMode; }
        }

        public override RenderStateItem GetNaturalRenderColors(ITextEditor textEditor)
        {
            var rsi = new RenderStateItem();
            this.FillRenderStateItem(textEditor, rsi);
            return rsi;
        }

        public override void Paint(IntPtr hdc, ITextSegmentStyled textSegment, ITextView textView, TextSegmentVisualInfo info, int x, int y, int lineHeight, StyleRenderInfo sri)
        {
        }

        public override void FillRenderStateItem(ITextEditor textEditor, RenderStateItem rsi)
        {
            if (this._win32ColorFore == -1)
            {
                this._win32ColorFore = ColorTranslator.ToWin32(this.ColorFont);
                this._win32ColorBack = ColorTranslator.ToWin32(this.ColorBackground);
            }

            if (this._win32ColorBack != -1)
            {
                rsi.BackColor = this._win32ColorBack;
            }

            if (this._win32ColorFore != -1)
            {
                rsi.ForeColor = this._win32ColorFore;
            }

            rsi.BackColorZIndex = rsi.ForeColorZIndex = this.RenderZIndex;
        }

        public override void Dispose()
        {
            base.Dispose();
            
            if (this._font != null)
            {
                this._font.Dispose();
            }
        }

        public override TextStyleBase Clone()
        {
            var style = new TextStyleManual(
                this._name, this._nameKey, this._description,
                this._colorFont, this._colorBackground, this._font,
                this._displayMode, this._paintMode);

            return style;
        }
    }
}