using Autodesk.Revit.DB;
using MyRevit.MyTests.PipeAnnotationTest;
using MyRevit.Utilities;
using PmSoft.Common.CommonClass;
using PmSoft.Common.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyRevit.MyTests.PAA
{
    /// <summary>
    /// 梁,线,标注 位置处理 IUpdater
    /// </summary>
    public class PAAUpdater_Edit : IUpdater
    {
        static AddInId AddInId;
        static UpdaterId UpdaterId;

        public PAAUpdater_Edit(AddInId addinID)
        {
            AddInId = addinID;
            UpdaterId = new UpdaterId(AddInId, new Guid("43D4A45E-48C4-4AFF-A44D-95C4DEA43370"));
        }

        #region MyTestContext.GetCollection方案
        public void Execute(UpdaterData updateData)
        {
            try
            {
                var document = updateData.GetDocument();
                var edits = updateData.GetModifiedElementIds();
                var collection = PAAContext.GetCollection(document);
                if (PAAContext.IsEditing == true)
                {
                    PAAContext.IsEditing = false;
                    return;
                }
                List<int> movedEntities = new List<int>();
                foreach (var changeId in edits)
                {
                    PAAModelForSingle model = null;
                    //if (VLConstraintsForCSA.Doc == null)
                    //    VLConstraintsForCSA.Doc = document;

                    #region 根据Target重新生成
                    var targetMoved = collection.Data.FirstOrDefault(c => c.TargetId.IntegerValue == changeId.IntegerValue);
                    if (targetMoved != null)
                    {
                        model = targetMoved;
                        if (movedEntities.Contains(model.TargetId.IntegerValue))
                            continue;
                        var creater = PAAContext.Creator;
                        var target = document.GetElement(model.TargetId);//标注主体失效时删除
                        if (target == null)
                        {
                            collection.Data.Remove(model);
                            continue;
                        }
                        //TODO_PAA 管道移动 计算offset
                        PAAContext.Creator.Regenerate(document, model, target);
                        movedEntities.Add(model.TargetId.IntegerValue);
                        //PAAContext.IsEditing = true;//重新生成无需避免移动导致的重复触发
                    }
                    #endregion

                    #region 根据Text重新生成
                    var textMoved = collection.Data.FirstOrDefault(c => c.AnnotationId.IntegerValue== changeId.IntegerValue);
                    if (textMoved != null)
                    {
                        model = textMoved;
                        if (movedEntities.Contains(model.TargetId.IntegerValue))
                            continue;
                        var creater = PAAContext.Creator;
                        var target = document.GetElement(model.TargetId);//标注主体失效时删除
                        if (target == null)
                        {
                            collection.Data.Remove(model);
                            continue;
                        }
                        //TODO_PAA 标注移动 计算offset
                        PAAContext.Creator.Regenerate(document, model, target);
                        movedEntities.Add(model.TargetId.IntegerValue);
                        //PAAContext.IsEditing = true;//重新生成无需避免移动导致的重复触发
                    }
                    #endregion

                    #region 根据Line重新生成
                    var lineMoved = collection.Data.FirstOrDefault(c => c.GroupId.IntegerValue == changeId.IntegerValue);
                    if (lineMoved != null)
                    {
                        model = lineMoved;
                        if (movedEntities.Contains(model.TargetId.IntegerValue))
                            continue;
                        var creater = PAAContext.Creator;
                        var target = document.GetElement(model.TargetId);
                        if (target == null)
                        {
                            collection.Data.Remove(model);
                            continue;
                        }
                        //TODO_PAA 线组移动 计算offset
                        PAAContext.Creator.Regenerate(document, model, target);
                        movedEntities.Add(model.TargetId.IntegerValue);
                        //CSAContext.IsEditing = true;//重新生成无需避免移动导致的重复触发
                    }
                    #endregion
                }
                PAAContext.Save(document);
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
            return "PAAUpdater_Edit";
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
