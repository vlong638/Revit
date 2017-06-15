using Autodesk.Revit.DB;
using System;

namespace MyRevit.MyTests.PipeAnnotation
{
    public enum QuadrantType
    {
        One ,
        Two,
        Three,
        Four,
        OneAndFour,
        OneAndTwo,
    }
    public static class LocationHelper
    {
        public static XYZ GetVerticalVector(XYZ vector)
        {
            return new XYZ(vector.Y, -vector.X, 0);
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
    }
}
