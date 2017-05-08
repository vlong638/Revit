using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MyRevit.SubsidenceMonitor.Entities;
using MyRevit.SubsidenceMonitor.Interfaces;
using MyRevit.SubsidenceMonitor.Controls;
using Microsoft.Office.Interop.Excel;
using Autodesk.Revit.DB;
//using Autodesk.Revit.DB;

namespace MyRevit.SubsidenceMonitor.UI
{
    public enum ShowDialogType
    {
        Idle,
        AddElements,
        DeleleElements,
    }

    /// <summary>
    /// 几个注意点
    /// 传list和details,把关联关系的构件职责收束,确保关联关系
    /// CurrentDetailIndex可能为0,0表示TList下无TDetail
    /// MemorialDetail用于有数据时的提交和取消处理
    /// </summary>
    public partial class SubsidenceMonitorForm : System.Windows.Forms.Form
    {
        #region 初始化和主要参数
        public SubsidenceMonitorForm(Document doc, TList list) : base()
        {
            InitializeComponent();

            //初始化控件配置
            InitControlSettings();
            ////更新主要参数
            Model = new MultipleSingleMemorableDetails();
            //如果有详情则加载详情数据
            Model.OnDataChanged += Model_OnDataChanged;
            Model.OnStateChanged += Model_OnStateChanged;
            Model.OnConfirmChangeCurrentWhileIsEdited += Model_OnChangeCurrentWhileIsEdited;
            Model.OnConfirmChangeCurrentWhileHasCreateNew += Model_OnChangeCurrentWhileHasCreateNew;
            Model.OnConfirmDelete += Model_OnConfirmDelete;
            //事件附加后再作数据的初始化,否则关联的信息无法在初始化的时候渲染出来
            Model.Init(doc, list);
            IssueTypeEntity = list.IssueType.GetEntity();
        }
        public List<ElementId> SelectedElementIds { set; get; } = new List<ElementId>();
        public ShowDialogType ShowDialogType { set; get; }
        MultipleSingleMemorableDetails Model { set; get; }
        EIssueTypeEntity IssueTypeEntity { set; get; }

        private bool Model_OnConfirmDelete()
        {
            throw new NotImplementedException();
        }
        private bool Model_OnChangeCurrentWhileHasCreateNew()
        {
            //TODO 确认处理
            MessageBox.Show("用户确定了");
            return true;
        }
        private bool Model_OnChangeCurrentWhileIsEdited()
        {
            //TODO 确认处理
            MessageBox.Show("用户确定了");
            return true;
        }

