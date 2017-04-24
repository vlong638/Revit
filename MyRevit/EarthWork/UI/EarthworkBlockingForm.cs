using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.EarthWork.Entity;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MyRevit.EarthWork.UI
{
    public enum ShowDialogType
    {
        Idle,
        AddElements,
        DeleleElements,
    }

    public partial class EarthworkBlockingForm : System.Windows.Forms.Form
    {
        public EarthworkBlocking Blocking;
        public DataGridViewRow Row { set; get; }
        public EarthworkBlock Block { set; get; }
        public UIApplication m_UIApp;
        public UIDocument m_UIDoc;
        public Document m_Doc;

        #region 初始化
        /// <summary>
        /// 测试处理
        /// </summary>
        public EarthworkBlockingForm()
        {
            InitializeComponent();

            InitForm();
        }
        public EarthworkBlockingForm(UIApplication uiApp)
        {
            InitializeComponent();

            m_UIApp = uiApp;
            m_UIDoc = uiApp.ActiveUIDocument;
            m_Doc = m_UIDoc.Document;
            InitForm();
        }
        private void InitForm()
        {
            //初始化参数
            TopMost = true;
            ShowDialogType = ShowDialogType.Idle;
            //初始化DataGridView
            dgv_Blocks.AutoGenerateColumns = false;
            Node_Name.DataPropertyName = nameof(EarthworkBlock.Name);
            Node_Description.DataPropertyName = nameof(EarthworkBlock.Description);
            PmSoft.Common.CommonClass.FaceRecorderForRevit recorder = new PmSoft.Common.CommonClass.FaceRecorderForRevit(nameof(EarthworkBlockingForm) + m_Doc.Title
                , PmSoft.Common.CommonClass.ApplicationPath.GetCurrentPath(m_Doc));
            var blockingStr = "";
            recorder.ReadValue(SaveKeyHelper.GetSaveKeyOfEarthworkBlocking(), ref blockingStr, 10000);
            if (blockingStr != "")
            {

                Blocking = Newtonsoft.Json.JsonConvert.DeserializeObject<EarthworkBlocking>(blockingStr);
            }
            else
            {
                Blocking = new EarthworkBlocking();
                //Blocking.Add(Blocking.CreateNew());
            }
            Blocking.InitByDocument(m_Doc);
            Blocking.Start();
            if (Blocking.Count() > 0)
                dgv_Blocks.DataSource = Blocking.Blocks;
            //初始化按钮
            btn_Save.Enabled = false;
            ToolTip tip = new ToolTip();
            tip.SetToolTip(btn_AddNode, "新增节点");
            tip.SetToolTip(btn_DeleteNode, "删除选中节点");
            tip.SetToolTip(btn_UpNode, "上移节点");
            tip.SetToolTip(btn_DownNode, "下移节点");
            tip.SetToolTip(btn_AddElement, "新增构件");
            tip.SetToolTip(btn_DeleteElement, "删除构件");
        }
        #endregion

        #region 模态,元素选取
        public ShowDialogType ShowDialogType { set; get; }
        public List<ElementId> SelectedElementIds { set; get; } = new List<ElementId>();
        public void FinishElementSelection()
        {
            switch (ShowDialogType)
            {
                case ShowDialogType.AddElements:
                    if (SelectedElementIds != null)
                        Block.Add(Blocking, SelectedElementIds);
                    ShowDialogType = ShowDialogType.Idle;
                    break;
                case ShowDialogType.DeleleElements:
                    if (SelectedElementIds != null)
                        Block.Delete(Blocking, SelectedElementIds);
                    ShowDialogType = ShowDialogType.Idle;
                    break;
                default:
                    break;
            }
            ValueChanged(null, null);
            ShowMessage("添加节点结束", $"节点:{Block.Name}现有:{Block.ElementIds.Count()}个元素");
        }
        #endregion

        /// <summary>
        /// 加载选中行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EarthworkBlockingForm_Load(object sender, EventArgs e)
        {
            if (dgv_Blocks.Rows.Count > 0)
                dgv_Blocks.Rows[0].Selected = true;
        }
        /// <summary>
        /// 首列序号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgv_Blocks_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SolidBrush b = new SolidBrush(this.dgv_Blocks.RowHeadersDefaultCellStyle.ForeColor);
            e.Graphics.DrawString((e.RowIndex + 1).ToString(System.Globalization.CultureInfo.CurrentUICulture)
                , this.dgv_Blocks.DefaultCellStyle.Font, b, e.RowBounds.Location.X + 20, e.RowBounds.Location.Y + 4);
        }
        /// <summary>
        /// 单击选行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgv_Blocks_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int rIndex = e.RowIndex;
            dgv_Blocks.Rows[rIndex].Selected = true;
        }
        /// <summary>
        /// 双击编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgv_Blocks_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int CIndex = e.ColumnIndex;
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewComboBoxColumn combo = dgv_Blocks.Columns[e.ColumnIndex] as DataGridViewComboBoxColumn;
                if (combo != null)  //如果该列是ComboBox列
                {
                    dgv_Blocks.BeginEdit(false); //结束该列的编辑状态
                    DataGridViewComboBoxEditingControl comboEdite = dgv_Blocks.EditingControl as DataGridViewComboBoxEditingControl;
                    if (comboEdite != null)
                    {
                        comboEdite.DroppedDown = true; //展现下拉列表
                    }
                }
                DataGridViewTextBoxColumn textbox = dgv_Blocks.Columns[e.ColumnIndex] as DataGridViewTextBoxColumn;
                if (textbox != null) //如果该列是TextBox列
                {
                    dgv_Blocks.BeginEdit(true); //开始编辑状态
                }
            }
        }
        /// <summary>
        /// 新增节点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_AddNode_Click(object sender, System.EventArgs e)
        {
            Blocking.Add(Blocking.CreateNew());
            Updatedgv_BlockWithSelectionChanged(-1, Blocking.Blocks.Count - 1);
            ValueChanged(sender, e);
        }
        /// <summary>
        // 更新DataGridView
        /// </summary>
        private void Updatedgv_Block()
        {
            dgv_Blocks.DataSource = null;
            dgv_Blocks.DataSource = Blocking.Blocks;
            dgv_Blocks.Refresh();
        }
        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_DeleteNode_Click(object sender, System.EventArgs e)
        {
            var rows = dgv_Blocks.SelectedRows;
            var index = -1;
            if (rows.Count > 0)
            {
                index = rows[0].Index > 0 ? rows[0].Index - 1 : 0;//删除项为0时 选中项重置为0
            }
            foreach (DataGridViewRow row in rows)
            {
                Blocking.Delete(row.DataBoundItem as EarthworkBlock);
            }
            Updatedgv_BlockWithSelectionChanged(-1, index);
            ValueChanged(sender, e);
        }
        private bool IsSingleBlockSelected(DataGridViewSelectedRowCollection rows)
        {
            if (rows == null || rows.Count == 0)
            {
                if (dgv_Blocks.CurrentCell != null)
                {
                    Row = rows[dgv_Blocks.CurrentCell.RowIndex];
                    Block = Row.DataBoundItem as EarthworkBlock;
                    return true;
                }
                ShowMessage("警告", "请选中节点");
                return false;
            }
            if (rows.Count > 1)
            {
                ShowMessage("警告", "暂不支持多项处理");
                return false;
            }
            Row = rows[0];
            Block = Row.DataBoundItem as EarthworkBlock;
            return true;
        }
        /// <summary>
        /// 上移节点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_UpNode_Click(object sender, System.EventArgs e)
        {
            var rows = dgv_Blocks.SelectedRows;
            if (IsSingleBlockSelected(rows))
            {
                var row = rows[0];
                if (Blocking.MoveStep1Foward(row.DataBoundItem as EarthworkBlock))
                {
                    Updatedgv_BlockWithSelectionChanged(row.Index, row.Index - 1);
                }
            }
            ValueChanged(sender, e);
        }
        /// <summary>
        /// 下移节点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_DownNode_Click(object sender, System.EventArgs e)
        {
            var rows = dgv_Blocks.SelectedRows;
            if (IsSingleBlockSelected(rows))
            {
                var row = rows[0];
                if (Blocking.MoveStep1Backward(row.DataBoundItem as EarthworkBlock))
                {
                    Updatedgv_BlockWithSelectionChanged(row.Index, row.Index + 1);
                }
            }
            ValueChanged(sender, e);
        }
        /// <summary>
        /// 刷新DataGridView数据并且更新选中
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        private void Updatedgv_BlockWithSelectionChanged(int startIndex, int endIndex)
        {
            Updatedgv_Block();
            if (startIndex >= 0)
                dgv_Blocks.Rows[startIndex].Selected = false;
            if (dgv_Blocks.Rows.Count > 0 && endIndex >= 0)
            {
                dgv_Blocks.Rows[endIndex].Selected = true;
                var cell = dgv_Blocks.Rows.Count > 0 ? dgv_Blocks.Rows[endIndex].Cells[0] : null;
                dgv_Blocks.CurrentCell = cell;
            }
        }
        /// <summary>
        /// 确定-提交
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Commit_Click(object sender, System.EventArgs e)
        {
            Blocking.Commit(this);
            DialogResult = DialogResult.OK;
            Close();
        }
        /// <summary>
        /// 应用-保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Save_Click(object sender, System.EventArgs e)
        {
            Blocking.Commit(this);
            Blocking.Start();
            btn_Save.Enabled = false;
        }
        /// <summary>
        /// 预览
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Preview_Click(object sender, System.EventArgs e)
        {
            //TODO 预览
            throw new NotImplementedException();
        }
        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Cancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        /// <summary>
        /// 在选中项前插入新节点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_InsertBefore_Click(object sender, System.EventArgs e)
        {
            var rows = dgv_Blocks.SelectedRows;
            if (IsSingleBlockSelected(rows))
            {
                var row = rows[0];
                Blocking.InsertBefore(row.Index, Blocking.CreateNew());
                Updatedgv_BlockWithSelectionChanged(row.Index, row.Index);
            }
            ValueChanged(sender, e);
        }
        /// <summary>
        /// 在选中项后插入新节点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_InsertAfter_Click(object sender, System.EventArgs e)
        {
            var rows = dgv_Blocks.SelectedRows;
            if (IsSingleBlockSelected(rows))
            {
                var row = rows[0];
                Blocking.InsertAfter(row.Index, Blocking.CreateNew());
                Updatedgv_BlockWithSelectionChanged(row.Index, row.Index + 1);
            }
            ValueChanged(sender, e);
        }
        /// <summary>
        /// 组合选中项和前项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_CombineBefore_Click(object sender, System.EventArgs e)
        {
            var rows = dgv_Blocks.SelectedRows;
            if (IsSingleBlockSelected(rows))
            {
                var row = rows[0];
                if (row.Index == 0)
                    return;
                Blocking.CombineBefore(row.Index);
                Updatedgv_BlockWithSelectionChanged(-1, row.Index - 1);
            }
            ValueChanged(sender, e);
        }
        /// <summary>
        /// 组合选中项和后项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_CombineAfter_Click(object sender, System.EventArgs e)
        {
            var rows = dgv_Blocks.SelectedRows;
            if (IsSingleBlockSelected(rows))
            {
                var row = rows[0];
                if (row.Index == Blocking.Count() - 1)
                    return;
                Blocking.CombineAfter(row.Index);
                Updatedgv_BlockWithSelectionChanged(-1, row.Index);
            }
            ValueChanged(sender, e);
        }
        #region 构件变更
        static List<int> SelectedRows = new List<int>();
        static List<int> SelectedCell = new List<int>();
        /// <summary>
        /// 保存列表选中项,由于采用了模态形式,页面显示总是刷新掉选中项
        /// </summary>
        void SaveDataGridViewSelection()
        {
            SelectedRows = new List<int>();
            foreach (DataGridViewRow row in dgv_Blocks.SelectedRows)
            {
                SelectedRows.Add(row.Index);
            }
            SelectedCell = new List<int>();
            if (dgv_Blocks.SelectedCells.Count > 0)
            {
                SelectedCell.Add(dgv_Blocks.SelectedCells[0].RowIndex);
                SelectedCell.Add(dgv_Blocks.SelectedCells[0].ColumnIndex);
            }
        }
        /// <summary>
        /// 加载列表选中项
        /// </summary>
        public void LoadDataGridViewSelection()
        {
            if (SelectedRows.Count() > 0)
            {
                foreach (int rowIndex in SelectedRows)
                {
                    dgv_Blocks.Rows[rowIndex].Selected = true;
                }
            }
            if (SelectedCell.Count() > 0)
            {
                dgv_Blocks.CurrentCell = dgv_Blocks[SelectedCell[1], SelectedCell[0]];
            }
        }
        /// <summary>
        /// 增加构件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_AddElement_Click(object sender, System.EventArgs e)
        {
            var rows = dgv_Blocks.SelectedRows;
            if (!IsSingleBlockSelected(rows))
            {
                return;
            }
            DialogResult = DialogResult.Retry;
            ShowDialogType = ShowDialogType.AddElements;
            SaveDataGridViewSelection();
            Close();
        }
        /// <summary>
        /// 删除构件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_DeleteElement_Click(object sender, System.EventArgs e)
        {
            var rows = dgv_Blocks.SelectedRows;
            if (!IsSingleBlockSelected(rows))
            {
                return;
            }
            DialogResult = DialogResult.Retry;
            ShowDialogType = ShowDialogType.DeleleElements;
            SaveDataGridViewSelection();
            Close();
        }
        #endregion
        /// <summary>
        /// 颜色/透明配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_CPSettings_Click(object sender, System.EventArgs e)
        {
            var rows = dgv_Blocks.SelectedRows;
            if (IsSingleBlockSelected(rows))
            {
                var row = rows[0];
                //含义不明,可能误加
                //if (row.Index == Blocking.Count() - 1)
                //    return;
                var block = row.DataBoundItem as EarthworkBlock;
                this.Hide();
                EarthworkBlockCPSettingsForm form = new EarthworkBlockCPSettingsForm(this, block.CPSettings);
                form.ShowDialog();
                this.Show();
            }
        }
        /// <summary>
        /// 消息反馈
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public void ShowMessage(string title, string content)
        {
            MessageBox.Show(content);
        }
        /// <summary>
        /// 列表编辑结束
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgv_Blocks_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            ValueChanged(sender, e);
        }
        private void ValueChanged(object sender, EventArgs e)
        {
            btn_Save.Enabled = true;
        }
        /// <summary>
        /// 页面加载时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EarthworkBlockingForm_Shown(object sender, EventArgs e)
        {
            LoadDataGridViewSelection();
        }
    }
}
