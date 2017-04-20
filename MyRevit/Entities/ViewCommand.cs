using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.Utilities;
using System;
using System.Linq;
using System.Text;

namespace MyRevit.Entities
{
    [Transaction(TransactionMode.Manual)]
    class ViewCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                #region 验证elementId=new ElementId(elementId.IntegerValue)
                //ElementId new ElementId(ElementId.IntegerValue)
                //Document.GetElement(@ElementId)
                var doc = commandData.Application.ActiveUIDocument.Document;
                var elementId = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).ToElementIds().First();
                var elementByElementId = doc.GetElement(elementId);
                var elementByIntegerElementId = doc.GetElement(new ElementId(elementId.IntegerValue));
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"elementByElementId:{elementByElementId.Name}");
                sb.AppendLine($"elementByIntegerElementId:{elementByIntegerElementId.Name}");
                TaskDialog.Show("消息", sb.ToString());
                #endregion

                #region 获取所有的FillPatternElement类
                //StringBuilder sb = new StringBuilder();
                //var doc = commandData.Application.ActiveUIDocument.Document;
                //var fillPatternElements = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).ToElements();
                //foreach (var fillPatternElement in fillPatternElements)
                //{
                //    sb.AppendLine($"fillPatternElement.Name:{fillPatternElement.Name}");
                //}
                //TaskDialog.Show("消息", sb.ToString()); 
                #endregion

                #region 视图检测,创建视图
                //string viewName = "土方分块";
                //var doc = commandData.Application.ActiveUIDocument.Document;
                //var view = DocumentHelper.GetElementByNameAs<View>(doc, viewName);
                //if (view==null)
                //{
                //    using (var transaction = new Transaction(doc,"创建土方分块视图"))
                //    {
                //        transaction.Start();
                //        try
                //        {
                //            //var viewType = ViewType.FloorPlan;
                //            var elevation = 10;
                //            var level = Level.Create(doc, elevation);
                //            var viewFamilyTypes = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).ToElements();
                //            var floorPlan = viewFamilyTypes.First(c => c.Name == "楼层平面");
                //            view = ViewPlan.Create(doc, floorPlan.Id, level.Id);
                //            view.Name = viewName;
                //            transaction.Commit();
                //            TaskDialog.Show("消息", "视图(土方分块)新建成功");
                //        }
                //        catch (Exception ex)
                //        {
                //            transaction.RollBack();
                //            TaskDialog.Show("消息", "视图(土方分块)新建失败,错误详情:" + ex.ToString());
                //        }
                //    }
                //}
                //else
                //{
                //    TaskDialog.Show("消息", "视图(土方分块)已存在");
                //} 
                #endregion

                #region 视图 透视,正交 区域框
                //StringBuilder sb = new StringBuilder();
                //if (view is View3D)
                //{
                //    var view3D = view as View3D;
                //    sb.AppendLine($"{(view3D.IsPerspective ? "透视三维视图" : "正交三维视图")}");
                //    sb.AppendLine($"view.CropBox.Min X:{view.CropBox.Min.X},Y:{view.CropBox.Min.Y},Z:{view.CropBox.Min.Z}");
                //    sb.AppendLine($"view.CropBox.Max X:{view.CropBox.Max.X},Y:{view.CropBox.Max.Y},Z:{view.CropBox.Max.Z}");
                //    sb.AppendLine($"view.Outline.Min U:{view.Outline.Min.U},V:{view.Outline.Min.V}");
                //    sb.AppendLine($"view.Outline.Max U:{view.Outline.Max.U},V:{view.Outline.Max.V}");
                //    TaskDialog.Show("视图内容如下", sb.ToString());
                //    view3D.SetSectionBox(new BoundingBoxXYZ());
                //}
                //else
                //{
                //    TaskDialog.Show("警告", "这不是一个三维视图");
                //} 
                #endregion
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
