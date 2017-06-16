using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.MyTests.PipeAnnotation;
using MyRevit.Utilities;
using PmSoft.Common.CommonClass;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MyRevit.Entities
{

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
                var selectedIds = uiDoc.Selection.PickObjects(ObjectType.Element, new PipeFilter());
                if (false)
                {
                    TransactionHelper.DelegateTransaction(doc, "多管-平行检测", () =>
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
                    });
                }
                FamilySymbol singleTagSymbol = null;
                FamilySymbol multipleTagSymbol = null;
                if (!TransactionHelper.DelegateTransaction(doc, "多管-族加载", () =>
                {
                    singleTagSymbol = LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0609管道标注\管道尺寸标记.rfa", "管道尺寸标记", "管道尺寸标记", BuiltInCategory.OST_PipeTags);
                    multipleTagSymbol = LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0609管道标注\多管直径标注.rfa", "多管直径标注", "引线标注_文字在右端", BuiltInCategory.OST_DetailComponents);
                    return true;
                }))
                    return Result.Failed;

                var familyDoc = doc.EditFamily(singleTagSymbol.Family);
                var textElement = new FilteredElementCollector(familyDoc).OfClass(typeof(TextElement)).First(c => c.Name == "2.5") as TextElement;
                var textSizeStr = textElement.Symbol.get_Parameter(BuiltInParameter.TEXT_SIZE).AsValueString();
                double textSize = double.Parse(textSizeStr.Substring(0, textSizeStr.IndexOf(" mm")));
                double widthScale = double.Parse(textElement.Symbol.get_Parameter(BuiltInParameter.TEXT_WIDTH_SCALE).AsValueString());
                Font font = new Font("Angsana New", 20);
                TransactionHelper.DelegateTransaction(doc, "多管-文字位于线端", () =>
                {
                    if (singleTagSymbol == null || multipleTagSymbol == null)
                        return false;

                    AnnotationCreater builder = new AnnotationCreater(doc, singleTagSymbol, multipleTagSymbol, textSize, widthScale);
                    builder.Calculator = new LocationCalculator(textSize);
                    switch (builder.GenerateMultipleTagSymbol(selectedIds))
                    {
                        case AnnotationBuildResult.Success:
                            return true;
                        case AnnotationBuildResult.NotParallel:
                        case AnnotationBuildResult.NoLocationType:
                        default:
                            return false;
                    }

                    //var collection = PipeAnnotationContext.GetCollection(doc);
                    //PipeAnnotationEntity entity = new PipeAnnotationEntity();
                    //entity.ViewId = view.Id.IntegerValue;
                    //XYZ parallelVector = null;
                    //XYZ verticalVector = null;
                    //List<Pipe> pipes = new List<Pipe>();
                    //foreach (var selectedId in selectedIds)
                    //{
                    //    pipes.Add(doc.GetElement(selectedId) as Pipe);
                    //    entity.PipeIds.Add(selectedId.ElementId.IntegerValue);
                    //}
                    ////平行检测
                    //foreach (var pipe in pipes)
                    //{
                    //    var direction = ((pipe.Location as LocationCurve).Curve as Line).Direction;
                    //    if (parallelVector == null)
                    //    {
                    //        parallelVector = direction;
                    //        verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
                    //        parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
                    //        verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
                    //    }
                    //    var crossProduct = direction.CrossProduct(verticalVector);
                    //    if (crossProduct.X != 0 || crossProduct.Y != 0)
                    //        return false;
                    //}
                    ////节点计算
                    //List<XYZ> nodePoints = new List<XYZ>();
                    //foreach (var pipe in pipes)
                    //{
                    //    var locationCurve = (pipe.Location as LocationCurve).Curve;
                    //    if (nodePoints.Count > 0)
                    //    {
                    //        locationCurve.MakeUnbound();
                    //        nodePoints.Add(locationCurve.Project(nodePoints.First()).XYZPoint);
                    //    }
                    //    else
                    //    {
                    //        var midPoint = (locationCurve.GetEndPoint(0) + locationCurve.GetEndPoint(1)) / 2;
                    //        nodePoints.Add(midPoint);
                    //    }
                    //}
                    //var unitType = MyTests.PipeAnnotation.UnitType.millimeter;
                    //var textHeight = 150;
                    ////线下探长度
                    //double deepLength = nodePoints.First().DistanceTo(nodePoints.Last());
                    //nodePoints = nodePoints.OrderByDescending(c => c.Y).ToList();
                    ////if (verticalVector.Y < 0 || (verticalVector.Y == 0 && verticalVector.X == -1))//控制到一二象限
                    ////    verticalVector = new XYZ(-verticalVector.X, -verticalVector.Y, verticalVector.Z);
                    ////起始点
                    //var startPoint = nodePoints.First();// - verticalVector * deepLength;
                    ////线
                    //if (!multipleTagSymbol.IsActive)
                    //    multipleTagSymbol.Activate();
                    //var line = doc.Create.NewFamilyInstance(startPoint, multipleTagSymbol, view);
                    //entity.LineId = line.Id.IntegerValue;
                    ////线 旋转处理
                    //if (verticalVector.Y != 1)
                    //{
                    //    LocationPoint locationPoint = line.Location as LocationPoint;
                    //    if (locationPoint != null)
                    //    {
                    //        locationPoint.RotateByXY(startPoint, verticalVector);

                    //        //var start = startPoint;
                    //        //Line axis = Line.CreateBound(start, start.Add(new XYZ(0, 0, 10)));
                    //        //locationPoint.Rotate(axis, verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
                    //    }
                    //}
                    ////节点偏移设置
                    //line.GetParameters(TagProperty.线下探长度.ToString()).First().Set(deepLength);
                    //line.GetParameters(TagProperty.间距.ToString()).First().Set(UnitHelper.ConvertToInch(textHeight, unitType));
                    //for (int i = 2; i <= 8; i++)
                    //{
                    //    var first = nodePoints.First();
                    //    if (nodePoints.Count() >= i)
                    //    {
                    //        var cur = nodePoints[i - 1];
                    //        line.GetParameters(string.Format("节点{0}可见性", i)).First().Set(1);
                    //        line.GetParameters(string.Format("节点{0}距离", i)).First().Set(cur.DistanceTo(first));
                    //        //tag.GetParameters(string.Format("节点{0}距离", i)).First().Set(UnitHelper.ConvertFromInchTo(cur.DistanceTo(first), unitType));
                    //    }
                    //    else
                    //    {
                    //        line.GetParameters(string.Format("节点{0}可见性", i)).First().Set(0);
                    //        line.GetParameters(string.Format("节点{0}距离", i)).First().Set(0);
                    //    }
                    //}
                    //line.GetParameters(TagProperty.文字行数.ToString()).First().Set(nodePoints.Count());
                    //if (false)
                    //{
                    //    entity.LocationType = MultiPipeTagLocation.OnLineEdge;
                    //    #region 文字位于线端
                    //    //添加对应的单管直径标注
                    //    line.GetParameters(TagProperty.线宽度.ToString()).First().Set(UnitHelper.ConvertToInch(200, unitType));
                    //    var height = Convert.ToDouble(line.GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) +
                    //     (nodePoints.Count() - 1) * textHeight;
                    //    var skewLength = PipeAnnotationConstaints.SkewLengthForOffLine;
                    //    //var typeList= new FilteredElementCollector(doc).OfClass(typeof(TextNoteType)).ToElements().Select(p => p as TextNoteType).ToList();
                    //    for (int i = 0; i < pipes.Count(); i++)
                    //    {
                    //        var subTag = doc.Create.NewTag(view, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal
                    //            , startPoint + UnitHelper.ConvertToInch(height - i * textHeight, unitType) * verticalVector + skewLength * parallelVector);
                    //        var text = subTag.TagText;
                    //        var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, font).Width;
                    //        var actualLength = textLength / (textSize * widthScale);
                    //        subTag.TagHeadPosition += actualLength / 25.4 * parallelVector;
                    //    }
                    //    #endregion
                    //}
                    //if (true)
                    //{
                    //    entity.LocationType = MultiPipeTagLocation.OnLine;
                    //    #region 文字位于线上
                    //    //添加对应的单管直径标注
                    //    line.GetParameters(TagProperty.线宽度.ToString()).First().Set(UnitHelper.ConvertToInch(800, unitType));
                    //    var height = Convert.ToDouble(line.GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) +
                    //     (nodePoints.Count() - 1) * textHeight;
                    //    var skewLength = PipeAnnotationConstaints.SkewLengthForOnLine;
                    //    //var typeList= new FilteredElementCollector(doc).OfClass(typeof(TextNoteType)).ToElements().Select(p => p as TextNoteType).ToList();
                    //    for (int i = 0; i < pipes.Count(); i++)
                    //    {
                    //        var subTag = doc.Create.NewTag(view, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal
                    //            , startPoint + UnitHelper.ConvertToInch(height - (i - 0.5) * textHeight, unitType) * verticalVector + skewLength * parallelVector);
                    //        var text = subTag.TagText;
                    //        var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, font).Width;
                    //        var actualLength = textLength / (textSize * widthScale);
                    //        subTag.TagHeadPosition += actualLength / 25.4 * parallelVector;
                    //        entity.TagIds.Add(subTag.Id.IntegerValue);
                    //    }
                    //    #endregion
                    //}
                    //collection.Add(entity);
                    //collection.Save(doc);
                    //return true;
                });
            }

            TransactionHelper.DelegateTransaction(doc, "多管-文字位于线上", () =>
            {
                FamilySymbol tagSymbol = LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0609管道标注\多管直径标注.rfa", "多管直径标注", "引线标注_文字在线上", BuiltInCategory.OST_DetailComponents);
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

        //private void SetParameterValue(Element element, string parameterName, bool parameterValue)
        //{
        //    SetParameterValue(element, parameterName, Convert.ToInt32(parameterValue));
        //}
        //private void SetParameterValue(Element element, string parameterName, int parameterValue)
        //{
        //    element.GetParameters(parameterName).First().Set(parameterValue);
        //}
        //private void SetParameterValue(Element element, string parameterName, double parameterValue)
        //{
        //    element.GetParameters(parameterName).First().Set(parameterValue);
        //}
        //private void SetParameterValue(Element element, string parameterName, string parameterValue)
        //{
        //    element.GetParameters(parameterName).First().Set(parameterValue);
        //}
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
