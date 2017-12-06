using Autodesk.Revit.DB;
using MyRevit.MyTests.PBPA;
using MyRevit.Utilities;
using System;
using System.Linq;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    /// <summary>
    /// 梁,线,标注 位置处理 IUpdater
    /// </summary>
    public class PBPAUpdater_Delete : IUpdater
    {
        static AddInId AddInId;
        static UpdaterId UpdaterId;

        public PBPAUpdater_Delete(AddInId addinID)
        {
            AddInId = addinID;
            UpdaterId = new UpdaterId(AddInId, new Guid("BBDC7AD2-A42C-426C-99BF-DBB78D3E9A25"));
        }

        #region MyTestContext.GetCollection方案
        public void Execute(UpdaterData updateData)
        {
            try
            {
                //if (PBPAContext.IsDeleting == true)
                //{
                //    PBPAContext.IsDeleting = false;
                //    return;
                //}
                var doc = updateData.GetDocument();
                var deletes = updateData.GetDeletedElementIds();
                var collection = PBPAContext.GetCollection(doc);
                if (deletes.Count == 0)
                    return;
                bool isDeleted = false;
                foreach (var deleteId in deletes)
                {
                    var itemToDelete = collection.Data.FirstOrDefault(c => c.TargetId == deleteId);
                    if (itemToDelete == null)
                        itemToDelete = collection.Data.FirstOrDefault(c => c.AnnotationId == deleteId);
                    if (itemToDelete != null)
                    {
                        itemToDelete.Document = doc;
                        collection.Data.Remove(itemToDelete);
                        itemToDelete.Clear();
                        isDeleted = true;
                    }
                }
                if (isDeleted)
                    PBPAContext.Save(doc);
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
            return "PBPAUpdater_Delete";
        }
    }
}
