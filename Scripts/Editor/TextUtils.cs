using System.Globalization;

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
    }
}