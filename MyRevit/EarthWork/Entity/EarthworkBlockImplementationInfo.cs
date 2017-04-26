using Newtonsoft.Json;
using System;

namespace MyRevit.EarthWork.Entity
{
    /// <summary>
    /// 节点施工信息
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class EarthworkBlockImplementationInfo
    {
        public EarthworkBlockImplementationInfo()
        {
        }

        [JsonIgnore]
        public string Name { set; get; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { set; get; } = DateTime.MinValue;
        [JsonIgnore]
        public string StartTimeStr
        {
            set
            {
                StartTime = DateTime.Parse(value);
            }
            get
            {
                if (StartTime == DateTime.MinValue)
                {
                    return "";
                }
                return StartTime.ToString("yyyy/MM/dd HH:mm");
            }
        }
        /// <summary>
        /// 无支撑暴露时间
        /// </summary>
        public int ExposureTime { set; get; }
        /// <summary>
        /// 完成事件
        /// </summary>
        public DateTime EndTime { set; get; } = DateTime.MinValue;
        [JsonIgnore]
        public string EndTimeStr
        {
            set
            {
                EndTime = DateTime.Parse(value);
            }
            get
            {
                if (EndTime == DateTime.MinValue)
                {
                    return "";
                }
                return EndTime.ToString("yyyy/MM/dd HH:mm");
            }
        }
        /// <summary>
        /// 已被完成
        /// </summary>
        [JsonIgnore]
        public bool IsSettled
        {
            get
            {
                return StartTime != DateTime.MinValue
              && EndTime != DateTime.MinValue
              && ExposureTime != 0;
            }
        }
        public bool IsConflicted { set; get; }
        public System.Drawing.Color ColorForUnsettled { set; get; }
        public System.Drawing.Color ColorForSettled { set; get; }
        public void Preview()
        {
            //TODO 预览
            throw new NotImplementedException();
        }
    }
}
