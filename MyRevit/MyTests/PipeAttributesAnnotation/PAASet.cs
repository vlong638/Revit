using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PmSoft.Optimization.DrawingProduction;
using System;

namespace MyRevit.MyTests.PAA
{
    /// <summary>
    /// 命令对象
    /// </summary>
    public class PAASet : OptimizationCToolCmd
    {

        public UIApplication UIApplication;
        public Document Document { get { return UIApplication.ActiveUIDocument.Document; } }
        public PAAViewModel ViewModel { set; get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="app"></param>
        public PAASet(UIApplication app) : base(app)
        {
            Init(app);
            ViewModel = new PAAViewModel(app);
        }
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
            try
            {
                ViewModel.Execute();
                return true;
            }
            catch (Exception ex)
            {
                //TODO Log
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
