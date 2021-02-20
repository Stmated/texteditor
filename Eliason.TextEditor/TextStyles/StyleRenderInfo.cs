using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Eliason.TextEditor.TextStyles
{
    public class StyleRenderInfo
    {
        public ITextSegmentStyled SelectedTextSegment { get; private set; }

        private readonly Dictionary<String, Object> _variables = new Dictionary<string, object>(); 

        public StyleRenderInfo(ITextSegmentStyled selectedSegmentStyled)
        {
            this.SelectedTextSegment = selectedSegmentStyled;
        }

        public T Get<T>(String key)
        {
            if (this._variables.ContainsKey(key) == false)
            {
                return default(T);
            }

            return (T) this._variables[key];
        }

        public void Set<T>(String key, T value)
        {
            if (this._variables.ContainsKey(key))
            {
                this._variables.Remove(key);
            }

            this._variables.Add(key, value);
        }
    }
}
