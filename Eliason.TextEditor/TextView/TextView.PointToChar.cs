#region

using System;
using System.Drawing;

#endregion

namespace Eliason.TextEditor.TextView
{
    partial class TextView
    {
        public Point GetPositionFromCharIndex(int index)
        {
            var p = this.GetVirtualPositionFromCharIndex(index);

            return new Point(
                p.X - this.ScrollHost.ScrollPosH,
                p.Y - this.ScrollHost.ScrollPosVIntegral);
        }

        public Point GetVirtualPositionFromCharIndex(int index)
        {
            return this.GetVirtualPositionFromCharIndex(index, this.CurrentTextColumnIndex);
        }

        public Point GetVirtualPositionFromCharIndex(int index, int textColumnIndex)
        {
            float x;
            float y = 0;

            var lineIndex = GetLineFromCharIndex(index);

            if (lineIndex == -1)
            {
                return Point.Empty;
            }

            for (var i = 0; i < lineIndex; i++)
            {
                if (this.GetVisualInformation(i).GetLineSplitIndexes(textColumnIndex) == null)
                {
                    y = y + this.LineHeight;
                }
                else
                {
                    y = y + (this.GetVisualInformation(i).GetLineCountVisual(textColumnIndex)*this.LineHeight);
                }
            }

            var relativeIndex = index - GetFirstCharIndexFromLine(lineIndex);

            var lineSplitIndexes = this.GetVisualInformation(lineIndex).GetLineSplitIndexes(textColumnIndex);

            if (lineSplitIndexes == null)
            {
                var lineLength = GetLineLength(lineIndex);
                var lineSizeCharLength = Math.Min(lineLength, relativeIndex);

                x = this.GetLineSize(lineIndex, 0, 0, lineSizeCharLength, textColumnIndex).Width;
            }
            else
            {
                var lastNewlineCharIndex = 0;
                for (var i = 0; i < lineSplitIndexes.Length; i++)
                {
                    if (relativeIndex <= lineSplitIndexes[i])
                    {
                        break;
                    }

                    lastNewlineCharIndex = lineSplitIndexes[i];
                    y += this.LineHeight;
                }

                var size = this.GetLineSize(lineIndex, 0, lastNewlineCharIndex, relativeIndex - (lastNewlineCharIndex), textColumnIndex);

                x = size.Width;
            }

            var textRectangle = this.GetTextRectangle(true);
            return new Point(Math.Max(0, (int)x + textRectangle.Left), (int)y + textRectangle.Top);
        }

        public int GetCharIndexFromVirtualPosition(Point p, int textColumnIndex)
        {
            var textRectangle = this.GetTextRectangle(false);
            float y = textRectangle.Top;
            var lineCount = LineCount;
            var lineIndex = 0;
            TextSegmentVisualInfos visualInfos = null;

            for (; lineIndex < lineCount; lineIndex++)
            {
                // First we quickly go through all the lines just to find which line/Y we are on.
                visualInfos = this.GetVisualInformation(lineIndex);
                float lineTotalheight = (visualInfos.GetLineCountVisual(textColumnIndex)*this.LineHeight);

                if (y + lineTotalheight <= p.Y)
                {
                    // We are still not on the same line as the clicked line.
                    // So we iterate the loop again right away.
                    y += lineTotalheight;
                }
                else
                {
                    break;
                }
            }

            if (lineIndex == lineCount)
            {
                return this.TextLength;
            }

            // We are now on the same line as the supplied position.
            var start = -1;
            var end = -1;

            if (visualInfos.GetLineSplitIndexes(textColumnIndex) == null)
            {
                y += this.LineHeight;
                start = 0;
                end = this.GetLineLength(lineIndex);
            }
            else
            {
                var linesplits = visualInfos.GetLineSplitIndexes(textColumnIndex);

                // Then we go through the possible wordwrappings and find the actual line that we are on.
                for (var i = 0; i < linesplits.Length + 1; i++)
                {
                    if (p.Y <= y + this.LineHeight)
                    {
                        start = i == 0 ? 0 : linesplits[i - 1];
                        end = i == linesplits.Length ? this.GetLineLength(lineIndex) : linesplits[i];

                        break;
                    }

                    y += this.LineHeight;
                }
            }

            if (end <= start || p.X <= textRectangle.Left)
            {
                // If we are outside the text document, then we just return the first index of the line.
                return this.GetFirstCharIndexFromLine(lineIndex);
            }

            // Get the width of the whole string if rendered on one line.
            var lineWidth = this.GetLineSize(lineIndex, 0, start, end - start, textColumnIndex).Width;
            var averageCharWidth = lineWidth/(double) (end - start);

            // Get the average width of a character in the string.
            // And from that, guess the correct character index of the Point.
            var guessedStart = Math.Min(end, (int) Math.Round((p.X - (textRectangle.Left - Padding.Left))/averageCharWidth));
            var index = Math.Min(start + guessedStart, end);

            // And then to finetune, we check if we are in a match, and
            // iterate through the string to find the position.
            for (; index < end && index > start;)
            {
                // We get the current line (wordwrapping accounted for), and then we get the left and right characters' half width.
                // From that, we can see if we are in a fitting range for if this current index is the closest one to the position.
                var w = this.GetLineSize(lineIndex, 0, start, index - start, textColumnIndex).Width + (textRectangle.Left);
                var wl = this.GetLineSize(lineIndex, 0, index - 1, 1, textColumnIndex).Width*0.67;
                var wr = this.GetLineSize(lineIndex, 0, index, 1, textColumnIndex).Width*0.34;

                if (p.X >= (w - wl) && p.X <= (w + wr))
                {
                    return this.GetFirstCharIndexFromLine(lineIndex) + index;
                }

                //bool? incrementing = w < p.X;
                //if (incrementing.Value)
                if (w < p.X)
                {
                    //if (incrementing.HasValue && incrementing.Value == false)
                    //{
                    // We are in a perfect location between two characters, and no definite match is available.
                    // Which is noticed by first being told to look at a different index for a match, and then being told to go back.
                    // Endless loop would ensue.

                    // So we will just return from here and take the current index as the most likely. Which is fine.
                    // This hapens mostly if we have many different kind of characters on the same line,
                    // which disrupts the "averageCharWidth" to an irregular decimal number.
                    //    break;
                    //}

                    index++;
                }
                else
                {
                    //if (incrementing.HasValue && incrementing.Value)
                    //{
                    // See comment above.
                    //    break;
                    //}

                    index--;
                }
            }

            return this.GetFirstCharIndexFromLine(lineIndex) + index;
        }

        public int GetCharIndexFromPhysicalPosition(Point p)
        {
            return this.GetCharIndexFromPhysicalPosition(p, this.CurrentTextColumnIndex);
        }

        public int GetCharIndexFromPhysicalPosition(Point p, int textColumnIndex)
        {
            p.Offset(this.ScrollHost.ScrollPosH, this.ScrollHost.ScrollPosVIntegral);

            return this.GetCharIndexFromVirtualPosition(p, textColumnIndex);
        }
    }
}