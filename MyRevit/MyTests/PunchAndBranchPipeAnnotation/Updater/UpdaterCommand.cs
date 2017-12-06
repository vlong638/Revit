using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.MyTests.CompoundStructureAnnotation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.MyTests.PBPA
{
    /// <summary>
    /// 编辑Updater
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class PBPAUpdaterCommand : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            var uiApp = application;
            var app = uiApp.ControlledApplication;
            var editUpdater = new PBPAUpdater_Edit(new AddInId(new Guid("4DF1BD0D-4F1D-49BD-8FC3-ED3CC253C714")));
            var updaterInfo = UpdaterRegistry.GetRegisteredUpdaterInfos().FirstOrDefault(c => c.UpdaterName == editUpdater.GetUpdaterName());
            if (updaterInfo != null)
                try
                {
                    UpdaterRegistry.UnregisterUpdater(editUpdater.GetUpdaterId());
                }
                catch
                {
                }
            UpdaterRegistry.RegisterUpdater(editUpdater, true);
            UpdaterRegistry.AddTrigger(editUpdater.GetUpdaterId(), new LogicalOrFilter(new List<ElementFilter>() {
                    new ElementCategoryFilter(BuiltInCategory.OST_PipeAccessory),//开洞 套管
                    new ElementCategoryFilter(BuiltInCategory.OST_PipeAccessoryTags),//开洞 套管标注
                })
            , Element.GetChangeTypeAny());
            var deleteUpdater = new PBPAUpdater_Delete(new AddInId(new Guid("4DF1BD0D-4F1D-49BD-8FC3-ED3CC253C714")));
            updaterInfo = UpdaterRegistry.GetRegisteredUpdaterInfos().FirstOrDefault(c => c.UpdaterName == deleteUpdater.GetUpdaterName());
            if (updaterInfo != null)
                try
                {
                    UpdaterRegistry.UnregisterUpdater(deleteUpdater.GetUpdaterId());
                }
                catch
                {
                }
            UpdaterRegistry.RegisterUpdater(deleteUpdater, true);
            UpdaterRegistry.AddTrigger(deleteUpdater.GetUpdaterId(), new LogicalOrFilter(new List<ElementFilter>() {
                    new ElementCategoryFilter(BuiltInCategory.OST_PipeAccessory),//开洞 套管
                    new ElementCategoryFilter(BuiltInCategory.OST_PipeAccessoryTags),//开洞 套管标注
                })
            , Element.GetChangeTypeElementDeletion());
            return Result.Succeeded;
        }
    }
}
