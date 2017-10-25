using Autodesk.Revit.DB;
using MyRevit.Entities;
using PmSoft.Optimization.DrawingProduction;
using PmSoft.Optimization.DrawingProduction.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    public abstract class ModelBase<T>
    {
        public static string PropertyInnerSplitter = "_";
        public static char PropertyInnerSplitter_Char = '_';
        public static string PropertyInnerSplitter2 = "&";
        public static char PropertyInnerSplitter2_Char = '&';
        public static string PropertySplitter = "~";
        public static char PropertySplitter_Char = '~';

        public ModelBase(string data)
        {
            FromData(data);
        }

        public abstract void FromData(string data);
        public abstract string ToData();
    }

    public abstract class CollectionBase<T>
    {
        public static string EntitySplitter = ";";
        public static char EntitySplitter_Char = ';';
        public List<T> Data = new List<T>();

        public CollectionBase(string data)
        {
            Data = FromData(data);
        }

        public abstract List<T> FromData(string data);
        public abstract string ToData();
    }

    /// <summary>
    /// 存储对象集合
    /// </summary>
    public class CSAModelCollection:CollectionBase<CSAModel>
    {
        public CSAModelCollection(string data) : base(data)
        {
        }

        public override List<CSAModel> FromData(string dataStr)
        {
            List<CSAModel> models = new List<CSAModel>();
            if (string.IsNullOrEmpty(dataStr))
                return models;
            var entityStrs = dataStr.Split(EntitySplitter_Char);
            foreach (var entityStr in entityStrs)
            {
                if (string.IsNullOrEmpty(entityStr))
                    continue;
                models.Add(new CSAModel(entityStr));
            }
            return models;
        }

        public override string ToData()
        {
            return string.Join(EntitySplitter, Data.Select(c => c.ToData()));
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="doc"></param>
        public void Save(Document doc)
        {
            CompoundStructureAnnotationContext.SaveCollection(doc);
        }
    }


    /// <summary>
    /// PipeAnnotationEntityCollection扩展存储对象
    /// </summary>
    class CollectionStorageEntity : PmSoft.Optimization.DrawingProduction.IExtensibleStorageEntity
    {
        public List<string> FieldNames { get { return new List<string>() { FieldOfData, FieldOfSetting }; } }
        public Guid SchemaId { get { return new Guid("5E0549F8-1F10-4388-A0B5-2FAA6884E81B"); } }
        public string StorageName { get { return "CompoundStructureAnnotation_Schema"; } }
        public string SchemaName { get { return "CompoundStructureAnnotation_Schema"; } }
        public string FieldOfData { get { return "CompoundStructureAnnotation_Collection"; } }
        public string FieldOfSetting { get { return "CompoundStructureAnnotation_Settings"; } }
    }

    class CompoundStructureAnnotationContext
    {
        public static bool IsEditing;
        private static CSAModelCollection Collection;
        private static CollectionStorageEntity CStorageEntity = new CollectionStorageEntity();
        public static CSACreater Creater = new CSACreater();

        /// <summary>
        /// 取数据Collection
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static CSAModelCollection GetCollection(Document doc)
        {
            //if (Collection != null)
            //    return Collection;
            Collection = DelegateHelper.DelegateTryCatch(
                () =>
                {
                    string data = ExtensibleStorageHelper.GetData(doc, CStorageEntity, CStorageEntity.FieldOfData);
                    return new CSAModelCollection(data);
                },
                () =>
                {
                    return new CSAModelCollection("");
                }
            );
            return Collection;
        }

        /// <summary>
        /// 保存Collection
        /// </summary>
        /// <param name="doc"></param>
        public static void SaveCollection(Document doc, bool isSecondTry = false)
        {
            if (Collection == null)
                return;
            var data = Collection.ToData();
            DelegateHelper.DelegateTryCatch(
               () =>
               {
                   ExtensibleStorageHelper.SetData(doc, CStorageEntity, CStorageEntity.FieldOfData, data);
               },
               () =>
               {
                   ExtensibleStorageHelper.RemoveStorage(doc, CStorageEntity);
                   ExtensibleStorageHelper.SetData(doc, CStorageEntity, CStorageEntity.FieldOfData, data);
               }
           );
        }
    }
}
