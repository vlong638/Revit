using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.MyTests.BeamAlignToFloor
{
    /// <summary>
    /// 分层轮廓集合,一面一层级的轮廓
    /// </summary>
    public class LevelOutLines
    {
        public bool IsValid { get { return OutLines.Count() > 0; } }
        public List<OutLine> OutLines = new List<OutLine>();
        /// <summary>
        /// 添加面所有的轮廓
        /// </summary>
        /// <param name="face"></param>
        public void Add(Face face, BeamAlignToFloorModel model)
        {
            var current = this;
            //闭合区间集合,EdgeArray
            foreach (EdgeArray edgeArray in face.EdgeLoops)
            {
                Add(new OutLine(edgeArray, model));
            }

            //if (face is PlanarFace)
            //{
            //    var current = this;
            //    //闭合区间集合,EdgeArray
            //    foreach (EdgeArray edgeArray in face.EdgeLoops)
            //    {
            //        Add(new OutLine(edgeArray, model));
            //    }
            //}
            //else if (face is CylindricalFace)
            //{
            //    var current = this;
            //    //闭合区间集合,EdgeArray TODO 改回去 无效调整
            //    EdgeArray edgeArrayColleciton = new EdgeArray();
            //    foreach (EdgeArray edgeArray in face.EdgeLoops)
            //    {
            //        for (int i = 0; i < edgeArray.Size; i++)
            //        {
            //            edgeArrayColleciton.Append(edgeArray.get_Item(i));
            //        }
            //    }
            //    Add(new OutLine(edgeArrayColleciton, model));
            //}
        }

        void Add(OutLine newOne)
        {
            //子节点的下级
            foreach (var OutLine in OutLines)
            {
                if (OutLine.Contains(newOne))
                {
                    OutLine.Add(newOne);
                    return;
                }
            }
            //子节点的上级
            bool isTopLevel = false;
            for (int i = OutLines.Count() - 1; i >= 0; i--)
            {
                var SubOutLine = OutLines[i];
                if (newOne.Contains(SubOutLine))
                {
                    OutLines.Remove(SubOutLine);
                    SubOutLine.RevertAllOutLineType();
                    newOne.SubOutLines.Add(SubOutLine);
                    isTopLevel = true;
                }
            }
            if (isTopLevel)
            {
                newOne.IsSolid = true;
                OutLines.Add(newOne);
                return;
            }
            //无相关的新节点
            OutLines.Add(newOne);
        }

        public bool IsCover(Line line)
        {
            foreach (var SubOutLine in OutLines)
            {
                if (SubOutLine.IsCover(line) != CoverType.Disjoint)
                    return true;
            }
            return false;
        }

        public Triangle GetContainer(XYZ pointZ0)
        {
            foreach (var subOutLine in OutLines)
            {
                var triangle = subOutLine.GetContainer(pointZ0);
                if (triangle != null)
                {
                    return triangle;
                }
            }
            return null;
        }

        ///// <summary>
        ///// 检测轮廓是否相交或包含 有限线段
        ///// </summary>
        ///// <param name="outLine"></param>
        ///// <returns></returns>
        //public bool IsCover(XYZ pointZ0)
        //{
        //    foreach (var subOutLine in OutLines)
        //    {
        //        if (subOutLine.GetContainer(pointZ0) != null)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        /// <summary>
        /// 获得拆分点
        /// </summary>
        /// <returns></returns>
        public SeperatePoints GetFitLines(Line beamLineZ0)
        {
            SeperatePoints fitLines = new SeperatePoints();
            var p0 = beamLineZ0.GetEndPoint(0);
            var p1 = beamLineZ0.GetEndPoint(1);
            foreach (var SubOutLine in OutLines)
            {
                var coverType = SubOutLine.IsCover(beamLineZ0);
                if (coverType != CoverType.Disjoint)
                    fitLines.AdvancedPoints.AddRange(SubOutLine.GetFitLines(beamLineZ0).AdvancedPoints);
                //线的端点增加 注意检测是否已存在
                var point = p0;
                if (fitLines.AdvancedPoints.FirstOrDefault(c=>c.Point.VL_XYEqualTo(point))==null)
                {
                    var triangle = SubOutLine.GetContainer(point);
                    if (triangle != null)
                    {
                        var directOutLine = SubOutLine.GetContainedOutLine(point);
                        fitLines.AdvancedPoints.Add(new AdvancedPoint(GeometryHelper.GetIntersection(triangle, point, new XYZ(0, 0, 1)), beamLineZ0.Direction, directOutLine.IsSolid));
                    }
                }
                point = p1;
                if (fitLines.AdvancedPoints.FirstOrDefault(c => c.Point.VL_XYEqualTo(point)) == null)
                {
                    var triangle = SubOutLine.GetContainer(point);
                    if (triangle != null)
                    {
                        var directOutLine = SubOutLine.GetContainedOutLine(point);
                        fitLines.AdvancedPoints.Add(new AdvancedPoint(GeometryHelper.GetIntersection(triangle, point, new XYZ(0, 0, 1)), beamLineZ0.Direction, directOutLine.IsSolid));
                    }
                }
            }
            return fitLines;
        }
    }
}
