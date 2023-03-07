using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GameFramework.FairyGUI.Editor
{
    public static class TextUtils
    {
        private static readonly TextInfo TextInfo = new CultureInfo("en-US", false).TextInfo;
        
        public static string TitleCase(this string str)
        {
            return TextInfo.ToTitleCase(str); 
        }

        public static string LowerFirst(this string str)
        {
            return str[..1].ToLower() + str[1..];
        }

        public static string UpperFirst(this string str)
        {
            return str[..1].ToUpper() + str[1..];
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