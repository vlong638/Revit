namespace MyRevit.SubsidenceMonitor.Interfaces
{
    public interface ILazyLoadData
    {
        bool IsLoad { set; get; }
        void LoadData();
    }
}
