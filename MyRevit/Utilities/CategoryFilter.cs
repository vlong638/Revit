using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;

namespace MyRevit.MyTests.Utilities
{
    /// <summary>
    /// 管道 选择过滤器
    /// </summary>
    public class CategoryFilter : ISelectionFilter
    {
        BuiltInCategory TargetCategory { set; get; }

        public CategoryFilter(BuiltInCategory category)
        {
            TargetCategory = category;
        }

        public bool AllowElement(Element element)
        {
            var document = element.Document;
            var category = Category.GetCategory(document, TargetCategory);
            return element.Category.Id == category.Id;
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
