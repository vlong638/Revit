namespace MyRevit.SubsidenceMonitor.Interfaces
{
    public interface ITNodeData
    {
        string NodeCode { set; get; }
        void DeserializeFromString(string nodeCode, string str);
        string SerializeToString();
    }
}
