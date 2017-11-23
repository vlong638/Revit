using Autodesk.Revit.DB;
using MyRevit.MyTests.BeamAlignToFloor;
using MyRevit.MyTests.PipeAnnotationTest;
using MyRevit.MyTests.Utilities;
using PmSoft.Optimization.DrawingProduction;
using System.Collections.Generic;

namespace MyRevit.MyTests.PipeAnnotation
{
    public class AvoidData
    {
        #region 碰撞检测数据
        public Document Document;
        public IEnumerable<ElementId> SelectedPipeIds;
        public PipeAnnotationEntity Entity;
        public FamilySymbol MultipleTagSymbol;
        public List<Line> CurrentLines;
        public Triangle CurrentTriangle;
        public List<Line> TemporaryLines;
        public Triangle TemporaryTriangle;
        #endregion

        #region 避让数据
        public XYZ ParallelVector;
        public double LeftSpace;
        public double RightSpace;
        #endregion

        public AvoidData(
            //碰撞检测数据
            Document document, IEnumerable<ElementId> selectedPipeIds, PipeAnnotationEntity entity, List<Line> currentLines, Triangle currentTriangle, FamilySymbol multipleTagSymbol,
            //避让数据
            XYZ parallelVector, double leftSpace, double rightSpace
            )
        {
            Document = document;
            SelectedPipeIds = selectedPipeIds;
            Entity = entity;
            CurrentLines = currentLines;
            CurrentTriangle = currentTriangle;
            MultipleTagSymbol = multipleTagSymbol;
            ParallelVector = parallelVector;
            LeftSpace = leftSpace;
            RightSpace = rightSpace;
        }
    }
}
