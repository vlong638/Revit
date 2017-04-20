using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Text;

namespace MyRevit.Entities
{
    [Transaction(TransactionMode.Manual)]
    class SelectElementCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                Selection selection = uidoc.Selection;
                var elementIds = selection.GetElementIds();
                foreach (var elementId in elementIds)
                {
                    var element =uidoc.Document.GetElement(elementId);
                    sb.AppendLine($"{element.Name}");
                }
                TaskDialog.Show("vl selected elements", sb.ToString());
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
}
