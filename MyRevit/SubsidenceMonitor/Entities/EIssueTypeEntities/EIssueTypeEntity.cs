using Microsoft.Office.Interop.Excel;
using MyRevit.SubsidenceMonitor.Interfaces;
using MyRevit.SubsidenceMonitor.Utilities;
using System;
using System.Collections.Generic;

namespace MyRevit.SubsidenceMonitor.Entities
{
    public abstract class EIssueTypeEntity
    {
        public abstract EIssueType IssueType { get; }
        public abstract string SheetName { get; }

        public ParseResult ParseInto(Workbook workbook, TDetail detail)
        {
            var sheetName = SheetName;
            if (workbook.Worksheets[sheetName] == null)
                return ParseResult.ReportName_ParseFailure;
            //throw new NotImplementedException("Worksheet名称无效");
            var sheet = workbook.Worksheets[sheetName] as Worksheet;

            return ParseInto(sheet, detail);
        }
        public abstract ParseResult ParseInto(Worksheet sheet, TDetail detail);
        /// <summary>
        /// 从横向两组多段式取出有序的数据区间
        /// 1 2
        /// 3 4
        /// 5 6
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="startRow"></param>
        /// <param name="dataColumns"></param>
        /// <param name="rowSpanLimit"></param>
        /// <returns></returns>
        protected List<DataRange> GetDataRangesFromMultipleTwoParagraphsContent(Worksheet sheet, int startRow, int dataColumns, int rowSpanLimit)
        {
            List<DataRange> dataRanges = new List<DataRange>();
            int previousRow = startRow;//StartRow
            int currentRow = previousRow + 1;
            int currentSpan = 0;
            while (true)
            {
                if (sheet.GetCellValueAsString(currentRow, 1).Contains("监测员"))
                {
                    dataRanges.Add(new DataRange(previousRow, 1, currentRow - 2));//-2=>监测员该行不要,前一行也是一个备注不要
                    dataRanges.Add(new DataRange(previousRow, dataColumns + 1, currentRow - 2));
                    previousRow = currentRow + 1;//从数据行开始
                    currentSpan = 0;
                    if (currentRow == sheet.Rows.Count || currentSpan == rowSpanLimit)
                    {
                        break;
                    }
                }
                else if (currentRow == sheet.Rows.Count || currentSpan == rowSpanLimit)
                {
                    dataRanges.Add(new DataRange(previousRow, 1, currentRow));
                    dataRanges.Add(new DataRange(previousRow, dataColumns + 1, currentRow));
                    break;
                }
                currentRow++;
                currentSpan++;
            }
            return dataRanges;
        }
        protected struct DataRange
        {
            public DataRange(int startRow, int startColumn, int endRow) : this()
            {
                StartRow = startRow;
                StartColumn = startColumn;
                EndRow = endRow;
            }

            public int StartRow { set; get; }
            public int StartColumn { set; get; }
            public int EndRow { set; get; }
        }
        /// <summary>
        /// 从多段区间里面解析数据
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="dataRanges"></param>
        /// <param name="emptyCountToStop"></param>
        /// <returns></returns>
        protected List<TNode> GetNodes(TDetail detail, Worksheet sheet, List<DataRange> dataRanges, int emptyCountToStop, Func<Worksheet, int, DataRange, ITNodeData> parseData)
        {
            List<TNode> nodes = new List<TNode>();
            bool isEnd = false;
            short index = 1;
            foreach (var dataRange in dataRanges)
            {
                if (isEnd)
                    break;

                int emptyCount = 0;
                for (int i = dataRange.StartRow; i <= dataRange.EndRow - 2; i++)
                {
                    //空行检测
                    var nodeCode = sheet.GetCellValueAsString(i, dataRange.StartColumn);
                    if (string.IsNullOrEmpty(nodeCode))
                    {
                        emptyCount++;
                        if (emptyCount > emptyCountToStop)//空一行+1,若实际超出设定的空行数3>2则认为超限,超限有两种情况(后续无数据,后续继续检测段)
                        {
                            if (i == dataRange.StartRow + emptyCountToStop - 1)//空两行发生在段首,则认为后续无内容,结束节点读取(认为后面为空段,无视后面的段)
                            {
                                isEnd = true;
                            }
                            break;
                        }
                        continue;
                    }
                    else
                    {
                        emptyCount = 0;
                    }
                    //非空行数据加载
                    TNode node = new TNode();
                    node.IssueType = detail.IssueType;
                    node.IssueDateTime = detail.IssueDateTime;
                    node.NodeCode = nodeCode;
                    node.Data = parseData(sheet, i, dataRange).SerializeToString();
                    node.Index = index;
                    nodes.Add(node);
                    index++;
                }
            }
            return nodes;
        }
    }
}
