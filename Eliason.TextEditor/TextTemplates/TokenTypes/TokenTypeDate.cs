using System;
using System.Collections.Generic;

namespace Eliason.TextEditor.TextTemplates.TokenTypes
{
    public class TokenTypeDate : TokenTypeBase
    {
        public override string Key
        {
            get { return "Date"; }
        }

        public override IEnumerable<string> Process(string foundAs, Token token)
        {
            yield return DateTime.Now.ToShortDateString();
        }
    }
}