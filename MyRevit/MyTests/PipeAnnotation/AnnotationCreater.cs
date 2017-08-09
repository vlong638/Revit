using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB.Plumbing;

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
            if (selectedIds.Count()> AnnotationConstaints.PipeCountMax)
                throw new NotImplementedException("暂不支持8根以上及管道的多管直径标注生成");
            Document = document;
            Collection = PipeAnnotationContext.GetCollection(Document);
            PipeAnnotationEntity entity = new PipeAnnotationEntity();
            entity.LocationType = setting.Location;
            View view = Document.ActiveView;
            AnnotationBuildResult result = GenerateMultipleTagSymbol(document, selectedIds, entity, setting);
            if (generateSingleOne && result == AnnotationBuildResult.Success)
            {
                Collection.Add(entity);
                Collection.Save(Document);
            }
            else if(result == AnnotationBuildResult.Success)
            {
                Collection.Add(entity);
            }
            return result;
        }

        public void FinishMultipleGenerate(Document document)
        {
            if (Collection==null)
                Collection= PipeAnnotationContext.GetCollection(Document);
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
        private AnnotationBuildResult GenerateMultipleTagSymbol(Document document, IEnumerable<ElementId> selectedIds, PipeAnnotationEntity entity, MultiPipeAnnotationSettings setting)
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
            if (!CheckParallel(pipeAndNodePoints.Select(c=>c.Pipe), verticalVector))
            {
                return AnnotationBuildResult.NotParallel;
            }
            //节点计算
            if (!GetNodePoints(pipeAndNodePoints))
            {
                return AnnotationBuildResult.NoOverlap;
            }
            pipeAndNodePoints = pipeAndNodePoints.OrderByDescending(c => c.NodePoint.Y).ToList();
            var pipes = pipeAndNodePoints.Select(c => c.Pipe).ToList();
            var orderedNodePoints = pipeAndNodePoints.Select(c => c.NodePoint).ToList();
            //起始点
            startPoint = orderedNodePoints.First();
            //线 创建
            var multipleTagSymbol = PipeAnnotationContext.GetMultipleTagSymbol(document);
            if (!multipleTagSymbol.IsActive)
                multipleTagSymbol.Activate();
            line = Document.Create.NewFamilyInstance(startPoint, multipleTagSymbol, view);
            //线 旋转处理
            LocationPoint locationPoint = line.Location as LocationPoint;
            if (locationPoint != null)
                locationPoint.RotateByXY(startPoint, verticalVector);
            //线 参数设置
            UpdateLineParameters(orderedNodePoints, line);
            //标注 创建
            var textSize = PipeAnnotationContext.TextSize;
            var widthScale = PipeAnnotationContext.WidthScale;
            switch (entity.LocationType)
            {
                case MultiPipeTagLocation.OnLineEdge:
                    //添加对应的单管直径标注
                    line.GetParameters(TagProperty.线宽度.ToString()).First().Set(UnitHelper.ConvertToInch(200, AnnotationConstaints.UnitType));
                    var height = Convert.ToDouble(line.GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) + (orderedNodePoints.Count() - 1) * AnnotationConstaints.TextHeight;
                    var skewLength = AnnotationConstaints.SkewLengthForOffLine;
                    for (int i = 0; i < pipes.Count(); i++)
                    {
                        var subTag = Document.Create.NewTag(view, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, startPoint);
                        var text = subTag.TagText;
                        var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, AnnotationConstaints.Font).Width;
                        var actualLength = textLength / (textSize * widthScale);
                        subTag.TagHeadPosition = startPoint + skewLength * parallelVector + actualLength / 25.4 * parallelVector
                            + UnitHelper.ConvertToInch(height - i * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType) * verticalVector;
                        tags.Add(subTag);
                    }
                    break;
                case MultiPipeTagLocation.OnLine:
                    //添加对应的单管直径标注
                    line.GetParameters(TagProperty.线宽度.ToString()).First().Set(UnitHelper.ConvertToInch(800, AnnotationConstaints.UnitType));
                    height = Convert.ToDouble(line.GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) +
                     (orderedNodePoints.Count() - 1) * AnnotationConstaints.TextHeight;
                    skewLength = AnnotationConstaints.SkewLengthForOnLine;
                    for (int i = 0; i < pipes.Count(); i++)
                    {
                        var subTag = Document.Create.NewTag(view, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, startPoint);
                        var text = subTag.TagText;
                        var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, AnnotationConstaints.Font).Width;
                        var actualLength = textLength / (textSize * widthScale);
                        subTag.TagHeadPosition = startPoint + skewLength * parallelVector + actualLength / 25.4 * parallelVector
                            + UnitHelper.ConvertToInch(height - i * AnnotationConstaints.TextHeight + 0.5 * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType) * verticalVector;
                        tags.Add(subTag);
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
            return AnnotationBuildResult.Success;
        }

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
            var multipleTagSymbol = PipeAnnotationContext.GetMultipleTagSymbol(document);
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
            UpdateLineParameters(nodePoints, line);
            //标注 创建
            var nodesHeight = UnitHelper.ConvertToInch((nodePoints.Count() - 1) * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType);
            var lineHeight = orientLineHeight + verticalSkew > nodesHeight ? orientLineHeight + verticalSkew : nodesHeight;
            var tagHeight = lineHeight + nodesHeight;
            line.GetParameters(TagProperty.线高度1.ToString()).First().Set(lineHeight);
            var textSize = PipeAnnotationContext.TextSize;
            var widthScale = PipeAnnotationContext.WidthScale;
            switch (entity.LocationType)
            {
                case MultiPipeTagLocation.OnLineEdge:
                    //添加对应的单管直径标注
                    line.GetParameters(TagProperty.线宽度.ToString()).First().Set(UnitHelper.ConvertToInch(200, AnnotationConstaints.UnitType));
                    var skewLength = AnnotationConstaints.SkewLengthForOffLine;
                    for (int i = 0; i < pipes.Count(); i++)
                    {
                        //var subTag = Document.Create.NewTag(view, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, startPoint);
                        var subTag = tags[i];
                        var text = subTag.TagText;
                        var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, AnnotationConstaints.Font).Width;
                        var actualLength = textLength / (textSize * widthScale);
                        subTag.TagHeadPosition = startPoint + skewLength * parallelVector + actualLength / 25.4 * parallelVector
                            + (tagHeight + UnitHelper.ConvertToInch(-i * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType)) * verticalVector;
                        tags.Add(subTag);
                    }
                    break;
                case MultiPipeTagLocation.OnLine:
                    //添加对应的单管直径标注
                    line.GetParameters(TagProperty.线宽度.ToString()).First().Set(UnitHelper.ConvertToInch(800, AnnotationConstaints.UnitType));
                    skewLength = AnnotationConstaints.SkewLengthForOnLine;
                    for (int i = 0; i < pipes.Count(); i++)
                    {
                        //var subTag = Document.Create.NewTag(view, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, startPoint);
                        var subTag = tags[i];
                        var text = subTag.TagText;
                        var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, AnnotationConstaints.Font).Width;
                        var actualLength = textLength / (textSize * widthScale);
                        subTag.TagHeadPosition = startPoint + skewLength * parallelVector + actualLength / 25.4 * parallelVector
                            + (tagHeight + UnitHelper.ConvertToInch(-i * AnnotationConstaints.TextHeight + 0.5 * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType)) * verticalVector;
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
        private void UpdateLineParameters(List<XYZ> nodePoints, FamilyInstance line)
        {
            double deepLength = nodePoints.First().DistanceTo(nodePoints.Last());
            line.GetParameters(TagProperty.线下探长度.ToString()).First().Set(deepLength);
            line.GetParameters(TagProperty.间距.ToString()).First().Set(UnitHelper.ConvertToInch(AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType));
            line.GetParameters(TagProperty.文字行数.ToString()).First().Set(nodePoints.Count());
            for (int i = 2; i <= 8; i++)
            {
                var first = nodePoints.First();
                if (nodePoints.Count() >= i)
                {
                    var cur = nodePoints[i - 1];
                    line.GetParameters(string.Format("节点{0}可见性", i)).First().Set(1);
                    line.GetParameters(string.Format("节点{0}距离", i)).First().Set(cur.DistanceTo(first));
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
        private static List<XYZ> GetNodePoints(List<Pipe> pipes)
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
        private static List<XYZ> GetNodePoints(List<Pipe> pipes, XYZ startPoint)
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
        private static void GetNodePoints(List<PipeAndNodePoint> pipes, XYZ startPoint)
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
        private static bool GetNodePoints(List<PipeAndNodePoint> pipes)
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
            var rightOfLefts = lefts.First(c => c.X == lefts.Max(p => p.X));
            var leftOfRights = rights.First(c => c.X == rights.Min(p => p.X));
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
        private bool CheckParallel(IEnumerable<Pipe> pipes, XYZ verticalVector)
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
