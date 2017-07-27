using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.MyTests.BeamAlignToFloor
{
    class LeveledOutLines
    {
        public bool IsValid { get { return SubOutLines.Count() > 0; } }
        List<OutLine> SubOutLines = new List<OutLine>();

        /// <summary>
        /// 添加面所有的轮廓
        /// </summary>
        /// <param name="face"></param>
        public void Add(Face face)
        {
            var current = this;
            //闭合区间集合,EdgeArray
            foreach (EdgeArray edgeArray in face.EdgeLoops)
            {
                Add(new OutLine(face,edgeArray));
            }
        }

        void Add(OutLine newOne)
        {
            //子节点的下级
            foreach (var OutLine in SubOutLines)
            {
                if (OutLine.Contains(newOne))
                {
                    OutLine.Add(newOne);
                    return;
                }
            }
            //子节点的上级
            bool isTopLevel = false;
            for (int i = SubOutLines.Count() - 1; i >= 0; i--)
            {
                var SubOutLine = SubOutLines[i];
                if (newOne.Contains(SubOutLine))
                {
                    SubOutLines.Remove(SubOutLine);
                    SubOutLine.RevertAllOutLineType();
                    newOne.SubOutLines.Add(SubOutLine);
                    isTopLevel = true;
                }
            }
            if (isTopLevel)
            {
                newOne.IsSolid = true;
                SubOutLines.Add(newOne);
                return;
            }
            //无相关的新节点
            SubOutLines.Add(newOne);
        }

        public bool IsCover(Line line)
        {
            foreach (var SubOutLine in SubOutLines)
            {
                if (SubOutLine.IsCover(line))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 获得拆分点
        /// </summary>
        /// <returns></returns>
        public LineSeperatePoints GetFitLines(Line line)
        {
            LineSeperatePoints fitLines = new LineSeperatePoints();
            foreach (var SubOutLine in SubOutLines)
                if (SubOutLine.IsCover(line))
                    fitLines.Points.AddRange(SubOutLine.GetFitLines(line).Points);
            return fitLines;
        }
    }
}
