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
using static MyRevit.MyTests.PAA.PAACreator;

namespace MyRevit.MyTests.PAA
{
    /// <summary>
    /// 标注对象
    /// </summary>
    public enum PAATargetType
    {
        /// <summary>
        /// 管道
        /// </summary>
        Pipe,
        /// <summary>
        /// 风管
        /// </summary>
        Duct,
        /// <summary>
        /// 桥架
        /// </summary>
        CableTray,
        /// <summary>
        /// 线管
        /// </summary>
        Conduit,
    }

    public static class TargetTypeEx
    {
        public static Line GetLine(this PAATargetType targetType, Element element)
        {
            switch (targetType)
            {
                case PAATargetType.Pipe:
                case PAATargetType.Duct:
                case PAATargetType.CableTray:
                    return (element.Location as LocationCurve).Curve as Line;
                case PAATargetType.Conduit:
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
            //PmSoft.Optimization.DrawingProduction.Utils.GraphicsDisplayerManager.Display(@"E:\WorkingSpace\Outputs\Images\1028轮廓.png", faces);
            return topLine;
        }
    }
    /// <summary>
    /// 标记样式  System/Size/Location
    /// </summary>
    public enum PAAAnnotationType
    {
        /// <summary>
        /// System/Size/Location
        /// Pipe:系统缩写 管道尺寸 离地高度
        /// Duct:系统缩写 截面尺寸 离地高度
        /// CableTray:类型名称 宽x高 离地高度
        /// </summary>
        SPL,
        /// <summary>
        /// System/Location
        /// Pipe:系统缩写 离地高度
        /// Duct:系统缩写 离地高度
        /// CableTray:类型名称 离地高度
        /// </summary>
        SL,
        /// <summary>
        /// Size/Location
        /// Pipe:管道尺寸 离地高度
        /// Duct:截面尺寸 离地高度
        /// CableTray:宽x高 离地高度
        /// </summary>
        PL,
        /// <summary>
        /// System/Size
        /// Duct:系统缩写 截面尺寸
        /// CableTray:类型名称 宽x高
        /// </summary>
        SP,
    }
    static class PAAAnnotationTypeEx
    {
        /// <summary>
        /// 获取 线族
        /// </summary>
        public static FamilySymbol GetLineFamily(this PAATextType type, Document doc)
        {
            switch (type)
            {
                case PAATextType.Option1:
                    return PAAContext.Get_MultipleLineOnLine(doc);
                case PAATextType.Option2:
                    return PAAContext.Get_MultipleLineOnEdge(doc);
                default:
                    throw new NotImplementedException("暂不支持该类型");
            }
        }
    }
    /// <summary>
    /// 离地模式
    /// </summary>
    public enum PAALocationType
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
    static class PAALocationTypeEx
    {
        /// <summary>
        /// 获取 标注的离地高度
        /// </summary>
        public static double GetLocationValue(this PAALocationType type, double offset, double diameter)
        {
            switch (type)
            {
                case PAALocationType.Center:
                    return offset + diameter / 2;
                case PAALocationType.Top:
                    return offset;
                case PAALocationType.Bottom:
                    return offset - diameter / 2;
                default:
                    throw new NotImplementedException("暂不支持该类型");
            }
        }
    }
    /// <summary>
    /// 文字方式
    /// </summary>
    public enum PAATextType
    {
        /// <summary>
        /// 文字在线上|位于风管中心|位于桥架中心
        /// </summary>
        Option1,
        /// <summary>
        /// 文字在线端|位于风管上方|位于桥架上方
        /// </summary>
        Option2,
    }
    static class PAATextTypeEx
    {
        /// <summary>
        /// 获取定位对应的横线宽度
        /// </summary>
        /// <param name="textType"></param>
        /// <returns></returns>
        public static double GetLineWidth(this PAATextType textType)
        {
            return textType == PAATextType.Option2 ? 200 : 1000;
        }
        /// <summary>
        /// 获取文本的定位
        /// </summary>
        /// <param name="textType">定位方式</param>
        /// <param name="currentFontHeight">字体高度</param>
        /// <param name="verticalFix">垂直修正</param>
        /// <param name="verticalVector">垂直向量</param>
        /// <param name="start">横线起点</param>
        /// <param name="end">横线终点</param>
        /// <returns></returns>
        public static XYZ GetTextLocation(this PAATextType textType, double currentFontHeight, XYZ verticalVector, XYZ start, XYZ end)
        {
            switch (textType)
            {
                case PAATextType.Option1:
                    return start + (currentFontHeight / 3) * verticalVector; //+ verticalFix) * verticalVector;
                case PAATextType.Option2:
                    return end;
                default:
                    throw new NotImplementedException("未支持系类定位方案");
            }
            //return textType == PAATextType.OnEdge ? end + (currentFontHeight / 2 + verticalFix) * verticalVector : start + (currentFontHeight + verticalFix) * verticalVector;
        }
    }

