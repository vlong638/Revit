using Autodesk.Revit.DB;

namespace MyRevit.Utilities
{
    public class DocumentHelper
    {
        public static ViewSet GetAllViews(Document doc)
        {
            ViewSet views = new ViewSet();
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            FilteredElementIterator it = collector.OfClass(typeof(View)).GetElementIterator();
            it.Reset();
            while (it.MoveNext())
            {
                View view = it.Current as View3D;
                if (null != view && !view.IsTemplate && view.CanBePrinted)
                {
                    views.Insert(view);
                }
                else if (null == view)
                {
                    View view2D = it.Current as View;
                    if (view2D.ViewType == ViewType.FloorPlan | view2D.ViewType == ViewType.CeilingPlan | view2D.ViewType == ViewType.AreaPlan | view2D.ViewType == ViewType.Elevation | view2D.ViewType == ViewType.Section)
                    {
                        views.Insert(view2D);
                    }
                }
            }
            return views;
        }
        public static Element GetElementByName(Document doc,string name)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            FilteredElementIterator it = collector.OfClass(typeof(View)).GetElementIterator();
            it.Reset();
            while (it.MoveNext())
            {
                if (it.Current.Name == name)
                    return it.Current;
            }
            return null;
        }
        public static T GetElementByNameAs<T>(Document doc, string name) where T : class
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            FilteredElementIterator it = collector.OfClass(typeof(View)).GetElementIterator();
            it.Reset();
            while (it.MoveNext())
            {
                if (it.Current.Name == name)
                    return it.Current as T;
            }
            return null;
        }
    }
}
