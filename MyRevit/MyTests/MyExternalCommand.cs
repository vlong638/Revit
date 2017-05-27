using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.Utilities;
using System.IO;
using System.Linq;

namespace MyRevit.Entities
{
    [Transaction(TransactionMode.Manual)]
    class MyExternalCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Hello World
            //TaskDialog.Show("VL title", "VL says Hello Revit");



            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;

            #region 放置类型为"0762*2032 mm"的门
            //首先通过类型过滤出 类型为门的族类型,找到名称相同的
            string doorTypeName = "0762*2032 mm";
            FamilySymbol doorType = null;
            var filter = new LogicalAndFilter(
                new ElementCategoryFilter(BuiltInCategory.OST_Doors),
                new ElementClassFilter(typeof(FamilySymbol))
                );
            var collector = new FilteredElementCollector(doc).WherePasses(filter);
            bool symbolFound = collector.FirstOrDefault(c => c.Name == doorTypeName) != null;
            //如果没有则通过文件加载族
            if (symbolFound)
            {
                doorType = collector.FirstOrDefault(c => c.Name == doorTypeName) as FamilySymbol;
            }
            else
            {
                string file = @"familyFilePath";
                Family family;
                if (doc.LoadFamily(file, out family))
                {
                    var validType = family.GetValidTypes().FirstOrDefault(c =>
                    {
                        var symbol = (doc.GetElement(c) as FamilySymbol);
                        if (symbol != null && symbol.Name == doorTypeName)
                            return true;
                        return false;
                    });
                    if (validType != null)
                    {
                        doorType = doc.GetElement(validType) as FamilySymbol;
                        symbolFound = true;
                    }
                }
            }
            //使用族类型创建门 线性的门是有着LocationCurve的且LocationCurve.Curve为Line的元素
            Wall wall = null;
            if (doorType != null)
            {
                Element element = new FilteredElementCollector(doc)
                    .WherePasses(new ElementClassFilter(typeof(Wall)))
                    .FirstOrDefault(c =>
                    {
                        var locationCurve = c.Location as LocationCurve;
                        if (locationCurve != null)
                        {
                            var line = locationCurve.Curve as Line;
                            if (line!=null)
                            {
                                return true;
                            }
                            return false;
                        }
                        return false;
                    });
                if (element != null)
                    wall = element as Wall;
            }
            //在墙的中心创建一个门
            if (wall != null)
            {
                var line = (wall.Location as LocationCurve).Curve as Line;
                var wallLevel = doc.GetElement(wall.LevelId) as Level;
                XYZ midPoint = (line.GetEndPoint(0) + line.GetEndPoint(1)) / 2;
                FamilyInstance door = doc.Create.NewFamilyInstance(midPoint, doorType, wall, wallLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            }
            #endregion

            #region 复制墙类型
            var wallElementId = 1111;
            wall = doc.GetElement(new ElementId(wallElementId)) as Wall;
            if (wall!=null)
            {
                var wallType = wall.WallType;
                ElementType duplicatedType = wallType.Duplicate(wall.Name + "duplicated");
            }
            #endregion

            #region 元素移动
            TransactionHelper.DelegateTransaction(doc, () =>
            {


                return true;
            });
            #endregion

            return Result.Succeeded;
        }
    }
}
