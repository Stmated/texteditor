using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eliason.TextEditor
{
    public class NotifierSelectRequest<T>
    {
        public String Title { get; set; }
        public String Message { get; set; }

        public T[] Options { get; set; }
        public T DefaultValue { get; set; }

        public bool MultiSelect { get; set; }

        public Type Type { get; private set; }
        public KeyValuePair<string, string>[] Parameters { get; set; }

        public bool CanCancel { get; set; }

        public Func<T, String> ToDisplayValueFunction { get; set; }

        public NotifierSelectRequest()
        {
            this.Type = typeof (T);
        }
    }
}
