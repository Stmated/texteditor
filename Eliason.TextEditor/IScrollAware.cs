using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eliason.TextEditor
{
    public interface IScrollAware
    {
        IScrollHost ScrollHost { get; }


    }
}
