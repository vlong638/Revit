﻿using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using System.Linq;
using MyRevit.MyTests.Utilities;
using PmSoft.Common.RevitClass.Utils;

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

            bool isWinnerSettled;
            bool isLoserSettled;
            ValueNode winner=null;
            ValueNode loser=null;
            if (Comparer.Compare(ValueNode1.ConflictLineSections.PriorityValue, ValueNode2.ConflictLineSections.PriorityValue) > 0)
            {
                isWinnerSettled = ValueNode1.ConflictLineSections.PriorityValue.CompeteType == CompeteType.Winner;
                if (!isWinnerSettled)
                {
                    ValueNode1.ConflictLineSections.PriorityValue.CompeteType = CompeteType.Winner;
                    winner = ValueNode1;
                }
                isLoserSettled = ValueNode2.ConflictLineSections.PriorityValue.CompeteType == CompeteType.Loser;
                if (!isLoserSettled)
                {
                    ValueNode2.ConflictLineSections.PriorityValue.CompeteType = CompeteType.Loser;
                    loser = ValueNode2;
                }
            }
            else
            {
                isLoserSettled = ValueNode1.ConflictLineSections.PriorityValue.CompeteType == CompeteType.Loser;
                if (!isLoserSettled)
                {
                    ValueNode1.ConflictLineSections.PriorityValue.CompeteType = CompeteType.Loser;
                    loser = ValueNode1;
                }
                isWinnerSettled = ValueNode2.ConflictLineSections.PriorityValue.CompeteType == CompeteType.Winner;
                if (!isWinnerSettled)
                {
                    ValueNode2.ConflictLineSections.PriorityValue.CompeteType = CompeteType.Winner;
                    winner = ValueNode2;
                }
            }
            if (!isWinnerSettled)
            {
                foreach (var ConflictLineSection in winner.ConflictLineSections)
                    foreach (var ConflictElement in ConflictLineSection.ConflictElements)
                    {
                        ConflictElement.CompeteType = CompeteType.Winner;
                        ConflictElement.GroupId = winner.ConflictLineSections.GroupId;
                    }
                var conflictElement = winner.OrientAvoidElement.ConflictElements.First(c => c.ConflictLocation.VL_XYEqualTo(winner.ValuedConflictNode.ConflictLocation));
                CalculateLocations(winner.OrientAvoidElement, conflictElement);
                CalculateLocations(conflictElement, winner, ConflictLocation, avoidElements);

                //var avoidElement = avoidElements.First(c => c.MEPElement == winner.OrientAvoidElement.MEPElement);
                //var conflictElement = avoidElement.ConflictElements.First(c => c.ConflictLocation.VL_XYEqualTo(winner.ValuedConflictNode.ConflictLocation));
                //CalculateLocations(avoidElement, conflictElement);
                //CalculateLocations(conflictElement, winner, ConflictLocation, avoidElements);
            }
            if (!isLoserSettled)
            {
                foreach (var ConflictLineSection in loser.ConflictLineSections)
                    foreach (var ConflictElement in ConflictLineSection.ConflictElements)
                        ConflictElement.CompeteType = CompeteType.Loser;
            }
            //定位计算
            IsCompeted = true;
        }

        /// <summary>
        /// TODO 可能存在的问题 
        /// conflictEle
        /// </summary>
        /// <param name="startConflictElement"></param>
        /// <param name="winner"></param>
        /// <param name="conflictLocation"></param>
        private void CalculateLocations(ConflictElement startConflictElement, ValueNode winner, XYZ conflictLocation, List<AvoidElement> avoidElements)
        {
            foreach (var ConflictLineSection in winner.ConflictLineSections)
            {
                var avoidElement = avoidElements.First(c => c.MEPElement.Id == ConflictLineSection.ElementId);
                for (int i = 0; i < ConflictLineSection.ConflictElements.Count(); i++)
                {
                    var conflictEle = ConflictLineSection.ConflictElements[i];
                    //边界计算
                    if (i == 0 || i == ConflictLineSection.ConflictElements.Count() - 1)
                        CalculateLocations(avoidElement, conflictEle, startConflictElement.Height);
                }
            }
        }

        static XYZ VerticalDirection = new XYZ(0, 0, 1);
        private static void CalculateLocations(AvoidElement avoidElement, ConflictElement conflictElement, double height = -1)
        {
            var miniMepLength = UnitTransUtils.MMToFeet(96);//最短连接管长度 双向带连接件
            var miniSpace = UnitTransUtils.MMToFeet(5);//避免碰撞及提供留白的安全距离
            var angleToTurn = Math.PI / 4;//45°
            var curve = (avoidElement.MEPElement.Location as LocationCurve).Curve;
            var pointStart = avoidElement.StartPoint;
            var pointEnd = avoidElement.EndPoint;
            XYZ parallelDirection = (pointStart - pointEnd).Normalize();
            var elementToAvoid = conflictElement.ConflictEle;

            XYZ direction1 = (curve as Line).Direction;
            double elementToAvoidHeight = 0;
            double elementToAvoidWidth = 0;
            double faceAngle = 0;
            if (conflictElement.IsConnector)
            {
                elementToAvoidHeight = 0;
                elementToAvoidWidth = 0;
                faceAngle = Math.PI / 2;
                conflictElement.ConnectorLocation = conflictElement.ConflictLocation + height * VerticalDirection;
            }
            else
            {
                elementToAvoidHeight = elementToAvoid.Height;
                elementToAvoidWidth = elementToAvoid.Width;
                XYZ direction2 = ((elementToAvoid.MEPElement.Location as LocationCurve).Curve as Line).Direction;
                faceAngle = direction1.AngleOnPlaneTo(direction2, new XYZ(0, 0, 1));
            }
            //对象信息反填到基础模型中
            //点位计算
            var midPoint = conflictElement.ConflictLocation;
            //max(垂直最短留白距离,最小斜边长度,最短切割距离) 
            if (height == -1)
            {
                height = avoidElement.Height / 2 + elementToAvoidHeight / 2 + miniSpace;
                //height = Math.Max(height,构件的最小高度);//TODO 考虑构件的最小高度需求
                conflictElement.Height = height;
            }
            var widthUp = miniMepLength / 2;
            //widthUp = Math.Max(widthUp, height - 构件的最小宽度); //TODO 考虑构件的最小宽度需求
            var diameterAvoid = Math.Max(avoidElement.Width, avoidElement.Height);
            var diameterToAvoid = Math.Max(elementToAvoidWidth, elementToAvoidHeight);
            widthUp = Math.Max(widthUp, (diameterAvoid / 2 + diameterToAvoid / 2 + miniSpace) / Math.Sin(angleToTurn) - height * Math.Tan(angleToTurn));
            var widthDown = widthUp + height / Math.Tan(angleToTurn);//水平最短距离对应的水平偏移
            widthDown = Math.Max(widthDown, avoidElement.Width / 2 + elementToAvoidWidth / 2 + miniSpace);
            widthUp = widthUp / Math.Abs(Math.Sin(faceAngle));
            widthDown = widthDown / Math.Abs(Math.Sin(faceAngle));
            conflictElement.StartSplit = midPoint + parallelDirection * widthDown;
            conflictElement.EndSplit = midPoint - parallelDirection * widthDown;
            midPoint += height * VerticalDirection;
            conflictElement.MiddleStart = midPoint + parallelDirection * widthUp;
            conflictElement.MiddleEnd = midPoint - parallelDirection * widthUp;
        }
        #endregion
    }
}