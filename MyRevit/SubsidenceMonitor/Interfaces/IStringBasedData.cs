namespace MyRevit.SubsidenceMonitor.Interfaces
{
    public interface IStringBasedData
    {
        void DeserializeFromString(string str);
        string SerializeToString();
    }
}
