using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace MyRevit.Utilities
{
    public class VLElementIdComparer : IEqualityComparer<ElementId>
    {
        public bool Equals(ElementId t1, ElementId t2)
        {
            return (t1.IntegerValue == t2.IntegerValue);
        }
        public int GetHashCode(ElementId t)
        {
            return t.GetHashCode();
        }
    }
}
