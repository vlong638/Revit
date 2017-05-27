using VL.Logger;

namespace MyRevit.Utilities
{
    enum LogType
    {
        Debug,
        Release,
    }

    public static class LogHelper
    {
        static Log4netLogger Logger;

        static LogHelper()
        {
            Logger = new Log4netLogger();
            Logger.SetupLog(LogType.Debug.ToString());
        }

        public static void Info(LogData locator)
        {
            Logger.Info(locator);
        }
        public static void Error(LogData locator)
        {
            Logger.Error(locator);
        }
    }
}
