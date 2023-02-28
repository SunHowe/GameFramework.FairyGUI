using System;
using FairyGUI;
using GameFramework.UI;
using UnityGameFramework.Runtime;

namespace GameFramework.FairyGUI.Runtime
{
    public sealed class FairyGUIFormHelper : UIFormHelperBase
    {
        private ResourceComponent m_ResourceComponent = null;
        private FairyGUIComponent m_FairyGUIComponent = null;

        public override object InstantiateUIForm(object uiFormAsset)
        {
            return m_FairyGUIComponent.InstantiateUIForm(uiFormAsset);
        }

        public override IUIForm CreateUIForm(object uiFormInstance, IUIGroup uiGroup, object userData)
        {
            var component = (GComponent)uiFormInstance;
            var logicType = m_FairyGUIComponent.GetUIFormLogicType(component.packageItem.owner.name);
            return new FairyGUIForm(component, (IUIFormLogic) Activator.CreateInstance(logicType));
        }

        public override void ReleaseUIForm(object uiFormAsset, object uiFormInstance)
        {
            if (uiFormInstance is GComponent component && !component.isDisposed)
                component.Dispose();
            
            m_ResourceComponent.UnloadAsset(uiFormAsset);
        }

        private void Start()
        {
            m_ResourceComponent = GameEntry.GetComponent<ResourceComponent>();
            m_FairyGUIComponent = GameEntry.GetComponent<FairyGUIComponent>();
        }
    }
}