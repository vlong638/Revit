using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.MyTests.CompoundStructureAnnotation;
using MyRevit.MyTests.PipeAnnotationTest;
using MyRevit.MyTests.Utilities;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    /// <summary>
    /// ViewType 当前ViewModel的工作状态
    /// </summary>
    public enum CSAViewType
    {
        Closing = -1,//右上角或Alt+F4关闭
        Close = 0,//按钮关闭或ESC关闭
        Idle = 1,//闲置
        Select,
        Generate,
    }

    /// <summary>
    /// 文字的定位方案
    /// </summary>
    public enum CSALocationType
    {
        /// <summary>
        /// 在线端
        /// </summary>
        OnEdge,
        /// <summary>
        /// 在线上
        /// </summary>
        OnLine,
    }

    static class CSALocationTypeEx
    {
        public static double GetLineWidth(this CSALocationType CSALocationType)
        {
            return CSALocationType == CSALocationType.OnEdge ? 400 : 3000;
        }

        public static XYZ GetTextLocation(this CSALocationType CSALocationType, double currentFontHeight, double verticalFix, XYZ verticalVector, double horizontalFix, XYZ horizontalVector, XYZ start, XYZ end)
        {
            return CSALocationType == CSALocationType.OnEdge ? end + (currentFontHeight / 2 + verticalFix) * verticalVector : start + (currentFontHeight + verticalFix) * verticalVector + horizontalFix * horizontalVector;
        }
    }

    /// <summary>
    /// 附加信息的目标类型
    /// </summary>
    public enum TargetType
    {
        Wall,
        Floor,
        RoofBase,
    }
    public static class TargetTypeEx
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Line GetLine(this TargetType targetType, Element element)
        {
            switch (targetType)
            {
                case TargetType.Wall:
                    return (element.Location as LocationCurve).Curve as Line;
                case TargetType.Floor:
                case TargetType.RoofBase:
                    var option = new Options() { View = VLConstraintsForCSA.Doc.ActiveView };
                    var geometry = element.get_Geometry(option);
                    var geometryElements = geometry as GeometryElement;
                    var solid = geometryElements.FirstOrDefault() as Solid;
                    if (solid == null)
                        throw new NotImplementedException("实体与预期不一致");
                    List<Face> faces = new List<Face>();
                    foreach (Face face in solid.Faces)
                    {
                        //矩形面
                        var planarFace = face as PlanarFace;
                        if (planarFace != null)
                        {
                            if (planarFace.FaceNormal.Z > 0)
                                faces.Add(face);
                        }
                    }
                    //选出
                    Line topLine = null;
                    topLine = GetTopLine(faces);
                    var z = VLConstraintsForCSA.Doc.ActiveView.SketchPlane.GetPlane().Origin.Z;
                    XYZ p1 = topLine.GetEndPoint(0);
                    XYZ p2 = topLine.GetEndPoint(1);
                    return Line.CreateBound(new XYZ(p1.X, p1.Y, z), new XYZ(p2.X, p2.Y, z));
                default:
                    throw new NotImplementedException("该类型暂不支持");
            }
        }

        /// <summary>
        /// 获取位置最上的点
        /// </summary>
        /// <param name="faces"></param>
        /// <returns></returns>
        private static Line GetTopLine(List<Face> faces)
        {
            Line topLine = null;
            double weightY = double.MinValue;//Revit中 从上到下 Y递减 上大
            double weightX = double.MaxValue;//Revit中 从左到右 X递增 右大
            foreach (var face in faces)
            {
                foreach (EdgeArray edgeLoop in face.EdgeLoops)
                {
                    foreach (Edge edge in edgeLoop)
                    {
                        var points = edge.Tessellate();
                        for (int i = 0; i <= points.Count - 2; i++)
                        {
                            var currentWeightY = points[i].Y + points[i + 1].Y;
                            var currentWeightX = points[i].X + points[i + 1].X;
                            if (currentWeightY > weightY || (currentWeightY == weightY && currentWeightX < weightX))
                            {
                                topLine = Line.CreateBound(points[i], points[i + 1]);
                                weightY = currentWeightY;
                                weightX = currentWeightX;
                            }
                        }
                    }
                }
            }
            //TEST 显示分析
            //PmSoft.Optimization.DrawingProduction.Utils.GraphicsDisplayerManager.Display(@"E:\WorkingSpace\Outputs\Images\1028轮廓.png", faces);
            return topLine;
        }
    }
}
