//using System;
//using System.Collections.Generic;

//namespace MyRevit.SubsidenceMonitor.Interfaces
//{
//    public interface ITNodeData
//    {
//        void DeserializeFromString(string str);
//        string SerializeToString();
//    }
//    public interface ITNodeDataCollection<out TOut>
//        where TOut : ITNodeData
//    {
//        IEnumerable<TOut> GetCurrentMaxNode();
//        IEnumerable<TOut> GetTotalMaxNode();
//    }
//    public abstract class IAbstractTNodeDataCollection<T> : List<T>, ITNodeDataCollection<T>
//        where T : ITNodeData
//    {
//        public abstract IEnumerable<T> GetCurrentMaxNode();
//        public abstract IEnumerable<T> GetTotalMaxNode();
//    }
//    /// <summary>
//    /// 建筑物沉降.Data
//    /// </summary>
//    public class BuildingSubsidenceDataV1 : ITNodeData
//    {
//        /// <summary>
//        /// str需为this.ToString()的数据
//        /// </summary>
//        /// <param name="str"></param>
//        public BuildingSubsidenceDataV1(string nodeCode, string str)
//        {
//            NodeCode = nodeCode;
//            DeserializeFromString(str);
//        }
//        public BuildingSubsidenceDataV1(string nodeCode, string currentChanges, string sumChanges, string sumPeriodBuildingEnvelope, string sumBuildingEnvelope)
//        {
//            NodeCode = nodeCode;
//            CurrentChanges = currentChanges;
//            SumChanges = sumChanges;
//            SumPeriodBuildingEnvelope = sumPeriodBuildingEnvelope;
//            SumBuildingEnvelope = sumBuildingEnvelope;
//        }

//        /// <summary>
//        /// 测点编号
//        /// </summary>
//        public string NodeCode { set; get; }
//        /// <summary>
//        /// 本次变量(mm)
//        /// </summary>
//        public string CurrentChanges { set; get; }
//        /// <summary>
//        /// 累计变量(mm)
//        /// </summary>
//        public string SumChanges { set; get; }
//        /// <summary>
//        /// 围护结构施工期间累计值（mm）
//        /// </summary>
//        public string SumPeriodBuildingEnvelope { set; get; }
//        /// <summary>
//        /// 总累计值（mm）
//        /// </summary>
//        public string SumBuildingEnvelope { set; get; }
//        /// <summary>
//        /// 备注
//        /// </summary>
//        public string Conment
//        {
//            get
//            {
//                float f;
//                return float.TryParse(CurrentChanges, out f)
//                    && float.TryParse(SumChanges, out f)
//                    && float.TryParse(SumPeriodBuildingEnvelope, out f)
//                    && float.TryParse(SumBuildingEnvelope, out f) ? "" : "点位破坏";
//            }
//        }
//        public string SerializeToString()
//        {
//            return $"{CurrentChanges},{SumChanges},{SumPeriodBuildingEnvelope},{SumBuildingEnvelope}";
//        }
//        public void DeserializeFromString(string str)
//        {
//            var args = str.Split(',');
//            CurrentChanges = args[0];
//            SumChanges = args[1];
//            SumPeriodBuildingEnvelope = args[2];
//            SumBuildingEnvelope = args[3];
//        }

//        float _CurrentChanges_Float = float.MinValue;
//        public float CurrentChanges_Float
//        {
//            get
//            {
//                if (_CurrentChanges_Float == float.MinValue)
//                {
//                    float result = float.MinValue;
//                    if (!float.TryParse(CurrentChanges, out result))
//                    {
//                        _CurrentChanges_Float = float.NaN;
//                    }
//                }
//                return _CurrentChanges_Float;
//            }
//        }
//        float _SumChanges_Float = float.MinValue;
//        public float SumChanges_Float
//        {
//            get
//            {
//                if (_SumChanges_Float == float.MinValue)
//                {
//                    float result = float.MinValue;
//                    if (!float.TryParse(SumChanges, out result))
//                    {
//                        _SumChanges_Float = float.NaN;
//                    }
//                }
//                return _SumChanges_Float;
//            }
//        }
//    }
//    public class BuildingSubsidenceCollection<T> : IAbstractTNodeDataCollection<T>
//        where T : BuildingSubsidenceDataV1
//    {
//        public override IEnumerable<T> GetCurrentMaxNode()
//        {
//            return this.Where(c => c.CurrentChanges_Float == this.Max(p => p.CurrentChanges_Float));
//        }
//        public override IEnumerable<T> GetTotalMaxNode()
//        {
//            return this.Where(c => c.SumChanges_Float == this.Max(p => p.SumChanges_Float));
//        }
//    }
//}
