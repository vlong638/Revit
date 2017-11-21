using Autodesk.Revit.DB;
using System.Drawing;

namespace MyRevit.Utilities
{
    public class FontManagement
    {
        #region Orient
        /// <summary>
        /// 原始文本字体
        /// </summary>
        public Font OrientFont = new Font("Angsana New", 12);//12码 4.23毫米
        /// <summary>
        /// 原始文本大小比例 以毫米表示
        /// </summary>
        public double OrientFontSizeScale { set; get; } = 4.23;
        /// <summary>
        /// 原始文本 Revit中的宽度缩放比例
        /// 原始的高度比例为4(?.23) 高为320
        /// </summary>
        public double OrientFontHeight = UnitHelper.ConvertToFoot(320, VLUnitType.millimeter);
        #endregion

        #region Current
        /// <summary>
        /// 是否需要设置当前字体
        /// </summary>
        public bool IsCurrentFontSettled { get { return CurrentFontSizeScale != -1; } }
        /// <summary>
        /// 桥梁 对接基本的字体和现用的字体
        /// </summary>
        /// <param name="textType"></param>
        /// <returns></returns>
        public void SetCurrentFont(TextNoteType textType)
        {
            CurrentFontSizeScale = UnitHelper.ConvertFromFootTo(textType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble(), VLUnitType.millimeter);//文本大小
            CurrentFontHeight = OrientFontHeight / 4 * CurrentFontSizeScale;//额外的留白 + HeightSpan;//宽度的比例基准似乎是以4mm来的
            CurrentFontWidthScale = textType.get_Parameter(BuiltInParameter.TEXT_WIDTH_SCALE).AsDouble();//文本宽度系数
        }
        /// <summary>
        /// 桥梁 对接基本的字体和现用的字体
        /// </summary>
        /// <param name="textType"></param>
        /// <returns></returns>
        public void SetCurrentFont(TextElementType textType)
        {
            CurrentFontSizeScale = UnitHelper.ConvertFromFootTo(textType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble(), VLUnitType.millimeter);//文本大小
            CurrentFontHeight = OrientFontHeight / 4 * CurrentFontSizeScale;//额外的留白 + HeightSpan;//宽度的比例基准似乎是以4mm来的
            CurrentFontWidthScale = textType.get_Parameter(BuiltInParameter.TEXT_WIDTH_SCALE).AsDouble();//文本宽度系数
        }
        /// <summary>
        /// 当前文本大小比例 以毫米表示
        /// </summary>
        public double CurrentFontSizeScale = -1;
        /// <summary>
        /// 当前文本高度 double, foot
        /// </summary>
        public double CurrentFontHeight { set; get; }
        /// <summary>
        /// 当前文本 Revit中的宽度缩放比例
        /// </summary>
        public double CurrentFontWidthScale { set; get; }
        #endregion
    }

}
