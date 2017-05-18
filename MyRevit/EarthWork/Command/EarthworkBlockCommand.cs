using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using PmSoft.MainModel.EntData;
using MyRevit.EarthWork.UI;
using PmSoft.Common.RevitClass;
using System.Windows.Forms;
using PmSoft.Common.Controls.RevitMethod;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.UI.Selection;

namespace MyRevit.Commands
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

        EarthworkBlockingForm Form;
        protected override bool DoUI()
        {
            Form = new EarthworkBlockingForm(this.m_app);
            PickObjectsMouseHook mouseHook = new PickObjectsMouseHook();
            DialogResult result = DialogResult.Retry;
            while ((result = Form.ShowDialog(new RevitHandle(Process.GetCurrentProcess().MainWindowHandle))) == DialogResult.Retry)
            {
                if (result == DialogResult.Cancel)
                    return false;
                if (result == DialogResult.OK)
                    return true;
                if (Form.ShowDialogType==ShowDialogType.AddElements|| Form.ShowDialogType == ShowDialogType.DeleleElements)
                {
                    try
                    {
                        mouseHook.InstallHook();
                        Form.SelectedElementIds = m_uiDoc.Selection.PickObjects(ObjectType.Element, "选择要添加的构件")
                            .Select(p => m_doc.GetElement(p.ElementId).Id).ToList();
                        mouseHook.UninstallHook();
                    }
                    catch
                    {
                        mouseHook.UninstallHook();
                    }
                    Form.FinishElementSelection();
                }
                if (Form.ShowDialogType==ShowDialogType.ViewGT6|| Form.ShowDialogType == ShowDialogType.ViewCompletion)
                {
                    try
                    {
                        mouseHook.InstallHook();
                        m_uiDoc.Selection.PickObjects(ObjectType.Element, "");
                        mouseHook.UninstallHook();
                    }
                    catch
                    {
                        mouseHook.UninstallHook();
                    }
                }
            }
            return true;

            #region 非模态
            //try
            //{
            //    if (ConfigPathManager.IniProjectDB(this.m_doc))
            //    {
            //        Form = new EarthworkBlockingForm(this.m_app);
            //        Form.ShowDialog();
            //        return true;
            //    }
            //    else
            //        return false;
            //}
            //catch(Exception ex)
            //{
            //    TaskDialog.Show("错误", ex.Message);
            //    return false;
            //} 
            #endregion
        }
    }
}
