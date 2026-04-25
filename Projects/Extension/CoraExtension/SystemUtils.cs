using System;
using System.Runtime.InteropServices;

namespace Extension.CoraExtension
{
    
    class SystemUtils
    {
        // ==================== DrawTextW (Unicode) ====================
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int DrawTextW(
            IntPtr hdc,
            string lpchText,
            int cchText,
            ref RECT lprc,
            uint uFormat);

        // ==================== DrawTextA (ANSI) ====================
        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern int DrawTextA(
            IntPtr hdc,
            string lpchText,
            int cchText,
            ref RECT lprc,
            uint uFormat);

        public struct RECT { public int Left, Top, Right, Bottom; }

        public static ushort ToRgb565(byte r, byte g, byte b)
        {
            // 取紅色的前5位，綠色的前6位，藍色的前5位
            return (ushort)(((r & 0xF8) << 8) | ((g & 0xFC) << 3) | (b >> 3));
        }
    }
}