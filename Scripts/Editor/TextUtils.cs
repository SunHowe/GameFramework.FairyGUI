using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GameFramework.FairyGUI.Editor
{
    public static class TextUtils
    {
        private static readonly StringBuilder _sbFormatName = new StringBuilder();

        public static string UppercaseFirst(this string name)
        {
            return FormatCamelName(name, false);
        }

        public static string LowercaseFirst(this string name)
        {
            return FormatCamelName(name, true);
        }

        public static string ToTitleCase(this string input)
        {
            _sbFormatName.Clear();
            for (var index = 0; index < input.Length; ++index)
            {
                var ch = input[index];
                if (ch == '_' && index + 1 < input.Length)
                {
                    var upper = input[index + 1];
                    if (char.IsLower(upper))
                        upper = char.ToUpper(upper, CultureInfo.InvariantCulture);
                    _sbFormatName.Append(upper);
                    ++index;
                }
                else
                    _sbFormatName.Append(ch);
            }

            return _sbFormatName.ToString();
        }

        private static string FormatCamelName(this string name, bool lowerCaseFirstChar)
        {
            var upperCase = !lowerCaseFirstChar;

            _sbFormatName.Clear();
            foreach (var t in name)
            {
                if (t == '_')
                {
                    upperCase = true;
                    continue;
                }

                if (lowerCaseFirstChar)
                {
                    _sbFormatName.Append(char.ToLower(t));
                    lowerCaseFirstChar = false;
                    continue;
                }

                if (upperCase)
                {
                    _sbFormatName.Append(char.ToUpper(t));
                    upperCase = false;
                    continue;
                }

                _sbFormatName.Append(t);
            }

            return _sbFormatName.ToString();
        }

        public static string RemoveLines(this string str, int removeCount)
        {
            if (removeCount == 0)
                return str;

            var lines = str.Split('\n').ToList();
            lines.RemoveRange(0, removeCount);
            lines.RemoveAll(item => string.IsNullOrEmpty(item.Trim()));
            return lines.Contact("\n");
        }

        public static string Contact(this IEnumerable<string> strings, string separator)
        {
            var stringBuilder = new StringBuilder();

            var first = true;
            foreach (var str in strings)
            {
                if (!first)
                    stringBuilder.Append(separator);
                first = false;

                stringBuilder.Append(str);
            }

            return stringBuilder.ToString();
        }
    }
}