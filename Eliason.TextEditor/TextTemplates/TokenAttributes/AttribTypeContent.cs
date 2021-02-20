using System.Collections.Generic;

namespace Eliason.TextEditor.TextTemplates.TokenAttributes
{
    public class AttribTypeContent : TokenAttributeTypeBase
    {
        public override string Key
        {
            get { return "[Content]"; }
        }

        public override void Process(TokenAttribute attribute, List<string> currentValues, bool isAltering)
        {
        }
    }
}