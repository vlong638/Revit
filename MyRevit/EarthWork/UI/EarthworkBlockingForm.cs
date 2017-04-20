using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.EarthWork.Entity;
using PmSoft.Common.RevitClass;
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
        public ShowDialogType ShowDialogType { set; get; }
        public DataGridViewRow Row { set; get; }
        public EarthworkBlock Block { set; get; }
        public List<ElementId> ElementIds { set; get; } = new List<ElementId>();
        public UIApplication m_UIApp;
        public UIDocument m_UIDoc;
        public Document m_Doc;

        /// <summary>
        /// 测试处理
        /// </summary>
        public EarthworkBlockingForm()
        {
            InitializeComponent();

            InitForm();
        }

        private void InitForm()
        {
            //初始化参数
            TopMost = true;
            ShowDialogType = ShowDialogType.Idle;
            //初始化DataGridView
            PmSoft.Common.CommonClass.FaceRecorderForRevit recorder = new PmSoft.Common.CommonClass.FaceRecorderForRevit(nameof(EarthworkBlockingForm)
                , PmSoft.Common.CommonClass.ApplicationPath.GetCurrentPath(m_Doc));
            var blockingStr = "";
            recorder.ReadValue(nameof(EarthworkBlocking), ref blockingStr, 10000);
            if (blockingStr != "")
                Blocking = Newtonsoft.Json.JsonConvert.DeserializeObject<EarthworkBlocking>(blockingStr);
            else
                Blocking = new EarthworkBlocking();
            Blocking.InitByDocument(m_Doc);
            ////TEST测试数据
            //for (int i = 0; i < 6; i++)
            //{
            //    Blocking.Add(Blocking.CreateNew());
            //}
            dgv_Blocks.AutoGenerateColumns = false;
            Node_Name.DataPropertyName = nameof(EarthworkBlock.Name);
            Node_Description.DataPropertyName = nameof(EarthworkBlock.Description);
            dgv_Blocks.DataSource = Blocking.Blocks;
            //初始化按钮
            ToolTip tip = new ToolTip();
            tip.SetToolTip(btn_AddNode, "新增节点");
            tip.SetToolTip(btn_DeleteNode, "删除选中节点");
            tip.SetToolTip(btn_UpNode, "上移节点");
            tip.SetToolTip(btn_DownNode, "下移节点");
            tip.SetToolTip(btn_AddElement, "新增构件");
            tip.SetToolTip(btn_DeleteElement, "删除构件");
        }

        public EarthworkBlockingForm(UIApplication uiApp)
        {
            InitializeComponent();

            m_UIApp = uiApp;
            m_UIDoc = uiApp.ActiveUIDocument;
            m_Doc = m_UIDoc.Document;
            InitForm();
        }

        public new DialogResult ShowDialog(IWin32Window owner)
        {
            switch (ShowDialogType)
            {
                case ShowDialogType.AddElements:
                    if (ElementIds != null)
                        Block.AddElementIds(Blocking, ElementIds);
                    ShowDialogType = ShowDialogType.Idle;
                    break;
                case ShowDialogType.DeleleElements:
                    if (ElementIds != null)
                        Block.RemoveElementIds(Blocking, ElementIds);
                    ShowDialogType = ShowDialogType.Idle;
                    break;
                default:
                    break;
            }
            return base.ShowDialog(owner);
        }

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
                index = rows[0].Index - 1;// rows[0].Index > 0 ? rows[0].Index - 1 : 0;
            }
            foreach (DataGridViewRow row in rows)
            {
                Blocking.Remove(row.DataBoundItem as EarthworkBlock);
            }
            Updatedgv_BlockWithSelectionChanged(-1, index);
        }
        private bool IsSingleBlockSelected(DataGridViewSelectedRowCollection rows)
        {
            if (rows.Count == 0)
            {
                ShowMessage("警告", "请选中迁移项");
                return false;
            }
            if (rows.Count > 1)
            {
                ShowMessage("警告", "暂不支持多项处理");
                return false;
            }
            Row = rows[0];
            Block = rows[0].DataBoundItem as EarthworkBlock;
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
            if (endIndex >= 0)
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
            btn_Save_Click(sender, e);
            this.Close();
        }
        /// <summary>
        /// 应用-保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Save_Click(object sender, System.EventArgs e)
        {
            //保存数据
            PmSoft.Common.CommonClass.FaceRecorderForRevit recorder = new PmSoft.Common.CommonClass.FaceRecorderForRevit(nameof(EarthworkBlockingForm)
                , PmSoft.Common.CommonClass.ApplicationPath.GetCurrentPath(m_Doc));
            recorder.WriteValue(nameof(EarthworkBlocking), Newtonsoft.Json.JsonConvert.SerializeObject(Blocking));
            //更新视图内容
            Block.CPSettings.ApplySetting(Blocking, Block.ElementIds);
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
            this.ShowDialogType = ShowDialogType.Idle;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
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
            this.Hide();
            this.TopMost = false;

            #region Hook
            using (var mouseHook = new PickObjectsMouseHook())
            {
                try
                {
                    mouseHook.InstallHook();
                    ElementIds = m_UIApp.ActiveUIDocument.Selection.PickObjects(ObjectType.Element, "选择要添加的构件").Select(p => m_Doc.GetElement(p.ElementId).Id).ToList();
                    mouseHook.UninstallHook();
                    if (ElementIds != null)
                        Block.AddElementIds(Blocking, ElementIds);
                }
                catch(Exception ex)
                {
                    ElementIds = null;
                    mouseHook.UninstallHook();
                }
            }
            #endregion
            this.TopMost = true;
            this.Show();
            ShowMessage("添加节点结束", $"节点:{Block.Name}现有:{Block.ElementIds.Count()}个元素");
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
            this.Hide();
            this.TopMost = false;
            #region Hook
            using (var mouseHook = new PickObjectsMouseHook())
            {
                try
                {
                    mouseHook.InstallHook();
                    ElementIds = m_UIApp.ActiveUIDocument.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, "选择要删除的构件").Select(p => m_Doc.GetElement(p.ElementId).Id).ToList();
                    mouseHook.UninstallHook();
                    if (ElementIds != null)
                        Block.RemoveElementIds(Blocking, ElementIds);
                }
                catch
                {
                    ElementIds = null;
                    mouseHook.UninstallHook();
                }
            }
            #endregion
            this.TopMost = true;
            this.Show();
            ShowMessage("添加节点结束", $"节点:{Block.Name}现有:{Block.ElementIds.Count()}个元素");
        }
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
                if (row.Index == Blocking.Count() - 1)
                    return;
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
    }
}
