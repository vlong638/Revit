using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB.Plumbing;
using MyRevit.MyTests.BeamAlignToFloor;
using MyRevit.MyTests.PipeAnnotation;
using MyRevit.MyTests.PipeAnnotationTest;
using MyRevit.Utilities;
using MyRevit.MyTests.Utilities;

namespace PmSoft.Optimization.DrawingProduction
{
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
    /// 多管标注生成返回码
    /// </summary>
    public enum AnnotationBuildResult
    {
        Success,
        Error,
        /// <summary>
        /// 管道并非平行
        /// </summary>
        NotParallel,
        /// <summary>
        /// 管道无重叠区间
        /// </summary>
        NoOverlap,
        /// <summary>
        /// 无该类型的标注生成方案
        /// </summary>
        NoLocationType,
    }

    /// <summary>
    /// 标注生成类
    /// </summary>
    public class AnnotationCreater
    {
        public AnnotationCreater()
        {
        }

        Document Document { set; get; }
        PipeAnnotationEntityCollection Collection { set; get; }

        /// <summary>
        /// 生成标注
        /// </summary>
        public AnnotationBuildResult GenerateMultipleTagSymbol(Document document, IEnumerable<ElementId> selectedIds, MultiPipeAnnotationSettings setting, bool generateSingleOne)
        {
            if (selectedIds.Count() > AnnotationConstaints.PipeCountMax)
                throw new NotImplementedException("暂不支持8根以上及管道的多管直径标注生成");
            Document = document;
            Collection = PAContext.GetCollection(Document);
            PipeAnnotationEntity entity = new PipeAnnotationEntity();
            entity.LocationType = setting.Location;
            View view = Document.ActiveView;
            AnnotationBuildResult result = GenerateMultipleTagSymbol(document, selectedIds, entity, setting);
            if (generateSingleOne && result == AnnotationBuildResult.Success)
            {
                Collection.Add(entity);
                Collection.Save(Document);
            }
            else if (result == AnnotationBuildResult.Success)
            {
                Collection.Add(entity);
            }
            return result;
        }

        public void FinishMultipleGenerate(Document document)
        {
            if (Collection == null)
                Collection = PAContext.GetCollection(Document);
            Collection.Save(Document);
        }

        class PipeAndNodePoint
        {
            public PipeAndNodePoint(Pipe pipe, IndependentTag tag)
            {
                Pipe = pipe;
                Tag = tag;
            }

            public PipeAndNodePoint(Pipe pipe)
            {
                Pipe = pipe;
            }

            public Pipe Pipe { set; get; }
            public IndependentTag Tag { set; get; }
            public XYZ NodePoint { set; get; }
        }

