﻿using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.MyTests.Utilities
{
    /// <summary>
    /// class 选择过滤器
    /// 支持文档及链接文档内的选择
    /// </summary>
    public class ClassFilter : ISelectionFilter
    {
        Type TargetType { set; get; }
        bool IsLinkInstance { set; get; }
        RevitLinkInstance LinkInstance { set; get; }

        public ClassFilter(Type targetType, bool isLinkInstance = false)
        {
            TargetType = targetType;
            IsLinkInstance = isLinkInstance;
        }

        public bool AllowElement(Element element)
        {
            if (IsLinkInstance)
            {
                LinkInstance = element as RevitLinkInstance;
                return (LinkInstance != null);
            }
            else
            {
                return TargetType == element.GetType();
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            if (LinkInstance == null)
                return false;

            Document linkedDoc = LinkInstance.GetLinkDocument();
            Element element = linkedDoc.GetElement(reference.LinkedElementId);
            return TargetType == element.GetType();
        }
    }
}
