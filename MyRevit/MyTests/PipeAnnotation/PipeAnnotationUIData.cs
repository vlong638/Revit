using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using PmSoft.Optimization.DrawingProduction.Utils;

namespace PmSoft.Optimization.DrawingProduction
{
    public abstract class OptimizationBaseData
    {
        public abstract void LoadConfig(Document doc);

        public abstract void SaveConfig(Document doc);
    }

    public class PipeAnnotationUIData : OptimizationBaseData
    {
        #region 运行期间临时内容
        #endregion

        #region 需记忆的内容
        public SinglePipeAnnotationSettings SettingForSingle { set; get; }
        public MultiPipeAnnotationSettings SettingForMultiple { set; get; }
        public CommonAnnotationSettings SettingForCommon { set; get; }
        #endregion

        public bool IsSuccess { set; get; }

        public override void LoadConfig(Document doc)
        {
            IsSuccess = DelegateHelper.DelegateTransaction(doc, "标注族加载", () =>
            {
                return PipeAnnotationContext.LoadFamilySymbols(doc);
            });// && PipeAnnotationContext.LoadParameterOfSymbols(doc);
            if (!IsSuccess)
            {
                TaskDialog.Show("错误", "加载必要的族时失败");
                return;
            }
            //PipeAnnotationContext.LoadParameterOfSymbols(doc);
            DelegateHelper.DelegateTransaction(doc, "加载配置记忆", () =>
            {
                SettingForSingle = new SinglePipeAnnotationSettings(this);
                SettingForMultiple = new MultiPipeAnnotationSettings(this);
                SettingForCommon = new CommonAnnotationSettings();
                var data = PipeAnnotationContext.GetSetting(doc);
                var values = data.Split(Splitter.ToCharArray().First());
                if (values.Count() >= 7)
                {
                    SettingForSingle.LengthFromLine_Milimeter = Convert.ToInt32(values[0]);
                    SettingForSingle.Location = (SinglePipeTagLocation)Enum.Parse(typeof(SinglePipeTagLocation), values[1]);
                    SettingForSingle.NeedLeader = Convert.ToBoolean(values[2]);
                    SettingForMultiple.LengthBetweenPipe_Milimeter = Convert.ToInt32(values[3]);
                    SettingForMultiple.Location = (MultiPipeTagLocation)Enum.Parse(typeof(MultiPipeTagLocation), values[4]);
                    SettingForCommon.IncludeLinkPipe = Convert.ToBoolean(values[5]);
                    SettingForCommon.AutoPreventCollision = Convert.ToBoolean(values[6]);
                    //0728长度过滤
                    if (values.Count() > 7)
                    {
                        SettingForCommon.MinLength_Milimeter = Convert.ToInt32(values[7]);
                        SettingForCommon.FilterVertical = Convert.ToBoolean(values[8]);
                    }
                }
                return true;
            });
            IsSuccess = true;
        }

        string Splitter = ",";

        public override void SaveConfig(Document doc)
        {
            DelegateHelper.DelegateTransaction(doc, "加载配置记忆", (Func<bool>)(() =>
            {
                List<string> values = new List<string>();
                values.Add((string)SettingForSingle.LengthFromLine_Milimeter.ToString());
                values.Add(SettingForSingle.Location.ToString());
                values.Add(SettingForSingle.NeedLeader.ToString());
                values.Add(SettingForMultiple.LengthBetweenPipe_Milimeter.ToString());
                values.Add(SettingForMultiple.Location.ToString());
                values.Add(SettingForCommon.IncludeLinkPipe.ToString());
                values.Add(SettingForCommon.AutoPreventCollision.ToString());
                //0728长度过滤
                values.Add(SettingForCommon.MinLength_Milimeter.ToString());
                values.Add(SettingForCommon.FilterVertical.ToString());
                PipeAnnotationContext.SaveSetting(doc, string.Join(Splitter, values));
                return true;
            }));
        }
    }
}
