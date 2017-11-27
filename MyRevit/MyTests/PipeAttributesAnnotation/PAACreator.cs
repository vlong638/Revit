using Autodesk.Revit.DB;
using System.Collections.Generic;
using System;
using System.Linq;
using MyRevit.Utilities;

namespace MyRevit.MyTests.PAA
{

    public class ElementAndNodePoint
    {
        public ElementAndNodePoint(Element target, IndependentTag tag)
        {
            Target = target;
            Line = (target.Location as LocationCurve).Curve as Line;
            Tag = tag;
        }

        public ElementAndNodePoint(Element target)
        {
            Target = target;
            Line = (target.Location as LocationCurve).Curve as Line;
        }

        public Element Target { set; get; }
        public Line Line { set; get; }
        public IndependentTag Tag { set; get; }
        /// <summary>
        /// 节点位置
        /// </summary>
        public XYZ NodePoint { set; get; }
        /// <summary>
        /// 标注位置
        /// </summary>
        public XYZ AnnotationPoint { set; get; }
    }

    public class PAACreator
    {
        #region 多管节点计算
        /// <summary>
        /// 节点位置计算
        /// </summary>
        /// <param name="elementNodePairs"></param>
        /// <param name="rightOfLefts"></param>
        /// <param name="leftOfRights"></param>
        /// <param name="startPoint"></param>
        /// <returns></returns>
        static bool CalculateLocations(Document doc, PAAModel model, List<ElementAndNodePoint> elementNodePairs, XYZ offset)
        {
            //XYZ rightOfLefts;
            //XYZ leftOfRights;
            //XYZ startPoint = model.TargetLocation;
            //bool isRegenerate = startPoint != null;
            ////重叠区间
            //List<XYZ> lefts = new List<XYZ>();
            //List<XYZ> rights = new List<XYZ>();
            //if (!isRegenerate)
            //{
            //    bool usingX = GetLeftsAndRights(elementNodePairs, lefts, rights);
            //    rightOfLefts = usingX ? lefts.First(c => c.X == lefts.Max(p => p.X)) : lefts.First(c => c.Y == lefts.Max(p => p.Y));
            //    leftOfRights = usingX ? rights.First(c => c.X == rights.Min(p => p.X)) : rights.First(c => c.Y == rights.Min(p => p.Y));
            //    if ((usingX && rightOfLefts.X > leftOfRights.X) || (!usingX && rightOfLefts.Y > leftOfRights.Y))
            //        return false;
            //}
            //else
            //{
            //    rightOfLefts = leftOfRights = null;
            //}
            ////节点计算
            //XYZ firstNode;
            //if (!isRegenerate)
            //{
            //    firstNode = (rightOfLefts + leftOfRights) / 2;
            //}
            //else
            //{
            //    var locationCurve = elementNodePairs[0].Line;
            //    firstNode = locationCurve.Project(startPoint).XYZPoint;
            //}
            //elementNodePairs[0].NodePoint = firstNode;
            ////节点位置
            //for (int i = 1; i < elementNodePairs.Count(); i++)
            //    elementNodePairs[i].NodePoint = elementNodePairs[i].Line.Project(elementNodePairs[0].NodePoint).XYZPoint;
            ////排序
            //if (elementNodePairs.Count() > 1)
            //{
            //    if (Math.Abs(elementNodePairs[0].NodePoint.Y - elementNodePairs[1].NodePoint.Y) < 0.01)
            //        elementNodePairs = elementNodePairs.OrderBy(c => c.NodePoint.X).ToList();
            //    else
            //        elementNodePairs = elementNodePairs.OrderByDescending(c => c.NodePoint.Y).ToList();
            //}
            ////标注定位计算
            //model.TargetLocation = elementNodePairs.First().NodePoint;
            //bool overMoved = false;//位移是否超过的最低限制
            //double verticalSkew = 0;
            //if (isRegenerate)// && regenerateType != RegenerateType.RegenerateByPipe)
            //{
            //    //原始线高度+偏移数据
            //    var line = doc.GetElement(model.LineId);
            //    var orientLineHeight = isRegenerate ? line.GetParameters(TagProperty.线高度1.ToString()).First().AsDouble() : 0;
            //    verticalSkew = LocationHelper.GetLengthBySide(offset, model.VerticalVector);
            //    if (Math.Abs(model.VerticalVector.X) > 1 - UnitHelper.MiniValueForXYZ)
            //        verticalSkew = -verticalSkew;
            //    var nodesHeight = UnitHelper.ConvertToFoot((elementNodePairs.Count() - 1) * model.CurrentFontHeight, VLUnitType.millimeter);
            //    overMoved = orientLineHeight + verticalSkew < nodesHeight;
            //    var lineHeight = orientLineHeight + verticalSkew;
            //    if (overMoved)
            //    {
            //        lineHeight = nodesHeight;
            //        verticalSkew = nodesHeight - orientLineHeight;
            //    }
            //    model.LineHeight = lineHeight;
            //}
            //var scale = 1 / PAAContext.FontManagement.OrientFontSizeScale * model.CurrentFontSizeScale;
            //var width = model.TextType.GetLineWidth() * scale;
            ////标注位置
            //for (int i = 0; i < elementNodePairs.Count(); i++)
            //{
            //    var start = model.TargetLocation + (model.LineHeight + i * model.LineSpace) * model.VerticalVector;
            //    var end = start + model.LineWidth * model.ParallelVector;
            //    elementNodePairs[i].AnnotationPoint = model.TextType.GetTextLocation(model.CurrentFontHeight, 0, model.VerticalVector, start, end);
            //}
            return true;
        }
        public static bool GetLeftsAndRights(List<ElementAndNodePoint> pipes, List<XYZ> lefts, List<XYZ> rights)
        {
            bool usingX = Math.Abs(pipes[0].Line.GetEndPoint(0).X - (pipes[0].Line.GetEndPoint(1).X)) > 0.01;
            var firstCurve = pipes.First().Line;
            firstCurve.MakeUnbound();
            for (int i = 1; i < pipes.Count(); i++)
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
        /// <summary>
        /// 多管直径标注的属性
        /// </summary>
        public enum TagProperty
        {
            线宽度,
            线下探长度,
            线高度1,
            线高度2,
            节点2距离,
            节点3距离,
            节点4距离,
            节点5距离,
            节点6距离,
            节点7距离,
            节点8距离,
            间距,//文本间距
            节点2可见性,
            节点3可见性,
            节点4可见性,
            节点5可见性,
            节点6可见性,
            节点7可见性,
            节点8可见性,
            文字行数,
        }
        /// <summary>
        /// 线 参数设置
        /// </summary>
        /// <param name="nodePoints"></param>
        /// <param name="line"></param>
        private void UpdateLineParameters(PAAModel model, List<ElementAndNodePoint> nodePoints, FamilyInstance line, XYZ verticalVector)
        {
            double deepLength = Math.Abs((nodePoints.Last().NodePoint - nodePoints.First().NodePoint).DotProduct(verticalVector));
            var scale = 1 / PAAContext.FontManagement.OrientFontSizeScale * model.CurrentFontSizeScale;
            var width = model.TextType.GetLineWidth() * scale;
            line.GetParameters(TagProperty.线高度1.ToString()).First().Set(model.LineHeight);
            line.GetParameters(TagProperty.线宽度.ToString()).First().Set(model.LineWidth);
            line.GetParameters(TagProperty.线下探长度.ToString()).First().Set(deepLength);
            line.GetParameters(TagProperty.间距.ToString()).First().Set(model.CurrentFontHeight);
            line.GetParameters(TagProperty.文字行数.ToString()).First().Set(nodePoints.Count());
            for (int i = 2; i <= 8; i++)
            {
                var first = nodePoints.First();
                if (nodePoints.Count() >= i)
                {
                    var cur = nodePoints[i - 1];
                    //line.GetParameters(string.Format("节点{0}可见性", i)).First().Set(1);
                    line.GetParameters(string.Format("节点{0}距离", i)).First().Set(Math.Abs((cur.NodePoint - first.NodePoint).DotProduct(verticalVector)));
                }
                else
                {
                    //line.GetParameters(string.Format("节点{0}可见性", i)).First().Set(0);
                    line.GetParameters(string.Format("节点{0}距离", i)).First().Set(0);
                }
            }
        }
        #endregion

        public bool Generate(PAAModel model, XYZ offset = null)
        {
            switch (model.ModelType)
            {
                case ModelType.Single:
                    return GenerateSingle(model, offset);
                case ModelType.Multiple:
                    return GenerateMultiple(model, offset);
                default:
                    return false;
            }
        }

        private bool GenerateMultiple(PAAModel model, XYZ offset)
        {
            Document doc = model.Document;
            View view = doc.ActiveView;
            var isRegenerate = offset != null;
            model.CalculateLocations(offset);
            XYZ parallelVector = model.ParallelVector;
            XYZ verticalVector = model.VerticalVector;
            var pipeAndNodePoints = model.PipeAndNodePoints;
            //线 创建
            FamilyInstance line = doc.Create.NewFamilyInstance(model.TargetLocation, model.GetLineFamily(doc), view);
            //线 旋转处理
            LocationPoint locationPoint = line.Location as LocationPoint;
            if (locationPoint != null)
                locationPoint.RotateByXY(model.TargetLocation, verticalVector.X.IsMiniValue() ? new XYZ(-verticalVector.X, -verticalVector.Y, verticalVector.Z) : verticalVector);
            model.LineId = line.Id;
            UpdateLineParameters(model, pipeAndNodePoints, line, verticalVector);
            //标注 创建
            model.AnnotationIds = new List<ElementId>();
            for (int i = 0; i < pipeAndNodePoints.Count(); i++)
            {
                var subTag = doc.Create.NewTag(view, pipeAndNodePoints[i].Target, false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, pipeAndNodePoints[i].AnnotationPoint);
                model.AnnotationIds.Add(subTag.Id);

                //var subTag = isRegenerate ? pipeAndNodePoints[i].Tag : doc.Create.NewTag(view, pipeAndNodePoints[i].Target, false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, pipeAndNodePoints[i].AnnotationPoint);
                //model.AnnotationIds.Add(subTag.Id);
            }
            return true;
        }

        private static bool GenerateSingle(PAAModel model, XYZ offset)
        {
            Document doc = model.Document;
            //主体
            var target = doc.GetElement(model.TargetId);
            var targetLocation = target.Location as LocationCurve;
            var p0 = targetLocation.Curve.GetEndPoint(0);
            var p1 = targetLocation.Curve.GetEndPoint(1);
            var pMiddle = new XYZ((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2, (p0.Z + p1.Z) / 2);
            model.TargetLocation = pMiddle;
            //线生成
            List<Line> lines = new List<Line>();
            model.CalculateLocations(offset);//计算内容定位
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
            subTag.ChangeTypeId(model.GetAnnotationFamily(doc).Id);
            return true;
        }

        //public void Regenerate(Document doc, PAAModelForSingle model, Element target)
        //{
        //    //FamilyInstance line = doc.GetElement(model.LineId) as FamilyInstance;
        //    //PAAContext.Creator.Regenerate(doc, model, target, (line.Location as LocationPoint).Point - model.LineLocation);
        //}

        public void RegenerateSingle(PAAModel model)
        {
            Clear(model);
            Generate(model);
        }
        public void Clear(PAAModel model)
        {
            Document doc = model.Document;
            switch (model.ModelType)
            {
                case ModelType.Single:
                    //删除线
                    foreach (var item in model.LineIds)
                        if (doc.GetElement(item) != null)
                            doc.Delete(item);
                    //删除标注
                    if (doc.GetElement(model.AnnotationId) != null)
                        doc.Delete(model.AnnotationId);
                    break;
                case ModelType.Multiple:
                    //清理线族
                    if (doc.GetElement(model.LineId) != null)
                        doc.Delete(model.LineId);
                    //清理标注
                    foreach (var item in model.AnnotationIds)
                        if (doc.GetElement(item) != null)
                            doc.Delete(item);
                    break;
                default:
                    break;
            }
        }
    }
}
