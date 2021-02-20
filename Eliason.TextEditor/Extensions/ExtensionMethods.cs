using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eliason.TextEditor.Extensions
{
    public static class ExtensionMethods
    {
        public static Keys StringToKeys(this string str)
        {
            Keys key = Keys.None;
            var strParts = str.Split('+');

            foreach (var part in strParts)
            {
                Keys k;
                if (part.Equals("ctrl", StringComparison.InvariantCultureIgnoreCase))
                {
                    k = Keys.Control;
                }
                else if (part.Equals("ctrl", StringComparison.InvariantCultureIgnoreCase))
                {
                    k = Keys.Alt;
                }
                else if (part.Equals("ctrl", StringComparison.InvariantCultureIgnoreCase))
                {
                    k = Keys.Shift;
                }
                else
                {
                    k = (Keys)Enum.Parse(typeof(Keys), part, true);
                }

                key = (key | k);
            }

            return key;
        }

        /// <summary>
        ///   Prefix pads the string to be the set length, with the specified char until it has reached the required length.
        /// </summary>
        /// <param name = "text">The original string that may require prefixing.</param>
        /// <param name = "c">The character that should be repeated</param>
        /// <param name = "length">The length the string will be forced to have as minumum.</param>
        /// <returns>The formatted string with possible prefixed zeroes.</returns>
        public static string Prefix(this string text, char c, int length)
        {
            if (text.Length < length)
            {
                return new String(c, length - text.Length) + text;
            }

            return text;
        }

        public static string EscapeAsFilepath(this string path)
        {
            var illegalRegex = "[" + Regex.Escape(
                String.Join(
                    string.Empty,
                    new string(Path.GetInvalidPathChars()),
                    new string(Path.GetInvalidFileNameChars()))
                ) + "]";

            return Regex.Replace(path, illegalRegex, "_", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
    }
}
