using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Eliason.TextEditor.TextDocument.ByLines;
using Eliason.TextEditor.TextStyles;
using Eliason.TextEditor.UndoRedo;

namespace Eliason.TextEditor.TextEditor
{
    /// <summary>
    /// This is an abstract class base for a text control, that contains all the code that maintains the text but is not involved
    /// with anything visual, such as rendering, selection handling, keyboard/mouse support.
    /// </summary>
    public abstract class TextEditor : Control, ITextEditor
    {
        private const string AUTO_SAVE_DATE_FORMAT = "yyMMdd-HH.mm.ss";

        /// <summary>
        ///   The system path to the last used Auto-Save location for this text's file.
        /// </summary>
        //protected readonly Queue<string> lastAutoSaveFilePaths = new Queue<string>();

        /// <summary>
        ///   The Auto-Save timer, set to a certain interval for when to save the text file to temp.
        /// </summary>
        private Timer _autoSaveTimer;

        private Encoding _currentEncoding;

        private string _currentFilePath;

        public int CurrentTextColumnIndex { get; protected set; }

        public bool IsVirtual { get; protected set; }

        public ITextDocument TextDocument { get; private set; }

        public string Description { get; set; }

        public ISettings Settings { get; private set; }

        public bool KeepAutoSavesOnDispose { get; set; }

        public virtual string CurrentFilePath
        {
            get
            {
                if (this.IsVirtual == false)
                {
                    return this._currentFilePath;
                }

                foreach (var textView in this.TextDocument.GetTextViews())
                {
                    if (textView.IsVirtual == false)
                    {
                        return textView.CurrentFilePath;
                    }
                }

                throw new InvalidOperationException("There is no known CurrentFilePath for the TextEditor. This is a serious error.");
            }

            set
            {
                if (this.IsVirtual)
                {
                    foreach (var textView in this.TextDocument.GetTextViews())
                    {
                        if (textView.IsVirtual == false)
                        {
                            textView.CurrentFilePath = value;
                            return;
                        }
                    }

                    throw new InvalidOperationException("There was no source text view to set the current filepath to. All was virtual. This should not be possible.");
                }

                this._currentFilePath = value;
            }
        }

        public bool IsModified
        {
            get { return this.TextDocument.IsModified; }
            set { this.TextDocument.IsModified = value; }
        }

        public virtual CultureInfo Language { get; set; }

        protected TextEditor(ISettings settings)
        {
            this.CurrentTextColumnIndex = 0;
            this.Settings = settings;
        }

        /// <summary>
        /// Gets all the text of the text editor as a single newline-delimited string.
        /// </summary>
        /// <returns>The System.String of all text editor content.</returns>
        public string TextGet()
        {
            return this.TextDocument.TextGet(0, this.TextLength, this.CurrentTextColumnIndex);
        }

        /// <summary>
        /// Clears all text from the text editor and begins afresh.
        /// </summary>
        public virtual void Clear()
        {
            this.TextDocument.Clear();

            this.TextDocument.UndoRedoManager.ClearUndoRedo();
            this.TextDocument.TextSegmentStyledManager.Clear(true);

            this.CurrentFilePath = string.Empty;
            this.IsModified = false;
            this.Description = string.Empty;
        }

        /// <summary>
        /// Opens a new text file in the text editor.
        /// </summary>
        public abstract void Open();

