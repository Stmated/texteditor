using System;

namespace Eliason.TextEditor.TextView
{
    public class SelectionChangedArgs : EventArgs
    {
        public ByInterface By { get; private set; }
        public int TextColumnIndex { get; private set; }

        public SelectionChangedArgs(ByInterface by, int textColumnIndex)
        {
            this.By = by;
            this.TextColumnIndex = textColumnIndex;
        }
    }
}