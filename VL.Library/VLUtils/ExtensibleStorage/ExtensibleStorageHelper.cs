using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;
using System.Collections.Generic;

namespace VL.Library
{
    /// <summary>
    /// 扩展存储辅助
    /// </summary>
    public static class ExtensibleStorageHelper
    {
        #region 基本操作
        /// <summary>
        /// 取Schema
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static Schema GetSchema(Guid schemaId, string schemaName, IEnumerable<string> fieldNames)
        {
            SchemaBuilder schemaBuilder = new SchemaBuilder(schemaId);
            //SchemaName
            schemaBuilder.SetSchemaName(schemaName);
            //读权限 
            schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
            //写权限
            schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
            //FieldName
            foreach (var fieldName in fieldNames)
                schemaBuilder.AddSimpleField(fieldName, typeof(string));
            return schemaBuilder.Finish();
        }

        /// <summary>
        /// 获取DataStorage
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static DataStorage GetOrCreateStorage(Document doc, string storageName)
        {
            var storage = GetStorageByName(doc, storageName);
            if (storage == null)
            {
                storage = CreateStorage(doc, storageName);
            }
            return storage;
        }

        /// <summary>
        /// 创建DataStorage
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="storageName"></param>
        /// <returns></returns>
        private static DataStorage CreateStorage(Document doc, string storageName)
        {
            DataStorage st = DataStorage.Create(doc);
            st.Name = storageName;
            return st;
        }

        /// <summary>
        /// 获取或者无时创建DataStorage
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="storageName"></param>
        /// <returns></returns>
        private static DataStorage GetStorageByName(Document doc, string storageName)
        {
            FilteredElementCollector eleCollector = new FilteredElementCollector(doc).OfClass(typeof(DataStorage));
            foreach (DataStorage dt in eleCollector)
            {
                if (dt.Name == storageName)
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
        /// <param name="storageName"></param>
        public static void RemoveStorage(Document doc, IExtensibleStorageEntity storageEntity)
        {
            var storage = GetStorageByName(doc, storageEntity.StorageName);
            if (storage != null)
                doc.Delete(storage.Id);
        }
        #endregion

        #region 封装后接口
        /// <summary>
        /// 通过storageEntity获取对应dataField的值
        /// </summary>
        public static string GetData(Document doc, IExtensibleStorageEntity storageEntity, string dataField)
        {
            var schema = GetSchema(storageEntity.SchemaId, storageEntity.SchemaName, storageEntity.FieldNames);
            Entity entity;
            var storage = GetStorageByName(doc, storageEntity.StorageName);
            if (storage == null)
            {
                storage = CreateStorage(doc, storageEntity.StorageName);
                entity = new Entity(schema);
                entity.Set<string>(schema.GetField(dataField), "");
                storage.SetEntity(entity);
            }
            entity = storage.GetEntity(schema);
            if (entity.IsValid())
                return entity.Get<string>(schema.GetField(dataField));
            else
            {
                RemoveStorage(doc, storageEntity);
                return GetData(doc, storageEntity, dataField);
            }
        }

        /// <summary>
        /// 设置storageEntity对应dataField的值
        /// </summary>
        public static void SetData(Document doc, IExtensibleStorageEntity storageEntity, string dataField, string data)
        {
            var storage = GetStorageByName(doc, storageEntity.StorageName);
            var schema = GetSchema(storageEntity.SchemaId, storageEntity.SchemaName, storageEntity.FieldNames);
            Entity entity = storage.GetEntity(schema);
            if (storage == null)
            {
                storage = CreateStorage(doc, storageEntity.StorageName);
                entity = new Entity(schema);
                entity.Set<string>(schema.GetField(dataField), data);
                storage.SetEntity(entity);
            }
            else
            {
                if (entity.IsValid())
                {
                    entity.Set<string>(schema.GetField(dataField), data);
                    storage.SetEntity(entity);
                }
                else
                {
                    entity = new Entity(schema);
                    entity.Set<string>(schema.GetField(dataField), data);
                }
            }
        } 
        #endregion
    }
}
