using MyRevit.Utilities;
using PmSoft.Optimization.DrawingProduction.Utils;
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
            GraphicsDisplayer.DisplayClosedInterval(lineSeperatePoints.AdvancedPoints.Select(c => c.Point).ToList(), new Pen(Brushes.Red), true);
            var randomValue = new Random().Next(10);
            GraphicsDisplayer.DisplayPointsText(lineSeperatePoints.AdvancedPoints.Select(c => c.Point).ToList(), Brushes.Red, randomValue, randomValue);
            GraphicsDisplayer.SaveTo(path);
        }
        static void Display(OutLine outLine)
        {
            var randomValue = new Random().Next(10);
            GraphicsDisplayer.DisplayClosedInterval(outLine.Points, null, false);
            if (outLine.Points.Count <= 6)
                GraphicsDisplayer.DisplayPointsText(outLine.Points, null, randomValue, randomValue);

            foreach (var subOutLine in outLine.SubOutLines)
                Display(subOutLine);

            GraphicsDisplayer.DisplayPoints(outLine.Points, null, true);

        }

        #endregion

        #region 0802
        internal static void Display(OutLineManager0802 collector, List<LevelFloor> levelFloors)
        {
            var outLinesCollection = levelFloors.Select(c => collector.GetLeveledOutLines(c));
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

            GraphicsDisplayerManager.Save(@"E:\WorkingSpace\Outputs\Images\display3.png");
        }
        internal static void Display(SeperatePoints seperatePoints)
        {
            GraphicsDisplayer.DisplayClosedInterval(seperatePoints.AdvancedPoints.Select(c => c.Point).ToList(), new Pen(Brushes.Red), true);
            var randomValue = new Random().Next(10);
            GraphicsDisplayer.DisplayPointsText(seperatePoints.AdvancedPoints.Select(c => c.Point).ToList(), Brushes.Red, randomValue, randomValue);

            GraphicsDisplayerManager.Save(@"E:\WorkingSpace\Outputs\Images\display3.png");
        }
        internal static void Save(string path)
        {
            GraphicsDisplayer.SaveTo(path);
        }
        #endregion

        internal static void Display(ValidFaces collector, List<LevelFloor> levelFloors)
        {
            var outLinesCollection = levelFloors.Select(c => collector.GetLeveledOutLines(c));
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

            GraphicsDisplayerManager.Save(@"E:\WorkingSpace\Outputs\Images\display4.png");
        }
    }
}
