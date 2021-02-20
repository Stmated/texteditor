using System.Collections.Generic;

namespace Eliason.TextEditor.TextTemplates.TokenAttributes
{
    public class AttribTypeSeparator : TokenAttributeTypeBase
    {
        public override string Key
        {
            get { return "S"; }
        }

        public override void Process(TokenAttribute attribute, List<string> currentValues, bool isAltering)
        {
            if (isAltering == false)
            {
                attribute.Token.ItemSeparator = attribute.Value;
            }
        }
    }
}