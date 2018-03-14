using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.MyTests.Utilities;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyRevit.MyTests.BeamAlignToFloor
{
    #region Model
    public enum ContentType
    {
        Document,
        LinkDocument
    }
    public enum AlignType
    {
        BeamTopToFloorTop,
        BeamTopToFloorBottom,
    }
    public class BeamAlignToFloorModel
    {
        public ContentType ContentType { set; get; }
        public AlignType AlignType { set; get; }
        /// <summary>
        /// 偏移,用于链接模型存在偏移的情况
        /// </summary>
        public XYZ Offset = new XYZ(0, 0, 0);
    }
    #endregion

    class LevelFloor
    {
        public double Elevation;
        public Floor Floor;

        public LevelFloor(double elevation, Floor floor)
        {
            Elevation = elevation;
            Floor = floor;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class CommandOfBeamAlignToFloor : IExternalCommand
    {
        static BeamAlignToFloorModel model = new BeamAlignToFloorModel()
        {
            AlignType = AlignType.BeamTopToFloorBottom,
            ContentType = ContentType.Document,
        };

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;

            ////链接模板测试
            //var linkFilter = new CategoryFilter(BuiltInCategory.OST_Floors, true);
            //Reference reference = uiDoc.Selection.PickObject(ObjectType.LinkedElement, linkFilter, "先选择一个链接文件");
            //Element element = doc.GetElement(reference.ElementId);
            //if (element.Category.Name != "RVT 链接")
            //    return Result.Cancelled;
            //var floors = uiDoc.Selection.PickObjects(ObjectType.LinkedElement, linkFilter, "在链接文件中选择板:").ToList();
            ////194278
            //element = doc.GetElement(new ElementId(194278));
            //var linkInstance = doc.GetElement(reference.ElementId) as RevitLinkInstance;
            //if (linkInstance!=null)
            //{
            //    var linkDoc = linkInstance.GetLinkDocument();
            //    element = linkDoc.GetElement(new ElementId(194278));
            //}

            //MessageHelper.TaskDialogShow("开始选择板");
            //业务逻辑处理
            VLTransactionHelper.DelegateTransaction(doc, "梁齐板", () =>
            {
                Document linkDocument = null;
                IEnumerable<ElementId> floorIds = null;
                if (model.ContentType == ContentType.Document)
                {
                    //基础板,OST_StructuralFoundation
                    //结构楼板,OST_Floors
                    floorIds = uiDoc.Selection.PickObjects(ObjectType.Element, new CategoryFilter(BuiltInCategory.OST_Floors), "选择楼板").Select(c => c.ElementId);
                    if (floorIds == null || floorIds.Count() == 0)
                        return false;
                }
                else
                {
                    var linkFilter = new CategoryFilter(BuiltInCategory.OST_Floors, true);
                    Reference reference = uiDoc.Selection.PickObject(ObjectType.LinkedElement, linkFilter, "先选择一个链接文件");
                    Element element = doc.GetElement(reference.ElementId);
                    if (element.Category.Name != "RVT 链接")
                        return false;
                    linkDocument = (element as RevitLinkInstance).GetLinkDocument();
                    floorIds = uiDoc.Selection.PickObjects(ObjectType.LinkedElement, linkFilter, "在链接文件中选择板:").Select(c => c.LinkedElementId);
                    model.Offset = (element as Instance).GetTotalTransform().Origin;

                    ////链接元素测试
                    //foreach (var floor in floors)
                    //{
                    //    var f = doc.GetElement(floor.ElementId);
                    //    f = (element as RevitLinkInstance).GetLinkDocument().GetElement(floor.ElementId);
                    //}
                }
                //MessageHelper.TaskDialogShow("开始选择梁");
                var beamIds = uiDoc.Selection.PickObjects(ObjectType.Element, new CategoryFilter(BuiltInCategory.OST_StructuralFraming), "选择梁").Select(c => c.ElementId);
                if (beamIds == null || beamIds.Count() == 0)
                    return false;


                //ValidFaces collector = new ValidFaces(doc, model);
                ////对板按高程从高到底处理
                //List<LevelFloor> levelFloors = new List<LevelFloor>();
                //foreach (var floorId in floorIds)
                //{
                //    var floor = doc.GetElement(floorId) as Floor;
                //    var level = doc.GetElement(floor.LevelId) as Level;
                //    levelFloors.Add(new LevelFloor(level.Elevation, floor));
                //}
                //List<Line> beamLines = collector.DealAll(null, new List<Line>(), levelFloors);
                //GraphicsDisplayerManager.Display(collector, levelFloors);


                #region 0803版本
                OutLineManager0802 collector = new OutLineManager0802(doc, model);
                //对板按高程从高到底处理
                List<LevelFloor> levelFloors = new List<LevelFloor>();
                foreach (var floorId in floorIds)
                {
                    if (model.ContentType == ContentType.Document)
                    {
                        var floor = doc.GetElement(floorId) as Floor;
                        var level = doc.GetElement(floor.LevelId) as Level;
                        levelFloors.Add(new LevelFloor(level.Elevation, floor));
                    }
                    else
                    {
                        collector.LinkDocument = linkDocument;
                        var floor = linkDocument.GetElement(floorId) as Floor;
                        var level = linkDocument.GetElement(floor.LevelId) as Level;
                        levelFloors.Add(new LevelFloor(level.Elevation, floor));
                    }
                }
                //依次对各个梁进行个板面的拆分处理
                foreach (var beamId in beamIds)
                {
                    var beam = doc.GetElement(beamId);
                    var beamSymbol = (beam as FamilyInstance).Symbol;
                    var beamLevel = doc.GetElement(beam.LevelId) as Level;
                    var beamLine = (beam.Location as LocationCurve).Curve as Line;
                    if (beamLine == null)
                        throw new NotImplementedException("暂不支持曲线梁");
                    var start = new XYZ(beamLine.GetEndPoint(0).X, beamLine.GetEndPoint(0).Y, 0);
                    var end = new XYZ(beamLine.GetEndPoint(1).X, beamLine.GetEndPoint(1).Y, 0);
                    var beamLineZ0 = Line.CreateBound(start, end);
                    GraphicsDisplayerManager.Display(collector, levelFloors);
                    List<Line> beamLines = collector.DealAll(beam, new List<Line>() { beamLineZ0 }, levelFloors);
                    //最终未贴合板的梁段生成
                    foreach (var ungenerateBeamLine in beamLines)
                    {
                        var sp0 = ungenerateBeamLine.GetEndPoint(0);
                        var sp1 = ungenerateBeamLine.GetEndPoint(1);
                        var fixedSP0 = VLGeometryHelper.VL_GetIntersectionOnLine(sp0, beamLine.GetEndPoint(0), beamLine.Direction);
                        var fixedSP1 = VLGeometryHelper.VL_GetIntersectionOnLine(sp1, beamLine.GetEndPoint(0), beamLine.Direction);
                        var sectionBeam = doc.Create.NewFamilyInstance(Line.CreateBound(fixedSP0, fixedSP1), beamSymbol, beamLevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                        collector.CreatedBeams.Add(sectionBeam);
                    }
                    collector.LinkBeamWithAngleGT180(beam);
                    doc.Delete(beam.Id);
                }
                #endregion

                #region 0803前
                ////添加板
                //foreach (var floorId in floorIds)
                //{
                //    var floor = doc.GetElement(floorId) as Floor;
                //    collector.Add(floor);
                //}
                ////计算梁的偏移处理
                //foreach (var beamId in beamIds)
                //{
                //    var beam = doc.GetElement(beamId);
                //    var fitLineCollection = collector.Fit(beam);
                //    var seperatePoints = collector.Merge(fitLineCollection, new DirectionPoint((beam.Location as LocationCurve).Curve.GetEndPoint(0), ((beam.Location as LocationCurve).Curve as Line).Direction, false), new DirectionPoint((beam.Location as LocationCurve).Curve.GetEndPoint(1), ((beam.Location as LocationCurve).Curve as Line).Direction, false));
                //    collector.Adapt(doc, beam, seperatePoints.SeperatedLines);

                //    //绘图分析
                //    GraphicsDisplayerManager.Display(@"E:\WorkingSpace\Outputs\Images\display2.png", seperatePoints, collector.LeveledOutLines);
                //} 
                #endregion
                return true;
            });
            //model.ContentType = model.ContentType == ContentType.Document ? ContentType.LinkDocument : ContentType.Document;
            return Result.Succeeded;
        }
    }
}
