using System;

namespace MyRevit.MyTests.PipeAnnotationTest
{
    public enum UnitType
    {
        centimeter,//厘米
        millimeter,//毫米
    }

    public class UnitHelper
    {
        static double MmPerInch = 304.8;//304.8mm
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
    }
}
