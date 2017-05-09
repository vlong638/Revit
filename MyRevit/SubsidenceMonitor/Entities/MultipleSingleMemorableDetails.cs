using Autodesk.Revit.DB;
using Microsoft.Office.Interop.Excel;
using MyRevit.SubsidenceMonitor.Interfaces;
using System.Collections.Generic;
using System.Linq;

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
        public void AddElementIds(string nodeCode,List<ElementId> elementIds)
        {
            var targetNode = MemorableData.Data.Nodes.First(c => c.NodeCode == nodeCode);
            foreach (var elementId in elementIds)
            {
                var elementId_Int = elementId.IntegerValue;
                var elementNode = MemorableData.Data.Nodes.FirstOrDefault(c => c.ElementIds_Int.Contains(elementId_Int));
                if (elementNode == targetNode)
                    continue;
                if (elementNode != null && elementNode != targetNode)
                    elementNode.ElementIds_Int.Remove(elementId_Int);
                targetNode.ElementIds_Int.Add(elementId_Int);
            }
        }
        public void DeleteElementIds(string nodeCode, List<ElementId> elementIds)
        {
            var targetNode = MemorableData.Data.Nodes.First(c => c.NodeCode == nodeCode);
            foreach (var elementId in elementIds)
            {
                var elementId_Int = elementId.IntegerValue;
                targetNode.ElementIds_Int.Remove(elementId_Int);
            }
        }
        public List<ElementId> GetElementIds(string nodeCode,Document doc)
        {
            List<ElementId> availableElementIds = new List<ElementId>();
            var targetNode = MemorableData.Data.Nodes.First(c => c.NodeCode == nodeCode);
            var storedElements = targetNode.ElementIds_Int.Select(c => doc.GetElement(new ElementId(c)));
            foreach (var element in storedElements)
                if (element != null)
                    availableElementIds.Add(element.Id);
                else
                    targetNode.ElementIds_Int.Remove(element.Id.IntegerValue);//获取ElementId的时候,需在文档中监测Element是否存在,移除无效的Element(可能被用户在文档中删除)
            return availableElementIds;
        }
        public List<ElementId> GetElementIds(List<string> nodeCodes, Document doc)
        {
            List<ElementId> availableElementIds = new List<ElementId>();
            foreach (var nodeCode in nodeCodes)
            {
                availableElementIds.AddRange(GetElementIds(nodeCode, doc));
            }
            return availableElementIds;
        }
        public List<ElementId> GetAllElementIds(Document doc)
        {
            List<ElementId> availableElementIds = new List<ElementId>();
            foreach (var node in MemorableData.Data.Nodes)
            {
                availableElementIds.AddRange(GetElementIds(node.NodeCode, doc));
            }
            return availableElementIds;
        }
    }
}
