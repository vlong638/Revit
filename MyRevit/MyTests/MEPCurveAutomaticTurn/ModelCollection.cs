using Autodesk.Revit.DB;
using MyRevit.MyTests.CompoundStructureAnnotation;
using MyRevit.MyTests.MAT;

namespace MyRevit.MyTests.MAT
{
    /// <summary>
    /// Model数据集合
    /// </summary>
    public class MATModelCollection : VLModelCollection<MATModel>
    {
        public MATModelCollection(string data) : base(data)
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
