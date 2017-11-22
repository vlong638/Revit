using MyRevit.Utilities;
using System;
using System.Collections.Generic;

namespace MyRevit.MyTests.PAA
{
    /// <summary>
    /// PipeAnnotationEntityCollection扩展存储对象
    /// </summary>
    public class PAAStorageEntity : IExtensibleStorageEntity
    {
        public List<string> FieldNames { get { return new List<string>() { FieldOfData, FieldOfSetting }; } }
        public Guid SchemaId { get { return new Guid("4A622209-267D-4BA7-BFFD-55C3CDA0F809"); } }
        public string StorageName { get { return "PAA_Schema"; } }
        public string SchemaName { get { return "PAA_Schema"; } }
        public string FieldOfData { get { return "PAA_Collection"; } }
        public string FieldOfSetting { get { return "PAA_Settings"; } }
    }
}
