using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MyRevit.SubsidenceMonitor.Entities;
using MyRevit.SubsidenceMonitor.Interfaces;
using Microsoft.Office.Interop.Excel;
using Autodesk.Revit.DB;
using MyRevit.Utilities;
using Autodesk.Revit.UI;
using MyRevit.SubsidenceMonitor.Operators;
//using Autodesk.Revit.DB;

namespace MyRevit.SubsidenceMonitor.UI
{

    /// <summary>
    /// 几个注意点
    /// 传list和details,把关联关系的构件职责收束,确保关联关系
    /// CurrentDetailIndex可能为0,0表示TList下无TDetail
    /// MemorialDetail用于有数据时的提交和取消处理
    /// </summary>
    public partial class SkewBackMonitorForm : System.Windows.Forms.Form
    {
        #region 初始化和主要参数
        public SkewBackMonitorForm(ListForm listForm, UIDocument ui_doc, TList list) : base()
        {
            InitializeComponent();

            ////更新主要参数
            UI_Doc = ui_doc;
            ListForm = listForm;
            Model = new MultipleSingleMemorableDetails();
            IssueTypeEntity = list.IssueType.GetEntity();
            //初始化控件配置
            InitControlSettings();
            //如果有详情则加载详情数据
            Model.OnDataChanged += Model_OnDataChanged;
            Model.OnStateChanged += Model_OnStateChanged;
            Model.OnConfirmChangeCurrentWhileIsEdited += Model_OnChangeCurrentWhileIsEdited;
            Model.OnConfirmChangeCurrentWhileHasCreateNew += Model_OnChangeCurrentWhileHasCreateNew;
            Model.OnConfirmDelete += Model_OnConfirmDelete;
            //事件附加后再作数据的初始化,否则关联的信息无法在初始化的时候渲染出来
            Model.Init(ui_doc.Document, list);
            if (Model.MemorableData.Data.ExtraValue1.HasValue)
                tb_SkewBack_Well.Text = Model.MemorableData.Data.ExtraValue1.Value.ToString();
            if (Model.MemorableData.Data.ExtraValue2.HasValue)
                tb_SkewBack_Standard.Text = Model.MemorableData.Data.ExtraValue2.Value.ToString();
            if (Model.MemorableData.Data.ExtraValue3.HasValue)
                tb_SkewBack_Speed.Text = Model.MemorableData.Data.ExtraValue3.Value.ToString();

            Shown += SkewBackMonitorForm_Shown;
        }

        private void SkewBackMonitorForm_Shown(object sender, EventArgs e)
        {
            ListForm.ShowDialogType = ShowDialogType.Idle;
        }

        public UIDocument UI_Doc { set; get; }
        public ListForm ListForm { set; get; }
        public List<ElementId> SelectedElementIds { set; get; } = new List<ElementId>();
        public MultipleSingleMemorableDetails Model { set; get; }
        EIssueTypeEntity IssueTypeEntity { set; get; }

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
            tb_WarnArgs.Text = ListForm.WarnSettings.GetText(IssueTypeEntity.IssueType);
            //dgv 总宽479-42=437/5=87.4 420 17的Scroll空间
            //560-42-18-500/6 84 504 56 42+16 差不多? 保存还有多2左右 实际是 42的序列号+14的Scroll
            var headerNodes = IssueTypeEntity.GetHeaderNodes();
            dgv_left.HeaderNodes = headerNodes;
            dgv_left.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            dgv_right.HeaderNodes = headerNodes;
            dgv_right.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            dgv_left.Click += dgv_left_Click;
            dgv_right.Click += dgv_right_Click;
            Shown += SubsidenceMonitorForm_Shown;
            cb_NodeCode.SelectedIndexChanged += Cb_NodeCode_SelectedIndexChanged;
            dgv_left.BindingContextChanged += Dgv_left_BindingContextChanged;
            dgv_right.BindingContextChanged += Dgv_right_BindingContextChanged;
        }

