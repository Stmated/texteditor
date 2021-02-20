using System;
using System.Collections.Generic;

namespace Eliason.TextEditor.TextTemplates.TokenTypes
{
    public class TokenTypeFile : TokenTypeBase
    {
        public override string Key
        {
            get { return "File"; }
        }

        public override IEnumerable<string> Process(string foundAs, Token token)
        {
            yield return token.TextView.CurrentFilePath ?? String.Empty;
        }
    }
}