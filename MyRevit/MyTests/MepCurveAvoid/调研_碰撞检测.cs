using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.MyTests.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.Entities
{

    [Transaction(TransactionMode.Manual)]
    public class 调研_碰撞检测 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;

            var elementIds = uiDoc.Selection.PickObjects(ObjectType.Element, new VLClassFilter(typeof(Pipe)), "选择要添加的构件").Select(c => c.ElementId);
            if (elementIds.Count() == 0)
                return Result.Cancelled;

            List<Element> pickedElements = elementIds.Select(c => doc.GetElement(c)).ToList();
            foreach (var pickedElement in pickedElements)
            {
                ElementIntersectsElementFilter filter = new ElementIntersectsElementFilter(pickedElement);
                var conflict = pickedElements.Where(c => filter.PassesFilter(c)).ToList();
            }
            return Result.Succeeded;
        }

        private static void TaskDialogShow(string message)
        {
            TaskDialog.Show("a", message);
        }
    }
}
