using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;

namespace MyRevit.MyTests.Utilities
{
    /// <summary>
    /// BuiltInCategory选择过滤器
    /// 支持文档及链接文档内的选择
    /// </summary>
    public class CategoryFilter : ISelectionFilter
    {
        BuiltInCategory TargetCategory { set; get; }
        bool IsLinkInstance { set; get; }
        RevitLinkInstance LinkInstance { set; get; }


        public CategoryFilter(BuiltInCategory category,bool isLinkInstance=false)
        {
            TargetCategory = category;
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
                var document = element.Document;
                var category = Category.GetCategory(document, TargetCategory);
                return element.Category.Id == category.Id;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            if (LinkInstance == null)
                return false;

            Document linkedDoc = LinkInstance.GetLinkDocument();
            Element element = linkedDoc.GetElement(reference.LinkedElementId);
            var category = Category.GetCategory(linkedDoc, TargetCategory);
            return element.Category.Id == category.Id;
        }
    }
}
