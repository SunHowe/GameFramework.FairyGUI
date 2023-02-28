using GameFramework.Event;

namespace GameFramework.FairyGUI.Runtime
{
    /// <summary>
    /// FairyGUI 添加包失败事件。
    /// </summary>
    public sealed class FairyGUIAddPackageFailureEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(FairyGUIAddPackageFailureEventArgs).GetHashCode();
        public override int Id => EventId;
        
        public string packageName { get; private set; }

        public override void Clear()
        {
            packageName = string.Empty;
        }

        public static FairyGUIAddPackageFailureEventArgs Create(string packageName)
        {
            var fairyGUIAddPackageFailureEventArgs = ReferencePool.Acquire<FairyGUIAddPackageFailureEventArgs>();
            fairyGUIAddPackageFailureEventArgs.packageName = packageName;
            return fairyGUIAddPackageFailureEventArgs;
        }
    }
}