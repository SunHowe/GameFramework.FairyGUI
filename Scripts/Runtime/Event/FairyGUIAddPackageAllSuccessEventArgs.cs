using GameFramework.Event;

namespace GameFramework.FairyGUI.Runtime
{
    public sealed class FairyGUIAddPackageAllSuccessEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(FairyGUIAddPackageAllSuccessEventArgs).GetHashCode();
        public override int Id => EventId;

        public override void Clear()
        {
        }

        public static FairyGUIAddPackageAllSuccessEventArgs Create()
        {
            var args = ReferencePool.Acquire<FairyGUIAddPackageAllSuccessEventArgs>();
            return args;
        }
    }
}