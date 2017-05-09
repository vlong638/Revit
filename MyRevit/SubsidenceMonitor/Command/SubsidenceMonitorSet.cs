using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.SubsidenceMonitor.UI;
using PmSoft.Common.Controls.RevitMethod;
using PmSoft.Common.RevitClass;
using PmSoft.MainModel.EntData;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace MyRevit.SubsidenceMonitor.Command
{
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
            Form = new ListForm(this.m_app.ActiveUIDocument);
            PickObjectsMouseHook mouseHook = new PickObjectsMouseHook();
            DialogResult result = DialogResult.Retry;
            while ((result = Form.ShowDialog(new RevitHandle(Process.GetCurrentProcess().MainWindowHandle))) == DialogResult.Retry)
            {
                switch (Form.ShowDialogType)
                {
                    case ShowDialogType.AddElements_ForDetail:
                    case ShowDialogType.DeleleElements_ForDetail:
                        try
                        {
                            mouseHook.InstallHook();
                            Form.SubForm.SelectedElementIds = m_uiDoc.Selection.PickObjects(ObjectType.Element, "选择要添加的构件")
                                .Select(p => m_doc.GetElement(p.ElementId).Id).ToList();
                            mouseHook.UninstallHook();
                        }
                        catch
                        {
                            mouseHook.UninstallHook();
                        }
                        Form.SubForm.FinishElementSelection();
                        break;
                    case ShowDialogType.ViewElementsBySelectedNodes:
                    case ShowDialogType.ViewElementsByAllNodes:
                    case ShowDialogType.ViewCurrentMaxByRed:
                    case ShowDialogType.ViewCurrentMaxByAll:
                    case ShowDialogType.ViewTotalMaxByRed:
                    case ShowDialogType.ViewTotalMaxByAll:
                    case ShowDialogType.ViewCloseWarn:
                    case ShowDialogType.ViewOverWarn:
                        mouseHook.InstallHook();
                        m_uiDoc.Selection.PickObjects(ObjectType.Element, "");
                        mouseHook.UninstallHook();
                        break;
                }
            }
            return true;
        }
    }
}
