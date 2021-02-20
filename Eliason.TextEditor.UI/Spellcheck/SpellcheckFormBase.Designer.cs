

namespace Eliason.TextEditor.Spellcheck
{
    partial class SpellcheckFormBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpellcheckFormBase));
            this.lbxSuggestions = new System.Windows.Forms.ListBox();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tpSpellcheck = new System.Windows.Forms.TabPage();
            this.btnAddWord = new System.Windows.Forms.Button();
            this.lblSuggestions = new libCommon.UI.HeaderLabel();
            this.lblReplaceWith = new libCommon.UI.HeaderLabel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnReplaceAll = new System.Windows.Forms.Button();
            this.btnReplace = new System.Windows.Forms.Button();
            this.btnIgnoreWord = new System.Windows.Forms.Button();
            this.btnIgnore = new System.Windows.Forms.Button();
            this.tbxReplaceWith = new System.Windows.Forms.TextBox();
            this.tpOptions = new System.Windows.Forms.TabPage();
            this.btnDownloadOO = new System.Windows.Forms.Button();
            this.gbxIgnoreOptions = new System.Windows.Forms.GroupBox();
            this.cbxEnableInlineSpellcheck = new System.Windows.Forms.CheckBox();
            this.cbxIgnoreAllCaps = new System.Windows.Forms.CheckBox();
            this.cbxIgnoreWordsWithNumbers = new System.Windows.Forms.CheckBox();
            this.cbxIgnoreWordsWithAsianChars = new System.Windows.Forms.CheckBox();
            this.tbxLinesWithPrefix = new System.Windows.Forms.TextBox();
            this.hlblLinesWithPrefix = new libCommon.UI.HeaderLabel();
            this.tabControl.SuspendLayout();
            this.tpSpellcheck.SuspendLayout();
            this.tpOptions.SuspendLayout();
            this.gbxIgnoreOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbxSuggestions
            // 
            resources.ApplyResources(this.lbxSuggestions, "lbxSuggestions");
            this.lbxSuggestions.FormattingEnabled = true;
            this.lbxSuggestions.Name = "lbxSuggestions";
            this.lbxSuggestions.SelectedIndexChanged += new System.EventHandler(this.lbxSuggestions_SelectedIndexChanged);
            this.lbxSuggestions.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lbxSuggestions_KeyDown);
            this.lbxSuggestions.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbxSuggestions_MouseDoubleClick);
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tpSpellcheck);
            this.tabControl.Controls.Add(this.tpOptions);
            resources.ApplyResources(this.tabControl, "tabControl");
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            // 
            // tpSpellcheck
            // 
            this.tpSpellcheck.Controls.Add(this.btnAddWord);
            this.tpSpellcheck.Controls.Add(this.lblSuggestions);
            this.tpSpellcheck.Controls.Add(this.lblReplaceWith);
            this.tpSpellcheck.Controls.Add(this.btnClose);
            this.tpSpellcheck.Controls.Add(this.btnReplaceAll);
            this.tpSpellcheck.Controls.Add(this.btnReplace);
            this.tpSpellcheck.Controls.Add(this.btnIgnoreWord);
            this.tpSpellcheck.Controls.Add(this.btnIgnore);
            this.tpSpellcheck.Controls.Add(this.tbxReplaceWith);
            this.tpSpellcheck.Controls.Add(this.lbxSuggestions);
            resources.ApplyResources(this.tpSpellcheck, "tpSpellcheck");
            this.tpSpellcheck.Name = "tpSpellcheck";
            this.tpSpellcheck.UseVisualStyleBackColor = true;
            // 
            // btnAddWord
            // 
            resources.ApplyResources(this.btnAddWord, "btnAddWord");
            this.btnAddWord.Name = "btnAddWord";
            this.btnAddWord.UseVisualStyleBackColor = true;
            this.btnAddWord.Click += new System.EventHandler(this.btnAddWord_Click);
            // 
            // lblSuggestions
            // 
            resources.ApplyResources(this.lblSuggestions, "lblSuggestions");
            this.lblSuggestions.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lblSuggestions.Name = "lblSuggestions";
            this.lblSuggestions.TabStop = false;
            // 
            // lblReplaceWith
            // 
            resources.ApplyResources(this.lblReplaceWith, "lblReplaceWith");
            this.lblReplaceWith.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lblReplaceWith.Name = "lblReplaceWith";
            this.lblReplaceWith.TabStop = false;
            // 
            // btnClose
            // 
            resources.ApplyResources(this.btnClose, "btnClose");
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Name = "btnClose";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
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
            // btnIgnoreWord
            // 
            resources.ApplyResources(this.btnIgnoreWord, "btnIgnoreWord");
            this.btnIgnoreWord.Name = "btnIgnoreWord";
            this.btnIgnoreWord.UseVisualStyleBackColor = true;
            this.btnIgnoreWord.Click += new System.EventHandler(this.btnIgnoreWord_Click);
            // 
            // btnIgnore
            // 
            resources.ApplyResources(this.btnIgnore, "btnIgnore");
            this.btnIgnore.Name = "btnIgnore";
            this.btnIgnore.UseVisualStyleBackColor = true;
            this.btnIgnore.Click += new System.EventHandler(this.btnIgnore_Click);
            // 
            // tbxReplaceWith
            // 
            resources.ApplyResources(this.tbxReplaceWith, "tbxReplaceWith");
            this.tbxReplaceWith.Name = "tbxReplaceWith";
            // 
            // tpOptions
            // 
            this.tpOptions.Controls.Add(this.btnDownloadOO);
            this.tpOptions.Controls.Add(this.gbxIgnoreOptions);
            resources.ApplyResources(this.tpOptions, "tpOptions");
            this.tpOptions.Name = "tpOptions";
            this.tpOptions.UseVisualStyleBackColor = true;
            // 
            // btnDownloadOO
            // 
            resources.ApplyResources(this.btnDownloadOO, "btnDownloadOO");
            this.btnDownloadOO.Name = "btnDownloadOO";
            this.btnDownloadOO.UseVisualStyleBackColor = true;
            this.btnDownloadOO.Click += new System.EventHandler(this.btnDownloadOO_Click);
            // 
            // gbxIgnoreOptions
            // 
            resources.ApplyResources(this.gbxIgnoreOptions, "gbxIgnoreOptions");
            this.gbxIgnoreOptions.Controls.Add(this.hlblLinesWithPrefix);
            this.gbxIgnoreOptions.Controls.Add(this.tbxLinesWithPrefix);
            this.gbxIgnoreOptions.Controls.Add(this.cbxEnableInlineSpellcheck);
            this.gbxIgnoreOptions.Controls.Add(this.cbxIgnoreAllCaps);
            this.gbxIgnoreOptions.Controls.Add(this.cbxIgnoreWordsWithNumbers);
            this.gbxIgnoreOptions.Controls.Add(this.cbxIgnoreWordsWithAsianChars);
            this.gbxIgnoreOptions.Name = "gbxIgnoreOptions";
            this.gbxIgnoreOptions.TabStop = false;
            // 
            // cbxEnableInlineSpellcheck
            // 
            resources.ApplyResources(this.cbxEnableInlineSpellcheck, "cbxEnableInlineSpellcheck");
            this.cbxEnableInlineSpellcheck.Checked = true;
            this.cbxEnableInlineSpellcheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxEnableInlineSpellcheck.Name = "cbxEnableInlineSpellcheck";
            this.cbxEnableInlineSpellcheck.UseVisualStyleBackColor = true;
            this.cbxEnableInlineSpellcheck.CheckedChanged += new System.EventHandler(this.cbxEnableInlineSpellcheck_CheckedChanged);
            // 
            // cbxIgnoreAllCaps
            // 
            resources.ApplyResources(this.cbxIgnoreAllCaps, "cbxIgnoreAllCaps");
            this.cbxIgnoreAllCaps.Name = "cbxIgnoreAllCaps";
            this.cbxIgnoreAllCaps.UseVisualStyleBackColor = true;
            this.cbxIgnoreAllCaps.CheckedChanged += new System.EventHandler(this.cbxIgnoreAllCaps_CheckedChanged);
            // 
            // cbxIgnoreWordsWithNumbers
            // 
            resources.ApplyResources(this.cbxIgnoreWordsWithNumbers, "cbxIgnoreWordsWithNumbers");
            this.cbxIgnoreWordsWithNumbers.Name = "cbxIgnoreWordsWithNumbers";
            this.cbxIgnoreWordsWithNumbers.UseVisualStyleBackColor = true;
            this.cbxIgnoreWordsWithNumbers.CheckedChanged += new System.EventHandler(this.cbxIgnoreWordsWithNumbers_CheckedChanged);
            // 
            // cbxIgnoreWordsWithAsianChars
            // 
            resources.ApplyResources(this.cbxIgnoreWordsWithAsianChars, "cbxIgnoreWordsWithAsianChars");
            this.cbxIgnoreWordsWithAsianChars.Name = "cbxIgnoreWordsWithAsianChars";
            this.cbxIgnoreWordsWithAsianChars.UseVisualStyleBackColor = true;
            this.cbxIgnoreWordsWithAsianChars.CheckedChanged += new System.EventHandler(this.cbxIgnoreWordsWithAsianChars_CheckedChanged);
            // 
            // tbxLinesWithPrefix
            // 
            resources.ApplyResources(this.tbxLinesWithPrefix, "tbxLinesWithPrefix");
            this.tbxLinesWithPrefix.Name = "tbxLinesWithPrefix";
            this.tbxLinesWithPrefix.TextChanged += new System.EventHandler(this.tbxLinesWithPrefix_TextChanged);
            // 
            // hlblLinesWithPrefix
            // 
            this.hlblLinesWithPrefix.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.hlblLinesWithPrefix.ForeColor = System.Drawing.SystemColors.Highlight;
            resources.ApplyResources(this.hlblLinesWithPrefix, "hlblLinesWithPrefix");
            this.hlblLinesWithPrefix.Name = "hlblLinesWithPrefix";
            this.hlblLinesWithPrefix.TabStop = false;
            // 
            // SpellcheckFormBase
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SpellcheckFormBase";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.tabControl.ResumeLayout(false);
            this.tpSpellcheck.ResumeLayout(false);
            this.tpSpellcheck.PerformLayout();
            this.tpOptions.ResumeLayout(false);
            this.gbxIgnoreOptions.ResumeLayout(false);
            this.gbxIgnoreOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbxSuggestions;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tpSpellcheck;
        private System.Windows.Forms.TabPage tpOptions;
        private System.Windows.Forms.CheckBox cbxIgnoreAllCaps;
        private System.Windows.Forms.CheckBox cbxIgnoreWordsWithNumbers;
        private System.Windows.Forms.CheckBox cbxIgnoreWordsWithAsianChars;
        private System.Windows.Forms.TextBox tbxReplaceWith;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnReplaceAll;
        private System.Windows.Forms.Button btnReplace;
        private System.Windows.Forms.Button btnIgnoreWord;
        private System.Windows.Forms.Button btnIgnore;
        private HeaderLabel lblReplaceWith;
        private HeaderLabel lblSuggestions;
        private System.Windows.Forms.GroupBox gbxIgnoreOptions;
        private System.Windows.Forms.Button btnAddWord;
        private System.Windows.Forms.CheckBox cbxEnableInlineSpellcheck;
        private System.Windows.Forms.Button btnDownloadOO;
        private HeaderLabel hlblLinesWithPrefix;
        private System.Windows.Forms.TextBox tbxLinesWithPrefix;
    }
}