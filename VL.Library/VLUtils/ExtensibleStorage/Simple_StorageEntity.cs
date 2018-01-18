using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace VL.Library
{
    /// <summary>
    /// 事务Helper
    /// </summary>
    public class VLDelegateHelper
    {
        /// <summary>
        ///  Try Catch 流程模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static void DelegateTryCatch(Action func)
        {
            try
            {
                func();
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }
        /// <summary>
        ///  Try Catch 流程模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static bool DelegateTryCatch(Func<bool> func)
        {
            try
            {
                func();
                return true;
            }
            catch (Exception ex)
            {
                Log(ex);
                return false;
            }
        }
        /// <summary>
        ///  Try Catch 流程模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static T DelegateTryCatch<T>(Func<T> func, Func<T> onError)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                Log(ex);
            }
            try
            {
                return onError();
            }
            catch (Exception ex)
            {
                Log(ex);
            }
            return default(T);
        }

        /// <summary>
        /// Try Catch 流程模板
        /// </summary>
        /// <param name="action"></param>
        /// <param name="onError"></param>
        public static void DelegateTryCatch(Action action, Action onError)
        {
            try
            {
                action();
                return;
            }
            catch (Exception ex)
            {
                Log(ex);
            }
            try
            {
                onError();
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }

        /// <summary>
        /// 异常记录,有待优化
        /// </summary>
        /// <param name="ex"></param>
        private static void Log(Exception ex)
        {
            //LogClass.GetInstance().AddLog(ex);
        }
    }

    /// <summary>
    /// 简易的支持Collection和Setting 的方案
    /// </summary>
    /// <typeparam name="TCollection"></typeparam>
    /// <typeparam name="TSetting"></typeparam>
    public abstract class Collection_StorageEntity<TCollection> : IExtensibleStorageEntity
        where TCollection : VLSerializable, new()
    {
        public abstract string StorageName { get; }
        public abstract string SchemaName { get; }
        public abstract Guid SchemaId { get; }
        public abstract string FieldOfCollection { get; }
        public abstract string FieldOfSetting { get; }
        public List<string> FieldNames { get { return new List<string>() { FieldOfCollection, FieldOfSetting }; } }

        #region Collection
        TCollection _collection;
        /// <summary>
        /// 取数据Collection
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public TCollection GetCollection(Document doc)
        {
            if (_collection != null)
                return _collection;
            _collection = new TCollection();
            VLDelegateHelper.DelegateTryCatch(
                () =>
                {
                    string data = ExtensibleStorageHelper.GetData(doc, this, this.FieldOfCollection);
                    _collection.LoadData(data);
                }
            );
            return _collection;
        }
        /// <summary>
        /// 保存Collection
        /// </summary>
        /// <param name="doc"></param>
        public void SaveCollection(Document doc, TCollection collection)
        {
            _collection = collection;
            var data = _collection.ToData();
            VLDelegateHelper.DelegateTryCatch(
               () =>
               {
                   ExtensibleStorageHelper.SetData(doc, this, this.FieldOfCollection, data);
               },
               () =>
               {
                   ExtensibleStorageHelper.RemoveStorage(doc, this);
                   ExtensibleStorageHelper.SetData(doc, this, this.FieldOfCollection, data);
               }
           );
        }
        #endregion
    }
    /// <summary>
    /// 简易的支持Collection和Setting 的方案
    /// 可以参考 PipeAttributesAnnotation 项目中的运用
    /// </summary>
    /// <typeparam name="TCollection"></typeparam>
    /// <typeparam name="TSetting"></typeparam>
    public abstract class CollectionAndSetting_StorageEntity<TCollection, TSetting> : IExtensibleStorageEntity
        where TCollection : VLSerializable, new()
        where TSetting : VLSerializable, new()
    {
        public abstract string StorageName { get; }
        public abstract string SchemaName { get; }
        public abstract Guid SchemaId { get; }
        public abstract string FieldOfCollection { get; }
        public abstract string FieldOfSetting { get; }
        public List<string> FieldNames { get { return new List<string>() { FieldOfCollection, FieldOfSetting }; } }

        #region Collection
        TCollection _collection;
        /// <summary>
        /// 取数据Collection
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public TCollection GetCollection(Document doc)
        {
            if (_collection != null)
                return _collection;
            _collection = new TCollection();
            VLDelegateHelper.DelegateTryCatch(
                () =>
                {
                    string data = ExtensibleStorageHelper.GetData(doc, this, this.FieldOfCollection);
                    _collection.LoadData(data);
                }
            );
            return _collection;
        }
        /// <summary>
        /// 保存Collection
        /// </summary>
        /// <param name="doc"></param>
        public void SaveCollection(Document doc, TCollection collection)
        {
            _collection = collection;
            var data = _collection.ToData();
            VLDelegateHelper.DelegateTryCatch(
               () =>
               {
                   ExtensibleStorageHelper.SetData(doc, this, this.FieldOfCollection, data);
               },
               () =>
               {
                   ExtensibleStorageHelper.RemoveStorage(doc, this);
                   ExtensibleStorageHelper.SetData(doc, this, this.FieldOfCollection, data);
               }
           );
        }
        #endregion

        #region Setting
        TSetting _setting;
        /// <summary>
        /// 取数据Setting
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public TSetting GetSetting(Document doc)
        {
            if (_setting != null)
                return _setting;
            _setting = new TSetting();
            VLDelegateHelper.DelegateTryCatch(
                () =>
                {
                    string data = ExtensibleStorageHelper.GetData(doc, this, this.FieldOfSetting);
                    _setting.LoadData(data);
                }
            );
            return _setting;
        }
        /// <summary>
        /// 保存Setting
        /// </summary>
        /// <param name="doc"></param>
        public void SaveSetting(Document doc, TSetting setting)
        {
            _setting = setting;
            var data = _setting.ToData();
            VLDelegateHelper.DelegateTryCatch(
               () =>
               {
                   ExtensibleStorageHelper.SetData(doc, this, this.FieldOfSetting, data);
               },
               () =>
               {
                   ExtensibleStorageHelper.RemoveStorage(doc, this);
                   ExtensibleStorageHelper.SetData(doc, this, this.FieldOfSetting, data);
               }
           );
        }
        #endregion
    }
}
