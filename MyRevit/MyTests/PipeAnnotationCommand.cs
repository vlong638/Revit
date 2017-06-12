using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.Utilities;
using PmSoft.Common.CommonClass;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.Entities
{
    public enum SinglePipeTagLocation
    {
        OnPipe,
        AbovePipe,
    }
    class SinglePipeAnnotationSettings
    {
        public bool NeedLeader { set; get; }
        public double LengthFromLine { set; get; }
        public SinglePipeTagLocation Location { set; get; }
        //public bool Cover { set; get; }
    }

    public enum MultiPipeTagLocation
    {
        OnLineEdge,
        OnLine,
    }
    class MultiPipeAnnotationSettings
    {
        public double LengthBetweenPipe { set; get; }
        public MultiPipeTagLocation Location { set; get; }
        //public bool Cover { set; get; }
    }

    public class PipeFilter : ISelectionFilter
    {
        RevitLinkInstance rvtIns = null;
        public bool AllowElement(Element elem)
        {
            var doc = elem.Document;
            var category = Category.GetCategory(doc, BuiltInCategory.OST_PipeCurves);
            return elem.Category.Id == category.Id;
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
    [Transaction(TransactionMode.Manual)]
    public class PipeAnnotationCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region Recorder验证
            ////Recorder验证 发现使用了doc的全文件路径则功能失效
            //var recorder =  new FaceRecorderForRevit( "Xia", ApplicationPath.GetCurrentPath(doc));
            //var value = recorder.GetValue("Xia1", "", 10);
            //recorder.WriteValue("Xia1","111"); 
            #endregion

            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;
            var view = doc.ActiveView;
            if (view.ViewType != ViewType.FloorPlan &&
                view.ViewType != ViewType.Elevation &&
                view.ViewType != ViewType.DrawingSheet &&
                view.ViewType != ViewType.CeilingPlan &&
                view.ViewType != ViewType.Section &&
                view.ViewType != ViewType.Detail &&
                view.ViewType != ViewType.DraftingView &&
                view.ViewType != ViewType.EngineeringPlan)
            {
                PmSoft.Common.Controls.PMMessageBox.Show("需选择二维视图或者图纸");
                return Result.Cancelled;
            }

            if (false)
            {
                var selectedId = uiDoc.Selection.PickObject(ObjectType.Element, new PipeFilter()).ElementId;
                FamilySymbol tagSymbol = null;
                TransactionHelper.DelegateTransaction(doc, "加载族", () =>
                {
                //查找族类型
                string tagName = "管道尺寸标记";
                    var symbols = new FilteredElementCollector(doc)
                        .WherePasses(new ElementCategoryFilter(BuiltInCategory.OST_PipeTags))
                        .WherePasses(new ElementClassFilter(typeof(FamilySymbol)));
                    var targetSymbol = symbols.FirstOrDefault(c => (c as FamilySymbol).FamilyName == tagName);
                    if (targetSymbol != null)
                        tagSymbol = targetSymbol as FamilySymbol;
                //空时加载族类型
                if (tagSymbol == null)
                    {
                        var symbolFile = @"E:\WorkingSpace\Tasks\0609管道标注\管道尺寸标记.rfa";
                        Family family;
                        if (doc.LoadFamily(symbolFile, out family))
                        {
                        //获取族类型集合Id
                        var familySymbolIds = family.GetFamilySymbolIds();
                            foreach (var familySymbolId in familySymbolIds)
                            {
                                var element = doc.GetElement(familySymbolId) as FamilySymbol;
                                if (element != null && element.FamilyName == tagName)
                                {
                                    tagSymbol = element;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            TaskDialogShow("加载族文件失败");
                        }
                    }
                    return true;
                });
                if (tagSymbol == null)
                    return Result.Failed;

                TransactionHelper.DelegateTransaction(doc, "选择使用的标注", () =>
                {
                    //TODO
                    return true;
                });

                TransactionHelper.DelegateTransaction(doc, "文字位于管道", () =>
                {
                    var pipe = doc.GetElement(selectedId);
                    var locationCurve = (pipe.Location as LocationCurve).Curve;
                    var midPoint = (locationCurve.GetEndPoint(0) + locationCurve.GetEndPoint(1)) / 2;
                    var tag = doc.Create.NewTag(view, pipe, false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, midPoint);
                    return true;
                });

                TransactionHelper.DelegateTransaction(doc, "文字位于管道上方", () =>
                {
                    double length = 8;
                    var pipe = doc.GetElement(selectedId);
                    var locationCurve = (pipe.Location as LocationCurve).Curve;
                    var midPoint = (locationCurve.GetEndPoint(0) + locationCurve.GetEndPoint(1)) / 2;
                    var parallelVector = (locationCurve as Line).Direction;
                    var verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
                    if (verticalVector.Y < 0 || (verticalVector.Y == 0 && verticalVector.X == -1))//控制到一二象限
                    verticalVector = new XYZ(-verticalVector.X, -verticalVector.Y, verticalVector.Z);
                    var tag = doc.Create.NewTag(view, pipe, false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, midPoint + length * verticalVector);
                    return true;
                });

                TransactionHelper.DelegateTransaction(doc, "引线", () =>
                {
                    double length = 5;
                    bool needLeader = true;
                    var pipe = doc.GetElement(selectedId);
                    var locationCurve = (pipe.Location as LocationCurve).Curve;
                    var midPoint = (locationCurve.GetEndPoint(0) + locationCurve.GetEndPoint(1)) / 2;
                    var parallelVector = (locationCurve as Line).Direction;
                    var verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
                    if (verticalVector.Y < 0 || (verticalVector.Y == 0 && verticalVector.X == -1))
                        verticalVector = new XYZ(-verticalVector.X, -verticalVector.Y, verticalVector.Z);
                    var tag = doc.Create.NewTag(view, pipe, needLeader, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, midPoint + length * verticalVector);
                    return true;
                });

                TransactionHelper.DelegateTransaction(doc, "标记距离管道边缘5mm", () =>
                {
                    double length = 5;
                    bool needLeader = false;
                    var pipe = doc.GetElement(selectedId);
                    var locationCurve = (pipe.Location as LocationCurve).Curve;
                    var midPoint = (locationCurve.GetEndPoint(0) + locationCurve.GetEndPoint(1)) / 2;
                    var parallelVector = (locationCurve as Line).Direction;
                    var verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
                    if (verticalVector.Y < 0 || (verticalVector.Y == 0 && verticalVector.X == -1))
                        verticalVector = new XYZ(-verticalVector.X, -verticalVector.Y, verticalVector.Z);
                    var tag = doc.Create.NewTag(view, pipe, needLeader, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, midPoint + length * verticalVector);
                    return true;
                });
            }

            if (false)
            {
                TransactionHelper.DelegateTransaction(doc, "一键标注", () =>
                {
                    //可以通过Tag的TaggedLocalElelemtId获取其对应的对象
                    //Tag是属于某View的
                    var pipesWithTag = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_PipeTags);
                    pipesWithTag = pipesWithTag.WhereElementIsNotElementType();
                    var tagIds = pipesWithTag.Where(c => c.OwnerViewId == view.Id)
                    .Select(c => (c as IndependentTag).TaggedLocalElementId).ToList().Distinct();
                    var pipes = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_PipeCurves);
                    pipes = pipes.WhereElementIsNotElementType();
                    foreach (var pipe in pipes)
                    {
                        if (!tagIds.Contains(pipe.Id))
                        {
                            var locationCurve = (pipe.Location as LocationCurve).Curve;
                            var midPoint = (locationCurve.GetEndPoint(0) + locationCurve.GetEndPoint(1)) / 2;
                            var tag = doc.Create.NewTag(view, pipe, false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, midPoint);
                        }
                    }
                    return true;
                });

            }

            if (true)
            {
                FamilySymbol tagSymbol = null;
                TransactionHelper.DelegateTransaction(doc, "加载族", () =>
                {
                    //查找族类型
                    string tagName = "多管直径标注";
                    var symbols = new FilteredElementCollector(doc)
                        .WherePasses(new ElementCategoryFilter(BuiltInCategory.OST_PipeTags))
                        .WherePasses(new ElementClassFilter(typeof(FamilySymbol)));
                    var targetSymbol = symbols.FirstOrDefault(c => (c as FamilySymbol).FamilyName == tagName);
                    if (targetSymbol != null)
                        tagSymbol = targetSymbol as FamilySymbol;
                    //空时加载族类型
                    if (tagSymbol == null)
                    {
                        var symbolFile = @"E:\WorkingSpace\Tasks\0609管道标注\多管直径标注.rfa";
                        Family family;
                        if (doc.LoadFamily(symbolFile, out family))
                        {
                            //获取族类型集合Id
                            var familySymbolIds = family.GetFamilySymbolIds();
                            foreach (var familySymbolId in familySymbolIds)
                            {
                                var element = doc.GetElement(familySymbolId) as FamilySymbol;
                                if (element != null && element.FamilyName == tagName)
                                {
                                    tagSymbol = element;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            TaskDialogShow("加载族文件失败");
                        }
                    }
                    return true;
                });
                if (tagSymbol == null)
                    return Result.Failed;

                var selectedIds = uiDoc.Selection.PickObjects(ObjectType.Element, new PipeFilter());

                if (TransactionHelper.DelegateTransaction(doc, "多管-平行检测", () =>
                {
                    XYZ verticalVector = null;
                    foreach (var selectedId in selectedIds)
                    {
                        var pipe = doc.GetElement(selectedId) as Autodesk.Revit.DB.Plumbing.Pipe;
                        var direction = ((pipe.Location as LocationCurve).Curve as Line).Direction;
                        if (verticalVector == null)
                            verticalVector = new XYZ(direction.Y, -direction.X, direction.Z);
                        if (direction.X * verticalVector.X + direction.Y * verticalVector.Y != 0)
                            return false;
                    }
                    return true;
                }))
                {
                    TransactionHelper.DelegateTransaction(doc, "多管-文字位于线端", () =>
                    {


                        return true;
                    });
                }
            }


            TransactionHelper.DelegateTransaction(doc, "多管-文字位于线端", () =>
            {
                return true;
            });

            TransactionHelper.DelegateTransaction(doc, "多管-文字位于线上", () =>
            {
                return true;
            });
            TransactionHelper.DelegateTransaction(doc, "多管-一键标注", () =>
            {
                return true;
            });
            TransactionHelper.DelegateTransaction(doc, "多管-包括链接进来的管道", () =>
            {
                return true;
            });
            TransactionHelper.DelegateTransaction(doc, "多管-标注自动避让", () =>
            {
                return true;
            });







            TransactionHelper.DelegateTransaction(doc, "修改标高的基面", () =>
            {
                return true;
            });
            return Result.Succeeded;
        }

        private static void TaskDialogShow(string message)
        {
            TaskDialog.Show("a", message);
        }
    }
}