        private void Model_OnStateChanged(bool hasPrevious, bool hasNext, bool canCreateNew, bool canDelete, bool canSave)
        {
            btn_Previous.Enabled = hasPrevious;
            btn_Next.Enabled = hasNext;
            btn_CreateNew.Enabled = canCreateNew;
            btn_Delete.Enabled = canDelete;
            btn_Submit.Enabled = canSave;
            if (canDelete|| canSave)//可保存或可删除意味着编辑主体的存在
            {
                btn_AddComponent.Enabled = true;
                btn_DeleteComponent.Enabled = true;
                btn_RenderComponent.Enabled = true;
                btn_ViewSelection.Enabled = true;
                btn_ViewAll.Enabled = true;
                btn_ViewCurrentMax_Red.Enabled = true;
                btn_ViewCurrentMax_All.Enabled = true;
                btn_ViewSumMax_Red.Enabled = true;
                btn_ViewSumMax_All.Enabled = true;
                btn_CloseWarnSettings.Enabled = true;
                btn_ViewCloseWarn.Enabled = true;
                btn_OverWarnSettings.Enabled = true;
                btn_ViewOverWarn.Enabled = true;
            }
            else
            {
                btn_AddComponent.Enabled = false;
                btn_DeleteComponent.Enabled = false;
                btn_RenderComponent.Enabled = false;
                btn_ViewSelection.Enabled = false;
                btn_ViewAll.Enabled = false;
                btn_ViewCurrentMax_Red.Enabled = false;
                btn_ViewCurrentMax_All.Enabled = false;
                btn_ViewSumMax_Red.Enabled = false;
                btn_ViewSumMax_All.Enabled = false;
                btn_CloseWarnSettings.Enabled = false;
                btn_ViewCloseWarn.Enabled = false;
                btn_OverWarnSettings.Enabled = false;
                btn_ViewOverWarn.Enabled = false;
            }
        }
        private void InitControlSettings()
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
            //560-42-18-500/6 84 504 56 42+16 差不多? 保存还有多2左右 实际是 42的序列号+14的Scroll
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
                new HeaderNode(5,46,84,"备注",nameof(BuildingSubsidenceDataV1.Conment)),
            };
            dgv_left.HeaderNodes = headerNodes;
            dgv_left.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            dgv_right.HeaderNodes = headerNodes;
            dgv_right.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        }
        private void Model_OnDataChanged(TDetail detail)
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
            dgv_left.DataSource = leftNodes.Select(c => new BuildingSubsidenceDataV1(c.NodeCode, c.Data)).ToList();
            dgv_right.DataSource = null;
            dgv_right.DataSource = rightNodes.Select(c => new BuildingSubsidenceDataV1(c.NodeCode, c.Data)).ToList();
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
            if (Model.CanSave)
            {
                var result = Model.Commit();
                if (!result.IsSuccess && !string.IsNullOrEmpty(result.Message))
                    MessageBox.Show(result.Message);
            }
        }
        /// <summary>
        /// 取消
        /// </summary>
        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            Model.Cancel();
            this.Close();
        }
        /// <summary>
        /// 新建
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Create_Click(object sender, EventArgs e)
        {
            if (Model.CanCreateNew)
            {
                Model.CreateNew();
            }
        }
        /// <summary>
        /// 删除
        /// </summary>
        private void btn_Delete_Click(object sender, EventArgs e)
        {
            if (Model.CanDelete)
            {
                var result = Model.Delete();
                if (!result.IsSuccess && !string.IsNullOrEmpty(result.Message))
                    MessageBox.Show(result.Message);
            }
        }
        /// <summary>
        /// 下一份
        /// </summary>
        private void btn_Next_Click(object sender, EventArgs e)
        {
            Model.Next();
        }
        /// <summary>
        /// 上一份
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Previous_Click(object sender, EventArgs e)
        {
            Model.Previous();
        }
        #endregion

        #region 编辑操作
        /// <summary>
        /// 导入Excel数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_LoadExcel_Click(object sender, EventArgs e)
        {
            //选择文件路径
            string path;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Excel 文件(*.xls)|*.xls";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                path = dialog.FileName;
            }
            else
            {
                return;
            }
            if (!File.Exists(path))
                throw new NotImplementedException("无效的文件路径");
            ApplicationClass excelApp = new ApplicationClass();
            var workbook = excelApp.Workbooks.Open(path);
            if (workbook == null)
                throw new NotImplementedException("Workbook文档对象无效");
            //TODO 加载动画效果
            var result = Model.ImportExcel(workbook);
            workbook.Close();
            workbook = null;
            excelApp.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
            excelApp = null;
            string message = "";
            switch (result)
            {
                case ParseResult.Success:
                    break;
                case ParseResult.ReportName_ParseFailure:
                    message = @"内容格式不符合预期(承包单位：@承包单位\s+监理单位：@监理单位\s+监测单位：@监测单位)";
                    break;
                case ParseResult.Participants_ParseFailure:
                    message = @"内容格式不符合预期(承包单位：@承包单位\s+监理单位：@监理单位\s+监测单位：@监测单位)";
                    break;
                case ParseResult.DateTime_ParseFailure:
                    message = @"内容格式不符合预期(监测日期：@yyyy.MM.dd\s+监测时间：@hh:mm-@hh:mm)";
                    break;
                case ParseResult.DateTime_Invalid:
                    message = $"文档日期不一致,当前记录日期:{Model.List.IssueDate.ToShortDateString()},请检查导入文件的监测日期是否存在差异";
                    break;
                case ParseResult.Instrument_ParseFailure:
                    message = @"内容格式不符合预期(仪器名称：@仪器名称\s+仪器编号：@仪器编号)";
                    break;
                default:
                    message = "未处理的反馈编码" + result.ToString();
                    break;
            }
            if (!string.IsNullOrEmpty(message))
            {
                Model.Rollback(false);
                MessageBox.Show(message);
            }
        }
        /// <summary>
        /// 关闭页面
        /// </summary>
        /// <param name="senedr"></param>
        /// <param name="e"></param>
        private void SubsidenceMonitorForm_FormClosing(object senedr, FormClosingEventArgs e)
        {
            //退出窗口时 需要清空未保存的内容
            Model.Rollback(true);
        }
        #endregion

        #region 浏览操作
        /// <summary>
        /// 接近警戒值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_CloseWarn_Click(object sender, EventArgs e)
        {
            //TODO Revit
            var form = new CPSettingsForm(null, Model.MemorableData.Data.CloseCTSettings);
            var result = form.ShowDialog();
            if (result == DialogResult.OK)
            {
                Model.MemorableData.Data.CloseCTSettings = form.Data;
                Model.Edited();
            }
        }
        /// <summary>
        /// 超过警戒值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_OverWarn_Click(object sender, EventArgs e)
        {
            var form = new CPSettingsForm(null, Model.MemorableData.Data.OverCTSettings);
            var result = form.ShowDialog();
            if (result == DialogResult.OK)
            {
                Model.MemorableData.Data.OverCTSettings = form.Data;
                Model.Edited();
            }
        } 
        #endregion

        /// <summary>
        /// 添加构件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_AddComponent_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 删除构件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_DeleteComponent_Click(object sender, EventArgs e)
        {

        }
    }
}
