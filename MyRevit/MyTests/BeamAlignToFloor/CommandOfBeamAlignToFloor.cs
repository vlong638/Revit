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
                OutLineManager collector = new OutLineManager(doc, model);
                //添加板
                foreach (var floorId in floorIds)
                {
                    var floor = doc.GetElement(floorId) as Floor;
                    collector.Add(floor);
                }
                //计算梁的偏移处理
                foreach (var beamId in beamIds)
                {
                    var beam = doc.GetElement(beamId);
                    var fitLineCollection = collector.Fit(beam);
                    var seperatePoints = collector.Merge(fitLineCollection);
                    //collector.Adapt(beam, fitLine);

                    //绘图分析
                    GraphicsDisplayerManager.Display(@"E:\WorkingSpace\Outputs\Images\display2.png", seperatePoints, collector.LeveledOutLines);
                }
                return true;
            });

            return Result.Succeeded;
        }
    }
}
