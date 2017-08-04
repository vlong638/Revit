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
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;
            BeamAlignToFloorModel model = new BeamAlignToFloorModel()
            {
                AlignType = AlignType.BeamTopToFloorTop,
                ContentType = ContentType.Document,
            };

            //MessageHelper.TaskDialogShow("开始选择板");
            IEnumerable<ElementId> floorIds = null;
            if (model.ContentType == ContentType.Document)
            {
                //基础板,OST_StructuralFoundation
                //结构楼板,OST_Floors
                floorIds = uiDoc.Selection.PickObjects(ObjectType.Element, new CategoryFilter(BuiltInCategory.OST_Floors)).Select(c => c.ElementId);
                if (floorIds == null || floorIds.Count() == 0)
                    return Result.Cancelled;
            }
            //MessageHelper.TaskDialogShow("开始选择梁");
            var beamIds = uiDoc.Selection.PickObjects(ObjectType.Element, new CategoryFilter(BuiltInCategory.OST_StructuralFraming)).Select(c => c.ElementId);
            if (beamIds == null || beamIds.Count() == 0)
                return Result.Cancelled;
            //业务逻辑处理
            TransactionHelper.DelegateTransaction(doc, "梁齐板", () =>
            {
                OutLineManager0802 collector = new OutLineManager0802(doc, model);
                //对板按高程从高到底处理
                List<LevelFloor> levelFloors = new List<LevelFloor>();
                foreach (var floorId in floorIds)
                {
                    var floor = doc.GetElement(floorId) as Floor;
                    var level = doc.GetElement(floor.LevelId) as Level;
                    levelFloors.Add(new LevelFloor(level.Elevation, floor));
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
                        var fixedSP0 = GeometryHelper.VL_GetIntersectionOnLine(sp0, beamLine.GetEndPoint(0), beamLine.Direction);
                        var fixedSP1 = GeometryHelper.VL_GetIntersectionOnLine(sp1, beamLine.GetEndPoint(0), beamLine.Direction);
                        var sectionBeam = doc.Create.NewFamilyInstance(Line.CreateBound(fixedSP0, fixedSP1), beamSymbol, beamLevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                        collector.CreatedBeams.Add(sectionBeam);
                    }
                    collector.LinkBeamWithAngleGT180(beam);
                    doc.Delete(beam.Id);
                }
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

            return Result.Succeeded;
        }
    }
}
