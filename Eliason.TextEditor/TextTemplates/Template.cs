using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Eliason.TextEditor.Extensions;
using Eliason.TextEditor.TextStyles;
using Eliason.TextEditor.TextTemplates.TokenAttributes;

namespace Eliason.TextEditor.TextTemplates
{
    public class Template
    {
        public string Name { get; private set; }
        public string ContentRaw { get; private set; }
        public Keys DefaultHotkey { get; private set; }

        public Template(string name, string content)
        {
            this.DefaultHotkey = Keys.None;

            if (content.StartsWith("$$"))
            {
                var hotkeyEndIndex = content.IndexOf("$$", 2, StringComparison.InvariantCultureIgnoreCase);
                var hotkey = content.Substring(2, hotkeyEndIndex - 2);
                this.DefaultHotkey = hotkey.StringToKeys();
                content = content.Substring(hotkeyEndIndex + 2);
            }

            this.Name = name;
            this.ContentRaw = content;
        }

        //private IProject GetProjectFromTextView(ITextView textView)
        //{
        //    IProject project = Bridge.Get<IBProjects>().ProjectGlobal;

        //    if (String.IsNullOrEmpty(textView.CurrentFilePath))
        //    {
        //        if (textView is IProjectTarget)
        //        {
        //            project = (textView as IProjectTarget).GetProject();
        //        }
        //    }
        //    else
        //    {
        //        project = Bridge.Get().GetSystemNode(textView.CurrentFilePath).GetProject(true);
        //    }

        //    return project;
        //}

        public void ProcessInline(ITextView textView, int textColumnIndex)
        {
            if (String.IsNullOrEmpty(textView.CurrentFilePath))
            {
                textView.Settings.Notifier.Info(Strings.TextControl_TextTemplates_Title, Strings.TextControl_TextTemples_TextFileMustBeSaved);
                return;
            }

            var tokens = new List<Token>(this.GetTokens(textView));

            if (textView.GetTextStyle("TemplateToken") == null)
            {
                // Add the text style if it does not already exist.
                textView.AddStyle(
                    new TextStyleManual(
                        "Template Token",
                        "TemplateToken",
                        "The default text style for the text",
                        Color.Empty, Color.LawnGreen,
                        null,
                        TextStyleDisplayMode.Always,
                        TextStylePaintMode.Inline));
            }

            var lastSelectedIdx = textView.SelectionStart;
            var start = lastSelectedIdx;
            textView.SelectionLength = 0;
            const string tokenPlaceholder = "[?]";

            foreach (var t in tokens)
            {
                if (t.Type.IsFreetext)
                {
                    var processResult = t.Process();
                    textView.TextInsert(textView.SelectionStart, processResult);
                    textView.SelectionStart += processResult.Length;
                    lastSelectedIdx += processResult.Length;
                }
                else
                {
                    var placeholderStart = textView.SelectionStart;
                    textView.TextInsert(textView.SelectionStart, tokenPlaceholder);
                    textView.SelectionStart += tokenPlaceholder.Length;

                    var segment = textView.TextDocument.CreateStyledTextSegment(textView.GetTextStyle("TemplateToken"));

                    segment.Index = placeholderStart - textView.GetFirstCharIndexFromLine(textView.GetLineFromCharIndex(placeholderStart));
                    segment.SetLength(textColumnIndex, tokenPlaceholder.Length);
                    segment.Object = t;

                    // Add the style to the text.
                    textView.TextDocument.TextSegmentStyledManager.AddManualTextSegment(segment, placeholderStart, textColumnIndex);
                }
            }

            while (true)
            {
                var foundTemplateTokens = 0;
                foreach (var style in textView.TextDocument.TextSegmentStyledManager.GetStyledTextSegments("TemplateToken"))
                {
                    foundTemplateTokens++;

                    var t = style.Object as Token;

                    var idx = style.IndexGlobal;
                    textView.SelectionStart = idx;
                    textView.SelectionLength = style.GetLength(textColumnIndex);

                    var tokenResult = t.Process();

                    textView.SelectionLength = 0;

                    textView.TextRemove(idx, style.GetLength(textColumnIndex));
                    textView.TextInsert(idx, tokenResult);

                    // Fake the key as finalizing, since we are swapping out the content dynamically
                    // and hence never actually sending the "\n" character which would recheck for text segments.
                    textView.TextDocument.FakeFinalizingKey(idx, textColumnIndex);
                    lastSelectedIdx += tokenResult.Length;

                    // Redo it all from the beginning, since otherwise we will get a "enumeration was modified" exception.
                    break;
                }

                if (foundTemplateTokens == 0)
                {
                    break;
                }
            }

            textView.SelectionStart = lastSelectedIdx;

            // Invalidate the painting of the textview.
            textView.Invalidate();
        }

