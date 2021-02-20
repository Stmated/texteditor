using System.Collections.Generic;

namespace Eliason.TextEditor.TextTemplates
{
    public class TokenAttribute
    {
        public string Value { get; private set; }
        public Token Token { get; private set; }
        public TokenAttributeTypeBase Type { get; private set; }
        public object Result { get; set; }

        public TokenAttribute(Token token, TokenAttributeTypeBase type, string value)
        {
            this.Token = token;
            this.Value = value;
            this.Type = type;
        }

        public void Process(List<string> currentValues, bool isAltering)
        {
            this.Type.Process(this, currentValues, isAltering);
        }

        public override string ToString()
        {
            return "Attribute: " + this.Type.Key + ": " + this.Value;
        }
    }
}