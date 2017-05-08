using System;
using System.Windows.Forms;
using WinformTests.SubsidenceMonitor.Controls;
using System.Collections.Generic;
using WinformTests.SubsidenceMonitor.Entity;
//using Autodesk.Revit.DB;

namespace WinformTests
{
    /// <summary>
    /// 几个注意点
    /// 传list和details,把关联关系的构件职责收束,确保关联关系
    /// CurrentDetailIndex可能为0,0表示TList下无TDetail
    /// MemorialDetail用于有数据时的提交和取消处理
    /// </summary>
    public partial class SubsidenceMonitorForm : System.Windows.Forms.Form
    {
        #region 初始化和主要参数
        Object Doc { set; get; }
        TList List { set; get; }
        List<TDetail> Details { set; get; }
        int CurrentDetailIndex { set; get; }
        MemorableDetail _memorableDetail;
        MemorableDetail MemorableDetail
        {
            set
            {
                if (value != null && !value.Data.IsLoad)
                    value.Data.LoadData();
                _memorableDetail = value;
                _memorableDetail.Start();
            }
            get
            {
                return _memorableDetail;
            }
        }
        bool IsCreateNew { set; get; }//创建新项
        bool IsChanged { set; get; }//数据更改

        public SubsidenceMonitorForm(object doc, TList list, List<TDetail> details) : base()
        {
            InitializeComponent();

            //TODO TEST 临时创建数据表
            SubsidenceMonitor.Utilities.SQLiteHelper.CheckAndCreateTables();

            //更新主要参数
            Doc = doc;
            List = list;
            Details = details;
            int currentDetailIndex;
            if (Details == null || Details.Count == 0)
            {
                Details = new List<TDetail>();
                currentDetailIndex = -1;
                IsCreateNew = true;
            }
            else
            {
                //关联关系维护
                foreach (var detail in Details)
                {
                    detail.List = List;
                }
                currentDetailIndex = 0;
                IsCreateNew = false;
            }
            //初始化控件配置
            InitControls(details);
            //如果有详情则加载详情数据
            ChangeCurrentDetail(currentDetailIndex);
        }

        private void InitByObject(TDetail detail)
        {
            tb_ReportName.Text = detail.ReportName;//报告名称
            tb_IssueType.Text = detail.IssueType.ToString();//沉降类型
            //tb_WarnArgs.Text = true;//TODO 报警值
            tb_Contractor.Text = detail.Contractor;//承包单位
            tb_Supervisor.Text = detail.Supervisor;//监理单位
            tb_Monitor.Text = detail.Monitor;//监测单位
            tb_Date.Text = detail.IssueDateTime.ToString("yyyy.MM.dd");//监测日期
            var endTime = detail.IssueDateTime.AddMinutes(detail.IssueTimeRange);
            var timeFormat = "hh:mm";
            tb_Time.Text = $"{detail.IssueDateTime.ToString(timeFormat)}-{endTime.ToString(timeFormat)}";//监测时间
            tb_InstrumentName.Text = detail.InstrumentName;//仪器名称
            tb_InstrumentCode.Text = detail.InstrumentCode;//仪器编号
            //DGV
            var normalHeight = 20;
            List<TNode> leftNodes = new List<TNode>();
            List<TNode> rightNodes = new List<TNode>();
            if (detail.Nodes.Count <= normalHeight)
            {
                leftNodes.AddRange(detail.Nodes);
            }
            else
            {
                var height = normalHeight;
                if (detail.Nodes.Count > normalHeight * 2)
                    height = detail.Nodes.Count / 2;
                for (int i = 0; i < detail.Nodes.Count; i++)
                {
                    if (i < height)
                    {
                        leftNodes.Add(detail.Nodes[i]);
                    }
                    else
                    {
                        rightNodes.Add(detail.Nodes[i]);
                    }
                }
            }
            dgv_left.DataSource = null;
            dgv_left.DataSource = leftNodes;
            dgv_right.DataSource = null;
            dgv_right.DataSource = rightNodes;
        }
        private void InitControls(List<TDetail> details)
        {
            //内容不可改
            tb_ReportName.ReadOnly = true;//报告名称
            tb_IssueType.ReadOnly = true;//沉降类型
            tb_WarnArgs.ReadOnly = true;//报警值
            tb_Contractor.ReadOnly = true;//承包单位
            tb_Supervisor.ReadOnly = true;//监理单位
            tb_Monitor.ReadOnly = true;//监测单位
            tb_Date.ReadOnly = true;//监测日期
            tb_Time.ReadOnly = true;//监测时间
            tb_InstrumentName.ReadOnly = true;//仪器名称
            tb_InstrumentCode.ReadOnly = true;//仪器编号
            //录入Excel_按钮_文本
            btn_LoadExcel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            btn_LoadExcel.Text = $"录入{Environment.NewLine}Excel";
            //dgv 总宽479-42=437/5=87.4 420 17的Scroll空间
            var headerNodes = new List<HeaderNode>()
            {
                new HeaderNode(0,46,84,"测点编号",nameof(BuildingSubsidenceDataV1.NodeCode)),
                new HeaderNode(1,23,168,"沉降变化量(mm)",null,
                new List<HeaderNode>() {
                     new HeaderNode(-1,23,84,"本次变量",nameof(BuildingSubsidenceDataV1.CurrentChanges)),
                     new HeaderNode(-1,23,84,"累计变量",nameof(BuildingSubsidenceDataV1.SumChanges)),
                }),
                new HeaderNode(3,46,84,$"围护结构施工{System.Environment.NewLine}期间累计值（mm）",nameof(BuildingSubsidenceDataV1.SumPeriodBuildingEnvelope)),
                new HeaderNode(4,46,84,"总累计值（mm）",nameof(BuildingSubsidenceDataV1.SumBuildingEnvelope)),
            };
            dgv_left.HeaderNodes = headerNodes;
            dgv_left.ScrollBars = ScrollBars.Vertical;
            dgv_right.HeaderNodes = headerNodes;
            dgv_right.ScrollBars = ScrollBars.Vertical;

            if (Details == null || Details.Count == 0)
                btn_Delete.Enabled = false;
        }
        #endregion

