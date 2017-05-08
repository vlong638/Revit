using System;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;
using WinformTests.SubsidenceMonitor.Controls;
using System.Collections.Generic;
using System.Drawing;

namespace WinformTests
{
    public partial class SubsidenceMonitorForm : Form
    {
        public SubsidenceMonitorForm()
        {
            InitializeComponent();

            tb_WarnArgs.ReadOnly = true;

            btn_LoadExcel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            btn_LoadExcel.Text = $"录入{System.Environment.NewLine}Excel";


            //myDGV04271.HeaderNodes = new List<HeaderNode>()
            //{
            //    new HeaderNode(20,100,"节点-Code"),
            //    new HeaderNode(20,100,"节点-Changes(mm)",new List<HeaderNode>() {
            //         new HeaderNode(20,40,"节点-CurrentChange"),
            //         new HeaderNode(20,60,"节点-TotalChanges"),
            //    }),
            //    new HeaderNode(20,100,"节点-PreviousElevation"),
            //    new HeaderNode(20,100,"节点-CurrentElevation"),
            //};
            myDGV04271.HeaderNodes = new List<HeaderNode>()
            {
                new HeaderNode(80,200,"节点-1"),
                new HeaderNode(40,200,"节点-2(mm)",new List<HeaderNode>() {
                     new HeaderNode(40,80,"节点-21"),
                     new HeaderNode(40,120,"节点-22"),
                }),
                new HeaderNode(80,200,"节点-3"),
                new HeaderNode(80,200,"节点-4"),
            };

            #region dataGridView1
            this.dataGridView1.Columns.Add("JanWin", "Win11111");
            this.dataGridView1.Columns.Add("JanLoss", "Loss11111");
            this.dataGridView1.Columns.Add("FebWin", "Win11111");
            this.dataGridView1.Columns.Add("FebLoss", "Loss11111");
            this.dataGridView1.Columns.Add("MarWin", "Win11111");
            this.dataGridView1.Columns.Add("MarLoss", "Loss11111");
            for (int j = 0; j < this.dataGridView1.ColumnCount; j++)
            {
                this.dataGridView1.Columns[j].Width = 45;
            }
            dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            var lines = 2;
            this.dataGridView1.ColumnHeadersHeight = this.dataGridView1.ColumnHeadersHeight * lines;
            this.dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomCenter;
            this.dataGridView1.CellPainting += DataGridView1_CellPainting;
            this.dataGridView1.Paint += DataGridView1_Paint;
            dataGridView1.Columns[0].DataPropertyName = nameof(SubsidenceNode.Code);

            List<SubsidenceNode> datas = new List<SubsidenceNode>()
            {
                new SubsidenceNode()
                {
                    Code="Code1",
                    CurrentChange=-1,
                    TotalChanges=-2,
                    PreviousElevation=2.13f,
                    CurrentElevation=3.22f,
                }
            };
            dataGridView1.DataSource = datas;
        }

        private void DataGridView1_Paint(object sender, PaintEventArgs e)
        {
            string[] monthes = { "January", "February", "March" };
            for (int j = 0; j < 6;)
            {
                System.Drawing.Rectangle r1 = this.dataGridView1.GetCellDisplayRectangle(j, -1, true);
                System.Drawing.Rectangle r2 = this.dataGridView1.GetCellDisplayRectangle(j + 1, -1, true);
                r1.X += 1;
                r1.Y += 1;
                r1.Width = r1.Width + r2.Width - 1;
                r1.Height = r1.Height / 2 - 1;
                e.Graphics.FillRectangle(new SolidBrush(this.dataGridView1.ColumnHeadersDefaultCellStyle.BackColor), r1);
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                e.Graphics.DrawString(monthes[j / 2],
                    this.dataGridView1.ColumnHeadersDefaultCellStyle.Font,
                    new SolidBrush(this.dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor),
                    r1,
                    format);
                j += 2;
            }
        }

        private void DataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            //if (e.RowIndex==-1 && e.ColumnIndex>-1)
            //{
            //    e.PaintBackground(e.CellBounds, false);
            //    System.Drawing.Rectangle r2 = e.CellBounds;
            //    r2.Y += e.CellBounds.Height / 2;
            //    r2.Height += e.CellBounds.Height / 2;
            //    e.PaintContent(r2);
            //    e.Handled = true;
            //}
        }
        #endregion

        /// <summary>
        /// 沉降
        /// </summary>
        class SubsidenceNode
        {
            /// <summary>
            /// 测点编号
            /// </summary>
            public string Code { set; get; }
            /// <summary>
            /// 本次变量
            /// </summary>
            public int CurrentChange { set; get; }
            /// <summary>
            /// 累计变量
            /// </summary>
            public int TotalChanges { set; get; }
            /// <summary>
            /// 上次高程(m)
            /// </summary>
            public float PreviousElevation { set; get; }
            /// <summary>
            /// 本次高程(m)
            /// </summary>
            public float CurrentElevation { set; get; }
        }



        private void btn_LoadExcel_Click(object sender, EventArgs e)
        {
            ApplicationClass excelApp = new ApplicationClass();
            //添加新WorkBook
            //application.Workbooks.Add()
            //打开WorkBook
            var path = @"E:\WorkingSpace\Tasks\0417土方\锦寓路站沉降3-30 - 副本.xls";
            var workbook = excelApp.Workbooks.Open(path);
            //设置Excel标题栏
            excelApp.Caption = "应用程序调用Excel";
            //设置WorkSheet为活动表
            if (excelApp.Worksheets.Count > 1)
            {
                (excelApp.Worksheets[2] as Worksheet).Activate();
            }
            //单元格赋值
            if (workbook.ActiveSheet == null)
                return;
            var sheet = workbook.ActiveSheet as Worksheet;
            var cell = sheet.Cells[1, 1] as Range;
            var value = cell.Value;
        }
    }
}
