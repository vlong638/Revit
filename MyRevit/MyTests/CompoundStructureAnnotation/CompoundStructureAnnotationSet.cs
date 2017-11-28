using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using System;
using System.Linq;
using MyRevit.MyTests.CompoundStructureAnnotation;
using System.Windows.Interop;
using System.Collections.Generic;

/// <summary>
/// 结构做法标注
/// 作者:夏锦慧
/// 创建日期:  2017/09/06 15:24 
/// 修改日期:
/// </summary>
namespace PmSoft.Optimization.DrawingProduction
{
    /// <summary>
    /// 命令对象
    /// </summary>
    public class CompoundStructureAnnotationSet : OptimizationCToolCmd
    {
        public UIApplication UIApplication;
        public Document Document { get { return UIApplication.ActiveUIDocument.Document; } }
        public CSAViewModel ViewModel { set; get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="app"></param>
        public CompoundStructureAnnotationSet(UIApplication app) : base(app)
        {
            Init(app);
            ViewModel = new CSAViewModel(app);
        }

        /// <summary>
        /// 构造函数.初始化基于UIApplication的内容
        /// </summary>
        /// <param name="app"></param>
        private void Init(UIApplication app)
        {
            UIApplication = app;
        }

        protected override bool Analyse()
        {
            //结果分析
            return true;
        }

        protected override bool DoUI()
        {
            var uiApp = UIApplication;
            var app = UIApplication.Application;
            var uiDoc = UIApplication.ActiveUIDocument;
            var doc = UIApplication.ActiveUIDocument.Document;
            VLConstraintsForCSA.Doc = doc;
            try
            {
                CompoundStructureAnnotationWindow window = new CompoundStructureAnnotationWindow(ViewModel);
                while (ViewModel.ViewType != CSAViewType.Close)
                {
                    ViewModel.Execute();
                }
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
}
