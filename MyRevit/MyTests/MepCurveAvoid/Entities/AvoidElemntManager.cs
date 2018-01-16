using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using MyRevit.MyTests.Utilities;
using System.Collections.Generic;
using System.Linq;
using MyRevit.Utilities;
using System;
using PmSoft.Common.RevitClass.Utils;
using Autodesk.Revit.UI;
using PmSoft.MepProject.ControlService;

namespace MyRevit.MyTests.MepCurveAvoid
{
    /// <summary>
    /// 避让统筹管理
    /// 这里存在两批模型的概念
    /// Revit中的基础模型
    /// 非Revit的价值模型
    /// </summary>
    public class AvoidElemntManager
    {
        /// <summary>
        ///基础模型
        /// </summary>
        List<AvoidElement> AvoidElements = new List<AvoidElement>();
        /// <summary>
        /// 价值模型
        /// </summary>
        List<ValuedConflictNode> ValuedConflictNodes = new List<ValuedConflictNode>();
        /// <summary>
        /// 区块迁移
        /// </summary>
        List<ConflictLineSections> ConflictLineSections_Collection = new List<ConflictLineSections>();
        /// <summary>
        /// 连接点位
        /// </summary>
        List<ConnectionNode> ConnectionNodes = new List<ConnectionNode>();
        private UIApplication uiApp;

        public AvoidElemntManager(UIApplication uiApp)
        {
            this.uiApp = uiApp;
        }

        /// <summary>
        /// 元素按分类添加
        /// </summary>
        /// <param name="elements"></param>
        public void AddElements(List<Element> elements)
        {
            AvoidElements.AddRange(elements.Where(c => c is Pipe).Select(c => new AvoidElement(c as MEPCurve, AvoidElementType.Pipe)));
            AvoidElements.AddRange(elements.Where(c => c is Duct).Select(c => new AvoidElement(c as MEPCurve, AvoidElementType.Duct)));
            AvoidElements.AddRange(elements.Where(c => c is Conduit).Select(c => new AvoidElement(c as MEPCurve, AvoidElementType.Conduit)));
            AvoidElements.AddRange(elements.Where(c => c is CableTray).Select(c => new AvoidElement(c as MEPCurve, AvoidElementType.CableTray)));
        }

        /// <summary>
        /// 碰撞检测
        /// </summary>
        public void CheckConflict()
        {
            //基本碰撞&&构建碰撞网络
            foreach (var avoidElement in AvoidElements)
                avoidElement.SetConflictElements(AvoidElements, ValuedConflictNodes);
            //价值分组
            for (int i = ValuedConflictNodes.Count() - 1; i >= 0; i--)
            {
                var conflictNode = ValuedConflictNodes[i];
                conflictNode.Grouping(ValuedConflictNodes, AvoidElements);
                conflictNode.Valuing(ValuedConflictNodes, AvoidElements);
            }
            //价值对抗(按照优势者优先原则) 价值模型
            //由组队团体进行团体对抗
            //按照优势团体优先的原则进行对抗
            List<ValueNode> ValueNodes = new List<ValueNode>();
            foreach (var ValuedConflictNode in ValuedConflictNodes)
            {
                ValueNodes.Add(ValuedConflictNode.ValueNode1);
                ValueNodes.Add(ValuedConflictNode.ValueNode2);
            }
            ValueNodes = ValueNodes.OrderBy(c => c.ConflictLineSections.AvoidPriorityValue, new PriorityValueComparer()).ToList();
            for (int i = ValueNodes.Count() - 1; i >= 0; i--)
            {
                var ValueNode = ValueNodes[i];
                ValueNode.ValuedConflictNode.Compete(AvoidElements, ConflictLineSections_Collection);
            }
            var winners = ValueNodes.Where(c => c.ConflictLineSections.AvoidPriorityValue.CompeteType == CompeteType.Winner);
            PmSoft.Optimization.DrawingProduction.Utils.GraphicsDisplayerManager.Display(@"E:\WorkingSpace\Outputs\Images\AvoidElement.png", AvoidElements, winners);
        }

