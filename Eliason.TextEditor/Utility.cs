using System;
using System.Globalization;

namespace Eliason.TextEditor
{
    public static class Utility
    {
        public static UnicodeCategory GetSimplifiedUnicodeCategory(this char c)
        {
            if (c == '\'')
            {
                return UnicodeCategory.LowercaseLetter;
            }

            var category = Char.GetUnicodeCategory(c);

            switch (category)
            {
                // These two are character that usually connect other characters, such as "-" and other weird symbols in other languages.
                // For our needs we will count these character as lowercase letters, to make it simpler for code looking for words.
                case UnicodeCategory.ConnectorPunctuation:
                case UnicodeCategory.DashPunctuation:
                {
                    //return UnicodeCategory.OtherPunctuation;
                    return UnicodeCategory.LowercaseLetter;
                }

                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.OtherNumber:
                case UnicodeCategory.DecimalDigitNumber:
                {
                    return UnicodeCategory.LowercaseLetter;
                }

                default:
                {
                    return category;
                }
            }
        }

        /// <summary>
        /// TODO: Has language-specific code. Should take an ILanguage that is called for specific functions.
        /// </summary>
        /// <param name="globalIndex"></param>
        /// <param name="listStartIndex"></param>
        /// <param name="lineText"></param>
        /// <param name="strict"></param>
        /// <returns></returns>
        public static WordSegment GetWord(this string lineText, int globalIndex, int listStartIndex, bool strict)
        {
            var relativeIndex = globalIndex - listStartIndex;

            var pre = Math.Min(lineText.Length - 1, relativeIndex);
            var post = Math.Min(relativeIndex, lineText.Length - 1);

            // We get it from pre here since we might be double-clicking on the end of a line and then we'd get an IndexOutOfRange
            // exception since the end of lines do not contain the linebreak character.
            var originCategory = lineText[pre].GetSimplifiedUnicodeCategory();

            if (pre > 0 && post > 0 && originCategory == UnicodeCategory.SpaceSeparator)
            {
                var previousCharCategory = lineText[pre - 1].GetSimplifiedUnicodeCategory();
                if (previousCharCategory != UnicodeCategory.SpaceSeparator)
                {
                    originCategory = previousCharCategory;
                    pre--;
                    post--;
                }
            }

            pre = GetWordIndexOfChange(lineText, pre, strict, originCategory, true) + 1;
            post = GetWordIndexOfChange(lineText, post, strict, originCategory, false);

            if (strict)
            {
                pre = GetWordStripNonCharacter(lineText, pre, false);
                post = GetWordStripNonCharacter(lineText, post, true) + 1;
            }

            if (pre >= post)
            {
                return null;
            }

            var segment = new WordSegment
            {
                Start = globalIndex - (relativeIndex - pre)
            };

            segment.End = segment.Start + (post - pre);

            if (strict)
            {
                if ((segment.End - segment.Start) > 1 && lineText[segment.Start - listStartIndex] == '\'')
                {
                    if (lineText[segment.End - listStartIndex - 1] == '\'')
                    {
                        segment.Start++;
                        segment.End--;
                    }
                }
                else
                {
                    if (segment.End - segment.Start > 2)
                    {
                        // TODO: This code is too language-dependant and may not apply for other languages. Needs to be abstracted.

                        var last2Chars = lineText.Substring(segment.End - listStartIndex - 2, 2);

                        if (last2Chars.Equals("'s", StringComparison.CurrentCultureIgnoreCase))
                        {
                            segment.End -= 2;
                        }
                    }
                }
            }

            segment.Word = lineText.Substring(segment.Start - listStartIndex, segment.End - segment.Start);

            return segment;
        }

        private static int GetWordIndexOfChange(string lineText, int idx, bool strict, UnicodeCategory originCategory, bool left)
        {
            var end = left ? -1 : lineText.Length - 1;
            var inc = left ? -1 : 1;

            for (; idx != end; idx += inc)
            {
                var cat = lineText[idx].GetSimplifiedUnicodeCategory();

                if (strict)
                {
                    if (cat != originCategory)
                    {
                        break;
                    }
                }
                else
                {
                    if (Char.IsWhiteSpace(lineText[idx]) || cat == UnicodeCategory.ClosePunctuation || cat == UnicodeCategory.OpenPunctuation)
                    {
                        break;
                    }
                }
            }

            return idx;
        }

        private static int GetWordStripNonCharacter(string lineText, int idx, bool left)
        {
            var end = left ? 0 : lineText.Length - 1;
            var inc = left ? -1 : 1;

            for (;; idx += inc)
            {
                if (Char.IsLetter(lineText[idx]))
                {
                    break;
                }

                if (idx == end)
                {
                    break;
                }
            }

            return idx;
        }
    }
}