        #region 提交操作
        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Submit_Click(object sender, EventArgs e)
        {
            if (MemorableDetail == null && CurrentDetailIndex == -1)//当前无选中Detail,无新增
            {
                this.Close();
            }
            else if (MemorableDetail == null && CurrentDetailIndex == 0)//当前无选中Detail,新增
            {
                var detail = Details[CurrentDetailIndex];
                var result = WriteFacade.DataCommit(detail.List, detail, detail.Nodes);
                if (result.IsSuccess)
                    MessageBox.Show("操作成功");
                else
                    MessageBox.Show(result.Message);
            }
            else if (MemorableDetail!=null)//当前选中某Detail
            {
                var result = MemorableDetail.Commit();
                if (result.IsSuccess)
                    MessageBox.Show("操作成功");
                else
                    MessageBox.Show(result.Message);
            }
        }
        /// <summary>
        /// 取消
        /// </summary>
        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            if (IsChanged)
                MemorableDetail.Rollback();
        }
        /// <summary>
        /// 新建
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Create_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 删除
        /// </summary>
        private void btn_Delete_Click(object sender, EventArgs e)
        {
            if (MemorableDetail == null)//删除只删已有的选中项Detail
            {
                MessageBox.Show("当前无详情数据,不能删除");
            }
            else
            {
                var result = MemorableDetail.Delete();
                Details.Remove(MemorableDetail.Data);
                


                //TODO 多项时候删除当前项 换至下一项,无时创建新项
                //CurrentDetail.Data
            }
        }
        /// <summary>
        /// 下一份
        /// </summary>
        private void btn_Next(object sender, EventArgs e)
        {
            if (Details.Count > 1 && CurrentDetailIndex != Details.Count - 1)
            {
                ChangeCurrentDetail(CurrentDetailIndex + 1);
            }
        }
        /// <summary>
        /// 上一份
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Previous(object sender, EventArgs e)
        {
            if (Details.Count > 1 && CurrentDetailIndex != 0)
            {
                ChangeCurrentDetail(CurrentDetailIndex - 1);
            }
        }
        #endregion

        #region 编辑操作
        /// <summary>
        /// 更改选中的Detail
        /// </summary>
        /// <param name="currentDetailIndex"></param>
        private void ChangeCurrentDetail(int currentDetailIndex)
        {



            CurrentDetailIndex = currentDetailIndex;
            if (CurrentDetailIndex != -1)//-1为无Details
            {
                //当前文档的存储业务模型,构件存储业务模型时会加载详情数据
                MemorableDetail = new MemorableDetail(Doc, Details[CurrentDetailIndex]);
                //加载对象信息
                InitByObject(MemorableDetail.Data);
            }
        }
        /// <summary>
        /// 导入Excel数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_LoadExcel_Click(object sender, EventArgs e)
        {
            ////选择文件路径
            //string path;
            //OpenFileDialog dialog = new OpenFileDialog();
            //dialog.Filter = "PowerDesigner PDM 文件(*.pdm)|*.pdm";
            //if (dialog.ShowDialog() == DialogResult.OK)
            //{
            //    path= dialog.FileName;
            //}
            //else
            //{
            //    return;
            //}
            //TODO TEST 快速定位文件
            string path = @"E:\WorkingSpace\Tasks\0417土方\锦寓路站沉降3-30 - 副本.xls";
            try
            {
                if (CurrentDetailIndex == -1)
                {
                    var detail = new TDetail() { IssueType = List.IssueType, List = List };
                    detail.IssueType.GetEntity().ParseInto(path, detail);
                    Details.Add(detail);
                    CurrentDetailIndex = Details.Count - 1;//无选中项时可能有Details,当前页面的新增项总是在末尾,在重新打开页面后按照时间先后顺序排序
                }
                else
                {
                    //TODO 稍微有点担心 MemorialDetail的内容不会更新,使用引用传参应该会更新(验证一下)
                    var detail = Details[CurrentDetailIndex];
                    detail.IssueType.GetEntity().ParseInto(path, detail);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载Excel数据失败,错误内容:" + ex.Message);
                return;
            }
            //加载的数据渲染到控件
            InitByObject(MemorableDetail.Data);
        }
        private void tb_ReportName_TextChanged(object sender, EventArgs e)
        {

        }
        #endregion
    }
}
