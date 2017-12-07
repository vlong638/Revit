using MyRevit.Utilities;
using System;
using System.Collections.Generic;

namespace MyRevit.MyTests.Analysis
{
    /// <summary>
    /// PipeAnnotationEntityCollection扩展存储对象
    /// </summary>
    public class AnalysisStorageEntity : IExtensibleStorageEntity
    {
        public List<string> FieldNames { get { return new List<string>() { FieldOfData, FieldOfSetting }; } }
        public Guid SchemaId { get { return new Guid("2859BDA6-63C3-4AD7-9DB8-9AC2F346EE02"); } }
        public string StorageName { get { return "Analysis_Schema"; } }
        public string SchemaName { get { return "Analysis_Schema"; } }
        public string FieldOfData { get { return "Analysis_Collection"; } }
        public string FieldOfSetting { get { return "Analysis_Settings"; } }
    }
}
