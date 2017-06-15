using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.MyTests.PipeAnnotation;
using MyRevit.Utilities;
using PmSoft.Common.CommonClass;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MyRevit.Entities
{
    #region 关联存储
    /// <summary>
    /// 存储对象
    /// </summary>
    public class PipeAnnotationEntity
    {
        public MultiPipeTagLocation LocationType { set; get; }
        public int ViewId { set; get; }
        public int LineId { set; get; }
        public List<int> PipeIds { set; get; }
        public List<int> TagIds { set; get; }

        public PipeAnnotationEntity()
        {
            PipeIds = new List<int>();
            TagIds = new List<int>();
        }

        public PipeAnnotationEntity(MultiPipeTagLocation locationType, int viewId, int lineId, List<int> pipeIds, List<int> tagIds)
        {
            LocationType = locationType;
            ViewId = viewId;
            LineId = lineId;
            PipeIds = pipeIds;
            TagIds = tagIds;
        }
    }
    /// <summary>
    /// 存储对象集合
    /// </summary>
    public class PipeAnnotationEntityCollection : List<PipeAnnotationEntity>
    {
        public static string PropertyInnerSplitter = "_";
        public static string PropertySplitter = ",";
        public static string EntitySplitter = ";";
        public static char PropertyInnerSplitter_Char = '_';
        public static char PropertySplitter_Char = ',';
        public static char EntitySplitter_Char = ';';

        public PipeAnnotationEntityCollection(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;
            var entities = data.Split(EntitySplitter_Char);
            var propertySplitter = PropertySplitter_Char;
            foreach (var entity in entities)
            {
                if (string.IsNullOrEmpty(entity))
                    continue;
                var properties = entity.Split(propertySplitter);
                if (properties.Count() == 5)
                {
                    MultiPipeTagLocation locationType = (MultiPipeTagLocation)Enum.Parse(typeof(MultiPipeTagLocation), properties[0]);
                    int viewId = Convert.ToInt32(properties[1]);
                    int specialTagFrame = Convert.ToInt32(properties[2]);
                    List<int> pipeIds = new List<int>();
                    foreach (var item in properties[3].Split(PropertyInnerSplitter_Char))
                    {
                        pipeIds.Add(Convert.ToInt32(item));
                    }
                    List<int> annotationIds = new List<int>();
                    foreach (var item in properties[4].Split(PropertyInnerSplitter_Char))
                    {
                        annotationIds.Add(Convert.ToInt32(item));
                    }
                    Add(new PipeAnnotationEntity(locationType, viewId, specialTagFrame, pipeIds, annotationIds));
                }
            }
        }

        /// <summary>
        /// 转String
        /// </summary>
        /// <returns></returns>
        public string ToData()
        {
            return string.Join(EntitySplitter, this.Select(c => (int)c.LocationType + PropertySplitter + c.ViewId + PropertySplitter + c.LineId + PropertySplitter + string.Join(PropertyInnerSplitter, c.PipeIds) + PropertySplitter + string.Join(PropertyInnerSplitter, c.TagIds)));
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="doc"></param>
        public void Save(Document doc)
        {
            PipeAnnotationContext.SaveCollection(doc);
        }
    }
    /// <summary>
    /// 上下文
    /// </summary>
    public class PipeAnnotationContext
    {
        /// <summary>
        /// 取Entity
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static Entity GetEntity(Document doc)
        {
            Entity entity;
            var storage = GetStorage(doc);
            var schema = GetSchema(SchemaId, SchemaName);
            entity = storage.GetEntity(schema);
            return entity;
        }

        #region Schema
        private static string FieldName = "PipeAnnotation_Collection";

        private static Schema Schema;
        private static Guid SchemaId = new Guid("720080CB-DA99-40DC-9415-E53F280AA1F1");
        private static string SchemaName = "PipeAnnotation";

        /// <summary>
        /// 取Schema
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static Schema GetSchema(Guid schemaId, string schemaName)
        {
            if (Schema == null)
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(schemaId);
                //SchemaName
                schemaBuilder.SetSchemaName(schemaName);
                //读权限 
                schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                //写权限
                schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
                //FieldName
                schemaBuilder.AddSimpleField(FieldName, typeof(string));
                Schema = schemaBuilder.Finish();
            }
            return Schema;
        }
        #endregion

        #region DataStorage
        private static DataStorage Storage;
        private static string StorageName = "PipeAnnotation";

        /// <summary>
        /// 获取DataStorage
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static DataStorage GetStorage(Document doc)
        {
            if (Storage == null)
            {
                Storage = GetStorageByName(doc, StorageName);
            }
            return Storage;
        }

        /// <summary>
        /// 创建DataStorage
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static DataStorage CreateStorage(Document doc, string name)
        {
            DataStorage st = DataStorage.Create(doc);
            st.Name = name;
            return st;
        }

        /// <summary>
        /// 获取或者无时创建DataStorage
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static DataStorage GetStorageByName(Document doc, string name)
        {
            FilteredElementCollector eleCollector = new FilteredElementCollector(doc).OfClass(typeof(DataStorage));
            foreach (DataStorage dt in eleCollector)
            {
                if (dt.Name == name)
                {
                    return dt;
                }
            }
            return null;
        }

        /// <summary>
        /// 删除对应名称的DataStorage
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name"></param>
        public static void RemoveStorageByName(Document doc, string name)
        {
            var storage = GetStorageByName(doc, name);
            if (storage != null)
                doc.Delete(storage.Id);
        }
        #endregion

        #region Collection
        private static PipeAnnotationEntityCollection Collection;

        /// <summary>
        /// 取数据Collection
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static PipeAnnotationEntityCollection GetCollection(Document doc)
        {
            ////TEST
            //RemoveStorageByName(doc, StorageName);

            if (Collection != null)
                return Collection;

            var schema = GetSchema(SchemaId, SchemaName);
            var storage = GetStorage(doc);
            Entity entity;
            if (storage == null)
            {
                storage = CreateStorage(doc, StorageName);
                entity = new Entity(schema);
                entity.Set<string>(schema.GetField(FieldName), "");
                storage.SetEntity(entity);
            }
            entity = storage.GetEntity(schema);
            string data = entity.Get<string>(schema.GetField(FieldName));
            Collection = new PipeAnnotationEntityCollection(data);
            return Collection;
        }

        /// <summary>
        /// 保存Collection
        /// </summary>
        /// <param name="doc"></param>
        public static void SaveCollection(Document doc)
        {
            var data = Collection.ToData();
            var storage = GetStorage(doc);
            var schema = GetSchema(SchemaId, SchemaName);
            Entity entity = storage.GetEntity(schema);
            entity.Set<string>(schema.GetField(FieldName), data);
            storage.SetEntity(entity);
        }
        #endregion
    }
    #endregion

    /// <summary>
    /// 常量集合
    /// </summary>
    public class PipeAnnotationConstaints
    {
        public const double SkewLengthForOnLine = 0.5;
        public const double SkewLengthForOffLine = 1.5;
    }

    /// <summary>
    /// 编辑Updater
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class PipeUpdaterCommand : IExternalApplication
    {

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            var uiApp = application;
            var app = uiApp.ControlledApplication;

            var updater = new PipeAnnotationUpdater(new Guid("B593F2C4-F38C-41D7-AE2C-369BB0149D9B"), new Guid("51879AF5-03AE-4B95-9B86-E24DF7181943"));
            var updaterInfo = UpdaterRegistry.GetRegisteredUpdaterInfos().FirstOrDefault(c => c.UpdaterName == updater.GetUpdaterName());
            if (updaterInfo == null)
            {
                UpdaterRegistry.RegisterUpdater(updater, true);
                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), new LogicalOrFilter(new List<ElementFilter>() {
                    new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves),//管
                    new ElementCategoryFilter(BuiltInCategory.OST_DetailComponents),//线
                    new ElementCategoryFilter(BuiltInCategory.OST_PipeTags),//标注
                })
                , Element.GetChangeTypeAny());
            }
            return Result.Succeeded;
        }
    }


    /// <summary>
    /// 梁,线,标注 位置处理 IUpdater
    /// </summary>
    class PipeAnnotationUpdater : IUpdater
    {
        UpdaterId UpdaterId;

        public PipeAnnotationUpdater(Guid commandId, Guid updaterId)
        {
            this.UpdaterId = new UpdaterId(new AddInId(commandId), updaterId);
        }

        #region MyTestContext.GetCollection方案
        public void Execute(UpdaterData updateData)
        {
            var doc = updateData.GetDocument();
            //var adds = updateData.GetAddedElementIds();
            var edits = updateData.GetModifiedElementIds();
            //var deletes = updateData.GetDeletedElementIds();
            var collection = PipeAnnotationContext.GetCollection(doc);
            if (edits.Count == 0)
                return;

            foreach (var editId in edits)
            {
                var element = doc.GetElement(editId);

                PipeAnnotationEntity entity = null;
                var lineMoved = collection.FirstOrDefault(c => c.LineId == editId.IntegerValue);
                if (lineMoved != null)
                {
                    TaskDialog.Show("警告", "线移动了");
                    entity = lineMoved;
                    //线移动处理
                }
                var pipeMoved = collection.FirstOrDefault(c => c.PipeIds.Contains(editId.IntegerValue));
                if (pipeMoved != null)
                {
                    TaskDialog.Show("警告", "管道移动了");
                    entity = pipeMoved;
                    //管道移动处理
                }
                var tagMoved = collection.FirstOrDefault(c => c.TagIds.Contains(editId.IntegerValue));
                if (tagMoved != null)
                {
                    TaskDialog.Show("警告", "标注移动了");
                    entity = tagMoved;
                    //标注移动处理
                }
            }
            PipeAnnotationContext.SaveCollection(doc);
        }

        private static XYZ CalculateTagPointAndLinePointFromLine(Curve beamCurve, XYZ vecticalVector, ref XYZ currentPoint0, ref XYZ currentPoint1)
        {
            var standardLength = BeamAnnotationConstaints.standardLength;

            var midPoint = (beamCurve.GetEndPoint(0) + beamCurve.GetEndPoint(1)) / 2;
            beamCurve.MakeUnbound();
            var project = beamCurve.Project(currentPoint0);
            var lengthPoint1 = currentPoint1.DistanceTo(project.XYZPoint);
            var lengthPoint0 = currentPoint0.DistanceTo(project.XYZPoint);
            if (lengthPoint0 > lengthPoint1)//在梁上方
            {
                currentPoint1 = project.XYZPoint;
                if (lengthPoint0 < standardLength)
                    currentPoint0 = currentPoint1 - standardLength * vecticalVector;
                return currentPoint0;
            }
            else
            {
                currentPoint0 = project.XYZPoint;
                if (lengthPoint1 < standardLength)
                    currentPoint1 = currentPoint0 + standardLength * vecticalVector;
                return currentPoint1 - standardLength * vecticalVector;
            }
        }
        private static XYZ CalculateTagPointAndLinePointFromTag(Curve beamCurve, XYZ tagPoint, XYZ parallelVector, XYZ vecticalVector, out XYZ currentPoint0, out XYZ currentPoint1)
        {
            var standardLength = BeamAnnotationConstaints.standardLength;
            var parallelLength = BeamAnnotationConstaints.parallelLength;
            var vecticalLength = BeamAnnotationConstaints.vecticalLength;
            var tagDiagonalXYZ = parallelLength * parallelVector + vecticalLength * vecticalVector;

            currentPoint0 = tagPoint - tagDiagonalXYZ;
            var z = currentPoint0.Z;//梁的Z轴为0与Curve绘制的轴不一致,调整为以Curve为准
            var midPoint = (beamCurve.GetEndPoint(0) + beamCurve.GetEndPoint(1)) / 2;
            var orientPoint0 = new XYZ(midPoint.X, midPoint.Y, currentPoint0.Z);//梁的Z轴为0与Curve绘制的轴不一致,调整为以Curve为准
            currentPoint1 = currentPoint0 + standardLength * vecticalVector;
            beamCurve.MakeUnbound();
            var project = beamCurve.Project(currentPoint0);
            if (currentPoint0.DistanceTo(project.XYZPoint) > currentPoint1.DistanceTo(project.XYZPoint))
            {
                currentPoint1 = project.XYZPoint;
                currentPoint1 = new XYZ(currentPoint1.X, currentPoint1.Y, z);
                if (currentPoint0.DistanceTo(project.XYZPoint) < standardLength)
                {
                    currentPoint0 = currentPoint1 - standardLength * vecticalVector;
                    currentPoint0 = new XYZ(currentPoint0.X, currentPoint0.Y, z);
                    return currentPoint0;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                currentPoint0 = project.XYZPoint;
                currentPoint0 = new XYZ(currentPoint0.X, currentPoint0.Y, z);
                if (currentPoint1.DistanceTo(project.XYZPoint) < standardLength)
                {
                    currentPoint1 = currentPoint0 + standardLength * vecticalVector;
                    currentPoint1 = new XYZ(currentPoint1.X, currentPoint1.Y, z);
                    return currentPoint1 - standardLength * vecticalVector;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion

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
            return nameof(MyUpdater);
        }
    }
}
