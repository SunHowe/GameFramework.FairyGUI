using FairyGUI.Utils;

namespace GameFramework.FairyGUI.Runtime
{
    public static class ByteBufferUtility
    {
        /// <summary>
        /// 获取包名
        /// </summary>
        public static string GetPackageName(ByteBuffer buffer)
        {
            buffer.ReadUint();
            buffer.version = buffer.ReadInt();
            buffer.ReadBool(); //compressed
            var packageId = buffer.ReadString();
            var packageName = buffer.ReadString();
            buffer.position = 0;

            return packageName;
        }
    }
}