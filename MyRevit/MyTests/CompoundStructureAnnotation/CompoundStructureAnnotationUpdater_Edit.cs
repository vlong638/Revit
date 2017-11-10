using Autodesk.Revit.DB;
using MyRevit.MyTests.PipeAnnotationTest;
using PmSoft.Common.CommonClass;
using PmSoft.Common.Controls;
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
                var collection = CSAContext.GetCollection(document);
                if (CSAContext.IsEditing == true)
                {
                    CSAContext.IsEditing = false;
                    return;
                }
                List<int> movedEntities = new List<int>();
                foreach (var changeId in edits)
                {
                    CSAModelForFamilyInstance model = null;
                    if (VLConstraints.Doc == null)
                        VLConstraints.Doc = document;

                    #region 根据Target重新生成
                    var targetMoved = collection.Datas.FirstOrDefault(c => c.TargetId.IntegerValue == changeId.IntegerValue);
                    if (targetMoved != null)
                    {
                        model = targetMoved;
                        if (movedEntities.Contains(model.TargetId.IntegerValue))
                            continue;
                        var creater = CSAContext.Creater;
                        var target = document.GetElement(model.TargetId);//标注主体失效时删除
                        if (target == null)
                        {
                            collection.Datas.Remove(model);
                            continue;
                        }
                        CSAContext.Creater.Regenerate(document, model, target, new XYZ(0,0,0));
                        movedEntities.Add(model.TargetId.IntegerValue);
                        CSAContext.IsEditing = true;
                    }
                    #endregion

                    #region 根据Text重新生成
                    var textMoved = collection.Datas.FirstOrDefault(c => c.TextNoteIds.FirstOrDefault(p => p.IntegerValue == changeId.IntegerValue) != null);
                    if (textMoved != null)
                    {
                        model = textMoved;
                        if (movedEntities.Contains(model.TargetId.IntegerValue))
                            continue;
                        var creater = CSAContext.Creater;
                        var target = document.GetElement(model.TargetId);//标注主体失效时删除
                        if (target == null)
                        {
                            collection.Datas.Remove(model);
                            continue;
                        }
                        //文本更改处理
                        var index = model.TextNoteIds.IndexOf(changeId);
                        var newText = (document.GetElement(changeId) as TextNote).Text;
                        if (model.Texts[index] != newText)
                        {
                            CompoundStructure compoundStructure = null;
                            HostObjAttributes hoster = null;
                            if (target is Wall)
                            {
                                hoster = (HostObjAttributes)((target as Wall).WallType);
                                compoundStructure = hoster.GetCompoundStructure();
                            }
                            if (target is Floor)
                            {
                                hoster = (HostObjAttributes)((target as Floor).FloorType);
                                compoundStructure = hoster.GetCompoundStructure();
                            }
                            if (target is RoofBase)//屋顶有多种类型
                            {
                                hoster = (HostObjAttributes)((target as RoofBase).RoofType);
                                compoundStructure = hoster.GetCompoundStructure();
                            }
                            if (compoundStructure == null)
                                throw new NotImplementedException("关联更新失败,未获取有效的CompoundStructure类型");
                            var layers = compoundStructure.GetLayers();
                            string pattern = @"([\d+\.]+)厚(.+)[\r]?";
                            Regex regex = new Regex(pattern);
                            var match = regex.Match(newText);
                            if (!match.Success)
                            {
                                PMMessageBox.ShowError("关联更新失败,文本不符合规范");
                                return;
                            }
                            var length = match.Groups[1].Value;
                            var lengthFoot = UnitHelper.ConvertToFoot(Convert.ToDouble(length), VLUnitType.millimeter);
                            var materialName = match.Groups[2].Value;
                            var material = new FilteredElementCollector(document).OfClass(typeof(Material))
                                .FirstOrDefault(c => c.Name == materialName);
                            if (material == null)
                            {
                                PMMessageBox.ShowError("关联更新失败,未获取到对应名称的材质");
                                return;
                            }
                            //更新
                            layers[index].Width = lengthFoot;
                            layers[index].MaterialId = material.Id;
                            compoundStructure.SetLayers(layers);
                            IDictionary<int, CompoundStructureError> report = null;
                            IDictionary<int, int> errorMap;
                            try
                            {
                                compoundStructure.IsValid(document, out report, out errorMap);
                                hoster.SetCompoundStructure(compoundStructure);
                            }
                            catch (Exception ex)
                            {
                                PMMessageBox.ShowError("材质设置失败,错误详情:" + (report != null ? string.Join(",", report.Select(c => c.Value)) : ""));
                                throw ex;
                            }

                            ////报错This operation is valid only for non-vertically compound structures
                            //layer = layers[index];
                            //layer.MaterialId = material.Id;
                            ////compoundStructure.SetLayer(index, layer);
                        }
                        else
                        {
                            var textNote = document.GetElement(changeId) as TextNote;
                            if (model.TextNoteTypeElementId.IntegerValue!= textNote.TextNoteType.Id.IntegerValue)
                            {
                                model.TextNoteTypeElementId = textNote.TextNoteType.Id;
                                CSAContext.Creater.Regenerate(document, model, target, new XYZ(0, 0, 0));
                            }
                            else
                            {
                                //文本偏移处理
                                //var index = model.TextNoteIds.IndexOf(changeId);
                                //var offset = (document.GetElement(changeId) as TextNote).Coord - model.TextLocations[index];
                                //CompoundStructureAnnotationContext.Creater.Regenerate(document, model, target, offset);

                                //CSAContext.IsEditing = true;//移动会导致偏移 从而二次触发
                            }
                        }
                        movedEntities.Add(model.TargetId.IntegerValue);
                    }
                    #endregion

                    #region 根据Line重新生成
                    var lineMoved = collection.Datas.FirstOrDefault(c => c.LineId.IntegerValue == changeId.IntegerValue);
                    if (lineMoved != null)
                    {
                        model = lineMoved;
                        if (movedEntities.Contains(model.TargetId.IntegerValue))
                            continue;
                        var creater = CSAContext.Creater;
                        var target = document.GetElement(model.TargetId);
                        if (target == null)
                        {
                            collection.Datas.Remove(model);
                            continue;
                        }
                        CSAContext.Creater.Regenerate(document, model, target);
                        movedEntities.Add(model.TargetId.IntegerValue);
                        CSAContext.IsEditing = true;
                    }
                    #endregion
                }
                CSAContext.SaveCollection(document);
            }
            catch (Exception ex)
            {
                var logger = new TextLogger("PmLogger.txt", ApplicationPath.GetParentPathOfCurrent + @"\SysData");
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
