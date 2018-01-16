using Autodesk.Revit.DB;
using System.Collections.Generic;
using System;
using System.Linq;
using MyRevit.MyTests.Utilities;

namespace MyRevit.MyTests.MepCurveAvoid
{
    public class ConflictLineSections : List<ConflictLineSection>
    {
        public Guid GroupId = Guid.NewGuid();
        public bool IsGrouped = false;
        public bool IsValued = false;
        /// <summary>
        /// 为其避让的价值
        /// </summary>
        public PriorityValue AvoidPriorityValue { set; get; }
        /// <summary>
        /// 组价值
        /// </summary>
        public PriorityValue GroupPriorityValue { set; get; }
        public double Height { set; get; }
    }
    /// <summary>
    /// 碰撞区间的价值
    /// </summary>
    public class ConflictLineSection
    {
        public ElementId ElementId { get { return AvoidElement.MEPElement.Id; } }
        public List<ConflictElement> ConflictElements;
        public AvoidElement AvoidElement;

        public XYZ StartPoint { set; get; }
        public Connector StartLinkedConnector { get; internal set; }
        public ElementId NewStartElementId { set; get; }

        public XYZ EndPoint { set; get; }
        public Connector EndLinkedConnector { get; internal set; }
        public ElementId NewEndElementId { set; get; }

        public ConflictLineSection(AvoidElement startElement)
        {
            AvoidElement = startElement;
            ConflictElements = new List<ConflictElement>();
        }
    }

    public class PriorityValueComparer : IComparer<PriorityValue>
    { 
        public static List<PriorityElementType> PriorityElementTypes = new List<PriorityElementType>()
        {
            PriorityElementType.UnpressedPipe,
            PriorityElementType.LargeDuct,
            PriorityElementType.LargePressedPipe,
            PriorityElementType.LargeCableTray,
            PriorityElementType.NormalDuct,
            PriorityElementType.NormalPressedPipe,
            PriorityElementType.NormalCableTray,
        };
        public int Compare(PriorityValue item, PriorityValue itemToCompare)
        {
            if (item.CompeteType != CompeteType.None && itemToCompare.CompeteType != CompeteType.None)
                if (item.CompeteType==CompeteType.Winner&& itemToCompare.CompeteType==CompeteType.Winner||
                    item.CompeteType == CompeteType.Loser && itemToCompare.CompeteType == CompeteType.Loser)
                throw new NotImplementedException("暂不支持互斥避让");
            if (item.CompeteType == CompeteType.Winner)
                return 1;
            if (item.CompeteType == CompeteType.Loser)
                return -1;
            if (itemToCompare.CompeteType == CompeteType.Winner)
                return -1;
            if (itemToCompare.CompeteType == CompeteType.Loser)
                return 1;

            foreach (var PriorityElementType in PriorityElementTypes)
            {
                var type = PriorityElementType;
                if (item.TypeNumber[type] > itemToCompare.TypeNumber[type])
                    return 1;
                if (item.TypeNumber[type] < itemToCompare.TypeNumber[type])
                    return -1;
                if (item.TypeMaxLength[type] > itemToCompare.TypeMaxLength[type])
                    return 1;
                if (item.TypeMaxLength[type] < itemToCompare.TypeMaxLength[type])
                    return -1;
            }
            if (item.FullLength > itemToCompare.FullLength)
                return 1;
            if (item.FullLength < itemToCompare.FullLength)
                return -1;
            return 0;
        }
    }

    public class PriorityValue
    {
        public CompeteType CompeteType { set; get; }
        public Dictionary<PriorityElementType, int> TypeNumber = new Dictionary<PriorityElementType, int>()
        {
            {PriorityElementType.UnpressedPipe,0},
            {PriorityElementType.LargeDuct,0},
            {PriorityElementType.LargePressedPipe,0},
            {PriorityElementType.LargeCableTray,0},
            {PriorityElementType.NormalDuct,0},
            {PriorityElementType.NormalPressedPipe,0},
            {PriorityElementType.NormalCableTray,0},
            {PriorityElementType.Connector,0},
        };
        public Dictionary<PriorityElementType, double> TypeMaxLength = new Dictionary<PriorityElementType, double>()
        {
            {PriorityElementType.UnpressedPipe,0},
            {PriorityElementType.LargeDuct,0},
            {PriorityElementType.LargePressedPipe,0},
            {PriorityElementType.LargeCableTray,0},
            {PriorityElementType.NormalDuct,0},
            {PriorityElementType.NormalPressedPipe,0},
            {PriorityElementType.NormalCableTray,0},
            {PriorityElementType.Connector,0},
        };
        public double FullLength = 0;

