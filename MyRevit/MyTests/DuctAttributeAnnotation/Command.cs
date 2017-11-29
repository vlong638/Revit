using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace MyRevit.MyTests.DAA
{
    [Transaction(TransactionMode.Manual)]
    class DAACommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;
            return new DAASet(uiApp).DoCmd() ? Result.Succeeded : Result.Failed;
        }
    }
}
