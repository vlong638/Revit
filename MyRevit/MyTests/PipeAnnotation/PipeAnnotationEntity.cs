using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.MyTests.PipeAnnotation
{
    #region 关联存储
    /// <summary>
    /// 存储对象
    /// </summary>
    public class PipeAnnotationEntity
    {
        public MultiPipeTagLocation LocationType { set; get; }
        public int ViewId { set; get; }
        public int LineId { set; get; }
        public List<int> PipeIds { set; get; }
        public List<int> TagIds { set; get; }

        public PipeAnnotationEntity()
        {
            PipeIds = new List<int>();
            TagIds = new List<int>();
        }

        public PipeAnnotationEntity(MultiPipeTagLocation locationType, int viewId, int lineId, List<int> pipeIds, List<int> tagIds)
        {
            LocationType = locationType;
            ViewId = viewId;
            LineId = lineId;
            PipeIds = pipeIds;
            TagIds = tagIds;
        }
    }
    /// <summary>
    /// 存储对象集合
    /// </summary>
    public class PipeAnnotationEntityCollection : List<PipeAnnotationEntity>
    {
        public static string PropertyInnerSplitter = "_";
        public static string PropertySplitter = ",";
        public static string EntitySplitter = ";";
        public static char PropertyInnerSplitter_Char = '_';
        public static char PropertySplitter_Char = ',';
        public static char EntitySplitter_Char = ';';

        public PipeAnnotationEntityCollection(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;
            var entities = data.Split(EntitySplitter_Char);
            var propertySplitter = PropertySplitter_Char;
            foreach (var entity in entities)
            {
                if (string.IsNullOrEmpty(entity))
                    continue;
                var properties = entity.Split(propertySplitter);
                if (properties.Count() == 5)
                {
                    MultiPipeTagLocation locationType = (MultiPipeTagLocation)Enum.Parse(typeof(MultiPipeTagLocation), properties[0]);
                    int viewId = Convert.ToInt32(properties[1]);
                    int specialTagFrame = Convert.ToInt32(properties[2]);
                    List<int> pipeIds = new List<int>();
                    foreach (var item in properties[3].Split(PropertyInnerSplitter_Char))
                    {
                        if (item != "")
                            pipeIds.Add(Convert.ToInt32(item));
                    }
                    List<int> annotationIds = new List<int>();
                    foreach (var item in properties[4].Split(PropertyInnerSplitter_Char))
                    {
                        if (item != "")
                            annotationIds.Add(Convert.ToInt32(item));
                    }
                    Add(new PipeAnnotationEntity(locationType, viewId, specialTagFrame, pipeIds, annotationIds));
                }
            }
        }

        /// <summary>
        /// 转String
        /// </summary>
        /// <returns></returns>
        public string ToData()
        {
            return string.Join(EntitySplitter, this.Select(c => (int)c.LocationType + PropertySplitter + c.ViewId + PropertySplitter + c.LineId + PropertySplitter + string.Join(PropertyInnerSplitter, c.PipeIds) + PropertySplitter + string.Join(PropertyInnerSplitter, c.TagIds)));
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="doc"></param>
        public void Save(Document doc)
        {
            PipeAnnotationContext.SaveCollection(doc);
        }
    }
    /// <summary>
    /// 上下文
    /// </summary>
    public class PipeAnnotationContext
    {
        /// <summary>
        /// 取Entity
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static Entity GetEntity(Document doc)
        {
            Entity entity;
            var storage = GetStorage(doc);
            var schema = GetSchema(SchemaId, SchemaName);
            entity = storage.GetEntity(schema);
            return entity;
        }

        #region Schema
        private static string FieldName = "PipeAnnotation_Collection";

        private static Schema Schema;
        private static Guid SchemaId = new Guid("720080CB-DA99-40DC-9415-E53F280AA1F1");
        private static string SchemaName = "PipeAnnotation";

        /// <summary>
        /// 取Schema
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static Schema GetSchema(Guid schemaId, string schemaName)
        {
            if (Schema == null)
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(schemaId);
                //SchemaName
                schemaBuilder.SetSchemaName(schemaName);
                //读权限 
                schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                //写权限
                schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
                //FieldName
                schemaBuilder.AddSimpleField(FieldName, typeof(string));
                Schema = schemaBuilder.Finish();
            }
            return Schema;
        }
        #endregion

        #region DataStorage
        private static DataStorage Storage;
        private static string StorageName = "PipeAnnotation";

        /// <summary>
        /// 获取DataStorage
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static DataStorage GetStorage(Document doc)
        {
            if (Storage == null)
            {
                Storage = GetStorageByName(doc, StorageName);
            }
            return Storage;
        }

        /// <summary>
        /// 创建DataStorage
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static DataStorage CreateStorage(Document doc, string name)
        {
            DataStorage st = DataStorage.Create(doc);
            st.Name = name;
            return st;
        }

        /// <summary>
        /// 获取或者无时创建DataStorage
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static DataStorage GetStorageByName(Document doc, string name)
        {
            FilteredElementCollector eleCollector = new FilteredElementCollector(doc).OfClass(typeof(DataStorage));
            foreach (DataStorage dt in eleCollector)
            {
                if (dt.Name == name)
                {
                    return dt;
                }
            }
            return null;
        }

        /// <summary>
        /// 删除对应名称的DataStorage
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name"></param>
        public static void RemoveStorageByName(Document doc, string name)
        {
            var storage = GetStorageByName(doc, name);
            if (storage != null)
                doc.Delete(storage.Id);
        }
        #endregion

        #region Collection
        private static PipeAnnotationEntityCollection Collection;

        /// <summary>
        /// 取数据Collection
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static PipeAnnotationEntityCollection GetCollection(Document doc)
        {
            ////TEST
            //RemoveStorageByName(doc, StorageName);

            if (Collection != null)
                return Collection;

            var schema = GetSchema(SchemaId, SchemaName);
            var storage = GetStorage(doc);
            Entity entity;
            if (storage == null)
            {
                storage = CreateStorage(doc, StorageName);
                entity = new Entity(schema);
                entity.Set<string>(schema.GetField(FieldName), "");
                storage.SetEntity(entity);
            }
            entity = storage.GetEntity(schema);
            string data = entity.Get<string>(schema.GetField(FieldName));
            Collection = new PipeAnnotationEntityCollection(data);
            return Collection;
        }

        /// <summary>
        /// 保存Collection
        /// </summary>
        /// <param name="doc"></param>
        public static void SaveCollection(Document doc)
        {
            var data = Collection.ToData();
            var storage = GetStorage(doc);
            var schema = GetSchema(SchemaId, SchemaName);
            Entity entity = storage.GetEntity(schema);
            entity.Set<string>(schema.GetField(FieldName), data);
            storage.SetEntity(entity);
        }
        #endregion
    }
    #endregion
}
