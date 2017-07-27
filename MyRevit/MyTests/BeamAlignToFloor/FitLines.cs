using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace MyRevit.MyTests.BeamAlignToFloor
{
    /// <summary>
    /// 用以裁剪线段的信息
    /// </summary>
    public class LineSeperatePoints
    {
        /// <summary>
        /// 极限高程,用于重叠的多面的裁剪优先级
        /// </summary>
        public double Z;
        public List<XYZ> Points = new List<XYZ>();
    }
}
