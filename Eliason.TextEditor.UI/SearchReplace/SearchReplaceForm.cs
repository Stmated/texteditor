#region

using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

#endregion

namespace Eliason.TextEditor.SearchReplace
{
    public partial class SearchReplaceForm : Form
    {
        // Modes: 0 = Cleartext, 1 = Wildcard, 2 = Regex
        // In: 0 = current, 1 = current selected, 2 = all open

        private static bool staticTextChangedAutomatically;

        private bool controlledAltering;

        public TextBoxBase TextBoxSearch
        {
            get { return this.tbxSearch; }
        }

        public SearchReplaceForm()
        {
            this.InitializeComponent();
        }

        public SearchReplaceForm(bool replaceMode) : this()
        {
            this.cbxReplace.Checked = !replaceMode;
            this.cbxReplace.Checked = replaceMode;

            // Load preferences of the last-used stuff for the search & replace form.
            this.cbxReplace.Checked = Bridge.Get().Get<bool>("Text.SearchReplace.Replace");
            this.cbxMethod.SelectedIndex = Bridge.Get().Get<int>("Text.SearchReplace.Method");
            this.cbxSearchIn.SelectedIndex = Bridge.Get().Get<int>("Text.SearchReplace.SearchIn");
            this.cbxOptionMatchCase.Checked = Bridge.Get().Get<bool>("Text.SearchReplace.MatchCase");
            this.cbxOptionWholeWord.Checked = Bridge.Get().Get<bool>("Text.SearchReplace.MatchWholeWord");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Save preferences for the next time the search is opened.
            Bridge.Get().Set("Text.SearchReplace.Replace", this.cbxReplace.Checked);
            Bridge.Get().Set("Text.SearchReplace.Method", this.cbxMethod.SelectedIndex);
            Bridge.Get().Set("Text.SearchReplace.SearchIn", this.cbxSearchIn.SelectedIndex);
            Bridge.Get().Set("Text.SearchReplace.MatchCase", this.cbxOptionMatchCase.Checked);
            Bridge.Get().Set("Text.SearchReplace.MatchWholeWord", this.cbxOptionWholeWord.Checked);

            base.OnClosing(e);
        }

        private void cbxReplace_CheckedChanged(object sender, EventArgs e)
        {
            this.tbxReplace.Enabled = this.cbxReplace.Checked;
            this.btnReplace.Enabled = this.cbxReplace.Checked;
            this.btnReplaceAll.Enabled = this.cbxReplace.Checked;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            this.Search(true);
        }

        private void Search(bool manualSearch)
        {
            if (String.IsNullOrEmpty(this.tbxSearch.Text))
            {
                this.tabControl.TabPages.Clear();
                return;
            }

            SuspendLayout();
            //GUI.SuspendControlPainting(this);

            foreach (SearchReplaceResultPage page in this.tabControl.TabPages)
            {
                page.TextControl.Modified -= this.TextControl_Modified;
            }

            this.tabControl.TabPages.Clear();

            switch (this.cbxSearchIn.SelectedIndex)
            {
                case 0: // Current document
                {
                    var textView = Bridge.Get<IBText>().GetCurrentTextControl<ITextView>();

                    if (textView != null)
                    {
                        this.SearchTextControl(textView, manualSearch);

                        textView.Modified += this.TextControl_Modified;
                    }
                }
                    break;
                case 1: // All open documents
                {
                    foreach (ITextView textView in Bridge.Get<IBText>().GetTextControls<ITextView>())
                    {
                        this.SearchTextControl(textView, manualSearch);

                        textView.Modified += this.TextControl_Modified;
                    }
                }
                    break;
            }

            ResumeLayout();
            //GUI.ResumeControlPainting(this);
            Invalidate();
        }

        private void TextControl_Modified(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                ((ITextBase) sender).Modified -= this.TextControl_Modified;
                return;
            }

            if (this.controlledAltering == false)
            {
                // If the altering of text in any text control was not under a controlled method (the user manually altering it and 
                // not the "replace" functions altering it), the search should be redone to ensure correct representation.
                this.Search(true);
            }
        }

