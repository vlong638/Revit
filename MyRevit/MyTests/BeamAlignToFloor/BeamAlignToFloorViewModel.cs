//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
//using Autodesk.Revit.UI.Selection;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;

//namespace MyRevit.MyTests.BeamAlignToFloor
//{
//    public enum CompoundStructureAnnotationViewType
//    {
//        Idle,
//        Select,
//        Close,
//    }

//    class CompoundStructureAnnotationModel
//    {
//        public ElementId ElementId { set; get; }
//    }

//    class CompoundStructureAnnotationViewModel
//    {
//        public CompoundStructureAnnotationViewModel()
//        {
//            Status = CompoundStructureAnnotationViewType.Idle;
//            Model = new CompoundStructureAnnotationModel();
//        }

//        public CompoundStructureAnnotationViewType Status { set; get; }
//        public CompoundStructureAnnotationModel Model { set; get; }




//        public void Execute(Window window, UIDocument uiDoc)
//        {
//            switch (Status)
//            {
//                case CompoundStructureAnnotationViewType.Idle:
//                    break;
//                case CompoundStructureAnnotationViewType.Select:
//                    using (PmSoft.Common.RevitClass.PickObjectsMouseHook MouseHook = new PmSoft.Common.RevitClass.PickObjectsMouseHook())
//                    {
//                        MouseHook.InstallHook(PmSoft.Common.RevitClass.PickObjectsMouseHook.OKModeENUM.Objects);
//                        try
//                        {
//                            Model.ElementId= uiDoc.Selection.PickObjects(ObjectType.Element,)

//                            Document linkDocument = null;
//                            IEnumerable<ElementId> elementIds =  uiDoc.Selection.PickObjects(ObjectType.Element, new CategoryFilter(BuiltInCategory.OST_Floors), "选择楼板").Select(c => c.ElementId);
//                            model.FloorIds = elementIds;
//                            MouseHook.UninstallHook();
//                        }
//                        catch (Exception ex)
//                        {
//                            MouseHook.UninstallHook();
//                        }
//                    }
//                    break;
//                default:
//                    break;
//            }
//        }
//    }
//}
