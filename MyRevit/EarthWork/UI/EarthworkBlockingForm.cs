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
            //dgv_Blocks
            dgv_Blocks.AutoGenerateColumns = false;
            Node_Name.DataPropertyName = nameof(EarthworkBlock.Name);
            Node_Description.DataPropertyName = nameof(EarthworkBlock.Description);
            PmSoft.Common.CommonClass.FaceRecorderForRevit recorder = EarthworkBlockingConstraints.GetRecorder(nameof(EarthworkBlockingForm), m_Doc);
            var blockingStr = "";
            recorder.ReadValue(SaveKeyHelper.GetSaveKeyOfEarthworkBlocking(), ref blockingStr, recorder.GetValueAsInt(SaveKeyHelper.GetSaveKeyOfEarthworkBlockingSize(), 1000) + 2);
            if (blockingStr != "")
                Blocking = Newtonsoft.Json.JsonConvert.DeserializeObject<EarthworkBlocking>(blockingStr);
            else
                Blocking = new EarthworkBlocking();
            Blocking.InitByDocument(m_Doc);
            Blocking.Start();
            if (Blocking.Count() > 0)
                dgv_Blocks.DataSource = Blocking.Blocks;
            //dgv_ConstructionInfo
            dgv_ImplementationInfo.AutoGenerateColumns = false;
            ConstructionNode_Name.DataPropertyName = nameof(EarthworkBlockImplementationInfo.Name);
            ConstructionNode_Name.ReadOnly = true;
            ConstructionNode_StartTime.DataPropertyName = nameof(EarthworkBlockImplementationInfo.StartTimeStr);
            ConstructionNode_StartTime.Tag = nameof(DateTime);
            ConstructionNode_ExposureTime.DataPropertyName = nameof(EarthworkBlockImplementationInfo.ExposureTime);
            ConstructionNode_EndTime.DataPropertyName = nameof(EarthworkBlockImplementationInfo.EndTimeStr);
            ConstructionNode_EndTime.Tag = nameof(DateTime);
            //初始化按钮
            ToolTip tip = new ToolTip();
            tip.SetToolTip(btn_AddNode, "新增节点");
            tip.SetToolTip(btn_DeleteNode, "删除选中节点");
            tip.SetToolTip(btn_UpNode, "上移节点");
            tip.SetToolTip(btn_DownNode, "下移节点");
            tip.SetToolTip(btn_AddElement, "新增构件");
            tip.SetToolTip(btn_DeleteElement, "删除构件");
            //DatePicker
            DatePicker = new DateTimePicker();
            DatePicker.Parent = this;
            DatePicker.Width = 100;
            DatePicker.Format = DateTimePickerFormat.Custom;
            DatePicker.CustomFormat = "yyyy/MM/dd";
            DatePicker.ShowCheckBox = true;
            DatePicker.Hide();
            DatePicker.LostFocus += DatePicker_LostFocus;
            DatePicker.TextChanged += DatePicker_TextChanged;
            //DateTimePicker
            DateTimePicker = new DateTimePicker();
            DatePicker.Parent = this;
            DateTimePicker.Hide();
            DateTimePicker.Format = DateTimePickerFormat.Custom;
            DateTimePicker.CustomFormat = "yyyy/MM/dd HH:mm";
            DateTimePicker.LostFocus += DateTimePicker_LostFocus;
            DateTimePicker.TextChanged += DateTimePicker_TextChanged;
            dgv_ImplementationInfo.Controls.Add(DateTimePicker);
        }
        DateTimePicker DateTimePicker;
        DateTimePicker DatePicker;
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

        #region Tab_土方分块
        /// <summary>
        /// 加载选中行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EarthworkBlockingForm_Load(object sender, EventArgs e)
        {
            if (dgv_Blocks.Rows.Count > 0)
                dgv_Blocks.Rows[0].Selected = true;

            tabControl1_SelectedIndexChanged(null, null);
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
            if (rows == null || rows.Count == 0 || rows[0].DataBoundItem == null)
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
        /// 列表编辑结束
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgv_Blocks_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            //TODO 变更检测
            //TODO 名称更改影响ConstructionInfo
            ValueChanged(sender, e);
        }
        //TODO 存在更改时,关闭时提示保存
        bool IsChanged = false;
        private void ValueChanged(object sender, EventArgs e)
        {
            IsChanged = true;
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
        #endregion

        #region Tab_Common
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
        /// 预览
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Preview_Click(object sender, System.EventArgs e)
        {
            //TODO 预览 如果非当前视图,则打开默认视图
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
        /// 消息反馈
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public void ShowMessage(string title, string content)
        {
            MessageBox.Show(content);
        }
        /// <summary>
        /// tab切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                lable_OrderByTime.Hide();
                btn_SortByDate.Hide();
                lbl_BlockingColor.Hide();
                lbl_Completed.Hide();
                lbl_Uncompleted.Hide();
                btn_Completed.Hide();
                btn_Uncompleted.Hide();

                //btn_Preview.Show();
                //btn_Commit.Show();
                //btn_Cancel.Show();
                //btn_Save.Show();
            }
            else
            {
                ////打开"实际施工节点信息管理".加载对应的窗体信息,因为dgv_实际施工节点信息管理总是受dgv_土方分块影响
                //dgv_ImplementationInfo.DataSource = null;
                //var implementationInfos = Blocking.GetBlockingImplementationInfos();
                //dgv_ImplementationInfo.DataSource = implementationInfos;
                ////TODO 分段内容有变动 高亮or...
                //if (implementationInfos.FirstOrDefault(c => c.IsConflicted != false) != null)
                //{
                //    TaskDialog.Show("警告", "分段内容存在变动,变化部分已高亮显示");
                //}

                if (string.IsNullOrEmpty(btn_SortByDate.Text))
                    btn_SortByDate.Text = SortAll;
                btn_SortByDate_TextChanged(null, null);//打开"实际施工节点信息管理".加载对应的窗体信息,因为dgv_实际施工节点信息管理总是受dgv_土方分块影响
                lable_OrderByTime.Show();
                btn_SortByDate.Show();
                lbl_BlockingColor.Show();
                lbl_Completed.Show();
                lbl_Uncompleted.Show();
                btn_Completed.Show();
                btn_Uncompleted.Show();

                //btn_Preview.Show();
                //btn_Commit.Show();
                //btn_Cancel.Show();
                //btn_Save.Show();
            }
        }
        #endregion

        #region Tab_实际施工节点信息管理
        /// <summary>
        /// dgv_Implementation更新首列序号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgv_ImplementationInfo_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SolidBrush b = new SolidBrush(this.dgv_Blocks.RowHeadersDefaultCellStyle.ForeColor);
            e.Graphics.DrawString((e.RowIndex + 1).ToString(System.Globalization.CultureInfo.CurrentUICulture)
                , this.dgv_Blocks.DefaultCellStyle.Font, b, e.RowBounds.Location.X + 20, e.RowBounds.Location.Y + 4);
        }
        public EarthworkBlockImplementationInfo ImplementationInfo { set; get; }
        /// <summary>
        /// 选中项变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgv_ImplementationInfo_SelectionChanged(object sender, EventArgs e)
        {
            var cell = dgv_ImplementationInfo.CurrentCell;
            if (ImplementationInfo != Blocking.Blocks[cell.RowIndex].ImplementationInfo)
            {
                ImplementationInfo = Blocking.Blocks[cell.RowIndex].ImplementationInfo;
                RenderColorButton(btn_Completed, ImplementationInfo.ColorForSettled);
                RenderColorButton(btn_Uncompleted, ImplementationInfo.ColorForUnsettled);
            }
        }
        /// <summary>
        /// 渲染选中颜色到按钮
        /// </summary>
        /// <param name="button"></param>
        /// <param name="color"></param>
        private void RenderColorButton(Button button, System.Drawing.Color color)
        {
            int width = button.Width;
            var height = button.Height;
            var image = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(image);
            graphics.FillRectangle(new SolidBrush(color), new System.Drawing.Rectangle(0, 0, width, height));
            button.Image = image;
        }
        /// <summary>
        /// 设置"已完工"颜色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Completed_Click(object sender, EventArgs e)
        {
            if (dgv_ImplementationInfo.Rows.Count == 0)
                return;

            //禁止使用自定义颜色  
            colorDialog1.AllowFullOpen = false;
            //提供自己给定的颜色  
            colorDialog1.CustomColors = new int[] { 6916092, 15195440, 16107657, 1836924, 3758726, 12566463, 7526079, 7405793, 6945974, 241502, 2296476, 5130294, 3102017, 7324121, 14993507, 11730944 };
            colorDialog1.ShowHelp = true;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                if (colorDialog1.Color != ImplementationInfo.ColorForSettled)
                {
                    ImplementationInfo.ColorForSettled = colorDialog1.Color;
                    RenderColorButton(btn_Completed, ImplementationInfo.ColorForSettled);
                    ValueChanged(sender, e);
                }
            }
        }
        /// <summary>
        /// 设置"未完工"颜色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Uncompleted_Click(object sender, EventArgs e)
        {
            if (dgv_ImplementationInfo.Rows.Count == 0)
                return;

            //禁止使用自定义颜色  
            colorDialog1.AllowFullOpen = false;
            //提供自己给定的颜色  
            colorDialog1.CustomColors = new int[] { 6916092, 15195440, 16107657, 1836924, 3758726, 12566463, 7526079, 7405793, 6945974, 241502, 2296476, 5130294, 3102017, 7324121, 14993507, 11730944 };
            colorDialog1.ShowHelp = true;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                if (colorDialog1.Color != ImplementationInfo.ColorForUnsettled)
                {
                    ImplementationInfo.ColorForUnsettled = colorDialog1.Color;
                    RenderColorButton(btn_Uncompleted, ImplementationInfo.ColorForUnsettled);
                    ValueChanged(sender, e);
                }
            }
        }
        public List<int> CellLocation;
        /// <summary>
        /// 进入编辑状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgv_ImplementationInfo_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            DataGridViewTextBoxColumn textbox = dgv_ImplementationInfo.Columns[e.ColumnIndex] as DataGridViewTextBoxColumn;
            if (textbox != null) //如果该列是TextBox列
            {
                if (textbox.Tag != null && textbox.Tag.ToString() == nameof(DateTime))
                {
                    if (dgv_ImplementationInfo.Rows.Count != 0)
                    {
                        DataGridViewCellCancelEventArgs = null;
                        var text = dgv_ImplementationInfo[e.ColumnIndex, e.RowIndex].Value.ToString();
                        DateTimePicker.Value = string.IsNullOrEmpty(text) ? DateTime.Now : DateTime.Parse(text);
                        DataGridViewCellCancelEventArgs = e;
                        var rectangle = dgv_ImplementationInfo.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                        DateTimePicker.Size = new Size(rectangle.Width, rectangle.Height);
                        DateTimePicker.Location = new System.Drawing.Point(rectangle.X, rectangle.Y);
                        DateTimePicker.Show();
                    }
                }
            }
        }
        DataGridViewCellCancelEventArgs DataGridViewCellCancelEventArgs;
        ///// <summary>
        ///// 双击
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void dgv_ConstructionInfo_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        //{
        //    int CIndex = e.ColumnIndex;
        //    if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
        //    {
        //        DataGridViewTextBoxColumn textbox = dgv_ImplementationInfo.Columns[e.ColumnIndex] as DataGridViewTextBoxColumn;
        //        if (textbox != null) //如果该列是TextBox列
        //        {

        //            if (textbox.Tag.ToString() == nameof(DateTime))
        //            {
        //                if (dgv_ImplementationInfo.Rows.Count != 0)
        //                {
        //                    DataGridViewCellEventArgs = null;
        //                    var text = dgv_ImplementationInfo[e.ColumnIndex, e.RowIndex].Value.ToString();
        //                    DateTimePicker.Value = string.IsNullOrEmpty(text) ? DateTime.Now : DateTime.Parse(text);
        //                    DataGridViewCellEventArgs = e;
        //                    var rectangle = dgv_ImplementationInfo.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
        //                    DateTimePicker.Size = new Size(rectangle.Width, rectangle.Height);
        //                    DateTimePicker.Location = new System.Drawing.Point(rectangle.X, rectangle.Y);
        //                    DateTimePicker.Show();
        //                }
        //            }
        //            else
        //            {
        //                dgv_ImplementationInfo.BeginEdit(true); //开始编辑状态
        //            }

        //            //CellLocation = new List<int>();
        //            //if (textbox.Tag.ToString() == nameof(DateTime))
        //            //{
        //            //    CellLocation.Add(e.RowIndex);
        //            //    CellLocation.Add(e.ColumnIndex);
        //            //    //TODO 日期的编辑处理

        //            //    if (IsDateTimePickerFocused)
        //            //    {
        //            //        IsDateTimePickerFocused = false;
        //            //    }
        //            //    else
        //            //    {
        //            //        //var btnLocation = dgv_ImplementationInfo.Rows[CellLocation[0]].Cells[CellLocation[1]] as DataGridViewTextBoxColumn;
        //            //        //DatePicker.Location = new System.Drawing.Point(btnLocation.X - (DatePicker.Width - btn_SortByDate.Width) / 2, btnLocation.Y + 24);
        //            //        //DatePicker.Show();
        //            //        //DatePicker.BringToFront();
        //            //        //DatePicker.Focus();
        //            //        //IsDateTimePickerFocused = true;
        //            //    }
        //            //}
        //        }
        //    }
        //}
        /// <summary>
        /// 文本变更事件,检测日期限制,总是将DateTimePicker的内容更新到Cell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DateTimePicker_TextChanged(object sender, EventArgs e)
        {
            //if (DataGridViewCellEventArgs != null)
            //{
            //    if (DateTimePicker.Value < DateTime.Now)
            //    {
            //        MessageBox.Show("设置的时间必须大于当前时间");
            //        return;
            //    }
            //    dgv_ImplementationInfo[DataGridViewCellEventArgs.ColumnIndex, DataGridViewCellEventArgs.RowIndex].Value = DateTimePicker.Value;
            //}
        }
        /// <summary>
        /// DateTimePicker失去焦点,认为此时完成编辑,隐藏控件,并退出编辑模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DateTimePicker_LostFocus(object sender, EventArgs e)
        {
            if (DataGridViewCellCancelEventArgs != null)
            {
                if (DateTimePicker.Value < DateTime.Now)
                {
                    MessageBox.Show("设置的时间必须大于当前时间");
                    return;
                }
                dgv_ImplementationInfo[DataGridViewCellCancelEventArgs.ColumnIndex, DataGridViewCellCancelEventArgs.RowIndex].Value = DateTimePicker.Value;
            }
            DateTimePicker.Hide();
            dgv_ImplementationInfo.EndEdit();
        }
        /// <summary>
        /// 退出编辑状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgv_ImplementationInfo_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            //TODO发生变更检测
            ValueChanged(sender, e);
        }
        /// <summary>
        /// 根据时间过滤
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_SortByTime_Click(object sender, EventArgs e)
        {
            if (IsDatePickerFocused)
            {
                IsDatePickerFocused = false;
            }
            else
            {
                var btnLocation = btn_SortByDate.Location;
                DatePicker.Location = new System.Drawing.Point(btnLocation.X - (DatePicker.Width - btn_SortByDate.Width) / 2, btnLocation.Y + 24);
                DatePicker.Show();
                DatePicker.BringToFront();
                DatePicker.Focus();
                IsDatePickerFocused = true;
            }
        }
        /// <summary>
        /// DatePicker失去焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DatePicker_LostFocus(object sender, EventArgs e)
        {
            DatePicker_TextChanged(null, null);
            DatePicker.Hide();

        }
        string SortAll = "全部";
        /// <summary>
        /// 文本改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DatePicker_TextChanged(object sender, EventArgs e)
        {
            if (DatePicker.Checked)
            {
                btn_SortByDate.Text = DatePicker.Text;
            }
            else
            {
                btn_SortByDate.Text = SortAll;
            }
        }
        /// <summary>
        /// 时间过滤_文本更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_SortByDate_TextChanged(object sender, EventArgs e)
        {
            ////TODO 分段内容有变动 高亮or...
            //if (implementationInfos.FirstOrDefault(c => c.IsConflicted != false) != null)
            //{
            //    TaskDialog.Show("警告", "分段内容存在变动,变化部分已高亮显示");
            //}
            if (btn_SortByDate.Text == SortAll)
            {
                dgv_ImplementationInfo.DataSource = null;
                if (Blocking.Blocks.Count > 0)
                    dgv_ImplementationInfo.DataSource = Blocking.GetBlockingImplementationInfos();
            }
            else
            {
                var date = DateTime.Parse(btn_SortByDate.Text);
                dgv_ImplementationInfo.DataSource = null;
                if (Blocking.Blocks.Count > 0)
                    dgv_ImplementationInfo.DataSource = Blocking.GetBlockingImplementationInfos().Where(c => c.StartTime.Date == date.Date);
            }
        }
        bool IsDatePickerFocused { get; set; }
        /// <summary>
        /// Form的Click事件,主要用于DateTimePicker控件的焦点解除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EarthworkBlockingForm_Click(object sender, EventArgs e)
        {
            if (DatePicker.Focused)
            {
                var contained = DatePicker.Bounds.Contains((e as MouseEventArgs).X, (e as MouseEventArgs).Y);
                this.Focus();
            }
        }



        #endregion
    }
}
