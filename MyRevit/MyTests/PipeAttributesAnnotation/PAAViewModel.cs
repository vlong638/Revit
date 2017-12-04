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
        PickSingleDuct,//选择风管
        PickSingleCableTray,//选择桥架
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
            Model.Document = Document;
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
                        var startPoint = Model.BodyStartPoint.ToWindowsPoint();
                        var endPoint= (Model.BodyStartPoint + Model.LineWidth * 1.02 * Model.ParallelVector).ToWindowsPoint();
                        var pEnd = new VLPointPicker().PickPointWithPreview(UIApplication,(view)=> {
                                var mousePosition = System.Windows.Forms.Control.MousePosition;
                                //已在初始化处理 为何重复?
                                var rect = view.UIView.GetWindowRectangle();
                                var Left = rect.Left;
                                var Top = rect.Top;
                                var Width = rect.Right - rect.Left;
                                var Height = rect.Bottom - rect.Top;

                                var startDrawP = view.ConvertToDrawPointFromViewPoint(startPoint);//起点
                                var endP = view.ConvertToDrawPointFromViewPoint(endPoint);//终点
                                var midDrawP = new System.Windows.Point(mousePosition.X - rect.Left, mousePosition.Y - rect.Top);//中间选择点
                                var height = midDrawP - startDrawP;
                                midDrawP -= height / 50;
                                var endDrawP = endP + height;
                                if (Math.Abs(startDrawP.X - midDrawP.X) < 2 && Math.Abs(startDrawP.Y - midDrawP.Y) < 2)
                                    return;

                                var canvas = view.canvas;
                                midDrawP.X = midDrawP.X > startDrawP.X ? midDrawP.X - 1 : midDrawP.X + 1;
                                midDrawP.Y = midDrawP.Y > startDrawP.Y ? midDrawP.Y - 1 : midDrawP.Y + 1;
                                canvas.Children.RemoveRange(0, canvas.Children.Count);
                                var line = new System.Windows.Shapes.Line() { X1 = startDrawP.X, Y1 = startDrawP.Y, X2 = midDrawP.X, Y2 = midDrawP.Y, Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(136, 136, 136)), StrokeThickness = 1 };
                                var line2 = new System.Windows.Shapes.Line() { X1 = midDrawP.X, Y1 = midDrawP.Y, X2 = endDrawP.X, Y2 = endDrawP.Y, Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(136, 136, 136)), StrokeThickness = 1 };
                                System.Windows.Media.RenderOptions.SetBitmapScalingMode(line, System.Windows.Media.BitmapScalingMode.LowQuality);
                                System.Windows.Media.RenderOptions.SetBitmapScalingMode(line2, System.Windows.Media.BitmapScalingMode.LowQuality);
                                canvas.Children.Add(line);
                                canvas.Children.Add(line2);
                            })
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
                            Model.ModelType = PAAModelType.SinglePipe;
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
                        Model.ViewId = Document.ActiveView.Id;
                        Model.CurrentFontHeight = PAAContext.FontManagement.CurrentHeight;
                        Model.CurrentFontSizeScale = PAAContext.FontManagement.CurrentFontSizeScale;
                        Model.CurrentFontWidthSize = PAAContext.FontManagement.CurrentFontWidthScale;
                        Model.ModelType = PAAModelType.MultiplePipe;
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
                //TODO
                case PAAViewType.PickSingleDuct:
                    View.Close();
                    if (!VLMouseHookHelper.DelegateMouseHook(() =>
                    {
                        //业务逻辑处理
                        //选择符合类型的过滤
                        var targetType = Model.GetFilter();
                        var targetElementId = UIDocument.Selection.PickObject(ObjectType.Element, targetType, "请选择需要标注的对象");
                        if (targetElementId != null )
                        {
                            Model.TargetId = targetElementId.ElementId;
                        }
                    }))
                    {
                        ViewType = PAAViewType.Idle;
                        Execute();
                        return;
                    }
                    //获取族内参数信息
                    if (!GetFamilySymbolInfo(Model.TargetId))
                    {
                        ShowMessage("加载族文字信息失败");
                        ViewType = PAAViewType.Idle;
                        Execute();
                        return;
                    }
                    else
                    {
                        Model.CurrentFontHeight = PAAContext.FontManagement.CurrentHeight;
                        Model.CurrentFontSizeScale = PAAContext.FontManagement.CurrentFontSizeScale;
                        Model.CurrentFontWidthSize = PAAContext.FontManagement.CurrentFontWidthScale;
                    }
                    if (!VLTransactionHelper.DelegateTransaction(Document, "PickSingleDuct", (Func<bool>)(() =>
                    {
                        Model.ModelType = PAAModelType.SingleDuct;
                        PAAContext.Creator.Generate(Model);
                        return true;
                    })))
                    {
                        ViewType = PAAViewType.Idle;
                        Execute();
                        return;
                    }
                    Execute();
                    break;
                case PAAViewType.PickSingleCableTray:
                    View.Close();
                    if (!VLMouseHookHelper.DelegateMouseHook(() =>
                    {
                        //业务逻辑处理
                        //选择符合类型的过滤
                        var targetType = Model.GetFilter();
                        var targetElementId = UIDocument.Selection.PickObject(ObjectType.Element, targetType, "请选择需要标注的对象");
                        if (targetElementId != null)
                        {
                            Model.TargetId = targetElementId.ElementId;
                        }
                    }))
                    {
                        ViewType = PAAViewType.Idle;
                        Execute();
                        return;
                    }
                    //获取族内参数信息
                    if (!GetFamilySymbolInfo(Model.TargetId))
                    {
                        ShowMessage("加载族文字信息失败");
                        ViewType = PAAViewType.Idle;
                        Execute();
                        return;
                    }
                    else
                    {
                        Model.CurrentFontHeight = PAAContext.FontManagement.CurrentHeight;
                        Model.CurrentFontSizeScale = PAAContext.FontManagement.CurrentFontSizeScale;
                        Model.CurrentFontWidthSize = PAAContext.FontManagement.CurrentFontWidthScale;
                    }
                    if (!VLTransactionHelper.DelegateTransaction(Document, "SingleCableTray", (Func<bool>)(() =>
                    {
                        Model.ModelType = PAAModelType.SingleCableTray;
                        PAAContext.Creator.Generate(Model);
                        return true;
                    })))
                    {
                        ViewType = PAAViewType.Idle;
                        Execute();
                        return;
                    }
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
                        TextType_Option1_Title = "文字在线上";
                        TextType_Option2_Title = "文字在线端";
                        Btn_Left_Title = "单管标注";
                        Btn_Right_Title = "多管标注";
                        break;
                    case PAATargetType.Duct:
                        AnnotationType = PAAAnnotationType.SPL;
                        LocationType = PAALocationType.Center;
                        TextType_Option1_Title = "位于风管中心";
                        TextType_Option2_Title = "位于风管上方";
                        Btn_Left_Title = "选管标注";
                        Btn_Right_Title = "取消";
                        break;
                    case PAATargetType.CableTray:
                        AnnotationType = PAAAnnotationType.SPL;
                        LocationType = PAALocationType.Bottom;
                        TextType_Option1_Title = "位于桥架中心";
                        TextType_Option2_Title = "位于桥架上方";
                        Btn_Left_Title = "选择桥架";
                        Btn_Right_Title = "取消";
                        break;
                    case PAATargetType.Conduit:
                    default:
                        break;
                }
                RaisePropertyChanged("TextType_Option1_Title");
                RaisePropertyChanged("TextType_Option2_Title");
                RaisePropertyChanged("Btn_Left_Title");
                RaisePropertyChanged("Btn_Right_Title");
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
                RaisePropertyChanged("TextType_Option1");
                RaisePropertyChanged("TextType_Option2");
            }
        }
        public bool TextType_Option1
        {
            get { return TextType == PAATextType.Option1; }
            set { if (value) TextType = PAATextType.Option1; }
        }
        public bool TextType_Option2
        {
            get { return TextType == PAATextType.Option2; }
            set { if (value) TextType = PAATextType.Option2; }
        }
        public string TextType_Option1_Title { set; get; }
        public string TextType_Option2_Title { set; get; }
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

        internal void UpdateViewType(bool isLeft)
        {
            switch (TargetType)
            {
                case PAATargetType.Pipe:
                    if (isLeft)
                        ViewType = PAAViewType.PickSinglePipe_Pipe;
                    else
                        ViewType = PAAViewType.PickMultiplePipes;
                    break;
                case PAATargetType.Duct:
                    if (isLeft)
                        ViewType = PAAViewType.PickSingleDuct;
                    else
                        ViewType = PAAViewType.Close;
                    break;
                case PAATargetType.CableTray:
                    if (isLeft)
                        ViewType = PAAViewType.PickSingleCableTray;
                    else
                        ViewType = PAAViewType.Close;
                    break;
                case PAATargetType.Conduit:
                default:
                    throw new NotImplementedException();
            }
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

        public string Btn_Left_Title { get; set; }
        public string Btn_Right_Title { get; set; }
        #endregion

        #endregion
    }
}
