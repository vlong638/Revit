using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using MyRevit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.Utilities
{

    /// <summary>
    /// 扩展存储的存储对象接口
    /// 注!!!Schema及Field创建后不能修改
    /// 需删除Storage后,且并未加载同名Schema(已加载旧版本会导致新版本创建失败)
    /// 故StorageEntity更新后原有数据会丢失
    /// 优点是,数据可以与模型保持一致,不会因为模型的回退而导致数据不一致
    /// </summary>
    public interface IExtensibleStorageEntity
    {
        string StorageName { get; }
        string SchemaName { get; }
        Guid SchemaId { get; }
        List<string> FieldNames { get; }
    }

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
            {
                schemaBuilder.AddSimpleField(fieldName, typeof(string));
            }
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

    //class ExtensibleStorageHelperV1
    //{
    //    //public static void Test(Document doc)
    //    //{
    //    //    XYZ dataToStore = null;
    //    //    var storageName = "MyTest";
    //    //    RemoveStorage(doc, storageName);
    //    //    var storage = CreateStorage(doc, storageName);
    //    //    storage = GetOrCreateStorage(doc, storageName);

    //    //    #region AddSimpleField
    //    //    if (true)
    //    //    {
    //    //        SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("720080CB-DA99-40DC-9415-E53F280AA1F0"));
    //    //        //读权限 
    //    //        schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
    //    //        //写权限
    //    //        schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
    //    //        //Field
    //    //        var fieldName = "versionNum";
    //    //        schemaBuilder.AddSimpleField(fieldName, typeof(short));
    //    //        var fieldName2 = "businessName";
    //    //        schemaBuilder.AddSimpleField(fieldName2, typeof(string));
    //    //        //SchemaName
    //    //        var schemaName = "AddSimpleField";
    //    //        schemaBuilder.SetSchemaName(schemaName);
    //    //        var schema = schemaBuilder.Finish();
    //    //        //write
    //    //        Entity entityToWrite = new Entity(schema);
    //    //        Field valueField = schema.GetField(fieldName);
    //    //        entityToWrite.Set<short>(valueField, 11);
    //    //        valueField = schema.GetField(fieldName2);
    //    //        entityToWrite.Set<string>(valueField, "VL");
    //    //        storage.SetEntity(entityToWrite);
    //    //        //read
    //    //        Entity entityFromRead = storage.GetEntity(schema);
    //    //        short dataFromStore = entityFromRead.Get<short>(schema.GetField(fieldName));
    //    //        string businusessName = entityFromRead.Get<string>(schema.GetField(fieldName2));

    //    //        var storageEntity = new TestStorageEntity();
    //    //        var data = ExtensibleStorageHelperV2.GetData(doc, storageEntity, storageEntity.FieldStr);
    //    //    }
    //    //    if (true)
    //    //    {
    //    //        SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("720080CB-DA99-40DC-9415-E53F280AA1F0"));
    //    //        //读权限 
    //    //        schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
    //    //        //写权限
    //    //        schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
    //    //        //Field
    //    //        var fieldName = "versionNum";
    //    //        schemaBuilder.AddSimpleField(fieldName, typeof(short));
    //    //        var fieldName2 = "businessName2";
    //    //        schemaBuilder.AddSimpleField(fieldName2, typeof(string));
    //    //        //SchemaName
    //    //        var schemaName = "AddSimpleField";
    //    //        schemaBuilder.SetSchemaName(schemaName);
    //    //        var schema = schemaBuilder.Finish();
    //    //        //write
    //    //        Entity entityToWrite = new Entity(schema);
    //    //        Field valueField = schema.GetField(fieldName);
    //    //        entityToWrite.Set<short>(valueField, 11);
    //    //        valueField = schema.GetField(fieldName2);
    //    //        entityToWrite.Set<string>(valueField, "VL");
    //    //        storage.SetEntity(entityToWrite);
    //    //        //read
    //    //        Entity entityFromRead = storage.GetEntity(schema);
    //    //        short dataFromStore = entityFromRead.Get<short>(schema.GetField(fieldName));
    //    //        string businusessName = entityFromRead.Get<string>(schema.GetField(fieldName2));
    //    //    }
    //    //    if (true)
    //    //    {
    //    //        SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("720080CB-DA99-40DC-9415-E53F280AA1F0"));
    //    //        //读权限 
    //    //        schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
    //    //        //写权限
    //    //        schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
    //    //        //AddSimpleField
    //    //        var fieldName = "versionNum";
    //    //        schemaBuilder.AddSimpleField(fieldName, typeof(short));
    //    //        var fieldName2 = "businessName";
    //    //        schemaBuilder.AddSimpleField(fieldName2, typeof(string));
    //    //        var schemaName = "AddSimpleField";
    //    //        schemaBuilder.SetSchemaName(schemaName);
    //    //        var schema = schemaBuilder.Finish();
    //    //        var entity = storage.GetEntity(schema);
    //    //        short dataFromStore = entity.Get<short>(schema.GetField(fieldName));
    //    //        schema.Dispose();
    //    //    }
    //    //    #endregion

    //    //    #region AddMapField
    //    //    if (true)
    //    //    {
    //    //        SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("720080CB-DA99-40DC-9415-E53F280AA1F1"));
    //    //        //读权限 
    //    //        schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
    //    //        //写权限
    //    //        schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
    //    //        //AddMapField
    //    //        var fieldName = "parms_str";
    //    //        schemaBuilder.AddMapField(fieldName, typeof(string), typeof(string));
    //    //        var schemaName = "AddMapField";
    //    //        schemaBuilder.SetSchemaName(schemaName);
    //    //        var schema = schemaBuilder.Finish();
    //    //        //write
    //    //        Entity entityToWrite = new Entity(schema);
    //    //        Field valueField = schema.GetField(fieldName);
    //    //        entityToWrite.Set<IDictionary<string, string>>(valueField, new Dictionary<string, string>() { { "A", "64" } });
    //    //        storage.SetEntity(entityToWrite);
    //    //        //read
    //    //        Entity entityFromRead = storage.GetEntity(schema);
    //    //        IDictionary<string, string> dataFromStore = entityFromRead.Get<IDictionary<string, string>>(schema.GetField(fieldName));
    //    //    }
    //    //    #endregion

    //    //    #region AddArrayField
    //    //    if (true)
    //    //    {
    //    //        SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("720080CB-DA99-40DC-9415-E53F280AA1F2"));
    //    //        //读权限 
    //    //        schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
    //    //        //写权限
    //    //        schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
    //    //        //AddArrayField
    //    //        var fieldName = "tables_name";
    //    //        schemaBuilder.AddArrayField(fieldName, typeof(string));
    //    //        var schemaName = "AddArrayField";
    //    //        schemaBuilder.SetSchemaName(schemaName);
    //    //        var schema = schemaBuilder.Finish();
    //    //        //write
    //    //        Entity entityToWrite = new Entity(schema);
    //    //        Field valueField = schema.GetField(fieldName);
    //    //        entityToWrite.Set<IList<string>>(valueField, new List<string>() { "A" });
    //    //        storage.SetEntity(entityToWrite);
    //    //        //read
    //    //        Entity entityFromRead = storage.GetEntity(schema);
    //    //        IList<string> dataFromStore = entityFromRead.Get<IList<string>>(schema.GetField(fieldName));
    //    //    }
    //    //    #endregion

    //    //    #region 定制类型 XYZ
    //    //    if (true)
    //    //    {
    //    //        SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("720080CB-DA99-40DC-9415-E53F280AA1F0"));
    //    //        //读权限 
    //    //        schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
    //    //        //写权限
    //    //        schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
    //    //        //Names
    //    //        var name = "MyExtendData";//FieldName,SchemaName
    //    //                                  //FieldBuilder
    //    //        FieldBuilder fieldBuilder = schemaBuilder.AddSimpleField(name, typeof(XYZ));
    //    //        fieldBuilder.SetUnitType(UnitType.UT_Length);
    //    //        fieldBuilder.SetDocumentation("描述信息,说明扩展的数据的用途");
    //    //        //Write
    //    //        schemaBuilder.SetSchemaName(name);
    //    //        var schema = schemaBuilder.Finish();
    //    //        Entity entityToWrite = new Entity(schema);
    //    //        Field valueField = schema.GetField(name);
    //    //        entityToWrite.Set<XYZ>(valueField, dataToStore, DisplayUnitType.DUT_METERS);
    //    //        storage.SetEntity(entityToWrite);
    //    //        //Read
    //    //        Entity entityFromRead = storage.GetEntity(schema);
    //    //        XYZ dataFromStore = entityFromRead.Get<XYZ>(schema.GetField(name), DisplayUnitType.DUT_METERS);
    //    //    }
    //    //    #endregion
    //    //}

    //    #region DataStorage
    //    public static DataStorage CreateStorage(Document doc, string name)
    //    {
    //        DataStorage st = DataStorage.Create(doc);
    //        st.Name = name;
    //        return st;
    //    }
    //    public static DataStorage GetOrCreateStorage(Document doc, string name)
    //    {
    //        FilteredElementCollector eleCollector = new FilteredElementCollector(doc).OfClass(typeof(DataStorage));
    //        foreach (DataStorage dt in eleCollector)
    //        {
    //            if (dt.Name == name)
    //            {
    //                return dt;
    //            }
    //        }
    //        return CreateStorage(doc, name);
    //    }
    //    public static void RemoveStorage(Document doc, string name)
    //    {
    //        var storage = GetOrCreateStorage(doc, name);
    //        if (storage != null)
    //            doc.Delete(storage.Id);
    //    } 
    //    #endregion
    //}
}
