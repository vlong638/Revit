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
        public Point? StartPoint { set; get; }
        UIView UIView { set; get; }
        public Point? EndPoint { get; internal set; }
        public double Offset { get; private set; }

        public DrawAreaView(UIApplication uiApp)
        {
            UIView = uiApp.ActiveUIDocument.GetOpenUIViews().FirstOrDefault(p => p.ViewId == uiApp.ActiveUIDocument.ActiveGraphicalView.Id);

            InitializeComponent();

            new System.Windows.Interop.WindowInteropHelper(this).Owner = Autodesk.Windows.ComponentManager.ApplicationWindow;
            var rect = this.UIView.GetWindowRectangle();
            this.Left = rect.Left;
            this.Top = rect.Top;
            this.Width = rect.Right - rect.Left;
            this.Height = rect.Bottom - rect.Top;
        }

        /// <summary>
        /// 绘制过程
        /// </summary>
        /// <param name="p"></param>
        public void PreviewLine(System.Drawing.Point p)
        {
            //已在初始化处理 为何重复?
            var rect = this.UIView.GetWindowRectangle();
            var Left = rect.Left;
            var Top = rect.Top;
            var Width = rect.Right - rect.Left;
            var Height = rect.Bottom - rect.Top;

            if (this.StartPoint == null)
                return;

            var startDrawP = ConvertToDrawPointFromViewPoint(StartPoint.Value);//起点
            var endP = ConvertToDrawPointFromViewPoint(EndPoint.Value);//终点
            var midDrawP = new Point(p.X - rect.Left, p.Y - rect.Top);//中间选择点
            var height = midDrawP - startDrawP;
            midDrawP -= height / 50;
            var endDrawP = endP + height;// new Point(height.X + EndPoint.Value.X, height.Y + EndPoint.Value.Y);
            if (Math.Abs(startDrawP.X - midDrawP.X) < 2 && Math.Abs(startDrawP.Y - midDrawP.Y) < 2)
                return;

            midDrawP.X = midDrawP.X > startDrawP.X ? midDrawP.X - 1 : midDrawP.X + 1;
            midDrawP.Y = midDrawP.Y > startDrawP.Y ? midDrawP.Y - 1 : midDrawP.Y + 1;
            canvas.Children.RemoveRange(0, this.canvas.Children.Count);
            var line = new Line() { X1 = startDrawP.X, Y1 = startDrawP.Y, X2 = midDrawP.X, Y2 = midDrawP.Y, Stroke = new SolidColorBrush(Color.FromRgb(136, 136, 136)), StrokeThickness = 1 };
            var line2 = new Line() { X1 = midDrawP.X, Y1 = midDrawP.Y, X2 = endDrawP.X, Y2 = endDrawP.Y, Stroke = new SolidColorBrush(Color.FromRgb(136, 136, 136)), StrokeThickness = 1 };
            RenderOptions.SetBitmapScalingMode(line, BitmapScalingMode.LowQuality);
            RenderOptions.SetBitmapScalingMode(line2, BitmapScalingMode.LowQuality);
            canvas.Children.Add(line);
            canvas.Children.Add(line2);
        }

        /// <summary>
        /// 将revit坐标转换为绘图坐标
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private Point ConvertToDrawPointFromViewPoint(Point p)
        {
            var rect = UIView.GetWindowRectangle();
            var xyzs = UIView.GetZoomCorners();
            return new Point((p.X - xyzs.First().X) * (rect.Right - rect.Left) / (xyzs.Last().X - xyzs.First().X), (p.Y - xyzs.Last().Y) * (rect.Bottom - rect.Top) / (-xyzs.Last().Y + xyzs.First().Y));
        }

    }
}
