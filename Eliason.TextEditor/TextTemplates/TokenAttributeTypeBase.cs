using System.Collections.Generic;

namespace Eliason.TextEditor.TextTemplates
{
    public abstract class TokenAttributeTypeBase
    {
        public TokenAttribute GetTokenAttribute(Token token, TokenAttributeTypeBase type, string value)
        {
            return new TokenAttribute(token, type, value);
        }

        public abstract string Key { get; }

        public abstract void Process(TokenAttribute attribute, List<string> currentValues, bool isAltering);
    }
}