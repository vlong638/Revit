using Autodesk.Revit.DB;
using MyRevit.MyTests.CompoundStructureAnnotation;
using MyRevit.MyTests.PBPA;

namespace MyRevit.MyTests.PBPA
{
    /// <summary>
    /// Model数据集合
    /// </summary>
    public class PBPAModelCollection : VLModelCollection<PBPAModel>
    {
        public PBPAModelCollection(string data) : base(data)
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
