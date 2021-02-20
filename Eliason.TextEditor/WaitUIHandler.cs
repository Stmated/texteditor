using System;
using System.Windows.Forms;

namespace Eliason.TextEditor
{
    public class WaitUIHandler : IDisposable
    {
        private readonly Control control;
        private readonly Cursor previousCursor;

        public WaitUIHandler(Control c)
        {
            this.control = c;

            this.previousCursor = this.control.Cursor;
            this.control.Cursor = Cursors.WaitCursor;
        }

        public void Dispose()
        {
            this.control.Cursor = this.previousCursor;
        }
    }
}