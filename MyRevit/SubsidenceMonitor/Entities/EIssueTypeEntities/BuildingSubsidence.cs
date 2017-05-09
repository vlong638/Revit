using Microsoft.Office.Interop.Excel;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyRevit.SubsidenceMonitor.Entities
{
    /// <summary>
    /// 建筑物沉降.EIssueTypeEntity
    /// </summary>
    public class BuildingSubsidence : EIssueTypeEntity
    {
        public override EIssueType IssueType { get { return EIssueType.建筑物沉降; } }
        public override string SheetName { get { return "建筑物沉降"; } }
        public override ParseResult ParseInto(Worksheet sheet, TDetail detail)
        {
            //ReportName
            detail.ReportName = sheet.GetCellValueAsString(1, 1).Trim() + sheet.GetCellValueAsString(2, 1).Trim();
            //Contractor,Supervisor,Monitor
            Regex regex = new Regex(@"\s?承包单位：(.+)\s+监理单位：(.+)\s+监测单位：(.+)\s?");
            var match = regex.Match(sheet.GetCellValueAsString(3, 1));
            if (match.Groups.Count != 4)
                return ParseResult.Participants_ParseFailure;
            detail.Contractor = match.Groups[1].ToString().Trim();
            detail.Supervisor = match.Groups[2].ToString().Trim();
            detail.Monitor = match.Groups[3].ToString().Trim();
            //ReportName
            detail.ReportName = sheet.GetCellValueAsString(1, 1) + sheet.GetCellValueAsString(2, 1);
            //IssueDateTime,IssueTimeRange
            regex = new Regex(@"\s?监测日期：(.+)\s+监测时间：(.+)-(.+)\s?");
            match = regex.Match(sheet.GetCellValueAsString(4, 1));
            if (match.Groups.Count != 4)
                return ParseResult.DateTime_ParseFailure;
            var dateArgs = match.Groups[1].ToString().Split('.').Select(c => int.Parse(c)).ToArray();
            var startTimeArgs = match.Groups[2].ToString().Split(':').Select(c => int.Parse(c)).ToArray();
            var endTimeArgs = match.Groups[3].ToString().Split(':').Select(c => int.Parse(c)).ToArray();
            var startTime = new DateTime(dateArgs[0], dateArgs[1], dateArgs[2], startTimeArgs[0], startTimeArgs[1], 0);
            var endTime = new DateTime(dateArgs[0], dateArgs[1], dateArgs[2], endTimeArgs[0], endTimeArgs[1], 0);
            if (endTime < startTime)//如23:00-1:00认为跨天 但是跨天不支持10:00-11:00(第二天的11:00),这种情况跨天比较特殊,甚至跨多天,这个需要另提需求处理
                endTime.AddDays(1);
            //检测:监测时间的日期需与列表一致
            if (startTime.Date != detail.IssueDateTime)
                return ParseResult.DateTime_Invalid;
            detail.IssueDateTime = startTime;
            detail.IssueTimeRange = (short)(endTime - startTime).TotalMinutes;
            //InstrumentName,InstrumentCode
            regex = new Regex(@"\s?仪器名称：(.+)\s+仪器编号：(.+)\s?");
            match = regex.Match(sheet.GetCellValueAsString(5, 1));
            if (match.Groups.Count != 3)
                return ParseResult.Instrument_ParseFailure;
            detail.InstrumentName = match.Groups[1].ToString().Trim();
            detail.InstrumentCode = match.Groups[2].ToString().Trim();
            //定位点集合 默认从9,1开始
            int dataColumns = 8;//数据列数
            int rowSpan = 30;//上下分段间隔不超过30行
            int startRow = 9;
            var dataRanges = GetDataRangesFromMultipleTwoParagraphsContent(sheet, startRow, dataColumns, rowSpan);
            //节点解析
            List<TNode> nodes;
            int emptyCountToStop = 2;//2即空两行则跳过
            nodes = GetNodes(detail, sheet, dataRanges, emptyCountToStop, (s, r, range) =>
            {
                return new BuildingSubsidenceDataV1(
                     s.GetCellValueAsString(r, range.StartColumn).Trim(),
                     s.GetCellValueAsString(r, range.StartColumn + 1).Trim(),
                     s.GetCellValueAsString(r, range.StartColumn + 2).Trim(),
                     s.GetCellValueAsString(r, range.StartColumn + 6).Trim(),
                     s.GetCellValueAsString(r, range.StartColumn + 7).Trim());
            });
            detail.Nodes.AddRange(nodes);
            return ParseResult.Success;
        }
    }
}
