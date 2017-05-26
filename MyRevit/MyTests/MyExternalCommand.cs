using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
            //放置类型为"0762*2032 mm"的门
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

            #region 创建拉伸实体族
            var currentPath = System.Environment.CurrentDirectory;
            var familyTemplateDoc = app.NewFamilyDocument(Path.Combine(currentPath, "MyFamilyTemplate.rft"));
            using (Transaction transaction=new Transaction(familyTemplateDoc))
            {
                transaction.Start("Create Familiy");
                try
                {
                    CurveArray curvyArray = new CurveArray();
                    curvyArray.Append(Line.CreateBound(new XYZ(0, 0, 0), new XYZ(5, 0, 0)));
                    curvyArray.Append(Line.CreateBound(new XYZ(5, 0, 0), new XYZ(5, 5, 0)));
                    curvyArray.Append(Line.CreateBound(new XYZ(5, 5, 0), new XYZ(0, 5, 0)));
                    curvyArray.Append(Line.CreateBound(new XYZ(0, 5, 0), new XYZ(0, 0, 0)));
                    CurveArrArray curveArrArray = new CurveArrArray();
                    curveArrArray.Append(curvyArray);
                    var isSolid = true;
                    var length = 10;
                    var sketchPlane = SketchPlane.Create(familyTemplateDoc, app.Create.NewPlane(new XYZ(0, 0, 1), XYZ.Zero));
                    familyTemplateDoc.FamilyCreate.NewExtrusion(isSolid, curveArrArray, sketchPlane, length);
                    familyTemplateDoc.FamilyManager.NewType("MyFamilyType");
                    transaction.Commit();
                }
                catch (System.Exception ex)
                {
                    //TODO Log it 
                    transaction.RollBack();
                }
            }
            familyTemplateDoc.Save();
            #endregion

            return Result.Succeeded;
        }
    }
}
