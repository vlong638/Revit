using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.SubsidenceMonitor.UI;
using PmSoft.Common.Controls.RevitMethod;
using PmSoft.Common.RevitClass;
using PmSoft.MainModel.EntData;
using System.Diagnostics;
using System.Windows.Forms;

namespace MyRevit.SubsidenceMonitor.Command
{

    namespace MyRevit.SubsidenceMonitor.Command
    {
        [Transaction(TransactionMode.Manual)]
        class SubsidenceMonitorCommand : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                return new SubsidenceMonitorExe().Execute(commandData, ref message, elements);
            }
        }
        public class SubsidenceMonitorExe
        {
            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                if (!PmSoft.Common.MFCDll.CRevitModelCheck.Instance().HasModel())
                {
                    return Result.Cancelled;
                }
                SubsidenceMonitorSet set = new SubsidenceMonitorSet(commandData);
                if (!set.DoCmd())
                {
                    return Result.Failed;
                }
                return Result.Succeeded;
            }
        }
        public class SubsidenceMonitorSet : CToolCmd
        {
            public SubsidenceMonitorSet(ExternalCommandData revit) : base(revit)
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
            ListForm Form;
            protected override bool DoUI()
            {
                Form = new ListForm(this.m_app.ActiveUIDocument.Document);
                PickObjectsMouseHook mouseHook = new PickObjectsMouseHook();
                DialogResult result = DialogResult.Retry;
                while ((result = Form.ShowDialog(new RevitHandle(Process.GetCurrentProcess().MainWindowHandle))) == DialogResult.Retry)
                {
                    if (result == DialogResult.Cancel)
                        return false;
                    if (result == DialogResult.OK)
                        return true;
                    //if (Form.ShowDialogType == ShowDialogType.AddElements || Form.ShowDialogType == ShowDialogType.DeleleElements)
                    //{
                    //    try
                    //    {
                    //        mouseHook.InstallHook();
                    //        Form.SelectedElementIds = m_uiDoc.Selection.PickObjects(ObjectType.Element, "选择要添加的构件")
                    //            .Select(p => m_doc.GetElement(p.ElementId).Id).ToList();
                    //        mouseHook.UninstallHook();
                    //    }
                    //    catch
                    //    {
                    //        mouseHook.UninstallHook();
                    //    }
                    //    Form.FinishElementSelection();
                    //}
                    //if (Form.ShowDialogType == ShowDialogType.ViewGT6 || Form.ShowDialogType == ShowDialogType.ViewCompletion)
                    //{
                    //    try
                    //    {
                    //        mouseHook.InstallHook();
                    //        m_uiDoc.Selection.PickObjects(ObjectType.Element, "");
                    //        mouseHook.UninstallHook();
                    //    }
                    //    catch
                    //    {
                    //        mouseHook.UninstallHook();
                    //    }
                    //}
                }
                return true;
            }
        }
    }
}
