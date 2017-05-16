using MyRevit.SubsidenceMonitor.Entities;
using MyRevit.SubsidenceMonitor.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MyRevit.SubsidenceMonitor.Entities
{
    public partial class TDepthNode : IPDMTBase
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
        /// 测点深度
        /// </summary>
        public String Depth { get; set; }
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
        public TDepthNode()
        {
        }
        public TDepthNode(EIssueType issueType, DateTime issueDateTime, String nodeCode, String depth)
        {
            IssueType = issueType;
            IssueDateTime = issueDateTime;
            NodeCode = nodeCode;
            Depth = depth;
        }
        public TDepthNode(IDataReader reader) : base(reader)
        {
        }
        #endregion

        #region Methods
        public override void Init(IDataReader reader)
        {
            this.IssueType = (EIssueType)Enum.Parse(typeof(EIssueType), reader[nameof(this.IssueType)].ToString());
            this.IssueDateTime = Convert.ToDateTime(reader[nameof(this.IssueDateTime)]);
            this.NodeCode = Convert.ToString(reader[nameof(this.NodeCode)]);
            this.Depth = Convert.ToString(reader[nameof(this.Depth)]);
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
            if (fields.Contains(nameof(Depth)))
            {
                this.Depth = Convert.ToString(reader[nameof(this.Depth)]);
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
                return nameof(TDepthNode);
            }
        }
        #endregion

        #region Manual
        public static string ElementIds_IntSplitter = ",";
        List<int> _ElementIds_Int;
        public List<int> ElementIds_Int
        {
            set
            {
                _ElementIds_Int = value;
            }
            get
            {
                if (_ElementIds_Int == null)
                {
                    _ElementIds_Int = new List<int>();
                    if (!string.IsNullOrEmpty(ElementIds))
                        _ElementIds_Int = ElementIds.Split(',').Select(c => Convert.ToInt32(c)).ToList();
                }
                return _ElementIds_Int;
            }
        }
        public string GetElementIds()
        {
            return string.Join(ElementIds_IntSplitter, ElementIds_Int);
        }
        #endregion
    }
}