    public enum PAAModelType
    {
        SinglePipe,
        MultiplePipe,
        SingleDuct,
        SingleCableTray,
    }
    public enum RegenerateType
    {
        BySingle,
        ByMultipleTarget,
        ByMultipleLine,
    }

    public class PAAModel : VLModel
    {
        #region 需要存储
        public PAAModelType ModelType { set; get; }
        public RegenerateType RegenerateType { set; get; }
        public ElementId ViewId { set; get; }

        #region Single
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
        /// 单一标注 文本定位坐标
        /// </summary>
        public XYZ AnnotationLocation { set; get; }
        #endregion

        #region 多标注
        /// <summary>
        /// 多标注 目标对象
        /// </summary>
        public List<ElementId> TargetIds { set; get; }
        /// <summary>
        /// 多标注 线对象 
        /// </summary>
        public ElementId LineId { get; set; }
        /// <summary>
        /// 多标注 标注Id
        /// </summary>
        public List<ElementId> AnnotationIds { set; get; }
        #endregion

        #region 通用属性
        /// <summary>
        /// 标注对象
        /// </summary>
        public PAATargetType TargetType { set; get; }
        /// <summary>
        /// 标记样式
        /// </summary>
        public PAAAnnotationType AnnotationType { set; get; }
        /// <summary>
        /// 离地模式前缀
        /// </summary>
        public string AnnotationPrefix { set; get; }
        /// <summary>
        /// 离地模式
        /// </summary>
        public PAALocationType LocationType { set; get; }
        /// <summary>
        /// 文字方式
        /// </summary>
        public PAATextType TextType { set; get; }
        /// <summary>
        /// 目标定位, 单管以中点为准 多管以最上管道的中点为准
        /// </summary>
        public XYZ TargetLocation { get; set; }
        /// <summary>
        /// 当前文本大小比例 以毫米表示
        /// </summary>
        public double CurrentFontSizeScale { set; get; }

        /// <summary>
        /// 当前文本高度 double, foot
        /// </summary>
        public double CurrentFontHeight { set; get; }
        /// <summary>
        /// 当前文本 Revit中的宽度缩放比例 
        /// </summary>
        public double CurrentFontWidthSize { set; get; }
        ///// <summary>
        ///// 用以比对更新用的L数据
        ///// </summary>
        //public double LValue { get; set; }
        #endregion

        #endregion

        #region 非留存数据
        public bool IsReversed { set; get; }
        public bool IsRegenerate { set; get; }
        public Document Document { get; set; }
        /// <summary>
        /// 线宽
        /// </summary>
        public double LineWidth { get; set; }
        /// <summary>
        /// 间距
        /// </summary>
        public double LineSpace { get; private set; }
        /// <summary>
        /// 字体高度高度 由基础基础字体高度/4*FontScale得出
        /// </summary>
        public double LineHeight { set; get; }


