using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using MyRevit.MyTests.BeamAlignToFloor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.MyTests.PipeAnnotation
{
    /// <summary>
    /// 避让策略对应的行为集合
    /// </summary>
    public abstract class AvoidStrategyEntity
    {
        public AvoidStrategyEntity(AvoidStrategy avoidStrategy, AvoidStrategy nextStrategy)
        {
            this.CurrentStrategy = avoidStrategy;
            this.NextStrategy = nextStrategy;
        }

        AvoidStrategy CurrentStrategy { set; get; }
        AvoidStrategy NextStrategy { set; get; }
        public AvoidData Data { set; get; }

        public AvoidStrategyEntity GetNextStratetyEntity()
        {
            var strategy = AvoidStrategyFactory.GetAvoidStrategyEntity(NextStrategy);
            strategy.Data = this.Data;
            return strategy;
        }

        /// <summary>
        /// 避让测试
        /// return true when collided
        /// return false when passed
        /// </summary>
        /// <returns></returns>
        public bool CheckCollision(AvoidData data, bool tryAvoid = false)
        {
            Document document = data.Document;
            View view = document.ActiveView;
            IEnumerable<ElementId> selectedPipeIds = data.SelectedPipeIds;
            ElementId currentLineId = new ElementId(data.Entity.LineId);
            List<Line> currentLines = tryAvoid ? data.TemporaryLines : data.CurrentLines;
            Triangle currentTriangle = tryAvoid ? data.TemporaryTriangle : data.CurrentTriangle;
            FamilySymbol multipleTagSymbol = data.MultipleTagSymbol;
            //管道避让
            var otherPipeLines = new FilteredElementCollector(document).OfClass(typeof(Pipe))
                .Select(c => Line.CreateBound((c.Location as LocationCurve).Curve.GetEndPoint(0), (c.Location as LocationCurve).Curve.GetEndPoint(1))).ToList();
            var pipeCollisions = new FilteredElementCollector(document).OfClass(typeof(Pipe)).Excluding(selectedPipeIds.ToList())
                .Select(c => Line.CreateBound((c.Location as LocationCurve).Curve.GetEndPoint(0), (c.Location as LocationCurve).Curve.GetEndPoint(1))).ToList()
                .Where(c => GeometryHelper.IsPlanarCover(currentLines, currentTriangle, c) != GeometryHelper.VLCoverType.Disjoint).ToList();
            //标注避让
            var collector = new FilteredElementCollector(document).OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType().Excluding(new List<ElementId>() { currentLineId });
            var otherLines = collector.Where(c => (c as FamilyInstance).Symbol.Id == multipleTagSymbol.Id);
            var boundingBoxes = otherLines.Select(c => c.get_BoundingBox(view));
            List<BoundingBoxXYZ> crossedBoundingBox = new List<BoundingBoxXYZ>();
            List<BoundingBoxXYZ> uncrossedBoundingBox = new List<BoundingBoxXYZ>();
            foreach (var boundingBox in boundingBoxes.Where(c => c != null))
                if (GeometryHelper.VL_IsRectangleCrossed(currentTriangle.A, currentTriangle.C, boundingBox.Min, boundingBox.Max))
                    crossedBoundingBox.Add(boundingBox);
                else
                    uncrossedBoundingBox.Add(boundingBox);
            PmSoft.Optimization.DrawingProduction.Utils.GraphicsDisplayerManager.Display(@"E:\WorkingSpace\Outputs\Images\0822标注避让.png", tryAvoid ? data.TemporaryTriangle : data.CurrentTriangle, otherPipeLines, pipeCollisions, crossedBoundingBox, uncrossedBoundingBox);
            return pipeCollisions.Count() > 0 || crossedBoundingBox.Count() > 0;
        }

        /// <summary>
        /// 尝试避让
        /// true for 避让成功
        /// false for 避让后以后碰撞
        /// </summary>
        /// <returns></returns>
        public abstract bool TryAvoid();

        /// <summary>
        /// 应用避让
        /// </summary>
        public abstract void Apply(AvoidData data);
    }
}
