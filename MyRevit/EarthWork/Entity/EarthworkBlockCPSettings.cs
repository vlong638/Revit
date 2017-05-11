using Autodesk.Revit.DB;
using MyRevit.EarthWork.UI;
using MyRevit.Utilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyRevit.EarthWork.Entity
{
    /// <summary>
    /// 颜色/透明度配置
    /// </summary>
    public class EarthworkBlockCPSettings : MemorableData<EarthworkBlockingForm, EarthworkBlockCPSettings>
    {
        public EarthworkBlockCPSettings()
        {
        }

        #region 属性
        /// <summary>
        /// 图元可见
        /// </summary>
        public bool IsVisible { set; get; } = true;
        /// <summary>
        /// 半色调
        /// </summary>
        public bool IsHalftone { set; get; } = false;
        /// <summary>
        /// 表面可见
        /// </summary>
        public bool IsSurfaceVisible { set; get; } = true;
        /// <summary>
        /// 颜色 Surface即Projection
        /// </summar>y
        public System.Drawing.Color Color { set; get; } = System.Drawing.Color.White;
        /// <summary>
        /// 填充物 Surface即Projection
        /// </summary>
        public int FillerId { set; get; } = -1;
        /// <summary>
        /// 曲面透明度
        /// </summary>
        public int SurfaceTransparency { set; get; }
        #endregion

        #region MemorableData
        public override void Preview(EarthworkBlockingForm storage)
        {
            //更新视图内容
            ApplySetting(storage.Blocking, storage.Block.ElementIds);
        }
        public override void Commit(EarthworkBlockingForm storage)
        {
            //更新视图内容
            ApplySetting(storage.Blocking, storage.Block.ElementIds);
            //保存数据
            PmSoft.Common.CommonClass.FaceRecorderForRevit recorder = EarthworkBlockingConstraints.GetRecorder(nameof(EarthworkBlockingForm), storage.m_Doc);
            var jsonObj = JsonConvert.SerializeObject(this);
            recorder.WriteValue(SaveKeyHelper.GetSaveKey(SaveKeyHelper.SaveKeyTypeForEarthWork.EarthworkBlockCPSettings_Size, storage.Block.Id), jsonObj.Length.ToString());
            recorder.WriteValue(SaveKeyHelper.GetSaveKey(SaveKeyHelper.SaveKeyTypeForEarthWork.EarthworkBlockCPSettings, storage.Block.Id), jsonObj);
        }
        public override void Rollback()
        {
            IsVisible = Memo.IsVisible;
            IsSurfaceVisible = Memo.IsSurfaceVisible;
            IsHalftone = Memo.IsHalftone;
            Color = Memo.Color;
            FillerId = Memo.FillerId;
            SurfaceTransparency = Memo.SurfaceTransparency;
        }
        protected override EarthworkBlockCPSettings Clone()
        {
            return new EarthworkBlockCPSettings()
            {
                IsVisible = IsVisible,
                IsSurfaceVisible = IsSurfaceVisible,
                IsHalftone = IsHalftone,
                Color = Color,
                FillerId = FillerId,
                SurfaceTransparency = SurfaceTransparency,
            };
        }
        public override int GetSimpleHashCode()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(IsVisible);
            sb.Append(IsHalftone);
            sb.Append(IsSurfaceVisible);
            sb.Append(Color.R);
            sb.Append(Color.G);
            sb.Append(Color.B);
            sb.Append(FillerId);
            sb.Append(SurfaceTransparency);
            return sb.ToString().GetHashCode();
        }
        #endregion

        #region 行为
        /// <summary>
        /// 对元素增加节点的配置
        /// </summary>
        /// <param name="element"></param>
        void ApplySetting(View view, ElementId elementId, OverrideGraphicSettings setting)
        {
            view.SetElementOverrides(elementId, setting);
            //TaskDialog.Show("INFO", $"elementId:{elementId.IntegerValue}颜色/透明度设置已更新");
        }
        void ApplySetting(View view, ElementId elementId)
        {
            OverrideGraphicSettings setting = GetOverrideGraphicSettings(view.Document);
            ApplySetting(view, elementId, setting);
        }
        public void ApplySetting(EarthworkBlocking blocking, List<ElementId> elementIds)
        {
            if (elementIds == null || elementIds.Count == 0)
                return;
            using (var transaction = new Transaction(blocking.Doc, "EarthworkBlocking." + nameof(ApplySetting)))
            {
                OverrideGraphicSettings setting = GetOverrideGraphicSettings(blocking.Doc);
                transaction.Start();
                //元素可见性
                if (IsVisible)
                    blocking.View3D.UnhideElements(elementIds);
                else
                    blocking.View3D.HideElements(elementIds);
                //元素表面填充物配置
                foreach (var elementId in elementIds)
                    ApplySetting(blocking.View3D, elementId, setting);
                transaction.Commit();
            }
        }
        public static ElementId _DefaultFillPatternId = null;
        public static ElementId GetDefaultFillPatternId(Document doc)
        {
            if (_DefaultFillPatternId != null)
                return _DefaultFillPatternId;

            _DefaultFillPatternId = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).ToElements().First(c => c.Name == "实体填充").Id;
            return _DefaultFillPatternId;
        }
        OverrideGraphicSettings GetOverrideGraphicSettings(Document doc)
        {
            var setting = new OverrideGraphicSettings();
            setting.SetHalftone(IsHalftone);
            setting.SetProjectionFillPatternVisible(IsSurfaceVisible);
            setting.SetProjectionFillColor(new Color(Color.R, Color.G, Color.B));
            if (FillerId == -1)
                setting.SetProjectionFillPatternId(GetDefaultFillPatternId(doc));
            else
                setting.SetProjectionFillPatternId(new ElementId(FillerId));
            setting.SetSurfaceTransparency(SurfaceTransparency);
            return setting;
        }
        /// <summary>
        /// 解除对元素增加的节点的配置
        /// </summary>
        /// <param name="element"></param>
        void DeapplySetting(View view, ElementId elementId)
        {
            view.SetElementOverrides(elementId, EarthworkBlockingConstraints.DefaultCPSettings);
        }
        /// <summary>
        /// 解除对元素增加的节点的配置
        /// </summary>
        /// <param name="element"></param>
        public void DeapplySetting(EarthworkBlocking blocking, List<ElementId> elementIds)
        {
            using (var transaction = new Transaction(blocking.Doc, "EarthworkBlocking." + nameof(DeapplySetting)))
            {
                OverrideGraphicSettings setting = GetOverrideGraphicSettings(blocking.Doc);
                transaction.Start();
                //元素可见性
                blocking.View3D.UnhideElements(elementIds);
                //元素表面填充物配置
                foreach (var elementId in elementIds)
                    DeapplySetting(blocking.View3D, elementId);
                transaction.Commit();
            }
        }
        #endregion
    }
}
