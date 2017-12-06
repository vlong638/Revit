using System;
using Autodesk.Revit.DB;
using MyRevit.MyTests.PBPA;
using System.Collections.Generic;

namespace MyRevit.MyTests.PBPA
{
    public class PBPACreator
    {
        internal bool Generate(PBPAModel model)
        {
            Document doc = model.Document;
            View view = doc.GetElement(model.ViewId) as View;
            if (view == null)
                return false;

            //主体
            var target = doc.GetElement(model.TargetId);
            var targetLocation = target.Location as LocationCurve;
            var p0 = targetLocation.Curve.GetEndPoint(0);
            var p1 = targetLocation.Curve.GetEndPoint(1);
            var pMiddle = new XYZ((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2, (p0.Z + p1.Z) / 2);
            model.TargetLocation = pMiddle;
            //线生成
            List<Line> lines = new List<Line>();
            model.CalculateLocations();//计算内容定位
            lines.Add(Line.CreateBound(model.BodyStartPoint, model.BodyEndPoint));//竖干线
            lines.Add(Line.CreateBound(model.BodyEndPoint, model.LeafEndPoint));//斜支线
            model.LineIds = new List<ElementId>();
            foreach (var line in lines)
            {
                var lineElement = doc.Create.NewDetailCurve(view, line);
                model.LineIds.Add(lineElement.Id);
            }
            //文本生成
            IndependentTag subTag = doc.Create.NewTag(view, doc.GetElement(model.TargetId), false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, model.AnnotationLocation);
            model.AnnotationId = subTag.Id;
            subTag.TagHeadPosition = model.AnnotationLocation;
            subTag.ChangeTypeId(model.GetAnnotationFamily(doc, model.TargetId).Id);
            return true;
        }

        public void Generate(Document doc, PBPAModel model, Element element)
        {
            Generate(doc, model, element);
        }

        internal bool Regenerate(PBPAModel model)
        {
            model.Clear();
            return Generate(model);
        }
    }
}
