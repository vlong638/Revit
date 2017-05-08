using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace MyRevit.SubsidenceMonitor.Command
{
    [Transaction(TransactionMode.Manual)]
    public class SubsidenceMonitorCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return new SubsidenceMonitorExe().Execute(commandData, ref message, elements);
        }
    }
}
