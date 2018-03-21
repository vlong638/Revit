using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace PMSoft.ConstructionManagementV2
{
    [Transaction(TransactionMode.Manual)]
    public class CMCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            return CMExe.Execute(commandData, ref message, elements);
        }
    }
}
