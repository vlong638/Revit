using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace MouseHook.VLHooker
{
    public class Hooker
    {
        #region Fields

        public int hMouseHook = 0;

        //全局钩子常量
        public const int WH_MOUSE_LL = 14;

        //声明消息的常量,鼠标按下和释放
        public const int WM_RBUTTONDOWN = 0x204;
        public const int WM_RBUTTONUP = 0x205;

        //定义委托
        public delegate int HookProc(int nCode, int wParam, IntPtr lParam);
        public HookProc MouseHookProcedure;

        #endregion 

        #region 声明Api函数，需要引入空间(System.Runtime.InteropServices)

        //寻找符合条件的窗口
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern int FindWindow(
            string lpClassName,
            string lpWindowName
        );
        [DllImport("user32.dll", EntryPoint = "FindWindowEx", CharSet = CharSet.Auto)]
        public extern static IntPtr FindWindowEx(
            IntPtr hwndParent,
            IntPtr hwndChildAfter,
            string lpszClass,
            string lpszWindow
        );

        //获取窗口的矩形区域
        [DllImport("user32.dll", EntryPoint = "GetWindowRect")]
        public static extern int GetWindowRect(
            int hwnd,
            ref Rectangle lpRect
        );

        //安装钩子
        [DllImport("user32.dll")]
        public static extern int SetWindowsHookEx(
            int idHook,
            HookProc lpfn,
            IntPtr hInstance,
            int threadId
        );

        //卸载钩子
        [DllImport("user32.dll", EntryPoint = "UnhookWindowsHookEx")]
        public static extern bool UnhookWindowsHookEx(
            int hHook
        );

        //调用下一个钩子
        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(
            int idHook,
            int nCode,
            int wParam,
            IntPtr lParam
        );

        //获取当前线程的标识符
        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();

        //获取一个应用程序或动态链接库的模块句柄
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);

        #endregion

        public void Start()
        {
            if (hMouseHook == 0)
            {
                hMouseHook = SetWindowsHookEx(WH_MOUSE_LL, MouseHookProcedure, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);

                if (hMouseHook == 0)
                {//如果设置钩子失败. 

                    Stop();
                    MessageBox.Show("Set windows hook failed!");
                }
            }
        }

        public void Stop()
        {
            bool stop = true;

            if (hMouseHook != 0)
            {
                stop = UnhookWindowsHookEx(hMouseHook);
                hMouseHook = 0;

                if (!stop)
                {//卸载钩子失败

                    MessageBox.Show("Unhook failed!");
                }
            }
        }
    }
}
