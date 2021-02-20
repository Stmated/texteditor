using System;
using System.Collections.Generic;

namespace Eliason.TextEditor.TextTemplates.TokenAttributes
{
    public class AttribTypeQuantity : TokenAttributeTypeBase
    {
        public override string Key
        {
            get { return "Q"; }
        }

        public override void Process(TokenAttribute attribute, List<string> currentValues, bool isAltering)
        {
            if (isAltering)
            {
                var maxQuantity = Convert.ToInt32(attribute.Value);
                while (currentValues.Count > maxQuantity)
                {
                    currentValues.RemoveAt(maxQuantity);
                }
            }
        }
    }
}