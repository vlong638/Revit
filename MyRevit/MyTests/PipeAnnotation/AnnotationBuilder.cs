using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using MyRevit.Entities;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MyRevit.MyTests.PipeAnnotation
{ 
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
    /// 常量,静态量
    /// </summary>
    public class AnnotationConstaints
    {
        public const double SkewLengthForOnLine = 0.2;
        public const double SkewLengthForOffLine = 0.4;
        public static Font Font = new Font("Angsana New", 20);
        public static double TextHeight = 150;
        public static UnitType UnitType = UnitType.millimeter;
    }
    /// <summary>
    /// 标注生成类
    /// </summary>
    public class AnnotationCreater
    {
        public AnnotationCreater()
        {
        }

        /// <summary>
        /// 加载族文件
        /// </summary>
        public bool LoadFamilySymbols(Document doc,bool needTransaction)
        {
            if (needTransaction)
            {
                if (!TransactionHelper.DelegateTransaction(doc, "多管标注生成", () =>
                {
                    SingleTagSymbol = LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0609管道标注\管道尺寸标记.rfa", "管道尺寸标记", "管道尺寸标记", BuiltInCategory.OST_PipeTags);
                    MultipleTagSymbol = LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0609管道标注\多管直径标注.rfa", "多管直径标注", "引线标注_文字在右端", BuiltInCategory.OST_DetailComponents);
                    return true;
                }))
                    return false;
            }
            else
            {
                SingleTagSymbol = LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0609管道标注\管道尺寸标记.rfa", "管道尺寸标记", "管道尺寸标记", BuiltInCategory.OST_PipeTags);
                MultipleTagSymbol = LoadFamilySymbol(doc, @"E:\WorkingSpace\Tasks\0609管道标注\多管直径标注.rfa", "多管直径标注", "引线标注_文字在右端", BuiltInCategory.OST_DetailComponents);
            }
            var familyDoc = doc.EditFamily(SingleTagSymbol.Family);
            var textElement = new FilteredElementCollector(familyDoc).OfClass(typeof(TextElement)).First(c => c.Name == "2.5") as TextElement;
            var textSizeStr = textElement.Symbol.get_Parameter(BuiltInParameter.TEXT_SIZE).AsValueString();
            TextSize = double.Parse(textSizeStr.Substring(0, textSizeStr.IndexOf(" mm")));
            WidthScale = double.Parse(textElement.Symbol.get_Parameter(BuiltInParameter.TEXT_WIDTH_SCALE).AsValueString());
            return true;
        }

        /// <summary>
        /// 加载族文件
        /// </summary>
        private FamilySymbol LoadFamilySymbol(Document doc, string familyFilePath, string familyName, string name, BuiltInCategory category)
        {
            FamilySymbol symbol = null;
            var symbols = new FilteredElementCollector(doc)
                .WherePasses(new ElementCategoryFilter(category))
                .WherePasses(new ElementClassFilter(typeof(FamilySymbol)));
            var targetSymbol = symbols.FirstOrDefault(c => (c as FamilySymbol).FamilyName == familyName && (c as FamilySymbol).Name == name);
            if (targetSymbol != null)
                symbol = targetSymbol as FamilySymbol;
            //空时加载族类型
            if (symbol == null)
            {
                var symbolFile = familyFilePath;
                Family family;
                if (doc.LoadFamily(symbolFile, out family))
                {
                    //获取族类型集合Id
                    var familySymbolIds = family.GetFamilySymbolIds();
                    foreach (var familySymbolId in familySymbolIds)
                    {
                        var element = doc.GetElement(familySymbolId) as FamilySymbol;
                        if (element != null && element.FamilyName == name)
                        {
                            symbol = element;
                            break;
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            return symbol;
        }

        #region 外部参数
        Document Document { set; get; }
        FamilySymbol SingleTagSymbol { set; get; }
        FamilySymbol MultipleTagSymbol { set; get; }
        public double TextSize { set; get; }
        public double WidthScale { set; get; }
        #endregion

        #region Doc关联参数
        PipeAnnotationEntityCollection Collection { set; get; }
        #endregion

        /// <summary>
        /// 生成标注
        /// </summary>
        public AnnotationBuildResult GenerateMultipleTagSymbol(Document document, IEnumerable<ElementId> selectedIds, MultiPipeTagLocation location)
        {
            Document = document;
            Collection = PipeAnnotationContext.GetCollection(Document);
            PipeAnnotationEntity entity = new PipeAnnotationEntity();
            entity.LocationType = location;
            View view = Document.ActiveView;
            AnnotationBuildResult result = GenerateMultipleTagSymbol(selectedIds, entity, view);
            if (result == AnnotationBuildResult.Success)
            {
                Collection.Add(entity);
                Collection.Save(Document);
            }
            return result;
        }

        private AnnotationBuildResult GenerateMultipleTagSymbol(IEnumerable<ElementId> selectedIds, PipeAnnotationEntity entity, View view)
        {
            XYZ startPoint = null;
            FamilyInstance line = null;
            var pipes = new List<Pipe>();
            var tags = new List<IndependentTag>();
            //管道 获取
            foreach (var selectedId in selectedIds)
                pipes.Add(Document.GetElement(selectedId) as Pipe);
            //平行,垂直 向量
            XYZ parallelVector = null;
            XYZ verticalVector = null;
            parallelVector = ((pipes.First().Location as LocationCurve).Curve as Line).Direction;
            verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
            parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
            verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
            //平行检测
            if (!CheckParallel(pipes, verticalVector))
            {
                return AnnotationBuildResult.NotParallel;
            }
            //节点计算
            List<XYZ> nodePoints = GetNodePoints(pipes).OrderByDescending(c => c.Y).ToList();
            if (nodePoints.Count() == 0)
            {
                return AnnotationBuildResult.NoOverlap;
            }
            //起始点
            startPoint = nodePoints.First();
            //线 创建
            if (!MultipleTagSymbol.IsActive)
                MultipleTagSymbol.Activate();
            line = Document.Create.NewFamilyInstance(startPoint, MultipleTagSymbol, view);
            //线 旋转处理
            if (verticalVector.Y != 1)
            {
                LocationPoint locationPoint = line.Location as LocationPoint;
                if (locationPoint != null)
                    locationPoint.RotateByXY(startPoint, verticalVector);
            }
            //线 参数设置
            UpdateLineParameters(nodePoints, line);
            //标注 创建
            switch (entity.LocationType)
            {
                case MultiPipeTagLocation.OnLineEdge:
                    //添加对应的单管直径标注
                    line.GetParameters(TagProperty.线宽度.ToString()).First().Set(UnitHelper.ConvertToInch(200, AnnotationConstaints.UnitType));
                    var height = Convert.ToDouble(line.GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) + (nodePoints.Count() - 1) * AnnotationConstaints.TextHeight;
                    var skewLength = AnnotationConstaints.SkewLengthForOffLine;
                    for (int i = 0; i < pipes.Count(); i++)
                    {
                        var subTag = Document.Create.NewTag(view, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, startPoint);
                        var text = subTag.TagText;
                        var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, AnnotationConstaints.Font).Width;
                        var actualLength = textLength / (TextSize * WidthScale);
                        subTag.TagHeadPosition = startPoint + skewLength * parallelVector + actualLength / 25.4 * parallelVector
                            + UnitHelper.ConvertToInch(height - i * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType) * verticalVector;
                        tags.Add(subTag);
                    }
                    break;
                case MultiPipeTagLocation.OnLine:
                    //添加对应的单管直径标注
                    line.GetParameters(TagProperty.线宽度.ToString()).First().Set(UnitHelper.ConvertToInch(800, AnnotationConstaints.UnitType));
                    height = Convert.ToDouble(line.GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) +
                     (nodePoints.Count() - 1) * AnnotationConstaints.TextHeight;
                    skewLength = AnnotationConstaints.SkewLengthForOnLine;
                    for (int i = 0; i < pipes.Count(); i++)
                    {
                        var subTag = Document.Create.NewTag(view, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, startPoint);
                        var text = subTag.TagText;
                        var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, AnnotationConstaints.Font).Width;
                        var actualLength = textLength / (TextSize * WidthScale);
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
        public bool RegenerateMultipleTagSymbolByEntity(Document document, PipeAnnotationEntity entity,XYZ skewVector)
        {
            Document = document;
            XYZ startPoint = null;
            View view = Document.ActiveView;
            FamilyInstance line = document.GetElement(new ElementId(entity.LineId)) as FamilyInstance;
            List<Pipe> pipes = new List<Pipe>();
            foreach (var pipeId in entity.PipeIds)
                pipes.Add(Document.GetElement(new ElementId(pipeId)) as Pipe);
            List<IndependentTag> tags = new List<IndependentTag>();
            foreach (var tagId in entity.TagIds)
                tags.Add(Document.GetElement(new ElementId(tagId)) as IndependentTag);
            ////偏移量
            //XYZ skew = (line.Location as LocationPoint).Point - entity.StartPoint;
            //管道 获取
            //平行,垂直 向量
            XYZ parallelVector = null;
            XYZ verticalVector = null;
            parallelVector = ((pipes.First().Location as LocationCurve).Curve as Line).Direction;
            verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
            parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
            verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
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
            List<XYZ> nodePoints = GetNodePoints(pipes, entity.StartPoint + skewVector).OrderByDescending(c => c.Y).ToList();
            //if (nodePoints.Count() == 0)
            //{
            //    return AnnotationBuildResult.NoOverlap;
            //}
            //起始点
            startPoint = nodePoints.First();
            //线 创建
            if (!MultipleTagSymbol.IsActive)
                MultipleTagSymbol.Activate();
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
            var lineHeight = orientLineHeight + verticalSkew> nodesHeight ? orientLineHeight + verticalSkew : nodesHeight;
            var tagHeight = lineHeight + nodesHeight;
            line.GetParameters(TagProperty.线高度1.ToString()).First().Set(lineHeight);
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
                        var actualLength = textLength / (TextSize * WidthScale);
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
                        var actualLength = textLength / (TextSize * WidthScale);
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

        ///// <summary>
        ///// 根据线的移动,重定位内容
        ///// </summary>
        //public bool RegenerateMultipleTagSymbolByLine(Document document, PipeAnnotationEntity entity)
        //{
        //    Document = document;
        //    var view = Document.ActiveView;
        //    //线
        //    var line = document.GetElement(new ElementId(entity.LineId));
        //    var lineLocation = line.Location as LocationPoint;

        //    //管道
        //    List<Pipe> pipes = new List<Pipe>();
        //    foreach (var pipeId in entity.PipeIds)
        //        pipes.Add(document.GetElement(new ElementId(pipeId)) as Pipe);
        //    //平行,垂直 向量
        //    XYZ parallelVector = null;
        //    XYZ verticalVector = null;
        //    parallelVector = ((pipes.First().Location as LocationCurve).Curve as Line).Direction;
        //    verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
        //    parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
        //    verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
        //    //节点计算
        //    List<XYZ> nodePoints = new List<XYZ>();
        //    for (int i = 0; i < pipes.Count(); i++)
        //    {
        //        var pipe = pipes[i];
        //        var locationCurve = (pipe.Location as LocationCurve).Curve;
        //        locationCurve.MakeUnbound();
        //        nodePoints.Add(locationCurve.Project(lineLocation.Point).XYZPoint);
        //    }
        //    //起始点
        //    var startPoint = nodePoints.First();
        //    ////线 创建
        //    //if (!MultipleTagSymbol.IsActive)
        //    //    MultipleTagSymbol.Activate();
        //    //line = Document.Create.NewFamilyInstance(startPoint, MultipleTagSymbol, view);
        //    ////线 旋转处理
        //    //if (verticalVector.Y != 1)
        //    //{
        //    //    LocationPoint locationPoint = line.Location as LocationPoint;
        //    //    if (locationPoint != null)
        //    //        locationPoint.RotateByXY(startPoint, verticalVector);
        //    //}
        //    //线 参数设置
        //    double deepLength = nodePoints.First().DistanceTo(nodePoints.Last());
        //    double textHeight = UnitHelper.ConvertToInch(AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType);
        //    line.GetParameters(TagProperty.线下探长度.ToString()).First().Set(deepLength);
        //    line.GetParameters(TagProperty.间距.ToString()).First().Set(textHeight);
        //    line.GetParameters(TagProperty.文字行数.ToString()).First().Set(nodePoints.Count());
        //    //偏移量
        //    var moveLength = startPoint.DistanceTo(lineLocation.Point);
        //    if (lineLocation.Point.Y < startPoint.Y)
        //        moveLength = -moveLength;
        //    var lineHeightParameter = line.GetParameters(TagProperty.线高度1.ToString()).First();
        //    var lineHeight = lineHeightParameter.AsDouble();
        //    lineHeight += moveLength;
        //    lineHeightParameter.Set(UnitHelper.ConvertToInch(lineHeight, AnnotationConstaints.UnitType));
        //    for (int i = 2; i <= 8; i++)
        //    {
        //        var first = nodePoints.First();
        //        if (nodePoints.Count() >= i)
        //        {
        //            var cur = nodePoints[i - 1];
        //            line.GetParameters(string.Format("节点{0}可见性", i)).First().Set(1);
        //            line.GetParameters(string.Format("节点{0}距离", i)).First().Set(cur.DistanceTo(first));
        //        }
        //        else
        //        {
        //            line.GetParameters(string.Format("节点{0}可见性", i)).First().Set(0);
        //            line.GetParameters(string.Format("节点{0}距离", i)).First().Set(0);
        //        }
        //    }
        //    //标注 创建
        //    switch (entity.LocationType)
        //    {
        //        case MultiPipeTagLocation.OnLineEdge:
        //            //添加对应的单管直径标注
        //            var height = Convert.ToDouble(line.GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) + (nodePoints.Count() - 1) * AnnotationConstaints.TextHeight + moveLength;
        //            var skewLength = PipeAnnotationConstaints.SkewLengthForOffLine;
        //            for (int i = 0; i < pipes.Count(); i++)
        //            {
        //                //var subTag = Document.Create.NewTag(View, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, startPoint);
        //                var subTag = document.GetElement(new ElementId(entity.TagIds[i])) as IndependentTag;
        //                subTag.TagHeadPosition = GetHeadPositionForOnLineEdge(parallelVector, verticalVector, startPoint, height, skewLength, i, subTag);
        //            }
        //            break;
        //        case MultiPipeTagLocation.OnLine:
        //            //添加对应的单管直径标注
        //            height = Convert.ToDouble(line.GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) + (nodePoints.Count() - 1) * AnnotationConstaints.TextHeight + moveLength;
        //            skewLength = PipeAnnotationConstaints.SkewLengthForOnLine;
        //            for (int i = 0; i < pipes.Count(); i++)
        //            {
        //                //var subTag = Document.Create.NewTag(View, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, startPoint);
        //                var subTag = document.GetElement(new ElementId(entity.TagIds[i])) as IndependentTag;
        //                subTag.TagHeadPosition = GetHeadPositionForOnLine(parallelVector, verticalVector, startPoint, height, skewLength, i, subTag);
        //            }
        //            break;
        //        default:
        //            return false;
        //    }
        //    return true;
        //}

        ///// <summary>
        ///// 根据线的移动,重定位内容
        ///// </summary>
        //public bool RegenerateMultipleTagSymbolByLine(Document document, PipeAnnotationEntity entity)
        //{
        //    Document = document;
        //    View = Document.ActiveView;
        //    //线
        //    var line = document.GetElement(new ElementId(entity.LineId));
        //    var lineLocation = line.Location as LocationPoint;
        //    //管道
        //    List<Pipe> pipes = new List<Pipe>();
        //    foreach (var pipeId in entity.PipeIds)
        //        pipes.Add(document.GetElement(new ElementId(pipeId)) as Pipe);
        //    //平行,垂直 向量
        //    XYZ parallelVector = null;
        //    XYZ verticalVector = null;
        //    parallelVector = ((pipes.First().Location as LocationCurve).Curve as Line).Direction;
        //    verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
        //    parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
        //    verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
        //    //节点计算
        //    List<XYZ> nodePoints = new List<XYZ>();
        //    for (int i = 0; i < pipes.Count(); i++)
        //    {
        //        var pipe = pipes[i];
        //        var locationCurve = (pipe.Location as LocationCurve).Curve;
        //        locationCurve.MakeUnbound();
        //        nodePoints.Add(locationCurve.Project(lineLocation.Point).XYZPoint);
        //    }
        //    //起始点
        //    var startPoint = nodePoints.First();
        //    //线 参数设置
        //    double deepLength = nodePoints.First().DistanceTo(nodePoints.Last());
        //    double textHeight = UnitHelper.ConvertToInch(AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType);
        //    line.GetParameters(TagProperty.线下探长度.ToString()).First().Set(deepLength);
        //    line.GetParameters(TagProperty.间距.ToString()).First().Set(textHeight);
        //    line.GetParameters(TagProperty.文字行数.ToString()).First().Set(nodePoints.Count());
        //    //偏移量
        //    var moveLength = startPoint.DistanceTo(lineLocation.Point);
        //    if (lineLocation.Point.Y < startPoint.Y)
        //        moveLength = -moveLength;
        //    var lineHeightParameter = line.GetParameters(TagProperty.线高度1.ToString()).First();
        //    var lineHeight = lineHeightParameter.AsDouble();
        //    lineHeight += moveLength;
        //    lineHeightParameter.Set(UnitHelper.ConvertToInch(lineHeight, AnnotationConstaints.UnitType));
        //    for (int i = 2; i <= 8; i++)
        //    {
        //        var first = nodePoints.First();
        //        if (nodePoints.Count() >= i)
        //        {
        //            var cur = nodePoints[i - 1];
        //            line.GetParameters(string.Format("节点{0}可见性", i)).First().Set(1);
        //            line.GetParameters(string.Format("节点{0}距离", i)).First().Set(cur.DistanceTo(first));
        //        }
        //        else
        //        {
        //            line.GetParameters(string.Format("节点{0}可见性", i)).First().Set(0);
        //            line.GetParameters(string.Format("节点{0}距离", i)).First().Set(0);
        //        }
        //    }
        //    //标注 创建
        //    switch (entity.LocationType)
        //    {
        //        case MultiPipeTagLocation.OnLineEdge:
        //            //添加对应的单管直径标注
        //            var height = Convert.ToDouble(line.GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) + (nodePoints.Count() - 1) * AnnotationConstaints.TextHeight + moveLength;
        //            var skewLength = PipeAnnotationConstaints.SkewLengthForOffLine;
        //            for (int i = 0; i < pipes.Count(); i++)
        //            {
        //                //var subTag = Document.Create.NewTag(View, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, startPoint);
        //                var subTag = document.GetElement(new ElementId(entity.TagIds[i])) as IndependentTag;
        //                subTag.TagHeadPosition = GetHeadPositionForOnLineEdge(parallelVector, verticalVector, startPoint, height, skewLength, i, subTag);
        //            }
        //            break;
        //        case MultiPipeTagLocation.OnLine:
        //            //添加对应的单管直径标注
        //            height = Convert.ToDouble(line.GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) + (nodePoints.Count() - 1) * AnnotationConstaints.TextHeight + moveLength;
        //            skewLength = PipeAnnotationConstaints.SkewLengthForOnLine;
        //            for (int i = 0; i < pipes.Count(); i++)
        //            {
        //                //var subTag = Document.Create.NewTag(View, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, startPoint);
        //                var subTag = document.GetElement(new ElementId(entity.TagIds[i])) as IndependentTag;
        //                subTag.TagHeadPosition = GetHeadPositionForOnLine(parallelVector, verticalVector, startPoint, height, skewLength, i, subTag);
        //            }
        //            break;
        //        default:
        //            return false;
        //    }
        //    return true;
        //}

        /// <summary>
        /// 位于线上,标注定位计算
        /// </summary>
        private XYZ GetHeadPositionForOnLine(XYZ parallelVector, XYZ verticalVector, XYZ startPoint, double height, double skewLength, int i, IndependentTag subTag)
        {
            var text = subTag.TagText;
            var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, AnnotationConstaints.Font).Width;
            var actualLength = textLength / (TextSize * WidthScale);
            return startPoint + skewLength * parallelVector + actualLength / 25.4 * parallelVector + UnitHelper.ConvertToInch(height - (i - 0.5) * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType) * verticalVector;
        }

        /// <summary>
        /// 位于线端,标注定位计算
        /// </summary>
        private XYZ GetHeadPositionForOnLineEdge(XYZ parallelVector, XYZ verticalVector, XYZ startPoint, double height, double skewLength, int i, IndependentTag subTag)
        {
            var text = subTag.TagText;
            var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, AnnotationConstaints.Font).Width;
            var actualLength = textLength / (TextSize * WidthScale);
            return startPoint + skewLength * parallelVector + actualLength / 25.4 * parallelVector + UnitHelper.ConvertToInch(height - i * AnnotationConstaints.TextHeight, AnnotationConstaints.UnitType) * verticalVector;
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
                    if (curve.GetEndPoint(0).X < curve.GetEndPoint(1).Y)
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
                    if (curve.GetEndPoint(0).X < curve.GetEndPoint(1).Y)
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
        /// 平行检测
        /// </summary>
        /// <param name="pipes"></param>
        /// <param name="verticalVector"></param>
        /// <returns></returns>
        private bool CheckParallel(List<Pipe> pipes, XYZ verticalVector)
        {
            foreach (var pipe in pipes)
            {
                var direction = ((pipe.Location as LocationCurve).Curve as Line).Direction;
                var crossProduct = direction.CrossProduct(verticalVector);
                if (crossProduct.X != 0 || crossProduct.Y != 0)
                    return false;
            }
            return true;
        }
    }
}
