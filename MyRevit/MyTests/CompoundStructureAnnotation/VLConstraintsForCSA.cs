using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.MyTests.PipeAnnotationTest;
using System.Drawing;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{

    public partial class VLConstraintsForCSA
    {
        public static Document Doc { set; get; }

        public static VLUnitType CurrentUnitType = VLUnitType.millimeter;
        public static double MiniValueForXYZ = 0.001;

        #region 文字
        /// <summary>
        /// 原始文本字体
        /// </summary>
        public static Font OrientFont = new Font("Angsana New", 12);//12码 4.23毫米
        /// <summary>
        /// 原始文本大小比例 以毫米表示
        /// </summary>
        public static double OrientFontSizeScale { set; get; } = 4.23;
        /// <summary>
        /// 原始文本 Revit中的宽度缩放比例
        /// 原始的高度比例为4(?.23) 高为320
        /// </summary>
        public static double OrientFontHeight = UnitHelper.ConvertToFoot(320, VLUnitType.millimeter);
        ///// <summary>
        ///// 文本间留白
        ///// </summary>
        //public static double HeightSpan = UnitHelper.ConvertToFoot(20, VLUnitType.millimeter);

        /// <summary>
        /// 线间额外距离,0.3*文字高度
        /// </summary>
        public static double TextSpace = 0.5;

        #endregion

        #region 线族
        private static FamilySymbol MultipleTagSymbol { set; get; }
        public static FamilySymbol GetMultipleTagSymbol(Document doc)
        {
            if (MultipleTagSymbol == null || !MultipleTagSymbol.IsValidObject)
                LoadFamilySymbols(doc);
            return MultipleTagSymbol;
        }

        /// <summary>
        /// 获取标注族
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static bool LoadFamilySymbols(Document doc)
        {
            MultipleTagSymbol = FamilySymbolHelper.LoadFamilySymbol(doc, "结构做法标注", "引线标注_文字在右端");
            return true;
        }

        ///// <summary>
        ///// 加载族 加载失败或者本文档未找到,返回null 
        ///// </summary>
        //public static FamilySymbol LoadFamilySymbol(Document doc, string familyFilePath, string familyName, string symbolName)
        //{
        //    //return FamilyLoadUtils.FindFamilySymbol_SubTransaction(doc, familyName, name);

        //    FamilySymbol symbol = null;
        //    var symbolFile = familyFilePath;
        //    Family family;
        //    if (doc.LoadFamily(symbolFile, out family))
        //    {
        //        //获取族类型集合Id
        //        var familySymbolIds = family.GetFamilySymbolIds();
        //        foreach (var familySymbolId in familySymbolIds)
        //        {
        //            var element = doc.GetElement(familySymbolId) as FamilySymbol;
        //            if (element != null && element.FamilyName == familyName)
        //            {
        //                symbol = element;
        //                break;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var symbols = new FilteredElementCollector(doc)
        //            .WherePasses(new ElementClassFilter(typeof(FamilySymbol)));
        //        var targetSymbol = symbols.FirstOrDefault(c => (c as FamilySymbol).FamilyName == familyName && (c as FamilySymbol).Name == symbolName);
        //        if (targetSymbol != null)
        //            symbol = targetSymbol as FamilySymbol;
        //    }
        //    return symbol;
        //}
        #endregion
    }
}
