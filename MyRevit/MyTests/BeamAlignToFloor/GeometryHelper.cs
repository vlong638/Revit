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
        public static bool VL_IsRectangleCrossed(XYZ point1, XYZ point2, XYZ point3, XYZ point4)
        {
            var h1 = Math.Abs(point1.Y - point2.Y)/2;
            var h2 = Math.Abs(point3.Y - point4.Y)/2;
            var w1 = Math.Abs(point1.X - point2.X)/2;
            var w2 = Math.Abs(point3.X - point4.X)/2;
            var mid1 = (point1 + point2) / 2;
            var mid2 = (point3 + point4) / 2;
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

        /// <summary>
        /// XYZ的X与Y相同判断
        /// </summary>
        public static bool XYEqualTo(this XYZ point1, XYZ point2)
        {
            return point1.X == point2.X && point1.Y == point2.Y;
        }

        /// <summary>
        /// 获得线与点在Z轴上的交点
        /// </summary>
        /// <param name="boardLine"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public static List<DirectionPoint> VL_GetZLineIntersection(this Line boardLine, List<XYZ> points)
        {
            var board0 = boardLine.GetEndPoint(0);
            var board1 = boardLine.GetEndPoint(1);
            var negativeA = ( board0.Z * board1.Y- board1.Z * board0.Y) / (board0.X * board1.Y - board1.X * board0.Y);
            var negativeB = (board0.Z * board1.X- board1.Z * board0.X) / (board0.Y * board1.X - board1.Y * board0.X);
            List<DirectionPoint> result = new List<DirectionPoint>();
            foreach (var point in points)
                result.Add(new DirectionPoint(new XYZ(point.X, point.Y, negativeA * point.X + negativeB * point.Y), boardLine.Direction));
            return result;
        }

        public static XYZ VL_GetIntersectionOnLine(XYZ board0, XYZ boardDirection, XYZ beam0,XYZ beamDirection)
        {
            if (Math.Abs(beamDirection.Y)< ConstraintsOfBeamAlignToFloor.XYZTolerance)
            {
                return beam0 + ((beam0.X - board0.X) / beamDirection.X) * beamDirection;
            }
            else
            {
                return beam0 + ((beam0.Y - board0.Y) / beamDirection.Y) * beamDirection;
            }


            //var beam1 = beam0 + new XYZ(0, 0, 1);
            //var kBoard = direction.Y / direction.X;
            //var kBeam = (beam1.Y - beam0.Y) / (beam1.X - beam0.X);
            //double y0, x0;
            //if (kBoard == 0)
            //{
            //    y0 = board0.Y;
            //    x0 = double.IsNaN(kBeam) ? beam0.X : (y0 - beam0.Y) / kBeam + beam0.X;
            //}
            //else if (kBeam == 0)
            //{
            //    y0 = beam0.Y;
            //    x0 = double.IsNaN(kBoard) ? board0.X : (y0 - board0.Y) / kBoard + board0.X;
            //}
            //else if (double.IsNaN(kBoard))
            //{
            //    x0 = board0.X;
            //    y0 = kBeam * (board0.X - beam0.X) + beam0.Y;
            //}
            //else if (double.IsNaN(kBeam))
            //{
            //    x0 = beam0.X;
            //    y0 = kBoard * (beam0.X - board0.X) + board0.Y;
            //}
            //else
            //{
            //    x0 = (beam0.Y - board0.Y + kBoard * board0.X - kBeam * beam0.X) / (kBoard - kBeam);
            //    y0 = (kBoard * kBeam * (board0.X - beam0.X) + kBoard * beam0.Y - kBeam * board0.Y) / (kBoard - kBeam);
            //}
            //return new XYZ(x0, y0, 0);
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
            var board_rightUp = board_leftDown.XYEqualTo(board0) ? board1 : board0;
            var beam_leftDown = GetLeftDown(beam0, beam1);
            var beam_rightUp = board_leftDown.XYEqualTo(beam0) ? beam1 : beam0;
            var kBoard = (board_leftDown.Y - board_rightUp.Y) / (board_leftDown.X - board_rightUp.X);
            var kBeam = (beam_leftDown.Y - beam_rightUp.Y) / (beam_leftDown.X - beam_rightUp.X);
            if (kBoard == kBeam)
            {
                //修正板线与梁线重叠的算法
                var up_rightUp = GetLeftDown(board_rightUp, beam_rightUp).XYEqualTo(board_rightUp) ? board_rightUp : beam_rightUp;//XYEqualTo
                var up_leftDown = up_rightUp.XYEqualTo(board_rightUp) ? board_leftDown : beam_leftDown;
                var down_rightUp = up_rightUp.XYEqualTo(board_rightUp) ? beam_rightUp : board_rightUp;
                var down_leftDown = up_rightUp.XYEqualTo(board_rightUp) ? beam_leftDown : board_leftDown;
                if (up_rightUp.XYEqualTo(down_rightUp))
                {
                    if (up_leftDown.XYEqualTo(down_leftDown))
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
                else if (up_leftDown.XYEqualTo(down_rightUp))
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
                    x0 = double.IsNaN(kBeam) ? beam0.X : (y0 - beam0.Y) / kBeam + beam0.X;
                }
                else if (kBeam == 0)
                {
                    y0 = beam0.Y;
                    x0 = double.IsNaN(kBoard) ? board0.X : (y0 - board0.Y) / kBoard + board0.X;
                }
                else if (double.IsNaN(kBoard))
                {
                    x0 = board0.X;
                    y0 = kBeam * (board0.X - beam0.X) + beam0.Y;
                }
                else if (double.IsNaN(kBeam))
                {
                    x0 = beam0.X;
                    y0 = kBoard * (beam0.X - board0.X) + board0.Y;
                }
                else
                {
                    x0 = (beam0.Y - board0.Y + kBoard * board0.X - kBeam * beam0.X) / (kBoard - kBeam);
                    y0 = (kBoard * kBeam * (board0.X -beam0.X) + kBoard * beam0.Y - kBeam * board0.Y) / (kBoard - kBeam);
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
        /// 获取边的所有点
        /// </summary>
        /// <param name="edgeArray"></param>
        /// <returns></returns>
        public static List<XYZ> GetPoints(EdgeArray edgeArray)
        {
            List<XYZ> points = new List<XYZ>();
            foreach (Edge edge in edgeArray)
            {
                var tPoints = edge.Tessellate();
                if (tPoints.Count == 2)
                {
                    points.Add(tPoints[0]);
                }
                else
                {
                    foreach (var point in tPoints)
                        if (points.FirstOrDefault(c => c.IsAlmostEqualTo(point, ConstraintsOfBeamAlignToFloor.XYZTolerance)) == null)
                            points.Add(point);
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
            if (u < 0 || u > 1)
                return false;
            double v = (dotB_B * dotC_P - dotB_C * dotB_P) * inverDeno;
            if (v < 0 || v > 1)
                return false;
            return u + v <= 1;
        }
    }
}
