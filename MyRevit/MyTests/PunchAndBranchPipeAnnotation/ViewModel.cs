using System;
using Autodesk.Revit.UI;
using MyRevit.MyTests.VLBase;
using MyRevit.Utilities;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using Autodesk.Revit.DB;
using MyRevit.MyTests.PipeAttributesAnnotation;
using System.Collections.Generic;

namespace MyRevit.MyTests.PBPA
{
    public class ElementIdComparer : IEqualityComparer<ElementId>
    {
        public bool Equals(ElementId t1, ElementId t2)
        {
            return (t1.IntegerValue == t2.IntegerValue );
        }
        public int GetHashCode(ElementId t)
        {
            return t.GetHashCode();
        }
    }

    public enum PBPAViewType
    {
        Idle,//闲置
        Close,//关闭
        PickSinglePipe_Pipe,//选择单管 管道
        PickSinglePipe_Location,//选择单管 定位
        GenerateSinglePipe,//单管标注生成
        PickMultiplePipes,//选择多管
    }

    public class PBPAViewModel : VLViewModel<PBPAModel, PBPAWindow, PBPAViewType>
    {
        public PBPAViewModel(UIApplication app) : base(app)
        {
            Model = new PBPAModel("");
            View = new PBPAWindow(this);
            //用以打开时更新页面
            AnnotationType = PBPAAnnotationType.SPL;
            LocationType = PBPALocationType.Center;
        }

        public override bool IsIdling { get { return ViewType == PBPAViewType.Idle; } }
        public override void Close() { ViewType = PBPAViewType.Close; }

        public override void Execute()
        {
            switch (ViewType)
            {
                case PBPAViewType.Idle:
                    View = new PBPAWindow(this);
                    View.ShowDialog();
                    break;
                case PBPAViewType.Close:
                    View.Close();
                    break;
                case PBPAViewType.PickSinglePipe_Pipe:
                    View.Close();
                    if (!VLMouseHookHelper.DelegateMouseHook(() =>
                    {
                        //业务逻辑处理
                        //选择符合类型的过滤
                        var targetType = Model.GetFilter();
                        Model.TargetIds = new System.Collections.Generic.List<ElementId>() { UIDocument.Selection.PickObject(ObjectType.Element, targetType, "请选择管道标注点").ElementId };
                        if (Model.TargetIds.Count > 0)
                            ViewType = PBPAViewType.PickSinglePipe_Location;
                    }))
                        ViewType = PBPAViewType.Idle;
                    Execute();
                    break;
                case PBPAViewType.PickSinglePipe_Location:
                    if (!VLMouseHookHelper.DelegateMouseHook(() =>
                    {
                        ////业务逻辑处理
                        //var target = Document.GetElement(Model.TargetIds.First());
                        //var targetLocation = target.Location as LocationCurve;
                        //var p0 = targetLocation.Curve.GetEndPoint(0);
                        //var p1 = targetLocation.Curve.GetEndPoint(1);
                        //var pStart = new XYZ((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2, (p0.Z + p1.Z) / 2);
                        //var pEnd = new VLPointPicker().PickPointWithLinePreview(UIApplication, pStart);
                        //if (pEnd == null)
                        //    ViewType = PBPAViewType.Idle;
                        //else
                        //    ViewType = PBPAViewType.GenerateSinglePipe;
                    }))
                        ViewType = PBPAViewType.Idle;
                    Execute();
                    break;
                case PBPAViewType.GenerateSinglePipe:
                    if (VLTransactionHelper.DelegateTransaction(Document, "GenerateSinglePipe", (Func<bool>)(() =>
                        {
                            var element = Document.GetElement(Model.TargetIds.First());
                            var Collection = PBPAContext.GetCollection(Document);
                            //避免重复生成 由于一个对象可能在不同的视图中进行标注设置 所以还是需要重复生成的
                            var existedModels = Collection.Data.Where(c => c.TargetIds.Intersect(Model.TargetIds, new ElementIdComparer()) != null);
                            if (existedModels != null)
                            {
                                foreach (var existedModel in existedModels)
                                {
                                    Collection.Data.Remove(existedModel);
                                    PBPAContext.Creator.Clear(Document, existedModel);
                                }
                            }
                            PBPAContext.Creator.Generate(Document, Model, element);
                            Collection.Data.Add(Model);
                            Collection.Save(Document);
                            return true;
                        })))
                            ViewType = PBPAViewType.PickSinglePipe_Pipe;
                    else
                        ViewType = PBPAViewType.Idle;
                    Execute();
                    break;
                default:
                    throw new NotImplementedException("功能未实现");
            }
        }

        #region RatioButtons

