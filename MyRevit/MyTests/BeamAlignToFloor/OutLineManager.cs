using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.MyTests.BeamAlignToFloor
{
    /// <summary>
    /// 轮廓处理类
    /// </summary>
    class OutLineManager
    {
        List<LeveledOutLines> LeveledOutLines = new List<LeveledOutLines>();
        Document Document { set; get; }
        BeamAlignToFloorModel Model { set; get; }

        public OutLineManager(Document document, BeamAlignToFloorModel model)
        {
            Document = document;
            Model = model;
        }

        /// <summary>
        /// 添加板
        /// </summary>
        /// <param name="floor"></param>
        public void Add(Floor floor)
        {
            var geometry = floor.get_Geometry(new Options() { View = Document.ActiveView });
            var geometryElements = geometry as GeometryElement;
            LeveledOutLines leveledOutLines = new LeveledOutLines();
            foreach (Solid geometryElement in geometryElements)
            {
                var faces = geometryElement.Faces;
                List<Face> addFaces = new List<Face>();
                foreach (Face face in faces)
                {
                    //矩形面
                    var planarFace = face as PlanarFace;
                    if (planarFace != null)
                    {
                        if (Model.AlignType == AlignType.BeamTopToFloorTop && planarFace.FaceNormal.Z > 0)
                            addFaces.Add(face);
                        else if (Model.AlignType == AlignType.BeamTopToFloorBottom && planarFace.FaceNormal.Z < 0)
                            addFaces.Add(face);
                    }
                    //圆面
                    var cylindricalFace = face as CylindricalFace;
                    if (cylindricalFace != null)
                    {
                        //最外的轮廓面必为矩形
                        //即如有其他原型作为最外轮廓面的...需重写逻辑,要判断最外轮廓面
                        //矩形面的最外轮廓可以通过XYZ(0,0,1)取得
                    }
                }
                foreach (var addFace in addFaces.OrderByDescending(c => c.Area))
                    leveledOutLines.Add(addFace);
            }
            if (!leveledOutLines.IsValid)
                throw new NotImplementedException("添加的板无效,无子轮廓");
            else
                LeveledOutLines.Add(leveledOutLines);
        }

        /// <summary>
        /// 裁剪梁
        /// </summary>
        /// <param name="beam"></param>
        /// <returns></returns>
        public List<LineSeperatePoints> Fit(Element beam)
        {
            var curve = (beam.Location as LocationCurve).Curve;
            var start = new XYZ(curve.GetEndPoint(0).X, curve.GetEndPoint(0).Y, 0);
            var end = new XYZ(curve.GetEndPoint(1).X, curve.GetEndPoint(1).Y, 0);
            var beamLine = Line.CreateBound(start, end);
            List<LineSeperatePoints> fitLinesCollection = new List<LineSeperatePoints>();
            foreach (var LeveledOutLine in LeveledOutLines)
                if (LeveledOutLine.IsCover(beamLine))
                {
                    var fitLines = LeveledOutLine.GetFitLines(beamLine);
                    //if (fitLines.Points.Count > 0)
                    fitLines.Z = fitLines.Points.Max(c => c.Z);
                    fitLinesCollection.Add(fitLines);
                }
            return fitLinesCollection;
        }
        /// <summary>
        /// 多个裁剪集合的整合
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="lines"></param>
        public LineSeperatePoints Merge(List<LineSeperatePoints> pointsCollection)
        {
            LineSeperatePoints result = new LineSeperatePoints();
            if (pointsCollection.Count() == 0)
                return result;

            pointsCollection = pointsCollection.OrderByDescending(c => c.Z).ToList();
            double max = double.MaxValue;
            double min = double.MinValue;
            if (pointsCollection.FirstOrDefault().Points[0].Y == pointsCollection.FirstOrDefault().Points[1].Y)//X轴平行线,以X轴为准
            {
                foreach (var pointsLine in pointsCollection)
                {
                    foreach (var point in pointsLine.Points)
                        if (point.X < max && point.X > min)
                            result.Points.Add(point);

                    max = pointsLine.Points.Max(c => c.X);
                    min = pointsLine.Points.Min(c => c.X);
                }
            }
            else//其他的以Y轴为准
            {
                foreach (var pointsLine in pointsCollection)
                {
                    foreach (var point in pointsLine.Points)
                        if (point.Y < max && point.Y > min)
                            result.Points.Add(point);

                    max = pointsLine.Points.Max(c => c.Y);
                    min = pointsLine.Points.Min(c => c.Y);
                }
            }
            return result;
        }
        /// <summary>
        /// 梁 适应到 裁剪集合
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="lines"></param>
        public void Adapt(Element beam, LineSeperatePoints lineSeperatePoints)
        {
            //TODO0719
            throw new NotImplementedException();
        }
    }
}
