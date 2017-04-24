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
    }
}
