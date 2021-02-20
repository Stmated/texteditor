#region

using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Eliason.TextEditor.TextStyles;

#endregion

namespace Eliason.TextEditor.Spellcheck
{
    public abstract partial class SpellcheckFormBase : Form
    {
        private readonly ITextView textView;
        private readonly CultureInfo language;
        private int selectionLength;
        private int selectionStart = -1;

        protected SpellcheckFormBase(ITextView textView, CultureInfo cultureInfo)
        {
            this.textView = textView;

            this.language = cultureInfo;

            this.textView.SelectionChanged += this.SpellcheckForm_SelectionChanged;

            this.InitializeComponent();
        }

        protected abstract void ShowGetAddtionalSpellcheckResources();

        protected override void OnClosing(CancelEventArgs e)
        {
            this.textView.SelectionChanged -= this.SpellcheckForm_SelectionChanged;

            base.OnClosing(e);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (this.textView.TextLength == 0)
            {
                Bridge.Get().Error(Strings.Spellchecking_TextControlIsEmpty, Strings.Spellchecking_Title);
                return;
            }

            if (this.language == null)
            {
                Bridge.Get().Error(@"There was no language for the cultureinfo, so cannot spellcheck for it.", @"Internal error");
                Close();
                return;
            }

            if (LanguageFeature.SupportsFeature(this.language, "Spellcheck.Check") == false && LanguageFeature.SupportsFeature(this.language, "Synonym") == false)
            {
                var question = String.Format(Strings.Spellchecking_LexiconsAreNotInstalled_AskDownload, this.language.Name);

                if (Bridge.Get<IBDialogs>().AskYesNo(question, Strings.Spellchecking_Title))
                {
                    this.ShowGetAddtionalSpellcheckResources();

                    if (Bridge.Get<IBDialogs>().AskYesNo(Strings.Languages_Download_RestartRequired, Strings.Spellchecking_Title))
                    {
                        Application.Restart();
                    }
                }

                Close();
                return;
            }

            this.selectionStart = this.textView.SelectionStart;

            this.cbxEnableInlineSpellcheck.Checked = SpellcheckSettings.InlineEnabled;
            this.cbxIgnoreAllCaps.Checked = SpellcheckSettings.IgnoreAllCaps;
            this.cbxIgnoreWordsWithNumbers.Checked = SpellcheckSettings.IgnoreWithNumbers;
            this.cbxIgnoreWordsWithAsianChars.Checked = SpellcheckSettings.IgnoreWithAsian;
            this.tbxLinesWithPrefix.Text = SpellcheckSettings.LinesWithPrefix;

            this.GotoNext();
        }

        private void SpellcheckForm_SelectionChanged(object sender, EventArgs e)
        {
            if (Tag == null)
            {
                Tag = new object();

                if (this.selectionStart != -1)
                {
                    this.textView.SelectionStart = this.selectionStart;
                    this.textView.SelectionLength = this.selectionLength;

                    this.textView.ScrollToCaret();
                }

                Tag = null;
            }
        }

        private bool GotoNext()
        {
            if (LanguageFeature.SupportsFeature(this.textView.Language, "Spellcheck.Check") == false)
            {
                return false;
            }

            var searchFrom = Math.Max(0, this.selectionStart);

            var regex = new Regex(InvariantStrings.REGEX_WORD_BARRIER_STRICT + "([\\w'-]+?)" + InvariantStrings.REGEX_WORD_BARRIER_STRICT);

            var text = this.textView.TextGet();

            while (true)
            {
                var match = regex.Match(text, searchFrom);

                if (match.Success == false)
                {
                    break;
                }

                var value = match.Groups[2];

                // Check for unwanted match characters, and abort if so is instructed.
                if (SpellcheckSettings.CheckIfWordIsValid(value.Value, this.textView.GetLineText(this.textView.GetLineFromCharIndex(searchFrom))) == false)
                {
                    searchFrom += value.Length;
                    continue;
                }

                var spellcheckValid = false;
                foreach (var check in LanguageFeature.FeatureFetchMultiple<bool>(this.textView.Language, "Spellcheck.Check", new LFSString(value.Value)))
                {
                    spellcheckValid = check;
                    if (spellcheckValid)
                    {
                        break;
                    }
                }

                if (spellcheckValid == false)
                {
                    this.lbxSuggestions.Items.Clear();

                    Tag = new object();

                    this.textView.SelectionStart = this.selectionStart = value.Index;
                    this.textView.SelectionLength = this.selectionLength = value.Length;
                    this.textView.ScrollToCaret();

                    Tag = null;

                    foreach (string suggestion in LanguageFeature.FeatureFetchMultiple<string>(this.textView.Language, "Spellcheck.Suggest", new LFSString(value.Value)))
                    {
                        this.lbxSuggestions.Items.Add(suggestion);
                    }

                    if (this.lbxSuggestions.Items.Count == 0)
                    {
                        this.tbxReplaceWith.Clear();
                        this.tbxReplaceWith.Focus();
                    }
                    else
                    {
                        this.lbxSuggestions.SelectedIndex = 0;
                    }

                    return true;
                }

                searchFrom += value.Length;
            }

            Bridge.Get().Info(Strings.Spellchecking_EndOfText_Success, Strings.Spellchecking_Title);
            return false;
        }

