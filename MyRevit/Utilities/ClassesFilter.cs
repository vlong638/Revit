using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.Utilities
{
    /// <summary>
    /// multiple class 选择过滤器
    /// 支持文档及链接文档内的选择
    /// </summary>
    public class ClassesFilter : ISelectionFilter
    {
        List<Type> TargetTypes { set; get; }
        bool IsLinkInstance { set; get; }
        RevitLinkInstance LinkInstance { set; get; }

        public ClassesFilter(bool isLinkInstance, params Type[] types)
        {
            TargetTypes = types.ToList();
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
                return TargetTypes.Contains(element.GetType());
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            if (LinkInstance == null)
                return false;

            Document linkedDoc = LinkInstance.GetLinkDocument();
            Element element = linkedDoc.GetElement(reference.LinkedElementId);
            return TargetTypes.Contains(element.GetType());
        }
    }
}
