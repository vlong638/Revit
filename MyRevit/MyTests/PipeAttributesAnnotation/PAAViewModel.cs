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
            AnnotationType = PAAAnnotationType.SPL;
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
                            Model.BodyStartPoint = obj.GlobalPoint;
                        }
                    }))
                        ViewType = PAAViewType.Idle;
                    Execute();
                    break;
                case PAAViewType.PickSinglePipe_Location:
                    if (!MouseHookHelper.DelegateMouseHook(() =>
                    {
                        //业务逻辑处理
                        var pEnd = new VLPointPicker().PickPointWithLinePreview(UIApplication, Model.BodyStartPoint).ToSameZ(Model.BodyStartPoint);
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
                    //更新必要参数
                    UpdateModelAnnotationPrefix();
                    //获取族
                    FamilySymbol annotationFamily = null;
                    if (!TransactionHelper.DelegateTransaction(Document, "GenerateSinglePipe", (Func<bool>)(() =>
                    {
                        var family = Model.GetAnnotationFamily(Document);
                        return family != null;
                    })))
                    {
                        ShowMessage("加载功能所需的族失败");
                        ViewType = PAAViewType.Idle;
                        Execute();
                        return;
                    }
                    //准备族内参数
                    if (!PAAContext.FontManagement.IsCurrentFontSettled)
                    {
                        var familyDoc = Document.EditFamily(annotationFamily.Family);
                        var textElement = new FilteredElementCollector(familyDoc).OfClass(typeof(TextElement)).First(c => c.Name == "2.5") as TextElement;
                        var textElementType = textElement.Symbol as TextElementType;
                        PAAContext.FontManagement.SetCurrentFont(textElementType);
                    }
                    //生成处理
                    if (TransactionHelper.DelegateTransaction(Document, "GenerateSinglePipe", (Func<bool>)(() =>
                        {
                            Model.Document = Document;
                            #region 生成处理
                            var Collection = PAAContext.GetCollection(Document);
                            var existedModels = Collection.Data.Where(c => Model.TargetId==c.TargetId).ToList();//避免重复生成
                            if (existedModels != null)
                            {
                                for (int i = existedModels.Count() - 1; i >= 0; i--)
                                {
                                    existedModels[i].Document = Document;
                                    Collection.Data.Remove(existedModels[i]);
                                    PAAContext.Creator.Clear(existedModels[i]);
                                }
                            }
                            Model.CurrentFontHeight = PAAContext.FontManagement.CurrentFontHeight;
                            Model.CurrentFontSizeScale = PAAContext.FontManagement.CurrentFontSizeScale;
                            Model.CurrentFontWidthScale = PAAContext.FontManagement.CurrentFontWidthScale;
                            Model.ModelType = ModelType.Single;
                            if (!PAAContext.Creator.Generate( Model))
                                return false;
                            Collection.Data.Add(Model);
                            Collection.Save(Document); 
                            #endregion

                            #region 共享参数设置
                            var element = Document.GetElement(Model.TargetId);
                            if (element.GetParameters(PAAContext.SharedParameterPL).Count == 0)
                            {
                                string shareFilePath = @"E:\WorkingSpace\Tasks\1101管道特性标注\PMSharedParameters.txt";//GetShareFilePath();
                                var parameterHelper = new ShareParameter(shareFilePath);
                                parameterHelper.AddShadeParameter(Document, PAAContext.SharedParameterGroupName, PAAContext.SharedParameterPL, element.Category, true, BuiltInParameterGroup.PG_TEXT);
                            }
                            var offset = element.GetParameters(PAAContext.SharedParameterOffset).FirstOrDefault().AsDouble();
                            var diameter = element.GetParameters(PAAContext.SharedParameterDiameter).FirstOrDefault().AsDouble();
                            element.GetParameters(PAAContext.SharedParameterPL).FirstOrDefault().Set(UnitHelper.ConvertFromFootTo(Model.LocationType.GetLocationValue(offset, diameter), VLUnitType.millimeter).ToString());
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
                    if (!MouseHookHelper.DelegateMouseHook(() =>
                    {
                        //业务逻辑处理
                        //选择符合类型的过滤
                        var targetType = Model.GetFilter();
                        var targetElementIds = UIDocument.Selection.PickObjects(ObjectType.Element, targetType, "请选择需要标注的管道").Select(c => c.ElementId);
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
                    //更新必要参数
                    UpdateModelAnnotationPrefix();
                    //获取族
                    annotationFamily = null;
                    if (!TransactionHelper.DelegateTransaction(Document, "PickMultiplePipes", (Func<bool>)(() =>
                    {
                        annotationFamily = Model.GetAnnotationFamily(Document);
                        var lineFamily = Model.GetLineFamily(Document);
                        return annotationFamily != null && lineFamily != null;
                    })))
                    {
                        ShowMessage("加载功能所需的族失败");
                        ViewType = PAAViewType.Idle;
                        Execute();
                        return;
                    }
                    //准备族内参数
                    if (!PAAContext.FontManagement.IsCurrentFontSettled)
                    {
                        var familyDoc = Document.EditFamily(annotationFamily.Family);
                        var textElement = new FilteredElementCollector(familyDoc).OfClass(typeof(TextElement)).First(c => c.Name == "2.5") as TextElement;
                        var textElementType = textElement.Symbol as TextElementType;
                        PAAContext.FontManagement.SetCurrentFont(textElementType);
                    }
                    //生成处理
                    if (!TransactionHelper.DelegateTransaction(Document, "PickMultiplePipes", (Func<bool>)(() =>
                    {
                        #region 生成处理
                        var Collection = PAAContext.GetCollection(Document);
                        var existedModels = Collection.Data.Where(c => c.TargetIds != null && Model.TargetIds.Intersect(c.TargetIds, new ElementIdComparer()).Count() > 0).ToList();//避免重复生成
                        if (existedModels != null)
                        {
                            for (int i = existedModels.Count() - 1; i >= 0; i--)
                            {
                                Collection.Data.Remove(existedModels[i]);
                                existedModels[i].Document = Document;
                                PAAContext.Creator.Clear(existedModels[i]);
                            }
                        }
                        Model.Document = Document;
                        Model.CurrentFontHeight = PAAContext.FontManagement.CurrentFontHeight;
                        Model.CurrentFontSizeScale = PAAContext.FontManagement.CurrentFontSizeScale;
                        Model.CurrentFontWidthScale = PAAContext.FontManagement.CurrentFontWidthScale;
                        Model.ModelType = ModelType.Multiple;
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
                            var offset = element.GetParameters(PAAContext.SharedParameterOffset).FirstOrDefault().AsDouble();
                            var diameter = element.GetParameters(PAAContext.SharedParameterDiameter).FirstOrDefault().AsDouble();
                            element.GetParameters(PAAContext.SharedParameterPL).FirstOrDefault().Set(UnitHelper.ConvertFromFootTo(Model.LocationType.GetLocationValue(offset, diameter), VLUnitType.millimeter).ToString());
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

        private static void ShowMessage(string msg)
        {
            throw new NotImplementedException("");
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
            }
        }

        private void UpdateModelAnnotationPrefix()
        {
            if (Model.LocationType == PAALocationType.Center)
                Model.AnnotationPrefix = CenterPrefix;
            else if (Model.LocationType == PAALocationType.Top)
                Model.AnnotationPrefix = TopPrefix;
            else if (Model.LocationType == PAALocationType.Bottom)
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

        public string SPLPreview { get { return string.Format("如:ZP DN100 {0}2600", CenterPrefix); }  }
        public string SLPreview { get { return string.Format("如:ZP {0}2600", TopPrefix); } }
        public string PLPreview { get { return "如:ZP DN100"; } }
        #endregion
    }
}
