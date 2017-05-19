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
    public class SkewBack : EIssueTypeEntity
    {
        public override EIssueType IssueType { get { return EIssueType.侧斜监测; } }
        public override string SheetName { get { return null; } }

        public override ParseResult ParseBookInto(Workbook workbook, TDetail detail)
        {
            //通用信息
            Worksheet sheet = workbook.Sheets[1] as Worksheet;
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
            //IssueDateTime,IssueTimeRange
            regex = new Regex(@"\s?监测日期：(.+)\s+监测时间：(.+)-(.+)\s?");
            match = regex.Match(sheet.GetCellValueAsString(5, 1));
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
            //InstrumentName,InstrumentCode
            regex = new Regex(@"\s?仪器名称：(.+)\s+仪器编号：(.+)\s?");
            match = regex.Match(sheet.GetCellValueAsString(6, 1));
            if (match.Groups.Count != 3)
                return ParseResult.Instrument_ParseFailure;
            detail.InstrumentName = match.Groups[1].ToString().Trim();
            detail.InstrumentCode = match.Groups[2].ToString().Trim();
            //节点解析
            detail.DepthNodes = new List<TDepthNode>();
            foreach (Worksheet subSheet in workbook.Sheets)
            {
                int currentRow = 9;
                string cellValue = sheet.GetCellValueAsString(currentRow, 1);
                while (!string.IsNullOrEmpty(cellValue) && !cellValue.Contains("备注"))
                {
                    var data = new SkewBackDataV1(
                    subSheet.Name,
                    sheet.GetCellValueAsString(currentRow, 1),
                    sheet.GetCellValueAsString(currentRow, 2),
                    sheet.GetCellValueAsString(currentRow, 3),
                    sheet.GetCellValueAsString(currentRow, 4),
                    sheet.GetCellValueAsString(currentRow, 5)
                    );
                    var depthNode = new TDepthNode(detail.IssueType, detail.IssueDateTime, subSheet.Name, data.Depth);
                    depthNode.Data = data.SerializeToString();
                    detail.DepthNodes.Add(depthNode);
                    currentRow++;
                    cellValue = sheet.GetCellValueAsString(currentRow, 1);
                }
            }
            return ParseResult.Success;
        }
        public override ParseResult ParseSheetInto(Worksheet sheet, TDetail detail)
        {
            throw new NotImplementedException("该类型文档采用ParseBookInto()方法解析");

            ////ReportName
            //detail.ReportName = sheet.GetCellValueAsString(1, 1).Trim() + sheet.GetCellValueAsString(2, 1).Trim();
            ////Contractor,Supervisor,Monitor
            //Regex regex = new Regex(@"\s?承包单位：(.+)\s+监理单位：(.+)\s+监测单位：(.+)\s?");
            //var match = regex.Match(sheet.GetCellValueAsString(3, 1));
            //if (match.Groups.Count != 4)
            //    return ParseResult.Participants_ParseFailure;
            //detail.Contractor = match.Groups[1].ToString().Trim();
            //detail.Supervisor = match.Groups[2].ToString().Trim();
            //detail.Monitor = match.Groups[3].ToString().Trim();
            ////IssueDateTime,IssueTimeRange
            //regex = new Regex(@"\s?监测日期：(.+)\s+监测时间：(.+)-(.+)\s?");
            //match = regex.Match(sheet.GetCellValueAsString(4, 1));
            //if (match.Groups.Count != 4)
            //    return ParseResult.DateTime_ParseFailure;
            //var dateArgs = match.Groups[1].ToString().Split('.').Select(c => int.Parse(c)).ToArray();
            //var startTimeArgs = match.Groups[2].ToString().Split(':').Select(c => int.Parse(c)).ToArray();
            //var endTimeArgs = match.Groups[3].ToString().Split(':').Select(c => int.Parse(c)).ToArray();
            //var startTime = new DateTime(dateArgs[0], dateArgs[1], dateArgs[2], startTimeArgs[0], startTimeArgs[1], 0);
            //var endTime = new DateTime(dateArgs[0], dateArgs[1], dateArgs[2], endTimeArgs[0], endTimeArgs[1], 0);
            //if (endTime < startTime)//如23:00-1:00认为跨天 但是跨天不支持10:00-11:00(第二天的11:00),这种情况跨天比较特殊,甚至跨多天,这个需要另提需求处理
            //    endTime.AddDays(1);
            ////检测:监测时间的日期需与列表一致
            //if (startTime.Date != detail.IssueDateTime.Date)
            //    return ParseResult.DateTime_Invalid;
            //detail.IssueDateTime = startTime;
            //detail.IssueTimeRange = (short)(endTime - startTime).TotalMinutes;
            ////InstrumentName,InstrumentCode
            //regex = new Regex(@"\s?仪器名称：(.+)\s+仪器编号：(.+)\s?");
            //match = regex.Match(sheet.GetCellValueAsString(5, 1));
            //if (match.Groups.Count != 3)
            //    return ParseResult.Instrument_ParseFailure;
            //detail.InstrumentName = match.Groups[1].ToString().Trim();
            //detail.InstrumentCode = match.Groups[2].ToString().Trim();
            ////定位点集合 默认从9,1开始
            //int dataColumns = 8;//数据列数
            //int rowSpan = 30;//上下分段间隔不超过30行
            //int startRow = 9;
            //var dataRanges = GetDataRangesFromMultipleTwoParagraphsContent(sheet, startRow, dataColumns, rowSpan);
            ////节点解析
            //List<TNode> nodes;
            //int emptyCountToStop = 2;//2即空两行则跳过
            //nodes = GetNodes(detail, sheet, dataRanges, emptyCountToStop, (s, r, range) =>
            //{
            //    return new SkewBackDataV1(
            //         s.GetCellValueAsString(r, range.StartColumn).Trim(),
            //         s.GetCellValueAsString(r, range.StartColumn + 1).Trim(),
            //         s.GetCellValueAsString(r, range.StartColumn + 2).Trim(),
            //         s.GetCellValueAsString(r, range.StartColumn + 6).Trim(),
            //         s.GetCellValueAsString(r, range.StartColumn + 7).Trim());
            //});
            //detail.Nodes.AddRange(nodes);
            //return ParseResult.Success;
        }
    }
    public class SkewBackDataV1 : ITDepthNodeData
    {
        /// <summary>
        /// str需为this.ToString()的数据
        /// </summary>
        /// <param name="str"></param>
        public SkewBackDataV1()
        {
        }
        public SkewBackDataV1(string nodeCode, string depth, string str)
        {
            DeserializeFromString(nodeCode, depth, str);
        }
        public SkewBackDataV1(string nodeCode, string depth, string previousChange, string currentChange, string previousSum, string currentSum)
        {
            NodeCode = nodeCode;
            Depth = depth;
            PreviousChange = previousChange;
            CurrentChange = currentChange;
            PreviousSum = previousSum;
            CurrentSum = currentSum;
        }

        /// <summary>
        /// 测点编号
        /// </summary>
        public string NodeCode { set; get; }
        /// <summary>
        /// 深度(m)
        /// </summary>
        public string Depth { set; get; }
        /// <summary>
        /// 上次位移量(mm)
        /// </summary>
        public string PreviousChange { set; get; }
        /// <summary>
        /// 本次位移量(mm)
        /// </summary>
        public string CurrentChange { set; get; }
        /// <summary>
        /// 上次累计(mm)
        /// </summary>
        public string PreviousSum { set; get; }
        /// <summary>
        /// 本次累计(mm)
        /// </summary>
        public string CurrentSum { set; get; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Conment
        {
            get
            {
                float f;
                return float.TryParse(PreviousChange, out f)
                    && float.TryParse(CurrentChange, out f)
                    && float.TryParse(PreviousSum, out f)
                    && float.TryParse(CurrentSum, out f)
                    ? "" : "点位破坏";
            }
        }
        public string SerializeToString()
        {
            return $"{PreviousChange},{CurrentChange},{PreviousSum},{CurrentSum}";
        }
        public void DeserializeFromString(string nodeCode, string depth, string str)
        {
            NodeCode = nodeCode;
            Depth = depth;
            var args = str.Split(',');
            PreviousChange = args[0];
            CurrentChange = args[1];
            PreviousSum = args[2];
            CurrentSum = args[3];
        }

        float _CurrentChanges_Float = float.MinValue;
        public float CurrentChanges_Float
        {
            get
            {
                throw new NotImplementedException();
                if (_CurrentChanges_Float == float.MinValue)
                {
                    float result = float.MinValue;
                    if (!float.TryParse(CurrentChange, out result))
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
        float _CurrentSum_Float = float.MinValue;
        public float CurrentSum_Float
        {
            get
            {
                throw new NotImplementedException();
                if (_CurrentSum_Float == float.MinValue)
                {
                    float result = float.MinValue;
                    if (!float.TryParse(CurrentSum, out result))
                    {
                        _CurrentSum_Float = float.NaN;
                    }
                    else
                    {
                        _CurrentSum_Float = Math.Abs(result);//计算以绝对值为准
                    }
                }
                return _CurrentSum_Float;
            }
        }
    }
    public class SkewBackCollection<T>
        : ITDepthNodeDataCollection<T>
        where T : SkewBackDataV1, new()
    {
        List<T> _Datas = new List<T>();
        public IEnumerable<T> Datas
        {
            get
            {
                return _Datas.AsEnumerable();
            }
        }
        public void Add(string nodeCode, string depth, string nodeString)
        {
            var t = new T();
            t.DeserializeFromString(nodeCode, depth, nodeString);
            _Datas.Add(t);
        }
        public void Remove(string nodeCode, string depth)
        {
            _Datas.Remove(_Datas.First(c => c.NodeCode == nodeCode && c.Depth == depth));
        }
        public IEnumerable<T> GetCurrentMaxNodes()
        {
            var maxFloat = _Datas.Max(p => p.CurrentChanges_Float);
            return _Datas.Where(c => c.CurrentChanges_Float == maxFloat);
        }
        public IEnumerable<T> GetTotalMaxNodes()
        {
            var maxFloat = _Datas.Max(p => p.CurrentSum_Float);
            return _Datas.Where(c => c.CurrentSum_Float == maxFloat);
        }
        public IEnumerable<T> GetCloseWarn(WarnSettings warnSettings, TDetail detail)
        {
            return getWarnResult(warnSettings, detail, WarnSettings.CloseCoefficient);
        }
        public IEnumerable<T> GetOverWarn(WarnSettings warnSettings, TDetail detail)
        {
            return getWarnResult(warnSettings, detail, WarnSettings.OverCoefficient);
        }
        private IEnumerable<T> getWarnResult(WarnSettings warnSettings, TDetail detail, double warnCoefficientMin)//, double warnCoefficientMax = double.NaN)
        {
            throw new NotImplementedException("该类型无可查看的构件内容");
        }
    }
}
