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
        public List<LevelOutLines> LeveledOutLines = new List<LevelOutLines>();
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
            LevelOutLines leveledOutLines = new LevelOutLines();
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
        public List<SeperatePoints> Fit(Element beam)
        {
            var curve = (beam.Location as LocationCurve).Curve;
            var start = new XYZ(curve.GetEndPoint(0).X, curve.GetEndPoint(0).Y, 0);
            var end = new XYZ(curve.GetEndPoint(1).X, curve.GetEndPoint(1).Y, 0);
            var beamLineZ0 = Line.CreateBound(start, end);
            List<SeperatePoints> fitLinesCollection = new List<SeperatePoints>();
            foreach (var LeveledOutLine in LeveledOutLines)
                if (LeveledOutLine.IsCover(beamLineZ0))
                {
                    var fitLines = LeveledOutLine.GetFitLines(beamLineZ0);
                    fitLines.Z = fitLines.AdvancedPoints.Max(c => c.Point.Z);
                    fitLinesCollection.Add(fitLines);
                }
            return fitLinesCollection;
        }

        /// <summary>
        /// 多个裁剪集合的整合
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="lines"></param>
        public SeperatePoints Merge(List<SeperatePoints> seperatePointsCollection, AdvancedPoint beamPoint0, AdvancedPoint beamPoint1)
        {
            SeperatePoints dealedPoints = new SeperatePoints();
            if (seperatePointsCollection.Count() == 0)
                return dealedPoints;

            seperatePointsCollection = seperatePointsCollection.OrderByDescending(c => c.Z).ToList();
            var usingX = seperatePointsCollection.FirstOrDefault().AdvancedPoints[0].Point.Y == seperatePointsCollection.FirstOrDefault().AdvancedPoints[1].Point.Y;
            if (usingX)
            {
                foreach (var seperatePoints in seperatePointsCollection)
                {
                    foreach (var point in seperatePoints.AdvancedPoints.Where(c => c.Point.X > dealedPoints.Max).OrderBy(c => c.Point.X))
                        dealedPoints.Add(point, point.Point.X);
                    foreach (var point in seperatePoints.AdvancedPoints.Where(c => c.Point.X < dealedPoints.Min).OrderByDescending(c => c.Point.X))
                        dealedPoints.Add(point, point.Point.X);
                }
            }
            else
            {
                foreach (var seperatePoints in seperatePointsCollection)
                {
                    foreach (var point in seperatePoints.AdvancedPoints.Where(c => c.Point.Y > dealedPoints.Max).OrderBy(c => c.Point.Y))
                        dealedPoints.Add(point, point.Point.Y);
                    foreach (var point in seperatePoints.AdvancedPoints.Where(c => c.Point.Y < dealedPoints.Min).OrderByDescending(c => c.Point.Y))
                        dealedPoints.Add(point, point.Point.Y);
                }
            }
            var directionPoint = beamPoint0;
            if (dealedPoints.AdvancedPoints.FirstOrDefault(c => c.Point.XYEqualTo(directionPoint.Point)) == null)
                if (usingX)
                    dealedPoints.Add(directionPoint, directionPoint.Point.X);
                else
                    dealedPoints.Add(directionPoint, directionPoint.Point.Y);
            directionPoint = beamPoint1;
            if (dealedPoints.AdvancedPoints.FirstOrDefault(c => c.Point.XYEqualTo(directionPoint.Point)) == null)
                if (usingX)
                    dealedPoints.Add(directionPoint, directionPoint.Point.X);
                else
                    dealedPoints.Add(directionPoint, directionPoint.Point.Y);
            return dealedPoints;
        }

        /// <summary>
        /// 梁 适应到 裁剪集合
        /// </summary>
        public void Adapt(Document doc,Element beam, SeperatedLines lines)
        {
            var symbol = (beam as FamilyInstance).Symbol;
            var level = doc.GetElement(beam.LevelId) as Level;
            doc.Delete(beam.Id);
            foreach (var line in lines.Lines)
            {
                doc.Create.NewFamilyInstance(line, symbol, level, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                //m_ReviteDoc.Create.NewFamilyInstance(Curve, familySymbol, simLevelInfo.FloorLevel, StructuralType.Beam);
            }
        }
    }
}
