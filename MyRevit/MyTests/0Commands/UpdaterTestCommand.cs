using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.Entities
{
    [Transaction(TransactionMode.Manual)]
    public class UpdaterTestCommand : IExternalApplication
    {
        private static void TaskDialogShow(string message)
        {
            TaskDialog.Show("提示", message);
        }

        public Result OnStartup(UIControlledApplication application)
        {
            application.ControlledApplication.DocumentOpening += ControlledApplication_DocumentOpening;
            return Result.Succeeded;
        }

        private void ControlledApplication_DocumentOpening(object sender, Autodesk.Revit.DB.Events.DocumentOpeningEventArgs e)
        {
            var updater = new UpdaterTest2(new Guid("08EF0675-AF5C-4845-85AE-AF305A58C7E0"), new Guid("BD8B76B9-1AFB-41C2-83CC-466F35607FFB"));
            var updaterInfo = UpdaterRegistry.GetRegisteredUpdaterInfos().FirstOrDefault(c => c.UpdaterName == updater.GetUpdaterName());
            if (updaterInfo != null)
                UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());

            UpdaterRegistry.RegisterUpdater(updater, true);
            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), new LogicalOrFilter(new List<ElementFilter>() {
                    new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves),//管
                })
            , Element.GetChangeTypeAny());
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }

    public class UpdaterTest : IUpdater
    {
        UpdaterId UpdaterId;

        public UpdaterTest(Guid commandId, Guid updaterId)
        {
            this.UpdaterId = new UpdaterId(new AddInId(commandId), updaterId);
        }

        public void Execute(UpdaterData updateData)
        {
            var document = updateData.GetDocument();
            var edits = updateData.GetModifiedElementIds();
            var storageEntity = new TestStorageEntity();
            ExtensibleStorageHelperV2.SetData(document, storageEntity, storageEntity.FieldStr, "111");
        }
        public string GetAdditionalInformation()
        {
            return "N/A";
        }
        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FreeStandingComponents;
        }
        public UpdaterId GetUpdaterId()
        {
            return UpdaterId;
        }
        public string GetUpdaterName()
        {
            return "MyUpdater";
        }
    }
    public class UpdaterTest2 : IUpdater
    {
        UpdaterId UpdaterId;

        public UpdaterTest2(Guid commandId, Guid updaterId)
        {
            this.UpdaterId = new UpdaterId(new AddInId(commandId), updaterId);
        }

        public void Execute(UpdaterData updateData)
        {
            var document = updateData.GetDocument();
            var edits = updateData.GetModifiedElementIds();
            var storageEntity = new TestStorageEntity();
            var lineCollector = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Lines).WhereElementIsNotElementType();
            foreach (var line in lineCollector)
            {
                ElementTransformUtils.MoveElement(document, line.Id, new XYZ(10, 20, 0));
            }
        }
        public string GetAdditionalInformation()
        {
            return "N/A";
        }
        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FreeStandingComponents;
        }
        public UpdaterId GetUpdaterId()
        {
            return UpdaterId;
        }
        public string GetUpdaterName()
        {
            return "MyUpdater";
        }
    }
}
