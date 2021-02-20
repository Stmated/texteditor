
namespace Eliason.TextEditor.SearchReplace
{
    partial class SearchReplaceForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            foreach (SearchReplaceResultPage page in this.tabControl.TabPages)
            {
                page.TextControl.Modified -= this.TextControl_Modified;
            }

            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchReplaceForm));
            this.tbxSearch = new System.Windows.Forms.TextBox();
            this.cbxSearchIn = new System.Windows.Forms.ComboBox();
            this.cbxMethod = new System.Windows.Forms.ComboBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.lblSearchIn = new System.Windows.Forms.Label();
            this.lblMethod = new System.Windows.Forms.Label();
            //this.hlblResults = new HeaderLabel();
            //this.hlblOptions = new HeaderLabel();
            this.cbxOptionMatchCase = new System.Windows.Forms.CheckBox();
            this.cbxOptionWholeWord = new System.Windows.Forms.CheckBox();
            this.cbxReplace = new System.Windows.Forms.CheckBox();
            this.tbxReplace = new System.Windows.Forms.TextBox();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnReplaceAll = new System.Windows.Forms.Button();
            this.btnReplace = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbxSearch
            // 
            resources.ApplyResources(this.tbxSearch, "tbxSearch");
            this.tbxSearch.Name = "tbxSearch";
            this.tbxSearch.TextChanged += new System.EventHandler(this.tbxSearch_TextChanged);
            // 
            // cbxSearchIn
            // 
            resources.ApplyResources(this.cbxSearchIn, "cbxSearchIn");
            this.cbxSearchIn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxSearchIn.FormattingEnabled = true;
            this.cbxSearchIn.Items.AddRange(new object[] {
            resources.GetString("cbxSearchIn.Items"),
            resources.GetString("cbxSearchIn.Items1")});
            this.cbxSearchIn.Name = "cbxSearchIn";
            this.cbxSearchIn.SelectedIndexChanged += new System.EventHandler(this.cbxSearchIn_SelectedIndexChanged);
            // 
            // cbxMethod
            // 
            resources.ApplyResources(this.cbxMethod, "cbxMethod");
            this.cbxMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxMethod.FormattingEnabled = true;
            this.cbxMethod.Items.AddRange(new object[] {
            resources.GetString("cbxMethod.Items"),
            resources.GetString("cbxMethod.Items1"),
            resources.GetString("cbxMethod.Items2")});
            this.cbxMethod.Name = "cbxMethod";
            this.cbxMethod.SelectedIndexChanged += new System.EventHandler(this.cbxMethod_SelectedIndexChanged);
            // 
            // lblSearch
            // 
            resources.ApplyResources(this.lblSearch, "lblSearch");
            this.lblSearch.Name = "lblSearch";
            // 
            // lblSearchIn
            // 
            resources.ApplyResources(this.lblSearchIn, "lblSearchIn");
            this.lblSearchIn.Name = "lblSearchIn";
            // 
            // lblMethod
            // 
            resources.ApplyResources(this.lblMethod, "lblMethod");
            this.lblMethod.Name = "lblMethod";
            // 
            // hlblResults
            // 
            //resources.ApplyResources(this.hlblResults, "hlblResults");
            //this.hlblResults.ForeColor = System.Drawing.SystemColors.Highlight;
            //this.hlblResults.Name = "hlblResults";
            //this.hlblResults.TabStop = false;
            // 
            // hlblOptions
            // 
            //resources.ApplyResources(this.hlblOptions, "hlblOptions");
            //this.hlblOptions.ForeColor = System.Drawing.SystemColors.Highlight;
            //this.hlblOptions.Name = "hlblOptions";
            //this.hlblOptions.TabStop = false;
            // 
            // cbxOptionMatchCase
            // 
            resources.ApplyResources(this.cbxOptionMatchCase, "cbxOptionMatchCase");
            this.cbxOptionMatchCase.Name = "cbxOptionMatchCase";
            this.cbxOptionMatchCase.UseVisualStyleBackColor = true;
            this.cbxOptionMatchCase.CheckedChanged += new System.EventHandler(this.cbxOptionMatchCase_CheckedChanged);
            // 
            // cbxOptionWholeWord
            // 
            resources.ApplyResources(this.cbxOptionWholeWord, "cbxOptionWholeWord");
            this.cbxOptionWholeWord.Name = "cbxOptionWholeWord";
            this.cbxOptionWholeWord.UseVisualStyleBackColor = true;
            this.cbxOptionWholeWord.CheckedChanged += new System.EventHandler(this.cbxOptionWholeWord_CheckedChanged);
            // 
            // cbxReplace
            // 
            resources.ApplyResources(this.cbxReplace, "cbxReplace");
            this.cbxReplace.Name = "cbxReplace";
            this.cbxReplace.UseVisualStyleBackColor = true;
            this.cbxReplace.CheckedChanged += new System.EventHandler(this.cbxReplace_CheckedChanged);
            // 
            // tbxReplace
            // 
            resources.ApplyResources(this.tbxReplace, "tbxReplace");
            this.tbxReplace.Name = "tbxReplace";
            // 
            // tabControl
            // 
            resources.ApplyResources(this.tabControl, "tabControl");
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            // 
            // btnSearch
            // 
            resources.ApplyResources(this.btnSearch, "btnSearch");
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // btnReplaceAll
            // 
            resources.ApplyResources(this.btnReplaceAll, "btnReplaceAll");
            this.btnReplaceAll.Name = "btnReplaceAll";
            this.btnReplaceAll.UseVisualStyleBackColor = true;
            this.btnReplaceAll.Click += new System.EventHandler(this.btnReplaceAll_Click);
            // 
            // btnReplace
            // 
            resources.ApplyResources(this.btnReplace, "btnReplace");
            this.btnReplace.Name = "btnReplace";
            this.btnReplace.UseVisualStyleBackColor = true;
            this.btnReplace.Click += new System.EventHandler(this.btnReplace_Click);
            // 
            // SearchReplaceForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnReplace);
            this.Controls.Add(this.btnReplaceAll);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.tbxReplace);
            this.Controls.Add(this.cbxReplace);
            this.Controls.Add(this.cbxOptionWholeWord);
            this.Controls.Add(this.cbxOptionMatchCase);
            //this.Controls.Add(this.hlblOptions);
            //this.Controls.Add(this.hlblResults);
            this.Controls.Add(this.lblMethod);
            this.Controls.Add(this.lblSearchIn);
            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.cbxMethod);
            this.Controls.Add(this.cbxSearchIn);
            this.Controls.Add(this.tbxSearch);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.KeyPreview = true;
            this.Name = "SearchReplaceForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbxSearch;
        private System.Windows.Forms.ComboBox cbxSearchIn;
        private System.Windows.Forms.ComboBox cbxMethod;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.Label lblSearchIn;
        private System.Windows.Forms.Label lblMethod;
        //private HeaderLabel hlblResults;
        //private HeaderLabel hlblOptions;
        private System.Windows.Forms.CheckBox cbxOptionMatchCase;
        private System.Windows.Forms.CheckBox cbxOptionWholeWord;
        private System.Windows.Forms.CheckBox cbxReplace;
        private System.Windows.Forms.TextBox tbxReplace;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnReplaceAll;
        private System.Windows.Forms.Button btnReplace;
    }
}