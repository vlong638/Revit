using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.MyTests.BeamAlignToFloor
{
    /// <summary>
    /// 几何处理帮助类
    /// </summary>
    public static class GeometryHelper
    {
        /// <summary>
        /// 计算两个矩形是否重叠
        /// </summary>
        public static bool VL_IsRectangleCrossed(XYZ rectangle1P1, XYZ rectangle1P2, XYZ rectangle2P1, XYZ rectangle2P2)
        {
            var h1 = Math.Abs(rectangle1P1.Y - rectangle1P2.Y) / 2;
            var h2 = Math.Abs(rectangle2P1.Y - rectangle2P2.Y) / 2;
            var w1 = Math.Abs(rectangle1P1.X - rectangle1P2.X) / 2;
            var w2 = Math.Abs(rectangle2P1.X - rectangle2P2.X) / 2;
            var mid1 = (rectangle1P1 + rectangle1P2) / 2;
            var mid2 = (rectangle2P1 + rectangle2P2) / 2;
            var h = Math.Abs(mid2.Y - mid1.Y);
            var w = Math.Abs(mid2.X - mid1.X);
            return h <= h1 + h2 && w <= w1 + w2;
        }

        /// <summary>
        /// 线段相交判断,首先是矩形重叠判断,在重叠时进行跨立判断(相互跨立)
        /// </summary>
        public static bool VL_IsIntersect(this Line line1, Line line2)
        {
            var p1_0 = line1.GetEndPoint(0);
            var p1_1 = line1.GetEndPoint(1);
            var p2_0 = line2.GetEndPoint(0);
            var p2_1 = line2.GetEndPoint(1);
            //先矩形相交
            if (!VL_IsRectangleCrossed(p1_0, p1_1, p2_0, p2_1))
                return false;
            //后跨立试验
            return (p1_0 - p2_0).CrossProduct(p2_1 - p2_0).DotProduct((p2_1 - p2_0).CrossProduct(p1_1 - p2_0)) >= 0
                && (p2_0 - p1_0).CrossProduct(p1_1 - p1_0).DotProduct((p1_1 - p1_0).CrossProduct(p2_1 - p1_0)) >= 0;
        }

        public const double XYZTolerance = 0.001;

        /// <summary>
        /// XYZ的X与Y相同判断
        /// </summary>
        public static bool VL_XYEqualTo(this XYZ point1, XYZ point2)
        {
            return Math.Abs(point1.X - point2.X) < XYZTolerance && Math.Abs(point1.Y - point2.Y) < XYZTolerance;
        }
        /// <summary>
        /// XYZ的X与Y相同判断
        /// </summary>
        public static bool VL_XYZEqualTo(this XYZ point1, XYZ point2)
        {
            return Math.Abs(point1.X - point2.X) < XYZTolerance && Math.Abs(point1.Y - point2.Y) < XYZTolerance && Math.Abs(point1.Z - point2.Z) < XYZTolerance;
        }

        /// <summary>
        /// 矩阵叉积,>0则p2在p1的逆时针方向, <0则p2在p1的顺时针方向
        /// p1.Y * p2.Z - p1.Z * p2.Y 
        /// p1.Z * p2.X - p1.X * p2.Z
        /// p1.X * p2.Y - p1.Y * p2.X
        /// </summary>
        public static double VL_CrossProduct(this XYZ p1, XYZ p2)
        {
            return p1.Y * p2.Z - p1.Z * p2.Y + p1.Z * p2.X - p1.X * p2.Z + p1.X * p2.Y - p1.Y * p2.X;
        }

        /// <summary>
        /// 获得线与点在Z轴上的交点
        /// </summary>
        /// <param name="boardLine"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public static List<AdvancedPoint> VL_GetZLineIntersection(this Line boardLine, List<XYZ> points, bool isSolid, Line beamLine)
        {
            XYZ lineP0, lineP1;
            lineP0 = boardLine.GetEndPoint(0);
            lineP1 = boardLine.GetEndPoint(1);
            var negativeA = (lineP1.Z * lineP0.Y - lineP0.Z * lineP1.Y) / (lineP1.X * lineP0.Y - lineP0.X * lineP1.Y);
            var negativeB = (lineP1.Z * lineP0.X - lineP0.Z * lineP1.X) / (lineP1.Y * lineP0.X - lineP0.Y * lineP1.X);
            List<AdvancedPoint> result = new List<AdvancedPoint>();
            foreach (var point in points)
                result.Add(new AdvancedPoint(new XYZ(point.X, point.Y, negativeA * point.X + negativeB * point.Y), beamLine.Direction, isSolid));
            return result;
        }

        /// <summary>
        /// 获取XY轴在同一直线上的线的Z轴修正后的点,即pre.Direction为(0,0,1)或(0,0,-1)
        /// </summary>
        public static XYZ VL_GetIntersectionOnLine(XYZ pre, XYZ next, XYZ nextDirection)//, bool usingX
        {
            if (Math.Abs(nextDirection.X) > ConstraintsOfBeamAlignToFloor.XYZTolerance)
                return next + ((pre.X - next.X) / nextDirection.X) * nextDirection;
            else if (Math.Abs(nextDirection.Y) > ConstraintsOfBeamAlignToFloor.XYZTolerance)
                return next + ((pre.Y - next.Y) / nextDirection.Y) * nextDirection;
            else
                throw new NotImplementedException("计算错误");
                //return next + ((pre.Z - next.Z) / nextDirection.Z) * nextDirection;
        }

        /// <summary>
        /// 获得线与线相交或者重叠的点
        /// </summary>
        public static List<XYZ> VL_GetIntersectedOrContainedPoints(this Line boardLineZ0, Line beamLineZ0)
        {
            List<XYZ> result = new List<XYZ>();
            var board0 = boardLineZ0.GetEndPoint(0);
            var board1 = boardLineZ0.GetEndPoint(1);
            var beam0 = beamLineZ0.GetEndPoint(0);
            var beam1 = beamLineZ0.GetEndPoint(1);
            var board_leftDown = GetLeftDown(board0, board1);
            var board_rightUp = board_leftDown.VL_XYEqualTo(board0) ? board1 : board0;
            var beam_leftDown = GetLeftDown(beam0, beam1);
            var beam_rightUp = beam_leftDown.VL_XYEqualTo(beam0) ? beam1 : beam0;
            var kBoard = (board_leftDown.Y - board_rightUp.Y) / (board_leftDown.X - board_rightUp.X);
            var kBeam = (beam_leftDown.Y - beam_rightUp.Y) / (beam_leftDown.X - beam_rightUp.X);
            if (kBoard == kBeam)
            {
                //修正板线与梁线重叠的算法
                var up_rightUp = GetLeftDown(board_rightUp, beam_rightUp).VL_XYEqualTo(board_rightUp) ? board_rightUp : beam_rightUp;//XYEqualTo
                var up_leftDown = up_rightUp.VL_XYEqualTo(board_rightUp) ? board_leftDown : beam_leftDown;
                var down_rightUp = up_rightUp.VL_XYEqualTo(board_rightUp) ? beam_rightUp : board_rightUp;
                var down_leftDown = up_rightUp.VL_XYEqualTo(board_rightUp) ? beam_leftDown : board_leftDown;
                if (up_rightUp.VL_XYEqualTo(down_rightUp))
                {
                    if (up_leftDown.VL_XYEqualTo(down_leftDown))
                    {
                        result.Add(up_leftDown);
                        result.Add(up_rightUp);
                    }
                    else if (GetLeftDown(up_leftDown, down_leftDown) == up_leftDown)
                    {
                        result.Add(down_leftDown);
                    }
                    else
                    {
                        result.Add(up_leftDown);
                    }
                }
                else if (up_leftDown.VL_XYEqualTo(down_rightUp))
                {
                    result.Add(up_leftDown);
                }
                else
                {
                    result.Add(up_leftDown);
                    result.Add(down_rightUp);
                }
            }
            else
            {
                double y0, x0;
                if (kBoard == 0)
                {
                    y0 = board0.Y;
                    x0 = double.IsNaN(kBeam) || double.IsInfinity(kBeam) ? beam0.X : (y0 - beam0.Y) / kBeam + beam0.X;
                }
                else if (kBeam == 0)
                {
                    y0 = beam0.Y;
                    x0 = double.IsNaN(kBoard) || double.IsInfinity(kBoard) ? board0.X : (y0 - board0.Y) / kBoard + board0.X;
                }
                else if (double.IsNaN(kBoard)|| double.IsInfinity(kBoard))
                {
                    x0 = board0.X;
                    y0 = kBeam * (board0.X - beam0.X) + beam0.Y;
                }
                else if (double.IsNaN(kBeam) || double.IsInfinity(kBeam))
                {
                    x0 = beam0.X;
                    y0 = kBoard * (beam0.X - board0.X) + board0.Y;
                }
                else
                {
                    x0 = (beam0.Y - board0.Y + kBoard * board0.X - kBeam * beam0.X) / (kBoard - kBeam);
                    y0 = (kBoard * kBeam * (board0.X - beam0.X) + kBoard * beam0.Y - kBeam * board0.Y) / (kBoard - kBeam);
                }
                result.Add(new XYZ(x0, y0, 0));
            }
            return result;
        }

        /// <summary>
        /// 获得左下的点,相同返回前点
        /// </summary>
        private static XYZ GetLeftDown(XYZ p1_0, XYZ p1_1)
        {
            return (p1_0.X < p1_1.X || (p1_0.X == p1_1.X && p1_0.Y <= p1_1.Y)) ? p1_0 : p1_1;
        }
        /// <summary>
        /// 获得左下的点,相同返回前点
        /// </summary>
        public static XYZ GetLeftUp(XYZ p0, XYZ p1)
        {
            return (p0.X < p1.X) || (p0.X == p1.X && p0.Y <= p1.Y) ? p0 : p1;
        }

        /// <summary>
        /// 获取边的所有点
        /// </summary>
        /// <param name="edgeArray"></param>
        /// <returns></returns>
        public static List<XYZ> GetPoints(EdgeArray edgeArray, BeamAlignToFloorModel model)
        {
            List<XYZ> points = new List<XYZ>();
            foreach (Edge edge in edgeArray)
            {
                var tPoints = edge.Tessellate();
                if (tPoints.Count == 2)
                {
                    var point = tPoints[0];
                    if (points.FirstOrDefault(c => c.IsAlmostEqualTo(point, ConstraintsOfBeamAlignToFloor.XYZTolerance)) == null)
                    {
                        points.Add(point + model.Offset);
                    }
                    else
                    {
                        point = tPoints[1];
                        if (points.FirstOrDefault(c => c.IsAlmostEqualTo(point, ConstraintsOfBeamAlignToFloor.XYZTolerance)) == null)
                            points.Add(point + model.Offset);
                    }
                }
                else
                {
                    foreach (var point in tPoints)
                        if (points.FirstOrDefault(c => c.IsAlmostEqualTo(point, ConstraintsOfBeamAlignToFloor.XYZTolerance)) == null)
                            points.Add(point + model.Offset);
                }
            }
            return points;
        }

        /// <summary>
        /// 获取点集合对应的三角区间集合
        /// CARE!!! 暂不支持大于180度角的图形
        /// </summary>
        /// <param name="edgeArray"></param>
        /// <returns></returns>
        public static List<Triangle> GetTriangles(List<XYZ> points)
        {
            if (points.Count <= 2)
                throw new NotImplementedException("点数小于2无法构建最小的三点面");

            List<Triangle> triangles = new List<Triangle>();
            var start = points[0];
            for (int i = 1; i < points.Count - 1; i++)
            {
                triangles.Add(new Triangle(start, points[i], points[i + 1]));
            }
            return triangles;
        }

        public enum VLCoverType
        {
            /// <summary>
            /// 无关
            /// </summary>
            Disjoint,
            /// <summary>
            /// 相交
            /// </summary>
            Intersect,
            /// <summary>
            /// 包含
            /// </summary>
            Contain,
        }

        #region 标注避让 二维平面避让
        public static VLCoverType IsPlanarCover(List<Line> lines, Triangle triangle, Line line)
        {
            //根据线段相交判断
            var p0 = line.GetEndPoint(0);
            var p1 = line.GetEndPoint(1);
            var intersect = lines.FirstOrDefault(c => c.VL_IsIntersect(line));
            if (intersect != null)
                return VLCoverType.Intersect;
            //线段包含判断
            List<XYZ> points = new List<XYZ>();
            points.Add(line.GetEndPoint(0));
            points.Add(line.GetEndPoint(1));
            foreach (var point in points)
                if (triangle.PlanarContains(point))
                    return VLCoverType.Contain;
            return VLCoverType.Disjoint;
        }

        /// <summary>
        /// 检测轮廓是否被包含 另一轮廓
        /// </summary>
        public static bool PlanarContains(List<Triangle> triangles, List<XYZ> points)
        {
            foreach (var pointZ0 in points)
            {
                var container = triangles.AsParallel().FirstOrDefault(c => c.Contains(pointZ0));
                if (container == null)
                    return false;
            }
            return true;
        } 
        #endregion

        /// <summary>
        /// 某轮廓对应的边和三角形 是否与某线有相交或包含关系
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="triangles"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static VLCoverType IsCover(List<Line> lines, List<Triangle> triangles, Line line)
        {
            //根据线段相交判断
            var intersect = lines.FirstOrDefault(c => c.VL_IsIntersect(line));
            if (intersect != null)
                return VLCoverType.Intersect;
            //线段包含判断
            List<XYZ> points = new List<XYZ>();
            points.Add(line.GetEndPoint(0));
            points.Add(line.GetEndPoint(1));
            if (PlanarContains(triangles,points))
                return VLCoverType.Contain;
            return VLCoverType.Disjoint;
        }

        /// <summary>
        /// 三角形包含点
        /// </summary>
        public static bool Contains(this Triangle triangle, XYZ point)
        {
            var vB = triangle.B - triangle.A;
            var vC = triangle.C - triangle.A;
            var vP = point - triangle.A;
            double dotB_B = vB.DotProduct(vB);
            double dotB_C = vB.DotProduct(vC);
            double dotB_P = vB.DotProduct(vP);
            double dotC_C = vC.DotProduct(vC);
            double dotC_P = vC.DotProduct(vP);
            double inverDeno = 1 / (dotB_B * dotC_C - dotB_C * dotB_C);
            double u = (dotC_C * dotB_P - dotB_C * dotC_P) * inverDeno;
            if (Math.Abs(u) < ConstraintsOfBeamAlignToFloor.XYZTolerance)
                u = 0;
            if (u < 0 || u > 1)
                return false;
            double v = (dotB_B * dotC_P - dotB_C * dotB_P) * inverDeno;
            if (Math.Abs(v) < ConstraintsOfBeamAlignToFloor.XYZTolerance)
                v = 0;
            if (v < 0 || v > 1)
                return false;
            return u + v <= 1 + GeometryHelper.XYZTolerance;
        }

        /// <summary>
        /// 一面两点 求线面交点
        /// t = (vT·pT - vT·pL)/(vT·vL)
        /// p = pL + t*vL
        /// </summary>
        public  static XYZ GetIntersection(Triangle triangle, XYZ point, XYZ direction)
        {
            var pT = triangle.A;
            var vT = (triangle.B - triangle.A).CrossProduct(triangle.C - triangle.A);
            var t = (vT.DotProduct(pT) - vT.DotProduct(point)) / vT.DotProduct(direction);
            var p = point + t * direction;
            return p;
        }
    }
}
