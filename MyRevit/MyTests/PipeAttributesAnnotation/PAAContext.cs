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
        public static bool IsDeleting;
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

        #region SPL
        private static FamilySymbol _SPLTag_Pipe = null;
        public static FamilySymbol GetSPLTag_Pipe(Document doc)
        {
            if (_SPLTag_Pipe == null || !_SPLTag_Pipe.IsValidObject)
                _SPLTag_Pipe = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\03.管道特性标注\管道尺寸标记(系统+直径+离地).rfa", "管道尺寸标记(系统+直径+离地)", "管道尺寸标记");
            if (!_SPLTag_Pipe.IsActive)
                _SPLTag_Pipe.Activate();
            return _SPLTag_Pipe;
        }

        private static FamilySymbol _SPLTag_Duct_Rectangle = null;
        public static FamilySymbol GetSPLTag_Duct_Rectangle(Document doc)
        {
            if (_SPLTag_Duct_Rectangle == null || !_SPLTag_Duct_Rectangle.IsValidObject)
                _SPLTag_Duct_Rectangle = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\04.风管特性标注\风管尺寸标记（系统+尺寸+离地）.rfa", "风管尺寸标记（系统+尺寸+离地）", "风管尺寸标记");
            if (!_SPLTag_Duct_Rectangle.IsActive)
                _SPLTag_Duct_Rectangle.Activate();
            return _SPLTag_Duct_Rectangle;
        }

        private static FamilySymbol _SPLTag_Duct_Round = null;
        public static FamilySymbol GetSPLTag_Duct_Round(Document doc)
        {
            if (_SPLTag_Duct_Round == null || !_SPLTag_Duct_Round.IsValidObject)
                _SPLTag_Duct_Round = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\04.风管特性标注\风管尺寸标记（系统+直径+离地）.rfa", "风管尺寸标记（系统+直径+离地）", "风管尺寸标记");
            if (!_SPLTag_Duct_Round.IsActive)
                _SPLTag_Duct_Round.Activate();
            return _SPLTag_Duct_Round;
        }

        private static FamilySymbol _SPLTag_CableTray = null;
        public static FamilySymbol GetSPLTag_CableTray(Document doc)
        {
            if (_SPLTag_CableTray == null || !_SPLTag_CableTray.IsValidObject)
                _SPLTag_CableTray = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\05.桥架特性标注\电缆桥架尺寸标记（类型+尺寸+离地）.rfa", "电缆桥架尺寸标记（类型+尺寸+离地）", "电缆桥架尺寸标记");
            if (!_SPLTag_CableTray.IsActive)
                _SPLTag_CableTray.Activate();
            return _SPLTag_CableTray;
        }
        #endregion

        #region PL
        private static FamilySymbol _PLTag_Pipe = null;
        public static FamilySymbol GetPLTag_Pipe(Document doc)
        {
            if (_PLTag_Pipe == null || !_PLTag_Pipe.IsValidObject)
                _PLTag_Pipe = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\03.管道特性标注\管道尺寸标记(直径+离地).rfa", "管道尺寸标记(直径+离地)", "管道尺寸标记");
            if (!_PLTag_Pipe.IsActive)
                _PLTag_Pipe.Activate();
            return _PLTag_Pipe;
        }

        private static FamilySymbol _PLTag_Duct_Rectangle = null;
        public static FamilySymbol GetPLTag_Duct_Rectangle(Document doc)
        {
            if (_PLTag_Duct_Rectangle == null || !_PLTag_Duct_Rectangle.IsValidObject)
                _PLTag_Duct_Rectangle = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\04.风管特性标注\风管尺寸标记（尺寸+离地）.rfa", "风管尺寸标记（尺寸+离地）", "风管尺寸标记");
            if (!_PLTag_Duct_Rectangle.IsActive)
                _PLTag_Duct_Rectangle.Activate();
            return _PLTag_Duct_Rectangle;
        }

        private static FamilySymbol _PLTag_Duct_Round = null;
        public static FamilySymbol GetPLTag_Duct_Round(Document doc)
        {
            if (_PLTag_Duct_Round == null || !_PLTag_Duct_Round.IsValidObject)
                _PLTag_Duct_Round = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\04.风管特性标注\风管尺寸标记（直径+离地）.rfa", "风管尺寸标记（直径+离地）", "风管尺寸标记");
            if (!_PLTag_Duct_Round.IsActive)
                _PLTag_Duct_Round.Activate();
            return _PLTag_Duct_Round;
        }

        private static FamilySymbol _PLTag_CableTray = null;
        public static FamilySymbol GetPLTag_CableTray(Document doc)
        {
            if (_PLTag_CableTray == null || !_PLTag_CableTray.IsValidObject)
                _PLTag_CableTray = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\05.桥架特性标注\电缆桥架尺寸标记（尺寸+离地）.rfa", "电缆桥架尺寸标记（尺寸+离地）", "电缆桥架尺寸标记");
            if (!_PLTag_CableTray.IsActive)
                _PLTag_CableTray.Activate();
            return _PLTag_CableTray;
        }
        #endregion

        #region SL
        private static FamilySymbol _SLTag_Pipe = null;
        public static FamilySymbol GetSLTag_Pipe(Document doc)
        {
            if (_SLTag_Pipe == null || !_SLTag_Pipe.IsValidObject)
                _SLTag_Pipe = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\03.管道特性标注\管道尺寸标记(系统+离地).rfa", "管道尺寸标记(系统+离地)", "管道尺寸标记");
            if (!_SLTag_Pipe.IsActive)
                _SLTag_Pipe.Activate();
            return _SLTag_Pipe;
        }

        private static FamilySymbol _SLTag_Duct = null;
        public static FamilySymbol GetSLTag_Duct(Document doc)
        {
            if (_SLTag_Duct == null || !_SLTag_Duct.IsValidObject)
                _SLTag_Duct = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\04.风管特性标注\风管尺寸标记（系统+离地）.rfa", "风管尺寸标记（系统+离地）", "风管尺寸标记");
            if (!_SLTag_Duct.IsActive)
                _SLTag_Duct.Activate();
            return _SLTag_Duct;
        }

        private static FamilySymbol _SLTag_CableTray = null;
        public static FamilySymbol GetSLTag_CableTray(Document doc)
        {
            if (_SLTag_CableTray == null || !_SLTag_CableTray.IsValidObject)
                _SLTag_CableTray = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\05.桥架特性标注\电缆桥架尺寸标记（类型+离地）.rfa", "电缆桥架尺寸标记（类型+离地）", "电缆桥架尺寸标记");
            if (!_SLTag_CableTray.IsActive)
                _SLTag_CableTray.Activate();
            return _SLTag_CableTray;
        }
        #endregion

        #region Line
        private static FamilySymbol _MultipleLineOnEdge = null;
        public static FamilySymbol Get_MultipleLineOnEdge(Document doc)
        {
            if (_MultipleLineOnEdge == null || !_MultipleLineOnEdge.IsValidObject)
                _MultipleLineOnEdge = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\03.管道特性标注\尺寸标记线族.rfa", "尺寸标记线族", "引线标注_文字在右端");
            if (!_MultipleLineOnEdge.IsActive)
                _MultipleLineOnEdge.Activate();
            return _MultipleLineOnEdge;
        }

        private static FamilySymbol _MultipleLineOnLine = null;
        public static FamilySymbol Get_MultipleLineOnLine(Document doc)
        {
            if (_MultipleLineOnLine == null || !_MultipleLineOnLine.IsValidObject)
                _MultipleLineOnLine = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0.族\03.管道特性标注\尺寸标记线族.rfa", "尺寸标记线族", "引线标注_文字在线上");
            if (!_MultipleLineOnLine.IsActive)
                _MultipleLineOnLine.Activate();
            return _MultipleLineOnLine;
        } 
        #endregion

        #endregion

        //字体
        public static FontManagement FontManagement = new FontManagement();

        //共享参数
        public static string SharedParameterGroupName = "管道特性标注";
        public static string SharedParameterPL = "管道离地高度";
        public static string SharedParameterOffset = "偏移量";
        public static string SharedParameterSystemAbbreviation = "系统缩写";
        public static string SharedParameterTypeName = "类型名称";
        public static string SharedParameterDiameter= "直径";
        public static string SharedParameterHeight= "高度";
    }
}