        /// <summary>
        /// 避让处理
        /// </summary>
        /// <param name="doc"></param>
        public void AutoAvoid(Document doc)
        {
            #region 管线重构
            foreach (var AvoidElement in AvoidElements)
                RebuiltCurves(doc, AvoidElement);
            #endregion

            #region 连接件迁移
            foreach (var ConflictLineSections in ConflictLineSections_Collection)
                TransportConnections(doc, ConflictLineSections);
            #endregion

            #region 连接点补充连接件
            var m_Factory = new MepControlServiceFactory(uiApp);
            var m_MEPCurveCS = m_Factory.IMEPCurveCS;
            foreach (var ConnectionNode in ConnectionNodes)
                m_MEPCurveCS.NewElbowDefault(ConnectionNode.MEPCurve1, ConnectionNode.MEPCurve2, null);
            #endregion

            doc.Regenerate();
        }

        /// <summary>
        /// 连接件迁移
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="ConflictLineSections"></param>
        private static void TransportConnections(Document doc, ConflictLineSections ConflictLineSections)
        {
            List<ElementId> elementsToMove = new List<ElementId>();
            var height = ConflictLineSections.Height;
            var vector = new XYZ(0, 0, 1);
            foreach (var ConflictLineSection in ConflictLineSections)
            {
                if (ConflictLineSection.NewStartElementId != null)
                {
                    //复位
                    ElementTransformUtils.MoveElement(doc, ConflictLineSection.NewStartElementId, -height * vector);
                    elementsToMove.Add(ConflictLineSection.NewStartElementId);
                    //修复连接关系
                    var element = doc.GetElement(ConflictLineSection.NewStartElementId);
                    var curve = (element.Location as LocationCurve).Curve;
                    var middle = (curve.GetEndPoint(0) + curve.GetEndPoint(1)) / 2;
                    Connector start, end;
                    AvoidElement.GetStartAndEndConnector(element as MEPCurve, middle, out start, out end);
                    start.ConnectTo(ConflictLineSection.StartLinkedConnector);
                }
                if (ConflictLineSection.NewEndElementId != null)
                {
                    //复位
                    ElementTransformUtils.MoveElement(doc, ConflictLineSection.NewEndElementId, -height * vector);
                    elementsToMove.Add(ConflictLineSection.NewEndElementId);
                    //修复连接关系
                    var element = doc.GetElement(ConflictLineSection.NewEndElementId);
                    var curve = (element.Location as LocationCurve).Curve;
                    var middle = (curve.GetEndPoint(0) + curve.GetEndPoint(1)) / 2;
                    Connector start, end;
                    AvoidElement.GetStartAndEndConnector(element as MEPCurve, middle, out start, out end);
                    end.ConnectTo(ConflictLineSection.EndLinkedConnector);
                }
            }
            //二次复位
            if (elementsToMove.Count() > 0)
                ElementTransformUtils.MoveElements(doc, elementsToMove, height * vector);
        }

