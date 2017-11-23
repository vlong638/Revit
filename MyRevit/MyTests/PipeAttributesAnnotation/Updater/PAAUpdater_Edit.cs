using Autodesk.Revit.DB;
using MyRevit.Utilities;
using PmSoft.Common.CommonClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyRevit.MyTests.PAA
{
    /// <summary>
    /// 梁,线,标注 位置处理 IUpdater
    /// </summary>
    public class PAAUpdater_Edit : IUpdater
    {
        static AddInId AddInId;
        static UpdaterId UpdaterId;

        public PAAUpdater_Edit(AddInId addinID)
        {
            AddInId = addinID;
            UpdaterId = new UpdaterId(AddInId, new Guid("43D4A45E-48C4-4AFF-A44D-95C4DEA43370"));
        }

        #region MyTestContext.GetCollection方案
        public void Execute(UpdaterData updateData)
        {
            try
            {
                var document = updateData.GetDocument();
                var edits = updateData.GetModifiedElementIds();
                var collection = PAAContext.GetCollection(document);
                if (PAAContext.IsEditing == true)
                {
                    PAAContext.IsEditing = false;
                    return;
                }
                List<int> movedEntities = new List<int>();
                foreach (var changeId in edits)
                {
                    PAAModelForSingle model = null;
                    //if (VLConstraintsForCSA.Doc == null)
                    //    VLConstraintsForCSA.Doc = document;

                    #region 根据 主体 重新生成
                    var targetMoved = collection.Data.FirstOrDefault(c => c.TargetId.IntegerValue == changeId.IntegerValue);
                    if (targetMoved != null)
                    {
                        model = targetMoved;
                        if (movedEntities.Contains(model.TargetId.IntegerValue))
                            continue;
                        var creater = PAAContext.Creator;
                        var target = document.GetElement(model.TargetId);//标注主体失效时删除
                        if (target == null)
                        {
                            collection.Data.Remove(model);
                            continue;
                        }
                        var targetLocation = target.Location as LocationCurve;
                        var p0 = targetLocation.Curve.GetEndPoint(0);
                        var p1 = targetLocation.Curve.GetEndPoint(1);
                        var pMiddle = new XYZ((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2, (p0.Z + p1.Z) / 2);
                        var offset = pMiddle - model.TargetLocation;
                        model.BodyStartPoint += offset;
                        model.BodyEndPoint += offset;
                        model.LeafEndPoint += offset;
                        PAAContext.Creator.Regenerate(document, model, target);
                        movedEntities.Add(model.TargetId.IntegerValue);
                        //PAAContext.IsEditing = true;//重新生成无需避免移动导致的重复触发
                    }
                    #endregion

                    #region 根据 标注 重新生成
                    var textMoved = collection.Data.FirstOrDefault(c => c.AnnotationId.IntegerValue== changeId.IntegerValue);
                    if (textMoved != null)
                    {
                        model = textMoved;
                        if (movedEntities.Contains(model.TargetId.IntegerValue))
                            continue;
                        var creater = PAAContext.Creator;
                        var target = document.GetElement(model.TargetId);//标注主体失效时删除
                        if (target == null)
                        {
                            collection.Data.Remove(model);
                            continue;
                        }
                        var annotation = document.GetElement(changeId) as IndependentTag;
                        var offset = annotation.TagHeadPosition - model.AnnotationLocation;
                        model.BodyEndPoint += offset;
                        model.LeafEndPoint += offset;
                        PAAContext.Creator.Regenerate(document, model, target);
                        movedEntities.Add(model.TargetId.IntegerValue);
                        //PAAContext.IsEditing = true;//重新生成无需避免移动导致的重复触发
                    }
                    #endregion

                    #region 根据Line重新生成
                    //var lineMoved = collection.Data.FirstOrDefault(c => c.GroupId.IntegerValue == changeId.IntegerValue);
                    //if (lineMoved != null)
                    //{
                    //    model = lineMoved;
                    //    if (movedEntities.Contains(model.TargetId.IntegerValue))
                    //        continue;
                    //    var creater = PAAContext.Creator;
                    //    var target = document.GetElement(model.TargetId);
                    //    if (target == null)
                    //    {
                    //        collection.Data.Remove(model);
                    //        continue;
                    //    }
                    //    var line0 = document.GetElement(model.LineIds[0]);
                    //    var pStart = (line0.Location as LocationCurve).Curve.GetEndPoint(0);
                    //    PAAContext.Creator.Regenerate(document, model, target, pStart - model.BodyStartPoint);
                    //    movedEntities.Add(model.TargetId.IntegerValue);
                    //    //CSAContext.IsEditing = true;//重新生成无需避免移动导致的重复触发
                    //}
                    #endregion
                }
                PAAContext.Save(document);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
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
            return "PAAUpdater_Edit";
        }
    }
}