        public XYZ ParallelVector = null;//坐标定位,平行于标注对象
        public XYZ VerticalVector = null;//坐标定位,垂直于标注对象
        public FamilySymbol GetAnnotationFamily(Document doc, ElementId targetId)
        {
            switch (AnnotationType)
            {
                case PAAAnnotationType.SPL:
                    switch (TargetType)
                    {
                        case PAATargetType.Pipe:
                            return PAAContext.GetSPLTag_Pipe(doc);
                        case PAATargetType.Duct:
                            if (IsRoundDuct(doc.GetElement(targetId)))
                                return PAAContext.GetSPLTag_Duct_Round(doc);
                            else
                                return PAAContext.GetSPLTag_Duct_Rectangle(doc);
                        case PAATargetType.CableTray:
                            return PAAContext.GetSPLTag_CableTray(doc);
                        case PAATargetType.Conduit:
                        default:
                            throw new NotImplementedException("暂不支持");
                    }
                case PAAAnnotationType.SL:
                    switch (TargetType)
                    {
                        case PAATargetType.Pipe:
                            return PAAContext.GetSLTag_Pipe(doc);
                        case PAATargetType.Duct:
                            return PAAContext.GetSLTag_Duct(doc);
                        case PAATargetType.CableTray:
                            return PAAContext.GetSLTag_CableTray(doc);
                        case PAATargetType.Conduit:
                        default:
                            throw new NotImplementedException("暂不支持");
                    }
                case PAAAnnotationType.PL:
                    switch (TargetType)
                    {
                        case PAATargetType.Pipe:
                            return PAAContext.GetPLTag_Pipe(doc);
                        case PAATargetType.Duct:
                            if (IsRoundDuct(doc.GetElement(targetId)))
                                return PAAContext.GetPLTag_Duct_Round(doc);
                            else
                                return PAAContext.GetPLTag_Duct_Rectangle(doc);
                        case PAATargetType.CableTray:
                            return PAAContext.GetPLTag_CableTray(doc);
                        case PAATargetType.Conduit:
                        default:
                            throw new NotImplementedException("暂不支持");
                    }
                case PAAAnnotationType.SP:
                    switch (TargetType)
                    {
                        case PAATargetType.Pipe:
                            throw new NotImplementedException("管道无该类型");
                        case PAATargetType.Duct:
                            if (IsRoundDuct(doc.GetElement(targetId)))
                                return PAAContext.GetSPTag_Duct_Round(doc);
                            else
                                return PAAContext.GetSPTag_Duct_Rectangle(doc);
                        case PAATargetType.CableTray:
                            return PAAContext.GetSPTag_CableTray(doc);
                        case PAATargetType.Conduit:
                        default:
                            throw new NotImplementedException("暂不支持");
                    }
                default:
                    throw new NotImplementedException("暂不支持该类型");
            }
        }

        private static bool IsRoundDuct(Element element)
        {
            return element.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == "圆形风管";
        }

        public FamilySymbol GetLineFamily(Document doc)
        {
            return TextType.GetLineFamily(doc);
        }
        public List<ElementAndNodePoint> PipeAndNodePoints { set; get; }
        #endregion


        internal bool CheckParallel(Document document)
        {
            var curves = TargetIds.Select(c => (document.GetElement(c).Location as LocationCurve).Curve as Line);
            XYZ verticalVector = curves.First().Direction;
            foreach (var curve in curves)
            {
                var crossProduct = curve.Direction.CrossProduct(verticalVector);
                if (!crossProduct.X.IsMiniValue() || !crossProduct.Y.IsMiniValue())
                    return false;
            }
            return true;
        }

        public void UpdateVectors(Line locationCurve)
        {
            XYZ parallelVector = null;
            XYZ verticalVector = null;
            parallelVector = locationCurve.Direction;
            verticalVector = VLLocationHelper.GetVerticalVector(parallelVector, CoordinateType.XY);
            parallelVector = VLLocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
            verticalVector = VLLocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
            if ((verticalVector.X - 1).IsMiniValue())
                verticalVector = verticalVector.RevertByCoordinateType(CoordinateType.XY);
            VerticalVector = verticalVector;
            ParallelVector = parallelVector;
        }

        public PAAModel() : base("")
        {
        }
        public PAAModel(string data) : base(data)
        {
        }

