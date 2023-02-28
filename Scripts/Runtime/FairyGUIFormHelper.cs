using System;
using FairyGUI;
using FairyGUI.Utils;
using GameFramework.Resource;
using GameFramework.UI;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameFramework.FairyGUI.Runtime
{
    public sealed class FairyGUIFormHelper : UIFormHelperBase
    {
        private ResourceComponent m_ResourceComponent = null;
        private FairyGUIComponent m_FairyGUIComponent = null;

        public override object InstantiateUIForm(object uiFormAsset)
        {
            var buffer = new ByteBuffer(((TextAsset)uiFormAsset).bytes);
            buffer.ReadUint();
            buffer.version = buffer.ReadInt();
            buffer.ReadBool(); //compressed
            var packageId = buffer.ReadString();
            var packageName = buffer.ReadString();
            buffer.position = 0;
            
            var package = UIPackage.AddPackage(buffer, m_FairyGUIComponent.GetPackageAssetPrefixPath(packageName), LoadFunc);
            return package.CreateObject(m_FairyGUIComponent.UIMainComponentName).asCom;
        }

        public override IUIForm CreateUIForm(object uiFormInstance, IUIGroup uiGroup, object userData)
        {
            var component = (GComponent)uiFormInstance;
            var logicType = m_FairyGUIComponent.GetUIFormLogicType(component.packageItem.owner.name);
            return new FairyGUIForm(component, (IUIFormLogic) Activator.CreateInstance(logicType));
        }

        public override void ReleaseUIForm(object uiFormAsset, object uiFormInstance)
        {
            m_ResourceComponent.UnloadAsset(uiFormAsset);
        }

        private void Start()
        {
            m_ResourceComponent = GameEntry.GetComponent<ResourceComponent>();
            m_FairyGUIComponent = GameEntry.GetComponent<FairyGUIComponent>();
            
            NAudioClip.CustomDestroyMethod = CustomDestroyMethod;
            NTexture.CustomDestroyMethod += CustomDestroyMethod;
        }

        private void LoadFunc(string assetName, string extension, System.Type assetType, PackageItem item)
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
            m_ResourceComponent.UnloadAsset(asset);
        }

        private void CustomDestroyMethod(AudioClip asset)
        {
            m_ResourceComponent.UnloadAsset(asset);
        }
    }
}