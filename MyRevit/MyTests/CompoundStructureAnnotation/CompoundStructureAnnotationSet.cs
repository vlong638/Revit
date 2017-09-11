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

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="app"></param>
        public CompoundStructureAnnotationSet(UIApplication app) : base(app)
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

            //CompoundStructureAnnotationWindow window = new CompoundStructureAnnotationWindow(this);
            //var model = window.ViewModel.Model;
            //PmSoft.Common.RevitClass.PickObjectsMouseHook mouseHook = new Common.RevitClass.PickObjectsMouseHook();
            //while (window.ViewModel.ViewType != CompoundStructureAnnotationViewType.Close)
            //{
            //    try
            //    {
            //        switch (window.ViewModel.ViewType)
            //        {
            //            case CompoundStructureAnnotationViewType.Idle:
            //                break;
            //            case CompoundStructureAnnotationViewType.Select:
            //                mouseHook.InstallHook(Common.RevitClass.PickObjectsMouseHook.OKModeENUM.Objects);
            //                Document linkDocument = null;
            //                IEnumerable<ElementId> elementIds = null;
            //                //选择
            //                if (model.ContentType == ContentType.Document)
            //                {
            //                    elementIds = uiDoc.Selection.PickObjects(ObjectType.Element, new CategoryFilter(BuiltInCategory.OST_Floors), "选择楼板").Select(c => c.ElementId);
            //                    if (elementIds == null || elementIds.Count() == 0)
            //                        break;
            //                }
            //                else
            //                {
            //                    var linkFilter = new CategoryFilter(BuiltInCategory.OST_Floors, true);
            //                    Reference reference = uiDoc.Selection.PickObject(ObjectType.LinkedElement, linkFilter, "先选择一个链接文件");
            //                    Element element = doc.GetElement(reference.ElementId);
            //                    if (element.Category.Name != "RVT 链接")
            //                        break;
            //                    linkDocument = (element as RevitLinkInstance).GetLinkDocument();
            //                    elementIds = uiDoc.Selection.PickObjects(ObjectType.LinkedElement, linkFilter, "在链接文件中选择板:").Select(c => c.LinkedElementId);
            //                    model.LinkDocument = linkDocument;
            //                    model.Offset = (element as Instance).GetTotalTransform().Origin;
            //                }
            //                //状态更改
            //                if (elementIds == null || elementIds.Count() == 0)
            //                {
            //                    window.ViewModel.ViewType = ViewTypeOfBeamAlignToFloor.Idle;
            //                }
            //                else
            //                {
            //                    window.ViewModel.Model.FloorIds = elementIds;
            //                    window.ViewModel.ViewType = ViewTypeOfBeamAlignToFloor.SelectBeam;
            //                }
            //                break;
            //            case ViewTypeOfBeamAlignToFloor.SelectBeam:
            //                mouseHook.InstallHook(Common.RevitClass.PickObjectsMouseHook.OKModeENUM.Objects);
            //                elementIds = uiDoc.Selection.PickObjects(ObjectType.Element, new CategoryFilter(BuiltInCategory.OST_StructuralFraming)).Select(c => c.ElementId);
            //                //状态更改
            //                if (elementIds == null || elementIds.Count() == 0)
            //                {
            //                    window.ViewModel.ViewType = ViewTypeOfBeamAlignToFloor.Idle;
            //                }
            //                else
            //                {
            //                    window.ViewModel.Model.BeamIds = elementIds;
            //                    window.ViewModel.ViewType = ViewTypeOfBeamAlignToFloor.BeamAlignToFloor;
            //                }
            //                break;
            //            case ViewTypeOfBeamAlignToFloor.BeamAlignToFloor:
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        mouseHook.UninstallHook();
            //        window.ViewModel.ViewType = ViewTypeOfBeamAlignToFloor.Idle;
            //    }
            //    mouseHook.UninstallHook();
            //    window.Work();
            //}
            //window.Close();



            try
            {
                CompoundStructureAnnotationWindow window = new CompoundStructureAnnotationWindow(this);
                IntPtr rvtPtr = Autodesk.Windows.ComponentManager.ApplicationWindow;
                WindowInteropHelper helper = new WindowInteropHelper(window);
                helper.Owner = rvtPtr;
                window.ShowDialog();
                while (window.ViewModel.ViewType != CompoundStructureAnnotationViewType.Close)
                {
                    window.ViewModel.Execute(window, uiDoc);
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
