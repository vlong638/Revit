using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace MyRevit.MyTests.BeamAlignToFloor
{
    public class DirectionPoint
    {
        public DirectionPoint(XYZ point, XYZ direction, bool isSolid)
        {
            Point = point;
            Direction = direction;
            IsSolid = isSolid;
        }

        public XYZ Point { set; get; }
        public XYZ Direction { set; get; }
        public bool IsSolid { set; get; }
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
        public List<DirectionPoint> DirectionPoints = new List<DirectionPoint>();
        public SeperatedLines SeperatedLines = new SeperatedLines();
        public double Max = double.MinValue;
        public double Min = double.MaxValue;

        public void Add(DirectionPoint directionPoint, double value)
        {
            if (DirectionPoints.Count == 0)
            {
                DirectionPoints.Add(directionPoint);
                Max = Math.Max(Max, value);
                Min = Math.Min(Min, value);
                return;
            }
            if (value > Max)
            {
                Max = Math.Max(Max, value);
                var neighborPoint = DirectionPoints[DirectionPoints.Count - 1].Point;
                var neighborPointFixed = GeometryHelper.VL_GetIntersectionOnLine(neighborPoint,new XYZ(0,0,1), directionPoint.Point, directionPoint.Direction);
                SeperatedLines.Add(Line.CreateBound(directionPoint.Point, neighborPointFixed));
                DirectionPoints.Add(directionPoint);
            }
            else if (value < Min)
            {
                Min = Math.Min(Min, value);
                var neighborPoint = DirectionPoints[0].Point;
                var neighborPointFixed = GeometryHelper.VL_GetIntersectionOnLine(neighborPoint, new XYZ(0, 0, 1), directionPoint.Point, directionPoint.Direction);
                SeperatedLines.Add(Line.CreateBound(directionPoint.Point, neighborPointFixed));
                DirectionPoints.Insert(0, directionPoint);
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
