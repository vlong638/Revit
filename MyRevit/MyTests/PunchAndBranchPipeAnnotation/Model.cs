using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI.Selection;
using MyRevit.MyTests.Utilities;
using MyRevit.MyTests.VLBase;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MyRevit.MyTests.PBPA
{
    /// <summary>
    /// 标注对象
    /// </summary>
    [Flags]
    public enum PBPATargetType
    {
        None = 0,
        /// <summary>
        /// 开洞
        /// </summary>
        Punch = 1,
        /// <summary>
        /// 套管
        /// </summary>
        BranchPipe = 2,
    }
    /// <summary>
    /// 标记样式
    /// </summary>
    public enum PBPAAnnotationType
    {
        /// <summary>
        /// 双行式
        /// </summary>
        TwoLine,
        /// <summary>
        /// 单行式
        /// </summary>
        OneLine,
    }
    /// <summary>
    /// 离地模式
    /// </summary>
    public enum PBPALocationType
    {
        /// <summary>
        /// 中心离地
        /// </summary>
        Center,
        /// <summary>
        /// 顶部离地
        /// </summary>
        Top,
        /// <summary>
        /// 底部离地
        /// </summary>
        Bottom,
    }

    public class PBPAModel : VLModel
    {
        public Document Document { set; get; }

        #region 留存数据
        public ElementId ViewId { get; internal set; }
        /// <summary>
        /// 标注对象
        /// </summary>
        public PBPATargetType TargetType { set; get; }
        /// <summary>
        /// 离地模式
        /// </summary>
        public PBPALocationType LocationType { set; get; }
        /// <summary>
        /// 标记样式
        /// </summary>
        public PBPAAnnotationType AnnotationType { set; get; }
        /// <summary>
        /// 标注前缀
        /// </summary>
        public string AnnotationPrefix { get; set; }
        /// <summary>
        /// 单一标注 目标对象
        /// </summary>
        public ElementId TargetId { set; get; }
        /// <summary>
        /// 单一标注 线对象
        /// </summary>
        public List<ElementId> LineIds { get; set; }
        /// <summary>
        /// 单一标注 标注
        /// </summary>
        public ElementId AnnotationId { set; get; }
        /// <summary>
        /// 单一标注 干线终点
        /// </summary>
        public XYZ BodyEndPoint { get; set; }
        /// <summary>
        /// 单一标注 干线起点
        /// </summary>
        public XYZ BodyStartPoint { get; set; }
        /// <summary>
        /// 单一标注 支线终点
        /// </summary>
        public XYZ LeafEndPoint { set; get; }
        /// <summary>
        /// 目标定位, 单管以中点为准 多管以最上管道的中点为准
        /// </summary>
        public XYZ TargetLocation { get; set; }
        /// <summary>
        /// 单一标注 文本定位坐标
        /// </summary>
        public XYZ AnnotationLocation { set; get; }
        /// <summary>
        /// 当前文本 Revit中的宽度缩放比例 
        /// </summary>
        public double CurrentFontWidthSize { set; get; }
        #endregion


        #region 非留存数据
        public bool IsReversed { set; get; }
        public CoordinateType CoordinateType { set; get; }
        public bool IsRegenerate { set; get; }
        /// <summary>
        /// 线宽
        /// </summary>
        public double LineWidth { get; set; }
        public XYZ ParallelVector = null;//坐标定位,平行于标注对象
        public XYZ VerticalVector = null;//坐标定位,垂直于标注对象

        #endregion

        public PBPAModel() : base("")
        {
        }
        public PBPAModel(string data) : base(data)
        {
        }

        public override bool LoadData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return false;
            try
            {
                StringReader sr = new StringReader(data);
                ViewId = sr.ReadFormatStringAsElementId();
                TargetType = sr.ReadFormatStringAsEnum<PBPATargetType>();
                LocationType = sr.ReadFormatStringAsEnum<PBPALocationType>();
                AnnotationType = sr.ReadFormatStringAsEnum<PBPAAnnotationType>();
                AnnotationPrefix = sr.ReadFormatString();
                TargetId = sr.ReadFormatStringAsElementId();
                LineIds = sr.ReadFormatStringAsElementIds();
                AnnotationId = sr.ReadFormatStringAsElementId();
                BodyEndPoint = sr.ReadFormatStringAsXYZ();
                BodyStartPoint = sr.ReadFormatStringAsXYZ();
                LeafEndPoint = sr.ReadFormatStringAsXYZ();
                TargetLocation = sr.ReadFormatStringAsXYZ();
                AnnotationLocation = sr.ReadFormatStringAsXYZ();
                CurrentFontWidthSize = sr.ReadFormatStringAsDouble();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override string ToData()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendItem(ViewId);
            sb.AppendItem(TargetType);
            sb.AppendItem(LocationType);
            sb.AppendItem(AnnotationType);
            sb.AppendItem(AnnotationPrefix);
            sb.AppendItem(TargetId);
            sb.AppendItem(LineIds);
            sb.AppendItem(AnnotationId);
            sb.AppendItem(BodyEndPoint);
            sb.AppendItem(BodyStartPoint);
            sb.AppendItem(LeafEndPoint);
            sb.AppendItem(TargetLocation);
            sb.AppendItem(AnnotationLocation);
            sb.AppendItem(CurrentFontWidthSize);
            return sb.ToData();
        }

        public ISelectionFilter GetFilter()
        {
            List<VLClassFilter> ClassFilters = new List<VLClassFilter>();
            if ((int)(TargetType & PBPATargetType.BranchPipe) > 0)
            {
                ClassFilters.Add(new VLClassFilter(typeof(FamilyInstance), false, (element) =>
                {
                    var familySymbol = Document.GetElement(element.GetTypeId()) as FamilySymbol;
                    if (familySymbol == null)
                        return false;
                    return new List<string>() { "圆形套管", "椭圆形套管", "矩形套管" }.Contains(familySymbol.Family.Name);
                }));
            }
            if ((int)(TargetType & PBPATargetType.Punch) > 0)
            {
                ClassFilters.Add(new VLClassFilter(typeof(FamilyInstance), false, (element) =>
                {
                    var familySymbol = Document.GetElement(element.GetTypeId()) as FamilySymbol;
                    if (familySymbol == null)
                        return false;
                    return new List<string>() { "圆形洞口", "椭圆形洞口", "矩形洞口" }.Contains(familySymbol.Family.Name);
                }));
            }
            return new VLClassesFilter(false, ClassFilters);
        }

        internal void UpdateLineWidth(Element target)
        {
            switch (AnnotationType)
            {
                case PBPAAnnotationType.TwoLine:
                    string symbolName = (Document.GetElement(target.GetTypeId()) as FamilySymbol).Name;
                    string pl = target.GetParameters(PBPAContext.SharedParameterPL).First().AsString();
                    var textWidth = Math.Max(TextRenderer.MeasureText(symbolName, PBPAContext.FontManagement.OrientFont).Width, TextRenderer.MeasureText(pl, PBPAContext.FontManagement.OrientFont).Width);
                    LineWidth = textWidth * CurrentFontWidthSize;
                    break;
                case PBPAAnnotationType.OneLine:
                    LineWidth = 10 * CurrentFontWidthSize;
                    break;
                default:
                    break;
            }
        }

        public FamilySymbol GetAnnotationFamily(Document doc)
        {
            switch (AnnotationType)
            {
                case PBPAAnnotationType.TwoLine:
                    return PBPAContext.GetTwoLine_Annotation(doc);
                case PBPAAnnotationType.OneLine:
                    return PBPAContext.GetOneLine_Annotation(doc);
                default:
                    throw new NotImplementedException("暂不支持该类型");
            }
        }

        public void Clear()
        {
            foreach (var lineId in LineIds)
                if (Document.GetElement(lineId) != null)
                    Document.Delete(lineId);
            if (Document.GetElement(AnnotationId) != null)
                Document.Delete(AnnotationId);
            return;
        }

        private static bool IsRound(Element element)
        {
            var familyName = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString();
            return familyName.Contains("圆形") && !familyName.Contains("椭圆形");
        }

        public bool IsPunch(Element element)
        {
            var familyName = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString();
            return familyName.Contains("洞口");
        }

        private double GetDN(Element target)
        {
            switch (TargetType)
            {
                case PBPATargetType.BranchPipe:
                    if (IsRound(target))
                        return (Document.GetElement(target.GetTypeId()) as FamilySymbol).GetParameters(PBPAContext.SharedParameterWidth).First().AsDouble();
                    else
                        return (Document.GetElement(target.GetTypeId()) as FamilySymbol).GetParameters(PBPAContext.SharedParameterHeight).First().AsDouble();
                //{
                //    var symbol = (Document.GetElement(target.GetTypeId()) as FamilySymbol);
                //    var height = symbol.GetParameters(PBPAContext.SharedParameterHeight);
                //    return height.First().AsDouble();
                //}
                case PBPATargetType.Punch:
                    if (IsRound(target))
                        return (Document.GetElement(target.GetTypeId()) as FamilySymbol).GetParameters(PBPAContext.SharedParameterWidth).First().AsDouble();
                    else
                        return (Document.GetElement(target.GetTypeId()) as FamilySymbol).GetParameters(PBPAContext.SharedParameterHeight).First().AsDouble();
                case PBPATargetType.BranchPipe | PBPATargetType.Punch:
                default:
                    throw new NotImplementedException("暂未支持该类型");
            }
        }

        internal string GetFull_L(Element target)
        {
            var offset = (target as Instance).GetTotalTransform().Origin.Z;
            switch (LocationType)
            {
                case PBPALocationType.Center:
                    break;
                case PBPALocationType.Top:
                    var DN = GetDN(target);
                    offset += DN / 2;
                    break;
                case PBPALocationType.Bottom:
                    DN = GetDN(target);
                    offset -= DN / 2;
                    break;
                default:
                    break;
            }
            return AnnotationPrefix + UnitHelper.ConvertFromFootTo(offset, VLUnitType.millimeter).ToString("f1").TrimEnd(".0");
        }

        public FamilySymbol GetAnnotationFamily(Document doc, ElementId targetId)
        {
            switch (AnnotationType)
            {
                case PBPAAnnotationType.TwoLine:
                    return PBPAContext.GetTwoLine_Annotation(Document);
                case PBPAAnnotationType.OneLine:
                    return PBPAContext.GetOneLine_Annotation(Document);
                default:
                    throw new NotImplementedException("暂不支持该类型");
            }
        }

        public bool CalculateLocations()
        {
            CoordinateType coordinateType = CoordinateType.XY;
            UpdateVector(coordinateType);

            var target = Document.GetElement(TargetId);
            var locationCurve = (target.Location as LocationCurve).Curve as Line;
            //干线起始点 
            var lineBound = Line.CreateBound(BodyStartPoint, BodyEndPoint);
            if (lineBound.VL_IsIntersect(locationCurve))
            {
                var intersectionPoints = lineBound.VL_GetIntersectedOrContainedPoints(locationCurve);
                if (intersectionPoints.Count == 1)
                    BodyStartPoint = intersectionPoints.FirstOrDefault().ToSameZ(BodyEndPoint);//.ToSameZ(BodyStartPoint);
            }
            else { } //否则不改变原始坐标,仅重置
                     //支线终点
            if (AnnotationType == PBPAAnnotationType.OneLine)
            {
                if (!IsRegenerate)
                    LeafEndPoint = BodyEndPoint + LineWidth * ParallelVector;
                //文本位置 start:(附着元素中点+线基本高度+文本高度*(文本个数-1))  end: start+宽
                //高度,宽度 取决于文本 
                AnnotationLocation = BodyEndPoint;
            }
            else
            {
                if (!IsRegenerate)
                    LeafEndPoint = BodyEndPoint + (IsReversed ? -LineWidth * ParallelVector : LineWidth * ParallelVector);
                var bb = BodyEndPoint - BodyStartPoint;
                var lb = (LeafEndPoint - BodyEndPoint);
                if (lb.CrossProductByCoordinateType(bb, CoordinateType.XY) < 0)
                {
                    var temp = LeafEndPoint;
                    LeafEndPoint = BodyEndPoint;
                    BodyEndPoint = temp;
                }
                //文本位置 start:(附着元素中点+线基本高度+文本高度*(文本个数-1))  end: start+宽
                //高度,宽度 取决于文本 
                if (bb.CrossProductByCoordinateType(ParallelVector, CoordinateType.XY) < 0)
                    AnnotationLocation = LeafEndPoint;
                else
                    AnnotationLocation = BodyEndPoint;
            }
            return true;
        }

        public void UpdateVector(CoordinateType coordinateType)
        {
            var pVector = coordinateType.GetParallelVector();
            pVector = VLLocationHelper.GetVectorByQuadrant(pVector, QuadrantType.OneAndFour, coordinateType);
            var vVector = VLLocationHelper.GetVerticalVector(pVector, CoordinateType.XY);
            vVector = VLLocationHelper.GetVectorByQuadrant(vVector, QuadrantType.OneAndFour, coordinateType);
            VerticalVector = vVector;
            ParallelVector = pVector;
        }
    }
}
