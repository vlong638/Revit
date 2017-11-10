using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{

    public abstract class VLModelBase<T> : VLSerialize
    {
        public static string PropertyInnerSplitter { get { return VLConstraints.PropertyInnerSplitter; } }
        public static char PropertyInnerSplitter_Char { get { return VLConstraints.PropertyInnerSplitter_Char; } }
        public static string PropertyInnerSplitter2 { get { return VLConstraints.PropertyInnerSplitter2; } }
        public static char PropertyInnerSplitter2_Char { get { return VLConstraints.PropertyInnerSplitter2_Char; } }
        public static string PropertySplitter { get { return VLConstraints.PropertySplitter; } }
        public static char PropertySplitter_Char { get { return VLConstraints.PropertySplitter_Char; } }

        public VLModelBase(string data)
        {
            LoadData(data);
        }

        public abstract bool LoadData(string data);
        public abstract string ToData();
    }
}
