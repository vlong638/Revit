using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.MyTests.BeamAlignToFloor
{
    /// <summary>
    /// 分层轮廓集合,一面一层级的轮廓
    /// </summary>
    public class LevelOutLines
    {
        public bool IsValid { get { return OutLines.Count() > 0; } }
        public List<OutLine> OutLines = new List<OutLine>();

        /// <summary>
        /// 添加面所有的轮廓
        /// </summary>
        /// <param name="face"></param>
        public void Add(Face face)
        {
            var current = this;
            //闭合区间集合,EdgeArray
            foreach (EdgeArray edgeArray in face.EdgeLoops)
            {
                Add(new OutLine(edgeArray));
            }
        }

        void Add(OutLine newOne)
        {
            //子节点的下级
            foreach (var OutLine in OutLines)
            {
                if (OutLine.Contains(newOne))
                {
                    OutLine.Add(newOne);
                    return;
                }
            }
            //子节点的上级
            bool isTopLevel = false;
            for (int i = OutLines.Count() - 1; i >= 0; i--)
            {
                var SubOutLine = OutLines[i];
                if (newOne.Contains(SubOutLine))
                {
                    OutLines.Remove(SubOutLine);
                    SubOutLine.RevertAllOutLineType();
                    newOne.SubOutLines.Add(SubOutLine);
                    isTopLevel = true;
                }
            }
            if (isTopLevel)
            {
                newOne.IsSolid = true;
                OutLines.Add(newOne);
                return;
            }
            //无相关的新节点
            OutLines.Add(newOne);
        }

        public bool IsCover(Line line)
        {
            foreach (var SubOutLine in OutLines)
            {
                if (SubOutLine.IsCover(line) != CoverType.Disjoint)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 获得拆分点
        /// </summary>
        /// <returns></returns>
        public SeperatePoints GetFitLines(Line beamLineZ0)
        {
            SeperatePoints fitLines = new SeperatePoints();
            var p0 = beamLineZ0.GetEndPoint(0);
            var p1 = beamLineZ0.GetEndPoint(1);
            foreach (var SubOutLine in OutLines)
            {
                var coverType = SubOutLine.IsCover(beamLineZ0);
                if (coverType != CoverType.Disjoint)
                    fitLines.DirectionPoints.AddRange(SubOutLine.GetFitLines(beamLineZ0).DirectionPoints);
                //线的端点增加
                var triangle = SubOutLine.GetContainer(p0);
                if (triangle != null)
                    fitLines.DirectionPoints.Add(new DirectionPoint(GetIntersection(triangle, p0, p0 + new XYZ(0, 0, 1)), beamLineZ0.Direction, SubOutLine.IsSolid));
                triangle = SubOutLine.GetContainer(p1);
                if (triangle != null)
                    fitLines.DirectionPoints.Add(new DirectionPoint(GetIntersection(triangle, p1, p1 + new XYZ(0, 0, 1)), beamLineZ0.Direction, SubOutLine.IsSolid));
            }
            return fitLines;
        }

        /// <summary>
        /// 一面两点 求线面交点
        /// t = (vT·pT - vT·pL)/(vT·vL)
        /// p = pL + t*vL
        /// </summary>
        private static XYZ GetIntersection(Triangle triangle, XYZ pL, XYZ pL2)
        {
            var pT = triangle.A;
            var vT = (triangle.B - triangle.A).CrossProduct(triangle.C - triangle.A);
            var vL = pL2 - pL;
            var t = (vT.DotProduct(pT) - vT.DotProduct(pL)) / vT.DotProduct(vL);
            var p = pL + t * vL;
            return p;


            //var unboundLine = Line.CreateBound(point, point + new XYZ(0, 0, 1));
            //unboundLine.MakeUnbound();
            //IntersectionResultArray faceIntersect;
            //face.Intersect(unboundLine, out faceIntersect);
            //return faceIntersect.get_Item(0).XYZPoint;
        }
    }
}
