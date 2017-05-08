using System;
using System.Collections.Generic;
using System.Data;

namespace MyRevit.SubsidenceMonitor.Interfaces
{
    /// <summary>
    /// 表基类 
    /// 它定义了复合PDM模型生成的表 
    /// 需有TableName,Properties
    /// 同时他定义了基于会话IDbSession的操作基本规范
    /// </summary>
    public abstract class IPDMTBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public IPDMTBase()
        {
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="reader"></param>
        public IPDMTBase(IDataReader reader)
        {
            Init(reader);
        }

        /// <summary>
        /// 通过DataReader初始化数据
        /// </summary>
        /// <param name="reader"></param>
        public virtual void Init(IDataReader reader)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 通过DataReader初始化数据
        /// </summary>
        public virtual void Init(IDataReader reader, List<string> fields)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 获取表名
        /// </summary>
        public virtual string TableName { get; }
    }
}
