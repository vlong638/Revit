using System;
using Autodesk.Revit.UI;
using MyRevit.MyTests.VLBase;
using MyRevit.Utilities;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace MyRevit.MyTests.Analysis
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

    public enum AnalysisViewType
    {
        Idle,//闲置
        Close,//关闭
        DisplayGeometry,
    }

    public class AnalysisViewModel : VLViewModel<AnalysisModel, AnalysisWindow>
    {
        public AnalysisViewModel(UIApplication app) : base(app)
        {
            VLTransactionHelper.DelegateTransaction(Document, "AnalysisContext", (() =>
            {
                Model = AnalysisContext.GetCollection(Document);
                return true;
            }));
            View = new AnalysisWindow(this);
            ////用以打开时更新页面
            //AnnotationType = AnalysisAnnotationType.SPL;
            //LocationType = AnalysisLocationType.Center;
        }
        
        public override void Execute()
        {
            var viewType = (AnalysisViewType)Enum.Parse(typeof(AnalysisViewType), ViewType.ToString());
            switch (viewType)
            {
                case AnalysisViewType.Idle:
                    View = new AnalysisWindow(this);
                    View.ShowDialog();
                    break;
                case AnalysisViewType.Close:
                    View.Close();
                    break;
                case AnalysisViewType.DisplayGeometry:
                    View.Close();
                    if (!VLMouseHookHelper.DelegateKeyBoardHook(() =>
                    {
                        Model.TargetId = UIDocument.Selection.PickObject(ObjectType.Element, "请选择对象").ElementId;
                    }))
                    {
                        Model.TargetId = null;
                        ViewType = (int)AnalysisViewType.Idle;
                    }
                    if (VLTransactionHelper.DelegateTransaction(Document, "DisplayGeometry", () =>
                    {
                        if (Model.TargetId == null)
                        {
                            return false;
                        }
                        if (string.IsNullOrEmpty(RootPath))
                        {
                            MessageHelper.TaskDialogShow("根路径不能为空");
                            return false;
                        }
                        Model.Document = Document;
                        Model.RootPath = RootPath;
                        Model.DisplayName = ViewType.ToString();
                        Model.DisplayGeometry();
                        AnalysisContext.SaveCollection(Document);
                        return true;
                    }))
                        ViewType = (int)AnalysisViewType.DisplayGeometry;
                    else
                        ViewType = (int)AnalysisViewType.Idle;
                    Execute();
                    break;
                default:
                    throw new NotImplementedException("功能未实现");
            }
        }

        #region RatioButtons
        
        public string RootPath { set { Model.RootPath = value; } get { return Model.RootPath; } }

        #endregion
    }
}
