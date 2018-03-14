using System;
using Autodesk.Revit.UI;
using MyRevit.MyTests.VLBase;
using MyRevit.Utilities;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using PmSoft.Common.RevitClass.Utils;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace MyRevit.MyTests.PBPA
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

    public enum PBPAViewType
    {
        Closing = -1,//右上角或Alt+F4关闭
        Close = 0,//按钮关闭或ESC关闭
        Idle = 1,//闲置
        PickSingle_Target,//选择引注_起点
        PickSingle_End,//选择引注_终点
        GenerateSingle,
        GenerateAll,//一键引注
        RegenerateAllFor_L,//更新离地高度
    }

    public class PBPASetting : VLModel
    {
        public bool TargetType_Punch = true;
        public bool TargetType_BranchPipe = true;
        public PBPAAnnotationType AnnotationType = PBPAAnnotationType.TwoLine;
        public PBPALocationType LocationType = PBPALocationType.Center;
        public string CenterPrefix = "CL+";
        public string TopPrefix = "TL+";
        public string BottomPrefix = "BL+";

        public PBPASetting(string data) : base(data)
        {
        }

        public override bool LoadData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return false;
            try
            {
                StringReader sr = new StringReader(data);
                TargetType_Punch = sr.ReadFormatStringAsBoolean();
                TargetType_BranchPipe = sr.ReadFormatStringAsBoolean();
                AnnotationType = sr.ReadFormatStringAsEnum<PBPAAnnotationType>();
                LocationType = sr.ReadFormatStringAsEnum<PBPALocationType>();
                CenterPrefix = sr.ReadFormatString();
                TopPrefix = sr.ReadFormatString();
                BottomPrefix = sr.ReadFormatString();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override string ToData()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendItem(TargetType_Punch);
            sb.AppendItem(TargetType_BranchPipe);
            sb.AppendItem(AnnotationType);
            sb.AppendItem(LocationType);
            sb.AppendItem(CenterPrefix);
            sb.AppendItem(TopPrefix);
            sb.AppendItem(BottomPrefix);
            return sb.ToString();
        }

        public PBPASetting CloneText()
        {
            PBPASetting newSetting = new PBPASetting("");
            newSetting.CenterPrefix = this.CenterPrefix;
            newSetting.TopPrefix = this.TopPrefix;
            newSetting.BottomPrefix = this.BottomPrefix;
            return newSetting;
        }
    }

    public class PBPASettingMemo
    {
        public PBPASetting SettingTextMemo;
        public bool IsSame { set; get; }
        public bool IsSame_CenterPrefix { set; get; }
        public bool IsSame_TopPrefix { set; get; }
        public bool IsSame_BottomPrefix { set; get; }

        void CheckDifference(PBPASetting origin)
        {
            IsSame_CenterPrefix = SettingTextMemo.CenterPrefix == origin.CenterPrefix;
            IsSame_TopPrefix = SettingTextMemo.TopPrefix == origin.TopPrefix;
            IsSame_BottomPrefix = SettingTextMemo.BottomPrefix == origin.BottomPrefix;
            IsSame = IsSame_CenterPrefix && IsSame_TopPrefix && IsSame_BottomPrefix;
        }

        public void UpdateDifference(Document doc, PBPASetting origin, bool needUserConfirm = true)
        {
            bool IsPunchUpdate = origin.TargetType_Punch;
            bool IsBranchPipeUpdate = origin.TargetType_BranchPipe;
            CheckDifference(origin);
            if (!IsSame)
            {
                if (!needUserConfirm || VLViewModel.ShowQuestion("前缀发生变化,是否更新所对应的标签") == DialogResult.OK)
                {
                    SettingTextMemo = origin.CloneText();
                    VLTransactionHelper.DelegateTransaction(doc, "RegenerateAllFor_L", (Func<bool>)(() =>
                    {
                        var collection = PBPAContext.GetCollection(doc);
                        if (!IsSame_CenterPrefix)
                        {
                            if (IsPunchUpdate)
                            {
                                var targetType = PBPATargetType.Punch;
                                var locationType = PBPALocationType.Center;
                                RegenerateAllFor_Prefix(doc, collection, targetType, locationType);
                            }
                            if (IsBranchPipeUpdate)
                            {
                                var targetType = PBPATargetType.BranchPipe;
                                var locationType = PBPALocationType.Center;
                                RegenerateAllFor_Prefix(doc, collection, targetType, locationType);
                            }
                        }
                        if (!IsSame_TopPrefix)
                        {
                            if (IsPunchUpdate)
                            {
                                var targetType = PBPATargetType.Punch;
                                var locationType = PBPALocationType.Top;
                                RegenerateAllFor_Prefix(doc, collection, targetType, locationType);
                            }
                            if (IsBranchPipeUpdate)
                            {
                                var targetType = PBPATargetType.BranchPipe;
                                var locationType = PBPALocationType.Top;
                                RegenerateAllFor_Prefix(doc, collection, targetType, locationType);
                            }
                        }
                        if (!IsSame_BottomPrefix)
                        {
                            if (IsPunchUpdate)
                            {
                                var targetType = PBPATargetType.Punch;
                                var locationType = PBPALocationType.Bottom;
                                RegenerateAllFor_Prefix(doc, collection, targetType, locationType);
                            }
                            if (IsBranchPipeUpdate)
                            {
                                var targetType = PBPATargetType.BranchPipe;
                                var locationType = PBPALocationType.Bottom;
                                RegenerateAllFor_Prefix(doc, collection, targetType, locationType);
                            }
                        }
                        collection.Save(doc);
                        return true;
                    }));
                }
            }
            IsSame = true;
        }

        private void RegenerateAllFor_Prefix(Document doc, PBPAModelCollection collection, PBPATargetType targetType, PBPALocationType locationType)
        {
            string prefix = GetPrefix(targetType, locationType);
            var dataToChange = collection.Data.Where(c => c.TargetType == targetType && c.LocationType == locationType).ToList();
            for (int i = dataToChange.Count - 1; i >= 0; i--)
            {
                var model = dataToChange[i];
                model.Document = doc;
                model.IsRegenerate = true;
                model.AnnotationPrefix = prefix;
                var element = doc.GetElement(model.TargetId);
                element.GetParameters(PBPAContext.SharedParameterPL).FirstOrDefault().Set(model.GetFull_L(element));
                if (!PBPAContext.Creator.Regenerate(model))
                    collection.Data.Remove(model);
            }
        }

        private string GetPrefix(PBPATargetType targetType, PBPALocationType locationType)
        {
            string prefix = "";
            switch (locationType)
            {
                case PBPALocationType.Center:
                    prefix = SettingTextMemo.CenterPrefix;
                    break;
                case PBPALocationType.Top:
                    prefix = SettingTextMemo.TopPrefix;
                    break;
                case PBPALocationType.Bottom:
                    prefix = SettingTextMemo.BottomPrefix;
                    break;
                default:
                    break;
            }
            return prefix;
        }
    }

    public class PBPAViewModel : VLViewModel<PBPAModel, PBPAWindow>
    {
        PBPASetting Setting;
        PBPASettingMemo MemoHelper = new PBPASettingMemo();

        public PBPAViewModel(UIApplication app) : base(app)
        {
            Model = new PBPAModel("");
            View = new PBPAWindow(this);

            Setting = PBPAContext.GetSetting(Document);
            MemoHelper.SettingTextMemo = Setting.CloneText();
            TargetType_BranchPipe = Setting.TargetType_BranchPipe;
            TargetType_Punch = Setting.TargetType_Punch;
            AnnotationType = Setting.AnnotationType;
            LocationType = Setting.LocationType;
            CenterPrefix = Setting.CenterPrefix;
            TopPrefix = Setting.TopPrefix;
            BottomPrefix = Setting.BottomPrefix;
        }

        public bool Prepare()
        {
            if (!VLTransactionHelper.DelegateTransaction(Document, "PBPAViewModel", (Func<bool>)(() =>
            {
                //添加共享参数
                var parameterHelper = new ShareParameter(VLSharedParameterHelper.GetShareFilePath());
                parameterHelper.AddShadeParameter(Document, PBPAContext.SharedParameterGroupName, PBPAContext.SharedParameterPL, Document.Settings.Categories.get_Item(BuiltInCategory.OST_PipeAccessory), true, BuiltInParameterGroup.PG_TEXT);
                return true;
            })))
            {
                ShowMessage("加载功能所需的共享参数失败");
                return false;
            }
            return true;
        }

        public override void Execute()
        {
            PBPAModelCollection collection;
            var viewType = (PBPAViewType)Enum.Parse(typeof(PBPAViewType), ViewType.ToString());
            switch (viewType)
            {
                case PBPAViewType.Idle:
                    View = new PBPAWindow(this);
                    View.ShowDialog();
                    break;
                case PBPAViewType.Close:
                    View.Close();
                    SaveSetting();
                    break;
                case PBPAViewType.Closing:
                    SaveSetting();
                    break;
                case PBPAViewType.RegenerateAllFor_L:
                    VLTransactionHelper.DelegateTransaction(Document, "RegenerateAllFor_L", (Func<bool>)(() =>
                    {
                        collection = PBPAContext.GetCollection(Document);
                        for (int i = collection.Data.Count - 1; i >= 0; i--)
                        {
                            var model = collection.Data[i];
                            model.Document = Document;
                            model.IsRegenerate = true;

                            var element = Document.GetElement(model.TargetId);
                            element.GetParameters(PBPAContext.SharedParameterPL).FirstOrDefault().Set(model.GetFull_L(element));
                            if (!PBPAContext.Creator.Regenerate(model))
                                collection.Data.Remove(model);
                        }
                        collection.Save(Document);
                        return true;
                    }));
                    ViewType = (int)PBPAViewType.Idle;
                    break;
                case PBPAViewType.PickSingle_Target:
                    UpdateSetting();
                    MemoHelper.UpdateDifference(Document, Setting, false);
                    Model.Document = Document;
                    Model.ViewId = Document.ActiveView.Id;
                    View.Close();
                    if (!VLHookHelper.DelegateKeyBoardHook(() =>
                    {
                        //业务逻辑处理
                        //选择符合类型的过滤
                        UpdateModelTargetType();
                        var targetType = Model.GetFilter();
                        var obj = UIDocument.Selection.PickObject(ObjectType.Element, targetType, "请选择标注点");
                        if (obj != null)
                        {
                            Model.TargetId = obj.ElementId;
                            Model.TargetType = Model.IsPunch(Document.GetElement(obj.ElementId)) ? PBPATargetType.Punch : PBPATargetType.BranchPipe;
                            ViewType = (int)PBPAViewType.PickSingle_End;
                            Model.BodyStartPoint = obj.GlobalPoint;
                        }
                    }))
                        ViewType = (int)PBPAViewType.Idle;
                    //获取族内参数信息
                    if (!GetFamilySymbolInfo(Model.TargetId))
                    {
                        ShowMessage("加载族信息失败");
                        ViewType = (int)PBPAViewType.Idle;
                        Execute();
                        return;
                    }
                    Model.CurrentFontWidthSize = PBPAContext.FontManagement.CurrentFontWidthSize;
                    Execute();
                    break;
                case PBPAViewType.PickSingle_End:
                    //业务逻辑处理
                    if (!VLHookHelper.DelegateKeyBoardHook(() =>
                    {
                        //var locationCurve = (target.Location as LocationCurve).Curve as Line;
                        //XYZ vVector, pVector;
                        //VLLocationHelper.GetVectors(locationCurve, CoordinateType.XY, out vVector, out pVector);

                        #region 平面+立面支持
                        //CoordinateType coordinateType;
                        //coordinateType = VLLocationHelper.GetCoordinateType(Document);
                        //Model.CoordinateType = coordinateType;
                        //var hVector = coordinateType.GetParallelVector();
                        //hVector = VLLocationHelper.GetVectorByQuadrant(hVector, QuadrantType.OneAndFour, coordinateType);
                        //var vVector = VLLocationHelper.GetVerticalVector(hVector);
                        //vVector = VLLocationHelper.GetVectorByQuadrant(vVector, QuadrantType.OneAndFour, coordinateType); 
                        #endregion

                        VLCoordinateType coordinateType = VLCoordinateType.XY;
                        Model.UpdateVector(coordinateType);
                        var target = Document.GetElement(Model.TargetId);
                        Model.UpdateLineWidth(target);
                        var startPoint = Model.BodyStartPoint.ToWindowsPoint(coordinateType);
                        var offSet = (Model.LineWidth * Model.ParallelVector).ToWindowsPoint();
                        //var endPoint = (Model.BodyStartPoint + Model.LineWidth * 1.02 * Model.ParallelVector).ToWindowsPoint(coordinateType);
                        var pEnd = new VLPointPicker().PickPointWithPreview(UIApplication, coordinateType, (view) =>
                        {
                            var mousePosition = System.Windows.Forms.Control.MousePosition;
                            var midDrawP = new System.Windows.Point(mousePosition.X - view.Left, mousePosition.Y - view.Top);//中间选择点
                            var midPoint = view.ConvertToRevitPointFromDrawPoint(midDrawP);
                            var startDrawP = view.ConvertToDrawPointFromRevitPoint(startPoint);//起点
                            var M_S = midPoint - Model.BodyStartPoint;
                            if (Model.AnnotationType == PBPAAnnotationType.OneLine || Model.ParallelVector.CrossProductByCoordinateType(M_S, VLCoordinateType.XY) > 0)
                            {
                                var endPoint = startPoint.Plus(offSet);
                                var endP = view.ConvertToDrawPointFromRevitPoint(endPoint);//终点
                                if (Math.Abs(startDrawP.X - midDrawP.X) < 2 && Math.Abs(startDrawP.Y - midDrawP.Y) < 2)
                                    return;
                                var mid_s = midDrawP - startDrawP;
                                mid_s.Normalize();
                                var midSDrawP = midDrawP - mid_s * view.co_s;
                                var height = midDrawP - startDrawP;
                                var endDrawP = endP + height;
                                var mid_e = midDrawP - endDrawP;
                                mid_e.Normalize();
                                var midEDrawP = midDrawP - mid_e * view.co_e;
                                if (double.IsNaN(midEDrawP.X))
                                    return;

                                var canvas = view.canvas;
                                canvas.Children.RemoveRange(0, canvas.Children.Count);
                                var line = new System.Windows.Shapes.Line() { X1 = startDrawP.X, Y1 = startDrawP.Y, X2 = midSDrawP.X, Y2 = midSDrawP.Y, Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(136, 136, 136)), StrokeThickness = 1 };
                                var line2 = new System.Windows.Shapes.Line() { X1 = midEDrawP.X, Y1 = midEDrawP.Y, X2 = endDrawP.X, Y2 = endDrawP.Y, Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(136, 136, 136)), StrokeThickness = 1 };
                                System.Windows.Media.RenderOptions.SetBitmapScalingMode(line, System.Windows.Media.BitmapScalingMode.LowQuality);
                                System.Windows.Media.RenderOptions.SetBitmapScalingMode(line2, System.Windows.Media.BitmapScalingMode.LowQuality);
                                canvas.Children.Add(line);
                                canvas.Children.Add(line2);
                                Model.IsReversed = false;
                            }
                            else
                            {
                                var endPoint = startPoint.Minus(offSet);
                                var endP = view.ConvertToDrawPointFromRevitPoint(endPoint);//终点
                                if (Math.Abs(startDrawP.X - midDrawP.X) < 2 && Math.Abs(startDrawP.Y - midDrawP.Y) < 2)
                                    return;
                                var mid_s = midDrawP - startDrawP;
                                mid_s.Normalize();
                                var midSDrawP = midDrawP - mid_s * view.co_s;
                                var height = midDrawP - startDrawP;
                                var endDrawP = endP + height;
                                var mid_e = midDrawP - endDrawP;
                                mid_e.Normalize();
                                var midEDrawP = midDrawP - mid_e * view.co_e;
                                if (double.IsNaN(midEDrawP.X))
                                    return;

                                var canvas = view.canvas;
                                canvas.Children.RemoveRange(0, canvas.Children.Count);
                                var line = new System.Windows.Shapes.Line() { X1 = startDrawP.X, Y1 = startDrawP.Y, X2 = midSDrawP.X, Y2 = midSDrawP.Y, Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(136, 136, 136)), StrokeThickness = 1 };
                                var line2 = new System.Windows.Shapes.Line() { X1 = midEDrawP.X, Y1 = midEDrawP.Y, X2 = endDrawP.X, Y2 = endDrawP.Y, Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(136, 136, 136)), StrokeThickness = 1 };
                                System.Windows.Media.RenderOptions.SetBitmapScalingMode(line, System.Windows.Media.BitmapScalingMode.LowQuality);
                                System.Windows.Media.RenderOptions.SetBitmapScalingMode(line2, System.Windows.Media.BitmapScalingMode.LowQuality);
                                canvas.Children.Add(line);
                                canvas.Children.Add(line2);
                                Model.IsReversed = true;
                            }
                        });
                        if (pEnd == null)
                            ViewType = (int)PBPAViewType.Idle;
                        else
                        {
                            Model.BodyEndPoint = pEnd.ToSame(Model.BodyStartPoint, coordinateType);
                            ViewType = (int)PBPAViewType.GenerateSingle;
                        }
                    }))
                        ViewType = (int)PBPAViewType.Idle;
                    Execute();
                    break;
                case PBPAViewType.GenerateSingle:
                    //生成处理
                    if (VLTransactionHelper.DelegateTransaction(Document, "GenerateSingle", (Func<bool>)(() =>
                    {
                        #region 生成处理
                        collection = PBPAContext.GetCollection(Document);
                        var existedModels = collection.Data.Where(c => Model.TargetId == c.TargetId).ToList();//避免重复生成
                        if (existedModels != null)
                        {
                            for (int i = existedModels.Count() - 1; i >= 0; i--)
                            {
                                existedModels[i].Document = Document;
                                collection.Data.Remove(existedModels[i]);
                                existedModels[i].Clear();
                            }
                        }
                        if (!PBPAContext.Creator.Generate(Model))
                            return false;
                        collection.Data.Add(Model);
                        collection.Save(Document);
                        #endregion

                        #region 共享参数设置
                        var element = Document.GetElement(Model.TargetId);
                        element.GetParameters(PBPAContext.SharedParameterPL).FirstOrDefault().Set(Model.GetFull_L(element));
                        #endregion
                        return true;
                    })))
                        ViewType = (int)PBPAViewType.PickSingle_Target;
                    else
                        ViewType = (int)PBPAViewType.Idle;
                    Execute();
                    break;
                case PBPAViewType.GenerateAll:
                default:
                    throw new NotImplementedException("功能未实现");
            }
        }

        private void SaveSetting()
        {
            VLTransactionHelper.DelegateTransaction(Document, "Close", (Func<bool>)(() =>
            {
                Setting.TargetType_BranchPipe = TargetType_BranchPipe;
                Setting.TargetType_Punch = TargetType_Punch;
                Setting.AnnotationType = AnnotationType;
                Setting.LocationType = LocationType;
                Setting.BottomPrefix = BottomPrefix;
                Setting.CenterPrefix = CenterPrefix;
                Setting.TopPrefix = TopPrefix;
                PBPAContext.SaveSetting(Document);
                return true;
            }));
        }

        private void UpdateSetting()
        {
            Setting.TargetType_BranchPipe = TargetType_BranchPipe;
            Setting.TargetType_Punch = TargetType_Punch;
            Setting.AnnotationType = AnnotationType;
            Setting.LocationType = LocationType;
            Setting.BottomPrefix = BottomPrefix;
            Setting.CenterPrefix = CenterPrefix;
            Setting.TopPrefix = TopPrefix;
        }

        private bool GetFamilySymbolInfo(ElementId targetId)
        {
            FamilySymbol annotationFamily = null;
            if (!VLTransactionHelper.DelegateTransaction(Document, "GetFamilySymbolInfo", (Func<bool>)(() =>
            {
                annotationFamily = Model.GetAnnotationFamily(Document);
                return annotationFamily != null;
            })))
                return false;
            //准备族内参数
            if (!PBPAContext.FontManagement.IsCurrentFontSettled)
            {
                var familyDoc = Document.EditFamily(annotationFamily.Family);
                var textElement = new FilteredElementCollector(familyDoc).OfClass(typeof(TextElement)).First(c => c.Name == "3mm") as TextElement;
                var textElementType = textElement.Symbol as TextElementType;

#if Revit2016
                PBPAContext.FontManagement.SetCurrentFont(textElementType, 1, 1.2);
#elif Revit2017
                PBPAContext.FontManagement.SetCurrentFont(textElementType, 1, 1.55);
#else
                PBPAContext.FontManagement.SetCurrentFont(textElementType, 1, 1.6);
#endif
            }
            return true;
        }
        #region RatioButtons

        #region PBPATargetType 标注对象

        private void UpdateModelTargetType()
        {
            var punch = TargetType_Punch ? PBPATargetType.Punch : PBPATargetType.None;
            var branchPipe = TargetType_BranchPipe ? PBPATargetType.BranchPipe : PBPATargetType.None;
            Model.TargetType = punch | branchPipe;
        }

        private bool targetType_Punch;
        public bool TargetType_Punch
        {
            get
            {
                return targetType_Punch;
            }

            set
            {
                targetType_Punch = value;
                UpdateModelTargetType();
            }
        }

        private bool targetType_BranchPipe;
        public bool TargetType_BranchPipe
        {
            get
            {
                return targetType_BranchPipe;
            }

            set
            {
                targetType_BranchPipe = value;
                UpdateModelTargetType();
            }
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
                RaisePropertyChanged("AnnotationType_TwoLine");
                RaisePropertyChanged("AnnotationType_OneLine");
            }
        }
        public bool AnnotationType_TwoLine
        {
            get { return AnnotationType == PBPAAnnotationType.TwoLine; }
            set { if (value) AnnotationType = PBPAAnnotationType.TwoLine; }
        }
        public bool AnnotationType_OneLine
        {
            get { return AnnotationType == PBPAAnnotationType.OneLine; }
            set { if (value) AnnotationType = PBPAAnnotationType.OneLine; }
        }
        #endregion

        #region PBPALocationType
        private void UpdateModelAnnotationPrefix()
        {
            if (Model.LocationType == PBPALocationType.Center)
                Model.AnnotationPrefix = CenterPrefix;
            else if (Model.LocationType == PBPALocationType.Top)
                Model.AnnotationPrefix = TopPrefix;
            else if (Model.LocationType == PBPALocationType.Bottom)
                Model.AnnotationPrefix = BottomPrefix;
        }

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
        #endregion

        #endregion

    }
}
