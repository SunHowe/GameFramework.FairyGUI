using System;
using UnityEngine;

namespace GameFramework.FairyGUI.Runtime
{
    [CreateAssetMenu(fileName = "FairyGUISettings.asset", menuName = "Game Framework/FairyGUI Settings")]
    public sealed class FairyGUISettings : ScriptableObject
    {
        [Header("FairyGUI资源根路径")]
        [SerializeField]
        private string m_UIAssetsRoot = "Assets/GameMain/UI";

        [Header("FairyGUI二进制文件后缀")]
        [SerializeField]
        private string m_UIByteSuffix = "_fui.bytes";

        [Header("FairyGUI界面导出组件的名字")]
        [SerializeField]
        private string m_UIFormComponentName = "UIForm";

        [Header("FairyGUI包名列表(由工具动态生成)")]
        [SerializeField]
        private string[] m_UIPackageNames = Array.Empty<string>();
        
        /// <summary>
        /// FairyGUI资源根路径
        /// </summary>
        public string UIAssetsRoot => m_UIAssetsRoot;
        
        /// <summary>
        /// FairyGUI二进制文件后缀
        /// </summary>
        public string UIByteSuffix => m_UIByteSuffix;
        
        /// <summary>
        /// FairyGUI界面导出组件的名字
        /// </summary>
        public string UIFormComponentName => m_UIFormComponentName;
        
        /// <summary>
        /// FairyGUI包名列表
        /// </summary>
        public string[] UIPackageNames => m_UIPackageNames;
    }
}