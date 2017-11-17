using MyRevit.SubsidenceMonitor.Entities;
using System.Collections.Generic;

namespace MyRevit.SubsidenceMonitor.Interfaces
{
    public interface ITNodeDataCollection<out T>
    {
        IEnumerable<T> Datas { get; }
        IEnumerable<T> GetCurrentMaxNodes();
        IEnumerable<T> GetTotalMaxNodes();
        IEnumerable<T> GetCloseWarn(WarnSettings warnSettings, TDetail detail);
        IEnumerable<T> GetOverWarn(WarnSettings warnSettings, TDetail detail);
        void Add(string nodeCode, string nodeString);
        void Remove(string nodeCode);
    }
    public interface ITDepthNodeDataCollection<out T>
    {
        IEnumerable<T> Datas { get; }
        IEnumerable<T> GetCurrentMaxNodes();
        IEnumerable<T> GetTotalMaxNodes();
        IEnumerable<T> GetCloseWarn(WarnSettings warnSettings, TDetail detail);
        IEnumerable<T> GetOverWarn(WarnSettings warnSettings, TDetail detail);
        void Add(string nodeCode, string depth, string nodeString);
        void Remove(string nodeCode, string depth);
    }
}
