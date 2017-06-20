using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.Utilities;
using PmSoft.Common.CommonClass;
using PmSoft.Common.RevitClass.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyRevit.Entities
{
    /// <summary>
    /// 文本数据名称
    /// </summary>
    enum BeamAnnotationParameters
    {
        //PG_TEXT
        梁名称,//已有入口
            //梁截面,//无效
        箍筋直径,//已有入口
             //INVALID
             //乘号,//无入口
        梁高,//已有入口
        梁宽,//已有入口
        梁加腋,//已有入口
        梁加腋可见性,//已有入口
        梁箍筋,//已有入口
        梁纵筋,//已有入口
        梁腰筋,//已有入口
        纵筋级别,//已有入口
        腰筋级别,//已有入口
             //钢筋级别,//无效
        梁顶绝对标高,//已有入口
        梁顶相对偏移,//已有入口
    }
    /// <summary>
    /// 文本数据记忆
    /// </summary>
    public class BeamAnnotationData
    {
        /// <summary>
        /// `梁名称
        /// </summary>
        public string BeamName { set; get; }
        ///// <summary>
        ///// `梁截面,多余
        ///// </summary>
        //public string BeamSection{ set; get; }
        /// <summary>
        /// 梁宽
        /// </summary>
        public string BeamWidth { set; get; }
        /// <summary>
        /// 梁高
        /// </summary>
        public string BeamHeight { set; get; }
        /// <summary>
        /// 梁加腋可见性
        /// </summary>
        public bool BeamHaunchingVisibility { set; get; }
        /// <summary>
        /// 梁加腋
        /// </summary>
        public string BeamHaunching { set; get; }
        /// <summary>
        /// 箍筋直径
        /// </summary>
        public string HoopingDiameter { set; get; }
        /// <summary>
        /// 梁箍筋
        /// </summary>
        public string Hooping { set; get; }
        /// <summary>
        /// 梁纵筋
        /// </summary>
        public string LongitudinalBar { set; get; }
        /// <summary>
        /// 纵筋级别
        /// </summary>
        public string LongitudinalBarLevel { set; get; }
        /// <summary>
        /// 腰筋级别
        /// </summary>
        public string WaistBarLevel { set; get; }
        /// <summary>
        /// 梁腰筋
        /// </summary>
        public string WaistBar { set; get; }
        ///// <summary>
        ///// 钢筋级别,多余
        ///// </summary>
        //public string Rebar { set; get; }
        /// <summary>
        /// 梁顶绝对标高
        /// </summary>
        public string BeamAbsoluteTop { set; get; }
        /// <summary>
        /// 梁顶相对偏移
        /// </summary>
        public string BeamRelationalSkew { set; get; }

        enum LabelNameType
        {
            梁名称,
            梁宽,
            梁高,
            梁加腋,
            梁加腋可见,
            梁箍筋,
            梁箍筋等级,//箍筋直径
            梁纵筋,
            梁纵筋等级,
            梁腰筋,
            梁腰筋等级,
            梁顶相对位移,
            梁顶绝对位移,
        }
        public void LoadConfig(Autodesk.Revit.DB.Document doc)
        {
            FaceRecorderForRevit Recorder = GetRecorder(doc);
            BeamName = Recorder.GetValue(nameof(BeamName), "KL1(1)", 40);
            BeamWidth = Recorder.GetValue(nameof(BeamWidth), "300", 20);
            BeamHeight = Recorder.GetValue(nameof(BeamHeight), "600", 20);
            BeamHaunching = Recorder.GetValue(nameof(BeamHaunching), "Y350x500", 40);
            BeamHaunchingVisibility = Recorder.GetValueAsBOOL(nameof(BeamHaunchingVisibility), false);
            Hooping = Recorder.GetValue(nameof(Hooping), "8@200(2)", 40);
            HoopingDiameter = Recorder.GetValue(nameof(HoopingDiameter), "C", 40);
            LongitudinalBar = Recorder.GetValue(nameof(LongitudinalBar), "3 14;3 18", 40);
            LongitudinalBarLevel = Recorder.GetValue(nameof(LongitudinalBarLevel), "B", 40);
            WaistBar = Recorder.GetValue(nameof(WaistBar), "N4 15", 40);
            WaistBarLevel = Recorder.GetValue(nameof(WaistBarLevel), "C", 40);
            BeamAbsoluteTop = Recorder.GetValue(nameof(BeamAbsoluteTop), "(0.600)", 40);
            BeamRelationalSkew = Recorder.GetValue(nameof(BeamRelationalSkew), "(3.150)", 40);
        }
        public void SaveConfig(Autodesk.Revit.DB.Document doc)
        {
            FaceRecorderForRevit Recorder = GetRecorder(doc);
            Recorder.WriteValue(nameof(BeamName), BeamName);
            Recorder.WriteValue(nameof(BeamWidth), BeamWidth);
            Recorder.WriteValue(nameof(BeamHeight), BeamHeight);
            Recorder.WriteValue(nameof(BeamHaunchingVisibility), BeamHaunchingVisibility.ToString());
            Recorder.WriteValue(nameof(BeamHaunching), BeamHaunching);
            Recorder.WriteValue(nameof(HoopingDiameter), HoopingDiameter);
            Recorder.WriteValue(nameof(Hooping), Hooping);
            Recorder.WriteValue(nameof(LongitudinalBar), LongitudinalBar);
            Recorder.WriteValue(nameof(LongitudinalBarLevel), LongitudinalBarLevel);
            Recorder.WriteValue(nameof(WaistBarLevel), WaistBarLevel);
            Recorder.WriteValue(nameof(WaistBar), WaistBar);
            Recorder.WriteValue(nameof(BeamAbsoluteTop), BeamAbsoluteTop);
            Recorder.WriteValue(nameof(BeamRelationalSkew), BeamRelationalSkew);
        }
        public static FaceRecorderForRevit GetRecorder(Autodesk.Revit.DB.Document doc)
        {
            return new FaceRecorderForRevit(nameof(BeamAnnotationData), ApplicationPath.GetCurrentPath(doc));
        }
    }

    public class BeamAnnotationEntity
    {
        public bool IsEditing { set; get; }
        public int ViewId { set; get; }
        public int BeamId { set; get; }
        public int LineId { set; get; }
        public int TagId { set; get; }

        public BeamAnnotationEntity(int viewId, int beamId, int lineId, int tagId)
        {
            ViewId = viewId;
            BeamId = beamId;
            LineId = lineId;
            TagId = tagId;
        }
    }
    public class BeamAnnotationEntityCollection : List<BeamAnnotationEntity>
    {
        public static string PropertySplitter = ",";
        public static string EntitySplitter = ";";

        public BeamAnnotationEntityCollection(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;
            var entities = data.Split(EntitySplitter.ToCharArray()[0]);
            var propertySplitter = PropertySplitter.ToCharArray()[0];
            foreach (var entity in entities)
            {
                if (string.IsNullOrEmpty(entity))
                    continue;
                var properties = entity.Split(propertySplitter);
                Add(new BeamAnnotationEntity(Convert.ToInt32(properties[0]), Convert.ToInt32(properties[1]), Convert.ToInt32(properties[2]), Convert.ToInt32(properties[3])));
            }
        }

        public string ToData()
        {
            return string.Join(EntitySplitter, this.Select(c => c.ViewId + PropertySplitter + c.BeamId + PropertySplitter + c.LineId + PropertySplitter + c.TagId));
        }
    }
    public enum SaveKey
    {
        CollectionLength,
        CollectionData,
    }

    public class MyTestContext
    {
        #region Collection
        static BeamAnnotationEntityCollection Collection;
        public static BeamAnnotationEntityCollection GetCollection(Document doc)
        {
            var recorder = PMSoftHelper.GetRecorder(nameof(BeamAnnotationData), doc);
            var length = int.Parse(recorder.GetValue(nameof(SaveKey.CollectionLength), "0", 10));
            var data = recorder.GetValue(nameof(SaveKey.CollectionData), "", length + 2);
            Collection = new BeamAnnotationEntityCollection(data);
            //if (Collection == null)
            //{
            //    var recorder = PMSoftHelper.GetRecorder(nameof(BeamAnnotationData), doc);
            //    var length = recorder.GetValueAsInt(nameof(SaveKey.CollectionLength), 0);
            //    var data = recorder.GetValue(nameof(SaveKey.CollectionData), "", length + 2);
            //    Collection = new BeamAnnotationEntityCollection(data);
            //}
            return Collection;
        }
        public static void SaveCollection(Document doc)
        {
            var data = Collection.ToData();
            var recorder = PMSoftHelper.GetRecorder(nameof(BeamAnnotationData), doc);
            recorder.WriteValue(nameof(SaveKey.CollectionData), data);
            recorder.WriteValue(nameof(SaveKey.CollectionLength), data.Length.ToString());
        }
        #endregion
    }

    [Transaction(TransactionMode.Manual)]
    public class BeamAnnotationCommand : IExternalCommand
    {
        string BeamAnnotationParameterGroupName = "梁集中标注";
        BeamAnnotationData BeamAnnotationData = new BeamAnnotationData();
        static string SharedParameterFile = Path.Combine(ApplicationPath.GetParentPathOfCurrent, "ConstructionManagement.txt");
        ElementId selectedId;


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;
            if (selectedId == null)
                selectedId = uiDoc.Selection.PickObject(ObjectType.Element, new BeamFramingFilter()).ElementId;
            string tagName = "梁平法_集中标_左对齐";
            FamilySymbol tagSymbol = null;
            TransactionHelper.DelegateTransaction(doc, "加载族", () =>
            {
                //查找族类型
                var symbols = new FilteredElementCollector(doc)
                    .WherePasses(new ElementCategoryFilter(BuiltInCategory.OST_StructuralFramingTags))
                    .WherePasses(new ElementClassFilter(typeof(FamilySymbol)));
                var targetSymbol = symbols.FirstOrDefault(c => (c as FamilySymbol).FamilyName == tagName);
                if (targetSymbol != null)
                    tagSymbol = targetSymbol as FamilySymbol;
                //空时加载族类型
                if (tagSymbol == null)
                {
                    var symbolFile = @"E:\WorkingSpace\Tasks\0526标注\梁平法_集中标_左对齐.rfa";
                    Family family;
                    if (doc.LoadFamily(symbolFile, out family))
                    {
                        //获取族类型集合Id
                        var familySymbolIds = family.GetFamilySymbolIds();
                        foreach (var familySymbolId in familySymbolIds)
                        {
                            var element = doc.GetElement(familySymbolId) as FamilySymbol;
                            if (element != null && element.FamilyName == tagName)
                            {
                                tagSymbol = element;
                                break;
                            }
                        }
                    }
                    else
                    {
                        TaskDialogShow("加载族文件失败");
                    }
                }
                return true;
            });
            if (tagSymbol == null)
                return Result.Failed;
            TransactionHelper.DelegateTransaction(doc, "参数赋值", () =>
            {
                //如果上述两者获取到了对应的族
                var view = doc.ActiveView;
                var element = doc.GetElement(selectedId);
                //参数赋值
                BeamAnnotationData.LoadConfig(doc);
                var beamParameterNames = Enum.GetNames(typeof(BeamAnnotationParameters));
                var parameter = element.GetParameters(nameof(BeamAnnotationParameters.梁名称)).FirstOrDefault();
                if (parameter == null)
                {
                    string shareFilePath = @"E:\WorkingSpace\Tasks\0526标注\标注_共享参数(全).txt";
                    var parameterHelper = new ShareParameter(shareFilePath);
                    foreach (var beamParameterName in beamParameterNames)
                    {
                        if (new List<string>() {
                            nameof(BeamAnnotationParameters.梁名称)
                            ,nameof(BeamAnnotationParameters.箍筋直径)}
                            .Contains(beamParameterName))
                            AddSharedParameterByDefaulGroupName(doc, shareFilePath, BeamAnnotationParameterGroupName, beamParameterName, element.Category, true, Autodesk.Revit.DB.BuiltInParameterGroup.PG_TEXT);
                        //parameterHelper.AddShadeParameter(doc, shareFilePath, BeamAnnotationParameterGroupName, beamParameterName, element.Category, false, Autodesk.Revit.DB.BuiltInParameterGroup.PG_TEXT);
                        else if (new List<string>() {
                            nameof(BeamAnnotationParameters.梁宽)
                            ,nameof(BeamAnnotationParameters.梁高)}
                            .Contains(beamParameterName))
                            AddSharedParameterByDefaulGroupName(doc, shareFilePath, BeamAnnotationParameterGroupName, beamParameterName, element.Category, false, Autodesk.Revit.DB.BuiltInParameterGroup.PG_GEOMETRY);
                        //parameterHelper.AddShadeParameter(doc, BeamAnnotationParameterGroupName, beamParameterName, element.Category, true, Autodesk.Revit.DB.BuiltInParameterGroup.PG_GEOMETRY);
                        else
                            AddSharedParameterByDefaulGroupName(doc, shareFilePath, BeamAnnotationParameterGroupName, beamParameterName, element.Category, true, Autodesk.Revit.DB.BuiltInParameterGroup.INVALID);
                        //parameterHelper.AddShadeParameter(doc, BeamAnnotationParameterGroupName, beamParameterName, element.Category, false, Autodesk.Revit.DB.BuiltInParameterGroup.INVALID);
                    }
                }
                //参数赋值
                element.GetParameters(nameof(BeamAnnotationParameters.梁名称)).FirstOrDefault().Set(BeamAnnotationData.BeamName);
                element.GetParameters(nameof(BeamAnnotationParameters.梁加腋)).FirstOrDefault().Set(BeamAnnotationData.BeamHaunching);
                element.GetParameters(nameof(BeamAnnotationParameters.梁加腋可见性)).FirstOrDefault().Set(Convert.ToInt32(BeamAnnotationData.BeamHaunchingVisibility));
                element.GetParameters(nameof(BeamAnnotationParameters.梁箍筋)).FirstOrDefault().Set(BeamAnnotationData.Hooping);
                element.GetParameters(nameof(BeamAnnotationParameters.箍筋直径)).FirstOrDefault().Set(BeamAnnotationData.HoopingDiameter);
                element.GetParameters(nameof(BeamAnnotationParameters.梁纵筋)).FirstOrDefault().Set(BeamAnnotationData.LongitudinalBar);
                element.GetParameters(nameof(BeamAnnotationParameters.纵筋级别)).FirstOrDefault().Set(BeamAnnotationData.LongitudinalBarLevel);
                element.GetParameters(nameof(BeamAnnotationParameters.梁腰筋)).FirstOrDefault().Set(BeamAnnotationData.WaistBar);
                element.GetParameters(nameof(BeamAnnotationParameters.腰筋级别)).FirstOrDefault().Set(BeamAnnotationData.WaistBarLevel);
                element.GetParameters(nameof(BeamAnnotationParameters.梁顶绝对标高)).FirstOrDefault().Set(BeamAnnotationData.BeamAbsoluteTop);
                element.GetParameters(nameof(BeamAnnotationParameters.梁顶相对偏移)).FirstOrDefault().Set(BeamAnnotationData.BeamRelationalSkew);
                var symbol = (element as FamilyInstance).Symbol;
                symbol.GetParameters(nameof(BeamAnnotationParameters.梁宽)).FirstOrDefault().Set(BeamAnnotationData.BeamWidth);
                symbol.GetParameters(nameof(BeamAnnotationParameters.梁高)).FirstOrDefault().Set(BeamAnnotationData.BeamHeight);
                var familyDoc= doc.EditFamily(symbol.Family);

                return true;
            });
            TransactionHelper.DelegateTransaction(doc, "绘图处理", () =>
            {
                //绘图处理
                double parallelLength = BeamAnnotationConstaints.parallelLength;
                int vecticalLength = BeamAnnotationConstaints.vecticalLength;
                int standardLength = BeamAnnotationConstaints.standardLength;
                string relatedLineField = BeamAnnotationConstaints.relatedLineField;
                string relatedTagField = BeamAnnotationConstaints.relatedTagField;
                string relatedBeamField = BeamAnnotationConstaints.relatedBeamField;
                string relatedViewField = BeamAnnotationConstaints.relatedViewField;
                var view = doc.ActiveView;
                var beam = doc.GetElement(selectedId);
                //中点出线
                var locationCurve = (beam.Location as LocationCurve).Curve;
                var midPoint = (locationCurve.GetEndPoint(0) + locationCurve.GetEndPoint(1)) / 2;
                var parallelVector = (locationCurve as Line).Direction;
                if (parallelVector.X < 0 || (parallelVector.X == 0 && parallelVector.Y == -1))
                    parallelVector = new XYZ(-parallelVector.X, -parallelVector.Y, parallelVector.Z);
                var vecticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
                var line = doc.Create.NewDetailCurve(view, Line.CreateBound(midPoint, midPoint + standardLength * vecticalVector));
                //中点绘字
                var tag1 = doc.Create.NewTag(view, beam, false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, midPoint);
                //通过BoundingBox计算
                //var boundingBox = tag1.get_BoundingBox(view);
                //var diagonalLine = Line.CreateBound(boundingBox.Max ,boundingBox.Min);
                //var intersectionAngle = parallelVector.AngleTo(diagonalLine.Direction);
                //var targetPoint = midPoint + diagonalLine.Length * Math.Abs(Math.Sin(intersectionAngle))* vecticalVector + diagonalLine.Length * Math.Abs(Math.Cos(intersectionAngle)) * parallelVector;
                //确定长宽
                var targetPoint = (locationCurve.GetEndPoint(0) + locationCurve.GetEndPoint(1)) / 2 + parallelLength * parallelVector + vecticalLength * vecticalVector;
                ////删除中点绘字
                doc.Delete(tag1.Id);
                ////平行移动
                //doc.Create.NewTag(view, element, false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, midPoint + parallelLength * parallelVector);
                ////垂直移动
                //doc.Create.NewTag(view, element, false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, midPoint + vecticalLength * vecticalVector);
                //综合移动
                var tag2 = doc.Create.NewTag(view, beam, false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, targetPoint);

                ////更新扩展存储
                //var opr = new SchemaEntityOpr(line);
                //opr.SetParm(relatedViewField, view.Id.IntegerValue.ToString());
                //opr.SetParm(relatedBeamField, beam.Id.IntegerValue.ToString());
                //opr.SetParm(relatedTagField, tag2.Id.IntegerValue.ToString());
                //opr.SaveTo(line);
                //opr = new SchemaEntityOpr(tag2);
                //opr.SetParm(relatedViewField, view.Id.IntegerValue.ToString());
                //opr.SetParm(relatedBeamField, beam.Id.IntegerValue.ToString());
                //opr.SetParm(relatedLineField, line.Id.IntegerValue.ToString());
                //opr.SaveTo(tag2);

                //关系存储
                var collection = MyTestContext.GetCollection(doc);
                collection.Add(new BeamAnnotationEntity(view.Id.IntegerValue, beam.Id.IntegerValue, line.Id.IntegerValue, tag2.Id.IntegerValue));
                MyTestContext.SaveCollection(doc);
                return true;
            });
            return Result.Succeeded;
        }

        public static bool AddSharedParameterByDefaulGroupName(Document doc, string sharedParameterFile, string groupName, string parameterName, Category newCategory, bool isInstance, BuiltInParameterGroup parameterGroup)
        {
            doc.Application.SharedParametersFilename = sharedParameterFile;
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
                foreach (Category c in orientBinding.Categories)
                    categories.Insert(c);
            categories.Insert(newCategory);
            if (isInstance)
                binding = doc.Application.Create.NewInstanceBinding(categories);
            else
                binding = doc.Application.Create.NewTypeBinding(categories);
            doc.ParameterBindings.Remove(definition);
            var result = doc.ParameterBindings.Insert(definition, binding, parameterGroup);
            return result;
        }
        private static void TaskDialogShow(string message)
        {
            TaskDialog.Show("a", message);
        }
    }
}
