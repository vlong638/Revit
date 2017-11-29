using Autodesk.Revit.DB;
using MyRevit.MyTests.CompoundStructureAnnotation;
using MyRevit.MyTests.DAA;

namespace MyRevit.MyTests.DAA
{
    /// <summary>
    /// Model数据集合
    /// </summary>
    public class DAAModelCollection : VLModelCollectionBase<DAAModel>
    {
        public DAAModelCollection(string data) : base(data)
        {
        }

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
