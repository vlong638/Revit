using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace MyRevit.MyTests.PipeAnnotation
{
    /// <summary>
    /// 管道 选择过滤器
    /// </summary>
    public class PipeFilter : ISelectionFilter
    {
        RevitLinkInstance rvtIns = null;
        public bool AllowElement(Element elem)
        {
            var doc = elem.Document;
            var category = Category.GetCategory(doc, BuiltInCategory.OST_PipeCurves);
            return elem.Category.Id == category.Id;
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
