using PmSoft.Common.Controls.RevitMethod;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using PmSoft.Common.RevitClass.WorkViewCheck;

/// <summary>
/// 多管集中标注
/// 作者:夏锦慧
/// 创建日期: 2017/06/20 15:53
/// 修改日期:
/// </summary>
namespace PmSoft.Optimization.DrawingProduction
{
    public abstract class OptimizationCToolCmd : PmSoft.MainModel.EntData.CToolCmd
    {
        public OptimizationCToolCmd(ExternalCommandData revit)
            : base(revit)
        {
            if (revit.Application.ActiveUIDocument.ActiveGraphicalView.Id != revit.Application.ActiveUIDocument.ActiveView.Id)
                revit.Application.ActiveUIDocument.ActiveView = revit.Application.ActiveUIDocument.ActiveGraphicalView;
        }

        public OptimizationCToolCmd(UIApplication app)
            : base(app)
        {
            if (app.ActiveUIDocument.ActiveGraphicalView.Id != app.ActiveUIDocument.ActiveView.Id)
                app.ActiveUIDocument.ActiveView = app.ActiveUIDocument.ActiveGraphicalView;
        }
    }

    /// <summary>
    /// 命令对象
    /// </summary>
    public class PipeAnnotationCmd : OptimizationCToolCmd
    {
        public UIApplication UIApplication;
        public Document Document { get { return UIApplication.ActiveUIDocument.Document; } }
        public PipeAnnotationUIData PipeAnnotationUIData;



        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="revit"></param>
        public PipeAnnotationCmd(ExternalCommandData revit) : base(revit)
        {
            Init(revit.Application);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="app"></param>
        public PipeAnnotationCmd(UIApplication app) : base(app)
        {
            Init(app);
        }

        /// <summary>
        /// 构造函数.初始化基于UIApplication的内容
        /// </summary>
        /// <param name="app"></param>
        private void Init(UIApplication app)
        {
            UIApplication = app;
            PipeAnnotationUIData = new PipeAnnotationUIData();
            PipeAnnotationUIData.LoadConfig(UIApplication.ActiveUIDocument.Document);
        }

        protected override bool Analyse()
        {
            //结果分析
            return true;
        }

        protected override bool DoUI()
        {
            if (!PipeAnnotationUIData.IsSuccess)
            {
                TaskDialog.Show("警告", "所需的族加载失败");
                return false;
            }

            try
            {
                List<ViewType> viewTypeList = new List<ViewType>();
                viewTypeList.Add(ViewType.FloorPlan);
                viewTypeList.Add(ViewType.EngineeringPlan);
                if (!viewTypeList.Contains(m_doc.ActiveView.ViewType))
                {
                    ViewPlanForm planeView = new ViewPlanForm(base.m_revit.Application.ActiveUIDocument, viewTypeList);
                    if (planeView.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        return true;
                    }
                }
                PipeAnnotationForm form = new PipeAnnotationForm(this);
                Common.RevitClass.PickObjectsMouseHook mouseHook = new Common.RevitClass.PickObjectsMouseHook();
                System.Windows.Forms.DialogResult result = System.Windows.Forms.DialogResult.Retry;
                while ((result = form.ShowDialog(new RevitHandle(Process.GetCurrentProcess().MainWindowHandle))) == System.Windows.Forms.DialogResult.Retry)
                {
                    switch (form.ActionType)
                    {
                        case ActionType.SelectSinglePipe:
                            try
                            {
                                mouseHook.InstallHook();
                                var selectedId = m_uiDoc.Selection.PickObject(ObjectType.Element, new PipeFramingFilter()).ElementId;
                                form.SelectedElementIds.Clear();
                                form.SelectedElementIds.Add(selectedId);
                                form.FinishSelection();
                            }
                            catch (Exception ex)
                            {
                                mouseHook.UninstallHook();
                            }
                            mouseHook.UninstallHook();
                            break;
                        case ActionType.SelectMultiplePipe:
                            try
                            {
                                mouseHook.InstallHook();
                                var selectedIds = m_uiDoc.Selection.PickObjects(ObjectType.Element, new PipeFramingFilter()).Select(c => c.ElementId);
                                form.SelectedElementIds.Clear();
                                form.SelectedElementIds.AddRange(selectedIds);
                                form.FinishSelection();
                            }
                            catch (Exception ex)
                            {
                                mouseHook.UninstallHook();
                            }
                            mouseHook.UninstallHook();
                            break;
                        default:
                            break;
                    }
                }
                //if (result == System.Windows.Forms.DialogResult.Cancel)
                //    return false;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 预处理
        /// </summary>
        protected override void Reset()
        {
        }
    }

    /// <summary>
    /// 梁.选择过滤器
    /// </summary>
    public class PipeFramingFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            var doc = elem.Document;
            var category = Category.GetCategory(doc, BuiltInCategory.OST_PipeCurves);
            return elem.Category.Id == category.Id;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
