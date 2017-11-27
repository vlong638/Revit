using Autodesk.Revit.DB;
using MyRevit.MyTests.PAA;
using MyRevit.Utilities;
using System;
using System.Linq;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    /// <summary>
    /// 梁,线,标注 位置处理 IUpdater
    /// </summary>
    public class PAAUpdater_Delete : IUpdater
    {
        static AddInId AddInId;
        static UpdaterId UpdaterId;

        public PAAUpdater_Delete(AddInId addinID)
        {
            AddInId = addinID;
            UpdaterId = new UpdaterId(AddInId, new Guid("74E88A96-F15C-4A20-8E61-93A71CF50233"));
        }

        #region MyTestContext.GetCollection方案
        public void Execute(UpdaterData updateData)
        {
            try
            {
                var doc = updateData.GetDocument();
                var deletes = updateData.GetDeletedElementIds();
                var collection = PAAContext.GetCollection(doc);
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
                        var creater = PAAContext.Creator;
                        creater.Clear(itemToDelete);
                        isDeleted = true;
                    }
                }
                if (isDeleted)
                    PAAContext.Save(doc);
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
            return "PAAUpdater_Delete";
        }
    }
}
