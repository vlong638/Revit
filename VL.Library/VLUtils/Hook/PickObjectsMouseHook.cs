//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Drawing;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;

//namespace VL.Library
//{
//    #region Resource
//    public enum HookTypes
//    {
//        None = -100,
//        Keyboard = 2,
//        Mouse = 7,
//        Hardware = 8,
//        Shell = 10,
//        KeyboardLL = 13,
//        MouseLL = 14
//    }
//    public abstract class SystemHook : IDisposable
//    {
//        #region Member Variables and Delegates

//        /// 返回ture表示截获该消息，不再传递下去
//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/HookProcessedHandler/*'/>
//        protected delegate bool HookProcessedHandler(int code, UIntPtr wparam, IntPtr lparam);

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/type/*'/>
//        private HookTypes _type = HookTypes.None;

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/processHandler/*'/>
//        private HookProcessedHandler _processHandler = null;

//        private bool _isHooked = false;

//        #endregion

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/ctor/*'/>
//        public SystemHook(HookTypes type)
//        {
//            _type = type;

//            _processHandler = new HookProcessedHandler(InternalHookCallback);
//            SetCallBackResults result = SetUserHookCallback(_processHandler, _type);
//            if (result != SetCallBackResults.Success)
//            {
//                this.Dispose();
//                GenerateCallBackException(type, result);
//            }
//        }

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/dtor/*'/>
//        ~SystemHook()
//        {
//            Trace.WriteLine("SystemHook (" + _type + ") WARNING: Finalizer called, " +
//                "a reference has not been properly disposed.");

//            Dispose(false);
//        }

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/HookCallback/*'/>
//        protected abstract bool HookCallback(int code, UIntPtr wparam, IntPtr lparam);

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/InternalHookCallback/*'/>
//        [MethodImpl(MethodImplOptions.NoInlining)]
//        private bool InternalHookCallback(int code, UIntPtr wparam, IntPtr lparam)
//        {
//            try
//            {
//                return HookCallback(code, wparam, lparam);
//            }
//            catch (Exception e)
//            {
//                //
//                // While it is not generally a good idea to trap and discard all exceptions
//                // in a base class, this is a special case. Remember, this is the entry point
//                // for the C++ library to call into our .NET code. We don't want to return
//                // .NET exceptions to C++. If it gets this far all we can do is drop them.
//                //
//                Debug.WriteLine("Exception during hook callback: " + e.Message + " " + e.ToString());
//                return false;
//            }
//        }

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/InstallHook/*'/>
//        public void InstallHook()
//        {
//            if (!InitializeHook(_type, 0))
//            {
//                throw new NotImplementedException("Hook initialization failed.");
//            }
//            _isHooked = true;
//        }

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/UninstallHook/*'/>
//        public void UninstallHook()
//        {
//            _isHooked = false;
//            UninitializeHook(_type);
//        }

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/HookType/*'/>
//        protected HookTypes HookType
//        {
//            get
//            {
//                return _type;
//            }
//        }

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/IsHooked/*'/>
//        public bool IsHooked
//        {
//            get
//            {
//                return _isHooked;
//            }
//        }

//        #region Exception Generation Helper Methods

//        private void GenerateCallBackException(HookTypes type, SetCallBackResults result)
//        {
//            if (result == SetCallBackResults.Success)
//            {
//                return;
//            }

//            string msg;

//            switch (result)
//            {
//                case SetCallBackResults.AlreadySet:
//                    msg = "A hook of type " + type + " is already registered. You can only register ";
//                    msg += "a single instance of each type of hook class. This can also occur when you forget ";
//                    msg += "to unregister or dispose a previous instance of the class.";

//                    throw new NotImplementedException(msg);

//                case SetCallBackResults.ArgumentError:
//                    msg = "Failed to set hook callback due to an error in the arguments.";

//                    throw new ArgumentException(msg);

//                case SetCallBackResults.NotImplemented:
//                    msg = "The hook type of type " + type + " is not implemented in the C++ layer. ";
//                    msg += "You must implement this hook type before you can use it. See the C++ function ";
//                    msg += "SetUserHookCallback.";

//                    throw new NotImplementedException(msg);
//            }

//            msg = "Unrecognized exception during hook callback setup. Error code " + result + ".";
//            throw new NotImplementedException(msg);
//        }

//        private void GenerateFilterMessageException(HookTypes type, FilterMessageResults result)
//        {
//            if (result == FilterMessageResults.Success)
//            {
//                return;
//            }

//            string msg;

//            if (result == FilterMessageResults.NotImplemented)
//            {
//                msg = "The hook type of type " + type + " is not implemented in the C++ layer. ";
//                msg += "You must implement this hook type before you can use it. See the C++ function ";
//                msg += "FilterMessage.";

//                throw new NotImplementedException(msg);
//            }

//            //
//            // All other errors are general errors.
//            //
//            msg = "Unrecognized exception during hook FilterMessage call. Error code " + result + ".";
//            throw new NotImplementedException(msg);
//        }

//        #endregion

//        #region IDisposable Members

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/Dispose/*'/>
//        public void Dispose()
//        {
//            Dispose(true);
//        }

//        private void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                GC.SuppressFinalize(this);
//            }

//            UninstallHook();
//            DisposeCppLayer(_type);
//        }

//        #endregion

//        #region Win32 API Utility Methods

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/GetMousePosition/*'/>
//        protected void GetMousePosition(UIntPtr wparam, IntPtr lparam, ref int x, ref int y)
//        {
//            if (!InternalGetMousePosition(wparam, lparam, ref x, ref y))
//            {
//                throw new NotImplementedException("Failed to access mouse position.");
//            }
//        }

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/GetKeyboardReading/*'/>
//        protected void GetKeyboardReading(UIntPtr wparam, IntPtr lparam, ref int vkCode)
//        {
//            if (!IntenralGetKeyboardReading(wparam, lparam, ref vkCode))
//            {
//                throw new NotImplementedException("Failed to access keyboard settings.");
//            }
//        }

//        /// 过滤截获的消息类型：可以把需要截获的消息一个一个添加上去，默认全部消息都截获
//		/// <include file='ManagedHooks.xml' path='Docs/SystemHook/FilterMessage/*'/>
//		protected void FilterMessage(HookTypes hookType, int message)
//        {
//            FilterMessageResults result = InternalFilterMessage(hookType, message);
//            if (result != FilterMessageResults.Success)
//            {
//                GenerateFilterMessageException(hookType, result);
//            }
//        }

//        #endregion

//        #region Imported Static Methods for PmSoft.Common.UnManaged20_x64

//        private enum SetCallBackResults
//        {
//            Success = 1,
//            AlreadySet = -2,
//            NotImplemented = -3,
//            ArgumentError = -4
//        };

//        private enum FilterMessageResults
//        {
//            Success = 1,
//            Failed = -2,
//            NotImplemented = -3
//        };

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/SetUserHookCallback/*'/>
//        [DllImport("PmSoft.Common.UnManaged20_x64.dll", EntryPoint = "SetUserHookCallback", SetLastError = true,
//             CharSet = CharSet.Unicode, ExactSpelling = true,
//             CallingConvention = CallingConvention.StdCall)]
//        private static extern SetCallBackResults SetUserHookCallback(HookProcessedHandler hookCallback, HookTypes hookType);

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/InitializeHook/*'/>
//        [DllImport("PmSoft.Common.UnManaged20_x64.dll", EntryPoint = "InitializeHook", SetLastError = true,
//             CharSet = CharSet.Unicode, ExactSpelling = true,
//             CallingConvention = CallingConvention.StdCall)]
//        private static extern bool InitializeHook(HookTypes hookType, int threadID);

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/UninitializeHook/*'/>
//        [DllImport("PmSoft.Common.UnManaged20_x64.dll", EntryPoint = "UninitializeHook", SetLastError = true,
//             CharSet = CharSet.Unicode, ExactSpelling = true,
//             CallingConvention = CallingConvention.StdCall)]
//        private static extern void UninitializeHook(HookTypes hookType);

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/InternalGetMousePosition/*'/>
//        [DllImport("PmSoft.Common.UnManaged20_x64.dll", EntryPoint = "GetMousePosition", SetLastError = true,
//             CharSet = CharSet.Unicode, ExactSpelling = true,
//             CallingConvention = CallingConvention.StdCall)]
//        private static extern bool InternalGetMousePosition(UIntPtr wparam, IntPtr lparam, ref int x, ref int y);

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/IntenralGetKeyboardReading/*'/>
//        [DllImport("PmSoft.Common.UnManaged20_x64.dll", EntryPoint = "GetKeyboardReading", SetLastError = true,
//             CharSet = CharSet.Unicode, ExactSpelling = true,
//             CallingConvention = CallingConvention.StdCall)]
//        private static extern bool IntenralGetKeyboardReading(UIntPtr wparam, IntPtr lparam, ref int vkCode);

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/DisposeCppLayer/*'/>
//        [DllImport("PmSoft.Common.UnManaged20_x64.dll", EntryPoint = "Dispose", SetLastError = true,
//             CharSet = CharSet.Unicode, ExactSpelling = true,
//             CallingConvention = CallingConvention.StdCall)]
//        private static extern void DisposeCppLayer(HookTypes hookType);

//        /// <include file='ManagedHooks.xml' path='Docs/SystemHook/InternalFilterMessage/*'/>
//        [DllImport("PmSoft.Common.UnManaged20_x64.dll", EntryPoint = "FilterMessage", SetLastError = true,
//             CharSet = CharSet.Unicode, ExactSpelling = true,
//             CallingConvention = CallingConvention.StdCall)]
//        private static extern FilterMessageResults InternalFilterMessage(HookTypes hookType, int message);

//        #endregion

//    }
//    /// <include file='ManagedHooks.xml' path='Docs/MouseEvents/enum/*'/>
//    public enum MouseEvents
//    {
//        /// <include file='ManagedHooks.xml' path='Docs/General/Empty/*'/>
//        LeftButtonDown = 0x0201,
//        /// <include file='ManagedHooks.xml' path='Docs/General/Empty/*'/>
//        LeftButtonUp = 0x0202,
//        /// <include file='ManagedHooks.xml' path='Docs/General/Empty/*'/>
//        Move = 0x0200,
//        /// <include file='ManagedHooks.xml' path='Docs/General/Empty/*'/>
//        MouseWheel = 0x020A,
//        /// <include file='ManagedHooks.xml' path='Docs/General/Empty/*'/>
//        RightButtonDown = 0x0204,
//        /// <include file='ManagedHooks.xml' path='Docs/General/Empty/*'/>
//        RightButtonUp = 0x0205
//    }
//    public class MouseHook : SystemHook
//    {
//        /// 返回ture表示截获该消息，不再传递下去
//		/// <include file='ManagedHooks.xml' path='Docs/MouseHook/MouseEventHandler/*'/>
//		public delegate bool MouseEventHandler(MouseEvents mEvent, Point point);

//        /// <include file='ManagedHooks.xml' path='Docs/MouseHook/MouseEvent/*'/>
//        public event MouseEventHandler MouseEvent;

//        /// <include file='ManagedHooks.xml' path='Docs/MouseHook/ctor/*'/>
//        public MouseHook() : base(HookTypes.MouseLL)
//        {
//        }

//        /// <include file='ManagedHooks.xml' path='Docs/MouseHook/HookCallback/*'/>
//        protected override bool HookCallback(int code, UIntPtr wparam, IntPtr lparam)
//        {
//            if (MouseEvent == null)
//            {
//                return false;
//            }

//            int x = 0, y = 0;
//            MouseEvents mEvent = (MouseEvents)wparam.ToUInt32();

//            switch (mEvent)
//            {
//                case MouseEvents.LeftButtonDown:
//                    GetMousePosition(wparam, lparam, ref x, ref y);
//                    break;
//                case MouseEvents.LeftButtonUp:
//                    GetMousePosition(wparam, lparam, ref x, ref y);
//                    break;
//                case MouseEvents.MouseWheel:
//                    break;
//                case MouseEvents.Move:
//                    GetMousePosition(wparam, lparam, ref x, ref y);
//                    break;
//                case MouseEvents.RightButtonDown:
//                    GetMousePosition(wparam, lparam, ref x, ref y);
//                    break;
//                case MouseEvents.RightButtonUp:
//                    GetMousePosition(wparam, lparam, ref x, ref y);
//                    break;
//                default:
//                    //System.Diagnostics.Trace.WriteLine("Unrecognized mouse event");
//                    break;
//            }

//            return MouseEvent(mEvent, new Point(x, y));
//        }

//        /// 过滤截获的消息类型：可以把需要截获的消息一个一个添加上去，默认全部消息都截获
//		/// <include file='ManagedHooks.xml' path='Docs/MouseHook/FilterMessage/*'/>
//		public void FilterMessage(MouseEvents eventType)
//        {
//            base.FilterMessage(this.HookType, (int)eventType);
//        }

//    }
//    /// <include file='ManagedHooks.xml' path='Docs/MouseHookExt/Class/*'/>
//    public class MouseHookExt : MouseHook
//    {
//        /// 返回ture表示截获该消息，不再传递下去
//		/// <include file='ManagedHooks.xml' path='Docs/MouseHookExt/MouseEventHandlerExt/*'/>
//		public delegate bool MouseEventHandlerExt(Point pt);

//        /// <include file='ManagedHooks.xml' path='Docs/MouseHookExt/LeftButtonDown/*'/>
//        public event MouseEventHandlerExt LeftButtonDown;
//        /// <include file='ManagedHooks.xml' path='Docs/MouseHookExt/RightButtonDown/*'/>
//        public event MouseEventHandlerExt RightButtonDown;
//        /// <include file='ManagedHooks.xml' path='Docs/MouseHookExt/LeftButtonUp/*'/>
//        public event MouseEventHandlerExt LeftButtonUp;
//        /// <include file='ManagedHooks.xml' path='Docs/MouseHookExt/RightButtonUp/*'/>
//        public event MouseEventHandlerExt RightButtonUp;
//        /// <include file='ManagedHooks.xml' path='Docs/MouseHookExt/MouseWheel/*'/>
//        public event MouseEventHandlerExt MouseWheel;
//        /// <include file='ManagedHooks.xml' path='Docs/MouseHookExt/Move/*'/>
//        public event MouseEventHandlerExt Move;

//        /// <include file='ManagedHooks.xml' path='Docs/MouseHookExt/ctor/*'/>
//        public MouseHookExt()
//        {
//            this.MouseEvent += new MouseEventHandler(MouseHookExt_MouseEvent);
//        }

//        /// <include file='ManagedHooks.xml' path='Docs/MouseHook/HookCallback/*'/>
//        protected override bool HookCallback(int code, UIntPtr wparam, IntPtr lparam)
//        {
//            return base.HookCallback(code, wparam, lparam);
//        }

//        private bool MouseHookExt_MouseEvent(MouseEvents mEvent, System.Drawing.Point point)
//        {
//            bool ret = false;
//            switch (mEvent)
//            {
//                case MouseEvents.LeftButtonUp:
//                    ret = Fire_LeftButtonUp(point);
//                    break;
//                case MouseEvents.LeftButtonDown:
//                    ret = Fire_LeftButtonDown(point);
//                    break;
//                case MouseEvents.RightButtonUp:
//                    ret = Fire_RightButtonUp(point);
//                    break;
//                case MouseEvents.RightButtonDown:
//                    ret = Fire_RightButtonDown(point);
//                    break;
//                case MouseEvents.MouseWheel:
//                    ret = Fire_MouseWheel(point);
//                    break;
//                case MouseEvents.Move:
//                    ret = Fire_Move(point);
//                    break;
//            }
//            return ret;
//        }

//        private bool Fire_LeftButtonDown(Point point)
//        {
//            if (LeftButtonDown != null)
//            {
//                return LeftButtonDown(point);
//            }

//            return false;
//        }

//        private bool Fire_LeftButtonUp(Point point)
//        {
//            if (LeftButtonUp != null)
//            {
//                return LeftButtonUp(point);
//            }

//            return false;
//        }

//        private bool Fire_RightButtonDown(Point point)
//        {
//            if (RightButtonDown != null)
//            {
//                return RightButtonDown(point);
//            }

//            return false;
//        }

//        private bool Fire_RightButtonUp(Point point)
//        {
//            if (RightButtonUp != null)
//            {
//                return RightButtonUp(point);
//            }

//            return false;
//        }

//        private bool Fire_Move(Point point)
//        {
//            if (Move != null)
//            {
//                return Move(point);
//            }

//            return false;
//        }

//        private bool Fire_MouseWheel(Point point)
//        {
//            if (MouseWheel != null)
//            {
//                return MouseWheel(point);
//            }

//            return false;
//        }
//    }
//    /// <summary>
//    /// 多选元素时的选择按钮
//    /// </summary>
//    public class ButtonHandle
//    {
//        [DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true)]
//        public static extern IntPtr FindWindowEx(IntPtr hwndParent, uint hwndChildAfter, string lpszClass, string lpszWindow);
//        [DllImport("user32.dll", EntryPoint = "FindWindowEx", CharSet = CharSet.Auto)]
//        extern static IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
//        [DllImport("User32.dll", EntryPoint = "SendMessage")]
//        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);
//        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow", SetLastError = true)]
//        public static extern void SetForegroundWindow(IntPtr hwnd);
//        [DllImport("user32.dll")]
//        public static extern int EnumChildWindows(IntPtr hWndParent, CallBack lpfn, int lParam);
//        public delegate bool CallBack(IntPtr hwnd, int lParam);

//        /// <summary>
//        /// 查找窗体上控件句柄
//        /// </summary>
//        /// <param name="hwnd">父窗体句柄</param>
//        /// <param name="lpszWindow">控件标题(Text)</param>
//        /// <param name="bChild">设定是否在子窗体中查找</param>
//        /// <returns>控件句柄，没找到返回IntPtr.Zero</returns>
//        private IntPtr FindWindowEx(IntPtr hwnd, string lpszClass, string lpszWindow, bool bChild)
//        {
//            IntPtr iResult = IntPtr.Zero;
//            // 首先在父窗体上查找控件
//            iResult = FindWindowEx(hwnd, 0, lpszClass, lpszWindow);
//            // 如果找到直接返回控件句柄
//            if (iResult != IntPtr.Zero) return iResult;

//            if (!bChild) return iResult;

//            int i = EnumChildWindows(
//            hwnd,
//            (h, l) =>
//            {
//                IntPtr f1 = FindWindowEx(h, 0, lpszClass, lpszWindow);
//                if (f1 == IntPtr.Zero)
//                    return true;
//                else
//                {
//                    iResult = f1;
//                    return false;
//                }
//            },
//            0);
//            return iResult;
//        }

//        public bool Mouse_RightButtonDown(System.Drawing.Point pt)
//        {
//            return SendMessageToButton("完成");
//        }

//        public bool Mouse_Cancle_RightButtonDown(System.Drawing.Point pt)
//        {
//            return SendMessageToButton("取消");
//        }

//        public bool SendMessageToButton(string buttonName)
//        {
//            IntPtr mainHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
//            //IntPtr controlBarHandle = IntPtr.Zero;
//            IntPtr buttonHandle = IntPtr.Zero;
//            //for (int i = 0; i < 30; i++)
//            {
//                //#if Revit2017
//                //                controlBarHandle = FindWindowEx(mainHandle, controlBarHandle, "Afx:ControlBar:347d0000:8:10003:10", null);
//                //#else
//                //                controlBarHandle = FindWindowEx(mainHandle, controlBarHandle, "Afx:ControlBar:40000000:8:10003:10", null);
//                //#endif
//                //if (controlBarHandle == IntPtr.Zero) continue;
//                //else
//                {
//                    //buttonHandle = FindWindowEx(controlBarHandle, "Button", buttonName, true);
//                    IntPtr titleWIntPtr = new CWndManager().SearchByTitle(mainHandle, buttonName);
//                    buttonHandle = titleWIntPtr;
//                    //if (buttonHandle == IntPtr.Zero) continue;
//                    //else
//                    {
//                        SendMessage(buttonHandle, 0x201, (IntPtr)1, null);
//                        SendMessage(buttonHandle, 0x202, (IntPtr)1, null);
//                        //break;
//                    }
//                }
//            }
//            return true;
//        }

//        public bool IsButtonExit(string buttonName)
//        {
//            IntPtr mainHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
//            IntPtr buttonHandle = IntPtr.Zero;
//            buttonHandle = new CWndManager().SearchByTitle(mainHandle, "完成");
//            if (buttonHandle == IntPtr.Zero) return false;
//            else return true;
//        }
//    }
//    public class MouseKeyBoardControl
//    {
//        public static int WM_KEYDOWN = 0x0100;
//        //释放一个键
//        public static int WM_KEYUP = 0x0101;
//        //按下某键，并已发出WM_KEYDOWN， WM_KEYUP消息
//        public static int WM_CHAR = 0x102;

//        //创建一个窗口   
//        const int WM_CREATE = 0x01;
//        //当一个窗口被破坏时发送   
//        const int WM_DESTROY = 0x02;
//        //移动一个窗口   
//        const int WM_MOVE = 0x03;
//        //改变一个窗口的大小   
//        const int WM_SIZE = 0x05;
//        //一个窗口被激活或失去激活状态   
//        const int WM_ACTIVATE = 0x06;
//        //一个窗口获得焦点   
//        const int WM_SETFOCUS = 0x07;
//        //一个窗口失去焦点   
//        const int WM_KILLFOCUS = 0x08;
//        //一个窗口改变成Enable状态   
//        const int WM_ENABLE = 0x0A;
//        //设置窗口是否能重画   
//        const int WM_SETREDRAW = 0x0B;
//        //应用程序发送此消息来设置一个窗口的文本   
//        const int WM_SETTEXT = 0x0C;
//        //应用程序发送此消息来复制对应窗口的文本到缓冲区   
//        const int WM_GETTEXT = 0x0D;
//        //得到与一个窗口有关的文本的长度（不包含空字符）   
//        const int WM_GETTEXTLENGTH = 0x0E;
//        //要求一个窗口重画自己   
//        const int WM_PAINT = 0x0F;
//        //当一个窗口或应用程序要关闭时发送一个信号   
//        const int WM_CLOSE = 0x10;
//        //当用户选择结束对话框或程序自己调用ExitWindows函数   
//        const int WM_QUERYENDSESSION = 0x11;
//        //用来结束程序运行   
//        const int WM_QUIT = 0x12;
//        //当用户窗口恢复以前的大小位置时，把此消息发送给某个图标   
//        const int WM_QUERYOPEN = 0x13;
//        //当窗口背景必须被擦除时（例在窗口改变大小时）   
//        const int WM_ERASEBKGND = 0x14;
//        //当系统颜色改变时，发送此消息给所有顶级窗口   
//        const int WM_SYSCOLORCHANGE = 0x15;
//        //当系统进程发出WM_QUERYENDSESSION消息后，此消息发送给应用程序，通知它对话是否结束 

//        const int WM_ENDSESSION = 0x16;
//        //当隐藏或显示窗口是发送此消息给这个窗口   
//        const int WM_SHOWWINDOW = 0x18;
//        //发此消息给应用程序哪个窗口是激活的，哪个是非激活的   
//        const int WM_ACTIVATEAPP = 0x1C;
//        //当系统的字体资源库变化时发送此消息给所有顶级窗口   
//        const int WM_FONTCHANGE = 0x1D;
//        //当系统的时间变化时发送此消息给所有顶级窗口   
//        const int WM_TIMECHANGE = 0x1E;
//        //发送此消息来取消某种正在进行的摸态（操作）   
//        const int WM_CANCELMODE = 0x1F;
//        //如果鼠标引起光标在某个窗口中移动且鼠标输入没有被捕获时，就发消息给某个窗口   
//        const int WM_SETCURSOR = 0x20;
//        //当光标在某个非激活的窗口中而用户正按着鼠标的某个键发送此消息给//当前窗口   
//        const int WM_MOUSEACTIVATE = 0x21;
//        //发送此消息给MDI子窗口//当用户点击此窗口的标题栏，或//当窗口被激活，移动，改变大小   
//        const int WM_CHILDACTIVATE = 0x22;
//        //此消息由基于计算机的训练程序发送，通过WH_JOURNALPALYBACK的hook程序分离出用户输入消息 

//        const int WM_QUEUESYNC = 0x23;
//        //此消息发送给窗口当它将要改变大小或位置   
//        const int WM_GETMINMAXINFO = 0x24;
//        //发送给最小化窗口当它图标将要被重画   
//        const int WM_PAINTICON = 0x26;
//        //此消息发送给某个最小化窗口，仅//当它在画图标前它的背景必须被重画   
//        const int WM_ICONERASEBKGND = 0x27;
//        //发送此消息给一个对话框程序去更改焦点位置   
//        const int WM_NEXTDLGCTL = 0x28;
//        //每当打印管理列队增加或减少一条作业时发出此消息    
//        const int WM_SPOOLERSTATUS = 0x2A;
//        //当button，combobox，listbox，menu的可视外观改变时发送   
//        const int WM_DRAWITEM = 0x2B;
//        //当button, combo box, list box, list view control, 
//        const int WM_MEASUREITEM = 0x2C;
//        //此消息有一个LBS_WANTKEYBOARDINPUT风格的发出给它的所有者来响应WM_KEYDOWN消息 

//        const int WM_VKEYTOITEM = 0x2E;
//        //此消息由一个LBS_WANTKEYBOARDINPUT风格的列表框发送给他的所有者来响应WM_CHAR消息 

//        const int WM_CHARTOITEM = 0x2F;
//        //当绘制文本时程序发送此消息得到控件要用的颜色   
//        const int WM_SETFONT = 0x30;
//        //应用程序发送此消息得到当前控件绘制文本的字体   
//        const int WM_GETFONT = 0x31;
//        //应用程序发送此消息让一个窗口与一个热键相关连    
//        const int WM_SETHOTKEY = 0x32;
//        //应用程序发送此消息来判断热键与某个窗口是否有关联   
//        const int WM_GETHOTKEY = 0x33;
//        //此消息发送给最小化窗口，当此窗口将要被拖放而它的类中没有定义图标，应用程序能返回一个图标或光标的句柄，当用户拖放图标时系统显示这个图标或光标 

//        const int WM_QUERYDRAGICON = 0x37;
//        //发送此消息来判定combobox或listbox新增加的项的相对位置   
//        const int WM_COMPAREITEM = 0x39;
//        //显示内存已经很少了   
//        const int WM_COMPACTING = 0x41;
//        //发送此消息给那个窗口的大小和位置将要被改变时，来调用setwindowpos函数或其它窗口管理函数 

//        const int WM_WINDOWPOSCHANGING = 0x46;
//        //发送此消息给那个窗口的大小和位置已经被改变时，来调用setwindowpos函数或其它窗口管理函数 

//        const int WM_WINDOWPOSCHANGED = 0x47;
//        //当系统将要进入暂停状态时发送此消息   
//        const int WM_POWER = 0x48;
//        //当一个应用程序传递数据给另一个应用程序时发送此消息   
//        const int WM_COPYDATA = 0x4A;
//        //当某个用户取消程序日志激活状态，提交此消息给程序   
//        const int WM_CANCELJOURNA = 0x4B;
//        //当某个控件的某个事件已经发生或这个控件需要得到一些信息时，发送此消息给它的父窗口    
//        const int WM_NOTIFY = 0x4E;
//        //当用户选择某种输入语言，或输入语言的热键改变   
//        const int WM_INPUTLANGCHANGEREQUEST = 0x50;
//        //当平台现场已经被改变后发送此消息给受影响的最顶级窗口   
//        const int WM_INPUTLANGCHANGE = 0x51;
//        //当程序已经初始化windows帮助例程时发送此消息给应用程序   
//        const int WM_TCARD = 0x52;
//        //此消息显示用户按下了F1，如果某个菜单是激活的，就发送此消息个此窗口关联的菜单，否则就发送给有焦点的窗口，如果//当前都没有焦点，就把此消息发送给//当前激活的窗口 

//        const int WM_HELP = 0x53;
//        //当用户已经登入或退出后发送此消息给所有的窗口，//当用户登入或退出时系统更新用户的具体设置信息，在用户更新设置时系统马上发送此消息 

//        const int WM_USERCHANGED = 0x54;
//        //公用控件，自定义控件和他们的父窗口通过此消息来判断控件是使用ANSI还是UNICODE结构   
//        const int WM_NOTIFYFORMAT = 0x55;
//        //当用户某个窗口中点击了一下右键就发送此消息给这个窗口   
//        //const int WM_CONTEXTMENU = ??;   
//        //当调用SETWINDOWLONG函数将要改变一个或多个 窗口的风格时发送此消息给那个窗口   
//        const int WM_STYLECHANGING = 0x7C;
//        //当调用SETWINDOWLONG函数一个或多个 窗口的风格后发送此消息给那个窗口   
//        const int WM_STYLECHANGED = 0x7D;
//        //当显示器的分辨率改变后发送此消息给所有的窗口   
//        const int WM_DISPLAYCHANGE = 0x7E;
//        //此消息发送给某个窗口来返回与某个窗口有关连的大图标或小图标的句柄   
//        const int WM_GETICON = 0x7F;
//        //程序发送此消息让一个新的大图标或小图标与某个窗口关联   
//        const int WM_SETICON = 0x80;
//        //当某个窗口第一次被创建时，此消息在WM_CREATE消息发送前发送   
//        const int WM_NCCREATE = 0x81;
//        //此消息通知某个窗口，非客户区正在销毁    
//        const int WM_NCDESTROY = 0x82;
//        //当某个窗口的客户区域必须被核算时发送此消息   
//        const int WM_NCCALCSIZE = 0x83;
//        //移动鼠标，按住或释放鼠标时发生   
//        const int WM_NCHITTEST = 0x84;
//        //程序发送此消息给某个窗口当它（窗口）的框架必须被绘制时   
//        const int WM_NCPAINT = 0x85;
//        //此消息发送给某个窗口仅当它的非客户区需要被改变来显示是激活还是非激活状态   
//        const int WM_NCACTIVATE = 0x86;
//        //发送此消息给某个与对话框程序关联的控件，widdows控制方位键和TAB键使输入进入此控件通过应 

//        const int WM_GETDLGCODE = 0x87;
//        //当光标在一个窗口的非客户区内移动时发送此消息给这个窗口 非客户区为：窗体的标题栏及窗 的边框体  

//        const int WM_NCMOUSEMOVE = 0xA0;
//        //当光标在一个窗口的非客户区同时按下鼠标左键时提交此消息   
//        const int WM_NCLBUTTONDOWN = 0xA1;
//        //当用户释放鼠标左键同时光标某个窗口在非客户区十发送此消息    
//        const int WM_NCLBUTTONUP = 0xA2;
//        //当用户双击鼠标左键同时光标某个窗口在非客户区十发送此消息   
//        const int WM_NCLBUTTONDBLCLK = 0xA3;
//        //当用户按下鼠标右键同时光标又在窗口的非客户区时发送此消息   
//        const int WM_NCRBUTTONDOWN = 0xA4;
//        //当用户释放鼠标右键同时光标又在窗口的非客户区时发送此消息   
//        const int WM_NCRBUTTONUP = 0xA5;
//        //当用户双击鼠标右键同时光标某个窗口在非客户区十发送此消息   
//        const int WM_NCRBUTTONDBLCLK = 0xA6;
//        //当用户按下鼠标中键同时光标又在窗口的非客户区时发送此消息   
//        const int WM_NCMBUTTONDOWN = 0xA7;
//        //当用户释放鼠标中键同时光标又在窗口的非客户区时发送此消息   
//        const int WM_NCMBUTTONUP = 0xA8;
//        //当用户双击鼠标中键同时光标又在窗口的非客户区时发送此消息   
//        const int WM_NCMBUTTONDBLCLK = 0xA9;
//        //WM_KEYDOWN 按下一个键  

//        const int WM_DEADCHAR = 0x103;
//        //当用户按住ALT键同时按下其它键时提交此消息给拥有焦点的窗口   
//        const int WM_SYSKEYDOWN = 0x104;
//        //当用户释放一个键同时ALT 键还按着时提交此消息给拥有焦点的窗口   
//        const int WM_SYSKEYUP = 0x105;
//        //当WM_SYSKEYDOWN消息被TRANSLATEMESSAGE函数翻译后提交此消息给拥有焦点的窗口 

//        const int WM_SYSCHAR = 0x106;
//        //当WM_SYSKEYDOWN消息被TRANSLATEMESSAGE函数翻译后发送此消息给拥有焦点的窗口 

//        const int WM_SYSDEADCHAR = 0x107;
//        //在一个对话框程序被显示前发送此消息给它，通常用此消息初始化控件和执行其它任务   
//        const int WM_INITDIALOG = 0x110;
//        //当用户选择一条菜单命令项或当某个控件发送一条消息给它的父窗口，一个快捷键被翻译   
//        const int WM_COMMAND = 0x111;
//        //当用户选择窗口菜单的一条命令或//当用户选择最大化或最小化时那个窗口会收到此消息   
//        const int WM_SYSCOMMAND = 0x112;
//        //发生了定时器事件   
//        const int WM_TIMER = 0x113;
//        //当一个窗口标准水平滚动条产生一个滚动事件时发送此消息给那个窗口，也发送给拥有它的控件   
//        const int WM_HSCROLL = 0x114;
//        //当一个窗口标准垂直滚动条产生一个滚动事件时发送此消息给那个窗口也，发送给拥有它的控件   
//        const int WM_VSCROLL = 0x115;
//        //当一个菜单将要被激活时发送此消息，它发生在用户菜单条中的某项或按下某个菜单键，它允许程序在显示前更改菜单 

//        const int WM_INITMENU = 0x116;
//        //当一个下拉菜单或子菜单将要被激活时发送此消息，它允许程序在它显示前更改菜单，而不要改变全部   
//        const int WM_INITMENUPOPUP = 0x117;
//        //当用户选择一条菜单项时发送此消息给菜单的所有者（一般是窗口）   
//        const int WM_MENUSELECT = 0x11F;
//        //当菜单已被激活用户按下了某个键（不同于加速键），发送此消息给菜单的所有者   
//        const int WM_MENUCHAR = 0x120;
//        //当一个模态对话框或菜单进入空载状态时发送此消息给它的所有者，一个模态对话框或菜单进入空载状态就是在处理完一条或几条先前的消息后没有消息它的列队中等待 

//        const int WM_ENTERIDLE = 0x121;
//        //在windows绘制消息框前发送此消息给消息框的所有者窗口，通过响应这条消息，所有者窗口可以通过使用给定的相关显示设备的句柄来设置消息框的文本和背景颜色 

//        const int WM_CTLCOLORMSGBOX = 0x132;
//        //当一个编辑型控件将要被绘制时发送此消息给它的父窗口通过响应这条消息，所有者窗口可以通过使用给定的相关显示设备的句柄来设置编辑框的文本和背景颜色 

//        const int WM_CTLCOLOREDIT = 0x133;

//        //当一个列表框控件将要被绘制前发送此消息给它的父窗口通过响应这条消息，所有者窗口可以通过使用给定的相关显示设备的句柄来设置列表框的文本和背景颜色 

//        const int WM_CTLCOLORLISTBOX = 0x134;
//        //当一个按钮控件将要被绘制时发送此消息给它的父窗口通过响应这条消息，所有者窗口可以通过使用给定的相关显示设备的句柄来设置按纽的文本和背景颜色 

//        const int WM_CTLCOLORBTN = 0x135;
//        //当一个对话框控件将要被绘制前发送此消息给它的父窗口通过响应这条消息，所有者窗口可以通过使用给定的相关显示设备的句柄来设置对话框的文本背景颜色 

//        const int WM_CTLCOLORDLG = 0x136;
//        //当一个滚动条控件将要被绘制时发送此消息给它的父窗口通过响应这条消息，所有者窗口可以通过使用给定的相关显示设备的句柄来设置滚动条的背景颜色 

//        const int WM_CTLCOLORSCROLLBAR = 0x137;
//        //当一个静态控件将要被绘制时发送此消息给它的父窗口通过响应这条消息，所有者窗口可以 
//        // 通过使用给定的相关显示设备的句柄来设置静态控件的文本和背景颜色   
//        const int WM_CTLCOLORSTATIC = 0x138;
//        //当鼠标轮子转动时发送此消息个当前有焦点的控件   
//        const int WM_MOUSEWHEEL = 0x20A;
//        //双击鼠标中键   
//        const int WM_MBUTTONDBLCLK = 0x209;
//        //释放鼠标中键   
//        const int WM_MBUTTONUP = 0x208;
//        //移动鼠标时发生，同WM_MOUSEFIRST   
//        const int WM_MOUSEMOVE = 0x200;
//        //按下鼠标左键   
//        public const int WM_LBUTTONDOWN = 0x02;
//        //释放鼠标左键   
//        public const int WM_LBUTTONUP = 0x04;
//        //双击鼠标左键   
//        public const int WM_LBUTTONDBLCLK = 0x203;
//        //按下鼠标右键   
//        public const int WM_RBUTTONDOWN = 0x204;
//        //释放鼠标右键   
//        public const int WM_RBUTTONUP = 0x205;
//        //双击鼠标右键   
//        const int WM_RBUTTONDBLCLK = 0x206;
//        //按下鼠标中键   
//        const int WM_MBUTTONDOWN = 0x207;

//        const int WM_USER = 0x0400;
//        const int MK_LBUTTON = 0x0001;
//        const int MK_RBUTTON = 0x0002;
//        const int MK_SHIFT = 0x0004;
//        const int MK_CONTROL = 0x0008;
//        const int MK_MBUTTON = 0x0010;
//        const int MK_XBUTTON1 = 0x0020;
//        const int MK_XBUTTON2 = 0x0040;

//        public const int VK_SPACE = 0x20; //空格
//        public const int VK_A = 0x64;
//        public const int VK_ONE = 0x31;//1
//        public const int VK_TWO = 0x32;//2
//        public const int VK_THR = 0x33;//3

//        //键盘消息
//        public const int VbKeyY = 0x89;
//        public const int VbKeyH = 0x72;
//        /*
//         * vbKeyA 65 A 键  
//           vbKeyB 66 B 键  
//           vbKeyC 67 C 键  
//           vbKeyD 68 D 键  
//           vbKeyE 69 E 键  
//           vbKeyF 70 F 键  
//           vbKeyG 71 G 键  
//           vbKeyH 72 H 键  
//           vbKeyI 73 I 键  
//           vbKeyJ 74 J 键  
//           vbKeyK 75 K 键  
//           vbKeyL 76 L 键  
//           vbKeyM 77 M 键  
//           vbKeyN 78 N 键  
//           vbKeyO 79 O 键  
//           vbKeyP 80 P 键  
//           vbKeyQ 81 Q 键  
//           vbKeyR 82 R 键  
//           vbKeyS 83 S 键  
//           vbKeyT 84 T 键  
//           vbKeyU 85 U 键  
//           vbKeyV 86 V 键  
//           vbKeyW 87 W 键  
//           vbKeyX 88 X 键  
//           vbKeyY 89 Y 键
//           vbKeyZ 90 Z 键  
   
//           vbKey0 48 0 键  
//           vbKey1 49 1 键  
//           vbKey2 50 2 键  
//           vbKey3 51 3 键  
//           vbKey4 52 4 键  
//           vbKey5 53 5 键  
//           vbKey6 54 6 键  
//           vbKey7 55 7 键  
//           vbKey8 56 8 键  
//           vbKey9 57 9 键 
//         * vbKeyBack 0x8 BACKSPACE 键
//         * vbKeySpace 0x20 SPACEBAR 键 
//         * vbKeyNumpad0 0x60 0 键  
//           vbKeyNumpad1 0x61 1 键  
//           vbKeyNumpad2 0x62 2 键  
//           vbKeyNumpad3 0x63 3 键  
//           vbKeyNumpad4 0x64 4 键  
//           vbKeyNumpad5 0x65 5 键  
//           vbKeyNumpad6 0x66 6 键  
//           vbKeyNumpad7 0x67 7 键  
//           vbKeyNumpad8 0x68 8 键  
//           vbKeyNumpad9 0x69 9 键 
//         * */
//        [DllImport("user32.dll")]
//        public static extern int SetCursorPos(int x, int y);
//        [DllImport("USER32.DLL")]
//        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);  //导入寻找windows窗体的方法
//        [DllImport("USER32.DLL")]
//        public static extern bool SetForegroundWindow(IntPtr hWnd);  //导入为windows窗体设置焦点的方法
//        [DllImport("USER32.DLL")]
//        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);  //导入模拟键盘的方法
//        [DllImport("user32.dll")]
//        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
//        [DllImport("user32.dll")]
//        public static extern bool mouse_event(int hand, int dx, int dy, int dwdata, int dwextralnfo);
//        [DllImport("user32.dll", EntryPoint = "PostMessageA", SetLastError = true)]
//        public static extern int PostMessage(IntPtr hWnd, int Msg, Keys wParam, int lParam);