        /// <summary>
        /// 生成标注
        /// </summary>
        /// <param name="selectedIds"></param>
        /// <param name="entity"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        AnnotationBuildResult GenerateMultipleTagSymbol(Document document, IEnumerable<ElementId> selectedIds, PipeAnnotationEntity entity, MultiPipeAnnotationSettings setting)
        {
            View view = document.ActiveView;
            XYZ startPoint = null;
            FamilyInstance line = null;
            List<PipeAndNodePoint> pipeAndNodePoints = new List<PipeAndNodePoint>();
            var tags = new List<IndependentTag>();
            //管道 获取
            foreach (var selectedId in selectedIds)
                pipeAndNodePoints.Add(new PipeAndNodePoint(Document.GetElement(selectedId) as Pipe));
            //平行,垂直 向量
            XYZ parallelVector = null;
            XYZ verticalVector = null;
            parallelVector = ((pipeAndNodePoints.First().Pipe.Location as LocationCurve).Curve as Line).Direction;
            verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
            parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
            verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
            if (parallelVector.Y == 1)
                verticalVector = -verticalVector;
            //平行检测
            if (!CheckParallel(pipeAndNodePoints.Select(c => c.Pipe), verticalVector))
            {
                return AnnotationBuildResult.NotParallel;
            }
            XYZ rightOfLefts;//左侧点的右边界
            XYZ leftOfRights;//右侧点的左边界
            //节点计算
            if (!GetNodePoints(pipeAndNodePoints, out rightOfLefts, out leftOfRights))
            {
                return AnnotationBuildResult.NoOverlap;
            }
            pipeAndNodePoints = pipeAndNodePoints.OrderByDescending(c => c.NodePoint.Y).ToList();
            var pipes = pipeAndNodePoints.Select(c => c.Pipe).ToList();
            var orderedNodePoints = pipeAndNodePoints.Select(c => c.NodePoint).ToList();
            //起始点
            startPoint = orderedNodePoints.First();
            //线 创建
            var multipleTagSymbol = PAContext.GetMultipleTagSymbol(document);
            if (!multipleTagSymbol.IsActive)
                multipleTagSymbol.Activate();
            line = Document.Create.NewFamilyInstance(startPoint, multipleTagSymbol, view);
            //线 旋转处理
            LocationPoint locationPoint = line.Location as LocationPoint;
            if (locationPoint != null)
                locationPoint.RotateByXY(startPoint, verticalVector);
            //线 参数设置
            UpdateLineParameters(orderedNodePoints, line, verticalVector);
            //标注 创建
            var textSize = PAContext.TextSize;
            var widthScale = PAContext.WidthScale;
            //碰撞检测区域点
            XYZ avoidP1_Line1 = orderedNodePoints.Last();
            XYZ avoidP2_Line2 = null;
            XYZ avoidP3_Annotation1 = null;
            switch (entity.LocationType)
            {
                case MultiPipeTagLocation.OnLineEdge:
                    //添加对应的单管直径标注
                    line.GetParameters(TagProperty.线宽度.ToString()).First().Set(UnitHelper.ConvertToFoot(200, AnnotationConstaints.UnitType));
                    var height = Convert.ToDouble(line.GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) + (orderedNodePoints.Count() - 1) * AnnotationConstaints.TextHeight;
                    avoidP2_Line2 = avoidP1_Line1 + UnitHelper.ConvertToFoot(height, AnnotationConstaints.UnitType) * verticalVector;
                    var skewLength = AnnotationConstaints.SkewLengthForOffLine;
                    for (int i = 0; i < pipes.Count(); i++)
                    {
                        var subTag = Document.Create.NewTag(view, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, startPoint);
                        var text = subTag.TagText;
                        var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, AnnotationConstaints.Font).Width;
                        var actualLength = textLength / (textSize * widthScale);
                        subTag.TagHeadPosition = startPoint + skewLength * parallelVector + UnitHelper.ConvertToInch(actualLength, VLUnitType.millimeter) * parallelVector
                            + UnitHelper.ConvertToFoot(height - i * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType) * verticalVector;
                        tags.Add(subTag);
                        if (i == 0)
                            avoidP3_Annotation1 = subTag.TagHeadPosition + skewLength * parallelVector + UnitHelper.ConvertToInch(actualLength, VLUnitType.millimeter) * parallelVector 
                                + UnitHelper.ConvertToFoot(0.5 * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType) * verticalVector;
                    }
                    break;
                case MultiPipeTagLocation.OnLine:
                    //添加对应的单管直径标注
                    line.GetParameters(TagProperty.线宽度.ToString()).First().Set(UnitHelper.ConvertToFoot(800, AnnotationConstaints.UnitType));
                    height = Convert.ToDouble(line.GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) + (orderedNodePoints.Count() - 1) * AnnotationConstaints.TextHeight;
                    avoidP2_Line2 = avoidP1_Line1 + UnitHelper.ConvertToFoot(height, AnnotationConstaints.UnitType) * verticalVector;
                    skewLength = AnnotationConstaints.SkewLengthForOnLine;
                    for (int i = 0; i < pipes.Count(); i++)
                    {
                        var subTag = Document.Create.NewTag(view, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, startPoint);
                        var text = subTag.TagText;
                        var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, AnnotationConstaints.Font).Width;
                        var actualLength = textLength / (textSize * widthScale);
                        subTag.TagHeadPosition = startPoint + skewLength * parallelVector + UnitHelper.ConvertToInch(actualLength, VLUnitType.millimeter) * parallelVector
                            + UnitHelper.ConvertToFoot(height - i * AnnotationConstaints.TextHeight + 0.5 * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType) * verticalVector;
                        tags.Add(subTag);
                        if (i == 0)
                            avoidP3_Annotation1 = subTag.TagHeadPosition + skewLength * parallelVector + UnitHelper.ConvertToInch(actualLength, VLUnitType.millimeter) * parallelVector 
                                + UnitHelper.ConvertToFoot(AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType) * verticalVector;
                    }
                    break;
                default:
                    return AnnotationBuildResult.NoLocationType;
            }
            entity.ViewId = view.Id.IntegerValue;
            entity.LineId = line.Id.IntegerValue;
            foreach (var pipe in pipes)
                entity.PipeIds.Add(pipe.Id.IntegerValue);
            foreach (var tag in tags)
                entity.TagIds.Add(tag.Id.IntegerValue);
            entity.StartPoint = startPoint;
            //碰撞检测
            VLTriangle triangle = new VLTriangle(avoidP1_Line1, avoidP2_Line2, avoidP3_Annotation1);
            List<Line> lines = triangle.GetLines();
            AvoidData data = new AvoidData(document, selectedIds, entity, lines, triangle, multipleTagSymbol, parallelVector, rightOfLefts.GetLength(), leftOfRights.GetLength());
            AvoidStrategy strategty = AvoidStrategy.MoveLeft;
            var strategyEntity = AvoidStrategyFactory.GetAvoidStrategyEntity(strategty);
            strategyEntity.Data = data;
            while (strategyEntity.CheckCollision(strategyEntity.Data))
            {
                if (strategyEntity.TryAvoid())
                {
                    strategyEntity.Apply(data);
                    break;
                }
                else
                {
                    strategyEntity = strategyEntity.GetNextStratetyEntity();
                }
            }
            return AnnotationBuildResult.Success;

