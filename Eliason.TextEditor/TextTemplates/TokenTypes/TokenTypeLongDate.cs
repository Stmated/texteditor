using System;
using System.Collections.Generic;

namespace Eliason.TextEditor.TextTemplates.TokenTypes
{
    public class TokenTypeLongDate : TokenTypeBase
    {
        public override string Key
        {
            get { return "LongDate"; }
        }

        public override IEnumerable<string> Process(string foundAs, Token token)
        {
            yield return DateTime.Now.ToLongDateString();
        }
    }
}