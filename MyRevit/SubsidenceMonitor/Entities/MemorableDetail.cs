using MyRevit.SubsidenceMonitor.Interfaces;
using MyRevit.SubsidenceMonitor.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.SubsidenceMonitor.Entities
{
    /// <summary>
    /// 职责:作为业务逻辑的Facade
    /// </summary>
    public class MemorableDetail : MemorableData<object, TDetail>
    {
        public MemorableDetail(object storage, TDetail data) : base(storage, data)
        {
        }

        public override BLLResult Commit()
        {
            return WriteFacade.DataCommit(Data.List, Data, Data.Nodes);
        }
        public override BLLResult Delete()
        {
            return WriteFacade.DeleteDetail(Data);
        }
        public override void Rollback()
        {
            Data = Memo;
            Memo = Clone();
            //TODO 回退需要取消掉被刷新过的内容
        }
        protected override TDetail Clone()
        {
            var memo = new TDetail()
            {
                IssueType = Data.IssueType,
                IssueDateTime = Data.IssueDateTime,
                IssueTimeRange = Data.IssueTimeRange,
                ReportName = Data.ReportName,
                Contractor = Data.Contractor,
                Supervisor = Data.Supervisor,
                Monitor = Data.Monitor,
                InstrumentName = Data.InstrumentName,
                InstrumentCode = Data.InstrumentCode,
                CloseCTSettings = Data.CloseCTSettings,
                OverCTSettings = Data.OverCTSettings,
                IsLoad = Data.IsLoad,
                List = Data.List,//List可以拷贝引用
                //Nodes需重建Copy
            };
            memo.Nodes.AddRange(Data.Nodes.Select(c => new TNode()
            {
                IssueType = c.IssueType,
                IssueDateTime = c.IssueDateTime,
                NodeCode = c.NodeCode,
                Data = c.Data,
                ElementIds = c.ElementIds,
                Index = c.Index,
            }));
            return memo;
        }
    }
}
