using Autodesk.Revit.DB;
using System;
using System.Linq;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    /// <summary>
    /// 梁,线,标注 位置处理 IUpdater
    /// </summary>
    public class CompoundStructureAnnotationUpdater_Delete : IUpdater
    {
        static AddInId AddInId;
        static UpdaterId UpdaterId;

        public CompoundStructureAnnotationUpdater_Delete(AddInId addinID)
        {
            AddInId = addinID;
            UpdaterId = new UpdaterId(AddInId, new Guid("D56BB276-0FEE-4F0A-A5B4-3474B56654B3"));
        }

        #region MyTestContext.GetCollection方案
        public void Execute(UpdaterData updateData)
        {
            try
            {
                var doc = updateData.GetDocument();
                var deletes = updateData.GetDeletedElementIds();
                var collection = CSAContext.GetCollection(doc);
                if (deletes.Count == 0)
                    return;
                bool isDeleted = false;
                foreach (var deleteId in deletes)
                {
                    var itemToDelete = collection.Data.FirstOrDefault(c => c.TargetId.IntegerValue == deleteId.IntegerValue);
                    if (itemToDelete == null)
                        itemToDelete = collection.Data.FirstOrDefault(c => c.TextNoteIds.FirstOrDefault(p => p.IntegerValue == deleteId.IntegerValue) != null);
                    if (itemToDelete != null)
                    {
                        collection.Data.Remove(itemToDelete);
                        var creater = CSAContext.Creater;
                        creater.Clear(doc, itemToDelete);
                        isDeleted = true;
                    }
                }
                if (isDeleted)
                    CSAContext.SaveCollection(doc);
            }
            catch (Exception ex)
            {
                var logger = new TextLogger("PmLogger.txt", @"D:\");
                logger.Error(ex.ToString());
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
            return "CompoundStructureAnnotationUpdater_Delete";
        }
    }
}
