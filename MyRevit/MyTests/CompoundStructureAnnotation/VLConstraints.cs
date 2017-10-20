using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Drawing;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    class VLConstraints
    {
        public static Document Doc { private set; get; }
        public static UIDocument UIDoc { private set; get; }

        public static void InitByDocument(UIDocument uidoc)
        {
            UIDoc = uidoc;
            Doc = UIDoc.Document;
        }

        //TODO
        public static Font CurrentFont { set; get; }
        public static double CurrentFontHeight { set; get; }
        public static double FontSizeScale { set; get; }
        public static double FontWidthScale { set; get; }
    }
}
