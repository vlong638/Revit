using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.MyTests.MepCurveAvoid;
using MyRevit.MyTests.Utilities;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.Entities
{
    ///// <summary>
    ///// 碰撞节点
    ///// </summary>
    //public class ConflictNode
    //{
    //    public XYZ ConflictPoint { set; get; }
    //    public AvoidMEPElement MEPElement { set; get; }
    //    public ConflictNodeDealType DealType { set; get; }

    //    //点位计算
    //    public XYZ Start { set; get; }//起始点
    //    public XYZ StartSplit { set; get; }//起始点切割端
    //    public XYZ MiddleStart { set; get; }//中间段起始点
    //    public XYZ MiddleEnd { set; get; }//中间段终结点
    //    public XYZ EndSplit { set; get; }//终结点切割端
    //    public XYZ End { set; get; }//终结点
    //}


    [Transaction(TransactionMode.Manual)]
    public class 调研_类型区分_碰撞检测 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;

            var elementIds = uiDoc.Selection.PickObjects(ObjectType.Element, new VLClassesFilter(false,
                typeof(Pipe), typeof(Duct), typeof(CableTray), typeof(Conduit)
                ), "选择要添加的构件").Select(c => c.ElementId);
            if (elementIds.Count() == 0)
                return Result.Cancelled;

            var selectedElements = elementIds.Select(c => doc.GetElement(c)).ToList();
            AvoidElemntManager manager = new AvoidElemntManager();
            manager.AddElements(selectedElements);
            manager.CheckConflict();

            return Result.Succeeded;
        }
    }
}