            #region old
            //if (CheckCollision(document, view, selectedIds, line, lines, triangle, multipleTagSymbol))
            //{
            //    int offsetLength = 10;
            //    XYZ offset = null;
            //    if (rightOfLefts.GetLength() > offsetLength)
            //        offset = parallelVector * offsetLength;
            //    else if (leftOfRights.GetLength() > offsetLength)
            //        offset = -parallelVector * offsetLength;
            //    else
            //        return AnnotationBuildResult.Success;

            //    avoidP1_Line1 += offset;
            //    avoidP2_Line2 += offset;
            //    avoidP3_Annotation1 += offset;
            //    lines = GetLines(avoidP1_Line1, avoidP2_Line2, avoidP3_Annotation1);
            //    triangle = new Triangle(avoidP1_Line1, avoidP2_Line2, avoidP3_Annotation1);
            //    if (!CheckCollision(document, view, selectedIds, line, lines, triangle, multipleTagSymbol))
            //    {
            //        //TODO 偏移处理
            //        Autodesk.Revit.DB.ElementTransformUtils.MoveElement(document, new ElementId(entity.LineId), offset);
            //        foreach (var tagId in entity.TagIds)
            //        {
            //            Autodesk.Revit.DB.ElementTransformUtils.MoveElement(document, new ElementId(tagId), offset);
            //        }
            //    }
            //}
            //return AnnotationBuildResult.Success; 
            #endregion
        }

        //private static bool CheckCollision(Document document, View view, IEnumerable<ElementId> selectedPipeIds, FamilyInstance currentLine, List<Line> currentLines, Triangle currentTriangle, FamilySymbol multipleTagSymbol)
        //{
        //    //管道避让
        //    var otherPipeLines = new FilteredElementCollector(document).OfClass(typeof(Pipe))
        //        .Select(c => Line.CreateBound((c.Location as LocationCurve).Curve.GetEndPoint(0), (c.Location as LocationCurve).Curve.GetEndPoint(1))).ToList();
        //    var pipeCollisions = new FilteredElementCollector(document).OfClass(typeof(Pipe)).Excluding(selectedPipeIds.ToList())
        //        .Select(c => Line.CreateBound((c.Location as LocationCurve).Curve.GetEndPoint(0), (c.Location as LocationCurve).Curve.GetEndPoint(1))).ToList()
        //        .Where(c => GeometryHelper.IsCover(currentLines, currentTriangle, c) != GeometryHelper.VLCoverType.Disjoint).ToList();
        //    //TODO 标注避让
        //    var collector = new FilteredElementCollector(document).OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType().Excluding(new List<ElementId>() { currentLine.Id });
        //    var otherLines = collector.Where(c => (c as FamilyInstance).Symbol.Id == multipleTagSymbol.Id);
        //    var boundingBoxes = otherLines.Select(c => c.get_BoundingBox(view));
        //    List<BoundingBoxXYZ> crossedBoundingBox = new List<BoundingBoxXYZ>();
        //    List<BoundingBoxXYZ> uncrossedBoundingBox = new List<BoundingBoxXYZ>();
        //    foreach (var boundingBox in boundingBoxes.Where(c => c != null))
        //        if (GeometryHelper.VL_IsRectangleCrossed(currentTriangle.A, currentTriangle.C, boundingBox.Min, boundingBox.Max))
        //            crossedBoundingBox.Add(boundingBox);
        //        else
        //            uncrossedBoundingBox.Add(boundingBox);
        //    Utils.GraphicsDisplayerManager.Display(@"E:\WorkingSpace\Outputs\Images\0822标注避让1.png", currentTriangle, otherPipeLines, pipeCollisions, crossedBoundingBox, uncrossedBoundingBox);
        //    return crossedBoundingBox.Count() > 0;
        //}

        //private static List<Triangle> GetTrianglesFromBoundingBox(BoundingBoxXYZ lineBoundingBox)
        //{
        //    return new List<Triangle>()
        //    {
        //        new Triangle(lineBoundingBox.Min,lineBoundingBox.Max,lineBoundingBox.Min+new XYZ(0,(lineBoundingBox.Max-lineBoundingBox.Min).Y,0)),
        //        new Triangle(lineBoundingBox.Min,lineBoundingBox.Max,lineBoundingBox.Min+new XYZ(0,(lineBoundingBox.Max-lineBoundingBox.Min).X,0)),
        //    };
        //}

        //private static List<Line> GetLinesFromBoundingBox(BoundingBoxXYZ lineBoundingBox)
        //{
        //    return new List<Line>()
        //    {
        //        Line.CreateBound(lineBoundingBox.Min,lineBoundingBox.Min+new XYZ(0,(lineBoundingBox.Max-lineBoundingBox.Min).Y,0)),
        //        Line.CreateBound(lineBoundingBox.Max,lineBoundingBox.Min+new XYZ(0,(lineBoundingBox.Max-lineBoundingBox.Min).Y,0)),
        //        Line.CreateBound(lineBoundingBox.Max,lineBoundingBox.Max-new XYZ(0,(lineBoundingBox.Max-lineBoundingBox.Min).Y,0)),
        //        Line.CreateBound(lineBoundingBox.Min,lineBoundingBox.Max-new XYZ(0,(lineBoundingBox.Max-lineBoundingBox.Min).Y,0)),
        //    };
        //}

        /// <summary>
        /// 根据线的移动,重定位内容
        /// </summary>
        public bool RegenerateMultipleTagSymbolByEntity(Document document, PipeAnnotationEntity entity, XYZ skewVector)
        {
            Document = document;
            XYZ startPoint = null;
            View view = Document.ActiveView;
            FamilyInstance line = document.GetElement(new ElementId(entity.LineId)) as FamilyInstance;
            List<PipeAndNodePoint> pipeAndNodePoints = new List<PipeAndNodePoint>();
            for (int i = 0; i < entity.PipeIds.Count(); i++)
            {
                var pipeId = entity.PipeIds[i];
                var pipe = Document.GetElement(new ElementId(pipeId));
                if (pipe == null)
                    return false;
                var tagId = entity.TagIds[i];
                pipeAndNodePoints.Add(new PipeAndNodePoint(pipe as Pipe, Document.GetElement(new ElementId(tagId)) as IndependentTag));
            }
            ////偏移量
            //XYZ skew = (line.Location as LocationPoint).Point - entity.StartPoint;
            //管道 获取
            //平行,垂直 向量
            XYZ parallelVector = null;
            XYZ verticalVector = null;
            parallelVector = ((pipeAndNodePoints.First().Pipe.Location as LocationCurve).Curve as Line).Direction;
            verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
            parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
            verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
            if (parallelVector.Y == 1)
                verticalVector = -verticalVector;
            //原始线高度
            var orientLineHeight = line.GetParameters(TagProperty.线高度1.ToString()).First().AsDouble();
            ////原始对象清理
            //Document.Delete(line.Id);
            //foreach (var tagId in entity.TagIds)
            //    Document.Delete(new ElementId(tagId));
            ////平行检测
            //if (!CheckParallel(pipes, verticalVector))
            //{
            //    return AnnotationBuildResult.NotParallel;
            //}
            //节点计算
            //TODO entity.StartPoint 不以记录为准 以其他点位的信息来计算
            GetNodePoints(pipeAndNodePoints, entity.StartPoint + skewVector);
            pipeAndNodePoints = pipeAndNodePoints.OrderByDescending(c => c.NodePoint.Y).ToList();
            List<XYZ> nodePoints = pipeAndNodePoints.Select(c => c.NodePoint).ToList();
            List<Pipe> pipes = pipeAndNodePoints.Select(c => c.Pipe).ToList();
            List<IndependentTag> tags = pipeAndNodePoints.Select(c => c.Tag).ToList();
            //if (nodePoints.Count() == 0)
            //{
            //    return AnnotationBuildResult.NoOverlap;
            //}
            //起始点
            startPoint = nodePoints.First();
            //线 创建
            var multipleTagSymbol = PAContext.GetMultipleTagSymbol(document);
            if (!multipleTagSymbol.IsActive)
                multipleTagSymbol.Activate();
            //line = Document.Create.NewFamilyInstance(startPoint, MultipleTagSymbol, view);
            (line.Location as LocationPoint).Point = startPoint;
            ////线 旋转处理
            //if (verticalVector.Y != 1)//TODO 判断是否相同方向
            //{
            //    LocationPoint locationPoint = line.Location as LocationPoint;
            //    if (locationPoint != null)
            //        locationPoint.RotateByXY(startPoint, verticalVector);
            //}
            //偏移计算
            var verticalSkew = LocationHelper.GetLengthBySide(skewVector, verticalVector);
            var parallelSkew = LocationHelper.GetLengthBySide(skewVector, parallelVector);
            //线参数
            UpdateLineParameters(nodePoints, line, verticalVector);
            //标注 创建
            var nodesHeight = UnitHelper.ConvertToFoot((nodePoints.Count() - 1) * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType);
            var lineHeight = orientLineHeight + verticalSkew > nodesHeight ? orientLineHeight + verticalSkew : nodesHeight;
            var tagHeight = lineHeight + nodesHeight;
            line.GetParameters(TagProperty.线高度1.ToString()).First().Set(lineHeight);
            var textSize = PAContext.TextSize;
            var widthScale = PAContext.WidthScale;
            switch (entity.LocationType)
            {
                case MultiPipeTagLocation.OnLineEdge:
                    //添加对应的单管直径标注
                    line.GetParameters(TagProperty.线宽度.ToString()).First().Set(UnitHelper.ConvertToFoot(200, AnnotationConstaints.UnitType));
                    var skewLength = AnnotationConstaints.SkewLengthForOffLine;
                    for (int i = 0; i < pipes.Count(); i++)
                    {
                        //var subTag = Document.Create.NewTag(view, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, startPoint);
                        var subTag = tags[i];
                        var text = subTag.TagText;
                        var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, AnnotationConstaints.Font).Width;
                        var actualLength = textLength / (textSize * widthScale);
                        subTag.TagHeadPosition = startPoint + skewLength * parallelVector + actualLength / 25.4 * parallelVector
                            + (tagHeight + UnitHelper.ConvertToFoot(-i * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType)) * verticalVector;
                        tags.Add(subTag);
                    }
                    break;
                case MultiPipeTagLocation.OnLine:
                    //添加对应的单管直径标注
                    line.GetParameters(TagProperty.线宽度.ToString()).First().Set(UnitHelper.ConvertToFoot(800, AnnotationConstaints.UnitType));
                    skewLength = AnnotationConstaints.SkewLengthForOnLine;
                    for (int i = 0; i < pipes.Count(); i++)
                    {
                        //var subTag = Document.Create.NewTag(view, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, startPoint);
                        var subTag = tags[i];
                        var text = subTag.TagText;
                        var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, AnnotationConstaints.Font).Width;
                        var actualLength = textLength / (textSize * widthScale);
                        subTag.TagHeadPosition = startPoint + skewLength * parallelVector + actualLength / 25.4 * parallelVector
                            + (tagHeight + UnitHelper.ConvertToFoot(-i * AnnotationConstaints.TextHeight + 0.5 * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType)) * verticalVector;
                        tags.Add(subTag);
                    }
                    break;
                default:
                    return false;
            }
            entity.ViewId = view.Id.IntegerValue;
            entity.StartPoint = startPoint;
            //entity.LineId = line.Id.IntegerValue;
            //entity.PipeIds.Clear();
            //entity.PipeIds.AddRange(pipes.Select(c => c.Id.IntegerValue));
            //entity.TagIds.Clear();
            //entity.TagIds.AddRange(tags.Select(c => c.Id.IntegerValue));
            return true;
        }

        /// <summary>
        /// 线 参数设置
        /// </summary>
        /// <param name="nodePoints"></param>
        /// <param name="line"></param>
        void UpdateLineParameters(List<XYZ> nodePoints, FamilyInstance line, XYZ verticalVector)
        {
            double deepLength = Math.Abs((nodePoints.Last() - nodePoints.First()).DotProduct(verticalVector));//nodePoints.First().DistanceTo(nodePoints.Last())
            line.GetParameters(TagProperty.线下探长度.ToString()).First().Set(deepLength);
            line.GetParameters(TagProperty.间距.ToString()).First().Set(UnitHelper.ConvertToFoot(AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType));
            line.GetParameters(TagProperty.文字行数.ToString()).First().Set(nodePoints.Count());
            for (int i = 2; i <= 8; i++)
            {
                var first = nodePoints.First();
                if (nodePoints.Count() >= i)
                {
                    var cur = nodePoints[i - 1];
                    line.GetParameters(string.Format("节点{0}可见性", i)).First().Set(1);
                    line.GetParameters(string.Format("节点{0}距离", i)).First().Set(Math.Abs((cur - first).DotProduct(verticalVector)));//cur.DistanceTo(first)
                }
                else
                {
                    line.GetParameters(string.Format("节点{0}可见性", i)).First().Set(0);
                    line.GetParameters(string.Format("节点{0}距离", i)).First().Set(0);
                }
            }
        }

        /// <summary>
        /// 节点计算 不带初始点参数,即首次生成,需检测重叠区间
        /// </summary>
        static List<XYZ> GetNodePoints(List<Pipe> pipes)
        {
            List<XYZ> nodePoints = new List<XYZ>();
            //重叠区间
            List<XYZ> lefts = new List<XYZ>();
            List<XYZ> rights = new List<XYZ>();
            var firstCurve = (pipes.First().Location as LocationCurve).Curve;
            firstCurve.MakeUnbound();
            for (int i = 0; i < pipes.Count(); i++)
            {
                var curve = (pipes[i].Location as LocationCurve).Curve;
                if (i == 0)
                {
                    if (curve.GetEndPoint(0).X < curve.GetEndPoint(1).X)
                    {
                        lefts.Add(curve.GetEndPoint(0));
                        rights.Add(curve.GetEndPoint(1));
                    }
                    else
                    {
                        lefts.Add(curve.GetEndPoint(1));
                        rights.Add(curve.GetEndPoint(0));
                    }
                }
                else
                {
                    if (curve.GetEndPoint(0).X < curve.GetEndPoint(1).X)
                    {
                        lefts.Add(firstCurve.Project(curve.GetEndPoint(0)).XYZPoint);
                        rights.Add(firstCurve.Project(curve.GetEndPoint(1)).XYZPoint);
                    }
                    else
                    {
                        lefts.Add(firstCurve.Project(curve.GetEndPoint(1)).XYZPoint);
                        rights.Add(firstCurve.Project(curve.GetEndPoint(0)).XYZPoint);
                    }
                }
            }
            var rightOfLefts = lefts.First(c => c.X == lefts.Max(p => p.X));
            var leftOfRights = rights.First(c => c.X == rights.Min(p => p.X));
            if (rightOfLefts.X > leftOfRights.X)
                return nodePoints;
            //节点计算
            var firstNode = (rightOfLefts + leftOfRights) / 2;
            nodePoints.Add(firstNode);
            for (int i = 1; i < pipes.Count(); i++)
            {
                var pipe = pipes[i];
                var locationCurve = (pipe.Location as LocationCurve).Curve;
                nodePoints.Add(locationCurve.Project(nodePoints.First()).XYZPoint);
            }
            return nodePoints;
        }

        /// <summary>
        /// 节点计算 带初始点参数,即定位变更,非首次生成
        /// </summary>
        static List<XYZ> GetNodePoints(List<Pipe> pipes, XYZ startPoint)
        {
            List<XYZ> nodePoints = new List<XYZ>();
            //节点计算
            for (int i = 0; i < pipes.Count(); i++)
            {
                var pipe = pipes[i];
                var locationCurve = (pipe.Location as LocationCurve).Curve;
                locationCurve.MakeUnbound();
                nodePoints.Add(locationCurve.Project(startPoint).XYZPoint);
            }
            return nodePoints;
        }

        /// <summary>
        /// 节点计算 带初始点参数,即定位变更,非首次生成
        /// </summary>
        static void GetNodePoints(List<PipeAndNodePoint> pipes, XYZ startPoint)
        {
            //节点计算
            for (int i = 0; i < pipes.Count(); i++)
            {
                var pipe = pipes[i].Pipe;
                var locationCurve = (pipe.Location as LocationCurve).Curve;
                locationCurve.MakeUnbound();
                pipes[i].NodePoint = locationCurve.Project(startPoint).XYZPoint;
            }
        }

        /// <summary>
        /// 节点计算 不带初始点参数,即首次生成,需检测重叠区间
        /// </summary>
        static bool GetNodePoints(List<PipeAndNodePoint> pipes, out XYZ rightOfLefts, out XYZ leftOfRights)
        {
            //重叠区间
            List<XYZ> lefts = new List<XYZ>();
            List<XYZ> rights = new List<XYZ>();
            var firstCurve = (pipes.First().Pipe.Location as LocationCurve).Curve;
            firstCurve.MakeUnbound();
            for (int i = 0; i < pipes.Count(); i++)
            {
                var curve = (pipes[i].Pipe.Location as LocationCurve).Curve;
                if (i == 0)
                {
                    if (curve.GetEndPoint(0).X < curve.GetEndPoint(1).X)
                    {
                        lefts.Add(curve.GetEndPoint(0));
                        rights.Add(curve.GetEndPoint(1));
                    }
                    else
                    {
                        lefts.Add(curve.GetEndPoint(1));
                        rights.Add(curve.GetEndPoint(0));
                    }
                }
                else
                {
                    if (curve.GetEndPoint(0).X < curve.GetEndPoint(1).X)
                    {
                        lefts.Add(firstCurve.Project(curve.GetEndPoint(0)).XYZPoint);
                        rights.Add(firstCurve.Project(curve.GetEndPoint(1)).XYZPoint);
                    }
                    else
                    {
                        lefts.Add(firstCurve.Project(curve.GetEndPoint(1)).XYZPoint);
                        rights.Add(firstCurve.Project(curve.GetEndPoint(0)).XYZPoint);
                    }
                }
            }
            rightOfLefts = lefts.First(c => c.X == lefts.Max(p => p.X));
            leftOfRights = rights.First(c => c.X == rights.Min(p => p.X));
            if (rightOfLefts.X > leftOfRights.X)
                return false;
            //节点计算
            var firstNode = (rightOfLefts + leftOfRights) / 2;
            pipes[0].NodePoint = firstNode;
            for (int i = 1; i < pipes.Count(); i++)
            {
                var pipe = pipes[i].Pipe;
                var locationCurve = (pipe.Location as LocationCurve).Curve;
                pipes[i].NodePoint = locationCurve.Project(firstNode).XYZPoint;
            }
            return true;
        }

        /// <summary>
        /// 平行检测
        /// </summary>
        /// <param name="pipes"></param>
        /// <param name="verticalVector"></param>
        /// <returns></returns>
        bool CheckParallel(IEnumerable<Pipe> pipes, XYZ verticalVector)
        {
            foreach (var pipe in pipes)
            {
                var direction = ((pipe.Location as LocationCurve).Curve as Line).Direction;
                var crossProduct = direction.CrossProduct(verticalVector);
                if (!AnnotationConstaints.IsMiniValue(crossProduct.X) || !AnnotationConstaints.IsMiniValue(crossProduct.Y))
                    return false;
            }
            return true;
        }
    }
}
