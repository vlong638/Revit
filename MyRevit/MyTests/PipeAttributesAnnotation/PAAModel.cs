﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI.Selection;
using MyRevit.MyTests.Utilities;
using MyRevit.MyTests.VLBase;
using System;
using System.Collections.Generic;

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

    public class PAAModel : VLModel
    {
        public PAATargetType TargetType { set; get; }//标注对象
        public PAAAnnotationType AnnotationType { set; get; }//标记样式
        public PAALocationType LocationType { set; get; }//离地模式
        public PAATextType TextType { set; get; }//文字方式

        public List<ElementId> TargetIds { set; get; }//标记的目标对象

        public PAAModel() : base("")
        {
        }
        public PAAModel(string data) : base(data)
        {
        }

        public override bool LoadData(string data)
        {
            return false;
        }

        public override string ToData()
        {
            return "";
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
    }
}
