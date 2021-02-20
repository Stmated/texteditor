#region

using System;
using System.Collections.Generic;
using System.Linq;
using Eliason.TextEditor.TextStyles;

#endregion

namespace Eliason.TextEditor.TextDocument.ByLines
{
    public partial class TextDocumentByLines
    {
        private class LineTextSegmentStyledManager : ITextSegmentStyledManager
        {
            //private readonly List<TextStyleBase> _anchorStyles = new List<TextStyleBase>();
            //private readonly List<TextStyleBase> _customStyles = new List<TextStyleBase>();
            private List<TextLine> _lines;

            public LineTextSegmentStyledManager(ITextDocument textHandler)
            {
                //foreach (var anchorStyle in TextStyleBase.Styles)
                //{
                //    var newAnchorStyle = anchorStyle.Value.Clone();
                //    this._anchorStyles.Add(newAnchorStyle);
                //}

                this._lines = ((TextDocumentByLines)textHandler).Lines;
                this.TextHandler = textHandler;
            }


            private ITextDocument TextHandler { get; set; }

            public void Dispose()
            {
                //foreach (var textStyle in this._customStyles)
                //{
                //    textStyle.Dispose();
                //}

                //this._anchorStyles.Clear();

                this._lines = null;
            }

            //public void AddStyle(TextStyleBase anchorStyle)
            //{
            //    this._customStyles.Add(anchorStyle);
            //}

            public void Clear(bool clearPinned)
            {
                for (var i = 0; i < this._lines.Count; i++)
                {
                    this._lines[i].ClearStyles(clearPinned);
                }
            }

            public void AddManualTextSegment(ITextSegmentStyled segmentStyled, int index, int textColumnIndex)
            {
                var textAnchor = segmentStyled as TextAnchor;

                if (textAnchor.TextLine != null)
                {
                    textAnchor.TextLine = null;
                }

                var lineIndex = this.TextHandler.GetLineFromCharIndex(index, textColumnIndex);
                textAnchor.TextLine = this._lines[lineIndex];
            }

            public void RemoveTextSegment(ITextSegmentStyled textSegment)
            {
                var textAnchor = (textSegment as TextAnchor);
                textAnchor.TextLine = null;
            }

            public IEnumerable<ITextSegmentStyled> Get(int index, int textColumnIndex)
            {
                return this.Get(index, null, textColumnIndex);
            }

            public IEnumerable<ITextSegmentStyled> Get(int index, string ofType, int textColumnIndex)
            {
                var lineIndex = this.TextHandler.GetLineFromCharIndex(index, textColumnIndex);
                if (lineIndex != -1)
                {
                    var relativeIndex = index - this.TextHandler.GetFirstCharIndexFromLine(lineIndex);

                    foreach (var styledSegment in GetAll(this._lines[lineIndex], relativeIndex, ofType))
                    {
                        yield return styledSegment;
                    }
                }
            }

            private static IEnumerable<ITextSegmentStyled> GetAll(TextLine textLine, int relativeIndex, string ofType)
            {
                for (var n = 0; n < textLine.StyledTextSegments.Count; n++)
                {
                    var anchor = textLine.StyledTextSegments[n];

                    if (ofType != null && anchor.Style.NameKey != ofType)
                    {
                        continue;
                    }

                    if (anchor.Contains(relativeIndex, 1, true, true))
                    {
                        yield return anchor;
                    }
                }
            }

            public ITextSegmentStyled GetClosest(int index, string ofType)
            {
                ITextSegmentStyled closestMatch = null;
                var foundLineStartIndex = -1;

                foreach (var textSegmentStyled in this.GetStyledTextSegments())
                {
                    if ((textSegmentStyled is TextAnchor) == false)
                    {
                        // This styled text segment is not an anchor, so we of course do not use it.
                        continue;
                    }

                    var anchor = (TextAnchor)textSegmentStyled;
                    if (ofType != null && anchor.Style.NameKey != ofType)
                    {
                        continue;
                    }

                    if (anchor.IndexGlobal > index)
                    {
                        if (foundLineStartIndex != -1 && foundLineStartIndex != anchor.TextLine.Index)
                        {
                            break;
                        }

                        continue;
                    }

                    if (closestMatch == null || anchor.IndexGlobal > closestMatch.IndexGlobal)
                    {
                        foundLineStartIndex = anchor.TextLine.Index;
                        closestMatch = anchor;
                    }
                }

                return closestMatch;
            }

