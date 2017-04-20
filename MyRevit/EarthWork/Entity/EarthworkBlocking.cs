using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.EarthWork.Entity
{
    public class EarthworkBlockingConstraints
    {
        public static OverrideGraphicSettings DefaultCPSettings = new OverrideGraphicSettings();
    }
    /// <summary>
    /// 土方分块
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class EarthworkBlocking
    {

        [JsonProperty]
        protected int Indexer = 1;
        [JsonProperty]
        public List<EarthworkBlock> Blocks { private set; get; } = new List<EarthworkBlock>();
        [JsonProperty]
        public Dictionary<int, int> ElementToBlockMapper { set; get; } = new Dictionary<int, int>();

        #region DocRelatedInfo
        public View3D View3D { private set; get; }
        public void InitByDocument(Document doc)
        {
            string viewName = "土方分块";
            View3D = DocumentHelper.GetElementByNameAs<View3D>(doc, viewName);
            if (View3D == null)
            {
                using (var transaction = new Transaction(doc, "创建(土方分块)视图"))
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
        }
        #endregion

        #region Insert
        /// <summary>
        /// 在选择节点后面插入(默认)
        /// </summary>
        /// <param name="index">所选节点Index,从0开始</param>
        /// <returns></returns>
        public void Insert(int index, EarthworkBlock block)
        {
            Blocks.Insert(index, block);
        }
        public void InsertBefore(int index, EarthworkBlock block)
        {
            Insert(index, block);
        }
        public void InsertAfter(int index, EarthworkBlock block)
        {
            Insert(index + 1, block);
        }
        /// <summary>
        /// 在尾部增加节点,默认节点+(index+1)
        /// </summary>
        /// <returns></returns>
        public void Add(EarthworkBlock block)
        {
            Insert(Blocks.Count(), block);
        }
        public EarthworkBlock CreateNew()
        {
            var block = new EarthworkBlock(Indexer, "节点" + Indexer);
            Indexer++;
            return block;
        }
        /// <summary>
        /// 删除节点
        /// </summary>
        /// <returns></returns>
        public void Remove(EarthworkBlock block)
        {
            //索引处理
            foreach (var pair in ElementToBlockMapper.Where(c => c.Value == block.Id))
            {
                ElementToBlockMapper.Remove(pair.Key);
            }
            Blocks.Remove(block);
        }
        #endregion

        #region Combine
        /// <summary>
        /// 合并节点
        /// </summary>
        /// <param name="index1">所选节点Index1,从0开始</param>
        /// <param name="index2">所选节点Index2,从0开始</param>
        /// <returns></returns>
        public bool Combine(int index1, int index2)
        {
            if (index1 == index2 ||
                (index1 < 0 || index1 >= Blocks.Count) ||
                (index2 < 0 || index2 >= Blocks.Count))
            {
                return false;
            }

            var b1 = Blocks[index1];
            var b2 = Blocks[index2];
            //TODO 还需细化合并处理 如属性的合并处理?
            var newB = CreateNew();
            newB.ElementIds.AddRange(b1.ElementIds);
            newB.ElementIds.AddRange(b2.ElementIds);
            newB.ElementIdValues = newB.ElementIds.Select(c => c.IntegerValue).ToList();
            Insert(index1, newB);
            Remove(b1);
            Remove(b2);
            return true;
        }
        public bool CombineBefore(int index)
        {
            return Combine(index, index - 1);
        }
        public bool CombineAfter(int index)
        {
            return Combine(index, index + 1);
        }
        #endregion

        #region Move
        public bool Move(int indexOrient, int indexTarget)
        {
            if (indexTarget < 0)
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

        public int Count()
        {
            return Blocks.Count();
        }
        public void UpdateBlockName(int index, string name)
        {
            Blocks[index].Name = name;
            if (Blocks[index].ImplementationInfo.IsSettled)
            {
                Blocks[index].ImplementationInfo.Unsettle();
            }
        }

        //用于下一页面实现
        //public bool IsExistChanging { set; get; }
        //public string ShowOnChanging()
        //{
        //    return "分段内容有变动，请修改相应工期设置";
        //}
    }
    /// <summary>
    /// 土方分块节点
    /// </summary>

    [JsonObject(MemberSerialization.OptOut)]
    public class EarthworkBlock
    {
        public EarthworkBlock(int id, string name)
        {
            //Parent = parent;
            CPSettings = new EarthworkBlockCPSettings();
            ImplementationInfo = new EarthworkBlockImplementationInfo();

            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public EarthworkBlockCPSettings CPSettings { set; get; }
        public EarthworkBlockImplementationInfo ImplementationInfo { set; get; }
        public List<int> ElementIdValues { get; set; } = new List<int>();

        [JsonIgnore]
        public List<ElementId> ElementIds { get; set; } = new List<ElementId>();

        ////TEST
        //public List<int> ElementIds { get; set; } = new List<int>();
        ///// <summary>
        ///// 添加构建元素Id
        ///// </summary>
        ///// <param name="elementId"></param>
        //public void AddElement(int elementId)
        //{
        //    ElementIds.Add(elementId);
        //}
        ///// <summary>
        ///// 删除构建元素Id
        ///// </summary>
        ///// <param name="elementId"></param>
        ///// <returns></returns>
        //public bool RemoveElement(int elementId)
        //{
        //    return ElementIds.Remove(elementId);
        //}
        /// <summary>
        /// 添加构件元素
        /// </summary>
        /// <param name="elementId"></param>
        public void AddElement(EarthworkBlocking blocking, ElementId elementId)
        {
            //索引处理
            if (blocking.ElementToBlockMapper.ContainsKey(elementId.IntegerValue))
            {
                var i = blocking.ElementToBlockMapper.First(c => c.Key == elementId.IntegerValue);
                if (i.Value == Id)//已在本节点存在不作处理
                    return;
                else
                    blocking.Blocks.First(c => c.Id == i.Value).RemoveElementId(blocking, elementId);
            }
            blocking.ElementToBlockMapper.Add(elementId.IntegerValue, Id);
            //对象处理
            ElementIds.Add(elementId);
            ElementIdValues.Add(elementId.IntegerValue);
            CPSettings.ApplySetting(blocking.View3D, elementId);
        }
        /// <summary>
        /// 添加构件元素(批量)
        /// </summary>
        /// <param name="elementId"></param>
        public void AddElementIds(EarthworkBlocking blocking, IEnumerable<ElementId> elementIds)
        {
            if (elementIds == null)
                return;
            foreach (var elementId in elementIds)
                AddElement(blocking, elementId);
        }
        /// <summary>
        /// 删除构件元素
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        public void RemoveElementId(EarthworkBlocking blocking, ElementId elementId)
        {
            //索引处理
            if (!blocking.ElementToBlockMapper.ContainsKey(elementId.IntegerValue))//元素不存在
                return;
            var i = blocking.ElementToBlockMapper.First(c => c.Key == elementId.IntegerValue);
            if (i.Value != Id)//元素非本块
                return;
            blocking.ElementToBlockMapper.Remove(i.Key);
            //对象处理
            ElementIds.Remove(ElementIds.First(c => c == elementId));
            ElementIdValues.Remove(elementId.IntegerValue);
            CPSettings.DeapplySetting(blocking.View3D, elementId);
        }
        /// <summary>
        /// 删除构件元素(批量)
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        public void RemoveElementIds(EarthworkBlocking blocking, IEnumerable<ElementId> elementIds)
        {
            if (elementIds == null)
                return;
            foreach (var elementId in elementIds)
            {
                RemoveElementId(blocking, elementId);
            }
        }
    }
    /// <summary>
    /// 颜色/透明度配置
    /// </summary>
    public class EarthworkBlockCPSettings
    {
        public EarthworkBlockCPSettings()
        {
        }

        /// <summary>
        /// 可见
        /// </summary>
        public bool IsVisible { set; get; }
        /// <summary>
        /// 半色调
        /// </summary>
        public bool IsHalftone { set; get; }
        /// <summary>
        /// 颜色
        /// </summar>y
        public System.Drawing.Color Color { set; get; } = System.Drawing.Color.White;
        /// <summary>
        /// 填充物
        /// </summary>
        public int FillerId { set; get; }
        /// <summary>
        /// 曲面透明度
        /// </summary>
        public int SurfaceTransparency { set; get; }

        /// <summary>
        /// 克隆(用于回退)
        /// </summary>
        /// <returns></returns>
        public EarthworkBlockCPSettings Clone()
        {
            return new EarthworkBlockCPSettings()
            {
                IsVisible = IsVisible,
                IsHalftone = IsHalftone,
                Color = Color,
                FillerId = FillerId,
                SurfaceTransparency = SurfaceTransparency,
            };
        }
        /// <summary>
        /// 复制(用于回退)
        /// </summary>
        /// <param name="cpSettings"></param>
        public void Copy(EarthworkBlockCPSettings cpSettings)
        {
            this.IsVisible = cpSettings.IsVisible;
            this.IsHalftone = cpSettings.IsHalftone;
            this.Color = cpSettings.Color;
            this.FillerId = cpSettings.FillerId;
            this.SurfaceTransparency = cpSettings.SurfaceTransparency;
        }

        /// <summary>
        /// 对元素增加节点的配置
        /// </summary>
        /// <param name="element"></param>
        public void ApplySetting(View view, ElementId elementId, OverrideGraphicSettings setting)
        {
            view.SetElementOverrides(elementId, setting);
            TaskDialog.Show("INFO", $"elementId:{elementId.IntegerValue}颜色/透明度设置已更新");
        }
        public void ApplySetting(View view, ElementId elementId)
        {
            OverrideGraphicSettings setting = GetOverrideGraphicSettings();
            ApplySetting(view, elementId, setting);
        }
        public void ApplySetting(View view, List<ElementId> elementIds)
        {
            OverrideGraphicSettings setting = GetOverrideGraphicSettings();
            foreach (var elementId in elementIds)
                ApplySetting(view, elementId, setting);
        }
        OverrideGraphicSettings GetOverrideGraphicSettings()
        {
            var setting = new OverrideGraphicSettings();
            setting.SetCutFillPatternVisible(IsVisible);
            setting.SetHalftone(IsHalftone);
            setting.SetCutFillColor(new Color(Color.R, Color.G, Color.B));
            setting.SetCutFillPatternId(new ElementId(FillerId));
            setting.SetSurfaceTransparency(SurfaceTransparency);
            return setting;
        }

        /// <summary>
        /// 解除对元素增加的节点的配置
        /// </summary>
        /// <param name="element"></param>
        public void DeapplySetting(View view, ElementId elementId)
        {
            view.SetElementOverrides(elementId, EarthworkBlockingConstraints.DefaultCPSettings);
        }
        /// <summary>
        /// 解除对元素增加的节点的配置
        /// </summary>
        /// <param name="element"></param>
        public void DeapplySetting(View view, List<ElementId> elementIds)
        {
            foreach (var elementId in elementIds)
            {
                DeapplySetting(view, elementId);
            }
        }
    }
    /// <summary>
    /// 节点施工信息
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class EarthworkBlockImplementationInfo
    {
        public EarthworkBlockImplementationInfo()
        {
        }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { set; get; }
        /// <summary>
        /// 无支撑暴露时间
        /// </summary>
        public int ExposureTime { set; get; }
        /// <summary>
        /// 完成事件
        /// </summary>
        public DateTime EndTime { set; get; }
        /// <summary>
        /// 已被完成
        /// </summary>
        [JsonIgnore]
        public bool IsSettled
        {
            get
            {
                return StartTime != DateTime.MinValue
              && EndTime != DateTime.MinValue
              && ExposureTime != 0;
            }
        }
        public System.Drawing.Color ColorForUnsettled { set; get; }
        public System.Drawing.Color ColorForSettled { set; get; }
        /// <summary>
        /// 已完成的节点更改节点名称后
        /// </summary>
        public void Unsettle()
        {
            //TODO 节点名称变更后
            //当前面土方分块节点重命名或有增减后，用户再次打开此界面则提示“分段内容有变动，请修改相应工期设置”
            throw new NotImplementedException();
        }
        public void Preview()
        {
            //TODO 预览
            throw new NotImplementedException();
        }
    }
}
