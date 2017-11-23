using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace MyRevit.MyTests.PAA
{
    public class PAACreator
    {
        public void Generate(Document doc, PAAModelForSingle model, Element element)
        {
            Generate(doc, model, element, null);
        }
        public void Generate(Document doc, PAAModelForSingle model, Element element, XYZ offset)
        {
            //主体
            model.TargetId = element.Id;
            var target = doc.GetElement(model.TargetId);
            var targetLocation = target.Location as LocationCurve;
            var p0 = targetLocation.Curve.GetEndPoint(0);
            var p1 = targetLocation.Curve.GetEndPoint(1);
            var pMiddle = new XYZ((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2, (p0.Z + p1.Z) / 2);
            model.TargetLocation = pMiddle;
            //线生成
            List<Line> lines = new List<Line>();
            model.CalculateLocations(element, offset);//计算内容定位
            lines.Add(Line.CreateBound(model.BodyStartPoint, model.BodyEndPoint));//竖干线
            lines.Add(Line.CreateBound(model.BodyEndPoint, model.LeafEndPoint));//斜支线
            model.LineIds = new List<ElementId>();
            foreach (var line in lines)
            {
                var lineElement = doc.Create.NewDetailCurve(doc.ActiveView,line);
                model.LineIds.Add(lineElement.Id);
            }
            //var group = doc.Create.NewGroup(model.LineIds);
            //group.GroupType.Name = model.TargetId.ToString();
            //model.GroupId = group.Id;
            //文本生成
            var tagSymbol = model.AnnotationType.GetAnnotationFamily(doc);//获取线标注类型 
            IndependentTag subTag = doc.Create.NewTag(doc.ActiveView, doc.GetElement(model.TargetId), false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, model.AnnotationLocation);
            model.AnnotationId = subTag.Id;
            subTag.TagHeadPosition = model.AnnotationLocation;
            subTag.ChangeTypeId(tagSymbol.Id);
        }
        //public void Regenerate(Document doc, PAAModelForSingle model, Element target)
        //{
        //    //FamilyInstance line = doc.GetElement(model.LineId) as FamilyInstance;
        //    //PAAContext.Creator.Regenerate(doc, model, target, (line.Location as LocationPoint).Point - model.LineLocation);
        //}
        public void Regenerate(Document doc, PAAModelForSingle model, Element target)
        {
            Clear(doc, model);
            Generate(doc, model, target);


            //TODO_PAA
            //model.BodyStartPoint =;
            //model.BodyEndPoint =;
            //model.LeafEndPoint=;
            //Clear(doc, model);
            //Generate(doc, model, target, offset);
        }
        public void Clear(Document doc, PAAModelForSingle model)
        {
            ////删除组
            //if (doc.GetElement(model.GroupId) != null)
            //    doc.Delete(model.GroupId);
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
