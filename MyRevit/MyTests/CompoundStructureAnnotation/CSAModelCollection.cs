using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    /// <summary>
    /// 存储对象集合
    /// </summary>
    public class CSAModelCollection : VLModelCollectionBase<CSAModel>
    {
        public CSAModelCollection(string data) : base(data)
        {
        }

        //public override bool LoadData(string dataStr)
        //{
        //    Data = new List<CSAModelForFamilyInstance>();
        //    if (string.IsNullOrEmpty(dataStr))
        //        return false;
        //    try
        //    {
        //        var entityStrs = dataStr.Split(EntitySplitter_Char);
        //        foreach (var entityStr in entityStrs)
        //        {
        //            if (string.IsNullOrEmpty(entityStr))
        //                continue;
        //            var model = new CSAModelForFamilyInstance();
        //            if (model.LoadData(entityStr))
        //                Data.Add(model);
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        //TODO log
        //        return false;
        //    }
        //}

        //public override string ToData()
        //{
        //    return string.Join(EntitySplitter, Data.Select(c => c.ToData()));
        //}

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="doc"></param>
        public void Save(Document doc)
        {
            CSAContext.Save(doc);
        }
    }

}
