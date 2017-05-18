using MyRevit.EarthWork.Entity;
using MyRevit.SubsidenceMonitor.Entities;
using MyRevit.SubsidenceMonitor.UI;
using System;

namespace MyRevit.Utilities
{
    public class SaveKeyHelper
    {
        //改为Enum取值更简洁...
        public enum SaveKeyTypeForEarthWork
        {
            EarthworkBlocking,
            EarthworkBlocking_Size,

            EarthworkBlock,
            EarthworkBlock_Size,

            EarthworkBlockCPSettings,
            EarthworkBlockCPSettings_Size,

            EarthworkBlockImplementationInfo,
            EarthworkBlockImplementationInfo_Size,
        }
        public static string GetSaveKey(SaveKeyTypeForEarthWork saveKeyType, int id)
        {
            switch (saveKeyType)
            {
                case SaveKeyTypeForEarthWork.EarthworkBlocking:
                    return nameof(EarthworkBlocking);
                case SaveKeyTypeForEarthWork.EarthworkBlocking_Size:
                    return nameof(EarthworkBlocking) + "Size";
                case SaveKeyTypeForEarthWork.EarthworkBlock:
                    return nameof(EarthworkBlocking) + nameof(EarthworkBlock) + id;
                case SaveKeyTypeForEarthWork.EarthworkBlock_Size:
                    return nameof(EarthworkBlocking) + nameof(EarthworkBlock) + id + "Size";
                case SaveKeyTypeForEarthWork.EarthworkBlockCPSettings:
                    return nameof(EarthworkBlocking) + nameof(EarthworkBlockCPSettings) + id;
                case SaveKeyTypeForEarthWork.EarthworkBlockCPSettings_Size:
                    return nameof(EarthworkBlocking) + nameof(EarthworkBlockCPSettings) + id + "Size";
                case SaveKeyTypeForEarthWork.EarthworkBlockImplementationInfo:
                    return nameof(EarthworkBlocking) + nameof(EarthworkBlockImplementationInfo) + id;
                case SaveKeyTypeForEarthWork.EarthworkBlockImplementationInfo_Size:
                    return nameof(EarthworkBlocking) + nameof(EarthworkBlockImplementationInfo) + id + "Size";
                default:
                    throw new NotImplementedException("暂不支持该类型");
            }
        }

        public enum SaveKeyTypeForSubsidenceMonitor
        {
            WarnSettings,
            ChartForm,
        }
        public static string GetSaveKey(SaveKeyTypeForSubsidenceMonitor saveKeyType, int id)
        {
            switch (saveKeyType)
            {
                case SaveKeyTypeForSubsidenceMonitor.WarnSettings:
                    return nameof(WarnSettingsForm);
                case SaveKeyTypeForSubsidenceMonitor.ChartForm:
                    return nameof(ChartForm);
                default:
                    throw new NotImplementedException("暂不支持该类型");
            }
        }
    }
}
