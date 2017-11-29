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
        public double OrientFontSizeScale = 4.23;
        public double OrientWidthSize = 25.4;
        public double OrientHeight = 609;//338.4//针对4.23毫米的字体的高度
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
        public void SetCurrentFont(LineAndTextAttrSymbol textType)
        {
            CurrentFontSizeScale = UnitHelper.ConvertFromFootTo(textType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble(), VLUnitType.millimeter);//文本大小
            CurrentFontWidthScale = textType.get_Parameter(BuiltInParameter.TEXT_WIDTH_SCALE).AsDouble();//文本宽度系数
            CurrentHeight = UnitHelper.ConvertToFoot(OrientHeight /OrientFontSizeScale * CurrentFontSizeScale, VLUnitType.millimeter);
            CurrentFontWidthSize = UnitHelper.ConvertToFoot(OrientWidthSize / OrientFontSizeScale * CurrentFontSizeScale * CurrentFontWidthScale, VLUnitType.millimeter);
        }

        /// <summary>
        /// 当前文本大小比例 以毫米表示
        /// </summary>
        public double CurrentFontSizeScale = -1;
        /// <summary>
        /// 当前文本 Revit中的宽度缩放比例
        /// </summary>
        public double CurrentFontWidthScale { set; get; }
        /// <summary>
        /// 当前文本高度比例系数 double, foot
        /// </summary>
        public double CurrentHeight { set; get; }
        /// <summary>
        /// 当前文本宽度比例系数 double, foot
        /// </summary>
        public double CurrentFontWidthSize { set; get; }
        #endregion
    }

}
