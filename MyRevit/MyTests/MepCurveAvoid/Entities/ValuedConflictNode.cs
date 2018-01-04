using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace MyRevit.MyTests.MepCurveAvoid
{
    /// <summary>
    /// 价值分析的碰撞节点
    /// 被碰撞的基础单元,包含被碰撞双方和碰撞节点的关键信息
    /// 
    /// </summary>
    public class ValuedConflictNode
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="avoidElement">谁</param>
        /// <param name="conflictLocation">在哪里</param>
        /// <param name="conflictElement">跟谁撞了</param>
        public ValuedConflictNode(AvoidElement avoidElement, XYZ conflictLocation, AvoidElement conflictElement)
        {
            ConflictLocation = conflictLocation;
            ValueNode1 = new ValueNode(avoidElement);
            ValueNode2 = new ValueNode(conflictElement);
        }

        public XYZ ConflictLocation { set; get; }
        public ValueNode ValueNode1 { set; get; }
        public ValueNode ValueNode2 { set; get; }

        #region 价值分组
        public bool IsSettled { get { return (ValueNode1.IsSettled && ValueNode2.IsSettled); } }
        internal void Settle(List<ValuedConflictNode> conflictNodes, List<AvoidElement> avoidElements)
        {
            if (IsSettled)
                return;

            ValueNode1.SettleValue(ConflictLocation, conflictNodes, avoidElements);
            ValueNode2.SettleValue(ConflictLocation, conflictNodes, avoidElements);
        }
        #endregion
    }
}
