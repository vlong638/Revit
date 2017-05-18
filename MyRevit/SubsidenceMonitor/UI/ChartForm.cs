using Microsoft.Office.Interop.Excel;
using MyRevit.SubsidenceMonitor.Entities;
using MyRevit.SubsidenceMonitor.Operators;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
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
                System.Data.DataTable table = new System.Data.DataTable();
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

            new TypeKeyDdscription(EIssueType.钢支撑轴力监测,nameof(STBAPDataV1.AxialForce),"本次变量(mm)"),
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
        List<DateTimeValue> Datas = new List<DateTimeValue>();
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
                    Datas = Facade.GetDateTimeValues(type, nodeCode, yField, date, range);
                    chart1.Series[0].Points.Clear();
                    foreach (var data in Datas)
                    {
                        chart1.Series[0].Points.AddXY(data.DateTime, data.Value);
                    }
                    break;
                case EIssueType.侧斜监测:
                    Datas = Facade.GetDateTimeValues(type, nodeCode, cb_Depth.Text, yField, date, range);
                    chart1.Series[0].Points.Clear();
                    foreach (var data in Datas)
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
                DatePicker.Location = new System.Drawing.Point(btnLocation.X - (DatePicker.Width - btn_StartDate.Width) / 2, btnLocation.Y + 24);
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

        private void btn_Export_Click(object sender, EventArgs e)
        {
            //信息准备
            var reportName = tb_ReportName.Text;
            var issueType = cb_IssueType.Text;
            var nodeCode = cb_NodeCode.Text;
            var depth = cb_Depth.Text;
            var yField = cb_Y.Text;
            var startTime = btn_StartDate.Text;
            var datas = Datas;
            #region 导出处理
            //Excel导出
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "EXCEL文件(*.xls)|*.xls";
            if (save.ShowDialog() != DialogResult.OK)
                return;
            var path = save.FileName;
            // 创建Excel对象
            ApplicationClass xlApp = new ApplicationClass();
            // 创建Excel工作薄
            Workbook xlBook = xlApp.Workbooks.Add(true);
            Worksheet xlSheet = (Worksheet)xlBook.Worksheets[1];
            //第一列双倍宽度
            Range range = xlSheet.Cells[1, 1] as Range;
            range.ColumnWidth = double.Parse(range.ColumnWidth.ToString()) * 2;
            //开始输出数据
            int x = 1, y = 1;
            int width = 0;
            //报告名称
            y = SetRangeValue("图表名称:", xlSheet, x, y, 1);
            y = SetRangeValue(reportName, xlSheet, x, y, 4);
            //测点名称
            y = SetRangeValue("测点名称:", xlSheet, x, y, 2);
            y = SetRangeValue(nodeCode, xlSheet, x, y, 3);
            int totalWidth = y - 1;
            x += 1; y = 1;//换行
            //报告类型
            y = SetRangeValue("报告类型:", xlSheet, x, y, 1);
            y = SetRangeValue(issueType, xlSheet, x, y, 4);
            //监测日期起点
            y = SetRangeValue("监测日期起点:", xlSheet, x, y, 2);
            y = SetRangeValue(startTime, xlSheet, x, y, 3);
            x += 1; y = 1;//换行
            //监测日期
            y = SetRangeValue("监测日期", xlSheet, x, y, 1);
            //测点数据
            y = SetRangeValue("测点数据", xlSheet, x, y, 1);
            int dataStartX = x + 1, dataStartY = 1;
            foreach (var data in datas)
            {
                x += 1; y = 1;//换行
                y = SetRangeValue(data.DateTime.ToString("yyyy/MM/dd HH") + "时", xlSheet, x, y, 1);
                y = SetRangeValue(data.Value.ToString(), xlSheet, x, y, 1);
            }
            int dataEndX = x, dataEndY = y - 1;
            //图表
            xlBook.Charts.Add(Missing.Value, Missing.Value, 1, Missing.Value);
            xlBook.ActiveChart.ChartType = XlChartType.xlLineMarkers;//设置图表类型
            xlBook.ActiveChart.Location(XlChartLocation.xlLocationAsObject, xlSheet.Name);//放置到Sheet中
            xlBook.ActiveChart.SetSourceData(xlSheet.get_Range(xlSheet.Cells[dataStartX, dataStartY], xlSheet.Cells[dataEndX, dataEndY])
                , XlRowCol.xlColumns);//设置数据区间
            var chart = xlSheet.Shapes.Item("Chart 1");
            range = xlSheet.get_Range(xlSheet.Cells[1, 1], xlSheet.Cells[dataStartX - 2, 2]);
            chart.Top = float.Parse(range.Height.ToString());
            chart.Left = float.Parse(range.Width.ToString());
            range = xlSheet.get_Range(xlSheet.Cells[dataStartX - 1, 3], xlSheet.Cells[dataEndX, totalWidth]);
            chart.Height = float.Parse(range.Height.ToString());
            chart.Width = float.Parse(range.Width.ToString());
            int formatNum = GetFormatNum(xlApp);
            xlBook.SaveAs(path, formatNum);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(xlSheet);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(xlBook);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp); 
            #endregion
            KillProcess("Excel");
        }
        private static int SetRangeValue(string reportName, Worksheet xlSheet, int x, int y, int width)
        {
            Range range;
            if (width > 1)
            {
                var start = xlSheet.Cells[x, y];
                y += width - 1;
                var end = xlSheet.Cells[x, y];
                range = xlSheet.get_Range(start, end);
                range.MergeCells = true;
            }
            else
            {
                range = xlSheet.Cells[x, y] as Range;
            }
            range.Value = reportName;
            return y += 1;//下一格
        }
        private static void KillProcess(string name)
        {
            System.Diagnostics.Process myproc = new System.Diagnostics.Process();//得到所有打开的进程
            try
            {
                foreach (System.Diagnostics.Process thisproc in System.Diagnostics.Process.GetProcessesByName(name))
                {
                    if (!thisproc.CloseMainWindow())
                    {
                        thisproc.Kill();
                    }
                }
            }
            catch { }
        }
        private static int GetFormatNum(ApplicationClass xlApp)
        {
            int formatNum;
            string Version = xlApp.Version;//excel 的版本号
            if (Convert.ToDouble(Version) < 12)//You use Excel 97-2003
            {
                formatNum = -4143;
            }
            else//you use excel 2007 or later
            {
                formatNum = 56;
            }
            return formatNum;
        }
        private void btn_Submit_Click(object sender, EventArgs e)
        {
            //TODO 当前页面选项的备份,下次加载上次打开的
            this.Close();
        }
        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
