﻿using MyRevit.SubsidenceMonitor.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.SubsidenceMonitor.Entities
{
    public partial class TNode : IPDMTBase
    {

        #region Properties
        /// <summary>
        /// 监测类型
        /// </summary>
        public EIssueType IssueType { get; set; }
        /// <summary>
        /// 监测时间
        /// </summary>
        public DateTime IssueDateTime { get; set; }
        /// <summary>
        /// 测点编号
        /// </summary>
        public String NodeCode { get; set; }
        /// <summary>
        /// 测点数据
        /// </summary>
        public String Data { get; set; }
        /// <summary>
        /// 测点构件
        /// </summary>
        public String ElementIds { get; set; }
        /// <summary>
        /// 顺序号
        /// </summary>
        public Int16 Index { get; set; }
        #endregion

        #region Constructors
        public TNode()
        {
        }
        public TNode(EIssueType issueType, DateTime issueDateTime, String nodeCode)
        {
            IssueType = issueType;
            IssueDateTime = issueDateTime;
            NodeCode = nodeCode;
        }
        public TNode(IDataReader reader) : base(reader)
        {
        }
        #endregion

        #region Methods
        public override void Init(IDataReader reader)
        {
            this.IssueType = (EIssueType)Enum.Parse(typeof(EIssueType), reader[nameof(this.IssueType)].ToString());
            this.IssueDateTime = Convert.ToDateTime(reader[nameof(this.IssueDateTime)]);
            this.NodeCode = Convert.ToString(reader[nameof(this.NodeCode)]);
            this.Data = Convert.ToString(reader[nameof(this.Data)]);
            this.ElementIds = Convert.ToString(reader[nameof(this.ElementIds)]);
            this.Index = Convert.ToInt16(reader[nameof(this.Index)]);
        }
        public override void Init(IDataReader reader, List<string> fields)
        {
            if (fields.Contains(nameof(IssueType)))
            {
                this.IssueType = (EIssueType)Enum.Parse(typeof(EIssueType), reader[nameof(this.IssueType)].ToString());
            }
            if (fields.Contains(nameof(IssueDateTime)))
            {
                this.IssueDateTime = Convert.ToDateTime(reader[nameof(this.IssueDateTime)]);
            }
            if (fields.Contains(nameof(NodeCode)))
            {
                this.NodeCode = Convert.ToString(reader[nameof(this.NodeCode)]);
            }
            if (fields.Contains(nameof(Data)))
            {
                this.Data = Convert.ToString(reader[nameof(this.Data)]);
            }
            if (fields.Contains(nameof(ElementIds)))
            {
                this.ElementIds = Convert.ToString(reader[nameof(this.ElementIds)]);
            }
            if (fields.Contains(nameof(Index)))
            {
                this.Index = Convert.ToInt16(reader[nameof(this.Index)]);
            }
        }
        public override string TableName
        {
            get
            {
                return nameof(TNode);
            }
        }
        #endregion

        #region Manual
        #endregion
    }
}
