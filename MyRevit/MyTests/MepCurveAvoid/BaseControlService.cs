using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.Utilities;
using PmSoft.Common.CommonClass;
using PmSoft.Common.RevitClass.Utils;
using PmSoft.MainModel.EntData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.MyTests.MepCurveAvoid
{
    /// <summary>
    /// 安装基础服务
    /// <remarks>作者:黄克强，创建日期:2016/7/19 17:13:43，修改日期:</remarks>
    /// </summary>
    public abstract class BaseControlService
    {
        #region 属性
        public LogClass log = LogClass.GetInstance();
        private UIApplication m_UIApp;
        private Document m_RevitDoc;

        public UIApplication UIApp
        {
            get { return m_UIApp; }
            set { m_UIApp = value; }
        }
        public Document RevitDoc
        {
            get { return m_RevitDoc; }
            set { m_RevitDoc = value; }
        }
        #endregion

        public BaseControlService(UIApplication uiApp)
        {
            this.m_UIApp = uiApp;
            this.m_RevitDoc = uiApp.ActiveUIDocument.Document;
        }

        /// <summary>
        /// 设置实例属性
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="builtInParameter">参数属性</param>
        public void SetInstanceParameter(ElementId instance, int height, BuiltInParameter builtInParameter)
        {
            Element familyInstance = this.RevitDoc.GetElement(instance);
            Parameter p = familyInstance.get_Parameter(builtInParameter);
            if (p != null)
            {
                p.Set(PmSoft.Common.RevitClass.Utils.UnitTransUtils.MMToFeet(height));
            }
        }

        /// <summary>
        /// ADSK文件载入
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="family"></param>
        /// <param name="categoryType"></param>
        /// <returns></returns>
        public bool LoadFamilyADSK(string filePath, out Family family, params BuiltInCategory[] categoryType)
        {
            Document adsk_doc = this.UIApp.Application.OpenBuildingComponentDocument(filePath);
            family = adsk_doc.LoadFamily(this.RevitDoc);

            if (family == null) return false;
            var list = family.GetFamilySymbolIds().ToList();
            foreach (ElementId eleid in list)
            {
                FamilySymbol familySymbol = this.RevitDoc.GetElement(eleid) as FamilySymbol;
                if (categoryType != null)
                {
                    if (!categoryType.Any(x => (int)x == familySymbol.Category.Id.IntegerValue))
                    {
                        throw new Exception("载入失败！当前载入族类型不符！重新载入！");
                    }
                }

            }
            return true;
        }

        /// <summary>
        /// 载入族
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="categoryType"></param>
        /// <returns></returns>
        public bool LoadFamiliy(string filePath, out Family family, params BuiltInCategory[] categoryType)
        {
            string fileExtension = Path.GetExtension(filePath);
            if (fileExtension == ".adsk")
            {
                bool b = LoadFamilyADSK(filePath, out family, categoryType);
                if (b)
                    return b;
            }

            using (Transaction ts = new Transaction(this.m_RevitDoc, "LoadFamiliy"))
            {
                try
                {

                    ts.Start();
                    PmFamilyLoadOptions option = new PmFamilyLoadOptions();
                    if (this.m_RevitDoc.LoadFamily(filePath, option, out family))
                    {
                        var list = family.GetFamilySymbolIds().ToList();
                        foreach (ElementId eleid in list)
                        {
                            FamilySymbol familySymbol = this.RevitDoc.GetElement(eleid) as FamilySymbol;
                            if (categoryType != null)
                            {
                                if (!categoryType.Any(x => (int)x == familySymbol.Category.Id.IntegerValue))
                                {
                                    throw new Exception("载入失败！当前载入族类型不符！重新载入！");
                                }
                            }
                        }
                        ts.Commit();
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    ts.RollBack();
                    this.log.AddLog(ex);
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 载入族
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="categoryType"></param>
        /// <returns></returns>
        public bool LoadFamiliy(string filePath, params BuiltInCategory[] categoryType)
        {
            Family family;
            return LoadFamiliy(filePath, out family, categoryType);
        }

        /// <summary>
        /// 删除元素
        /// </summary>
        public bool DeleteElement(int elementId, string name)
        {
            using (Transaction ts = new Transaction(this.m_RevitDoc, name))
            {
                try
                {
                    ts.Start();
                    this.m_RevitDoc.Delete(new ElementId(elementId));
                    ts.Commit();
                    return true;
                }
                catch (Exception ex)
                {

                    ts.RollBack();
                    this.log.AddLog(ex);
                    throw ex;
                }
            }
        }

        /// <summary>
        /// <remarks>郑海盛</remarks>
        /// 复制type
        /// </summary>
        /// <param name="eleid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual ElementType CopyFamilySymbol(ElementId eleid, string name)
        {
            ElementType eType = this.m_RevitDoc.GetElement(eleid) as ElementType;
            using (Transaction trans = new Transaction(this.m_RevitDoc))
            {
                try
                {
                    trans.Start("复制族类型");
                    var type = eType.Duplicate(name);
                    trans.Commit();
                    return type;
                }
                catch
                {
                    trans.RollBack();
                    return null;
                }
            }
        }

        /// <summary>
        /// 获取某标高下属于某comType信息的ElementId集合
        /// </summary>
        /// <param name="levelId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual List<ElementId> GetElementIdByTypeOnLevel(ElementId levelId, PmComTypeEnum type)
        {
            List<ElementId> elementIds = new List<ElementId>();
            var elements = new FilteredElementCollector(this.m_RevitDoc).OfClass(typeof(FamilyInstance)).ToElements();
            foreach (var element in elements)
            {
                ElementId levelEmentId = element.GetParameters("标高").First().AsElementId();
                if (levelEmentId == levelId)
                {
                    bool isTypeOK = element.CheckTypeParam((int)type);
                    if (!isTypeOK)
                        continue;
                    elementIds.Add(element.Id);
                }
            }

            return elementIds;
        }

        /// <summary>
        /// 根据元素id获取元素信息
        /// </summary>
        public Element GetElementByElementID(int elementId)
        {
            Autodesk.Revit.DB.ElementId idOfElement = new ElementId(elementId);
            return this.m_RevitDoc.GetElement(idOfElement);
        }

        public FamilySymbol FindFamilySymbol(Document doc, string familyName, string familySybmolName)
        {
            Family family = null;
            FamilySymbol familySybmol = null;

            //查找文档中是否有该族
            var familys = new FilteredElementCollector(doc).OfClass(typeof(Family));
            foreach (Family f in familys)
            {
                if (f.Name == familyName)
                {
                    family = f;

                    //找到对应族类型
                    foreach (ElementId id in family.GetFamilySymbolIds())
                    {
                        FamilySymbol fs = doc.GetElement(id) as FamilySymbol;
                        if (fs != null)
                        {
                            if (familySybmolName == null)
                            {
                                familySybmol = fs;
                                break;
                            }
                            else if (fs.Name == familySybmolName)
                            {
                                familySybmol = fs;
                            }
                        }
                    }
                    break;
                }
            }

            //在指定路径下加载族
            string familyLibPath = FamilyLoadUtils.FamilyLibPath;
            if (family == null)
            {
                using (Transaction transaction = new Transaction(doc, "加载族"))
                {
                    try
                    {
                        transaction.Start();

                        var files = System.IO.Directory.GetFiles(familyLibPath, familyName + ".*", System.IO.SearchOption.AllDirectories);
                        if (files.Count() == 0)
                            return null;

                        if (!doc.LoadFamily(files[0], out family))
                            return null;

                        foreach (ElementId id in family.GetFamilySymbolIds())
                        {
                            FamilySymbol fs = doc.GetElement(id) as FamilySymbol;
                            if (fs != null)
                            {
                                if (familySybmolName == null)
                                {
                                    familySybmol = fs;
                                    familySybmol.Activate();
                                    break;
                                }
                                else if (fs.Name == familySybmolName)
                                {
                                    familySybmol = fs;
                                    familySybmol.Activate();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.RollBack();
                        return null;
                    }
                    finally
                    {
                        if (transaction != null && transaction.GetStatus() == TransactionStatus.Started)
                            transaction.RollBack();
                    }
                }
            }

            if (familySybmol != null && !familySybmol.IsActive)
            {
                using (Transaction transaction = new Transaction(doc, "激活族类型"))
                {
                    transaction.Start();
                    familySybmol.Activate();
                    transaction.Commit();
                }
            }

            return familySybmol;
        }

        /// <summary>
        /// 根据名字查找FamilySymbol
        /// </summary>
        /// <param name="name">类型名称</param>
        /// <param name="doc"></param>
        /// <param name="category">类型类别</param>
        /// <param name="isFamilyName">true 表示根据族名称寻找 false表示根据类型名称</param>
        /// <returns></returns>
        public FamilySymbol GetFamilySymbolByName(string name, Document doc, BuiltInCategory category, bool isFamilyName = false)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> DList = collector.OfCategory(category).OfClass(typeof(FamilySymbol)).ToElements();
            foreach (Element ele in DList)
            {
                string tempName = "";
                if (isFamilyName)
                {
                    tempName = (ele as FamilySymbol).FamilyName;
                }
                else
                {
                    tempName = ele.Name;
                }
                if (tempName == name)
                    return ele as FamilySymbol;
            }
            return null;
        }
    }
}
