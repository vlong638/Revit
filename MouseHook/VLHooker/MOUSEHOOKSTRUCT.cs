using System.Drawing;
using System.Runtime.InteropServices;

namespace MouseHook.VLHooker
{
    //鼠标结构，保存了鼠标的信息
    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEHOOKSTRUCT
    {
        public Point pt;
        public int hwnd;
        public int wHitTestCode;
        public int dwExtraInfo;
    }
}
