using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.MyTests.Utilities;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyRevit.Entities
{
    public enum ContentType
    {
        Document,
        LinkDocument
    }
    public enum AlignType
    {
        BeamTopToFloorTop,
        BeamTopToFloorBottom,
    }
    public class BeamAlignToFloorModel
    {
        public ContentType ContentType { set; get; }
        public AlignType AlignType { set; get; }
    }

    [Transaction(TransactionMode.Manual)]
    public class CommandOfBeamAlignToFloor : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;
            BeamAlignToFloorModel model = new BeamAlignToFloorModel()
            {
                AlignType = AlignType.BeamTopToFloorTop,
                ContentType = ContentType.Document,
            };

            //MessageHelper.TaskDialogShow("开始选择板");
            IEnumerable<ElementId> floorIds = null;
            if (model.ContentType == ContentType.Document)
            {
                //基础板,OST_StructuralFoundation
                //结构楼板,OST_Floors
                floorIds = uiDoc.Selection.PickObjects(ObjectType.Element, new CategoryFilter(BuiltInCategory.OST_Floors)).Select(c => c.ElementId);
                if (floorIds == null || floorIds.Count() == 0)
                    return Result.Cancelled;
            }
            //MessageHelper.TaskDialogShow("开始选择梁");
            var beamIds = uiDoc.Selection.PickObjects(ObjectType.Element, new CategoryFilter(BuiltInCategory.OST_StructuralFraming)).Select(c => c.ElementId);
            if (beamIds == null || beamIds.Count() == 0)
                return Result.Cancelled;
            //业务逻辑处理
            TransactionHelper.DelegateTransaction(doc, "梁齐板", () =>
            {
                OutLineManager collector = new OutLineManager(doc, model);
                //添加板
                foreach (var floorId in floorIds)
                {
                    var floor = doc.GetElement(floorId) as Floor;
                    collector.Add(floor);
                }
                //计算梁的偏移处理
                foreach (var beamId in beamIds)
                {
                    var beam = doc.GetElement(beamId);
                    var lines = collector.Fit(beam);
                    collector.Adapt(beam, lines);
                }
                return true;
            });

            return Result.Succeeded;
        }
    }

    /// <summary>
    /// 轮廓类型
    /// </summary>
    enum OutLineType
    {
        /// <summary>
        /// 实体边框
        /// </summary>
        Solid,
        /// <summary>
        /// 洞口边框
        /// </summary>
        Opening,
    }

    /// <summary>
    /// 轮廓
    /// </summary>
    class OutLine
    {
        EdgeArray Edges;
        List<XYZ> Points;
        List<Triangle> Triangles;
        bool IsSolid { set; get; }
        List<OutLine> SubOutLines = new List<OutLine>();

        public OutLine(EdgeArray edgeArray)
        {
            Init(edgeArray);
        }

        public OutLine(EdgeArrayArray edgeLoops)
        {
            //闭合区间集合,EdgeArray
            foreach (EdgeArray edgeArray in edgeLoops)
            {
                if (Edges == null)
                {
                    Init(edgeArray);
                    continue;
                }
                Add(new OutLine(edgeArray));
            }
        }

        private void Init(EdgeArray edgeArray)
        {
            Edges = edgeArray;
            Points = GeometryHelper.GetPoints(Edges);
            Triangles = GeometryHelper.GetTriangles(Points);
            IsSolid = true;
        }

        void Add(OutLine newOne)
        {
            //当前节点下级的上级
            bool isUpLevel = false;
            for (int i = SubOutLines.Count() - 1; i >= 0; i--)
            {
                var outLine = SubOutLines[i];
                if (newOne.Contains(outLine))
                {
                    SubOutLines.Remove(outLine);
                    outLine.RevertAllOutLineType();
                    newOne.SubOutLines.Add(outLine);
                    isUpLevel = true;
                }
            }
            if (isUpLevel)
            {
                newOne.IsSolid = !IsSolid;
                SubOutLines.Add(newOne);
                return;
            }
            //当前节点下级的下级
            for (int i = SubOutLines.Count() - 1; i >= 0; i--)
            {
                var outLine = SubOutLines[i];
                if (outLine.Contains(newOne))
                {
                    outLine.Add(newOne);
                    return;
                }
            }
            //与其他下级无关联
            newOne.IsSolid = !IsSolid;
            SubOutLines.Add(newOne);
        }

        /// <summary>
        /// 检测轮廓是否被包含
        /// </summary>
        /// <param name="outLine"></param>
        /// <returns></returns>
        public bool Contains(OutLine outLine)
        {
            foreach (var point in outLine.Points)
            {
                var container = Triangles.AsParallel().FirstOrDefault(c => c.Contains(point));
                if (container == null)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 更改轮廓及其子轮廓的实体性质
        /// </summary>
        public void RevertAllOutLineType()
        {
            IsSolid = !IsSolid;
            foreach (var subOutLine in SubOutLines)
                subOutLine.RevertAllOutLineType();
        }
    }

    public class Triangle
    {
        public Triangle(XYZ a, XYZ b, XYZ c)
        {
            A = a;
            B = b;
            C = c;
        }

        public XYZ A { set; get; }
        public XYZ B { set; get; }
        public XYZ C { set; get; }


        /// <summary>
        /// 三角形包含点
        /// </summary>
        public bool Contains(XYZ point)
        {
            var vB = this.B - this.A;
            var vC = this.C - this.A;
            var vP = point - this.A;
            double dotB_B = vB.DotProduct(vB);
            double dotB_C = vB.DotProduct(vC);
            double dotB_P = vB.DotProduct(vP);
            double dotC_C = vC.DotProduct(vC);
            double dotC_P = vC.DotProduct(vP);
            double inverDeno = 1 / (dotB_B * dotC_C - dotB_C * dotB_C);
            double u = (dotC_C * dotB_P - dotB_C * dotC_P) * inverDeno;
            if (u < 0 || u > 1)
                return false;
            double v = (dotB_B * dotC_P - dotB_C * dotB_P) * inverDeno;
            if (v < 0 || v > 1)
                return false;
            return u + v <= 1;
        }
    }

    public class GeometryHelper
    {
        /// <summary>
        /// 获取边的所有点
        /// </summary>
        /// <param name="edgeArray"></param>
        /// <returns></returns>
        public static List<XYZ> GetPoints(EdgeArray edgeArray)
        {
            List<XYZ> points = new List<XYZ>();
            foreach (Edge edge in edgeArray)
                foreach (var point in edge.Tessellate())
                    if (points.FirstOrDefault(c => c.IsAlmostEqualTo(point, Constraints.XYZTolerance)) == null)
                        points.Add(point);
            return points;
        }

        /// <summary>
        /// 获取点集合对应的三角区间集合,暂不支持大于180度角的图形
        /// </summary>
        /// <param name="edgeArray"></param>
        /// <returns></returns>
        public static List<Triangle> GetTriangles(List<XYZ> points)
        {
            if (points.Count <= 2)
                throw new NotImplementedException("点数小于2无法构建最小的三点面");

            List<Triangle> triangles = new List<Triangle>();
            var start = points[0];
            for (int i = 1; i < points.Count - 1; i++)
            {
                triangles.Add(new Triangle(start, points[i], points[i + 1]));
            }
            return triangles;
        }

        /// <summary>
        /// 三角形包含点
        /// </summary>
        public static bool IsContained(Triangle triangle, XYZ point)
        {
            var vB = triangle.B - triangle.A;
            var vC = triangle.C - triangle.A;
            var vP = point - triangle.A;
            double dotB_B = vB.DotProduct(vB);
            double dotB_C = vB.DotProduct(vC);
            double dotB_P = vB.DotProduct(vP);
            double dotC_C = vC.DotProduct(vC);
            double dotC_P = vC.DotProduct(vP);
            double inverDeno = 1 / (dotB_B * dotC_C - dotB_C * dotB_C);
            double u = (dotC_C * dotB_P - dotB_C * dotC_P) * inverDeno;
            if (u < 0 || u > 1)
                return false;
            double v = (dotB_B * dotC_P - dotB_C * dotB_P) * inverDeno;
            if (v < 0 || v > 1)
                return false;
            return u + v <= 1;
        }

        /// <summary>
        /// 相交
        /// </summary>
        public static bool IsIntersect()
        {
            return false;
        }
    }

    class Constraints
    {
        public const double XYZTolerance = 0.001;
    }

    /// <summary>
    /// 轮廓集合
    /// </summary>
    class OutLineManager
    {
        List<OutLine> OutLines = new List<OutLine>();
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
            foreach (Solid geometryElement in geometryElements)
            {
                var faces = geometryElement.Faces;
                List<Line> subFaceLines = new List<Line>();
                foreach (Face face in faces)
                {
                    //矩形面
                    var planarFace = face as PlanarFace;
                    if (planarFace != null)
                    {
                        if (Model.AlignType == AlignType.BeamTopToFloorTop && planarFace.Normal.Z > 0)
                        {
                            OutLines.Add(new OutLine(planarFace.EdgeLoops));
                            return;
                        }
                        else if (Model.AlignType == AlignType.BeamTopToFloorBottom && planarFace.Normal.Z < 0)
                        {
                            OutLines.Add(new OutLine(planarFace.EdgeLoops));
                            return;
                        }
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
            }
            //TODO0719
            throw new NotImplementedException("添加的板中未按规则找到对应的表面");
        }

        /// <summary>
        /// 裁剪梁
        /// </summary>
        /// <param name="beam"></param>
        /// <returns></returns>
        public List<Line> Fit(Element beam)
        {
            //TODO0719
            throw new NotImplementedException();
        }

        /// <summary>
        /// 梁 适应到 裁剪集合
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="lines"></param>
        public void Adapt(Element beam, List<Line> lines)
        {
            //TODO0719
            throw new NotImplementedException();
        }
    }
}