//        //public static void getprocess(string name)
//        //{
//        //System.Diagnostics.Process[] GamesProcess = System.Diagnostics.Process.GetProcessesByName("notepad"); 
//        //if (GamesProcess.Length == 0) return; 
//        //        IntPtr hWnd = GamesProcess[0].Handle;  
//        //}
//    }
//    public enum ShortcutsCommand
//    {
//        #region 墙绘制

//        #region 建筑墙
//        /// <summary>
//        /// 建筑墙：直线
//        /// </summary>
//        BuildWall_Line,

//        /// <summary>
//        /// 建筑墙:矩形
//        /// </summary>
//        BuildWall_Rectangle,

//        /// <summary>
//        /// 建筑墙:内接多边形
//        /// </summary>
//        BuildWall_InscribedPolygon,

//        /// <summary>
//        /// 建筑墙:外接多边形
//        /// </summary>
//        BuildWall_ExternalPolygon,

//        /// <summary>
//        /// 建筑墙:圆形
//        /// </summary>
//        BuildWall_Circle,

//        /// <summary>
//        /// 建筑墙:起点-终点-半径弧
//        /// </summary>
//        BuildWall_Start_End_RoundedArc,

//        /// <summary>
//        /// 建筑墙:圆心-端点弧
//        /// </summary>
//        BuildWall_CenterCircle_EndpointArc,

