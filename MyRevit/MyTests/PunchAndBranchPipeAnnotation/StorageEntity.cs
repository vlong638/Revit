using MyRevit.Utilities;
using System;
using System.Collections.Generic;

namespace MyRevit.MyTests.PBPA
{

    /// <summary>
    /// PipeAnnotationEntityCollection扩展存储对象
    /// </summary>
    public class PBPAStorageEntity : IExtensibleStorageEntity
    {
        public List<string> FieldNames { get { return new List<string>() { FieldOfData, FieldOfSetting }; } }
        public Guid SchemaId
        {
            get
            {
                return new Guid("72C9F88D-D84D-4F90-B4D8-A0016E1E30E7");
            }
        }
        public string StorageName { get { return "PBPA_Schema"; } }
        public string SchemaName { get { return "PBPA_Schema"; } }
        public string FieldOfData { get { return "PBPA_Collection"; } }
        public string FieldOfSetting { get { return "PBPA_Settings"; } }
    }
}