        private void Cb_NodeCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_NodeCode.SelectedItem != null)
            {
                var detail = Model.MemorableData.Data;
                var nodes = Model.MemorableData.Data.DepthNodes.Where(c => c.NodeCode == cb_NodeCode.Text).ToList();
                //DGV
                var normalHeight = 20;
                ITDepthNodeDataCollection<ITDepthNodeData> leftNodes = new SkewBackCollection<SkewBackDataV1>();
                ITDepthNodeDataCollection<ITDepthNodeData> rightNodes = new SkewBackCollection<SkewBackDataV1>();
                if (nodes.Count <= normalHeight)
                {
                    foreach (var node in nodes)
                    {
                        leftNodes.Add(node.NodeCode, node.Depth, node.Data);
                    }
                }
                else
                {
                    var height = normalHeight;
                    if (nodes.Count > normalHeight * 2)
                        height = nodes.Count / 2;
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        var node = nodes[i];
                        if (i < height)
                            leftNodes.Add(node.NodeCode, node.Depth, node.Data);
                        else
                            rightNodes.Add(node.NodeCode, node.Depth, node.Data);
                    }
                }
                dgv_left.DataSource = null;
                dgv_left.DataSource = leftNodes.Datas;
                dgv_right.DataSource = null;
                dgv_right.DataSource = rightNodes.Datas;
            }
            else
            {
                dgv_left.DataSource = null;
                dgv_right.DataSource = null;
            }
        }
        private void SubsidenceMonitorForm_Shown(object sender, EventArgs e)
        {
            LoadDataGridViewSelection();
            ListForm.SubFormType = SubFormType.SkewBack;
        }
        private void SubsidenceMonitorForm_FormClosing(object senedr, FormClosingEventArgs e)
        {
            //退出窗口时 需要清空未保存的内容
            //编辑控件或者进行浏览时,不进行回滚处理
            if (DialogResult == DialogResult.Retry)
            {
                SaveDataGridViewSelection();
            }
            else
            {
                ClearDataGridViewSelection();
                Model.Rollback(true);
                ListForm.Activate();
            }
        }
        private bool Model_OnConfirmDelete()
        {
            //TODO 确认处理
            //ShowMessage("提醒", "用户确定了");
            return true;
        }
        private bool Model_OnChangeCurrentWhileHasCreateNew()
        {
            //TODO 确认处理
            //ShowMessage("提醒", "用户确定了");
            return true;
        }
        private bool Model_OnChangeCurrentWhileIsEdited()
        {
            //TODO 确认处理
            //ShowMessage("提醒", "用户确定了");
            return true;
        }
        private void Model_OnStateChanged(bool hasPrevious, bool hasNext, bool canCreateNew, bool canDelete, bool canSave)
        {
            btn_Previous.Enabled = hasPrevious;
            btn_Next.Enabled = hasNext;
            btn_CreateNew.Enabled = canCreateNew;
            btn_Delete.Enabled = canDelete;
            btn_Submit.Enabled = canSave;
            if (canDelete || canSave)//可保存或可删除意味着编辑主体的存在
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
        private void Model_OnDataChanged(TDetail detail)
        {
            tb_ReportName.Text = detail.ReportName;//报告名称
            tb_IssueType.Text = detail.IssueType.ToString();//沉降类型
            tb_Contractor.Text = detail.Contractor;//承包单位
            tb_Supervisor.Text = detail.Supervisor;//监理单位
            tb_Monitor.Text = detail.Monitor;//监测单位
            tb_Date.Text = detail.IssueDateTime.ToString("yyyy.MM.dd");//监测日期
            var endTime = detail.IssueDateTime.AddMinutes(detail.IssueTimeRange);
            var timeFormat = "HH:mm";
            tb_Time.Text = $"{detail.IssueDateTime.ToString(timeFormat)}-{endTime.ToString(timeFormat)}";//监测时间
            tb_InstrumentName.Text = detail.InstrumentName;//仪器名称
            tb_InstrumentCode.Text = detail.InstrumentCode;//仪器编号
            //cb_NodeCode
            if (Model.MemorableData.Data.DepthNodes.Count > 0)
            {
                cb_NodeCode.DataSource = Model.MemorableData.Data.DepthNodes.Select(c => c.NodeCode).Distinct().ToList();
                cb_NodeCode.SelectedIndex = 0;

                foreach (var node in Model.MemorableData.Data.DepthNodes)
                    detail.DepthNodeDatas.Add(node.NodeCode, node.Depth, node.Data);
            }
            else
            {
                cb_NodeCode.DataSource = null;
            }
        }
        /// <summary>
        /// 清空绑定时的默认选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dgv_right_BindingContextChanged(object sender, EventArgs e)
        {
            dgv_right.ClearSelection();
        }
        /// <summary>
        /// 清空绑定时的默认选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dgv_left_BindingContextChanged(object sender, EventArgs e)
        {
            dgv_left.ClearSelection();
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
                    ShowMessage("警告", result.Message);
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
                ClearDataGridViewSelection();
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
                    ShowMessage("警告", result.Message);
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
            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            path = dialog.FileName;
            if (!File.Exists(path))
                throw new NotImplementedException("无效的文件路径");
            ApplicationClass excelApp = new ApplicationClass();
            var workbook = excelApp.Workbooks.Open(path);
            if (workbook == null)
                throw new NotImplementedException("Workbook文档对象无效");
            //TODO 加载动画效果
            var result = Model.ImportExcel(workbook);
            //workbook.Close();
            //workbook = null;
            //excelApp.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
            excelApp = null;
            string message = "";
            switch (result)
            {
                case ParseResult.Success:
                    break;
                case ParseResult.ReportName_ParseFailure:
                    message = @"预期的表不存在";
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
        /// 添加构件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btn_AddComponent_Click(object sender, EventArgs e)
        {
            if (SelectedNodes.Count == 0)
            {
                ShowMessage("警告", "需选中节点");
            }
            else if (SelectedNodes.Count > 1)
            {
                ShowMessage("警告", "需选中单一节点");
            }
            else
            {
                ListForm.ShowDialogType = ShowDialogType.AddElements_ForDetail;
                string viewName = ListForm.ShowDialogType.GetViewName();
                var view = Revit_Document_Helper.GetElementByNameAs<View3D>(UI_Doc.Document, viewName);
                var doc = UI_Doc.Document;
                var transactionName = nameof(SubsidenceMonitor) + nameof(btn_AddComponent_Click);
                if (view == null)
                {
                    bool isSuccess = DealWithTransaction(viewName, doc, transactionName, () =>
                    {
                        view = Revit_Document_Helper.Create3DView(doc, viewName);
                    });
                    if (!isSuccess)
                        return;
                }
                UI_Doc.ActiveView = view;
                this.DialogResult = DialogResult.Retry;
                this.Close();
                ListForm.DialogResult = DialogResult.Retry;
                ListForm.Close();
            }
        }
        /// <summary>
        /// 删除构件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btn_DeleteComponent_Click(object sender, EventArgs e)
        {
            if (SelectedNodes.Count == 0)
            {
                ShowMessage("警告", "需选中节点");
            }
            else if (SelectedNodes.Count > 1)
            {
                ShowMessage("警告", "需选中单一节点");
            }
            else
            {
                ListForm.ShowDialogType = ShowDialogType.DeleleElements_ForDetail;
                string viewName = ListForm.ShowDialogType.GetViewName();
                var view = Revit_Document_Helper.GetElementByNameAs<View3D>(UI_Doc.Document, viewName);
                var doc = UI_Doc.Document;
                var transactionName = nameof(SubsidenceMonitor) + nameof(btn_AddComponent_Click);
                if (view == null)
                {
                    bool isSuccess = DealWithTransaction(viewName, doc, transactionName, () =>
                   {
                       view = Revit_Document_Helper.Create3DView(doc, viewName);
                   });
                    if (!isSuccess)
                        return;
                }
                UI_Doc.ActiveView = view;
                var elementIds = Model.GetElementIdsByTDepthNode(SelectedNodes[0], UI_Doc.Document);
                Revit_View_Helper.IsolateElements(UI_Doc.Document, nameof(SubsidenceMonitor) + nameof(btn_DeleteComponent_Click), UI_Doc.Document.ActiveView, elementIds);
                this.DialogResult = DialogResult.Retry;
                this.Close();
                ListForm.DialogResult = DialogResult.Retry;
                ListForm.Close();
            }
        }
        /// <summary>
        /// 结束构件选择
        /// </summary>
        public void FinishElementSelection()
        {
            switch (ListForm.ShowDialogType)
            {
                case ShowDialogType.AddElements_ForDetail:
                    if (SelectedElementIds != null && SelectedElementIds.Count > 0)
                    {
                        Model.AddElementIds(SelectedNodes[0].NodeCode, SelectedNodes[0].Depth, SelectedElementIds);
                        Model.Edited();
                    }
                    //ListForm.ShowDialogType = ShowDialogType.Idle;
                    break;
                case ShowDialogType.DeleleElements_ForDetail:
                    if (SelectedElementIds != null && SelectedElementIds.Count > 0)
                    {
                        Model.DeleteElementIds(SelectedNodes[0].NodeCode, SelectedNodes[0].Depth, SelectedElementIds);
                        Model.Edited();
                    }
                    Revit_View_Helper.DeisolateElements(UI_Doc.Document, nameof(SubsidenceMonitor) + nameof(FinishElementSelection), UI_Doc.ActiveView);
                    //ListForm.ShowDialogType = ShowDialogType.Idle;
                    break;
                default:
                    break;
            }
        }
        struct CellLocation
        {
            public int RowIndex;
            public int ColumnIndex;

            public CellLocation(int rowIndex, int columnIndex)
            {
                this.RowIndex = rowIndex;
                this.ColumnIndex = columnIndex;
            }
        }
        static List<int> SelectedRows_left { get; set; } = new List<int>();
        static List<CellLocation> SelectedCells_left { get; set; } = new List<CellLocation>();
        static List<int> SelectedRows_right { get; set; } = new List<int>();
        static List<CellLocation> SelectedCells_right { get; set; } = new List<CellLocation>();
        /// <summary>
        /// 保存列表选中项,由于采用了模态形式,页面显示总是刷新掉选中项
        /// </summary>
        void SaveDataGridViewSelection(MyDGV0427 dgv, List<int> rows, List<CellLocation> cells)
        {
            rows.Clear();
            foreach (DataGridViewRow row in dgv.SelectedRows)
            {
                rows.Add(row.Index);
            }
            cells.Clear();
            if (dgv.SelectedCells.Count > 0)
            {
                cells.Add(new CellLocation(dgv.SelectedCells[0].RowIndex, dgv.SelectedCells[0].ColumnIndex));
            }
        }
        void SaveDataGridViewSelection()
        {
            SaveDataGridViewSelection(dgv_left, SelectedRows_left, SelectedCells_left);
            SaveDataGridViewSelection(dgv_right, SelectedRows_right, SelectedCells_right);
        }
        void ClearDataGridViewSelection(MyDGV0427 dgv, List<int> rows, List<CellLocation> cells)
        {
            rows.Clear();
            cells.Clear();
        }
        void ClearDataGridViewSelection()
        {
            ClearDataGridViewSelection(dgv_left, SelectedRows_left, SelectedCells_left);
            ClearDataGridViewSelection(dgv_right, SelectedRows_right, SelectedCells_right);
        }
        /// <summary>
        /// 加载列表选中项
        /// </summary>
        void LoadDataGridViewSelection(MyDGV0427 dgv, List<int> rows, List<CellLocation> cells)
        {
            if (rows.Count() > 0)
            {
                foreach (int rowIndex in rows)
                {
                    dgv.Rows[rowIndex].Selected = true;
                }
            }
            if (cells.Count() > 0)
            {
                //2017/05/09 10:05 修正Cells的选中,CurrentCell是左侧箭头的设置
                foreach (CellLocation cell in cells)
                {
                    dgv.Rows[cell.RowIndex].Cells[cell.ColumnIndex].Selected = true;
                }
                dgv.CurrentCell = dgv[cells[0].ColumnIndex, cells[0].RowIndex];
            }
        }
        void LoadDataGridViewSelection()
        {
            LoadDataGridViewSelection(dgv_left, SelectedRows_left, SelectedCells_left);
            LoadDataGridViewSelection(dgv_right, SelectedRows_right, SelectedCells_right);
        }
        #region 节点选中处理
        List<TDepthNode> SelectedNodes = new List<TDepthNode>();
        private void dgv_left_Click(object sender, EventArgs e)
        {
            var dgv = dgv_left;
            dgv_right.ClearSelection();
            ChangeCurrentNode(dgv);
        }
        private void dgv_right_Click(object sender, EventArgs e)
        {
            var dgv = dgv_right;
            dgv_left.ClearSelection();
            ChangeCurrentNode(dgv);
        }
        private void ChangeCurrentNode(MyDGV0427 dgv)
        {
            SelectedNodes = new List<TDepthNode>();
            if (dgv.SelectedRows.Count > 0)
            {
                SelectedNodes.Clear();
                foreach (DataGridViewRow row in dgv.SelectedRows)
                {
                    var data = dgv.Rows[0].DataBoundItem as ITDepthNodeData;// as BuildingSubsidenceDataV1;
                    SelectedNodes.Add(Model.MemorableData.Data.DepthNodes.First(c => c.NodeCode == data.NodeCode && c.Depth == data.Depth));
                }
                return;
            }
            else if (dgv.SelectedCells.Count > 0)
            {
                List<int> rowIndexes = new List<int>();
                foreach (DataGridViewCell cell in dgv.SelectedCells)
                {
                    if (!rowIndexes.Contains(cell.RowIndex))
                    {
                        rowIndexes.Add(cell.RowIndex);
                    }
                }
                foreach (var rowIndex in rowIndexes)
                {
                    var data = dgv.Rows[rowIndex].DataBoundItem as ITDepthNodeData;// as BuildingSubsidenceDataV1;
                    SelectedNodes.Add(Model.MemorableData.Data.DepthNodes.First(c => c.NodeCode == data.NodeCode && c.Depth == data.Depth));
                }
            }
        }
        #endregion
        /// <summary>
        /// 测点赋值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btn_RenderComponent_Click(object sender, EventArgs e)
        {
            if (SelectedNodes.Count == 0)
            {
                ShowMessage("警告", "需选中节点");
                return;
            }

            var transactionName = nameof(SubsidenceMonitor) + nameof(btn_RenderComponent_Click);
            bool isSuccess = DealWithTransaction(UI_Doc.Document, transactionName
                , () => { Model.RenderNodeInfoToElements(SelectedNodes.Select(c => c.NodeCode).ToList(), UI_Doc.Document); });
        }
        #endregion

        #region 浏览操作

        Color ColorForSelectedElements = new Color(System.Drawing.Color.Red.R, System.Drawing.Color.Red.G, System.Drawing.Color.Red.B);

        #region 辅助函数
        /// <summary>
        /// 显示对用户反馈
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        public void ShowMessage(string title, string message)
        {
            TaskDialog.Show(title, message);
        }
        /// <summary>
        /// 视图逻辑处理
        /// 支持(隐藏,淡显,红显)和(反隐藏,淡显,红显)
        /// </summary>
        /// <param name="showDialogType"></param>
        /// <param name="needHide"></param>
        /// <param name="getElementIds"></param>
        /// <returns></returns>
        private bool DetailWithView(ShowDialogType showDialogType, bool needHide, Func<Document, List<ElementId>> getElementIds)
        {
            ListForm.ShowDialogType = showDialogType;
            string viewName = ListForm.ShowDialogType.GetViewName();
            var doc = UI_Doc.Document;
            View3D view = null;
            var transactionName = nameof(SubsidenceMonitor) + nameof(btn_ViewSelection_Click);
            bool isSuccess = DealWithTransaction(viewName, doc, transactionName, () =>
            {
                view = Revit_Document_Helper.GetElementByNameAs<View3D>(UI_Doc.Document, viewName);
                if (view == null)
                    view = Revit_Document_Helper.Create3DView(doc, viewName);
                if (needHide)
                {
                    //渲染_所有 隐藏
                    IList<Element> allElements = GetAllElements(doc);
                    List<ElementId> elementIdsToHid = new List<ElementId>();
                    foreach (var element in allElements)
                        if (element.CanBeHidden(view) && element.Id != view.Id)
                            elementIdsToHid.Add(element.Id);
                    if (elementIdsToHid.Count > 0)
                        view.HideElements(elementIdsToHid);
                    //渲染_测点 淡显
                    var nodesElementIds = Model.GetAllNodesElementIdsByTDepthNode(doc);
                    if (nodesElementIds.Count > 0)
                        view.UnhideElements(nodesElementIds);
                    var defaultOverrideGraphicSettings = CPSettings.GetTingledOverrideGraphicSettings(doc);
                    foreach (var elementId in nodesElementIds)
                        view.SetElementOverrides(elementId, defaultOverrideGraphicSettings);
                }
                else
                {
                    ////渲染_所有 反隐藏
                    //IList<Element> allElements = GetAllElements(doc);
                    //List<ElementId> elementIdsToHid = new List<ElementId>();
                    //foreach (var element in allElements)
                    //    if (element.CanBeHidden(view) && element.Id != view.Id)
                    //        elementIdsToHid.Add(element.Id);
                    //view.UnhideElements(elementIdsToHid);
                    //渲染_所有 淡显
                    var allElementIds = GetAllElements(doc);
                    var defaultOverrideGraphicSettings = CPSettings.GetTingledOverrideGraphicSettings(doc);
                    foreach (var elementId in allElementIds)
                        view.SetElementOverrides(elementId.Id, defaultOverrideGraphicSettings);
                }
                //渲染_选中 红显
                var overrideGraphicSettings = Revit_Helper.GetOverrideGraphicSettings(ColorForSelectedElements, CPSettings.GetDefaultFillPatternId(doc), 0);
                var selectedElementIds = getElementIds(doc);
                foreach (var elementId in selectedElementIds)
                    view.SetElementOverrides(elementId, overrideGraphicSettings);
            });
            if (view != null)
                UI_Doc.ActiveView = view;
            return isSuccess;
        }
        /// <summary>
        /// 视图处理
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="view"></param>
        /// <param name="doc"></param>
        /// <param name="transactionName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        bool DealWithTransaction(string viewName, Document doc, string transactionName, System.Action action)
        {
            bool isSuccess;
            using (var transaction = new Transaction(doc, transactionName))
            {
                transaction.Start();
                try
                {
                    action();
                    transaction.Commit();
                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    transaction.RollBack();
                    ShowMessage("警告", $"视图({viewName})加载失败,错误详情:" + ex.Message);
                    isSuccess = false;
                }
            }
            return isSuccess;
        }
        bool DealWithTransaction(Document doc, string transactionName, System.Action action)
        {
            bool isSuccess;
            using (var transaction = new Transaction(doc, transactionName))
            {
                transaction.Start();
                try
                {
                    action();
                    transaction.Commit();
                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    transaction.RollBack();
                    ShowMessage("警告", $"处理存在错误,错误详情:" + ex.Message);
                    isSuccess = false;
                }
            }
            return isSuccess;
        }
        /// <summary>
        /// 隐藏所有元素
        /// </summary>
        /// <param name="view"></param>
        /// <param name="doc"></param>
        static void HideAllElements(View3D view, Document doc)
        {
            //按Category处理
            ////取消可见
            //try
            //{
            //    foreach (Category category in doc.Settings.Categories)
            //    {
            //        category.set_Visible(view, false);
            //    }
            //}
            //catch { };//由于存在某些User Hidden的Category暂不明如何针对性的设置

            //按Element处理
            IList<Element> allElements = GetAllElements(doc);
            List<ElementId> elementIdsToHid = new List<ElementId>();
            foreach (var element in allElements)
            {
                if (element.CanBeHidden(view) && element.Id != view.Id)
                {
                    elementIdsToHid.Add(element.Id);
                }
            }
            if (elementIdsToHid.Count > 0)
                view.HideElements(elementIdsToHid);
        }
        /// <summary>
        /// 获取所有元素
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        static IList<Element> GetAllElements(Document doc)
        {
            //按元素隐藏
            return new FilteredElementCollector(doc).WherePasses(new LogicalOrFilter(new ElementIsElementTypeFilter(false), new ElementIsElementTypeFilter(true))).ToElements();
        }
        #endregion

        /// <summary>
        /// 测点查看-查看选中
        /// 选中-红色
        /// 测点-淡显
        /// 非测点-隐藏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ViewSelection_Click(object sender, EventArgs e)
        {
            if (SelectedNodes.Count == 0)
            {
                ShowMessage("提醒", "需选中节点");
                return;
            }
            bool isSuccess = DetailWithView(ShowDialogType.ViewElementsBySelectedNodes, true,
                (doc) => Model.GetElementIdsByTDepthNode(SelectedNodes, doc));
            if (!isSuccess)
                return;
            this.DialogResult = DialogResult.Retry;
            this.Close();
            ListForm.DialogResult = DialogResult.Retry;
            ListForm.Close();
        }
        /// <summary>
        /// 测点查看-整体查看
        /// 选中-红色
        /// 其他-淡显
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ViewAll_Click(object sender, EventArgs e)
        {
            if (SelectedNodes.Count == 0)
            {
                ShowMessage("提醒", "需选中节点");
                return;
            }
            bool isSuccess = DetailWithView(ShowDialogType.ViewElementsByAllNodes, false, (doc) => Model.GetElementIdsByTDepthNode(SelectedNodes, doc));
            if (!isSuccess)
                return;
            this.DialogResult = DialogResult.Retry;
            this.Close();
            ListForm.DialogResult = DialogResult.Retry;
            ListForm.Close();
        }
        /// <summary>
        /// 本次最大变量点位查看
        /// 最大-红色
        /// 测点-淡显
        /// 非测点-隐藏
        /// </summary>
        private void btn_ViewCurrentMax_Red_Click(object sender, EventArgs e)
        {
            bool isSuccess = DetailWithView(ShowDialogType.ViewCurrentMaxByRed, true, (doc) => Model.GetCurrentMaxNodesElementsByTDepthNode(doc));
            if (!isSuccess)
                return;
            this.DialogResult = DialogResult.Retry;
            this.Close();
            ListForm.DialogResult = DialogResult.Retry;
            ListForm.Close();
        }
        /// <summary>
        /// 单次最大变量-整体查看
        /// 最大-红色
        /// 测点-淡显
        /// 非测点-淡显
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ViewCurrentMax_All_Click(object sender, EventArgs e)
        {
            bool isSuccess = DetailWithView(ShowDialogType.ViewCurrentMaxByAll, false, (doc) => Model.GetCurrentMaxNodesElementsByTDepthNode(doc));
            if (!isSuccess)
                return;
            this.DialogResult = DialogResult.Retry;
            this.Close();
            ListForm.DialogResult = DialogResult.Retry;
            ListForm.Close();
        }
        /// <summary>
        /// 累计最大变量-红色显示
        /// 最大-红色
        /// 测点-淡显
        /// 非测点-隐藏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ViewSumMax_Red_Click(object sender, EventArgs e)
        {
            bool isSuccess = DetailWithView(ShowDialogType.ViewTotalMaxByRed, true, (doc) => Model.GetTotalMaxNodesElementsByTDepthNode(doc));
            if (!isSuccess)
                return;
            this.DialogResult = DialogResult.Retry;
            this.Close();
            ListForm.DialogResult = DialogResult.Retry;
            ListForm.Close();
        }
        /// <summary>
        /// 累计最大变量-整体查看
        /// 最大-红色
        /// 测点-淡显
        /// 非测点-淡显
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ViewSumMax_All_Click(object sender, EventArgs e)
        {
            bool isSuccess = DetailWithView(ShowDialogType.ViewTotalMaxByAll, false, (doc) => Model.GetTotalMaxNodesElementsByTDepthNode(doc));
            if (!isSuccess)
                return;
            this.DialogResult = DialogResult.Retry;
            this.Close();
            ListForm.DialogResult = DialogResult.Retry;
            ListForm.Close();
        }
        /// <summary>
        /// 接近警戒值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_CloseWarnCPSettings_Click(object sender, EventArgs e)
        {
            var form = new CPSettingsForm(UI_Doc.Document, Model.MemorableData.Data.CloseCTSettings);
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
        private void btn_OverWarnCPSettings_Click(object sender, EventArgs e)
        {
            var form = new CPSettingsForm(UI_Doc.Document, Model.MemorableData.Data.OverCTSettings);
            var result = form.ShowDialog();
            if (result == DialogResult.OK)
            {
                Model.MemorableData.Data.OverCTSettings = form.Data;
                Model.Edited();
            }
        }
        /// <summary>
        /// 接近预警预览
        /// 接近:报警值*80%,天数-1
        /// 显示测点,
        /// 接近:颜色1
        /// 非接近:颜色2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ViewCloseWarn_Click(object sender, EventArgs e)
        {
            string s = IssueTypeEntity.CheckWarnSettings(ListForm.WarnSettings);
            if (!string.IsNullOrEmpty(s))
            {
                ShowMessage("警告", s);
                return;
            }
            short value = 0;
            if (!short.TryParse(tb_SkewBack_Well.Text, out value))
            {
                ShowMessage("警告", $"请输入有效的{"端头井累计值"}");
                return;
            }
            var warnValue = ListForm.WarnSettings.SkewBack_WellMillimeter;
            if (warnValue == int.MinValue)
            {
                ShowMessage("警告", $"请联系管理员设置{"端头井累计值"}的预警值");
                return;
            }
            if (value >= warnValue * WarnSettings.CloseCoefficient && value < warnValue * WarnSettings.OverCoefficient)
            {
                ShowMessage("警告", $"输入的{"端头井累计值"}接近设定的预警值");
                return;
            }

            if (!short.TryParse(tb_SkewBack_Standard.Text, out value))
            {
                ShowMessage("警告", $"请输入有效的{"标准段累计值"}");
                return;
            }
            warnValue = ListForm.WarnSettings.SkewBack_StandardMillimeter;
            if (warnValue == int.MinValue)
            {
                ShowMessage("警告", $"请联系管理员设置{"标准段累计值"}的预警值");
                return;
            }
            if (value >= warnValue * WarnSettings.CloseCoefficient && value < warnValue * WarnSettings.OverCoefficient)
            {
                ShowMessage("警告", $"输入的{"标准段累计值"}接近设定的预警值");
                return;
            }

            if (!short.TryParse(tb_SkewBack_Speed.Text, out value))
            {
                ShowMessage("警告", $"请输入有效的{"变形速率"}");
                return;
            }
            warnValue = ListForm.WarnSettings.SkewBack_Speed;
            if (warnValue == int.MinValue)
            {
                ShowMessage("警告", $"请联系管理员设置{"变形速率"}的预警值");
                return;
            }
            warnValue = ListForm.WarnSettings.SkewBack_Day;
            if (warnValue == int.MinValue)
            {
                ShowMessage("警告", $"请联系管理员设置{"变形速率连续天数"}的预警值");
                return;
            }
            var currentDate = Model.MemorableData.Data.IssueDateTime.Date;
            var dateValues = Facade.GetDateTimeValues(IssueTypeEntity.IssueType, currentDate.AddDays(-warnValue + 1), warnValue).OrderByDescending(c => c.DateTime).ToList();
            dateValues.Add(new DateTimeValue(Model.MemorableData.Data.IssueDateTime, short.Parse(tb_SkewBack_Speed.Text)));//添加当前值,数据基于数据库,避免导入未保存缺失数据
            int index = 1;
            int maxSpan = warnValue + 10;
            int warnCount = 0;
            while (warnValue > 0 && index <= maxSpan)
            {
                var dailyDate = currentDate.Date.AddDays(-index);
                var dailyValue = dateValues.Where(c => c.DateTime.Date == dailyDate);
                if (dailyValue.Count() == 0)
                {
                    index++;
                    continue;
                }
                if (dailyValue.Where(c => c.Value != double.NaN && c.Value >= ListForm.WarnSettings.SkewBack_Speed * WarnSettings.CloseCoefficient).Count() == 0)
                {
                    //未超过预警值
                    ShowMessage("提醒", "变形速率未达到预警值");
                    return;
                }
                else
                {
                    warnCount++;
                    if (warnCount >= ListForm.WarnSettings.SkewBack_Day)
                    {
                        ShowMessage("警告", $"变形速率已连续{warnCount}天的超过预警值");
                        return;
                    }
                }
                index++;
                warnValue--;
            }
            ShowMessage("提醒", "校验通过");
            return;//侧斜不作预览处理
        }

        private bool DetailWithView(Document doc, string transactionName, string cpSettingsString, View3D view, ShowDialogType dialogType, Func<List<ElementId>> GetElementIds)
        {
            bool isSuccess;
            ListForm.ShowDialogType = dialogType;
            string viewName = dialogType.GetViewName();
            isSuccess = DealWithTransaction(viewName, doc, transactionName, () =>
            {
                view = Revit_Document_Helper.GetElementByNameAs<View3D>(UI_Doc.Document, viewName);
                if (view == null)
                    view = Revit_Document_Helper.Create3DView(doc, viewName);
                ////渲染_所有 隐藏
                //IList<Element> allElements = GetAllElements(doc);
                //List<ElementId> elementIdsToHid = new List<ElementId>();
                //foreach (var element in allElements)
                //    if (element.CanBeHidden(view) && element.Id != view.Id)
                //        elementIdsToHid.Add(element.Id);
                //if (elementIdsToHid.Count > 0)
                //    view.HideElements(elementIdsToHid);
                ////渲染_测点 淡显,显示
                //var allNodesElementIds = Model.GetAllNodesElementIds(doc);
                //var defaultOverrideGraphicSettings = CPSettings.GetTingledOverrideGraphicSettings(doc);
                //if (allNodesElementIds.Count > 0)
                //    view.UnhideElements(allNodesElementIds);
                //foreach (var elementId in allNodesElementIds)
                //    view.SetElementOverrides(elementId, defaultOverrideGraphicSettings);
                //渲染_选中 选中颜色显示
                var cpSettings = new CPSettings(cpSettingsString);
                var overrideGraphicSettings = Revit_Helper.GetOverrideGraphicSettings(new Color(cpSettings.Color.R, cpSettings.Color.G, cpSettings.Color.B), new ElementId(cpSettings.FillerId), cpSettings.SurfaceTransparency);
                var selectedElementIds = GetElementIds();
                foreach (var elementId in selectedElementIds)
                    view.SetElementOverrides(elementId, overrideGraphicSettings);
            });
            if (view != null)
                UI_Doc.ActiveView = view;
            return isSuccess;
        }

        /// <summary>
        /// 超出预警预览
        /// 超出:报警值*80%,天数-1
        /// 超出:颜色1
        /// 非超出:颜色2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ViewOverWarn_Click(object sender, EventArgs e)
        {
            string s = IssueTypeEntity.CheckWarnSettings(ListForm.WarnSettings);
            if (!string.IsNullOrEmpty(s))
            {
                ShowMessage("警告", s);
                return;
            }
            short value = 0;
            if (!short.TryParse(tb_SkewBack_Well.Text, out value))
            {
                ShowMessage("警告", $"请输入有效的{"端头井累计值"}");
                return;
            }
            if (ListForm.WarnSettings.SkewBack_WellMillimeter == int.MinValue)
            {
                ShowMessage("警告", $"请联系管理员设置{"端头井累计值"}值的预警值");
                return;
            }
            var warnValue = ListForm.WarnSettings.SkewBack_WellMillimeter;
            if (value >= warnValue * WarnSettings.OverCoefficient)
            {
                ShowMessage("警告", $"输入的{"端头井累计值"}超出设定的预警值");
                return;
            }

            if (ListForm.WarnSettings.SkewBack_StandardMillimeter == int.MinValue)
            {
                ShowMessage("警告", $"请联系管理员设置{"端头井累计值"}值的预警值");
                return;
            }
            if (!short.TryParse(tb_SkewBack_Standard.Text, out value))
            {
                ShowMessage("警告", $"请输入有效的{"标准段累计值"}");
                return;
            }
            warnValue = ListForm.WarnSettings.SkewBack_StandardMillimeter;
            if (value >= warnValue * WarnSettings.OverCoefficient)
            {
                ShowMessage("警告", $"输入的{"标准段累计值"}超出设定的预警值");
                return;
            }

            if (ListForm.WarnSettings.SkewBack_Speed == int.MinValue)
            {
                ShowMessage("警告", $"请联系管理员设置{"端头井累计值"}值的预警值");
                return;
            }
            if (!short.TryParse(tb_SkewBack_Speed.Text, out value))
            {
                ShowMessage("警告", $"请输入有效的{"变形速率"}");
                return;
            }
            warnValue = ListForm.WarnSettings.SkewBack_Speed;
            if (value >= warnValue * WarnSettings.OverCoefficient)
            {
                ShowMessage("警告", $"输入的{"变形速率"}超出设定的预警值");
                return;
            }
            //TODO 侧斜存在连续N天的速率监测算法
            warnValue = ListForm.WarnSettings.SkewBack_Day;
            if (warnValue == int.MinValue)
            {
                ShowMessage("警告", $"请联系管理员设置{"变形速率"}值的预警值");
                return;
            }
            var currentDate = Model.MemorableData.Data.IssueDateTime.Date;
            var dateValues = Facade.GetDateTimeValues(IssueTypeEntity.IssueType, currentDate.AddDays(-warnValue + 1), warnValue).OrderByDescending(c => c.DateTime).ToList();
            dateValues.Add(new DateTimeValue(Model.MemorableData.Data.IssueDateTime, short.Parse(tb_SkewBack_Speed.Text)));//添加当前值,数据基于数据库,避免导入未保存缺失数据
            int index = 1;
            int maxSpan = warnValue + 10;
            int warnCount = 0;
            while (warnValue > 0 && index <= maxSpan)
            {
                var dailyDate = currentDate.Date.AddDays(-index);
                var dailyValue = dateValues.Where(c => c.DateTime.Date == dailyDate);
                if (dailyValue.Count() == 0)
                {
                    index++;
                    continue;
                }
                if (dailyValue.Where(c => c.Value != double.NaN && c.Value >= ListForm.WarnSettings.SkewBack_Speed * WarnSettings.OverCoefficient).Count() == 0)
                {
                    //未超过预警值
                    ShowMessage("提醒", "变形速率未达到预警值");
                    return;
                }
                else
                {
                    warnCount++;
                    if (warnCount >= ListForm.WarnSettings.SkewBack_Day)
                    {
                        ShowMessage("警告", $"变形速率已连续{warnCount}天的超过预警值");
                        return;
                    }
                }
                index++;
                warnValue--;
            }
            ShowMessage("提醒", "校验通过");
            return;//侧斜不作预览处理
        }
        #endregion

        private void tb_SkewBack_Well_TextChanged(object sender, EventArgs e)
        {
            short value = 0;
            if (!short.TryParse(tb_SkewBack_Well.Text, out value))
            {
                ShowMessage("警告", "请输入正确的数值");
                return;
            }
            Model.MemorableData.Data.ExtraValue1 = value;
            Model.Edited();
        }
        private void tb_SkewBack_Standard_TextChanged(object sender, EventArgs e)
        {
            short value = 0;
            if (!short.TryParse(tb_SkewBack_Standard.Text, out value))
            {
                ShowMessage("警告", "请输入正确的数值");
                return;
            }
            Model.MemorableData.Data.ExtraValue2 = value;
            Model.Edited();
        }
        private void tb_SkewBack_Speed_TextChanged(object sender, EventArgs e)
        {
            short value = 0;
            if (!short.TryParse(tb_SkewBack_Speed.Text, out value))
            {
                ShowMessage("警告", "请输入正确的数值");
                return;
            }
            Model.MemorableData.Data.ExtraValue3 = value;
            Model.Edited();
        }
    }
}
