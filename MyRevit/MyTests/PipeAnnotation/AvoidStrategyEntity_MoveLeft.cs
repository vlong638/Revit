using Autodesk.Revit.DB;
using MyRevit.MyTests.BeamAlignToFloor;
using MyRevit.Utilities;

namespace MyRevit.MyTests.PipeAnnotation
{
    public class AvoidStrategyEntity_MoveLeft : AvoidStrategyEntity
    {
        public AvoidStrategyEntity_MoveLeft(AvoidStrategy avoidStrategy, AvoidStrategy nextStrategy) : base(avoidStrategy, nextStrategy)
        {
        }

        double OffsetLength = 1;

        public override bool TryAvoid()
        {
            if (UnitHelper.ConvertToInch(Data.LeftSpace, VLUnitType.millimeter) < OffsetLength)
                return false;

            Data.TemporaryTriangle = new Triangle(Data.CurrentTriangle, GetSideVector());
            Data.TemporaryLines = Data.TemporaryTriangle.GetLines();
            return !CheckCollision(Data, true);
        }

        XYZ GetSideVector()
        {
            return -Data.ParallelVector * OffsetLength;
        }

        public override void Apply(AvoidData data)
        {
            Autodesk.Revit.DB.ElementTransformUtils.MoveElement(data.Document, new ElementId(data.Entity.LineId), GetSideVector());
            foreach (var tagId in data.Entity.TagIds)
            {
                Autodesk.Revit.DB.ElementTransformUtils.MoveElement(data.Document, new ElementId(tagId), GetSideVector());
            }
        }
    }
}
