using System;
using System.Collections.Generic;

namespace Eliason.TextEditor.TextTemplates.TokenAttributes
{
    public class AttribTypeOrder : TokenAttributeTypeBase
    {
        public override string Key
        {
            get { return "O"; }
        }

        public override void Process(TokenAttribute attribute, List<string> currentValues, bool isAltering)
        {
            if (isAltering)
            {
                var sortOption = String.IsNullOrEmpty(attribute.Value) ? 'A' : attribute.Value[0];

                switch (sortOption)
                {
                    case 'A':
                        currentValues.Sort(StringComparer.OrdinalIgnoreCase);
                        currentValues.Reverse();
                        break;
                    case 'D':
                        currentValues.Sort(StringComparer.OrdinalIgnoreCase);
                        break;
                }
            }
        }
    }
}