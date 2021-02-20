#region

using System;
using Eliason.TextEditor.TextStyles;

#endregion

namespace Eliason.TextEditor.TextDocument.ByLines
{
    public partial class TextDocumentByLines
    {
        private sealed class TextAnchor : IComparable<TextAnchor>, ITextSegmentStyled
        {
            private IComparable obj;
            private string[] texts;

            private TextLine textLine;

            public TextAnchor(TextStyleBase style)
            {
                this.Style = style;
            }

            public ITextSegmentVisual Parent
            {
                get { return this.TextLine; }
            }

            public TextLine TextLine
            {
                get { return this.textLine; }
                set
                {
                    if (this.textLine != null)
                    {
                        this.textLine.StyledTextSegments.Remove(this);
                    }

                    this.textLine = value;

                    if (this.textLine != null)
                    {
                        this.textLine.StyledTextSegments.Add(this);
                    }
                }
            }

            #region IComparable<TextAnchor> Members

            public int CompareTo(TextAnchor other)
            {
                var result = this.Index.CompareTo(other.Index);

                if (result == 0)
                {
                    result = this.Length.CompareTo(other.Length);
                }

                return result;
            }

            #endregion

            #region ITextSegmentStyled Members

            /// <summary>
            /// Gets the text that this text anchor should be displayed as if it is textually displayed.
            /// </summary>
            public string[] Texts
            {
                get
                {
                    if (this.Object == null)
                    {
                        return new[] {Strings.NotAvailable};
                    }

                    return this.texts ?? (this.texts = new[] {this.Style.GetStringRepresentation(this.Object)});
                }
            }

            public string GetText(int textColumnIndex)
            {
                return this.Texts.Length <= textColumnIndex ? "" : this.Texts[textColumnIndex];
            }

            /// <summary>
            /// Gets or sets the index this text anchor has in offset to the line, and not the document.
            /// </summary>
            public int Index { get; set; }

            /// <summary>
            /// Gets the index that text anchor has in offset to the document, and not the line.
            /// </summary>
            public int IndexGlobal
            {
                get { return this.Index + this.TextLine.Index; }
            }

            /// <summary>
            ///   Gets the length of this text anchor followed after the starting index.
            /// </summary>
            private int Length { get; set; }

            public int GetLength(int textColumnIndex)
            {
                return this.Length;
            }

            public void SetLength(int textColumnIndex, int value)
            {
                this.Length = value;
            }

            /// <summary>
            ///   Gets or sets the object that this text anchor holds.
            /// </summary>
            public IComparable Object
            {
                get { return this.obj; }

                set
                {
                    this.obj = value;
                    this.texts = null;
                }
            }

            /// <summary>
            ///   Gets or the style of this text anchor.
            /// </summary>
            public TextStyleBase Style { get; private set; }

            public bool CanExecute
            {
                get { return this.Style.CanExecute; }
            }

            public void ShowInfo()
            {
                this.Style.ShowInfo(this);
            }

            public bool Execute()
            {
                return this.Style.Execute(this);
            }

            /// <summary>
            ///   Returns a boolean true if the specified index+length is inside the this text anchor.
            /// </summary>
            /// <param name = "index"></param>
            /// <param name = "length"></param>
            /// <param name = "matchFirst"></param>
            /// <param name = "matchLast"></param>
            /// <returns></returns>
            public bool Contains(int index, int length, bool matchFirst, bool matchLast)
            {
                if (matchLast)
                {
                    if (this.Index + this.Length < index)
                    {
                        // The anchor ends before the index where the change begins.
                        return false;
                    }
                }
                else
                {
                    if (this.Index + this.Length <= index)
                    {
                        // The anchor ends before the index where the change begins.
                        return false;
                    }
                }

                if (matchFirst)
                {
                    if (this.Index > index + length)
                    {
                        // The anchor starts after the index where the change begins.
                        return false;
                    }
                }
                else
                {
                    if (this.Index >= index + length)
                    {
                        // The anchor starts after the index where the change begins.
                        return false;
                    }
                }

                return true;
            }

            #endregion

            /// <summary>
            ///   Returns a textual, informational representation of this text anchor.
            /// </summary>
            /// <returns>A System.String that tells information about the text anchor.</returns>
            public override string ToString()
            {
                var objectText = this.Object == null ? Strings.NotAvailable : this.Object.ToString();

                return this.Style.Name + ": " + this.Index + "+" + this.Length + " (" + objectText + ")";
            }
        }
    }
}