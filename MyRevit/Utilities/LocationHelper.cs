using Autodesk.Revit.DB;
using System;

namespace MyRevit.Utilities
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
        /// <summary>
        /// 获取垂直向量
        /// </summary>
        /// <param name="parallelVector"></param>
        /// <returns></returns>
        public static XYZ GetVerticalVector(XYZ parallelVector)
        {
            return new XYZ(parallelVector.Y, -parallelVector.X, 0);
        }

        /// <summary>
        /// 根据象限获取向量
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="quadrantType"></param>
        /// <returns></returns>
        public static XYZ GetVectorByQuadrant(XYZ vector, QuadrantType quadrantType)
        {
            var result = vector;
            switch (quadrantType)
            {
                case QuadrantType.OneAndFour:
                    if (vector.X < 0 || (vector.X == 0 && vector.Y == -1))
                        result = new XYZ(-vector.X, -vector.Y, vector.Z);
                    return result;
                case QuadrantType.OneAndTwo:
                    if (vector.Y < 0 || (vector.Y == 0 && vector.X == -1))//控制到一二象限
                        result = new XYZ(-vector.X, -vector.Y, vector.Z);
                    return result;
                case QuadrantType.One:
                case QuadrantType.Two:
                case QuadrantType.Three:
                case QuadrantType.Four:
                default:
                    throw new NotImplementedException("");
            }
        }

        /// <summary>
        /// 根据XY轴旋转到线段朝向
        /// </summary>
        /// <param name="point">旋转点</param>
        /// <param name="xyz">方向量</param>
        /// <param name="verticalVector">垂直向量</param>
        public static void RotateByXY(this LocationPoint point, XYZ xyz, XYZ verticalVector)
        {
            Line axis = Line.CreateBound(xyz, xyz.Add(new XYZ(0, 0, 10)));
            point.Rotate(axis, verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
        }

        /// <summary>
        /// 获取方向上的长度
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="sideVector"></param>
        /// <returns></returns>
        public static double GetLengthBySide(this XYZ vector, XYZ sideVector)
        {
            var v = new XYZ(vector.X, vector.Y, 0);
            var sv = new XYZ(sideVector.X, sideVector.Y, 0);
            if (Math.Abs(v.DotProduct(sv)) < 0.01)
                return 0;
            return v.GetLength() * Math.Cos(v.AngleTo(sv));
        }

        /// <summary>
        /// 转移到相同的Z轴
        /// </summary>
        /// <param name="point"></param>
        /// <param name="pointZ"></param>
        /// <returns></returns>
        public static XYZ ToSameZ(this XYZ point, XYZ pointZ)
        {
            return new XYZ(point.X, point.Y, pointZ.Z);
        }
    }
}
