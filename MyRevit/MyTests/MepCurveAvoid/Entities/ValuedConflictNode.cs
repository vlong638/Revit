using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using System.Linq;
using MyRevit.MyTests.Utilities;
using PmSoft.Common.RevitClass.Utils;
using MyRevit.Utilities;

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
        public bool IsGrouped { get { return (ValueNode1.ConflictLineSections.IsGrouped && ValueNode2.ConflictLineSections.IsGrouped); } }
        public bool IsValued { get { return (ValueNode1.ConflictLineSections.IsValued && ValueNode2.ConflictLineSections.IsValued); } }
        public void Grouping(List<ValuedConflictNode> conflictNodes, List<AvoidElement> avoidElements)
        {
            if (IsGrouped)
                return;

            ValueNode1.Grouping(ValueNode1.OrientAvoidElement.ConflictElements.First(c => c.ConflictLocation.VL_XYEqualTo(ConflictLocation)), conflictNodes, avoidElements);
            ValueNode2.Grouping(ValueNode2.OrientAvoidElement.ConflictElements.First(c => c.ConflictLocation.VL_XYEqualTo(ConflictLocation)), conflictNodes, avoidElements);
        }

        internal void Valuing(List<ValuedConflictNode> conflictNodes, List<AvoidElement> avoidElements)
        {
            if (IsValued)
                return;

            ValueNode1.Valuing(ValueNode1.OrientAvoidElement.ConflictElements.First(c => c.ConflictLocation.VL_XYEqualTo(ConflictLocation)), conflictNodes, avoidElements);
            ValueNode2.Valuing(ValueNode2.OrientAvoidElement.ConflictElements.First(c => c.ConflictLocation.VL_XYEqualTo(ConflictLocation)), conflictNodes, avoidElements);
        }

        public static PriorityValueComparer Comparer = new PriorityValueComparer();
        public bool IsCompeted = false;
        internal void Compete(List<AvoidElement> avoidElements, List<ConflictLineSections> conflictLineSections_Collection)
        {
            if (!IsGrouped || IsCompeted)
                return;

            bool isWinnerSettled;
            bool isLoserSettled;
            ValueNode winner = null;
            ValueNode loser = null;
            if (Comparer.Compare(ValueNode1.ConflictLineSections.AvoidPriorityValue, ValueNode2.ConflictLineSections.AvoidPriorityValue) > 0)
            {
                isWinnerSettled = ValueNode1.ConflictLineSections.AvoidPriorityValue.CompeteType == CompeteType.Winner;
                if (!isWinnerSettled)
                {
                    ValueNode1.ConflictLineSections.AvoidPriorityValue.CompeteType = CompeteType.Winner;
                    winner = ValueNode1;
                }
                isLoserSettled = ValueNode2.ConflictLineSections.AvoidPriorityValue.CompeteType == CompeteType.Loser;
                if (!isLoserSettled)
                {
                    ValueNode2.ConflictLineSections.AvoidPriorityValue.CompeteType = CompeteType.Loser;
                    loser = ValueNode2;
                }
            }
            else
            {
                isLoserSettled = ValueNode1.ConflictLineSections.AvoidPriorityValue.CompeteType == CompeteType.Loser;
                if (!isLoserSettled)
                {
                    ValueNode1.ConflictLineSections.AvoidPriorityValue.CompeteType = CompeteType.Loser;
                    loser = ValueNode1;
                }
                isWinnerSettled = ValueNode2.ConflictLineSections.AvoidPriorityValue.CompeteType == CompeteType.Winner;
                if (!isWinnerSettled)
                {
                    ValueNode2.ConflictLineSections.AvoidPriorityValue.CompeteType = CompeteType.Winner;
                    winner = ValueNode2;
                }
            }
            if (!isWinnerSettled)
            {
                foreach (var ConflictLineSection in winner.ConflictLineSections)
                    foreach (var ConflictElement in ConflictLineSection.ConflictElements)
                        ConflictElement.CompeteType = CompeteType.Winner;
                var conflictElement = winner.OrientAvoidElement.ConflictElements.First(c => c.ConflictLocation.VL_XYEqualTo(winner.ValuedConflictNode.ConflictLocation));
                //定位计算
                CalculateLocations(winner.OrientAvoidElement, conflictElement);
                winner.ConflictLineSections.Height = conflictElement.Height;
                CalculateLocations(conflictElement, winner, ConflictLocation, avoidElements);
                if (conflictLineSections_Collection.FirstOrDefault(c => c.GroupId == winner.ConflictLineSections.GroupId) == null)//ConflictLineSections汇总
                    conflictLineSections_Collection.Add(winner.ConflictLineSections);//TODO StartConnector信息有 EndConnector信息已经录入但是丢失了
            }
            if (!isLoserSettled)
            {
                foreach (var ConflictLineSection in loser.ConflictLineSections)
                    foreach (var ConflictElement in ConflictLineSection.ConflictElements)
                        ConflictElement.CompeteType = CompeteType.Loser;
            }
            IsCompeted = true;
        }

        private void CalculateLocations(ConflictElement startConflictElement, ValueNode winner, XYZ conflictLocation, List<AvoidElement> avoidElements)
        {
            foreach (var ConflictLineSection in winner.ConflictLineSections)
            {
                var avoidElement = avoidElements.First(c => c.MEPCurve.Id == ConflictLineSection.ElementId);
                for (int i = 0; i < ConflictLineSection.ConflictElements.Count(); i++)
                {
                    var currentConflictEle = ConflictLineSection.ConflictElements[i];
                    //边界计算
                    if (i == 0 || i == ConflictLineSection.ConflictElements.Count() - 1)
                    {
                        //区块信息整理
                        if (currentConflictEle.IsConnector)
                        {
                            if (avoidElement.IsStartPoint(currentConflictEle))
                            {
                                var connector = avoidElement.ConnectorStart;
                                Connector linkedConnector = connector.GetConnectedConnector();
                                if (linkedConnector != null)
                                    connector.DisconnectFrom(linkedConnector);
                                ConflictLineSection.StartLinkedConnector = linkedConnector;
                                ConflictLineSection.StartPoint = avoidElement.StartPoint;
                            }
                            if (avoidElement.IsEndPoint(currentConflictEle))
                            {
                                var connector = avoidElement.ConnectorEnd;
                                Connector linkedConnector = connector.GetConnectedConnector();
                                if (linkedConnector != null)
                                    connector.DisconnectFrom(linkedConnector);
                                ConflictLineSection.EndLinkedConnector = linkedConnector;
                                ConflictLineSection.EndPoint = avoidElement.EndPoint;
                            }
                        }
                        CalculateLocations(avoidElement, currentConflictEle, startConflictElement.Height);
                        ////重叠关系处理
                        //if (i == 0)
                        //{
                        //    var j = 1;
                        //    while (j < ConflictLineSection.ConflictElements.Count() - 1)
                        //    {
                        //        var next = ConflictLineSection.ConflictElements[j];
                        //        if (currentConflictEle.ConflictLocation.VL_XYEqualTo(next.ConflictLocation))
                        //            CalculateLocations(avoidElement, currentConflictEle, startConflictElement.Height);
                        //        else
                        //            break;
                        //        j++;
                        //    }
                        //}
                        //if (i == ConflictLineSection.ConflictElements.Count() - 1)
                        //{
                        //    var j = ConflictLineSection.ConflictElements.Count() - 2;
                        //    while (j > 0)
                        //    {
                        //        var next = ConflictLineSection.ConflictElements[j];
                        //        if (currentConflictEle.ConflictLocation.VL_XYEqualTo(next.ConflictLocation))
                        //            CalculateLocations(avoidElement, currentConflictEle, startConflictElement.Height);
                        //        else
                        //            break;
                        //        j--;
                        //    }
                        //}
                    }
                }
            }
        }

        static double MiniMepLength = UnitTransUtils.MMToFeet(20);//最短连接管长度 双向带连接件
        static double MiniSpace = UnitTransUtils.MMToFeet(10);//避免碰撞及提供留白的安全距离
        public static double GetFixedJumpLength(double orientJumpLength, double angle)
        {
            return orientJumpLength / Math.Abs(Math.Sin(angle));
        }
        private static void CalculateLocations(AvoidElement avoidElement, ConflictElement conflictElement, double height = -1)
        {
            int id = avoidElement.MEPCurve.Id.IntegerValue;
            XYZ verticalDirection = avoidElement.GetVerticalVector();
            double angleToTurn = avoidElement.AngleToTurn;
            double miniConnectHeight = avoidElement.ConnectHeight;
            double miniConnectWidth = avoidElement.ConnectWidth;
            double offsetWidth = avoidElement.OffsetWidth;
            var curve = (avoidElement.MEPCurve.Location as LocationCurve).Curve;
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
                conflictElement.ConnectorLocation = conflictElement.ConflictLocation + height * verticalDirection;
            }
            else
            {
                elementToAvoidHeight = elementToAvoid.Height;
                elementToAvoidWidth = elementToAvoid.Width;
                XYZ direction2 = ((elementToAvoid.MEPCurve.Location as LocationCurve).Curve as Line).Direction;
                faceAngle = direction1.AngleOnPlaneTo(direction2, new XYZ(0, 0, 1));
            }
            //对象信息反填到基础模型中
            //点位计算
            var midPoint = conflictElement.ConflictLocation;
            //max(垂直最短留白距离,最小斜边长度,最短切割距离) 
            if (height == -1)
            {
                height = avoidElement.Height / 2 + elementToAvoidHeight / 2 + MiniSpace;
                height = Math.Max(height, MiniMepLength + miniConnectHeight * 2);//考虑构件的最小高度需求
                conflictElement.Height = height;
            }
            //widthUp = Math.Max(widthUp, offsetWidth+miniMepLength); //TODO 考虑构件的最小宽度需求 //height - miniConnectWidth
            //TODO 考虑矩形的最佳方案
            //if (avoidElement.Width!= avoidElement.Height|| elementToAvoidWidth!= elementToAvoidHeight)
            //{
            //}
            var widthUp = MiniMepLength / 2 + offsetWidth;//构件最短需求
            var diameterAvoid = Math.Max(avoidElement.Width, avoidElement.Height);
            var diameterToAvoid = Math.Max(elementToAvoidWidth, elementToAvoidHeight);
            widthUp = Math.Max(widthUp, (diameterAvoid / 2 + diameterToAvoid / 2 + MiniSpace) / Math.Sin(angleToTurn) - height * Math.Tan(angleToTurn));//斜边最短需求
            widthUp = Math.Max(widthUp, avoidElement.Width / 2 + elementToAvoidWidth / 2 + MiniSpace);//直径最短需求
            var widthDown = widthUp + height / Math.Tan(angleToTurn);//水平最短距离对应的水平偏移
            widthUp = GetFixedJumpLength(widthUp, faceAngle);
            widthDown = GetFixedJumpLength(widthDown, faceAngle);
            //double co = Math.Abs(Math.Cos(faceAngle));
            //if (!co.IsMiniValue())
            //{
            //    widthUp = widthUp / co;
            //    widthDown = widthDown / co;
            //}
            conflictElement.StartSplit = midPoint + parallelDirection * widthDown;
            conflictElement.EndSplit = midPoint - parallelDirection * widthDown;
            midPoint += height * verticalDirection;
            conflictElement.MiddleStart = midPoint + parallelDirection * widthUp;
            conflictElement.MiddleEnd = midPoint - parallelDirection * widthUp;
        }
        #endregion
    }
}
