using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using PmSoft.Common.RevitClass.VLUtils;
using PmSoft.Common.RevitClass.Utils;

namespace PmSoft.MepProject.MepWork.FullFunctions.MEPCurveAutomaticTurn
{
    public enum MATViewType
    {
        Closing = -1,//右上角或Alt+F4关闭
        Close = 0,//按钮关闭或ESC关闭
        Idle = 1,//闲置
        PickSinglePipe_Pipe,//选择单管 管道
        PickSinglePipe_Location,//选择单管 定位
        GenerateSinglePipe,//单管标注生成
        PickMultiplePipes,//选择多管
    }

    public class MATViewModel : VLViewModel<MATModel, MATWindow>
    {
        public MATViewModel(UIApplication app) : base(app)
        {
            Model = new MATModel("");
            View = new MATWindow(this);
            //用以打开时更新页面
            //LoadSetting();
            AnnotationType = MATAnnotationType.SPL;
            LocationType = MATLocationType.Center;
        }

        public override void Execute()
        {
            var viewType = (MATViewType)Enum.Parse(typeof(MATViewType), ViewType.ToString());
            switch (viewType)
            {
                case MATViewType.Idle:
                    View = new MATWindow(this);
                    View.ShowDialog();
                    break;
                case MATViewType.Close:
                    View.Close();
                    //SaveSetting();
                    break;
                case MATViewType.Closing:
                    //SaveSetting();
                    break;
                case MATViewType.PickSinglePipe_Pipe:
                    View.Close();
                    if (!VLHookHelper.DelegateKeyBoardHook(() =>
                    {
                        //业务逻辑处理
                        //选择符合类型的过滤
                        var targetType = Model.GetFilter();
                        Model.TargetIds = new List<ElementId>() { UIDocument.Selection.PickObject(ObjectType.Element, targetType, "请选择管道标注点").ElementId };
                        if (Model.TargetIds.Count > 0)
                            ViewType = (int)MATViewType.PickSinglePipe_Location;
                    }))
                        ViewType = (int)MATViewType.Idle;
                    Execute();
                    break;
                case MATViewType.PickSinglePipe_Location:
                    if (!VLHookHelper.DelegateKeyBoardHook(() =>
                    {
                        ////业务逻辑处理
                        //var target = Document.GetElement(Model.TargetIds.First());
                        //var targetLocation = target.Location as LocationCurve;
                        //var p0 = targetLocation.Curve.GetEndPoint(0);
                        //var p1 = targetLocation.Curve.GetEndPoint(1);
                        //var pStart = new XYZ((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2, (p0.Z + p1.Z) / 2);
                        //var pEnd = new VLPointPicker().PickPointWithLinePreview(UIApplication, pStart);
                        //if (pEnd == null)
                        //    ViewType = MATViewType.Idle;
                        //else
                        //    ViewType = MATViewType.GenerateSinglePipe;
                    }))
                        ViewType = (int)MATViewType.Idle;
                    Execute();
                    break;
                case MATViewType.GenerateSinglePipe:
                    if (TransactionUtils.DelegateTransaction(Document, "GenerateSinglePipe", (Func<bool>)(() =>
                        {
                            var element = Document.GetElement(Model.TargetIds.First());
                            var Collection = MATContext.GetCollection(Document);
                            //避免重复生成 由于一个对象可能在不同的视图中进行标注设置 所以还是需要重复生成的
                            var existedModels = Collection.Data.Where(c => c.TargetIds.Intersect(Model.TargetIds, new VLElementIdComparer()) != null);
                            if (existedModels != null)
                            {
                                foreach (var existedModel in existedModels)
                                {
                                    Collection.Data.Remove(existedModel);
                                    MATContext.Creator.Clear(Document, existedModel);
                                }
                            }
                            MATContext.Creator.Generate(Document, Model, element);
                            Collection.Data.Add(Model);
                            Collection.Save(Document);
                            return true;
                        })))
                            ViewType = (int)MATViewType.PickSinglePipe_Pipe;
                    else
                        ViewType = (int)MATViewType.Idle;
                    Execute();
                    break;
                default:
                    throw new NotImplementedException("功能未实现");
            }
        }

        #region RatioButtons

        #region MATTargetType
        MATTargetType TargetType
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
            get { return TargetType == MATTargetType.Pipe; }
            set { if (value) TargetType = MATTargetType.Pipe; }
        }
        public bool TargetType_Duct
        {
            get { return TargetType == MATTargetType.Duct; }
            set { if (value) TargetType = MATTargetType.Duct; }
        }
        public bool TargetType_CableTray
        {
            get { return TargetType == MATTargetType.CableTray; }
            set { if (value) TargetType = MATTargetType.CableTray; }
        }
        public bool TargetType_Conduit
        {
            get { return TargetType == MATTargetType.Conduit; }
            set { if (value) TargetType = MATTargetType.Conduit; }
        }
        #endregion

        #region MATAnnotationType
        MATAnnotationType AnnotationType
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
            get { return AnnotationType == MATAnnotationType.SPL; }
            set { if (value) AnnotationType = MATAnnotationType.SPL; }
        }
        public bool AnnotationType_SL
        {
            get { return AnnotationType == MATAnnotationType.SL; }
            set { if (value) AnnotationType = MATAnnotationType.SL; }
        }
        public bool AnnotationType_PL
        {
            get { return AnnotationType == MATAnnotationType.PL; }
            set { if (value) AnnotationType = MATAnnotationType.PL; }
        }
        #endregion

        #region MATLocationType
        MATLocationType LocationType
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
            get { return LocationType == MATLocationType.Center; }
            set { if (value) LocationType = MATLocationType.Center; }
        }
        public bool LocationType_Top
        {
            get { return LocationType == MATLocationType.Top; }
            set { if (value) LocationType = MATLocationType.Top; }
        }
        public bool LocationType_Bottom
        {
            get { return LocationType == MATLocationType.Bottom; }
            set { if (value) LocationType = MATLocationType.Bottom; }
        }
        #endregion

        #region MATTextType
        MATTextType TextType
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
            get { return TextType == MATTextType.TextType_OnLineOrOnLeft; }
            set { if (value) TextType = MATTextType.TextType_OnLineOrOnLeft; }
        }
        public bool TextType_OnEdgeOrOnMiddle
        {
            get { return TextType == MATTextType.TextType_OnEdgeOrOnMiddle; }
            set { if (value) TextType = MATTextType.TextType_OnEdgeOrOnMiddle; }
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
            if (Model.LocationType == MATLocationType.Center)
                Model.AnnotationPrefix = CenterPrefix;
            else if (Model.LocationType == MATLocationType.Top)
                Model.AnnotationPrefix = TopPrefix;
            else if (Model.LocationType == MATLocationType.Bottom)
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
