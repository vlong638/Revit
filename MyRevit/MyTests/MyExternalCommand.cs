using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyRevit.Entities
{
    [Transaction(TransactionMode.Manual)]
    public class MyExternalCommand : IExternalCommand
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
                var structureType = Autodesk.Revit.DB.Structure.StructuralType.NonStructural;
                FamilyInstance door = doc.Create.NewFamilyInstance(midPoint, doorType, wall, wallLevel, structureType);
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
            //TransactionHelper.DelegateTransaction(doc, () =>
            //{
            //    //Revit文档的创建句柄
            //    Autodesk.Revit.Creation.Document creator = doc.Create;
            //    //创建一根柱子
            //    XYZ origin = new XYZ(0, 0, 0);
            //    Level level = GetALevel(doc);
            //    FamilySymbol columnType = GetAColumnType(doc);
            //    var structureType = Autodesk.Revit.DB.Structure.StructuralType.Column;
            //    FamilyInstance column = creator.NewFamilyInstance(origin, columnType, level, structureType);
            //    XYZ newPlace = new XYZ(10, 20, 30);
            //    ElementTransformUtils.MoveElement(doc, column.Id, newPlace);
            //    return true;
            //});
            #endregion

            #region ElementTransformUtils
            //ElementTransformUtils.CopyElement();
            //ElementTransformUtils.CopyElements();
            //ElementTransformUtils.MirrorElement();
            //ElementTransformUtils.MirrorElements();
            //ElementTransformUtils.MoveElement();
            //ElementTransformUtils.MoveElements();
            //ElementTransformUtils.RotateElement();
            //ElementTransformUtils.RotateElements(); 
            #endregion

            #region 元素旋转
            //ElementTransformUtils旋转方法
            TransactionHelper.DelegateTransaction(doc, () =>
            {
                LocationCurve wallLine = wall.Location as LocationCurve;
                XYZ p1 = wallLine.Curve.GetEndPoint(0);
                XYZ p2 = new XYZ(p1.X, p1.Y, 30);
                Line axis = Line.CreateBound(p1, p2);
                ElementTransformUtils.RotateElement(doc, wall.Id, axis, Math.PI / 3);//逆时针60°
                return true;
            });
            //LocationCurve,LocationPoint,自带的旋转方法
            TransactionHelper.DelegateTransaction(doc, () =>
            {
                LocationCurve locationCurve = wall.Location as LocationCurve;//线性坐标自带线
                if (locationCurve != null)
                {
                    Curve curve = locationCurve.Curve;
                    var start = curve.GetEndPoint(0);
                    Line axis = Line.CreateBound(start, start.Add(new XYZ(0, 0, 10)));
                    locationCurve.Rotate(axis, Math.PI);//PI=180°                
                }
                LocationPoint locationPoint = wall.Location as LocationPoint;
                if (locationPoint!=null)
                {
                    var start = locationPoint.Point;
                    Line axis = Line.CreateBound(start, start.Add(new XYZ(0, 0, 10)));
                    locationPoint.Rotate(axis, Math.PI);
                }
                return true;
            });
            #endregion

            #region 元素镜像
            TransactionHelper.DelegateTransaction(doc, () =>
            {
                Plane plane = new Plane(XYZ.BasisX, XYZ.Zero);
                if (ElementTransformUtils.CanMirrorElement(doc, wall.Id))
                    ElementTransformUtils.MirrorElement(doc, wall.Id, plane);
                return true;
            });
            #endregion

            #region 元素删除
            //var deleteElements = Document.Delete(@ElementIds);
            #endregion

            #region 元素组合
            TransactionHelper.DelegateTransaction(doc, () =>
            {
                List<ElementId> elementIds = new List<ElementId>()
                {
                    new ElementId(1000),
                    new ElementId(1001),
                    new ElementId(1002),
                };
                Group group = doc.Create.NewGroup(elementIds);
                return true;
            });
            #endregion

            #region 元素编辑
            //创建参照平面
            TransactionHelper.DelegateTransaction(doc, () =>
            {
                XYZ bubbleEnd = new XYZ(0, 5, 5);
                XYZ freeEnd = new XYZ(5, 5, 5);
                XYZ cutVector = XYZ.BasisY;
                View view = doc.ActiveView;
                ReferencePlane referencePlane = doc.FamilyCreate.NewReferencePlane(bubbleEnd, freeEnd, cutVector, view);
                referencePlane.Name = "MyReferencePlane";
                return true;
            });
            //创建参照线,由模型线-转>参照线
            TransactionHelper.DelegateTransaction(doc, () =>
            {
                ModelCurve modelCurve = doc.GetElement(new ElementId(1000)) as ModelCurve;//ModelCurve模型线
                modelCurve.ChangeToReferenceLine();
                //modelCurve.IsReferenceLine;
                return true;
            });
            //通过标高创建草图平面,然后在草图平面创建模型线
            TransactionHelper.DelegateTransaction(doc, () =>
            {
                Level level = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).FirstOrDefault() as Level;
                Line line = Line.CreateBound(XYZ.Zero, new XYZ(10, 10, 0));
                SketchPlane sketchPlane=SketchPlane.Create(doc,level.Id);
                ModelCurve modelLine = doc.FamilyCreate.NewModelCurve(line, sketchPlane);
                return true;
            });
            //使用拉身体获取相应的草图平面
            TransactionHelper.DelegateTransaction(doc, () =>
            {
                Extrusion extrusion = doc.GetElement(new ElementId(11212)) as Extrusion;
                SketchPlane sketchPlane = extrusion.Sketch.SketchPlane;
                CurveArrArray sketchProfile = extrusion.Sketch.Profile;
                return true;
            });
            #endregion

            #region 族

            string tagName = "梁平法_集中标_左对齐";
            FamilySymbol tagSymbol = null;
            //查找族类型
            var symbols = new FilteredElementCollector(doc)
                .WherePasses(new ElementClassFilter(typeof(FamilySymbol)))
                .WherePasses(new ElementCategoryFilter(BuiltInCategory.OST_StructuralFramingTags));
            var targetSymbol = symbols.FirstOrDefault(c => c.Name == tagName);
            if (targetSymbol != null)
                tagSymbol = targetSymbol as FamilySymbol;
            //空时加载族类型
            if (tagSymbol==null)
            {
                var symbolFile = @"E:\WorkingSpace\Tasks\0526标注\梁平法_集中标_左对齐.rfa";
                Family family;
                if (doc.LoadFamily(symbolFile,out family))
                {
                    foreach (ElementId typeId in family.GetValidTypes())
                    {
                        var validType = doc.GetElement(typeId) as FamilySymbol;
                        if (validType != null&& validType.Name==tagName)
                        {
                            tagSymbol = validType;
                            break;
                        }
                    }
                }
                else
                {
                    TaskDialogShow("加载族文件失败");
                }
            }
            //如果上述两者获取到了对应的族
            if (tagSymbol!=null)
            {
                //doc.Create.NewFamilyInstance(, tagSymbol);
            }

            #endregion



            TransactionHelper.DelegateTransaction(doc, () =>
            {

                return true;
            });
            return Result.Succeeded;
        }

        private static void TaskDialogShow(string message)
        {
            TaskDialog.Show("a", message);
        }

        class ProjectFamilyLoadOption : IFamilyLoadOptions
        {
            public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
            {
                overwriteParameterValues = true;//true时更新已加载的族的参数
                return true;
            }

            public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
            {
                //source = FamilySource.Family;
                source = FamilySource.Project;
                overwriteParameterValues = true;// true时更新已加载的族的参数
                return true;
            }
        }
    }
}
