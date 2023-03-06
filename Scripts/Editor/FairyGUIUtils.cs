using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameFramework.FairyGUI.Editor
{
    public static class FairyGUIUtils
    {
        /// <summary>
        /// 获取UIPackage文件名列表
        /// </summary>
        public static List<string> GetUIPackageFileNames(string uiAssetsRoot, string uiByteSuffix)
        {
            return (from file in Directory.GetFiles(uiAssetsRoot)
                    where file.EndsWith(uiByteSuffix)
                    select Path.GetFileName(file)
                    into fileName
                    select fileName[..^uiByteSuffix.Length])
                .ToList();
        }
    }
}