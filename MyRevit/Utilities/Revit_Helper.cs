using Autodesk.Revit.DB;

namespace MyRevit.Utilities
{
    public class Revit_Helper
    {
        public static OverrideGraphicSettings GetOverrideGraphicSettings(Color color, ElementId fillPatternId, int transparency)
        {
            OverrideGraphicSettings settings = new OverrideGraphicSettings();
            settings.SetCutFillColor(color);
            settings.SetCutFillPatternId(fillPatternId);
            settings.SetCutLineColor(color);
            settings.SetCutLinePatternId(fillPatternId);
            settings.SetProjectionFillColor(color);
            settings.SetProjectionFillPatternId(fillPatternId);
            settings.SetProjectionLineColor(color);
            settings.SetProjectionLinePatternId(fillPatternId);
            settings.SetSurfaceTransparency(transparency);
            return settings;
        }
    }
}
