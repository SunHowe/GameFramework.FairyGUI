using System;
using UnityEngine;

namespace GameFramework.FairyGUI.Runtime
{
    [CreateAssetMenu(fileName = "FairyGUISettings.asset", menuName = "Game Framework/FairyGUI Settings")]
    public sealed class FairyGUISettings : ScriptableObject
    {
        [Header("FairyGUI资源根路径")]
        public string uiAssetsRoot = "Assets/GameMain/UI";

        [Header("FairyGUI二进制文件后缀")]
        public string uiByteSuffix = "_fui.bytes";

        [Header("FairyGUI界面导出组件的名字")]
        public string uiFormComponentName = "UIForm";

        [Header("FairyGUI包名列表(由工具动态生成)")]
        public string[] uiPackageNames = Array.Empty<string>();
    }
}