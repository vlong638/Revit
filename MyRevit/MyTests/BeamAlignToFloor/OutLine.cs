using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.MyTests.BeamAlignToFloor
{
    /// <summary>
    /// 轮廓
    /// </summary>
    public class OutLine
    {
        EdgeArray Edges;
        public List<XYZ> Points;
        List<XYZ> PointZ0s;
        List<Line> Lines;
        List<Line> LineZ0s;
        List<Triangle> Triangles;
        List<Triangle> TriangleZ0s;
        public List<OutLine> SubOutLines = new List<OutLine>();
        public bool IsSolid { set; get; }

        #region Constructor
        public OutLine(EdgeArray edgeArray)
        {
            Init(edgeArray);
        }

        void Init(EdgeArray edgeArray)
        {
            Edges = edgeArray;
            Points = GeometryHelper.GetPoints(Edges);
            PointZ0s = Points.Select(c => new XYZ(c.X, c.Y, 0)).ToList();
            Lines = new List<Line>();
            AddLinesFromPoints(ref Lines, Points);
            LineZ0s = new List<Line>();
            AddLinesFromPoints(ref LineZ0s, PointZ0s);
            Triangles = GeometryHelper.GetTriangles(Points);
            TriangleZ0s = GeometryHelper.GetTriangles(PointZ0s);
            IsSolid = true;
        }

        private void AddLinesFromPoints(ref List<Line> lines, List<XYZ> points)
        {
            for (int i = 0; i < points.Count - 1; i++)
                lines.Add(Line.CreateBound(points[i], points[i + 1]));
            lines.Add(Line.CreateBound(points[points.Count - 1], points[0]));
        }
        #endregion

        /// <summary>
        /// 更改轮廓及其子轮廓的实体性质
        /// </summary>
        public void RevertAllOutLineType()
        {
            IsSolid = !IsSolid;
            foreach (var subOutLine in SubOutLines)
                subOutLine.RevertAllOutLineType();
        }

        //public bool IsInit { get { return Edges != null; } }
        //public OutLine()
        //{
        //}
        //void Add(EdgeArray edgeArray)
        //{
        //    if (!IsInit)
        //    {
        //        Init(edgeArray);
        //    }
        //    else
        //    {
        //        Add(new OutLine(edgeArray));
        //    }
        //}
        //public OutLine Add(EdgeArrayArray edgeLoops)
        //{
        //    var current = this;
        //    //闭合区间集合,EdgeArray
        //    foreach (EdgeArray edgeArray in edgeLoops)
        //    {
        //        if (!IsInit)
        //        {
        //            Init(edgeArray);
        //            continue;
        //        }
        //        current= current.Add(new OutLine(edgeArray));
        //    }
        //    return current;
        //}

        public void Add(OutLine newOne)
        {
            //当前节点下级的上级
            bool isTopLevel = false;
            for (int i = SubOutLines.Count() - 1; i >= 0; i--)
            {
                var outLine = SubOutLines[i];
                if (newOne.Contains(outLine))
                {
                    SubOutLines.Remove(outLine);
                    outLine.RevertAllOutLineType();
                    newOne.SubOutLines.Add(outLine);
                    isTopLevel = true;
                }
            }
            if (isTopLevel)
            {
                newOne.IsSolid = !IsSolid;
                SubOutLines.Add(newOne);
                return;
            }
            //当前节点下级的下级
            for (int i = SubOutLines.Count() - 1; i >= 0; i--)
            {
                var outLine = SubOutLines[i];
                if (outLine.Contains(newOne))
                {
                    outLine.Add(newOne);
                    return;
                }
            }
            //与其他下级无关联
            newOne.IsSolid = !IsSolid;
            SubOutLines.Add(newOne);
            return;
        }

        /// <summary>
        /// 检测轮廓是否被包含 另一轮廓
        /// </summary>
        public bool Contains(OutLine outline)
        {
            return Contains(outline.PointZ0s);
        }
        /// <summary>
        /// 检测轮廓是否被包含 另一轮廓
        /// </summary>
        public bool Contains(List<XYZ> points)
        {
            foreach (var pointZ0 in points)
            {
                var container = TriangleZ0s.AsParallel().FirstOrDefault(c => c.Contains(pointZ0));
                if (container == null)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// 检测轮廓是否被包含 另一轮廓
        /// </summary>
        public Triangle GetContainer(XYZ point)
        {
            var triangleZ0 = TriangleZ0s.AsParallel().FirstOrDefault(c => c.Contains(point));
            if (triangleZ0 != null)
                return Triangles.First(c => c.A.XYEqualTo(triangleZ0.A) && c.B.XYEqualTo(triangleZ0.B) && c.C.XYEqualTo(triangleZ0.C));
            return null;
        }


        /// <summary>
        /// 检测轮廓是否相交或包含 有限线段
        /// </summary>
        /// <param name="outLine"></param>
        /// <returns></returns>
        public CoverType IsCover(Line line)
        {
            //根据线段相交判断
            var intersect = LineZ0s.FirstOrDefault(c => c.VL_IsIntersect(line));
            if (intersect != null)
                return CoverType.Intersect;
            //线段包含判断
            List<XYZ> points = new List<XYZ>();
            points.Add(line.GetEndPoint(0));
            points.Add(line.GetEndPoint(1));
            if (Contains(points))
                return CoverType.Contain;
            return CoverType.Disjoint;



            ////根据线段相交判断
            //if (LineZ0s.FirstOrDefault(c => c.Intersect(line) == SetComparisonResult.Overlap) != null)
            //{
            //    return true;
            //}
            //else//线段包含判断
            //{
            //    var point = line.GetEndPoint(0);
            //    if (TriangleZ0s.AsParallel().FirstOrDefault(c => c.Contains(point)) != null)
            //        return true;
            //    point = line.GetEndPoint(1);
            //    if (TriangleZ0s.AsParallel().FirstOrDefault(c => c.Contains(point)) != null)
            //        return true;
            //}
            //return false;
        }


        /// <summary>
        /// 获得拆分点
        /// </summary>
        /// <returns></returns>
        public SeperatePoints GetFitLines(Line lineZ0)
        {
            var intersectLineZ0s = LineZ0s.Where(c => c.VL_IsIntersect(lineZ0));
            //这里计算出了裁剪点,但这里是Z0面的交点
            SeperatePoints result = new SeperatePoints();
            foreach (var intersectLineZ0 in intersectLineZ0s)
            {
                var pointZ0s = intersectLineZ0.VL_GetIntersectedOrContainedPoints(lineZ0);
                var orientLine = Lines.First(c => c.GetEndPoint(0).XYEqualTo(intersectLineZ0.GetEndPoint(0)) && c.GetEndPoint(1).XYEqualTo(intersectLineZ0.GetEndPoint(1)));
                result.DirectionPoints.AddRange(orientLine.VL_GetZLineIntersection(pointZ0s));
            }
            ////裁剪点需回归到面板
            //foreach (var point in points)
            //{
            //    var intersectPoint = Points.FirstOrDefault(p => p.XYEqualTo(point));
            //    if (intersectPoint != null)
            //        result.Points.Add(intersectPoint);
            //    else
            //    {
            //        var unboundLine = Line.CreateBound(point, point + new XYZ(0, 0, 1));
            //        unboundLine.MakeUnbound();
            //        IntersectionResultArray faceIntersect;
            //        Face.Intersect(unboundLine, out faceIntersect);
            //        result.Points.Add(faceIntersect.get_Item(0).XYZPoint);
            //    }
            //}
            //result.Points.AddRange(points);
            //result.Points.AddRange(points.Select(c => new XYZ(c.X, c.Y, Points.FirstOrDefault(p => p.XYEqualTo(c)).Z)));
            foreach (var SubOutLine in SubOutLines)
            {
                var coverType = SubOutLine.IsCover(lineZ0);
                if (coverType != CoverType.Disjoint)
                    result.DirectionPoints.AddRange(SubOutLine.GetFitLines(lineZ0).DirectionPoints);

            }
            return result;
        }
    }
}
