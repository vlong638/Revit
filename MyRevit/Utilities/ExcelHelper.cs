using Microsoft.Office.Interop.Excel;
using MyRevit.SubsidenceMonitor.Entities;
using System;
using System.IO;

namespace MyRevit.Utilities
{
    class ExcelHelper
    {
        public static Worksheet GetWorksheet(string path, EIssueType issueType)
        {
            if (!File.Exists(path))
                throw new NotImplementedException("无效的文件路径");

            ApplicationClass excelApp = new ApplicationClass();
            var workbook = excelApp.Workbooks.Open(path);
            if (workbook == null)
                throw new NotImplementedException("Workbook文档对象无效");
            var sheetName = issueType.GetSheetName();
            if (workbook.Worksheets[sheetName] == null)
                throw new NotImplementedException("Worksheet名称无效");
            return workbook.Worksheets[sheetName] as Worksheet;
        }
    }
    static class ExcelEx
    {
        public static string GetSheetName(this EIssueType issueType)
        {
            switch (issueType)
            {
                case EIssueType.建筑物沉降:
                    return "建筑物沉降";
                case EIssueType.地表沉降:
                    return "地表沉降";
                case EIssueType.管线沉降_无压:
                case EIssueType.管线沉降_有压:
                    return "管线沉降";
                case EIssueType.侧线监测:
                case EIssueType.钢支撑轴力监测:
                default:
                    throw new NotImplementedException();
            }
        }
        public static string GetCellValueAsString(this Worksheet sheet, int rowIndex, int columnIndex)
        {
            var cell = sheet.Cells[rowIndex, columnIndex] as Range;
            if (cell.Text == null)
                return "";

            return cell.Text.ToString();
        }
    }
}