            public IEnumerable<ITextSegmentStyled> GetStyledTextSegments()
            {
                return this._lines.SelectMany(line => line.StyledTextSegments);
            }

            public IEnumerable<ITextSegmentStyled> GetStyledTextSegments(string typeKey)
            {
                return this.GetStyledTextSegments().Where(styledTextSegment => styledTextSegment.Style.NameKey == typeKey);
            }

            #region Search & Apply

            public bool SearchAndApplyTo(ITextView textView, int lineIndex, int textColumnIndex)
            {
                if (lineIndex < 0 || lineIndex >= this._lines.Count)
                {
                    return false;
                }

                return this.SearchAndApplyTo(textView, this._lines[lineIndex], 0, this._lines[lineIndex].GetLength(textColumnIndex), textColumnIndex);
            }

            public bool SearchAndApplyTo(ITextView textView, ITextSegment textSegment, int index, int length, int textColumnIndex)
            {
                return this.SearchAndApplyTo(textView, textSegment, index, length, true, textColumnIndex);
            }

            /// <summary>
            /// TODO: Make this threaded and make the text control not editable meanwhile
            /// </summary>
            public bool SearchAndApplyTo(ITextView textView, ITextSegment textSegment, int index, int length, bool changeWasFinalizer, int textColumnIndex)
            {
                var foundOne = false;
                var canSkip = false;
                var previousWasWhitespace = false;

                var textLine = textSegment as TextLine;

                var textStyles = new List<TextStyleBase>(textView.GetTextStyles());
                string previousStyleType = null;
                var previousStyleIndex = -1;

                if (textLine == null)
                {
                    return false;
                }

                for (var i = index; i <= index + length && i < textSegment.GetLength(textColumnIndex); i++)
                {
                    #region If whitespace found, enable word-jump-search and only search on first char per word

                    var isWhitespace = i < index + length && Char.IsWhiteSpace(textSegment.GetText(textColumnIndex)[i]);

                    if (canSkip)
                    {
                        if (isWhitespace == false)
                        {
                            if (previousWasWhitespace == false)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            previousWasWhitespace = true;
                            continue; // Don't need to search on a whitespace.
                        }
                    }
                    else if (isWhitespace)
                    {
                        canSkip = true;
                        previousWasWhitespace = true;
                        continue; // Don't need to search on a whitespace.
                    }

                    previousWasWhitespace = false;

                    #endregion

                    ITextSegmentStyled newStyledTextSegment = null;

                    for (var s = 0; s < textStyles.Count; s++)
                    {
                        if (textStyles[s].UpdateOnlyOnFinalizingChange && changeWasFinalizer == false)
                        {
                            continue;
                        }

                        newStyledTextSegment = textStyles[s].FindStyledTextSegment(textView, textSegment, this.TextHandler, i, -1, textColumnIndex);

                        if (newStyledTextSegment != null)
                        {
                            //textStyles.RemoveAt(s);
                            break;
                        }
                    }

                    if (newStyledTextSegment == null)
                    {
                        continue;
                    }

                    var found = false;

                    foreach (var existingStyle in GetAll(textLine, i, newStyledTextSegment.Style.NameKey))
                    {
                        if (existingStyle.Style.NameKey == newStyledTextSegment.Style.NameKey)
                        {
                            if (newStyledTextSegment.Index == existingStyle.Index && newStyledTextSegment.GetLength(textColumnIndex) == existingStyle.GetLength(textColumnIndex))
                            {
                                found = true;
                                break;
                            }
                            this.RemoveTextSegment(existingStyle);
                            break;
                        }
                    }

                    if (found == false)
                    {
                        foundOne = true;

                        // TODO: Make this line actually work. (NOTE: what did I mean here? ;D)
                        this.AddManualTextSegment(newStyledTextSegment, textLine.Index + newStyledTextSegment.Index, textColumnIndex);
                    }

                    if (previousStyleType != newStyledTextSegment.Style.NameKey && previousStyleIndex != newStyledTextSegment.Index)
                    {
                        // Decrease it so that we stay on the same location, in case there are several styles overlapping on the same spot.
                        i--;
                    }

                    previousStyleType = newStyledTextSegment.Style.NameKey;
                    previousStyleIndex = newStyledTextSegment.Index;
                }

                return foundOne;
            }

            #endregion
        }
    }
}