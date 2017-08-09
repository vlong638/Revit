using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.Utilities;
using PmSoft.Common.CommonClass;
using PmSoft.Common.RevitClass.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MyRevit.Entities
{
    public class BeamAnnotationConstaints
    {
        public static double parallelLength = 9.5;
        public static int vecticalLength = 1;
        public static int standardLength = 7;
        public static string relatedLineField = "RelatedLineId";
        public static string relatedTagField = "RelatedTagId";
        public static string relatedBeamField = "RelatedBeamId";
        public static string relatedViewField = "RelatedViewId";
    }

    [Transaction(TransactionMode.Manual)]
    public class BeamUpdaterCommand : IExternalCommand
    {
        static BeamUpdater Updater;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region 获取所有选中元素的坐标集合
            //var references = uiDoc.Selection.PickObjects(ObjectType.Element);
            //StringBuilder sb = new StringBuilder();
            //foreach (var reference in references)
            //{
            //    var element = doc.GetElement(reference.ElementId);
            //    var locationCurve = element.Location as LocationCurve;
            //    if (locationCurve != null)
            //    {
            //        sb.AppendLine($"start:{locationCurve.Curve.GetEndPoint(0).ToString()},end:{locationCurve.Curve.GetEndPoint(1).ToString()}");
            //    }
            //} 
            #endregion

            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;
            Updater = new BeamUpdater(new Guid("63593602-311D-4142-9AF6-50CC97F6B01A"));
            var updaterInfo = UpdaterRegistry.GetRegisteredUpdaterInfos().FirstOrDefault(c => c.UpdaterName == Updater.GetUpdaterName());
            if (updaterInfo == null)
            {
                UpdaterRegistry.RegisterUpdater(Updater, true);
                UpdaterRegistry.AddTrigger(Updater.GetUpdaterId(), new LogicalOrFilter(new List<ElementFilter>() { new ElementCategoryFilter(BuiltInCategory.OST_Lines), new ElementCategoryFilter(BuiltInCategory.OST_StructuralFramingTags) })
                    , Element.GetChangeTypeAny());
            }
            var updater2 = new BeamUpdater2(new Guid("54EB0FC2-89AF-466B-91F8-A1CD10985200"));
            updaterInfo = UpdaterRegistry.GetRegisteredUpdaterInfos().FirstOrDefault(c => c.UpdaterName == updater2.GetUpdaterName());
            if (updaterInfo == null)
            {
                UpdaterRegistry.RegisterUpdater(updater2, true);
                UpdaterRegistry.AddTrigger(updater2.GetUpdaterId(), new ElementCategoryFilter(BuiltInCategory.OST_StructuralFramingTags)
                    , Element.GetChangeTypeElementDeletion());
            }
            return Result.Succeeded;
        }

        private static void TaskDialogShow(string message)
        {
            TaskDialog.Show("a", message);
        }
    }


    /// <summary>
    /// 梁,线,标注 位置处理 IUpdater
    /// </summary>
    class BeamUpdater : IUpdater
    {
        UpdaterId UpdaterId;

        public BeamUpdater(Guid _uid)
        {
            this.UpdaterId = new UpdaterId(new AddInId(new Guid("B593F2C4-F38C-41D7-AE2C-369BB0149D9B")), _uid);
        }

        static bool IsEditing;

        #region Extensible Storage 方案
        ///// <summary>
        ///// 采用 Extensible Storage
        ///// </summary>
        ///// <param name="updateData"></param>
        //public void Execute(UpdaterData updateData)
        //{
        //    var doc = updateData.GetDocument();
        //    var adds = updateData.GetAddedElementIds();
        //    var edits = updateData.GetModifiedElementIds();
        //    var deletes = updateData.GetDeletedElementIds();
        //    //var collection = MyTestContext.GetCollection(doc);
        //    if (edits.Count == 0)
        //        return;
        //    if (IsEditing == true)
        //    {
        //        IsEditing = false;
        //        return;
        //    }
        //    double parallelLength = BeamAnnotationConstaints.parallelLength;
        //    int vecticalLength = BeamAnnotationConstaints.vecticalLength;
        //    int standardLength = BeamAnnotationConstaints.standardLength;
        //    string relatedLineField = BeamAnnotationConstaints.relatedLineField;
        //    string relatedTagField = BeamAnnotationConstaints.relatedTagField;
        //    string relatedBeamField = BeamAnnotationConstaints.relatedBeamField;
        //    string relatedViewField = BeamAnnotationConstaints.relatedViewField;
        //    foreach (var editId in edits)
        //    {
        //        var element = doc.GetElement(editId);
        //        View view;
        //        Element beam;
        //        IndependentTag tag;
        //        DetailLine line;
        //        SchemaEntityOpr opr = new SchemaEntityOpr(element);
        //        var relatedBeamId = opr.GetParm(relatedBeamField);
        //        if (string.IsNullOrEmpty(relatedBeamId))
        //            return;
        //        var relatedViewId = opr.GetParm(relatedViewField);
        //        if (string.IsNullOrEmpty(relatedViewId))
        //            return;
        //        beam = doc.GetElement(new ElementId(int.Parse(relatedBeamId)));
        //        view = doc.GetElement(new ElementId(int.Parse(relatedViewId))) as View;
        //        tag = element as IndependentTag;
        //        bool isLineMoved;
        //        if (tag != null)
        //        {
        //            var relatedLineId = opr.GetParm(relatedLineField);
        //            if (string.IsNullOrEmpty(relatedLineId))
        //                return;
        //            line = doc.GetElement(new ElementId(int.Parse(relatedLineId))) as DetailLine;
        //            isLineMoved = false;
        //        }
        //        else
        //        {
        //            line = element as DetailLine;
        //            var relatedTagId = opr.GetParm(relatedTagField);
        //            tag = doc.GetElement(new ElementId(int.Parse(relatedTagId))) as IndependentTag;
        //            isLineMoved = true;
        //        }
        //        var beamCurve = (beam.Location as LocationCurve).Curve;
        //        //平行 单位向量
        //        var parallelVector = (beamCurve as Line).Direction;
        //        if (parallelVector.X < 0 || (parallelVector.X == 0 && parallelVector.Y == -1))
        //            parallelVector = new XYZ(-parallelVector.X, -parallelVector.Y, parallelVector.Z);
        //        //垂直 单位向量
        //        var vecticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
        //        //重新计算线的位置
        //        if (isLineMoved)
        //        {
        //            //重新计算线的位置
        //            var lineLocationCurve = line.Location as LocationCurve;
        //            var currentPoint0 = lineLocationCurve.Curve.GetEndPoint(0);
        //            var currentPoint1 = lineLocationCurve.Curve.GetEndPoint(1);
        //            beamCurve.MakeUnbound();
        //            var project = beamCurve.Project(currentPoint0);
        //            var length = currentPoint1.DistanceTo(project.XYZPoint);
        //            currentPoint0 = project.XYZPoint;
        //            if (currentPoint1.DistanceTo(currentPoint0) < standardLength)
        //                currentPoint1 = currentPoint0 + standardLength * vecticalVector;
        //            doc.Delete(line.Id);
        //            var newLine = doc.Create.NewDetailCurve(view, Line.CreateBound(currentPoint0, currentPoint1));
        //            //重新计算标签的位置
        //            tag.TagHeadPosition = currentPoint1 - standardLength * vecticalVector + (parallelLength * parallelVector + vecticalLength * vecticalVector);
        //            //更新扩展存储
        //            opr = new SchemaEntityOpr(newLine);
        //            opr.SetParm(relatedViewField, view.Id.IntegerValue.ToString());
        //            opr.SetParm(relatedBeamField, beam.Id.IntegerValue.ToString());
        //            opr.SetParm(relatedTagField, tag.Id.IntegerValue.ToString());
        //            opr.SaveTo(newLine);
        //            opr = new SchemaEntityOpr(tag);
        //            opr.SetParm(relatedLineField, newLine.Id.IntegerValue.ToString());
        //            opr.SaveTo(tag);
        //        }
        //        else
        //        {
        //            //重新计算线的位置
        //            var currentPoint0 = tag.TagHeadPosition - (parallelLength * parallelVector + vecticalLength * vecticalVector);
        //            var midPoint = (beamCurve.GetEndPoint(0) + beamCurve.GetEndPoint(1)) / 2;
        //            var orientPoint0 = new XYZ(midPoint.X, midPoint.Y, currentPoint0.Z);//梁的Z轴为0与Curve绘制的轴不一致,调整为以Curve为准
        //            var currentPoint1 = currentPoint0 + standardLength * vecticalVector;
        //            beamCurve.MakeUnbound();
        //            var project = beamCurve.Project(currentPoint0);
        //            if (currentPoint1.DistanceTo(project.XYZPoint) < standardLength)
        //            {
        //                currentPoint0 = project.XYZPoint;
        //                currentPoint1 = currentPoint0 + standardLength * vecticalVector;
        //                //重新计算标签的位置
        //                tag.TagHeadPosition = currentPoint1 - standardLength * vecticalVector + (parallelLength * parallelVector + vecticalLength * vecticalVector);
        //            }
        //            else
        //            {
        //                var skewLine = Line.CreateBound(orientPoint0, currentPoint0);
        //                currentPoint0 = orientPoint0 + skewLine.Length * Math.Cos(skewLine.Direction.AngleTo(parallelVector)) * parallelVector;
        //            }
        //            if (line != null)
        //                doc.Delete(line.Id);
        //            var newLine = doc.Create.NewDetailCurve(view, Line.CreateBound(currentPoint0, currentPoint1));
        //            //更新扩展存储
        //            opr = new SchemaEntityOpr(newLine);
        //            opr.SetParm(relatedViewField, view.Id.IntegerValue.ToString());
        //            opr.SetParm(relatedBeamField, beam.Id.IntegerValue.ToString());
        //            opr.SetParm(relatedTagField, tag.Id.IntegerValue.ToString());
        //            opr.SaveTo(newLine);
        //            opr = new SchemaEntityOpr(tag);
        //            opr.SetParm(relatedLineField, newLine.Id.IntegerValue.ToString());
        //            opr.SaveTo(tag);
        //        }
        //    }
        //    IsEditing = true;// 标签的移动 会触发标签移动的回调
        //} 
        #endregion

        #region MyTestContext.GetCollection方案
        public void Execute(UpdaterData updateData)
        {
            var doc = updateData.GetDocument();
            var adds = updateData.GetAddedElementIds();
            var edits = updateData.GetModifiedElementIds();
            var deletes = updateData.GetDeletedElementIds();
            var collection = MyTestContext.GetCollection(doc);
            if (edits.Count == 0)
                return;
            if (IsEditing == true)
            {
                IsEditing = false;
                return;
            }
            double parallelLength = BeamAnnotationConstaints.parallelLength;
            int vecticalLength = BeamAnnotationConstaints.vecticalLength;
            string relatedLineField = BeamAnnotationConstaints.relatedLineField;
            string relatedTagField = BeamAnnotationConstaints.relatedTagField;
            string relatedBeamField = BeamAnnotationConstaints.relatedBeamField;
            string relatedViewField = BeamAnnotationConstaints.relatedViewField;
            foreach (var editId in edits)
            {
                var element = doc.GetElement(editId);
                View view;
                Element beam;
                IndependentTag tag;
                DetailLine line;
                BeamAnnotationEntity entity = null;
                var lineMoved = collection.FirstOrDefault(c => c.LineId == editId.IntegerValue);
                if (lineMoved != null)
                {
                    entity = lineMoved;
                }
                var tagMoved = collection.FirstOrDefault(c => c.TagId == editId.IntegerValue);
                if (tagMoved != null)
                    entity = tagMoved;
                if (entity == null)
                    return;
                beam = doc.GetElement(new ElementId(entity.BeamId));
                view = doc.GetElement(new ElementId(entity.ViewId)) as View;
                bool isLineMoved;
                if (lineMoved != null)
                {
                    tag= doc.GetElement(new ElementId(entity.TagId)) as IndependentTag;
                    line = doc.GetElement(editId) as DetailLine;
                    isLineMoved = true;
                }
                else
                {
                    tag = doc.GetElement(editId) as IndependentTag;
                    line = doc.GetElement(new ElementId(entity.LineId)) as DetailLine;
                    isLineMoved = false;
                }
                var beamCurve = (beam.Location as LocationCurve).Curve;
                //平行 单位向量
                var parallelVector = (beamCurve as Line).Direction;
                if (parallelVector.X < 0 || (parallelVector.X == 0 && parallelVector.Y == -1))
                    parallelVector = new XYZ(-parallelVector.X, -parallelVector.Y, parallelVector.Z);
                //垂直 单位向量
                var vecticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
                var tagDiagonalXYZ = parallelLength * parallelVector + vecticalLength * vecticalVector;
                //重新计算线的位置
                if (isLineMoved)
                {
                    var lineLocationCurve = line.Location as LocationCurve;
                    var currentPoint0 = lineLocationCurve.Curve.GetEndPoint(0);
                    var currentPoint1 = lineLocationCurve.Curve.GetEndPoint(1);
                    //重新计算线的位置
                    //重新计算标签的位置
                    var tagPoint = CalculateTagPointAndLinePointFromLine(beamCurve, vecticalVector, ref currentPoint0, ref currentPoint1);
                    //模型更新
                    doc.Delete(line.Id);
                    var newLine = doc.Create.NewDetailCurve(view, Line.CreateBound(currentPoint0, currentPoint1));
                    tag.TagHeadPosition = tagPoint + tagDiagonalXYZ;
                    //更新存储
                    entity.LineId = newLine.Id.IntegerValue;
                }
                else
                {
                    //重新计算线的位置
                    //重新计算标签的位置
                    XYZ currentPoint0, currentPoint1;
                    var tagPoint = CalculateTagPointAndLinePointFromTag(beamCurve,tag.TagHeadPosition, parallelVector, vecticalVector, out currentPoint0, out currentPoint1);
                    if (line != null)
                        doc.Delete(line.Id);
                    var newLine = doc.Create.NewDetailCurve(view, Line.CreateBound(currentPoint0, currentPoint1));
                    if (tagPoint != null)
                        tag.TagHeadPosition = tagPoint + tagDiagonalXYZ;
                    //更新存储
                    entity.LineId = newLine.Id.IntegerValue;
                }
            }
            IsEditing = true;// 标签的移动 会触发标签移动的回调
            MyTestContext.SaveCollection(doc);
        }

        private static XYZ CalculateTagPointAndLinePointFromLine( Curve beamCurve,XYZ vecticalVector, ref XYZ currentPoint0, ref XYZ currentPoint1)
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
            return nameof(BeamUpdater);
        }
    }
    /// <summary>
    /// 梁,线,标注 梁删除 IUpdater
    /// </summary>
    class BeamUpdater2 : IUpdater
    {
        UpdaterId UpdaterId;

        public BeamUpdater2(Guid _uid)
        {
            this.UpdaterId = new UpdaterId(new AddInId(new Guid("B593F2C4-F38C-41D7-AE2C-369BB0149D9B")), _uid);
        }

        static bool IsEditing;
        /// <summary>
        /// 采用 Extensible Storage
        /// </summary>
        /// <param name="updateData"></param>
        public void Execute(UpdaterData updateData)
        {
            var doc = updateData.GetDocument();
            var adds = updateData.GetAddedElementIds();
            var edits = updateData.GetModifiedElementIds();
            var deletes = updateData.GetDeletedElementIds();
            var collection = MyTestContext.GetCollection(doc);
            if (deletes.Count == 0)
                return;
            foreach (var deleteId in deletes)
            {
                var entity = collection.FirstOrDefault(c => c.TagId == deleteId.IntegerValue);
                if (entity != null)
                {
                    var line = doc.GetElement(new ElementId(entity.LineId));
                    if (line != null)
                        doc.Delete(line.Id);
                    collection.Remove(entity);
                }


                //var element = doc.GetElement(deleteId);
                //SchemaEntityOpr opr = new SchemaEntityOpr(element);
                //string relatedLineField = BeamAnnotationConstaints.relatedLineField;
                //var relatedLineId = opr.GetParm(relatedLineField);
                //var line = doc.GetElement(new ElementId(int.Parse(relatedLineId))) as DetailLine;
                //if (line != null)
                //    doc.Delete(line.Id);
            }
        }
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
            return nameof(BeamUpdater2);
        }
    }
}
