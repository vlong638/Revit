using Autodesk.Revit.DB;
using PmSoft.Common.RevitClass.VLUtils;

namespace PmSoft.MepProject.MepWork.FullFunctions.MEPCurveAutomaticTurn
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
            MATContext.SaveCollection(doc);
        }
    }
}