//        /// <summary>
//        /// 建筑墙:相切-端点弧
//        /// </summary>
//        BuildWall_Tangent_EndpointArc,

//        /// <summary>
//        /// 建筑墙:圆角弧
//        /// </summary>
//        BuildWall_RoundedArc,

//        /// <summary>
//        /// 建筑墙:拾取线
//        /// </summary>
//        BuildWall_Pickup_Line,

//        /// <summary>
//        /// 建筑墙:拾取面
//        /// </summary>
//        BuildWall_Pickup_Surface,
//        #endregion

//        #region 结构墙
//        /// <summary>
//        /// 结构墙：直线
//        /// </summary>
//        StructWall_Line,

//        /// <summary>
//        /// 结构墙:矩形
//        /// </summary>
//        StructWall_Rectangle,

//        /// <summary>
//        /// 结构墙:内接多边形
//        /// </summary>
//        StructWall_InscribedPolygon,

//        /// <summary>
//        /// 结构墙:外接多边形
//        /// </summary>
//        StructWall_ExternalPolygon,

//        /// <summary>
//        /// 结构墙:圆形
//        /// </summary>
//        StructWall_Circle,

//        /// <summary>
//        /// 结构墙:起点-终点-半径弧
//        /// </summary>
//        StructWall_Start_End_RoundedArc,

