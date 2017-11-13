using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    public abstract class VLModelBase<T> : VLSerialize
    {
        public VLModelBase(string data)
        {
            LoadData(data);
        }

        public abstract bool LoadData(string data);
        public abstract string ToData();
    }
}
