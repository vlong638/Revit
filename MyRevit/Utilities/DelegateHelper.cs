using Autodesk.Revit.DB;
using PmSoft.Common.CommonClass;
using System;

namespace MyRevit.Utilities
{
    /// <summary>
    /// 事务Helper
    /// </summary>
    public class DelegateHelper
    {
        /// <summary>
        /// 事务简化.模板
        /// </summary>
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
                    Log(ex);
                    return false;
                }
            }
        }

        /// <summary>
        ///  Try Catch 流程模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static T DelegateTryCatch<T>(Func<T> func, Func<T> onError)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                Log(ex);
            }
            try
            {
                return onError();
            }
            catch (Exception ex)
            {
                Log(ex);
            }
            return default(T);
        }

        /// <summary>
        /// Try Catch 流程模板
        /// </summary>
        /// <param name="action"></param>
        /// <param name="onError"></param>
        public static void DelegateTryCatch(Action action, Action onError)
        {
            try
            {
                action();
                return;
            }
            catch (Exception ex)
            {
                Log(ex);
            }
            try
            {
                onError();
            }
            catch (Exception ex)
            {
                Log(ex);
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
