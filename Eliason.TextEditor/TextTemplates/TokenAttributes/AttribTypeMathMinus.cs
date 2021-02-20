using System.Collections.Generic;

namespace Eliason.TextEditor.TextTemplates.TokenAttributes
{
    public class AttribTypeMathMinus : TokenAttributeTypeBase
    {
        public override string Key
        {
            get { return "-"; }
        }

        public override void Process(TokenAttribute attribute, List<string> currentValues, bool isAltering)
        {
            if (isAltering)
            {
                for (var i = 0; i < currentValues.Count; i++)
                {
                    int currentIntValue;
                    if (int.TryParse(currentValues[i], out currentIntValue) == false)
                    {
                        continue;
                    }

                    int attributeIntValue;
                    if (int.TryParse(attribute.Value, out attributeIntValue) == false)
                    {
                        continue;
                    }

                    currentValues[i] = (currentIntValue - attributeIntValue).ToString();
                }
            }
        }
    }
}