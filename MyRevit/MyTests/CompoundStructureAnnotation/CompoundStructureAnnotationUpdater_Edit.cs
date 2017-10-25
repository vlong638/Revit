using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    /// <summary>
    /// 梁,线,标注 位置处理 IUpdater
    /// </summary>
    public class CompoundStructureAnnotationUpdater_Edit : IUpdater
    {
        static AddInId AddInId;
        static UpdaterId UpdaterId;

        public CompoundStructureAnnotationUpdater_Edit(AddInId addinID)
        {
            AddInId = addinID;
            UpdaterId = new UpdaterId(AddInId, new Guid("380C1562-65E3-47C7-B5D6-19AC04D2775D"));
        }

        #region MyTestContext.GetCollection方案
        public void Execute(UpdaterData updateData)
        {
            try
            {
                var document = updateData.GetDocument();
                var edits = updateData.GetModifiedElementIds();
                var collection = CompoundStructureAnnotationContext.GetCollection(document);
                if (CompoundStructureAnnotationContext.IsEditing == true)
                {
                    CompoundStructureAnnotationContext.IsEditing = false;
                    return;
                }
                List<int> movedEntities = new List<int>();
                foreach (var changeId in edits)
                {
                    CSAModel model = null;

                    #region 根据Target重新生成
                    var targetMoved = collection.Data.FirstOrDefault(c => c.TargetId.IntegerValue == changeId.IntegerValue);
                    if (targetMoved != null)
                    {
                        model = targetMoved;
                        if (movedEntities.Contains(model.TargetId.IntegerValue))
                            continue;
                        var creater = CompoundStructureAnnotationContext.Creater;
                        var target = document.GetElement(model.TargetId);//标注主体失效时删除
                        if (target == null)
                        {
                            collection.Data.Remove(model);
                            continue;
                        }
                        CompoundStructureAnnotationContext.Creater.Regenerate(document, model, target);
                        movedEntities.Add(model.TargetId.IntegerValue);
                        CompoundStructureAnnotationContext.IsEditing = true;
                    }
                    #endregion

                    #region 根据Text重新生成
                    var textMoved = collection.Data.FirstOrDefault(c => c.TextNoteIds.FirstOrDefault(p => p.IntegerValue == changeId.IntegerValue) != null);
                    if (textMoved != null)
                    {
                        model = textMoved;
                        if (movedEntities.Contains(model.TargetId.IntegerValue))
                            continue;
                        var creater = CompoundStructureAnnotationContext.Creater;
                        var target = document.GetElement(model.TargetId);//标注主体失效时删除
                        if (target == null)
                        {
                            collection.Data.Remove(model);
                            continue;
                        }
                        //文本更改处理
                        var index = model.TextNoteIds.IndexOf(changeId);
                        var newText = (document.GetElement(changeId) as TextNote).Text;
                        if (model.Texts[index] != newText)
                        {
                            CompoundStructure compoundStructure = null;
                            if (target is Wall)
                                compoundStructure = (target as Wall).WallType.GetCompoundStructure();
                            if (target is Floor)
                                compoundStructure = (target as Floor).FloorType.GetCompoundStructure();
                            if (target is RoofBase)//屋顶有多种类型
                                compoundStructure = (target as RoofBase).RoofType.GetCompoundStructure();
                            if (compoundStructure == null)
                                throw new NotImplementedException("关联更新失败,未获取有效的CompoundStructure类型");
                            var layers = compoundStructure.GetLayers();
                            string pattern = @"[\d+,\.]+(.+)[\n]+";
                            Regex regex = new Regex(pattern);
                            var match = regex.Match(newText);
                            if (!match.Success)
                                return;
                            var materialName = match.Groups[1].Value;
                            var material = new FilteredElementCollector(document).OfClass(typeof(Material))
                                .FirstOrDefault(c => c.Name == materialName);
                            if (material==null)
                                return;
                            //无效
                            //var layer = layers[index];
                            //compoundStructure.SetMaterialId(index, material.Id);

                            //无效
                            //layers[index].MaterialId = material.Id;
                            //compoundStructure.SetLayers(layers);

                            //报错This operation is valid only for non-vertically compound structures
                            var layer = layers[index];
                            layer.MaterialId = material.Id;
                            compoundStructure.SetLayer(index, layer);
                        }
                        //文本偏移处理
                        //var index = model.TextNoteIds.IndexOf(changeId);
                        //var offset = (document.GetElement(changeId) as TextNote).Coord - model.TextLocations[index];
                        //CompoundStructureAnnotationContext.Creater.Regenerate(document, model, target, offset);
                        movedEntities.Add(model.TargetId.IntegerValue);
                        CompoundStructureAnnotationContext.IsEditing = true;
                    }
                    #endregion
                }
                CompoundStructureAnnotationContext.SaveCollection(document);
            }
            catch (Exception ex)
            {
                var logger = new TextLogger("PmLogger.txt", @"D:\");
                logger.Error(ex.ToString());
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
            return "CompoundStructureAnnotationUpdater_Edit";
        }
    }

    public class TextLogger
    {
        #region Properties
        static object LogLocker = new object();
        string _fileName;
        public string FileName
        {
            set
            {
                _fileName = value;
            }
            get
            {
                return _fileName;
            }
        }
        string _directoryName;
        public string DirectoryName
        {
            set
            {
                _directoryName = value;
            }
            get
            {
                return _directoryName;
            }
        }
        public string FilePath
        {
            get
            {
                return Path.Combine(DirectoryName, FileName);
            }
        }
        #endregion

        public TextLogger(string fileName, string directoryName)
        {
            FileName = fileName;
            DirectoryName = directoryName;
            if (!Directory.Exists(DirectoryName))
            {
                Directory.CreateDirectory(DirectoryName);
            }
        }

        #region Methods
        protected void writeLog(string message)
        {
            lock (LogLocker)
            {
                using (StreamWriter stream = File.AppendText(FilePath))
                {
                    stream.WriteLine(DateTime.Now.ToString() + Environment.NewLine + message + Environment.NewLine);
                }
            }
        }
        public virtual void Error(string message)
        {
            writeLog(message);
        }
        public void Error(string pattern, params object[] args)
        {
            Error(string.Format(pattern, args));
        }
        public void Info(string pattern, params object[] args)
        {
            Info(string.Format(pattern, args));
        }
        public virtual void Info(string message)
        {
            writeLog(message);
        }
        #endregion
    }
}