        private const string PLACEHOLDER_PATTERN = @"\[{(.*?)}\s*(.*?)\]";

        private IEnumerable<Token> GetTokens(ITextView textView)
        {
            var matches = Regex.Matches(this.ContentRaw, PLACEHOLDER_PATTERN, RegexOptions.IgnoreCase);
            var nextMatchIdx = 0;
            var tokenFreetext = new TokenTypes.TokenTypeFreetext();
            var previousIdx = 0;

            for (var i = 0; i < this.ContentRaw.Length; i++)
            {
                if (nextMatchIdx == matches.Count)
                {
                    var t = new Token(textView, tokenFreetext, null);
                    t.Attributes = new[] {new TokenAttribute(t, new AttribTypeContent(), this.ContentRaw.Substring(i))};

                    yield return t;
                    break;
                }

                var nextMatch = matches[nextMatchIdx];

                if (nextMatch.Index != i)
                {
                    continue;
                }

                var freeText = this.ContentRaw.Substring(previousIdx, i - previousIdx);
                var freetextToken = new Token(textView, tokenFreetext, null);
                freetextToken.Attributes = new[] {new TokenAttribute(freetextToken, new AttribTypeContent(), freeText)};
                yield return freetextToken;

                var tokenTypeKey = nextMatch.Groups[1].Value;
                var attributeString = nextMatch.Groups[2].Value;
                var attributes = new List<TokenAttribute>();
                TokenTypeBase foundTokenType = null;

                foreach (var ttt in textView.Settings.GetTemplateTokenTypes().Where(ttt => ttt.Key == tokenTypeKey && !ttt.IsDynamic))
                {
                    foundTokenType = ttt;
                }

                if (foundTokenType == null)
                {
                    #region Get dynamic variable

                    var dynamicTokens = textView.Settings.GetTemplateTokenTypes().Where(ttt => ttt.IsDynamic).ToList();

                    if (dynamicTokens.Count > 1)
                    {
                        // TODO: Something should be done here to ask the user which method (s)he wants to use.
                        //       But this should only happen if a user has added another through plugins, so no rush to implement.
                    }

                    if (dynamicTokens.Count == 0)
                    {
                        textView.Settings.Notifier.Error(Strings.TextControl_Title, Strings.Text_Template_DynamicTokenNotFound);
                    }
                    else
                    {
                        foundTokenType = dynamicTokens[0];
                    }

                    #endregion
                }

                var token = new Token(textView, foundTokenType, tokenTypeKey);

                foreach (var attributeKeyValue in attributeString.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries))
                {
                    var keyValue = attributeKeyValue.Split('=');
                    string key;
                    string value;

                    switch (keyValue.Length)
                    {
                        case 2:
                            key = keyValue[0];
                            value = keyValue[1];
                            break;
                        case 1:
                            key = attributeKeyValue[0].ToString();
                            value = attributeKeyValue.Substring(1);
                            break;
                        default:
                            throw new FormatException(String.Format("The KeyValue string '{0}' does not follow the set standard.", attributeKeyValue));
                    }

                    var added = false;
                    foreach (var tokenAttributeType in textView.Settings.GetTemplateTokenAttributeTypes())
                    {
                        if (tokenAttributeType.Key == key)
                        {
                            added = true;
                            attributes.Add(new TokenAttribute(token, tokenAttributeType, value));
                            break;
                        }
                    }

                    if (added == false)
                    {
                        throw new FormatException(String.Format("The text template token attribute for '{0}' does not exist.", key));
                    }
                }

                token.Attributes = attributes.ToArray();
                yield return token;

                i += nextMatch.Length;
                nextMatchIdx++;
                previousIdx = i;
            }
        }
    }
}