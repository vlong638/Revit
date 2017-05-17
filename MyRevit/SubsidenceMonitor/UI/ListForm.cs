using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MyRevit.SubsidenceMonitor.Entities;
using MyRevit.SubsidenceMonitor.Operators;
using Autodesk.Revit.UI;
using PmSoft.Common.CommonClass;
using MyRevit.Utilities;
using Newtonsoft.Json;

namespace MyRevit.SubsidenceMonitor.UI
{
    public partial class ListForm : System.Windows.Forms.Form
    {
        #region Init
        public ListForm(UIDocument ui_doc)
        {
            InitializeComponent();

            InitControls();
            UI_Doc = ui_doc;
            FaceRecorderForRevit recorder = PMSoftHelper.GetRecorder(nameof(WarnSettings), ui_doc.Document);
            var str = recorder.GetValue(SaveKeyHelper.GetSaveKey(SaveKeyHelper.SaveKeyTypeForSubsidenceMonitor.WarnSettings, 1), "", 1000);
            if (!string.IsNullOrEmpty(str))
                WarnSettings = JsonConvert.DeserializeObject<WarnSettings>(str);
        }

        public WarnSettings WarnSettings { set; get; } = new WarnSettings();
        public ShowDialogType ShowDialogType { set; get; }
        public SubFormType SubFormType { set; get; }
        public SubsidenceMonitorForm SubFormForSubsidence { set; get; }
        public SkewBackMonitorForm SubFormForSkewBack { set; get; }
        protected UIDocument UI_Doc { set; get; }
        private void InitControls()
        {
            //MonthPicker
            MonthPicker = new DateTimePicker();
            MonthPicker.Parent = this;
            MonthPicker.Width = 100;
            MonthPicker.Location = new System.Drawing.Point(btn_IssueMonth.Location.X - (MonthPicker.Width - btn_IssueMonth.Width) / 2, btn_IssueMonth.Location.Y + 24);
            MonthPicker.Format = DateTimePickerFormat.Custom;
            MonthPicker.CustomFormat = "   yyyy/MM";
            MonthPicker.ShowUpDown = true;
            MonthPicker.LostFocus += MonthPicker_LostFocus;
            MonthPicker.Hide();
            //btn_IssueMonth
            btn_IssueMonth.TextChanged += Btn_IssueMonth_TextChanged;
            //cb_IssueType
            var types = Enum.GetNames(typeof(EIssueType)).Select(c => new { Name = c, Value = (int)Enum.Parse(typeof(EIssueType), c) }).ToList();
            cb_IssueType.DisplayMember = "Name";
            cb_IssueType.ValueMember = "Value";
            cb_IssueType.DataSource = types;
            //dgv
            dgv.AutoGenerateColumns = false;
            dgv_Date.DataPropertyName = nameof(TList.IssueDate);
            dgv_Imported.DataPropertyName = nameof(TList.dgv_Imported);
            dgv_Operation.DataPropertyName = nameof(TList.dgv_Operation);
            //form
            this.Shown += ListForm_Shown;
        }
        private void ListForm_Shown(object sender, EventArgs e)
        {
            switch (ShowDialogType)
            {
                case ShowDialogType.Idle:
                    SubFormType = SubFormType.None;
                    break;
                case ShowDialogType.AddElements_ForDetail:
                case ShowDialogType.DeleleElements_ForDetail:
                case ShowDialogType.ViewElementsBySelectedNodes:
                case ShowDialogType.ViewElementsByAllNodes:
                case ShowDialogType.ViewCurrentMaxByRed:
                case ShowDialogType.ViewCurrentMaxByAll:
                case ShowDialogType.ViewTotalMaxByRed:
                case ShowDialogType.ViewTotalMaxByAll:
                case ShowDialogType.ViewCloseWarn:
                case ShowDialogType.ViewOverWarn:
                    switch (SubFormType)
                    {
                        case SubFormType.Subsidence:
                            SubFormForSubsidence.ShowDialog();
                            CurrentLists.FirstOrDefault(c => c.IssueDate == SubFormForSubsidence.Model.MemorableData.Data.List.IssueDate).Datas = SubFormForSubsidence.Model.MemorableData.Data.List.Datas;
                            dgv.DataSource = null;
                            dgv.DataSource = CurrentLists;
                            break;
                        case SubFormType.SkewBack:
                            SubFormForSkewBack.ShowDialog();
                            CurrentLists.FirstOrDefault(c => c.IssueDate == SubFormForSkewBack.Model.MemorableData.Data.List.IssueDate).Datas = SubFormForSkewBack.Model.MemorableData.Data.List.Datas;
                            dgv.DataSource = null;
                            dgv.DataSource = CurrentLists;
                            break;
                    }
                    break;
                default:
                    throw new NotImplementedException("暂未支持该类型");
            }
            //ShowDialogType = ShowDialogType.Idle;
        }
        #endregion

