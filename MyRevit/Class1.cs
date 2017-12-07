namespace Autodesk.Revit.DB
{
    public enum ViewType
    {
        Undefined = 0,//未定义
        FloorPlan = 1,//楼层平面
        CeilingPlan = 2,//天花板平面
        Elevation = 3,//建筑立面
        ThreeD = 4,//3D平面
        Schedule = 5,//明细表/数量
        DrawingSheet = 6,
        ProjectBrowser = 7,//项目浏览器
        Report = 8,//
        DraftingView = 10,//
        Legend = 11,//
        SystemBrowser = 12,//
        EngineeringPlan = 115,
        AreaPlan = 116,
        Section = 117,
        Detail = 118,
        CostReport = 119,
        LoadsReport = 120,
        PresureLossReport = 121,
        ColumnSchedule = 122,
        PanelSchedule = 123,
        Walkthrough = 124,
        Rendering = 125,
        Internal = 214
    }
}