        /// <summary>
        /// Opens the specified text file in the text editor.
        /// </summary>
        /// <param name="filePath">The system filepath to the text file to open.</param>
        public virtual void Open(string filePath)
        {
            string usedFilePath = null;

            #region Check for autosaved version

            var tempFilePath = filePath + ".tmp";
            if (File.Exists(tempFilePath))
            {
                //var tempFile =  new File(filePath + ".tmp");
                //var loadDate = File.GetLastWriteTime(tempFilePath);
                var fileInfo = new FileInfo(filePath);
                var tempFileInfo = new FileInfo(tempFilePath);

                if (this.Settings.Notifier.AskYesNo(null, String.Format(Strings.TextControl_AutoSave_OneOpenOption_Ask, fileInfo.LastWriteTime, tempFileInfo.LastWriteTime)))
                {
                    usedFilePath = tempFileInfo.FullName;
                    this.Settings.Notifier.Info(Strings.TextControl_Title, Strings.TextControl_AutoSave_OpenInformation);
                }

                /*var temporaryFiles = new List<string>();

                foreach (var file in Directory.GetFiles(this.Settings.AutoSaveDirectoryPath))
                {
                    var fileInfo = new FileInfo(file);
                    var fileName = fileInfo.Name;

                    // If it's not a temporary file of this text file,
                    if (fileName.StartsWith(loadFile) == false)
                    {
                        continue;
                    }

                    var beginDate = file.LastIndexOf('[');
                    var endDate = file.LastIndexOf(']');

                    if (beginDate == -1 || endDate == -1 || beginDate > endDate)
                    {
                        // This is not an auto-save file, since it does not follow the proper file-naming scheme.
                        continue;
                    }

                    var dateString = file.Substring(beginDate + 1, endDate - beginDate - 1);

                    var fileDate = DateTime.ParseExact(dateString, AUTO_SAVE_DATE_FORMAT, CultureInfo.InvariantCulture);

                    // or the saved file is newer than the auto-save.
                    if (fileDate.CompareTo(loadDate) < 1)
                    {
                        File.Delete(file);
                        var anchorFile = GetAnchorFileName(file);

                        if (File.Exists(anchorFile))
                        {
                            File.Delete(anchorFile);
                        }

                        continue;
                    }

                    temporaryFiles.Add(file);
                }

                if (temporaryFiles.Count > 1)
                {
                    temporaryFiles.Sort(StringComparer.OrdinalIgnoreCase);

                    var shortFileNames = new List<string>();

                    foreach (var temporaryFile in temporaryFiles)
                    {
                        shortFileNames.Add(new FileInfo(temporaryFile).LastWriteTime.ToString(CultureInfo.InvariantCulture));
                    }

                    var fi = new FileInfo(filePath);
                    var selectionResult = this.Settings.Notifier.AskSelect(new NotifierSelectRequest<String>()
                    {
                        Title = Strings.TextControl_AutoSave_MultipleOpenOptions_Title,
                        Message = String.Format(Strings.TextControl_AutoSave_MultipleOpenOptions, fi.Name),

                        Options = shortFileNames.ToArray(),
                        CanCancel = true,
                        MultiSelect = false
                    });

                    if (selectionResult.HasResult)
                    {
                        usedFilePath = selectionResult.Result;

                        this.Settings.Notifier.Info(Strings.TextControl_Title, Strings.TextControl_AutoSave_OpenInformation);
                    }
                }
                else if (temporaryFiles.Count == 1)
                {
                    var latestAutoSave = new FileInfo(temporaryFiles[0]);

                    if (this.Settings.Notifier.AskYesNo(null, String.Format(Strings.TextControl_AutoSave_OneOpenOption_Ask, loadDate, latestAutoSave.LastWriteTime)))
                    {
                        usedFilePath = latestAutoSave.FullName;
                        this.Settings.Notifier.Info(Strings.TextControl_Title, Strings.TextControl_AutoSave_OpenInformation);
                    }
                }*/
            }

            #endregion

            if (usedFilePath == null)
            {
                usedFilePath = filePath;
            }

            if (File.Exists(usedFilePath))
            {
                Encoding openEncoding = null;
                bool retry;
                var retries = 0;
                var shownANSIWarning = false;

                do
                {
                    retry = false;

                    using (var s = this.GetFileStream(usedFilePath))
                    {
                        if (s == null)
                        {
                            this.Settings.Notifier.Error(Strings.TextControl_Title, String.Format(Strings.FileXCouldNotLoad, usedFilePath));
                            Dispose();
                            return;
                        }

                        if (retries == 0)
                        {
                            openEncoding = GetCheckIfAscii(s) ? Encoding.Default : Encoding.UTF8;
                        }

                        using (var sr = new StreamReader(s, openEncoding, true))
                        {
                            sr.BaseStream.Seek(0, SeekOrigin.Begin);

                            this.Clear();
                            this.CurrentFilePath = filePath;

                            var index = 0;
                            while (sr.EndOfStream == false)
                            {
                                var lineText = sr.ReadLine();

                                if (shownANSIWarning == false && lineText.Contains("�"))
                                {
                                    shownANSIWarning = true;

                                    if (Equals(openEncoding, Encoding.UTF8))
                                    {
                                        var result = this.Settings.Notifier.AskYesNo(
                                            null,
                                            string.Format("UTF-8 Encoding loading of the text file failed on:\n\"{0}\".\n\nWould you like to try and load it as ANSI?", lineText));

                                        if (result)
                                        {
                                            openEncoding = Encoding.Default;
                                            retry = true;
                                            break;
                                        }
                                    }
                                }

                                var fixedLineText = lineText.Replace("\0", string.Empty);

                                var lengthDifference = Math.Max(lineText.Length, fixedLineText.Length) -
                                                       Math.Min(lineText.Length, fixedLineText.Length);

                                if (lengthDifference > lineText.Length / 3)
                                {
                                    // If the length difference is this big, it most likely is a malformed unicode file that has \0 characters
                                    // after each character, ie. plaintext being straightly translated into multibyte unicode.
                                    // So... we just remove those and use that text instead. A third of the original length is a pretty safe number.
                                    // TODO: Needs foolproof testing!
                                    lineText = fixedLineText;
                                }

                                // Seems there are a few texts that have this odd and weird newline order. Let's just remove it.
                                // Since so far I have no idea what it might cause or why it's there to begin with.
                                // But anyway; the rich text control seems to have issues with it, so this is a thing I am forced to do, I presume.
                                lineText = lineText.Replace("\r\r\n", "\r\n").Replace("\r\n", "\n").Replace("\r", "");

                                this.TextDocument.TextAppendLine(lineText, this.CurrentTextColumnIndex);

                                index += lineText.Length + 1; // +1 for the hidden, virtual newline char.
                            }

                            if (this.LineCount == 0)
                            {
                                this.TextDocument.TextAppendLine(String.Empty, this.CurrentTextColumnIndex);
                            }
                        }
                    }

                    retries++;
                } while (retry);

                this._currentEncoding = openEncoding;
            }
            else
            {
                this.Settings.Notifier.Error(String.Format(Strings.FileXDoesNotExist, usedFilePath), Strings.TextControl_Title);
                Dispose();
            }
        }

