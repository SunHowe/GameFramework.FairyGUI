using System;
using System.IO;
using GameFramework.FairyGUI.Runtime;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GameFramework.FairyGUI.Editor
{
    public sealed class FairyGUIEditorSettings : ScriptableObject
    {
        [Header("配置列表")]
        public FairyGUIExportSettings[] settings = Array.Empty<FairyGUIExportSettings>();

        private const string Path = "ProjectSettings/" + nameof(FairyGUIEditorSettings) + ".asset";

        public void Save()
        {
            UnityEngine.Object[] obj = { this };
            InternalEditorUtility.SaveToSerializedFileAndForget(obj, Path, true);
        }

        public static FairyGUIEditorSettings Instance
        {
            get
            {
                if (s_Instance != null)
                    return s_Instance;

                GetOrCreateSettings();
                return s_Instance;
            }
        }

        private static FairyGUIEditorSettings s_Instance;

        private FairyGUIEditorSettings()
        {
            if (s_Instance != null)
                return;
            
            s_Instance = this;
        }

        private static void GetOrCreateSettings()
        {
            if (!string.IsNullOrEmpty(Path) && File.Exists(Path))
                InternalEditorUtility.LoadSerializedFileAndForget(Path);

            if (s_Instance != null) 
                return;
            
            var inst = CreateInstance<FairyGUIEditorSettings>();
            inst.hideFlags = HideFlags.HideAndDontSave;
            inst.Save();
        }

        [Serializable]
        public sealed class FairyGUIExportSettings
        {
            public const string StrReplaceKeyPackageName = "{PackageName}";
            
            [Header("运行时配置文件")]
            public FairyGUISettings runtimeSettings;

            [Header("UI窗体代码命名空间(支持占位符{PackageName})")]
            public string uiFormCodeNamespace = "Game.Hotfix.UI.{PackageName}.Form";

            [Header("UI窗体代码导出根目录(支持占位符{PackageName})")]
            public string uiFormCodeExportRoot = "Assets/GameMain/Scripts/Game.Hotfix.UI/{PackageName}/Form";

            [Header("UI窗体基类")]
            public string uiFormBaseTypeName = "GameFramework.FairyGUI.Runtime.FairyGUIFormLogic";

            [Header("UI窗体视图实例属性名")]
            public string uiFormContentPanePropertyName = "ContentPane";

            [Header("UI组件命名正则")]
            public string uiComponentNameRegex = "Component$";

            [Header("UI组件代码命名空间(支持占位符{PackageName})")]
            public string uiComponentCodeNamespace = "Game.Hotfix.UI.{PackageName}.Component";

            [Header("UI组件代码导出根目录(支持占位符{PackageName})")]
            public string uiComponentCodeExportRoot = "Assets/GameMain/Scripts/Game.Hotfix.UI/{PackageName}/Component";

            [Header("UI绑定代码文件后缀")]
            public string uiBindingCodeFileSuffix = ".Bindings";

            [Header("UI绑定代码函数名")]
            public string uiBidingMethodName = "InitBindings";
            
            [Header("是否忽略默认名字的子节点(n+数字)")]
            public bool ignoreDefaultNameChildren = true;

            [Header("是否生成控制器对应的枚举")]
            public bool generateControllerEnum = true;
        }
    }
}