using MyRevit.Utilities;
using System;
using System.Collections.Generic;

namespace MyRevit.MyTests.Template
{

    /// <summary>
    /// PipeAnnotationEntityCollection扩展存储对象
    /// </summary>
    public class TemplateStorageEntity : IExtensibleStorageEntity
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
        public string StorageName { get { return "Template_Schema"; } }
        public string SchemaName { get { return "Template_Schema"; } }
        public string FieldOfData { get { return "Template_Collection"; } }
        public string FieldOfSetting { get { return "Template_Settings"; } }
    }
}
