namespace Eliason.TextEditor.TextStyles
{
    public class RenderStateItem
    {
        public int ForeColorZIndex { get; set; }
        public int BackColorZIndex { get; set; }

        public int ForeColor { get; set; }
        public int BackColor { get; set; }

        public ITextSegmentStyled Segment { get; set; }

        public RenderStateItem()
        {
            this.ForeColor = -1;
            this.BackColor = -1;
        }
    }
}