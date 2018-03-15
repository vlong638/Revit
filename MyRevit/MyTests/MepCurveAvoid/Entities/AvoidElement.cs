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
    public class XYComparer : IComparer<XYZ>
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
            MEPCurve = element;
            AvoidElementType = avoidElementType;
        }

        public AvoidElement(MEPCurve connectedMepElement)
        {
            MEPCurve = connectedMepElement;
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
        public MEPCurve MEPCurve
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
        #region 定位计算
        public double AngleToTurn { get; internal set; }
        public double ConnectHeight { get; internal set; }
        public double ConnectWidth { get; internal set; }
        public double OffsetWidth { get; private set; }
        #endregion
        /// <summary>
        /// 更新相关属性
        /// </summary>
        private void UpdateProperties()
        {
            UpdateSize(MEPCurve);
            if (AvoidElementType == AvoidElementType.Pipe)
            {
                UpdateIsPressed(MEPCurve);
                UpdateIsHot(MEPCurve);
            }
            else if (AvoidElementType == AvoidElementType.Duct)
            {
                UpdateIsHeavy(MEPCurve);
            }
            UpdatePriorityElementType();
            var curve = (MEPCurve.Location as LocationCurve).Curve;
            var p1 = curve.GetEndPoint(0);
            var p2 = curve.GetEndPoint(1);
            var middle = (p1 + p2) / 2;
            if (new XYComparer().Compare(p1, p2) > 0)
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
            GetStartAndEndConnector(MEPCurve, middle, out start, out end);
            ConnectorStart = start;
            ConnectorEnd = end;
            //更新避让所需的点位信息
            ConnectInfo connectInfo;
            string keyName = MEPCurve.Name + MEPCurve.get_Parameter(BuiltInParameter.RBS_CALCULATED_SIZE).AsString();
            if (!ConnectInfoDic.ContainsKey(keyName))
            {
                double angleToTurn, connectHeight, connectWidth, offsetWidth;
                if (GetFittingData(MEPCurve, out connectWidth, out connectHeight, out offsetWidth, out angleToTurn))
                {
                    connectInfo = new ConnectInfo(connectHeight, connectHeight, offsetWidth, angleToTurn);
                    ConnectInfoDic.Add(keyName, connectInfo);
                }
                else
                {
                    throw new NotImplementedException("加载连接件信息失败,构件Id:" + MEPCurve.Id.IntegerValue.ToString());
                }
            }
            else
            {
                connectInfo = ConnectInfoDic[keyName];
            }
            ConnectHeight = connectInfo.ConnectHeight;
            ConnectWidth = connectInfo.ConnectWidth;
            OffsetWidth = connectInfo.OffsetWidth;
            AngleToTurn = connectInfo.AngleToTurn;
        }

        struct ConnectInfo
        {
            public double ConnectHeight;
            public double ConnectWidth;
            public double OffsetWidth;
            public double AngleToTurn;

            public ConnectInfo(double connectHeight, double connectWidth, double offsetWidth, double angleToTurn)
            {
                ConnectHeight = connectHeight;
                ConnectWidth = connectWidth;
                OffsetWidth = offsetWidth;
                AngleToTurn = angleToTurn;
            }
        }
        static Dictionary<string, ConnectInfo> ConnectInfoDic = new Dictionary<string, ConnectInfo>();


        #region 获取构件信息
        protected enum MEPCurveConnectTypeENUM { MultiShapeTransition, Transition, Elbow, Tee, Cross, TakeOff }
        /// <summary>
        /// 获取设置文件中管件名称 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetDefaultFittingName(MEPCurve src, MEPCurveConnectTypeENUM type)
        {
            string searchText = "/Root/";
            if (src is Pipe)
            {
                searchText += "Pipe/";
            }
            else if (src is Duct)
            {
                var con = src.ConnectorManager.Lookup(0);
                if (con.Shape == ConnectorProfileType.Round)
                    searchText += "Duct/圆形/";
                else if (con.Shape == ConnectorProfileType.Rectangular)
                    searchText += "Duct/矩形/";
                else if (con.Shape == ConnectorProfileType.Oval)
                    searchText += "Duct/椭圆/";
            }
            else if (src is Conduit)
                searchText += "Conduit/";
            else if (src is CableTray)
                searchText += "CableTray/";

            switch (type)
            {
                case MEPCurveConnectTypeENUM.Elbow://弯头
                    searchText += "弯头"; break;
                case MEPCurveConnectTypeENUM.Tee://三通
                    searchText += "三通"; break;
                case MEPCurveConnectTypeENUM.Cross://四通
                    searchText += "四通"; break;
                case MEPCurveConnectTypeENUM.Transition://过渡件
                    searchText += "过渡件"; break;
                case MEPCurveConnectTypeENUM.TakeOff://侧接
                    searchText += "侧接"; break;
                default:
                    break;
            }

            System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
            xml.Load(FamilyLoadUtils.FamilyLibPath + "\\..\\..\\SysData\\MEPCurveConnect.xml");
            var node = xml.SelectSingleNode(searchText);
            if (node == null)
                return null;
            else
                return node.InnerText;
        }
        public bool IsOnlyUseRevitDefault { get; set; }

        /// <summary>
        /// 检测系统中是否有默认连接项，无则进行添加
        /// </summary>
        /// <param name="src"></param>
        /// <param name="type"></param>
        protected FamilySymbol Judge_LoadDefaultFitting(MEPCurve src, MEPCurveConnectTypeENUM type)
        {
            FamilySymbol fs = null;
            switch (type)
            {
                case MEPCurveConnectTypeENUM.Elbow://弯头
                    fs = src.GetDefaultFittingSymbol(RoutingPreferenceRuleGroupType.Elbows); break;
                case MEPCurveConnectTypeENUM.Tee://三通
                    fs = src.GetDefaultFittingSymbol(RoutingPreferenceRuleGroupType.Junctions); break;
                case MEPCurveConnectTypeENUM.Cross://四通
                    fs = src.GetDefaultFittingSymbol(RoutingPreferenceRuleGroupType.Crosses); break;
                case MEPCurveConnectTypeENUM.Transition://过渡件
                    fs = src.GetDefaultFittingSymbol(RoutingPreferenceRuleGroupType.Transitions); break;
                case MEPCurveConnectTypeENUM.TakeOff://侧接
                    fs = src.GetDefaultTakeoffFittingSymbol(); break;
                default:
                    fs = null;
                    break;
            }

            if (fs != null)
                return fs;

            if (this.IsOnlyUseRevitDefault)
                return null;

            var familyName = this.GetDefaultFittingName(src, type);
            if (familyName == null)
                return null;

            fs = FamilyLoadUtils.FindFamilySymbol_SubTransaction(src.Document, familyName, null);
            if (fs == null)
                return null;
            if (src is Pipe || src is Duct)
            {
                RoutingPreferenceManager rpm = src.GetMEPCurveType().RoutingPreferenceManager;
                var rule = new RoutingPreferenceRule(fs.Id, "");
                rule.AddCriterion(PrimarySizeCriterion.All());

                switch (type)
                {
                    case MEPCurveConnectTypeENUM.Elbow://弯头
                        rpm.AddRule(RoutingPreferenceRuleGroupType.Elbows, rule); break;
                    case MEPCurveConnectTypeENUM.Tee://三通
                        rpm.AddRule(RoutingPreferenceRuleGroupType.Junctions, rule); break;
                    case MEPCurveConnectTypeENUM.Cross://四通
                        rpm.AddRule(RoutingPreferenceRuleGroupType.Crosses, rule); break;
                    case MEPCurveConnectTypeENUM.Transition://过渡件
                        rpm.AddRule(RoutingPreferenceRuleGroupType.Transitions, rule); break;
                    case MEPCurveConnectTypeENUM.TakeOff://侧接
                        rpm.AddRule(RoutingPreferenceRuleGroupType.Junctions, rule); break;
                    default:
                        break;
                }

            }
            else if (src is Conduit)
            {
                Parameter param = null;

                switch (type)
                {
                    case MEPCurveConnectTypeENUM.Elbow://弯头
                        param = (src.Document.GetElement(src.GetTypeId())).GetParameters("弯头").FirstOrDefault(); break;
                    case MEPCurveConnectTypeENUM.Tee://三通
                        param = (src.Document.GetElement(src.GetTypeId())).GetParameters("T 形三通").FirstOrDefault(); break;
                    case MEPCurveConnectTypeENUM.Cross://四通
                        param = (src.Document.GetElement(src.GetTypeId())).GetParameters("交叉线").FirstOrDefault(); break;
                    case MEPCurveConnectTypeENUM.Transition://过渡件
                        param = (src.Document.GetElement(src.GetTypeId())).GetParameters("过渡件").FirstOrDefault(); break;
                    default:
                        break;
                }

                if (param != null)
                {
                    param.Set(fs.Id);
                }
            }
            else if (src is CableTray)
            {
                Parameter param = null;
                var t = new FilteredElementCollector(src.Document).OfClass(typeof(CableTrayType)).FirstOrDefault(p => p.GetParameters("族名称").First().AsString() == "带配件的电缆桥架");
                switch (type)
                {
                    case MEPCurveConnectTypeENUM.Elbow://弯头
                        param = t.GetParameters("水平弯头").FirstOrDefault(); break;
                    case MEPCurveConnectTypeENUM.Tee://三通
                        param = t.GetParameters("T 形三通").FirstOrDefault(); break;
                    case MEPCurveConnectTypeENUM.Cross://四通
                        param = t.GetParameters("交叉线").FirstOrDefault(); break;
                    case MEPCurveConnectTypeENUM.Transition://过渡件
                        param = t.GetParameters("过渡件").FirstOrDefault(); break;
                    default:
                        break;
                }

                if (param != null)
                {
                    param.Set(fs.Id);
                }
            }
            return fs;
        }
        public bool GetFittingData(MEPCurve mep, out double widthX, out double widthY, out double offsetX, out double angle)
        {
            widthX = 0;
            widthY = 0;
            offsetX = 0;
            angle = 0;
            Connector con1 = null, con2 = null;
            FamilySymbol fs = null;
            var doc = mep.Document;
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
                #region 构件的角度处理
                if (AvoidElementType == AvoidElementType.CableTray)
                {
                    angle = Math.PI / 4;
                    var angleP = fi.GetParameters("角度").FirstOrDefault();
                    angleP.Set(angle);
                }
                else
                {
                    var angleP = fi.GetParameters("角度").FirstOrDefault();
                    if (angleP == null)
                    {
                        angle = Math.PI / 2;
                        angleP.Set(angle);
                    }
                    else
                    {
                        angle = angleP.AsDouble();
                    }
                }
                #endregion

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

                //计算X向和Y向的距离
                widthX = Math.Abs(con1.Origin.X - con2.Origin.X);
                widthY = Math.Abs(con1.Origin.Y - con2.Origin.Y);
                offsetX = Math.Max((fi.Location as LocationPoint).Point.DistanceTo(con1.Origin), (fi.Location as LocationPoint).Point.DistanceTo(con2.Origin));//取长边作为偏移量
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
        #endregion

        public static void GetStartAndEndConnector(MEPCurve mepCurve, XYZ middle, out Connector start, out Connector end)
        {
            var c1 = mepCurve.ConnectorManager.Connectors.GetConnectorById(0);
            start = null;
            end = null;
            if (c1 != null)
            {
                if (new XYComparer().Compare(c1.Origin, middle) > 0)
                    start = c1;
                else
                    end = c1;
            }
            var c2 = mepCurve.ConnectorManager.Connectors.GetConnectorById(1);
            if (c2 != null)
            {
                if (new XYComparer().Compare(c2.Origin, middle) > 0)
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
            return Width;
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
        XYZ VerticalVector;
        internal XYZ GetVerticalVector()
        {
            if (VerticalVector == null)
            {
                //VerticalVector = PriorityElementType == PriorityElementType.UnpressedPipe ? ;
                var direction = ((MEPCurve.Location as LocationCurve).Curve as Line).Direction;
                var normal = direction.CrossProduct(new XYZ(0, 0, 1));
                var verticalVector = direction.CrossProduct(normal).Normalize();
                //var normal = direction.GetNormal(new XYZ(0, 0, 1));
                //var verticalVector = direction.GetNormal(normal);
                if (PriorityElementType == PriorityElementType.UnpressedPipe)
                {
                    if (verticalVector.Z > 0)
                    {
                        verticalVector = -verticalVector;
                    }
                }
                else
                {
                    if (verticalVector.Z < 0)
                    {
                        verticalVector = -verticalVector;
                    }
                }
                VerticalVector = verticalVector;
            }
            return VerticalVector;
        }
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
            IsHeavy = Width >= 1000;//Width or Height
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
            ElementIntersectsElementFilter filter = new ElementIntersectsElementFilter(MEPCurve);
            var elementsConflicted = elementsToAvoid.Where(c => filter.PassesFilter(c.MEPCurve)).ToList();
            foreach (var elementConflicted in elementsConflicted)
            {
                var conflictLocation = GetConflictPoint(elementConflicted.MEPCurve);
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
        internal ConflictElement AddConflictElement(Connector connectorToMepElement, ConflictElement srcConflictElement)
        {
            if (ConflictElements == null)
                ConflictElements = new List<ConflictElement>();
            ConflictElement conflictElement = null;
            if (ConnectorStart != null && ConnectorStart.Origin.VL_XYEqualTo(connectorToMepElement.Origin))
                conflictElement = new ConflictElement(this, StartPoint, connectorToMepElement, srcConflictElement.ConflictEle);
            else if (ConnectorEnd != null && ConnectorEnd.Origin.VL_XYEqualTo(connectorToMepElement.Origin))
                conflictElement = new ConflictElement(this, EndPoint, connectorToMepElement, srcConflictElement.ConflictEle);
            else
                throw new NotImplementedException("错误的连接点");
            ConflictElements.Add(conflictElement);
            SortConflictElements();
            return conflictElement;
        }

        public void SortConflictElements()
        {
            ConflictElements = ConflictElements.OrderByDescending(c => c.ConflictLocation, new XYComparer()).ToList();
        }

        /// <summary>
        /// 计算线面交点
        /// 平行为null
        /// </summary>
        public XYZ GetConflictPoint(MEPCurve mepCurve)
        {
            var line = (MEPCurve.Location as LocationCurve).Curve as Line;
            var lineToAvoid = (mepCurve.Location as LocationCurve).Curve as Line;
            var lineDirection1 = line.Direction;
            var lineDirection2 = lineToAvoid.Direction;
            if ((Math.Abs(lineDirection1.DotProductByCoordinate(lineDirection2, VLCoordinateType.XY)) - lineDirection1.GetLengthByCoordinate(VLCoordinateType.XY) * lineDirection2.GetLengthByCoordinate(VLCoordinateType.XY)).IsMiniValue())
                return null;
            VLTriangle triangle = new VLTriangle(lineToAvoid.GetEndPoint(0), lineToAvoid.GetEndPoint(1), lineToAvoid.GetEndPoint(0) + new XYZ(0, 0, 1));
            return VLGeometryHelper.GetIntersection(triangle, line.GetEndPoint(0), line.Direction);
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
            if (src == null)
                return null;
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
        public static List<Connector> GetConnectorsToMepElement(this Connector source)
        {
            List<Connector> result = new List<Connector>();
            if (source.ConnectorType == ConnectorType.MasterSurface || source.ConnectorType == ConnectorType.Logical)
                return result;
            if (!source.IsConnected)
                return result;
            foreach (Connector linkedConnector in source.AllRefs)
            {
                if (linkedConnector.IsConnectedTo(source) && linkedConnector.Owner.Id != source.Owner.Id && linkedConnector.Shape != ConnectorProfileType.Invalid)
                {
                    SearchByFamily(source, result, linkedConnector);
                }
            }
            return result;
        }

        private static void SearchByFamily(Connector source, List<Connector> result, Connector currentConnector, Connector preConnector=null)
        {
            // 弯头 三通 四通 大转小 终止(风口)
            var fi = currentConnector.Owner as FamilyInstance;
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
                    if (linkedConnector.Owner is MEPCurve)
                    {
                        if (linkedConnector.Owner.Id != source.Owner.Id)
                        {
                            result.Add(linkedConnector);
                        }
                    }
                    else
                    {
                        //其他连接件情况
                        if (preConnector == null|| linkedConnector.Owner.Id!=preConnector.Owner.Id)
                            SearchByFamily(source, result, linkedConnector, currentConnector);
                    }
                }
            }
        }
    }
}
