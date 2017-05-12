using MyRevit.SubsidenceMonitor.Interfaces;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Collections;
using MyRevit.SubsidenceMonitor.Operators;

namespace MyRevit.SubsidenceMonitor.Entities
{
    public class BuildingSubsidenceDataV1 : ITNodeData
    {
        /// <summary>
        /// str需为this.ToString()的数据
        /// </summary>
        /// <param name="str"></param>
        public BuildingSubsidenceDataV1()
        {
        }
        public BuildingSubsidenceDataV1(string nodeCode, string str)
        {
            DeserializeFromString(nodeCode, str);
        }
        public BuildingSubsidenceDataV1(string nodeCode, string currentChanges, string sumChanges, string sumPeriodBuildingEnvelope, string sumBuildingEnvelope)
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
                    && float.TryParse(SumBuildingEnvelope, out f) ? "" : "点位破坏";
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
        float _SumBuildingEnvelope = float.MinValue;
        public float SumBuildingEnvelope_Float
        {
            get
            {
                if (_SumBuildingEnvelope == float.MinValue)
                {
                    float result = float.MinValue;
                    if (!float.TryParse(SumBuildingEnvelope, out result))
                    {
                        _SumBuildingEnvelope = float.NaN;
                    }
                    else
                    {
                        _SumBuildingEnvelope = Math.Abs(result);//计算以绝对值为准
                    }
                }
                return _SumBuildingEnvelope;
            }
        }
    }
    public class BuildingSubsidenceCollection<T>
        : ITNodeDataCollection<T>
        where T : BuildingSubsidenceDataV1, new()
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
            var maxFloat = _Datas.Max(p => p.SumBuildingEnvelope_Float);
            return _Datas.Where(c => c.SumBuildingEnvelope_Float == maxFloat);
        }
        public IEnumerable<T> GetCloseWarn(WarnSettings warnSettings,TDetail detail)
        {
            return getWarnResult(warnSettings, detail, 0.8f, 1);
        }
        private IEnumerable<T> getWarnResult(WarnSettings warnSettings, TDetail detail, double warnCoefficientMin, double warnCoefficientMax=double.NaN)
        {
            List<T> result = new List<T>();
            var d = Datas.FirstOrDefault();
            if (d == null)
                return result;

            var totalHourRange = warnSettings.BuildingSubsidence_Day * 24;
            var endTime = detail.IssueDateTime;
            var details = Facade.GetDetailsByTimeRange(detail.IssueType, endTime.AddHours(-totalHourRange), endTime);
            var orderedDetails = details.OrderByDescending(c => c.IssueDateTime).ToList();
            var currentDetail = detail;
            //需预警的节点
            //监测 warnSettings.BuildingSubsidence_SumMillimeter;
            foreach (var data in Datas)
            {
                if (double.IsNaN(warnCoefficientMax))
                {
                    if (data.SumBuildingEnvelope_Float >= warnSettings.BuildingSubsidence_SumMillimeter * warnCoefficientMin)
                        result.Add(data);
                }
                else
                {
                    if (data.SumBuildingEnvelope_Float >= warnSettings.BuildingSubsidence_SumMillimeter * warnCoefficientMin
                        && data.SumBuildingEnvelope_Float < warnSettings.BuildingSubsidence_SumMillimeter * warnCoefficientMax)
                        result.Add(data);
                }
            }
            //监测 warnSettings.BuildingSubsidence_DailyMillimeter;
            //数据天数达标监测
            if (totalHourRange != 0)
            {
                double warnDailyMillimeterMin = warnSettings.BuildingSubsidence_DailyMillimeter * warnCoefficientMin;
                double warnDailyMillimeterMax = 0;
                if (!double.IsNaN(warnCoefficientMax))
                {
                    warnDailyMillimeterMax = warnSettings.BuildingSubsidence_DailyMillimeter * warnCoefficientMax;
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
                    int days = warnSettings.BuildingSubsidence_Day;
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

        public IEnumerable<T> GetOverWarn(WarnSettings warnSettings, TDetail detail)
        {
            return getWarnResult(warnSettings, detail, 1);
        }
    }

    #region 协变,逆变
    //interface IData<T>
    //{
    //    T Data { set; get; }
    //}
    //interface IInNode<in T>
    //{
    //    void SetT(T t);
    //}
    //interface IOutNode<out T>
    //{
    //    T GetT();
    //}
    //class IObject<T> : IData<T>, IInNode<T>, IOutNode<T>
    //{
    //    T _Data;
    //    public T Data
    //    {
    //        set { SetT(value); }
    //        get { return GetT(); }
    //    }
    //    public T GetT()
    //    {
    //        throw new NotImplementedException();
    //    }
    //    public void SetT(T t)
    //    {
    //        _Data = t;
    //    }
    //}
    #endregion
}
