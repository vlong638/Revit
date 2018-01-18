using Autodesk.Revit.UI;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;

namespace VL.Library
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

        public DrawAreaView(UIApplication uiApp)
        {
            UIView = uiApp.ActiveUIDocument.GetOpenUIViews().FirstOrDefault(p => p.ViewId == uiApp.ActiveUIDocument.ActiveGraphicalView.Id);

            InitializeComponent();

            new System.Windows.Interop.WindowInteropHelper(this).Owner = Autodesk.Windows.ComponentManager.ApplicationWindow;

            Rect = UIView.GetWindowRectangle();
            this.Left = Rect.Left;
            this.Top = Rect.Top;
            this.Width = Rect.Right - Rect.Left;
            this.Height = Rect.Bottom - Rect.Top;
            co_s = co_e = Height / 400;
        }
      //  严重性 代码  说明 项目  文件 行   禁止显示状态
      //错误  CS1069 The type name 'IComponentConnector' could not be found in the namespace 'System.Windows.Markup'. This type has been forwarded to assembly 'System.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' Consider adding a reference to that assembly.VL.Library E:\WorkingSpace\Repository\Git\Revit\VL.Library\obj\Debug\VLUtils\PointPicker\DrawAreaView.g.cs	40	活动

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

        public void RemoveChildren()
        {
            canvas.Children.RemoveRange(0, canvas.Children.Count);
        }

        public void AddChild(Line line)
        {
            canvas.Children.Add(line);
        }
    }
}
