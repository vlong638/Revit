using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.MyTests.BeamAlignToFloor
{
    /// <summary>
    /// 加强点,可承载额外的信息
    /// </summary>
    public class AdvancedPoint
    {
        public AdvancedPoint(XYZ point, XYZ direction, bool isSolid)
        {
            Point = point;
            Direction = direction;
            IsSolid = isSolid;
        }

        public XYZ Point { set; get; }
        public XYZ Direction { set; get; }
        public bool IsSolid { set; get; }
    }

    public class AdvancedPoints : List<AdvancedPoint>
    {
        public AdvancedPoints()
        {
        }
        public AdvancedPoints(List<AdvancedPoint> xPoints)
        {
            this.AddRange(xPoints);
        }


        public new void Add(AdvancedPoint xPoint)
        {
            if (this.FirstOrDefault(c => c.Point.VL_XYEqualTo(xPoint.Point)) == null)
            {
                base.Add(xPoint);
            }
        }

        public void AddRange(List<AdvancedPoint> points)
        {
            foreach (var point in points)
            {
                this.Add(point);
            }
        }
    }


    /// <summary>
    /// 用以裁剪线段的信息
    /// </summary>
    public class SeperatePoints
    {
        /// <summary>
        /// 极限高程,用于重叠的多面的裁剪优先级
        /// </summary>
        public double Z;
        public AdvancedPoints AdvancedPoints = new AdvancedPoints();
        //public SeperatedLines SeperatedLines = new SeperatedLines();
        public double Max = double.MinValue;
        public double Min = double.MaxValue;

        public void Add(AdvancedPoint directionPoint, double value)
        {
            if (AdvancedPoints.Count == 0)
            {
                AdvancedPoints.Add(directionPoint);
                Max = Math.Max(Max, value);
                Min = Math.Min(Min, value);
                return;
            }
            if (value > Max)
            {
                Max = Math.Max(Max, value);
                var neighborPoint = AdvancedPoints[AdvancedPoints.Count - 1].Point;
                //var neighborPointFixed = GeometryHelper.VL_GetIntersectionOnLine(neighborPoint,new XYZ(0,0,1), directionPoint.Point, directionPoint.Direction);
                //SeperatedLines.Add(Line.CreateBound(directionPoint.Point, neighborPointFixed));
                AdvancedPoints.Add(directionPoint);
            }
            else if (value < Min)
            {
                Min = Math.Min(Min, value);
                var neighborPoint = AdvancedPoints[0].Point;
                //var neighborPointFixed = GeometryHelper.VL_GetIntersectionOnLine(neighborPoint, new XYZ(0, 0, 1), directionPoint.Point, directionPoint.Direction);
                //SeperatedLines.Add(Line.CreateBound(directionPoint.Point, neighborPointFixed));
                AdvancedPoints.Insert(0, directionPoint);
            }
        }
    }
    public class SeperatedLines
    {
        public List<Line> Lines = new List<Line>();

        public void Add(Line line)
        {
            Lines.Add(line);
        }
    }
}
