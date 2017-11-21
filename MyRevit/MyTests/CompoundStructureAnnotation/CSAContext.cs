using Autodesk.Revit.DB;
using MyRevit.Entities;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    /// <summary>
    /// PipeAnnotationEntityCollection扩展存储对象
    /// </summary>
    class CollectionStorageEntity : IExtensibleStorageEntity
    {
        public List<string> FieldNames { get { return new List<string>() { FieldOfData, FieldOfSetting }; } }
        public Guid SchemaId { get { return new Guid("5E0549F8-1F10-4388-A0B5-2FAA6884E81B"); } }
        public string StorageName { get { return "CompoundStructureAnnotation_Schema"; } }
        public string SchemaName { get { return "CompoundStructureAnnotation_Schema"; } }
        public string FieldOfData { get { return "CompoundStructureAnnotation_Collection"; } }
        public string FieldOfSetting { get { return "CompoundStructureAnnotation_Settings"; } }
    }

    class CSAContext
    {
        #region Creator
        private static CSACreater _Creator = null;
        public static CSACreater Creator
        {
            get
            {
                return _Creator ?? (_Creator = new CSACreater());
            }
        }
        #endregion

        #region Storage
        public static bool IsEditing;
        private static CSAModelCollection Collection;
        private static CollectionStorageEntity CStorageEntity = new CollectionStorageEntity();
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
        public static bool Save(Document doc)
        {
            if (Collection == null)
                return false;
            var data = Collection.ToData();
            return DelegateHelper.DelegateTryCatch(
                () =>
                {
                    ExtensibleStorageHelper.SetData(doc, CStorageEntity, CStorageEntity.FieldOfData, data);
                    return true;
                },
                () =>
                {
                    ExtensibleStorageHelper.RemoveStorage(doc, CStorageEntity);
                    ExtensibleStorageHelper.SetData(doc, CStorageEntity, CStorageEntity.FieldOfData, data);
                    return false;
                }
            );
        } 
        #endregion
    }
}
