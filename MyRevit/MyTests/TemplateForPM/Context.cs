using Autodesk.Revit.DB;
using PmSoft.Common.RevitClass.VLUtils;

namespace PMSoft.ConstructionManagementV2
{
    class CMContext
    {
        #region Creator
        private static CMCreator _Creator = null;
        public static CMCreator Creator
        {
            get
            {
                return _Creator ?? (_Creator = new CMCreator());
            }
        }
        #endregion

        #region Storage
        public static bool IsEditing;
        private static CMModelCollection Collection;
        private static CMStorageEntity CStorageEntity = new CMStorageEntity();
        /// <summary>
        /// 取数据Collection
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static CMModelCollection GetCollection(Document doc)
        {
            Collection = VLDelegateHelper.DelegateTryCatch(
                () =>
                {
                    string data = ExtensibleStorageHelper.GetData(doc, CStorageEntity, CStorageEntity.FieldOfData);
                    return new CMModelCollection(data);
                },
                () =>
                {
                    return new CMModelCollection("");
                }
            );
            return Collection;
        }

        /// <summary>
        /// 保存Collection
        /// </summary>
        /// <param name="doc"></param>
        public static bool SaveCollection(Document doc)
        {
            if (Collection == null)
                return false;
            var data = Collection.ToData();
            return VLDelegateHelper.DelegateTryCatch(
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