//        /// <summary>
//        /// 结构墙:圆心-端点弧
//        /// </summary>
//        StructWall_CenterCircle_EndpointArc,

//        /// <summary>
//        /// 结构墙:相切-端点弧
//        /// </summary>
//        StructWall_Tangent_EndpointArc,

//        /// <summary>
//        /// 结构墙:圆角弧
//        /// </summary>
//        StructWall_RoundedArc,

//        /// <summary>
//        /// 结构墙:拾取线
//        /// </summary>
//        StructWall_Pickup_Line,

//        /// <summary>
//        /// 结构墙:拾取面
//        /// </summary>
//        StructWall_Pickup_Surface,
//        #endregion

//        #endregion

//        #region 系统默认
//        /// <summary>
//        /// 默认值:直线
//        /// </summary>
//        System_Line,
//        /// <summary>
//        /// 默认值:起点-终点-半径弧
//        /// </summary>
//        System_Start_End_RoundedArc,
//        /// <summary>
//        /// 默认值:圆心-端点弧
//        /// </summary>
//        System_CenterCircle_EndpointArc,
//        /// <summary>
//        /// 默认值:相切-端点弧
//        /// </summary>
//        System_Tangent_EndpointArc,
//        /// <summary>
//        /// 默认值:圆角弧
//        /// </summary>
//        System_RoundedArc,
//        /// <summary>
//        /// 默认值:样条曲线
//        /// </summary>
//        System_SplineCurve,
//        /// <summary>
//        /// 默认值:半椭圆
//        /// </summary>
//        System_HalfEllipse,
//        /// <summary>
//        /// 默认值:拾取线
//        /// </summary>
//        System_PickupLine,
//        #endregion