        protected static string GetAnchorFileName(string filepath)
        {
            var textFileInfo = new FileInfo(filepath);

            return Path.Combine(
                textFileInfo.Directory.ToString(),
                String.Format("{0}.{1}", textFileInfo.Name.Substring(0, textFileInfo.Name.Length - textFileInfo.Extension.Length), "anchors"));
        }

        protected virtual void OnfterOpen()
        {

        }

        public virtual bool Save()
        {
            if (String.IsNullOrEmpty(this.CurrentFilePath))
            {
                return this.SaveAs();
            }

            return this.SaveAs(this.CurrentFilePath, false);
        }

        public abstract bool SaveAs();

        public bool SaveAs(string filePath)
        {
            return this.SaveAs(filePath, false);
        }

        public void SaveAuto()
        {
            if (this.IsModified == false)
            {
                return;
            }

            if (String.IsNullOrEmpty(this.CurrentFilePath))
            {
                // This file has never been saved and does not deserve auto-saving.
                // Also, it would be a nightmare to try and keep track of what file is which.
                return;
            }

            // Let's remove just the oldest auto-save file if there are three files already.
            // This way, we can make it safer for auto-saving, especially if there is a crash
            // and that auto-save saved upon crashing is complete trash. The user can then reopen and select an older auto-save.
            //if (this.lastAutoSaveFilePaths.Count > 2)
            //{
            //    var autoSaveFilePath = this.lastAutoSaveFilePaths.Dequeue();

            //    if (File.Exists(autoSaveFilePath))
            //    {
            //        File.Delete(autoSaveFilePath);

            //        var anchorsFile = GetAnchorFileName(autoSaveFilePath);

            //        if (File.Exists(anchorsFile))
            //        {
            //            File.Delete(anchorsFile);
            //        }
            //    }
            //}

            if (IsDisposed)
            {
                return;
            }

            try
            {
                //string currentFilePath = this.CurrentFilePath.EscapeAsFilepath();
                //var lastDot = currentFilePath.LastIndexOf('.');
                //var currentDate = "[" + DateTime.Now.ToString(AUTO_SAVE_DATE_FORMAT) + "]";

                //var autoSaveFilePath = Path.Combine(
                //    this.Settings.AutoSaveDirectoryPath,
                //    currentFilePath.Substring(0, lastDot) + currentDate + currentFilePath.Substring(lastDot));

                //this.lastAutoSaveFilePaths.Enqueue(autoSaveFilePath);

                this.SaveAs(this.CurrentFilePath + ".tmp", true);
            }
            catch (Exception ex)
            {
                this.Settings.Notifier.Error(Strings.TextControl_CouldNotAutoSave + " - " + ex.Message, Strings.TextControl_Title);
            }
        }

        public int TextLength
        {
            get { return this.TextDocument.TextLength; }
        }

        public int LineCount
        {
            get { return this.TextDocument.LineCount; }
        }

        //public abstract ILanguage Language { get; }

        public event EventHandler Modified
        {
            add { this.TextDocument.Modified += value; }
            remove { this.TextDocument.Modified -= value; }
        }

