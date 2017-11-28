using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.Utilities;
using PmSoft.Common.RevitClass;

namespace MyRevit.MyTests.PipeAttributesAnnotation
{
    public class VLPointPicker
    {
        /// <summary>
        /// 选择点,带预览,空时返回null
        /// </summary>
        /// <param name="uiApp"></param>
        /// <param name="startPoint"></param>
        /// <returns></returns>
        public XYZ PickPointWithLinePreview(UIApplication uiApp,XYZ startPoint,XYZ endPoint)
        {
            XYZ result = null;
            Document doc = uiApp.ActiveUIDocument.Document;
            PickObjectsMouseHook mouseHook = null;
            mouseHook = InitMouseHook();
            System.Windows.Forms.Timer timer = null;
            var view = new DrawAreaView(uiApp);
            view.Show();
            view.StartPoint = new System.Windows.Point(startPoint.X, startPoint.Y);
            view.EndPoint = new System.Windows.Point(endPoint.X, endPoint.Y);
            //开启定时器 实时绘图
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 6;
            timer.Tick += (sender, e) =>
            {
                view.PreviewLine(System.Windows.Forms.Control.MousePosition);
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