        #region Compare


        public static List<PriorityElementType> PriorityElementTypes = new List<PriorityElementType>()
        {
            PriorityElementType.UnpressedPipe,
            PriorityElementType.LargeDuct,
            PriorityElementType.LargePressedPipe,
            PriorityElementType.LargeCableTray,
            PriorityElementType.NormalDuct,
            PriorityElementType.NormalPressedPipe,
            PriorityElementType.NormalCableTray,
        };
        public static PriorityValue operator +(PriorityValue item, PriorityValue itemToAdd)
        {
            foreach (var PriorityElementType in PriorityElementTypes)
            {
                var type = PriorityElementType;
                item.TypeNumber[type] += itemToAdd.TypeNumber[type];
                item.TypeMaxLength[type] += itemToAdd.TypeMaxLength[type];
            }
            item.FullLength += itemToAdd.FullLength;
            return item;
        }


        //public static List<PriorityElementType> PriorityElementTypes = new List<PriorityElementType>()
        //{
        //    PriorityElementType.UnpressedPipe,
        //    PriorityElementType.LargeDuct,
        //    PriorityElementType.LargePressedPipe,
        //    PriorityElementType.LargeCableTray,
        //    PriorityElementType.NormalDuct,
        //    PriorityElementType.NormalPressedPipe,
        //    PriorityElementType.NormalCableTray,
        //};
        //public static bool operator >(PriorityValue item, PriorityValue itemToCompare)
        //{
        //    if (item.CompeteType == CompeteType.Winner && itemToCompare.CompeteType == CompeteType.Winner)
        //        throw new NotImplementedException("暂不支持同时避让");
        //    if (item.CompeteType == CompeteType.Winner)
        //        return false;
        //    if (itemToCompare.CompeteType == CompeteType.Winner)
        //        return true;

        //    foreach (var PriorityElementType in PriorityElementTypes)
        //    {
        //        var type = PriorityElementType;
        //        if (item.TypeNumber[type] > itemToCompare.TypeNumber[type])
        //            return true;
        //        if (item.TypeNumber[type] < itemToCompare.TypeNumber[type])
        //            return false;
        //        if (item.TypeMaxLength[type] > itemToCompare.TypeMaxLength[type])
        //            return true;
        //        if (item.TypeMaxLength[type] < itemToCompare.TypeMaxLength[type])
        //            return false;
        //    }
        //    return item.FullLength > itemToCompare.FullLength;
        //}
        //public static bool operator <(PriorityValue item, PriorityValue itemToCompare)
        //{
        //    if (item.CompeteType == CompeteType.Winner && itemToCompare.CompeteType == CompeteType.Winner)
        //        throw new NotImplementedException("暂不支持同时避让");
        //    if (item.CompeteType == CompeteType.Winner)
        //        return true;
        //    if (itemToCompare.CompeteType == CompeteType.Winner)
        //        return false;

        //    foreach (var PriorityElementType in PriorityElementTypes)
        //    {
        //        var type = PriorityElementType;
        //        if (item.TypeNumber[type] > itemToCompare.TypeNumber[type])
        //            return false;
        //        if (item.TypeNumber[type] < itemToCompare.TypeNumber[type])
        //            return true;
        //        if (item.TypeMaxLength[type] > itemToCompare.TypeMaxLength[type])
        //            return false;
        //        if (item.TypeMaxLength[type] < itemToCompare.TypeMaxLength[type])
        //            return true;
        //    }
        //    return item.FullLength < itemToCompare.FullLength;
        //} 
        #endregion
    }

    /// <summary>
    /// 碰撞价值体
    /// </summary>
    public class ValueNode
    {
        //价值体不同,价值组相同
        //public Guid Id = Guid.NewGuid();
        public ValuedConflictNode ValuedConflictNode;
        public AvoidElement OrientAvoidElement { set; get; }
        public ConflictLineSections ConflictLineSections { set; get; }
        public ValueNode OtherNode { get; internal set; }

        public ValueNode(ValuedConflictNode valuedConflictNode, AvoidElement avoidElement)
        {            
            ValuedConflictNode = valuedConflictNode;
            OrientAvoidElement = avoidElement;
            ConflictLineSections = new ConflictLineSections();
        }

        #region 价值分析


