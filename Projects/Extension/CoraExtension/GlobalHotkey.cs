using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using System.Collections;
using Extension.Coroutines;
using System.Linq;
using DynamicPatcher;

namespace Extension.CoraExtension
{
    public static class GlobalHotkey
    {
        #region Win32 API
        [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int vk);
        [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")] private static extern bool IsIconic(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("kernel32.dll")] private static extern IntPtr GetConsoleWindow();
        #endregion

        #region 常量
        private const int SW_RESTORE = 9;
        private const int SW_MINIMIZE = 6;
        private const int SW_MAXIMIZE = 3;
        private static readonly IntPtr HWND_TOPMOST = new(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new(-2);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;

        public const int MOD_NONE = 0;
        public const int MOD_ALT = 1;
        public const int MOD_CONTROL = 2;
        public const int MOD_SHIFT = 4;
        public const int MOD_WIN = 8;
        #endregion

        public enum KeyEvent { Down, Up }

        #region 热键数据（纯内存，无API）
        private class HotkeyItem
        {
            public int Id;
            public int Modifiers;
            public int VK;
            public Action Action;
            public KeyEvent Event;
            public bool LastDown;
        }

        private static List<HotkeyItem> _hotkeys = new List<HotkeyItem>();
        private static Thread _thread;
        private static bool _running;
        #endregion

        // ==========================
        // 你要的添加方法
        // ==========================
        public static void AddHotKeyEvent(int hotkeyId, Action action, int modifiers, int vk, KeyEvent evt = KeyEvent.Down)
        {
            // 改为使用 Any 方法，更符合语义
            if (_hotkeys.Any(hk => hk.Id == hotkeyId))
            {
                Logger.Log($"Hotkey with ID {hotkeyId} already exists.");
                return;
            }
            
            var hotkeyItem = new HotkeyItem
            {
                Id = hotkeyId,
                Modifiers = modifiers,
                VK = vk,
                Action = action,
                Event = evt
            };
            
            _hotkeys.Add(hotkeyItem);
        }
        public static Coroutine Start(CoroutineSystem coroutineSystem)
        {
            // if (_running) return null;
            _running = true;

            return coroutineSystem.StartCoroutine(CheckLoop());
        }
        


        private static IEnumerator CheckLoop()
        {
            while (_running)
            {
                try
                {
                    foreach (var hk in _hotkeys)
                    {
                        bool modOk = true;
                        if ((hk.Modifiers & MOD_ALT) != 0) modOk &= (GetAsyncKeyState(0x12) & 0x8000) != 0;
                        if ((hk.Modifiers & MOD_CONTROL) != 0) modOk &= (GetAsyncKeyState(0x11) & 0x8000) != 0;
                        if ((hk.Modifiers & MOD_SHIFT) != 0) modOk &= (GetAsyncKeyState(0x10) & 0x8000) != 0;

                        bool keyDown = (GetAsyncKeyState(hk.VK) & 0x8000) != 0;
                        bool nowDown = modOk && keyDown;

                        if (hk.Event == KeyEvent.Down && nowDown && !hk.LastDown)
                        {
                            hk.Action?.Invoke();
                        }

                        hk.LastDown = nowDown;
                    }

                  
                }
                catch
                {
                    
                }
                yield return null;
            }
        }

        public static void Stop()
        {
            _running = false;
            _hotkeys.Clear();
        }

        #region 窗口功能（保留）
        public static void SetTopMost(IntPtr hWnd) => SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        public static void CancelTopMost(IntPtr hWnd) => SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        public static void ActiveWindow(IntPtr hWnd) => SetForegroundWindow(hWnd);
        public static void Maximize(IntPtr hWnd) => ShowWindow(hWnd, SW_MAXIMIZE);
        public static void Minimize(IntPtr hWnd) => ShowWindow(hWnd, SW_MINIMIZE);
        public static void Restore(IntPtr hWnd) => ShowWindow(hWnd, SW_RESTORE);
        public static bool IsMinimized(IntPtr hWnd) => IsIconic(hWnd);
        #endregion
    }
    public static class MOD
    {
        public const int MOD_NONE = 0;
        public const int MOD_ALT = 1;
        public const int MOD_CONTROL = 2;
        public const int MOD_SHIFT = 4;
        public const int MOD_WIN = 8;
        private static Dictionary<int, string> _keyCodeToString;
        private static Dictionary<string, int> _stringToKeyCode;
        static MOD () {
            _keyCodeToString = new Dictionary<int, string> (){
                { MOD_ALT, nameof(MOD_ALT) },
                { MOD_CONTROL, nameof(MOD_CONTROL) },
                { MOD_SHIFT, nameof(MOD_SHIFT) },
                { MOD_WIN, nameof(MOD_WIN) },
                { MOD_NONE, nameof(MOD_NONE) },
            };
            _stringToKeyCode = new Dictionary<string, int>();
            foreach (var kvp in _keyCodeToString)
                _stringToKeyCode[kvp.Value] = kvp.Key;

        }

        /// <summary>
        /// 按键码 → 字符串
        /// 例：0x28 → "VK_DOWN"
        /// </summary>
        public static string GetKeyString(int keyCode)
        {
            return _keyCodeToString.TryGetValue(keyCode, out var name) ? name : "UNKNOWN";
        }

        /// <summary>
        /// 字符串 → 按键码
        /// 例："VK_DOWN" → 0x28
        /// </summary>
        public static int GetKey(string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName)) return -1;
            keyName = keyName.Trim();
            return _stringToKeyCode.TryGetValue(keyName, out var code) ? code : -1;
        }
        
    }
    public static class VK
    {
        public const int VK_A = 0x41;
        public const int VK_B = 0x42;
        public const int VK_C = 0x43;
        public const int VK_D = 0x44;
        public const int VK_E = 0x45;
        public const int VK_F = 0x46;
        public const int VK_G = 0x47;
        public const int VK_H = 0x48;
        public const int VK_I = 0x49;
        public const int VK_J = 0x4A;
        public const int VK_K = 0x4B;
        public const int VK_L = 0x4C;
        public const int VK_M = 0x4D;
        public const int VK_N = 0x4E;
        public const int VK_O = 0x4F;
        public const int VK_P = 0x50;
        public const int VK_Q = 0x51;
        public const int VK_R = 0x52;
        public const int VK_S = 0x53;
        public const int VK_T = 0x54;
        public const int VK_U = 0x55;
        public const int VK_V = 0x56;
        public const int VK_W = 0x57;
        public const int VK_X = 0x58;
        public const int VK_Y = 0x59;
        public const int VK_Z = 0x5A;

