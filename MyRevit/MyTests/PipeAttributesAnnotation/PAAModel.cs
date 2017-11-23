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
using System.Text;

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
                    return (element.Location as LocationCurve).Curve as Line;
                case PAATargetType.CableTray:
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
            PmSoft.Optimization.DrawingProduction.Utils.GraphicsDisplayerManager.Display(@"E:\WorkingSpace\Outputs\Images\1028轮廓.png", faces);
            return topLine;
        }
    }
    /// <summary>
    /// 标记样式
    /// </summary>
    public enum PAAAnnotationType
    {
        /// <summary>
        /// 系统缩写 管道尺寸 离地高度
        /// </summary>
        SPL,
        /// <summary>
        /// 系统缩写 离地高度
        /// </summary>
        SL,
        /// <summary>
        /// 管道尺寸 离地高度
        /// </summary>
        PL,
    }
    static class PAAAnnotationTypeEx
    {
        /// <summary>
        /// 获取
        /// </summary>
        public static FamilySymbol GetTagFamily(this PAAAnnotationType type,Document doc)
        {
            switch (type)
            {
                case PAAAnnotationType.SPL:
                    return PAAContext.GetSPLTag(doc);
                case PAAAnnotationType.SL:
                    return PAAContext.GetSLTag(doc);
                case PAAAnnotationType.PL:
                    return PAAContext.GetPLTag(doc);
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
    /// <summary>
    /// 文字方式
    /// </summary>
    public enum PAATextType
    {
        /// <summary>
        /// 文字在线上
        /// </summary>
        OnLine,
        /// <summary>
        /// 文字在线端
        /// </summary>
        OnEdge,
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
            return textType == PAATextType.OnEdge ? 200 : 1000;
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
        public static XYZ GetTextLocation(this PAATextType textType, double currentFontHeight, double verticalFix, XYZ verticalVector, XYZ start, XYZ end)
        {
            switch (textType)
            {
                case PAATextType.OnLine:
                    return  start + (currentFontHeight / 2) * verticalVector; //+ verticalFix) * verticalVector;
                case PAATextType.OnEdge:
                    return end;
                default:
                    throw new NotImplementedException("未支持系类定位方案");
            }
            //return textType == PAATextType.OnEdge ? end + (currentFontHeight / 2 + verticalFix) * verticalVector : start + (currentFontHeight + verticalFix) * verticalVector;
        }
    }

    public class PAAModelForSingle : VLModel
    {
        #region 需要存储
        public PAAAnnotationType AnnotationType { set; get; }//标记样式
        public string AnnotationPrefix { set; get; }//离地模式前缀
        public PAALocationType LocationType { set; get; }//离地模式
        public PAATextType TextType { set; get; }//文字方式
        public ElementId TargetId { set; get; }//目标对象
        public List<ElementId> LineIds { get; set; }//线对象
        public ElementId GroupId { get; set; }//线组对象
        public ElementId AnnotationId { set; get; }//标注
        public XYZ BodyEndPoint { get; set; }//干线终点
        public XYZ BodyStartPoint { get; set; }//干线起点
        public XYZ LeafEndPoint { set; get; }//支线终点
        public XYZ TextLocation { set; get; }//文本定位坐标
        public double CurrentFontSizeScale { set; get; }//当前文本大小比例 以毫米表示
        public double CurrentFontHeight { set; get; }//当前文本高度 double, foot
        public double CurrentFontWidthScale { set; get; }//当前文本 Revit中的宽度缩放比例
        #endregion

        public PAATargetType TargetType { set; get; }//标注对象
        public XYZ ParallelVector = null;//坐标定位,平行于标注对象
        public XYZ VerticalVector = null;//坐标定位,垂直于标注对象

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
                verticalVector = new XYZ(-verticalVector.X, -verticalVector.Y, verticalVector.Z);
            VerticalVector = verticalVector;
            ParallelVector = parallelVector;
        }

        public PAAModelForSingle() : base("")
        {
        }
        public PAAModelForSingle(string data) : base(data)
        {
        }

        public override bool LoadData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return false;
            try
            {
                StringReader sr = new StringReader(data);
                AnnotationType = sr.ReadFormatStringAsEnum<PAAAnnotationType>();
                AnnotationPrefix = sr.ReadFormatString();
                LocationType = sr.ReadFormatStringAsEnum<PAALocationType>();
                TextType = sr.ReadFormatStringAsEnum<PAATextType>();
                TargetId = sr.ReadFormatStringAsElementId();
                LineIds = sr.ReadFormatStringAsElementIds();
                GroupId = sr.ReadFormatStringAsElementId();
                AnnotationId = sr.ReadFormatStringAsElementId();
                BodyEndPoint = sr.ReadFormatStringAsXYZ();
                BodyStartPoint = sr.ReadFormatStringAsXYZ();
                LeafEndPoint = sr.ReadFormatStringAsXYZ();
                TextLocation = sr.ReadFormatStringAsXYZ();
                CurrentFontSizeScale = sr.ReadFormatStringAsDouble();
                CurrentFontHeight = sr.ReadFormatStringAsDouble();
                CurrentFontWidthScale = sr.ReadFormatStringAsDouble();
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
            sb.AppendItem(AnnotationType);
            sb.AppendItem(AnnotationPrefix);
            sb.AppendItem(LocationType);
            sb.AppendItem(TextType);
            sb.AppendItem(TargetId);
            sb.AppendItem(LineIds);
            sb.AppendItem(GroupId);
            sb.AppendItem(AnnotationId);
            sb.AppendItem(BodyEndPoint);
            sb.AppendItem(BodyStartPoint);
            sb.AppendItem(LeafEndPoint);
            sb.AppendItem(TextLocation);
            sb.AppendItem(CurrentFontSizeScale);
            sb.AppendItem(CurrentFontHeight);
            sb.AppendItem(CurrentFontWidthScale);
            return sb.ToData();
        }
        public ISelectionFilter GetFilter()
        {
            switch (TargetType)
            {
                case PAATargetType.Pipe:
                    return new ClassFilter(typeof(Pipe));
                case PAATargetType.Duct:
                    return new ClassFilter(typeof(Duct));
                case PAATargetType.CableTray:
                    return new ClassFilter(typeof(CableTray));
                case PAATargetType.Conduit:
                    return new ClassFilter(typeof(Conduit));
                default:
                    throw new NotImplementedException("未支持该类型的过滤:" + TargetType.ToString());
            }
        }

        public void CalculateLocations(Element element, XYZ offset)
        {
            var scale = 1 / PAAContext.FontManagement.OrientFontSizeScale * CurrentFontSizeScale;
            var width = TextType.GetLineWidth() * scale;
            var height = 400 * scale;
            var widthFoot = UnitHelper.ConvertToFoot(width, VLUnitType.millimeter);
            var heightFoot = UnitHelper.ConvertToFoot(height, VLUnitType.millimeter);
            var verticalFix = UnitHelper.ConvertToFoot(100, VLUnitType.millimeter) * scale;
            var locationCurve = TargetType.GetLine(element);
            UpdateVectors(locationCurve);
            //干线起始点 
            if (BodyStartPoint!=null)
            {
                var line = Line.CreateUnbound(BodyStartPoint, BodyEndPoint);
                IntersectionResultArray result;
                var setResult = line.Intersect(locationCurve,out result);
                if (result.Size > 0)//相交
                {
                    BodyStartPoint = result.get_Item(0).XYZPoint;
                }
                else { } //否则不改变原始坐标,仅重置
            }
            else
                BodyStartPoint = (locationCurve.GetEndPoint(0) + locationCurve.GetEndPoint(1)) / 2;
            //支线终点
            LeafEndPoint= BodyEndPoint + widthFoot * ParallelVector;
            //文本位置 start:(附着元素中点+线基本高度+文本高度*(文本个数-1))  end: start+宽
            //高度,宽度 取决于文本 
            TextLocation = TextType.GetTextLocation(CurrentFontHeight, verticalFix, VerticalVector, BodyEndPoint, LeafEndPoint);
        }
    }
}
