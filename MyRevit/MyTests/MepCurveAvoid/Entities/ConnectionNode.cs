using Autodesk.Revit.DB;

namespace MyRevit.MyTests.MepCurveAvoid
{
    public class ConnectionNode
    {
        public ConnectionNode(MEPCurve mEPCurve1, MEPCurve mEPCurve2)
        {
            MEPCurve1 = mEPCurve1;
            MEPCurve2 = mEPCurve2;
        }

        public MEPCurve MEPCurve1 { set; get; }
        public MEPCurve MEPCurve2 { set; get; }
    }
}