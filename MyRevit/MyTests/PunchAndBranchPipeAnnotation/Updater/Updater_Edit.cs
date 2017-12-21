using Autodesk.Revit.DB;
using MyRevit.Utilities;
using PmSoft.Common.CommonClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyRevit.MyTests.PBPA
{
    /// <summary>
    /// 梁,线,标注 位置处理 IUpdater
    /// </summary>
    public class PBPAUpdater_Edit : IUpdater
    {
        static AddInId AddInId;
        static UpdaterId UpdaterId;

        public PBPAUpdater_Edit(AddInId addinID)
        {
            AddInId = addinID;
            UpdaterId = new UpdaterId(AddInId, new Guid("AFDD17AB-210A-43A4-9F8C-6DBFD7608B1B"));
        }

        #region MyTestContext.GetCollection方案
        public void Execute(UpdaterData updateData)
        {
            try
            {
                //if (PBPAContext.IsEditing == true)
                //{
                //    PBPAContext.IsEditing = false;
                //    return;
                //}
                var document = updateData.GetDocument();
                var edits = updateData.GetModifiedElementIds();
                var collection = PBPAContext.GetCollection(document);
                List<int> movedEntities = new List<int>();
                foreach (var changeId in edits)
                {
                    PBPAModel model = null;

                    #region 单管 主体 重新生成
                    var targetMoved = collection.Data.FirstOrDefault(c => c.TargetId == changeId);
                    if (targetMoved != null)
                    {
                        model = targetMoved;
                        if (movedEntities.Contains(model.TargetId.IntegerValue))
                            continue;
                        var creater = PBPAContext.Creator;
                        var target = document.GetElement(model.TargetId);//标注主体失效时删除
                        var targetLocation = target.Location as LocationCurve;
                        var p0 = targetLocation.Curve.GetEndPoint(0);
                        var p1 = targetLocation.Curve.GetEndPoint(1);
                        var pMiddle = new XYZ((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2, (p0.Z + p1.Z) / 2);
                        var offset = pMiddle - model.TargetLocation;
                        offset.ToSameZ(new XYZ(0, 0, 0));
                        model.BodyStartPoint += offset;
                        model.BodyEndPoint += offset;
                        model.LeafEndPoint += offset;
                        //必要族
                        model.Document = document;
                        model.IsRegenerate = true;
                        if (!PBPAContext.Creator.Regenerate(model))
                            collection.Data.Remove(model);
                        movedEntities.Add(model.TargetId.IntegerValue);
                        //PBPAContext.IsEditing = true;//重新生成无需避免移动导致的重复触发
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
                        var creater = PBPAContext.Creator;
                        var annotation = document.GetElement(changeId) as IndependentTag;
                        var offset = annotation.TagHeadPosition - model.AnnotationLocation;
                        offset.ToSameZ(new XYZ(0, 0, 0));
                        model.BodyEndPoint += offset;
                        model.LeafEndPoint += offset;
                        model.Document = document;
                        model.IsRegenerate = true;
                        if (!PBPAContext.Creator.Regenerate(model))
                            collection.Data.Remove(model);
                        movedEntities.Add(model.TargetId.IntegerValue);
                        //PBPAContext.IsEditing = true;//重新生成无需避免移动导致的重复触发
                        continue;
                    }
                    #endregion
                }
                PBPAContext.SaveCollection(document);
            }
            catch (Exception ex)
            {
                VLLogHelper.Error(ex);
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
            return "PBPAUpdater_Edit";
        }
    }
}
