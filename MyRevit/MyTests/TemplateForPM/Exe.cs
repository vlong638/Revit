using Autodesk.Revit.UI;

namespace PMSoft.ConstructionManagementV2
{
    public class CMExe
    {
        public static Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            CMSet set = new CMSet(commandData.Application);
            if (set.DoCmd())
                return Result.Succeeded;
            else
                return Result.Failed;
        }
    }
}
