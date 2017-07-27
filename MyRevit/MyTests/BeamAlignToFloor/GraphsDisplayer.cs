using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MyRevit.MyTests.BeamAlignToFloor
{
    public class GraphicsDisplayer
    {
        Graphics CurrentGraphics;
        Image CurrentImage;
        string Path2 = @"E:\WorkingSpace\Outputs\Images\display2.png";

        public GraphicsDisplayer(int width, int height)
        {
            CurrentImage = new Bitmap(width * Scale + 100, height * Scale + 100);
            CurrentGraphics = Graphics.FromImage(CurrentImage);
            CurrentGraphics.Clear(System.Drawing.Color.White);
        }

        int Scale = 5;
        Brush DefaultBrush = Brushes.DarkGray;
        Pen DefaultPen = new Pen(Brushes.Black);
        Font DefaultFont = new Font("宋体", 12, FontStyle.Regular);

        #region LineSeperatePoints
        public void Display(LineSeperatePoints lineSeperatePoints)
        {
            DisplayLines(lineSeperatePoints.Points, new Pen(Brushes.Red), true);
            DisplayPointsText(lineSeperatePoints.Points, Brushes.Red, 10, 10);
        }
        #endregion

        #region List<LevelOutLines>
        public void Display(List<LevelOutLines> outLinesCollection)
        {
            foreach (var levelOutLines in outLinesCollection)
            {
                foreach (var outLine in levelOutLines.OutLines)
                {
                    Display(outLine);
                }
            }
        }
        public void Display(OutLine outLine)
        {
            DisplayLines(outLine.Points, DefaultPen, false);
            if (outLine.Points.Count <= 6)
                DisplayPointsText(outLine.Points, DefaultBrush, -10, -10);

            foreach (var subOutLine in outLine.SubOutLines)
                Display(subOutLine);
        }
        #endregion

        private void DisplayLines(List<XYZ> sourcePoints, Pen pen, bool needPoint = false)
        {
            var points = sourcePoints.Select(c => new System.Drawing.Point((int)c.X * Scale, (int)c.Y * Scale)).ToList();
            points.Add(new System.Drawing.Point((int)sourcePoints.First().X * Scale, (int)sourcePoints.First().Y * Scale));
            CurrentGraphics.DrawLines(pen, points.ToArray());
            if (needPoint)
                foreach (var point in points)
                {
                    CurrentGraphics.DrawEllipse(pen, point.X, point.Y, 5, 5);
                }
        }

        private void DisplayPointsText(List<XYZ> points, Brush brush, int offsetX = 0, int offsetY = 0)
        {
            foreach (var point in points)
                CurrentGraphics.DrawString($"{(int)point.X * Scale},{(int)point.Y * Scale}", DefaultFont, brush, new System.Drawing.Point((int)point.X * Scale + offsetX, (int)point.Y * Scale + offsetY));
        }

        public void Save()
        {
            CurrentImage.Save(Path2);
        }
    }
}