        static double GroupingDistance = PmSoft.Common.RevitClass.Utils.UnitTransUtils.MMToFeet(600);//成组的最小距离

        /// <summary>
        /// 价值分析
        /// 组团+价值计算
        /// 基本逻辑:
        /// 找到原始碰撞单元
        /// 连续组团处理
        /// 边界连接件组团处理
        /// 价值计算
        /// </summary>
        /// <param name="conflictElement"></param>
        /// <param name="valuedConflictNodes">源碰撞</param>
        /// <param name="avoidElements">源碰撞</param>

        public void Grouping(ConflictElement conflictElement, List<ValuedConflictNode> valuedConflictNodes, List<AvoidElement> avoidElements)
        {
            if (ConflictLineSections.IsGrouped)
                return;

            ConflictLineSections = new ConflictLineSections();
            SetupGroup(OrientAvoidElement, conflictElement, valuedConflictNodes, avoidElements);
            RenderGroupInfo(valuedConflictNodes);
            SetupGroupPriorityValue();
            //SetupAvoidPriorityValue(valuedConflictNodes);
            ConflictLineSections.IsGrouped = true;
        }
        public void Valuing(ConflictElement conflictElement, List<ValuedConflictNode> valuedConflictNodes, List<AvoidElement> avoidElements)
        {
            if (ConflictLineSections.IsValued)
                return;

            SetupAvoidPriorityValue(valuedConflictNodes);
            ConflictLineSections.IsValued = true;
        }

        /// <summary>
        /// 组内关系传递 避免重复计算
        /// </summary>
        /// <param name="valuedConflictNodes"></param>
        private void RenderGroupInfo(List<ValuedConflictNode> valuedConflictNodes)
        {
            foreach (ConflictLineSection ConflictLineSection in ConflictLineSections)
            {
                foreach (ConflictElement ConflictElement in ConflictLineSection.ConflictElements)
                {
                    var valuedConflictNode = valuedConflictNodes.FirstOrDefault(c => !c.ValueNode1.ConflictLineSections.IsGrouped && c.ValueNode1.OrientAvoidElement.MEPElement.Id == ConflictElement.AvoidEle.MEPElement.Id && c.ConflictLocation.VL_XYEqualTo(ConflictElement.ConflictLocation));
                    if (valuedConflictNode != null)
                        valuedConflictNode.ValueNode1.Settle(ConflictLineSections);
                    valuedConflictNode = valuedConflictNodes.FirstOrDefault(c => !c.ValueNode2.ConflictLineSections.IsGrouped && c.ValueNode2.OrientAvoidElement.MEPElement.Id == ConflictElement.AvoidEle.MEPElement.Id && c.ConflictLocation.VL_XYEqualTo(ConflictElement.ConflictLocation));
                    if (valuedConflictNode != null)
                        valuedConflictNode.ValueNode2.Settle(ConflictLineSections);
                }
            }
        }

        private void Settle(ConflictLineSections conflictLineSections)
        {
            ConflictLineSections = conflictLineSections;
            ConflictLineSections.IsGrouped = true;
        }

        /// <summary>
        /// 价值计算
        /// </summary>

        private void SetupAvoidPriorityValue(List<ValuedConflictNode> valuedConflictNodes)
        {
            //为之避让的内容的价值
            ConflictLineSections.AvoidPriorityValue = new PriorityValue();
            foreach (var conflictLineSection in ConflictLineSections)
            {
                foreach (var conflictElement in conflictLineSection.ConflictElements)
                {
                    if (conflictElement.IsConnector)
                        continue;

                    foreach (var valuedConflictNode in valuedConflictNodes)
                    {
                        if (valuedConflictNode.ValueNode1.ConflictLineSections.GroupId == conflictElement.GroupId)
                        {
                            ConflictLineSections.AvoidPriorityValue += valuedConflictNode.ValueNode1.ConflictLineSections.GroupPriorityValue;
                            break;
                        }
                        if (valuedConflictNode.ValueNode2.ConflictLineSections.GroupId == conflictElement.GroupId)
                        {
                            ConflictLineSections.AvoidPriorityValue += valuedConflictNode.ValueNode2.ConflictLineSections.GroupPriorityValue;
                            break;
                        }
                    }
                }
            }
        }
        private void SetupGroupPriorityValue()
        {
            //组的价值
            ConflictLineSections.GroupPriorityValue = new PriorityValue();
            foreach (var conflictLineSection in ConflictLineSections)
            {
                ConflictLineSections.GroupPriorityValue.TypeNumber[conflictLineSection.AvoidElement.PriorityElementType]++;
                ConflictLineSections.GroupPriorityValue.TypeMaxLength[conflictLineSection.AvoidElement.PriorityElementType]
                    = Math.Max(ConflictLineSections.GroupPriorityValue.TypeMaxLength[conflictLineSection.AvoidElement.PriorityElementType], conflictLineSection.AvoidElement.GetSize());
                ConflictLineSections.GroupPriorityValue.FullLength += conflictLineSection.ConflictElements.First().ConflictLocation.DistanceTo(conflictLineSection.ConflictElements.Last().ConflictLocation);
            }
        }

