using Autodesk.Revit.UI;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MyRevit.Utilities
{

    /// <summary>
    /// DrawAreaView.xaml 的交互逻辑
    /// </summary>
    public partial class DrawAreaView : Window
    {
        public UIView UIView { set; get; }
#if Revit2016
        public Autodesk.Revit.UI.Rectangle Rect { set; get; }
#else
        public Autodesk.Revit.DB.Rectangle Rect { set; get; }
#endif
        public double co_s;
        public double co_e;
        VLCoordinateType CoordinateType { set; get; }

        public DrawAreaView(UIApplication uiApp, VLCoordinateType coordinateType = VLCoordinateType.XY)
        {
            UIView = uiApp.ActiveUIDocument.GetOpenUIViews().FirstOrDefault(p => p.ViewId == uiApp.ActiveUIDocument.ActiveGraphicalView.Id);
            CoordinateType = coordinateType;

            InitializeComponent();

            new System.Windows.Interop.WindowInteropHelper(this).Owner = Autodesk.Windows.ComponentManager.ApplicationWindow;

            Rect = UIView.GetWindowRectangle();
            this.Left = Rect.Left;
            this.Top = Rect.Top;
            this.Width = Rect.Right - Rect.Left;
            this.Height = Rect.Bottom - Rect.Top;
            co_s = co_e = Height / 400;
        }

        /// <summary>
        /// 将revit坐标转换为绘图坐标
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Point ConvertToDrawPointFromRevitPoint(Point p)
        {
            var rect = UIView.GetWindowRectangle();
            var xyzs = UIView.GetZoomCorners();
            return new Point((p.X - xyzs.First().X) * (rect.Right - rect.Left) / (xyzs.Last().X - xyzs.First().X), (p.Y - xyzs.Last().Y) * (rect.Bottom - rect.Top) / (xyzs.First().Y - xyzs.Last().Y));
        }
        /// <summary>
        /// 将绘图坐标转换为revit坐标
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Autodesk.Revit.DB.XYZ ConvertToRevitPointFromDrawPoint(Point p)
        {
            var rect = UIView.GetWindowRectangle();
            var xyzs = UIView.GetZoomCorners();
            return new Autodesk.Revit.DB.XYZ(p.X * (xyzs.Last().X - xyzs.First().X) / (rect.Right - rect.Left) + xyzs.First().X, (p.Y) * (xyzs.First().Y - xyzs.Last().Y) / (rect.Bottom - rect.Top) + xyzs.Last().Y, 0);
        }
    }
}
