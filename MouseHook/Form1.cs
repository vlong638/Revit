
///Author:danseshi
///Email:danseshi@yahoo.com.cn
///Bolg:http://blog.csdn.net/danseshi/
///Date:2008.7.12


using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using MouseHook.VLHooker;

namespace MouseHook
{



    public partial class Form1 : Form
    {
        Hooker Hooker = new Hooker();
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 安装钩子
        /// </summary>
        private void StartHook()
        {
            Hooker.Start();

            //if (Hooker.hMouseHook == 0)
            //{
            //    Hooker.hMouseHook = Hooker.SetWindowsHookEx(Hooker.WH_MOUSE_LL, Hooker.MouseHookProcedure, Hooker.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
            //    if (Hooker.hMouseHook == 0)
            //    {//如果设置钩子失败. 

            //        this.StopHook();
            //        MessageBox.Show("Set windows hook failed!");
            //    }
            //}
        }

        /// <summary>
        /// 卸载钩子
        /// </summary>
        private void StopHook()
        {
            Hooker.Stop();

            //bool stop = true;
            //if (hMouseHook != 0)
            //{
            //    stop = UnhookWindowsHookEx(hMouseHook);
            //    hMouseHook = 0;

            //    if (!stop)
            //    {//卸载钩子失败

            //        MessageBox.Show("Unhook failed!");
            //    }
            //}
        }

        private int MouseHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                //把参数lParam在内存中指向的数据转换为MOUSEHOOKSTRUCT结构
                MOUSEHOOKSTRUCT mouse = (MOUSEHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MOUSEHOOKSTRUCT));//鼠标
                //这句为了看鼠标的位置
                this.Text = "MousePosition:" + mouse.pt.ToString();
                if (wParam == Hooker.WM_RBUTTONDOWN || wParam == Hooker.WM_RBUTTONUP)
                { //鼠标按下或者释放时候截获

                    if (newTaskBarRect.Contains(mouse.pt))
                    { //当鼠标在任务栏的范围内

                        //如果返回1，则结束消息，这个消息到此为止，不再传递。
                        //如果返回0或调用CallNextHookEx函数则消息出了这个钩子继续往下传递，也就是传给消息真正的接受者
                        return 1;
                    }
                }
            }
            return Hooker.CallNextHookEx(Hooker.hMouseHook, nCode, wParam, lParam);
        }


        #region Events

        Rectangle newTaskBarRect = new Rectangle();
        private void Form1_Load(object sender, EventArgs e)
        {
            Hooker.MouseHookProcedure = new Hooker.HookProc(MouseHookProc);

            //taskBarHandle为返回的任务栏的句柄
            //Shell_TrayWnd为任务栏的类名
            int taskBarHandle = Hooker.FindWindow("Shell_TrayWnd", null);

            //获得任务栏的区域
            //有一点要注意，函数返回时，taskBarRect包含的是窗口的左上角和右下角的屏幕坐标
            //就是说taskBarRect.Width和taskBarRect.Height是相对于屏幕左上角（0，0）的数值
            //这与c#的Rectangle结构是不同的

            //保存任务栏的矩形区域
            Rectangle taskBarRect = new Rectangle();
            Hooker.GetWindowRect(taskBarHandle, ref taskBarRect);
            this.richTextBox1.Text = "taskBarRect.Location:" + taskBarRect.Location.ToString() + "\n";
            this.richTextBox1.Text += "taskBarRect.Size:" + taskBarRect.Size.ToString() + "\n\n";

            //构造一个c#中的Rectangle结构
            newTaskBarRect = new Rectangle(
            taskBarRect.X,
            taskBarRect.Y,
            taskBarRect.Width - taskBarRect.X,
            taskBarRect.Height - taskBarRect.Y
            );

            this.richTextBox1.Text += "newTaskBarRect.Location:" + newTaskBarRect.Location.ToString() + "\n";
            this.richTextBox1.Text += "newTaskBarRect.Size:" + newTaskBarRect.Size.ToString();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.StopHook();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var parent = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            var handle = Hooker.FindWindowEx(parent, IntPtr.Zero, null, "安装钩子");


            //this.StartHook();
            //this.button1.Enabled = false;
            //this.button2.Enabled = true;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.StopHook();
            this.button1.Enabled = true;
            this.button2.Enabled = false;
        }


        #endregion


    }
}