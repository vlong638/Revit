using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.MyTests.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    public enum CompoundStructureAnnotationViewType
    {
        Idle,
        Select,
        Close,
    }

    public class CompoundStructureAnnotationModel
    {
        public ElementId ElementId { set; get; }
    }

    public class CompoundStructureAnnotationViewModel
    {
        public CompoundStructureAnnotationViewModel()
        {
            ViewType = CompoundStructureAnnotationViewType.Idle;
            Model = new CompoundStructureAnnotationModel();
        }

        public CompoundStructureAnnotationViewType ViewType { set; get; }
        public CompoundStructureAnnotationModel Model { set; get; }

        public void Execute(Window window, UIDocument uiDoc)
        {
            switch (ViewType)
            {
                case CompoundStructureAnnotationViewType.Idle:
                    break;
                case CompoundStructureAnnotationViewType.Select:
                    using (PmSoft.Common.RevitClass.PickObjectsMouseHook MouseHook = new PmSoft.Common.RevitClass.PickObjectsMouseHook())
                    {
                        MouseHook.InstallHook(PmSoft.Common.RevitClass.PickObjectsMouseHook.OKModeENUM.Objects);
                        try
                        {
                            Model.ElementId = uiDoc.Selection.PickObject(ObjectType.Element, new ClassFilter(typeof(WallType))).ElementId;
                            MouseHook.UninstallHook();
                        }
                        catch (Exception ex)
                        {
                            ViewType = CompoundStructureAnnotationViewType.Idle;
                            MouseHook.UninstallHook();
                        }
                    }
                    break;
                case CompoundStructureAnnotationViewType.Close:
                    window.Close();
                    break;
                default:
                    break;
            }
        }
    }
}