        public virtual UndoRedoCommandBase TextInsert(int start, string text)
        {
            var undoRedo = this.TextDocument.TextInsert(start, text, this.CurrentTextColumnIndex);

            return undoRedo;
        }

        public virtual UndoRedoCommandBase TextRemove(int start, int length)
        {
            var undoRedo = this.TextDocument.TextRemove(start, length, this.CurrentTextColumnIndex);

            return undoRedo;
        }

        public void TextAppendLine(string text)
        {
            this.TextDocument.TextAppendLine(text, this.CurrentTextColumnIndex);
        }

        public string TextGet(int start, int length)
        {
            return this.TextDocument.TextGet(start, length, this.CurrentTextColumnIndex);
        }

        public IEnumerable<char> TextGetStream(int start, bool right)
        {
            return this.TextDocument.TextGetStream(start, right, this.CurrentTextColumnIndex);
        }

        public char GetCharFromIndex(int index)
        {
            return this.TextDocument.GetCharFromIndex(index, this.CurrentTextColumnIndex);
        }

        public string GetLineText(int lineIndex)
        {
            return this.TextDocument.GetLineText(lineIndex, this.CurrentTextColumnIndex);
        }

        public int GetFirstCharIndexFromLine(int lineIndex)
        {
            return this.TextDocument.GetFirstCharIndexFromLine(lineIndex);
        }

        public int GetLineLength(int lineIndex)
        {
            return this.TextDocument.GetLineLength(lineIndex, this.CurrentTextColumnIndex);
        }

        public ITextSegmentVisual GetVisualTextSegment(int lineIndex)
        {
            return this.TextDocument.GetVisualTextSegment(lineIndex);
        }

        public int GetLineFromCharIndex(int index)
        {
            return this.TextDocument.GetLineFromCharIndex(index, this.CurrentTextColumnIndex);
        }

        public ITextSegmentStyled CreateStyledTextSegment(TextStyleBase style)
        {
            return this.TextDocument.CreateStyledTextSegment(style);
        }

        public WordSegment GetWord(int globalIndex, ITextSegment insideSegment, bool strict)
        {
            return this.TextDocument.GetWord(globalIndex, insideSegment, strict, this.CurrentTextColumnIndex);
        }

        public WordSegment GetWord(int globalIndex, bool strict)
        {
            return this.TextDocument.GetWord(globalIndex, strict, this.CurrentTextColumnIndex);
        }

        public virtual void Undo()
        {
            this.TextDocument.UndoRedoManager.Undo();
        }

        public virtual void Redo()
        {
            this.TextDocument.UndoRedoManager.Redo();
        }

        protected virtual void SetTextBufferStrategy(ITextDocument textBuffer)
        {
            this.IsVirtual = textBuffer != null;

            if (this.IsVirtual == false)
            {
                textBuffer = new TextDocumentByLines();
            }

            this.TextDocument = textBuffer;
        }

        protected virtual void InitializeComponent()
        {
            this.TextDocument.UndoRedoManager.MaxUndoLevel = this.TextDocument.UndoRedoManager.MaxUndoLevel;

            if (this.Settings.AutoSaveEnabled)
            {
                this._autoSaveTimer = new Timer();
                this._autoSaveTimer.Tick += this._autoSaveTimer_Tick;
                this._autoSaveTimer.Interval = this.Settings.AutoSaveInterval;
            }

            this.Modified += this.TextEditor_Modified;
        }

        private void TextEditor_Modified(object sender, EventArgs e)
        {
            if (this._autoSaveTimer != null)
            {
                if (this.IsModified)
                {
                    this._autoSaveTimer.Start();
                }
                else
                {
                    this._autoSaveTimer.Stop();
                }
            }
        }

        protected virtual Stream GetFileStream(string filePath)
        {
            return new FileStream(filePath, FileMode.Open);
        }

        private static bool GetCheckIfAscii(Stream s)
        {
            // Save current position
            var position = s.Position;

            var ascii = true;
            while (s.Position < s.Length)
            {
                var b = s.ReadByte();

                if (b > 127)
                {
                    ascii = false;
                    break;
                }
            }

            // Move back to the previous position before we started checking for non-ascii characters.
            s.Seek(position, SeekOrigin.Begin);

            return ascii;
        }

        protected virtual bool GetCanSaveTo(string filePath)
        {
            return true;
        }

