using System;

namespace MyRevit.SubsidenceMonitor.Entities
{
    public class DateTimeValue
    {
        public DateTime DateTime { set; get; }
        public double Value { set; get; } = double.NaN;

        public DateTimeValue()
        {
        }
        public DateTimeValue(DateTime dateTime, double value)
        {
            DateTime = dateTime;
            this.Value = value;
        }
    }
    public class DateTimeValues
    {
        public DateTime DateTime;
        public double Value1 { set; get; } = double.NaN;
        public double Value2 { set; get; } = double.NaN;
        public double Value3 { set; get; } = double.NaN;
    }
}
