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

        public static bool IsDeleting { get; set; }
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

        #region Family

        private static FamilySymbol _OneLine_Annotation = null;
        public static FamilySymbol GetOneLine_Annotation(Document doc)
        {
            if (_OneLine_Annotation == null || !_OneLine_Annotation.IsValidObject)
                _OneLine_Annotation = FamilySymbolHelper.LoadFamilySymbol(doc,
                    @"E:\WorkingSpace\Tasks\0.族\06.开洞套管引注\管道附件标记（单行）.rfa",
                    "管道附件标记（单行）", "管道附件标记（单行）");
            if (!_OneLine_Annotation.IsActive)
                _OneLine_Annotation.Activate();
            return _OneLine_Annotation;
        }

        private static FamilySymbol _TwoLine_Annotation = null;
        public static FamilySymbol GetTwoLine_Annotation(Document doc)
        {
            if (_TwoLine_Annotation == null || !_TwoLine_Annotation.IsValidObject)
                _TwoLine_Annotation = FamilySymbolHelper.LoadFamilySymbol(doc,
                    @"E:\WorkingSpace\Tasks\0.族\06.开洞套管引注\管道附件标记（两行）.rfa",
                    "管道附件标记（两行）", "管道附件标记（两行）");
            if (!_TwoLine_Annotation.IsActive)
                _TwoLine_Annotation.Activate();
            return _TwoLine_Annotation;
        }

        #endregion

        //字体
        public static FontManagement FontManagement = new FontManagement();

        //共享参数
        public static string SharedParameterGroupName = "出图深化";
        public static string SharedParameterPL = "开洞或套管高度";
        public static string SharedParameterOffset = "偏移量";
    }
}
