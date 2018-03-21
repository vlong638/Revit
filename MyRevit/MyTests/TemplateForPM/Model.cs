using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI.Selection;
using PmSoft.Common.RevitClass.VLUtils;
using System;
using System.Collections.Generic;

namespace PMSoft.ConstructionManagementV2
{
    /// <summary>
    /// 标注对象
    /// </summary>
    public enum CMTargetType
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
    public enum CMAnnotationType
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
    public enum CMLocationType
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
    public enum CMTextType
    {
        /// <summary>
        /// 文字在线上
        /// </summary>
        TextType_OnLineOrOnLeft,
        /// <summary>
        /// 文字在线端
        /// </summary>
        TextType_OnEdgeOrOnMiddle,
    }

    public class CMModel : VLModel
    {
        public CMTargetType TargetType { set; get; }//标注对象
        public CMAnnotationType AnnotationType { set; get; }//标记样式
        public CMLocationType LocationType { set; get; }//离地模式
        public CMTextType TextType { set; get; }//文字方式

        public List<ElementId> TargetIds { set; get; }//标记的目标对象
        public string AnnotationPrefix { get; internal set; }//标注前缀

        public CMModel() : base("")
        {
        }
        public CMModel(string data) : base(data)
        {
        }

        // 消除注释 ///.+\r\n

        //public ([\w<>]+) (\w+) .+
        // 转为ToData()
        //sb.AppendItem($2);
        // 转为LoadData()
        //$2=sr.ReadFormatStringAs$1(); 

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
                case CMTargetType.Pipe:
                    return new VLClassFilter(typeof(Pipe));
                case CMTargetType.Duct:
                    return new VLClassFilter(typeof(Duct));
                case CMTargetType.CableTray:
                    return new VLClassFilter(typeof(CableTray));
                case CMTargetType.Conduit:
                    return new VLClassFilter(typeof(Conduit));
                default:
                    throw new NotImplementedException("未支持该类型的过滤:" + TargetType.ToString());
            }
        }

        internal string GetPreview()
        {
            switch (TargetType)
            {
                case CMTargetType.Pipe:
                    switch (AnnotationType)
                    {
                        case CMAnnotationType.SPL:
                            return string.Format("如:ZP DN100 {0}2600", AnnotationPrefix);
                        case CMAnnotationType.SL:
                            return string.Format("如:ZP {0}2600", AnnotationPrefix);
                        case CMAnnotationType.PL:
                            return string.Format("如:DN100 {0}2600", AnnotationPrefix);
                        default:
                            throw new NotImplementedException("未支持该类型的");
                    }
                case CMTargetType.Duct:
                    switch (AnnotationType)
                    {
                        case CMAnnotationType.SPL:
                            return string.Format("如:SF 400mmx400mm {0}2600", AnnotationPrefix);
                        case CMAnnotationType.SL:
                            return string.Format("如:SF {0}2600", AnnotationPrefix);
                        case CMAnnotationType.PL:
                            return string.Format("如:400mmx400mm {0}2600", AnnotationPrefix);
                        default:
                            throw new NotImplementedException("未支持该类型的");
                    }
                case CMTargetType.CableTray:
                    switch (AnnotationType)
                    {
                        case CMAnnotationType.SPL:
                            return string.Format("如:ZP DN100 {0}2600", AnnotationPrefix);
                        case CMAnnotationType.SL:
                            return string.Format("如:ZP {0}2600", AnnotationPrefix);
                        case CMAnnotationType.PL:
                            return string.Format("如:DN100 {0}2600", AnnotationPrefix);
                        default:
                            throw new NotImplementedException("未支持该类型的");
                    }
                case CMTargetType.Conduit:
                default:
                    throw new NotImplementedException("未支持该类型的");
            }
        }
    }
}
