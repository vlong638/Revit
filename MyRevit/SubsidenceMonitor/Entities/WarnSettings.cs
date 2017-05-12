namespace MyRevit.SubsidenceMonitor.Entities
{
    public class WarnSettings
    {
        //建筑物沉降
        public int BuildingSubsidence_Day { set; get; } = int.MinValue;
        public int BuildingSubsidence_DailyMillimeter { set; get; } = int.MinValue;
        public int BuildingSubsidence_SumMillimeter { set; get; } = int.MinValue;
        //地表沉降
        public int SurfaceSubsidence_Day { set; get; } = int.MinValue;
        public int SurfaceSubsidence_DailyMillimeter { set; get; } = int.MinValue;
        public int SurfaceSubsidence_SumMillimeter { set; get; } = int.MinValue;
        //钢支撑轴力
        public int STBAP_MaxAxle { set; get; } = int.MinValue;
        public int STBAP_MinAxle { set; get; } = int.MinValue;
        //管线沉降(有压)
        public int PressedPipeLineSubsidence_Day { set; get; } = int.MinValue;
        public int PressedPipeLineSubsidence_PipelineMillimeter { set; get; } = int.MinValue;
        public int PressedPipeLineSubsidence_WellMillimeter { set; get; } = int.MinValue;
        public int PressedPipeLineSubsidence_SumMillimeter { set; get; } = int.MinValue;
        //管线沉降(无压)
        public int UnpressedPipeLineSubsidence_Day { set; get; } = int.MinValue;
        public int UnpressedPipeLineSubsidence_PipelineMillimeter { set; get; } = int.MinValue;
        public int UnpressedPipeLineSubsidence_WellMillimeter { set; get; } = int.MinValue;
        public int UnpressedPipeLineSubsidence_SumMillimeter { set; get; } = int.MinValue;
        //墙体水平位移(侧斜)
        public int SkewBack_WellMillimeter { set; get; } = int.MinValue;
        public int SkewBack_StandardMillimeter { set; get; } = int.MinValue;
        public int SkewBack_Speed { set; get; } = int.MinValue;
        public int SkewBack_Day { set; get; } = int.MinValue;

        //建筑物沉降
        public static string Tag_BuildingSubsidence_Day{set;get;}= "建筑物沉降_日报警值_连续天数";
        public static string Tag_BuildingSubsidence_DailyMillimeter{set;get;}= "建筑物沉降_日报警值_日变量";
        public static string Tag_BuildingSubsidence_SumMillimeter{set;get;}= "建筑物沉降_累计变量";
        //地表沉降
        public static string Tag_SurfaceSubsidence_Day{set;get;}= "地表沉降_日报警值_连续天数";
        public static string Tag_SurfaceSubsidence_DailyMillimeter{set;get;}= "地表沉降_日报警值_日变量";
        public static string Tag_SurfaceSubsidence_SumMillimeter{set;get;}= "地表沉降_累计变量";
        //钢支撑轴力
        public static string Tag_STBAP_MaxAxle{set;get;}= "钢支撑轴力_轴力上限";
        public static string Tag_STBAP_MinAxle{set;get;}= "钢支撑轴力_轴力下限";
        //管线沉降(有压)
        public static string Tag_StressedPipeLineSubsidence_Day{set;get;}= "管线沉降_有压_日报警值_连续天数";
        public static string Tag_StressedPipeLineSubsidence_PipelineMillimeter{set;get;}= "管线沉降_有压_日报警值_线变量";
        public static string Tag_StressedPipeLineSubsidence_WellMillimeter{set;get;}= "管线沉降_有压_日报警值_井变量";
        public static string Tag_StressedPipeLineSubsidence_SumMillimeter{set;get;}= "管线沉降_有压_累计变量";
        //管线沉降(无压)
        public static string Tag_UnstressedPipeLineSubsidence_Day{set;get;}= "管线沉降_无压_日报警值_连续天数";
        public static string Tag_UnstressedPipeLineSubsidence_PipelineMillimeter{set;get;}= "管线沉降_无压_日报警值_线变量";
        public static string Tag_UnstressedPipeLineSubsidence_WellMillimeter{set;get;}= "管线沉降_无压_日报警值_井变量";
        public static string Tag_UnstressedPipeLineSubsidence_SumMillimeter{set;get;}= "管线沉降_无压_累计变量";
        //墙体水平位移(侧斜)
        public static string Tag_SkewBack_WellMillimeter{set;get;}= "墙体水平位移_端头井累计值";
        public static string Tag_SkewBack_StandardMillimeter{set;get;}= "墙体水平位移_标准段累计值";
        public static string Tag_SkewBack_Speed{set;get;}= "墙体水平位移_变形速率";
        public static string Tag_SkewBack_Day{set;get;}= "墙体水平位移_连续天数";
    }
}
