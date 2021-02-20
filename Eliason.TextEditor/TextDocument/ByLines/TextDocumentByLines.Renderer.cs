using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Eliason.Common;
using Eliason.TextEditor.Native;
using Eliason.TextEditor.TextStyles;

namespace Eliason.TextEditor.TextDocument.ByLines
{
    public class RendererState
    {
        private readonly int _lineHeight;
        private readonly Rectangle _textRectangle;

        public ITextView TextView { get; set; }
        public RenderState RenderState { get; set; }
        public ITextSegmentVisual Line { get; set; }
        public int LineIndexPhysical { get; set; }
        public int LineIndexVirtual { get; set; }
        public int LineIndexVirtualFocused { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int ViewportX { get; set; }
        public int ViewportY { get; set; }

        public Rectangle TextRectangle
        {
            get { return this._textRectangle; }
        }

        public int LineHeight
        {
            get { return this._lineHeight; }
        }

        public RendererState(Rectangle textRectangle, int lineHeight)
        {
            this._textRectangle = textRectangle;
            this._lineHeight = lineHeight;
            this.LineIndexVirtualFocused = -1;
        }
    }

    public partial class TextDocumentByLines
    {
        private class TextDocumentLineRenderer : ITextDocumentRenderer
        {
            private const int TOOL_TIP_CALL_COUNT_MAX = 60;
            private int _tooltipCallCount;
            private bool _isPaintingTooltip;
            private Timer _tooltipTimer;
            private string _tooltipTextPrevious;
            private static readonly IntPtr staticPenWordwrap = SafeNativeMethods.CreatePen(NativeConstants.PS_DOT, -1, ColorTranslator.ToWin32(Color.Gray));
            private Font _fontTooltipOverlay;
            private SafeHandleGDI _brushCurrentLine;

            public ITextView TextView { get; private set; }

            private TextDocumentByLines Strategy { get; set; }

            public ITextSegmentStyled FocusedStyledSegment { get; set; }

            public TextDocumentLineRenderer(ITextView textView, TextDocumentByLines strategy)
            {
                this.TextView = textView;
                this.Strategy = strategy;
            }

            public void Dispose()
            {
                if (this._brushCurrentLine != null)
                {
                    this._brushCurrentLine.Dispose();
                    this._brushCurrentLine = null;
                }

                if (this._fontTooltipOverlay != null)
                {
                    this._fontTooltipOverlay.Dispose();
                }
            }

