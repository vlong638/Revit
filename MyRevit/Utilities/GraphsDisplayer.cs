using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using MyRevit.MyTests.BeamAlignToFloor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static PmSoft.Optimization.DrawingProduction.AnnotationCreater;

namespace PmSoft.Optimization.DrawingProduction.Utils
{
    public class GraphicsDisplayerManager
    {
        #region 标注避让
        public static void Display(string path, Triangle triangle, List<Line> pipeLines, List<Line> pipeCollisions, List<BoundingBoxXYZ> crossedBoundingBoxes, List<BoundingBoxXYZ> uncrossedBoundingBoxes)
        {
            if (pipeLines.Count() == 0)
                return;

            var uncross = new Pen(Brushes.LightGray);
            var cross = new Pen(Brushes.Red);
            var self = new Pen(Brushes.Black);
            var maxX = (int)pipeLines.Max(c => new XYZ[] { c.GetEndPoint(0), c.GetEndPoint(1) }.Max(b => b.X));
            var minX = (int)pipeLines.Min(c => new XYZ[] { c.GetEndPoint(0), c.GetEndPoint(1) }.Min(b => b.X));
            var maxY = (int)pipeLines.Max(c => new XYZ[] { c.GetEndPoint(0), c.GetEndPoint(1) }.Max(b => b.Y));
            var minY = (int)pipeLines.Min(c => new XYZ[] { c.GetEndPoint(0), c.GetEndPoint(1) }.Min(b => b.Y));
            var offSetX = -minX;
            var offSetY = -minY;
            var graphicsDisplayer = new GraphicsDisplayer(maxX - minX, maxY - minY, offSetX, offSetY);
            int displayLength = 10;
            graphicsDisplayer.DisplayLines(pipeLines.Where(c => c.Length >= displayLength).ToList(), uncross, false, true);
            graphicsDisplayer.DisplayLines(pipeLines.Where(c => c.Length < displayLength).ToList(), uncross, false, false);
            graphicsDisplayer.DisplayLines(pipeCollisions, cross, false, true);
            graphicsDisplayer.DisplayClosedInterval(new List<XYZ>() { triangle.A, triangle.B, triangle.C }, self, false, true);
            foreach (var boundingBox in crossedBoundingBoxes)
                graphicsDisplayer.DisplayClosedInterval(GetPointsFromBoundingBox(boundingBox), cross, false, true);
            foreach (var boundingBox in uncrossedBoundingBoxes)
                graphicsDisplayer.DisplayClosedInterval(GetPointsFromBoundingBox(boundingBox), uncross, false, true);
            graphicsDisplayer.SaveTo(path);
        }

        private static List<XYZ> GetPointsFromBoundingBox(BoundingBoxXYZ bounding)
        {
            return new List<XYZ>()
            {
                bounding.Min,
                bounding.Min + new XYZ(0, (bounding.Max - bounding.Min).Y, 0),
                bounding.Max,
                bounding.Max - new XYZ(0, (bounding.Max - bounding.Min).Y, 0),
            };
        }
        #endregion

        #region 梁齐板分析支持
        //public static void Display(string path, SeperatePoints lineSeperatePoints, List<LevelOutLines> outLinesCollection)
        //{
        //    var maxX = (int)outLinesCollection.Max(c => c.OutLines.Max(v => v.Points.Max(b => b.X)));
        //    var minX = (int)outLinesCollection.Min(c => c.OutLines.Min(v => v.Points.Min(b => b.X)));
        //    var maxY = (int)outLinesCollection.Max(c => c.OutLines.Max(v => v.Points.Max(b => b.Y)));
        //    var minY = (int)outLinesCollection.Min(c => c.OutLines.Min(v => v.Points.Min(b => b.Y)));
        //    var offSetX = -minX;
        //    var offSetY = -minY;
        //    GraphicsDisplayer = new GraphicsDisplayer(maxX - minX, maxY - minY, offSetX, offSetY);
        //    foreach (var levelOutLines in outLinesCollection)
        //        foreach (var outLine in levelOutLines.OutLines)
        //            Display(outLine);
        //    GraphicsDisplayer.DisplayClosedInterval(lineSeperatePoints.AdvancedPoints.Select(c => c.Point).ToList(), new Pen(Brushes.Red), true);
        //    var randomValue = new Random().Next(10);
        //    GraphicsDisplayer.DisplayPointsText(lineSeperatePoints.AdvancedPoints.Select(c => c.Point).ToList(), Brushes.Red, randomValue, randomValue);
        //    GraphicsDisplayer.SaveTo(path);
        //}

        //static void Display(OutLine outLine)
        //{
        //    var randomValue = new Random().Next(10);
        //    GraphicsDisplayer.DisplayClosedInterval(outLine.Points, null, false);
        //    if (outLine.Points.Count <= 6)
        //        GraphicsDisplayer.DisplayPointsText(outLine.Points, null, randomValue, randomValue);

