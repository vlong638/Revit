using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
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
    /// <summary>
    /// 常量集合
    /// </summary>
    public class PipeAnnotationConstaints
    {
        public const double SkewLengthForOnLine = 0.2;
        public const double SkewLengthForOffLine = 0.4;
    }

    /// <summary>
    /// 编辑Updater
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class PipeUpdaterCommand : IExternalApplication
    {

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            var uiApp = application;
            var app = uiApp.ControlledApplication;
            app.DocumentOpened += App_DocumentOpened;

            var updater = new PipeAnnotationEditUpdater(new Guid("B593F2C4-F38C-41D7-AE2C-369BB0149D9B"), new Guid("51879AF5-03AE-4B95-9B86-E24DF7181943"));
            var updaterInfo = UpdaterRegistry.GetRegisteredUpdaterInfos().FirstOrDefault(c => c.UpdaterName == updater.GetUpdaterName());
            if (updaterInfo == null)
            {
                UpdaterRegistry.RegisterUpdater(updater, true);
                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), new LogicalOrFilter(new List<ElementFilter>() {
                    new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves),//管
                    new ElementCategoryFilter(BuiltInCategory.OST_DetailComponents),//线
                    new ElementCategoryFilter(BuiltInCategory.OST_PipeTags),//标注
                })
                , Element.GetChangeTypeAny());
            }
            return Result.Succeeded;
        }

        private void App_DocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs e)
        {
            PipeAnnotationContext.UpdateCreater(e.Document);
        }
    }


    /// <summary>
    /// 梁,线,标注 位置处理 IUpdater
    /// </summary>
    class PipeAnnotationEditUpdater : IUpdater
    {
        UpdaterId UpdaterId;

        public PipeAnnotationEditUpdater(Guid commandId, Guid updaterId)
        {
            this.UpdaterId = new UpdaterId(new AddInId(commandId), updaterId);
        }

        #region MyTestContext.GetCollection方案
        static bool IsEditing;
        public void Execute(UpdaterData updateData)
        {
            var document = updateData.GetDocument();
            var edits = updateData.GetModifiedElementIds();
            var collection = PipeAnnotationContext.GetCollection(document);
            if (IsEditing == true)
            {
                IsEditing = false;
                return;
            }
            List<int> movedEntities = new List<int>();
            foreach (var editId in edits)
            {
                var element = document.GetElement(editId);
                PipeAnnotationEntity entity = null;
                var lineMoved = collection.FirstOrDefault(c => c.LineId == editId.IntegerValue);
                if (lineMoved != null)
                {
                    TaskDialog.Show("警告", "线移动了");
                    entity = lineMoved;
                    var creater = PipeAnnotationContext.Creater;
                    FamilyInstance line = document.GetElement(new ElementId(entity.LineId)) as FamilyInstance;
                    XYZ skewVector = (line.Location as LocationPoint).Point - entity.StartPoint;
                    creater.RegenerateMultipleTagSymbolByEntity(document, entity, skewVector);
                    movedEntities.Add(entity.LineId);
                    IsEditing = true;
                }
                var pipeMoved = collection.FirstOrDefault(c => c.PipeIds.Contains(editId.IntegerValue));
                if (pipeMoved != null)
                {
                    TaskDialog.Show("警告", "管道移动了");
                    entity = pipeMoved;
                    if (movedEntities.Contains(entity.LineId))
                        continue;
                    var creater = PipeAnnotationContext.Creater;
                    XYZ parallelVector = null;
                    XYZ verticalVector = null;
                    parallelVector = ((document.GetElement(new ElementId(entity.PipeIds.First())).Location as LocationCurve).Curve as Line).Direction;
                    verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
                    parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
                    verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
                    int indexOfPipe = entity.PipeIds.IndexOf(editId.IntegerValue);
                    var startPoint = entity.StartPoint;
                    var movedPipe = document.GetElement(new ElementId(editId.IntegerValue)) as Pipe;
                    var pipeLine = (movedPipe.Location as LocationCurve).Curve;
                    pipeLine.MakeUnbound();
                    var projectionPoint = pipeLine.Project(startPoint).XYZPoint;
                    var line = document.GetElement(new ElementId(entity.LineId)) as FamilyInstance;
                    double preHeight = 0;
                    if (indexOfPipe > 0)
                    {
                        preHeight = line.GetParameters(string.Format("节点{0}距离", indexOfPipe + 1)).First().AsDouble();
                        startPoint -= preHeight * verticalVector;
                    }
                    creater.RegenerateMultipleTagSymbolByEntity(document, entity, startPoint - projectionPoint);
                    movedEntities.Add(entity.LineId);
                    IsEditing = true;
                }
                var tagMoved = collection.FirstOrDefault(c => c.TagIds.Contains(editId.IntegerValue));
                if (tagMoved != null)
                {
                    TaskDialog.Show("警告", "标注移动了");
                    entity = tagMoved;
                    if (movedEntities.Contains(entity.LineId))
                        continue;
                    var creater = PipeAnnotationContext.Creater;
                    var index = entity.TagIds.IndexOf(editId.IntegerValue);
                    var subTag = document.GetElement(new ElementId(editId.IntegerValue)) as IndependentTag;
                    var text = subTag.TagText;
                    var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, AnnotationConstaints.Font).Width;
                    var actualLength = textLength / (creater.TextSize * creater.WidthScale);
                    XYZ parallelVector = null;
                    XYZ verticalVector = null;
                    parallelVector = ((document.GetElement(new ElementId(entity.PipeIds.First())).Location as LocationCurve).Curve as Line).Direction;
                    verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
                    parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
                    verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
                    var height = Convert.ToDouble(document.GetElement(new ElementId(entity.LineId)).GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) + (entity.PipeIds.Count() - 1) * AnnotationConstaints.TextHeight;
                    double skewLength = 0;
                    XYZ startPoint = null;
                    switch (entity.LocationType)
                    {
                        case MultiPipeTagLocation.OnLineEdge:
                            skewLength = PipeAnnotationConstaints.SkewLengthForOffLine;
                            startPoint = subTag.TagHeadPosition - skewLength * parallelVector - actualLength / 25.4 * parallelVector
                                - UnitHelper.ConvertToInch(height - index * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType) * verticalVector;
                            break;
                        case MultiPipeTagLocation.OnLine:
                            skewLength = PipeAnnotationConstaints.SkewLengthForOffLine;
                            startPoint = subTag.TagHeadPosition - skewLength * parallelVector - actualLength / 25.4 * parallelVector
                                - UnitHelper.ConvertToInch(height - index * AnnotationConstaints.TextHeight + 0.5 * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType) * verticalVector;
                            break;
                    }
                    XYZ skewVector = startPoint - entity.StartPoint;
                    creater.RegenerateMultipleTagSymbolByEntity(document, entity, skewVector);
                    movedEntities.Add(entity.LineId);
                    IsEditing = true;
                }
            }
            PipeAnnotationContext.SaveCollection(document);
        }

        //private XYZ GetHeadPositionForOnLine(XYZ parallelVector, XYZ verticalVector, XYZ startPoint, List<IndependentTag> tags, double height, double skewLength, int i, IndependentTag subTag)
        //{
        //    var text = subTag.TagText;
        //    var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, AnnotationConstaints.Font).Width;
        //    var actualLength = textLength / (TextSize * WidthScale);
        //    return startPoint + skewLength * parallelVector + actualLength / 25.4 * parallelVector + UnitHelper.ConvertToInch(height - (i - 0.5) * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType) * verticalVector;
        //}

        //private XYZ GetHeadPositionForOnLineEdge(XYZ parallelVector, XYZ verticalVector, XYZ startPoint, double height, double skewLength, int i, IndependentTag subTag)
        //{
        //    var text = subTag.TagText;
        //    var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, Font).Width;
        //    var actualLength = textLength / (TextSize * WidthScale);
        //    return startPoint + skewLength * parallelVector + actualLength / 25.4 * parallelVector + UnitHelper.ConvertToInch(height - i * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType) * verticalVector;
        //}

        private static XYZ CalculateTagPointAndLinePointFromLine(Curve beamCurve, XYZ vecticalVector, ref XYZ currentPoint0, ref XYZ currentPoint1)
        {
            var standardLength = BeamAnnotationConstaints.standardLength;

            var midPoint = (beamCurve.GetEndPoint(0) + beamCurve.GetEndPoint(1)) / 2;
            beamCurve.MakeUnbound();
            var project = beamCurve.Project(currentPoint0);
            var lengthPoint1 = currentPoint1.DistanceTo(project.XYZPoint);
            var lengthPoint0 = currentPoint0.DistanceTo(project.XYZPoint);
            if (lengthPoint0 > lengthPoint1)//在梁上方
            {
                currentPoint1 = project.XYZPoint;
                if (lengthPoint0 < standardLength)
                    currentPoint0 = currentPoint1 - standardLength * vecticalVector;
                return currentPoint0;
            }
            else
            {
                currentPoint0 = project.XYZPoint;
                if (lengthPoint1 < standardLength)
                    currentPoint1 = currentPoint0 + standardLength * vecticalVector;
                return currentPoint1 - standardLength * vecticalVector;
            }
        }
        private static XYZ CalculateTagPointAndLinePointFromTag(Curve beamCurve, XYZ tagPoint, XYZ parallelVector, XYZ vecticalVector, out XYZ currentPoint0, out XYZ currentPoint1)
        {
            var standardLength = BeamAnnotationConstaints.standardLength;
            var parallelLength = BeamAnnotationConstaints.parallelLength;
            var vecticalLength = BeamAnnotationConstaints.vecticalLength;
            var tagDiagonalXYZ = parallelLength * parallelVector + vecticalLength * vecticalVector;

            currentPoint0 = tagPoint - tagDiagonalXYZ;
            var z = currentPoint0.Z;//梁的Z轴为0与Curve绘制的轴不一致,调整为以Curve为准
            var midPoint = (beamCurve.GetEndPoint(0) + beamCurve.GetEndPoint(1)) / 2;
            var orientPoint0 = new XYZ(midPoint.X, midPoint.Y, currentPoint0.Z);//梁的Z轴为0与Curve绘制的轴不一致,调整为以Curve为准
            currentPoint1 = currentPoint0 + standardLength * vecticalVector;
            beamCurve.MakeUnbound();
            var project = beamCurve.Project(currentPoint0);
            if (currentPoint0.DistanceTo(project.XYZPoint) > currentPoint1.DistanceTo(project.XYZPoint))
            {
                currentPoint1 = project.XYZPoint;
                currentPoint1 = new XYZ(currentPoint1.X, currentPoint1.Y, z);
                if (currentPoint0.DistanceTo(project.XYZPoint) < standardLength)
                {
                    currentPoint0 = currentPoint1 - standardLength * vecticalVector;
                    currentPoint0 = new XYZ(currentPoint0.X, currentPoint0.Y, z);
                    return currentPoint0;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                currentPoint0 = project.XYZPoint;
                currentPoint0 = new XYZ(currentPoint0.X, currentPoint0.Y, z);
                if (currentPoint1.DistanceTo(project.XYZPoint) < standardLength)
                {
                    currentPoint1 = currentPoint0 + standardLength * vecticalVector;
                    currentPoint1 = new XYZ(currentPoint1.X, currentPoint1.Y, z);
                    return currentPoint1 - standardLength * vecticalVector;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion

        public string GetAdditionalInformation()
        {
            return "N/A";
        }
        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FreeStandingComponents;
        }
        public UpdaterId GetUpdaterId()
        {
            return UpdaterId;
        }
        public string GetUpdaterName()
        {
            return nameof(MyUpdater);
        }
    }
    /// <summary>
    /// 梁,线,标注 删除 IUpdater
    /// </summary>
    class PipeAnnotationDeleteUpdater : IUpdater
    {
        UpdaterId UpdaterId;

        public PipeAnnotationDeleteUpdater(Guid commandId, Guid updaterId)
        {
            this.UpdaterId = new UpdaterId(new AddInId(commandId), updaterId);
        }

        #region MyTestContext.GetCollection方案
        static bool IsEditing;
        public void Execute(UpdaterData updateData)
        {
            var document = updateData.GetDocument();
            var edits = updateData.GetModifiedElementIds();
            var collection = PipeAnnotationContext.GetCollection(document);
            if (IsEditing == true)
            {
                IsEditing = false;
                return;
            }
            List<int> movedEntities = new List<int>();
            foreach (var editId in edits)
            {
                var element = document.GetElement(editId);
                PipeAnnotationEntity entity = null;
                var lineMoved = collection.FirstOrDefault(c => c.LineId == editId.IntegerValue);
                if (lineMoved != null)
                {
                    TaskDialog.Show("警告", "线移动了");
                    entity = lineMoved;
                    var creater = PipeAnnotationContext.Creater;
                    FamilyInstance movedLine = document.GetElement(new ElementId(entity.LineId)) as FamilyInstance;
                    XYZ skewVector = (movedLine.Location as LocationPoint).Point - entity.StartPoint;
                    creater.RegenerateMultipleTagSymbolByEntity(document, entity, skewVector);
                    movedEntities.Add(entity.LineId);
                    IsEditing = true;
                }
                var pipeMoved = collection.FirstOrDefault(c => c.PipeIds.Contains(editId.IntegerValue));
                if (pipeMoved != null)
                {
                    TaskDialog.Show("警告", "管道移动了");
                    entity = pipeMoved;
                    if (movedEntities.Contains(entity.LineId))
                        continue;
                    var creater = PipeAnnotationContext.Creater;
                    XYZ parallelVector = null;
                    XYZ verticalVector = null;
                    parallelVector = ((document.GetElement(new ElementId(entity.PipeIds.First())).Location as LocationCurve).Curve as Line).Direction;
                    verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
                    parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
                    verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
                    int indexOfPipe = entity.PipeIds.IndexOf(editId.IntegerValue);
                    var startPoint = entity.StartPoint;
                    var movedPipe = document.GetElement(new ElementId(editId.IntegerValue)) as Pipe;
                    var pipeLine = (movedPipe.Location as LocationCurve).Curve;
                    pipeLine.MakeUnbound();
                    var projectionPoint = pipeLine.Project(startPoint).XYZPoint;
                    var line = document.GetElement(new ElementId(entity.LineId)) as FamilyInstance;
                    double preHeight = 0;
                    if (indexOfPipe > 0)
                    {
                        preHeight = line.GetParameters(string.Format("节点{0}距离", indexOfPipe + 1)).First().AsDouble();
                        startPoint -= preHeight * verticalVector;
                    }
                    creater.RegenerateMultipleTagSymbolByEntity(document, entity, startPoint - projectionPoint);
                    movedEntities.Add(entity.LineId);
                    IsEditing = true;
                }
                var tagMoved = collection.FirstOrDefault(c => c.TagIds.Contains(editId.IntegerValue));
                if (tagMoved != null)
                {
                    TaskDialog.Show("警告", "标注移动了");
                    entity = tagMoved;
                    if (movedEntities.Contains(entity.LineId))
                        continue;
                    var creater = PipeAnnotationContext.Creater;
                    var indexOfTag = entity.TagIds.IndexOf(editId.IntegerValue);
                    var subTag = document.GetElement(new ElementId(editId.IntegerValue)) as IndependentTag;
                    var text = subTag.TagText;
                    var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, AnnotationConstaints.Font).Width;
                    var actualLength = textLength / (creater.TextSize * creater.WidthScale);
                    XYZ parallelVector = null;
                    XYZ verticalVector = null;
                    parallelVector = ((document.GetElement(new ElementId(entity.PipeIds.First())).Location as LocationCurve).Curve as Line).Direction;
                    verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
                    parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
                    verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
                    var height = Convert.ToDouble(document.GetElement(new ElementId(entity.LineId)).GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) + (entity.PipeIds.Count() - 1) * AnnotationConstaints.TextHeight;
                    double skewLength = 0;
                    XYZ startPoint = null;
                    switch (entity.LocationType)
                    {
                        case MultiPipeTagLocation.OnLineEdge:
                            skewLength = PipeAnnotationConstaints.SkewLengthForOffLine;
                            startPoint = subTag.TagHeadPosition - skewLength * parallelVector - actualLength / 25.4 * parallelVector
                                - UnitHelper.ConvertToInch(height - indexOfTag * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType) * verticalVector;
                            break;
                        case MultiPipeTagLocation.OnLine:
                            skewLength = PipeAnnotationConstaints.SkewLengthForOffLine;
                            startPoint = subTag.TagHeadPosition - skewLength * parallelVector - actualLength / 25.4 * parallelVector
                                - UnitHelper.ConvertToInch(height - indexOfTag * AnnotationConstaints.TextHeight + 0.5 * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType) * verticalVector;
                            break;
                    }
                    XYZ skewVector = startPoint - entity.StartPoint;
                    creater.RegenerateMultipleTagSymbolByEntity(document, entity, skewVector);
                    movedEntities.Add(entity.LineId);
                    IsEditing = true;
                }
            }
            PipeAnnotationContext.SaveCollection(document);
        }

        //private XYZ GetHeadPositionForOnLine(XYZ parallelVector, XYZ verticalVector, XYZ startPoint, List<IndependentTag> tags, double height, double skewLength, int i, IndependentTag subTag)
        //{
        //    var text = subTag.TagText;
        //    var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, AnnotationConstaints.Font).Width;
        //    var actualLength = textLength / (TextSize * WidthScale);
        //    return startPoint + skewLength * parallelVector + actualLength / 25.4 * parallelVector + UnitHelper.ConvertToInch(height - (i - 0.5) * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType) * verticalVector;
        //}

        //private XYZ GetHeadPositionForOnLineEdge(XYZ parallelVector, XYZ verticalVector, XYZ startPoint, double height, double skewLength, int i, IndependentTag subTag)
        //{
        //    var text = subTag.TagText;
        //    var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, Font).Width;
        //    var actualLength = textLength / (TextSize * WidthScale);
        //    return startPoint + skewLength * parallelVector + actualLength / 25.4 * parallelVector + UnitHelper.ConvertToInch(height - i * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType) * verticalVector;
        //}

        private static XYZ CalculateTagPointAndLinePointFromLine(Curve beamCurve, XYZ vecticalVector, ref XYZ currentPoint0, ref XYZ currentPoint1)
        {
            var standardLength = BeamAnnotationConstaints.standardLength;

            var midPoint = (beamCurve.GetEndPoint(0) + beamCurve.GetEndPoint(1)) / 2;
            beamCurve.MakeUnbound();
            var project = beamCurve.Project(currentPoint0);
            var lengthPoint1 = currentPoint1.DistanceTo(project.XYZPoint);
            var lengthPoint0 = currentPoint0.DistanceTo(project.XYZPoint);
            if (lengthPoint0 > lengthPoint1)//在梁上方
            {
                currentPoint1 = project.XYZPoint;
                if (lengthPoint0 < standardLength)
                    currentPoint0 = currentPoint1 - standardLength * vecticalVector;
                return currentPoint0;
            }
            else
            {
                currentPoint0 = project.XYZPoint;
                if (lengthPoint1 < standardLength)
                    currentPoint1 = currentPoint0 + standardLength * vecticalVector;
                return currentPoint1 - standardLength * vecticalVector;
            }
        }
        private static XYZ CalculateTagPointAndLinePointFromTag(Curve beamCurve, XYZ tagPoint, XYZ parallelVector, XYZ vecticalVector, out XYZ currentPoint0, out XYZ currentPoint1)
        {
            var standardLength = BeamAnnotationConstaints.standardLength;
            var parallelLength = BeamAnnotationConstaints.parallelLength;
            var vecticalLength = BeamAnnotationConstaints.vecticalLength;
            var tagDiagonalXYZ = parallelLength * parallelVector + vecticalLength * vecticalVector;

            currentPoint0 = tagPoint - tagDiagonalXYZ;
            var z = currentPoint0.Z;//梁的Z轴为0与Curve绘制的轴不一致,调整为以Curve为准
            var midPoint = (beamCurve.GetEndPoint(0) + beamCurve.GetEndPoint(1)) / 2;
            var orientPoint0 = new XYZ(midPoint.X, midPoint.Y, currentPoint0.Z);//梁的Z轴为0与Curve绘制的轴不一致,调整为以Curve为准
            currentPoint1 = currentPoint0 + standardLength * vecticalVector;
            beamCurve.MakeUnbound();
            var project = beamCurve.Project(currentPoint0);
            if (currentPoint0.DistanceTo(project.XYZPoint) > currentPoint1.DistanceTo(project.XYZPoint))
            {
                currentPoint1 = project.XYZPoint;
                currentPoint1 = new XYZ(currentPoint1.X, currentPoint1.Y, z);
                if (currentPoint0.DistanceTo(project.XYZPoint) < standardLength)
                {
                    currentPoint0 = currentPoint1 - standardLength * vecticalVector;
                    currentPoint0 = new XYZ(currentPoint0.X, currentPoint0.Y, z);
                    return currentPoint0;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                currentPoint0 = project.XYZPoint;
                currentPoint0 = new XYZ(currentPoint0.X, currentPoint0.Y, z);
                if (currentPoint1.DistanceTo(project.XYZPoint) < standardLength)
                {
                    currentPoint1 = currentPoint0 + standardLength * vecticalVector;
                    currentPoint1 = new XYZ(currentPoint1.X, currentPoint1.Y, z);
                    return currentPoint1 - standardLength * vecticalVector;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion

        public string GetAdditionalInformation()
        {
            return "N/A";
        }
        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FreeStandingComponents;
        }
        public UpdaterId GetUpdaterId()
        {
            return UpdaterId;
        }
        public string GetUpdaterName()
        {
            return nameof(MyUpdater);
        }
    }
}
