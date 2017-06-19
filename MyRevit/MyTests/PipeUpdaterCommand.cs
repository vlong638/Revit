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

            var updater = new PipeAnnotationUpdater(new Guid("B593F2C4-F38C-41D7-AE2C-369BB0149D9B"), new Guid("51879AF5-03AE-4B95-9B86-E24DF7181943"));
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
    class PipeAnnotationUpdater : IUpdater
    {
        UpdaterId UpdaterId;

        public PipeAnnotationUpdater(Guid commandId, Guid updaterId)
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
                    try
                    {
                        creater.RegenerateMultipleTagSymbolByEntity(document, entity);
                    }
                    catch (Exception ex)
                    {
                        var xs = ex.ToString();
                    }
                    IsEditing = true;

                    #region 线移动处理
                    ////线
                    //var line = document.GetElement(new ElementId(entity.LineId));
                    //var lineLocation = line.Location as LocationPoint;
                    ////管道
                    //List<Pipe> pipes = new List<Pipe>();
                    //foreach (var pipeId in entity.PipeIds)
                    //    pipes.Add(document.GetElement(new ElementId(pipeId)) as Pipe);
                    ////平行,垂直 向量
                    //XYZ parallelVector = null;
                    //XYZ verticalVector = null;
                    //parallelVector = ((pipes.First().Location as LocationCurve).Curve as Line).Direction;
                    //verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
                    //parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
                    //verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
                    ////节点计算
                    //List<XYZ> nodePoints = new List<XYZ>();
                    //for (int i = 0; i < pipes.Count(); i++)
                    //{
                    //    var pipe = pipes[i];
                    //    var locationCurve = (pipe.Location as LocationCurve).Curve;
                    //    locationCurve.MakeUnbound();
                    //    nodePoints.Add(locationCurve.Project(lineLocation.Point).XYZPoint);
                    //}
                    ////起始点
                    //var startPoint = nodePoints.First();
                    ////线 参数设置
                    //double deepLength = nodePoints.First().DistanceTo(nodePoints.Last());
                    //double textHeight = UnitHelper.ConvertToInch(AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType);
                    //line.GetParameters(TagProperty.线下探长度.ToString()).First().Set(deepLength);
                    //line.GetParameters(TagProperty.间距.ToString()).First().Set(textHeight);
                    //line.GetParameters(TagProperty.文字行数.ToString()).First().Set(nodePoints.Count());
                    ////偏移量
                    //var skewLength = startPoint.DistanceTo(lineLocation.Point);
                    //var lineHeightParameter = line.GetParameters(TagProperty.线高度1.ToString()).First();
                    //var lineHeight = lineHeightParameter.AsDouble();
                    //if (lineLocation.Point.Y>startPoint.Y)
                    //    lineHeight += skewLength;
                    //else
                    //    lineHeight -= skewLength;
                    //lineHeightParameter.Set(lineHeight);
                    //for (int i = 2; i <= 8; i++)
                    //{
                    //    var first = nodePoints.First();
                    //    if (nodePoints.Count() >= i)
                    //    {
                    //        var cur = nodePoints[i - 1];
                    //        line.GetParameters(string.Format("节点{0}可见性", i)).First().Set(1);
                    //        line.GetParameters(string.Format("节点{0}距离", i)).First().Set(cur.DistanceTo(first));
                    //    }
                    //    else
                    //    {
                    //        line.GetParameters(string.Format("节点{0}可见性", i)).First().Set(0);
                    //        line.GetParameters(string.Format("节点{0}距离", i)).First().Set(0);
                    //    }
                    //}
                    ////标注 创建
                    //List<IndependentTag> tags = new List<IndependentTag>();
                    //switch (entity.LocationType)
                    //{
                    //    case MultiPipeTagLocation.OnLineEdge:
                    //        //添加对应的单管直径标注
                    //        var height = Convert.ToDouble(line.GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) + (nodePoints.Count() - 1) * AnnotationConstaints.TextHeight;
                    //        var skewLength = PipeAnnotationConstaints.SkewLengthForOffLine;
                    //        for (int i = 0; i < pipes.Count(); i++)
                    //        {
                    //            var subTag = Document.Create.NewTag(View, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, startPoint);
                    //            subTag.TagHeadPosition = GetHeadPositionForOnLineEdge(parallelVector, verticalVector, startPoint, height, skewLength, i, subTag);
                    //            tags.Add(subTag);
                    //        }
                    //        break;
                    //    case MultiPipeTagLocation.OnLine:
                    //        //添加对应的单管直径标注
                    //        height = Convert.ToDouble(line.GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) +
                    //         (nodePoints.Count() - 1) * AnnotationConstaints.TextHeight;
                    //        skewLength = PipeAnnotationConstaints.SkewLengthForOnLine;
                    //        for (int i = 0; i < pipes.Count(); i++)
                    //        {
                    //            var subTag = Document.Create.NewTag(View, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, startPoint);
                    //            subTag.TagHeadPosition = GetHeadPositionForOnLine(parallelVector, verticalVector, startPoint, tags, height, skewLength, i, subTag);
                    //            tags.Add(subTag);
                    //        }
                    //        break;
                    //    default:
                    //        result = AnnotationBuildResult.NoLocationType;
                    //        return result == AnnotationBuildResult.Success;
                    //}

                    #endregion
                }
                var pipeMoved = collection.FirstOrDefault(c => c.PipeIds.Contains(editId.IntegerValue));
                if (pipeMoved != null)
                {
                    TaskDialog.Show("警告", "管道移动了");
                    entity = pipeMoved;
                    #region 管道移动处理

                    #endregion
                }
                var tagMoved = collection.FirstOrDefault(c => c.TagIds.Contains(editId.IntegerValue));
                if (tagMoved != null)
                {
                    TaskDialog.Show("警告", "标注移动了");
                    entity = tagMoved;
                    #region 标注移动处理

                    #endregion
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