        //    foreach (var subOutLine in outLine.SubOutLines)
        //        Display(subOutLine);
        //}
        #endregion

        #region 多管标注分析支持
        //internal static void Display(string path, List<PipeAndNodePoint> pipes)
        //{
        //    var maxX = (int)pipes.Max(c => new XYZ[] { (c.Pipe.Location as LocationCurve).Curve.GetEndPoint(0), (c.Pipe.Location as LocationCurve).Curve.GetEndPoint(1) }.Max(v => v.X));
        //    var minX = (int)pipes.Min(c => new XYZ[] { (c.Pipe.Location as LocationCurve).Curve.GetEndPoint(0), (c.Pipe.Location as LocationCurve).Curve.GetEndPoint(1) }.Min(v => v.X));
        //    var maxY = (int)pipes.Max(c => new XYZ[] { (c.Pipe.Location as LocationCurve).Curve.GetEndPoint(0), (c.Pipe.Location as LocationCurve).Curve.GetEndPoint(1) }.Max(v => v.Y));
        //    var minY = (int)pipes.Min(c => new XYZ[] { (c.Pipe.Location as LocationCurve).Curve.GetEndPoint(0), (c.Pipe.Location as LocationCurve).Curve.GetEndPoint(1) }.Min(v => v.Y));
        //    var offSetX = -minX;
        //    var offSetY = -minY;
        //    var GraphicsDisplayer = new GraphicsDisplayer(maxX - minX, maxY - minY, offSetX, offSetY);
        //    GraphicsDisplayer.DisplayLines(pipes.Select(c => (c.Pipe.Location as LocationCurve).Curve as Line).ToList(), new Pen(Brushes.Black), true, true);
        //    GraphicsDisplayer.DisplayPoints(pipes.Select(c => c.NodePoint).ToList(), new Pen(Brushes.Red), true);
        //    GraphicsDisplayer.SaveTo(path);
        //}
        #endregion

