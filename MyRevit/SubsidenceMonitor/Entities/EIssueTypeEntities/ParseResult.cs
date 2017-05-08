namespace MyRevit.SubsidenceMonitor.Entities
{
    public enum ParseResult
    {
        Success,
        ReportName_ParseFailure,
        Participants_ParseFailure,
        DateTime_ParseFailure,
        DateTime_Invalid,
        Instrument_ParseFailure,
    }
}
