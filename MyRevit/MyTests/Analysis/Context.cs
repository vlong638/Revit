using Autodesk.Revit.DB;
using MyRevit.Utilities;
using PmSoft.Optimization.DrawingProduction;
using PmSoft.Optimization.DrawingProduction.Utils;

namespace MyRevit.MyTests.Analysis
{
    class AnalysisContext
    {

        #region Storage
        public static bool IsEditing;
        private static AnalysisModel Collection;
        private static AnalysisStorageEntity CStorageEntity = new AnalysisStorageEntity();
        /// <summary>
        /// 取数据Collection
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static AnalysisModel GetCollection(Document doc)
        {
            Collection = DelegateHelper.DelegateTryCatch(
                () =>
                {
                    string data = ExtensibleStorageHelper.GetData(doc, CStorageEntity, CStorageEntity.FieldOfData);
                    return new AnalysisModel(data);
                },
                () =>
                {
                    return new AnalysisModel("");
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
