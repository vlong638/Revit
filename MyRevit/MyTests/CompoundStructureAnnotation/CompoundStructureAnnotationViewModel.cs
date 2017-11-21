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
    /// CompoundStructureAnnotation ViewType
    /// </summary>
    public enum CSAViewType
    {
        Idle,
        Select,
        Generate,
        Close,
    }

    /// <summary>
    /// CompoundStructureAnnotation 文字的定位方案
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
    public static class CSALocationTypeEx
    {
        public static double GetLineWidth(this CSALocationType CSALocationType)
        {
            return CSALocationType == CSALocationType.OnEdge ? 400 : 3000;
        }
        public static XYZ GetTextLocation(this CSALocationType CSALocationType, double currentFontHeight, double verticalFix, XYZ verticalVector, XYZ start, XYZ end)
        {
            return CSALocationType == CSALocationType.OnEdge ? end + (currentFontHeight / 2 + verticalFix) * verticalVector : start + (currentFontHeight + verticalFix) * verticalVector;
        }
    }

    public class TextLocation
    {
        public TextLocation(XYZ start, XYZ end)
        {
            Start = start;
            End = end;
        }

        public XYZ Start { set; get; }
        public XYZ End { set; get; }
    }

    public enum TargetType
    {
        Wall,
        Floor,
        RoofBase,
    }
    public static class TargetTypeEx
    {
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
        /// 根据Y优先上面 X次优先左面的方案取附着线
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
            PmSoft.Optimization.DrawingProduction.Utils.GraphicsDisplayerManager.Display(@"E:\WorkingSpace\Outputs\Images\1028轮廓.png", faces);
            return topLine;
        }
    }

    /// <summary>
    /// 为CSAModel数据服务的处理类
    /// </summary>
    public static class CSAModelEx
    {
        /// <summary>
        /// 获取文字的样式方案
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<TextNoteType> GetTextNoteTypes(this CSAModel model)
        {
            var textNoteTypes = new FilteredElementCollector(VLConstraintsForCSA.Doc).OfClass(typeof(TextNoteType)).ToElements().Select(p => p as TextNoteType).ToList();
            return textNoteTypes;
        }

        /// <summary>
        /// 线 参数设置
        /// </summary>
        public static void UpdateLineParameters(this CSAModel model, FamilyInstance line, double lineHeight, double lineWidth, double space, int textCount)
        {
            line.GetParameters(TagProperty.线高度1.ToString()).First().Set(lineHeight);
            line.GetParameters(TagProperty.线宽度.ToString()).First().Set(lineWidth);
            line.GetParameters(TagProperty.间距.ToString()).First().Set(space);
            line.GetParameters(TagProperty.文字行数.ToString()).First().Set(textCount);
        }

        /// <summary>
        /// 获取 CompoundStructure 对象
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static CompoundStructure GetCompoundStructure(this CSAModel model, Element element)
        {
            CompoundStructure compoundStructure = null;
            if (element is Wall)
            {
                compoundStructure = (element as Wall).WallType.GetCompoundStructure();
                model.TargetType = TargetType.Wall;
            }
            else if (element is Floor)
            {
                compoundStructure = (element as Floor).FloorType.GetCompoundStructure();
                model.TargetType = TargetType.Floor;
            }
            else if (element is RoofBase)
            {
                compoundStructure = (element as RoofBase).RoofType.GetCompoundStructure();
                model.TargetType = TargetType.RoofBase;
            }
            else
            {
                throw new NotImplementedException("暂不支持该类型的对象,未能成功获取CompoundStructure");
            }
            return compoundStructure;
        }

        /// <summary>
        /// 从 CompoundStructure 中获取标注信息
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="compoundStructure"></param>
        /// <returns></returns>
        public static List<string> FetchTextsFromCompoundStructure(this CSAModel model, Document doc, CompoundStructure compoundStructure)
        {
            var layers = compoundStructure.GetLayers();
            var texts = new List<string>();
            foreach (var layer in layers)
            {
                string name = "";
                var material = doc.GetElement(layer.MaterialId);
                if (material == null)
                    name = "<按类别>";
                else
                    name = doc.GetElement(layer.MaterialId).Name;
                texts.Add(UnitHelper.ConvertFromFootTo(layer.Width, VLUnitType.millimeter) + "厚" + name);
            }
            model.Texts = texts;
            return texts;
        }

        /// <summary>
        /// 从需要标注的对象中获取标注的源位置
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static XYZ GetElementLocation(this CSAModel model, Element element)
        {
            var locationCurve = element.Location as LocationCurve;
            var location = (locationCurve.Curve.GetEndPoint(0) + locationCurve.Curve.GetEndPoint(1)) / 2;
            return location;
        }

        private static void SetCurveFromLine(CSAModel model, LocationCurve locationCurve)
        {
            XYZ parallelVector = null;
            XYZ verticalVector = null;
            parallelVector = (locationCurve.Curve as Line).Direction;
            verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
            parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
            verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
            double xyzTolarance = 0.01;
            if (Math.Abs(verticalVector.X) > 1 - xyzTolarance)
            {
                verticalVector = new XYZ(-verticalVector.X, -verticalVector.Y, verticalVector.Z);
            }
            model.VerticalVector = verticalVector;
            model.ParallelVector = parallelVector;
        }

        /// <summary>
        /// 按XY的点的Z轴旋转
        /// </summary>
        /// <param name="point"></param>
        /// <param name="xyz"></param>
        /// <param name="verticalVector"></param>
        public static void RotateByXY(this LocationPoint point, XYZ xyz, XYZ verticalVector)
        {
            Line axis = Line.CreateBound(xyz, xyz.Add(new XYZ(0, 0, 10)));
            if (verticalVector.X > VLConstraintsForCSA.MiniValueForXYZ)
                point.Rotate(axis, 2 * Math.PI - verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
            else
                point.Rotate(axis, verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
        }

        /// <summary>
        /// 按XY的点的Z轴旋转
        /// </summary>
        /// <param name="point"></param>
        /// <param name="xyz"></param>
        /// <param name="verticalVector"></param>
        public static void RotateByXY(this Location point, XYZ xyz, XYZ verticalVector)
        {
            Line axis = Line.CreateBound(xyz, xyz.Add(new XYZ(0, 0, 10)));
            if (verticalVector.X > VLConstraintsForCSA.MiniValueForXYZ)
                point.Rotate(axis, 2 * Math.PI - verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
            else
                point.Rotate(axis, verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
        }
    }


    //public class VLViewModelBase : DependencyObject, INotifyPropertyChanged
    //{
    //    #region INotifyPropertyChanged
    //    /// <summary>
    //    /// 实现INPC接口 监控属性改变
    //    /// </summary>
    //    public event PropertyChangedEventHandler PropertyChanged;

    //    public void RaisePropertyChanged(string propertyName)
    //    {
    //        if (PropertyChanged != null)
    //            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //    #endregion
    //}


}
