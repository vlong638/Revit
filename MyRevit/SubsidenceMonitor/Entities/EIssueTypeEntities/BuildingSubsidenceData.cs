using MyRevit.SubsidenceMonitor.Interfaces;

namespace MyRevit.SubsidenceMonitor.Entities
{
    /// <summary>
    /// 建筑物沉降.Data
    /// </summary>
    public class BuildingSubsidenceDataV1 : ITNodeData
    {
        /// <summary>
        /// str需为this.ToString()的数据
        /// </summary>
        /// <param name="str"></param>
        public BuildingSubsidenceDataV1(string nodeCode, string str)
        {
            NodeCode = nodeCode;
            DeserializeFromString(str);
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
        public void DeserializeFromString(string str)
        {
            var args = str.Split(',');
            CurrentChanges = args[0];
            SumChanges = args[1];
            SumPeriodBuildingEnvelope = args[2];
            SumBuildingEnvelope = args[3];
        }
    }
}
