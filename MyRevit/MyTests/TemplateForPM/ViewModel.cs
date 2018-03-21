using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using PmSoft.Common.RevitClass.VLUtils;
using PmSoft.Common.RevitClass.Utils;

namespace PMSoft.ConstructionManagementV2
{
    public enum CMViewType
    {
        Closing = -1,//右上角或Alt+F4关闭
        Close = 0,//按钮关闭或ESC关闭
        Idle = 1,//闲置
        PickSinglePipe_Pipe,//选择单管 管道
        PickSinglePipe_Location,//选择单管 定位
        GenerateSinglePipe,//单管标注生成
        PickMultiplePipes,//选择多管
    }

    public class CMViewModel : VLViewModel<CMModel, CMWindow>
    {
        public CMViewModel(UIApplication app) : base(app)
        {
            Model = new CMModel("");
            View = new CMWindow(this);
            //用以打开时更新页面
            //LoadSetting();
            AnnotationType = CMAnnotationType.SPL;
            LocationType = CMLocationType.Center;
        }

        public override void Execute()
        {
            var viewType = (CMViewType)Enum.Parse(typeof(CMViewType), ViewType.ToString());
            switch (viewType)
            {
                case CMViewType.Idle:
                    View = new CMWindow(this);
                    View.ShowDialog();
                    break;
                case CMViewType.Close:
                    View.Close();
                    //SaveSetting();
                    break;
                case CMViewType.Closing:
                    //SaveSetting();
                    break;
                case CMViewType.PickSinglePipe_Pipe:
                    View.Close();
                    if (!VLHookHelper.DelegateKeyBoardHook(() =>
                    {
                        //业务逻辑处理
                        //选择符合类型的过滤
                        var targetType = Model.GetFilter();
                        Model.TargetIds = new List<ElementId>() { UIDocument.Selection.PickObject(ObjectType.Element, targetType, "请选择管道标注点").ElementId };
                        if (Model.TargetIds.Count > 0)
                            ViewType = (int)CMViewType.PickSinglePipe_Location;
                    }))
                        ViewType = (int)CMViewType.Idle;
                    Execute();
                    break;
                case CMViewType.PickSinglePipe_Location:
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
                        //    ViewType = CMViewType.Idle;
                        //else
                        //    ViewType = CMViewType.GenerateSinglePipe;
                    }))
                        ViewType = (int)CMViewType.Idle;
                    Execute();
                    break;
                case CMViewType.GenerateSinglePipe:
                    if (TransactionUtils.DelegateTransaction(Document, "GenerateSinglePipe", (Func<bool>)(() =>
                        {
                            var element = Document.GetElement(Model.TargetIds.First());
                            var Collection = CMContext.GetCollection(Document);
                            //避免重复生成 由于一个对象可能在不同的视图中进行标注设置 所以还是需要重复生成的
                            var existedModels = Collection.Data.Where(c => c.TargetIds.Intersect(Model.TargetIds, new ElementIdComparer()) != null);
                            if (existedModels != null)
                            {
                                foreach (var existedModel in existedModels)
                                {
                                    Collection.Data.Remove(existedModel);
                                    CMContext.Creator.Clear(Document, existedModel);
                                }
                            }
                            CMContext.Creator.Generate(Document, Model, element);
                            Collection.Data.Add(Model);
                            Collection.Save(Document);
                            return true;
                        })))
                            ViewType = (int)CMViewType.PickSinglePipe_Pipe;
                    else
                        ViewType = (int)CMViewType.Idle;
                    Execute();
                    break;
                default:
                    throw new NotImplementedException("功能未实现");
            }
        }

        #region RatioButtons

        #region CMTargetType
        CMTargetType TargetType
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
            get { return TargetType == CMTargetType.Pipe; }
            set { if (value) TargetType = CMTargetType.Pipe; }
        }
        public bool TargetType_Duct
        {
            get { return TargetType == CMTargetType.Duct; }
            set { if (value) TargetType = CMTargetType.Duct; }
        }
        public bool TargetType_CableTray
        {
            get { return TargetType == CMTargetType.CableTray; }
            set { if (value) TargetType = CMTargetType.CableTray; }
        }
        public bool TargetType_Conduit
        {
            get { return TargetType == CMTargetType.Conduit; }
            set { if (value) TargetType = CMTargetType.Conduit; }
        }
        #endregion

        #region CMAnnotationType
        CMAnnotationType AnnotationType
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
            get { return AnnotationType == CMAnnotationType.SPL; }
            set { if (value) AnnotationType = CMAnnotationType.SPL; }
        }
        public bool AnnotationType_SL
        {
            get { return AnnotationType == CMAnnotationType.SL; }
            set { if (value) AnnotationType = CMAnnotationType.SL; }
        }
        public bool AnnotationType_PL
        {
            get { return AnnotationType == CMAnnotationType.PL; }
            set { if (value) AnnotationType = CMAnnotationType.PL; }
        }
        #endregion

        #region CMLocationType
        CMLocationType LocationType
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
            get { return LocationType == CMLocationType.Center; }
            set { if (value) LocationType = CMLocationType.Center; }
        }
        public bool LocationType_Top
        {
            get { return LocationType == CMLocationType.Top; }
            set { if (value) LocationType = CMLocationType.Top; }
        }
        public bool LocationType_Bottom
        {
            get { return LocationType == CMLocationType.Bottom; }
            set { if (value) LocationType = CMLocationType.Bottom; }
        }
        #endregion

        #region CMTextType
        CMTextType TextType
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
            get { return TextType == CMTextType.TextType_OnLineOrOnLeft; }
            set { if (value) TextType = CMTextType.TextType_OnLineOrOnLeft; }
        }
        public bool TextType_OnEdgeOrOnMiddle
        {
            get { return TextType == CMTextType.TextType_OnEdgeOrOnMiddle; }
            set { if (value) TextType = CMTextType.TextType_OnEdgeOrOnMiddle; }
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
            if (Model.LocationType == CMLocationType.Center)
                Model.AnnotationPrefix = CenterPrefix;
            else if (Model.LocationType == CMLocationType.Top)
                Model.AnnotationPrefix = TopPrefix;
            else if (Model.LocationType == CMLocationType.Bottom)
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
