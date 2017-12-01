using Autodesk.Revit.DB;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    /// <summary>
    /// 存储对象集合
    /// </summary>
    public class CSAModelCollection : VLModelCollection<CSAModel>
    {
        public CSAModelCollection(string data) : base(data)
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
