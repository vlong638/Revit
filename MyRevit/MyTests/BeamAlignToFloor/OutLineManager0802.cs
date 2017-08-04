using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.MyTests.BeamAlignToFloor
{
    /// <summary>
    /// 轮廓处理类
    /// </summary>
    class OutLineManager0802
    {
        Document Document { set; get; }
        BeamAlignToFloorModel Model { set; get; }
        public FamilySymbol BeamSymbol { set; get; }
        public List<Element> CreatedBeams { set; get; }

        public OutLineManager0802(Document document, BeamAlignToFloorModel model)
        {
            Document = document;
            Model = model;
            CreatedBeams = new List<Element>();
        }

        internal List<Line> DealAll(Element beam, List<Line> beamLineZ0s, List<LevelFloor> levelFloors)
        {
            List<Line> result = new List<Line>();
            foreach (var levelFloor in levelFloors.OrderByDescending(c => c.Elevation))
            {
                result.AddRange(Deal(beam, beamLineZ0s, levelFloor));
            }
            return result;
        }

        internal List<Line> Deal(Element beam, List<Line> beamLineZ0s, LevelFloor levelFloor)
        {
            List<Line> result = new List<Line>();
            foreach (var beamLineZ0 in beamLineZ0s)
            {
                result.AddRange(Deal(beam, beamLineZ0, levelFloor));
            }
            return result;
        }

        /// <summary>
        /// 处理逻辑:
        /// 首先获取板的分层嵌套轮廓
        /// 然后检测线是否与上诉轮廓集合有相交
        /// 如果相交则进行线的裁剪处理
        /// (
        ///   获取所有交点
        ///   检测是否包含线的端点
        ///   结合交点和端点进行梁的分段处理
        ///   返回镂空区间的梁
        /// )
        /// 没有相交则返回完整的线,交于其他板进行裁剪处理
        /// </summary>
        internal List<Line> Deal(Element beam , Line beamLineZ0, LevelFloor levelFloor)
        {
            var beamLine = (beam.Location as LocationCurve).Curve as Line;
            List<Line> undealedZ0 = new List<Line>();
            LevelOutLines leveledOutLines = GetLeveledOutLines(levelFloor);
            if (leveledOutLines.IsCover(beamLineZ0))
            {
                var seperatePoints = leveledOutLines.GetFitLines(beamLineZ0);
                var pBeamZ0 = beamLineZ0.GetEndPoint(0);
                var pBeamZ1 = beamLineZ0.GetEndPoint(1);
                if (seperatePoints.AdvancedPoints.FirstOrDefault(c => c.Point.VL_XYEqualTo(pBeamZ0)) == null)
                    seperatePoints.AdvancedPoints.Add(new AdvancedPoint(pBeamZ0, beamLine.Direction, false));
                if (seperatePoints.AdvancedPoints.FirstOrDefault(c => c.Point.VL_XYEqualTo(pBeamZ1)) == null)
                    seperatePoints.AdvancedPoints.Add(new AdvancedPoint(pBeamZ1, beamLine.Direction, false));
                seperatePoints.AdvancedPoints = seperatePoints.AdvancedPoints.OrderByDescending(c => c.Point.X).ThenBy(c => c.Point.Y).ToList();
                bool isSolid = seperatePoints.AdvancedPoints.First().IsSolid;//点的IsSolid可能是其他分层的记录,需更新为当前分层的最新值
                var beamSymbol = (beam as FamilyInstance).Symbol;
                var beamLevel = Document.GetElement(beam.LevelId) as Level;
                for (int i = 0; i < seperatePoints.AdvancedPoints.Count-1; i++)
                {
                    var sp0 = seperatePoints.AdvancedPoints[i].Point;
                    var sp1 = seperatePoints.AdvancedPoints[i+1].Point;
                    if (isSolid)
                    {
                        var triangle =leveledOutLines.GetContainer(sp0);
                        if (triangle==null)
                            throw new NotImplementedException("Container Not Found");
                        var fixedSP0 = GeometryHelper.GetIntersection(triangle, sp0, new XYZ(0, 0, 1));
                        triangle = leveledOutLines.GetContainer(sp1);
                        if (triangle == null)
                            throw new NotImplementedException("Container Not Found");
                        var fixedSP1 = GeometryHelper.GetIntersection(triangle, sp1, new XYZ(0, 0, 1));
                        var sectionBeam = Document.Create.NewFamilyInstance(Line.CreateBound(fixedSP0, fixedSP1), beamSymbol, beamLevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                        CreatedBeams.Add(sectionBeam);
                    }
                    else
                    {
                        var fixedSP0 = GeometryHelper.VL_GetIntersectionOnLine(sp0, pBeamZ0, beamLineZ0.Direction);
                        var fixedSP1 = GeometryHelper.VL_GetIntersectionOnLine(sp1, pBeamZ0, beamLineZ0.Direction);
                        undealedZ0.Add(Line.CreateBound(fixedSP0, fixedSP1));
                    }
                    isSolid = !isSolid;
                }
                //CARE23415
                GraphicsDisplayerManager.Display(seperatePoints);
            }
            else
            {
                undealedZ0.Add(beamLine);
            }
            return undealedZ0;
        }

        /// <summary>
        /// 轮廓嵌套分层
        /// </summary>
        /// <param name="levelFloor"></param>
        /// <returns></returns>
        public　LevelOutLines GetLeveledOutLines(LevelFloor levelFloor)
        {
            var leveledOutLines = new LevelOutLines();
            var geometry = levelFloor.Floor.get_Geometry(new Options() { View = Document.ActiveView });
            var geometryElements = geometry as GeometryElement;
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
            return leveledOutLines;
        }

        /// <summary>
        /// 衔接点相连且夹角大于90度开角朝上
        /// </summary>
        internal void LinkBeamWithAngleGT180(Element orientBeam)
        {
            var height = orientBeam.GetParameters("顶部高程").First().AsDouble() - orientBeam.GetParameters("底部高程").First().AsDouble();
            CreatedBeams = CreatedBeams.OrderBy(c => (c.Location as LocationCurve).Curve.GetEndPoint(0).X).ThenBy(c => (c.Location as LocationCurve).Curve.GetEndPoint(0).Y).ToList();
            for (int i = 0; i < CreatedBeams.Count-1; i++)
            {
                var currentBeam = CreatedBeams[i];
                var nextBeam = CreatedBeams[i+1];
                var pCurrent0 = (currentBeam.Location as LocationCurve).Curve.GetEndPoint(0);
                var pCurrent1 = (currentBeam.Location as LocationCurve).Curve.GetEndPoint(1);
                var pNext0 = (nextBeam.Location as LocationCurve).Curve.GetEndPoint(0);
                var pNext1 = (nextBeam.Location as LocationCurve).Curve.GetEndPoint(1);
                XYZ currentDirection, nextDirection;//总是由交点向外侧的方向
                double angle;
                XYZ pOuter, pCross;
                if (pCurrent0.VL_XYZEqualTo(pNext0))
                {
                    pOuter = pCurrent1;
                    pCross = pCurrent0;
                    currentDirection = pCurrent1 - pCurrent0;
                    nextDirection = pNext1 - pNext0;
                }
                else if(pCurrent0.VL_XYZEqualTo(pNext1))
                {
                    pOuter = pCurrent1;
                    pCross = pCurrent0;
                    currentDirection = pCurrent1 - pCurrent0;
                    nextDirection = pNext0 - pNext1;
                }
                else if (pCurrent1.VL_XYZEqualTo(pNext0))
                {
                    pOuter = pCurrent0;
                    pCross = pCurrent1;
                    currentDirection = pCurrent0 - pCurrent1;
                    nextDirection = pNext1 - pNext0;
                }
                else if (pCurrent1.VL_XYZEqualTo(pNext1))
                {
                    pOuter = pCurrent0;
                    pCross = pCurrent1;
                    currentDirection = pCurrent0 - pCurrent1;
                    nextDirection = pNext0 - pNext1;
                }
                else
                {
                    continue;
                }
                angle = currentDirection.AngleTo(nextDirection);
                if (angle <= Math.PI / 2)
                    continue;
                if (currentDirection.Z + nextDirection.Z > 0)//向量和求是否中线向上
                {
                    var locationCurve = (currentBeam.Location as LocationCurve);
                    locationCurve.Curve = Line.CreateBound(pOuter, pCross - currentDirection.Normalize() * (Math.PI - angle) * height);
                }
            }
            
        }
    }
}