        DateTimePicker MonthPicker { set; get; }
        bool IsMonthPickerFocused { set; get; }
        string TextAll = "全部";
        string TextEmpty = "";
        /// <summary>
        /// MonthPicker失去焦点,主要用于更新显示文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonthPicker_LostFocus(object sender, EventArgs e)
        {
            if (MonthPicker.Checked)
            {
                btn_IssueMonth.Text = MonthPicker.Text.TrimStart();
            }
            else
            {
                btn_IssueMonth.Text = TextAll;
            }
            MonthPicker.Hide(); ;
        }
        /// <summary>
        /// 日期按钮点击,根据显示情况切换日期选择控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_IssueMonth_Click(object sender, EventArgs e)
        {
            if (IsMonthPickerFocused)
            {
                IsMonthPickerFocused = false;
            }
            else
            {
                var btn = btn_IssueMonth;
                MonthPicker.Show();
                MonthPicker.BringToFront();
                MonthPicker.Focus();
                IsMonthPickerFocused = true;
            }
        }
        /// <summary>
        /// Form的Click事件,主要用于DateTimePicker控件的焦点解除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListForm_Click(object sender, EventArgs e)
        {
            if (MonthPicker.Focused)
            {
                var contained = MonthPicker.Bounds.Contains((e as MouseEventArgs).X, (e as MouseEventArgs).Y);
                this.Focus();
            }
        }
        /// <summary>
        /// 报告类型变更,清空月份文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_IssueType_SelectedIndexChanged(object sender, EventArgs e)
        {
            btn_IssueMonth.Text = TextEmpty;
        }
        /// <summary>
        /// 月份文本更改,加载DataGridView的内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_IssueMonth_TextChanged(object sender, EventArgs e)
        {
            if ((int)cb_IssueType.SelectedValue != 0 && btn_IssueMonth.Text != TextEmpty)
            {
                var issueType = (EIssueType)Enum.Parse(typeof(EIssueType), cb_IssueType.SelectedValue.ToString());
                var yearMonth = DateTime.Parse(btn_IssueMonth.Text);
                var year = yearMonth.Year;
                var month = yearMonth.Month;
                var dayLists = new List<DateTime>();
                var tempDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Local);
                while (tempDate.Month == month)
                {
                    dayLists.Add(tempDate);
                    tempDate = tempDate.AddDays(1);
                }
                //根据类型和月份加载列表数据
                var loadedLists = Facade.GetLists(issueType, new DateTime(year, month, 1));
                //构建完整列表
                CurrentLists = dayLists.Select(c => new TList(issueType, c)).ToList();
                foreach (var loadedList in loadedLists)
                {
                    var list = CurrentLists.First(c => c.IssueDate == loadedList.IssueDate);
                    list.DataCount = loadedList.DataCount;
                }
                dgv.DataSource = null;
                dgv.DataSource = CurrentLists;
            }
            else
            {
                dgv.DataSource = null;
            }
        }
        List<TList> CurrentLists { set; get; }
        /// <summary>
        /// Cell内容点击事件,主要负责按钮的点击检测
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            var column = dgv.Columns[e.ColumnIndex] as DataGridViewButtonColumn;
            if (column != null && dgv.Rows[e.RowIndex].DataBoundItem != null)
            {
                var list = dgv.Rows[e.RowIndex].DataBoundItem as TList;
                switch (list.IssueType)
                {
                    case EIssueType.建筑物沉降:
                    case EIssueType.地表沉降:
                    case EIssueType.管线沉降_无压:
                    case EIssueType.管线沉降_有压:
                    case EIssueType.钢支撑轴力监测:
                        if (list.Datas.Count() == 0 && list.DataCount > 0)
                            Facade.FetchDetails(list);
                        SubFormForSubsidence = new SubsidenceMonitorForm(this, UI_Doc, list);
                        SubFormForSubsidence.ShowDialog();
                        CurrentLists.FirstOrDefault(c => c.IssueDate == list.IssueDate).Datas = list.Datas;
                        dgv.DataSource = null;
                        dgv.DataSource = CurrentLists;
                        break;
                    case EIssueType.侧斜监测:
                        if (list.Datas.Count() == 0 && list.DataCount > 0)
                            Facade.FetchDetails(list);
                        SubFormForSkewBack = new SkewBackMonitorForm(this, UI_Doc, list);
                        SubFormForSkewBack.ShowDialog();
                        CurrentLists.FirstOrDefault(c => c.IssueDate == list.IssueDate).Datas = list.Datas;
                        dgv.DataSource = null;
                        dgv.DataSource = CurrentLists;
                        break;
                    default:
                        throw new NotImplementedException("未支持该类型的ITNodeDataCollection:" + list.IssueType.ToString());
                }
                if (ShowDialogType != ShowDialogType.Idle)
                {
                    DialogResult = DialogResult.Retry;
                    Close();
                }
                return;
            }
        }

        private void btn_Chart_Click(object sender, EventArgs e)
        {
            new ChartForm().ShowDialog();
        }
    }
}
