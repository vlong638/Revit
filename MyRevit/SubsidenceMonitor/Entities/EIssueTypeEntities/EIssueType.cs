using System;

namespace MyRevit.SubsidenceMonitor.Entities
{
    public enum EIssueType
    {
        未指定 = 0,
        /// <summary>
        /// 建筑物沉降
        /// BuildingSubsidence
        /// </summary>
        建筑物沉降 = 1,
        /// <summary>
        /// 地表沉降
        /// SurfaceSubsidence
        /// </summary>
        地表沉降 = 2,
        /// <summary>
        /// 管线沉降_有压
        /// PressedPipeLineSubsidence
        /// </summary>
        管线沉降_有压 = 3,
        /// <summary>
        /// 管线沉降_无压
        /// UnpressedPipeLineSubsidence
        /// </summary>
        管线沉降_无压 = 4,
        /// <summary>
        /// 侧线监测
        /// SkewBack
        /// </summary>
        侧斜监测 = 5,
        /// <summary>
        /// 钢支撑轴力监测
        /// STBAP,SteelTubeBracingAxialPressure
        /// </summary>
        钢支撑轴力监测 = 6,

        ///// <summary>
        ///// 建筑物沉降
        ///// </summary>
        //BuildingSubsidence = 1,
        ///// <summary>
        ///// 地表沉降
        ///// </summary>
        //SurfaceSubsidence = 2,
        ///// <summary>
        ///// 管线沉降
        ///// </summary>
        //PipeLineSubsidence = 3,
        ///// <summary>
        ///// 侧线监测
        ///// </summary>
        //SkewBack = 4,
        ///// <summary>
        ///// 钢支撑轴力监测-SteelTubeBracingAxialPressure
        ///// </summary>
        //STBAP = 5,
    }
    public static class EIssueTypeEx
    {
        public static EIssueTypeEntity GetEntity(this EIssueType issueType)
        {
            switch (issueType)
            {
                case EIssueType.建筑物沉降:
                    return new BuildingSubsidence();
                case EIssueType.地表沉降:
                    return new SurfaceSubsidence();
                case EIssueType.钢支撑轴力监测:
                    return new STBAP();
                case EIssueType.管线沉降_无压:
                    return new UnpressedPipeLineSubsidence();
                case EIssueType.管线沉降_有压:
                    return new PressedPipeLineSubsidence();
                case EIssueType.侧斜监测:
                    return new SkewBack();
                default:
                    throw new NotImplementedException($"暂未实现类型{issueType.ToString()}的EIssueTypeEntity");
            }
        }
    }
}
