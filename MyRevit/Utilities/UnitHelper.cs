using System;

namespace MyRevit.Utilities
{
    /// <summary>
    /// 单位枚举
    /// </summary>
    public enum VLUnitType
    {
        centimeter,//厘米
        millimeter,//毫米
    }

    /// <summary>
    /// 单位换算帮助类
    /// </summary>
    public class UnitHelper
    {
        public static double MiniValueForXYZ = 0.0038;

        #region Foot,英尺
        /// <summary>
        /// Foot Revit中的double数值,是绘图长度的基本单位
        /// </summary>
        static double MmPerFoot = 304.8;
        public static double ConvertFromFootTo(double foot, VLUnitType type)
        {
            switch (type)
            {
                case VLUnitType.centimeter:
                    return foot * MmPerFoot / 10;
                case VLUnitType.millimeter:
                    return foot * MmPerFoot;
                default:
                    throw new NotImplementedException("未实现该类型的单位换算");
            }
        }
        public static double ConvertToFoot(double value, VLUnitType type)
        {
            switch (type)
            {
                case VLUnitType.centimeter:
                    return value / MmPerFoot * 10;
                case VLUnitType.millimeter:
                    return value / MmPerFoot;
                default:
                    throw new NotImplementedException("未实现该类型的单位换算");
            }
        }
        #endregion

        #region Inch,英寸
        static double MmPerInch = 25.4;
        public static double ConvertFromInchTo(double inch, VLUnitType type)
        {
            switch (type)
            {
                case VLUnitType.centimeter:
                    return inch * MmPerInch / 10;
                case VLUnitType.millimeter:
                    return inch * MmPerInch;
                default:
                    throw new NotImplementedException("未实现该类型的单位换算");
            }
        }
        public static double ConvertToInch(double value, VLUnitType type)
        {
            switch (type)
            {
                case VLUnitType.centimeter:
                    return value / MmPerInch * 10;
                case VLUnitType.millimeter:
                    return value / MmPerInch;
                default:
                    throw new NotImplementedException("未实现该类型的单位换算");
            }
        }
        #endregion
    }

    public static class UnitHelperEx
    {
        #region Double
        public static bool IsMiniValue(this double value)
        {
            return Math.Abs(value) < UnitHelper.MiniValueForXYZ;
        } 
        #endregion
    }
}
