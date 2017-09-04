using Autodesk.Revit.DB;
using System;

namespace MyRevit.MyTests.PipeAnnotationTest
{
    public enum QuadrantType
    {
        One,
        Two,
        Three,
        Four,
        OneAndFour,
        OneAndTwo,
    }
    public static class LocationHelper
    {
        public static XYZ GetVerticalVector(XYZ parallelVector)
        {
            return new XYZ(parallelVector.Y, -parallelVector.X, 0);
        }

        public static XYZ GetVectorByQuadrant(XYZ vector, QuadrantType quadrantType)
        {
            var result = vector;
            switch (quadrantType)
            {
                case QuadrantType.OneAndFour:
                    if (vector.X < 0 || (vector.X == 0 && vector.Y == -1))
                        result= new XYZ(-vector.X, -vector.Y, vector.Z);
                    return result;
                case QuadrantType.OneAndTwo:
                    if (vector.Y < 0 || (vector.Y == 0 && vector.X == -1))//控制到一二象限
                        result= new XYZ(-vector.X, -vector.Y, vector.Z);
                    return result;
                case QuadrantType.One:
                case QuadrantType.Two:
                case QuadrantType.Three:
                case QuadrantType.Four:
                default:
                    throw new NotImplementedException("");
            }
        }

        public static void RotateByXY(this LocationPoint point , XYZ xyz,XYZ verticalVector)
        {
            Line axis = Line.CreateBound(xyz, xyz.Add(new XYZ(0, 0, 10)));
            point.Rotate(axis, verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
        }

        public static double GetLengthBySide(this XYZ vector, XYZ sideVector)
        {
            var v = new XYZ(vector.X, vector.Y, 0);
            var sv = new XYZ(sideVector.X, sideVector.Y, 0);
            if (Math.Abs(v.DotProduct(sv)) < 0.01)
                return 0;
            return v.GetLength() * Math.Cos(v.AngleTo(sv));
        }
    }
}
