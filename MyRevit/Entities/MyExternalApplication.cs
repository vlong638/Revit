using Autodesk.Revit.UI;
using System;
using System.Windows.Media.Imaging;

namespace MyRevit.Entities
{
    class MyExternalApplication : IExternalApplication
    {
        /// <summary>
        /// 主要负责资源释放
        /// </summary>
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        /// <summary>
        /// 主要负责启动时的定制化处理
        /// </summary>
        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel panel = application.CreateRibbonPanel("VL Ribbon Panel");
            PushButton button = panel.AddItem(new PushButtonData("vlName", "vlText", @"C:\Users\Administrator\Documents\visual studio 2015\Projects\MyRevit\MyRevit\bin\Debug\MyRevit.dll"
                , "MyRevit.Entities.MyExternalCommand")) as PushButton;
            button.LargeImage = new BitmapImage(new Uri(@"C:\Users\Administrator\Documents\Visual Studio 2015\Projects\MyRevit\MyRevit\Images\logoko.png"));
            return Result.Succeeded;
        }
    }
}
