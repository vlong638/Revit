using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MyRevitConsole
{
    public class GraphicsDisplayer
    {
        Graphics Graphics;
        Image Image;
        string Path = @"E:\WorkingSpace\Outputs\Images\display.png";
        string Path2 = @"E:\WorkingSpace\Outputs\Images\display2.png";

        public GraphicsDisplayer()
        {
            Image = Image.FromFile(Path);
            Graphics = Graphics.FromImage(Image);
            Graphics.Clear(Color.White);
            Graphics.DrawLine(DefaultPen, new Point(1, 1), new Point(10, 10));
            Image.Save(Path2);
        }

        ~GraphicsDisplayer()
        {
            Graphics.Dispose();
            Image.Dispose();
        }

        Brush DefaultBrush = Brushes.DarkGray;
        Pen DefaultPen = new Pen(Brushes.Black);
        Font DefaultFont = new Font("宋体", 15, FontStyle.Bold);

        public void Save()
        {
            Graphics.Save();
        }
    }
}
