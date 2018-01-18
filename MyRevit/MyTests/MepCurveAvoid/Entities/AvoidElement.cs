using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using MyRevit.MyTests.Utilities;
using MyRevit.Utilities;
using PmSoft.Common.RevitClass.CommonRevitClass;
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
            //AvoidElementType 类型自动识别
            if (connectedMepElement is Pipe)
                AvoidElementType = AvoidElementType.Pipe;
            else if (connectedMepElement is Duct)
                AvoidElementType = AvoidElementType.Duct;
            else if (connectedMepElement is CableTray)
                AvoidElementType = AvoidElementType.CableTray;
            else
                throw new NotImplementedException("类型未在范围内");
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
        public double AngleToTurn { get; internal set; }
        public double ConnectHeight { get; internal set; }
        public double ConnectWidth { get; internal set; }
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
            Connector start, end;
            GetStartAndEndConnector(MEPElement, middle, out start, out end);
            ConnectorStart = start;
            ConnectorEnd = end;
            //更新避让所需的点位信息
            AngleToTurn = 0;
            ConnectHeight = 0;
            ConnectWidth = 0;
        }


        /// <summary>
        /// 计算管道所用管件的WidthX，widthY，Length
        /// </summary>
        /// <param name="mep"></param>
        /// <param name="angle"></param>
        /// <param name="ent"></param>
        /// <returns></returns>
        private bool GetFittingData(Document doc,MEPCurve mep, double angle, out double widthX, out double widthY, out double length)
        {
            widthX = 0;
            widthY = 0;
            length = 0;

            Connector con1 = null, con2 = null;
            Line line1 = null, line2 = null;
            FamilySymbol fs = null;
            if (mep is CableTray)
            {
                var param = (mep as CableTray).GetMEPCurveType().GetParameters("垂直内弯头");
                if (param.Count != 0)
                    fs = doc.GetElement(param.First().AsElementId()) as FamilySymbol;
            }
            else
                fs = this.Judge_LoadDefaultFitting(mep, MEPCurveConnectTypeENUM.Elbow);
            if (fs == null)
                return false;

            Transaction trans = new Transaction(mep.Document);
            try
            {
                trans.Start("临时");
                var fi = doc.Create.NewFamilyInstance(XYZ.Zero, fs, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                var angleP = fi.GetParameters("角度").FirstOrDefault();
                if (angleP != null)
                    angleP.Set(Math.PI * angle / 180);
                foreach (Connector con in fi.MEPModel.ConnectorManager.Connectors)
                {
                    if (mep is CableTray || (mep is Duct && con.Shape == ConnectorProfileType.Rectangular))
                        con.SetWidth(mep.ConnectorManager.Lookup(0), true);
                    else
                        con.SetWidth(mep.ConnectorManager.Lookup(0));
                    if (con1 == null)
                        con1 = con;
                    else
                        con2 = con;
                }
                doc.Regenerate();

                //生成出口直线
                line1 = Line.CreateUnbound(con1.Origin, con1.CoordinateSystem.BasisZ);
                line2 = Line.CreateUnbound(con2.Origin, con2.CoordinateSystem.BasisZ);
                length = line1.GetIntersectWithPoint(line2).DistanceTo(con1.Origin);

                //计算X向和Y向的距离
                widthX = Math.Abs(con1.Origin.X - con2.Origin.X);
                widthY = Math.Abs(con1.Origin.Y - con2.Origin.Y);

                trans.RollBack();
            }
            catch
            {
                if (trans.HasStarted())
                    trans.RollBack();

                return false;
            }
            return true;
        }


        public static void GetStartAndEndConnector(MEPCurve mepCurve, XYZ middle, out Connector start, out Connector end)
        {
            var c1 = mepCurve.ConnectorManager.Connectors.GetConnectorById(0);
            start = null;
            end = null;
            if (c1 != null)
            {
                if (new XYZComparer().Compare(c1.Origin, middle) > 0)
                    start = c1;
                else
                    end = c1;
            }
            var c2 = mepCurve.ConnectorManager.Connectors.GetConnectorById(1);
            if (c2 != null)
            {
                if (new XYZComparer().Compare(c2.Origin, middle) > 0)
                    start = c2;
                else
                    end = c2;
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
                default:
                    throw new NotImplementedException("未分类的内容");
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
