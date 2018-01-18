using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;

namespace VL.Library
{
    /// <summary>
    /// class 选择过滤器
    /// 支持文档及链接文档内的选择
    /// </summary>
    public class VLClassFilter : ISelectionFilter
    {
        Type TargetType { set; get; }
        bool IsLinkInstance { set; get; }
        RevitLinkInstance LinkInstance { set; get; }
        Func<Element, bool> IsExconditionFit { set; get; }

        public VLClassFilter(Type targetType, bool isLinkInstance = false, Func<Element, bool> isExconditionFit = null)
        {
            TargetType = targetType;
            IsLinkInstance = isLinkInstance;
            IsExconditionFit = isExconditionFit;
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
                if (IsExconditionFit == null)
                {
                    return TargetType == element.GetType();
                }
                else
                {
                    return TargetType == element.GetType() && IsExconditionFit(element);
                }
            }
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            if (LinkInstance == null)
                return false;

            Document linkedDoc = LinkInstance.GetLinkDocument();
            Element element = linkedDoc.GetElement(reference.LinkedElementId);
            if (IsExconditionFit == null)
            {
                return TargetType == element.GetType();
            }
            else
            {
                return TargetType == element.GetType() && IsExconditionFit(element);
            }
        }
        public static VLClassesFilter operator |(VLClassFilter c1, VLClassFilter c2)
        {
            return new VLClassesFilter(c1.IsLinkInstance, c1, c2);
        }
    }
}
