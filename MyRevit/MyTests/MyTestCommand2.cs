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
    [Transaction(TransactionMode.Manual)]
    public class MyTestCommand2 : IExternalCommand
    {
        static MyUpdater Updater;

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
            Updater = new MyUpdater(new Guid("63593602-311D-4142-9AF6-50CC97F6B01A"));
            try
            {
                UpdaterRegistry.RegisterUpdater(Updater, true);
                UpdaterRegistry.AddTrigger(Updater.GetUpdaterId(), doc
                    , new LogicalOrFilter(new List<ElementFilter>() { new ElementCategoryFilter(BuiltInCategory.OST_Lines), new ElementCategoryFilter(BuiltInCategory.OST_StructuralFramingTags) })
                    , Element.GetChangeTypeAny());
            }
            catch 
            {
            }
            return Result.Succeeded;
        }

        //public static void Register(Document doc)
        //{
        //    UpdaterRegistry.RegisterUpdater(Updater, true);
        //    UpdaterRegistry.AddTrigger(Updater.GetUpdaterId(), doc
        //        , new LogicalOrFilter(new List<ElementFilter>() { new ElementCategoryFilter(BuiltInCategory.OST_Lines), new ElementCategoryFilter(BuiltInCategory.OST_StructuralFramingTags) })
        //        , Element.GetChangeTypeAny());
        //}
        //public static void Unregister(Document doc)
        //{
        //    UpdaterRegistry.UnregisterUpdater(Updater.GetUpdaterId(), doc);
        //}

        private static void TaskDialogShow(string message)
        {
            TaskDialog.Show("a", message);
        }
    }


    class MyUpdater : IUpdater
    {
        UpdaterId UpdaterId;

        public MyUpdater(Guid _uid)
        {
            this.UpdaterId = new UpdaterId(
                new AddInId(new Guid("B593F2C4-F38C-41D7-AE2C-369BB0149D9B"))
                , _uid);
        }

        public void Execute(UpdaterData updateData)
        {
            var doc = updateData.GetDocument();
            var view = doc.ActiveView;
            var adds = updateData.GetAddedElementIds();
            var edits = updateData.GetModifiedElementIds();
            var deletes = updateData.GetDeletedElementIds();
            var collection = MyTestContext.GetCollection(doc);

            if (edits.Count>0)
            {
                var edit = edits.FirstOrDefault();
                var parallelLength = 9.5;
                var vecticalLength = 1;
                var standardLength = 6;
                var lineMoved = collection.FirstOrDefault(c => c.LineId == edit.IntegerValue);
                if (lineMoved!=null)
                {
                    var entity = lineMoved;
                    if (entity.IsEditing == true)
                    {
                        entity.IsEditing = false;
                        return;
                    }

                    //关联对象
                    var beamCurve = (doc.GetElement(new ElementId(entity.BeamId)).Location as LocationCurve).Curve;
                    var line = doc.GetElement(new ElementId(entity.LineId)) as DetailLine;
                    var tag = doc.GetElement(new ElementId(entity.TagId)) as IndependentTag;
                    //平行 单位向量
                    var parallelVector = (beamCurve as Line).Direction;
                    if (parallelVector.X < 0 || (parallelVector.X == 0 && parallelVector.Y == -1))
                        parallelVector = new XYZ(-parallelVector.X, -parallelVector.Y, parallelVector.Z);
                    //垂直 单位向量
                    var vecticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
                    //重新计算线的位置
                    var lineLocationCurve = line.Location as LocationCurve;
                    var currentPoint0 = lineLocationCurve.Curve.GetEndPoint(0);
                    var currentPoint1 = lineLocationCurve.Curve.GetEndPoint(1);
                    beamCurve.MakeUnbound();
                    var project =beamCurve.Project(currentPoint0);
                    var length = currentPoint1.DistanceTo(project.XYZPoint);
                    currentPoint0 = project.XYZPoint;
                    if (currentPoint1.DistanceTo(currentPoint0)< standardLength)
                        currentPoint1 = currentPoint0 + standardLength * vecticalVector;
                    doc.Delete(line.Id);
                    var newLine = doc.Create.NewDetailCurve(view, Line.CreateBound(currentPoint0, currentPoint1));
                    entity.LineId = newLine.Id.IntegerValue;
                    //重新计算标签的位置
                    tag.TagHeadPosition = currentPoint1 - standardLength * vecticalVector + (parallelLength * parallelVector + vecticalLength * vecticalVector);

                    entity.IsEditing = true;
                    MyTestContext.SaveCollection(doc);
                }
                var tagEntity = collection.FirstOrDefault(c => c.TagId == edit.IntegerValue);
                if (tagEntity != null)
                {
                    var entity = tagEntity;
                    if (entity.IsEditing == true)
                    {
                        entity.IsEditing = false;
                        return;
                    }

                    //关联对象
                    var beamCurve = (doc.GetElement(new ElementId(entity.BeamId)).Location as LocationCurve).Curve;
                    var line = doc.GetElement(new ElementId(entity.LineId)) as DetailLine;
                    var tag = doc.GetElement(new ElementId(entity.TagId)) as IndependentTag;
                    if (line != null)
                        doc.Delete(line.Id);
                    //平行 单位向量
                    var parallelVector = (beamCurve as Line).Direction;
                    if (parallelVector.X < 0 || (parallelVector.X == 0 && parallelVector.Y == -1))
                        parallelVector = new XYZ(-parallelVector.X, -parallelVector.Y, parallelVector.Z);
                    //垂直 单位向量
                    var vecticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
                    //移动后的位置
                    var currentPoint0 = tag.TagHeadPosition - (parallelLength * parallelVector + vecticalLength * vecticalVector);
                    var midPoint = (beamCurve.GetEndPoint(0) + beamCurve.GetEndPoint(1)) / 2;
                    var orientPoint0 = new XYZ(midPoint.X, midPoint.Y, currentPoint0.Z);//梁的Z轴为0与Curve绘制的轴不一致,调整为以Curve为准
                    var currentPoint1 = currentPoint0 + standardLength * vecticalVector;
                    var skewLine = Line.CreateBound(orientPoint0, currentPoint0);
                    currentPoint0 = orientPoint0 + skewLine.Length * Math.Cos(skewLine.Direction.AngleTo(parallelVector)) * parallelVector;
                    var newLine = doc.Create.NewDetailCurve(view, Line.CreateBound(currentPoint0, currentPoint1));
                    entity.LineId = newLine.Id.IntegerValue;

                    entity.IsEditing = true;
                    MyTestContext.SaveCollection(doc);
                }
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
            return nameof(MyUpdater);
        }
    }
}
