using MyRevit.MyTests.VLBase;
using System;

namespace MyRevit.MyTests.PipeAttributesAnnotation
{
    public class TemplateModel : VLModel
    {
        public TemplateModel(string data) : base(data)
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
