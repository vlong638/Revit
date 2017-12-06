using System;
using Autodesk.Revit.UI;
using MyRevit.MyTests.VLBase;
using MyRevit.Utilities;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using PmSoft.Common.RevitClass.Utils;

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
        Idle,//闲置
        Close,//关闭
        PickSingle_Target,//选择引注_起点
        PickSingle_End,//选择引注_终点
        GenerateSingle,
        GenerateAll,//一键引注
    }

    public class PBPAViewModel : VLViewModel<PBPAModel, PBPAWindow, PBPAViewType>
    {
        public PBPAViewModel(UIApplication app) : base(app)
        {
            Model = new PBPAModel("");
            View = new PBPAWindow(this);
            //用以打开时更新页面
            TargetType_BranchPipe = true;
            TargetType_Punch = true;
            AnnotationType = PBPAAnnotationType.TwoLine;
            LocationType = PBPALocationType.Center;
        }

        public bool Prepare()
        {
            if (!VLTransactionHelper.DelegateTransaction(Document, "PBPAViewModel", (Func<bool>)(() =>
            {
                //添加共享参数
                string shareFilePath = @"E:\WorkingSpace\Tasks\1101管道特性标注\PMSharedParameters.txt";
                var parameterHelper = new ShareParameter(shareFilePath);
                parameterHelper.AddShadeParameter(Document, PBPAContext.SharedParameterGroupName, PBPAContext.SharedParameterPL, Document.Settings.Categories.get_Item(BuiltInCategory.OST_PipeAccessory), true, BuiltInParameterGroup.PG_TEXT);
                return true;
            })))
            {
                ShowMessage("加载功能所需的共享参数失败");
                return false;
            }
            return true;
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
                case PBPAViewType.PickSingle_Target:
                    Model.Document = Document;
                    Model.ViewId = Document.ActiveView.Id;
                    View.Close();
                    if (!VLMouseHookHelper.DelegateMouseHook(() =>
                    {
                        //业务逻辑处理
                        //选择符合类型的过滤
                        var targetType = Model.GetFilter();
                        var obj = UIDocument.Selection.PickObject(ObjectType.Element, targetType, "请选择标注点");
                        if (obj != null)
                        {
                            Model.TargetId = obj.ElementId;
                            ViewType = PBPAViewType.PickSingle_End;
                            Model.BodyStartPoint = obj.GlobalPoint;
                        }
                    }))
                        ViewType = PBPAViewType.Idle;
                    //获取族内参数信息
                    if (!GetFamilySymbolInfo(Model.TargetId))
                    {
                        ShowMessage("加载族文字信息失败");
                        ViewType = PBPAViewType.Idle;
                        Execute();
                        return;
                    }
                    Model.CurrentFontWidthSize = PBPAContext.FontManagement.CurrentFontWidthSize;
                    Execute();
                    break;
                case PBPAViewType.PickSingle_End:
                    //业务逻辑处理
                    if (!VLMouseHookHelper.DelegateMouseHook(() =>
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

                        CoordinateType coordinateType = CoordinateType.XY;
                        Model.UpdateVector(coordinateType);
                        var target = Document.GetElement(Model.TargetId);
                        Model.UpdateLineWidth(target);
                        var startPoint = Model.BodyStartPoint.ToWindowsPoint(coordinateType);
                        var endPoint = (Model.BodyStartPoint + Model.LineWidth * 1.02 * Model.ParallelVector).ToWindowsPoint(coordinateType);
                        var pEnd = new VLPointPicker().PickPointWithPreview(UIApplication, coordinateType, (view) =>
                        {
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
                        });
                        if (pEnd == null)
                            ViewType = PBPAViewType.Idle;
                        else
                        {
                            Model.BodyEndPoint = pEnd.ToSame(Model.BodyStartPoint, coordinateType);
                            ViewType = PBPAViewType.GenerateSingle;
                        }
                    }))
                        ViewType = PBPAViewType.Idle;
                    Execute();
                    break;
                case PBPAViewType.GenerateSingle:
                    //生成处理
                    if (VLTransactionHelper.DelegateTransaction(Document, "GenerateSingle", (Func<bool>)(() =>
                    {
                        #region 生成处理
                        var Collection = PBPAContext.GetCollection(Document);
                        var existedModels = Collection.Data.Where(c => Model.TargetId == c.TargetId).ToList();//避免重复生成
                        if (existedModels != null)
                        {
                            for (int i = existedModels.Count() - 1; i >= 0; i--)
                            {
                                existedModels[i].Document = Document;
                                Collection.Data.Remove(existedModels[i]);
                                existedModels[i].Clear();
                            }
                        }
                        if (!PBPAContext.Creator.Generate(Model))
                            return false;
                        Collection.Data.Add(Model);
                        Collection.Save(Document);
                        #endregion

                        #region 共享参数设置
                        var element = Document.GetElement(Model.TargetId);
                        element.GetParameters(PBPAContext.SharedParameterPL).FirstOrDefault().Set(Model.GetFull_L(element));
                        #endregion
                        return true;
                    })))
                        ViewType = PBPAViewType.PickSingle_Target;
                    else
                        ViewType = PBPAViewType.Idle;
                    Execute();
                    break;
                case PBPAViewType.GenerateAll:
                default:
                    throw new NotImplementedException("功能未实现");
            }
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
                PBPAContext.FontManagement.SetCurrentFont(textElementType);
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

        #endregion

    }
}
