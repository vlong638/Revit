using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace MyRevit.MyTests.Utilities
{
    public class Triangle
    {
        public Triangle(Triangle triangle, XYZ offset)
        {
            A = triangle.A + offset;
            B = triangle.B + offset;
            C = triangle.C + offset;
        }

        public Triangle(XYZ a, XYZ b, XYZ c)
        {
            A = a;
            B = b;
            C = c;
        }

        public XYZ A { set; get; }
        public XYZ B { set; get; }
        public XYZ C { set; get; }

        /// <summary>
        /// 三维,三角形包含点
        /// </summary>
        public bool Contains(XYZ point)
        {
            var vB = B - A;
            var vC = C - A;
            var vP = point - A;
            double dotB_B = vB.DotProduct(vB);
            double dotB_C = vB.DotProduct(vC);
            double dotB_P = vB.DotProduct(vP);
            double dotC_C = vC.DotProduct(vC);
            double dotC_P = vC.DotProduct(vP);
            double inverDeno = 1 / (dotB_B * dotC_C - dotB_C * dotB_C);
            double u = (dotC_C * dotB_P - dotB_C * dotC_P) * inverDeno;
            if (Math.Abs(u) < GeometryHelper.XYZTolerance)
                u = 0;
            if (u < 0 || u > 1)
                return false;
            double v = (dotB_B * dotC_P - dotB_C * dotB_P) * inverDeno;
            if (Math.Abs(v) < GeometryHelper.XYZTolerance)
                v = 0;
            if (v < 0 || v > 1)
                return false;
            return u + v <= 1 + GeometryHelper.XYZTolerance;
        }

        /// <summary>
        /// 二维,三角形包含点
        /// </summary>
        public bool PlanarContains(XYZ point)
        {
            var vA = A - C;
            var vB = B - C;
            var vP = point - C;
            var u = (vP.X * vB.Y - vP.Y * vB.X) / (vA.X * vB.Y - vA.Y * vB.X);
            var v = (vP.X * vA.Y - vP.Y * vA.X) / (vB.X * vA.Y - vB.Y * vA.X);
            if (u < 0 || u > 1)
                return false;
            if (v < 0 || v > 1)
                return false;
            return u + v <= 1 + GeometryHelper.XYZTolerance;
        }

        /// <summary>
        /// 获取三角形的边集
        /// </summary>
        /// <returns></returns>
        public List<Line> GetLines()
        {
            return new List<Line>() { Line.CreateBound(A, B), Line.CreateBound(B, C), Line.CreateBound(A, C) };
        }
    }
}
