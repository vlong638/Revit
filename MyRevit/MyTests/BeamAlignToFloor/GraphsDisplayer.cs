using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MyRevit.MyTests.BeamAlignToFloor
{
    public class GraphicsDisplayerManager
    {
        static GraphicsDisplayer GraphicsDisplayer;

        #region LineSeperatePoints
        public static void Display(string path, SeperatePoints lineSeperatePoints, List<LevelOutLines> outLinesCollection)
        {
            var maxX = (int)outLinesCollection.Max(c => c.OutLines.Max(v => v.Points.Max(b => b.X)));
            var minX = (int)outLinesCollection.Min(c => c.OutLines.Min(v => v.Points.Min(b => b.X)));
            var maxY = (int)outLinesCollection.Max(c => c.OutLines.Max(v => v.Points.Max(b => b.Y)));
            var minY = (int)outLinesCollection.Min(c => c.OutLines.Min(v => v.Points.Min(b => b.Y)));
            var offSetX = -minX;
            var offSetY = -minY;
            GraphicsDisplayer = new GraphicsDisplayer(maxX - minX, maxY - minY, offSetX, offSetY);
            foreach (var levelOutLines in outLinesCollection)
                foreach (var outLine in levelOutLines.OutLines)
                    Display(outLine);
            GraphicsDisplayer.DisplayLines(lineSeperatePoints.DirectionPoints.Select(c=>c.Point).ToList(), new Pen(Brushes.Red), true);
            var randomValue = new Random().Next(10);
            GraphicsDisplayer.DisplayPointsText(lineSeperatePoints.DirectionPoints.Select(c => c.Point).ToList(), Brushes.Red, randomValue, randomValue);
            GraphicsDisplayer.SaveTo(path);
        }
        static void Display(OutLine outLine)
        {
            var randomValue = new Random().Next(10);
            GraphicsDisplayer.DisplayLines(outLine.Points, null, false);
            if (outLine.Points.Count <= 6)
                GraphicsDisplayer.DisplayPointsText(outLine.Points, null, randomValue, randomValue);

            foreach (var subOutLine in outLine.SubOutLines)
                Display(subOutLine);
        }
        #endregion
    }


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
