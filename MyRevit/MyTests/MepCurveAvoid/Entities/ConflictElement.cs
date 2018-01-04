using Autodesk.Revit.DB;

namespace MyRevit.MyTests.MepCurveAvoid
{
    /// <summary>
    /// 碰撞节点
    /// 被碰撞的基础单元,包含被碰撞双方和碰撞节点的关键信息
    /// 
    /// </summary>
    public class ConflictElement
    {
        public AvoidElement AvoidEle { set; get; }
        public XYZ ConflictLocation { set; get; }

        public AvoidElement ConflictEle { set; get; }
        public bool IsConnector { set; get; }
        private Connector Connector { set; get; }

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
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="avoidElement">谁</param>
        /// <param name="conflictLocation">在哪里</param>
        /// <param name="conflictElement">跟谁撞了</param>
        public ConflictElement(AvoidElement avoidElement, XYZ conflictLocation, Connector connector)
        {
            ConflictLocation = conflictLocation;
            AvoidEle = avoidElement;
            Connector = connector;
            IsConnector = true;
        }

        //点位计算
        public XYZ Start { set; get; }//起始点
        public XYZ StartSplit { set; get; }//起始点切割端
        public XYZ MiddleStart { set; get; }//中间段起始点
        public XYZ MiddleEnd { set; get; }//中间段终结点
        public XYZ EndSplit { set; get; }//终结点切割端
        public XYZ End { set; get; }//终结点
    }
}