        public override bool LoadData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return false;
            try
            {
                StringReader sr = new StringReader(data);
                TargetType = sr.ReadFormatStringAsEnum<PAATargetType>();
                ModelType = sr.ReadFormatStringAsEnum<PAAModelType>();
                ViewId = sr.ReadFormatStringAsElementId();
                if (TargetType == PAATargetType.Pipe)
                {
                    switch (ModelType)
                    {
                        case PAAModelType.SinglePipe:
                            TargetId = sr.ReadFormatStringAsElementId();
                            LineIds = sr.ReadFormatStringAsElementIds();
                            AnnotationId = sr.ReadFormatStringAsElementId();
                            BodyEndPoint = sr.ReadFormatStringAsXYZ();
                            BodyStartPoint = sr.ReadFormatStringAsXYZ();
                            LeafEndPoint = sr.ReadFormatStringAsXYZ();
                            AnnotationLocation = sr.ReadFormatStringAsXYZ();
                            break;
                        case PAAModelType.MultiplePipe:
                            TargetIds = sr.ReadFormatStringAsElementIds();
                            LineId = sr.ReadFormatStringAsElementId();
                            AnnotationIds = sr.ReadFormatStringAsElementIds();
                            break;
                    }
                    TargetLocation = sr.ReadFormatStringAsXYZ();
                }
                else
                {
                    TargetId = sr.ReadFormatStringAsElementId();
                    AnnotationId = sr.ReadFormatStringAsElementId();
                }
                AnnotationType = sr.ReadFormatStringAsEnum<PAAAnnotationType>();
                AnnotationPrefix = sr.ReadFormatString();
                LocationType = sr.ReadFormatStringAsEnum<PAALocationType>();
                TextType = sr.ReadFormatStringAsEnum<PAATextType>();
                CurrentFontSizeScale = sr.ReadFormatStringAsDouble();
                CurrentFontHeight = sr.ReadFormatStringAsDouble();
                CurrentFontWidthSize = sr.ReadFormatStringAsDouble();
                //LValue = sr.ReadFormatStringAsDouble();
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
            sb.AppendItem(TargetType);
            sb.AppendItem(ModelType);
            sb.AppendItem(ViewId);
            if (TargetType == PAATargetType.Pipe)
            {
                switch (ModelType)
                {
                    case PAAModelType.SinglePipe:
                        sb.AppendItem(TargetId);
                        sb.AppendItem(LineIds);
                        sb.AppendItem(AnnotationId);
                        sb.AppendItem(BodyEndPoint);
                        sb.AppendItem(BodyStartPoint);
                        sb.AppendItem(LeafEndPoint);
                        sb.AppendItem(AnnotationLocation);
                        break;
                    case PAAModelType.MultiplePipe:
                        sb.AppendItem(TargetIds);
                        sb.AppendItem(LineId);
                        sb.AppendItem(AnnotationIds);
                        break;
                }
                sb.AppendItem(TargetLocation);
            }
            else
            {
                sb.AppendItem(TargetId);
                sb.AppendItem(AnnotationId);
            }
            sb.AppendItem(AnnotationType);
            sb.AppendItem(AnnotationPrefix);
            sb.AppendItem(LocationType);
            sb.AppendItem(TextType);
            sb.AppendItem(CurrentFontSizeScale);
            sb.AppendItem(CurrentFontHeight);
            sb.AppendItem(CurrentFontWidthSize);
            //sb.AppendItem(LValue);
            return sb.ToData();
        }
        public ISelectionFilter GetFilter()
        {
            switch (TargetType)
            {
                case PAATargetType.Pipe:
                    return new VLClassFilter(typeof(Pipe));
                case PAATargetType.Duct:
                    return new VLClassFilter(typeof(Duct));
                case PAATargetType.CableTray:
                    return new VLClassFilter(typeof(CableTray));
                case PAATargetType.Conduit:
                    return new VLClassFilter(typeof(Conduit));
                default:
                    throw new NotImplementedException("未支持该类型的过滤:" + TargetType.ToString());
            }
        }

