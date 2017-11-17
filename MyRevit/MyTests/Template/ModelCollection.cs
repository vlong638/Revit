using Autodesk.Revit.DB;
using MyRevit.MyTests.CompoundStructureAnnotation;
using MyRevit.MyTests.Template;

namespace MyRevit.MyTests.Template
{
    /// <summary>
    /// Model数据集合
    /// </summary>
    public class TemplateModelCollection : VLModelCollectionBase<TemplateModel>
    {
        public TemplateModelCollection(string data) : base(data)
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
