using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace MyRevit.Utilities
{
    public enum CoordinateType
    {
        /// <summary>
        /// 平面使用XY
        /// </summary>
        XY,
        /// <summary>
        /// 东西立面采用YZ
        /// </summary>
        YZ,
        /// <summary>
        /// 南北立面采用XZ
        /// </summary>
        XZ,
    }
    public static class CoordinateTypeEx
    {
        public static XYZ GetParallelVector(this CoordinateType type)
        {
            switch (type)
            {
                case CoordinateType.XY:
                    return new XYZ(1, 0, 0);
                case CoordinateType.YZ:
                    return new XYZ(0, 1, 0);
                case CoordinateType.XZ:
                    return new XYZ(1, 0, 0);
                default:
                    return null;
            }
        }
    }

    public enum QuadrantType
    {
        One,
        Two,
        Three,
        Four,
        OneAndFour,
        OneAndTwo,
    }

    public static class VLLocationHelper
    {
        public static CoordinateType GetCoordinateType(Document doc)
        {
            CoordinateType coordinateType;
            if (new List<string>() { "东", "西" }.Contains(doc.ActiveView.ViewName))
                coordinateType = CoordinateType.YZ;
            else if (new List<string>() { "南", "北" }.Contains(doc.ActiveView.ViewName))
                coordinateType = CoordinateType.XZ;
            else
                coordinateType = CoordinateType.XY;
            return coordinateType;
        }

        public static void GetVectors(Line locationCurve, CoordinateType coordinateType, out XYZ VerticalVector, out XYZ ParallelVector)
        {
            XYZ parallelVector = null;
            XYZ verticalVector = null;
            parallelVector = locationCurve.Direction;
            verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
            parallelVector = VLLocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour, coordinateType);
            verticalVector = VLLocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo, coordinateType);
            double xyzTolarance = 0.01;
            if (Math.Abs(verticalVector.X) > 1 - xyzTolarance)
                verticalVector = new XYZ(-verticalVector.X, -verticalVector.Y, verticalVector.Z);
            VerticalVector = verticalVector;
            ParallelVector = parallelVector;
        }


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
        public static XYZ GetVectorByQuadrant(XYZ vector, QuadrantType quadrantType, CoordinateType coordinateType)
        {
            var result = vector;
            switch (quadrantType)
            {
                case QuadrantType.OneAndFour:
                    switch (coordinateType)
                    {
                        case CoordinateType.XY:
                            if (vector.X < 0 || (vector.X.IsMiniValue() && vector.Y == -1))
                                result = new XYZ(-vector.X, -vector.Y, vector.Z);
                            return result;
                        case CoordinateType.YZ:
                            if (vector.Y < 0 || (vector.Y.IsMiniValue() && vector.Z == -1))
                                result = new XYZ(vector.X, -vector.Y, -vector.Z);
                            return result;
                        case CoordinateType.XZ:
                            if (vector.X < 0 || (vector.X.IsMiniValue() && vector.Z == -1))
                                result = new XYZ(-vector.X, vector.Y, -vector.Z);
                            return result;
                        default:
                            return null;
                    }
                case QuadrantType.OneAndTwo:
                    switch (coordinateType)
                    {
                        case CoordinateType.XY:
                            if (vector.Y < 0 || (vector.Y.IsMiniValue() && vector.X == -1))//控制到一二象限
                                result = new XYZ(-vector.X, -vector.Y, vector.Z);
                            return result;
                        case CoordinateType.YZ:
                            if (vector.Z < 0 || (vector.Z.IsMiniValue() && vector.Y == -1))//控制到一二象限
                                result = new XYZ(vector.X, -vector.Y, -vector.Z);
                            return result;
                        case CoordinateType.XZ:
                            if (vector.Z < 0 || (vector.Z.IsMiniValue() && vector.X == -1))//控制到一二象限
                                result = new XYZ(-vector.X, vector.Y, -vector.Z);
                            return result;
                        default:
                            return null;
                    }
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
        
        /// <summary>
        /// 转移到相同的Z轴
        /// </summary>
        /// <param name="point"></param>
        /// <param name="pointZ"></param>
        /// <returns></returns>
        public static XYZ ToSame(this XYZ point1, XYZ point2, CoordinateType coordinateType)
        {
            switch (coordinateType)
            {
                case CoordinateType.XY:
                    return new XYZ(point1.X, point1.Y, point2.Z);
                case CoordinateType.YZ:
                    return new XYZ(point2.X, point1.Y, point1.Z);
                case CoordinateType.XZ:
                    return new XYZ(point1.X, point2.Y, point1.Z);
                default:
                    return null;
            }
        }
    }
}