        public bool CalculateLocations()
        {
            switch (ModelType)
            {
                case PAAModelType.SinglePipe:
                    #region single
                    var target = Document.GetElement(TargetId);
                    var locationCurve = TargetType.GetLine(target);
                    UpdateVectors(locationCurve);
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
                    if (TextType == PAATextType.Option2)
                    {
                        if (!IsRegenerate)
                            LeafEndPoint = BodyEndPoint + LineWidth * ParallelVector;
                        //文本位置 start:(附着元素中点+线基本高度+文本高度*(文本个数-1))  end: start+宽
                        //高度,宽度 取决于文本 
                        AnnotationLocation = TextType.GetTextLocation(CurrentFontHeight, VerticalVector, BodyEndPoint, LeafEndPoint);
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
                            AnnotationLocation = TextType.GetTextLocation(CurrentFontHeight, VerticalVector, LeafEndPoint, BodyEndPoint);
                        else
                            AnnotationLocation = TextType.GetTextLocation(CurrentFontHeight, VerticalVector, BodyEndPoint, LeafEndPoint);
                    }
                    return true;
                #endregion
                case PAAModelType.MultiplePipe:
                    #region multiple
                    PipeAndNodePoints = new List<ElementAndNodePoint>();
                    if (IsRegenerate)
                    {
                        for (int i = 0; i < TargetIds.Count; i++)
                        {
                            target = Document.GetElement(TargetIds[i]);
                            var annotationId = AnnotationIds[i];
                            var annotation = Document.GetElement(annotationId);
                            if (annotation == null)
                                return false;
                            PipeAndNodePoints.Add(new ElementAndNodePoint(target, annotation as IndependentTag));
                        }
                    }
                    else
                    {
                        foreach (var selectedId in TargetIds)
                            PipeAndNodePoints.Add(new ElementAndNodePoint(Document.GetElement(selectedId)));
                    }
                    //平行,垂直 向量
                    UpdateVectors(PipeAndNodePoints.First().Line);
                    //重叠区间
                    XYZ rightOfLefts;
                    XYZ leftOfRights;
                    XYZ startPoint = TargetLocation;
                    List<XYZ> lefts = new List<XYZ>();
                    List<XYZ> rights = new List<XYZ>();
                    if (!IsRegenerate)
                    {
                        bool usingX = GetLeftsAndRights(PipeAndNodePoints, lefts, rights);
                        rightOfLefts = usingX ? lefts.First(c => c.X == lefts.Max(p => p.X)) : lefts.First(c => c.Y == lefts.Max(p => p.Y));
                        leftOfRights = usingX ? rights.First(c => c.X == rights.Min(p => p.X)) : rights.First(c => c.Y == rights.Min(p => p.Y));
                        if ((usingX && rightOfLefts.X > leftOfRights.X) || (!usingX && rightOfLefts.Y > leftOfRights.Y))
                        {
                            //TODO 提示无重叠区间
                            return false;
                        }
                    }
                    else
                    {
                        rightOfLefts = leftOfRights = null;
                    }
                    //节点计算
                    XYZ firstNode;
                    if (!IsRegenerate)
                    {
                        firstNode = (rightOfLefts + leftOfRights) / 2;
                    }
                    else
                    {
                        locationCurve = PipeAndNodePoints[0].Line;
                        firstNode = locationCurve.Project(startPoint).XYZPoint;
                    }
                    PipeAndNodePoints[0].NodePoint = firstNode;
                    //节点位置
                    for (int i = 1; i < PipeAndNodePoints.Count(); i++)
                        PipeAndNodePoints[i].NodePoint = PipeAndNodePoints[i].Line.Project(PipeAndNodePoints[0].NodePoint).XYZPoint;
                    //排序
                    if (PipeAndNodePoints.Count() > 1)
                    {
                        if (Math.Abs(PipeAndNodePoints[0].NodePoint.Y - PipeAndNodePoints[1].NodePoint.Y) < 0.01)
                            PipeAndNodePoints = PipeAndNodePoints.OrderBy(c => c.NodePoint.X).ToList();
                        else
                            PipeAndNodePoints = PipeAndNodePoints.OrderByDescending(c => c.NodePoint.Y).ToList();
                    }
                    TargetIds = PipeAndNodePoints.Select(c => c.Target.Id).ToList();
                    //标注定位计算
                    XYZ offset = GetOffSet();
                    bool overMoved = false;//位移是否超过的最低限制
                    double parallelSkew = offset.GetLengthBySide(ParallelVector);
                    foreach (var PipeAndNodePoint in PipeAndNodePoints)
                    {
                        PipeAndNodePoint.NodePoint += parallelSkew * ParallelVector;
                    }
                    TargetLocation = PipeAndNodePoints.First().NodePoint;
                    if (IsRegenerate)// && regenerateType != RegenerateType.RegenerateByPipe)
                    {
                        //原始线高度+偏移数据
                        var line = Document.GetElement(LineId);
                        var orientLineHeight = IsRegenerate ? line.GetParameters(TagProperty.线高度1.ToString()).First().AsDouble() : 0;
                        var verticalSkew = RegenerateType == RegenerateType.ByMultipleTarget ? 0 : VLLocationHelper.GetLengthBySide(offset, VerticalVector);
                        //if (Math.Abs(VerticalVector.X) > 1 - UnitHelper.MiniValueForXYZ)
                        //    verticalSkew = -verticalSkew;
                        var nodesHeight = UnitHelper.ConvertToFoot((PipeAndNodePoints.Count() - 1) * CurrentFontHeight, VLUnitType.millimeter);
                        overMoved = orientLineHeight + verticalSkew < nodesHeight;
                        var lineHeight = orientLineHeight + verticalSkew;
                        if (overMoved)
                        {
                            lineHeight = nodesHeight;
                            verticalSkew = nodesHeight - orientLineHeight;
                        }
                        LineHeight = lineHeight;
                    }
                    else
                    {
                        LineHeight = CurrentFontHeight;
                    }
                    LineSpace = CurrentFontHeight;
                    UpdateLineWidth(Document.GetElement(TargetIds.First()));
                    //string text = GetFullTextForLine(Document.GetElement(TargetIds.First()));
                    //var textWidth = TextRenderer.MeasureText(text, PAAContext.FontManagement.OrientFont).Width;
                    //LineWidth = textWidth * PAAContext.FontManagement.CurrentFontWidthSize;
                    //标注位置
                    for (int i = 0; i < PipeAndNodePoints.Count(); i++)
                    {
                        var start = TargetLocation + (LineHeight + i * LineSpace) * VerticalVector;
                        var end = start + LineWidth * ParallelVector;
                        PipeAndNodePoints[PipeAndNodePoints.Count() - 1 - i].AnnotationPoint = TextType.GetTextLocation(CurrentFontHeight, VerticalVector, start, end);
                    }
                    #endregion
                    return true;
                default:
                    throw new NotImplementedException("未支持该类型的生成");
            }
        }

