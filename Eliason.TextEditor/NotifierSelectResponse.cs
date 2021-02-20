using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eliason.TextEditor
{
    public class NotifierSelectResponse<T>
    {
        public bool Cancelled { get; set; }
        public T Result { get; set; }
        public int ResultIndex { get; set; }

        public bool HasResult
        {
            get { return this.Result != null || this.ResultIndex != -1; }
        }
    }
}