            public unsafe void Render(IntPtr hdc, int vpx, int vpy, Size size)
            {
                #region Initialize GDI resources

                if (this._brushCurrentLine == null)
                {
                    var colorWithoutTransparency = Color.FromArgb(this.TextView.LineHighlightColor.R, this.TextView.LineHighlightColor.G, this.TextView.LineHighlightColor.B);
                    this._brushCurrentLine = new SafeHandleGDI(SafeNativeMethods.CreateSolidBrush(ColorTranslator.ToWin32(colorWithoutTransparency)));
                }

                #endregion

                var styleSelection = this.TextView.GetTextStyle("Selection");
                var styleDefault = this.TextView.GetTextStyle("Default");

                var rs = new RendererState(this.TextView.GetTextRectangle(false), this.TextView.LineHeight);
                rs.TextView = this.TextView;
                rs.RenderState = new RenderState(this.TextView, styleDefault);
                rs.ViewportX = vpx - rs.TextRectangle.Left;
                rs.ViewportY = vpy;

                ITextSegmentStyled selectionSegment = null;
                ITextSegment firstVisibleLine = null;

                //var foundActiveLine = this.TextView.LineHighlightColor.A == Color.Transparent.A || this.TextView.LineHighlightColor == Color.Empty;

                var textColumnIndex = this.TextView.CurrentTextColumnIndex;

                var sri = new StyleRenderInfo(this.FocusedStyledSegment);

                // TODO: Paint some nice graphics for the current text column index, when it is stable enough.
                var clientSize = this.TextView.ClientSize;
                var lineCount = this.Strategy.Lines.Count;
                var firstCharIndex = this.TextView.GetFirstCharIndexFromLine(this.TextView.GetCurrentLine());
                var lineIndexPhysical = 0;
                for (rs.LineIndexPhysical = 0; rs.LineIndexPhysical < lineCount; lineIndexPhysical++, rs.LineIndexPhysical++)
                {
                    if (rs.Y > rs.ViewportY + size.Height)
                    {
                        // This line is after the viewport and does not need painting.
                        break;
                    }

                    rs.Line = this.Strategy.Lines[lineIndexPhysical];
                    var visualInfo = this.TextView.GetVisualInformation(lineIndexPhysical);
                    var lineCountVisual = visualInfo.GetLineCountVisual(textColumnIndex);
                    var linesHeight = lineCountVisual * rs.LineHeight;

                    if (rs.Y + linesHeight <= rs.ViewportY)
                    {
                        // This line is before the viewport.
                        rs.Y += linesHeight;
                        rs.LineIndexVirtual += lineCountVisual;
                        continue;
                    }

                    if (rs.Line.Index >= firstCharIndex && rs.Line.Index <= (this.TextView.SelectionStart + this.TextView.SelectionLength))
                    {
                        rs.LineIndexVirtualFocused = rs.LineIndexVirtual;

                        if (this.TextView.SelectionLength == 0)
                        {
                            this.RenderSelectedLineBackground(hdc, rs);
                        }
                    }

                    var textPoints = new Dictionary<int, List<ITextSegmentStyled>>();

                    if (firstVisibleLine == null)
                    {
                        firstVisibleLine = rs.Line;

                        #region Special handling for selection

                        if (this.TextView.SelectionLength > 0)
                        {
                            if (this.TextView.SelectionStart < rs.Line.Index && this.TextView.SelectionStart + this.TextView.SelectionLength >= rs.Line.Index)
                            {
                                // The selection begins on the line before this.
                                selectionSegment = new TextAnchor(styleSelection);
                                selectionSegment.Index = firstVisibleLine.Index;

                                var textPointIndex = rs.Line.Index;
                                if (textPoints.ContainsKey(textPointIndex) == false)
                                {
                                    textPoints.Add(textPointIndex, new List<ITextSegmentStyled>());
                                }

                                textPoints[textPointIndex].Add(selectionSegment);
                            }
                        }

                        #endregion
                    }

                    var lineLength = rs.Line.GetLength(textColumnIndex);
                    if (this.TextView.SelectionLength > 0)
                    {
                        #region Special handling for selection

                        if (this.TextView.SelectionStart >= rs.Line.Index && this.TextView.SelectionStart <= rs.Line.Index + lineLength)
                        {
                            // The selection begins on this line.
                            selectionSegment = new TextAnchor(styleSelection);
                            selectionSegment.Index = rs.Line.Index;
                            selectionSegment.SetLength(textColumnIndex, this.TextView.SelectionLength);

                            var textPointIndex = this.TextView.SelectionStart;
                            if (textPoints.ContainsKey(textPointIndex) == false)
                            {
                                textPoints.Add(textPointIndex, new List<ITextSegmentStyled>());
                            }

                            textPoints[textPointIndex].Add(selectionSegment);
                        }

                        if (this.TextView.SelectionStart + this.TextView.SelectionLength >= rs.Line.Index
                            && this.TextView.SelectionStart + this.TextView.SelectionLength <= rs.Line.Index + lineLength)
                        {
                            // The selection ends on this line.
                            var textPointIndex = -(this.TextView.SelectionStart + this.TextView.SelectionLength);
                            if (textPoints.ContainsKey(textPointIndex) == false)
                            {
                                textPoints.Add(textPointIndex, new List<ITextSegmentStyled>());
                            }

                            textPoints[textPointIndex].Add(selectionSegment);
                        }

                        #endregion
                    }

                    var textLine = (TextLine)rs.Line;
                    if (textLine.StyledTextSegments != null && textLine.StyledTextSegments.Count > 0)
                    {
                        foreach (var textSegment in textLine.StyledTextSegments)
                        {
                            var textPointStartIndex = textLine.Index + textSegment.Index;
                            if (textPoints.ContainsKey(textPointStartIndex) == false)
                            {
                                textPoints.Add(textPointStartIndex, new List<ITextSegmentStyled>());
                            }

                            textPoints[textPointStartIndex].Add(textSegment);

                            var textPointEndIndex = -(textLine.Index + textSegment.Index + textSegment.GetLength(textColumnIndex));
                            if (textPoints.ContainsKey(textPointEndIndex) == false)
                            {
                                textPoints.Add(textPointEndIndex, new List<ITextSegmentStyled>());
                            }

                            textPoints[textPointEndIndex].Add(textSegment);
                        }
                    }

                    var wordWrapIndex = 0;
                    var tabIndex = 0;
                    rs.X = 0;
                    var previousTextIndex = 0;
                    //var textLength = rs.Line.GetLength(textColumnIndex);
                    var lineSplitIndexes = visualInfo.GetLineSplitIndexes(textColumnIndex);
                    for (var textIndex = 0; textIndex <= lineLength; textIndex++)
                    {
                        var isLastIndex = textIndex == lineLength;

                        var isNewline = lineSplitIndexes != null;
                        if (isNewline)
                        {
                            isNewline = wordWrapIndex < lineSplitIndexes.Length && lineSplitIndexes[wordWrapIndex] == textIndex;

                            if (isNewline)
                            {
                                wordWrapIndex++;
                            }
                        }

                        var isTab = visualInfo.GetTabSplitIndexes(textColumnIndex) != null;
                        if (isTab)
                        {
                            isTab = tabIndex < visualInfo.GetTabSplitIndexes(textColumnIndex).Length && visualInfo.GetTabSplitIndexes(textColumnIndex)[tabIndex] == textIndex;

                            if (isTab)
                            {
                                tabIndex++;
                            }
                        }

                        var globalIndex = rs.Line.Index + textIndex;
                        var styleStart = textPoints.ContainsKey(globalIndex) ? textPoints[globalIndex] : null;
                        var styleEnd = globalIndex != 0 && textPoints.ContainsKey(-globalIndex) ? textPoints[-globalIndex] : null;

                        if ((isLastIndex || isNewline || isTab || styleStart != null || styleEnd != null) == false)
                        {
                            // This is a regular character, and no change is done for it.
                            continue;
                        }

                        var start = previousTextIndex;
                        var length = textIndex - start;

                        var outputText = rs.Line.GetText(textColumnIndex);

                        #region Add empty space at end of text if selection goes beyond current line

                        if (start + length == lineLength)
                        {
                            foreach (var rsi in rs.RenderState.GetRenderStateItems())
                            {
                                if (rsi.Segment.Style.NameKey != styleSelection.NameKey)
                                {
                                    continue;
                                }

                                if (styleEnd != null)
                                {
                                    var isStyleSelection = false;
                                    foreach (var se in styleEnd)
                                    {
                                        if (se.Style.NameKey == styleSelection.NameKey)
                                        {
                                            isStyleSelection = true;
                                            break;
                                        }
                                    }

                                    if (isStyleSelection)
                                    {
                                        break;
                                    }
                                }

                                outputText += " ";
                                length++;

                                break;
                            }
                        }

                        #endregion

                        fixed (char* c = outputText)
                        {
                            SafeNativeMethods.TextOut(hdc, rs.TextRectangle.Left + (rs.X - rs.ViewportX), rs.TextRectangle.Top + rs.Y - rs.ViewportY, c + start, length);

                            if ((isNewline == false && isLastIndex == false) || styleEnd != null)
                            {
                                rs.X += rs.Line.GetSize(hdc, rs.X, start, length, visualInfo.GetVisualInfo(textColumnIndex)).Width;
                            }

                            if (styleEnd != null)
                            {
                                foreach (var t in styleEnd)
                                {
                                    switch (t.Style.PaintMode)
                                    {
                                        case TextStylePaintMode.Custom:
                                        {
                                                t.Style.Paint(hdc, t, this.TextView, visualInfo.GetVisualInfo(textColumnIndex),
                                                    rs.TextRectangle.Left + (rs.X - rs.ViewportX),
                                                    rs.TextRectangle.Top + rs.Y - rs.ViewportY,
                                                    rs.LineHeight,
                                                    sri);
                                            }
                                            break;
                                    }
                                }
                            }
                        }

                        if (isNewline)
                        {
                            this.RenderLineColumns(hdc, clientSize, rs);
                            rs.X = 0;
                            rs.LineIndexVirtual++;
                            rs.Y += rs.LineHeight;
                        }

                        if (styleStart != null)
                        {
                            foreach (var t in styleStart)
                            {
                                rs.RenderState.Add(this.TextView, t);
                            }

                            rs.RenderState.Apply(hdc);
                        }

                        if (styleEnd != null)
                        {
                            foreach (var t in styleEnd)
                            {
                                rs.RenderState.Remove(t);
                            }

                            rs.RenderState.Apply(hdc);
                        }

                        previousTextIndex = textIndex;
                    }

                    if (this.TextView.WordWrapGlyphs && lineCountVisual > 1)
                    {
                        RenderWordWrapGlyph(hdc, rs.ViewportY, rs.Y, rs.TextRectangle, rs.LineHeight);
                    }

                    this.RenderLineColumns(hdc, clientSize, rs);
                    rs.X = 0;
                    rs.LineIndexVirtual++;
                    rs.Y += rs.LineHeight;
                }

                this.PaintTooltipOverlay(hdc);
            }

