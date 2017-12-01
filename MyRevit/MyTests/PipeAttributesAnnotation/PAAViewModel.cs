using System;
using Autodesk.Revit.UI;
using MyRevit.MyTests.VLBase;
using MyRevit.Utilities;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using Autodesk.Revit.DB;
using MyRevit.MyTests.PipeAttributesAnnotation;
using System.Collections.Generic;
using PmSoft.Common.RevitClass.Utils;
using System.Windows.Forms;

namespace MyRevit.MyTests.PAA
{
    public class ElementIdComparer : IEqualityComparer<ElementId>
    {
        public bool Equals(ElementId t1, ElementId t2)
        {
            return t1 == t2;
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

    public class PAAViewModel : VLViewModel<PAAModel, PAAWindow, PAAViewType>
    {
        public PAAViewModel(UIApplication app) : base(app)
        {
            Model = new PAAModel("");
            View = new PAAWindow(this);
            //用以打开时更新页面
            TargetType = PAATargetType.Pipe;
        }

        public bool Prepare()
        {
            try
            {
                if (!VLTransactionHelper.DelegateTransaction(Document, "GenerateSinglePipe", (Func<bool>)(() =>
                {
                    //添加共享参数
                    string shareFilePath = @"E:\WorkingSpace\Tasks\1101管道特性标注\PMSharedParameters.txt";//GetShareFilePath();
                    var parameterHelper = new ShareParameter(shareFilePath);
                    parameterHelper.AddShadeParameter(Document, PAAContext.SharedParameterGroupName, PAAContext.SharedParameterPL, Document.Settings.Categories.get_Item(BuiltInCategory.OST_PipeCurves), true, BuiltInParameterGroup.PG_TEXT);
                    parameterHelper.AddShadeParameter(Document, PAAContext.SharedParameterGroupName, PAAContext.SharedParameterPL, Document.Settings.Categories.get_Item(BuiltInCategory.OST_DuctCurves), true, BuiltInParameterGroup.PG_TEXT);
                    parameterHelper.AddShadeParameter(Document, PAAContext.SharedParameterGroupName, PAAContext.SharedParameterPL, Document.Settings.Categories.get_Item(BuiltInCategory.OST_CableTray), true, BuiltInParameterGroup.PG_TEXT);
                    //parameterHelper.AddShadeParameter(Document, PAAContext.SharedParameterGroupName, PAAContext.SharedParameterPL, Document.Settings.Categories.get_Item(BuiltInCategory.OST_Conduit), true, BuiltInParameterGroup.PG_TEXT);
                    return true;
                })))
                {
                    ShowMessage("加载功能所需的共享参数失败");
                    ViewType = PAAViewType.Idle;
                    Execute();
                    return false;
                }
                //if (!TransactionHelper.DelegateTransaction(Document, "GenerateSinglePipe", (Func<bool>)(() =>
                //{
                //    //获取族
                //    PAAContext.GetSPLTag_Pipe(Document);
                //    PAAContext.GetSLTag(Document);
                //    PAAContext.GetPLTag(Document);
                //    PAAContext.Get_MultipleLineOnEdge(Document);
                //    PAAContext.Get_MultipleLineOnLine(Document);
                //    return true;
                //})))
                //{
                //    ShowMessage("加载功能所需的族失败");
                //    ViewType = PAAViewType.Idle;
                //    Execute();
                //    return false;
                //}
            }
            catch (Exception ex)
            {
                ShowMessage("准备必要的内容时出现异常");
                return false;
            }
            return true;
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
                    Model.Document = Document;
                    Model.ViewId = Document.ActiveView.Id;
                    View.Close();
                    if (!VLMouseHookHelper.DelegateMouseHook(() =>
                    {
                        //业务逻辑处理
                        //选择符合类型的过滤
                        var targetType = Model.GetFilter();
                        var obj = UIDocument.Selection.PickObject(ObjectType.Element, targetType, "请选择管道标注点");
                        if (obj != null)
                        {
                            Model.TargetId = obj.ElementId;
                            ViewType = PAAViewType.PickSinglePipe_Location;
                            Model.BodyStartPoint = obj.GlobalPoint;
                        }
                    }))
                        ViewType = PAAViewType.Idle;
                    //获取族内参数信息
                    if (!GetFamilySymbolInfo(Model.TargetId))
                    {
                        ShowMessage("加载族文字信息失败");
                        ViewType = PAAViewType.Idle;
                        Execute();
                        return;
                    }
                    Execute();
                    break;
                case PAAViewType.PickSinglePipe_Location:
                    //业务逻辑处理
                    if (!VLMouseHookHelper.DelegateMouseHook(() =>
                    {
                        var target = Document.GetElement(Model.TargetId);
                        var locationCurve = TargetType.GetLine(target);
                        Model.UpdateVectors(locationCurve);
                        Model.UpdateLineWidth(target);
                        var pEnd = new VLPointPicker().PickPointWithLinePreview(UIApplication,
                            Model.BodyStartPoint, Model.BodyStartPoint + Model.LineWidth * 1.02 * Model.ParallelVector)
                            .ToSameZ(Model.BodyStartPoint);
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
                    //生成处理
                    if (VLTransactionHelper.DelegateTransaction(Document, "GenerateSinglePipe", (Func<bool>)(() =>
                        {
                            #region 生成处理
                            var Collection = PAAContext.GetCollection(Document);
                            var existedModels = Collection.Data.Where(c => Model.TargetId == c.TargetId || (c.TargetIds != null && c.TargetIds.Contains(Model.TargetId))).ToList();//避免重复生成
                            if (existedModels != null)
                            {
                                for (int i = existedModels.Count() - 1; i >= 0; i--)
                                {
                                    existedModels[i].Document = Document;
                                    Collection.Data.Remove(existedModels[i]);
                                    existedModels[i].Clear();
                                }
                            }
                            Model.CurrentFontHeight = PAAContext.FontManagement.CurrentHeight;
                            Model.CurrentFontSizeScale = PAAContext.FontManagement.CurrentFontSizeScale;
                            Model.CurrentFontWidthSize = PAAContext.FontManagement.CurrentFontWidthSize;
                            Model.ModelType = PAAModelType.Single;
                            if (!PAAContext.Creator.Generate(Model))
                                return false;
                            Collection.Data.Add(Model);
                            Collection.Save(Document);
                            #endregion

                            #region 共享参数设置
                            var element = Document.GetElement(Model.TargetId);
                            element.GetParameters(PAAContext.SharedParameterPL).FirstOrDefault().Set(Model.GetFull_L(element));
                            #endregion

                            return true;
                        })))
                        ViewType = PAAViewType.PickSinglePipe_Pipe;
                    else
                        ViewType = PAAViewType.Idle;
                    Execute();
                    break;
                case PAAViewType.PickMultiplePipes:
                    View.Close();
                    if (!VLMouseHookHelper.DelegateMouseHook(() =>
                    {
                        //业务逻辑处理
                        //选择符合类型的过滤
                        var targetType = Model.GetFilter();
                        var targetElementIds = UIDocument.Selection.PickObjects(ObjectType.Element, targetType, "请选择需要标注的对象").Select(c => c.ElementId);
                        if (targetElementIds != null && targetElementIds.Count() > 0)
                        {
                            if (targetElementIds.Count() < 2)
                            {
                                ShowMessage("多管直径标注需选择两个以上的管道");
                                ViewType = PAAViewType.Idle;
                                Execute();
                                return;
                            }
                            Model.TargetIds = new List<ElementId>();
                            Model.TargetIds.AddRange(targetElementIds);
                        }
                    }))
                    {
                        ViewType = PAAViewType.Idle;
                        Execute();
                        return;
                    }
                    //平行检测
                    if (!Model.CheckParallel(Document))
                    {
                        ShowMessage("多管的管道需为平行的");
                        ViewType = PAAViewType.Idle;
                        Execute();
                        return;
                    }
                    //获取族内参数信息
                    if (!GetFamilySymbolInfo(Model.TargetIds.First()))
                    {
                        ShowMessage("加载族文字信息失败");
                        ViewType = PAAViewType.Idle;
                        Execute();
                        return;
                    }
                    Model.UpdateLineWidth(Document.GetElement(Model.TargetIds.First()));
                    //生成处理
                    if (!VLTransactionHelper.DelegateTransaction(Document, "PickMultiplePipes", (Func<bool>)(() =>
                    {
                        #region 生成处理
                        var Collection = PAAContext.GetCollection(Document);
                        var existedModels = Collection.Data.Where(c => Model.TargetIds.Contains(c.TargetId) || (c.TargetIds != null && Model.TargetIds.Intersect(c.TargetIds, new ElementIdComparer()).Count() > 0)).ToList();//避免重复生成
                        if (existedModels != null)
                        {
                            for (int i = existedModels.Count() - 1; i >= 0; i--)
                            {
                                Collection.Data.Remove(existedModels[i]);
                                existedModels[i].Document = Document;
                                existedModels[i].Clear();
                            }
                        }
                        Model.Document = Document;
                        Model.ViewId = Document.ActiveView.Id;
                        Model.CurrentFontHeight = PAAContext.FontManagement.CurrentHeight;
                        Model.CurrentFontSizeScale = PAAContext.FontManagement.CurrentFontSizeScale;
                        Model.CurrentFontWidthSize = PAAContext.FontManagement.CurrentFontWidthScale;
                        Model.ModelType = PAAModelType.Multiple;
                        PAAContext.Creator.Generate(Model);
                        Collection.Data.Add(Model);
                        Collection.Save(Document);
                        #endregion

                        #region 共享参数设置
                        foreach (var TargetId in Model.TargetIds)
                        {
                            var element = Document.GetElement(TargetId);
                            if (element.GetParameters(PAAContext.SharedParameterPL).Count == 0)
                            {
                                string shareFilePath = @"E:\WorkingSpace\Tasks\1101管道特性标注\PMSharedParameters.txt";//GetShareFilePath();
                                var parameterHelper = new ShareParameter(shareFilePath);
                                parameterHelper.AddShadeParameter(Document, PAAContext.SharedParameterGroupName, PAAContext.SharedParameterPL, element.Category, true, BuiltInParameterGroup.PG_TEXT);
                            }
                            element.GetParameters(PAAContext.SharedParameterPL).FirstOrDefault().Set(Model.GetFull_L(element));
                        }
                        #endregion
                        return true;
                    })))
                        ViewType = PAAViewType.Idle;
                    Execute();
                    break;
                default:
                    throw new NotImplementedException("功能未实现");
            }
        }

        private bool GetFamilySymbolInfo(ElementId targetId)
        {
            FamilySymbol annotationFamily = null;
            if (!VLTransactionHelper.DelegateTransaction(Document, "GetFamilySymbolInfo", (Func<bool>)(() =>
            {
                annotationFamily = Model.GetAnnotationFamily(Document, targetId);
                var lineFamily = Model.GetLineFamily(Document);
                return annotationFamily != null && lineFamily != null;
            })))
                return false;
            //准备族内参数
            if (!PAAContext.FontManagement.IsCurrentFontSettled)
            {
                var familyDoc = Document.EditFamily(annotationFamily.Family);
                var textElement = new FilteredElementCollector(familyDoc).OfClass(typeof(TextElement)).First(c => c.Name == "2.5") as TextElement;
                var textElementType = textElement.Symbol as TextElementType;
                PAAContext.FontManagement.SetCurrentFont(textElementType);
            }
            return true;
        }
        #region RatioButtons

        #region PAATargetType
        PAATargetType TargetType
        {
            get
            {
                return Model.TargetType;
            }
            set
            {
                Model.TargetType = value;
                switch (value)
                {
                    case PAATargetType.Pipe:
                        AnnotationType = PAAAnnotationType.SPL;
                        LocationType = PAALocationType.Center;
                        break;
                    case PAATargetType.Duct:
                        AnnotationType = PAAAnnotationType.SPL;
                        LocationType = PAALocationType.Center;
                        break;
                    case PAATargetType.CableTray:
                        AnnotationType = PAAAnnotationType.SPL;
                        LocationType = PAALocationType.Bottom;
                        break;
                    case PAATargetType.Conduit:
                    default:
                        break;
                }

                RaisePropertyChanged("TargetType_Pipe");
                RaisePropertyChanged("TargetType_Duct");
                RaisePropertyChanged("TargetType_CableTray");
                RaisePropertyChanged("TargetType_Conduit");
                RowHeight_AnnotationType = TargetType == PAATargetType.Pipe ? 170 : 210;
                RaisePropertyChanged("WindowHeight_AnnotationType");
                RaisePropertyChanged("RowHeight_AnnotationType");
            }
        }
        public bool TargetType_Pipe
        {
            get { return TargetType == PAATargetType.Pipe; }
            set { if (value) { TargetType = PAATargetType.Pipe; } }
        }
        public bool TargetType_Duct
        {
            get { return TargetType == PAATargetType.Duct; }
            set { if (value) { TargetType = PAATargetType.Duct; } }
        }
        public bool TargetType_CableTray
        {
            get { return TargetType == PAATargetType.CableTray; }
            set { if (value) { TargetType = PAATargetType.CableTray; } }
        }
        public bool TargetType_Conduit
        {
            get { return TargetType == PAATargetType.Conduit; }
            set { if (value) { TargetType = PAATargetType.Conduit; } }
        }
        #endregion

        #region PAAAnnotationType
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
                RaisePropertyChanged("AnnotationType_SP");
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
        public bool AnnotationType_SP
        {
            get { return AnnotationType == PAAAnnotationType.SP; }
            set { if (value) AnnotationType = PAAAnnotationType.SP; }
        }
        #endregion

        #region PAALocationType
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
                UpdateModelAnnotationPrefix();
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
        #endregion

        #region PAATextType
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

        #region Texts and Size
        public int WindowHeight_AnnotationType { set; get; }
        public int RowHeight_AnnotationType { set; get; }


        private void UpdateModelAnnotationPrefix()
        {
            if (Model.LocationType == PAALocationType.Center)
                Model.AnnotationPrefix = CenterPrefix;
            else if (Model.LocationType == PAALocationType.Top)
                Model.AnnotationPrefix = TopPrefix;
            else if (Model.LocationType == PAALocationType.Bottom)
                Model.AnnotationPrefix = BottomPrefix;
            RaisePropertyChanged("SPLPreview");
            RaisePropertyChanged("SLPreview");
            RaisePropertyChanged("PLPreview");
            RaisePropertyChanged("SPPreview");
            RaisePropertyChanged("rbtn_SPL");
            RaisePropertyChanged("rbtn_SL");
            RaisePropertyChanged("rbtn_PL");
            RaisePropertyChanged("rbtn_SP");
        }

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
                UpdateModelAnnotationPrefix();
            }
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
                UpdateModelAnnotationPrefix();
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
                UpdateModelAnnotationPrefix();
            }
        }

        public string SPLPreview { get { return Model.GetPreview(PAAAnnotationType.SPL); } }
        public string SLPreview { get { return Model.GetPreview(PAAAnnotationType.SL); } }
        public string PLPreview { get { return Model.GetPreview(PAAAnnotationType.PL); } }
        public string SPPreview { get { return Model.GetPreview(PAAAnnotationType.SP); } }

        public string rbtn_SPL { get { return Model.GetTitle(PAAAnnotationType.SPL); } }
        public string rbtn_SL { get { return Model.GetTitle(PAAAnnotationType.SL); } }
        public string rbtn_PL { get { return Model.GetTitle(PAAAnnotationType.PL); } }
        public string rbtn_SP { get { return Model.GetTitle(PAAAnnotationType.SP); } }
        #endregion

        #endregion
    }
}
