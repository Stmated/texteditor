using System;
using System.Collections.Generic;
using System.Linq;

namespace Eliason.TextEditor.TextTemplates
{
    public class Token : IComparable
    {
        public TokenTypeBase Type { get; private set; }
        public TokenAttribute[] Attributes { get; set; }
        public ITextView TextView { get; private set; }
        public string ItemSeparator { get; set; }
        public string FoundAs { get; set; }

        public Token(ITextView textView, TokenTypeBase tokenType, string foundAs)
        {
            this.TextView = textView;
            this.Type = tokenType;
            this.ItemSeparator = Strings.Native_ItemSeparator;
            this.FoundAs = foundAs;
        }

        public string Process()
        {
            foreach (var attribute in this.Attributes)
            {
                attribute.Process(null, false);
            }

            var strings = new List<string>();

            foreach (var stringValue in this.Type.Process(this.FoundAs, this))
            {
                if (strings.Contains(stringValue) == false)
                {
                    strings.Add(stringValue);
                }
            }

            foreach (var attribute in this.Attributes)
            {
                attribute.Process(strings, true);
            }

            return String.Join(this.ItemSeparator, strings.ToArray()).Replace("\r\n", "\n");
        }

        public int CompareTo(object y)
        {
            var other = y as Token;

            var result = this.Type.Key.CompareTo(other.Type.Key);

            if (result == 0)
            {
                result = this.Attributes.Length.CompareTo(other.Attributes.Length);
            }

            if (result == 0)
            {
                result = this.TextView.CurrentFilePath.CompareTo(other.TextView.CurrentFilePath);
            }

            if (result == 0)
            {
                result = this.FoundAs.CompareTo(other.FoundAs);
            }

            return result;
        }

        public override string ToString()
        {
            return this.Type.Key + " " + String.Join(this.ItemSeparator, this.Attributes.Select(k => k.Value).ToArray());
        }
    }
}