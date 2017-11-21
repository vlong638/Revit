using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.Utilities;
using PmSoft.Common.CommonClass;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.Entities
{
    [Transaction(TransactionMode.Manual)]
    public class AdvancedRevitCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;
            var view = doc.ActiveView;

            TransactionHelper.DelegateTransaction(doc, "扩展存储", () =>
            {
                //ExtensibleStorageHelperV1.Test(doc);
                return true;
            });

            TransactionHelper.DelegateTransaction(doc, "文件存储", () =>
            {
                return true;
            });

            TransactionHelper.DelegateTransaction(doc, "日志系统", () =>
            {
                return true;
            });

            return Result.Succeeded;
        }

        private static void TaskDialogShow(string message)
        {
            TaskDialog.Show("a", message);
        }
    }
}
