using MyRevit.SubsidenceMonitor.Entities;
using MyRevit.SubsidenceMonitor.Operators;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MyRevit.SubsidenceMonitor.UI
{
    public partial class ChartForm : Form
    {
        public ChartForm()
        {
            InitializeComponent();

            //DatePicker 日期选择辅助
            DatePicker = new DateTimePicker()
             {
                 Format = DateTimePickerFormat.Custom,
                 CustomFormat = "yyyy/MM/dd",
                 //ShowCheckBox = true,
                 Width = 100,
                 Visible = false,
             };
            DatePicker.Parent = this;
            DatePicker.LostFocus += DatePicker_LostFocus;
            //监测类型
            var types = Enum.GetNames(typeof(EIssueType)).Select(c => new { Name = c, Value = (int)Enum.Parse(typeof(EIssueType), c) }).ToList();
            cb_IssueType.DisplayMember = "Name";
            cb_IssueType.ValueMember = "Value";
            cb_IssueType.DataSource = types;
            cb_IssueType.TextChanged += Cb_IssueType_TextChanged;
            //测点编码
            cb_NodeCode.TextChanged += Cb_NodeCode_TextChanged;
            //深度
            cb_Depth.TextChanged += Cb_Depth_TextChanged;
            //Y轴
            cb_Y.DisplayMember = nameof(TypeKeyDdscription.Description);
            cb_Y.ValueMember = nameof(TypeKeyDdscription.Key);
            cb_Y.TextChanged += Cb_Y_TextChanged;
            //日期区间
            btn_StartDate.TextChanged += Btn_StartDate_TextChanged;
            btn_StartDate.Enabled = false;
            //Chart
            //Series.Value 节点值显示
            chart1.Series[0].IsValueShownAsLabel = true;
            //Chart.ChartAreas.AxisX  AxisX轴样式设置
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "MM.dd";
            chart1.ChartAreas[0].AxisX.LabelStyle.Angle = 90;
            chart1.ChartAreas[0].AxisX.TextOrientation = System.Windows.Forms.DataVisualization.Charting.TextOrientation.Auto;
            chart1.ChartAreas[0].AxisX.Interval = 1;
            chart1.ChartAreas[0].AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Days;
            chart1.ChartAreas[0].AxisX.IntervalOffset = 1;
            chart1.ChartAreas[0].AxisX.LabelStyle.IsStaggered = true;
            //Chart.ChartAreas.Axis.MajorGrid.LineColor 背景网格样式设置
            chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            //Series.ChartType 图表类型
            chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            //Series.Border 节点间连线样式
            chart1.Series[0].BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chart1.Series[0].BorderWidth = 2;
            chart1.Series[0].BorderColor = Color.Blue;
            //Series.Marker 节点的点样式
            chart1.Series[0].MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Square;
            chart1.Series[0].MarkerColor = Color.Blue;
            chart1.Series[0].MarkerSize = 10;
            //无需Legends 右上角Series说明
            chart1.Legends.Clear();
        }

        #region 级联选中处理
        /// <summary>
        /// 当前选中的监测类型
        /// </summary>
        public EIssueType CurrentType { set; get; }
        /// <summary>
        /// 监测类型变更
        /// 级联影响(监测点,深度)
        /// </summary>

        private void Cb_IssueType_TextChanged(object sender, EventArgs e)
        {
            var issueType = (EIssueType)Enum.Parse(typeof(EIssueType), cb_IssueType.Text);
            CurrentType = issueType;
            if (CurrentType != EIssueType.未指定)
            {
                //Y轴选项 级联处理
                cb_Y.DisplayMember = nameof(TypeKeyDdscription.Description);
                cb_Y.ValueMember = nameof(TypeKeyDdscription.Key);
                DataTable table = new DataTable();
                var datas = TypeKeyDiscriptions.Where(c => c.Type == issueType).ToList();
                table.Columns.Add(new DataColumn(nameof(TypeKeyDdscription.Description), typeof(string)));
                table.Columns.Add(new DataColumn(nameof(TypeKeyDdscription.Key), typeof(string)));
                table.Columns.Add(new DataColumn(nameof(TypeKeyDdscription.Type), typeof(string)));
                foreach (var data in datas)
                {
                    var row = table.NewRow();
                    row[nameof(TypeKeyDdscription.Description)] = data.Description;
                    row[nameof(TypeKeyDdscription.Key)] = data.Key;
                    table.Rows.Add(row);
                }
                cb_Y.DataSource = table;
            }
            switch (issueType)
            {
                case EIssueType.未指定:
                    //清空监测点
                    Clear(cb_NodeCode);
                    break;
                case EIssueType.建筑物沉降:
                case EIssueType.地表沉降:
                case EIssueType.管线沉降_有压:
                case EIssueType.管线沉降_无压:
                case EIssueType.钢支撑轴力监测:
                    //监测点 级联处理
                    var nodeCodes = Facade.GetNodeCodesByType(issueType);
                    cb_NodeCode.DataSource = null;
                    cb_NodeCode.DataSource = nodeCodes;
                    //重置当前值
                    Clear(cb_Depth);
                    cb_Depth.Hide();
                    lbl_Depth.Hide();
                    break;
                case EIssueType.侧斜监测:
                    //监测点 级联处理
                    nodeCodes = Facade.GetNodeCodesByType(issueType);
                    cb_NodeCode.DataSource = null;
                    cb_NodeCode.DataSource = nodeCodes;
                    //深度 控件隐藏
                    cb_Depth.Show();
                    lbl_Depth.Show();
                    break;
                default:
                    throw new NotImplementedException("暂不支持该类型");
            }
        }
        #region TypeKeyDiscriptions
        class TypeKeyDdscription
        {
            public EIssueType Type;
            public string Key;
            public string Description;

            public TypeKeyDdscription(EIssueType type, string key, string description)
            {
                Type = type;
                Key = key;
                Description = description;
            }
        }
        List<TypeKeyDdscription> TypeKeyDiscriptions = new List<TypeKeyDdscription>()
        {
            new TypeKeyDdscription(EIssueType.建筑物沉降,nameof(BuildingSubsidenceDataV1.CurrentChanges),"本次变量(mm)"),
            new TypeKeyDdscription(EIssueType.建筑物沉降,nameof(BuildingSubsidenceDataV1.SumChanges),"累计变量(mm)"),
            new TypeKeyDdscription(EIssueType.建筑物沉降,nameof(BuildingSubsidenceDataV1.SumPeriodBuildingEnvelope),"围护结构施工期间累计值(mm)"),
            new TypeKeyDdscription(EIssueType.建筑物沉降,nameof(BuildingSubsidenceDataV1.SumBuildingEnvelope),"总累计值(mm)"),

            new TypeKeyDdscription(EIssueType.地表沉降,nameof(SurfaceSubsidenceDataV1.CurrentChanges),"本次变量(mm)"),
            new TypeKeyDdscription(EIssueType.地表沉降,nameof(SurfaceSubsidenceDataV1.SumChanges),"本次变量(mm)"),

            new TypeKeyDdscription(EIssueType.管线沉降_无压,nameof(UnpressedPipeLineSubsidenceDataV1.CurrentChanges),"本次变量(mm)"),
            new TypeKeyDdscription(EIssueType.管线沉降_无压,nameof(UnpressedPipeLineSubsidenceDataV1.SumChanges),"累计变量(mm)"),
            new TypeKeyDdscription(EIssueType.管线沉降_无压,nameof(UnpressedPipeLineSubsidenceDataV1.SumPeriodBuildingEnvelope),"围护结构施工期间累计值(mm)"),
            new TypeKeyDdscription(EIssueType.管线沉降_无压,nameof(UnpressedPipeLineSubsidenceDataV1.SumBuildingEnvelope),"总累计值(mm)"),

            new TypeKeyDdscription(EIssueType.管线沉降_有压,nameof(PressedPipeLineSubsidenceDataV1.CurrentChanges),"本次变量(mm)"),
            new TypeKeyDdscription(EIssueType.管线沉降_有压,nameof(PressedPipeLineSubsidenceDataV1.SumChanges),"累计变量(mm)"),
            new TypeKeyDdscription(EIssueType.管线沉降_有压,nameof(PressedPipeLineSubsidenceDataV1.SumPeriodBuildingEnvelope),"围护结构施工期间累计值(mm)"),
            new TypeKeyDdscription(EIssueType.管线沉降_有压,nameof(PressedPipeLineSubsidenceDataV1.SumBuildingEnvelope),"总累计值(mm)"),

            new TypeKeyDdscription(EIssueType.钢支撑轴力监测,nameof(STBAPDataV1.CurrentChanges),"本次变量(mm)"),
            new TypeKeyDdscription(EIssueType.钢支撑轴力监测,nameof(STBAPDataV1.SumChanges),"累计变量(mm)"),

            new TypeKeyDdscription(EIssueType.侧斜监测,nameof(SkewBackDataV1.PreviousChange),"上次位移量(mm)"),
            new TypeKeyDdscription(EIssueType.侧斜监测,nameof(SkewBackDataV1.CurrentChange),"本次位移量(mm)"),
            new TypeKeyDdscription(EIssueType.侧斜监测,nameof(SkewBackDataV1.PreviousSum),"上次累计(mm)"),
            new TypeKeyDdscription(EIssueType.侧斜监测,nameof(SkewBackDataV1.CurrentSum),"本次累计(mm)"),
        };
        #endregion
        //void Clear(ComboBox control)
        //{
        //    control.Text = "";
        //}
        void Clear(Control control)
        {
            control.Text = "";
        }
        /// <summary>
        /// 测点编号变更
        /// 级联影响(Y轴,深度)or(深度)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cb_NodeCode_TextChanged(object sender, EventArgs e)
        {
            switch (CurrentType)
            {
                case EIssueType.未指定:
                    //重置当前值
                    Clear(cb_Y);
                    Clear(cb_Depth);
                    break;
                case EIssueType.建筑物沉降:
                case EIssueType.地表沉降:
                case EIssueType.管线沉降_有压:
                case EIssueType.管线沉降_无压:
                case EIssueType.钢支撑轴力监测:
                    //重置当前值
                    Clear(cb_Y);
                    Clear(cb_Depth);
                    break;
                case EIssueType.侧斜监测:
                    //重置当前值
                    Clear(cb_Y);
                    //更新深度选项
                    var depths = Facade.GetDepthsByNodeCode(cb_NodeCode.Text);
                    cb_NodeCode.DataSource = null;
                    cb_NodeCode.DataSource = depths;
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 深度选项变更
        /// 级联影响(Y轴)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cb_Depth_TextChanged(object sender, EventArgs e)
        {
            //重置当前值
            Clear(cb_Y);
        }
        /// <summary>
        /// Y轴选项变更
        /// 级联影响(日期区间)
        /// </summary>
        private void Cb_Y_TextChanged(object sender, EventArgs e)
        {
            //重置当前值
            Clear(btn_StartDate);
            if (string.IsNullOrEmpty(cb_Y.Text))
                btn_StartDate.Enabled = false;
            else
                btn_StartDate.Enabled = true;
        }
        /// <summary>
        /// 日期区间变更
        /// 级联影响(图表数据)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_StartDate_TextChanged(object sender, EventArgs e)
        {
            DateTime date = DateTime.MinValue;
            if (!DateTime.TryParse(btn_StartDate.Text, out date))
                return;
            int range = 30;
            string yField = cb_Y.SelectedValue.ToString();
            string nodeCode = cb_NodeCode.Text;
            var type = CurrentType;
            switch (type)
            {
                case EIssueType.未指定:
                    break;
                case EIssueType.建筑物沉降:
                case EIssueType.地表沉降:
                case EIssueType.管线沉降_有压:
                case EIssueType.管线沉降_无压:
                case EIssueType.钢支撑轴力监测:
                    var datas = Facade.GetDateTimeValues(type, nodeCode, yField, date, range);
                    chart1.Series[0].Points.Clear();
                    foreach (var data in datas)
                    {
                        chart1.Series[0].Points.AddXY(data.DateTime, data.Value);
                    }
                    break;
                case EIssueType.侧斜监测:
                    datas = Facade.GetDateTimeValues(type, nodeCode, cb_Depth.Text, yField, date, range);
                    chart1.Series[0].Points.Clear();
                    foreach (var data in datas)
                    {
                        chart1.Series[0].Points.AddXY(data.DateTime, data.Value);
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region DatePicker
        private void DatePicker_LostFocus(object sender, EventArgs e)
        {
            btn_StartDate.Text = DatePicker.Text;
            DatePicker.Hide();
        }
        DateTimePicker DatePicker { set; get; }
        bool IsDatePickerFocused { get; set; }
        private void btn_StartDate_Click(object sender, EventArgs e)
        {
            if (IsDatePickerFocused)
            {
                IsDatePickerFocused = false;
            }
            else
            {
                var btnLocation = btn_StartDate.Location;
                DatePicker.Show();
                DatePicker.Location = new Point(btnLocation.X - (DatePicker.Width - btn_StartDate.Width) / 2, btnLocation.Y + 24);
                DatePicker.BringToFront();
                DatePicker.Focus();
                IsDatePickerFocused = true;
            }
        }
        private void ChartForm_Click(object sender, EventArgs e)
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
