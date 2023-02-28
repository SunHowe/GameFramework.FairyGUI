using System;
using System.Collections.Generic;
using FairyGUI;
using FairyGUI.Utils;
using GameFramework.Resource;
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
        private bool m_EnableAddPackageAllSuccessEvent = true;

        [SerializeField]
        private bool m_EnableAddPackageFailureEvent = true;
        
        [SerializeField]
        private FairyGUISettings m_StaticFairyGUISettings = null;

        /// <summary>
        /// 更新FairyGUI组件的设置
        /// </summary>
        public void UpdateSettings(FairyGUISettings settings)
        {
            m_FairyGUISettings = settings;
        }

        /// <summary>
        /// 加载FairyGUI包
        /// </summary>
        public void AddUIPackages()
        {
            if (m_FairyGUISettings == null)
                throw new Exception("请先调用InitSettings方法初始化FairyGUI组件的设置");

            if (m_LoadingBinaryCount > 0 || m_LoadBinarySuccessCount > 0)
                return;

            if (m_FairyGUISettings.UIPackageNames == null || m_FairyGUISettings.UIPackageNames.Length == 0)
            {
                OnLoadAllPackagesSuccess();
                return;
            }

            m_LoadingBinaryCount = m_FairyGUISettings.UIPackageNames.Length;
            m_LoadBinarySuccessCount = 0;
            m_LoadBinaryFailureCount = 0;

            m_LoadBinaryCallbacks ??= new LoadBinaryCallbacks(LoadBinarySuccessCallback, LoadBinaryFailureCallback);

            foreach (var packageName in m_FairyGUISettings.UIPackageNames)
                m_ResourceComponent.LoadBinary(GetUIPackageAssetPath(packageName), m_LoadBinaryCallbacks, packageName);
        }

        /// <summary>
        /// 卸载FairyGUI包
        /// </summary>
        public void RemoveUIPackages()
        {
            if (m_FairyGUISettings == null)
                throw new Exception("请先调用InitSettings方法初始化FairyGUI组件的设置");

            m_LoadingBinaryCount = 0;
            m_LoadBinarySuccessCount = 0;
            m_LoadBinaryFailureCount = 0;

            UIPackage.RemoveAllPackages();
        }

        /// <summary>
        /// 实例化FairyGUI界面
        /// </summary>
        public GComponent InstantiateUIForm(object asset)
        {
            if (m_FairyGUISettings == null)
                throw new Exception("请先调用InitSettings方法初始化FairyGUI组件的设置");

            var buffer = new ByteBuffer(((TextAsset)asset).bytes);
            var packageName = ByteBufferUtility.GetPackageName(buffer);

            var gObject = UIPackage.GetByName(packageName).CreateObject(m_FairyGUISettings.UIFormComponentName);
            if (gObject == null)
                return null;
            
            gObject.name = packageName;
            gObject.displayObject.gameObject.name = packageName;
            
            return gObject.asCom;
        }

        /// <summary>
        /// 获取UI界面的资源路径
        /// </summary>
        public string GetUIFormAssetPath(string uiFormName)
        {
            if (m_FairyGUISettings == null)
                throw new Exception("请先调用InitSettings方法初始化FairyGUI组件的设置");

            return GetUIPackageAssetPath(uiFormName);
        }

        #region [UIForm Logic]

        /// <summary>
        /// 查找UI界面逻辑实例
        /// </summary>
        public Type GetUIFormLogicType(string uiFormName)
        {
            return m_LogicTypeDict.TryGetValue(uiFormName, out var logicType) ? logicType : null;
        }

        /// <summary>
        /// 注册UI界面逻辑类型
        /// </summary>
        public void RegisterUIFormLogicType(string uiFormName, Type type)
        {
            m_LogicTypeDict[uiFormName] = type;
        }

        /// <summary>
        /// 取消注册UI界面逻辑类型
        /// </summary>
        public void UnregisterUIFormLogicType(string uiFormName)
        {
            m_LogicTypeDict.Remove(uiFormName);
        }

        /// <summary>
        /// 取消注册所有UI界面逻辑类型
        /// </summary>
        public void UnregisterAllUIFormLogicType()
        {
            m_LogicTypeDict.Clear();
        }

        #endregion

        #region [内部实现]

        #region [UIPackage]

        /// <summary>
        /// 二进制文件加载失败回调
        /// </summary>
        private void LoadBinaryFailureCallback(string assetName, LoadResourceStatus status, string errorMessage, object userdata)
        {
            if (m_LoadingBinaryCount == 0)
                return;

            --m_LoadingBinaryCount;
            ++m_LoadBinaryFailureCount;

            if (m_EnableAddPackageFailureEvent)
                m_EventComponent.Fire(this, FairyGUIAddPackageFailureEventArgs.Create((string)userdata));
        }

        /// <summary>
        /// 二进制文件加载成功回调
        /// </summary>
        private void LoadBinarySuccessCallback(string assetName, byte[] binaryBytes, float duration, object userdata)
        {
            if (m_LoadingBinaryCount == 0)
                return;

            --m_LoadingBinaryCount;
            ++m_LoadBinarySuccessCount;

            var packageName = (string)userdata;
            // 如果已经加载过了，就不再加载了
            if (UIPackage.GetByName(packageName) == null)
                UIPackage.AddPackage(binaryBytes, GetPackageAssetPrefixPath(packageName), LoadFunc);

            if (m_LoadingBinaryCount > 0)
                return;

            if (m_LoadBinaryFailureCount > 0)
                return;

            OnLoadAllPackagesSuccess();
        }

        /// <summary>
        /// 全部加载完成
        /// </summary>
        private void OnLoadAllPackagesSuccess()
        {
            if (m_EnableAddPackageAllSuccessEvent)
                m_EventComponent.Fire(this, FairyGUIAddPackageAllSuccessEventArgs.Create());
        }

        /// <summary>
        /// 通过包名获取包资源前缀
        /// </summary>
        private string GetPackageAssetPrefixPath(string packageName)
        {
            if (m_FairyGUISettings == null)
                throw new Exception("请先调用InitSettings方法初始化FairyGUI组件的设置");

            return Utility.Text.Format("{0}/{1}", m_FairyGUISettings.UIAssetsRoot, packageName);
        }

        /// <summary>
        /// 获取UI界面二进制文件加载路径
        /// </summary>
        private string GetUIPackageAssetPath(string packageName)
        {
            if (m_FairyGUISettings == null)
                throw new Exception("请先调用InitSettings方法初始化FairyGUI组件的设置");

            return Utility.Text.Format("{0}/{1}{2}", m_FairyGUISettings.UIAssetsRoot, packageName, m_FairyGUISettings.UIByteSuffix);
        }

        #endregion

        #region [Asset]

        private void LoadFunc(string assetName, string extension, Type assetType, PackageItem item)
        {
            var assetPath = Utility.Text.Format("{0}{1}", assetName, extension);

            void LoadAssetSuccessCallback(string _, object asset, float duration, object userdata)
            {
                item.owner.SetItemAsset(item, asset, DestroyMethod.Custom);
            }

            var callbacks = new LoadAssetCallbacks(LoadAssetSuccessCallback);

            m_ResourceComponent.LoadAsset(assetPath, assetType, callbacks);
        }

        private void CustomDestroyMethod(Texture asset)
        {
            if (asset == null)
                return;

            m_ResourceComponent.UnloadAsset(asset);
        }

        private void CustomDestroyMethod(AudioClip asset)
        {
            if (asset == null)
                return;

            m_ResourceComponent.UnloadAsset(asset);
        }

        #endregion

        #endregion

        private void Start()
        {
            m_FairyGUISettings = m_StaticFairyGUISettings;
            
            m_ResourceComponent = GameEntry.GetComponent<ResourceComponent>();
            m_EventComponent = GameEntry.GetComponent<EventComponent>();

            NAudioClip.CustomDestroyMethod = CustomDestroyMethod;
            NTexture.CustomDestroyMethod += CustomDestroyMethod;
        }

        private FairyGUISettings m_FairyGUISettings;
        private ResourceComponent m_ResourceComponent;
        private EventComponent m_EventComponent;

        private LoadBinaryCallbacks m_LoadBinaryCallbacks;

        private int m_LoadingBinaryCount;
        private int m_LoadBinarySuccessCount;
        private int m_LoadBinaryFailureCount;

        private readonly Dictionary<string, Type> m_LogicTypeDict = new Dictionary<string, Type>();
    }
}