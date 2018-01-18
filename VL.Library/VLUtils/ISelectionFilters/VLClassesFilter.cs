using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VL.Library
{
    /// <summary>
    /// multiple class 选择过滤器
    /// 支持文档及链接文档内的选择
    /// </summary>
    public class VLClassesFilter : ISelectionFilter
    {
        List<Type> TargetTypes { set; get; }
        bool IsLinkInstance { set; get; }
        RevitLinkInstance LinkInstance { set; get; }
        List<VLClassFilter> ClassFilters { set; get; }

        public VLClassesFilter(bool isLinkInstance, params Type[] types)
        {
            TargetTypes = types.ToList();
            IsLinkInstance = isLinkInstance;
        }
        public VLClassesFilter(bool isLinkInstance, params VLClassFilter[] classFilters)
        {
            IsLinkInstance = isLinkInstance;
            ClassFilters = classFilters.ToList();
        }
        public VLClassesFilter(bool isLinkInstance, List<VLClassFilter> classFilters)
        {
            IsLinkInstance = isLinkInstance;
            ClassFilters = classFilters;
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
                if (ClassFilters == null)
                {
                    return TargetTypes.Contains(element.GetType());
                }
                else
                {
                    foreach (var ClassFilter in ClassFilters)
                    {
                        if (ClassFilter.AllowElement(element))
                            return true;
                    }
                    return false;
                }
            }
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            if (LinkInstance == null)
                return false;

            if (ClassFilters == null)
            {
                Document linkedDoc = LinkInstance.GetLinkDocument();
                Element element = linkedDoc.GetElement(reference.LinkedElementId);
                return TargetTypes.Contains(element.GetType());
            }
            else
            {
                foreach (var ClassFilter in ClassFilters)
                {
                    if (ClassFilter.AllowReference(reference, position))
                        return true;
                }
                return false;
            }
        }
    }
}
