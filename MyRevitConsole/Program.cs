using System;
using System.Collections.Generic;
using System.Linq;
using VL.Logger;

// Configure log4net using the .config file
// This will cause log4net to look for a configuration file
// called ConsoleApp.exe.config in the application base
// directory (i.e. the directory containing ConsoleApp.exe)
namespace MyRevitConsole
{
    class Program
    {
        class Student
        {
            public Student(string name, string name2)
            {
                Name = name;
                Name2 = name2;
            }

            public string Name { set; get; }
            public string Name2 { set; get; }
        }

        static void Main(string[] args)
        {
            var i = 1;
            var j = ++i + i++;

            List<Student> aas = new List<Student>()
            {
                new Student("孙","宁波"),
                new Student("吴","宁波"),
                new Student("张","宁波"),
                new Student("扶","宁波"),
            };
            aas = aas.OrderBy(c => c.Name2).ThenBy(c => c.Name).ToList();

            //Autodesk.Revit.Creation.
            var logger = new Log4netLogger();
            logger.SetupLog("Debug");
            var logData = new LogData(nameof(Program), nameof(Main), "正在调试中");
            logger.Error(logData);
            logger.SetupLog("Info");
            logger.Info(logData);
            Console.ReadLine();
        }
    }
}