        #region PBPATargetType
        PBPATargetType TargetType
        {
            get
            {
                return Model.TargetType;
            }
            set
            {
                Model.TargetType = value;
                RaisePropertyChanged("TargetType_Pipe");
                RaisePropertyChanged("TargetType_Duct");
                RaisePropertyChanged("TargetType_CableTray");
                RaisePropertyChanged("TargetType_Conduit");
            }
        }
        public bool TargetType_Pipe
        {
            get { return TargetType == PBPATargetType.Pipe; }
            set { if (value) TargetType = PBPATargetType.Pipe; }
        }
        public bool TargetType_Duct
        {
            get { return TargetType == PBPATargetType.Duct; }
            set { if (value) TargetType = PBPATargetType.Duct; }
        }
        public bool TargetType_CableTray
        {
            get { return TargetType == PBPATargetType.CableTray; }
            set { if (value) TargetType = PBPATargetType.CableTray; }
        }
        public bool TargetType_Conduit
        {
            get { return TargetType == PBPATargetType.Conduit; }
            set { if (value) TargetType = PBPATargetType.Conduit; }
        }
        #endregion

        #region PBPAAnnotationType
        PBPAAnnotationType AnnotationType
        {
            get
            {
                return Model.AnnotationType;
            }
            set
            {
                Model.AnnotationType = value;
                RaisePropertyChanged("AnnotationType_SPL");
                RaisePropertyChanged("AnnotationType_SL");
                RaisePropertyChanged("AnnotationType_PL");
                UpdateModelAnnotationPrefix();
            }
        }
        public bool AnnotationType_SPL
        {
            get { return AnnotationType == PBPAAnnotationType.SPL; }
            set { if (value) AnnotationType = PBPAAnnotationType.SPL; }
        }
        public bool AnnotationType_SL
        {
            get { return AnnotationType == PBPAAnnotationType.SL; }
            set { if (value) AnnotationType = PBPAAnnotationType.SL; }
        }
        public bool AnnotationType_PL
        {
            get { return AnnotationType == PBPAAnnotationType.PL; }
            set { if (value) AnnotationType = PBPAAnnotationType.PL; }
        }
        #endregion

        #region PBPALocationType
        PBPALocationType LocationType
        {
            get
            {
                return Model.LocationType;
            }
            set
            {
                Model.LocationType = value;
                RaisePropertyChanged("LocationType_Center");
                RaisePropertyChanged("LocationType_Top");
                RaisePropertyChanged("LocationType_Bottom");
                UpdateModelAnnotationPrefix();
            }
        }
        public bool LocationType_Center
        {
            get { return LocationType == PBPALocationType.Center; }
            set { if (value) LocationType = PBPALocationType.Center; }
        }
        public bool LocationType_Top
        {
            get { return LocationType == PBPALocationType.Top; }
            set { if (value) LocationType = PBPALocationType.Top; }
        }
        public bool LocationType_Bottom
        {
            get { return LocationType == PBPALocationType.Bottom; }
            set { if (value) LocationType = PBPALocationType.Bottom; }
        }
        #endregion

        #region PBPATextType
        PBPATextType TextType
        {
            get
            {
                return Model.TextType;
            }
            set
            {
                Model.TextType = value;
                RaisePropertyChanged("TextType_OnLine");
                RaisePropertyChanged("TextType_OnEdge");
            }
        }
        public bool TextType_OnLineOrOnLeft
        {
            get { return TextType == PBPATextType.TextType_OnLineOrOnLeft; }
            set { if (value) TextType = PBPATextType.TextType_OnLineOrOnLeft; }
        }
        public bool TextType_OnEdgeOrOnMiddle
        {
            get { return TextType == PBPATextType.TextType_OnEdgeOrOnMiddle; }
            set { if (value) TextType = PBPATextType.TextType_OnEdgeOrOnMiddle; }
        }
        #endregion

        #region Texts
        private string centerPrefix = "CL+";
        public string CenterPrefix
        {
            get
            {
                return centerPrefix;
            }

            set
            {
                centerPrefix = value;
                RaisePropertyChanged("SPLPreview");
                RaisePropertyChanged("SLPreview");
                RaisePropertyChanged("PLPreview");
            }
        }

        private void UpdateModelAnnotationPrefix()
        {
            if (Model.LocationType == PBPALocationType.Center)
                Model.AnnotationPrefix = CenterPrefix;
            else if (Model.LocationType == PBPALocationType.Top)
                Model.AnnotationPrefix = TopPrefix;
            else if (Model.LocationType == PBPALocationType.Bottom)
                Model.AnnotationPrefix = BottomPrefix;
        }

        private string topPrefix = "TL+";
        public string TopPrefix
        {
            get
            {
                return topPrefix;
            }

            set
            {
                topPrefix = value;
                RaisePropertyChanged("SPLPreview");
                RaisePropertyChanged("SLPreview");
            }
        }

        private string bottomPrefix = "BL+";
        public string BottomPrefix
        {
            get
            {
                return bottomPrefix;
            }

            set
            {
                bottomPrefix = value;
                RaisePropertyChanged("SPLPreview");
                RaisePropertyChanged("SLPreview");
            }
        }

        public string SPLPreview { get { return Model.GetPreview(); } }
        public string SLPreview { get { return Model.GetPreview(); } }
        public string PLPreview { get { return Model.GetPreview(); } }
        #endregion

        #endregion

    }
}
