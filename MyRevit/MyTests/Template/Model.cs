using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI.Selection;
using MyRevit.MyTests.Utilities;
using MyRevit.MyTests.VLBase;
using System;
using System.Collections.Generic;

namespace MyRevit.MyTests.Template
{
    /// <summary>
    /// 标注对象
    /// </summary>
    public enum TemplateTargetType
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
    /// <summary>
    /// 标记样式
    /// </summary>
    public enum TemplateAnnotationType
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
    /// <summary>
    /// 离地模式
    /// </summary>
    public enum TemplateLocationType
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
    public enum TemplateTextType
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

    public class TemplateModel : VLModel
    {
        public TemplateTargetType TargetType { set; get; }//标注对象
        public TemplateAnnotationType AnnotationType { set; get; }//标记样式
        public TemplateLocationType LocationType { set; get; }//离地模式
        public TemplateTextType TextType { set; get; }//文字方式

        public List<ElementId> TargetIds { set; get; }//标记的目标对象
        public string AnnotationPrefix { get; internal set; }//标注前缀

        public TemplateModel() : base("")
        {
        }
        public TemplateModel(string data) : base(data)
        {
        }

        public override bool LoadData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return false;
            //try
            //{
            //    StringReader sr = new StringReader(data);
            //    TargetId = sr.ReadFormatStringAsElementId();
            //    LineIds = sr.ReadFormatStringAsElementIds();
            //    TextNoteIds = sr.ReadFormatStringAsElementIds();
            //    TextNoteTypeElementId = sr.ReadFormatStringAsElementId();
            //    CSALocationType = sr.ReadFormatStringAsEnum<CSALocationType>();
            //    TextLocations = sr.ReadFormatStringAsXYZs();
            //    Texts = sr.ReadFormatStringAsStrings();
            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    //TODO log
            //    return false;
            //}
            return true;
        }

        public override string ToData()
        {
            //public PAAAnnotationType AnnotationType { set; get; }//标记样式
            //public string AnnotationPrefix { set; get; }//离地模式前缀
            //public PAALocationType LocationType { set; get; }//离地模式
            //public PAATextType TextType { set; get; }//文字方式
            //public List<ElementId> LineIds { get; set; }//线对象
            //public ElementId GroupId { get; set; }//线组对象
            //public XYZ BodyEndPoint { get; set; }//干线终点
            //public XYZ BodyStartPoint { get; set; }//干线起点
            //public XYZ LeafEndPoint { set; get; }//支线终点
            //public XYZ TextLocation { set; get; }//文本定位坐标
            //public double CurrentFontSizeScale { set; get; }//当前文本大小比例 以毫米表示
            //public double CurrentFontHeight { set; get; }//当前文本高度 double, foot
            //public double CurrentFontWidthScale { set; get; }//当前文本 Revit中的宽度缩放比例


            //public ([\w<>]+) (\w+) .+
            //=>
            //sb.AppendItem($2);


            return "";
            //StringBuilder sb = new StringBuilder();
            //sb.AppendItem(TargetId);
            //sb.AppendItem(LineIds);
            //sb.AppendItem(TextNoteIds);
            //sb.AppendItem(TextNoteTypeElementId);
            //sb.AppendItem(CSALocationType);
            //sb.AppendItem(TextLocations);
            //sb.AppendItem(Texts);
            //return sb.ToData();
        }

        public ISelectionFilter GetFilter()
        {
            switch (TargetType)
            {
                case TemplateTargetType.Pipe:
                    return new ClassFilter(typeof(Pipe));
                case TemplateTargetType.Duct:
                    return new ClassFilter(typeof(Duct));
                case TemplateTargetType.CableTray:
                    return new ClassFilter(typeof(CableTray));
                case TemplateTargetType.Conduit:
                    return new ClassFilter(typeof(Conduit));
                default:
                    throw new NotImplementedException("未支持该类型的过滤:" + TargetType.ToString());
            }
        }

        internal string GetPreview()
        {
            switch (TargetType)
            {
                case TemplateTargetType.Pipe:
                    switch (AnnotationType)
                    {
                        case TemplateAnnotationType.SPL:
                            return string.Format("如:ZP DN100 {0}2600", AnnotationPrefix);
                        case TemplateAnnotationType.SL:
                            return string.Format("如:ZP {0}2600", AnnotationPrefix);
                        case TemplateAnnotationType.PL:
                            return string.Format("如:DN100 {0}2600", AnnotationPrefix);
                        default:
                            throw new NotImplementedException("未支持该类型的");
                    }
                case TemplateTargetType.Duct:
                    switch (AnnotationType)
                    {
                        case TemplateAnnotationType.SPL:
                            return string.Format("如:SF 400mmx400mm {0}2600", AnnotationPrefix);
                        case TemplateAnnotationType.SL:
                            return string.Format("如:SF {0}2600", AnnotationPrefix);
                        case TemplateAnnotationType.PL:
                            return string.Format("如:400mmx400mm {0}2600", AnnotationPrefix);
                        default:
                            throw new NotImplementedException("未支持该类型的");
                    }
                case TemplateTargetType.CableTray:
                    switch (AnnotationType)
                    {
                        case TemplateAnnotationType.SPL:
                            return string.Format("如:ZP DN100 {0}2600", AnnotationPrefix);
                        case TemplateAnnotationType.SL:
                            return string.Format("如:ZP {0}2600", AnnotationPrefix);
                        case TemplateAnnotationType.PL:
                            return string.Format("如:DN100 {0}2600", AnnotationPrefix);
                        default:
                            throw new NotImplementedException("未支持该类型的");
                    }
                case TemplateTargetType.Conduit:
                default:
                    throw new NotImplementedException("未支持该类型的");
            }
        }
    }
}
