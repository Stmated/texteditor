using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eliason.TextEditor
{
    public class NotifierInputRequest<T>
    {
        public String Title { get; set; }
        public String Message { get; set; }

        /// <summary>
        /// Can this be removed? Seems way too coupled! Can its usage be represented in the request in some other way?
        /// </summary>
        [Obsolete("Figure out a way to get rid of this usage")]
        public String VariableName { get; set; }
        public T DefaultValue { get; set; }
        public Type Type { get; private set; }
        public KeyValuePair<string, string>[] Parameters { get; set; }
        public bool CanCancel { get; set; }

        public NotifierInputRequest()
        {
            this.Type = typeof (T);
        }
    }
}
