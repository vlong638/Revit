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

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { set; get; }
        /// <summary>
        /// 无支撑暴露时间
        /// </summary>
        public int ExposureTime { set; get; }
        /// <summary>
        /// 完成事件
        /// </summary>
        public DateTime EndTime { set; get; }
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
        public System.Drawing.Color ColorForUnsettled { set; get; }
        public System.Drawing.Color ColorForSettled { set; get; }
        /// <summary>
        /// 已完成的节点更改节点名称后
        /// </summary>
        public void Unsettle()
        {
            //TODO 节点名称变更后
            //当前面土方分块节点重命名或有增减后，用户再次打开此界面则提示“分段内容有变动，请修改相应工期设置”
            throw new NotImplementedException();
        }
        public void Preview()
        {
            //TODO 预览
            throw new NotImplementedException();
        }
    }
}
