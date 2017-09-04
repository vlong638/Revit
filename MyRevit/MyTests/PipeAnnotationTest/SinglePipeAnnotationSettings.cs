using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.MyTests.PipeAnnotationTest
{
    /// <summary>
    /// 单管直径标注 位置类型
    /// </summary>
    public enum SinglePipeTagLocation
    {
        OnPipe,
        AbovePipe,
    }
    /// <summary>
    /// 单管直径标注 参数
    /// </summary>
    public class SinglePipeAnnotationSettings
    {
        public bool NeedLeader { set; get; }
        public double LengthFromLine { set; get; }
        public SinglePipeTagLocation Location { set; get; }
        //public bool Cover { set; get; }
    }
}
