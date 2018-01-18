using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VL.Library
{
    public class MethodForWin32
    {

        #region 常数和结构

        public const int WM_KEYDOWN = 0x100;

        public const int WM_KEYUP = 0x101;

        public const int WM_SYSKEYDOWN = 0x104;

        public const int WM_SYSKEYUP = 0x105;

        public const int WH_KEYBOARD_LL = 13;
        public const int WM_CLOSE = 0x10;



        [StructLayout(LayoutKind.Sequential)] //声明键盘钩子的封送结构类型 

        public class KeyboardHookStruct
        {

            public int vkCode; //表示一个在1到254间的虚似键盘码 

            public int scanCode; //表示硬件扫描码 

            public int flags;

            public int time;

            public int dwExtraInfo;

        }

        #endregion


        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        #region win32 引用
        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        /// <summary>
        /// 窗体查找
        /// </summary>
        /// <param name="lpClassName"></param>
        /// <param name="lpWindowName"></param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll", EntryPoint = "FindWindowEx", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);

        //安装钩子的函数 

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]

        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);


        //卸下钩子的函数 

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]

        public static extern bool UnhookWindowsHookEx(int idHook);


        //下一个钩挂的函数 

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]

        public static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);


        [DllImport("user32")]

        public static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);


        [DllImport("user32")]

        public static extern int GetKeyboardState(byte[] pbKeyState);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]

        public static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);
        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow", SetLastError = true)]
        public static extern void SetForegroundWindow(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern int EnumChildWindows(IntPtr hWndParent, CallBack lpfn, int lParam);
        public delegate bool CallBack(IntPtr hwnd, int lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowText(IntPtr hWnd, IntPtr lpString, int nMaxCount);


        #endregion

        #region Method
        /// <summary>
        /// 设置revit焦点
        /// </summary>
        public static void SetFocusToRevit(IntPtr handle)
        {
            int wMsg = 7;
            MethodForWin32.SendMessage(handle, wMsg, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// 取消revit操作
        /// </summary>
        public static void CancelRevitOperate()
        {
            byte bVk = 27;
            int dwFlags = 0;
            int dwFlags2 = 2;
            MethodForWin32.keybd_event(bVk, 0, dwFlags, 0);
            MethodForWin32.keybd_event(bVk, 0, dwFlags2, 0);
        }
        #endregion

    }
    public class KeyBoardHook
    {
        int hHook;

        MethodForWin32.HookProc KeyboardHookDelegate;
        public delegate int KeyboardHookProcHandle(int nCode, Int32 wParam, IntPtr lParam);

        public delegate int KeyboardHookHandler(object sender, KeyEventArgs e);
        public event KeyboardHookHandler OnKeyDownEvent;
        public KeyboardHookProcHandle KeyboardHookProcdelegate;
        public event KeyEventHandler OnKeyUpEvent;

        public event KeyPressEventHandler OnKeyPressEvent;
        public List<Keys> KeyList;

        public KeyBoardHook()
        {
            KeyList = new List<Keys>();
        }

        /// <summary>
        /// 加载钩子
        /// </summary>
        public void SetHook()
        {
            if (KeyboardHookProcdelegate == null)
                KeyboardHookDelegate = new MethodForWin32.HookProc(KeyboardHookProc);
            else
            {
                KeyboardHookDelegate = new MethodForWin32.HookProc(KeyboardHookProcdelegate);
            }
            Process cProcess = Process.GetCurrentProcess();

            ProcessModule cModule = cProcess.MainModule;

            var mh = MethodForWin32.GetModuleHandle(cModule.ModuleName);

            hHook = MethodForWin32.SetWindowsHookEx(MethodForWin32.WH_KEYBOARD_LL, KeyboardHookDelegate, mh, 0);

        }
        /// <summary>
        /// 卸载钩子
        /// </summary>
        public void UnHook()
        {

            MethodForWin32.UnhookWindowsHookEx(hHook);

        }

        private List<Keys> preKeysList = new List<Keys>();//存放被按下的控制键，用来生成具体的键

        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            try
            {
                Keys keyData = new Keys();
                //如果该消息被丢弃（nCode<0）或者没有事件绑定处理程序则不会触发事件

                if ((nCode >= 0) && (OnKeyDownEvent != null || OnKeyUpEvent != null || OnKeyPressEvent != null))
                {

                    MethodForWin32.KeyboardHookStruct KeyDataFromHook = (MethodForWin32.KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(MethodForWin32.KeyboardHookStruct));

                    keyData = (Keys)KeyDataFromHook.vkCode;

                    //按下控制键

                    if ((OnKeyDownEvent != null || OnKeyPressEvent != null) && (wParam == MethodForWin32.WM_KEYDOWN || wParam == MethodForWin32.WM_SYSKEYDOWN))
                    {

                        if (IsCtrlAltShiftKeys(keyData) && preKeysList.IndexOf(keyData) == -1)
                        {

                            preKeysList.Add(keyData);

                        }

                    }

                    //WM_KEYDOWN和WM_SYSKEYDOWN消息，将会引发OnKeyDownEvent事件

                    if (OnKeyDownEvent != null && (wParam == MethodForWin32.WM_KEYDOWN || wParam == MethodForWin32.WM_SYSKEYDOWN))
                    {

                        KeyEventArgs e = new KeyEventArgs(GetDownKeys(keyData));



                        int reslut = OnKeyDownEvent(this, e);
                        if (reslut == 1)
                            return 1;

                    }

                    //WM_KEYDOWN消息将引发OnKeyPressEvent 

                    if (OnKeyPressEvent != null && wParam == MethodForWin32.WM_KEYDOWN)
                    {

                        byte[] keyState = new byte[256];

                        MethodForWin32.GetKeyboardState(keyState);


                        byte[] inBuffer = new byte[2];

                        if (MethodForWin32.ToAscii(KeyDataFromHook.vkCode, KeyDataFromHook.scanCode, keyState, inBuffer, KeyDataFromHook.flags) == 1)
                        {

                            KeyPressEventArgs e = new KeyPressEventArgs((char)inBuffer[0]);

                            OnKeyPressEvent(this, e);

                        }

                    }

                    //松开控制键

                    if ((OnKeyDownEvent != null || OnKeyPressEvent != null) && (wParam == MethodForWin32.WM_KEYUP || wParam == MethodForWin32.WM_SYSKEYUP))
                    {

                        if (IsCtrlAltShiftKeys(keyData))
                        {

                            for (int i = preKeysList.Count - 1; i >= 0; i--)
                            {

                                if (preKeysList[i] == keyData) { preKeysList.RemoveAt(i); }

                            }

                        }

                    }

                    //WM_KEYUP和WM_SYSKEYUP消息，将引发OnKeyUpEvent事件 

                    if (OnKeyUpEvent != null && (wParam == MethodForWin32.WM_KEYUP || wParam == MethodForWin32.WM_SYSKEYUP))
                    {

                        KeyEventArgs e = new KeyEventArgs(GetDownKeys(keyData));

                        OnKeyUpEvent(this, e);

                    }

                }
                if (KeyList.Count > 0)
                {
                    bool keyFlag = false;
                    foreach (var key in KeyList)
                    {
                        if (keyData == key)
                        {
                            keyFlag = true;
                            break;
                        }
                    }
                    if (keyFlag)
                        return 1;
                    else
                        return MethodForWin32.CallNextHookEx(hHook, nCode, wParam, lParam);
                }
                else
                {
                    return MethodForWin32.CallNextHookEx(hHook, nCode, wParam, lParam);
                }


            }
            catch (Exception ex)
            {
                UnHook();
                throw new Exception("获取按键信息异常");
            }


        }

        //根据已经按下的控制键生成key

        private Keys GetDownKeys(Keys key)
        {

            Keys rtnKey = Keys.None;

            foreach (Keys i in preKeysList)
            {

                if (i == Keys.LControlKey || i == Keys.RControlKey) { rtnKey = rtnKey | Keys.Control; }

                if (i == Keys.LMenu || i == Keys.RMenu) { rtnKey = rtnKey | Keys.Alt; }

                if (i == Keys.LShiftKey || i == Keys.RShiftKey) { rtnKey = rtnKey | Keys.Shift; }

            }

            return rtnKey | key;

        }


        private Boolean IsCtrlAltShiftKeys(Keys key)
        {

            if (key == Keys.LControlKey || key == Keys.RControlKey || key == Keys.LMenu || key == Keys.RMenu || key == Keys.LShiftKey || key == Keys.RShiftKey) { return true; }

            return false;

        }

    }
}