        public const int VK_0 = 0x30;
        public const int VK_1 = 0x31;
        public const int VK_2 = 0x32;
        public const int VK_3 = 0x33;
        public const int VK_4 = 0x34;
        public const int VK_5 = 0x35;
        public const int VK_6 = 0x36;
        public const int VK_7 = 0x37;
        public const int VK_8 = 0x38;
        public const int VK_9 = 0x39;

        public const int VK_F1 = 0x70;
        public const int VK_F2 = 0x71;
        public const int VK_F3 = 0x72;
        public const int VK_F4 = 0x73;
        public const int VK_F5 = 0x74;
        public const int VK_F6 = 0x75;
        public const int VK_F7 = 0x76;
        public const int VK_F8 = 0x77;
        public const int VK_F9 = 0x78;
        public const int VK_F10 = 0x79;
        public const int VK_F11 = 0x7A;
        public const int VK_F12 = 0x7B;

        public const int VK_ESCAPE = 0x1B;
        public const int VK_SPACE = 0x20;
        public const int VK_RETURN = 0x0D;
        public const int VK_BACK = 0x08;
        public const int VK_TAB = 0x09;

        public const int VK_LEFT = 0x25;
        public const int VK_UP = 0x26;
        public const int VK_RIGHT = 0x27;
        public const int VK_DOWN = 0x28;