        /// <summary>
        /// 重构管线位置
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="avoidElement"></param>
        private void RebuiltCurves(Document doc, AvoidElement avoidElement)
        {
            int id = avoidElement.MEPElement.Id.IntegerValue;
            var source = avoidElement.ConflictElements.Where(c => c.CompeteType == CompeteType.Winner).ToList();
            if (source.Count == 0)
                return;
            XYZ startPoint = avoidElement.StartPoint;
            bool isContinuing = false;
            XYZ startSplit = null;
            XYZ middleStart = null;
            XYZ middleEnd = null;
            XYZ endSplit = null;
            XYZ endPoint = avoidElement.EndPoint;

            for (int i = 0; i < source.Count(); i++)
            {
                var currentConflictEle = source[i];
                if (currentConflictEle.CompeteType != CompeteType.Winner)
                    continue;

                //1.连续性
                var isContinue = (i < source.Count() - 1) && currentConflictEle.GroupId == source[i + 1].GroupId;//不是最后一个 且和下一个是一组
                if (isContinue)
                {
                    if (!isContinuing)
                    {
                        isContinuing = true;
                        if (currentConflictEle.IsConnector)
                            startPoint = currentConflictEle.ConnectorLocation;
                        startSplit = currentConflictEle.StartSplit;
                        middleStart = currentConflictEle.MiddleStart;
                    }
                }
                else
                {
                    //边界连接件点位变更
                    if (currentConflictEle.IsConnector)
                    {
                        if (i == 0 && i == source.Count() - 1)//是起始点 又是终结点
                            if (avoidElement.IsStartPoint(currentConflictEle))//avoidElement.StartPoint.VL_XYEqualTo(currentConflictEle.ConflictLocation))//拓扑时碰撞点即起始点
                                startPoint = currentConflictEle.ConnectorLocation;
                            else
                                endPoint = currentConflictEle.ConnectorLocation;
                        else if (i == 0)//起始点连接件
                            startPoint = currentConflictEle.ConnectorLocation;
                        else if (i == source.Count() - 1)//终结点连接件
                            endPoint = currentConflictEle.ConnectorLocation;
                    }
                    if (i == 0 && currentConflictEle.IsConnector && avoidElement.IsStartPoint(currentConflictEle))//起始点 单个连接件 Connector是否是起始点
                    {
                        //单点避让
                        startPoint = currentConflictEle.ConnectorLocation;
                        middleEnd = currentConflictEle.MiddleEnd;
                        endSplit = currentConflictEle.EndSplit;
                        var offsetMep = doc.GetElement(ElementTransformUtils.CopyElement(doc, avoidElement.MEPElement.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                        (offsetMep.Location as LocationCurve).Curve = Line.CreateBound(startPoint, middleEnd);
                        var leanEnd = doc.GetElement(ElementTransformUtils.CopyElement(doc, avoidElement.MEPElement.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                        (leanEnd.Location as LocationCurve).Curve = Line.CreateBound(middleEnd, endSplit);
                            //连接件补充
                        AddConnectionNode(offsetMep, leanEnd);
                        if (i == source.Count() - 1)
                        {
                            var mepEnd = doc.GetElement(ElementTransformUtils.CopyElement(doc, avoidElement.MEPElement.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                            (mepEnd.Location as LocationCurve).Curve = Line.CreateBound(endSplit, endPoint);
                            //连接件补充
                            AddConnectionNode(leanEnd, mepEnd);
                        }
                        //连接件迁移
                        foreach (var ConflictLineSections in ConflictLineSections_Collection)
                        {
                            var lineSection = ConflictLineSections.FirstOrDefault(d => d.ElementId == avoidElement.MEPElement.Id && d.StartPoint != null && d.StartPoint.VL_XYEqualTo(startPoint));
                            if (lineSection != null)
                            {
                                lineSection.NewStartElementId = offsetMep.Id;
                                break;
                            }
                        }
                    }
                    else if (i == source.Count() - 1 && currentConflictEle.IsConnector && avoidElement.IsEndPoint(currentConflictEle))//终结点 单个连接件
                    {
                        if (isContinuing) //连续避让
                        {
                            isContinuing = false;
                            if ((startPoint.Z - endPoint.Z).IsMiniValue())//前后边界都是Connector
                            {
                                var fullmep = doc.GetElement(ElementTransformUtils.CopyElement(doc, avoidElement.MEPElement.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                                (fullmep.Location as LocationCurve).Curve = Line.CreateBound(startPoint, endPoint);
                                //连接件迁移
                                foreach (var ConflictLineSections in ConflictLineSections_Collection)
                                {
                                    var lineSection = ConflictLineSections.FirstOrDefault(d => d.ElementId == avoidElement.MEPElement.Id && d.StartPoint != null && d.StartPoint.VL_XYEqualTo(startPoint) && d.EndPoint != null && d.EndPoint.VL_XYEqualTo(endPoint));
                                    if (lineSection != null)
                                    {
                                        lineSection.NewStartElementId = fullmep.Id;
                                        lineSection.NewEndElementId = fullmep.Id;
                                        break;
                                    }
                                }
                                continue;
                            }
                        }
                        else
                        {
                            //单点避让
                            startSplit = currentConflictEle.StartSplit;
                            middleStart = currentConflictEle.MiddleStart;
                            endPoint = currentConflictEle.ConnectorLocation;
                        }
                        var mepStart = doc.GetElement(ElementTransformUtils.CopyElement(doc, avoidElement.MEPElement.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                        (mepStart.Location as LocationCurve).Curve = Line.CreateBound(startPoint, startSplit);
                        var leanStart = doc.GetElement(ElementTransformUtils.CopyElement(doc, avoidElement.MEPElement.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                        (leanStart.Location as LocationCurve).Curve = Line.CreateBound(startSplit, middleStart);
                        var offsetMep = doc.GetElement(ElementTransformUtils.CopyElement(doc, avoidElement.MEPElement.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                        (offsetMep.Location as LocationCurve).Curve = Line.CreateBound(middleStart, endPoint);
                        //连接件补充
                        AddConnectionNode(mepStart, leanStart);
                        AddConnectionNode(leanStart, offsetMep);
                        //连接件迁移
                        foreach (var ConflictLineSections in ConflictLineSections_Collection)
                        {
                            var lineSection = ConflictLineSections.FirstOrDefault(d => d.ElementId == avoidElement.MEPElement.Id && d.EndPoint != null && d.EndPoint.VL_XYEqualTo(endPoint));
                            if (lineSection != null)
                            {
                                lineSection.NewEndElementId = offsetMep.Id;
                                break;
                            }
                        }
                    }
                    else//单点避让
                    {
                        if (isContinuing)
                        {
                            //连续避让
                            isContinuing = false;
                        }
                        else
                        {
                            //单点避让
                            startSplit = currentConflictEle.StartSplit;
                            middleStart = currentConflictEle.MiddleStart;
                        }
                        middleEnd = currentConflictEle.MiddleEnd;
                        endSplit = currentConflictEle.EndSplit;
                        var compare = new XYZComparer();
                        if (compare.Compare(startPoint, startSplit) <= 0)
                        {
                            var offsetMep = doc.GetElement(ElementTransformUtils.CopyElement(doc, avoidElement.MEPElement.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                            (offsetMep.Location as LocationCurve).Curve = Line.CreateBound(startPoint, middleEnd);
                            var leanMepEnd = doc.GetElement(ElementTransformUtils.CopyElement(doc, avoidElement.MEPElement.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                            (leanMepEnd.Location as LocationCurve).Curve = Line.CreateBound(middleEnd, endSplit);
                            //连接件补充
                            AddConnectionNode(offsetMep, leanMepEnd);
                        }
                        else
                        {
                            var mepStart = doc.GetElement(ElementTransformUtils.CopyElement(doc, avoidElement.MEPElement.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                            (mepStart.Location as LocationCurve).Curve = Line.CreateBound(startPoint, startSplit);
                            var leanMepStart = doc.GetElement(ElementTransformUtils.CopyElement(doc, avoidElement.MEPElement.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                            (leanMepStart.Location as LocationCurve).Curve = Line.CreateBound(startSplit, middleStart);
                            //连接件补充
                            AddConnectionNode(mepStart, leanMepStart);
                            if (i == source.Count() - 1)
                            {
                                if (compare.Compare(endPoint, endSplit) >= 0)
                                {
                                    var offsetMep = doc.GetElement(ElementTransformUtils.CopyElement(doc, avoidElement.MEPElement.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                                    (offsetMep.Location as LocationCurve).Curve = Line.CreateBound(middleStart, endPoint);
                                    //连接件补充
                                    AddConnectionNode(leanMepStart, offsetMep);
                                }
                                else
                                {
                                    var offsetMep = doc.GetElement(ElementTransformUtils.CopyElement(doc, avoidElement.MEPElement.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                                    (offsetMep.Location as LocationCurve).Curve = Line.CreateBound(middleStart, middleEnd);
                                    var leanMepEnd = doc.GetElement(ElementTransformUtils.CopyElement(doc, avoidElement.MEPElement.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                                    (leanMepEnd.Location as LocationCurve).Curve = Line.CreateBound(middleEnd, endSplit);
                                    var mepEnd = doc.GetElement(ElementTransformUtils.CopyElement(doc, avoidElement.MEPElement.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                                    (mepEnd.Location as LocationCurve).Curve = Line.CreateBound(endSplit, endPoint);
                                    //连接件补充
                                    AddConnectionNode(leanMepStart, offsetMep);
                                    AddConnectionNode(offsetMep, leanMepEnd);
                                    AddConnectionNode(leanMepEnd, mepEnd);
                                }
                            }
                            else
                            {
                                var offsetMep = doc.GetElement(ElementTransformUtils.CopyElement(doc, avoidElement.MEPElement.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                                (offsetMep.Location as LocationCurve).Curve = Line.CreateBound(middleStart, middleEnd);
                                var leanMepEnd = doc.GetElement(ElementTransformUtils.CopyElement(doc, avoidElement.MEPElement.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                                (leanMepEnd.Location as LocationCurve).Curve = Line.CreateBound(middleEnd, endSplit);
                                //连接件补充
                                AddConnectionNode(leanMepStart, offsetMep);
                                AddConnectionNode(offsetMep, leanMepEnd);
                            }
                        }
                    }
                    startPoint = endSplit;
                }
            }
            doc.Delete(avoidElement.MEPElement.Id);
        }

        /// <summary>
        /// 连接件补充
        /// </summary>
        /// <param name="offsetMep"></param>
        /// <param name="leanEnd"></param>
        private void AddConnectionNode(MEPCurve offsetMep, MEPCurve leanEnd)
        {
            ConnectionNodes.Add(new ConnectionNode(offsetMep, leanEnd));
        }

        #region 连接件生成
        //private enum MEPCurveConnectTypeENUM { MultiShapeTransition, Transition, Elbow, Tee, Cross, TakeOff }

        ///// <summary>
        ///// 获取Pipe和Duct的系统默认连接项设置
        ///// </summary>
        ///// <param name="src"></param>
        ///// <returns></returns>
        //public static MEPCurveType GetMEPCurveType(this MEPCurve src)
        //{
        //    if (src is Pipe)
        //        return (src as Pipe).PipeType;
        //    else if (src is Duct)
        //        return (src as Duct).DuctType;
        //    else if (src is Conduit)
        //        return src.Document.GetElement(src.GetTypeId()) as ConduitType;
        //    else if (src is CableTray)
        //        return src.Document.GetElement(src.GetTypeId()) as CableTrayType;
        //    else
        //        return null;
        //}
        ///// <summary>
        ///// 获取宽度
        ///// </summary>
        ///// <param name="src"></param>
        ///// <returns>单位为英制</returns>
        //public static double GetWidth(this MEPCurve src)
        //{
        //    Connector con = src.ConnectorManager.Lookup(0);
        //    if (con == null || con.Shape == ConnectorProfileType.Invalid)
        //        throw new Exception("无法获取宽度");
        //    else
        //    {
        //        if (con.Shape == ConnectorProfileType.Round)
        //            return con.Radius * 2;
        //        else
        //            return con.Width;
        //    }
        //}
        ///// <summary>
        ///// 获取MEPCurve的默认连接管件族
        ///// </summary>
        ///// <param name="src"></param>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //public static FamilySymbol GetDefaultFittingSymbol(this MEPCurve src, RoutingPreferenceRuleGroupType type)
        //{
        //    FamilySymbol fs = null;
        //    if (src is Pipe || src is Duct)
        //    {
        //        RoutingPreferenceManager rpm = GetMEPCurveType(src).RoutingPreferenceManager;

        //        int num = rpm.GetNumberOfRules(type);
        //        for (int i = 0; i < num; i++)
        //        {
        //            RoutingPreferenceRule rpr = rpm.GetRule(type, i);
        //            if (null != rpr && rpr.MEPPartId != ElementId.InvalidElementId)
        //            {
        //                PrimarySizeCriterion criterion = null;
        //                if (src is Pipe)
        //                {
        //                    if (rpr.NumberOfCriteria == 0 || (criterion = rpr.GetCriterion(0) as PrimarySizeCriterion) == null || GetWidth(src) > criterion.MaximumSize || GetWidth(src) < criterion.MinimumSize)
        //                        continue;
        //                }

        //                if (rpr.MEPPartId == null || rpr.MEPPartId == ElementId.InvalidElementId)
        //                    continue;

        //                fs = src.Document.GetElement(rpr.MEPPartId) as FamilySymbol;
        //                break;
        //            }
        //        }
        //    }
        //    else if (src is Conduit)
        //    {
        //        var text = "";
        //        switch (type)
        //        {
        //            case RoutingPreferenceRuleGroupType.Elbows:
        //                text = "弯头";
        //                break;
        //            case RoutingPreferenceRuleGroupType.Junctions:
        //                text = "T 形三通";
        //                break;
        //            case RoutingPreferenceRuleGroupType.Crosses:
        //                text = "交叉线";
        //                break;
        //            case RoutingPreferenceRuleGroupType.Transitions:
        //                text = "过渡件";
        //                break;
        //            case RoutingPreferenceRuleGroupType.Unions:
        //                text = "活接头";
        //                break;
        //        }
        //        var param = (src.Document.GetElement(src.GetTypeId())).GetParameters(text);
        //        if (param.Count != 0)
        //            fs = src.Document.GetElement(param.First().AsElementId()) as FamilySymbol;
        //    }
        //    else if (src is CableTray)
        //    {
        //        var text = "";
        //        switch (type)
        //        {
        //            case RoutingPreferenceRuleGroupType.Elbows:
        //                text = "水平弯头";
        //                break;
        //            case RoutingPreferenceRuleGroupType.Junctions:
        //                text = "T 形三通";
        //                break;
        //            case RoutingPreferenceRuleGroupType.Crosses:
        //                text = "交叉线";
        //                break;
        //            case RoutingPreferenceRuleGroupType.Transitions:
        //                text = "过渡件";
        //                break;
        //            case RoutingPreferenceRuleGroupType.Unions:
        //                text = "活接头";
        //                break;
        //        }

        //        var t = new FilteredElementCollector(src.Document).OfClass(typeof(CableTrayType)).FirstOrDefault(p => p.GetParameters("族名称").First().AsString() == "带配件的电缆桥架");
        //        if (t != null)
        //        {
        //            var param = t.GetParameters(text);
        //            if (param.Count != 0)
        //                fs = src.Document.GetElement(param.First().AsElementId()) as FamilySymbol;
        //        }

        //    }

        //    return fs;
        //}
        //public static FamilySymbol GetDefaultTakeoffFittingSymbol(this MEPCurve src)
        //{
        //    FamilySymbol fs = null;

        //    RoutingPreferenceManager rpm = GetMEPCurveType(src).RoutingPreferenceManager;

        //    int num = rpm.GetNumberOfRules(RoutingPreferenceRuleGroupType.Junctions);
        //    for (int i = 0; i < num; i++)
        //    {
        //        RoutingPreferenceRule rpr = rpm.GetRule(RoutingPreferenceRuleGroupType.Junctions, i);
        //        if (null != rpr && rpr.MEPPartId != ElementId.InvalidElementId)
        //        {
        //            var tempFs = src.Document.GetElement(rpr.MEPPartId) as FamilySymbol;
        //            if (tempFs.Family.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE).AsInteger() == 10)//接头 - 垂直
        //                fs = tempFs;
        //            break;
        //        }
        //    }
        //    return fs;
        //}
        ///// <summary>
        ///// 获取设置文件中管件名称 
        ///// </summary>
        ///// <param name="src"></param>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //private string GetDefaultFittingName(MEPCurve src, MEPCurveConnectTypeENUM type)
        //{
        //    string searchText = "/Root/";
        //    if (src is Pipe)
        //    {
        //        searchText += "Pipe/";
        //    }
        //    else if (src is Duct)
        //    {
        //        var con = src.ConnectorManager.Lookup(0);
        //        if (con.Shape == ConnectorProfileType.Round)
        //            searchText += "Duct/圆形/";
        //        else if (con.Shape == ConnectorProfileType.Rectangular)
        //            searchText += "Duct/矩形/";
        //        else if (con.Shape == ConnectorProfileType.Oval)
        //            searchText += "Duct/椭圆/";
        //    }
        //    else if (src is Conduit)
        //        searchText += "Conduit/";
        //    else if (src is CableTray)
        //        searchText += "CableTray/";

        //    switch (type)
        //    {
        //        case MEPCurveConnectTypeENUM.Elbow://弯头
        //            searchText += "弯头"; break;
        //        case MEPCurveConnectTypeENUM.Tee://三通
        //            searchText += "三通"; break;
        //        case MEPCurveConnectTypeENUM.Cross://四通
        //            searchText += "四通"; break;
        //        case MEPCurveConnectTypeENUM.Transition://过渡件
        //            searchText += "过渡件"; break;
        //        case MEPCurveConnectTypeENUM.TakeOff://侧接
        //            searchText += "侧接"; break;
        //        default:
        //            break;
        //    }

        //    System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
        //    xml.Load(FamilyLoadUtils.FamilyLibPath + "\\..\\..\\SysData\\MEPCurveConnect.xml");
        //    var node = xml.SelectSingleNode(searchText);
        //    if (node == null)
        //        return null;
        //    else
        //        return node.InnerText;
        //}
        //bool IsOnlyUseRevitDefault { get; set; }
        ///// <summary>
        ///// 检测系统中是否有默认连接项，无则进行添加
        ///// </summary>
        ///// <param name="src"></param>
        ///// <param name="type"></param>
        //private FamilySymbol Judge_LoadDefaultFitting(MEPCurve src, MEPCurveConnectTypeENUM type)
        //{
        //    FamilySymbol fs = null;
        //    switch (type)
        //    {
        //        case MEPCurveConnectTypeENUM.Elbow://弯头
        //            fs = GetDefaultFittingSymbol(src,RoutingPreferenceRuleGroupType.Elbows); break;
        //        case MEPCurveConnectTypeENUM.Tee://三通
        //            fs = GetDefaultFittingSymbol(src, RoutingPreferenceRuleGroupType.Junctions); break;
        //        case MEPCurveConnectTypeENUM.Cross://四通
        //            fs = GetDefaultFittingSymbol(src, RoutingPreferenceRuleGroupType.Crosses); break;
        //        case MEPCurveConnectTypeENUM.Transition://过渡件
        //            fs = GetDefaultFittingSymbol(src, RoutingPreferenceRuleGroupType.Transitions); break;
        //        case MEPCurveConnectTypeENUM.TakeOff://侧接
        //            fs = GetDefaultTakeoffFittingSymbol(src); break;
        //        default:
        //            fs = null;
        //            break;
        //    }

        //    if (fs != null)
        //        return fs;

        //    if (this.IsOnlyUseRevitDefault)
        //        return null;

        //    var familyName = this.GetDefaultFittingName(src, type);
        //    if (familyName == null)
        //        return null;

        //    fs = FamilyLoadUtils.FindFamilySymbol_SubTransaction(this.RevitDoc, familyName, null);
        //    if (fs == null)
        //        return null;
        //    if (src is Pipe || src is Duct)
        //    {
        //        RoutingPreferenceManager rpm = GetMEPCurveType(src).RoutingPreferenceManager;
        //        var rule = new RoutingPreferenceRule(fs.Id, "");
        //        rule.AddCriterion(PrimarySizeCriterion.All());

        //        switch (type)
        //        {
        //            case MEPCurveConnectTypeENUM.Elbow://弯头
        //                rpm.AddRule(RoutingPreferenceRuleGroupType.Elbows, rule); break;
        //            case MEPCurveConnectTypeENUM.Tee://三通
        //                rpm.AddRule(RoutingPreferenceRuleGroupType.Junctions, rule); break;
        //            case MEPCurveConnectTypeENUM.Cross://四通
        //                rpm.AddRule(RoutingPreferenceRuleGroupType.Crosses, rule); break;
        //            case MEPCurveConnectTypeENUM.Transition://过渡件
        //                rpm.AddRule(RoutingPreferenceRuleGroupType.Transitions, rule); break;
        //            case MEPCurveConnectTypeENUM.TakeOff://侧接
        //                rpm.AddRule(RoutingPreferenceRuleGroupType.Junctions, rule); break;
        //            default:
        //                break;
        //        }

        //    }
        //    else if (src is Conduit)
        //    {
        //        Parameter param = null;

        //        switch (type)
        //        {
        //            case MEPCurveConnectTypeENUM.Elbow://弯头
        //                param = (src.Document.GetElement(src.GetTypeId())).GetParameters("弯头").FirstOrDefault(); break;
        //            case MEPCurveConnectTypeENUM.Tee://三通
        //                param = (src.Document.GetElement(src.GetTypeId())).GetParameters("T 形三通").FirstOrDefault(); break;
        //            case MEPCurveConnectTypeENUM.Cross://四通
        //                param = (src.Document.GetElement(src.GetTypeId())).GetParameters("交叉线").FirstOrDefault(); break;
        //            case MEPCurveConnectTypeENUM.Transition://过渡件
        //                param = (src.Document.GetElement(src.GetTypeId())).GetParameters("过渡件").FirstOrDefault(); break;
        //            default:
        //                break;
        //        }

        //        if (param != null)
        //        {
        //            param.Set(fs.Id);
        //        }
        //    }
        //    else if (src is CableTray)
        //    {
        //        Parameter param = null;
        //        var t = new FilteredElementCollector(src.Document).OfClass(typeof(CableTrayType)).FirstOrDefault(p => p.GetParameters("族名称").First().AsString() == "带配件的电缆桥架");
        //        switch (type)
        //        {
        //            case MEPCurveConnectTypeENUM.Elbow://弯头
        //                param = t.GetParameters("水平弯头").FirstOrDefault(); break;
        //            case MEPCurveConnectTypeENUM.Tee://三通
        //                param = t.GetParameters("T 形三通").FirstOrDefault(); break;
        //            case MEPCurveConnectTypeENUM.Cross://四通
        //                param = t.GetParameters("交叉线").FirstOrDefault(); break;
        //            case MEPCurveConnectTypeENUM.Transition://过渡件
        //                param = t.GetParameters("过渡件").FirstOrDefault(); break;
        //            default:
        //                break;
        //        }

        //        if (param != null)
        //        {
        //            param.Set(fs.Id);
        //        }
        //    }
        //    return fs;
        //}
        //public FamilyInstance NewElbowDefault(MEPCurve src, Connector connector, FamilySymbol fs)
        //{
        //    this.Judge_LoadDefaultFitting(src, MEPCurveConnectTypeENUM.Elbow);
        //    this.Judge_LoadDefaultFitting(src, MEPCurveConnectTypeENUM.Transition);

        //    FamilyInstance fi = null;
        //    XYZ locTemp = connector.Origin;
        //    MEPCurveConnectTypeENUM type = MEPCurveConnectTypeENUM.Transition;
        //    try
        //    {
        //        var connectorSrc = src.GetClosestConnector(connector.Origin);

        //        JudgeAndThrow(new Connector[] { connectorSrc, connector }, fs, src);

        //        //假如平行或者在用同一条直线上
        //        if (connectorSrc.CoordinateSystem.BasisZ.IsAlmostEqualTo(connector.CoordinateSystem.BasisZ) || connectorSrc.CoordinateSystem.BasisZ.IsAlmostEqualTo(-connector.CoordinateSystem.BasisZ))
        //        {
        //            type = MEPCurveConnectTypeENUM.Transition;
        //            return fi = this.RevitDoc.Create.NewTransitionFitting(connectorSrc, connector);
        //        }
        //        //存在角度
        //        else
        //        {
        //            type = MEPCurveConnectTypeENUM.Elbow;
        //            if (src is Duct && connector.Owner is FamilyInstance)//在风管和风口连接时进行特殊处理
        //            {
        //                XYZ closestXYZ = src.GetClosestPoint(connector.Origin);
        //                //Duct duct = this.RevitDoc.Create.NewDuct(closestXYZ, connector, (src as Duct).DuctType);
        //                Duct duct = Duct.Create(this.RevitDoc, src.GetParameters("系统类型").First().AsElementId(), (src as Duct).DuctType.Id, src.GetParameters("参照标高").First().AsElementId(), connector.Origin, closestXYZ);
        //                duct.SetWidthAndHeight(connector, true);
        //                src.Document.Regenerate();
        //                duct.GetClosestConnector(connector.Origin).ConnectTo(connector);

        //                try
        //                {
        //                    return fi = this.RevitDoc.Create.NewElbowFitting(duct.GetClosestConnector(closestXYZ), connectorSrc);
        //                }
        //                catch
        //                {
        //                    return null;
        //                }
        //            }
        //            else//其他情况
        //                return fi = this.RevitDoc.Create.NewElbowFitting(connector, connectorSrc);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        base.log.AddLog(ex);
        //        this.ThrowExceptionDefault(ex, src, type);
        //        return fi;
        //    }
        //    finally
        //    {
        //        fi.WriteParm(src.GetParmType(), false);
        //        fi.WriteParm_ConnectedFitting(src.GetParmType());
        //    }
        //} 
        #endregion
    }
}