        protected bool SaveAs(string filePath, bool isAutoSave)
        {
            if (this.GetCanSaveTo(filePath) == false)
            {
                return false;
            }

            if (this._currentEncoding == null)
            {
                this._currentEncoding = Encoding.UTF8;
            }
            else if (this._currentEncoding.IsSingleByte)
            {
                // Just to make sure that the text you write will really end up as unicode.
                foreach (var c in this.TextGet())
                {
                    if (c > 128)
                    {
                        if (this.Settings.Notifier.AskYesNo(null, String.Format(Strings.TextControl_Save_SinglebyteToMultibyte_Ask, this.CurrentFilePath)))
                        {
                            this._currentEncoding = Encoding.UTF8;
                        }

                        break;
                    }
                }
            }

            if (isAutoSave)
            {
                // If this is an auto-save we should do nothing with the text, just save it. No filters.
                this.SaveToFile(filePath, this._currentEncoding);

                return true;
            }

            this.CurrentFilePath = filePath;

            this.FilterTextBuffer(this.TextDocument);

            this.SaveToFile(filePath, this._currentEncoding);

            this.IsModified = false;

            // Since the control is being saved manually, we can be pretty sure this is safe;
            // hence, we should remove the last autosave files so we limit the amount of crap files created.
            if (File.Exists(filePath + ".tmp"))
            {
                File.Delete(filePath + ".tmp");
            }
            //while (this.lastAutoSaveFilePaths.Count > 0)
            //{
            //    // Delete the temporary auto-saved file.
            //    var tempFile = this.lastAutoSaveFilePaths.Dequeue();
            //    var tempFileInfo = new FileInfo(tempFile);
            //    tempFileInfo.Delete();

            //    var anchorsFile = GetAnchorFileName(tempFileInfo.Name);

            //    if (File.Exists(anchorsFile))
            //    {
            //        File.Delete(anchorsFile);
            //    }
            //}

            this.TextDocument.UndoRedoManager.ClearUndoRedo();

            return true;
        }

        protected virtual string GetFormattedSaveString(string text)
        {
            return text;
        }

        protected virtual void SaveToFile(string filePath, Encoding encoding)
        {
            using (var sw = new StreamWriter(filePath, false, encoding))
            {
                sw.Write(this.GetFormattedSaveString(this.TextDocument.TextGet(0, -1, -1)));
            }

            var pinnedTextAnchors = new List<ITextSegmentStyled>();

            foreach (var textAnchor in this.TextDocument.TextSegmentStyledManager.GetStyledTextSegments())
            {
                if (textAnchor.Style.Type == TextStyleType.Pinned)
                {
                    pinnedTextAnchors.Add(textAnchor);
                }
            }

            var textFileInfo = new FileInfo(filePath);

            var anchorTextFile = Path.Combine(
                textFileInfo.Directory.ToString(),
                String.Format("{0}.{1}", textFileInfo.Name.Substring(0, textFileInfo.Name.Length - textFileInfo.Extension.Length), "anchors"));

            if (pinnedTextAnchors.Count > 0)
            {
                using (var sw = new StreamWriter(anchorTextFile, false, Encoding.UTF8))
                {
                    foreach (var textAnchor in pinnedTextAnchors)
                    {
                        sw.WriteLine("{0}|{1}|{2}|{3}", textAnchor.Style.NameKey, textAnchor.IndexGlobal, textAnchor.GetLength(this.CurrentTextColumnIndex), textAnchor.Object);
                    }
                }
            }
            else if (File.Exists(anchorTextFile))
            {
                // All the anchors that were used before have been deleted while the file was worked upon.
                File.Delete(anchorTextFile);
            }
        }

        private void _autoSaveTimer_Tick(object sender, EventArgs e)
        {
            this.SaveAuto();
        }

        public virtual string FilterStringLine(string str)
        {
            return str;
        }

        public virtual void FilterTextBuffer(ITextDocument textBuffer)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (this.KeepAutoSavesOnDispose == false && this.CurrentFilePath != null)
            {
                // Delete the temporary auto-saved file.
                //var tempFile = this.lastAutoSaveFilePaths.Dequeue();
                //var tempFileInfo = new FileInfo(tempFile);
                //tempFileInfo.Delete();

                //var anchorsFile = GetAnchorFileName(tempFileInfo.Name);

                if (File.Exists(this.CurrentFilePath + ".tmp"))
                {
                    File.Delete(this.CurrentFilePath + ".tmp");
                }
            }

            base.Dispose(disposing);

            if (this._autoSaveTimer != null)
            {
                this._autoSaveTimer.Stop();
                this._autoSaveTimer.Tick -= this._autoSaveTimer_Tick;
                this._autoSaveTimer.Dispose();
                this._autoSaveTimer = null;
            }
        }
    }
}