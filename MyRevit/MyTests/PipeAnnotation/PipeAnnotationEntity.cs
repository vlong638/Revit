//using Autodesk.Revit.DB;
//using MyRevit.MyTests.PipeAnnotation;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace PmSoft.Optimization.DrawingProduction
//{
//    /// <summary>
//    /// 存储对象
//    /// </summary>
//    public class PipeAnnotationEntity
//    {
//        public MultiPipeTagLocation LocationType { set; get; }
//        public int ViewId { set; get; }
//        public int LineId { set; get; }
//        public List<int> PipeIds { set; get; }
//        public List<int> TagIds { set; get; }
//        public XYZ StartPoint { set; get; }

//        public PipeAnnotationEntity()
//        {
//            PipeIds = new List<int>();
//            TagIds = new List<int>();
//        }

//        public PipeAnnotationEntity(MultiPipeTagLocation locationType, int viewId, int lineId, List<int> pipeIds, List<int> tagIds, XYZ startPoint)
//        {
//            LocationType = locationType;
//            ViewId = viewId;
//            LineId = lineId;
//            PipeIds = pipeIds;
//            TagIds = tagIds;
//            StartPoint = startPoint;
//        }
//    }

//    /// <summary>
//    /// 存储对象集合
//    /// </summary>
//    public class PipeAnnotationEntityCollection : List<PipeAnnotationEntity>
//    {
//        public static string PropertyInnerSplitter = "_";
//        public static string PropertySplitter = ",";
//        public static string EntitySplitter = ";";
//        public static char PropertyInnerSplitter_Char = '_';
//        public static char PropertySplitter_Char = ',';
//        public static char EntitySplitter_Char = ';';

//        public PipeAnnotationEntityCollection()
//        {
//        }
//        public PipeAnnotationEntityCollection(string data)
//        {
//            init(data);
//        }

//        private void init(string data)
//        {
//            if (string.IsNullOrEmpty(data))
//                return;
//            var entities = data.Split(EntitySplitter_Char);
//            var propertySplitter = PropertySplitter_Char;
//            foreach (var entity in entities)
//            {
//                if (string.IsNullOrEmpty(entity))
//                    continue;
//                var properties = entity.Split(propertySplitter);
//                if (properties.Count() == 6)
//                {
//                    MultiPipeTagLocation locationType = (MultiPipeTagLocation)Enum.Parse(typeof(MultiPipeTagLocation), properties[0]);
//                    int viewId = Convert.ToInt32(properties[1]);
//                    int specialTagFrame = Convert.ToInt32(properties[2]);
//                    List<int> pipeIds = new List<int>();
//                    foreach (var item in properties[3].Split(PropertyInnerSplitter_Char))
//                    {
//                        if (item != "")
//                            pipeIds.Add(Convert.ToInt32(item));
//                    }
//                    List<int> annotationIds = new List<int>();
//                    foreach (var item in properties[4].Split(PropertyInnerSplitter_Char))
//                    {
//                        if (item != "")
//                            annotationIds.Add(Convert.ToInt32(item));
//                    }
//                    var pointStr = properties[5].Split(PropertyInnerSplitter_Char);
//                    XYZ startPoint = new XYZ(Convert.ToDouble(pointStr[0]), Convert.ToDouble(pointStr[1]), Convert.ToDouble(pointStr[2]));
//                    Add(new PipeAnnotationEntity(locationType, viewId, specialTagFrame, pipeIds, annotationIds, startPoint));
//                }
//            }
//        }

//        /// <summary>
//        /// 转String
//        /// </summary>
//        /// <returns></returns>
//        public string ToData()
//        {
//            return string.Join(EntitySplitter, this.Select(c => (int)c.LocationType
//            + PropertySplitter + c.ViewId
//            + PropertySplitter + c.LineId
//            + PropertySplitter + string.Join(PropertyInnerSplitter, c.PipeIds)
//            + PropertySplitter + string.Join(PropertyInnerSplitter, c.TagIds)
//            + PropertySplitter + string.Join(PropertyInnerSplitter, new List<double>() { c.StartPoint.X, c.StartPoint.Y, c.StartPoint.Z })
//            ));
//        }

//        /// <summary>
//        /// 保存
//        /// </summary>
//        /// <param name="doc"></param>
//        public void Save(Document doc)
//        {
//            PAContext.SaveCollection(doc);
//        }
//    }
//}
