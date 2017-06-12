using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.Utilities
{    /// <summary>
     /// 用于我们的element对象上的扩展存储的添加删除、修改，可存储 普通参数、elmentid、二进制、等等数据
     /// <remarks>庄峰毅 2016.05.30</remarks>
     /// </summary>
    public class SchemaEntityOpr
    {
        private Autodesk.Revit.DB.ExtensibleStorage.Schema m_sch = null;
        private Autodesk.Revit.DB.ExtensibleStorage.Entity m_entity = null;

        /// <summary>
        /// 创建基于该表结构的ent
        /// </summary>
        /// <returns></returns>
        static public Entity CreateEntity()
        {
            if (SchemaOpr.Instance().IsValid())
            {
                Entity ent = new Entity(SchemaOpr.Instance().schema);
                return ent;
            }

            //
            return null;
        }

        public SchemaEntityOpr(Entity ent)
        {
            if (null != ent && ent.Schema.GUID == SchemaOpr.Instance().schema.GUID)
            {
                m_sch = ent.Schema;
                m_entity = ent;
            }
        }

        public SchemaEntityOpr(Element ele)
        {
            if (false == GetEntityFrom(ele))
            {
                m_sch = SchemaOpr.Instance().schema;
                m_entity = new Entity(m_sch);
            }
        }

        public bool IsValid()
        {
            return (null == m_entity || null == m_sch) ? false : true;
        }

        /// <summary>
        /// 从element获取扩展存储
        /// </summary>
        /// <param name="ele"></param>
        /// <returns></returns>
        public bool GetEntityFrom(Element ele)
        {
            Entity ent = ele.GetEntity(SchemaOpr.Instance().schema);
            if (ent.IsValid())
            {
                m_sch = ent.Schema;
                m_entity = ent;
                return true;
            }

            //
            return false;
        }

        /// <summary>
        /// 添加扩展存储到element
        /// </summary>
        /// <param name="ele"></param>
        /// <returns></returns>
        public bool SaveTo(Element ele)
        {
            try
            {
                if (IsValid())
                {

                    ele.SetEntity(m_entity);

                }
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }


        }

        /// <summary>
        /// 添加或者修改参数
        /// </summary>
        /// <param name="sParmName"></param>
        /// <param name="sParmValue"></param>
        public void SetParm(string sParmName, string sParmValue)
        {
            if (IsValid() == false)
            {
                return;
            }
            Field field = m_sch.GetField("parms_str");

            //读取原有map内容
            IDictionary<string, string> mapParm = m_entity.Get<IDictionary<string, string>>(field);
            if (null == mapParm)
                mapParm = new Dictionary<string, string>();

            //设置或添加新schema
            mapParm[sParmName] = sParmValue;

            //添加修改后的数据
            m_entity.Set<IDictionary<string, string>>(field, mapParm);
        }

        /// <summary>
        /// 移除参数
        /// </summary>
        /// <param name="sParmName"></param>
        /// <param name="sParmValue"></param>
        public void RemoveParm(string sParmName)
        {
            if (IsValid() == false)
            {
                return;
            }
            Field field = m_sch.GetField("parms_str");

            //读取原有map内容
            IDictionary<string, string> mapParm = m_entity.Get<IDictionary<string, string>>(field);
            if (null == mapParm)
            {
                return;
            }

            //设置或添加新schema
            mapParm.Remove(sParmName);

            //添加修改后的数据
            m_entity.Set<IDictionary<string, string>>(field, mapParm);
        }

        /// <summary>
        /// 获取参数
        /// </summary>
        /// <param name="sParmName"></param>
        /// <param name="sParmValue"></param>
        /// <returns></returns>
        public bool GetParm(string sParmName, ref string sParmValue)
        {
            if (IsValid() == false)
            {
                return false;
            }
            Field field = m_sch.GetField("parms_str");

            //读取原有map内容
            IDictionary<string, string> mapParm = m_entity.Get<IDictionary<string, string>>(field);
            if (null == mapParm)
                return false;

            //设置或添加新schema
            bool bOk = mapParm.TryGetValue(sParmName, out sParmValue);
            return bOk;
        }

        /// <summary>
        /// 获取参数
        /// </summary>
        /// <param name="sParmName"></param>
        /// <param name="sParmValue"></param>
        /// <returns></returns>
        public string GetParm(string sParmName)
        {
            if (IsValid() == false)
            {
                return "";
            }
            Field field = m_sch.GetField("parms_str");

            //读取原有map内容
            IDictionary<string, string> mapParm = m_entity.Get<IDictionary<string, string>>(field);
            if (null == mapParm)
                return "";

            //设置或添加新schema
            string sParmValue = "";
            mapParm.TryGetValue(sParmName, out sParmValue);
            return sParmValue;
        }

        //         public void AddElement(string sParmName, ElementId id)
        //         {
        //             
        //         }
        // 
        //         ElementId GetElement(string sParmName)
        //         {
        //             Autodesk.Revit.DB.ExtensibleStorage.Schema sch = Schema;
        //             ElementId eleValue = sch.Get<ElementId>(sch.GetField("parms_element"));      
        //      
        //             //
        //             return eleValue; 
        //         }

        /// <summary>
        /// 判断表名是否存在
        /// </summary>
        /// <param name="sTableName"></param>
        /// <returns></returns>
        public bool IsTableExist(string sTableName)
        {
            if (IsValid() == false)
            {
                return false;
            }
            Field field = m_sch.GetField("tables_name");

            //读取原有map内容
            IList<string> listParm = m_entity.Get<IList<string>>(field);
            if (null == listParm)
                return false;

            //设置或添加新schema
            int idx = listParm.IndexOf(sTableName);
            if (-1 == idx)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 添加或修改二进制数据，如果表数据长度为0，则删除本表
        /// </summary>
        /// <param name="sTableName"></param>
        /// <param name="pValue"></param>
        public void SetTableData(string sTableName, byte[] pValue)
        {
            if (IsValid() == false)
            {
                return;
            }
            Field fieldTableName = m_sch.GetField("tables_name");

            //读取原有map内容
            IList<string> listParm = m_entity.Get<IList<string>>(fieldTableName);
            if (null == listParm)
                listParm = new List<string>();
            //
            Field fieldTablePosition = m_sch.GetField("tables_postion");
            IList<int> listPosition = m_entity.Get<IList<int>>(fieldTablePosition);
            if (null == listPosition)
                listPosition = new List<int>();
            //
            Field fieldTableContent = m_sch.GetField("tables_content");
            IList<byte> listContent = m_entity.Get<IList<byte>>(fieldTableContent);
            if (null == listContent)
            {
                listContent = new List<byte>();
            }

            //
            if (pValue.Length == 0)
            {
                RemoveTable(sTableName);
                return;
            }

            //设置或添加新schema
            int idx = listParm.IndexOf(sTableName);
            if (-1 == idx)
            {
                //不存在表，则新建表
                listParm.Add(sTableName);

                //添加终止位置
                int nLen = pValue.Length;
                if (listPosition.Count == 0)
                    listPosition.Add(nLen - 1);   //记录终止位置
                else
                {
                    int iPos = listPosition.Last();
                    int iPosEndNew = iPos + nLen;
                    listPosition.Add(iPosEndNew);
                }

                //添加元素
                ((List<byte>)listContent).AddRange(pValue);
            }
            else
            {
                //已存在表，则修改表的内容
                int iPosCur = listPosition[idx];
                int iPosBegin = 0;
                if (0 == idx)
                    iPosBegin = 0;
                else
                    iPosBegin = listPosition[idx - 1] + 1;

                //
                int nLenDesOld = iPosCur - iPosBegin + 1;
                int nLenSrc = pValue.Length;

                //新的终止位置
                int iPosEndNew = iPosBegin + nLenSrc - 1;

                //修改/添加元素
                int iSizeOld = listContent.Count;
                for (int i = iPosBegin, j = 0; j < nLenSrc; i++, j++)
                {
                    if (j < nLenDesOld)
                        listContent[i] = pValue[j];
                    else
                        listContent.Insert(i, pValue[j]);
                }

                //重置下面所有元素的终止位置
                int nSubLen = nLenSrc - nLenDesOld;
                for (int i = idx; i < listPosition.Count; i++)
                {
                    listPosition[i] = listPosition[i] + nSubLen;
                }
            }

            //设置回entiy
            m_entity.Set<IList<string>>(fieldTableName, listParm);
            m_entity.Set<IList<int>>(fieldTablePosition, listPosition);
            m_entity.Set<IList<byte>>(fieldTableContent, listContent);
        }

        /// <summary>
        /// 获取二进制数据
        /// </summary>
        /// <param name="sTableName"></param>
        /// <param name="resultData"></param>
        /// <returns></returns>
        public bool GetTableData(string sTableName, out byte[] resultData)
        {
            if (IsValid() == false)
            {
                resultData = null;
                return false;
            }
            Field fieldTableName = m_sch.GetField("tables_name");

            //读取原有map内容
            IList<string> listParm = m_entity.Get<IList<string>>(fieldTableName);

            //设置或添加新schema
            int idx = listParm.IndexOf(sTableName);
            if (-1 == idx)
            {
                resultData = null;
                return false;
            }

            //获取内容终止位置
            IList<int> listPosition = m_entity.Get<IList<int>>(m_sch.GetField("tables_postion"));
            int iPosEnd = listPosition[idx];

            //获取所有内容
            Field fieldContent = m_sch.GetField("tables_content");
            IList<byte> listContent = m_entity.Get<IList<byte>>(fieldContent);

            //获取对应位置数值
            int iPosBegin = 0;
            if (0 == idx)
                iPosBegin = 0;
            else
            {
                iPosBegin = listPosition[idx - 1] + 1;
            }

            //拷贝数据
            long iLen = iPosEnd - iPosBegin + 1;
            resultData = new byte[iLen];
            Array.Copy(listContent.ToArray(), iPosBegin, resultData, 0, iLen);

            //
            return true;
        }

        /// <summary>
        /// 移除表
        /// </summary>
        /// <param name="sTableName"></param>
        public void RemoveTable(string sTableName)
        {
            if (IsValid() == false)
            {
                return;
            }
            Field fieldTableName = m_sch.GetField("tables_name");

            //读取原有map内容
            IList<string> listParm = m_entity.Get<IList<string>>(fieldTableName);
            if (null == listParm)
                listParm = new List<string>();
            //
            Field fieldTablePosition = m_sch.GetField("tables_postion");
            IList<int> listPosition = m_entity.Get<IList<int>>(fieldTablePosition);
            if (null == listPosition)
                listPosition = new List<int>();

            //
            Field fieldTableContent = m_sch.GetField("tables_content");
            IList<byte> listContent = m_entity.Get<IList<byte>>(fieldTableContent);
            if (null == listContent)
            {
                listContent = new List<byte>();
            }

            //设置或添加新schema
            int idx = listParm.IndexOf(sTableName);
            if (-1 == idx)
            {
                //不存在表
                return;
            }
            else
            {
                listParm.RemoveAt(idx);

                //已有位置计算
                int iPosCur = listPosition[idx];
                int iPosBegin = 0;
                if (0 == idx)
                    iPosBegin = 0;
                else
                    iPosBegin = listPosition[idx - 1] + 1;

                //
                int nLenDesOld = iPosCur - iPosBegin + 1;

                //修改/添加元素
                int iSizeOld = listContent.Count;
                for (int i = iPosCur; i >= iPosBegin; i--)
                {
                    listContent.RemoveAt(i);
                }

                //重置下面所有元素的终止位置
                for (int i = idx; i < listPosition.Count; i++)
                {
                    listPosition[i] -= nLenDesOld;
                }
                listPosition.RemoveAt(idx);

                //设置回entiy
                m_entity.Set<IList<string>>(fieldTableName, listParm);
                m_entity.Set<IList<int>>(fieldTablePosition, listPosition);
                m_entity.Set<IList<byte>>(fieldTableContent, listContent);
            }
        }

        /// <summary> 
        /// 将一个object对象序列化，返回一个byte[]         
        /// </summary> 
        /// <param name="obj">能序列化的对象</param>         
        /// <returns></returns> 
        public static byte[] ObjectToBytes(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {

                IFormatter formatter = new BinaryFormatter();
                formatter.Binder = new UBinder();
                formatter.Serialize(ms, obj);
                return ms.GetBuffer();
            }
        }

        /// <summary> 
        /// 将一个序列化后的byte[]数组还原         
        /// </summary>
        /// <param name="Bytes"></param>         
        /// <returns></returns> 
        public static object BytesToObject(byte[] Bytes)
        {
            using (MemoryStream ms = new MemoryStream(Bytes))
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Binder = new UBinder();
                return formatter.Deserialize(ms);
            }
        }
    }

    /// <summary>
    /// 用于我们的实例对象上的扩展存储，可存储 普通参数、elmentid、二进制、等等数据
    /// <remarks>庄峰毅 2016.05.30</remarks>
    /// </summary>
    public class SchemaOpr
    {
        public static SchemaOpr Instance()
        {
            if (null == m_opr)
            {
                m_opr = new SchemaOpr();
            }
            return m_opr;
        }

        /// <summary>
        /// 有效性判断
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            if (null == m_schema)
                return false;
            return m_schema.IsValidObject;
        }

        /// <summary>
        /// 获取表结构
        /// </summary>
        /// <returns></returns>
        public Schema schema
        {
            get { return m_schema; }
        }


        /// 添加字段
        /// <summary>
        ///  构造函数，创建schema 表
        /// </summary>
        private SchemaOpr()
        {
            //创建dictionary方式的schema
            System.Guid guid = GetCurGuid();
            SchemaBuilder builder = new SchemaBuilder(guid);

            // Set name to this schema builder  

            builder.SetSchemaName("PmSchemaComn");
            builder.SetDocumentation("品茗通用的数据扩展结构");

            // Set read and write access levels  

            builder.SetReadAccessLevel(AccessLevel.Public);
            builder.SetWriteAccessLevel(AccessLevel.Public);

            // 版本号，升级用
            builder.AddSimpleField("versionNum", typeof(short));

            // string元素
            builder.AddMapField("parms_str", typeof(string), typeof(string));

            // double元素
            FieldBuilder fb = builder.AddMapField("parms_double", typeof(string), typeof(double));
            fb.SetUnitType(UnitType.UT_Length);

            // element元素
            builder.AddMapField("parms_element", typeof(string), typeof(ElementId));

            // 表格元素
            builder.AddArrayField("tables_name", typeof(string));         //段名称
            builder.AddArrayField("tables_postion", typeof(int));      //段的二进制位置
            builder.AddArrayField("tables_content", typeof(byte));        //段的内容

            // 
            m_schema = builder.Finish();
        }

        /// <summary>
        /// 获取当前模块的guid
        /// </summary>
        /// <returns></returns>
        Guid GetCurGuid()
        {
            System.Reflection.Assembly ass = System.Reflection.Assembly.GetExecutingAssembly();
            object[] attrs = ass.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
            Guid id = new Guid(((System.Runtime.InteropServices.GuidAttribute)attrs[0]).Value);
            return id;
        }

        //
        static private SchemaOpr m_opr = null;
        private static Schema m_schema = null;
    }

    /// <summary>
    /// 用于创建storage对象
    /// <remarks>庄峰毅 2016.05.30</remarks>
    /// </summary>
    public class StorageOpr
    {
        /// <summary>
        /// 创建storage
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="sName"></param>
        /// <returns></returns>
        static public DataStorage CreateStorage(Document doc, String sName)
        {
            DataStorage st = DataStorage.Create(doc);
            st.Name = sName;
            return st;
        }

        /// <summary>
        /// 获取第一个storage
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        static public DataStorage GetFirstStorage(Document doc)
        {
            Element ele = new FilteredElementCollector(doc).OfClass(typeof(DataStorage)).FirstElement();
            DataStorage dt = ele as DataStorage;
            return dt;
        }

        /// <summary>
        /// 获取指定名称的storage
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="sName"></param>
        /// <returns></returns>
        static public DataStorage GetStorage(Document doc, String sName)
        {
            FilteredElementCollector eleCollector = new FilteredElementCollector(doc).OfClass(typeof(DataStorage));
            foreach (DataStorage dt in eleCollector)
            {
                if (dt.Name == sName)
                {
                    return dt;
                }
            }
            return null;
        }


    }


    internal class UBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type typeToDeserialize = null;
            typeToDeserialize = Type.GetType(String.Format("{0},{1}", typeName, assemblyName));
            return typeToDeserialize;
        }
    }
}
