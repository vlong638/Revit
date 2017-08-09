using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyRevit.Entities
{
    [Transaction(TransactionMode.Manual)]
    class FilterElementCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                Document document = commandData.Application.ActiveUIDocument.Document;
                //类型过滤器 快
                ElementClassFilter classFilter = new ElementClassFilter(typeof(FamilyInstance));
                //族实例过滤器 慢
                FamilyInstanceFilter familyInstanceFilter = new FamilyInstanceFilter(document, new ElementId(1));
                //内建类型过滤
                ElementCategoryFilter categoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
                //逻辑过滤器,组合前两者过滤
                LogicalAndFilter logicalAndFilter = new LogicalAndFilter(classFilter, categoryFilter);
                //过滤处理
                FilteredElementCollector collector = new FilteredElementCollector(document);
                ICollection<ElementId> doorIds = collector.WherePasses(logicalAndFilter).ToElementIds();
                foreach (var elementId in doorIds)
                {
                    var element = document.GetElement(elementId);
                    sb.AppendLine($"{element.Name}");
                }
                //简写
                var classFilteredCollector = new FilteredElementCollector(document)
                    .OfClass(typeof(FamilyInstance));
                var categoryFilteredCollector = new FilteredElementCollector(document)
                    .OfCategory(BuiltInCategory.OST_Doors);
                foreach (Category category in document.Settings.Categories)
                {
                    var categoryIdFilteredCollector = new FilteredElementCollector(document)
                        .OfCategoryId(category.Id);
                }
                TaskDialog.Show("vl selected elements", "所有的实例门如下:" + sb.ToString());
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
}
