using System;

namespace PmSoft.Optimization.DrawingProduction
{

    /// <summary>
    /// 单位枚举
    /// </summary>
    public enum UnitType
    {
        centimeter,//厘米
        millimeter,//毫米
    }
    /// <summary>
    /// 单位换算帮助类
    /// </summary>
    public class UnitHelper
    {
        #region Foot,英尺
        static double MmPerFoot = 304.8;
        public static double ConvertFromFootTo(double inch, UnitType type)
        {
            switch (type)
            {
                case UnitType.centimeter:
                    return inch * MmPerFoot / 10;
                case UnitType.millimeter:
                    return inch * MmPerFoot;
                default:
                    throw new NotImplementedException("未实现该类型的单位换算");
            }
        }
        public static double ConvertToFoot(double value, UnitType type)
        {
            switch (type)
            {
                case UnitType.centimeter:
                    return value / MmPerFoot * 10;
                case UnitType.millimeter:
                    return value / MmPerFoot;
                default:
                    throw new NotImplementedException("未实现该类型的单位换算");
            }
        }
        #endregion

        #region Inch,英寸
        static double MmPerInch= 25.4;
        public static double ConvertFromInchTo(double inch, UnitType type)
        {
            switch (type)
            {
                case UnitType.centimeter:
                    return inch * MmPerInch / 10;
                case UnitType.millimeter:
                    return inch * MmPerInch;
                default:
                    throw new NotImplementedException("未实现该类型的单位换算");
            }
        }
        public static double ConvertToInch(double value, UnitType type)
        {
            switch (type)
            {
                case UnitType.centimeter:
                    return value / MmPerInch * 10;
                case UnitType.millimeter:
                    return value / MmPerInch;
                default:
                    throw new NotImplementedException("未实现该类型的单位换算");
            }
        }
        #endregion
    }
}
