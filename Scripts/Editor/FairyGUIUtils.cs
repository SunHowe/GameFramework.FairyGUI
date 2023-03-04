using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameFramework.FairyGUI.Runtime;

namespace GameFramework.FairyGUI.Editor
{
    public static class FairyGUIUtils
    {
        /// <summary>
        /// 获取UIPackage文件名列表
        /// </summary>
        public static List<string> GetUIPackageFileNames(FairyGUISettings settings)
        {
            return (from file in Directory.GetFiles(settings.uiAssetsRoot)
                    where file.EndsWith(settings.uiByteSuffix)
                    select Path.GetFileName(file)
                    into fileName
                    select fileName[..^settings.uiByteSuffix.Length])
                .ToList();
        }
    }
}