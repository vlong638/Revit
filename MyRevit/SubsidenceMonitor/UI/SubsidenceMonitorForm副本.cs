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
            dgv_left.HeaderNodes = new List<HeaderNode>()
            {
                new HeaderNode(80,109,"1"),
                new HeaderNode(40,109,"2(mm)",new List<HeaderNode>() {
                     new HeaderNode(40,54,"21"),
                     new HeaderNode(40,54,"22"),
                }),
                new HeaderNode(80,109,"3"),
                new HeaderNode(80,109,"4"),
            };
        }

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
            //单元格-取值,赋值
            if (workbook.ActiveSheet != null)
            {
                var sheet = workbook.ActiveSheet as Worksheet;
                var cell = sheet.Cells[1, 1] as Range;
                var value = cell.Value;
            }
            //打开WorkSheet
            if (workbook.Worksheets["管线沉降"] != null)
            {
                var sheet = workbook.Worksheets["管线沉降"] as Worksheet;
                sheet.Activate();
                var cell = sheet.Cells[1, 1] as Range;
                var value = cell.Value;
            }
        }
    }
}
