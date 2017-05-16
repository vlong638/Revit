namespace MyRevit.SubsidenceMonitor.Interfaces
{
    public interface ITNodeData
    {
        string NodeCode { set; get; }
        void DeserializeFromString(string nodeCode, string str);//always key+value
        string SerializeToString();
    }
    public interface ITDepthNodeData
    {
        string NodeCode { set; get; }
        string Depth { set; get; }
        void DeserializeFromString(string nodeCode, string depth, string str);//always key+value
        string SerializeToString();
    }
}
