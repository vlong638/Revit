using System;

namespace MyRevit.EarthWork.Entity
{
    class SaveKeyHelper
    {
        //TODO 改为Enum取值更简洁...
        public enum SaveKeyType
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
        public static string GetSaveKey(SaveKeyType saveKeyType, int id)
        {
            switch (saveKeyType)
            {
                case SaveKeyType.EarthworkBlocking:
                    return nameof(EarthworkBlocking);
                case SaveKeyType.EarthworkBlocking_Size:
                    return nameof(EarthworkBlocking) + "Size";
                case SaveKeyType.EarthworkBlock:
                    return nameof(EarthworkBlocking) + nameof(EarthworkBlock) + id;
                case SaveKeyType.EarthworkBlock_Size:
                    return nameof(EarthworkBlocking) + nameof(EarthworkBlock) + id + "Size";
                case SaveKeyType.EarthworkBlockCPSettings:
                    return nameof(EarthworkBlocking) + nameof(EarthworkBlockCPSettings) + id;
                case SaveKeyType.EarthworkBlockCPSettings_Size:
                    return nameof(EarthworkBlocking) + nameof(EarthworkBlockCPSettings) + id + "Size";
                case SaveKeyType.EarthworkBlockImplementationInfo:
                    return nameof(EarthworkBlocking) + nameof(EarthworkBlockImplementationInfo) + id;
                case SaveKeyType.EarthworkBlockImplementationInfo_Size:
                    return nameof(EarthworkBlocking) + nameof(EarthworkBlockImplementationInfo) + id + "Size";
                default:
                    throw new NotImplementedException("暂不支持该类型");
            }
        }

        public static string GetSaveKeyOfEarthworkBlocking()
        {
            return nameof(EarthworkBlocking);
        }
        public static string GetSaveKeyOfEarthworkBlock(int id)
        {
            return nameof(EarthworkBlocking) + nameof(EarthworkBlock) + id;
        }
        public static string GetSaveKeyOfEarthworkBlockCPSettings(int id)
        {
            return nameof(EarthworkBlocking) + nameof(EarthworkBlockCPSettings) + id;
        }
        public static string GetSaveKeyOfEarthworkBlockImplementationInfo(int id)
        {
            return nameof(EarthworkBlocking) + nameof(EarthworkBlockImplementationInfo) + id;
        }

        public static string GetSaveKeyOfEarthworkBlockingSize()
        {
            return nameof(EarthworkBlocking) + "Size";
        }
        public static string GetSaveKeyOfEarthworkBlockSize(int id)
        {
            return nameof(EarthworkBlocking) + nameof(EarthworkBlock) + id + "Size";
        }
        public static string GetSaveKeyOfEarthworkBlockCPSettingsSize(int id)
        {
            return nameof(EarthworkBlocking) + nameof(EarthworkBlockCPSettings) + id + "Size";
        }
        public static string GetSaveKeyOfEarthworkBlockImplementationInfoSize(int id)
        {
            return nameof(EarthworkBlocking) + nameof(EarthworkBlockImplementationInfo) + id + "Size";
        }
    }
}
