namespace MyRevit.SubsidenceMonitor.Interfaces
{
    public interface ITNodeData
    {
        void DeserializeFromString(string str);
        string SerializeToString();
    }
}
