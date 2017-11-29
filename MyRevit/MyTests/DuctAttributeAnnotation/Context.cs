using Autodesk.Revit.DB;
using MyRevit.Utilities;
using PmSoft.Optimization.DrawingProduction;
using PmSoft.Optimization.DrawingProduction.Utils;

namespace MyRevit.MyTests.DAA
{
    class DAAContext
    {
        #region Creator
        private static DAACreator _Creator = null;
        public static DAACreator Creator
        {
            get
            {
                return _Creator ?? (_Creator = new DAACreator());
            }
        }
        #endregion

        #region Storage
        public static bool IsEditing;
        private static DAAModelCollection Collection;
        private static DAAStorageEntity CStorageEntity = new DAAStorageEntity();
        /// <summary>
        /// 取数据Collection
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static DAAModelCollection GetCollection(Document doc)
        {
            Collection = DelegateHelper.DelegateTryCatch(
                () =>
                {
                    string data = ExtensibleStorageHelper.GetData(doc, CStorageEntity, CStorageEntity.FieldOfData);
                    return new DAAModelCollection(data);
                },
                () =>
                {
                    return new DAAModelCollection("");
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
