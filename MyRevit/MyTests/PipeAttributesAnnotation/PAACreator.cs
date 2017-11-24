using Autodesk.Revit.DB;
using System.Collections.Generic;
using System;
using System.Linq;
using MyRevit.Utilities;

namespace MyRevit.MyTests.PAA
{

    public class ElementAndNodePoint
    {
        private Element element;

        public ElementAndNodePoint(Line line, IndependentTag tag)
        {
            Line = line;
            Tag = tag;
        }

        public ElementAndNodePoint(Line line)
        {
            Line = line;
        }

        public Line Line { set; get; }
        public IndependentTag Tag { set; get; }
        public XYZ NodePoint { set; get; }
    }

    public class PAACreator
    {
        #region 多管节点计算
        static bool GetNodePoints(List<ElementAndNodePoint> elementNodePairs, out XYZ rightOfLefts, out XYZ leftOfRights, XYZ startPoint = null)
        {
            bool isRegenerate = startPoint != null;
            //重叠区间
            List<XYZ> lefts = new List<XYZ>();
            List<XYZ> rights = new List<XYZ>();
            if (!isRegenerate)
            {
                bool usingX = GetLeftsAndRights(elementNodePairs, lefts, rights);
                rightOfLefts = usingX ? lefts.First(c => c.X == lefts.Max(p => p.X)) : lefts.First(c => c.Y == lefts.Max(p => p.Y));
                leftOfRights = usingX ? rights.First(c => c.X == rights.Min(p => p.X)) : rights.First(c => c.Y == rights.Min(p => p.Y));
                if ((usingX && rightOfLefts.X > leftOfRights.X) || (!usingX && rightOfLefts.Y > leftOfRights.Y))
                    return false;
            }
            else
            {
                rightOfLefts = leftOfRights = null;
            }
            //节点计算
            XYZ firstNode;
            if (!isRegenerate)
            {
                firstNode = (rightOfLefts + leftOfRights) / 2;
            }
            else
            {
                var locationCurve = elementNodePairs[0].Line;
                firstNode = locationCurve.Project(startPoint).XYZPoint;
            }
            elementNodePairs[0].NodePoint = firstNode;
            for (int i = 1; i < elementNodePairs.Count(); i++)
            {
                elementNodePairs[i].NodePoint = elementNodePairs[i].Line.Project(elementNodePairs[0].NodePoint).XYZPoint;
            }
            return true;
        }
        public static bool GetLeftsAndRights(List<ElementAndNodePoint> pipes, List<XYZ> lefts, List<XYZ> rights)
        {
            bool usingX = Math.Abs(pipes[0].Line.GetEndPoint(0).X - (pipes[0].Line.GetEndPoint(1).X)) > 0.01;
            var firstCurve = pipes.First().Line;
            firstCurve.MakeUnbound();
            for (int i = 0; i < pipes.Count(); i++)
            {
                var line = pipes[i].Line;
                double p1Location, p2Location;
                if (usingX)
                {
                    p1Location = line.GetEndPoint(0).X;
                    p2Location = line.GetEndPoint(1).X;
                }
                else
                {
                    p1Location = line.GetEndPoint(0).Y;
                    p2Location = line.GetEndPoint(1).Y;
                }
                if (i == 0)
                {
                    if (p1Location < p2Location)
                    {
                        lefts.Add(line.GetEndPoint(0));
                        rights.Add(line.GetEndPoint(1));
                    }
                    else
                    {
                        lefts.Add(line.GetEndPoint(1));
                        rights.Add(line.GetEndPoint(0));
                    }
                }
                else
                {
                    if (p1Location < p2Location)
                    {
                        lefts.Add(firstCurve.Project(line.GetEndPoint(0)).XYZPoint);
                        rights.Add(firstCurve.Project(line.GetEndPoint(1)).XYZPoint);
                    }
                    else
                    {
                        lefts.Add(firstCurve.Project(line.GetEndPoint(1)).XYZPoint);
                        rights.Add(firstCurve.Project(line.GetEndPoint(0)).XYZPoint);
                    }
                }
            }
            return usingX;
        }
        #endregion


        //public void Generate(Document doc, PAAModel model)
        //{
        //    Generate(doc, model, element, null);
        //}
        public bool Generate(Document doc, PAAModel model, XYZ offset=null)
        {
            switch (model.ModelType)
            {
                case ModelType.Single:
                    return GenerateSingle(doc, model, offset);
                case ModelType.Multiple:
                    return  GenerateMultiple(doc, model, offset);
                default:
                    return false;
            }
        }

