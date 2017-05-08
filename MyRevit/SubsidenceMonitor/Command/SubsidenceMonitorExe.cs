using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace MyRevit.SubsidenceMonitor.Command
{
    public class SubsidenceMonitorExe
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (!PmSoft.Common.MFCDll.CRevitModelCheck.Instance().HasModel())
            {
                return Result.Cancelled;
            }
            SubsidenceMonitorSet set = new SubsidenceMonitorSet(commandData);
            if (!set.DoCmd())
            {
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
}
