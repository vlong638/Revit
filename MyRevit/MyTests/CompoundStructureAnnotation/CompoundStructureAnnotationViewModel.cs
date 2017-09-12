using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.MyTests.Utilities;
using MyRevit.Utilities;
using PmSoft.Optimization.DrawingProduction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    public enum CompoundStructureAnnotationViewType
    {
        Idle,
        Select,
        Generate,
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

        public void Execute(CompoundStructureAnnotationWindow window, CompoundStructureAnnotationSet set, UIDocument uiDoc)
        {
            switch (ViewType)
            {
                case CompoundStructureAnnotationViewType.Idle:
                    window = new CompoundStructureAnnotationWindow(set);
                    IntPtr rvtPtr = Autodesk.Windows.ComponentManager.ApplicationWindow;
                    WindowInteropHelper helper = new WindowInteropHelper(window);
                    helper.Owner = rvtPtr;
                    window.ShowDialog();
                    break;
                case CompoundStructureAnnotationViewType.Select:
                    if (window.IsActive)
                        window.Close();
                    using (PmSoft.Common.RevitClass.PickObjectsMouseHook MouseHook = new PmSoft.Common.RevitClass.PickObjectsMouseHook())
                    {
                        MouseHook.InstallHook(PmSoft.Common.RevitClass.PickObjectsMouseHook.OKModeENUM.Objects);
                        try
                        {
                            Model.ElementId = uiDoc.Selection.PickObject(ObjectType.Element, new ClassFilter(typeof(Wall))).ElementId;
                            MouseHook.UninstallHook();
                            ViewType = CompoundStructureAnnotationViewType.Generate;
                        }
                        catch (Exception ex)
                        {
                            MouseHook.UninstallHook();
                            ViewType = CompoundStructureAnnotationViewType.Idle;
                        }
                    }
                    break;
                case CompoundStructureAnnotationViewType.Generate:
                    var doc = uiDoc.Document;
                    if (TransactionHelper.DelegateTransaction(doc, "Generate CompoundStructureAnnotation", () =>
                    {
                        var element = doc.GetElement(Model.ElementId);
                        CompoundStructure compoundStructure = null;
                        if (element is Wall)
                        {
                            compoundStructure = (element as Wall).WallType.GetCompoundStructure();
                        }
                        if (element is FloorType)
                        {
                            compoundStructure = (element as FloorType).GetCompoundStructure();
                        }
                        if (element is ExtrusionRoof)//屋顶有多种类型
                        {
                            compoundStructure = (element as ExtrusionRoof).RoofType.GetCompoundStructure();
                        }
                        if (compoundStructure == null)
                            return false;
                        var layers = compoundStructure.GetLayers();
                        List<string> texts = new List<string>();
                        foreach (var layer in layers)
                        {
                            if (layer.MaterialId.IntegerValue < 0)
                                continue;
                            var material = doc.GetElement(layer.MaterialId);
                            if (material == null)
                                continue;
                            texts.Add(layer.Width + doc.GetElement(layer.MaterialId).Name);
                        }
                        var locationCurve = element.Location as LocationCurve;
                        var location = (locationCurve.Curve.GetEndPoint(0) + locationCurve.Curve.GetEndPoint(1)) / 2;
                        var textNodeTypeId = new ElementId(84244);
                        TextNote.Create(doc, uiDoc.ActiveView.Id, location, texts.First(), textNodeTypeId);
                        return true;
                    }))
                        ViewType = CompoundStructureAnnotationViewType.Select;
                    else
                        ViewType = CompoundStructureAnnotationViewType.Idle;
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
