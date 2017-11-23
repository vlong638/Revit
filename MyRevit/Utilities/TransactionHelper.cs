
using Autodesk.Revit.DB;
using PmSoft.Common.CommonClass;
using System;
using VL.Logger;

namespace MyRevit.Utilities
{
    public class TransactionHelper
    {
        public static bool DelegateTransaction(Document doc, string transactionName, Func<bool> function)
        {
            using (var transaction = new Transaction(doc, transactionName))
            {
                transaction.Start();
                try
                {
                    if (function())
                    {
                        transaction.Commit();
                        return true;
                    }
                    else
                    {
                        transaction.RollBack();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollBack();
                    if (ex.Message.Contains("10 miles"))
                        Autodesk.Revit.UI.TaskDialog.Show("警告", "绘点区域超出了Revit的距离原点距离的限制");
                    LogHelper.Error(ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// 异常记录,有待优化
        /// </summary>
        /// <param name="ex"></param>
        private static void Log(Exception ex)
        {
            LogClass.GetInstance().AddLog(ex);
        }
    }
}
