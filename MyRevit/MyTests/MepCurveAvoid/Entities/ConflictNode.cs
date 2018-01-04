using Autodesk.Revit.DB;
using System.Collections.Generic;
using System;
using System.Linq;

namespace MyRevit.MyTests.MepCurveAvoid
{
    /// <summary>
    /// 线的价值取决于为其避让的元素的总和
    /// </summary>
    public class ConflictLine: List<ConflictElement> 
    {
        //public List<ConflictElement> ConflictElements { set; get; }
    }

    /// <summary>
    /// 碰撞价值体
    /// </summary>
    public class ValueNode
    {
        public AvoidElement OrientAvoidElement { set; get; }

        public ValueNode(AvoidElement avoidElement)
        {
            OrientAvoidElement = avoidElement;
        }

        #region 价值分析

        public bool IsSettled = false;
        public List<ConflictLine> ConflictLines { set; get; }
        public int PriorityValue { set; get; }

        /// <summary>
        /// 价值分析
        /// 组团+价值计算
        /// 基本逻辑:
        /// 找到原始碰撞单元
        /// 连续组团处理
        /// 边界连接件组团处理
        /// 价值计算
        /// </summary>
        /// <param name="conflictLocation"></param>
        /// <param name="conflictNodes">源碰撞</param>

        public void SettleValue(XYZ conflictLocation, List<ValuedConflictNode> conflictNodes, List<AvoidElement> avoidElements)
        {
            ///找到原始碰撞单元
            ///连续组团处理
            ///边界连接件组团处理
            ///价值计算
            var groupingDistance = PmSoft.Common.RevitClass.Utils.UnitTransUtils.MMToFeet(300);//成组的最小距离
            ConflictLines = new List<ConflictLine>();
            ConflictLine conflictLine = new ConflictLine();
            var conflictElement = OrientAvoidElement.ConflictElements.First(c => c.ConflictLocation == conflictLocation);
            conflictLine.Add(conflictElement);





            //ConflictNodes.Add(new ValuedConflictNode(OrientAvoidElement, conflictElement.ConflictLocation, conflictElement.ConflictEle));
            var startIndex = OrientAvoidElement.ConflictElements.IndexOf(conflictElement);
            var currentIndex = startIndex;
            ConflictElement current = conflictElement;
            ConflictElement next;
            //向后 连续组团处理
            for (int i = currentIndex + 1; i < OrientAvoidElement.ConflictElements.Count(); i++)
            {
                next = OrientAvoidElement.ConflictElements[i];
                if (current.ConflictLocation.DistanceTo(next.ConflictLocation) > groupingDistance)
                    break;
                current = next;
                currentIndex = i;
            }
            //向后 连接件处理
            if (currentIndex== OrientAvoidElement.ConflictElements.Count()-1)
            {
                if (current.ConflictLocation.DistanceTo(conflictElement.End) <= groupingDistance)
                {
                    ConflictNodes.Add(new ValuedConflictNode(conflictElement.ConflictLocation, OrientAvoidElement, conflictElement.AvoidEle.ConnectorEnd));
                }
            }
            //重置
            current = conflictElement;
            currentIndex = startIndex;
            //往前 连续组团处理
            for (int i = currentIndex - 1; i >= 0; i--)
            {
                next = OrientAvoidElement.ConflictElements[i];
                if (current.ConflictLocation.DistanceTo(next.ConflictLocation) > groupingDistance)
                    break;
                current = next;
                currentIndex = i;
            }
            //往前 连接件处理
            if (currentIndex == 0)
            {
                if (current.ConflictLocation.DistanceTo(conflictElement.Start) <= groupingDistance)
                {
                    ConflictNodes.Add(new ValuedConflictNode(OrientAvoidElement, conflictElement.ConflictLocation, conflictElement.ConflictEle));
                }
            }
            //价值计算

           

            throw new NotImplementedException();
        }
        #endregion
    }

}
