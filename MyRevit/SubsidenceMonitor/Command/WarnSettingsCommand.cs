using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.SubsidenceMonitor.UI;
using PmSoft.Common.CommonClass;
using PmSoft.MainModel.EntData;
using System;

namespace MyRevit.SubsidenceMonitor.Command
{
    [Transaction(TransactionMode.Manual)]
    public class WarnSettingsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return new WarnSettingsExe().Execute(commandData, ref message, elements);
        }
    }
    public class WarnSettingsExe
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (!PmSoft.Common.MFCDll.CRevitModelCheck.Instance().HasModel())
            {
                return Result.Cancelled;
            }
            WarnSettingsSet set = new WarnSettingsSet(commandData);
            if (!set.DoCmd())
            {
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
    public class WarnSettingsSet : CToolCmd
    {
        public WarnSettingsSet(ExternalCommandData revit) : base(revit)
        {
        }

        protected override bool Analyse()
        {
            //监测并创建数据表
            Utilities.SQLiteHelper.CheckAndCreateTables();
            return true;
        }
        protected override void Reset()
        {
        }
        protected override bool DoUI()
        {
            try
            {
                if (ConfigPathManager.IniProjectDB(this.m_doc))
                {
                    new WarnSettingsForm(this.m_doc).ShowDialog();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误", ex.Message);
                return false;
            }
        }
    }
}
