namespace MyRevit.EarthWork.Entity
{
    class SaveKeyHelper
    {
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
