using Autodesk.Revit.DB;
using PmSoft.Common.RevitClass.VLUtils;

namespace PMSoft.ConstructionManagementV2
{
    /// <summary>
    /// Model数据集合
    /// </summary>
    public class CMModelCollection : VLModelCollection<CMModel>
    {
        public CMModelCollection(string data) : base(data)
        {
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="doc"></param>
        public void Save(Document doc)
        {
            CMContext.SaveCollection(doc);
        }
    }
}
