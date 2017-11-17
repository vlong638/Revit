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
    public class STBAP : EIssueTypeEntity
    {
        public override EIssueType IssueType { get { return EIssueType.钢支撑轴力监测; } }
        public override string SheetName { get { return "钢支撑轴力"; } }

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
                return ParseResult.Date_Invalid;
            if (!string.IsNullOrEmpty(detail.InstrumentCode) && startTime.Hour != detail.IssueDateTime.Hour)
                return ParseResult.Time_Invalid;
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
            int dataColumns = 6;//数据列数
            int rowSpan = 30;//上下分段间隔不超过30行
            int startRow = 8;
            var dataRanges = GetDataRangesFromMultipleTwoParagraphsContent(sheet, startRow, dataColumns, rowSpan);
            //节点解析
            List<TNode> nodes;
            int emptyCountToStop = 2;//2即空两行则跳过
            nodes = GetNodes(detail, sheet, dataRanges, emptyCountToStop, (s, r, range) =>
            {
                return new STBAPDataV1(
                     s.GetCellValueAsString(r, range.StartColumn).Trim(),
                     s.GetCellValueAsString(r, range.StartColumn + 2).Trim(),
                     s.GetCellValueAsString(r, range.StartColumn + 3).Trim(),
                     s.GetCellValueAsString(r, range.StartColumn + 4).Trim());
            });
            detail.Nodes.AddRange(nodes);
            return ParseResult.Success;
        }
    }
    public class STBAPDataV1 : ITNodeData
    {
        /// <summary>
        /// str需为this.ToString()的数据
        /// </summary>
        /// <param name="str"></param>
        public STBAPDataV1()
        {
        }
        public STBAPDataV1(string nodeCode, string str)
        {
            DeserializeFromString(nodeCode, str);
        }
        public STBAPDataV1(string nodeCode, string axialForce, string currentChanges, string sumChanges)
        {
            NodeCode = nodeCode;
            AxialForce = axialForce;
            CurrentChanges = currentChanges;
            SumChanges = sumChanges;
        }

        /// <summary>
        /// 测点编号
        /// </summary>
        public string NodeCode { set; get; }
        /// <summary>
        /// 轴力值(KN)
        /// </summary>
        public string AxialForce { set; get; }
        /// <summary>
        /// 本次变量(mm)
        /// </summary>
        public string CurrentChanges { set; get; }
        /// <summary>
        /// 累计变量(mm)
        /// </summary>
        public string SumChanges { set; get; }
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
                     && float.TryParse(AxialForce, out f)
                    ? "" : "点位破坏";
            }
        }
        public string SerializeToString()
        {
            return $"{AxialForce},{CurrentChanges},{SumChanges}";
        }
        public void DeserializeFromString(string nodeCode, string str)
        {
            NodeCode = nodeCode;
            var args = str.Split(',');
            AxialForce = args[0];
            CurrentChanges = args[1];
            SumChanges = args[2];
        }

        float _AxialForce_Float = float.MinValue;
        public float AxialForce_Float
        {
            get
            {
                if (_AxialForce_Float == float.MinValue)
                {
                    float result = float.MinValue;
                    if (!float.TryParse(AxialForce, out result))
                    {
                        _AxialForce_Float = float.NaN;
                    }
                    else
                    {
                        _AxialForce_Float = Math.Abs(result);//计算以绝对值为准
                    }
                }
                return _AxialForce_Float;
            }
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
    public class STBAPCollection<T>
        : ITNodeDataCollection<T>
        where T : STBAPDataV1, new()
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
            return getWarnResult(warnSettings, detail, WarnSettings.CloseCoefficient);
        }
        public IEnumerable<T> GetOverWarn(WarnSettings warnSettings, TDetail detail)
        {
            return getWarnResult(warnSettings, detail, WarnSettings.OverCoefficient);
        }
        /// <summary>
        /// 只有warnCoefficientMin,表只有下界,过界则录入
        /// 有min和max则有上下界,界内录入
        /// </summary>
        /// <param name="warnSettings"></param>
        /// <param name="detail"></param>
        /// <param name="warnCoefficientMin"></param>
        /// <param name="warnCoefficientMax"></param>
        /// <returns></returns>
        private IEnumerable<T> getWarnResult(WarnSettings warnSettings, TDetail detail, double warnCoefficientMin)//, double warnCoefficientMax = double.NaN)
        {
            //算法
            List<T> result = new List<T>();
            var d = Datas.FirstOrDefault();
            if (d == null)
                return result;

            var totalHourRange = 24;
            var endTime = detail.IssueDateTime;
            var details = Facade.GetDetailsByTimeRange(detail.IssueType, endTime.AddHours(-totalHourRange), endTime);
            var orderedDetails = details.OrderByDescending(c => c.IssueDateTime).ToList();
            var currentDetail = detail;
            //需预警的节点
            var maxAxle = warnSettings.STBAP_MaxAxle;
            foreach (var data in Datas)
            {
                if (data.AxialForce_Float >= maxAxle * warnCoefficientMin)
                    result.Add(data);

                //if (double.IsNaN(warnCoefficientMax))
                //{
                //    if (data.AxialForce_Float >= maxAxle * warnCoefficientMin)
                //        result.Add(data);
                //}
                //else
                //{
                //    if (data.AxialForce_Float >= maxAxle * warnCoefficientMin
                //        && data.AxialForce_Float < maxAxle * warnCoefficientMax)
                //        result.Add(data);
                //}
            }
            var minAxle = warnSettings.STBAP_MinAxle;
            foreach (var data in Datas)
            {
                if (data.AxialForce_Float <= minAxle / warnCoefficientMin)
                    result.Add(data);

                //if (double.IsNaN(warnCoefficientMax))
                //{
                //    if (data.AxialForce_Float <= minAxle / warnCoefficientMin)
                //        result.Add(data);
                //}
                //else
                //{
                //    if (data.AxialForce_Float <= minAxle / warnCoefficientMin
                //        && data.AxialForce_Float > minAxle / warnCoefficientMax)
                //        result.Add(data);
                //}
            }
            return result;
        }
    }
}
