using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace MyRevit.Entities
{
    [Transaction(TransactionMode.Manual)]
    class MyExternalCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("VL title", "VL says Hello Revit");
            return Result.Succeeded;
        }
    }
}
