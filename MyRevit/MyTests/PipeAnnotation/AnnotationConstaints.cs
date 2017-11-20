﻿using MyRevit.MyTests.PipeAnnotationTest;
using MyRevit.Utilities;
using System;
using System.Drawing;

namespace PmSoft.Optimization.DrawingProduction
{
    /// <summary>
    /// 常量,静态量
    /// </summary>
    public class AnnotationConstaints
    {
        public const int PipeCountMax = 7;
        public const double SkewLengthForOnLine = 0.2;
        public const double SkewLengthForOffLine = 0.4;
        public static Font Font = new Font("Angsana New", 21);//21码 7.35毫米
        public static double FontSizeScale = 7.35;
        public static double TextHeight = UnitHelper.ConvertFromFootTo(150, VLUnitType.millimeter);
        public static VLUnitType UnitType = VLUnitType.millimeter;

        public static double MiniValueForXYZ = 0.001;
        /// <summary>
        /// 检测是否是极小值,XYZ经常出现(0,0,0) 其中0可能是某些小于0.001的极小值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsMiniValue(double value)
        {
            return Math.Abs(value) < MiniValueForXYZ;
        }
    }
}
