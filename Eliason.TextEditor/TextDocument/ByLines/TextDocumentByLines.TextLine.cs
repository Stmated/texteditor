#region

using System;
using System.Collections.Generic;
using System.Drawing;
using Eliason.TextEditor.Native;
using Eliason.TextEditor.TextStyles;
using Eliason.Common;

#endregion

namespace Eliason.TextEditor.TextDocument.ByLines
{
    public partial class TextDocumentByLines
    {
        /// <summary>
        ///   TODO: 
        /// 
        ///   Make "string Text" into "string[] Texts" that is the string split by wordwrap newlines and tabs,
        ///   and remove LineSplitIndexes and TabSplitIndexes and instead make a SplitIndexes that contains an
        ///   enum if it is a newline, tab, or potentially anything else.
        /// </summary>
        private class TextLine : ITextSegmentVisual
        {
            private static int staticTabWidth = -1;

            private readonly List<TextAnchor> _styledTextSegments = new List<TextAnchor>();

            private readonly Dictionary<string, string> _metadata = new Dictionary<string, string>();

            public TextLine(int index, string text)
            {
                this.Index = index;
                this.Texts = new[] {text};
            }

            ~TextLine()
            {
                this.ClearStyles(true);
            }

            public List<TextAnchor> StyledTextSegments
            {
                get { return this._styledTextSegments; }
            }

            public IDictionary<string, string> Metadata
            {
                get { return this._metadata; }
            }

            public string[] Texts { get; private set; }

            public string GetText(int textColumnIndex)
            {
                return this.Texts.Length <= textColumnIndex ? "" : this.Texts[textColumnIndex];
            }

            /// <summary>
            /// TODO: Protect from NullReferenceException and expansion of the array.
            /// </summary>
            /// <param name="textColumnIndex"></param>
            /// <param name="text"></param>
            public void SetText(int textColumnIndex, string text)
            {
                if (textColumnIndex >= this.Texts.Length)
                {
                    var newTexts = new string[this.Texts.Length + 1];
                    this.Texts.CopyTo(newTexts, 0);
                    newTexts[this.Texts.Length] = text;

                    this.Texts = newTexts;
                }

                this.Texts[textColumnIndex] = text;
            }

            /// <summary>
            /// Gets or sets the index of where this line begins.
            /// It is always the first character of the line and always the index
            /// of the first text column, which is what makes the different columns sync up.
            /// </summary>
            public int Index { get; set; }

            public int GetLength(int textColumnIndex)
            {
                var text = this.GetText(textColumnIndex);
                return text == null ? 0 : text.Length;
            }

            public void SetLength(int textColumnIndex, int value)
            {
                throw new NotSupportedException("Cannot set the length to a TextLine; the length is the string length.");
            }

            /// <summary>
            ///   TODO: This is not correct. But it is currently not used anyway.
            /// </summary>
            /// <param name = "index"></param>
            /// <param name = "length"></param>
            /// <param name = "startIncluded"></param>
            /// <param name = "endIncluded"></param>
            /// <returns></returns>
            public bool Contains(int index, int length, bool startIncluded, bool endIncluded)
            {
                return false;
            }

            public override string ToString()
            {
                return String.Join(" | ", this.Texts) + " (" + this.Texts[0].Length + ")";
            }

            public void ClearStyles(bool clearPinned)
            {
                if (this._styledTextSegments != null)
                {
                    var offset = 0;
                    while (this.StyledTextSegments.Count > offset)
                    {
                        if (clearPinned == false && this.StyledTextSegments[offset].Style.Type == TextStyleType.Pinned)
                        {
                            offset++;
                            continue;
                        }

                        this.StyledTextSegments[offset].TextLine = null;
                    }
                }
            }

            //private const string COLUMN_SPLIT = "■";

