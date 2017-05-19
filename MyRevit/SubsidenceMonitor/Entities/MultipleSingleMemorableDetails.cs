using Autodesk.Revit.DB;
using Microsoft.Office.Interop.Excel;
using MyRevit.SubsidenceMonitor.Interfaces;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.SubsidenceMonitor.Entities
{
    public class MultipleSingleMemorableDetails : MultipleSingleMemorableData<Document, TList, TDetail>
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
        string GroupName = "监测系统";
        enum CustomParameters
        {
            监测类型,
            /// <summary>
            /// for All Except 钢支撑轴力监测
            /// </summary>
            测点编号,
            /// <summary>
            /// for 钢支撑轴力监测
            /// </summary>
            监测点,
        }
        public void RenderNodeInfoToElements(List<string> nodeCodes, Document doc)
        {
            var nodes = MemorableData.Data.Nodes.Where(c => nodeCodes.Contains(c.NodeCode));
            foreach (var node in nodes)
            {
                for (int i = node.ElementIds_Int.Count - 1; i >= 0; i--)
                {
                    var element = doc.GetElement(new ElementId(node.ElementIds_Int[i]));
                    if (element == null)
                    {
                        node.ElementIds_Int.Remove(node.ElementIds_Int[i]);//获取ElementId的时候,需在文档中监测
                        continue;
                    }
                    //共享参数检测 添加共享参数
                    var parameterName = CustomParameters.监测类型.ToString();
                    var parameter = element.GetParameters(parameterName).FirstOrDefault();
                    if (parameter == null)
                    {
                        Revit_Document_Helper.AddSharedParameter(doc, GroupName, parameterName, element.Category, true);
                        parameter = element.GetParameters(parameterName).FirstOrDefault();
                        SetParameterValue(element, parameterName, node.IssueType.ToString());
                        if (node.IssueType == EIssueType.钢支撑轴力监测)
                        {
                            parameterName = CustomParameters.监测点.ToString();
                            Revit_Document_Helper.AddSharedParameter(doc, GroupName, parameterName, element.Category, true);
                            SetParameterValue(element, parameterName, node.NodeCode);
                        }
                        else
                        {
                            parameterName = CustomParameters.测点编号.ToString();
                            Revit_Document_Helper.AddSharedParameter(doc, GroupName, parameterName, element.Category, true);
                            parameter = element.GetParameters(parameterName).First();
                            SetParameterValue(element, parameterName, node.NodeCode);
                        }
                    }
                    else
                    {
                        //赋值及冲突监测
                        var value = parameter.AsString();
                        if (string.IsNullOrEmpty(value))//无值处理
                            SetParameterValue(element, parameterName, node.IssueType.ToString());
                        else if (value != node.IssueType.ToString())//类型重复
                            throw new NotImplementedException($"构件({element.Name})存在重复,原先的类型:{value},现在的类型:{node.IssueType.ToString()}");
                        if (node.IssueType == EIssueType.钢支撑轴力监测)
                        {
                            parameterName = CustomParameters.监测点.ToString();
                            parameter = element.GetParameters(parameterName).First();
                            SetParameterValue(element, parameterName, node.NodeCode);
                        }
                        else
                        {
                            parameterName = CustomParameters.测点编号.ToString();
                            parameter = element.GetParameters(parameterName).First();
                            SetParameterValue(element, parameterName, node.NodeCode);
                        }
                    }
                }
            }
            Edited();
        }
        private void SetParameterValue(Element element, string parameterName, string parameterValue)
        {
            var parameter = element.GetParameters(parameterName).FirstOrDefault();
            MemorableData.TemporaryData.Add(new ElementParameterValueSet(element.Id.IntegerValue, parameterName, parameter.AsString()));
            parameter.Set(parameterValue);
        }

        #region ByTNode
        public void AddElementIds(string nodeCode, List<ElementId> elementIds)
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
        List<ElementId> GetElementIds(string nodeCode, Document doc)
        {
            List<ElementId> availableElementIds = new List<ElementId>();
            var targetNode = MemorableData.Data.Nodes.First(c => c.NodeCode == nodeCode);
            for (int i = targetNode.ElementIds_Int.Count - 1; i >= 0; i--)
            {
                var element = doc.GetElement(new ElementId(targetNode.ElementIds_Int[i]));
                if (element == null)
                    targetNode.ElementIds_Int.Remove(targetNode.ElementIds_Int[i]);//获取ElementId的时候,需在文档中监测
                else
                    availableElementIds.Add(element.Id);
            }
            return availableElementIds;
        }
        public List<ElementId> GetElementIdsByTNode(TNode node, Document doc)
        {
            return GetElementIds(node.NodeCode, doc);
        }
        public List<ElementId> GetElementIdsByTNode(List<TNode> nodes, Document doc)
        {
            var nodeCodes = nodes.Select(c => c.NodeCode).ToList();
            List<ElementId> availableElementIds = new List<ElementId>();
            foreach (var nodeCode in nodeCodes)
            {
                availableElementIds.AddRange(GetElementIds(nodeCode, doc));
            }
            return availableElementIds;
        }
        public List<ElementId> GetAllNodesElementIdsByTNode(Document doc)
        {
            List<ElementId> availableElementIds = new List<ElementId>();
            foreach (var node in MemorableData.Data.Nodes)
            {
                availableElementIds.AddRange(GetElementIds(node.NodeCode, doc));
            }
            return availableElementIds;
        }
        public List<ElementId> GetCurrentMaxNodesElementsByTNode(Document doc)
        {
            List<ElementId> results = new List<ElementId>();
            foreach (var node in MemorableData.Data.NodeDatas.GetCurrentMaxNodes())
            {
                results.AddRange(GetElementIds(node.NodeCode, doc));
            }
            return results;
        }
        public List<ElementId> GetTotalMaxNodesElementsByTNode(Document doc)
        {
            List<ElementId> results = new List<ElementId>();
            foreach (var node in MemorableData.Data.NodeDatas.GetTotalMaxNodes())
            {
                results.AddRange(GetElementIds(node.NodeCode, doc));
            }
            return results;
        }
        public List<ElementId> GetCloseWarnNodesElementsByTNode(Document doc, WarnSettings warnSettings)
        {
            List<ElementId> results = new List<ElementId>();
            foreach (var node in MemorableData.Data.NodeDatas.GetCloseWarn(warnSettings, MemorableData.Data))
            {
                results.AddRange(GetElementIds(node.NodeCode, doc));
            }
            return results;
        }
        public List<ElementId> GetOverWarnNodesElementsByTNode(Document doc, WarnSettings warnSettings)
        {
            List<ElementId> results = new List<ElementId>();
            foreach (var node in MemorableData.Data.NodeDatas.GetOverWarn(warnSettings, MemorableData.Data))
            {
                results.AddRange(GetElementIds(node.NodeCode, doc));
            }
            return results;
        }
        #endregion
        #region ByTDepthNode
        public void AddElementIds(string nodeCode, string depth, List<ElementId> elementIds)
        {
            var targetNode = MemorableData.Data.DepthNodes.First(c => c.NodeCode == nodeCode && c.Depth == depth);
            foreach (var elementId in elementIds)
            {
                var elementId_Int = elementId.IntegerValue;
                var elementNode = MemorableData.Data.DepthNodes.FirstOrDefault(c => c.ElementIds_Int.Contains(elementId_Int));
                if (elementNode == targetNode)
                    continue;
                if (elementNode != null && elementNode != targetNode)
                    elementNode.ElementIds_Int.Remove(elementId_Int);
                targetNode.ElementIds_Int.Add(elementId_Int);
            }
        }
        public void DeleteElementIds(string nodeCode, string depth, List<ElementId> elementIds)
        {
            var targetNode = MemorableData.Data.DepthNodes.First(c => c.NodeCode == nodeCode && c.Depth == depth);
            foreach (var elementId in elementIds)
            {
                var elementId_Int = elementId.IntegerValue;
                targetNode.ElementIds_Int.Remove(elementId_Int);
            }
        }
        List<ElementId> GetElementIds(string nodeCode, string depth, Document doc)
        {
            List<ElementId> availableElementIds = new List<ElementId>();
            var targetNode = MemorableData.Data.DepthNodes.First(c => c.NodeCode == nodeCode && c.Depth == depth);
            for (int i = targetNode.ElementIds_Int.Count - 1; i >= 0; i--)
            {
                var element = doc.GetElement(new ElementId(targetNode.ElementIds_Int[i]));
                if (element == null)
                    targetNode.ElementIds_Int.Remove(targetNode.ElementIds_Int[i]);//获取ElementId的时候,需在文档中监测
                else
                    availableElementIds.Add(element.Id);
            }
            return availableElementIds;
        }
        public List<ElementId> GetElementIdsByTDepthNode(TDepthNode node, Document doc)
        {
            return GetElementIds(node.NodeCode, node.Depth, doc);
        }
        public List<ElementId> GetElementIdsByTDepthNode(List<TDepthNode> nodes, Document doc)
        {
            List<ElementId> availableElementIds = new List<ElementId>();
            foreach (var node in nodes)
            {
                availableElementIds.AddRange(GetElementIds(node.NodeCode, node.Depth, doc));
            }
            return availableElementIds;
        }
        public List<ElementId> GetAllNodesElementIdsByTDepthNode(Document doc)
        {
            List<ElementId> availableElementIds = new List<ElementId>();
            foreach (var node in MemorableData.Data.DepthNodes)
            {
                availableElementIds.AddRange(GetElementIds(node.NodeCode, node.Depth, doc));
            }
            return availableElementIds;
        }
        public List<ElementId> GetCurrentMaxNodesElementsByTDepthNode(Document doc)
        {
            List<ElementId> results = new List<ElementId>();
            foreach (var node in MemorableData.Data.DepthNodeDatas.GetCurrentMaxNodes())
            {
                results.AddRange(GetElementIds(node.NodeCode, node.Depth, doc));
            }
            return results;
        }
        public List<ElementId> GetTotalMaxNodesElementsByTDepthNode(Document doc)
        {
            List<ElementId> results = new List<ElementId>();
            foreach (var node in MemorableData.Data.DepthNodeDatas.GetTotalMaxNodes())
            {
                results.AddRange(GetElementIds(node.NodeCode, doc));
            }
            return results;
        }
        public List<ElementId> GetCloseWarnNodesElementsByTDepthNode(Document doc, WarnSettings warnSettings)
        {
            List<ElementId> results = new List<ElementId>();
            foreach (var node in MemorableData.Data.DepthNodeDatas.GetCloseWarn(warnSettings, MemorableData.Data))
            {
                results.AddRange(GetElementIds(node.NodeCode, node.Depth, doc));
            }
            return results;
        }
        public List<ElementId> GetOverWarnNodesElementsByTDepthNode(Document doc, WarnSettings warnSettings)
        {
            List<ElementId> results = new List<ElementId>();
            foreach (var node in MemorableData.Data.DepthNodeDatas.GetOverWarn(warnSettings, MemorableData.Data))
            {
                results.AddRange(GetElementIds(node.NodeCode, node.Depth, doc));
            }
            return results;
        }
        #endregion
    }
}
