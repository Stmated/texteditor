using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eliason.TextEditor
{
    public class NotifierInputResponse<T>
    {
        public bool Cancelled { get; set; }
        public T Result { get; set; }
    }
}
