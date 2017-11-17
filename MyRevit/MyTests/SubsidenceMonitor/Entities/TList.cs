using MyRevit.SubsidenceMonitor.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace MyRevit.SubsidenceMonitor.Entities
{
    public partial class TList : IPDMTBase, IContainer<TDetail>
    {
        #region Properties
        /// <summary>
        /// 监测类型
        /// </summary>
        public EIssueType IssueType { get; set; }
        /// <summary>
        /// 监测日期
        /// </summary>
        public DateTime IssueDate { get; set; }
        /// <summary>
        /// 下级数据量
        /// </summary>
        public Int16 DataCount { get; set; }
        #endregion

        #region Constructors
        public TList()
        {
        }
        public TList(EIssueType issueType, DateTime issueDate)
        {
            IssueType = issueType;
            IssueDate = issueDate;
        }
        public TList(IDataReader reader) : base(reader)
        {
        }
        #endregion

        #region Methods
        public override void Init(IDataReader reader)
        {
            this.IssueType = (EIssueType)Enum.Parse(typeof(EIssueType), reader[nameof(this.IssueType)].ToString());
            this.IssueDate = Convert.ToDateTime(reader[nameof(this.IssueDate)]);
            this.DataCount = Convert.ToInt16(reader[nameof(this.DataCount)]);
        }
        public override void Init(IDataReader reader, List<string> fields)
        {
            if (fields.Contains(nameof(IssueType)))
            {
                this.IssueType = (EIssueType)Enum.Parse(typeof(EIssueType), reader[nameof(this.IssueType)].ToString());
            }
            if (fields.Contains(nameof(IssueDate)))
            {
                this.IssueDate = Convert.ToDateTime(reader[nameof(this.IssueDate)]);
            }
            if (fields.Contains(nameof(DataCount)))
            {
                this.DataCount = Convert.ToInt16(reader[nameof(this.DataCount)]);
            }
        }
        public override string TableName
        {
            get
            {
                return nameof(TList);
            }
        }
        #endregion

        #region Manual
        /// <summary>
        /// dgv.dgv_Imported
        /// </summary>
        public string dgv_Imported { get { return DataCount > 0 ? "已录入" : ""; } }
        /// <summary>
        /// dgv.dgv_Operation
        /// </summary>
        public string dgv_Operation { get { return DataCount > 0 ? "查看" : "新建"; } }
        /// <summary>
        /// 相关联的Detail
        /// </summary>
        public List<TDetail> Datas { get; set; } = new List<TDetail>();
        #endregion
    }
}
