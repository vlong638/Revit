using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.MyTests.Utilities
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
        List<ClassFilter> ClassFilters { set; get; }


        public ClassesFilter(bool isLinkInstance, params Type[] types)
        {
            TargetTypes = types.ToList();
            IsLinkInstance = isLinkInstance;
        }

        public ClassesFilter(bool isLinkInstance, params ClassFilter[] classFilters)
        {
            IsLinkInstance = isLinkInstance;
            ClassFilters = classFilters.ToList();
        }
        public ClassesFilter(bool isLinkInstance, List<ClassFilter> classFilters)
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
