using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MyRevit.Utilities
{
    public class GraphicsDisplayer
    {
        Graphics CurrentGraphics;
        Image CurrentImage;
        int OffsetX;
        int OffsetY;
        int PaddingX = 100;
        int PaddingY = 100;

        public GraphicsDisplayer(int width, int height, int offsetX = 0, int offsetY = 0)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;
            CurrentImage = new Bitmap(width * Scale + 2 * PaddingX, height * Scale + 2 * PaddingY);
            CurrentGraphics = Graphics.FromImage(CurrentImage);
            CurrentGraphics.Clear(System.Drawing.Color.White);
        }

        int Scale = 5;
        Brush DefaultBrush = Brushes.DarkGray;
        Pen DefaultPen = new Pen(Brushes.Black);
        Font DefaultFont = new Font("宋体", 12, FontStyle.Regular);

        public void DisplayLines(List<XYZ> points, Pen pen, bool needPoint = false)
        {
            if (points.Count == 0)
                return;
            var scaledPoints = points.Select(c => GetPoint(c)).ToList();
            scaledPoints.Add(GetPoint(points.First()));
            CurrentGraphics.DrawLines(pen ?? DefaultPen, scaledPoints.ToArray());
            if (needPoint)
                foreach (var point in scaledPoints)
                    CurrentGraphics.DrawEllipse(pen ?? DefaultPen, point.X, point.Y, 5, 5);
        }

        public void DisplayPointsText(List<XYZ> points, Brush brush, int offsetX = 0, int offsetY = 0)
        {
            if (points.Count == 0)
                return;
            foreach (var point in points)
                CurrentGraphics.DrawString($"{(int)point.X },{(int)point.Y }", DefaultFont, brush ?? DefaultBrush, GetPoint(point, offsetX, offsetY));
        }

        public void SaveTo(string path)
        {
            CurrentImage.Save(path);
        }

        private System.Drawing.Point GetPoint(XYZ c, int offsetX = 0, int offsetY = 0)
        {
            return new System.Drawing.Point(((int)c.X + OffsetX) * Scale + PaddingX + offsetX, ((int)c.Y + OffsetY) * Scale + PaddingX + offsetY);
        }
    }
}
