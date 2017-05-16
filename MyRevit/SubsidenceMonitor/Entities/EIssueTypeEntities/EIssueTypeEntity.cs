using Microsoft.Office.Interop.Excel;
using MyRevit.SubsidenceMonitor.Interfaces;
using MyRevit.SubsidenceMonitor.UI;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyRevit.SubsidenceMonitor.Entities
{
    public abstract class EIssueTypeEntity
    {
        public abstract EIssueType IssueType { get; }
        public abstract string SheetName { get; }

        string MessageSuffix = "尚未进行有效配置,请联系管理人员";
        public string CheckWarnSettings(WarnSettings warnSettings)
        {
            StringBuilder sb = new StringBuilder();
            switch (IssueType)
            {
                case EIssueType.建筑物沉降:
                    //建筑物沉降
                    if (warnSettings.BuildingSubsidence_Day == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_BuildingSubsidence_Day + MessageSuffix);
                    if (warnSettings.BuildingSubsidence_DailyMillimeter == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_BuildingSubsidence_DailyMillimeter + MessageSuffix);
                    if (warnSettings.BuildingSubsidence_SumMillimeter == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_BuildingSubsidence_SumMillimeter + MessageSuffix);
                    return sb.ToString();
                case EIssueType.地表沉降:
                    //地表沉降
                    if (warnSettings.SurfaceSubsidence_Day == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_SurfaceSubsidence_Day + MessageSuffix);
                    if (warnSettings.SurfaceSubsidence_DailyMillimeter == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_SurfaceSubsidence_DailyMillimeter + MessageSuffix);
                    if (warnSettings.SurfaceSubsidence_SumMillimeter == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_SurfaceSubsidence_SumMillimeter + MessageSuffix);
                    return sb.ToString();
                case EIssueType.管线沉降_有压:
                    //管线沉降(有压)
                    if (warnSettings.PressedPipeLineSubsidence_Day == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_StressedPipeLineSubsidence_Day + MessageSuffix);
                    if (warnSettings.PressedPipeLineSubsidence_PipelineMillimeter == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_StressedPipeLineSubsidence_PipelineMillimeter + MessageSuffix);
                    if (warnSettings.PressedPipeLineSubsidence_WellMillimeter == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_StressedPipeLineSubsidence_WellMillimeter + MessageSuffix);
                    if (warnSettings.PressedPipeLineSubsidence_SumMillimeter == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_StressedPipeLineSubsidence_SumMillimeter + MessageSuffix);
                    return sb.ToString();
                case EIssueType.管线沉降_无压:
                    //管线沉降(无压)
                    if (warnSettings.UnpressedPipeLineSubsidence_Day == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_UnstressedPipeLineSubsidence_Day + MessageSuffix);
                    if (warnSettings.UnpressedPipeLineSubsidence_PipelineMillimeter == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_UnstressedPipeLineSubsidence_PipelineMillimeter + MessageSuffix);
                    if (warnSettings.UnpressedPipeLineSubsidence_WellMillimeter == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_UnstressedPipeLineSubsidence_WellMillimeter + MessageSuffix);
                    if (warnSettings.UnpressedPipeLineSubsidence_SumMillimeter == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_UnstressedPipeLineSubsidence_SumMillimeter + MessageSuffix);
                    return sb.ToString();
                case EIssueType.侧斜监测:
                    //墙体水平位移(侧斜)
                    if (warnSettings.SkewBack_WellMillimeter == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_SkewBack_WellMillimeter + MessageSuffix);
                    if (warnSettings.SkewBack_StandardMillimeter == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_SkewBack_StandardMillimeter + MessageSuffix);
                    if (warnSettings.SkewBack_Speed == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_SkewBack_Speed + MessageSuffix);
                    if (warnSettings.SkewBack_Day == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_SkewBack_Day + MessageSuffix);
                    return sb.ToString();
                case EIssueType.钢支撑轴力监测:
                    //钢支撑轴力
                    if (warnSettings.STBAP_MaxAxle == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_STBAP_MaxAxle + MessageSuffix);
                    if (warnSettings.STBAP_MinAxle == int.MinValue)
                        sb.AppendLine(WarnSettings.Tag_STBAP_MinAxle + MessageSuffix);
                    return sb.ToString();
                default:
                    throw new NotImplementedException("尚未支持的类型");
            }
        }
        public ParseResult ParseInto(Workbook workbook, TDetail detail)
        {
            if (!string.IsNullOrEmpty(SheetName))
            {
                var sheetName = SheetName;
                Worksheet sheet = null;
                foreach (Worksheet s in workbook.Worksheets)
                {
                    if (s.Name == sheetName)
                    {
                        sheet = s;
                        break;
                    }
                }
                if (sheet == null)
                    return ParseResult.ReportName_ParseFailure;
                return ParseSheetInto(sheet, detail);
            }
            else
            {
                return ParseBookInto(workbook, detail);
            }
        }
        public abstract ParseResult ParseSheetInto(Worksheet sheet, TDetail detail);
        public abstract ParseResult ParseBookInto(Workbook workbook, TDetail detail);
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
            if (IssueType == EIssueType.侧斜监测)
                return dataRanges;
            while (true)
            {
                if (IssueType==EIssueType.钢支撑轴力监测)
                {
                    if (sheet.GetCellValueAsString(currentRow, 1).Contains("监测点"))
                    {
                        dataRanges.Add(new DataRange(previousRow, 1, currentRow - 5));//-2=>监测员该行不要,前一行也是一个备注不要
                        dataRanges.Add(new DataRange(previousRow, dataColumns + 1, currentRow - 2));
                        previousRow = currentRow + 2;//从数据行开始
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
                else
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
                for (int i = dataRange.StartRow; i <= dataRange.EndRow - 1; i++)
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
        ///// <summary>
        ///// 获取节点数据对象
        ///// </summary>
        ///// <param name="nodeCode"></param>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //public ITNodeData GetNodeDataEntity(string nodeCode, string data)
        //{
        //    switch (IssueType)
        //    {
        //        case EIssueType.建筑物沉降:
        //            return new BuildingSubsidenceDataV1(nodeCode, data);
        //        //TODO
        //        case EIssueType.地表沉降:
        //        case EIssueType.管线沉降_无压:
        //        case EIssueType.管线沉降_有压:
        //        case EIssueType.侧线监测:
        //        case EIssueType.钢支撑轴力监测:
        //        default:
        //            throw new NotImplementedException("未支持该类型的ITNodeData:" + IssueType.ToString());
        //    }
        //}
        public ITNodeDataCollection<ITNodeData> GetNodeDataCollection()
        {
            switch (IssueType)
            {
                case EIssueType.建筑物沉降:
                    return new BuildingSubsidenceCollection<BuildingSubsidenceDataV1>();
                case EIssueType.地表沉降:
                    return new SurfaceSubsidenceCollection<SurfaceSubsidenceDataV1>();
                case EIssueType.钢支撑轴力监测:
                    return new STBAPCollection<STBAPDataV1>();
                case EIssueType.管线沉降_无压:
                    return new UnpressedPipeLineSubsidenceCollection<UnpressedPipeLineSubsidenceDataV1>();
                case EIssueType.管线沉降_有压:
                    return new PressedPipeLineSubsidenceCollection<PressedPipeLineSubsidenceDataV1>();
                //case EIssueType.侧斜监测:
                //    return new SkewBackCollection<SkewBackDataV1>();
                default:
                    throw new NotImplementedException("未支持该类型的ITNodeDataCollection:" + IssueType.ToString());
            }
        }
        public List<HeaderNode> GetHeaderNodes()
        {
            int totalWidths = 504;
            int ordinaryHeight = 30;
            int averageWidth;
            switch (IssueType)
            {
                case EIssueType.建筑物沉降:
                    averageWidth = totalWidths / 6;
                    return new List<HeaderNode>()
                    {
                        new HeaderNode(0,ordinaryHeight*2,averageWidth,"测点编号",nameof(BuildingSubsidenceDataV1.NodeCode)),
                        new HeaderNode(1,ordinaryHeight,averageWidth*2,"沉降变化量(mm)",null,
                        new List<HeaderNode>() {
                             new HeaderNode(-1,ordinaryHeight,averageWidth,"本次变量",nameof(BuildingSubsidenceDataV1.CurrentChanges)),
                             new HeaderNode(-1,ordinaryHeight,averageWidth,"累计变量",nameof(BuildingSubsidenceDataV1.SumChanges)),
                        }),
                        new HeaderNode(3,ordinaryHeight*2,averageWidth,$"围护结构施工{Environment.NewLine}期间累计值（mm）",nameof(BuildingSubsidenceDataV1.SumPeriodBuildingEnvelope)),
                        new HeaderNode(4,ordinaryHeight*2,averageWidth,"总累计值(mm)",nameof(BuildingSubsidenceDataV1.SumBuildingEnvelope)),
                        new HeaderNode(5,ordinaryHeight*2,averageWidth,"备注",nameof(BuildingSubsidenceDataV1.Conment)),
                    };
                case EIssueType.地表沉降:
                    averageWidth = totalWidths / 4;
                    return new List<HeaderNode>()
                    {
                        new HeaderNode(0,ordinaryHeight*2,averageWidth,"测点编号",nameof(SurfaceSubsidenceDataV1.NodeCode)),
                        new HeaderNode(1,ordinaryHeight,averageWidth*2,"沉降变化量(mm)",null,
                        new List<HeaderNode>() {
                             new HeaderNode(-1,ordinaryHeight,averageWidth,"本次变量",nameof(SurfaceSubsidenceDataV1.CurrentChanges)),
                             new HeaderNode(-1,ordinaryHeight,averageWidth,"累计变量",nameof(SurfaceSubsidenceDataV1.SumChanges)),
                        }),
                        new HeaderNode(3,ordinaryHeight*2,averageWidth,"备注",nameof(SurfaceSubsidenceDataV1.Conment)),
                    };
                case EIssueType.钢支撑轴力监测:
                    averageWidth = totalWidths / 4;
                    return new List<HeaderNode>()
                    {
                        new HeaderNode(0,ordinaryHeight*2,averageWidth,"监测点",nameof(STBAPDataV1.NodeCode)),
                        new HeaderNode(1,ordinaryHeight,averageWidth*2,"变化值(KN)",null,
                        new List<HeaderNode>() {
                             new HeaderNode(-1,ordinaryHeight,averageWidth,"本次变量",nameof(STBAPDataV1.CurrentChanges)),
                             new HeaderNode(-1,ordinaryHeight,averageWidth,"累计变量",nameof(STBAPDataV1.SumChanges)),
                        }),
                        new HeaderNode(3,ordinaryHeight*2,averageWidth,"备注",nameof(STBAPDataV1.Conment)),
                    };
                case EIssueType.管线沉降_有压:
                case EIssueType.管线沉降_无压:
                    averageWidth = totalWidths / 6;
                    return new List<HeaderNode>()
                    {
                        new HeaderNode(0,ordinaryHeight*2,averageWidth,"测点编号",nameof(PressedPipeLineSubsidenceDataV1.NodeCode)),
                        new HeaderNode(1,ordinaryHeight,averageWidth*2,"沉降变化量(mm)",null,
                        new List<HeaderNode>() {
                             new HeaderNode(-1,ordinaryHeight,averageWidth,"本次变量",nameof(PressedPipeLineSubsidenceDataV1.CurrentChanges)),
                             new HeaderNode(-1,ordinaryHeight,averageWidth,"累计变量",nameof(PressedPipeLineSubsidenceDataV1.SumChanges)),
                        }),
                        new HeaderNode(3,ordinaryHeight*2,averageWidth,$"围护结构施工{Environment.NewLine}期间累计值（mm）",nameof(PressedPipeLineSubsidenceDataV1.SumPeriodBuildingEnvelope)),
                        new HeaderNode(4,ordinaryHeight*2,averageWidth,"总累计值(mm)",nameof(PressedPipeLineSubsidenceDataV1.SumBuildingEnvelope)),
                        new HeaderNode(5,ordinaryHeight*2,averageWidth,"备注",nameof(PressedPipeLineSubsidenceDataV1.Conment)),
                    };
                case EIssueType.侧斜监测:
                    averageWidth = totalWidths / 7;
                    return new List<HeaderNode>()
                    {
                        new HeaderNode(0,ordinaryHeight*2,averageWidth,"监测点",nameof(SkewBackDataV1.NodeCode)),
                        new HeaderNode(1,ordinaryHeight*2,averageWidth,"深度(m)",nameof(SkewBackDataV1.Depth)),
                        new HeaderNode(2,ordinaryHeight,averageWidth*2,"位移量(mm)",null,
                        new List<HeaderNode>() {
                             new HeaderNode(-1,ordinaryHeight,averageWidth,"上次",nameof(SkewBackDataV1.PreviousChange)),
                             new HeaderNode(-1,ordinaryHeight,averageWidth,"本次",nameof(SkewBackDataV1.CurrentChange)),
                        }),
                        new HeaderNode(4,ordinaryHeight*2,averageWidth,"上次累计(mm)",nameof(SkewBackDataV1.PreviousSum)),
                        new HeaderNode(5,ordinaryHeight*2,averageWidth,"本次累计(mm)",nameof(SkewBackDataV1.CurrentSum)),
                        new HeaderNode(6,ordinaryHeight*2,averageWidth,"备注",nameof(SkewBackDataV1.Conment)),
                    };
                default:
                    throw new NotImplementedException("该类型未设置表头配置");
            }
        }
    }
    public enum ParseResult
    {
        Success,
        ReportName_ParseFailure,
        Participants_ParseFailure,
        DateTime_ParseFailure,
        DateTime_Invalid,
        Instrument_ParseFailure,
    }
}
