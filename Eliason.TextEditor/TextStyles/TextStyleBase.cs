using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Eliason.TextEditor.TextStyles
{
    public abstract class TextStyleBase : IDisposable, ICloneable<TextStyleBase>
    {
        protected TextStyleBase()
        {
            //this.Settings = settings;
        }

        //protected ISettings Settings { get; private set; }

        public virtual bool UpdateOnlyOnFinalizingChange
        {
            get { return false; }
        }

        public virtual TextStyleType Type
        {
            get { return TextStyleType.Automatic; }
        }

        public virtual TextStyleDisplayMode GetDisplayMode(ITextEditor textEditor)
        {
            return textEditor.Settings.GetDisplayMode(this);
        }

        public abstract TextStylePaintMode PaintMode { get; }

        [Localizable(true)]
        public abstract string Name { get; }

        public abstract string NameKey { get; }

        [Localizable(true)]
        public abstract string Description { get; }

        public virtual bool CanExecute
        {
            get { return false; }
        }

        public virtual ITextSegmentStyled FindStyledTextSegment(ITextEditor textEditor, ITextSegment textSegment, ITextDocument document, int index, int length, int textColumnIndex)
        {
            return null;
        }

        public virtual void PaintResetBuffer()
        {
        }

        public abstract void Paint(IntPtr hdc, ITextSegmentStyled textSegment, ITextView textView, TextSegmentVisualInfo info, int x, int y, int lineHeight, StyleRenderInfo sri);

        /// <summary>
        /// Fills the specified <see cref="RenderStateItem"/> with the colors used by this text style.
        /// </summary>
        /// <param name="textEditor"></param>
        /// <param name="rsi"></param>
        public abstract void FillRenderStateItem(ITextEditor textEditor, RenderStateItem rsi);

        /// <summary>
        /// Gets a render state that describes the most natural representation colors of the style.
        /// This is used so that no matter what kind of paintings the style does, it should be able to represent itself
        /// with two colors, for other parts of the application to make use of.
        /// </summary>
        /// <param name="textEditor"></param>
        public abstract RenderStateItem GetNaturalRenderColors(ITextEditor textEditor);

        public virtual string GetStringRepresentation(Object obj)
        {
            return obj.ToString();
        }

        public virtual void ShowInfo(ITextSegmentStyled textSegment)
        {
            //this.Settings.Notifier.Info("Not implemented yet. Will show some info.", "Text Anchor");
        }

        public virtual bool Execute(ITextSegmentStyled styledTextSegment)
        {
            return false;
        }

        public virtual void Dispose()
        {
        }

        public abstract TextStyleBase Clone();

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}