        private bool GenerateMultiple(Document doc, PAAModel model, XYZ offset)
        {
            View view = doc.ActiveView;
            var isRegenerate = offset != null;
            List<ElementAndNodePoint> pipeAndNodePoints = new List<ElementAndNodePoint>();
            if (isRegenerate)
            {
                for (int i = 0; i < model.TargetIds.Count; i++)
                {
                    var target = doc.GetElement(model.TargetIds[i]);
                    var tagId = model.AnnotationIds[i];
                    pipeAndNodePoints.Add(new ElementAndNodePoint((target.Location as LocationCurve).Curve as Line, doc.GetElement(tagId) as IndependentTag));
                }
            }
            else
            {
                foreach (var selectedId in model.TargetIds)
                    pipeAndNodePoints.Add(new ElementAndNodePoint((doc.GetElement(selectedId).Location as LocationCurve).Curve as Line));
            }
            //平行,垂直 向量
            XYZ parallelVector = null;
            XYZ verticalVector = null;
            parallelVector = pipeAndNodePoints.First().Line.Direction;
            verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
            parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
            verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
            //节点计算
            XYZ rightOfLefts;//左侧点的右边界
            XYZ leftOfRights;//右侧点的左边界
            if (!GetNodePoints(pipeAndNodePoints, out rightOfLefts, out leftOfRights, model.TargetLocation) && !isRegenerate)
            {
                return false;
            }
            if (pipeAndNodePoints.Count() > 1)
            {
                if (Math.Abs(pipeAndNodePoints[0].NodePoint.Y - pipeAndNodePoints[1].NodePoint.Y) < 0.01)
                {
                    pipeAndNodePoints = pipeAndNodePoints.OrderBy(c => c.NodePoint.X).ToList();
                }
                else
                {
                    pipeAndNodePoints = pipeAndNodePoints.OrderByDescending(c => c.NodePoint.Y).ToList();
                }
            }
            var orderedNodePoints = pipeAndNodePoints.Select(c => c.NodePoint).ToList();
            XYZ startPoint = orderedNodePoints.First();
            FamilyInstance line = null;
            if (isRegenerate)
                (line.Location as LocationPoint).Point = startPoint;
            else
            {
                line = doc.Create.NewFamilyInstance(startPoint, model.LineFamily, view);
                //线 旋转处理
                LocationPoint locationPoint = line.Location as LocationPoint;
                if (locationPoint != null)
                    locationPoint.RotateByXY(startPoint, verticalVector.X .IsMiniValue()? new XYZ(-verticalVector.X, -verticalVector.Y, verticalVector.Z) : verticalVector);
            }
            return true;
        }

        private static bool GenerateSingle(Document doc, PAAModel model, XYZ offset)
        {
            //主体
            var target = doc.GetElement(model.TargetId);
            var targetLocation = target.Location as LocationCurve;
            var p0 = targetLocation.Curve.GetEndPoint(0);
            var p1 = targetLocation.Curve.GetEndPoint(1);
            var pMiddle = new XYZ((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2, (p0.Z + p1.Z) / 2);
            model.TargetLocation = pMiddle;
            //线生成
            List<Line> lines = new List<Line>();
            model.CalculateLocations(doc.GetElement(model.TargetId), offset);//计算内容定位
            lines.Add(Line.CreateBound(model.BodyStartPoint, model.BodyEndPoint));//竖干线
            lines.Add(Line.CreateBound(model.BodyEndPoint, model.LeafEndPoint));//斜支线
            model.LineIds = new List<ElementId>();
            foreach (var line in lines)
            {
                var lineElement = doc.Create.NewDetailCurve(doc.ActiveView, line);
                model.LineIds.Add(lineElement.Id);
            }
            //文本生成
            IndependentTag subTag = doc.Create.NewTag(doc.ActiveView, doc.GetElement(model.TargetId), false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, model.AnnotationLocation);
            model.AnnotationId = subTag.Id;
            subTag.TagHeadPosition = model.AnnotationLocation;
            subTag.ChangeTypeId(model.AnnotationFamily.Id);
            return true;
        }

        //public void Regenerate(Document doc, PAAModelForSingle model, Element target)
        //{
        //    //FamilyInstance line = doc.GetElement(model.LineId) as FamilyInstance;
        //    //PAAContext.Creator.Regenerate(doc, model, target, (line.Location as LocationPoint).Point - model.LineLocation);
        //}

        public void RegenerateSingle(Document doc, PAAModel model)
        {
            Clear(doc, model);
            Generate(doc, model);
        }
        public void Clear(Document doc, PAAModel model)
        {
            //删除线
            foreach (var item in model.LineIds)
                if (doc.GetElement(item) != null)
                    doc.Delete(item);
            //删除标注
            if (doc.GetElement(model.AnnotationId) != null)
                doc.Delete(model.AnnotationId);
        }
    }
}
