using Autodesk.Revit.DB;
using MyRevit.MyTests.CompoundStructureAnnotation;
using MyRevit.MyTests.PAA;

namespace MyRevit.MyTests.PAA
{
    /// <summary>
    /// Model数据集合
    /// </summary>
    public class PAAModelCollection : VLModelCollectionBase<PAAModel>
    {
        public PAAModelCollection(string data) : base(data)
        {
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="doc"></param>
        public void Save(Document doc)
        {
            PAAContext.Save(doc);
        }
    }
}
