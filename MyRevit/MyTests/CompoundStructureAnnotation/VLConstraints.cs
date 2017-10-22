using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.MyTests.PipeAnnotationTest;
using System.Drawing;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    class VLConstraints
    {
        public static Document Doc { private set; get; }
        public static UIDocument UIDoc { private set; get; }

        public static void InitByDocument(UIDocument uidoc)
        {
            UIDoc = uidoc;
            Doc = UIDoc.Document;
        }

        public static VLUnitType CurrentUnitType = VLUnitType.millimeter;
        public static double MiniValueForXYZ = 0.001;

        #region 文字
        /// <summary>
        /// 原始文本字体
        /// </summary>
        public static Font OrientFont = new Font("Angsana New", 21);//21码 7.35毫米
        /// <summary>
        /// 原始文本大小比例 以毫米表示
        /// </summary>
        public static double OrientFontSizeScale { set; get; } = 7.35;
        /// <summary>
        /// 原始文本 Revit中的宽度缩放比例
        /// </summary>
        public static double OrientFontHeight = UnitHelper.ConvertFromFootTo(150, VLUnitType.millimeter);

        //选中的文本长宽关键信息
        private static double currentFontSizeScale;
        /// <summary>
        /// 当前文本大小比例 以毫米表示
        /// </summary>
        public static double CurrentFontSizeScale
        {
            get
            {
                return currentFontSizeScale;
            }

            set
            {
                currentFontSizeScale = value;
                CurrentFontHeight = OrientFontHeight / OrientFontSizeScale * currentFontSizeScale;
            }
        }
        /// <summary>
        /// 当前文本高度 double = foot
        /// </summary>
        public static double CurrentFontHeight { set; get; }
        /// <summary>
        /// 当前文本 Revit中的宽度缩放比例
        /// </summary>
        public static double CurrentFontWidthScale { set; get; } 
        #endregion
    }
}
