using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.Entities
{

    [Transaction(TransactionMode.Manual)]
    public class 调研_关联更新类型结构属性 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;
            var updater = new CompoundStructureUpdater(new Guid("63F56502-E2D2-4CB6-9150-D8D951F5ED7C"));
            var updaterInfo = UpdaterRegistry.GetRegisteredUpdaterInfos().FirstOrDefault(c => c.UpdaterName == updater.GetUpdaterName());
            if (updaterInfo == null)
            {
                UpdaterRegistry.RegisterUpdater(updater, true);
                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), new LogicalOrFilter(new List<ElementFilter>() {
                    new ElementClassFilter(typeof(WallType))
                    , new ElementClassFilter(typeof(FloorType)) })
                    , Element.GetChangeTypeAny());
            }
            //app.DocumentChanged += App_DocumentChanged;

            return Result.Succeeded;
        }

        //private void App_DocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs e)
        //{
        //    var doc = e.GetDocument();
        //    var elememtIds = e.GetModifiedElementIds();
        //    var elements = elememtIds.Select(c => doc.GetElement(c));
        //}

        private static void TaskDialogShow(string message)
        {
            TaskDialog.Show("a", message);
        }
    }


    /// <summary>
    /// 梁,线,标注 位置处理 IUpdater
    /// </summary>
    class CompoundStructureUpdater : IUpdater
    {
        UpdaterId UpdaterId;

        public CompoundStructureUpdater(Guid _uid)
        {
            this.UpdaterId = new UpdaterId(new AddInId(new Guid("BBC4E9C2-AD73-4F13-9D70-6CAFAFAE35CC")), _uid);
        }

        static bool IsEditing;
        
        public void Execute(UpdaterData updateData)
        {
            var doc = updateData.GetDocument();
            var adds = updateData.GetAddedElementIds();
            var edits = updateData.GetModifiedElementIds();
            var deletes = updateData.GetDeletedElementIds();
            if (edits.Count == 0)
                return;

            foreach (var editId in edits)
            {
                var element = doc.GetElement(editId);
                CompoundStructure compoundStructure = null;
                if (element is WallType)
                {
                    compoundStructure = (element as WallType).GetCompoundStructure();
                }
                if (element is FloorType)
                {
                    compoundStructure = (element as FloorType).GetCompoundStructure();
                }
                if (element is ExtrusionRoof)//屋顶有多种类型
                {
                    compoundStructure = (element as ExtrusionRoof).RoofType.GetCompoundStructure();
                }
                if (compoundStructure == null)
                    return;
                var layers = compoundStructure.GetLayers();
                string text = "";
                foreach (var layer in layers)
                {
                    if (layer.MaterialId.IntegerValue < 0)
                        continue;
                    var material = doc.GetElement(layer.MaterialId);
                    if (material == null)
                        continue;
                    text += layer.Width + doc.GetElement(layer.MaterialId).Name + System.Environment.NewLine;
                }
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
            return nameof(CompoundStructureUpdater);
        }
    }
}