            public TextSegmentVisualInfos CalculateVisuals(ITextView textView, IntPtr hdc, int width, int lineHeight)
            {
                //var information = new TextSegmentVisualInfos();

                if (staticTabWidth == -1)
                {
                    unsafe
                    {
                        var preferredLength = textView.Settings.TabWidth; // Bridge.Get().Get<int>("Text.TabWidth");
                        var spaceString = new String(' ', preferredLength);

                        fixed (char* c = spaceString)
                        {
                            var spaceWidth = Size.Empty;
                            SafeNativeMethods.GetTextExtentPoint32(hdc, c, spaceString.Length, ref spaceWidth);

                            staticTabWidth = spaceWidth.Width;
                        }
                    }
                }

                var newInfos = new List<TextSegmentVisualInfo>(this.Texts.Length);

                for (var columnIndex = 0; columnIndex < this.Texts.Length; columnIndex++)
                {
                    var info = new TextSegmentVisualInfo();
                    newInfos.Add(info);

                    // TODO: This should calculate visual TextSegmentVisualInfos for all text columns!
                    //var columnIndex = 0;

                    info.LineSplitIndexes = null;
                    info.TabSplitIndexes = null;
                    info.TextColumnIndex = columnIndex;
                    var text = this.GetText(columnIndex);

                    if (text.Length == 0)
                    {
                        info.LineCountVisual = 1;
                        continue;
                    }

                    var oneLineSize = this.GetSize(hdc, 0, 0, text.Length, info);

                    if (columnIndex > 0)
                    {
                        // If this is not the first column, we do not use wordwrapping, 
                        // since it makes things very difficult to handle. Things would become highly unstable/random.
                        info.LineCountVisual = 1;
                        info.Size = oneLineSize;
                        continue;
                    }

                    var indexesTabs = new List<int>();
                    for (var i = 0; i < text.Length; i++)
                    {
                        if (text[i] == '\t')
                        {
                            indexesTabs.Add(i + 1);
                        }
                    }

                    if (indexesTabs.Count > 0)
                    {
                        info.TabSplitIndexes = indexesTabs.ToArray();
                    }

                    if (oneLineSize.Width < width)
                    {
                        // If all the text is on one line, it it smaller than the viewport. So no wordwrapping.
                        info.LineCountVisual = 1;
                        info.Size = oneLineSize;
                        continue;
                    }

                    // There is wordwrapping taking place here, so we need some more advanced checks.
                    var indexesNewlines = new List<int>();
                    var totalSize = new Size();
                    var fallbackIndex = -1;
                    var lastNewlineIndex = 0;

                    for (var i = 0; i <= text.Length; i++)
                    {
                        if (i != text.Length && Char.IsWhiteSpace(text[i]) == false)
                        {
                            continue;
                        }

                        var sizeSoFar = this.GetSize(hdc, 0, lastNewlineIndex, i - lastNewlineIndex, info);

                        if (sizeSoFar.Width >= width)
                        {
                            // If there is no fallback, then the current line only has one really long word.
                            // We will just not wordwrap it and let it go outside the text editor. Forced newline is just ugly.
                            if (fallbackIndex != -1)
                            {
                                indexesNewlines.Add(fallbackIndex);
                                lastNewlineIndex = fallbackIndex;
                            }
                        }
                        else
                        {
                            if (sizeSoFar.Width > totalSize.Width)
                            {
                                totalSize.Width = sizeSoFar.Width;
                            }
                        }

                        fallbackIndex = i;
                    }

                    if (indexesNewlines.Count > 0)
                    {
                        info.LineSplitIndexes = indexesNewlines.ToArray();

                        // +1 since the last splitting does not account for the last line.
                        info.LineCountVisual = info.LineSplitIndexes.Length + 1;
                    }
                    else
                    {
                        info.LineCountVisual = 1;
                    }

                    totalSize.Height = info.LineCountVisual*lineHeight;
                    info.Size = totalSize;
                }

                return new TextSegmentVisualInfos(newInfos.ToArray());
            }

            /// <summary>
            ///   TODO: This does not work, since it does not take the lineHeight into account.
            /// </summary>
            /// <param name = "hdc">The DC handle onto which the painting is being done.</param>
            /// <param name = "x">The starting horizontal location, used to offset tabbings correctly.</param>
            /// <param name = "start">The starting index of the segment as-to where to begin measuring.</param>
            /// <param name = "length">The number of characters starting from the <see cref="start"/> that should be measured.</param>
            /// <param name = "info">The TextSegmentVisualInfos about wordwrappings and tabbings.</param>
            /// <returns></returns>
            public unsafe Size GetSize(IntPtr hdc, int x, int start, int length, TextSegmentVisualInfo info)
            {
                var text = this.GetText(info.TextColumnIndex);

                fixed (char* c = text)
                {
                    var sz = new Size();

                    if (info.TabSplitIndexes != null)
                    {
                        var previousTabIndex = start;

                        foreach (var tabSplitIndex in info.TabSplitIndexes)
                        {
                            if (tabSplitIndex > start && tabSplitIndex <= start + length)
                            {
                                var subStart = previousTabIndex;
                                var subLength = tabSplitIndex - previousTabIndex - 1; // -1 since we are not counting this tab.

                                if (subLength > 0)
                                {
                                    var sizeUntilTab = Size.Empty;
                                    SafeNativeMethods.GetTextExtentPoint32(hdc, c + subStart, subLength, ref sizeUntilTab);
                                    sz.Width += sizeUntilTab.Width;
                                }

                                sz.Width += (staticTabWidth) - ((x + sz.Width)%staticTabWidth);

                                previousTabIndex = tabSplitIndex;
                            }
                        }

                        if (previousTabIndex < start + length)
                        {
                            var lastStart = previousTabIndex;
                            var lastLength = length - (previousTabIndex - start);

                            var sizeAfterTab = Size.Empty;
                            SafeNativeMethods.GetTextExtentPoint32(hdc, c + lastStart, lastLength, ref sizeAfterTab);
                            sz.Width += sizeAfterTab.Width;
                        }
                    }
                    else
                    {
                        SafeNativeMethods.GetTextExtentPoint32(hdc, c + start, length, ref sz);
                    }

                    return sz;
                }
            }
        }
    }
}