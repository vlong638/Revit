using Autodesk.Revit.DB;
using System;

namespace MyRevit.MyTests.MepCurveAvoid
{
    public enum CompeteType
    {
        None,
        Winner,
        Loser,
    }

    /// <summary>
    /// 碰撞节点
    /// 被碰撞的基础单元,包含被碰撞双方和碰撞节点的关键信息
    /// 
    /// </summary>
    public class ConflictElement
    {
        public CompeteType CompeteType { set; get; }

        public AvoidElement AvoidEle { set; get; }
        /// <summary>
        /// 拓扑传递碰撞点位都是线的边界
        /// </summary>
        public XYZ ConflictLocation { set; get; }
        public AvoidElement ConflictEle { set; get; }
        public bool IsConnector { set; get; }
        private Connector Connector { set; get; }
        /// <summary>
        /// 待避让的对象高度
        /// </summary>
        public double AvoidHeight { set; get; }
        /// <summary>
        /// 待避让的对象宽度
        /// </summary>
        public double AvoidWidth { set; get; }
        /// <summary>
        /// 避让的高度
        /// </summary>
        public double OffsetHeight { set; get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="avoidElement">谁</param>
        /// <param name="conflictLocation">在哪里</param>
        /// <param name="conflictElement">跟谁撞了</param>
        public ConflictElement(AvoidElement avoidElement, XYZ conflictLocation, AvoidElement conflictElement)
        {
            AvoidEle = avoidElement;
            ConflictEle = conflictElement;
            ConflictLocation = conflictLocation;

            AvoidHeight = conflictElement.Height;
            AvoidWidth = conflictElement.Width;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="avoidElement">谁</param>
        /// <param name="conflictLocation">在哪里</param>
        /// <param name="conflictElement">跟谁撞了</param>
        public ConflictElement(AvoidElement avoidElement, XYZ conflictLocation, Connector connector, AvoidElement conflictElement)
        {
            AvoidEle = avoidElement;
            ConflictEle = conflictElement;
            Connector = connector;
            ConflictLocation = conflictLocation;
            IsConnector = true;

            AvoidHeight = conflictElement.Height;
            AvoidWidth = conflictElement.Width;
        }

        //点位计算
        public Guid GroupId { set; get; }
        //public XYZ Start { set; get; }//起始点
        //public XYZ End { set; get; }//终结点
        public XYZ StartSplit { set; get; }//起始点切割端
        public XYZ MiddleStart { set; get; }//中间段起始点
        public XYZ MiddleEnd { set; get; }//中间段终结点
        public XYZ EndSplit { set; get; }//终结点切割端
        public XYZ ConnectorLocation { set; get; }//边界位置

        internal double GetDistanceTo(ConflictElement next)
        {
            return ConflictLocation.DistanceTo(next.ConflictLocation) - (IsConnector ? 0 : ConflictEle.Width) - (next.IsConnector ? 0 : next.ConflictEle.Width);
        }

        internal double GetDistanceTo(XYZ point)
        {
            return ConflictLocation.DistanceTo(point) - (IsConnector ? 0 : ConflictEle.Width);
        }
    }
}
