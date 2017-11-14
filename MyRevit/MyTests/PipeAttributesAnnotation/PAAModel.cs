using MyRevit.MyTests.VLBase;
using System;

namespace MyRevit.MyTests.PipeAttributesAnnotation
{
    public class PAAModel : VLModel
    {
        public PAAModel(string data) : base(data)
        {
        }

        public override bool LoadData(string data)
        {
            return false;
        }

        public override string ToData()
        {
            return "";
        }
    }
}
