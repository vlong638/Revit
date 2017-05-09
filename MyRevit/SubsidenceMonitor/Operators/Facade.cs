using MyRevit.SubsidenceMonitor.Entities;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;

namespace MyRevit.SubsidenceMonitor.Operators
{
    public class WriteFacade
    {
        public static BLLResult CreateDetail(TList list, TDetail detail, List<TNode> nodes)
        {
            var result = new BLLResult();
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    list.DataCount = (short)list.Datas.Count;
                    if (!list.DbInsertOrReplace(connection))
                        throw new NotImplementedException("List数据未按预期存储");
                    if (!detail.DbInsert(connection))
                        throw new NotImplementedException("Detail数据未按预期存储");
                    //nodes.DbDeleteByDetailKey(connection);//由于提交时,Nodes数量发生了变更,这里省略对原有Nodes数量删除的监测
                    if (!nodes.DbInsert(connection))
                        throw new NotImplementedException("Nodes数据未按预期存储");
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    result.IsSuccess = false;
                    result.Message = ex.ToString();
                }
                connection.Close();
            }
            return result;
        }
        public static BLLResult UpdateDetail(TDetail detail, List<TNode> nodes)
        {
            var result = new BLLResult();
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    if (!detail.DbUpdate(connection))
                        throw new NotImplementedException("Detail数据未按预期存储");
                    if (!nodes.DbUpdate(connection))
                        throw new NotImplementedException("Nodes数据未按预期存储");
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    result.IsSuccess = false;
                    result.Message = ex.ToString();
                }
                connection.Close();
            }
            return result;
        }
        public static BLLResult DeleteDetail(TDetail detail)
        {
            var result = new BLLResult();
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    if (!detail.DbDelete(connection))
                        throw new NotImplementedException("Detail数据未成功删除");
                    if (!detail.Nodes.DbDeleteByDetailKey(connection))
                        throw new NotImplementedException("Nodes数据未成功删除");
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    result.IsSuccess = false;
                    result.Message = ex.ToString();
                }
                connection.Close();
            }
            return result;
        }
    }
    public class ReadFacade
    {
        public static void FetchDetails(TList list)
        {
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                list.FetchDetails(connection);
                connection.Close();
            }
        }
        public static void FetchNodes(TDetail detail)
        {
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                detail.FetchNodes(connection);
                connection.Close();
            }
        }
        public static List<TList> GetLists(EIssueType issueType, DateTime date)
        {
            List<TList> result = new List<TList>();
            using (var connection = SQLiteHelper.Connect())
            {
                connection.Open();
                result.GetListsByKeys(connection, issueType, date);
                connection.Close();
            }
            return result;
        }
    }
}
