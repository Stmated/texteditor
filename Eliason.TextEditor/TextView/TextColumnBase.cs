using System;
using System.Drawing;
using Eliason.TextEditor.TextDocument.ByLines;

namespace Eliason.TextEditor.TextView
{
    public abstract class TextColumnBase : IDisposable
    {
        private bool enabled;

        public abstract bool FloatLeft { get; }

        public abstract string Key { get; }
        public abstract string Name { get; }

        public abstract Image GetImage(ITextView textView);

        public bool Focused { get; private set; }

        public bool SetFocused(bool value, ITextView textView, int textColumnIndex)
        {
            var previous = this.Focused;
            this.Focused = value;

            if (previous != this.Focused)
            {
                if (this.Focused)
                {
                    this.PerformGotFocus(textView, textColumnIndex);
                }
                else
                {
                    this.PerformLostFocus(textView, textColumnIndex);
                }

                return true;
            }

            return false;
        }

        public int Width { get; private set; }

        public bool IsEnabled(ISettings settings)
        {
            return settings.IsTextColumnEnabled(this.Key);
        }

        public void SetEnabled(ISettings settings, bool value)
        {
            settings.SetTextColumnEnabled(this.Key, value);
        }

        //public TextColumnBase()
        //{
        //    this.Enabled = Bridge.Get().Get("Text.Column." + this.Key + ".Enabled", false);
        //}

        public abstract void PaintLine(IntPtr hdc, RendererState rs);

        public abstract void UpdateWidth(IntPtr hdc, ITextView textView);

        protected void SetWidth(int width)
        {
            this.Width = width;
        }

        public virtual void PerformMouseDown(ITextView textView, int lineIndex, Point p, int textColumnIndex)
        {
        }

        public virtual void PerformMouseUp(ITextView textView, int lineIndex, Point p, int textColumnIndex)
        {
        }

        public virtual void PerformMouseMove(ITextView textView, int lineIndex, Point p, int textColumnIndex)
        {
        }

        protected virtual void PerformGotFocus(ITextView textView, int textColumnIndex)
        {
        }

        protected virtual void PerformLostFocus(ITextView textView, int textColumnIndex)
        {
        }

        public abstract void Dispose();
    }
}