using PmSoft.Common.RevitClass.VLUtils;
using System;
using System.Collections.Generic;

namespace PMSoft.ConstructionManagementV2
{

    /// <summary>
    /// PipeAnnotationEntityCollection扩展存储对象
    /// </summary>
    public class CMStorageEntity : IExtensibleStorageEntity
    {
        public List<string> FieldNames { get { return new List<string>() { FieldOfData, FieldOfSetting }; } }
        public Guid SchemaId
        {
            get
            {
                throw new NotImplementedException("项目更改SchemaId需要更换");
                //return new Guid("4A622209-267D-4BA7-BFFD-55C3CDA0F809");
            }
        }
        public string StorageName { get { return "CM_Schema"; } }
        public string SchemaName { get { return "CM_Schema"; } }
        public string FieldOfData { get { return "CM_Collection"; } }
        public string FieldOfSetting { get { return "CM_Settings"; } }
    }
}
