using FairyGUI;

namespace GameFramework.FairyGUI.Runtime
{
    public abstract class FairyGUIFormLogic : IUIFormLogic
    {
        protected FairyGUIForm UIForm { get; private set; }
        protected GComponent ContentPane { get; private set; }
        
        public virtual void OnInit(FairyGUIForm uiForm, object userData)
        {
            UIForm = uiForm;
            ContentPane = (GComponent)uiForm.Handle;
        }

        public virtual void OnRecycle()
        {
        }

        public virtual void OnOpen(object userData)
        {
        }

        public virtual void OnClose(bool isShutdown, object userData)
        {
        }

        public virtual void OnPause()
        {
        }

        public virtual void OnResume()
        {
        }

        public virtual void OnCover()
        {
        }

        public virtual void OnReveal()
        {
        }

        public virtual void OnRefocus(object userData)
        {
        }

        public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        public virtual void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
        }
    }
}