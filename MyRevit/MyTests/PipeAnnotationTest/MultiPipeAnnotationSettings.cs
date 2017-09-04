using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.MyTests.PipeAnnotationTest
{
    /// <summary>
    /// 多管直径标注 位置类型
    /// </summary>
    public enum MultiPipeTagLocation
    {
        OnLineEdge,
        OnLine,
    }
    /// <summary>
    /// 多管直径标注 参数
    /// </summary>
    public class MultiPipeAnnotationSettings
    {
        public double LengthBetweenPipe { set; get; }
        public MultiPipeTagLocation Location { set; get; }
    }
}
