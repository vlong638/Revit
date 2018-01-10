using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using System.Linq;
using MyRevit.MyTests.Utilities;

namespace MyRevit.MyTests.MepCurveAvoid
{
    /// <summary>
    /// 价值分析的碰撞节点(对抗位)
    /// 被碰撞的基础单元,包含被碰撞双方和碰撞节点的关键信息
    /// 
    /// </summary>
    public class ValuedConflictNode
    {
        public Guid Id = Guid.NewGuid();

        public ValuedConflictNode(AvoidElement avoidElement, XYZ conflictLocation, AvoidElement elementConflicted)
        {
            ConflictLocation = conflictLocation;
            ValueNode1 = new ValueNode(this, avoidElement);
            ValueNode2 = new ValueNode(this, elementConflicted);
        }
        public XYZ ConflictLocation;
        public ValueNode ValueNode1 { set; get; }
        public ValueNode ValueNode2 { set; get; }

        #region 价值分组
        public bool IsSettled { get { return (ValueNode1.ConflictLineSections.IsSettled && ValueNode2.ConflictLineSections.IsSettled); } }
        public void Settle(List<ValuedConflictNode> conflictNodes, List<AvoidElement> avoidElements)
        {
            if (IsSettled)
                return;

            ValueNode1.SetupValue(ValueNode1.OrientAvoidElement.ConflictElements.First(c => c.ConflictLocation.VL_XYEqualTo(ConflictLocation)), conflictNodes, avoidElements);
            ValueNode2.SetupValue(ValueNode2.OrientAvoidElement.ConflictElements.First(c => c.ConflictLocation.VL_XYEqualTo(ConflictLocation)), conflictNodes, avoidElements);
        }

        public static PriorityValueComparer Comparer= new PriorityValueComparer();
        public bool IsCompeted = false;
        internal void Compete(List<AvoidElement> avoidElements)
        {
            if (!IsSettled || IsCompeted)
                return;

            ValueNode winner;
            ValueNode loser;
            if (Comparer.Compare(ValueNode1.ConflictLineSections.PriorityValue, ValueNode2.ConflictLineSections.PriorityValue) > 0)
            {
                ValueNode1.ConflictLineSections.PriorityValue.CompeteType = CompeteType.Winner;
                ValueNode2.ConflictLineSections.PriorityValue.CompeteType = CompeteType.Loser;
                winner = ValueNode1;
                loser = ValueNode2;
            }
            else
            {
                ValueNode1.ConflictLineSections.PriorityValue.CompeteType = CompeteType.Loser;
                ValueNode2.ConflictLineSections.PriorityValue.CompeteType = CompeteType.Winner;
                winner = ValueNode2;
                loser = ValueNode1;
            }
            foreach (var ConflictLineSection in winner.ConflictLineSections)
                foreach (var ConflictLineNode in ConflictLineSection)
                {
                    ConflictLineNode.CompeteType = CompeteType.Winner;
                    //对象信息反填到基础模型中
                    var avoidElement = avoidElements.First(c => c.MEPElement == winner.OrientAvoidElement.MEPElement);
                    var conflictElement = avoidElement.ConflictElements.First(c => c.ConflictLocation.VL_XYEqualTo(winner.ValuedConflictNode.ConflictLocation));
                    ConflictLineNode.GroupId = winner.ConflictLineSections.GroupId;
                }
            foreach (var ConflictLineSection in loser.ConflictLineSections)
                foreach (var ConflictLineNode in ConflictLineSection)
                    ConflictLineNode.CompeteType = CompeteType.Loser;
            IsCompeted = true;
        }
        #endregion
    }
}
