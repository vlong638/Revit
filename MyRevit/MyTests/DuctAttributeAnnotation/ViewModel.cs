using System;
using Autodesk.Revit.UI;
using MyRevit.MyTests.VLBase;
using MyRevit.Utilities;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using Autodesk.Revit.DB;
using MyRevit.MyTests.PipeAttributesAnnotation;
using System.Collections.Generic;

namespace MyRevit.MyTests.DAA
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

    public enum DAAViewType
    {
        Idle,//闲置
        Close,//关闭
        PickSinglePipe_Pipe,//选择单管 管道
        PickSinglePipe_Location,//选择单管 定位
        GenerateSinglePipe,//单管标注生成
        PickMultiplePipes,//选择多管
    }

    public class DAAViewModel : VLViewModel<DAAModel, DAAWindow, DAAViewType>
    {
        public DAAViewModel(UIApplication app) : base(app)
        {
            Model = new DAAModel("");
            View = new DAAWindow(this);
        }

        public override bool IsIdling { get { return ViewType == DAAViewType.Idle; } }
        public override void Close() { ViewType = DAAViewType.Close; }

        public override void Execute()
        {
            switch (ViewType)
            {
                case DAAViewType.Idle:
                    View = new DAAWindow(this);
                    View.ShowDialog();
                    break;
                case DAAViewType.Close:
                    View.Close();
                    break;
                case DAAViewType.PickSinglePipe_Pipe:
                    View.Close();
                    if (!MouseHookHelper.DelegateMouseHook(() =>
                    {
                        //业务逻辑处理
                        //选择符合类型的过滤
                        var targetType = Model.GetFilter();
                        Model.TargetIds = new System.Collections.Generic.List<ElementId>() { UIDocument.Selection.PickObject(ObjectType.Element, targetType, "请选择管道标注点").ElementId };
                        if (Model.TargetIds.Count > 0)
                            ViewType = DAAViewType.PickSinglePipe_Location;
                    }))
                        ViewType = DAAViewType.Idle;
                    Execute();
                    break;
                case DAAViewType.PickSinglePipe_Location:
                    if (!MouseHookHelper.DelegateMouseHook(() =>
                    {
                        //业务逻辑处理
                        var target = Document.GetElement(Model.TargetIds.First());
                        var targetLocation = target.Location as LocationCurve;
                        var p0 = targetLocation.Curve.GetEndPoint(0);
                        var p1 = targetLocation.Curve.GetEndPoint(1);
                        var pStart = new XYZ((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2, (p0.Z + p1.Z) / 2);
                        var pEnd = new VLPointPicker().PickPointWithLinePreview(UIApplication, pStart);
                        if (pEnd == null)
                            ViewType = DAAViewType.Idle;
                        else
                            ViewType = DAAViewType.GenerateSinglePipe;
                    }))
                        ViewType = DAAViewType.Idle;
                    Execute();
                    break;
                case DAAViewType.GenerateSinglePipe:
                    if (TransactionHelper.DelegateTransaction(Document, "GenerateSinglePipe", (Func<bool>)(() =>
                        {
                            var element = Document.GetElement(Model.TargetIds.First());
                            var Collection = DAAContext.GetCollection(Document);
                            //避免重复生成 由于一个对象可能在不同的视图中进行标注设置 所以还是需要重复生成的
                            var existedModels = Collection.Data.Where(c => c.TargetIds.Intersect(Model.TargetIds, new ElementIdComparer()) != null);
                            if (existedModels != null)
                            {
                                foreach (var existedModel in existedModels)
                                {
                                    Collection.Data.Remove(existedModel);
                                    DAAContext.Creator.Clear(Document, existedModel);
                                }
                            }
                            DAAContext.Creator.Generate(Document, Model, element);
                            Collection.Data.Add(Model);
                            Collection.Save(Document);
                            return true;
                        })))
                            ViewType = DAAViewType.PickSinglePipe_Pipe;
                    else
                        ViewType = DAAViewType.Idle;
                    Execute();
                    break;
                default:
                    throw new NotImplementedException("功能未实现");
            }
        }

        #region RatioButtons

        #region DAATargetType
        DAATargetType TargetType
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
            get { return TargetType == DAATargetType.Pipe; }
            set { if (value) TargetType = DAATargetType.Pipe; }
        }
        public bool TargetType_Duct
        {
            get { return TargetType == DAATargetType.Duct; }
            set { if (value) TargetType = DAATargetType.Duct; }
        }
        public bool TargetType_CableTray
        {
            get { return TargetType == DAATargetType.CableTray; }
            set { if (value) TargetType = DAATargetType.CableTray; }
        }
        public bool TargetType_Conduit
        {
            get { return TargetType == DAATargetType.Conduit; }
            set { if (value) TargetType = DAATargetType.Conduit; }
        }
        #endregion

        #region DAAAnnotationType
        DAAAnnotationType AnnotationType
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
                RaisePropertyChanged("AnnotationType_SP");
            }
        }
        public bool AnnotationType_SPL
        {
            get { return AnnotationType == DAAAnnotationType.SPL; }
            set { if (value) AnnotationType = DAAAnnotationType.SPL; }
        }
        public bool AnnotationType_SL
        {
            get { return AnnotationType == DAAAnnotationType.SL; }
            set { if (value) AnnotationType = DAAAnnotationType.SL; }
        }
        public bool AnnotationType_PL
        {
            get { return AnnotationType == DAAAnnotationType.PL; }
            set { if (value) AnnotationType = DAAAnnotationType.PL; }
        }
        public bool AnnotationType_SP
        {
            get { return AnnotationType == DAAAnnotationType.SP; }
            set { if (value) AnnotationType = DAAAnnotationType.SP; }
        }
        #endregion

        #region DAALocationType
        DAALocationType LocationType
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
            }
        }
        public bool LocationType_Center
        {
            get { return LocationType == DAALocationType.Center; }
            set { if (value) LocationType = DAALocationType.Center; }
        }
        public bool LocationType_Top
        {
            get { return LocationType == DAALocationType.Top; }
            set { if (value) LocationType = DAALocationType.Top; }
        }
        public bool LocationType_Bottom
        {
            get { return LocationType == DAALocationType.Bottom; }
            set { if (value) LocationType = DAALocationType.Bottom; }
        }
        #endregion

        #region DAATextType
        DAATextType TextType
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
        public bool TextType_OnLine
        {
            get { return TextType == DAATextType.OnLine; }
            set { if (value) TextType = DAATextType.OnLine; }
        }
        public bool TextType_OnEdge
        {
            get { return TextType == DAATextType.OnEdge; }
            set { if (value) TextType = DAATextType.OnEdge; }
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
            if (Model.LocationType == DAALocationType.Center)
                Model.AnnotationPrefix = CenterPrefix;
            else if (Model.LocationType == DAALocationType.Top)
                Model.AnnotationPrefix = TopPrefix;
            else if (Model.LocationType == DAALocationType.Bottom)
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
                RaisePropertyChanged("PLPreview");
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
                RaisePropertyChanged("PLPreview");
            }
        }

        public string SPLPreview { get { return string.Format("如:SF 400mmx400mm {0}2600", CenterPrefix); } }
        public string SLPreview { get { return string.Format("如:SF {0}2600", TopPrefix); } }
        public string PLPreview { get { return string.Format("如:400mmx400mm {0}2600", TopPrefix); } }
        public string SPPreview { get { return string.Format("如:SF 400mmx400mm", TopPrefix); } }
        #endregion
        #endregion

    }
}
