namespace MyRevit.MyTests.MepCurveAvoid
{
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
