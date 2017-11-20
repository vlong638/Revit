using System;
using Autodesk.Revit.UI;
using MyRevit.MyTests.VLBase;
using MyRevit.Utilities;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using Autodesk.Revit.DB;
using MyRevit.MyTests.PipeAttributesAnnotation;
using System.Collections.Generic;

namespace MyRevit.MyTests.PAA
{
    public class ElementIdComparer : IEqualityComparer<ElementId>
    {
        public bool Equals(ElementId t1, ElementId t2)
        {
            return (t1.IntegerValue == t2.IntegerValue);
        }
        public int GetHashCode(ElementId t)
        {
            return t.GetHashCode();
        }
    }

    public enum PAAViewType
    {
        Idle,//闲置
        Close,//关闭
        PickSinglePipe_Pipe,//选择单管 管道
        PickSinglePipe_Location,//选择单管 定位
        GenerateSinglePipe,//单管标注生成
        PickMultiplePipes,//选择多管
    }

    public class PAAViewModel : VLViewModel<PAAModelForSingle, PAAWindow, PAAViewType>
    {
        public PAAViewModel(UIApplication app) : base(app)
        {
            Model = new PAAModelForSingle("");
            View = new PAAWindow(this);
        }

        public override bool IsIdling { get { return ViewType == PAAViewType.Idle; } }
        public override void Close() { ViewType = PAAViewType.Close; }

        public override void Execute()
        {
            switch (ViewType)
            {
                case PAAViewType.Idle:
                    View = new PAAWindow(this);
                    View.ShowDialog();
                    break;
                case PAAViewType.Close:
                    View.Close();
                    break;
                case PAAViewType.PickSinglePipe_Pipe:
                    View.Close();
                    if (!MouseHookHelper.DelegateMouseHook(() =>
                    {
                        //业务逻辑处理
                        //选择符合类型的过滤
                        var targetType = Model.GetFilter();
                        var obj = UIDocument.Selection.PickObject(ObjectType.Element, targetType, "请选择管道标注点");
                        if (obj != null)
                        {
                            Model.TargetId = obj.ElementId;
                            ViewType = PAAViewType.PickSinglePipe_Location;
                        }
                    }))
                        ViewType = PAAViewType.Idle;
                    Execute();
                    break;
                case PAAViewType.PickSinglePipe_Location:
                    if (!MouseHookHelper.DelegateMouseHook(() =>
                    {
                        //业务逻辑处理
                        var target = Document.GetElement(Model.TargetId);
                        var targetLocation = target.Location as LocationCurve;
                        var p0 = targetLocation.Curve.GetEndPoint(0);
                        var p1 = targetLocation.Curve.GetEndPoint(1);
                        var pStart = new XYZ((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2, (p0.Z + p1.Z) / 2);
                        var pEnd = new VLPointPicker().PickPointWithLinePreview(UIApplication, pStart);
                        Model.BodyStartPoint = pStart;
                        Model.BodyEndPoint = pEnd;
                        if (pEnd == null)
                            ViewType = PAAViewType.Idle;
                        else
                            ViewType = PAAViewType.GenerateSinglePipe;
                    }))
                        ViewType = PAAViewType.Idle;
                    Execute();
                    break;
                case PAAViewType.GenerateSinglePipe:
                    if (TransactionHelper.DelegateTransaction(Document, "GenerateSinglePipe", (Func<bool>)(() =>
                        {
                            var element = Document.GetElement(Model.TargetId);
                            var Collection = PAAContext.GetCollection(Document);
                            var existedModels = Collection.Data.Where(c => c.TargetId.IntegerValue==Model.TargetId.IntegerValue);//避免重复生成
                            if (existedModels != null)
                            {
                                foreach (var existedModel in existedModels)
                                {
                                    Collection.Data.Remove(existedModel);
                                    PAAContext.Creator.Clear(Document, existedModel);
                                }
                            }
                            PAAContext.Creator.Generate(Document, Model, element);
                            Collection.Data.Add(Model);
                            Collection.Save(Document);
                            return true;
                        })))
                        ViewType = PAAViewType.PickSinglePipe_Pipe;
                    else
                        ViewType = PAAViewType.Idle;
                    Execute();
                    break;
                default:
                    throw new NotImplementedException("功能未实现");
            }
        }

        #region RatioButtons
        PAATargetType TargetType
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
            get { return TargetType == PAATargetType.Pipe; }
            set { if (value) TargetType = PAATargetType.Pipe; }
        }
        public bool TargetType_Duct
        {
            get { return TargetType == PAATargetType.Duct; }
            set { if (value) TargetType = PAATargetType.Duct; }
        }
        public bool TargetType_CableTray
        {
            get { return TargetType == PAATargetType.CableTray; }
            set { if (value) TargetType = PAATargetType.CableTray; }
        }
        public bool TargetType_Conduit
        {
            get { return TargetType == PAATargetType.Conduit; }
            set { if (value) TargetType = PAATargetType.Conduit; }
        }

        PAAAnnotationType AnnotationType
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
            }
        }
        public bool AnnotationType_SPL
        {
            get { return AnnotationType == PAAAnnotationType.SPL; }
            set { if (value) AnnotationType = PAAAnnotationType.SPL; }
        }
        public bool AnnotationType_SL
        {
            get { return AnnotationType == PAAAnnotationType.SL; }
            set { if (value) AnnotationType = PAAAnnotationType.SL; }
        }
        public bool AnnotationType_PL
        {
            get { return AnnotationType == PAAAnnotationType.PL; }
            set { if (value) AnnotationType = PAAAnnotationType.PL; }
        }

        PAALocationType LocationType
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
            get { return LocationType == PAALocationType.Center; }
            set { if (value) LocationType = PAALocationType.Center; }
        }
        public bool LocationType_Top
        {
            get { return LocationType == PAALocationType.Top; }
            set { if (value) LocationType = PAALocationType.Top; }
        }
        public bool LocationType_Bottom
        {
            get { return LocationType == PAALocationType.Bottom; }
            set { if (value) LocationType = PAALocationType.Bottom; }
        }


        PAATextType TextType
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
            get { return TextType == PAATextType.OnLine; }
            set { if (value) TextType = PAATextType.OnLine; }
        }
        public bool TextType_OnEdge
        {
            get { return TextType == PAATextType.OnEdge; }
            set { if (value) TextType = PAATextType.OnEdge; }
        }
        #endregion
    }
}
