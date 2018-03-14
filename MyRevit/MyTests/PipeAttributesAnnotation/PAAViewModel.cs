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
using PmSoft.Common.CommonClass;

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
        Closing = -1,//右上角或Alt+F4关闭
        Close = 0,//按钮关闭或ESC关闭
        Idle = 1,//闲置
        PickSinglePipe_Pipe,//选择单管 管道
        PickSinglePipe_Location,//选择单管 定位
        GenerateSinglePipe,//单管标注生成
        PickMultiplePipes,//选择多管
        PickSingleDuct,//选择风管
        PickSingleCableTray,//选择桥架
        RegenerateAllFor_L,//更新离地高度
    }

    public class PAASetting : VLModel
    {
        public PAATargetType TargetType = PAATargetType.Pipe;
        //Pipe
        public PAAAnnotationType Pipe_AnnotationType = PAAAnnotationType.SPL;
        public PAALocationType Pipe_LocationType = PAALocationType.Center;
        public PAATextType Pipe_TextType = PAATextType.Option1;
        public string Pipe_CenterPrefix = "CL+";
        public string Pipe_TopPrefix = "TL+";
        public string Pipe_BottomPrefix = "BL+";
        //Duct
        public PAAAnnotationType Duct_AnnotationType = PAAAnnotationType.SPL;
        public PAALocationType Duct_LocationType = PAALocationType.Center;
        public PAATextType Duct_TextType = PAATextType.Option1;
        public string Duct_CenterPrefix = "CL+";
        public string Duct_TopPrefix = "TL+";
        public string Duct_BottomPrefix = "BL+";
        //CableTray
        public PAAAnnotationType CableTray_AnnotationType = PAAAnnotationType.SPL;
        public PAALocationType CableTray_LocationType = PAALocationType.Bottom;
        public PAATextType CableTray_TextType = PAATextType.Option1;
        public string CableTray_CenterPrefix = "CL+";
        public string CableTray_TopPrefix = "TL+";
        public string CableTray_BottomPrefix = "BL+";

        public PAASetting(string data) : base(data)
        {
        }

        public override bool LoadData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return false;
            try
            {
                StringReader sr = new StringReader(data);
                TargetType = sr.ReadFormatStringAsEnum<PAATargetType>();
                Pipe_AnnotationType = sr.ReadFormatStringAsEnum<PAAAnnotationType>();
                Pipe_LocationType = sr.ReadFormatStringAsEnum<PAALocationType>();
                Pipe_TextType = sr.ReadFormatStringAsEnum<PAATextType>();
                Pipe_CenterPrefix = sr.ReadFormatString();
                Pipe_TopPrefix = sr.ReadFormatString();
                Pipe_BottomPrefix = sr.ReadFormatString();
                Duct_AnnotationType = sr.ReadFormatStringAsEnum<PAAAnnotationType>();
                Duct_LocationType = sr.ReadFormatStringAsEnum<PAALocationType>();
                Duct_TextType = sr.ReadFormatStringAsEnum<PAATextType>();
                Duct_CenterPrefix = sr.ReadFormatString();
                Duct_TopPrefix = sr.ReadFormatString();
                Duct_BottomPrefix = sr.ReadFormatString();
                CableTray_AnnotationType = sr.ReadFormatStringAsEnum<PAAAnnotationType>();
                CableTray_LocationType = sr.ReadFormatStringAsEnum<PAALocationType>();
                CableTray_TextType = sr.ReadFormatStringAsEnum<PAATextType>();
                CableTray_CenterPrefix = sr.ReadFormatString();
                CableTray_TopPrefix = sr.ReadFormatString();
                CableTray_BottomPrefix = sr.ReadFormatString();
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
            sb.AppendItem(TargetType);
            sb.AppendItem(Pipe_AnnotationType);
            sb.AppendItem(Pipe_LocationType);
            sb.AppendItem(Pipe_TextType);
            sb.AppendItem(Pipe_CenterPrefix);
            sb.AppendItem(Pipe_TopPrefix);
            sb.AppendItem(Pipe_BottomPrefix);
            sb.AppendItem(Duct_AnnotationType);
            sb.AppendItem(Duct_LocationType);
            sb.AppendItem(Duct_TextType);
            sb.AppendItem(Duct_CenterPrefix);
            sb.AppendItem(Duct_TopPrefix);
            sb.AppendItem(Duct_BottomPrefix);
            sb.AppendItem(CableTray_AnnotationType);
            sb.AppendItem(CableTray_LocationType);
            sb.AppendItem(CableTray_TextType);
            sb.AppendItem(CableTray_CenterPrefix);
            sb.AppendItem(CableTray_TopPrefix);
            sb.AppendItem(CableTray_BottomPrefix);
            return sb.ToString();
        }

        public PAASetting CloneText()
        {
            PAASetting newSetting = new PAASetting("");
            newSetting.Pipe_CenterPrefix = this.Pipe_CenterPrefix;
            newSetting.Pipe_TopPrefix = this.Pipe_TopPrefix;
            newSetting.Pipe_BottomPrefix = this.Pipe_BottomPrefix;
            newSetting.Duct_CenterPrefix = this.Duct_CenterPrefix;
            newSetting.Duct_TopPrefix = this.Duct_TopPrefix;
            newSetting.Duct_BottomPrefix = this.Duct_BottomPrefix;
            newSetting.CableTray_CenterPrefix = this.CableTray_CenterPrefix;
            newSetting.CableTray_TopPrefix = this.CableTray_TopPrefix;
            newSetting.CableTray_BottomPrefix = this.CableTray_BottomPrefix;
            return newSetting;
        }
    }

    public class PAASettingMemo
    {
        public PAASetting SettingTextMemo;
        public bool IsSame { set; get; }
        public bool IsSame_Pipe_CenterPrefix { set; get; }
        public bool IsSame_Pipe_TopPrefix { set; get; }
        public bool IsSame_Pipe_BottomPrefix { set; get; }
        public bool IsSame_Duct_CenterPrefix { set; get; }
        public bool IsSame_Duct_TopPrefix { set; get; }
        public bool IsSame_Duct_BottomPrefix { set; get; }
        public bool IsSame_CableTray_CenterPrefix { set; get; }
        public bool IsSame_CableTray_TopPrefix { set; get; }
        public bool IsSame_CableTray_BottomPrefix { set; get; }

        void CheckDifference(PAASetting origin)
        {
            IsSame_Pipe_CenterPrefix = SettingTextMemo.Pipe_CenterPrefix == origin.Pipe_CenterPrefix;
            IsSame_Pipe_TopPrefix = SettingTextMemo.Pipe_TopPrefix == origin.Pipe_TopPrefix;
            IsSame_Pipe_BottomPrefix = SettingTextMemo.Pipe_BottomPrefix == origin.Pipe_BottomPrefix;
            IsSame_Duct_CenterPrefix = SettingTextMemo.Duct_CenterPrefix == origin.Duct_CenterPrefix;
            IsSame_Duct_TopPrefix = SettingTextMemo.Duct_TopPrefix == origin.Duct_TopPrefix;
            IsSame_Duct_BottomPrefix = SettingTextMemo.Duct_BottomPrefix == origin.Duct_BottomPrefix;
            IsSame_CableTray_CenterPrefix = SettingTextMemo.CableTray_CenterPrefix == origin.CableTray_CenterPrefix;
            IsSame_CableTray_TopPrefix = SettingTextMemo.CableTray_TopPrefix == origin.CableTray_TopPrefix;
            IsSame_CableTray_BottomPrefix = SettingTextMemo.CableTray_BottomPrefix == origin.CableTray_BottomPrefix;
            IsSame = IsSame_Pipe_CenterPrefix && IsSame_Pipe_TopPrefix && IsSame_Pipe_BottomPrefix
                && IsSame_Duct_CenterPrefix && IsSame_Duct_TopPrefix && IsSame_Duct_BottomPrefix
                && IsSame_CableTray_CenterPrefix && IsSame_CableTray_TopPrefix && IsSame_CableTray_BottomPrefix;
        }

        public void UpdateDifference(Document doc, PAASetting origin, bool needUserConfirm = true)
        {
            CheckDifference(origin);
            if (!IsSame)
            {
                if (!needUserConfirm || VLViewModel.ShowQuestion("前缀发生变化,是否更新所对应的标签") == DialogResult.OK)
                {
                    SettingTextMemo = origin.CloneText();
                    VLTransactionHelper.DelegateTransaction(doc, "RegenerateAllFor_L", (Func<bool>)(() =>
                    {
                        var collection = PAAContext.GetCollection(doc);
                        if (!IsSame_Pipe_CenterPrefix)
                        {
                            var targetType = PAATargetType.Pipe;
                            var locationType = PAALocationType.Center;
                            RegenerateAllFor_Prefix(doc, collection, targetType, locationType);
                        }
                        if (!IsSame_Pipe_TopPrefix)
                        {
                            var targetType = PAATargetType.Pipe;
                            var locationType = PAALocationType.Top;
                            RegenerateAllFor_Prefix(doc, collection, targetType, locationType);
                        }
                        if (!IsSame_Pipe_BottomPrefix)
                        {
                            var targetType = PAATargetType.Pipe;
                            var locationType = PAALocationType.Bottom;
                            RegenerateAllFor_Prefix(doc, collection, targetType, locationType);
                        }
                        if (!IsSame_Duct_CenterPrefix)
                        {
                            var targetType = PAATargetType.Duct;
                            var locationType = PAALocationType.Center;
                            RegenerateAllFor_Prefix(doc, collection, targetType, locationType);
                        }
                        if (!IsSame_Duct_TopPrefix)
                        {
                            var targetType = PAATargetType.Duct;
                            var locationType = PAALocationType.Top;
                            RegenerateAllFor_Prefix(doc, collection, targetType, locationType);
                        }
                        if (!IsSame_Duct_BottomPrefix)
                        {
                            var targetType = PAATargetType.Duct;
                            var locationType = PAALocationType.Bottom;
                            RegenerateAllFor_Prefix(doc, collection, targetType, locationType);
                        }
                        if (!IsSame_CableTray_CenterPrefix)
                        {
                            var targetType = PAATargetType.CableTray;
                            var locationType = PAALocationType.Center;
                            RegenerateAllFor_Prefix(doc, collection, targetType, locationType);
                        }
                        if (!IsSame_CableTray_TopPrefix)
                        {
                            var targetType = PAATargetType.CableTray;
                            var locationType = PAALocationType.Top;
                            RegenerateAllFor_Prefix(doc, collection, targetType, locationType);
                        }
                        if (!IsSame_CableTray_BottomPrefix)
                        {
                            var targetType = PAATargetType.CableTray;
                            var locationType = PAALocationType.Bottom;
                            RegenerateAllFor_Prefix(doc, collection, targetType, locationType);
                        }
                        collection.Save(doc);
                        return true;
                    }));
                }
            }
            IsSame = true;
        }

        private void RegenerateAllFor_Prefix(Document doc, PAAModelCollection collection, PAATargetType targetType, PAALocationType locationType)
        {
            string prefix = GetPrefix(targetType, locationType);
            var dataToChange = collection.Data.Where(c => c.TargetType == targetType && c.LocationType == locationType).ToList();
            for (int i = dataToChange.Count - 1; i >= 0; i--)
            {
                var model = dataToChange[i];

                if (model.TargetType != PAATargetType.Pipe)
                {
                    model.Document = doc;
                    model.Clear();
                    model.AnnotationPrefix = prefix;
                    var element = doc.GetElement(model.TargetId);
                    element.GetParameters(PAAContext.SharedParameterPL).FirstOrDefault().Set(model.GetFull_L(element));
                    if (!PAAContext.Creator.Generate(model))
                        collection.Data.Remove(model);
                    continue;
                }
                model.RegenerateType = model.TargetIds == null ? RegenerateType.BySingle : RegenerateType.ByMultipleTarget;
                model.Document = doc;
                model.IsRegenerate = true;
                model.AnnotationPrefix = prefix;
                try
                {
                    switch (model.RegenerateType)
                    {
                        case RegenerateType.BySingle:
                            var element = doc.GetElement(model.TargetId);
                            element.GetParameters(PAAContext.SharedParameterPL).FirstOrDefault().Set(model.GetFull_L(element));
                            break;
                        case RegenerateType.ByMultipleTarget:
                        case RegenerateType.ByMultipleLine:
                            foreach (var TargetId in model.TargetIds)
                            {
                                element = doc.GetElement(TargetId);
                                element.GetParameters(PAAContext.SharedParameterPL).FirstOrDefault().Set(model.GetFull_L(element));
                            }
                            break;
                        default:
                            break;
                    }
                    if (!PAAContext.Creator.Regenerate(model))
                        collection.Data.Remove(model);
                }
                catch (Exception ex)
                {
                    collection.Data.Remove(model);
                }
            }
        }

        private string GetPrefix(PAATargetType targetType, PAALocationType locationType)
        {
            string prefix = "";
            switch (targetType)
            {
                case PAATargetType.Pipe:
                    switch (locationType)
                    {
                        case PAALocationType.Center:
                            prefix = SettingTextMemo.Pipe_CenterPrefix;
                            break;
                        case PAALocationType.Top:
                            prefix = SettingTextMemo.Pipe_TopPrefix;
                            break;
                        case PAALocationType.Bottom:
                            prefix = SettingTextMemo.Pipe_BottomPrefix;
                            break;
                        default:
                            break;
                    }
                    break;
                case PAATargetType.Duct:
                    switch (locationType)
                    {
                        case PAALocationType.Center:
                            prefix = SettingTextMemo.Duct_CenterPrefix;
                            break;
                        case PAALocationType.Top:
                            prefix = SettingTextMemo.Duct_TopPrefix;
                            break;
                        case PAALocationType.Bottom:
                            prefix = SettingTextMemo.Duct_BottomPrefix;
                            break;
                        default:
                            break;
                    }
                    break;
                case PAATargetType.CableTray:
                    switch (locationType)
                    {
                        case PAALocationType.Center:
                            prefix = SettingTextMemo.CableTray_CenterPrefix;
                            break;
                        case PAALocationType.Top:
                            prefix = SettingTextMemo.CableTray_TopPrefix;
                            break;
                        case PAALocationType.Bottom:
                            prefix = SettingTextMemo.CableTray_BottomPrefix;
                            break;
                        default:
                            break;
                    }
                    break;
                case PAATargetType.Conduit:
                default:
                    break;
            }
            return prefix;
        }
    }


    public class PAAViewModel : VLViewModel<PAAModel, PAAWindow>
    {
        public PAAViewModel(UIApplication app) : base(app)
        {
            Model = new PAAModel("");
            View = new PAAWindow(this);

            Setting = PAAContext.GetSetting(Document);
            MemoHelper.SettingTextMemo = Setting.CloneText();
            Model.TargetType = Setting.TargetType;
            LoadSettingByTargetType(TargetType);
            TargetType = Model.TargetType;
        }

        private void LoadSettingByTargetType(PAATargetType TargetType)
        {
            switch (TargetType)
            {
                case PAATargetType.Pipe:
                    AnnotationType = Setting.Pipe_AnnotationType;
                    LocationType = Setting.Pipe_LocationType;
                    CenterPrefix = Setting.Pipe_CenterPrefix;
                    TopPrefix = Setting.Pipe_TopPrefix;
                    BottomPrefix = Setting.Pipe_BottomPrefix;
                    TextType = Setting.Pipe_TextType;
                    break;
                case PAATargetType.Duct:
                    AnnotationType = Setting.Duct_AnnotationType;
                    LocationType = Setting.Duct_LocationType;
                    CenterPrefix = Setting.Duct_CenterPrefix;
                    TopPrefix = Setting.Duct_TopPrefix;
                    BottomPrefix = Setting.Duct_BottomPrefix;
                    TextType = Setting.Duct_TextType;
                    break;
                case PAATargetType.CableTray:
                    AnnotationType = Setting.CableTray_AnnotationType;
                    LocationType = Setting.CableTray_LocationType;
                    CenterPrefix = Setting.CableTray_CenterPrefix;
                    TopPrefix = Setting.CableTray_TopPrefix;
                    BottomPrefix = Setting.CableTray_BottomPrefix;
                    TextType = Setting.CableTray_TextType;
                    break;
                case PAATargetType.Conduit:
                default:
                    throw new NotImplementedException();
            }
        }

        PAASetting Setting;
        PAASettingMemo MemoHelper = new PAASettingMemo();

        private void SaveSetting()
        {
            VLTransactionHelper.DelegateTransaction(Document, "Close", (Func<bool>)(() =>
            {
                UpdateSetting();
                PAAContext.SaveSetting(Document);
                return true;
            }));
        }

        private void UpdateSetting()
        {
            Setting.TargetType = TargetType;
            switch (TargetType)
            {
                case PAATargetType.Pipe:
                    Setting.Pipe_AnnotationType = AnnotationType;
                    Setting.Pipe_LocationType = LocationType;
                    Setting.Pipe_TextType = TextType;
                    Setting.Pipe_CenterPrefix = CenterPrefix;
                    Setting.Pipe_TopPrefix = TopPrefix;
                    Setting.Pipe_BottomPrefix = BottomPrefix;
                    break;
                case PAATargetType.Duct:
                    Setting.Duct_AnnotationType = AnnotationType;
                    Setting.Duct_LocationType = LocationType;
                    Setting.Duct_TextType = TextType;
                    Setting.Duct_CenterPrefix = CenterPrefix;
                    Setting.Duct_TopPrefix = TopPrefix;
                    Setting.Duct_BottomPrefix = BottomPrefix;
                    break;
                case PAATargetType.CableTray:
                    Setting.CableTray_AnnotationType = AnnotationType;
                    Setting.CableTray_LocationType = LocationType;
                    Setting.CableTray_TextType = TextType;
                    Setting.CableTray_CenterPrefix = CenterPrefix;
                    Setting.CableTray_TopPrefix = TopPrefix;
                    Setting.CableTray_BottomPrefix = BottomPrefix;
                    break;
                case PAATargetType.Conduit:
                default:
                    throw new NotImplementedException();
            }
        }

        public bool Prepare()
        {
            if (!VLTransactionHelper.DelegateTransaction(Document, "GenerateSinglePipe", (Func<bool>)(() =>
            {
                //添加共享参数
                var parameterHelper = new MyRevit.Utilities.ShareParameter(VLSharedParameterHelper.GetShareFilePath());
                parameterHelper.AddShadeParameter(Document, PAAContext.SharedParameterGroupName, PAAContext.SharedParameterPL, Document.Settings.Categories.get_Item(BuiltInCategory.OST_PipeCurves), true, BuiltInParameterGroup.PG_TEXT);
                parameterHelper.AddShadeParameter(Document, PAAContext.SharedParameterGroupName, PAAContext.SharedParameterPL, Document.Settings.Categories.get_Item(BuiltInCategory.OST_DuctCurves), true, BuiltInParameterGroup.PG_TEXT);
                parameterHelper.AddShadeParameter(Document, PAAContext.SharedParameterGroupName, PAAContext.SharedParameterPL, Document.Settings.Categories.get_Item(BuiltInCategory.OST_CableTray), true, BuiltInParameterGroup.PG_TEXT);
                //parameterHelper.AddShadeParameter(Document, PAAContext.SharedParameterGroupName, PAAContext.SharedParameterPL, Document.Settings.Categories.get_Item(BuiltInCategory.OST_Conduit), true, BuiltInParameterGroup.PG_TEXT);
                return true;
            })))
            {
                ShowMessage("加载功能所需的共享参数失败");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 取共享文件的地址
        /// </summary>
        /// <returns></returns>
        public static string GetShareFilePath()
        {
            return ApplicationPath.GetParentPathOfCurrent + @"\sysdata\" + "PMSharedParameters.txt";
        }

        public override void Execute()
        {
            Model.Document = Document;
            Model.ViewId = Document.ActiveView.Id;
            PAAModelCollection collection;
            var viewType = (PAAViewType)Enum.Parse(typeof(PAAViewType), ViewType.ToString());
            switch (viewType)
            {
                case PAAViewType.Idle:
                    View = new PAAWindow(this);
                    View.ShowDialog();
                    break;
                case PAAViewType.Close:
                    View.Close();
                    SaveSetting();
                    break;
                case PAAViewType.Closing:
                    SaveSetting();
                    break;
                case PAAViewType.RegenerateAllFor_L:
                    VLTransactionHelper.DelegateTransaction(Document, "RegenerateAllFor_L", (Func<bool>)(() =>
                    {
                        collection = PAAContext.GetCollection(Document);
                        for (int i = collection.Data.Count - 1; i >= 0; i--)
                        {
                            var model = collection.Data[i];
                            if (model.TargetType != PAATargetType.Pipe)
                            {
                                model.Document = Document;
                                var element = Document.GetElement(model.TargetId);
                                element.GetParameters(PAAContext.SharedParameterPL).FirstOrDefault().Set(model.GetFull_L(element));
                                model.Clear();
                                if (!PAAContext.Creator.Generate(model))
                                    collection.Data.Remove(model);
                                continue;
                            }
                            model.RegenerateType = model.TargetIds == null ? RegenerateType.BySingle : RegenerateType.ByMultipleTarget;
                            model.Document = Document;
                            model.IsRegenerate = true;
                            try
                            {
                                switch (model.RegenerateType)
                                {
                                    case RegenerateType.BySingle:
                                        var element = Document.GetElement(model.TargetId);
                                        element.GetParameters(PAAContext.SharedParameterPL).FirstOrDefault().Set(model.GetFull_L(element));
                                        break;
                                    case RegenerateType.ByMultipleTarget:
                                    case RegenerateType.ByMultipleLine:
                                        foreach (var TargetId in model.TargetIds)
                                        {
                                            element = Document.GetElement(TargetId);
                                            element.GetParameters(PAAContext.SharedParameterPL).FirstOrDefault().Set(model.GetFull_L(element));
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                if (!PAAContext.Creator.Regenerate(model))
                                    collection.Data.Remove(model);
                            }
                            catch (Exception ex)
                            {
                                collection.Data.Remove(model);
                            }
                        }
                        collection.Save(Document);
                        return true;
                    }));
                    ViewType = (int)PAAViewType.Idle;
                    break;
                case PAAViewType.PickSinglePipe_Pipe:
                    UpdateSetting();
                    MemoHelper.UpdateDifference(Document, Setting, false);
                    View.Close();
                    if (!VLHookHelper.DelegateKeyBoardHook(() =>
                    {
                        //业务逻辑处理
                        //选择符合类型的过滤
                        var targetType = Model.GetFilter();
                        var obj = UIDocument.Selection.PickObject(ObjectType.Element, targetType, "请选择管道标注点");
                        if (obj != null)
                        {
                            Model.TargetId = obj.ElementId;
                            ViewType = (int)PAAViewType.PickSinglePipe_Location;
                            Model.BodyStartPoint = obj.GlobalPoint;
                        }
                    }))
                        ViewType = (int)PAAViewType.Idle;
                    //获取族内参数信息
                    if (!GetFamilySymbolInfo(Model.TargetId))
                    {
                        ShowMessage("加载族信息失败");
                        ViewType = (int)PAAViewType.Idle;
                        Execute();
                        return;
                    }
                    Model.CurrentFontHeight = PAAContext.FontManagement.CurrentHeight;
                    Model.CurrentFontSizeScale = PAAContext.FontManagement.CurrentFontSizeScale;
                    Model.CurrentFontWidthSize = PAAContext.FontManagement.CurrentFontWidthSize;
                    Execute();
                    break;
                case PAAViewType.PickSinglePipe_Location:
                    #region 业务逻辑处理
                    var target = Document.GetElement(Model.TargetId);
                    var locationCurve = TargetType.GetLine(target);
                    if (locationCurve.Direction.X.IsMiniValue() && locationCurve.Direction.Y.IsMiniValue())
                    {
                        ShowMessage("暂不支持立管");
                        throw new InvalidDataException();
                    }
                    Model.UpdateVectors(locationCurve);
                    Model.UpdateLineWidth(target);
                    var startPoint = Model.BodyStartPoint.ToWindowsPoint();
                    var offSet = (Model.LineWidth * Model.ParallelVector).ToWindowsPoint();
                    var pEnd = new VLPointPicker().PickPointWithPreview(UIApplication, VLCoordinateType.XY, (view) =>
                    {
                        var mousePosition = System.Windows.Forms.Control.MousePosition;
                        var midDrawP = new System.Windows.Point(mousePosition.X - view.Left, mousePosition.Y - view.Top);//中间选择点
                        var midPoint = view.ConvertToRevitPointFromDrawPoint(midDrawP);
                        var startDrawP = view.ConvertToDrawPointFromRevitPoint(startPoint);//起点
                        var M_S = midPoint - Model.BodyStartPoint;
                        if (Model.TextType == PAATextType.Option2 || Model.ParallelVector.CrossProductByCoordinateType(M_S, VLCoordinateType.XY) > 0)
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
                        ViewType = (int)PAAViewType.Idle;
                    else
                    {
                        Model.BodyEndPoint = pEnd.ToSameZ(Model.BodyStartPoint);
                        ViewType = (int)PAAViewType.GenerateSinglePipe;
                    }
                    #endregion
                    Execute();
                    break;
                case PAAViewType.GenerateSinglePipe:
                    //生成处理
                    if (VLTransactionHelper.DelegateTransaction(Document, "GenerateSinglePipe", (Func<bool>)(() =>
                    {
                        #region 重复检测                       
                        collection = PAAContext.GetCollection(Document);
                        var existedModels = collection.Data.Where(c => Model.TargetId == c.TargetId || (c.TargetIds != null && c.TargetIds.Contains(Model.TargetId))).ToList();//避免重复生成
                        if (existedModels != null)
                        {
                            for (int i = existedModels.Count() - 1; i >= 0; i--)
                            {
                                existedModels[i].Document = Document;
                                collection.Data.Remove(existedModels[i]);
                                existedModels[i].Clear();
                            }
                        }
                        #endregion

                        #region 共享参数设置
                        var element = Document.GetElement(Model.TargetId);
                        element.GetParameters(PAAContext.SharedParameterPL).FirstOrDefault().Set(Model.GetFull_L(element));
                        #endregion

                        #region 生成处理
                        Model.ModelType = PAAModelType.SinglePipe;
                        if (!PAAContext.Creator.Generate(Model))
                            return false;
                        collection.Data.Add(Model);
                        collection.Save(Document);
                        #endregion
                        return true;
                    })))
                        ViewType = (int)PAAViewType.PickSinglePipe_Pipe;
                    else
                        ViewType = (int)PAAViewType.Idle;
                    Execute();
                    break;
                case PAAViewType.PickMultiplePipes:
                    UpdateSetting();
                    MemoHelper.UpdateDifference(Document, Setting, false);
                    View.Close();
                    if (!VLHookHelper.DelegateKeyBoardHook(() =>
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
                                ViewType = (int)PAAViewType.Idle;
                                Execute();
                                return;
                            }
                            Model.TargetIds = new List<ElementId>();
                            Model.TargetIds.AddRange(targetElementIds);
                        }
                    }))
                    {
                        ViewType = (int)PAAViewType.Idle;
                        Execute();
                        return;
                    }
                    //平行检测
                    if (!Model.CheckParallel(Document))
                    {
                        ShowMessage("多管的管道需为平行的");
                        ViewType = (int)PAAViewType.Idle;
                        Execute();
                        return;
                    }
                    //获取族内参数信息
                    if (!GetFamilySymbolInfo(Model.TargetIds.First()))
                    {
                        ShowMessage("加载族信息失败");
                        ViewType = (int)PAAViewType.Idle;
                        Execute();
                        return;
                    }
                    Model.CurrentFontHeight = PAAContext.FontManagement.CurrentHeight;
                    Model.CurrentFontSizeScale = PAAContext.FontManagement.CurrentFontSizeScale;
                    Model.CurrentFontWidthSize = PAAContext.FontManagement.CurrentFontWidthSize;
                    Model.UpdateLineWidth(Document.GetElement(Model.TargetIds.First()));
                    //生成处理
                    if (!VLTransactionHelper.DelegateTransaction(Document, "PickMultiplePipes", (Func<bool>)(() =>
                    {
                        #region 重复检测
                        collection = PAAContext.GetCollection(Document);
                        var existedModels = collection.Data.Where(c => Model.TargetIds.Contains(c.TargetId) || (c.TargetIds != null && Model.TargetIds.Intersect(c.TargetIds, new ElementIdComparer()).Count() > 0)).ToList();//避免重复生成
                        if (existedModels != null)
                        {
                            for (int i = existedModels.Count() - 1; i >= 0; i--)
                            {
                                collection.Data.Remove(existedModels[i]);
                                existedModels[i].Document = Document;
                                existedModels[i].Clear();
                            }
                        }
                        #endregion

                        #region 生成处理
                        Model.ModelType = PAAModelType.MultiplePipe;
                        PAAContext.Creator.Generate(Model);
                        collection.Data.Add(Model);
                        collection.Save(Document);
                        #endregion

                        #region 共享参数设置
                        foreach (var TargetId in Model.TargetIds)
                        {
                            UpdateL(TargetId);
                        }
                        #endregion
                        return true;
                    })))
                        ViewType = (int)PAAViewType.Idle;
                    Execute();
                    break;
                case PAAViewType.PickSingleDuct:
                case PAAViewType.PickSingleCableTray:
                    UpdateSetting();
                    MemoHelper.UpdateDifference(Document, Setting, false);
                    View.Close();
                    #region 对象选择及必要族信息加载
                    if (!VLHookHelper.DelegateKeyBoardHook(() =>
                    {
                        //业务逻辑处理
                        //选择符合类型的过滤
                        var targetType = Model.GetFilter();
                        var targetElementId = UIDocument.Selection.PickObject(ObjectType.Element, targetType, "请选择需要标注的对象");
                        if (targetElementId != null)
                        {
                            Model.TargetId = targetElementId.ElementId;
                            locationCurve = TargetType.GetLine(Document.GetElement(Model.TargetId));
                            if (locationCurve.Direction.X.IsMiniValue() && locationCurve.Direction.Y.IsMiniValue())
                            {
                                ShowMessage("暂不支持立管");
                                throw new InvalidDataException();
                            }
                        }
                    }))
                    {
                        ViewType = (int)PAAViewType.Idle;
                        Execute();
                        return;
                    }
                    //获取族内参数信息
                    if (!GetFamilySymbolInfo(Model.TargetId))
                    {
                        ShowMessage("加载族信息失败");
                        ViewType = (int)PAAViewType.Idle;
                        Execute();
                        return;
                    }
                    else
                    {
                        Model.CurrentFontHeight = PAAContext.FontManagement.CurrentHeight;
                        Model.CurrentFontSizeScale = PAAContext.FontManagement.CurrentFontSizeScale;
                        Model.CurrentFontWidthSize = PAAContext.FontManagement.CurrentFontWidthSize;
                    }
                    #endregion

                    if (!VLTransactionHelper.DelegateTransaction(Document, "PickSingleDuct", (Func<bool>)(() =>
                    {
                        #region 重复检测                       
                        collection = PAAContext.GetCollection(Document);
                        var existedModel = collection.Data.FirstOrDefault(c => Model.TargetId == c.TargetId);//避免重复生成
                        if (existedModel != null)
                        {
                            existedModel.Document = Document;
                            existedModel.Clear();
                        }
                        #endregion

                        #region 生成处理
                        Model.ModelType = PAAModelType.SingleDuct;
                        PAAContext.Creator.Generate(Model);
                        UpdateL(Model.TargetId);
                        collection.Data.Add(Model);
                        collection.Save(Document);
                        #endregion

                        return true;
                    })))
                    {
                        ViewType = (int)PAAViewType.Idle;
                        Execute();
                        return;
                    }
                    Execute();
                    break;
                default:
                    throw new NotImplementedException("功能未实现");
            }
        }

        private void UpdateL(ElementId TargetId)
        {
            var element = Document.GetElement(TargetId);
            if (element.GetParameters(PAAContext.SharedParameterPL).Count == 0)
            {
                var parameterHelper = new MyRevit.Utilities.ShareParameter(VLSharedParameterHelper.GetShareFilePath());
                parameterHelper.AddShadeParameter(Document, PAAContext.SharedParameterGroupName, PAAContext.SharedParameterPL, element.Category, true, BuiltInParameterGroup.PG_TEXT);
            }
            element.GetParameters(PAAContext.SharedParameterPL).FirstOrDefault().Set(Model.GetFull_L(element));
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
#if Revit2016
                PAAContext.FontManagement.SetCurrentFont(textElementType, 1, 1.1);
#elif Revit2017
                PAAContext.FontManagement.SetCurrentFont(textElementType, 1, 1.4);
#else
                PAAContext.FontManagement.SetCurrentFont(textElementType, 1, 1.45);
#endif
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
                UpdateSetting();
                Model.TargetType = value;
                MemoHelper.UpdateDifference(Document, Setting);
                switch (value)
                {
                    case PAATargetType.Pipe:
                        TextType_Option1_Title = "文字在线上";
                        TextType_Option2_Title = "文字在线端";
                        Btn_Left_Title = "单管标注";
                        Btn_Right_Title = "多管标注";
                        break;
                    case PAATargetType.Duct:
                        TextType_Option1_Title = "位于风管中心";
                        TextType_Option2_Title = "位于风管上方";
                        Btn_Left_Title = "选管标注";
                        Btn_Right_Title = "取消";
                        break;
                    case PAATargetType.CableTray:
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
                LoadSettingByTargetType(Model.TargetType);
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

        internal void UpdateViewType(int option)
        {
            if (option == 3)
            {
                ViewType = (int)PAAViewType.RegenerateAllFor_L;
                return;
            }

            switch (TargetType)
            {
                case PAATargetType.Pipe:
                    if (option == 1)
                        ViewType = (int)PAAViewType.PickSinglePipe_Pipe;
                    else
                        ViewType = (int)PAAViewType.PickMultiplePipes;
                    break;
                case PAATargetType.Duct:
                    if (option == 1)
                        ViewType = (int)PAAViewType.PickSingleDuct;
                    else
                        ViewType = (int)PAAViewType.Close;
                    break;
                case PAATargetType.CableTray:
                    if (option == 1)
                        ViewType = (int)PAAViewType.PickSingleCableTray;
                    else
                        ViewType = (int)PAAViewType.Close;
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
                RaisePropertyChanged("CenterPrefix");
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
                RaisePropertyChanged("TopPrefix");
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
                RaisePropertyChanged("BottomPrefix");
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
