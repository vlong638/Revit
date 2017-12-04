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
            

            //try
            //{
            //    StringBuilder sb = new StringBuilder();
            //    UIDocument uidoc = commandData.Application.ActiveUIDocument;
            //    Selection selection = uidoc.Selection;
            //    var elementIds = selection.GetElementIds();
            //    foreach (var elementId in elementIds)
            //    {
            //        var element =uidoc.Document.GetElement(elementId);
            //        sb.AppendLine($"{element.Name}");
            //    }
            //    TaskDialog.Show("vl selected elements", sb.ToString());
            //}
            //catch (Exception ex)
            //{
            //    message = ex.Message;
            //    return Result.Failed;
            //}
            //return Result.Succeeded;



            PmSoft.Common.RevitClass.PickObjectsMouseHook mouseHook = new PmSoft.Common.RevitClass.PickObjectsMouseHook();
            try
            {
                mouseHook.InstallHook();
                //TODO 过滤选梁
                var selectedId = commandData.Application.ActiveUIDocument.Selection.PickObject(ObjectType.Element, new BeamFramingFilter()).ElementId.IntegerValue;
                TaskDialog.Show("1", selectedId.ToString());
            }
            catch
            {
                mouseHook.UninstallHook();
            }
            mouseHook.UninstallHook();
            return Result.Succeeded;
        }
    }
    public class BeamFramingFilter : ISelectionFilter
    {
        RevitLinkInstance rvtIns = null;
        public bool AllowElement(Element elem)
        {
            var doc = elem.Document;
            var category = Category.GetCategory(doc, BuiltInCategory.OST_StructuralFraming);
            return elem.Category.Id == category.Id;
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;

            //Document doc = rvtIns.GetLinkDocument();
            //Element elm = doc.GetElement(reference.LinkedElementId);
            //if (elm is Beam)
            //    return true;
            //return false;
        }
    }
}
