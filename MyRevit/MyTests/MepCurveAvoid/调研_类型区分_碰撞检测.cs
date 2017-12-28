using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.MyTests.Utilities;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.Entities
{
    /// <summary>
    /// 避让元素类型
    /// </summary>
    public enum AvoidElementType
    {
        None,
        Pipe,
        Duct,
        CableTray,
        Conduit,
    }

    /// <summary>
    /// 碰撞元素
    /// </summary>
    public class ConflictElement
    {
        public ConflictElement(AvoidMEPElement element, XYZ conflictPoint)
        {
            AvoidElement = element;
            ConflictPoint = conflictPoint;
        }

        public AvoidMEPElement AvoidElement { set; get; }
        public XYZ ConflictPoint { set; get; }
    }

    /// <summary>
    /// 碰撞节点
    /// </summary>
    public class ConflictNode
    {
        public Element[] Elements { set; get; }
        public XYZ ConflictPoint { set; get; }
    }

    /// <summary>
    /// 避让元素(MEP)
    /// </summary>
    public class AvoidMEPElement
    {
        public AvoidMEPElement(MEPCurve element, AvoidElementType avoidElementType)
        {
            MEPElement = element;
            AvoidElementType = avoidElementType;
        }

        #region 属性

        /// <summary>
        /// 主体元素
        /// </summary>
        private MEPCurve element;

        public MEPCurve MEPElement
        {
            get
            {
                return element;
            }

            set
            {
                element = value;
                if (avoidElementType != AvoidElementType.None)
                    UpdateProperties();//更改元素时
            }
        }

        #region 元素类型及所对应的参数
        /// <summary>
        /// 元素类型
        /// </summary>
        private AvoidElementType avoidElementType;
        public AvoidElementType AvoidElementType
        {
            get
            {
                return avoidElementType;
            }

            set
            {
                avoidElementType = value;
                if (avoidElementType != AvoidElementType.None)
                    UpdateProperties();//初始设置元素类型时
            }
        }
        /// <summary>
        /// 更新相关属性
        /// </summary>
        private void UpdateProperties()
        {
            UpdateSize(MEPElement);
            if (AvoidElementType == AvoidElementType.Pipe)
            {
                UpdateIsPressed(MEPElement);
                UpdateIsHot(MEPElement);
            }
            else if (AvoidElementType == AvoidElementType.Duct)
            {
                UpdateIsHeavy(MEPElement);
            }
        }

        #region Width and Height
        public double Width { set; get; }
        public double Height { set; get; }
        public void UpdateSize(Element element)
        {
            //TODO
            switch (AvoidElementType)
            {
                case AvoidElementType.Pipe:
                    Width = Height = element.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER).AsDouble();
                    break;
                case AvoidElementType.Duct:
                    break;
                case AvoidElementType.CableTray:
                    break;
                case AvoidElementType.Conduit:
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region IsPressed //For Pipe
        public bool IsPressed { set; get; }
        public static readonly string[] PipingSystem_YaLi = new string[] { "循环供水", "循环回水", "干式消防系统", "湿式消防系统", "预作用消防系统", "其他消防系统", "家用冷水", "家用热水" };
        public static readonly string[] PipingSystem_WuYaLi = new string[] { "卫生设备", "通气管", "其他" };
        public void UpdateIsPressed(Element pipe)
        {
            var p = pipe.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM);
            var value = p.AsString();
            if (PipingSystem_WuYaLi.Contains(value))
                IsPressed = false;
            else
                IsPressed = true;
        }
        #endregion

        #region IsHot //For Pipe
        public static readonly string[] NTSystem = new string[] { "循环供水", "循环回水" };
        public bool IsHot { set; get; }
        public void UpdateIsHot(Element pipe)
        {
            var p = pipe.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM);
            var value = p.AsString();
            if (NTSystem.Contains(value))
                IsHot = true;
            else
                IsHot = false;
        }
        #endregion

        #region IsHeavy For Duct
        public bool IsHeavy { set; get; }
        public void UpdateIsHeavy(Element duct)
        {
            IsHeavy = Width >= 1000;//TODO Width or Height
        }
        #endregion

        public int AvoidPriorityValue { set; get; }
        #endregion

        /// <summary>
        /// 碰撞的元素
        /// </summary>
        public List<ConflictElement> ConflictElements { set; get; }
        internal void SetConflictElements(List<AvoidMEPElement> elementsToAvoid)
        {
            ConflictElements = new List<ConflictElement>();
            ElementIntersectsElementFilter filter = new ElementIntersectsElementFilter(MEPElement);
            var conflictElements = elementsToAvoid.Where(c => filter.PassesFilter(c.MEPElement)).ToList();
            foreach (var conflictElement in conflictElements)
            {
                var conflictPoint = GetConflictPoint(conflictElement.MEPElement);
                if (conflictPoint != null)
                    ConflictElements.Add(new ConflictElement(conflictElement, conflictPoint));
            }

            //TEST
            var conflictIds = string.Join(",", conflictElements.Select(c => c.MEPElement.Id));
        }
        /// <summary>
        /// 计算线面交点
        /// 平行为null
        /// </summary>
        public XYZ GetConflictPoint(MEPCurve mepCurve)
        {
            var line = (MEPElement.Location as LocationCurve).Curve as Line;
            var lineToAvoid= (mepCurve.Location as LocationCurve).Curve as Line;
            var lineDirection1 = line.Direction;
            var lineDirection2 = lineToAvoid.Direction;
            if ((Math.Abs(lineDirection1.DotProductByCoordinate(lineDirection2, CoordinateType.XY)) - lineDirection1.GetLengthByCoordinate(CoordinateType.XY) * lineDirection2.GetLengthByCoordinate(CoordinateType.XY)).IsMiniValue())
                return null;
            Triangle triangle = new Triangle(lineToAvoid.GetEndPoint(0), lineToAvoid.GetEndPoint(1), lineToAvoid.GetEndPoint(0) + new XYZ(0, 0, 1));
            return GeometryHelper.GetIntersection(triangle, line.GetEndPoint(0), line.Direction);
        }
        #endregion

        #region 方法
        #endregion
    }

    /// <summary>
    /// 避让统筹管理
    /// </summary>
    public class AvoidElemntManager
    {
        List<AvoidMEPElement> AvoidElements = new List<AvoidMEPElement>();

        public void AddElements(List<Element> elements)
        {
            AvoidElements.AddRange(elements.Where(c => c is Pipe).Select(c => new AvoidMEPElement(c as MEPCurve, AvoidElementType.Pipe)));
            AvoidElements.AddRange(elements.Where(c => c is Duct).Select(c => new AvoidMEPElement(c as MEPCurve, AvoidElementType.Duct)));
            AvoidElements.AddRange(elements.Where(c => c is Conduit).Select(c => new AvoidMEPElement(c as MEPCurve, AvoidElementType.Conduit)));
            AvoidElements.AddRange(elements.Where(c => c is CableTray).Select(c => new AvoidMEPElement(c as MEPCurve, AvoidElementType.CableTray)));
        }

        /// <summary>
        /// 碰撞检测
        /// </summary>
        public void CheckConflict()
        {
            foreach (var avoidElement in AvoidElements)
                avoidElement.SetConflictElements(AvoidElements);
        }

        /// <summary>
        /// 碰撞结果整合
        /// </summary>
        internal void MergeConflict()
        {
            //TODO
            return;
        }

        /// <summary>
        /// 避让处理
        /// </summary>
        /// <param name="doc"></param>
        public void AutoAvoid(Document doc)
        {
            //var groupedAvoidElements = AvoidElements.GroupBy(c => c.AvoidPriorityValue).OrderBy(c => c.Key);

            var miniMepLength = UnitHelper.ConvertToFoot(96, VLUnitType.millimeter);//最短连接管长度 双向带连接件
            var miniSpace = UnitHelper.ConvertToFoot(5, VLUnitType.millimeter);//避免碰撞及提供留白的安全距离
            var angleToTurn = Math.PI / 4;//45°
            //var linkHypotenuse = -1;//TODO 连接件的斜边长度
            var orderedAvoidElements = AvoidElements.OrderBy(c => c.AvoidPriorityValue).ToList();
            for (int i = orderedAvoidElements.Count() - 1; i >= 0; i--)
            {
                var currentAvoidElement = orderedAvoidElements[i];
                var srcMep = currentAvoidElement.MEPElement;
                //TODO检查连续性
                if (i==0)//TODO 暂且作为首个进行避让测试用
                {
                    //单个避让
                    for (int j = currentAvoidElement.ConflictElements.Count() - 1; j >= 0; j--)
                    {
                        var currentElementToAvoid = currentAvoidElement.ConflictElements[j];

                        //拆分处理
                        XYZ pointStart, pointEnd;
                        var curve = (currentAvoidElement.MEPElement.Location as LocationCurve).Curve;
                        pointStart = curve.GetEndPoint(0);
                        pointEnd = curve.GetEndPoint(1);
                        //TODO 确定拆分点
                        var midPoint = currentElementToAvoid.ConflictPoint;
                        var verticalDirection = new XYZ(0, 0, 1);//TODO 由避让元素决定上下翻转
                        var parallelDirection = (pointStart - pointEnd).Normalize();//朝向Start

                        #region 点位计算公式
                        //Height=Math.Max(垂直的最短仅留白距离,构件的最短垂直间距)
                        //WidthUp=最小管道的长度/2
                        //WidthUp=Math.Max(WidthUp,水平的最短仅留白距离-构件的最短水平间距)
                        //WidthUp=Math.Max(WidthUp,考虑切边的最短距离) 
                        //WidthDown=WidthUp根据角度换算所得
                        //WidthDown=Math.Max(WidthDown,水平的最短仅留白距离)
                        #endregion

                        //max(垂直最短留白距离,最小斜边长度,最短切割距离) 
                        var height = currentAvoidElement.Height / 2 + currentElementToAvoid.AvoidElement.Height / 2 + miniSpace;
                        //height = Math.Max(height,构件的最小高度);//TODO 考虑构件的最小高度需求
                        var widthUp = miniMepLength / 2;
                        //widthUp = Math.Max(widthUp, height - 构件的最小宽度); //TODO 考虑构件的最小宽度需求
                        var diameterAvoid = Math.Max(currentAvoidElement.Width, currentAvoidElement.Height);
                        var diameterToAvoid = Math.Max(currentElementToAvoid.AvoidElement.Width, currentElementToAvoid.AvoidElement.Height);
                        widthUp = Math.Max(widthUp, (diameterAvoid / 2 + diameterToAvoid / 2 + miniSpace) / Math.Sin(angleToTurn) - height * Math.Tan(angleToTurn));
                        var widthDown = widthUp + height / Math.Tan(angleToTurn);//水平最短距离对应的水平偏移
                        widthDown = Math.Max(widthDown, currentAvoidElement.Width / 2 + currentElementToAvoid.AvoidElement.Width / 2 + miniSpace);

                        var direction1 = (curve as Line).Direction;
                        var direction2 = ((currentElementToAvoid.AvoidElement.MEPElement.Location as LocationCurve).Curve as Line).Direction;
                        var faceAngle = direction1.AngleOnPlaneTo(direction2, new XYZ(0, 0, 1));
                        widthUp = widthUp / Math.Abs(Math.Sin(faceAngle));
                        widthDown = widthDown / Math.Abs(Math.Sin(faceAngle));
                        var startSplit = midPoint + parallelDirection * widthDown;
                        var endSplit = midPoint - parallelDirection * widthDown;
                        midPoint += height * verticalDirection;
                        var midStart = midPoint + parallelDirection * widthUp;
                        var midEnd = midPoint - parallelDirection * widthUp;
                        //创建管道
                        var connector0 = srcMep.ConnectorManager.Connectors.GetConnectorById(0);
                        var connector1 = srcMep.ConnectorManager.Connectors.GetConnectorById(1);
                        Connector link0 = connector0.GetConnectedConnector();
                        if (link0 != null)
                            connector0.DisconnectFrom(link0);
                        Connector link1 = connector1.GetConnectedConnector();
                        if (link1 != null)
                            connector0.DisconnectFrom(link1);
                        bool isSameSide = (connector0.Origin - connector1.Origin).DotProduct(parallelDirection) > 0;
                        //偏移处理
                        var mepStart = doc.GetElement(ElementTransformUtils.CopyElement(doc, srcMep.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                        var mepEnd = doc.GetElement(ElementTransformUtils.CopyElement(doc, srcMep.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                        var leanMepStart = doc.GetElement(ElementTransformUtils.CopyElement(doc, srcMep.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                        var leanMepEnd = doc.GetElement(ElementTransformUtils.CopyElement(doc, srcMep.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                        var offsetMep = doc.GetElement(ElementTransformUtils.CopyElement(doc, srcMep.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
                        if (link0 != null)
                            mepStart.ConnectorManager.Connectors.GetConnectorById(0).ConnectTo(link0);
                        if (link1 != null)
                            mepEnd.ConnectorManager.Connectors.GetConnectorById(1).ConnectTo(link1);
                        doc.Delete(srcMep.Id);
                        //确定连接点,并重新连接
                        (mepStart.Location as LocationCurve).Curve = Line.CreateBound(pointStart, startSplit);
                        (leanMepStart.Location as LocationCurve).Curve = Line.CreateBound(startSplit, midStart);
                        (offsetMep.Location as LocationCurve).Curve = Line.CreateBound(midStart, midEnd);
                        (leanMepEnd.Location as LocationCurve).Curve = Line.CreateBound(midEnd, endSplit);
                        (mepEnd.Location as LocationCurve).Curve = Line.CreateBound(endSplit, pointEnd);
                        //TODO 连接件处理
                        //TODO 需转移对mep2的碰撞
                    }
                }
            }
            doc.Regenerate();
        }
    }

    public static class MEPCurveEx
    {
        #region 引用方案
        /// <summary>
        /// 获得拷贝
        /// </summary>
        /// <param name="src"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static MEPCurve GetCopy(this MEPCurve src, Document doc)
        {
            return doc.GetElement(ElementTransformUtils.CopyElement(doc, src.Id, new XYZ(0, 0, 0)).First()) as MEPCurve;
        }
        /// <summary>
        /// 获得指定Id的连接点
        /// </summary>
        /// <param name="set"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Connector GetConnectorById(this ConnectorSet set, int id)
        {
            foreach (Connector con in set)
            {
                if (id == con.Id)
                    return con;
            }

            return null;
        }
        /// <summary>
        /// 获取相连的连接点
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static Connector GetConnectedConnector(this Connector src)
        {
            if (src.ConnectorType == ConnectorType.MasterSurface || src.ConnectorType == ConnectorType.Logical)
                return null;

            if (!src.IsConnected)
                return null;

            foreach (Connector con in src.AllRefs)
            {
                if (con.IsConnectedTo(src) && con.Owner.Id != src.Owner.Id && con.Shape != ConnectorProfileType.Invalid)
                    return con;
            }

            return null;
        } 
        #endregion
    }


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
