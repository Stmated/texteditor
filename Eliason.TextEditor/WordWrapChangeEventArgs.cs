using System;

namespace Eliason.TextEditor
{
    public class WordWrapChangeEventArgs : EventArgs
    {
        public bool Enable { get; private set; }
        public bool Cancel { get; set; }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "WordWrapChangeEventArgs" /> class.
        /// </summary>
        public WordWrapChangeEventArgs(bool enable)
        {
            this.Enable = enable;
        }
    }
}