            private void RenderLineColumns(IntPtr hdc, Size clientSize, RendererState rs)
            {
                // TODO: Set the "X" property of the render state for the target to use (hardcoded positions right now)
                var offsetLeft = 0;
                var offsetRight = 0;
                var settings = this.TextView.Settings;
                foreach (var column in this.TextView.Columns)
                {
                    if (column.IsEnabled(settings))
                    {
                        // Set the render state's current X location.
                        // It will be reset to zero after this method returns, in the main render method.
                        rs.X = (column.FloatLeft ? offsetLeft : clientSize.Width - column.Width - offsetRight);
                        column.PaintLine(hdc, rs);
                    }

                    if (column.FloatLeft)
                    {
                        offsetLeft += column.Width;
                    }
                    else
                    {
                        offsetRight += column.Width;
                    }
                }
            }

            private static void RenderWordWrapGlyph(IntPtr hdc, int viewportY, int topY, Rectangle textRect, int lineHeight)
            {
                var previousPen = SafeNativeMethods.SelectObject(hdc, staticPenWordwrap);
                var previousBkMode = SafeNativeMethods.SetBkMode(hdc, NativeConstants.TRANSPARENT);

                SafeNativeMethods.MoveToEx(hdc, textRect.Left, topY + lineHeight - viewportY - 1, IntPtr.Zero);
                SafeNativeMethods.LineTo(hdc, textRect.Left + 10, topY + lineHeight - viewportY - 1);

                SafeNativeMethods.SelectObject(hdc, previousPen);
                SafeNativeMethods.SetBkMode(hdc, previousBkMode);
            }

