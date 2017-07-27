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
    }
}
