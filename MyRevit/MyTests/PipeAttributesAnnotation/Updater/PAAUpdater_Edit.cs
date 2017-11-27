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
                if (PAAContext.IsEditing == true)
                {
                    PAAContext.IsEditing = false;
                    return;
                }
                var document = updateData.GetDocument();
                var edits = updateData.GetModifiedElementIds();
                var collection = PAAContext.GetCollection(document);
                List<int> movedEntities = new List<int>();
                foreach (var changeId in edits)
                {
                    PAAModel model = null;

                    #region 单管 主体 重新生成
                    var targetMoved = collection.Data.FirstOrDefault(c => c.TargetId == changeId);
                    if (targetMoved != null)
                    {
                        model = targetMoved;
                        if (movedEntities.Contains(model.TargetId.IntegerValue))
                            continue;
                        var creater = PAAContext.Creator;
                        var target = document.GetElement(model.TargetId);//标注主体失效时删除
                        var targetLocation = target.Location as LocationCurve;
                        var p0 = targetLocation.Curve.GetEndPoint(0);
                        var p1 = targetLocation.Curve.GetEndPoint(1);
                        var pMiddle = new XYZ((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2, (p0.Z + p1.Z) / 2);
                        var offset = pMiddle - model.TargetLocation;
                        model.BodyStartPoint += offset;
                        model.BodyEndPoint += offset;
                        model.LeafEndPoint += offset;
                        //必要族
                        model.Document = document;
                        PAAContext.Creator.Regenerate(model);
                        movedEntities.Add(model.TargetId.IntegerValue);
                        //PAAContext.IsEditing = true;//重新生成无需避免移动导致的重复触发
                        continue;
                    }
                    #endregion

                    #region 多管 主体 重新生成
                    targetMoved = collection.Data.FirstOrDefault(c => c.TargetIds != null && c.TargetIds.Contains(changeId));
                    if (targetMoved != null)
                    {
                        model = targetMoved;
                        if (movedEntities.Contains(changeId.IntegerValue))
                            continue;
                        model.Document = document;
                        model.IsRegenerate = true;
                        model.RegenerateType = RegenerateType.ByMultipleTarget;
                        PAAContext.Creator.Regenerate(model);
                        movedEntities.AddRange(model.TargetIds.Select(c => c.IntegerValue));
                        //PAAContext.IsEditing = true;//重新生成无需避免移动导致的重复触发
                        continue;
                    }
                    #endregion


                    #region 根据 标注 重新生成
                    var textMoved = collection.Data.FirstOrDefault(c => c.AnnotationId== changeId);
                    if (textMoved != null)
                    {
                        model = textMoved;
                        if (movedEntities.Contains(model.TargetId.IntegerValue))
                            continue;
                        var creater = PAAContext.Creator;
                        var annotation = document.GetElement(changeId) as IndependentTag;
                        var offset = annotation.TagHeadPosition - model.AnnotationLocation;
                        model.BodyEndPoint += offset;
                        model.LeafEndPoint += offset;
                        model.Document = document;
                        PAAContext.Creator.Regenerate( model);
                        movedEntities.Add(model.TargetId.IntegerValue);
                        //PAAContext.IsEditing = true;//重新生成无需避免移动导致的重复触发
                        continue;
                    }
                    #endregion

                    #region 根据Line重新生成
                    var lineMoved = collection.Data.FirstOrDefault(c => c.LineId == changeId);
                    if (lineMoved != null)
                    {
                        model = lineMoved;
                        bool isExisted = false;
                        foreach (var TargetId in model.TargetIds)
                        {
                            if (movedEntities.Contains(TargetId.IntegerValue))
                            {
                                isExisted = true;
                                break;
                            }
                        }
                        if (isExisted)
                            continue;
                        model.Document = document;
                        model.IsRegenerate = true;
                        model.RegenerateType = RegenerateType.ByMultipleLine;
                        PAAContext.Creator.Regenerate(model);
                        PAAContext.IsEditing = true;
                        PAAContext.IsDeleting= true;
                        movedEntities.AddRange(model.TargetIds.Select(c => c.IntegerValue));
                        //var line0 = document.GetElement(model.LineIds[0]);
                        //var pStart = (line0.Location as LocationCurve).Curve.GetEndPoint(0);
                        //PAAContext.Creator.Regenerate(document, model, target, pStart - model.BodyStartPoint);
                        //movedEntities.Add(model.TargetId.IntegerValue);
                        ////CSAContext.IsEditing = true;//重新生成无需避免移动导致的重复触发
                        continue;
                    }
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