        private void btnIgnore_Click(object sender, EventArgs e)
        {
            if (LanguageFeature.SupportsFeature(this.textView.Language, "Spellcheck.Check") == false)
            {
                return;
            }

            this.selectionStart = this.textView.SelectionStart + this.textView.SelectionLength;
            this.GotoNext();
        }

        private void btnIgnoreWord_Click(object sender, EventArgs e)
        {
            if (LanguageFeature.SupportsFeature(this.textView.Language, "Spellcheck.Check") == false)
            {
                return;
            }

            foreach (var ignoredWord in SpellcheckSettings.IgnoredWords)
            {
                if (ignoredWord.Equals(this.textView.SelectedText, StringComparison.CurrentCultureIgnoreCase) == false)
                {
                    SpellcheckSettings.AddIgnoreWord(this.textView.SelectedText);
                }
            }

            this.selectionStart = this.textView.SelectionStart + this.textView.SelectionLength;
            this.GotoNext();
        }

        private void btnReplace_Click(object sender, EventArgs e)
        {
            if (LanguageFeature.SupportsFeature(this.textView.Language, "Spellcheck.Check") == false)
            {
                return;
            }

            var diff = this.tbxReplaceWith.Text.Length - this.textView.SelectionLength;

            Tag = new object();
            this.textView.SelectedText = this.tbxReplaceWith.Text;
            Tag = null;

            this.selectionStart += diff;

            this.GotoNext();
        }

        private void btnReplaceAll_Click(object sender, EventArgs e)
        {
            if (LanguageFeature.SupportsFeature(this.textView.Language, "Spellcheck.Check") == false)
            {
                return;
            }

            Tag = new object();

            var replacement = this.tbxReplaceWith.Text;

            do
            {
                var diff = replacement.Length - this.textView.SelectionLength;

                this.textView.SelectedText = replacement;

                this.selectionStart += diff;
            } while (this.GotoNext());

            Tag = null;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cbxEnableInlineSpellcheck_CheckedChanged(object sender, EventArgs e)
        {
            Bridge.Get().Set("Text.Style.Spellcheck.DisplayMode", this.cbxEnableInlineSpellcheck.Checked ? TextStyleDisplayMode.Always : TextStyleDisplayMode.Hidden);
            SpellcheckSettings.InlineEnabled = this.cbxEnableInlineSpellcheck.Checked;
        }

        private void cbxIgnoreAllCaps_CheckedChanged(object sender, EventArgs e)
        {
            Bridge.Get().Set("Text.Spellcheck.Ignore.AllCaps", this.cbxIgnoreAllCaps.Checked);
            SpellcheckSettings.IgnoreAllCaps = this.cbxIgnoreAllCaps.Checked;
        }

        private void cbxIgnoreWordsWithNumbers_CheckedChanged(object sender, EventArgs e)
        {
            Bridge.Get().Set("Text.Spellcheck.Ignore.WithNumbers", this.cbxIgnoreWordsWithNumbers.Checked);
            SpellcheckSettings.IgnoreWithNumbers = this.cbxIgnoreWordsWithNumbers.Checked;
        }

        private void cbxIgnoreWordsWithAsianChars_CheckedChanged(object sender, EventArgs e)
        {
            Bridge.Get().Set("Text.Spellcheck.Ignore.WithAsian", this.cbxIgnoreWordsWithAsianChars.Checked);
            SpellcheckSettings.IgnoreWithAsian = this.cbxIgnoreWordsWithAsianChars.Checked;
        }

        private void tbxLinesWithPrefix_TextChanged(object sender, EventArgs e)
        {
            Bridge.Get().Set("Text.Spellcheck.Ignore.LinesWithPrefix", this.tbxLinesWithPrefix.Text);
            SpellcheckSettings.LinesWithPrefix = this.tbxLinesWithPrefix.Text;
        }

        private void lbxSuggestions_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.tbxReplaceWith.Text = this.lbxSuggestions.SelectedItem.ToString();
        }

        private void lbxSuggestions_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                {
                    this.btnReplace_Click(sender, EventArgs.Empty);
                }
                    break;
            }
        }

        private void lbxSuggestions_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.btnReplace_Click(sender, EventArgs.Empty);
        }

        private void btnAddWord_Click(object sender, EventArgs e)
        {
            if (LanguageFeature.SupportsFeature(this.textView.Language, "Spellcheck.AddWord"))
            {
                if (String.IsNullOrEmpty(this.textView.SelectedText))
                {
                    Bridge.Get().Error(Strings.Spellchecking_CannotAddEmptyWord, Strings.Spellchecking_Title);
                    return;
                }

                var state = new LFSValueAsync<string>(this.textView.SelectedText);
                state.Done += state_Done;
                LanguageFeature.FeaturePerform(this.textView.Language, "Spellcheck.AddWord", state);
            }
        }

        private void state_Done(object sender, LFSDoneArgs e)
        {
            if (e.Success)
            {
                Bridge.Get().Info(String.Format(AppResources.Spellchecking_WordAddition_Successful, this.textView.SelectedText), Strings.Spellchecking_Title);

                this.selectionStart += this.textView.SelectionLength;

                this.GotoNext();
            }
            else
            {
                Bridge.Get().Error(String.Format(AppResources.Spellchecking_WordAddition_Unsuccessful, this.textView.SelectedText), Strings.Spellchecking_Title);
            }
        }

        private void btnDownloadOO_Click(object sender, EventArgs e)
        {
            this.ShowGetAddtionalSpellcheckResources();
        }
    }
}