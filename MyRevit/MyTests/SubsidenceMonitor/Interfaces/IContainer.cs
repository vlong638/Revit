using System.Collections.Generic;

namespace MyRevit.SubsidenceMonitor.Interfaces
{
    public interface IContainer<T>
    {
        List<T> Datas { set; get; }
    }
}
