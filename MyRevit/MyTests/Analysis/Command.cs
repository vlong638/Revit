using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace MyRevit.MyTests.Analysis
{
    [Transaction(TransactionMode.Manual)]
    class AnalysisCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;
            return new AnalysisSet(uiApp).DoCmd() ? Result.Succeeded : Result.Failed;
        }
    }
}
