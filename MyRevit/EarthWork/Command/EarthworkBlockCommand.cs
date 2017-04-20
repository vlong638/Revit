using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using PmSoft.MainModel.EntData;
using MyRevit.EarthWork.UI;
using PmSoft.Common.CommonClass;
using PmSoft.Common.RevitClass;
using System.Windows.Forms;
using PmSoft.Common.Controls.RevitMethod;
using System.Diagnostics;
using System.Linq;
using System;

namespace MyRevit.EarthWork.Command
{
    [Transaction(TransactionMode.Manual)]
    public class EarthworkBlockingCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return new EarthworkBlockingExe().Execute(commandData, ref message, elements);
        }
    }
    public class EarthworkBlockingExe
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (!PmSoft.Common.MFCDll.CRevitModelCheck.Instance().HasModel())
            {
                return Result.Cancelled;
            }
            EarthworkBlockingSet set = new EarthworkBlockingSet(commandData);
            if (!set.DoCmd())
            {
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
    public class EarthworkBlockingSet : CToolCmd
    {
        public EarthworkBlockingSet(ExternalCommandData revit) : base(revit)
        {
        }

        protected override bool Analyse()
        {
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
                    EarthworkBlockingForm form = new EarthworkBlockingForm(this.m_app);
                    form.Show();
                    return true;
                }
                else
                    return false;
            }
            catch(Exception ex)
            {
                TaskDialog.Show("错误", ex.Message);
                return false;
            }
        }
    }
}
