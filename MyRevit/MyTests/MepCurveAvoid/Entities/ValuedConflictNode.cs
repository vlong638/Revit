using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using System.Linq;
using MyRevit.MyTests.Utilities;
using PmSoft.Common.RevitClass.Utils;
using MyRevit.Utilities;
using System.IO;
using System.Text;

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
            #region TEST
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("MEPCurveId:" + ValueNode1.OrientAvoidElement.MEPCurve.Id.IntegerValue);
            sb.Append(ValueNode1.ConflictLineSections.AvoidPriorityValue.ToString());
            sb.AppendLine();
            sb.AppendLine("MEPCurveId:" + ValueNode2.OrientAvoidElement.MEPCurve.Id.IntegerValue);
            sb.Append(ValueNode2.ConflictLineSections.AvoidPriorityValue.ToString());
            var directory = @"D:\AvoidElement\" + DateTime.Now.ToString("MM_dd_HH_mm");
            if (Directory.Exists(directory))
            {
                var files = Directory.GetFiles(directory);
                if (files.FirstOrDefault(c => c.EndsWith(".png")) != null)
                    Directory.Delete(directory, true);
                Directory.CreateDirectory(directory);
            }
            else
                Directory.CreateDirectory(directory);
            File.WriteAllText(directory + $"\\{ValueNode1.OrientAvoidElement.MEPCurve.Id.IntegerValue}_{ValueNode2.OrientAvoidElement.MEPCurve.Id.IntegerValue}.txt", sb.ToString()); 
            #endregion
            if (!isWinnerSettled)
            {
                //避让总是以最大的避让幅度作为连续的标准
                var maxHeight = winner.ConflictLineSections.Max(c => c.ConflictElements.Max(d => d.AvoidHeight));
                var maxWidth = winner.ConflictLineSections.Max(c => c.ConflictElements.Max(d => d.AvoidWidth));
                foreach (var ConflictLineSection in winner.ConflictLineSections)
                    foreach (var ConflictElement in ConflictLineSection.ConflictElements)
                    {
                        ConflictElement.CompeteType = CompeteType.Winner;
                        ConflictElement.AvoidHeight = maxHeight;
                        ConflictElement.AvoidWidth = maxWidth;
                    }
                var conflictElement = winner.OrientAvoidElement.ConflictElements.First(c => c.ConflictLocation.VL_XYEqualTo(winner.ValuedConflictNode.ConflictLocation));
                //定位计算
                CalculateLocations(conflictElement, winner, ConflictLocation, avoidElements);
                if (conflictLineSections_Collection.FirstOrDefault(c => c.GroupId == winner.ConflictLineSections.GroupId) == null)//ConflictLineSections汇总
                    conflictLineSections_Collection.Add(winner.ConflictLineSections);
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
                #region 各避让点避让处理
                for (int i = 0; i < ConflictLineSection.ConflictElements.Count(); i++)
                {
                    var conflictElement = ConflictLineSection.ConflictElements[i];
                    //边界计算
                    if (i > 0 && i < ConflictLineSection.ConflictElements.Count() - 1)
                        continue;
                    //区块信息整理
                    if (conflictElement.IsConnector)
                    {
                        if (avoidElement.IsStartPoint(conflictElement))
                        {
                            var connector = avoidElement.ConnectorStart;
                            Connector linkedConnector = connector.GetConnectedConnector();
                            if (linkedConnector != null)
                                connector.DisconnectFrom(linkedConnector);
                            ConflictLineSection.StartLinkedConnector = linkedConnector;
                            ConflictLineSection.StartPoint = avoidElement.StartPoint;
                        }
                        if (avoidElement.IsEndPoint(conflictElement))
                        {
                            var connector = avoidElement.ConnectorEnd;
                            Connector linkedConnector = connector.GetConnectedConnector();
                            if (linkedConnector != null)
                                connector.DisconnectFrom(linkedConnector);
                            ConflictLineSection.EndLinkedConnector = linkedConnector;
                            ConflictLineSection.EndPoint = avoidElement.EndPoint;
                        }
                    }
                    CalculateLocations(avoidElement, conflictElement);
                    #region old 早期代码 理由待分析
                    //CalculateLocations(avoidElement, currentConflictEle, startConflictElement.Height);
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
                    #endregion
                }
                #endregion
            }

            #region 避让高度一致化
            var maxOffset = winner.ConflictLineSections.Max(c => c.ConflictElements.Max(d => d.OffsetHeight));
            foreach (var ConflictLineSection in winner.ConflictLineSections)
            {
                var avoidElement = avoidElements.First(c => c.MEPCurve.Id == ConflictLineSection.ElementId);
                XYZ verticalDirection = avoidElement.GetVerticalVector();
                for (int i = 0; i < ConflictLineSection.ConflictElements.Count(); i++)
                {
                    if (i > 0 && i < ConflictLineSection.ConflictElements.Count() - 1)
                        continue;
                    var conflictElement = ConflictLineSection.ConflictElements[i];
                    var diff = maxOffset - conflictElement.OffsetHeight;
                    if (!diff.IsMiniValue())
                    {
                        conflictElement.StartSplit += diff * verticalDirection;
                        conflictElement.MiddleStart += diff * verticalDirection;
                        conflictElement.MiddleEnd += diff * verticalDirection;
                        conflictElement.EndSplit += diff * verticalDirection;
                        if (new XYComparer().Compare(conflictElement.StartSplit, avoidElement.StartPoint) > 0)
                            avoidElement.StartPoint += diff * verticalDirection;
                        if (new XYComparer().Compare(conflictElement.EndSplit, avoidElement.EndPoint) < 0)
                            avoidElement.EndPoint += diff * verticalDirection;
                        conflictElement.OffsetHeight = maxOffset;
                    }
                }
            }
            winner.ConflictLineSections.OffsetHeight = maxOffset;
            #endregion
        }

        static double MiniMepLength = UnitTransUtils.MMToFeet(20);//最短连接管长度 双向带连接件
        static double MiniSpace = UnitTransUtils.MMToFeet(10);//避免碰撞及提供留白的安全距离
        public static double GetFixedJumpLength(double orientJumpLength, double angle)
        {
            if ((angle - Math.PI).IsMiniValue())
                return orientJumpLength;
            else if (angle.IsMiniValue())
                return 0;
            else
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
            var elementToAvoidHeight = conflictElement.AvoidHeight;
            var elementToAvoidWidth = conflictElement.AvoidWidth;
            XYZ direction1 = (curve as Line).Direction;
            double faceAngle = 0;
            XYZ direction2 = ((elementToAvoid.MEPCurve.Location as LocationCurve).Curve as Line).Direction;//TODO 连接件的ElementToAvoid依旧需要填上 作为溯源数据源
            faceAngle = direction1.AngleOnPlaneTo(direction2, new XYZ(0, 0, 1));
            #region old
            //if (conflictElement.IsConnector)
            //{
            //    faceAngle = Math.PI / 2;//问题出在 如果连接点作为
            //    conflictElement.ConnectorLocation = conflictElement.ConflictLocation + height * verticalDirection;
            //}
            //else
            //{
            //    XYZ direction2 = ((elementToAvoid.MEPCurve.Location as LocationCurve).Curve as Line).Direction;
            //    faceAngle = direction1.AngleOnPlaneTo(direction2, new XYZ(0, 0, 1));
            //} 
            #endregion
            //对象信息反填到基础模型中
            //点位计算
            var midPoint = conflictElement.ConflictLocation;
            //max(垂直最短留白距离,最小斜边长度,最短切割距离) 
            if (height == -1)
            {
                height = avoidElement.Height / 2 + elementToAvoidHeight / 2 + MiniSpace;
                height = Math.Max(height, MiniMepLength + miniConnectHeight * 2);//考虑构件的最小高度需求
            }
            if (conflictElement.IsConnector)
                conflictElement.ConnectorLocation = conflictElement.ConflictLocation + height * verticalDirection;
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
            conflictElement.StartSplit = midPoint + parallelDirection * widthDown;
            conflictElement.EndSplit = midPoint - parallelDirection * widthDown;
            midPoint += height * verticalDirection;
            conflictElement.MiddleStart = midPoint + parallelDirection * widthUp;
            conflictElement.MiddleEnd = midPoint - parallelDirection * widthUp;
            //过界修正,过界则做边界的垂直偏移
            if (new XYComparer().Compare(conflictElement.StartSplit, pointStart) > 0)
                avoidElement.StartPoint += height * verticalDirection;
            if (new XYComparer().Compare(conflictElement.EndSplit, pointEnd)<0)
                avoidElement.EndPoint += height * verticalDirection;
            conflictElement.OffsetHeight = height;
        }
        #endregion
    }
}
