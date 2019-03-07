using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace KeyboardDclickFix {
    public class KeyboardHook {
        private const int WM_KEYDOWN = 0x100;//按下消息   
        private const int WM_KEYUP = 0x101;//松开消息   
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;

        //全局事件   
        public event KeyEventHandler OnKeyDownEvent;
        public event KeyEventHandler OnKeyUpEvent;
        public event KeyPressEventHandler OnKeyPressEvent;

        static int hKeyboardHook = 0;
        static int intervalTime = 40;

        //鼠标常量   
        public const int WH_KEYBOARD_LL = 13;

        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);

        //声明键盘钩子事件类型   
        HookProc KeyboardHookProcedure;

        /// <summary>   
        /// 声明键盘钩子的封送结构类型   
        /// </summary>   
        [StructLayout(LayoutKind.Sequential)]
        public class KeyboardHookStruct {
            public int vkCode;//表示一个1到254间的虚拟键盘码   
            public int scanCode;//表示硬件扫描码   
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        //安装钩子   
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        //下一个钩子   
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);
        //卸载钩子   
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        Dictionary<int, DateTime> log = new Dictionary<int, DateTime>();

        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam) {
            if ((nCode >= 0) && (OnKeyDownEvent != null || OnKeyUpEvent != null || OnKeyPressEvent != null)) {
                KeyboardHookStruct MyKBHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));

                if (OnKeyDownEvent != null && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN)) {
                    if (!log.ContainsKey(MyKBHookStruct.vkCode)) {
                        log.Add(MyKBHookStruct.vkCode, DateTime.MinValue);
                    }
                    if ((DateTime.Now - log[MyKBHookStruct.vkCode]).TotalMilliseconds < intervalTime) {
                        Keys keyData = (Keys)MyKBHookStruct.vkCode;
                        KeyEventArgs e = new KeyEventArgs(keyData);
                        OnKeyDownEvent(this, e);
                        return 1;
                    }
                }
                else {
                    log[MyKBHookStruct.vkCode] = DateTime.Now;
                }
            }
            return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }

        public void Start() {
            if (hKeyboardHook == 0) {
                KeyboardHookProcedure = new HookProc(KeyboardHookProc);
                using (System.Diagnostics.Process curProcess = System.Diagnostics.Process.GetCurrentProcess())
                using (System.Diagnostics.ProcessModule curModule = curProcess.MainModule)
                    hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookProcedure, LoadLibrary(curModule.ModuleName), 0);

                if (hKeyboardHook == 0) {
                    Stop();
                    throw new Exception("Set GlobalKeyboardHook failed!");
                }
            }
        }
        public void Stop() {
            bool retKeyboard = true;
            if (hKeyboardHook != 0) {
                retKeyboard = UnhookWindowsHookEx(hKeyboardHook);
                hKeyboardHook = 0;
            }
            if (!retKeyboard)
                throw new Exception("Unload GlobalKeyboardHook failed!");
        }

        public void SetIntervalTime(int time) {
            intervalTime = time;
        }

        //构造函数中安装钩子   
        public KeyboardHook() {
            Start();
        }
        //析构函数中卸载钩子   
        ~KeyboardHook() {
            Stop();
        }
    }
}
