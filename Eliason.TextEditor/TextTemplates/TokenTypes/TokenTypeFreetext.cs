using System.Collections.Generic;

namespace Eliason.TextEditor.TextTemplates.TokenTypes
{
    public class TokenTypeFreetext : TokenTypeBase
    {
        public override string Key
        {
            get { return "[Freetext]"; }
        }

        public override bool IsFreetext
        {
            get { return true; }
        }

        public override IEnumerable<string> Process(string foundAs, Token token)
        {
            foreach (var ta in token.Attributes)
            {
                yield return ta.Value;
            }
        }
    }
}