using Microsoft.Office.Interop.Excel;
using MyRevit.SubsidenceMonitor.Interfaces;
using MyRevit.SubsidenceMonitor.Operators;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyRevit.SubsidenceMonitor.Entities
{
    /// <summary>
    /// 建筑物沉降.EIssueTypeEntity
    /// </summary>
    public class UnpressedPipeLineSubsidence : EIssueTypeEntity
    {
        public override EIssueType IssueType { get { return EIssueType.管线沉降_无压; } }
        public override string SheetName { get { return "管线沉降"; } }

        public override ParseResult ParseBookInto(Workbook workbook, TDetail detail)
        {
            throw new NotImplementedException();
        }
        public override ParseResult ParseSheetInto(Worksheet sheet, TDetail detail)
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
            if (startTime.Date != detail.IssueDateTime.Date)
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
                return new UnpressedPipeLineSubsidenceDataV1(
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
    public class UnpressedPipeLineSubsidenceDataV1 : ITNodeData
    {
        /// <summary>
        /// str需为this.ToString()的数据
        /// </summary>
        /// <param name="str"></param>
        public UnpressedPipeLineSubsidenceDataV1()
        {
        }
        public UnpressedPipeLineSubsidenceDataV1(string nodeCode, string str)
        {
            DeserializeFromString(nodeCode, str);
        }
        public UnpressedPipeLineSubsidenceDataV1(string nodeCode, string currentChanges, string sumChanges, string sumPeriodBuildingEnvelope, string sumBuildingEnvelope)
        {
            NodeCode = nodeCode;
            CurrentChanges = currentChanges;
            SumChanges = sumChanges;
            SumPeriodBuildingEnvelope = sumPeriodBuildingEnvelope;
            SumBuildingEnvelope = sumBuildingEnvelope;
        }

        /// <summary>
        /// 测点编号
        /// </summary>
        public string NodeCode { set; get; }
        /// <summary>
        /// 本次变量(mm)
        /// </summary>
        public string CurrentChanges { set; get; }
        /// <summary>
        /// 累计变量(mm)
        /// </summary>
        public string SumChanges { set; get; }
        /// <summary>
        /// 围护结构施工期间累计值（mm）
        /// </summary>
        public string SumPeriodBuildingEnvelope { set; get; }
        /// <summary>
        /// 总累计值（mm）
        /// </summary>
        public string SumBuildingEnvelope { set; get; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Conment
        {
            get
            {
                float f;
                return float.TryParse(CurrentChanges, out f)
                    && float.TryParse(SumChanges, out f)
                    && float.TryParse(SumPeriodBuildingEnvelope, out f)
                    && float.TryParse(SumBuildingEnvelope, out f) 
                    ? "" : "点位破坏";
            }
        }
        public string SerializeToString()
        {
            return $"{CurrentChanges},{SumChanges},{SumPeriodBuildingEnvelope},{SumBuildingEnvelope}";
        }
        public void DeserializeFromString(string nodeCode, string str)
        {
            NodeCode = nodeCode;
            var args = str.Split(',');
            CurrentChanges = args[0];
            SumChanges = args[1];
            SumPeriodBuildingEnvelope = args[2];
            SumBuildingEnvelope = args[3];
        }

        float _CurrentChanges_Float = float.MinValue;
        public float CurrentChanges_Float
        {
            get
            {
                if (_CurrentChanges_Float == float.MinValue)
                {
                    float result = float.MinValue;
                    if (!float.TryParse(CurrentChanges, out result))
                    {
                        _CurrentChanges_Float = float.NaN;
                    }
                    else
                    {
                        _CurrentChanges_Float = Math.Abs(result);//计算以绝对值为准
                    }
                }
                return _CurrentChanges_Float;
            }
        }
        float _SumChanges_Float = float.MinValue;
        public float SumChanges_Float
        {
            get
            {
                if (_SumChanges_Float == float.MinValue)
                {
                    float result = float.MinValue;
                    if (!float.TryParse(SumChanges, out result))
                    {
                        _SumChanges_Float = float.NaN;
                    }
                    else
                    {
                        _SumChanges_Float = Math.Abs(result);//计算以绝对值为准
                    }
                }
                return _SumChanges_Float;
            }
        }
    }
    public class UnpressedPipeLineSubsidenceCollection<T>
        : ITNodeDataCollection<T>
        where T : UnpressedPipeLineSubsidenceDataV1, new()
    {
        List<T> _Datas = new List<T>();
        public IEnumerable<T> Datas
        {
            get
            {
                return _Datas.AsEnumerable();
            }
        }
        public void Add(string nodeCode, string nodeString)
        {
            var t = new T();
            t.DeserializeFromString(nodeCode, nodeString);
            _Datas.Add(t);
        }
        public void Remove(string nodeCode)
        {
            _Datas.Remove(_Datas.First(c => c.NodeCode == nodeCode));
        }
        public IEnumerable<T> GetCurrentMaxNodes()
        {
            var maxFloat = _Datas.Max(p => p.CurrentChanges_Float);
            return _Datas.Where(c => c.CurrentChanges_Float == maxFloat);
        }
        public IEnumerable<T> GetTotalMaxNodes()
        {
            var maxFloat = _Datas.Max(p => p.SumChanges_Float);
            return _Datas.Where(c => c.SumChanges_Float == maxFloat);
        }
        public IEnumerable<T> GetCloseWarn(WarnSettings warnSettings, TDetail detail)
        {
            return getWarnResult(warnSettings, detail, WarnSettings.CloseCoefficient, WarnSettings.OverCoefficient);
        }
        public IEnumerable<T> GetOverWarn(WarnSettings warnSettings, TDetail detail)
        {
            return getWarnResult(warnSettings, detail, 1);
        }
        private IEnumerable<T> getWarnResult(WarnSettings warnSettings, TDetail detail, double warnCoefficientMin, double warnCoefficientMax = double.NaN)
        {
            List<T> result = new List<T>();
            var d = Datas.FirstOrDefault();
            if (d == null)
                return result;

            var totalHourRange = warnSettings.PressedPipeLineSubsidence_Day * 24;//检测项
            var endTime = detail.IssueDateTime;
            var details = Facade.GetDetailsByTimeRange(detail.IssueType, endTime.AddHours(-totalHourRange), endTime);
            var orderedDetails = details.OrderByDescending(c => c.IssueDateTime).ToList();
            var currentDetail = detail;
            //需预警的节点
            //监测 warnSettings.UnpressedPipeLineSubsidence_SumMillimeter;
            var sumMillimeter = warnSettings.PressedPipeLineSubsidence_SumMillimeter;//检测项
            foreach (var data in Datas)
            {
                if (double.IsNaN(warnCoefficientMax))
                {
                    if (data.SumChanges_Float >= sumMillimeter * warnCoefficientMin)
                        result.Add(data);
                }
                else
                {
                    if (data.SumChanges_Float >= sumMillimeter * warnCoefficientMin
                        && data.SumChanges_Float < sumMillimeter * warnCoefficientMax)
                        result.Add(data);
                }
            }
            //监测 warnSettings.UnpressedPipeLineSubsidence_DailyMillimeter;
            //数据天数达标监测
            if (totalHourRange != 0)
            {
                var dailyMillimeter = warnSettings.PressedPipeLineSubsidence_PipelineMillimeter;//检测项
                double warnDailyMillimeterMin = dailyMillimeter * warnCoefficientMin;
                double warnDailyMillimeterMax = 0;
                if (!double.IsNaN(warnCoefficientMax))
                {
                    warnDailyMillimeterMax = dailyMillimeter * warnCoefficientMax;
                }
                var tempTotalTimeRange = totalHourRange;
                int detailIndex = 0;
                while (tempTotalTimeRange > 0)
                {
                    if (detailIndex == orderedDetails.Count())
                        throw new NotImplementedException("未满足监测报警要求的天数");
                    var nextDetail = orderedDetails[detailIndex];
                    var currentTimeRange = (int)(currentDetail.IssueDateTime.AddMinutes(currentDetail.IssueTimeRange) - nextDetail.IssueDateTime.AddMinutes(nextDetail.IssueTimeRange)).TotalHours;
                    if (currentTimeRange <= tempTotalTimeRange)
                    {
                        tempTotalTimeRange -= currentTimeRange;
                    }
                    else
                    {
                        tempTotalTimeRange -= currentTimeRange;
                    }
                    currentDetail = nextDetail;
                    detailIndex++;
                }
                foreach (var data in Datas)
                {
                    if (result.Contains(data))
                        continue;

                    detailIndex = 0;
                    currentDetail = detail;
                    int days = warnSettings.PressedPipeLineSubsidence_PipelineMillimeter;
                    double overHours = 0;
                    double overValues = 0;
                    while (days > 0)
                    {
                        double dailyValue = 0;
                        double hoursToDeal = 0;
                        if (overHours >= 24)
                        {
                            dailyValue = overValues * 24 / overHours;
                            overValues -= dailyValue;
                            overHours -= 24;
                        }
                        else
                        {
                            dailyValue = overValues;
                            hoursToDeal = 24 - overHours;
                            while (hoursToDeal > 0)
                            {
                                var currentNodeData = currentDetail.NodeDatas.Datas.FirstOrDefault(c => c.NodeCode == data.NodeCode);
                                if (currentNodeData == null)//信息缺失,不作提醒处理  当前所需的节点数据不存在
                                {
                                    days = -1;//-1表信息缺失
                                    hoursToDeal = 0;
                                    break;
                                }
                                var nextDetail = orderedDetails[detailIndex];
                                double currentTimeRange = (currentDetail.IssueDateTime.AddMinutes(currentDetail.IssueTimeRange) - nextDetail.IssueDateTime.AddMinutes(nextDetail.IssueTimeRange)).TotalHours;
                                if (currentTimeRange <= hoursToDeal)
                                {
                                    dailyValue += (currentNodeData as T).CurrentChanges_Float;
                                }
                                else
                                {
                                    dailyValue += (currentNodeData as T).CurrentChanges_Float * (hoursToDeal / currentTimeRange);
                                    overHours = currentTimeRange - hoursToDeal;
                                    overValues = (currentNodeData as T).CurrentChanges_Float * (overHours / currentTimeRange);
                                }
                                hoursToDeal -= currentTimeRange;
                                detailIndex++;
                                currentDetail = nextDetail;
                            }
                        }
                        //时间已尽 检测是否到达预期值
                        if (days == -1)
                            break;
                        if (!double.IsNaN(warnCoefficientMax) && dailyValue > warnDailyMillimeterMax)
                        {
                            days = -3;//-3表信息已过高限
                            break;
                        }
                        else if (dailyValue >= warnDailyMillimeterMin)
                            days--;
                        else
                        {
                            days = -2;//-2表信息未到连续标准
                            break;
                        }
                    }
                    if (days == 0)//处理结束 认为按照标准的到达了日期0则各天检测通过
                        result.Add(data);
                }
            }
            return result;
        }
    }
}
