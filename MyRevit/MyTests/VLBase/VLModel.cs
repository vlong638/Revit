using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MyRevit.MyTests.VLBase
{
    public abstract class VLModel : VLSerializable
    {
        public VLModel(string data)
        {
            LoadData(data);
        }

        public abstract bool LoadData(string data);
        public abstract string ToData();
    }
}
