using System;

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
            var displayer = new GraphicsDisplayer();
            Console.ReadLine();

            //string s1 = "1234";
            //string s2 = s1;
            //bool eq = s1 == s2;
            //eq = string.ReferenceEquals(s1, s2);
            //eq = s1.Equals("1234");

            //var i = 1;
            //var j = ++i + i++;

            //List<Student> aas = new List<Student>()
            //{
            //    new Student("孙","宁波"),
            //    new Student("吴","宁波"),
            //    new Student("张","宁波"),
            //    new Student("扶","宁波"),
            //};
            //aas = aas.OrderBy(c => c.Name2).ThenBy(c => c.Name).ToList();

            ////Autodesk.Revit.Creation.
            //var logger = new Log4netLogger();
            //logger.SetupLog("Debug");
            //var logData = new LogData(nameof(Program), nameof(Main), "正在调试中");
            //logger.Error(logData);
            //logger.SetupLog("Info");
            //logger.Info(logData);
            //Console.ReadLine();
        }
    }
}
