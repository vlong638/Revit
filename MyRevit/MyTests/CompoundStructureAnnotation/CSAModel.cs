using Autodesk.Revit.DB;
using MyRevit.MyTests.CompoundStructureAnnotation;
using MyRevit.MyTests.PipeAnnotationTest;
using MyRevit.MyTests.VLBase;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    /// <summary>
    /// CompoundStructureAnnotation数据的载体 线族方案
    /// </summary>
    public class CSAModel : VLModel//Base<CSAModel>
    {
        #region Construction
        public CSAModel() : base("")
        {
            Init();
        }
        public CSAModel(string data) : base(data)
        {
            Init();
        }
        private void Init()
        {
            Texts = new List<string>();
            TextLocations = new List<XYZ>();
            Texts = new List<string>();
        }
        #endregion

        #region 需要留存的数据
        /// <summary>
        /// 标注元素 对象,可以是墙,屋顶,伸展屋顶等等
        /// </summary>
        public ElementId TargetId { set; get; }
        /// <summary>
        /// 线族
        /// </summary>
        public ElementId LineId { get; set; }
        /// <summary>
        /// 文字样式
        /// </summary>
        public ElementId TextNoteTypeElementId
        {
            get
            {
                return textNoteTypeElementId;
            }
            set
            {
                textNoteTypeElementId = value;
                if (value == null)
                    return;
                var element = VLConstraintsForCSA.Doc.GetElement(value);
                if (element == null || element as TextNoteType == null)
                    return;
                var textNoteType = element as TextNoteType;
                CurrentFontSizeScale = UnitHelper.ConvertFromFootTo(textNoteType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble(), VLUnitType.millimeter);//文本大小
                CurrentFontWidthScale = textNoteType.get_Parameter(BuiltInParameter.TEXT_WIDTH_SCALE).AsDouble();//文本宽度系数
            }
        }
        private ElementId textNoteTypeElementId = null;
        /// <summary>
        /// 文字 对象
        /// </summary>
        public List<ElementId> TextNoteIds { set; get; }
        /// <summary>
        /// 文字 定位方案
        /// </summary>
        public CSALocationType CSALocationType { set; get; }
        /// <summary>
        /// 线起点 在附着体上
        /// </summary>
        public XYZ LineLocation { set; get; }
        /// <summary>
        /// 需要显示的结构标注信息
        /// </summary>
        public List<string> Texts { set; get; }

        public override bool LoadData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return false;
            try
            {
                StringReader sr = new StringReader(data);
                TargetId = sr.ReadFormatStringAsElementId();
                LineId = sr.ReadFormatStringAsElementId();
                TextNoteIds = sr.ReadFormatStringAsElementIds();
                TextNoteTypeElementId = sr.ReadFormatStringAsElementId();
                CSALocationType = sr.ReadFormatStringAsEnum<CSALocationType>();
                LineLocation = sr.ReadFormatStringAsXYZ();
                Texts = sr.ReadFormatStringAsStrings();
                return true;
            }
            catch (Exception ex)
            {
                //TODO log
                return false;
            }
        }

        public override string ToData()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendItem(TargetId);
            sb.AppendItem(LineId);
            sb.AppendItem(TextNoteIds);
            sb.AppendItem(TextNoteTypeElementId);
            sb.AppendItem(CSALocationType);
            sb.AppendItem(LineLocation);
            sb.AppendItem(Texts);
            return sb.ToData();
        }
        #endregion

        #region 无需留存的数据
        /// <summary>
        /// 当前文本大小比例 以毫米表示
        /// </summary>
        public double CurrentFontSizeScale
        {
            get
            {
                return currentFontSizeScale;
            }
            set
            {
                currentFontSizeScale = value;
                CurrentFontHeight = VLConstraintsForCSA.OrientFontHeight / 4 * currentFontSizeScale;//额外的留白 + HeightSpan;//宽度的比例基准似乎是以4mm来的
            }
        }
        /// <summary>
        /// 字体的毫米数
        /// </summary>
        public double FontScale { get; set; }
        /// <summary>
        /// 字体高度高度 由基础基础字体高度/4*FontScale得出
        /// </summary>
        public double LineHeight { set; get; }
        //选中的文本长宽关键信息
        public double currentFontSizeScale;
        /// <summary>
        /// 当前文本高度 double = foot
        /// </summary>
        public double CurrentFontHeight { set; get; }
        /// <summary>
        /// 当前文本 Revit中的宽度缩放比例
        /// </summary>
        public double CurrentFontWidthScale { set; get; }
        /// <summary>
        /// 主体目标的类型
        /// </summary>
        public TargetType TargetType { set; get; }
        /// <summary>
        /// 文本 位置
        /// </summary>
        public List<XYZ> TextLocations { set; get; }
        /// <summary>
        /// 线宽
        /// </summary>
        public double LineWidth { get; set; }
        /// <summary>
        /// 间距
        /// </summary>
        public double LineSpace { get; private set; }

        /// <summary>
        /// 坐标定位,平行于标注对象
        /// </summary>
        public XYZ ParallelVector = null;
        /// <summary>
        /// 坐标定位,垂直于标注对象
        /// </summary>
        public XYZ VerticalVector = null;
        #endregion

        #region 方法
        public void UpdateVectors(Line locationCurve)
        {
            XYZ parallelVector = null;
            XYZ verticalVector = null;
            parallelVector = locationCurve.Direction;
            verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
            parallelVector = VLLocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
            verticalVector = VLLocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
            if ((verticalVector.X - 1).IsMiniValue())
                verticalVector = verticalVector.RevertByCoordinateType(CoordinateType.XY);
            VerticalVector = verticalVector;
            ParallelVector = parallelVector;
        }

        #region 详图线的计算方案
        //public void CalculateLocations(Element element, XYZ offset)
        //{
        //    var scale = 1 / VLConstraints.OrientFontSizeScale * VLConstraints.CurrentFontSizeScale;
        //    var width = CSALocationType.GetLineWidth() * scale;
        //    var height = 400 * scale;
        //    var widthFoot = UnitHelper.ConvertToFoot(width, VLUnitType.millimeter);
        //    var heightFoot = UnitHelper.ConvertToFoot(height, VLUnitType.millimeter);
        //    var verticalFix = UnitHelper.ConvertToFoot(100, VLUnitType.millimeter) * scale;
        //    var locationCurve = TargetType.GetLine(element);
        //    UpdateVectors(locationCurve);
        //    //线起始点 (中点)
        //    LineStartLocation = (locationCurve.GetEndPoint(0) + locationCurve.GetEndPoint(1)) / 2;
        //    //文本位置 start:(附着元素中点+线基本高度+文本高度*(文本个数-1))  end: start+宽
        //    //高度,宽度 取决于文本 
        //    ParallelLinesLocations = new List<TextLocation>();
        //    TextLocations = new List<XYZ>();
        //    for (int i = Texts.Count() - 1; i >= 0; i--)
        //    {
        //        var start = LineStartLocation + (heightFoot + i * VLConstraints.CurrentFontHeight) * VerticalVector;
        //        var end = start + widthFoot * ParallelVector;
        //        ParallelLinesLocations.Add(new TextLocation(start, end));
        //        TextLocations.Add(CSALocationType.GetTextLocation(verticalFix, VerticalVector, start, end));
        //    }
        //    //线终点 (最高的文本位置)
        //    LineEndLocation = ParallelLinesLocations[0].Start;
        //}

        //public void CalculateLocations(Element element)
        //{
        //    CalculateLocations(element, new XYZ(0, 0, 0));
        //} 
        #endregion


        #region 线族的计算方案
        public void CalculateLocations(Element element, FamilyInstance line, XYZ offset)
        {
            //数据准备
            var locationCurve = TargetType.GetLine(element);
            var miniHeight = CurrentFontHeight * 2;
            UpdateVectors((locationCurve as Line));
            //ParallelVector = VLLocationHelper.GetVectorByQuadrant((locationCurve as Line).Direction, QuadrantType.OneAndFour);
            //VerticalVector = VLLocationHelper.GetVectorByQuadrant(new XYZ(ParallelVector.Y, -ParallelVector.X, 0), QuadrantType.OneAndTwo);
            //计算线的定位位置
            bool isRegenerate = offset != null;
            if (!isRegenerate)
            {
                LineLocation = (locationCurve.GetEndPoint(0) + locationCurve.GetEndPoint(1)) / 2;
                offset = new XYZ(0, 0, 0);
                LineHeight = UnitHelper.ConvertToFoot(1000, VLUnitType.millimeter);
            }
            else
            {
                LineLocation = locationCurve.Project(LineLocation + offset).XYZPoint;
                LineHeight = line.GetParameters(TagProperty.线高度1.ToString()).First().AsDouble() + VLLocationHelper.GetLengthBySide(offset, VerticalVector);
                LineHeight = LineHeight > miniHeight ? LineHeight : miniHeight;//确保最短长度有一个文字高度
            }
            //高度,宽度 取决于文本 
            FontScale = 1 / VLConstraintsForCSA.OrientFontSizeScale * CurrentFontSizeScale;
            //LineWidth = UnitHelper.ConvertToFoot(CSALocationType.GetLineWidth() * FontScale, VLUnitType.millimeter);
            var verticalFix = CurrentFontHeight * VLConstraintsForCSA.TextSpace; ;//偏移修正 为了显示更好 方便更改
            var horizontalFix = UnitHelper.ConvertToFoot(50, VLUnitType.millimeter);//偏移修正 为了显示更好 方便更改
            LineSpace = CurrentFontHeight * (1 + VLConstraintsForCSA.TextSpace);
            TextLocations = new List<XYZ>();
            for (int i = Texts.Count() - 1; i >= 0; i--)
            {
                var start = LineLocation + (LineHeight + i * LineSpace) * VerticalVector;
                var end = start + LineWidth * ParallelVector;
                TextLocations.Add(CSALocationType.GetTextLocation(CurrentFontHeight, verticalFix, VerticalVector, horizontalFix, ParallelVector, start, end));
            }
        }
        #endregion

        #endregion
    }
    /// <summary>
    /// 为CSAModel数据服务的处理类
    /// </summary>
    public static class CSAModelEx
    {
        /// <summary>
        /// 获取文字的样式方案
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<TextNoteType> GetTextNoteTypes(this CSAModel model)
        {
            var textNoteTypes = new FilteredElementCollector(VLConstraintsForCSA.Doc).OfClass(typeof(TextNoteType)).ToElements().Select(p => p as TextNoteType).Where(c => !c.Name.Contains("明细表")).ToList();
            return textNoteTypes;
        }

        /// <summary>
        /// 线 参数设置
        /// </summary>
        public static void UpdateLineParameters(this CSAModel model, FamilyInstance line, double lineHeight, double lineWidth, double space, int textCount)
        {
            line.GetParameters(TagProperty.线高度1.ToString()).First().Set(lineHeight);
            line.GetParameters(TagProperty.线宽度.ToString()).First().Set(lineWidth);
            line.GetParameters(TagProperty.间距.ToString()).First().Set(space);
            line.GetParameters(TagProperty.文字行数.ToString()).First().Set(textCount);
        }

        /// <summary>
        /// 获取 CompoundStructure 对象
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static CompoundStructure GetCompoundStructure(this CSAModel model, Element element)
        {
            CompoundStructure compoundStructure = null;
            if (element is Wall)
            {
                compoundStructure = (element as Wall).WallType.GetCompoundStructure();
                model.TargetType = TargetType.Wall;
            }
            else if (element is Floor)
            {
                compoundStructure = (element as Floor).FloorType.GetCompoundStructure();
                model.TargetType = TargetType.Floor;
            }
            else if (element is RoofBase)
            {
                compoundStructure = (element as RoofBase).RoofType.GetCompoundStructure();
                model.TargetType = TargetType.RoofBase;
            }
            else
            {
                throw new NotImplementedException("暂不支持该类型的对象,未能成功获取CompoundStructure");
            }
            return compoundStructure;
        }

        /// <summary>
        /// 从 CompoundStructure 中获取标注信息
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="compoundStructure"></param>
        /// <returns></returns>
        public static List<string> FetchTextsFromCompoundStructure(this CSAModel model, Document doc, CompoundStructure compoundStructure)
        {
            var layers = compoundStructure.GetLayers();
            var texts = new List<string>();
            foreach (var layer in layers)
            {
                string name = "";
                var material = doc.GetElement(layer.MaterialId);
                if (material == null)
                    name = "<按类别>";
                else
                    name = doc.GetElement(layer.MaterialId).Name;
                texts.Add(UnitHelper.ConvertFromFootTo(layer.Width, VLUnitType.millimeter) + "厚" + name);
            }
            model.Texts = texts;
            return texts;
        }

        /// <summary>
        /// 从需要标注的对象中获取标注的源位置
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static XYZ GetElementLocation(this CSAModel model, Element element)
        {
            var locationCurve = element.Location as LocationCurve;
            var location = (locationCurve.Curve.GetEndPoint(0) + locationCurve.Curve.GetEndPoint(1)) / 2;
            return location;
        }

        private static void SetCurveFromLine(CSAModel model, LocationCurve locationCurve)
        {
            XYZ parallelVector = null;
            XYZ verticalVector = null;
            parallelVector = (locationCurve.Curve as Line).Direction;
            verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
            parallelVector = VLLocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
            verticalVector = VLLocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
            if ((verticalVector.X - 1).IsMiniValue())
                verticalVector = verticalVector.RevertByCoordinateType(CoordinateType.XY);
            model.VerticalVector = verticalVector;
            model.ParallelVector = parallelVector;
        }

        /// <summary>
        /// 按XY的点的Z轴旋转
        /// </summary>
        /// <param name="point"></param>
        /// <param name="xyz"></param>
        /// <param name="verticalVector"></param>
        public static void RotateByXY(this LocationPoint point, XYZ xyz, XYZ verticalVector)
        {
            Line axis = Line.CreateBound(xyz, xyz.Add(new XYZ(0, 0, 10)));
            if (verticalVector.X > VLConstraintsForCSA.MiniValueForXYZ)
                point.Rotate(axis, 2 * Math.PI - verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
            else
                point.Rotate(axis, verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
        }

        /// <summary>
        /// 按XY的点的Z轴旋转
        /// </summary>
        /// <param name="point"></param>
        /// <param name="xyz"></param>
        /// <param name="verticalVector"></param>
        public static void RotateByXY(this Location point, XYZ xyz, XYZ verticalVector)
        {
            Line axis = Line.CreateBound(xyz, xyz.Add(new XYZ(0, 0, 10)));
            if (verticalVector.X > VLConstraintsForCSA.MiniValueForXYZ)
                point.Rotate(axis, 2 * Math.PI - verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
            else
                point.Rotate(axis, verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
        }
    }
}
