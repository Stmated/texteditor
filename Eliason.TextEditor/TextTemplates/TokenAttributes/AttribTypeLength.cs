using System;
using System.Collections.Generic;
using Eliason.TextEditor.Extensions;

namespace Eliason.TextEditor.TextTemplates.TokenAttributes
{
    public class AttribTypeLength : TokenAttributeTypeBase
    {
        public override string Key
        {
            get { return "L"; }
        }

        public override void Process(TokenAttribute attribute, List<string> currentValues, bool isAltering)
        {
            if (isAltering)
            {
                for (var i = 0; i < currentValues.Count; i++)
                {
                    double number;
                    var isNumber = double.TryParse(currentValues[i], out number);

                    currentValues[i] = currentValues[i].Prefix((isNumber ? '0' : ' '), Convert.ToInt32(attribute.Value));
                }
            }
        }
    }
}