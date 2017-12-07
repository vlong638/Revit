using Autodesk.Revit.DB;
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
    /// CompoundStructureAnnotation数据的载体 详图线方案
    /// </summary>
    public class CSAModelForDetailLine : VLModel
    {
        public CSAModelForDetailLine(string data = "") : base(data)
        {
            Texts = new List<string>();
            TextLocations = new List<XYZ>();
            Texts = new List<string>();
        }

        #region 需要留存的数据
        /// <summary>
        /// 标注元素 对象,可以是墙,屋顶,伸展屋顶等等
        /// </summary>
        public ElementId TargetId { set; get; }
        /// <summary>
        /// 线 对象
        /// </summary>
        public List<ElementId> LineIds { get; set; }
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
        /// 文本 位置
        /// </summary>
        public List<XYZ> TextLocations { set; get; }

        public override bool LoadData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return false;
            try
            {
                StringReader sr = new StringReader(data);
                TargetId = sr.ReadFormatStringAsElementId();
                LineIds = sr.ReadFormatStringAsElementIds();
                TextNoteIds = sr.ReadFormatStringAsElementIds();
                TextNoteTypeElementId = sr.ReadFormatStringAsElementId();
                CSALocationType = sr.ReadFormatStringAsEnum<CSALocationType>();
                TextLocations = sr.ReadFormatStringAsXYZs();
                Texts = sr.ReadFormatStringAsStrings();
                return true;
            }
            catch (Exception ex)
            {
                //TODO log
                return false;
            }

            //var values = data.Split(PropertySplitter_Char);
            //if (values.Count() != 6)
            //    return false;

            //TargetId = new ElementId(Convert.ToInt32(values[0]));
            //LineIds = new List<ElementId>();
            //foreach (var item in values[1].Split(PropertyInnerSplitter_Char))
            //    LineIds.Add(new ElementId(Convert.ToInt32(item)));
            ////TextNoteIds
            //TextNoteIds = new List<ElementId>();
            //foreach (var item in values[2].Split(PropertyInnerSplitter_Char))
            //    TextNoteIds.Add(new ElementId(Convert.ToInt32(item)));
            //CSALocationType = (CSALocationType)Enum.Parse(typeof(CSALocationType), values[3]);
            ////TextLocations
            //TextLocations = new List<XYZ>();
            //foreach (var item in values[4].Split(PropertyInnerSplitter_Char))
            //{
            //    if (string.IsNullOrEmpty(item))
            //        continue;
            //    var innerItem = item.Split(PropertyInnerSplitter2_Char);
            //    TextLocations.Add(new XYZ(Convert.ToDouble(innerItem[0]), Convert.ToDouble(innerItem[1]), Convert.ToDouble(innerItem[2])));
            //}
            ////Texts
            //Texts = new List<string>();
            //foreach (var item in values[5].Split(PropertyInnerSplitter_Char))
            //{
            //    if (string.IsNullOrEmpty(item))
            //        continue;
            //    Texts.Add(item);
            //}
            //return true;
        }

        public override string ToData()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendItem(TargetId);
            sb.AppendItem(LineIds);
            sb.AppendItem(TextNoteIds);
            sb.AppendItem(TextNoteTypeElementId);
            sb.AppendItem(CSALocationType);
            sb.AppendItem(TextLocations);
            sb.AppendItem(Texts);
            return sb.ToCollection();

            ////string str = TargetId.IntegerValue.ToString();
            ////sb.Append(str.Length + PropertyInnerSplitter + str);
            ////string subStr = GetSubString(LineIds);
            ////sb.Append(subStr.Length + PropertyInnerSplitter + subStr);
            ////subStr = GetSubString(TextNoteIds);
            ////sb.Append(subStr.Length + PropertyInnerSplitter + subStr);
            ////sb.Append(TargetId.IntegerValue.ToString().Length + PropertyInnerSplitter + TargetId.IntegerValue);
            ////str = CSALocationType.ToString();
            ////sb.Append(str.Length + PropertyInnerSplitter + str);



            //return TargetId.IntegerValue
            //+ PropertySplitter + string.Join(PropertyInnerSplitter, LineIds.Select(c => c.IntegerValue))
            //+ PropertySplitter + string.Join(PropertyInnerSplitter, TextNoteIds.Select(c => c.IntegerValue))
            //+ PropertySplitter + (int)CSALocationType
            //+ PropertySplitter + string.Join(PropertyInnerSplitter, TextLocations.Select(c => c.X + PropertyInnerSplitter2 + c.Y + PropertyInnerSplitter2 + c.Z))
            //+ PropertySplitter + string.Join(PropertyInnerSplitter, Texts);



            //return TargetId.IntegerValue
            //+ PropertySplitter + string.Join(PropertyInnerSplitter, LineIds.Select(c => c.IntegerValue))
            //+ PropertySplitter + string.Join(PropertyInnerSplitter, TextNoteIds.Select(c => c.IntegerValue))
            //+ PropertySplitter + (int)CSALocationType
            //+ PropertySplitter + string.Join(PropertyInnerSplitter, TextLocations.Select(c => c.X + PropertyInnerSplitter2 + c.Y + PropertyInnerSplitter2 + c.Z))
            //+ PropertySplitter + string.Join(PropertyInnerSplitter, Texts);
        }
        #endregion

        #region 无需留存的数据
        //选中的文本长宽关键信息
        public double currentFontSizeScale;
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
        /// 当前文本高度 double = foot
        /// </summary>
        public double CurrentFontHeight { set; get; }
        /// <summary>
        /// 当前文本 Revit中的宽度缩放比例
        /// </summary>
        public double CurrentFontWidthScale { set; get; }
        public TargetType TargetType { set; get; }
        /// <summary>
        /// 需要显示的结构标注信息
        /// </summary>
        public List<string> Texts { set; get; }
        /// <summary>
        /// 坐标定位,平行于标注对象
        /// </summary>
        public XYZ ParallelVector = null;
        /// <summary>
        /// 坐标定位,垂直于标注对象
        /// </summary>
        public XYZ VerticalVector = null;
        /// <summary>
        /// 线起点
        /// </summary>
        public XYZ LineStartLocation { set; get; }
        /// <summary>
        /// 线终点
        /// </summary>
        public XYZ LineEndLocation { set; get; }
        /// <summary>
        /// 横线
        /// </summary>
        public List<TextLocation> ParallelLinesLocations { set; get; }
        #endregion

        #region 方法
        public void UpdateVectors(Line locationCurve)
        {
            XYZ parallelVector = null;
            XYZ verticalVector = null;
            parallelVector = locationCurve.Direction;
            verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
            parallelVector = VLLocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour, CoordinateType.XY);
            verticalVector = VLLocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo, CoordinateType.XY);
            double xyzTolarance = 0.01;
            if (Math.Abs(verticalVector.X) > 1 - xyzTolarance)
            {
                verticalVector = new XYZ(-verticalVector.X, -verticalVector.Y, verticalVector.Z);
            }
            VerticalVector = verticalVector;
            ParallelVector = parallelVector;
        }

        #region 详图线的计算方案
        public void CalculateLocations(Element element, XYZ offset)
        {
            var scale = 1 / VLConstraintsForCSA.OrientFontSizeScale * CurrentFontSizeScale;
            var width = CSALocationType.GetLineWidth() * scale;
            var height = 400 * scale;
            var widthFoot = UnitHelper.ConvertToFoot(width, VLUnitType.millimeter);
            var heightFoot = UnitHelper.ConvertToFoot(height, VLUnitType.millimeter);
            var verticalFix = UnitHelper.ConvertToFoot(100, VLUnitType.millimeter) * scale;
            var locationCurve = TargetType.GetLine(element);
            UpdateVectors(locationCurve);
            //线起始点 (中点)
            LineStartLocation = (locationCurve.GetEndPoint(0) + locationCurve.GetEndPoint(1)) / 2;
            //文本位置 start:(附着元素中点+线基本高度+文本高度*(文本个数-1))  end: start+宽
            //高度,宽度 取决于文本 
            ParallelLinesLocations = new List<TextLocation>();
            TextLocations = new List<XYZ>();
            for (int i = Texts.Count() - 1; i >= 0; i--)
            {
                var start = LineStartLocation + (heightFoot + i * CurrentFontHeight) * VerticalVector;
                var end = start + widthFoot * ParallelVector;
                ParallelLinesLocations.Add(new TextLocation(start, end));
                TextLocations.Add(CSALocationType.GetTextLocation(CurrentFontHeight, verticalFix, VerticalVector, start, end));
            }
            //线终点 (最高的文本位置)
            LineEndLocation = ParallelLinesLocations[0].Start;
        }

        public void CalculateLocations(Element element)
        {
            CalculateLocations(element, new XYZ(0, 0, 0));
        }
        #endregion

        #region 线族的计算方案
        //public void CalculateLocations(Element element, FamilyInstance line, XYZ offset)
        //{
        //    var locationCurve = element.Location as LocationCurve;
        //    LineLocation = (locationCurve.Curve.GetEndPoint(0) + locationCurve.Curve.GetEndPoint(1)) / 2;
        //    XYZ parallelVector = null;
        //    XYZ verticalVector = null;
        //    parallelVector = (locationCurve.Curve as Line).Direction;
        //    verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
        //    parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
        //    verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
        //    double xyzTolarance = 0.01;
        //    if (Math.Abs(verticalVector.X) > 1 - xyzTolarance)
        //    {
        //        verticalVector = new XYZ(-verticalVector.X, -verticalVector.Y, verticalVector.Z);
        //    }
        //    model.ParallelVector = parallelVector;
        //    model.VerticalVector = verticalVector;
        //    var height = Convert.ToDouble(line.GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) + (model.Texts.Count() - 1) * AnnotationConstaints.TextHeight;
        //    var textSize = PipeAnnotationContext.TextSize;
        //    var widthScale = PipeAnnotationContext.WidthScale;
        //    for (int i = 0; i < model.Texts.Count(); i++)
        //    {
        //        var textLength = System.Windows.Forms.TextRenderer.MeasureText(model.Texts[i], AnnotationConstaints.Font).Width;
        //        var actualLength = textLength / (textSize * widthScale);
        //        switch (model.CSALocationType)
        //        {
        //            case CSALocationType.OnLine:
        //                model.TextLocations.Add(GetTagHeadPositionWithParam(model, height, 0.2, 0.5, i, actualLength));
        //                break;
        //            case CSALocationType.OnEdge:
        //                model.TextLocations.Add(GetTagHeadPositionWithParam(model, height, 0.2, 0.5, i, actualLength));
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //}
        #endregion

        #endregion
    }

    /// <summary>
    /// CompoundStructureAnnotation数据的载体 线族方案
    /// </summary>
    public class CSAModel : VLModel
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


        //TODO
        /// <summary>
        /// 字体的毫米数
        /// </summary>
        public double FontScale { get; set; }
        /// <summary>
        /// 字体高度高度 由基础基础字体高度/4*FontScale得出
        /// </summary>
        public double LineHeight { set; get; }

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
            return sb.ToCollection();
        }
        #endregion

        #region 无需留存的数据
        //选中的文本长宽关键信息
        public double currentFontSizeScale;
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
        public double LineWidth { get; private set; }
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
            parallelVector = VLLocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour, CoordinateType.XY);
            verticalVector = VLLocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo, CoordinateType.XY);
            double xyzTolarance = 0.01;
            if (Math.Abs(verticalVector.X) > 1 - xyzTolarance)
            {
                verticalVector = new XYZ(-verticalVector.X, -verticalVector.Y, verticalVector.Z);
            }
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
            FontScale = 1 / VLConstraintsForCSA.OrientFontSizeScale * CurrentFontSizeScale;
            var fontHeight = CurrentFontHeight;
            XYZ parallelVector = null;
            XYZ verticalVector = null;
            parallelVector = (locationCurve as Line).Direction;
            verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
            parallelVector = VLLocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour, CoordinateType.XY);
            verticalVector = VLLocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo, CoordinateType.XY);
            //计算线的定位位置
            bool isRegenerate = offset != null;
            if (!isRegenerate)
            {
                LineLocation = (locationCurve.GetEndPoint(0) + locationCurve.GetEndPoint(1)) / 2;
                offset = new XYZ(0, 0, 0);
                LineHeight = fontHeight;
            }
            else
            {
                LineLocation = locationCurve.Project(LineLocation + offset).XYZPoint;
                LineHeight = line.GetParameters(TagProperty.线高度1.ToString()).First().AsDouble() + VLLocationHelper.GetLengthBySide(offset, verticalVector);
                LineHeight = LineHeight > fontHeight ? LineHeight : fontHeight;//确保最短长度有一个文字高度
            }
            double xyzTolarance = 0.01;
            if (Math.Abs(verticalVector.X) > 1 - xyzTolarance)
                verticalVector = new XYZ(-verticalVector.X, -verticalVector.Y, verticalVector.Z);
            ParallelVector = parallelVector;
            VerticalVector = verticalVector;
            //高度,宽度 取决于文本 
            LineWidth = UnitHelper.ConvertToFoot(CSALocationType.GetLineWidth() * FontScale, VLUnitType.millimeter);
            var verticalFix = fontHeight * VLConstraintsForCSA.TextSpace; ;//偏移修正 为了显示更好 方便更改
            LineSpace = fontHeight * (1 + VLConstraintsForCSA.TextSpace);
            TextLocations = new List<XYZ>();
            for (int i = Texts.Count() - 1; i >= 0; i--)
            {
                var start = LineLocation + (LineHeight + i * LineSpace) * VerticalVector;
                var end = start + LineWidth * ParallelVector;
                TextLocations.Add(CSALocationType.GetTextLocation(CurrentFontHeight, verticalFix, VerticalVector, start, end));
            }
        }
        #endregion

        #endregion
    }
}