        #region 结构做法标注
        public static void Display(string path,  List<Line> lines, List<XYZ> textLocations)
        {
            if (lines.Count() == 0)
                return;

            var uncross = new Pen(Brushes.LightGray);
            var cross = new Pen(Brushes.Red);
            var self = new Pen(Brushes.Black);
            var maxX = (int)lines.Max(c => new XYZ[] { c.GetEndPoint(0), c.GetEndPoint(1) }.Max(b => b.X));
            var minX = (int)lines.Min(c => new XYZ[] { c.GetEndPoint(0), c.GetEndPoint(1) }.Min(b => b.X));
            var maxY = (int)lines.Max(c => new XYZ[] { c.GetEndPoint(0), c.GetEndPoint(1) }.Max(b => b.Y));
            var minY = (int)lines.Min(c => new XYZ[] { c.GetEndPoint(0), c.GetEndPoint(1) }.Min(b => b.Y));
            minX--;
            minY--;
            maxX++;
            maxY++;
            var offSetX = -minX;
            var offSetY = -minY;
            var graphicsDisplayer = new GraphicsDisplayer(maxX - minX, maxY - minY, offSetX, offSetY);
            graphicsDisplayer.DisplayLines(lines, uncross, true, true);
            graphicsDisplayer.DisplayPoints(textLocations, Pens.Red, true);
            graphicsDisplayer.SaveTo(path);
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
        int Height;
        int Width;

        public GraphicsDisplayer(int width, int height, int offsetX = 0, int offsetY = 0)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;
            Scale = Math.Min(4000 / width, 4000 / height);
            Width = Scale * width;
            Height = Scale * height;
            CurrentImage = new Bitmap(width * Scale + 2 * PaddingX, height * Scale + 2 * PaddingY);
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
                    CurrentGraphics.DrawString($"{(int)point.X },{(int)point.Y }", DefaultFont, brush ?? DefaultBrush, GetPoint(point));
                    point = line.GetEndPoint(1);
                    CurrentGraphics.DrawString($"{(int)point.X },{(int)point.Y }", DefaultFont, brush ?? DefaultBrush, GetPoint(point));
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
        /// 点的文本
        /// </summary>
        /// <param name="points"></param>
        /// <param name="brush">null for default</param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public void DisplayPointsText(List<XYZ> points, Brush brush, int offsetX = 0, int offsetY = 0)
        {
            if (points.Count == 0)
                return;
            foreach (var point in points)
                CurrentGraphics.DrawString($"{(int)Math.Round(point.X, 0) },{(int)Math.Round(point.Y, 0) }", DefaultFont, brush ?? DefaultBrush, GetPoint(point, offsetX, offsetY));
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


//using Autodesk.Revit.DB;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;

//namespace MyRevit.Utilities
//{
//    public class GraphicsDisplayer
//    {
//        Graphics CurrentGraphics;
//        Image CurrentImage;
//        int OffsetX;
//        int OffsetY;
//        int PaddingX = 100;
//        int PaddingY = 100;

//        public GraphicsDisplayer(int width, int height, int offsetX = 0, int offsetY = 0)
//        {
//            OffsetX = offsetX;
//            OffsetY = offsetY;
//            CurrentImage = new Bitmap(width * Scale + 2 * PaddingX, height * Scale + 2 * PaddingY);
//            CurrentGraphics = Graphics.FromImage(CurrentImage);
//            CurrentGraphics.Clear(System.Drawing.Color.White);
//        }

//        int Scale = 5;
//        Brush DefaultBrush = Brushes.DarkGray;
//        Pen DefaultPen = new Pen(Brushes.Black);
//        Font DefaultFont = new Font("宋体", 12, FontStyle.Regular);

//        #region 多管标注分析支持
//        public void DisplayLines(List<Line> lines, Pen pen, bool needPoint = false)
//        {
//            if (lines.Count == 0)
//                return;
//            foreach (var line in lines)
//            {
//                var p0 = GetPoint(line.GetEndPoint(0));
//                var p1 = GetPoint(line.GetEndPoint(1));
//                CurrentGraphics.DrawLines(pen ?? DefaultPen, new System.Drawing.Point[] { p0, p1 });
//                if (needPoint)
//                {
//                    CurrentGraphics.DrawEllipse(pen ?? DefaultPen, p0.X, p0.Y, 5, 5);
//                    CurrentGraphics.DrawEllipse(pen ?? DefaultPen, p1.X, p1.Y, 5, 5);
//                }
//            }
//        }
//        public void DisplayPoints(List<Line> lines, Pen pen, bool needPoint = false)
//        {
//            if (lines.Count == 0)
//                return;
//            foreach (var line in lines)
//            {
//                var p0 = GetPoint(line.GetEndPoint(0));
//                var p1 = GetPoint(line.GetEndPoint(1));
//                CurrentGraphics.DrawLines(pen ?? DefaultPen, new System.Drawing.Point[] { p0, p1 });
//                if (needPoint)
//                {
//                    CurrentGraphics.DrawEllipse(pen ?? DefaultPen, p0.X, p0.Y, 5, 5);
//                    CurrentGraphics.DrawEllipse(pen ?? DefaultPen, p1.X, p1.Y, 5, 5);
//                }
//            }
//        } 
//        #endregion

//        #region 梁齐板分析支持
//        /// <summary>
//        /// 闭合区间
//        /// </summary>
//        /// <param name="points"></param>
//        /// <param name="pen"></param>
//        /// <param name="needPoint"></param>
//        public void DisplayClosedInterval(List<XYZ> points, Pen pen, bool needPoint = false)
//        {
//            if (points.Count == 0)
//                return;
//            var scaledPoints = points.Select(c => GetPoint(c)).ToList();
//            scaledPoints.Add(GetPoint(points.First()));
//            CurrentGraphics.DrawLines(pen ?? DefaultPen, scaledPoints.ToArray());
//            if (needPoint)
//                foreach (var point in scaledPoints)
//                    CurrentGraphics.DrawEllipse(pen ?? DefaultPen, point.X, point.Y, 5, 5);
//        }

//        /// <summary>
//        /// 点的文本
//        /// </summary>
//        /// <param name="points"></param>
//        /// <param name="brush"></param>
//        /// <param name="offsetX"></param>
//        /// <param name="offsetY"></param>
//        public void DisplayPointsText(List<XYZ> points, Brush brush, int offsetX = 0, int offsetY = 0)
//        {
//            if (points.Count == 0)
//                return;
//            foreach (var point in points)
//                CurrentGraphics.DrawString($"{(int)point.X },{(int)point.Y }", DefaultFont, brush ?? DefaultBrush, GetPoint(point, offsetX, offsetY));
//        } 
//        #endregion

//        /// <summary>
//        /// XYZ=>Point的通用转换
//        /// </summary>
//        /// <param name="c"></param>
//        /// <param name="offsetX"></param>
//        /// <param name="offsetY"></param>
//        /// <returns></returns>
//        private System.Drawing.Point GetPoint(XYZ c, int offsetX = 0, int offsetY = 0)
//        {
//            return new System.Drawing.Point(((int)c.X + OffsetX) * Scale + PaddingX + offsetX, ((int)c.Y + OffsetY) * Scale + PaddingX + offsetY);
//        }

//        /// <summary>
//        /// 保存
//        /// </summary>
//        /// <param name="path"></param>
//        public void SaveTo(string path)
//        {
//            CurrentImage.Save(path);
//        }
//    }
//}
