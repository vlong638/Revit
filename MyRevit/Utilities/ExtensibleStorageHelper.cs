using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.Utilities
{
    class ExtensibleStorageHelper
    {
        public static void Test(Document doc)
        {
            XYZ dataToStore = null;
            var storageName = "MyTest";
            var storage = CreateStorage(doc, storageName);
            storage = GetStorage(doc, storageName);

            #region AddSimpleField
            if (true)
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("720080CB-DA99-40DC-9415-E53F280AA1F0"));
                //读权限 
                schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                //写权限
                schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
                //AddSimpleField
                var fieldName = "versionNum";
                schemaBuilder.AddSimpleField(fieldName, typeof(short));
                var schemaName = "AddSimpleField";
                schemaBuilder.SetSchemaName(schemaName);
                var schema = schemaBuilder.Finish();
                //write
                Entity entityToWrite = new Entity(schema);
                Field valueField = schema.GetField(fieldName);
                entityToWrite.Set<short>(valueField, 11);
                storage.SetEntity(entityToWrite);
                //read
                Entity entityFromRead = storage.GetEntity(schema);
                short dataFromStore = entityFromRead.Get<short>(schema.GetField(fieldName));
            }
            if (true)
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("720080CB-DA99-40DC-9415-E53F280AA1F0"));
                //读权限 
                schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                //写权限
                schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
                //AddSimpleField
                var fieldName = "versionNum";
                schemaBuilder.AddSimpleField(fieldName, typeof(short));
                var schemaName = "AddSimpleField";
                schemaBuilder.SetSchemaName(schemaName);
                var schema = schemaBuilder.Finish();
                var entity = storage.GetEntity(schema);
                short dataFromStore = entity.Get<short>(schema.GetField(fieldName));
            }
            #endregion

            #region AddMapField
            if (true)
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("720080CB-DA99-40DC-9415-E53F280AA1F1"));
                //读权限 
                schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                //写权限
                schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
                //AddMapField
                var fieldName = "parms_str";
                schemaBuilder.AddMapField(fieldName, typeof(string), typeof(string));
                var schemaName = "AddMapField";
                schemaBuilder.SetSchemaName(schemaName);
                var schema = schemaBuilder.Finish();
                //write
                Entity entityToWrite = new Entity(schema);
                Field valueField = schema.GetField(fieldName);
                entityToWrite.Set<IDictionary<string, string>>(valueField, new Dictionary<string, string>() { { "A", "64" } });
                storage.SetEntity(entityToWrite);
                //read
                Entity entityFromRead = storage.GetEntity(schema);
                IDictionary<string, string> dataFromStore = entityFromRead.Get<IDictionary<string, string>>(schema.GetField(fieldName));
            }
            #endregion

            #region AddArrayField
            if (true)
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("720080CB-DA99-40DC-9415-E53F280AA1F2"));
                //读权限 
                schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                //写权限
                schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
                //AddArrayField
                var fieldName = "tables_name";
                schemaBuilder.AddArrayField(fieldName, typeof(string));
                var schemaName = "AddArrayField";
                schemaBuilder.SetSchemaName(schemaName);
                var schema = schemaBuilder.Finish();
                //write
                Entity entityToWrite = new Entity(schema);
                Field valueField = schema.GetField(fieldName);
                entityToWrite.Set<IList<string>>(valueField, new List<string>() { "A" });
                storage.SetEntity(entityToWrite);
                //read
                Entity entityFromRead = storage.GetEntity(schema);
                IList<string> dataFromStore = entityFromRead.Get<IList<string>>(schema.GetField(fieldName));
            }
            #endregion

            #region 定制类型 XYZ
            if (true)
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("720080CB-DA99-40DC-9415-E53F280AA1F0"));
                //读权限 
                schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                //写权限
                schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
                //Names
                var name = "MyExtendData";//FieldName,SchemaName
                                          //FieldBuilder
                FieldBuilder fieldBuilder = schemaBuilder.AddSimpleField(name, typeof(XYZ));
                fieldBuilder.SetUnitType(UnitType.UT_Length);
                fieldBuilder.SetDocumentation("描述信息,说明扩展的数据的用途");
                //Write
                schemaBuilder.SetSchemaName(name);
                var schema = schemaBuilder.Finish();
                Entity entityToWrite = new Entity(schema);
                Field valueField = schema.GetField(name);
                entityToWrite.Set<XYZ>(valueField, dataToStore, DisplayUnitType.DUT_METERS);
                storage.SetEntity(entityToWrite);
                //Read
                Entity entityFromRead = storage.GetEntity(schema);
                XYZ dataFromStore = entityFromRead.Get<XYZ>(schema.GetField(name), DisplayUnitType.DUT_METERS);
            }
            #endregion
        }

        public static DataStorage CreateStorage(Document doc, string name)
        {
            DataStorage st = DataStorage.Create(doc);
            st.Name = name;
            return st;
        }

        public static DataStorage GetStorage(Document doc, string name)
        {
            FilteredElementCollector eleCollector = new FilteredElementCollector(doc).OfClass(typeof(DataStorage));
            foreach (DataStorage dt in eleCollector)
            {
                if (dt.Name == name)
                {
                    return dt;
                }
            }
            return null;
        }

        public static void RemoveStorage(Document doc, string name)
        {
            var storage = GetStorage(doc, name);
            if (storage != null)
                doc.Delete(storage.Id);
        }
    }
}
