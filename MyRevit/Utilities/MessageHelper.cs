using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.Utilities
{
    class MessageHelper
    {
        public  static void TaskDialogShow(string message,string title= "警告")
        {
            TaskDialog.Show(title, message);
        }
    }
}