        private XYZ GetOffSet()
        {
            switch (RegenerateType)
            {
                case RegenerateType.ByMultipleTarget:
                    return IsRegenerate ? PipeAndNodePoints.First().NodePoint - TargetLocation : new XYZ(0, 0, 0);
                case RegenerateType.ByMultipleLine:
                    return IsRegenerate ? (Document.GetElement(LineId).Location as LocationPoint).Point - TargetLocation : new XYZ(0, 0, 0);
                case RegenerateType.BySingle:
                    return new XYZ(0, 0, 0);
                default:
                    throw new NotImplementedException("未实现该类型");
            }
        }

        public static bool GetLeftsAndRights(List<ElementAndNodePoint> pipes, List<XYZ> lefts, List<XYZ> rights)
        {
            bool usingX = Math.Abs(pipes[0].Line.GetEndPoint(0).X - (pipes[0].Line.GetEndPoint(1).X)) > 0.01;
            var firstCurve = pipes.First().Line;
            firstCurve.MakeUnbound();
            for (int i = 1; i < pipes.Count(); i++)
            {
                var line = pipes[i].Line;
                double p1Location, p2Location;
                if (usingX)
                {
                    p1Location = line.GetEndPoint(0).X;
                    p2Location = line.GetEndPoint(1).X;
                }
                else
                {
                    p1Location = line.GetEndPoint(0).Y;
                    p2Location = line.GetEndPoint(1).Y;
                }
                if (i == 0)
                {
                    if (p1Location < p2Location)
                    {
                        lefts.Add(line.GetEndPoint(0));
                        rights.Add(line.GetEndPoint(1));
                    }
                    else
                    {
                        lefts.Add(line.GetEndPoint(1));
                        rights.Add(line.GetEndPoint(0));
                    }
                }
                else
                {
                    if (p1Location < p2Location)
                    {
                        lefts.Add(firstCurve.Project(line.GetEndPoint(0)).XYZPoint);
                        rights.Add(firstCurve.Project(line.GetEndPoint(1)).XYZPoint);
                    }
                    else
                    {
                        lefts.Add(firstCurve.Project(line.GetEndPoint(1)).XYZPoint);
                        rights.Add(firstCurve.Project(line.GetEndPoint(0)).XYZPoint);
                    }
                }
            }
            return usingX;
        }

        public void Clear()
        {
            Document doc = Document;
            if (TargetType != PAATargetType.Pipe)
            {
                if (doc.GetElement(AnnotationId) != null)
                    doc.Delete(AnnotationId);
                return;
            }
            switch (ModelType)
            {
                case PAAModelType.SinglePipe:
                    //删除线
                    foreach (var item in LineIds)
                        if (doc.GetElement(item) != null)
                            doc.Delete(item);
                    //删除标注
                    if (doc.GetElement(AnnotationId) != null)
                        doc.Delete(AnnotationId);
                    break;
                case PAAModelType.MultiplePipe:
                    //清理线族
                    if (doc.GetElement(LineId) != null)
                        doc.Delete(LineId);
                    //清理标注
                    foreach (var item in AnnotationIds)
                        if (doc.GetElement(item) != null)
                            doc.Delete(item);
                    break;
                default:
                    break;
            }
        }

