using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace VL.Library
{ 
    public class GraphicsDisplayer
    {
        Graphics CurrentGraphics;
        Image CurrentImage;
        int OffsetX;
        int OffsetY;
        int PaddingX = 100;
        int PaddingY = 100;
        int Height;
        int Width;

        public GraphicsDisplayer(int xMin, int xMax, int yMin, int yMax)
        {
            Init(xMin, xMax, yMin, yMax);
        }

        private void Init(int xMin, int xMax, int yMin, int yMax)
        {
            Width = xMax - xMin;
            Height = yMax - yMin;
            OffsetX = -xMin;
            OffsetY = -yMin;
            Scale = Math.Min(4000 / Width, 4000 / Height);
            Width = Scale * Width;
            Height = Scale * Height;
            CurrentImage = new Bitmap(Width + 2 * PaddingX, Height + 2 * PaddingY);
            CurrentGraphics = Graphics.FromImage(CurrentImage);
            CurrentGraphics.Clear(System.Drawing.Color.White);
        }

        int Scale = 5;
        Brush DefaultBrush = Brushes.DarkGray;
        Pen DefaultPen = new Pen(Brushes.Black);
        Font DefaultFont = new Font("宋体", 12, FontStyle.Regular);

        public void DisplayLines(List<Line> lines, Pen pen, bool needPoint = false, bool needText = false)
        {
            if (lines.Count == 0)
                return;
            foreach (var line in lines)
            {
                var p0 = GetPoint(line.GetEndPoint(0));
                var p1 = GetPoint(line.GetEndPoint(1));
                CurrentGraphics.DrawLines(pen ?? DefaultPen, new System.Drawing.Point[] { p0, p1 });
                if (needPoint)
                {
                    CurrentGraphics.DrawEllipse(pen ?? DefaultPen, p0.X, p0.Y, 5, 5);
                    CurrentGraphics.DrawEllipse(pen ?? DefaultPen, p1.X, p1.Y, 5, 5);
                }
                if (needText)
                {
                    var brush = pen.Brush;
                    var point = line.GetEndPoint(0);
                    CurrentGraphics.DrawString($"{ point.X.ToString("f2") },{ point.Y.ToString("f2") }", DefaultFont, brush ?? DefaultBrush, GetPoint(point));
                    point = line.GetEndPoint(1);
                    CurrentGraphics.DrawString($"{ point.X.ToString("f2") },{ point.Y.ToString("f2") }", DefaultFont, brush ?? DefaultBrush, GetPoint(point));
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="pen">null for default</param>
        /// <param name="needText"></param>
        public void DisplayPoints(List<XYZ> points, Pen pen, bool needText = false)
        {
            if (points.Count == 0)
                return;
            foreach (var pXYZ in points)
            {
                var point = GetPoint(pXYZ);
                CurrentGraphics.DrawEllipse(pen ?? DefaultPen, point.X, point.Y, 5, 5);
                if (needText)
                {
                    var brush = (pen ?? DefaultPen).Brush;
                    CurrentGraphics.DrawString($"{(int)pXYZ.X },{(int)pXYZ.Y }", DefaultFont, brush, point);
                }
            }
        }
        /// <summary>
        /// 点的文本
        /// </summary>
        /// <param name="points"></param>
        /// <param name="brush">null for default</param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public void DisplayPoints(List<XYZ> points, Brush brush, int offsetX = 0, int offsetY = 0)
        {
            if (points.Count == 0)
                return;
            foreach (var point in points)
                CurrentGraphics.DrawString($"{(int)Math.Round(point.X, 0) },{(int)Math.Round(point.Y, 0) }", DefaultFont, brush ?? DefaultBrush, GetPoint(point, offsetX, offsetY));
        }

        /// <summary>
        /// 点的文本
        /// </summary>
        /// <param name="points"></param>
        /// <param name="brush">null for default</param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public void DisplayPointText(List<XYZ> points, List<string> texts, Brush brush, int offsetX = 0, int offsetY = 0)
        {
            if (points.Count == 0)
                return;
            for (int i = 0; i < points.Count(); i++)
            {
                var point = points[i];
                CurrentGraphics.DrawString($"{(int)Math.Round(point.X, 0) },{(int)Math.Round(point.Y, 0) }:{texts[i]}", DefaultFont, brush ?? DefaultBrush, GetPoint(point, offsetX, offsetY));
            }
        }
        /// <summary>
        /// 闭合区间
        /// </summary>
        /// <param name="points"></param>
        /// <param name="pen"></param>
        /// <param name="needPoint"></param>
        public void DisplayClosedInterval(List<XYZ> points, Pen pen, bool needPoint = false, bool needText = false)
        {
            if (points.Count == 0)
                return;
            var scaledPoints = points.Select(c => GetPoint(c)).ToList();
            scaledPoints.Add(GetPoint(points.First()));
            CurrentGraphics.DrawLines(pen ?? DefaultPen, scaledPoints.ToArray());
            if (needPoint)
            {
                foreach (var point in scaledPoints)
                    CurrentGraphics.DrawEllipse(pen ?? DefaultPen, point.X, point.Y, 5, 5);
            }
            if (needText)
            {
                var brush = pen.Brush;
                foreach (var point in points)
                    CurrentGraphics.DrawString($"{(int)Math.Round(point.X, 0) },{(int)Math.Round(point.Y, 0) }", DefaultFont, brush ?? DefaultBrush, GetPoint(point));
            }
        }


        /// <summary>
        /// XYZ=>Point的通用转换
        /// </summary>
        /// <param name="c"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <returns></returns>
        private System.Drawing.Point GetPoint(XYZ c, int offsetX = 0, int offsetY = 0)
        {
            return new System.Drawing.Point((int)Math.Round(c.X * Scale, 0) + OffsetX * Scale + PaddingX + offsetX, Height - (int)Math.Round(c.Y * Scale, 0) - OffsetY * Scale + PaddingY + offsetY);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="path"></param>
        public void SaveTo(string path)
        {
            CurrentImage.Save(path);
        }
    }
}
