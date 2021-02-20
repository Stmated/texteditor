using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eliason.TextEditor.TextStyles;
using Eliason.TextEditor.TextTemplates;

namespace Eliason.TextEditor
{
    public interface ISettings
    {
        // private readonly List<string> ignoredWords = new List<string>();

        INotifier Notifier { get; }

        TextStyleDisplayMode GetDisplayMode(TextStyleBase textStyle);

        TextStyleBase GetTextStyle(String key);

        IEnumerable<TextStyleBase> GetTextStyles();

        TokenTypeBase GetTemplateTokenType(String key);

        IEnumerable<TokenTypeBase> GetTemplateTokenTypes();

        TokenAttributeTypeBase GetTemplateTokenAttributeType(String key);

        IEnumerable<TokenAttributeTypeBase> GetTemplateTokenAttributeTypes();

        Color LineHighlightColor { get; }

        /// <summary>
        /// Color.Green
        /// </summary>
        Color ColorHighlightFore { get; }

        /// <summary>
        /// Color.Transparent
        /// </summary>
        Color ColorHighlightBack { get; }

        /// <summary>
        /// Color.Transparent
        /// </summary>
        Color ColorSpellcheckFore { get; }

        /// <summary>
        /// Color.Transparent
        /// </summary>
        Color ColorSpellcheckBack { get; }

        /// <summary>
        /// Color.Red
        /// </summary>
        Color ColorSpellcheckUnderline { get; }

        /// <summary>
        /// PenType.Dot
        /// </summary>
        PenType SpellcheckUnderlineType { get; }

        /// <summary>
        /// true
        /// </summary>
        bool SpellcheckUnderlineEnabled { get; }

        /// <summary>
        /// true, depending on language, ofc
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        bool SpellcheckEnabled(CultureInfo cultureInfo);

        /// <summary>
        /// True if spellchecking should be done inlined in the text control and updated while typing.
        /// </summary>
        bool InlineEnabled { get; }

        /// <summary>
        /// True if spellchecking should not be done on a word written in all capital letters.
        /// </summary>
        bool IgnoreAllCaps { get; }

        /// <summary>
        /// True if spellchecking should not be done on a word written containing numbers.
        /// </summary>
        bool IgnoreWithNumbers { get; }

        /// <summary>
        /// True if spellchecking should not be done on a word written containing asian characters.
        /// </summary>
        bool IgnoreWithAsian { get; }

        String IgnoreLinesWithPrefix { get; }

        IEnumerable<string> IgnoredWords { get; }

        int TabWidth{ get; }

        /// <summary>
        /// Gets the directory path to the location where the autosaved files should be temporarily placed.
        /// </summary>
        //string AutoSaveDirectoryPath { get; }

        int AutoSaveInterval { get; }

        bool AutoSaveEnabled { get; }
        IEnumerable<String> ResourceDirectoryPaths { get; }

        /// <summary>
        /// This should give "info.png", wherever it might be
        /// </summary>
        Bitmap BitmapInfo { get; }

        /// <summary>
        /// This should give "linenumbers.png" wherever it might be
        /// </summary>
        Bitmap BitmapLineNumbers { get; }

        void AddIgnoreWord(string word);

        bool CheckIfWordIsValid(string word, string line);
        //{
        //    if (String.IsNullOrEmpty(line) == false)
        //    {
        //        for (var i = 0; i < LinesWithPrefix.Length; i++)
        //        {
        //            if (line[0] == LinesWithPrefix[i])
        //            {
        //                return false;
        //            }
        //        }
        //    }

        //    var containsIllegal = false;
        //    var containsLowercase = false;
        //    var containsRegularChar = false;
        //    for (var i = 0; i < word.Length; i++)
        //    {
        //        var c = word[i];

        //        if (Char.IsLetterOrDigit(c))
        //        {
        //            containsRegularChar = true;
        //        }

        //        if (Char.IsLower(c))
        //        {
        //            containsLowercase = true;
        //        }

        //        if (IgnoreWithNumbers && Char.IsNumber(c))
        //        {
        //            containsIllegal = true;
        //            break;
        //        }

        //        if (IgnoreWithAsian)
        //        {
        //            if ((('\u4e00' <= c) && (c <= '\u9fa5'))
        //                || (('\u3005' <= c) && (c <= '\u3007'))
        //                || (('\u3041' <= c) && (c <= '\u309e'))
        //                || (('\uff66' <= c) && (c <= '\uff9d'))
        //                || (('\u30a1' <= c) && (c <= '\u30fe')))
        //            {
        //                containsIllegal = true;
        //                break;
        //            }
        //        }
        //    }

        //    if (containsRegularChar == false || containsIllegal)
        //    {
        //        return false;
        //    }

        //    if (IgnoreAllCaps && containsLowercase == false)
        //    {
        //        return false;
        //    }

        //    foreach (var ignoredWord in IgnoredWords)
        //    {
        //        if (ignoredWord.Equals(word, StringComparison.CurrentCultureIgnoreCase))
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}
        bool IsSpelledCorrectly(String word);
        bool IsTextColumnEnabled(string key);
        void SetTextColumnEnabled(string key, bool value);
    }
}