        private void SearchTextControl(ITextView textControl, bool manualSearch)
        {
            var text = textControl.TextGet();

            if (String.IsNullOrEmpty(textControl.CurrentFilePath))
            {
                return;
            }

            var systemNode = Bridge.Get().GetSystemNode(textControl.CurrentFilePath);

            SearchReplaceResultPage page;

            if (this.tabControl.TabPages.ContainsKey(systemNode.Name) == false)
            {
                page = new SearchReplaceResultPage(textControl);
                page.Text = page.Name = systemNode.Name;

                this.tabControl.TabPages.Add(page);
            }
            else
            {
                page = this.tabControl.TabPages[systemNode.Name] as SearchReplaceResultPage;
            }

            var searchRegex = this.GetRegexSearchText();
            var options = this.GetRegexOptions();

            MatchCollection matches;

            try
            {
                matches = Regex.Matches(text, searchRegex, options);
            }
            catch (Exception ex)
            {
                if (manualSearch)
                {
                    Bridge.Get().Error(String.Format(AppResources.Regex_NotValidBecause, ex.Message), Strings.FindReplace_Title);
                }

                return;
            }

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    var result = new ListResult();

                    result.MatchStart = match.Index;
                    result.MatchLength = match.Length;
                    result.MatchString = match.Value;

                    result.LineText = GetLineOfText(text, match.Index);

                    page.ListBox.AddResult(result);
                }
            }
            else
            {
                // Nothign found, so no use keeping it around.
                this.tabControl.TabPages.Remove(page);
            }
        }

        private string GetRegexSearchText()
        {
            string searchRegex;

            switch (this.cbxMethod.SelectedIndex)
            {
                case 0:
                {
                    // Escape all regex since we are search after actual text.
                    searchRegex = Regex.Escape(this.tbxSearch.Text);
                }
                    break;
                case 1:
                {
                    // Escape all regex except * since it is the allowed wildcard, and then convert into regex.
                    searchRegex = Regex.Escape(this.tbxSearch.Text).Replace("\\*", "*").Replace("*", ".*?");
                }
                    break;
                case 2:
                {
                    // Don't need to do anything, since we will be searching using regex.
                    searchRegex = this.tbxSearch.Text;
                }
                    break;
                default:
                    searchRegex = null;
                    break;
            }

            if (this.cbxOptionWholeWord.Checked)
            {
                searchRegex = @"\b" + searchRegex + @"\b";
            }

            return searchRegex;
        }

        private RegexOptions GetRegexOptions()
        {
            var options = RegexOptions.Singleline; //RegexOptions.Multiline;

            if (this.cbxOptionMatchCase.Checked == false)
            {
                options |= RegexOptions.IgnoreCase;
            }

            return options;
        }

        private static string GetLineOfText(string text, int index)
        {
            var startIndex = index;
            var endIndex = index;

            for (; startIndex > 0; startIndex--)
            {
                if (text[startIndex] == '\n')
                {
                    startIndex++; // Increment by one to not get the prepending '\n' for the result.
                    break;
                }
            }

            for (; endIndex < text.Length; endIndex++)
            {
                if (text[endIndex] == '\n')
                {
                    break;
                }
            }

            if (startIndex < 0)
            {
                return String.Empty;
            }

            if (endIndex - startIndex <= 0)
            {
                return String.Empty;
            }

            return text.Substring(startIndex, endIndex - startIndex).Replace("\r", String.Empty);
        }

        private void btnReplace_Click(object sender, EventArgs e)
        {
            if (this.tabControl.SelectedIndex == -1)
            {
                return;
            }

            var page = this.tabControl.TabPages[this.tabControl.SelectedIndex] as SearchReplaceResultPage;

            if (page.ListBox.SelectedIndex == -1)
            {
                return;
            }

            this.Replace(page.TextControl, page, page.ListBox.SelectedIndex);
        }

        private void btnReplaceAll_Click(object sender, EventArgs e)
        {
            foreach (SearchReplaceResultPage page in this.tabControl.TabPages)
            {
                for (var i = 0; i < page.ListBox.Items.Count; i++)
                {
                    this.Replace(page.TextControl, page, i);
                    i--; // Since we've had one removed.
                }
            }
        }

        private void Replace(ITextView textControl, SearchReplaceResultPage page, int index)
        {
            var result = page.ListBox.Items[index] as ListResult;

            if (result.BeenReplaced)
            {
                // If it has already been replaced, there's not much use trying to replace it again ^^
                return;
            }

            var textLength = textControl.TextLength;

            this.controlledAltering = true;

            // Set these two again, just to make sure it is selected properly and has not been manually tampered with.
            textControl.SelectionStart = result.MatchStart;
            textControl.SelectionLength = result.MatchLength;

            textControl.SelectedText = Regex.Replace(
                result.MatchString,
                this.GetRegexSearchText(),
                // TODO: This should be cached and not have to be reevaluated for each replace.
                this.tbxReplace.Text,
                this.GetRegexOptions());

            this.controlledAltering = false;

            var newText = textControl.TextGet();

            page.ListBox.Items.RemoveAt(index);

            if (page.ListBox.Items.Count < index)
            {
                page.ListBox.SelectedIndex = index;
            }

            var newTextLength = newText.Length;

            if (textLength != newTextLength)
            {
                for (var n = index; n < page.ListBox.Items.Count; n++)
                {
                    var subsequentResult = page.ListBox.Items[n] as ListResult;

                    subsequentResult.MatchStart = subsequentResult.MatchStart - (textLength - newTextLength);
                    subsequentResult.LineText = GetLineOfText(newText, subsequentResult.MatchStart);
                }
            }

            page.ListBox.Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }

            base.OnKeyDown(e);
        }

        private void tbxSearch_TextChanged(object sender, EventArgs e)
        {
            var tabCount = this.tabControl.TabCount;
            var tabIndex = this.tabControl.SelectedIndex;

            staticTextChangedAutomatically = true;
            this.Search(false);
            this.tbxSearch.Focus();
            staticTextChangedAutomatically = false;

            if (tabCount == this.tabControl.TabCount)
            {
                this.tabControl.SelectedIndex = tabIndex;
            }
        }

        private void cbxOptionMatchCase_CheckedChanged(object sender, EventArgs e)
        {
            var tabCount = this.tabControl.TabCount;
            var tabIndex = this.tabControl.SelectedIndex;

            // Criterias have changed, so let's search again.
            staticTextChangedAutomatically = true;
            this.Search(false);
            staticTextChangedAutomatically = false;

            if (tabCount == this.tabControl.TabCount)
            {
                this.tabControl.SelectedIndex = tabIndex;
            }
        }

        private void cbxOptionWholeWord_CheckedChanged(object sender, EventArgs e)
        {
            var tabCount = this.tabControl.TabCount;
            var tabIndex = this.tabControl.SelectedIndex;

            // Criterias have changed, so let's search again.
            staticTextChangedAutomatically = true;
            this.Search(false);
            staticTextChangedAutomatically = false;

            if (tabCount == this.tabControl.TabCount)
            {
                this.tabControl.SelectedIndex = tabIndex;
            }
        }

        private void cbxSearchIn_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tabCount = this.tabControl.TabCount;
            var tabIndex = this.tabControl.SelectedIndex;

            // Criterias have changed, so let's search again.
            staticTextChangedAutomatically = true;
            this.Search(false);
            staticTextChangedAutomatically = false;

            if (tabCount == this.tabControl.TabCount)
            {
                this.tabControl.SelectedIndex = tabIndex;
            }
        }

        private void cbxMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tabCount = this.tabControl.TabCount;
            var tabIndex = this.tabControl.SelectedIndex;

            // Criterias have changed, so let's search again.
            staticTextChangedAutomatically = true;
            this.Search(false);
            staticTextChangedAutomatically = false;

            if (tabCount == this.tabControl.TabCount)
            {
                this.tabControl.SelectedIndex = tabIndex;
            }
        }

        #region Nested type: ListBoxAdv

        private class ListBoxAdv : ListBox
        {
            private readonly ITextView textControl;

            public ListBoxAdv(ITextView textControl)
            {
                this.textControl = textControl;

                DrawMode = DrawMode.OwnerDrawFixed;
                DrawItem += this.ListBoxAdv_DrawItem;
            }

            private void ListBoxAdv_DrawItem(object sender, DrawItemEventArgs e)
            {
                if (e.Index == -1)
                {
                    return;
                }

                var result = Items[e.Index] as ListResult;

                var lineText = result.LineText.Replace("\t", "\\t");

                if (result.BeenReplaced)
                {
                    e.Graphics.FillRectangle(Brushes.LightGreen, e.Bounds);

                    using (var b = new SolidBrush(Color.Black))
                    {
                        e.Graphics.DrawString(lineText, e.Font, b, e.Bounds.Location);
                    }
                }
                else
                {
                    e.DrawBackground();

                    using (var b = new SolidBrush(e.ForeColor))
                    {
                        e.Graphics.DrawString(
                            lineText + " [" + result.MatchString.Replace("\n", "\\n").Replace("\t", "\\t") + "]", e.Font,
                            b, e.Bounds.Location);
                    }
                }

                if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
                {
                    e.DrawFocusRectangle();
                }
            }

            public void AddResult(ListResult result)
            {
                Items.Add(result);
            }

            protected override void OnSelectedIndexChanged(EventArgs e)
            {
                if (staticTextChangedAutomatically == false)
                {
                    if (Items.Count > 0 && SelectedIndex != -1)
                    {
                        var result = (Items[SelectedIndex] as ListResult);

                        if (result.BeenReplaced)
                        {
                            return;
                        }

                        using (var lock1 = new BlockLock("Page"))
                        {
                            using (var lock2 = new State.SkipParseState(true))
                            {
                                this.textControl.Focus(); // Give focus so that the docking library can open and focus the control.
                                Focus(); // And then give back focus so that the scroll wheel can be used (or we'd be scrolling the text control).
                                this.textControl.SelectionStart = result.MatchStart;
                                this.textControl.SelectionLength = result.MatchLength;

                                this.textControl.ScrollToCaret();
                            }
                        }
                    }
                }

                base.OnSelectedIndexChanged(e);
            }
        }

        #endregion

        #region Nested type: ListResult

        private class ListResult
        {
            public string LineText { get; set; }

            public int MatchStart { get; set; }
            public int MatchLength { get; set; }

            public string MatchString { get; set; }

            public bool BeenReplaced { get; set; }
        }

        #endregion

        #region Nested type: SearchReplaceResultPage

        private class SearchReplaceResultPage : TabPage
        {
            private readonly ListBoxAdv listBox;

            public SearchReplaceResultPage(ITextView textControl)
            {
                this.TextControl = textControl;
                this.listBox = new ListBoxAdv(textControl);

                this.listBox.Dock = DockStyle.Fill;
                Controls.Add(this.listBox);
            }

            public ListBoxAdv ListBox
            {
                get { return this.listBox; }
            }

            public ITextView TextControl { get; private set; }
        }

        #endregion
    }
}