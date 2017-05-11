using MyRevit.SubsidenceMonitor.Interfaces;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Collections;

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
                    if (!float.TryParse(SumChanges, out result))
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
        public IEnumerable<T> GetCloseWarn(WarnSettings warnSettings)
        {
            //TODO 暂为GetTotalMaxNodes()需修改为 接近预警值
            var maxFloat = _Datas.Max(p => p.SumBuildingEnvelope_Float);
            return _Datas.Where(c => c.SumBuildingEnvelope_Float == maxFloat);
        }
        public IEnumerable<T> GetOverWarn(WarnSettings warnSettings)
        {
            //TODO 暂为GetTotalMaxNodes()需修改为 超过预警值
            var maxFloat = _Datas.Max(p => p.SumBuildingEnvelope_Float);
            return _Datas.Where(c => c.SumBuildingEnvelope_Float == maxFloat);
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
