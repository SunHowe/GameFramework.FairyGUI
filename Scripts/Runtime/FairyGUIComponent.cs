using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameFramework.FairyGUI.Runtime
{
    /// <summary>
    /// FairyGUI组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/FairyGUI")]
    public class FairyGUIComponent : GameFrameworkComponent
    {
        [SerializeField]
        private string m_UIAssetsRoot = "Assets/GameMain/UI";

        [SerializeField]
        private string m_UIByteSuffix = "_fui.bytes";

        [SerializeField]
        private string m_UIMainComponentName = "Main";

        private Dictionary<string, Type> m_LogicTypeDict = new Dictionary<string, Type>();

        /// <summary>
        /// 界面主要导出组件的名字
        /// </summary>
        public string UIMainComponentName => m_UIMainComponentName;
        
        /// <summary>
        /// 通过包名获取包资源前缀
        /// </summary>
        public string GetPackageAssetPrefixPath(string packageName)
        {
            return Utility.Text.Format("{0}/{1}", m_UIAssetsRoot, packageName);
        }

        /// <summary>
        /// 获取UI界面二进制文件加载路径
        /// </summary>
        public string GetUIFormByteAssetPath(string uiFormName)
        {
            return Utility.Text.Format("{0}/{1}{2}", m_UIAssetsRoot, uiFormName, m_UIByteSuffix);
        }

        /// <summary>
        /// 查找UI界面逻辑实例
        /// </summary>
        public Type GetUIFormLogicType(string uiFormName)
        {
            return m_LogicTypeDict.TryGetValue(uiFormName, out var logicType) ? logicType : null;
        }

        /// <summary>
        /// 注册UI界面逻辑实例
        /// </summary>
        public void RegisterUIFormLogicType(string uiFormName, Type type)
        {
            m_LogicTypeDict[uiFormName] = type;
        }
    }
}