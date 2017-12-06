using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.Utilities;
using PmSoft.Common.RevitClass;
using System;

namespace MyRevit.Utilities
{
    public static class VLPointPickerEx
    {
        public static System.Windows.Point ToWindowsPoint(this XYZ xyz, CoordinateType coordinateType)
        {
            switch (coordinateType)
            {
                case CoordinateType.XY:
                    return new System.Windows.Point(xyz.X, xyz.Y);
                case CoordinateType.YZ:
                    return new System.Windows.Point(xyz.Y, xyz.Z);
                case CoordinateType.XZ:
                    return new System.Windows.Point(xyz.X, xyz.Z);
                default:
                    throw new NotImplementedException();
            }
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
        public XYZ PickPointWithPreview(UIApplication uiApp, CoordinateType coordinateType, Action<DrawAreaView> preview=null)
        {
            XYZ result = null;
            Document doc = uiApp.ActiveUIDocument.Document;
            PickObjectsMouseHook mouseHook = null;
            mouseHook = InitMouseHook();
            System.Windows.Forms.Timer timer = null;
            var view = new DrawAreaView(uiApp, coordinateType);
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
            catch (Exception ex)
            {
                if (ex.Message.Contains("No work plane set in current view."))
                {
                    timer.Stop();
                    uiApp.ActiveUIDocument.Selection.Dispose();
                    VLTransactionHelper.DelegateTransaction(doc, "PickPointWithPreview_CreateWorkPlaneSet", () =>
                     {
                         Plane plane = new Plane(uiApp.ActiveUIDocument.Document.ActiveView.ViewDirection, uiApp.ActiveUIDocument.Document.ActiveView.Origin);
                         SketchPlane sp = SketchPlane.Create(doc, plane);
                         doc.ActiveView.SketchPlane = sp;
                         doc.ActiveView.ShowActiveWorkPlane();
                         timer.Start();
                         try
                         {
                             result = uiApp.ActiveUIDocument.Selection.PickPoint("PM-预览绘线中，鼠标左键确定,右键取消");//Autodesk.Revit.UI.Selection.ObjectSnapTypes.Endpoints,
                         }
                         catch
                         {
                             mouseHook.UninstallHook();
                         }
                         return true;
                     });
                }
                else
                {
                    mouseHook.UninstallHook();
                }
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
