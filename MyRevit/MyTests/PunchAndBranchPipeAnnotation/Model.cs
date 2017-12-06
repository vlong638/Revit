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
        public Document Document { set; get;}

        #region 留存数据
        public ElementId ViewId { get; internal set; }
        /// <summary>
        /// 标记样式
        /// </summary>
        public PBPAAnnotationType AnnotationType { set; get; }
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
        public CoordinateType CoordinateType { set; get; }
        public bool IsRegenerate { set; get; }
        /// <summary>
        /// 线宽
        /// </summary>
        public double LineWidth { get; set; }
        public XYZ ParallelVector = null;//坐标定位,平行于标注对象
        public XYZ VerticalVector = null;//坐标定位,垂直于标注对象

        #endregion


        public PBPATargetType TargetType { set; get; }//标注对象
        public PBPALocationType LocationType { set; get; }//离地模式

        public string AnnotationPrefix { get; internal set; }//标注前缀

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
                AnnotationType = sr.ReadFormatStringAsEnum<PBPAAnnotationType>();
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
            sb.AppendItem(AnnotationType);
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
            List<ClassFilter> ClassFilters = new List<ClassFilter>();
            if ((int)(TargetType & PBPATargetType.BranchPipe) >0)
            {
                ClassFilters.Add(new ClassFilter(typeof(FamilyInstance), false, (element) =>
                {
                    var familySymbol = Document.GetElement(element.GetTypeId()) as FamilySymbol;
                    if (familySymbol == null)
                        return false;
                    return new List<string>() { "圆形套管", "椭圆形套管", "矩形套管" }.Contains(familySymbol.Family.Name);
                }));
            }
            if ((int)(TargetType & PBPATargetType.Punch) > 0)
            {
                ClassFilters.Add(new ClassFilter(typeof(FamilyInstance), false, (element) =>
                {
                    var familySymbol = Document.GetElement(element.GetTypeId()) as FamilySymbol;
                    if (familySymbol == null)
                        return false;
                    return new List<string>() { "圆形洞口", "椭圆形洞口", "矩形洞口" }.Contains(familySymbol.Family.Name);
                }));
            }
            return new ClassesFilter(false, ClassFilters);
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

        internal void Clear()
        {
            foreach (var lineId in LineIds)
                if (Document.GetElement(lineId) != null)
                    Document.Delete(lineId);
            if (Document.GetElement(AnnotationId) != null)
                Document.Delete(AnnotationId);
            return;
        }

        internal string GetFull_L(Element target)
        {
            var offset = UnitHelper.ConvertFromFootTo(target.GetParameters(PBPAContext.SharedParameterOffset).First().AsDouble(), VLUnitType.millimeter);
            return AnnotationPrefix + offset;
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
            var locationCurve =(target.Location as LocationCurve).Curve as Line;
            //干线起始点 
            var lineBound = Line.CreateBound(BodyStartPoint, BodyEndPoint);
            if (lineBound.VL_IsIntersect(locationCurve))
            {
                var intersectionPoints = lineBound.VL_GetIntersectedOrContainedPoints(locationCurve);
                if (intersectionPoints.Count == 1)
                    BodyStartPoint = intersectionPoints.FirstOrDefault().ToSameZ(BodyStartPoint);
            }
            else { } //否则不改变原始坐标,仅重置
                     //支线终点
            if (!IsRegenerate)
                LeafEndPoint = BodyEndPoint + LineWidth * ParallelVector;
            //文本位置 start:(附着元素中点+线基本高度+文本高度*(文本个数-1))  end: start+宽
            //高度,宽度 取决于文本 
            AnnotationLocation = BodyEndPoint;
            return true;
        }

        public void UpdateVector(CoordinateType coordinateType)
        {
            var pVector = coordinateType.GetParallelVector();
            pVector = VLLocationHelper.GetVectorByQuadrant(pVector, QuadrantType.OneAndFour, coordinateType);
            var vVector = VLLocationHelper.GetVerticalVector(pVector);
            vVector = VLLocationHelper.GetVectorByQuadrant(vVector, QuadrantType.OneAndFour, coordinateType);
            VerticalVector = vVector;
            ParallelVector = pVector;
        }
    }
}
