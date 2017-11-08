using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.MyTests.CompoundStructureAnnotation;
using MyRevit.MyTests.PipeAnnotationTest;
using MyRevit.MyTests.Utilities;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    /// <summary>
    /// CompoundStructureAnnotation ViewType
    /// </summary>
    public enum CSAViewType
    {
        Idle,
        Select,
        Generate,
        Close,
    }

    /// <summary>
    /// CompoundStructureAnnotation 文字的定位方案
    /// </summary>
    public enum CSALocationType
    {
        /// <summary>
        /// 在线端
        /// </summary>
        OnEdge,
        /// <summary>
        /// 在线上
        /// </summary>
        OnLine,
    }

    public class TextLocation
    {
        public TextLocation(XYZ start, XYZ end)
        {
            Start = start;
            End = end;
        }

        public XYZ Start { set; get; }
        public XYZ End { set; get; }
    }
    public enum TargetType
    {
        Wall,
        Floor,
        RoofBase,
    }
    public static class TargetTypeEx
    {
        public static Line GetLine(this TargetType targetType, Element element)
        {
            switch (targetType)
            {
                case TargetType.Wall:
                    return (element.Location as LocationCurve).Curve as Line;
                case TargetType.Floor:
                case TargetType.RoofBase:
                    var option = new Options() { View = VLConstraints.Doc.ActiveView };
                    var geometry = element.get_Geometry(option);
                    var geometryElements = geometry as GeometryElement;
                    var solid = geometryElements.FirstOrDefault() as Solid;
                    if (solid == null)
                        throw new NotImplementedException("实体与预期不一致");
                    List<Face> faces = new List<Face>();
                    foreach (Face face in solid.Faces)
                    {
                        //矩形面
                        var planarFace = face as PlanarFace;
                        if (planarFace != null)
                        {
                            if (planarFace.FaceNormal.Z > 0)
                                faces.Add(face);
                        }
                    }
                    //选出
                    Line topLine = null;
                    topLine = GetTopLine(faces);
                    var z = VLConstraints.Doc.ActiveView.SketchPlane.GetPlane().Origin.Z;
                    XYZ p1 = topLine.GetEndPoint(0);
                    XYZ p2 = topLine.GetEndPoint(1);
                    return Line.CreateBound(new XYZ(p1.X, p1.Y, z), new XYZ(p2.X, p2.Y, z));
                default:
                    throw new NotImplementedException("该类型暂不支持");
            }
        }

        /// <summary>
        /// 根据Y优先上面 X次优先左面的方案取附着线
        /// </summary>
        /// <param name="faces"></param>
        /// <returns></returns>
        private static Line GetTopLine(List<Face> faces)
        {
            Line topLine = null;
            double weightY = double.MinValue;//Revit中 从上到下 Y递减 上大
            double weightX = double.MaxValue;//Revit中 从左到右 X递增 右大
            foreach (var face in faces)
            {
                foreach (EdgeArray edgeLoop in face.EdgeLoops)
                {
                    foreach (Edge edge in edgeLoop)
                    {
                        var points = edge.Tessellate();
                        for (int i = 0; i <= points.Count - 2; i++)
                        {
                            var currentWeightY = points[i].Y + points[i + 1].Y;
                            var currentWeightX = points[i].X + points[i + 1].X;
                            if (currentWeightY > weightY || (currentWeightY == weightY && currentWeightX < weightX))
                            {
                                topLine = Line.CreateBound(points[i], points[i + 1]);
                                weightY = currentWeightY;
                                weightX = currentWeightX;
                            }
                        }
                    }
                }
            }
            PmSoft.Optimization.DrawingProduction.Utils.GraphicsDisplayerManager.Display(@"E:\WorkingSpace\Outputs\Images\1028轮廓.png", faces);
            return topLine;
        }
    }

    /// <summary>
    /// CompoundStructureAnnotation数据的载体 线族方案
    /// </summary>
    public class CSAModelForFamilyInstance : ModelBase<CSAModelForFamilyInstance>
    {
        public CSAModelForFamilyInstance() : base()
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
        /// 线族
        /// </summary>
        public ElementId LineId { get; set; }
        /// <summary>
        /// 文字 对象
        /// </summary>
        public List<ElementId> TextNoteIds { set; get; }
        /// <summary>
        /// 文字 定位方案
        /// </summary>
        public CSALocationType CSALocationType { set; get; }
        /// <summary>
        /// 字体的毫米数
        /// </summary>
        public double FontScale { get; set; }
        /// <summary>
        /// 字体高度高度 由基础基础字体高度/4*FontScale得出
        /// </summary>
        public double LineHeight { set; get; }
        /// <summary>
        /// 线起点 在附着体上
        /// </summary>
        public XYZ LineLocation { set; get; }
        /// <summary>
        /// 需要显示的结构标注信息
        /// </summary>
        public List<string> Texts { set; get; }

        public override bool FromData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return false;
            var values = data.Split(PropertySplitter_Char);
            if (values.Count() != 8)
                return false;
            TargetId = new ElementId(Convert.ToInt32(values[0]));
            LineId = new ElementId(Convert.ToInt32(values[1]));
            TextNoteIds = new List<ElementId>();
            foreach (var item in values[2].Split(PropertyInnerSplitter_Char))
                TextNoteIds.Add(new ElementId(Convert.ToInt32(item)));
            CSALocationType = (CSALocationType)Enum.Parse(typeof(CSALocationType), values[3]);
            FontScale = Convert.ToDouble(values[4]);
            LineHeight = Convert.ToDouble(values[5]);
            var subLocations = values[6].Split(PropertyInnerSplitter2_Char);
            LineLocation = new XYZ(Convert.ToDouble(subLocations[0]), Convert.ToDouble(subLocations[1]), Convert.ToDouble(subLocations[2]));
            Texts = new List<string>();
            foreach (var item in values[7].Split(PropertyInnerSplitter_Char))
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                Texts.Add(item);
            }
            return true;
        }

        public override string ToData()
        {
            return TargetId.IntegerValue
            + PropertySplitter + LineId.IntegerValue
            + PropertySplitter + string.Join(PropertyInnerSplitter, TextNoteIds.Select(c => c.IntegerValue))
            + PropertySplitter + (int)CSALocationType
            + PropertySplitter + FontScale
            + PropertySplitter + LineHeight
            + PropertySplitter + LineLocation.X + PropertyInnerSplitter2 + LineLocation.Y + PropertyInnerSplitter2 + LineLocation.Z
            + PropertySplitter + string.Join(PropertyInnerSplitter, Texts);
        }
        #endregion

        #region 无需留存的数据
        /// <summary>
        /// 主体目标的类型
        /// </summary>
        public TargetType TargetType { set; get; }
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
                var textNoteType = VLConstraints.Doc.GetElement(textNoteTypeElementId) as TextNoteType;
                VLConstraints.CurrentFontSizeScale = UnitHelper.ConvertFromFootTo(textNoteType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble(), VLUnitType.millimeter);//文本大小
                VLConstraints.CurrentFontWidthScale = textNoteType.get_Parameter(BuiltInParameter.TEXT_WIDTH_SCALE).AsDouble();//文本宽度系数
            }
        }
        private ElementId textNoteTypeElementId = null;
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
            parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
            verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
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
            FontScale = 1 / VLConstraints.OrientFontSizeScale * VLConstraints.CurrentFontSizeScale;
            var fontHeight = VLConstraints.CurrentFontHeight;
            XYZ parallelVector = null;
            XYZ verticalVector = null;
            parallelVector = (locationCurve as Line).Direction;
            verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
            parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
            verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
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
                LineHeight= line.GetParameters(TagProperty.线高度1.ToString()).First().AsDouble() + LocationHelper.GetLengthBySide(offset, verticalVector);
                LineHeight = LineHeight > fontHeight ? LineHeight : fontHeight;//确保最短长度有一个文字高度
            }
            double xyzTolarance = 0.01;
            if (Math.Abs(verticalVector.X) > 1 - xyzTolarance)
                verticalVector = new XYZ(-verticalVector.X, -verticalVector.Y, verticalVector.Z);
            ParallelVector = parallelVector;
            VerticalVector = verticalVector;
            //高度,宽度 取决于文本 
            LineWidth = UnitHelper.ConvertToFoot(CSALocationType.GetLineWidth() * FontScale, VLUnitType.millimeter);
            var verticalFix = fontHeight * VLConstraints.TextSpace;;//偏移修正 为了显示更好 方便更改
            LineSpace = fontHeight * (1 + VLConstraints.TextSpace);
            TextLocations = new List<XYZ>();
            for (int i = Texts.Count() - 1; i >= 0; i--)
            {
                var start = LineLocation + (LineHeight + i * LineSpace) * VerticalVector;
                var end = start + LineWidth * ParallelVector;
                TextLocations.Add(CSALocationType.GetTextLocation(verticalFix, VerticalVector, start, end));
            }
        }
        #endregion

        #endregion
    }
    /// <summary>
    /// CompoundStructureAnnotation数据的载体 详图线方案
    /// </summary>
    public class CSAModelForDetailLine : ModelBase<CSAModelForFamilyInstance>
    {
        public CSAModelForDetailLine() : base()
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

        public override bool FromData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return false;
            var values = data.Split(PropertySplitter_Char);
            if (values.Count() != 6)
                return false;

            TargetId = new ElementId(Convert.ToInt32(values[0]));
            LineIds = new List<ElementId>();
            foreach (var item in values[1].Split(PropertyInnerSplitter_Char))
                LineIds.Add(new ElementId(Convert.ToInt32(item)));
            //TextNoteIds
            TextNoteIds = new List<ElementId>();
            foreach (var item in values[2].Split(PropertyInnerSplitter_Char))
                TextNoteIds.Add(new ElementId(Convert.ToInt32(item)));
            CSALocationType = (CSALocationType)Enum.Parse(typeof(CSALocationType), values[3]);
            //TextLocations
            TextLocations = new List<XYZ>();
            foreach (var item in values[4].Split(PropertyInnerSplitter_Char))
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                var innerItem = item.Split(PropertyInnerSplitter2_Char);
                TextLocations.Add(new XYZ(Convert.ToDouble(innerItem[0]), Convert.ToDouble(innerItem[1]), Convert.ToDouble(innerItem[2])));
            }
            //Texts
            Texts = new List<string>();
            foreach (var item in values[5].Split(PropertyInnerSplitter_Char))
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                Texts.Add(item);
            }
            return true;
        }

        public override string ToData()
        {
            return TargetId.IntegerValue
            + PropertySplitter + string.Join(PropertyInnerSplitter, LineIds.Select(c => c.IntegerValue))
            + PropertySplitter + string.Join(PropertyInnerSplitter, TextNoteIds.Select(c => c.IntegerValue))
            + PropertySplitter + (int)CSALocationType
            + PropertySplitter + string.Join(PropertyInnerSplitter, TextLocations.Select(c => c.X + PropertyInnerSplitter2 + c.Y + PropertyInnerSplitter2 + c.Z))
            + PropertySplitter + string.Join(PropertyInnerSplitter, Texts);
        }
        #endregion

        #region 无需留存的数据
        public TargetType TargetType { set; get; }
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
                var textNoteType = VLConstraints.Doc.GetElement(textNoteTypeElementId) as TextNoteType;
                VLConstraints.CurrentFontSizeScale = UnitHelper.ConvertFromFootTo(textNoteType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble(), VLUnitType.millimeter);//文本大小
                VLConstraints.CurrentFontWidthScale = textNoteType.get_Parameter(BuiltInParameter.TEXT_WIDTH_SCALE).AsDouble();//文本宽度系数
            }
        }
        private ElementId textNoteTypeElementId = null;
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
            parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
            verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
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
            var scale = 1 / VLConstraints.OrientFontSizeScale * VLConstraints.CurrentFontSizeScale;
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
                var start = LineStartLocation + (heightFoot + i * VLConstraints.CurrentFontHeight) * VerticalVector;
                var end = start + widthFoot * ParallelVector;
                ParallelLinesLocations.Add(new TextLocation(start, end));
                TextLocations.Add(CSALocationType.GetTextLocation(verticalFix, VerticalVector, start, end));
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

    static class CSALocationTypeEx
    {
        public static double GetLineWidth(this CSALocationType CSALocationType)
        {
            return CSALocationType == CSALocationType.OnEdge ? 400 : 3000;
        }

        public static XYZ GetTextLocation(this CSALocationType CSALocationType, double verticalFix, XYZ verticalVector, XYZ start, XYZ end)
        {
            return CSALocationType == CSALocationType.OnEdge ? end + (VLConstraints.CurrentFontHeight / 2 + verticalFix) * verticalVector : start + (VLConstraints.CurrentFontHeight + verticalFix) * verticalVector;
        }
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
        public static List<TextNoteType> GetTextNoteTypes(this CSAModelForFamilyInstance model)
        {
            var textNoteTypes = new FilteredElementCollector(VLConstraints.Doc).OfClass(typeof(TextNoteType)).ToElements().Select(p => p as TextNoteType).ToList();
            return textNoteTypes;
        }

        /// <summary>
        /// 线 参数设置
        /// </summary>
        public static void UpdateLineParameters(this CSAModelForFamilyInstance model, FamilyInstance line, double lineHeight, double lineWidth, double space, int textCount)
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
        public static CompoundStructure GetCompoundStructure(this CSAModelForFamilyInstance model, Element element)
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
        public static List<string> FetchTextsFromCompoundStructure(this CSAModelForFamilyInstance model, Document doc, CompoundStructure compoundStructure)
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
        public static XYZ GetElementLocation(this CSAModelForFamilyInstance model, Element element)
        {
            var locationCurve = element.Location as LocationCurve;
            var location = (locationCurve.Curve.GetEndPoint(0) + locationCurve.Curve.GetEndPoint(1)) / 2;
            return location;
        }

        private static void SetCurveFromLine(CSAModelForFamilyInstance model, LocationCurve locationCurve)
        {
            XYZ parallelVector = null;
            XYZ verticalVector = null;
            parallelVector = (locationCurve.Curve as Line).Direction;
            verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
            parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
            verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
            double xyzTolarance = 0.01;
            if (Math.Abs(verticalVector.X) > 1 - xyzTolarance)
            {
                verticalVector = new XYZ(-verticalVector.X, -verticalVector.Y, verticalVector.Z);
            }
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
            if (verticalVector.X > VLConstraints.MiniValueForXYZ)
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
            if (verticalVector.X > VLConstraints.MiniValueForXYZ)
                point.Rotate(axis, 2 * Math.PI - verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
            else
                point.Rotate(axis, verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
        }
    }


    public class ViewModelBase : DependencyObject, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        /// <summary>
        /// 实现INPC接口 监控属性改变
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    /// <summary>
    /// CompoundStructureAnnotation ViewModel
    /// </summary>
    public class CSAViewModel : ViewModelBase
    {
        public CSAViewModel()
        {
            ViewType = CSAViewType.Idle;
            Model = new CSAModelForFamilyInstance();
            LocationType = CSALocationType.OnEdge;
        }

        public CSAViewType ViewType { set; get; }
        public CSAModelForFamilyInstance Model { set; get; }

        #region 绑定用属性 需在ViewModel中初始化
        CSALocationType LocationType
        {
            get
            {
                return Model.CSALocationType;
            }
            set
            {
                Model.CSALocationType = value;
                RaisePropertyChanged("IsDocument");
                RaisePropertyChanged("IsLinkDocument");
            }
        }
        public bool IsOnLine
        {
            get { return LocationType == CSALocationType.OnLine; }
            set { if (value) LocationType = CSALocationType.OnLine; }
        }
        public bool IsOnEdge
        {
            get { return LocationType == CSALocationType.OnEdge; }
            set { if (value) LocationType = CSALocationType.OnEdge; }
        }

        public List<TextNoteType> TextNoteTypes { get { return Model.GetTextNoteTypes(); } }
        public ElementId TextNoteTypeElementId
        {
            get
            {
                if (Model.TextNoteTypeElementId == null)
                {
                    Model.TextNoteTypeElementId = TextNoteTypes.FirstOrDefault().Id;
                }
                return Model.TextNoteTypeElementId;
            }
            set
            {
                Model.TextNoteTypeElementId = value;
                this.RaisePropertyChanged("Type");
            }
        }
        #endregion

        public void Execute(CompoundStructureAnnotationWindow window, PmSoft.Optimization.DrawingProduction.CompoundStructureAnnotationSet set, UIDocument uiDoc)
        {
            switch (ViewType)
            {
                case CSAViewType.Idle:
                    window = new CompoundStructureAnnotationWindow(set);
                    IntPtr rvtPtr = Autodesk.Windows.ComponentManager.ApplicationWindow;
                    WindowInteropHelper helper = new WindowInteropHelper(window);
                    helper.Owner = rvtPtr;
                    window.ShowDialog();
                    break;
                case CSAViewType.Select:
                    if (window.IsActive)
                        window.Close();
                    using (PmSoft.Common.RevitClass.PickObjectsMouseHook MouseHook = new PmSoft.Common.RevitClass.PickObjectsMouseHook())
                    {
                        MouseHook.InstallHook(PmSoft.Common.RevitClass.PickObjectsMouseHook.OKModeENUM.Objects);
                        try
                        {
                            Model.TargetId = uiDoc.Selection.PickObject(ObjectType.Element
                                , new ClassesFilter(false, typeof(Wall), typeof(Floor), typeof(ExtrusionRoof), typeof(FootPrintRoof))).ElementId;
                            MouseHook.UninstallHook();
                            ViewType = CSAViewType.Generate;
                        }
                        catch (Exception ex)
                        {
                            MouseHook.UninstallHook();
                            ViewType = CSAViewType.Idle;
                        }
                    }
                    break;
                case CSAViewType.Generate:
                    var doc = uiDoc.Document;
                    if (TransactionHelper.DelegateTransaction(doc, "生成结构标注", (Func<bool>)(() =>
                    {
                        var element = doc.GetElement(Model.TargetId);
                        var Collection = CSAContext.GetCollection(doc);
                        //避免重复生成 由于一个对象可能在不同的视图中进行标注设置 所以还是需要重复生成的
                        var existedModel = Collection.Data.FirstOrDefault(c => c.TargetId.IntegerValue == Model.TargetId.IntegerValue);
                        if (existedModel != null)
                        {
                            Collection.Data.Remove(existedModel);
                            CSAContext.Creater.Clear(doc, existedModel);
                        }
                        CSAContext.Creater.Generate(doc, Model, element);
                        Collection.Data.Add(Model);
                        Collection.Save(doc);
                        return true;
                    })))
                        ViewType = CSAViewType.Select;
                    else
                        ViewType = CSAViewType.Idle;
                    break;
                case CSAViewType.Close:
                    window.Close();
                    break;
                default:
                    break;
            }
        }
    }

    public class CSACreater
    {
        #region 详图线方案
        //public void Generate(Document doc, CSAModel model, Element element)
        //{
        //    Generate(doc, model, element, new XYZ(0, 0, 0));
        //}

        //private void Generate(Document doc, CSAModel model, Element element, XYZ offset)
        //{
        //    CompoundStructure compoundStructure = model.GetCompoundStructure(element);//获取文本载体
        //    if (compoundStructure == null)
        //        return;
        //    var texts = model.FetchTextsFromCompoundStructure(doc, compoundStructure);//获取文本数据
        //    if (texts.Count == 0)
        //        return;
        //    //主体
        //    model.TargetId = element.Id;

        //    #region old
        //    ////线生成
        //    //model.CalculateLocations(element, offset);//计算内容定位
        //    //List<Line> lines = new List<Line>();
        //    //lines.Add(Line.CreateBound(model.LineStartLocation, model.LineEndLocation));
        //    //foreach (var parallelLinesLocation in model.ParallelLinesLocations)
        //    //    lines.Add(Line.CreateBound(parallelLinesLocation.Start, parallelLinesLocation.End));
        //    //model.LineIds = new List<ElementId>();
        //    //foreach (var line in lines)
        //    //{
        //    //    var lineElement = doc.Create.NewModelCurve(line, VLConstraints.Doc.ActiveView.SketchPlane);
        //    //    model.LineIds.Add(lineElement.Id);
        //    //} 
        //    #endregion

        //    //族线
        //    var lineFamilySymbol = VLConstraints.GetMultipleTagSymbol(doc);//获取线标注类型
        //    var line = doc.Create.NewFamilyInstance(new XYZ(0, 0, 0), lineFamilySymbol, doc.ActiveView);//生成 线
        //    model.CalculateLocations(element, line, offset);//计算内容定位
        //    var lineLocation = model.LineLocation;
        //    var textLocations = model.TextLocations;
        //    ElementTransformUtils.MoveElement(doc, line.Id, lineLocation);//线定位
        //    LocationPoint locationPoint = line.Location as LocationPoint;//线 旋转处理
        //    locationPoint.RotateByXY(lineLocation, model.VerticalVector);
        //    model.LineId = line.Id;
        //    model.UpdateLineParameters(line, 500, 200, AnnotationConstaints.TextHeight, model.Texts.Count());//线参数设置 
        //    //文本生成
        //    List<TextNote> textNotes = new List<TextNote>();
        //    foreach (var text in model.Texts)//生成 文本
        //    {
        //        var textLocation = model.TextLocations[model.Texts.IndexOf(text)];
        //        var textNote = TextNote.Create(doc, doc.ActiveView.Id, textLocation, text, model.TextNoteTypeElementId);
        //        textNotes.Add(textNote);
        //        textNote.Location.RotateByXY(textLocation, model.VerticalVector);
        //    }
        //    model.TextNoteIds = textNotes.Select(c => c.Id).ToList();
        //    //测试用
        //    //GraphicsDisplayerManager.Display(@"E:\WorkingSpace\Outputs\Images\1023结构做法标注.png", lines, Model.TextLocations);
        //}

        //internal void Regenerate(Document doc, CSAModel model, Element target)
        //{
        //    CSAContext.Creater.Regenerate(doc, model, target, new XYZ(0, 0, 0));
        //}

        //internal void Regenerate(Document doc, CSAModel model, Element target, XYZ offset)
        //{
        //    //不是选取的文本类型 以Text的文本类型为准
        //    if (model.TextNoteTypeElementId == null)
        //        model.TextNoteTypeElementId = (doc.GetElement(model.TextNoteIds[0]) as TextNote).TextNoteType.Id;
        //    //删除线
        //    foreach (var item in model.LineIds)
        //        if (doc.GetElement(item) != null)
        //            doc.Delete(item);
        //    //删除标注
        //    foreach (var item in model.TextNoteIds)
        //        if (doc.GetElement(item) != null)
        //            doc.Delete(item);
        //    Generate(doc, model, target, offset);
        //}

        //internal void Clear(Document doc, CSAModel model)
        //{
        //    //删除线
        //    foreach (var item in model.LineIds)
        //        if (doc.GetElement(item) != null)
        //            doc.Delete(item);
        //    //删除标注
        //    foreach (var item in model.TextNoteIds)
        //        if (doc.GetElement(item) != null)
        //            doc.Delete(item);
        //}
        #endregion


        #region 线族方案
        public void Generate(Document doc, CSAModelForFamilyInstance model, Element element)
        {
            Generate(doc, model, element, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="model"></param>
        /// <param name="element"></param>
        /// <param name="offset">offset for Generate, offset should not be null when Regenerate </param>
        private void Generate(Document doc, CSAModelForFamilyInstance model, Element element, XYZ offset)
        {
            CompoundStructure compoundStructure = model.GetCompoundStructure(element);//获取文本载体
            if (compoundStructure == null)
                return;
            var texts = model.FetchTextsFromCompoundStructure(doc, compoundStructure);//获取文本数据
            if (texts.Count == 0)
                return;
            if (texts.Count==1)
            {
                TaskDialog.Show("警告", "暂不支持单层的结构做法标注");
            }
            else
            {
                model.TargetId = element.Id;//主体
                var lineFamilySymbol = VLConstraints.GetMultipleTagSymbol(doc);//获取线标注类型 
                bool isRegenerate = offset != null;
                FamilyInstance line;
                if (isRegenerate)
                {
                    line = doc.GetElement(model.LineId) as FamilyInstance;
                    model.CalculateLocations(element, line, offset);//计算内容定位
                    Clear(doc, model);
                    line = doc.Create.NewFamilyInstance(new XYZ(0, 0, 0), lineFamilySymbol, doc.ActiveView);//生成 线
                }
                else
                {
                    line = doc.Create.NewFamilyInstance(new XYZ(0, 0, 0), lineFamilySymbol, doc.ActiveView);//生成 线
                    model.CalculateLocations(element, line, offset);//计算内容定位
                }
                var lineLocation = model.LineLocation;
                var textLocations = model.TextLocations;
                ElementTransformUtils.MoveElement(doc, line.Id, lineLocation);//线定位
                LocationPoint locationPoint = line.Location as LocationPoint;//线 旋转处理
                locationPoint.RotateByXY(lineLocation, model.VerticalVector);
                model.LineId = line.Id;
                model.UpdateLineParameters(line, model.LineHeight, model.LineWidth, model.LineSpace, model.Texts.Count());//线参数设置
                List<TextNote> textNotes = new List<TextNote>();
                foreach (var text in model.Texts)//生成 文本
                {
                    var textLocation = model.TextLocations[model.Texts.IndexOf(text)];
                    var textNote = TextNote.Create(doc, doc.ActiveView.Id, textLocation, text, model.TextNoteTypeElementId);
                    textNotes.Add(textNote);
                    textNote.Location.RotateByXY(textLocation, model.VerticalVector);
                }
                model.TextNoteIds = textNotes.Select(c => c.Id).ToList();
                //测试用
                //GraphicsDisplayerManager.Display(@"E:\WorkingSpace\Outputs\Images\1023结构做法标注.png", lines, Model.TextLocations);
            }
        }

        internal void Regenerate(Document doc, CSAModelForFamilyInstance model, Element target)
        {
            FamilyInstance line = doc.GetElement(model.LineId) as FamilyInstance;
            CSAContext.Creater.Regenerate(doc, model, target, (line.Location as LocationPoint).Point - model.LineLocation);
        }

        internal void Regenerate(Document doc, CSAModelForFamilyInstance model, Element target, XYZ offset)
        {
            //不是选取的文本类型 以Text的文本类型为准
            if (model.TextNoteTypeElementId == null)
                model.TextNoteTypeElementId = (doc.GetElement(model.TextNoteIds[0]) as TextNote).TextNoteType.Id;
            Generate(doc, model, target, offset);
        }

        internal void Clear(Document doc, CSAModelForFamilyInstance model)
        {
            //删除线
            if (model.LineId != null && doc.GetElement(model.LineId) != null)
                doc.Delete(model.LineId);
            //删除标注
            foreach (var item in model.TextNoteIds)
                if (doc.GetElement(item) != null)
                    doc.Delete(item);
        }
        #endregion
    }
}