//        #region 梁绘制
//        /// <summary>
//        /// 在轴网线上
//        /// </summary>
//        Beam_OnGridLine,
//        #endregion

//        #region 柱绘制
//        /// <summary>
//        /// 垂直柱
//        /// </summary>
//        Pole_vertical,
//        #endregion

//        #region 直线
//        /// <summary>
//        /// 直线
//        /// </summary>
//        Line,
//        /// <summary>
//        /// 直线:矩形
//        /// </summary>
//        Line_Rectangle,
//        /// <summary>
//        /// 直线:内接多边形
//        /// </summary>
//        Line_InscribedPolygon,
//        /// <summary>
//        /// 直线:外接多边形
//        /// </summary>
//        Line_ExternalPolygon,
//        /// <summary>
//        /// 直线:圆形
//        /// </summary>
//        Line_Circle,
//        /// <summary>
//        /// 直线:起点-终点-半径弧
//        /// </summary>
//        Line_Start_End_RoundedArc,
//        /// <summary>
//        /// 直线:圆心-端点弧
//        /// </summary>
//        Line_CenterCircle_EndpointArc,
//        /// <summary>
//        /// 直线:相切-端点弧
//        /// </summary>
//        Line_Tangent_EndpointArc,
//        /// <summary>
//        /// 直线:圆角弧
//        /// </summary>
//        Line_RoundedArc,
//        /// <summary>
//        /// 直线:样条曲线
//        /// </summary>
//        Line_SplineCurve,
//        /// <summary>
//        /// 直线:椭圆
//        /// </summary>
//        Line_Ellipse,
//        /// <summary>
//        /// 直线:半椭圆
//        /// </summary>
//        Line_HalfEllipse,
//        /// <summary>
//        /// 直线:拾取线
//        /// </summary>
//        Line_PickupLine,
//        /// <summary>
//        /// 直线:拾取现有墙
//        /// </summary>
//        Line_PickupWall,
//        #endregion