        /// <summary>
        /// 这里形成了源对象上与之碰撞的碰撞元素价值组
        /// 价值组的价值越大,自身则越应避让.
        /// 即 赢退 输不动
        /// </summary>
        /// <param name="startElement">源对象</param>
        /// <param name="conflictElement"></param>
        /// <param name="conflictNodes"></param>
        /// <param name="avoidElements"></param>
        private void SetupGroup(AvoidElement startElement, ConflictElement conflictElement, List<ValuedConflictNode> conflictNodes, List<AvoidElement> avoidElements)
        {
            ConflictLineSection conflictLineSection = new ConflictLineSection(startElement);
            //碰撞点处理
            conflictLineSection.ConflictElements.Add(conflictElement);
            //向后 连续组团处理
            var startIndex = startElement.ConflictElements.IndexOf(conflictElement);
            var currentIndex = startIndex;
            ConflictElement current = conflictElement;
            ConflictElement next;
            for (int i = currentIndex + 1; i < startElement.ConflictElements.Count(); i++)
            {
                next = startElement.ConflictElements[i];
                if (current.GetDistanceTo(next) > GroupingDistance)
                    break;

                conflictLineSection.ConflictElements.Add(next);
                current = next;
                currentIndex = i;
            }
            //向后 连接件处理 边界连续处理
            if (conflictElement.ConflictLocation != startElement.EndPoint && currentIndex == startElement.ConflictElements.Count() - 1)
            {
                var connector = startElement.ConnectorEnd;
                var point = startElement.EndPoint;
                if (connector != null && current.GetDistanceTo(point) <= GroupingDistance)
                {
                    var continueEle = startElement.AddConflictElement(connector);
                    conflictLineSection.ConflictElements.Add(continueEle);
                    TopoConnector(conflictNodes, avoidElements, connector);
                }
            }
            //重置
            current = conflictElement;
            currentIndex = startIndex;
            //往前 连续组团处理
            for (int i = currentIndex - 1; i >= 0; i--)
            {
                next = startElement.ConflictElements[i];
                if (current.GetDistanceTo(next)> GroupingDistance)
                    break;

                conflictLineSection.ConflictElements.Add(next);
                current = next;
                currentIndex = i;
            }
            //往前 连接件处理
            if (conflictElement.ConflictLocation != startElement.StartPoint && currentIndex == 0)
            {
                var connector = startElement.ConnectorStart;
                var point = startElement.StartPoint;
                if (connector != null && current.GetDistanceTo(point) <= GroupingDistance)
                {
                    var continueEle = startElement.AddConflictElement(connector);
                    conflictLineSection.ConflictElements.Add(continueEle);
                    TopoConnector(conflictNodes, avoidElements, connector);
                }
            }
            conflictLineSection.ConflictElements = conflictLineSection.ConflictElements.OrderByDescending(c => c.ConflictLocation, new XYZComparer()).ToList();
            ConflictLineSections.Add(conflictLineSection);
        }

        private void TopoConnector(List<ValuedConflictNode> conflictNodes, List<AvoidElement> avoidElements, Connector connector)
        {
            var connectorsToMepElement = connector.GetConnectorsToMepElement();
            foreach (var connectorToMepElement in connectorsToMepElement)
            {
                var startEle = avoidElements.FirstOrDefault(c => c.MEPElement.Id == connectorToMepElement.Owner.Id);
                if (startEle == null)
                {
                    startEle = new AvoidElement(connectorToMepElement.Owner as MEPCurve);
                    avoidElements.Add(startEle);
                }
                var conflictEle = startEle.AddConflictElement(connectorToMepElement);
                SetupGroup(startEle, conflictEle, conflictNodes, avoidElements);
            }
        }
        #endregion
    }
}
