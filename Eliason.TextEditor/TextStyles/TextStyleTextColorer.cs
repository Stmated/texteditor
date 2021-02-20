using System.Drawing;

namespace Eliason.TextEditor.TextStyles
{
    public abstract class TextStyleTextColorer : TextStyleBase
    {
        private bool _win32ColorsLoaded;
        private int _win32ForeColor = -1;
        private int _win32BackColor = -1;

        protected TextStyleTextColorer()
        {
        }

        public override void FillRenderStateItem(ITextEditor textEditor, RenderStateItem rsi)
        {
            if (this._win32ColorsLoaded == false)
            {
                this._win32ColorsLoaded = true;

                var cF = this.GetColorFore(textEditor); // Bridge.Get().Get("Text.Style." + NameKey + ".Text.Color.Fore", this.ColorFore);
                var cB = this.GetColorBack(textEditor); // Bridge.Get().Get("Text.Style." + NameKey + ".Text.Color.Back", this.ColorBack);

                if (cF.A != 0 && cF != Color.Empty)
                {
                    this._win32ForeColor = ColorTranslator.ToWin32(cF);
                }

                if (cB.A != 0 && cB != Color.Empty)
                {
                    this._win32BackColor = ColorTranslator.ToWin32(cB);
                }
            }

            if (this._win32BackColor != -1)
            {
                rsi.BackColor = this._win32BackColor;
            }

            if (this._win32ForeColor != -1)
            {
                rsi.ForeColor = this._win32ForeColor;
            }
        }

        public abstract Color GetColorFore(ITextEditor textEditor);
        public abstract Color GetColorBack(ITextEditor textEditor);
    }
}