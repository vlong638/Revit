using Autodesk.Revit.DB;

namespace MyRevit.MyTests.PipeAnnotation
{
    public class AvoidStrategyEntity_None : AvoidStrategyEntity
    {
        public AvoidStrategyEntity_None(AvoidStrategy avoidStrategy, AvoidStrategy nextStrategy) : base(avoidStrategy, nextStrategy)
        {
        }

        int OffsetLength = 10;
        public override bool TryAvoid()
        {
            return true;
        }

        XYZ GetSideVector()
        {
            return new XYZ(0, 0, 0);
        }

        public override void Apply(AvoidData data)
        {
        }
    }
}