//        #region 风管
//        /// <summary>
//        /// 继承高程
//        /// </summary>
//        Inherit_High,

//        #endregion

//        #region 水管
//        /// <summary>
//        /// 自动连接
//        /// </summary>
//        Auto_Connect,
//        Slopepipe_Off,

//        #endregion

//        #region 标注
//        /// <summary>
//        /// 编辑 尺寸边界
//        /// </summary>
//        Dimension_Add,
//        /// <summary>
//        /// 快速标注
//        /// </summary>
//        Dimension_Speed,
//        #endregion

//        #region 操作
//        /// <summary>
//        /// 取消编辑
//        /// </summary>
//        Operate_CancleEdit,
//        #endregion
//        #region 楼梯
//        /// <summary>
//        /// 创建楼梯：按草图
//        /// </summary>
//        BuildStair_Sketch,
//        #endregion
//        /// <summary>
//        /// 放置工作平面
//        /// </summary>
//        Place_OnWorkPlane
//    }
//    /// <summary>
//    /// 执行键盘命令
//    /// </summary>
//    public class RevitKeyboardCommand
//    {
//        private static RevitKeyboardCommand instance;
//        private static object _lock = new object();
//        private Dictionary<string, string> keyboardShortcuts;

//        public static RevitKeyboardCommand GetInstance(Dictionary<string, string> keyboardShortcuts)
//        {
//            if (instance == null)
//            {
//                lock (_lock)
//                {
//                    if (instance == null)
//                    {
//                        instance = new RevitKeyboardCommand(keyboardShortcuts);

