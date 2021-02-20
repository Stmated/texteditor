using System;
using System.Collections.Generic;

namespace Eliason.TextEditor.TextTemplates.TokenAttributes
{
    public class AttribTypeDefault : TokenAttributeTypeBase
    {
        public override string Key
        {
            get { return "D"; }
        }

        public override void Process(TokenAttribute attribute, List<string> currentValues, bool isAltering)
        {
            if (isAltering)
            {
                if (currentValues.Count == 0)
                {
                    // If it has no current values, then we add the dafault value.
                    currentValues.Add(attribute.Value);
                    return;
                }

                for (var i = 0; i < currentValues.Count; i++)
                {
                    currentValues[i] = String.IsNullOrEmpty(currentValues[i]) ? attribute.Value : currentValues[i];
                }
            }
        }
    }
}