using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.Utilities;
using PmSoft.Common.RevitClass;
using System;

namespace MyRevit.MyTests.PipeAttributesAnnotation
{
    public static class VLPointPickerEx
    {
        public static System.Windows.Point ToWindowsPoint(this XYZ xyz)
        {
            return new System.Windows.Point(xyz.X, xyz.Y);
        }
    }

    public class VLPointPicker
    {
        /// <summary>
        /// 选择点,带预览,空时返回null
        /// </summary>
        /// <param name="uiApp"></param>
        /// <param name="startPoint"></param>
        /// <returns></returns>
        public XYZ PickPointWithPreview(UIApplication uiApp, Action<DrawAreaView> preview=null)
        {
            XYZ result = null;
            Document doc = uiApp.ActiveUIDocument.Document;
            PickObjectsMouseHook mouseHook = null;
            mouseHook = InitMouseHook();
            System.Windows.Forms.Timer timer = null;
            var view = new DrawAreaView(uiApp);
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
            //选点
            try
            {
                result = uiApp.ActiveUIDocument.Selection.PickPoint("PM-预览绘线中，鼠标左键确定,右键取消");//Autodesk.Revit.UI.Selection.ObjectSnapTypes.Endpoints,
            }
            catch
            {
                mouseHook.UninstallHook();
            }
            timer.Stop();
            view.Close();
            mouseHook.Dispose();
            mouseHook = null;
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
