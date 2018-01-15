using Autodesk.Revit.DB;
using MyRevit.MyTests.Utilities;
using MyRevit.Utilities;
using PmSoft.Common.RevitClass.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.MyTests.MepCurveAvoid
{
    /// <summary>
    /// 右大上大
    /// 从右到左,从上到下
    /// </summary>
    public class XYZComparer : IComparer<XYZ>
    {
        public int Compare(XYZ a, XYZ b)
        {
            if (!(a.X - b.X).IsMiniValue())
            {
                if (a.X > b.X)
                    return 1;
                if (a.X < b.X)
                    return -1;
            }
            if (!(a.Y - b.Y).IsMiniValue())
            {
                if (a.Y > b.Y)
                    return 1;
                if (a.Y < b.Y)
                    return -1;
            }
            return 0;
        }
    }


    /// <summary>
    /// 避让元素(MEP)(基础点)
    /// 录入元素的基础单元
    /// 负责在录入时整理必要的基础信息
    /// </summary>
    public class AvoidElement
    {
        public AvoidElement(MEPCurve element, AvoidElementType avoidElementType)
        {
            MEPElement = element;
            AvoidElementType = avoidElementType;
        }

        public AvoidElement(MEPCurve connectedMepElement)
        {
            MEPElement = connectedMepElement;
            //TODO AvoidElementType 类型自动识别
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
            UpdatePriorityElementType();
            var curve = (MEPElement.Location as LocationCurve).Curve;
            var p1 = curve.GetEndPoint(0);
            var p2 = curve.GetEndPoint(1);
            var middle = (p1 + p2) / 2;
            if (new XYZComparer().Compare(p1, p2) > 0)
            {
                StartPoint = p1;
                EndPoint = p2;
            }
            else
            {
                StartPoint = p2;
                EndPoint = p1;
            }
            var c1 = MEPElement.ConnectorManager.Connectors.GetConnectorById(0);
            if (c1 != null)
            {
                if (new XYZComparer().Compare(c1.Origin, middle) > 0)
                    ConnectorStart = c1;
                else
                    ConnectorEnd = c1;
            }
            var c2 = MEPElement.ConnectorManager.Connectors.GetConnectorById(1);
            if (c2 != null)
            {
                if (new XYZComparer().Compare(c2.Origin, middle) > 0)
                    ConnectorStart = c2;
                else
                    ConnectorEnd = c2;
            }
        }

        #region Width and Height
        public double Width { set; get; }
        public double Height { set; get; }
        public double GetSize()
        {
            return -1;
        }
        public void UpdateSize(Element element)
        {
            switch (AvoidElementType)
            {
                case AvoidElementType.Pipe:
                    Width = Height = element.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER).AsDouble();
                    break;
                case AvoidElementType.Duct:
                    if (IsRound(element))
                        Width = Height = element.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsDouble();
                    else
                    {
                        Width = element.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsDouble();
                        Height = element.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsDouble();
                    }
                    break;
                case AvoidElementType.CableTray:
                    Width = element.get_Parameter(BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM).AsDouble();
                    Height = element.get_Parameter(BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM).AsDouble();
                    break;
                case AvoidElementType.Conduit:
                    break;
                default:
                    break;
            }
        }

        private static bool IsRound(Element element)
        {
            var familyName = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString();
            return familyName.Contains("圆形") && !familyName.Contains("椭圆形");
        }
        #endregion

        public PriorityElementType PriorityElementType { set; get; }
        static double LargeDuctSize = UnitTransUtils.MMToFeet(800);
        static double LargePipeSize = UnitTransUtils.MMToFeet(100);
        static double LargeCableTraySize = UnitTransUtils.MMToFeet(300);

        private void UpdatePriorityElementType()
        {
            switch (AvoidElementType)
            {
                case AvoidElementType.None:
                    PriorityElementType = PriorityElementType.None;
                    break;
                case AvoidElementType.Pipe:
                    if (!IsPressed)
                        PriorityElementType = PriorityElementType.UnpressedPipe;
                    else if (Height>= LargeDuctSize)
                        PriorityElementType = PriorityElementType.LargePressedPipe;
                    else
                        PriorityElementType = PriorityElementType.NormalPressedPipe;
                    break;
                case AvoidElementType.Duct:
                    if (Height >= LargeDuctSize)
                        PriorityElementType = PriorityElementType.LargeDuct;
                    else
                        PriorityElementType = PriorityElementType.NormalDuct;
                    break;
                case AvoidElementType.CableTray:
                    if (Height >= LargeCableTraySize)
                        PriorityElementType = PriorityElementType.LargeCableTray;
                    else
                        PriorityElementType = PriorityElementType.NormalCableTray;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

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

        #region Connectors
        public Connector ConnectorStart { set; get; }
        public Connector ConnectorEnd { set; get; }
        #endregion

        #region EdgePoints
        /// <summary>
        /// 从大到小
        /// </summary>
        public XYZ StartPoint { set; get; }
        public bool IsStartPoint(ConflictElement conflictElement)
        {
            return StartPoint.VL_XYEqualTo(conflictElement.ConflictLocation);
        }
        /// <summary>
        /// 从大到小
        /// </summary>
        public XYZ EndPoint { set; get; }
        public bool IsEndPoint(ConflictElement conflictElement)
        {
            return EndPoint.VL_XYEqualTo(conflictElement.ConflictLocation);
        }
        #endregion

        #endregion

        /// <summary>
        /// 碰撞的元素
        /// 从大到小
        /// </summary>
        public List<ConflictElement> ConflictElements { set; get; }


        internal void SetConflictElements(List<AvoidElement> elementsToAvoid, List<ValuedConflictNode> conflictNodes)
        {
            ConflictElements = new List<ConflictElement>();
            ElementIntersectsElementFilter filter = new ElementIntersectsElementFilter(MEPElement);
            var elementsConflicted = elementsToAvoid.Where(c => filter.PassesFilter(c.MEPElement)).ToList();
            foreach (var elementConflicted in elementsConflicted)
            {
                var conflictLocation = GetConflictPoint(elementConflicted.MEPElement);
                if (conflictLocation != null)
                {
                    var conflictElement = new ConflictElement(this, conflictLocation, elementConflicted);
                    ConflictElements.Add(conflictElement);
                    if (conflictNodes.FirstOrDefault(c => c.ConflictLocation.VL_XYEqualTo(conflictLocation) && (c.ValueNode1.OrientAvoidElement == this || c.ValueNode2.OrientAvoidElement == this)) == null)
                        conflictNodes.Add(new ValuedConflictNode(this, conflictLocation, elementConflicted));
                }
            }
            SortConflictElements();

            ////TEST
            //var conflictIds = string.Join(",", elementsConflicted.Select(c => c.MEPElement.Id));
        }

        internal ConflictElement AddConflictElement(ConflictElement conflictElement)
        {
            if (ConflictElements == null)
                ConflictElements = new List<ConflictElement>();
            ConflictElements.Add(conflictElement);
            SortConflictElements();
            return conflictElement;
        }
        internal ConflictElement AddConflictElement(Connector connectorToMepElement)
        {
            if (ConflictElements == null)
                ConflictElements = new List<ConflictElement>();
            ConflictElement conflictElement = null;
            if (ConnectorStart.Origin.VL_XYEqualTo(connectorToMepElement.Origin))
                conflictElement = new ConflictElement(this, StartPoint, connectorToMepElement);
            else if (ConnectorEnd.Origin.VL_XYEqualTo(connectorToMepElement.Origin))
                conflictElement = new ConflictElement(this, EndPoint, connectorToMepElement);
            else
                throw new NotImplementedException("错误的连接点");
            ConflictElements.Add(conflictElement);
            SortConflictElements();
            return conflictElement;
        }

        public void SortConflictElements()
        {
            ConflictElements = ConflictElements.OrderByDescending(c => c.ConflictLocation, new XYZComparer()).ToList();
        }

        /// <summary>
        /// 计算线面交点
        /// 平行为null
        /// </summary>
        public XYZ GetConflictPoint(MEPCurve mepCurve)
        {
            var line = (MEPElement.Location as LocationCurve).Curve as Line;
            var lineToAvoid = (mepCurve.Location as LocationCurve).Curve as Line;
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

        //public static IList<MEPCurve> GetConnectingMEPs(this FamilyInstance fi)
        //{
        //    List<MEPCurve> list = new List<MEPCurve>();
        //    if (fi.MEPModel == null || fi.MEPModel.ConnectorManager == null)
        //        return list;

        //    foreach (Connector con in fi.MEPModel.ConnectorManager.Connectors)
        //    {
        //        var linkedCon = con.GetConnectedConnector();
        //        while (linkedCon != null && linkedCon.Owner is FamilyInstance)
        //        {
        //            var linkedFi = linkedCon.Owner as FamilyInstance;
        //            var linkedAnotherCon = linkedFi.GetAnthorConnector(linkedCon);
        //            if (linkedAnotherCon != null)
        //                linkedCon = linkedAnotherCon.GetConnectedConnector();
        //            else
        //                linkedCon = null;
        //        }

        //        if (linkedCon != null && linkedCon.Owner is MEPCurve)
        //            list.Add(linkedCon.Owner as MEPCurve);
        //    }

        //    return list;
        //}


        ///// <summary>
        ///// 获取相连的连接点
        ///// </summary>
        ///// <param name="src"></param>
        ///// <returns></returns>
        //public static Connector GetConnectedConnector(this Connector src)
        //{
        //    if (src.ConnectorType == ConnectorType.MasterSurface || src.ConnectorType == ConnectorType.Logical)
        //        return null;

        //    if (!src.IsConnected)
        //        return null;

        //    foreach (Connector con in src.AllRefs)
        //    {
        //        if (con.IsConnectedTo(src) && con.Owner.Id != src.Owner.Id && con.Shape != ConnectorProfileType.Invalid)
        //            return con;
        //    }

        //    return null;
        //}

        /// <summary>
        /// 获取相连的连接点
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static List<Connector> GetConnectorsToMepElement(this Connector src)
        {
            List<Connector> result = new List<Connector>();
            if (src.ConnectorType == ConnectorType.MasterSurface || src.ConnectorType == ConnectorType.Logical)
                return result;
            if (!src.IsConnected)
                return result;
            foreach (Connector con in src.AllRefs)
            {
                if (con.IsConnectedTo(src) && con.Owner.Id != src.Owner.Id && con.Shape != ConnectorProfileType.Invalid)
                {
                    // 弯头 三通 四通
                    var fi = con.Owner as FamilyInstance;
                    if (fi != null)
                    {
                        for (int i = 1; i <= fi.MEPModel.ConnectorManager.Connectors.Size; i++)
                        {
                            var fiConnector = fi.MEPModel.ConnectorManager.Connectors.GetConnectorById(i);
                            if (fiConnector == null || !fiConnector.IsConnected)
                                continue;
                            var linkedConnector = fiConnector.GetConnectedConnector();
                            if (linkedConnector == null)
                                continue;
                            if (linkedConnector.Owner is MEPCurve && linkedConnector.Owner.Id != src.Owner.Id)
                            {
                                result.Add(linkedConnector);
                            }
                            else
                            {
                                //TODO 其他连接件情况
                                continue;
                            }
                        }
                    }
                    //TODO 其他连接件情况
                }
            }
            return result;
        }
    }
}
