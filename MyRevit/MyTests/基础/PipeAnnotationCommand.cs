using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.MyTests.PipeAnnotation;
using MyRevit.Utilities;
using PmSoft.Optimization.DrawingProduction;
using System;
using System.Linq;

namespace MyRevit.Entities
{

    [Transaction(TransactionMode.Manual)]
    public class PipeAnnotationCommand : IExternalCommand
    {
        static bool generateOnLineEdge = true;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;
            var view = doc.ActiveView;
            if (view.ViewType != ViewType.FloorPlan &&
                view.ViewType != ViewType.Elevation)
            {
                PmSoft.Common.Controls.PMMessageBox.Show("需选择二维视图或者图纸");
                return Result.Cancelled;
            }
            //TODO0817 增加功能面板启动时获取字体高度
            VLTransactionHelper.DelegateTransaction(doc, "获取字体高度", () =>
            {
                PAContext.LoadFamilySymbols(doc);
                return true;
            });
            PAContext.LoadTextHeight(doc);

            PipeAnnotationForm form = new PipeAnnotationForm(new PipeAnnotationCmd(uiApp));
            System.Windows.Forms.DialogResult result = System.Windows.Forms.DialogResult.Retry;
            while ((result = form.ShowDialog()) == System.Windows.Forms.DialogResult.Retry)
            {
                switch (form.ActionType)
                {
                    case ActionType.SelectSinglePipe:
                        var selectedId = uiDoc.Selection.PickObject(ObjectType.Element, new PipeFramingFilter()).ElementId;
                        form.SelectedElementIds.Clear();
                        form.SelectedElementIds.Add(selectedId);
                        form.FinishSelection();
                        break;
                    case ActionType.SelectMultiplePipe:
                        var selectedIds = uiDoc.Selection.PickObjects(ObjectType.Element, new PipeFramingFilter()).Select(c => c.ElementId);
                        form.SelectedElementIds.Clear();
                        form.SelectedElementIds.AddRange(selectedIds);
                        form.FinishSelection();
                        break;
                    default:
                        break;
                }
            }
            return Result.Succeeded;
        }

        private static FamilySymbol LoadFamilySymbol(Document doc, string familyFilePath, string familyName, string name, BuiltInCategory category)
        {
            FamilySymbol symbol = null;
            var symbols = new FilteredElementCollector(doc)
                .WherePasses(new ElementCategoryFilter(category))
                .WherePasses(new ElementClassFilter(typeof(FamilySymbol)));
            var targetSymbol = symbols.FirstOrDefault(c => (c as FamilySymbol).FamilyName == familyName && (c as FamilySymbol).Name == name);
            if (targetSymbol != null)
                symbol = targetSymbol as FamilySymbol;
            //空时加载族类型
            if (symbol == null)
            {
                var symbolFile = familyFilePath;
                Family family;
                if (doc.LoadFamily(symbolFile, out family))
                {
                    //获取族类型集合Id
                    var familySymbolIds = family.GetFamilySymbolIds();
                    foreach (var familySymbolId in familySymbolIds)
                    {
                        var element = doc.GetElement(familySymbolId) as FamilySymbol;
                        if (element != null && element.FamilyName == name)
                        {
                            symbol = element;
                            break;
                        }
                    }
                }
                else
                {
                    TaskDialogShow("加载族文件失败");
                }
            }

            return symbol;
        }


        private static void TaskDialogShow(string message)
        {
            TaskDialog.Show("a", message);
        }
    }
}
