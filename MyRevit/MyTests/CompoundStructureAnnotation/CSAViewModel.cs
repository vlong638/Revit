using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.MyTests.Utilities;
using MyRevit.MyTests.VLBase;
using MyRevit.Utilities;
using PmSoft.Optimization.DrawingProduction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Interop;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    /// <summary>
    /// CompoundStructureAnnotation ViewModel
    /// </summary>
    public class CSAViewModel : VLViewModel<CSAModel, CompoundStructureAnnotationWindow>
    {
        public CSAViewModel(UIApplication app) : base(app)
        {
            Model = new CSAModel();
            LocationType = CSALocationType.OnEdge;
        }

        #region 绑定用属性 需在ViewModel中初始化
        CSALocationType LocationType
        {
            get
            {
                return Model.CSALocationType;
            }
            set
            {
                Model.CSALocationType = value;
                RaisePropertyChanged("IsDocument");
                RaisePropertyChanged("IsLinkDocument");
            }
        }
        public bool IsOnLine
        {
            get { return LocationType == CSALocationType.OnLine; }
            set { if (value) LocationType = CSALocationType.OnLine; }
        }
        public bool IsOnEdge
        {
            get { return LocationType == CSALocationType.OnEdge; }
            set { if (value) LocationType = CSALocationType.OnEdge; }
        }

        public List<TextNoteType> TextNoteTypes { get { return Model.GetTextNoteTypes(); } }
        public ElementId TextNoteTypeElementId
        {
            get
            {
                if (Model.TextNoteTypeElementId == null)
                {
                    Model.TextNoteTypeElementId = TextNoteTypes.FirstOrDefault().Id;
                }
                return Model.TextNoteTypeElementId;
            }
            set
            {
                Model.TextNoteTypeElementId = value;
                this.RaisePropertyChanged("Type");
            }
        }
        #endregion

        public override void Execute()
        {
            var viewType = (CSAViewType)Enum.Parse(typeof(CSAViewType), ViewType.ToString());
            switch (viewType)
            {
                case CSAViewType.Idle:
                    View = new CompoundStructureAnnotationWindow(this);
                    IntPtr rvtPtr = Autodesk.Windows.ComponentManager.ApplicationWindow;
                    WindowInteropHelper helper = new WindowInteropHelper(View);
                    helper.Owner = rvtPtr;
                    View.ShowDialog();
                    break;
                case CSAViewType.Select:
                    if (View.IsActive)
                        View.Close();
                    using (PmSoft.Common.RevitClass.PickObjectsMouseHook MouseHook = new PmSoft.Common.RevitClass.PickObjectsMouseHook())
                    {
                        MouseHook.InstallHook(PmSoft.Common.RevitClass.PickObjectsMouseHook.OKModeENUM.Objects);
                        try
                        {
                            Model.TargetId = UIDocument.Selection.PickObject(ObjectType.Element
                                , new VLClassesFilter(false, typeof(Wall), typeof(Floor), typeof(ExtrusionRoof), typeof(FootPrintRoof))).ElementId;
                            MouseHook.UninstallHook();
                            ViewType = (int)CSAViewType.Generate;
                        }
                        catch (Exception ex)
                        {
                            MouseHook.UninstallHook();
                            ViewType = (int)CSAViewType.Idle;
                        }
                    }
                    break;
                case CSAViewType.Generate:
                    var doc = UIDocument.Document;
                    if (VLTransactionHelper.DelegateTransaction(doc, "生成结构标注", () =>
                    {
                        var element = doc.GetElement(Model.TargetId);
                        var Collection = CSAContext.GetCollection(doc);
                        //避免重复生成 由于一个对象可能在不同的视图中进行标注设置 所以还是需要重复生成的
                        var existedModel = Collection.Data.FirstOrDefault(c => c.TargetId.IntegerValue == Model.TargetId.IntegerValue);
                        if (existedModel != null)
                        {
                            Collection.Data.Remove(existedModel);
                            CSAContext.Creator.Clear(doc, existedModel);
                        }
                        var fontScale = 1 / VLConstraintsForCSA.OrientFontSizeScale * Model.CurrentFontSizeScale;
                        Model.LineWidth = UnitHelper.ConvertToFoot(Model.CSALocationType.GetLineWidth() * fontScale, VLUnitType.millimeter);
                        CSAContext.Creator.Generate(doc, Model, element);
                        Collection.Data.Add(Model);
                        Collection.Save(doc);
                        return true;
                    }))
                        ViewType = (int)CSAViewType.Select;
                    else
                        ViewType = (int)CSAViewType.Idle;
                    break;
                case CSAViewType.Close:
                    View.Close();
                    break;
                case CSAViewType.Closing:
                default:
                    break;
            }
        }
    }
}
