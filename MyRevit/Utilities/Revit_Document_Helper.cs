using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.SubsidenceMonitor.Entities;
using PmSoft.Common.RevitClass.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.Utilities
{
    public class Revit_Document_Helper
    {
        #region View
        /// <summary>
        /// 获取文档下的所有视图
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static ViewSet GetAllViews(Document doc)
        {
            ViewSet views = new ViewSet();
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            FilteredElementIterator it = collector.OfClass(typeof(View)).GetElementIterator();
            it.Reset();
            while (it.MoveNext())
            {
                View view = it.Current as View3D;
                if (null != view && !view.IsTemplate && view.CanBePrinted)
                {
                    views.Insert(view);
                }
                else if (null == view)
                {
                    View view2D = it.Current as View;
                    if (view2D.ViewType == ViewType.FloorPlan | view2D.ViewType == ViewType.CeilingPlan | view2D.ViewType == ViewType.AreaPlan | view2D.ViewType == ViewType.Elevation | view2D.ViewType == ViewType.Section)
                    {
                        views.Insert(view2D);
                    }
                }
            }
            return views;
        }
        static ElementId Get3DProjectViewId(Document doc)
        {
            FilteredElementCollector Collector = new FilteredElementCollector(doc);
            List<Element> views = Collector.WherePasses(new ElementClassFilter(typeof(View3D))).ToElements().ToList();
            foreach (var view in views)
            {
                if (!(view as View).IsTemplate)
                {
                    return view.Id;
                }
            }
            return null;
        }
        /// <summary>
        /// 创建3D视图
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="doc"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public static View3D Create3DView(Document doc, string viewName)
        {
            var sampleViewId = Get3DProjectViewId(doc);
            if (sampleViewId == null)
            {
                throw new NotImplementedException("当前工程没有三维视图模版！请点击默认三维视图或创建模版后再操作！");
            }
            var viewTypeId = doc.GetElement(sampleViewId).get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsElementId();
            View3D view = View3D.CreateIsometric(doc, viewTypeId);
            view.Name = viewName;
            view.DetailLevel = ViewDetailLevel.Fine;
            view.DisplayStyle = DisplayStyle.Shading;
            return view;
            //view.get_Parameter(BuiltInParameter.VIEW_NAME).Set(viewName);
        }
        /// <summary>
        /// 淡显3D视图
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="doc"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public static View3D CreateTinged3DView(Document doc, string viewName)
        {
            var view = Create3DView(doc, viewName);
            //淡显处理
            SetOverrideGraphicSettingsOfCategories(view, CPSettings.GetTingledOverrideGraphicSettings(doc));
            //隐藏CAD处理
            HideCADFiles(doc, view);
            return view;
        }
        static void HideCADFiles(Document doc, View3D view)
        {
            var collection = new FilteredElementCollector(doc).WherePasses(new ElementClassFilter(typeof(ImportInstance))).ToElements().ToList();
            foreach (var ele in collection)
            {
                if (ele.Category != null)
                {
                    if (ele.Category.Name.Contains(".dwg") || (doc.GetElement(ele.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsElementId()) as CADLinkType) != null)
                    {
                        try { view.SetVisibility(ele.Category, false); }
                        catch { continue; }
                    }
                }
            }
        }
        static void SetOverrideGraphicSettingsOfCategories(View3D view, OverrideGraphicSettings settings)
        {
            Categories cates = view.Document.Settings.Categories;
            foreach (Category item in cates)
            {
                try
                {
                    view.SetCategoryOverrides(item.Id, settings);
                }
                catch { }
            }
            //需要另外设置以下元素，因为在Categories找不到
            view.SetCategoryOverrides(Category.GetCategory(view.Document, BuiltInCategory.OST_EdgeSlab).Id, settings);
            //SetOverride(Category.GetCategory(view.Document, BuiltInCategory.OST_EdgeSlab), setting, view);
            //隐藏以下线条
            try
            {
                Category.GetCategory(view.Document, BuiltInCategory.OST_DuctCurvesCenterLine).set_Visible(view, false);//风管中心线
                Category.GetCategory(view.Document, BuiltInCategory.OST_DuctFittingCenterLine).set_Visible(view, false);//风管管件中心线
                Category.GetCategory(view.Document, BuiltInCategory.OST_PipeFittingCenterLine).set_Visible(view, false);//管件中心线
                Category.GetCategory(view.Document, BuiltInCategory.OST_PipeCurvesCenterLine).set_Visible(view, false);//管道中心线
                Category.GetCategory(view.Document, BuiltInCategory.OST_CableTrayFittingCenterLine).set_Visible(view, false);//电缆桥架配件中心线
                Category.GetCategory(view.Document, BuiltInCategory.OST_CableTrayCenterLine).set_Visible(view, false);//电缆桥架中心线
            }
            catch { }
        }
        #endregion

        #region Element
        public static Element GetElementByName(Document doc, string name)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            FilteredElementIterator it = collector.OfClass(typeof(View)).GetElementIterator();
            it.Reset();
            while (it.MoveNext())
            {
                if (it.Current.Name == name)
                    return it.Current;
            }
            return null;
        }
        public static T GetElementByNameAs<T>(Document doc, string name) where T : class
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            FilteredElementIterator it = collector.OfClass(typeof(View)).GetElementIterator();
            it.Reset();
            while (it.MoveNext())
            {
                if (it.Current.Name == name)
                    return it.Current as T;
            }
            return null;
        }
        #endregion

        #region Parameter
        public static bool AddSharedParameter(Document doc, string groupName, string parameterName, Category newCategory, bool isInstance)
        {
            DefinitionFile definitionFile = doc.Application.OpenSharedParameterFile();
            DefinitionGroups groups = definitionFile.Groups;
            DefinitionGroup group = groups.get_Item(groupName);
            if (group == null)
                throw new Exception("没有找到对应的参数组");
            Definition definition = group.Definitions.get_Item(parameterName);
            if (definition == null)
                throw new Exception("没有找到对应的参数");
            ElementBinding binding = null;
            ElementBinding orientBinding = doc.ParameterBindings.get_Item(definition) as ElementBinding;
            CategorySet categories = new CategorySet(); ;
            if (orientBinding != null)
            {
                foreach (Category c in orientBinding.Categories)
                {
                    categories.Insert(c);
                }
            }
            categories.Insert(newCategory);
            if (isInstance)
                binding = doc.Application.Create.NewInstanceBinding(categories);
            else
                binding = doc.Application.Create.NewTypeBinding(categories);
            doc.ParameterBindings.Remove(definition);
            return doc.ParameterBindings.Insert(definition, binding, BuiltInParameterGroup.PG_TEXT);
        }
        #endregion
    }
}
