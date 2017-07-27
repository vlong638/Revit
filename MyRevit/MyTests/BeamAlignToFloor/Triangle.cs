using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.MyTests.BeamAlignToFloor
{
    public class Triangle
    {
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
        /// 三角形包含点
        /// </summary>
        public bool Contains(XYZ point)
        {
            var vB = this.B - this.A;
            var vC = this.C - this.A;
            var vP = point - this.A;
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