        internal string GetFullTextForLine(Element target)
        {
            string system = GetSystem(target);
            string size = GetSize(target);
            string location = GetFull_L(target);//target.GetParameters(PAAContext.SharedParameterPL).First().AsString();
            //double location = UnitHelper.ConvertFromFootTo(target.GetParameters(PAAContext.SharedParameterOffset).First().AsDouble(), VLUnitType.millimeter);
            switch (AnnotationType)
            {
                case PAAAnnotationType.SPL:
                    return string.Format("{0} {1} {2}", system, size, location);
                case PAAAnnotationType.SL:
                    return string.Format("{0} {1}", system, location);
                case PAAAnnotationType.PL:
                    return string.Format("{0} {1}", size, location);
                case PAAAnnotationType.SP:
                    return string.Format("{0} {1}", system, size);
                default:
                    throw new NotImplementedException("不支持该类型的处理");
            }
        }

        private string GetSystem(Element target)
        {
            switch (TargetType)
            {
                case PAATargetType.Pipe:
                case PAATargetType.Duct:
                    return target.GetParameters(PAAContext.SharedParameterSystemAbbreviation).First().AsString();
                case PAATargetType.CableTray:
                    return target.Name;
                //return target.GetParameters(PAAContext.SharedParameterTypeName).First().AsString();
                case PAATargetType.Conduit:
                default:
                    throw new NotImplementedException("暂未支持该类型");
            }
        }

        private string GetSize(Element target)
        {
            switch (TargetType)
            {
                case PAATargetType.Pipe:
                    var size = UnitHelper.ConvertFromFootTo(target.GetParameters(PAAContext.SharedParameterDiameter).First().AsDouble(), VLUnitType.millimeter);
                    return "DN" + size;
                case PAATargetType.Duct:
                    if (IsRoundDuct(target))
                    {
                        size = UnitHelper.ConvertFromFootTo(target.GetParameters(PAAContext.SharedParameterDiameter).First().AsDouble(), VLUnitType.millimeter);
                        return "Φ" + size;
                    }
                    else
                    {
                        var height = UnitHelper.ConvertFromFootTo(target.GetParameters(PAAContext.SharedParameterHeight).First().AsDouble(), VLUnitType.millimeter);
                        var width = UnitHelper.ConvertFromFootTo(target.GetParameters(PAAContext.SharedParameterWidth).First().AsDouble(), VLUnitType.millimeter);
                        return width + "x" + height;
                    }
                case PAATargetType.CableTray:
                    size = UnitHelper.ConvertFromFootTo(target.GetParameters(PAAContext.SharedParameterHeight).First().AsDouble(), VLUnitType.millimeter);
                    return size + "x" + size;
                case PAATargetType.Conduit:
                default:
                    throw new NotImplementedException("暂未支持该类型");
            }
        }

        private double GetDN(Element target)
        {
            switch (TargetType)
            {
                case PAATargetType.Pipe:
                    return target.GetParameters(PAAContext.SharedParameterDiameter).First().AsDouble();
                case PAATargetType.Duct:
                    if (IsRoundDuct(target))
                        return target.GetParameters(PAAContext.SharedParameterDiameter).First().AsDouble();
                    else
                        return target.GetParameters(PAAContext.SharedParameterHeight).First().AsDouble();
                case PAATargetType.CableTray:
                    return target.GetParameters(PAAContext.SharedParameterHeight).First().AsDouble();
                case PAATargetType.Conduit:
                default:
                    throw new NotImplementedException("暂未支持该类型");
            }
        }

        internal string GetFull_L(Element target)
        {
            var offset = target.get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM).AsDouble();//target.GetParameters(PAAContext.SharedParameterOffset).First().AsDouble();
            switch (LocationType)
            {
                case PAALocationType.Center:
                    break;
                case PAALocationType.Top:
                    var DN = GetDN(target);
                    offset += DN / 2;
                    break;
                case PAALocationType.Bottom:
                    DN = GetDN(target);
                    offset -= DN / 2;
                    break;
                default:
                    break;
            }
            return AnnotationPrefix + UnitHelper.ConvertFromFootTo(offset, VLUnitType.millimeter).ToString("f1").TrimEnd(".0");
        }

