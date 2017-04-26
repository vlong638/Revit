using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.EarthWork.UI;
using MyRevit.Utilities;
using Newtonsoft.Json;
using PmSoft.Common.CommonClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyRevit.EarthWork.Entity
{
    #region 应用(新增,更改) 删除 区间化集合
    // //准备开始应用抽象(区间化集合)
    // interface SectionalData<TRoot,TElement>
    // {
    //     void ApplyNew(TRoot root, TElement element);
    //     void ApplyDelete(TRoot root, TElement element);
    // }
    // interface MemorableData<T>
    // {
    //     void Clone();
    //     void Rollback();
    // }
    // interface Cloneable<TNode>
    //{
    //     TNode Clone();
    // }
    // class SectionalDataCollection<TRoot,TNode,TElement> : MemorableData<TNode>
    //     where TNode : SectionalData<TRoot, TElement>, Cloneable<TNode>
    // {
    //     public SectionalDataCollection(List<TNode> nodes)
    //     {
    //         Datas = nodes;
    //         Clone();
    //     }

    //     List<TNode> Datas { set; get; } = new List<TNode>();
    //     List<TNode> News { set; get; } = new List<TNode>();
    //     List<TNode> Deletes { set; get; } = new List<TNode>();
    //     IEnumerable<TData> Memo { set; get; }


    //     public void Add(TData data)
    //     {
    //         News.Add(data);
    //         Datas.Add(data);
    //     }
    //     public void Delete(TData data)
    //     {
    //         Deletes.Add(data);
    //         Datas.Remove(data);
    //     }
    //     public void Apply()
    //     {
    //         foreach (var New in News)
    //         {
    //             New.ApplyNew();
    //         }
    //         News = new List<TData>();
    //         foreach (var Delete in Deletes)
    //         {
    //             Delete.ApplyDelete();
    //         }
    //         Deletes = new List<TData>();
    //         Clone();
    //     }
    //     public void Cancel()
    //     {
    //         News = new List<TData>();
    //         Deletes = new List<TData>();
    //         Rollback();
    //     }
    //     public void Clone()
    //     {
    //         Memo = Datas.Select(c => c.Clone());
    //     }
    //     public void Rollback()
    //     {
    //         Datas = Memo.Select(c => c.Clone()).ToList();
    //     }
    // }
    #endregion

    public class EarthworkBlockingConstraints
    {
        public static OverrideGraphicSettings DefaultCPSettings = new OverrideGraphicSettings();

        public static FaceRecorderForRevit GetRecorder(string segName, Document doc)
        {
            return new FaceRecorderForRevit(segName, ApplicationPath.GetCurrentPath(doc));
        }
    }
    /// <summary>
    /// 土方分块
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class EarthworkBlocking : SectionalData<EarthworkBlockingForm, EarthworkBlocking, EarthworkBlock>
    {
        #region 属性
        [JsonProperty]
        protected int Indexer = 1;
        [JsonProperty]
        public Dictionary<int, int> BlockIdIndexMapper = new Dictionary<int, int>();
        public List<EarthworkBlock> Blocks { private set; get; } = new List<EarthworkBlock>();
        public int Count()
        {
            return Blocks.Count();
        }
        [JsonProperty]
        public bool IsImplementationInfoConflicted { get; set; }
        #endregion

        #region DocRelatedInfo
        public Document Doc { set; get; }
        public View3D View3D { private set; get; }
        public void InitByDocument(Document doc)
        {
            Doc = doc;
            string viewName = "土方分块";
            View3D = DocumentHelper.GetElementByNameAs<View3D>(doc, viewName);
            if (View3D == null)
            {
                using (var transaction = new Transaction(doc, "EarthworkBlocking." + nameof(InitByDocument)))
                {
                    transaction.Start();
                    try
                    {
                        var viewFamilyType = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).ToElements()
                            .First(c => c.Name == "三维视图");
                        View3D = View3D.CreateIsometric(doc, viewFamilyType.Id);
                        View3D.Name = viewName;
                        transaction.Commit();
                        TaskDialog.Show("消息", "三维视图(土方分块)新建成功");
                    }
                    catch (Exception ex)
                    {
                        transaction.RollBack();
                        TaskDialog.Show("消息", "视图(土方分块)新建失败,错误详情:" + ex.ToString());
                    }
                }
            }
            //加载 EarthworkBlock
            FaceRecorderForRevit recorder = EarthworkBlockingConstraints.GetRecorder(nameof(EarthworkBlockingForm), doc);
            var orderedBlockIdIndexs = BlockIdIndexMapper.OrderBy(c => c.Value);
            foreach (var orderedBlockIdIndex in orderedBlockIdIndexs)
            {
                string blockStr = "";
                recorder.ReadValue(SaveKeyHelper.GetSaveKeyOfEarthworkBlock(orderedBlockIdIndex.Key), ref blockStr, recorder.GetValueAsInt(SaveKeyHelper.GetSaveKeyOfEarthworkBlockSize(orderedBlockIdIndex.Key), 1000) + 2);
                var block = JsonConvert.DeserializeObject<EarthworkBlock>(blockStr);
                if (block == null)
                    return;
                string cpSettingsStr = "";
                recorder.ReadValue(SaveKeyHelper.GetSaveKeyOfEarthworkBlockCPSettings(orderedBlockIdIndex.Key), ref cpSettingsStr, recorder.GetValueAsInt(SaveKeyHelper.GetSaveKeyOfEarthworkBlockCPSettingsSize(orderedBlockIdIndex.Key), 1000) + 2);
                var cpSettings = JsonConvert.DeserializeObject<EarthworkBlockCPSettings>(cpSettingsStr);
                if (cpSettings!=null)
                    block.CPSettings = cpSettings;
                //foreach (KeyValuePair<int,int> elementIdValue in block.ElementIdValues)
                //{
                //    var element = doc.GetElement(new ElementId(elementIdValue.Key));
                //    if (element!=null)
                //    {
                //        block.ElementIds.Add(element.Id);
                //    }
                //    else
                //    {
                //        block.info
                //    }
                //}
                var elements = block.ElementIdValues.Select(c => doc.GetElement(new ElementId(c)));
                foreach (var element in elements)
                    if (element != null)
                        block.ElementIds.Add(element.Id);
                //var implementationStr = "";
                //recorder.ReadValue(SaveKeyHelper.GetSaveKeyOfEarthworkBlockImplementationInfo(orderedBlockIdIndex.Key), ref implementationStr, int.MaxValue);
                //var implementationInfo = JsonConvert.DeserializeObject<EarthworkBlockImplementationInfo>(implementationStr);
                //if (implementationInfo != null)
                //{
                //    implementationInfo.Name = block.Name;
                //    block.ImplementationInfo = implementationInfo;
                //}
                Blocks.Add(block);
            }
        }
        #endregion

        #region Combine
        /// <summary>
        /// 合并节点
        /// </summary>
        /// <param name="index1">所选节点Index1,从0开始</param>
        /// <param name="index2">所选节点Index2,从0开始</param>
        /// <returns></returns>
        public bool Combine(EarthworkBlocking blocking, int index1, int index2)
        {
            if (index1 == index2 ||
                (index1 < 0 || index1 >= Blocks.Count) ||
                (index2 < 0 || index2 >= Blocks.Count))
            {
                return false;
            }
            //合并处理
            var b1 = Blocks[index1];
            var b2 = Blocks[index2];
            b1.Add(blocking, b2.ElementIds);
            Delete(b2);
            return true;
        }
        public bool CombineBefore(int index)
        {
            return Combine(this, index, index - 1);
        }
        public bool CombineAfter(int index)
        {
            return Combine(this, index, index + 1);
        }
        #endregion

        #region Move
        public bool Move(int indexOrient, int indexTarget)
        {
            if (indexTarget < 0 || indexTarget >= Blocks.Count())
                return false;
            if (indexOrient < 0 || indexOrient >= Blocks.Count())
                return false;
            var item = Blocks[indexOrient];
            Blocks.Remove(item);
            Blocks.Insert(indexTarget, item);
            return true;
        }
        public bool MoveStep1Foward(EarthworkBlock block)
        {
            var index = Blocks.IndexOf(block);
            return Move(index, index - 1);
        }
        public bool MoveStep1Backward(EarthworkBlock block)
        {
            var index = Blocks.IndexOf(block);
            return Move(index, index + 1);
        }
        #endregion

        public EarthworkBlock CreateNew()
        {
            var block = new EarthworkBlock(Indexer, "节点" + Indexer);
            Indexer++;
            return block;
        }
        public void SetNameForBlockingImplementationInfos()
        {
            foreach (var Block in Blocks)
            {
                Block.ImplementationInfo.Name = Block.Name;
            }
        }
        public List<EarthworkBlockImplementationInfo> GetBlockingImplementationInfos()
        {
            var result = new List<EarthworkBlockImplementationInfo>();
            foreach (var Block in Blocks)
            {
                if (Block.ImplementationInfo.Name == null)
                    Block.ImplementationInfo.Name = Block.Name;
                if (Block.ImplementationInfo.Name != Block.Name)
                {
                    Block.ImplementationInfo.Name = Block.Name;
                    if (Block.ImplementationInfo.IsSettled)
                        Block.ImplementationInfo.IsConflicted = true;
                }
                result.Add(Block.ImplementationInfo);
            }
            return result;
        }

        //用于下一页面实现
        //public bool IsExistChanging { set; get; }
        //public string ShowOnChanging()
        //{
        //    return "分段内容有变动，请修改相应工期设置";
        //}

        #region SectionalData
        /// <summary>
        /// 在选择节点后面插入(默认)
        /// </summary>
        /// <param name="index">所选节点Index,从0开始</param>
        /// <returns></returns>
        void add(int index, EarthworkBlock block)
        {
            //foreach (var IndexBlockId in BlockIdIndexMapper)
            //{
            //    if (IndexBlockId.Value >= index)
            //    {
            //        BlockIdIndexMapper[IndexBlockId.Key]++;
            //    }
            //}
            Blocks.Insert(index, block);
            Adds.Add(block);
        }
        public override void Add(EarthworkBlock element)
        {
            add(Blocks.Count(), element);
        }
        public void InsertBefore(int index, EarthworkBlock block)
        {
            add(index, block);
        }
        public void InsertAfter(int index, EarthworkBlock block)
        {
            add(index + 1, block);
        }
        //public void UpdateBlockName(int index, string name)
        //{
        //    Blocks[index].Name = name;
        //    if (Blocks[index].ImplementationInfo.IsSettled)
        //    {
        //        Blocks[index].ImplementationInfo.Unsettle();
        //    }
        //}
        public override void Commit(EarthworkBlockingForm storage)
        {
            BlockIdIndexMapper = new Dictionary<int, int>();
            int index = 0;
            foreach (var block in Blocks)
            {
                BlockIdIndexMapper.Add(block.Id,index);
                index++;
                if (block.IsChanged)
                    block.Commit(storage);
            }
            FaceRecorderForRevit recorder = EarthworkBlockingConstraints.GetRecorder(nameof(EarthworkBlockingForm), storage.m_Doc);
            var jsonObj = JsonConvert.SerializeObject(this);
            recorder.WriteValue(SaveKeyHelper.GetSaveKeyOfEarthworkBlockingSize(), jsonObj.Length.ToString());
            recorder.WriteValue(SaveKeyHelper.GetSaveKeyOfEarthworkBlocking(), jsonObj);
            foreach (var block in Deletes)
            {
                //TODO ???Block的删除或需优化
                recorder.WriteValue(SaveKeyHelper.GetSaveKeyOfEarthworkBlockSize(block.Id), "");
                recorder.WriteValue(SaveKeyHelper.GetSaveKeyOfEarthworkBlock(block.Id), "");
                recorder.WriteValue(SaveKeyHelper.GetSaveKeyOfEarthworkBlockCPSettingsSize(block.Id), "");
                recorder.WriteValue(SaveKeyHelper.GetSaveKeyOfEarthworkBlockCPSettings(block.Id), "");
            }
            Adds.Clear();
            Deletes.Clear();
        }
        public override void Delete(EarthworkBlock block)
        {
            //移除节点的所有元素(目的:解除图形配置),然后移除节点
            block.Delete(this, block.ElementIds);
            Blocks.Remove(block);
            Deletes.Add(block);
            ////TODO 清空相关的信息
            //FaceRecorderForRevit recorder = EarthworkBlockingConstraints.GetRecorder(nameof(EarthworkBlockingForm), Doc);
            //recorder.WriteValue(SaveKeyHelper.GetSaveKeyOfEarthworkBlockSize(block.Id), "");
            //recorder.WriteValue(SaveKeyHelper.GetSaveKeyOfEarthworkBlock(block.Id), "");
            //recorder.WriteValue(SaveKeyHelper.GetSaveKeyOfEarthworkBlockCPSettingsSize(block.Id), "");
            //recorder.WriteValue(SaveKeyHelper.GetSaveKeyOfEarthworkBlockCPSettings(block.Id), "");
        }
        public override void Rollback()
        {
            Blocks = new List<EarthworkBlock>();
            Blocks.AddRange(Memo.Blocks);
            BlockIdIndexMapper = Memo.BlockIdIndexMapper;
            foreach (var Block in Blocks)
            {
                Block.Rollback();
            }
            Adds.Clear();
            Deletes.Clear();
        }
        protected override EarthworkBlocking Clone()
        {
            return new EarthworkBlocking()
            {
                Blocks = Blocks,
                BlockIdIndexMapper = BlockIdIndexMapper,
            };
        }
        public override int GetSimpleHashCode()
        {
            return 0;//不做区分判断,根据用户更改行为来设置
        }

        public override void Start()
        {
            Memo = Clone();
            MemoHashCode = GetSimpleHashCode();
            foreach (var Block in Blocks)
            {
                Block.Start();
            }
        }
        #endregion
    }
}
