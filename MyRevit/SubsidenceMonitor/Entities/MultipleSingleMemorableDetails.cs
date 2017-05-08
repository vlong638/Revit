using Autodesk.Revit.DB;
using Microsoft.Office.Interop.Excel;
using MyRevit.SubsidenceMonitor.Interfaces;
using System.Collections.Generic;

namespace MyRevit.SubsidenceMonitor.Entities
{
    public class MultipleSingleMemorableDetails : MultipleSingleMemorableData<object, TList, TDetail>
    {
        public MultipleSingleMemorableDetails() : base()
        {
        }
        public MultipleSingleMemorableDetails(Document storage, TList list) : base(storage, list)
        {
        }

        protected override TDetail createNew()
        {
            return new TDetail()
            {
                IssueType = List.IssueType,
                List = List,
                IssueDateTime = List.IssueDate,//新增项增加默认日期用于时间校对
                IsLoad = true,//新增项认为无需加载处理
            };
        }
        protected override ParseResult importExcel(Workbook workbook)
        {
            return MemorableData.Data.IssueType.GetEntity().ParseInto(workbook, MemorableData.Data);
        }
        protected override void UpdateMemorableData()
        {
            MemorableData = new MemorableDetail(Storage, Datas[DataIndex]);
            MemorableData.Start();
        }
        public void AddElementIds(List<ElementId> elementIds)
        {
            //TODO 添加构件
        

        }
        public void DeleteElementIds(List<ElementId> elementIds)
        {
            //TODO 删除构件

        }
    }
}
