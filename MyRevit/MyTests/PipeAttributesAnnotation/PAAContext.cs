using Autodesk.Revit.DB;
using MyRevit.Utilities;

namespace MyRevit.MyTests.PAA
{
    public class PAAContext
    {
        #region Creator
        private static PAACreator _Creator = null;
        public static PAACreator Creator
        {
            get
            {
                return _Creator ?? (_Creator = new PAACreator());
            }
        }
        #endregion

        #region Storage
        public static bool IsEditing;
        private static PAAModelCollection Collection;
        private static PAAStorageEntity CStorageEntity = new PAAStorageEntity();
        /// <summary>
        /// 取数据Collection
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static PAAModelCollection GetCollection(Document doc)
        {
            Collection = DelegateHelper.DelegateTryCatch(
                () =>
                {
                    string data = ExtensibleStorageHelper.GetData(doc, CStorageEntity, CStorageEntity.FieldOfData);
                    return new PAAModelCollection(data);
                },
                () =>
                {
                    return new PAAModelCollection("");
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
        public static FamilySymbol GetSPLTag(Document doc)
        {
            if (_SPLTag == null)
                _SPLTag = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\03.管道特性标注\管道尺寸标记(系统+直径+离地).rfa", "管道尺寸标记(系统+直径+离地)", "管道尺寸标记");
            if (!_SPLTag.IsActive)
                _SPLTag.Activate();
            return _SPLTag;
        }

        private static FamilySymbol _PLTag = null;
        public static FamilySymbol GetPLTag(Document doc)
        {
            if (_PLTag == null)
                _PLTag = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\03.管道特性标注\管道尺寸标记(直径+离地).rfa", "管道尺寸标记(直径+离地)", "管道尺寸标记");
            if (!_PLTag.IsActive)
                _PLTag.Activate();
            return _PLTag;
        }

        private static FamilySymbol _SLTag = null;
        public static FamilySymbol GetSLTag(Document doc)
        {
            if (_SLTag == null || !_SLTag.IsActive)
                _SLTag = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\03.管道特性标注\管道尺寸标记(系统+离地).rfa", "管道尺寸标记(系统+离地)", "管道尺寸标记");
            if (!_SLTag.IsActive)
                _SLTag.Activate();
            return _SLTag;
        }

        private static FamilySymbol _MultipleLineOnEdge = null;
        public static FamilySymbol Get_MultipleLineOnEdge(Document doc)
        {
            if (_MultipleLineOnEdge == null)
                _MultipleLineOnEdge = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\03.管道特性标注\管道尺寸标记线族.rfa", "管道尺寸标记线族", "引线标注_文字在右端");
            if (!_MultipleLineOnEdge.IsActive)
                _MultipleLineOnEdge.Activate();
            return _MultipleLineOnEdge;
        }

        private static FamilySymbol _MultipleLineOnLine = null;
        public static FamilySymbol Get_MultipleLineOnLine(Document doc)
        {
            if (_MultipleLineOnLine == null)
                _MultipleLineOnLine = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\03.管道特性标注\管道尺寸标记线族.rfa", "管道尺寸标记线族", "引线标注_文字在线上");//引线标注_文字在线上 取出来总是未激活IsActive
            if (!_MultipleLineOnLine.IsActive)
                _MultipleLineOnLine.Activate();
            return _MultipleLineOnLine;
        }
        #endregion

        //字体
        public static FontManagement FontManagement = new FontManagement();

        //共享参数
        public static string SharedParameterGroupName = "管道特性标注";
        public static string SharedParameterPL = "管道离地高度";
        public static string SharedParameterOffset = "偏移量";
        public static string SharedParameterSystem = "系统缩写";
        public static string SharedParameterDiameter= "直径";
    }
}
