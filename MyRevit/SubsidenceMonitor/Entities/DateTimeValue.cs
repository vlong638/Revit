using System;

namespace MyRevit.SubsidenceMonitor.Entities
{
    public class DateTimeValue
    {
        public DateTime DateTime;
        public double Value;

        public DateTimeValue(DateTime dateTime, double value)
        {
            DateTime = dateTime;
            this.Value = value;
        }
    }
}
