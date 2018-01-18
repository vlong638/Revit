using System;
using System.Collections.Generic;

namespace VL.Library
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
}
