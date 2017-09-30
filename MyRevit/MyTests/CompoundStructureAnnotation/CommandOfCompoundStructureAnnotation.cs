using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.MyTests.CompoundStructureAnnotation;
using PmSoft.Optimization.DrawingProduction;

namespace MyRevit.MyTests.BeamAlignToFloor
{
    [Transaction(TransactionMode.Manual)]
    public class CommandOfCompoundStructureAnnotation : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;
            return new CompoundStructureAnnotationSet(uiApp).DoCmd() ? Result.Succeeded : Result.Failed;

            //new CompoundStructureAnnotationWindow().ShowDialog();
            //CompoundStructureAnnotationWindow form = new CompoundStructureAnnotationWindow(new CompoundStructureAnnotationSet(uiApp));
            //System.Windows.Forms.DialogResult result = System.Windows.Forms.DialogResult.Retry;
            //while ((result = form.ShowDialog()) == System.Windows.Forms.DialogResult.Retry)
            //{
            //    switch (form.ActionType)
            //    {
            //        case ActionType.SelectSinglePipe:
            //            var selectedId = uiDoc.Selection.PickObject(ObjectType.Element, new PipeFramingFilter()).ElementId;
            //            form.SelectedElementIds.Clear();
            //            form.SelectedElementIds.Add(selectedId);
            //            form.FinishSelection();
            //            break;
            //        case ActionType.SelectMultiplePipe:
            //            var selectedIds = uiDoc.Selection.PickObjects(ObjectType.Element, new PipeFramingFilter()).Select(c => c.ElementId);
            //            form.SelectedElementIds.Clear();
            //            form.SelectedElementIds.AddRange(selectedIds);
            //            form.FinishSelection();
            //            break;
            //        default:
            //            break;
            //    }
            //}
            //return Result.Succeeded;
        }
    }
}
