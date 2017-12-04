using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.Utilities;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace MyRevit.Entities
{
    /// <summary>
    /// 扩展存储对象
    /// </summary>
    class TestStorageEntity : IExtensibleStorageEntity
    {
        public string FieldStr = "TestStr";
        public string FieldInt = "TestInt";
        public List<string> FieldNames
        {
            get
            {
                return new List<string>() { FieldStr, FieldInt };
            }
        }
        public List<string> FieldNames2
        {
            get
            {
                return new List<string>() { FieldStr, FieldInt, "额外的字段" };
            }
        }


        public Guid SchemaId
        {
            get
            {
                return new Guid("666DFF02-4D85-4A1D-B5AE-903E721AE8ED");
            }
        }

        public string SchemaName
        {
            get
            {
                return nameof(TestStorageEntity) +"SchemaV2";
            }
        }

        public string StorageName
        {
            get
            {
                return nameof(TestStorageEntity);
            }
        }
    }

    ///// <summary>
    ///// 扩展存储的存储对象接口
    ///// 注!!!Schema及Field创建后不能修改
    ///// 需删除Storage后,且并未加载同名Schema(已加载旧版本会导致新版本创建失败)
    ///// 故StorageEntity更新后原有数据会丢失
    ///// 优点是,数据可以与模型保持一致,不会因为模型的回退而导致数据不一致
    ///// </summary>
    //public interface IExtensibleStorageEntity
    //{
    //    string StorageName { get; }
    //    string SchemaName { get; }
    //    Guid SchemaId { get; }
    //    List<string> FieldNames { get; }
    //}

    /// <summary>
    /// 扩展存储辅助
    /// </summary>
    public static class ExtensibleStorageHelperV2
    {
        #region 基本操作
        /// <summary>
        /// 取Schema
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static Schema GetSchema(Guid schemaId, string schemaName, IEnumerable<string> fieldNames)
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
        public static DataStorage GetOrCreateStorage(Document doc, string storageName)
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
        public static DataStorage CreateStorage(Document doc, string storageName)
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
        public static DataStorage GetStorageByName(Document doc, string storageName)
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
            try
            {
                return getData(doc, storageEntity, dataField);
            }
            catch (Exception ex)
            {
                RemoveStorage(doc, storageEntity);
                return getData(doc, storageEntity, dataField);
            }
        }

        private static string getData(Document doc, IExtensibleStorageEntity storageEntity, string dataField)
        {
            var schema = GetSchema(storageEntity.SchemaId, storageEntity.SchemaName, storageEntity.FieldNames);
            var storage = GetOrCreateStorage(doc, storageEntity.StorageName);
            Entity entity = storage.GetEntity(schema);
            if (!entity.IsValid())
            {
                return "";
            }
            return entity.Get<string>(schema.GetField(dataField));
        }
        /// <summary>
        /// 设置storageEntity对应dataField的值
        /// </summary>
        public static void SetData(Document doc, IExtensibleStorageEntity storageEntity, string dataField, string data)
        {
            try
            {
                setData(doc, storageEntity, dataField, data);
            }
            catch (Exception ex)
            {
                RemoveStorage(doc, storageEntity);
                setData(doc, storageEntity, dataField, data);
            }
        }

        private static void setData(Document doc, IExtensibleStorageEntity storageEntity, string dataField, string data)
        {
            var storage = GetOrCreateStorage(doc, storageEntity.StorageName);
            var schema = GetSchema(storageEntity.SchemaId, storageEntity.SchemaName, storageEntity.FieldNames);
            Entity entity = new Entity(schema);
            entity.Set<string>(schema.GetField(dataField), data);
            storage.SetEntity(entity);
        }

        #region dataFields
        /// <summary>
        /// 通过storageEntity获取对应dataField的值
        /// </summary>
        public static string GetData(Document doc, IExtensibleStorageEntity storageEntity, List<string> dataFields, string dataField)
        {
            try
            {
                return getData(doc, storageEntity, dataFields, dataField);
            }
            catch (Exception ex)
            {
                RemoveStorage(doc, storageEntity);
                return getData(doc, storageEntity, dataField);
            }
        }

        private static string getData(Document doc, IExtensibleStorageEntity storageEntity, List<string> dataFields, string dataField)
        {
            var schema = GetSchema(storageEntity.SchemaId, storageEntity.SchemaName, dataFields);
            var storage = GetOrCreateStorage(doc, storageEntity.StorageName);
            Entity entity = storage.GetEntity(schema);
            if (!entity.IsValid())
            {
                return "";
            }
            return entity.Get<string>(schema.GetField(dataField));
        }
        /// <summary>
        /// 设置storageEntity对应dataField的值
        /// </summary>
        public static void SetData(Document doc, IExtensibleStorageEntity storageEntity, List<string> dataFields, string dataField, string data)
        {
            try
            {
                setData(doc, storageEntity, dataFields, dataField, data);
            }
            catch (Exception ex)
            {
                RemoveStorage(doc, storageEntity);
                setData(doc, storageEntity, dataFields, dataField, data);
            }
        }

        private static void setData(Document doc, IExtensibleStorageEntity storageEntity, List<string> dataFields, string dataField, string data)
        {
            var storage = GetOrCreateStorage(doc, storageEntity.StorageName);
            var schema = GetSchema(storageEntity.SchemaId, storageEntity.SchemaName, dataFields);
            Entity entity = new Entity(schema);
            entity.Set<string>(schema.GetField(dataField), data);
            storage.SetEntity(entity);
        }
        #endregion
        #endregion
    }

    [Transaction(TransactionMode.Manual)]
    public class StorageConsistencyCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;
            var view = doc.ActiveView;
            VLTransactionHelper.DelegateTransaction(doc, "扩展存储", () =>
            {
                var storageEntity = new TestStorageEntity();
                var data = ExtensibleStorageHelperV2.GetData(doc, storageEntity, storageEntity.FieldStr);
                TaskDialogShow(data);
                return true;
            });
            return Result.Succeeded;
        }

        private static void TaskDialogShow(string message)
        {
            TaskDialog.Show("提示", message);
        }
    }

    /// <summary>
    /// 验证扩展存储和模型的联动
    /// 即模型被回退,扩展存储一致回退
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class StorageConsistencyCommand2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;
            var view = doc.ActiveView;
            var storageEntity = new TestStorageEntity();
            var d1 = ExtensibleStorageHelperV2.GetData(doc, storageEntity, storageEntity.FieldStr);
            VLTransactionHelper.DelegateTransaction(doc, "扩展存储", () =>
            {
                ExtensibleStorageHelperV2.SetData(doc, storageEntity, storageEntity.FieldStr, "666");
                var element = doc.Delete(new ElementId(197387));
                return true;
            });
            var d2 = ExtensibleStorageHelperV2.GetData(doc, storageEntity, storageEntity.FieldStr);
            VLTransactionHelper.DelegateTransaction(doc, "扩展存储", () =>
            {
                ExtensibleStorageHelperV2.SetData(doc, storageEntity, storageEntity.FieldStr, "777");
                return true;
            });
            var d3 = ExtensibleStorageHelperV2.GetData(doc, storageEntity, storageEntity.FieldStr);
            return Result.Succeeded;
        }

        private static void TaskDialogShow(string message)
        {
            TaskDialog.Show("提示", message);
        }
    }

    /// <summary>
    /// 验证扩展存储变更情况下的机制
    /// 即模型Schema变更,扩展存储
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class StorageConsistencyCommand3 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;
            var view = doc.ActiveView;
            var storageEntity = new TestStorageEntity();
            //var s = "S1";
            ////事务1+事务2 是整体回退还是回退事务2?
            ////回退是事务型回退,事务2被回退了 Data=S1
            //TransactionHelper.DelegateTransaction(doc, "扩展存储", () =>
            //{
            //    var d1 = ExtensibleStorageHelperV2.GetData(doc, storageEntity, storageEntity.FieldStr);
            //    ExtensibleStorageHelperV2.SetData(doc, storageEntity, storageEntity.FieldStr, s);
            //    return true;
            //});
            VLTransactionHelper.DelegateTransaction(doc, "扩展存储", () =>
            {
                //var d2 = ExtensibleStorageHelperV2.GetData(doc, storageEntity, storageEntity.FieldStr);
                //var d2 = ExtensibleStorageHelperV2.GetData(doc, storageEntity, storageEntity.FieldNames2, storageEntity.FieldStr);
                ExtensibleStorageHelperV2.SetData(doc, storageEntity, storageEntity.FieldStr, "123");



                ////s += "S2";
                ////ExtensibleStorageHelperV2.SetData(doc, storageEntity, storageEntity.FieldStr, s);
                //var schema = ExtensibleStorageHelperV2.GetSchema(storageEntity.SchemaId, storageEntity.SchemaName, storageEntity.FieldNames2);
                //var storage = ExtensibleStorageHelperV2.GetOrCreateStorage(doc, storageEntity.StorageName);
                //Entity entity;
                //if (storage == null)
                //{
                //    storage = ExtensibleStorageHelperV2.CreateStorage(doc, storageEntity.StorageName);
                //    entity = new Entity(schema);
                //    entity.Set<string>(schema.GetField(storageEntity.FieldStr), "");
                //    storage.SetEntity(entity);
                //}
                //entity = storage.GetEntity(schema);
                //if (entity.IsValid())
                //{
                //    var data = entity.Get<string>(schema.GetField(storageEntity.FieldStr));
                //}
                return true;
            });
            var d3 = ExtensibleStorageHelperV2.GetData(doc, storageEntity, storageEntity.FieldStr);
            return Result.Succeeded;
        }

        private static void TaskDialogShow(string message)
        {
            TaskDialog.Show("提示", message);
        }
    }
}
