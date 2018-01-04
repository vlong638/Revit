using Autodesk.Revit.DB;
using System.Collections.Generic;
using System;
using System.Linq;

namespace MyRevit.MyTests.MepCurveAvoid
{
    /// <summary>
    /// 线的价值取决于为其避让的元素的总和
    /// </summary>
    public class ConflictLineSection : List<ConflictElement>
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
        public List<ConflictLineSection> ConflictLineSections { set; get; }
        public int PriorityValue { set; get; }

        static double GroupingDistance = PmSoft.Common.RevitClass.Utils.UnitTransUtils.MMToFeet(300);//成组的最小距离
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
            if (IsSettled)
                return;

            ///找到原始碰撞单元
            ///连续组团处理
            ///边界连接件组团处理
            ///价值计算
            ConflictLineSections = new List<ConflictLineSection>();

            ConflictLineSection conflictLineSection = new ConflictLineSection();
            //碰撞点处理
            var conflictElement = OrientAvoidElement.ConflictElements.First(c => c.ConflictLocation == conflictLocation);
            conflictLineSection.Add(conflictElement);
            //向后 连续组团处理
            var startIndex = OrientAvoidElement.ConflictElements.IndexOf(conflictElement);
            var currentIndex = startIndex;
            ConflictElement current = conflictElement;
            ConflictElement next;
            for (int i = currentIndex + 1; i < OrientAvoidElement.ConflictElements.Count(); i++)
            {
                next = OrientAvoidElement.ConflictElements[i];
                if (current.ConflictLocation.DistanceTo(next.ConflictLocation) > GroupingDistance)
                    break;
                current = next;
                currentIndex = i;
            }
            //向后 连接件处理
            if (currentIndex == OrientAvoidElement.ConflictElements.Count() - 1)
            {
                if (OrientAvoidElement.ConnectorEnd != null && current.ConflictLocation.DistanceTo(OrientAvoidElement.EndPoint) <= GroupingDistance)
                {
                    ConflictElement endContinue = new ConflictElement(OrientAvoidElement, OrientAvoidElement.EndPoint, OrientAvoidElement.ConnectorEnd);
                    conflictLineSection.Add(endContinue);
                    OrientAvoidElement.ConflictElements.Add(endContinue);
                    //TODO 边界连续处理
                    var connectedMepElement1 = OrientAvoidElement.ConnectorEnd.GetConnectedMepElements();
                }
            }
            //重置
            current = conflictElement;
            currentIndex = startIndex;
            //往前 连续组团处理
            for (int i = currentIndex - 1; i >= 0; i--)
            {
                next = OrientAvoidElement.ConflictElements[i];
                if (current.ConflictLocation.DistanceTo(next.ConflictLocation) > GroupingDistance)
                    break;
                current = next;
                currentIndex = i;
            }
            //往前 连接件处理
            if (currentIndex == 0)
            {
                if (OrientAvoidElement.ConnectorStart != null && current.ConflictLocation.DistanceTo(OrientAvoidElement.StartPoint) <= GroupingDistance)
                {
                    ConflictElement startContinue = new ConflictElement(OrientAvoidElement, OrientAvoidElement.StartPoint, OrientAvoidElement.ConnectorStart);
                    conflictLineSection.Add(startContinue);
                    OrientAvoidElement.ConflictElements.Add(startContinue);
                    //TODO 边界连续处理
                    var connectedMepElement = OrientAvoidElement.ConnectorEnd.GetConnectedMepElements();
                }
            }
            //TODO 组内关系传递 避免重复计算

            //TODO 价值计算

        }
        #endregion
    }

}
