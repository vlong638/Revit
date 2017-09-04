using Autodesk.Revit.DB;
using System;

namespace PmSoft.Optimization.DrawingProduction
{
    /// <summary>
    /// 象限枚举
    /// </summary>
    public enum QuadrantType
    {
        One,
        Two,
        Three,
        Four,
        OneAndFour,
        OneAndTwo,
    }

    /// <summary>
    /// 定位辅助类
    /// </summary>
    public static class LocationHelper
    {
        public static double MiniValueForXYZ = 0.001;

        /// <summary>
        /// 校准到与targetVector相同的Z轴
        /// </summary>
        /// <param name="parallelVector"></param>
        /// <param name="targetVector"></param>
        /// <returns></returns>
        public static XYZ ToSameZ(XYZ parallelVector, XYZ targetVector)
        {
            return new XYZ(parallelVector.X, parallelVector.Y, targetVector.Z);
        }

        /// <summary>
        /// 获得parallelVector相交的Z轴为0的向量
        /// </summary>
        /// <param name="parallelVector"></param>
        /// <returns></returns>
        public static XYZ GetVerticalVector(XYZ parallelVector)
        {
            return new XYZ(parallelVector.Y, -parallelVector.X, 0);
        }

        /// <summary>
        /// 将方向调整到指定象限
        /// OneAndFour在Y=-1的时候,调整为向上
        /// OneAndTwo在X=-1的时候,调整为向右
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
                    if ((vector.Y == -1 && AnnotationConstaints.IsMiniValue(vector.X)) || vector.X < 0)
                        result = new XYZ(-vector.X, -vector.Y, vector.Z);
                    return result;
                case QuadrantType.OneAndTwo:
                    if ((vector.X == -1 && AnnotationConstaints.IsMiniValue(vector.Y)) || vector.Y < 0)
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

        #region 平面XYZ
        public enum XYZAxle
        {
            X,
            Y,
            Z,
        }

        /// <summary>
        /// 平面XYZ 求垂直向量
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static XYZ GetVerticalVectorForPlaneXYZ(XYZ vector, XYZAxle axle)
        {
            switch (axle)
            {
                case XYZAxle.X:
                    return new XYZ(0, vector.Z, -vector.Y);
                case XYZAxle.Y:
                    return new XYZ(vector.Z, 0, -vector.X);
                case XYZAxle.Z:
                    return new XYZ(vector.Y, -vector.X, 0);
                default:
                    throw new NotImplementedException("该XYZ非平面向量");
            }
        }

        /// <summary>
        /// 将方向调整到指定象限
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="quadrantType"></param>
        /// <returns></returns>
        public static XYZ GetVectorByQuadrantForPlaneXYZ(XYZ vector, QuadrantType quadrantType, XYZAxle axle)
        {
            double axle1, axle2;
            axle1 = axle2 = 0;
            switch (axle)
            {
                case XYZAxle.X:
                    GetVectorByQuadrant(quadrantType, vector.Y, vector.Z, ref axle1, ref axle2);
                    return new XYZ(0, axle1, axle2);
                case XYZAxle.Y:
                    GetVectorByQuadrant(quadrantType, vector.X, vector.Z, ref axle1, ref axle2);
                    return new XYZ(axle1, 0, axle2);
                case XYZAxle.Z:
                    GetVectorByQuadrant(quadrantType, vector.X, vector.Y, ref axle1, ref axle2);
                    return new XYZ(axle1, axle2, 0);
                default:
                    throw new NotImplementedException("该XYZ非平面向量");
            }
        }

        /// <summary>
        /// 将方向调整到指定象限
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="quadrantType"></param>
        /// <returns></returns>
        public static void GetVectorByQuadrant(QuadrantType quadrantType,double axleX,double axleY, ref double axle1, ref double axle2)
        {
            switch (quadrantType)
            {
                case QuadrantType.OneAndFour:
                    if ((axleY == -1 && AnnotationConstaints.IsMiniValue(axleX)) || axleX< 0)
                    {
                        axle1 = -axleX;
                        axle2 = -axleY;
                    }
                    else
                    {
                        axle1 = axleX;
                        axle2 = axleY;
                    }
                    return;
                case QuadrantType.OneAndTwo:
                    if ((axleX== -1 && AnnotationConstaints.IsMiniValue(axleY)) || axleY < 0)
                    {
                        axle1 = -axleX;
                        axle2 = -axleY;
                    }
                    else
                    {
                        axle1 = axleX;
                        axle2 = axleY;
                    }
                    return;
                case QuadrantType.One:
                case QuadrantType.Two:
                case QuadrantType.Three:
                case QuadrantType.Four:
                default:
                    throw new NotImplementedException("未实现07130318");
            }
        }

        public static XYZ ToUnitVector(XYZ vector, XYZAxle defaultAxle = XYZAxle.X)
        {
            if (vector.IsUnitLength())
                return vector;
            else if (vector.GetLength() == 0)
            {
                switch (defaultAxle)
                {
                    case XYZAxle.X:
                        return new XYZ(1, 0, 0);
                    case XYZAxle.Y:
                        return new XYZ(0, 1, 0);
                    case XYZAxle.Z:
                        return new XYZ(0, 0, 1);
                    default:
                        throw new NotImplementedException("未实现07130319");
                }
            }
            else
                return vector * (1 / vector.GetLength());
        }
        #endregion

        /// <summary>
        /// 按XY的点的Z轴旋转
        /// </summary>
        /// <param name="point"></param>
        /// <param name="xyz"></param>
        /// <param name="verticalVector"></param>
        public static void RotateByXY(this LocationPoint point, XYZ xyz, XYZ verticalVector)
        {
            Line axis = Line.CreateBound(xyz, xyz.Add(new XYZ(0, 0, 10)));
            if (verticalVector.X > AnnotationConstaints.MiniValueForXYZ)
                point.Rotate(axis, 2 * Math.PI - verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
            else
                point.Rotate(axis, verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
        }

        /// <summary>
        /// 取向量在指定方向上的投影长度
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="sideVector"></param>
        /// <returns></returns>
        public static double GetLengthBySide(this XYZ vector, XYZ sideVector)
        {
            var v = new XYZ(vector.X, vector.Y, 0);
            var sv = new XYZ(sideVector.X, sideVector.Y, 0);
            if (Math.Abs(v.DotProduct(sv)) < AnnotationConstaints.MiniValueForXYZ)
                return 0;
            return v.GetLength() * Math.Cos(v.AngleTo(sv));
        }
    }
}