        internal void UpdateLineWidth(Element target)
        {
            switch (TextType)
            {
                case PAATextType.Option1:
                    string text = GetFullTextForLine(target);
                    string preFix = " ";
                    var textWidth = TextRenderer.MeasureText(preFix + text, PAAContext.FontManagement.OrientFont).Width;
                    LineWidth = textWidth * CurrentFontWidthSize;
                    break;
                case PAATextType.Option2:
                    switch (TargetType)
                    {
                        case PAATargetType.Pipe:
                            LineWidth = 10 * CurrentFontWidthSize;
                            break;
                        case PAATargetType.Duct:
                        case PAATargetType.CableTray:
                            text = GetFullTextForLine(target);
                            textWidth = TextRenderer.MeasureText(text, PAAContext.FontManagement.OrientFont).Width;
                            LineWidth = textWidth * CurrentFontWidthSize;
                            break;
                        case PAATargetType.Conduit:
                        default:
                            throw new NotImplementedException();
                    }
                    break;
            }
        }

        internal string GetPreview(PAAAnnotationType annotationType)
        {
            switch (TargetType)
            {
                case PAATargetType.Pipe:
                    switch (annotationType)
                    {
                        case PAAAnnotationType.SPL:
                            return string.Format("如:ZP DN100 {0}2600", AnnotationPrefix);
                        case PAAAnnotationType.SL:
                            return string.Format("如:ZP {0}2600", AnnotationPrefix);
                        case PAAAnnotationType.PL:
                            return string.Format("如:DN100 {0}2600", AnnotationPrefix);
                        case PAAAnnotationType.SP:
                            return "管道无该类型选项";
                        default:
                            throw new NotImplementedException("未支持该类型的");
                    }
                case PAATargetType.Duct:
                    switch (annotationType)
                    {
                        case PAAAnnotationType.SPL:
                            return string.Format("如:SF 400x400 {0}2600", AnnotationPrefix);
                        case PAAAnnotationType.SL:
                            return string.Format("如:SF {0}2600", AnnotationPrefix);
                        case PAAAnnotationType.PL:
                            return string.Format("如:400x400 {0}2600", AnnotationPrefix);
                        case PAAAnnotationType.SP:
                            return string.Format("如:SF 400x400");
                        default:
                            throw new NotImplementedException("未支持该类型的");
                    }
                case PAATargetType.CableTray:
                    switch (annotationType)
                    {
                        case PAAAnnotationType.SPL:
                            return string.Format("如:槽式桥架 400x400 {0}2600", AnnotationPrefix);
                        case PAAAnnotationType.SL:
                            return string.Format("如:槽式桥架 {0}2600", AnnotationPrefix);
                        case PAAAnnotationType.PL:
                            return string.Format("如:400x400 {0}2600", AnnotationPrefix);
                        case PAAAnnotationType.SP:
                            return string.Format("如:槽式桥架 400x400");
                        default:
                            throw new NotImplementedException("未支持该类型的");
                    }
                case PAATargetType.Conduit:
                default:
                    throw new NotImplementedException("未支持该类型的");
            }
        }

        internal string GetTitle(PAAAnnotationType annotationType)
        {
            switch (TargetType)
            {
                case PAATargetType.Pipe:
                    switch (annotationType)
                    {
                        case PAAAnnotationType.SPL:
                            return "系统缩写 管道尺寸 离地高度";
                        case PAAAnnotationType.SL:
                            return "系统缩写 离地高度";
                        case PAAAnnotationType.PL:
                            return "管道尺寸 离地高度";
                        case PAAAnnotationType.SP:
                            return "管道无该类型选项";
                        default:
                            throw new NotImplementedException("未支持该类型的");
                    }
                case PAATargetType.Duct:
                    switch (annotationType)
                    {
                        case PAAAnnotationType.SPL:
                            return "系统缩写 截面尺寸 离地高度";
                        case PAAAnnotationType.SL:
                            return "系统缩写 离地高度";
                        case PAAAnnotationType.PL:
                            return "截面尺寸 离地高度";
                        case PAAAnnotationType.SP:
                            return "系统缩写 截面尺寸";
                        default:
                            throw new NotImplementedException("未支持该类型的");
                    }
                case PAATargetType.CableTray:
                    switch (annotationType)
                    {
                        case PAAAnnotationType.SPL:
                            return "类型名称 宽x高 离地高度";
                        case PAAAnnotationType.SL:
                            return "类型名称 离地高度";
                        case PAAAnnotationType.PL:
                            return "宽x高 离地高度";
                        case PAAAnnotationType.SP:
                            return "类型名称 宽x高";
                        default:
                            throw new NotImplementedException("未支持该类型的");
                    }
                case PAATargetType.Conduit:
                default:
                    throw new NotImplementedException("未支持该类型的");
            }
        }
    }
}