            private void RenderSelectedLineBackground(IntPtr hdc, RendererState rs)
            {
                var rect = new RECT
                {
                    left = rs.TextRectangle.Left - this.TextView.ScrollHost.ScrollPosH,
                    right = int.MaxValue,
                    top = rs.TextRectangle.Top + rs.Y - rs.ViewportY,
                    bottom = rs.TextRectangle.Top + rs.Y + rs.LineHeight - rs.ViewportY
                };

                if (this.TextView.BackgroundImage == null)
                {
                    SafeNativeMethods.FillRect(hdc, ref rect, this._brushCurrentLine.DangerousGetHandle());
                }
                else
                {
                    // Paint using GDI+ if a background image is used, for the sake of using transparency.
                    using (var g = Graphics.FromHdc(hdc))
                    {
                        HsvColor c = HsvColor.FromColor(this.TextView.LineHighlightColor);

                        var alpha = (int)(255 - (255 * ((100 - c.Saturation) / 100d)));
                        c.Saturation = 100;

                        using (var currentLineBrush = new SolidBrush(Color.FromArgb(alpha, c.ToColor())))
                        {
                            g.FillRectangle(currentLineBrush, rect.left, rect.top, this.TextView.ClientSize.Width, rs.LineHeight);
                        }
                    }
                }
            }

