namespace MyRevit.MyTests.MepCurveAvoid
{
    public enum PriorityElementType
    {
        None,
        UnpressedPipe,//无压管
        LargeDuct,//大风管
        LargePressedPipe,//大有压管
        LargeCableTray,//大桥架
        NormalDuct,//标准风管
        NormalPressedPipe,//标准有压管
        NormalCableTray,//标准桥架
        Connector,//连接件
    }
    /// <summary>
    /// 避让处理类型
    /// </summary>
    public enum ConflictNodeDealType
    {
        /// <summary>
        /// 未处理
        /// </summary>
        Undealt,
        /// <summary>
        /// 避让
        /// </summary>
        Avoid,
        /// <summary>
        /// 被连续
        /// </summary>
        BeContinued,
        /// <summary>
        /// 无需避让
        /// </summary>
        NoAvoid,
        /// <summary>
        /// 已进行避让
        /// </summary>
        Avoided,
    }
}
