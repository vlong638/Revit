
using Autodesk.Revit.DB;
using System;
using VL.Logger;

namespace MyRevit.Utilities
{
    public class TransactionHelper
    {
        public static bool DelegateTransaction(Document doc, Func<bool> function)
        {
            using (var transaction = new Transaction(doc))
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
                    LogHelper.Error(new LogData(ex.ToString()));
                    return false;
                }
            }
        }
    }
}