            private void PaintTooltipOverlay(IntPtr hdc)
            {
                const double firstStepTh = TOOL_TIP_CALL_COUNT_MAX / 6d;

                if (this._isPaintingTooltip)
                {
                    #region Paint tooltip using GDI+ (slow, but can handle alpha, hence prettier)

                    double alpha;

                    if (this._tooltipCallCount < firstStepTh)
                    {
                        alpha = 255 * (this._tooltipCallCount / firstStepTh);
                    }
                    else
                    {
                        if (this._tooltipTextPrevious != this.TextView.TooltipText)
                        {
                            this._tooltipCallCount = (int)firstStepTh;
                            this._tooltipTextPrevious = this.TextView.TooltipText;
                        }

                        var half = TOOL_TIP_CALL_COUNT_MAX / 2d;

                        if (this._tooltipCallCount > half)
                        {
                            alpha = 255 - (255 * ((this._tooltipCallCount - half) / half));
                        }
                        else
                        {
                            alpha = 255;
                        }
                    }

                    using (var b1 = new SolidBrush(Color.FromArgb((int)alpha, 255, 255, 255)))
                    {
                        if (this._fontTooltipOverlay == null)
                        {
                            this._fontTooltipOverlay = new Font(this.TextView.Font.FontFamily, 30, FontStyle.Bold);
                        }

                        var c = Color.FromArgb((int)(alpha / 2), Color.LightBlue);
                        var t = this.TextView.TooltipText;

                        if (2 == 0)
                        {
                            #region Paint tooltip with win32 -- much faster, but less user-friendly

                            /*
                            unsafe
                            {
                                fixed (char* ch = t)
                                {
                                    var fHandle = this.fontTooltipOverlay.ToHfont();
                                    var previousFont = SafeNativeMethods.SelectObject(hdc, fHandle);

                                    Size tooltipSize = Size.Empty;
                                    if (SafeNativeMethods.GetTextExtentPoint32(hdc, ch, t.Length, ref tooltipSize) == false)
                                    {
                                        throw new InvalidOperationException("The fetching of the text extent failed.");
                                    }

                                    var rect = new RECT
                                                   {
                                                       left = 0,
                                                       top = 10,
                                                       right = tooltipSize.Width,
                                                       bottom = 10 + tooltipSize.Height
                                                   };

                                    var bkgBrush = SafeNativeMethods.CreateSolidBrush(ColorTranslator.ToWin32(c));

                                    SafeNativeMethods.FillRect(hdc, ref rect, bkgBrush);

                                    var previousTextColor = SafeNativeMethods.SetTextColor(hdc, ColorTranslator.ToWin32(Color.White));

                                    SafeNativeMethods.TextOut(hdc, rect.left, rect.top, ch, t.Length);

                                    SafeNativeMethods.DeleteObject(bkgBrush);
                                    SafeNativeMethods.SelectObject(hdc, previousFont);
                                    SafeNativeMethods.DeleteObject(fHandle);
                                    SafeNativeMethods.SetTextColor(hdc, previousTextColor);
                                }
                            }*/

                            #endregion
                        }
                        using (var g = Graphics.FromHdc(hdc))
                        {
                            var tooltipSize = g.MeasureString(t, this._fontTooltipOverlay, int.MaxValue, StringFormat.GenericDefault);

                            var tooltipRect = new RectangleF(0, 10, tooltipSize.Width * 1.2f, tooltipSize.Height);

                            using (var rectBrush = new SolidBrush(c))
                            {
                                g.FillRectangle(rectBrush, tooltipRect);
                            }

                            g.DrawString(t, this._fontTooltipOverlay, b1, tooltipRect);
                        }
                    }

                    #endregion
                }
                else if (this._tooltipTextPrevious != this.TextView.TooltipText)
                {
                    this._isPaintingTooltip = true;
                    this._tooltipCallCount = 0;

                    if (this._tooltipTimer == null)
                    {
                        this._tooltipTimer = new Timer { Interval = 25 };
                        this._tooltipTimer.Tick += this.tooltipTimer_Tick;
                    }

                    this._tooltipTimer.Stop();
                    this._tooltipTimer.Start();

                    this._tooltipTextPrevious = this.TextView.TooltipText;
                }
            }

            private void tooltipTimer_Tick(object sender, EventArgs e)
            {
                this._tooltipCallCount++;

                if (this._tooltipCallCount > TOOL_TIP_CALL_COUNT_MAX)
                {
                    this._isPaintingTooltip = false;
                    this._tooltipTimer.Stop();
                    this._tooltipTextPrevious = this.TextView.TooltipText;
                }

                this.TextView.Invalidate();
            }
        }
    }
}