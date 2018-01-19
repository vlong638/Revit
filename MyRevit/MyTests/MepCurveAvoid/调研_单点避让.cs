using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.Entities;
using MyRevit.MyTests.MepCurveAvoid;
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
            AvoidElemntManager manager = new AvoidElemntManager(uiApp);
            manager.AddElements(selectedElements);
            VLTransactionHelper.DelegateTransaction(doc, "调研_单点避让", () =>
            {
                manager.CheckConflict();
                manager.AutoAvoid(doc);
                return true;
            });
            VLTransactionHelper.DelegateTransaction(doc, "调研_单点避让", () =>
            {
                var result = string.Join(",", manager.ConnectionNodes.Select(c => c.MEPCurve1.Id + "->" + c.MEPCurve2.Id));
                var service = new MEPCurveConnectControlService(uiApp);
                foreach (var ConnectionNode in manager.ConnectionNodes)
                    try
                    {
                        service.NewTwoFitting(ConnectionNode.MEPCurve1, ConnectionNode.MEPCurve2, null);
                        doc.Regenerate();
                    }
                    catch (System.Exception ex)
                    {
                        VLLogHelper.Error(string.Format("Node1:{0},Node2:{1},Error:{2}", ConnectionNode.MEPCurve1.Id, ConnectionNode.MEPCurve2.Id, ex.Message));

                        var error = ex.ToString();
                    }
                return true;
            });
            return Result.Succeeded;
        }
    }
}
