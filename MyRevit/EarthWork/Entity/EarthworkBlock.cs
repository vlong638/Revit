using Autodesk.Revit.DB;
using MyRevit.EarthWork.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyRevit.EarthWork.Entity
{
    /// <summary>
    /// 土方分块节点
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class EarthworkBlock : SectionalData<EarthworkBlockingForm, EarthworkBlocking, EarthworkBlock, ElementId>
    {
        public EarthworkBlock(int id, string name)
        {
            //Parent = parent;
            CPSettings = new EarthworkBlockCPSettings();
            ImplementationInfo = new EarthworkBlockImplementationInfo();

            Id = id;
            Name = name;
        }

        #region 属性
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [JsonIgnore]//TODO InitByDocument()中根据Document初始化
        public EarthworkBlockCPSettings CPSettings { set; get; }
        public EarthworkBlockImplementationInfo ImplementationInfo { set; get; }
        public List<int> ElementIdValues { get; set; } = new List<int>();
        [JsonIgnore]//InitByDocument()中根据Document初始化
        public List<ElementId> ElementIds { get; set; } = new List<ElementId>();
        #endregion

        #region SectionalData
        protected override EarthworkBlock Clone()
        {
            var block = new EarthworkBlock(Id, Name);
            block.ElementIds.AddRange(ElementIds);
            block.ElementIdValues.AddRange(ElementIdValues);
            return block;
        }
        public override int GetSimpleHashCode()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Id);
            sb.Append(Name);
            sb.Append("Adds:" + string.Join(",", Adds.Select(c => c.IntegerValue)));
            sb.Append("Deletes:" + string.Join(",", Deletes.Select(c => c.IntegerValue)));
            return sb.ToString().GetHashCode();
        }
        public override void Commit(EarthworkBlockingForm storage)
        {
            PmSoft.Common.CommonClass.FaceRecorderForRevit recorder = new PmSoft.Common.CommonClass.FaceRecorderForRevit(nameof(EarthworkBlockingForm) + storage.m_Doc.Title
                , PmSoft.Common.CommonClass.ApplicationPath.GetCurrentPath(storage.m_Doc));
            recorder.WriteValue(SaveKeyHelper.GetSaveKeyOfEarthworkBlock(Id), JsonConvert.SerializeObject(this));
        }
        public override void Rollback()
        {
            Name = Memo.Name;
            Adds.Clear();
            Deletes.Clear();
        }
        public override void Add(EarthworkBlocking blocking, ElementId elementId)
        {
            var block = blocking.Blocks.FirstOrDefault(c => c.ElementIds.Exists(p => p.IntegerValue == elementId.IntegerValue));
            if (block != null)
            {
                if (block.Id == Id)
                    return;

                block.Delete(blocking, new List<ElementId>() { elementId });
            }
            ElementIds.Add(elementId);
            ElementIdValues.Add(elementId.IntegerValue);
            Adds.Add(elementId);
            CPSettings.ApplySetting(blocking, new List<ElementId>() { elementId });
        }
        public override void Delete(EarthworkBlocking blocking, ElementId elementId)
        {
            if (!ElementIds.Exists(p => p.IntegerValue == elementId.IntegerValue))
                return;
            ElementIds.Remove(elementId);
            ElementIdValues.Remove(elementId.IntegerValue);
            Deletes.Add(elementId);
            CPSettings.DeapplySetting(blocking, new List<ElementId>() { elementId });
        }
        #endregion
    }
}
