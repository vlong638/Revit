using Autodesk.Revit.DB;
using MyRevit.Utilities;
using PmSoft.Optimization.DrawingProduction;
using PmSoft.Optimization.DrawingProduction.Utils;

namespace MyRevit.MyTests.PBPA
{
    class PBPAContext
    {
        #region Creator
        private static PBPACreator _Creator = null;
        public static PBPACreator Creator
        {
            get
            {
                return _Creator ?? (_Creator = new PBPACreator());
            }
        }
        #endregion

        #region Storage
        public static bool IsEditing;
        private static PBPAModelCollection Collection;
        private static PBPAStorageEntity CStorageEntity = new PBPAStorageEntity();
        /// <summary>
        /// 取数据Collection
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static PBPAModelCollection GetCollection(Document doc)
        {
            Collection = DelegateHelper.DelegateTryCatch(
                () =>
                {
                    string data = ExtensibleStorageHelper.GetData(doc, CStorageEntity, CStorageEntity.FieldOfData);
                    return new PBPAModelCollection(data);
                },
                () =>
                {
                    return new PBPAModelCollection("");
                }
            );
            return Collection;
        }

        /// <summary>
        /// 保存Collection
        /// </summary>
        /// <param name="doc"></param>
        public static bool Save(Document doc)
        {
            if (Collection == null)
                return false;
            var data = Collection.ToData();
            return DelegateHelper.DelegateTryCatch(
                () =>
                {
                    ExtensibleStorageHelper.SetData(doc, CStorageEntity, CStorageEntity.FieldOfData, data);
                    return true;
                },
                () =>
                {
                    ExtensibleStorageHelper.RemoveStorage(doc, CStorageEntity);
                    ExtensibleStorageHelper.SetData(doc, CStorageEntity, CStorageEntity.FieldOfData, data);
                    return false;
                }
            );
        }
        #endregion

    }
}
