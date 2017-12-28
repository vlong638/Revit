using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.Entities;
using MyRevit.MyTests.Utilities;
using MyRevit.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.MyTests
{
    [Transaction(TransactionMode.Manual)]
    public class 调研_单点避让 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;

            List<ElementId> elementIds = new List<ElementId>();
            VLHookHelper.DelegateMouseHook(() =>
            {
                elementIds = uiDoc.Selection.PickObjects(ObjectType.Element, new VLClassesFilter(false,
                    typeof(Pipe), typeof(Duct), typeof(CableTray), typeof(Conduit)
                    ), "选择要添加的构件").Select(c => c.ElementId).ToList();
            });

            if (elementIds==null || elementIds.Count() < 2)
                return Result.Failed;
            var selectedElements = elementIds.Select(c => doc.GetElement(c)).ToList();
            AvoidElemntManager manager = new AvoidElemntManager();
            manager.AddElements(selectedElements);
            manager.CheckConflict();
            manager.MergeConflict();
            VLTransactionHelper.DelegateTransaction(doc, "调研_单点避让", () =>
            {
                manager.AutoAvoid(doc);
                return true;
            });
            return Result.Succeeded;
        }
    }
}
