using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace MyRevit.Entities
{
    [Transaction(TransactionMode.Manual)]
    public class 调研_获取类型结构属性 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;

            var elementReference = uiDoc.Selection.PickObject(ObjectType.Element, "选择要添加的构件");
            if (elementReference == null)
                return Result.Cancelled;
            var element = doc.GetElement(elementReference.ElementId);
            CompoundStructure compoundStructure = null;
            if (element is Wall)
            {
                compoundStructure = (element as Wall).WallType.GetCompoundStructure();
            }
            if (element is Floor)
            {
                compoundStructure = (element as Floor).FloorType.GetCompoundStructure();
            }
            if (element is ExtrusionRoof)//屋顶有多种类型
            {
                compoundStructure = (element as ExtrusionRoof).RoofType.GetCompoundStructure();
            }
            if (compoundStructure == null)
                return Result.Failed;
            var layers = compoundStructure.GetLayers();
            string text = "";
            foreach (var layer in layers)
            {
                text += layer.Width + doc.GetElement(layer.MaterialId).Name + System.Environment.NewLine;
            }
            return Result.Succeeded;
        }
    }
}