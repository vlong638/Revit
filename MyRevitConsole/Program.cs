using System;
using VL.Logger;

// Configure log4net using the .config file
// This will cause log4net to look for a configuration file
// called ConsoleApp.exe.config in the application base
// directory (i.e. the directory containing ConsoleApp.exe)
namespace MyRevitConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //Autodesk.Revit.Creation.
            var logger = new Log4netLogger();
            logger.SetupLog("Default");
            var logData = new LogData() { ClassName = nameof(Program), FuctionName = nameof(Main), Message = "正在调试中" };
            logger.Error(logData);
            logger.SetupLog("Info");
            logger.Info(logData);
            Console.ReadLine();
        }
    }
}
