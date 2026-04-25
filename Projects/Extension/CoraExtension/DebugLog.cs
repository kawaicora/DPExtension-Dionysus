using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Extension.CoraExtension
{
    public static unsafe class DebugLog
    {
        // 函数地址
        private static readonly IntPtr DebugLogAddr = new IntPtr(0x4A4AC0); 

        /// <summary>
        /// 调用 Debug_Log，message 为 UTF8 字符串
        /// </summary>
        public static void Log(string message)
        {
            if (string.IsNullOrEmpty(message))
                message = "";

            byte[] utf8Bytes = Encoding.UTF8.GetBytes(message + "\0");
            fixed (byte* pMessage = utf8Bytes)
            {
                var func = (delegate* unmanaged[Cdecl]<IntPtr, void>)DebugLogAddr.ToPointer();
                func((IntPtr)pMessage);
            }
        }

        /// <summary>
        /// 支持格式化输出
        /// </summary>
        public static void Log(string format, params object[] args)
        {
            Log(string.Format(format, args));
        }
        public static void Logln(string format, params object[] args)
        {
            Log(string.Format(format, args) + "\n");
        }
    }
}