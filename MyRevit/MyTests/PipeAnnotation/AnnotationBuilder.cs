using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using MyRevit.Entities;
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
        /// <summary>
        /// 管道并非平行
        /// </summary>
        NotParallel,
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
        public AnnotationCreater(Document document, FamilySymbol singleTagSymbol, FamilySymbol multipleTagSymbol, double textSize, double widthScale)
        {
            Document = document;
            SingleTagSymbol = singleTagSymbol;
            MultipleTagSymbol = multipleTagSymbol;
            TextSize = textSize;
            WidthScale = widthScale;

            View = Document.ActiveView;
            Collection = PipeAnnotationContext.GetCollection(Document);
        }

        #region 外部参数
        Document Document { set; get; }
        FamilySymbol SingleTagSymbol { set; get; }
        FamilySymbol MultipleTagSymbol { set; get; }
        double TextSize { set; get; }
        double WidthScale { set; get; }
        #endregion

        #region 关联参数
        PipeAnnotationEntityCollection Collection { set; get; }
        View View { set; get; }
        #endregion

        #region 辅助工具
        public LocationCalculator Calculator { set; get; }
        #endregion


        public AnnotationBuildResult GenerateMultipleTagSymbol(IList<Reference> selectedIds)
        {
            PipeAnnotationEntity entity = new PipeAnnotationEntity();
            //管道 获取
            List<Pipe> pipes = new List<Pipe>();
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
                return AnnotationBuildResult.NotParallel;
            //节点计算
            List<XYZ> nodePoints = GetNodePoints(pipes).OrderByDescending(c => c.Y).ToList();
            //起始点
            var startPoint = nodePoints.First();
            //线 创建
            if (!MultipleTagSymbol.IsActive)
                MultipleTagSymbol.Activate();
            var line = Document.Create.NewFamilyInstance(startPoint, MultipleTagSymbol, View);
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
            List<IndependentTag> tags = new List<IndependentTag>();
            switch (entity.LocationType)
            {
                case MultiPipeTagLocation.OnLineEdge:
                    //添加对应的单管直径标注
                    line.GetParameters(TagProperty.线宽度.ToString()).First().Set(UnitHelper.ConvertToInch(200, GetUnitType()));
                    var height = Convert.ToDouble(line.GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) +
                     (nodePoints.Count() - 1) * TextHeight;
                    var skewLength = PipeAnnotationConstaints.SkewLengthForOffLine;
                    for (int i = 0; i < pipes.Count(); i++)
                    {
                        var subTag = Document.Create.NewTag(View, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal
                            , startPoint + UnitHelper.ConvertToInch(height - i * TextHeight, GetUnitType()) * verticalVector + skewLength * parallelVector);
                        var text = subTag.TagText;
                        var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, Font).Width;
                        var actualLength = textLength / (TextSize * WidthScale);
                        subTag.TagHeadPosition += actualLength / 25.4 * parallelVector;
                        tags.Add(subTag);
                    }
                    break;
                case MultiPipeTagLocation.OnLine:
                    //添加对应的单管直径标注
                    line.GetParameters(TagProperty.线宽度.ToString()).First().Set(UnitHelper.ConvertToInch(800, GetUnitType()));
                    height = Convert.ToDouble(line.GetParameters(TagProperty.线高度1.ToString()).First().AsValueString()) +
                     (nodePoints.Count() - 1) * TextHeight;
                    skewLength = PipeAnnotationConstaints.SkewLengthForOnLine;
                    for (int i = 0; i < pipes.Count(); i++)
                    {
                        var subTag = Document.Create.NewTag(View, pipes[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal
                            , startPoint + UnitHelper.ConvertToInch(height - (i - 0.5) * TextHeight, GetUnitType()) * verticalVector + skewLength * parallelVector);
                        var text = subTag.TagText;
                        var textLength = System.Windows.Forms.TextRenderer.MeasureText(text, Font).Width;
                        var actualLength = textLength / (TextSize * WidthScale);
                        subTag.TagHeadPosition += actualLength / 25.4 * parallelVector;
                        tags.Add(subTag);
                    }
                    break;
                default:
                    return AnnotationBuildResult.NoLocationType;
            }
            entity.ViewId = View.Id.IntegerValue;
            entity.LineId = line.Id.IntegerValue;
            foreach (var pipe in pipes)
                entity.PipeIds.Add(pipe.Id.IntegerValue);
            foreach (var tag in tags)
                entity.TagIds.Add(tag.Id.IntegerValue);
            Collection.Add(entity);
            Collection.Save(Document);
            return AnnotationBuildResult.Success;
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
            line.GetParameters(TagProperty.间距.ToString()).First().Set(UnitHelper.ConvertToInch(TextHeight, GetUnitType()));
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

        double TextHeight = 150;
        Font Font = new Font("Angsana New", 20);
        private UnitType GetUnitType()
        {
            return UnitType.millimeter;
        }

        /// <summary>
        /// 节点计算
        /// </summary>
        /// <param name="pipes"></param>
        /// <returns></returns>
        private static List<XYZ> GetNodePoints(List<Pipe> pipes)
        {
            List<XYZ> nodePoints = new List<XYZ>();
            foreach (var pipe in pipes)
            {
                var locationCurve = (pipe.Location as LocationCurve).Curve;
                if (nodePoints.Count > 0)
                {
                    locationCurve.MakeUnbound();
                    nodePoints.Add(locationCurve.Project(nodePoints.First()).XYZPoint);
                }
                else
                {
                    var midPoint = (locationCurve.GetEndPoint(0) + locationCurve.GetEndPoint(1)) / 2;
                    nodePoints.Add(midPoint);
                }
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
    public class LocationCalculator
    {
        double TextSize { set; get; }
        Font Font { set; get; }

        public LocationCalculator(double textSize)
        {
            TextSize = textSize;

            Font = new Font("Angsana New", 20);
        }


    }
}
