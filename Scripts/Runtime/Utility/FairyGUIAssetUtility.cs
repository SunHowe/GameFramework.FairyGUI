using System;
using UnityGameFramework.Runtime;

namespace GameFramework.FairyGUI.Runtime
{
    public static class FairyGUIAssetUtility
    {
        /// <summary>
        /// 获取UI界面的资源路径
        /// </summary>
        public static string GetUIFormAssetPath(string uiFormName)
        {
            var fairyGUIComponent = GameEntry.GetComponent<FairyGUIComponent>();
            if (fairyGUIComponent == null)
                throw new Exception("请先添加FairyGUI组件");
            
            return fairyGUIComponent.GetUIFormAssetPath(uiFormName);
        }
    }
}