//                    }
//                }
//            }
//            return instance;
//        }

//        public RevitKeyboardCommand(Dictionary<string, string> keyboardShortcuts)
//        {
//            this.keyboardShortcuts = keyboardShortcuts;
//        }

//        /// <summary>
//        /// 向Revit发送Esc键盘消息，以取消当前操作
//        /// </summary>
//        public static void PostEscCommand()
//        {
//            Process main = Process.GetCurrentProcess();
//            IntPtr revithandle = main.MainWindowHandle;
//            //MouseKeyBoardControl.SetForegroundWindow(revithandle);
//            MouseKeyBoardControl.PostMessage(revithandle, 256, Keys.Escape, 2);
//        }


//        /// <summary>
//        /// 执行键盘命令
//        /// </summary>
//        /// <param name="command"></param>
//        public void DoKeyboard(ShortcutsCommand command)
//        {
//            Process[] ps = Process.GetProcessesByName("Revit");
//            Process main = Process.GetCurrentProcess();
//            IntPtr revithandle = main.MainWindowHandle;
//            MouseKeyBoardControl.SetForegroundWindow(revithandle);
//            string strCommand = GetShortcuts(command);
//            for (int i = 0; i < strCommand.Length; i++)
//            {
//                MouseKeyBoardControl.PostMessage(revithandle, 256, GetKeys(strCommand[i]), 2);
//                // MouseKeyBoardControl.PostMessage(revithandle, MouseKeyBoardControl.WM_KEYDOWN, GetKeys(strCommand[i]), 0);
//                //  MouseKeyBoardControl.PostMessage(revithandle, MouseKeyBoardControl.WM_KEYUP, GetKeys(strCommand[i]), 0);
//                //MouseKeyBoardControl.SendMessage(revithandle, MouseKeyBoardControl.WM_KEYDOWN, (int)GetKeys(strCommand[i]), 0);
//                //  MouseKeyBoardControl.SendMessage(revithandle, MouseKeyBoardControl.WM_KEYUP, (int)GetKeys(strCommand[i]), 0);
//            }
//        }

