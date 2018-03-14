using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.Utilities;
using PmSoft.Common.RevitClass;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MyRevit.Utilities
{
    public static class VLPointPickerEx
    {
        public static System.Windows.Point ToWindowsPoint(this XYZ xyz, VLCoordinateType coordinateType = VLCoordinateType.XY)
        {
            switch (coordinateType)
            {
                case VLCoordinateType.XY:
                    return new System.Windows.Point(xyz.X, xyz.Y);
                case VLCoordinateType.YZ:
                    return new System.Windows.Point(xyz.Y, xyz.Z);
                case VLCoordinateType.XZ:
                    return new System.Windows.Point(xyz.X, xyz.Z);
                default:
                    throw new NotImplementedException();
            }
        }

        public static System.Windows.Point Plus(this System.Windows.Point start, System.Windows.Point end)
        {
            return new System.Windows.Point(start.X + end.X, start.Y + end.Y);
        }
        public static System.Windows.Point Minus(this System.Windows.Point start, System.Windows.Point end)
        {
            return new System.Windows.Point(start.X - end.X, start.Y - end.Y);
        }
        public static System.Windows.Point Minus(this System.Drawing.Point start, System.Windows.Point end)
        {
            return new System.Windows.Point(start.X - end.X, start.Y - end.Y);
        }

        /// <summary>
        /// 小 start在end左侧
        /// 大 start在end右侧
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static double Dot(this System.Windows.Point start, System.Windows.Point end)
        {
            return start.X * end.Y - start.Y * end.X;
        }
        public static double Dot(this System.Windows.Point start, System.Windows.Vector end)
        {
            return start.X * end.Y - start.Y * end.X;
        }
        public static double Dot(this System.Windows.Vector start, System.Windows.Vector end)
        {
            return start.X * end.Y - start.Y * end.X;
        }
    }


    public class VLPointPicker
    {
        ///// <summary>
        ///// 选择点,带预览,空时返回null
        ///// </summary>
        ///// <param name="uiApp"></param>
        ///// <param name="startPoint"></param>
        ///// <returns></returns>
        //public XYZ PickPointWithPreview(UIApplication uiApp, CoordinateType coordinateType, Action<DrawAreaView> preview = null)
        //{
        //    XYZ result = null;
        //    Document doc = uiApp.ActiveUIDocument.Document;
        //    PickObjectsMouseHook mouseHook = null;
        //    mouseHook = InitMouseHook();
        //    System.Windows.Forms.Timer timer = null;
        //    var view = new DrawAreaView(uiApp, coordinateType);
        //    view.Show();
        //    //开启定时器 实时绘图
        //    timer = new System.Windows.Forms.Timer();
        //    timer.Interval = 6;
        //    timer.Tick += (sender, e) =>
        //    {
        //        if (preview != null)
        //            preview(view);
        //    };
        //    timer.Start();
        //    //选点
        //    try
        //    {
        //        result = uiApp.ActiveUIDocument.Selection.PickPoint("PM-预览绘线中，鼠标左键确定,右键取消");//Autodesk.Revit.UI.Selection.ObjectSnapTypes.Endpoints,
        //    }
        //    catch
        //    {
        //        mouseHook.UninstallHook();
        //    }
        //    timer.Stop();
        //    view.Close();
        //    mouseHook.Dispose();
        //    mouseHook = null;
        //    return result;
        //}

        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
        public static extern int GetForegroundWindow();
        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern int SetForegroundWindow(int hwnd);

        /// <summary>
        /// 选择点,带预览,空时返回null
        /// </summary>
        /// <param name="uiApp"></param>
        /// <param name="startPoint"></param>
        /// <returns></returns>
        public XYZ PickPointWithPreview(UIApplication uiApp, VLCoordinateType coordinateType, Action<DrawAreaView> preview = null)
        {
            XYZ result = null;
            Document doc = uiApp.ActiveUIDocument.Document;

            #region old鼠标右键取消
            //PickObjectsMouseHook mouseHook = null;
            //mouseHook = InitMouseHook();
            //try
            //{
            //    result = uiApp.ActiveUIDocument.Selection.PickPoint("PM-预览绘线中，鼠标左键确定,右键取消");//Autodesk.Revit.UI.Selection.ObjectSnapTypes.Endpoints,
            //}
            //catch
            //{
            //    mouseHook.UninstallHook();
            //}
            //mouseHook.Dispose();
            //mouseHook = null; 
            #endregion

            //选点
            System.Windows.Forms.Timer timer = null;
            DrawAreaView view = null;
            VLHookHelper.DelegateKeyBoardHook(() =>
            {
                IntPtr intPtr = Process.GetCurrentProcess().MainWindowHandle;
                view = new DrawAreaView(uiApp, coordinateType);
                view.Show();
                //开启定时器 实时绘图
                timer = new System.Windows.Forms.Timer();
                timer.Interval = 6;
                timer.Tick += (sender, e) =>
                {
                    if (preview != null)
                        preview(view);
                };
                timer.Start();
                SetForegroundWindow(intPtr.ToInt32());
                result = uiApp.ActiveUIDocument.Selection.PickPoint("PM-预览绘线中，鼠标左键确定,ESC取消");
            });
            timer.Stop();
            view.Close();
            return result;
        }

        private static PickObjectsMouseHook InitMouseHook()
        {
            PickObjectsMouseHook mouseHook;
            try
            {
                mouseHook = new PickObjectsMouseHook();
                mouseHook.InstallHook(PickObjectsMouseHook.OKModeENUM.Object);
            }
            catch
            {
                mouseHook = new PickObjectsMouseHook();
                mouseHook.InstallHook(PickObjectsMouseHook.OKModeENUM.Object);
            }
            return mouseHook;
        }
    }
}
