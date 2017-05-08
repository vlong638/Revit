using MyRevit.SubsidenceMonitor.Interfaces;
using MyRevit.SubsidenceMonitor.Operators;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.SubsidenceMonitor.Entities
{
    public partial class TDetail : IPDMTBase, ILazyLoadData
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
        /// 监测时间跨度(分钟)
        /// </summary>
        public Int16 IssueTimeRange { get; set; }
        /// <summary>
        /// 报告名称
        /// </summary>
        public String ReportName { get; set; }
        /// <summary>
        /// 承包单位
        /// </summary>
        public String Contractor { get; set; }
        /// <summary>
        /// 监理单位
        /// </summary>
        public String Supervisor { get; set; }
        /// <summary>
        /// 监测单位
        /// </summary>
        public String Monitor { get; set; }
        /// <summary>
        /// 仪器名称
        /// </summary>
        public String InstrumentName { get; set; }
        /// <summary>
        /// 仪器编号
        /// </summary>
        public String InstrumentCode { get; set; }
        /// <summary>
        /// 接近预警值(颜色/透明度配置)
        /// </summary>
        public String CloseCTSettings { get; set; }
        /// <summary>
        /// 超出预警值(颜色/透明度配置)
        /// </summary>
        public String OverCTSettings { get; set; }
        #endregion

        #region Constructors
        public TDetail()
        {
        }
        public TDetail(EIssueType issueType, DateTime issueDateTime)
        {
            IssueType = issueType;
            IssueDateTime = issueDateTime;
        }
        public TDetail(IDataReader reader) : base(reader)
        {
        }
        #endregion

        #region Methods
        public override void Init(IDataReader reader)
        {
            this.IssueType = (EIssueType)Enum.Parse(typeof(EIssueType), reader[nameof(this.IssueType)].ToString());
            this.IssueDateTime = Convert.ToDateTime(reader[nameof(this.IssueDateTime)]);
            this.IssueTimeRange = Convert.ToInt16(reader[nameof(this.IssueTimeRange)]);
            this.ReportName = Convert.ToString(reader[nameof(this.ReportName)]);
            this.Contractor = Convert.ToString(reader[nameof(this.Contractor)]);
            this.Supervisor = Convert.ToString(reader[nameof(this.Supervisor)]);
            this.Monitor = Convert.ToString(reader[nameof(this.Monitor)]);
            this.InstrumentName = Convert.ToString(reader[nameof(this.InstrumentName)]);
            this.InstrumentCode = Convert.ToString(reader[nameof(this.InstrumentCode)]);
            this.CloseCTSettings = Convert.ToString(reader[nameof(this.CloseCTSettings)]);
            this.OverCTSettings = Convert.ToString(reader[nameof(this.OverCTSettings)]);
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
            if (fields.Contains(nameof(IssueTimeRange)))
            {
                this.IssueTimeRange = Convert.ToInt16(reader[nameof(this.IssueTimeRange)]);
            }
            if (fields.Contains(nameof(ReportName)))
            {
                this.ReportName = Convert.ToString(reader[nameof(this.ReportName)]);
            }
            if (fields.Contains(nameof(Contractor)))
            {
                this.Contractor = Convert.ToString(reader[nameof(this.Contractor)]);
            }
            if (fields.Contains(nameof(Supervisor)))
            {
                this.Supervisor = Convert.ToString(reader[nameof(this.Supervisor)]);
            }
            if (fields.Contains(nameof(Monitor)))
            {
                this.Monitor = Convert.ToString(reader[nameof(this.Monitor)]);
            }
            if (fields.Contains(nameof(InstrumentName)))
            {
                this.InstrumentName = Convert.ToString(reader[nameof(this.InstrumentName)]);
            }
            if (fields.Contains(nameof(InstrumentCode)))
            {
                this.InstrumentCode = Convert.ToString(reader[nameof(this.InstrumentCode)]);
            }
            if (fields.Contains(nameof(CloseCTSettings)))
            {
                this.CloseCTSettings = Convert.ToString(reader[nameof(this.CloseCTSettings)]);
            }
            if (fields.Contains(nameof(OverCTSettings)))
            {
                this.OverCTSettings = Convert.ToString(reader[nameof(this.OverCTSettings)]);
            }
        }
        public override string TableName
        {
            get
            {
                return nameof(TDetail);
            }
        }
        #endregion

        #region Manual
        /// <summary>
        /// 是否已加载(延迟加载机制)
        /// </summary>
        public bool IsLoad { set; get; }
        /// <summary>
        /// 加载数据
        /// </summary>
        public void LoadData()
        {
            ReadFacade.FetchNodes(this);
            IsLoad = true;
        }
        /// <summary>
        /// 相关联的List
        /// </summary>
        public TList List { set; get; }
        /// <summary>
        /// 相关联的Nodes
        /// </summary>
        public List<TNode> Nodes { get; set; } = new List<TNode>();
        #endregion
    }
}