        // ==============================================
        // 你要的两个方法 👇 直接复制这里
        // ==============================================
        private static Dictionary<int, string> _keyCodeToString;
        private static Dictionary<string, int> _stringToKeyCode;

        static VK()
        {
            _keyCodeToString = new Dictionary<int, string>
            {
                { VK_A, nameof(VK_A) },
                { VK_B, nameof(VK_B) },
                { VK_C, nameof(VK_C) },
                { VK_D, nameof(VK_D) },
                { VK_E, nameof(VK_E) },
                { VK_F, nameof(VK_F) },
                { VK_G, nameof(VK_G) },
                { VK_H, nameof(VK_H) },
                { VK_I, nameof(VK_I) },
                { VK_J, nameof(VK_J) },
                { VK_K, nameof(VK_K) },
                { VK_L, nameof(VK_L) },
                { VK_M, nameof(VK_M) },
                { VK_N, nameof(VK_N) },
                { VK_O, nameof(VK_O) },
                { VK_P, nameof(VK_P) },
                { VK_Q, nameof(VK_Q) },
                { VK_R, nameof(VK_R) },
                { VK_S, nameof(VK_S) },
                { VK_T, nameof(VK_T) },
                { VK_U, nameof(VK_U) },
                { VK_V, nameof(VK_V) },
                { VK_W, nameof(VK_W) },
                { VK_X, nameof(VK_X) },
                { VK_Y, nameof(VK_Y) },
                { VK_Z, nameof(VK_Z) },

                { VK_0, nameof(VK_0) },
                { VK_1, nameof(VK_1) },
                { VK_2, nameof(VK_2) },
                { VK_3, nameof(VK_3) },
                { VK_4, nameof(VK_4) },
                { VK_5, nameof(VK_5) },
                { VK_6, nameof(VK_6) },
                { VK_7, nameof(VK_7) },
                { VK_8, nameof(VK_8) },
                { VK_9, nameof(VK_9) },

                { VK_F1, nameof(VK_F1) },
                { VK_F2, nameof(VK_F2) },
                { VK_F3, nameof(VK_F3) },
                { VK_F4, nameof(VK_F4) },
                { VK_F5, nameof(VK_F5) },
                { VK_F6, nameof(VK_F6) },
                { VK_F7, nameof(VK_F7) },
                { VK_F8, nameof(VK_F8) },
                { VK_F9, nameof(VK_F9) },
                { VK_F10, nameof(VK_F10) },
                { VK_F11, nameof(VK_F11) },
                { VK_F12, nameof(VK_F12) },

                { VK_ESCAPE, nameof(VK_ESCAPE) },
                { VK_SPACE, nameof(VK_SPACE) },
                { VK_RETURN, nameof(VK_RETURN) },
                { VK_BACK, nameof(VK_BACK) },
                { VK_TAB, nameof(VK_TAB) },

                { VK_LEFT, nameof(VK_LEFT) },
                { VK_UP, nameof(VK_UP) },
                { VK_RIGHT, nameof(VK_RIGHT) },
                { VK_DOWN, nameof(VK_DOWN) },

               
            };

            _stringToKeyCode = new Dictionary<string, int>();
            foreach (var kvp in _keyCodeToString)
                _stringToKeyCode[kvp.Value] = kvp.Key;
        }

        /// <summary>
        /// 按键码 → 字符串
        /// 例：0x28 → "VK_DOWN"
        /// </summary>
        public static string GetKeyString(int keyCode)
        {
            return _keyCodeToString.TryGetValue(keyCode, out var name) ? name : "UNKNOWN";
        }

        /// <summary>
        /// 字符串 → 按键码
        /// 例："VK_DOWN" → 0x28
        /// </summary>
        public static int GetKey(string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName)) return -1;
            keyName = keyName.Trim();
            return _stringToKeyCode.TryGetValue(keyName, out var code) ? code : -1;
        }
    }
}