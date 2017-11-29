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

        #region Family
        //管道尺寸标记线族
        //-引线标注_文字在右端
        //-引线标注_文字在线上

        //管道尺寸标记(系统+直径+离地) SPL
        //管道尺寸标记(直径+离地) PL
        //管道尺寸标记(系统+离地) SL
        //-管道尺寸标记

        private static FamilySymbol _SPLTag = null;
        public static FamilySymbol GetSPLTagForRectangle(Document doc)
        {
            if (_SPLTag == null || !_SPLTag.IsValidObject)
                _SPLTag = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\04.风管特性标注\风管尺寸标记（系统+尺寸+离地）.rfa", "风管尺寸标记（系统+尺寸+离地）", "管道尺寸标记");
            if (!_SPLTag.IsActive)
                _SPLTag.Activate();
            return _SPLTag;
        }

        private static FamilySymbol _PLTag = null;
        public static FamilySymbol GetPLTagForRectangle(Document doc)
        {
            if (_PLTag == null || !_PLTag.IsValidObject)
                _PLTag = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\04.风管特性标注\管道尺寸标记(直径+离地).rfa", "管道尺寸标记(直径+离地)", "管道尺寸标记");
            if (!_PLTag.IsActive)
                _PLTag.Activate();
            return _PLTag;
        }

        private static FamilySymbol _SLTag = null;
        public static FamilySymbol GetSLTagForRectangle(Document doc)
        {
            if (_SLTag == null || !_SLTag.IsValidObject)
                _SLTag = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\04.风管特性标注\管道尺寸标记(系统+离地).rfa", "管道尺寸标记(系统+离地)", "管道尺寸标记");
            if (!_SLTag.IsActive)
                _SLTag.Activate();
            return _SLTag;
        }

        private static FamilySymbol _SPTag = null;
        public static FamilySymbol GetSPTagForRectangle(Document doc)
        {
            if (_SPTag == null || !_SPTag.IsValidObject)
                _SPTag = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\04.风管特性标注\管道尺寸标记(系统+离地).rfa", "管道尺寸标记(系统+离地)", "管道尺寸标记");
            if (!_SPTag.IsActive)
                _SPTag.Activate();
            return _SPTag;
        }
        


        private static FamilySymbol _MultipleLineOnEdge = null;
        public static FamilySymbol Get_MultipleLineOnEdge(Document doc)
        {
            if (_MultipleLineOnEdge == null || !_MultipleLineOnEdge.IsValidObject)
                _MultipleLineOnEdge = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\03.管道特性标注\管道尺寸标记线族.rfa", "管道尺寸标记线族", "引线标注_文字在右端");
            if (!_MultipleLineOnEdge.IsActive)
                _MultipleLineOnEdge.Activate();
            return _MultipleLineOnEdge;
        }

        private static FamilySymbol _MultipleLineOnLine = null;
        public static FamilySymbol Get_MultipleLineOnLine(Document doc)
        {
            if (_MultipleLineOnLine == null || !_MultipleLineOnLine.IsValidObject)
                _MultipleLineOnLine = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\03.管道特性标注\管道尺寸标记线族.rfa", "管道尺寸标记线族", "引线标注_文字在线上");//引线标注_文字在线上 取出来总是未激活IsActive
            if (!_MultipleLineOnLine.IsActive)
                _MultipleLineOnLine.Activate();
            return _MultipleLineOnLine;
        }
        #endregion
    }
}
