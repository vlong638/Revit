using Autodesk.Revit.DB;
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
    public class MemorableDetail : MemorableData<Document, TDetail,List<ElementParameterValueSet>>
    {
        public MemorableDetail(Document storage, TDetail data) : base(storage, data)
        {
        }

        public override BLLResult Commit(bool isCreateNew)
        {
            if (isCreateNew)
            {
                return Facade.CreateDetail(Data.List, Data, Data.Nodes);
            }
            else
            {
                return Facade.UpdateDetail(Data, Data.Nodes);
            }
        }
        public override BLLResult Delete()
        {
            return Facade.DeleteDetail(Data);
        }
        public override void Rollback()
        {
            Data = Memo;
            Memo = Clone();

            //回退需要取消掉被刷新过的内容
            using (var transaction = new Transaction(Storage, nameof(MemorableDetail) + nameof(Rollback)))
            {
                transaction.Start();
                try
                {
                    for (int i = TemporaryData.Count()-1; i >=0; i--)
                    {
                        var data = TemporaryData[i];
                        var element = Storage.GetElement(new ElementId(data.ElementId));
                        var parameter = element.GetParameters(data.ParameterName).First();
                        parameter.Set(data.ParameterValue);
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.RollBack();
                    throw new NotImplementedException(ex.Message);
                }
            }
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
                ElementIds = c.GetElementIds(),//2017/05/09 09:59  ElementIds总是以ElementIds_Int为修改对象,ElementIds_Int是ElementIds的活动的修改对象,ElementIds是静态的存储对象
                Index = c.Index,
            }));
            return memo;
        }
    }
}
