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
        public const double SkewLengthForOnLine = 0.5;
        public const double SkewLengthForOffLine = 1.5;
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
        public void Execute(UpdaterData updateData)
        {
            var doc = updateData.GetDocument();
            //var adds = updateData.GetAddedElementIds();
            var edits = updateData.GetModifiedElementIds();
            //var deletes = updateData.GetDeletedElementIds();
            var collection = PipeAnnotationContext.GetCollection(doc);
            if (edits.Count == 0)
                return;

            foreach (var editId in edits)
            {
                var element = doc.GetElement(editId);

                PipeAnnotationEntity entity = null;
                var lineMoved = collection.FirstOrDefault(c => c.LineId == editId.IntegerValue);
                if (lineMoved != null)
                {
                    TaskDialog.Show("警告", "线移动了");
                    entity = lineMoved;
                    //线移动处理
                }
                var pipeMoved = collection.FirstOrDefault(c => c.PipeIds.Contains(editId.IntegerValue));
                if (pipeMoved != null)
                {
                    TaskDialog.Show("警告", "管道移动了");
                    entity = pipeMoved;
                    //管道移动处理
                }
                var tagMoved = collection.FirstOrDefault(c => c.TagIds.Contains(editId.IntegerValue));
                if (tagMoved != null)
                {
                    TaskDialog.Show("警告", "标注移动了");
                    entity = tagMoved;
                    //标注移动处理
                }
            }
            PipeAnnotationContext.SaveCollection(doc);
        }

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
