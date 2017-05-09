using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace MyRevit.Utilities
{
    public class Revit_View_Helper
    {
        #region 隔离显示
        /// <summary>
        /// 从视图(隔离元素)
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="transactionName"></param>
        /// <param name="view"></param>
        /// <param name="elementIds"></param>
        public static void IsolateElements(Document doc, string transactionName, View view, List<ElementId> elementIds)
        {
            using (var transaction = new Transaction(doc, transactionName))
            {
                transaction.Start();
                view.IsolateElementsTemporary(elementIds);//隔离显示
                transaction.Commit();
            }
        }
        /// <summary>
        /// 取消视图的元素隔离
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="transactionName"></param>
        /// <param name="view"></param>
        public static void DeisolateElements(Document doc, string transactionName, View view)
        {
            using (var transaction = new Transaction(doc, transactionName))
            {
                transaction.Start();
                view.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);//解除隔离显示
                transaction.Commit();
            }
        } 
        #endregion
    }
}