//        /// <summary>
//        /// 获取快捷键
//        /// </summary>
//        /// <param name="command"></param>
//        /// <returns></returns>
//        private string GetShortcuts(ShortcutsCommand command)
//        {
//            try
//            {
//                switch (command)
//                {
//                    #region 建筑墙
//                    case ShortcutsCommand.BuildWall_Line: return keyboardShortcuts["ID_WALL_LINE"];
//                    case ShortcutsCommand.BuildWall_Rectangle: return keyboardShortcuts["ID_WALL_RECT"];
//                    case ShortcutsCommand.BuildWall_InscribedPolygon: return keyboardShortcuts["ID_WALL_POLY_INSCRIBED"];
//                    case ShortcutsCommand.BuildWall_ExternalPolygon: return keyboardShortcuts["ID_WALL_POLY_CIRCUMSCRIBED"];
//                    case ShortcutsCommand.BuildWall_Circle: return keyboardShortcuts["ID_WALL_CIRCLE"];
//                    case ShortcutsCommand.BuildWall_Start_End_RoundedArc: return keyboardShortcuts["ID_WALL_ARC_THREE_PNT"];
//                    case ShortcutsCommand.BuildWall_Tangent_EndpointArc: return keyboardShortcuts["ID_WALL_ARC_TAN"];
//                    case ShortcutsCommand.BuildWall_RoundedArc: return keyboardShortcuts["ID_WALL_ARC_FILLET"];
//                    case ShortcutsCommand.BuildWall_Pickup_Line: return keyboardShortcuts["ID_WALL_PICK_LINES"];
//                    case ShortcutsCommand.BuildWall_Pickup_Surface: return keyboardShortcuts["ID_WALL_PICK_FACES"];
//                    case ShortcutsCommand.BuildWall_CenterCircle_EndpointArc: return keyboardShortcuts["ID_WALL_ARC_CENTER_ENDS"];
//                    #endregion
//                    #region 结构墙
//                    case ShortcutsCommand.StructWall_Line: return keyboardShortcuts["ID_WALL_STRUCT_LINE"];
//                    case ShortcutsCommand.StructWall_Rectangle: return keyboardShortcuts["ID_WALL_STRUCT_RECT"];
//                    case ShortcutsCommand.StructWall_InscribedPolygon: return keyboardShortcuts["ID_WALL_STRUCT_POLY_INSCRIBED"];
//                    case ShortcutsCommand.StructWall_ExternalPolygon: return keyboardShortcuts["ID_WALL_STRUCT_POLY_CIRCUMSCRIBED"];
//                    case ShortcutsCommand.StructWall_Circle: return keyboardShortcuts["ID_WALL_STRUCT_CIRCLE"];
//                    case ShortcutsCommand.StructWall_Start_End_RoundedArc: return keyboardShortcuts["ID_WALL_STRUCT_ARC_THREE_PNT"];
//                    case ShortcutsCommand.StructWall_CenterCircle_EndpointArc: return keyboardShortcuts["ID_WALL_STRUCT_ARC_CENTER_ENDS"];
//                    case ShortcutsCommand.StructWall_Tangent_EndpointArc: return keyboardShortcuts["ID_WALL_STRUCT_ARC_TAN"];
//                    case ShortcutsCommand.StructWall_RoundedArc: return keyboardShortcuts["ID_WALL_STRUCT_ARC_FILLET"];
//                    case ShortcutsCommand.StructWall_Pickup_Line: return keyboardShortcuts["ID_WALL_STRUCT_PICK_LINES"];
//                    case ShortcutsCommand.StructWall_Pickup_Surface: return keyboardShortcuts["ID_WALL_STRUCT_PICK_FACES"];
//                    #endregion
//                    #region 梁
//                    case ShortcutsCommand.System_Line: return keyboardShortcuts["IDC_RADIO_LINE"];
//                    case ShortcutsCommand.System_Start_End_RoundedArc: return keyboardShortcuts["IDC_RADIO_ARC_3_PNT"];
//                    case ShortcutsCommand.System_CenterCircle_EndpointArc: return keyboardShortcuts["IDC_RADIO_ARC_CENTER"];
//                    case ShortcutsCommand.System_Tangent_EndpointArc: return keyboardShortcuts["IDC_RADIO_ARC_TAN_END"];
//                    case ShortcutsCommand.System_RoundedArc: return keyboardShortcuts["IDC_RADIO_ARC_FILLET"];
//                    case ShortcutsCommand.System_SplineCurve: return keyboardShortcuts["IDC_RADIO_SPLINE"];
//                    case ShortcutsCommand.System_HalfEllipse: return keyboardShortcuts["IDC_RADIO_PARTIAL_ELLIPSE"];
//                    case ShortcutsCommand.System_PickupLine: return keyboardShortcuts["IDC_RADIO_COPY_CURVE"];
//                    case ShortcutsCommand.Beam_OnGridLine: return keyboardShortcuts["Dialog_Structural_NewStructuralBeam:Control_Structural_CreateBeamOnGrid"];
//                    #endregion
//                    #region 楼板
//                    case ShortcutsCommand.Line: return keyboardShortcuts["ID_OBJECTS_CURVE_LINE"];
//                    case ShortcutsCommand.Line_Rectangle: return keyboardShortcuts["ID_OBJECTS_CURVE_RECT"];
//                    case ShortcutsCommand.Line_InscribedPolygon: return keyboardShortcuts["ID_OBJECTS_CURVE_POLY_INSCRIBED"];
//                    case ShortcutsCommand.Line_ExternalPolygon: return keyboardShortcuts["ID_OBJECTS_CURVE_POLY_CIRCUMSCRIBED"];
//                    case ShortcutsCommand.Line_Circle: return keyboardShortcuts["ID_OBJECTS_CURVE_CIRCLE"];
//                    case ShortcutsCommand.Line_Start_End_RoundedArc: return keyboardShortcuts["ID_OBJECTS_CURVE_ARC_THREE_PNT"];
//                    case ShortcutsCommand.Line_CenterCircle_EndpointArc: return keyboardShortcuts["ID_OBJECTS_CURVE_ARC_CENTER_ENDS"];
//                    case ShortcutsCommand.Line_Tangent_EndpointArc: return keyboardShortcuts["ID_OBJECTS_CURVE_ARC_TAN"];
//                    case ShortcutsCommand.Line_RoundedArc: return keyboardShortcuts["ID_OBJECTS_CURVE_ARC_FILLET"];
//                    case ShortcutsCommand.Line_SplineCurve: return keyboardShortcuts["ID_OBJECTS_CURVE_SPLINE"];
//                    case ShortcutsCommand.Line_Ellipse: return keyboardShortcuts["ID_OBJECTS_CURVE_ELLIPSE"];
//                    case ShortcutsCommand.Line_HalfEllipse: return keyboardShortcuts["ID_OBJECTS_CURVE_ELLIPSE_PARTIAL"];
//                    case ShortcutsCommand.Line_PickupLine: return keyboardShortcuts["ID_OBJECTS_CURVE_PICK_LINES"];
//                    case ShortcutsCommand.Line_PickupWall: return keyboardShortcuts["ID_SKETCH_PICK_WALLS"];
//                    #endregion
//                    #region 柱
//                    case ShortcutsCommand.Pole_vertical: return keyboardShortcuts["Dialog_Structural_VerticalSlantedColumnsDialogBar:Control_Structural_VerticalColumns"];
//                    #endregion
//                    #region 风管
//                    case ShortcutsCommand.Inherit_High: return keyboardShortcuts["Dialog_BuildingSystems_RbsLayoutOptionsBar:Control_BuildingSystems_InheritElevation"];
//                    #endregion
//                    #region 水管
//                    case ShortcutsCommand.Auto_Connect: return keyboardShortcuts["Dialog_BuildingSystems_RbsLayoutOptionsBar:Control_BuildingSystems_RbsBtnAutoconnect"];
//                    case ShortcutsCommand.Slopepipe_Off: return keyboardShortcuts["ID_SLOPE_PIPE_OFF"];
//                    #endregion
//                    #region 操作
//                    case ShortcutsCommand.Operate_CancleEdit: return keyboardShortcuts["ID_CANCEL_SKETCH"];
//                    #endregion
//                    #region 标注
//                    case ShortcutsCommand.Dimension_Add:
//                        return keyboardShortcuts["IDC_EDIT_WITNESS_REFS"];
//                    case ShortcutsCommand.Dimension_Speed:
//                        return keyboardShortcuts["ID_ANNOTATIONS_DIMENSION_ALIGNED"];
//                    #endregion
//                    #region 楼梯
//                    case ShortcutsCommand.BuildStair_Sketch: return keyboardShortcuts["ID_OBJECTS_STAIRS_LEGACY"];
//                    #endregion
//                    case ShortcutsCommand.Place_OnWorkPlane:
//                        return keyboardShortcuts["Dialog_Essentials_GenhostCreate:Control_Essentials_PlaceByWorkplane"];
//                    default: return "";
//                }
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }

//        /// <summary>
//        /// 获取系统按键
//        /// </summary>
//        /// <param name="c"></param>
//        /// <returns></returns>
//        private Keys GetKeys(char c)
//        {
//            switch (c)
//            {
//                case 'A': return Keys.A;
//                case 'B': return Keys.B;
//                case 'C': return Keys.C;
//                case 'D': return Keys.D;
//                case 'E': return Keys.E;
//                case 'F': return Keys.F;
//                case 'G': return Keys.G;
//                case 'H': return Keys.H;
//                case 'I': return Keys.I;
//                case 'J': return Keys.J;
//                case 'K': return Keys.K;
//                case 'L': return Keys.L;
//                case 'M': return Keys.M;
//                case 'N': return Keys.N;
//                case 'O': return Keys.O;
//                case 'P': return Keys.P;
//                case 'Q': return Keys.Q;
//                case 'R': return Keys.R;
//                case 'S': return Keys.S;
//                case 'T': return Keys.T;
//                case 'U': return Keys.U;
//                case 'V': return Keys.V;
//                case 'W': return Keys.W;
//                case 'X': return Keys.X;
//                case 'Y': return Keys.Y;
//                case 'Z': return Keys.Z;
//                default: return Keys.Space;
//            }
//        }
//    } 
//    #endregion
//    public class PickObjectsMouseHook : MouseHookExt
//    {
//        public enum OKModeENUM { Objects, Object }
//        private ButtonHandle handle = null;
//        private OKModeENUM mode = OKModeENUM.Objects;

//        public PickObjectsMouseHook()
//        {
//            handle = new ButtonHandle();
//            this.RightButtonDown += PickObjectsMouseHook_RightButtonDown;
//            this.RightButtonUp += PickObjectsMouseHook_RightButtonUp;
//        }

//        public void InstallHook(OKModeENUM mode)
//        {
//            this.mode = mode;
//            this.InstallHook();
//        }

//        bool PickObjectsMouseHook_RightButtonDown(System.Drawing.Point pt)
//        {
//            return true;
//        }

//        bool PickObjectsMouseHook_RightButtonUp(System.Drawing.Point pt)
//        {
//            this.UninstallHook();

//            if (this.mode == OKModeENUM.Objects)
//            {
//                handle = new ButtonHandle();
//                handle.Mouse_RightButtonDown(pt);
//            }
//            else
//                RevitKeyboardCommand.PostEscCommand();

//            return true;
//        }
//    }
//}
