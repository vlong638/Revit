using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.MyTests
{
    /// <summary>
    /// 关联存储
    /// </summary>
    public class BeamAnnotationEntity
    {
        public bool IsEditing { set; get; }
        public int ViewId { set; get; }
        public int BeamId { set; get; }
        public int LineId { set; get; }
        public int TagId { set; get; }

        public BeamAnnotationEntity(int viewId, int beamId, int lineId, int tagId)
        {
            ViewId = viewId;
            BeamId = beamId;
            LineId = lineId;
            TagId = tagId;
        }
    }
    /// <summary>
    /// 关联数据集合
    /// </summary>
    public class BeamAnnotationEntityCollection : List<BeamAnnotationEntity>
    {
        public static string PropertySplitter = ",";
        public static string EntitySplitter = ";";

        public BeamAnnotationEntityCollection(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;
            var entities = data.Split(EntitySplitter.ToCharArray()[0]);
            var propertySplitter = PropertySplitter.ToCharArray()[0];
            foreach (var entity in entities)
            {
                if (string.IsNullOrEmpty(entity))
                    continue;
                var properties = entity.Split(propertySplitter);
                Add(new BeamAnnotationEntity(Convert.ToInt32(properties[0]), Convert.ToInt32(properties[1]), Convert.ToInt32(properties[2]), Convert.ToInt32(properties[3])));
            }
        }

        public string ToData()
        {
            return string.Join(EntitySplitter, this.Select(c => c.ViewId + PropertySplitter + c.BeamId + PropertySplitter + c.LineId + PropertySplitter + c.TagId));
        }
    }
}
