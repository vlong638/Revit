using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.MyTests.BeamAlignToFloor
{
    class ValidFaces
    {
        Document Document { set; get; }
        BeamAlignToFloorModel Model { set; get; }

        public ValidFaces(Document document, BeamAlignToFloorModel model)
        {
            Document = document;
            Model = model;
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
        internal List<Line> Deal(Element beam, Line beamLineZ0, LevelFloor levelFloor)
        {
            List<Line> undealedZ0 = new List<Line>();
            LevelOutLines leveledOutLines = GetLeveledOutLines(levelFloor);
            return undealedZ0;
        }

        /// <summary>
        /// 轮廓嵌套分层
        /// </summary>
        /// <param name="levelFloor"></param>
        /// <returns></returns>
        public LevelOutLines GetLeveledOutLines(LevelFloor levelFloor)
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
    }
}
