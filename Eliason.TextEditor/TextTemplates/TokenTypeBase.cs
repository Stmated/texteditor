using System.Collections.Generic;

namespace Eliason.TextEditor.TextTemplates
{
    public abstract class TokenTypeBase
    {
        public abstract string Key { get; }

        public virtual bool IsDynamic
        {
            get { return false; }
        }

        public virtual bool IsFreetext
        {
            get { return false; }
        }

        public abstract IEnumerable<string> Process(string foundAs, Token token);
    }
}