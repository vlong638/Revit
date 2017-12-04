using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
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
                #region 验证 doc删除了元素后doc.GetElement()的结果 会取到null对象 注意linq中不要访问null对象的属性
                //var doc = commandData.Application.ActiveUIDocument.Document;
                //var es = new List<int>() { 195650, 196343 }.Select(c => doc.GetElement(new ElementId(c)));
                //StringBuilder sb = new StringBuilder();
                //foreach (var e in es)
                //{
                //    //Element被删除后,
                //    if (e == null)
                //    {
                //        TaskDialog.Show("消息", "null entity");
                //        continue;
                //    }
                //    sb.AppendLine($"Id:{e.Id}");
                //}
                //TaskDialog.Show("消息", sb.ToString()); 
                #endregion


                #region 验证elementId=new ElementId(elementId.IntegerValue)
                ////ElementId new ElementId(ElementId.IntegerValue)
                ////Document.GetElement(@ElementId)
                //var doc = commandData.Application.ActiveUIDocument.Document;
                //var elementId = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).ToElementIds().First();
                //var elementByElementId = doc.GetElement(elementId);
                //var elementByIntegerElementId = doc.GetElement(new ElementId(elementId.IntegerValue));
                //StringBuilder sb = new StringBuilder();
                //sb.AppendLine($"elementByElementId:{elementByElementId.Name}");
                //sb.AppendLine($"elementByIntegerElementId:{elementByIntegerElementId.Name}");
                //TaskDialog.Show("消息", sb.ToString());
                #endregion

                #region 获取所有的FillPatternElement类
                StringBuilder sb = new StringBuilder();
                var doc = commandData.Application.ActiveUIDocument.Document;
                var fillPatternElements = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).ToElements();
                foreach (var fillPatternElement in fillPatternElements)
                {
                    sb.AppendLine($"fillPatternElement.Name:{fillPatternElement.Name}");
                }
                #region 所有类型188种
                //实体填充
                //对角线 - 上 3mm
                //对角线交叉填充 - 10mm
                //沙
                //600 x 1200mm
                //600 x 600mm
                //交叉线 - 1.5mm
                //砌块 225x450
                //砖 75x225
                //铝
                //混凝土 - 素砼
                //对角交叉线 1.5mm
                //下对角线 - 1.5mm
                //土壤
                //塑料
                //钢
                //三角形
                //150mm 正方形
                //砌块 225x225
                //100mm 水平
                //100mm 正方形
                //150mm 水平
                //50mm 水平
                //50mm 正方形
                //200mm 正方形
                //250mm 正方形
                //镶木地板 152mm
                //75mm 水平
                //75mm 垂直
                //砌砖
                //人字形 100x200
                //立砌砖层
                //隔热层 - 刚性
                //砌体 - 砖
                //砌体 - 混凝土砌块
                //木质 - 面层
                //对角交叉线 3mm
                //对角线 - 下 3mm
                //水平 3mm
                //交叉线 3mm
                //砌体 - 砌块 225x450mm
                //砌体 - 砖 75x225mm
                //松散 - 多孔材料
                //水平 - 1.5mm
                //金属 - 钢剖面
                //垂直 - 1.5mm
                //正方形 150mm
                //砌体 - 砌块 225x225mm
                //正方形 100mm
                //水平 150mm
                //水平 50mm
                //正方形 50mm
                //正方形 200mm
                //正方形 250mm
                //铺地 - 木地板 150mm
                //水平 75mm
                //垂直 75mm
                //铺地 - 人字形 100x200mm
                //砌体 - 立砌砖层
                //木材 - 剖面
                //无机合成 - 橡胶
                //木材 - 表面1
                //木材 - 表面2
                //木材 - 表面3
                //松散 - 砂浆 / 粉刷
                //松散 - 网状材料
                //对角线 - 上 9mm
                //石材 - 剖面纹理
                //砌体 - 普通砖剖面
                //砌体 - 耐火砖
                //砌体 - 饰面砖表面
                //垂直 1200mm
                //垂直 3mm
                //对角线 - 下 9mm
                //交叉线 5mm
                //金属 - 铝剖面
                //防水 - 防水材料
                //石膏 - 灰泥
                //场地 - 水
                //场地 - 草地
                //场地 - 铺地砾石
                //屋面 - 石板
                //垂直 600mm
                //分区01
                //分区02
                //分区03
                //分区04
                //分区05
                //分区06
                //分区07
                //分区08
                //分区09
                //分区10
                //分区11
                //分区12
                //金属 - 钢丝网 - 小比例
                //金属 - 钢板网
                //金属 - 钢丝网 - 大比例
                //门窗 - 百叶 30mm
                //门窗 - 百叶 50mm
                //金属 - 菱形网01
                //金属 - 菱形网02
                //金属 - 穿孔板
                //玻璃 - 玻璃剖面
                //松散 - 泡沫塑料
                //分区13
                //板材 - 石材 - 直缝 1200x400mm
                //板材 - 石材 - 直缝 1200x600mm
                //板材 - 石材 - 直缝 1200x800mm
                //板材 - 石材 - 直缝 1200x900mm
                //板材 - 石材 - 直缝 600x600mm
                //板材 - 石材 - 直缝 600x800mm
                //板材 - 石材 - 直缝 600x900mm
                //板材 - 石材 - 直缝 600x1200mm
                //板材 - 金属 - 直缝 1200x600mm
                //板材 - 金属 - 直缝 1800x600mm
                //板材 - 金属 - 直缝 2400x600mm
                //板材 - 金属 - 直缝 3000x600mm
                //板材 - 金属 - 直缝 600x1200mm
                //板材 - 金属 - 直缝 600x1800mm
                //板材 - 金属 - 直缝 600x2400mm
                //板材 - 金属 - 直缝 600x3000mm
                //板材 - 金属 - 直缝 600x600mm
                //板材 - 金属 - 直缝 1200x1200mm
                //板材 - 金属 - 直缝 1800x1800mm
                //板材 - 金属 - 直缝 2400x2400mm
                //板材 - 金属 - 错缝 1200x400mm
                //板材 - 金属 - 错缝 1200x600mm
                //板材 - 金属 - 错缝 1200x800mm
                //板材 - 金属 - 错缝 1200x900mm
                //板材 - 金属 - 错缝 600x1200mm
                //板材 - 金属 - 错缝 600x600mm
                //板材 - 金属 - 错缝 600x800mm
                //板材 - 金属 - 错缝 600x900mm
                //板材 - 石材 - 错缝 1200x400mm
                //板材 - 石材 - 错缝 1200x600mm
                //板材 - 石材 - 错缝 1200x800mm
                //板材 - 石材 - 错缝 1200x900mm
                //板材 - 石材 - 错缝 600x1200mm
                //板材 - 石材 - 错缝 600x600mm
                //板材 - 石材 - 错缝 600x800mm
                //板材 - 石材 - 错缝 600x900mm
                //水平 100mm
                //水平 30mm
                //水平 300mm
                //砌体 - 砖 80x240mm
                //正方形 100mm,45度
                //垂直 100mm
                //垂直 150mm
                //垂直 300mm
                //砌体 - 砌块 200x400mm
                //金属 - 铝格栅 50mm
                //木材 - 纹理
                //板材 - 垂直1500mm
                //板材 - 错缝 1600x800mm
                //砌体 - 砖02
                //砌体 - 砖01
                //砌体 - 砖03
                //砌体 - 砖04
                //屋面 - 筒瓦01
                //钢筋混凝土
                //砂浆
                //对角线交叉填充 - 0.3mm
                //上对角线 - 1.5mm
                //场地 - 沼泽
                //场地 - 行道砖人字花纹 1
                //场地 - 行道砖人字花纹 2
                //混凝土 - 钢砼
                //无机合成 - 塑料
                //松散 - 焦渣矿渣
                //松散 - 纤维材料
                //松散 - 砂石碎砖
                //砌体 - 加气砼
                //砌体 - 空心砖剖面
                //砌体 - 毛石
                //屋面 - 弯瓦
                //屋面 - 筒瓦
                //屋面 - 茅草
                //木材 - 饰面
                //砌体 - 玻璃砖
                //砌体 - 卵石
                //场地 - 铺地卵石
                //上对角线
                //下对角线
                //水平
                //垂直
                //交叉填充
                //对角线交叉填充 
                #endregion


                TaskDialog.Show("消息", sb.ToString());
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
